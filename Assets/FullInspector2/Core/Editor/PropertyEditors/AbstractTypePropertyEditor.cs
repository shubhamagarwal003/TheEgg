using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FullInspector.Internal {
    /// <summary>
    /// Provides a property editor for types which cannot be instantiated directly and require the
    /// user to select a specific instance to instantiate.
    /// </summary>
    public class AbstractTypePropertyEditor : IPropertyEditor, IPropertyEditorEditAPI {
        private AbstractTypeInstanceOptionManager _options;

        public AbstractTypePropertyEditor(Type baseType) {
            _options = new AbstractTypeInstanceOptionManager(baseType);
        }

        public PropertyEditorChain EditorChain {
            get;
            set;
        }


        public object OnSceneGUI(object element) {
            if (element != null) {
                PropertyEditorChain chain = PropertyEditor.Get(element.GetType(), null);
                IPropertyEditor editor = chain.SkipUntilNot(typeof(AbstractTypePropertyEditor));

                return editor.OnSceneGUI(element);
            }
            return element;
        }

        public object Edit(Rect region, GUIContent label, object element) {
            _options.RemoveExtraneousOptions();

            // draw the popup
            {
                int popupHeight = (int)EditorStyles.popup.CalcHeight(GUIContent.none, 100);

                Rect popupRegion = new Rect(region);
                popupRegion.height = popupHeight;
                region.y += popupRegion.height;
                region.height -= popupRegion.height;

                int selectedIndex = _options.GetDisplayOptionIndex(element);
                int updatedIndex = EditorGUI.Popup(popupRegion, label, selectedIndex, _options.GetDisplayOptions());

                element = _options.UpdateObjectInstance(element, selectedIndex, updatedIndex);
            }

            // no element; no editor
            if (element == null) {
                return null;
            }

            // draw the comment
            // TODO: move this into ReflectedPropertyEditor, draw the comment above the type
            {
                string comment = _options.GetComment(element);
                if (string.IsNullOrEmpty(comment) == false) {
                    Rect commentRegion = CommentUtils.GetCommentRect(comment, region);
                    region.y += commentRegion.height;
                    region.height += commentRegion.height;

                    EditorGUI.HelpBox(commentRegion, comment, MessageType.None);
                }
            }

            // draw the instance specific property editor
            {
                Rect selectedRegion = new Rect(region);
                selectedRegion = RectTools.IndentedRect(selectedRegion);
                region.y += selectedRegion.height;
                region.height -= selectedRegion.height;

                // show custom editor
                PropertyEditorChain chain = PropertyEditor.Get(element.GetType(), null);
                IPropertyEditor editor = chain.SkipUntilNot(typeof(AbstractTypePropertyEditor)); 
                
                return editor.Edit(selectedRegion, GUIContent.none, element);
            }
        }

        public float GetElementHeight(GUIContent label, object element) {
            float height = EditorStyles.popup.CalcHeight(label, 100);

            string comment = _options.GetComment(element);
            if (string.IsNullOrEmpty(comment) == false) {
                height += CommentUtils.GetCommentHeight(comment);
            }

            if (element != null) {
                PropertyEditorChain chain = PropertyEditor.Get(element.GetType(), null);
                IPropertyEditor editor = chain.SkipUntilNot(typeof(AbstractTypePropertyEditor));

                height += RectTools.IndentVertical;
                height += editor.GetElementHeight(GUIContent.none, element);
            }

            return height;
        }

        public GUIContent GetFoldoutHeader(GUIContent label, object element) {
            return new GUIContent(label.text + " (" + fiReflectionUtilitity.GetObjectTypeNameSafe(element) + ")");
        }

        public bool CanEdit(Type dataType) {
            throw new NotSupportedException();
        }

        public static IPropertyEditor TryCreate(Type dataType) {
            if (dataType.IsAbstract || dataType.IsInterface ||
                fiReflectionUtilitity.GetCreatableTypesDeriving(dataType).Count() > 1) {

                return new AbstractTypePropertyEditor(dataType);
            }

            return null;
        }
    }
}