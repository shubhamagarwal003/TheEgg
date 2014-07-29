using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace FullInspector.Internal {
    public class AttributePropertyEditor : IPropertyEditor, IPropertyEditorEditAPI {
        /// <summary>
        /// A map of attribute type to the IPropertyEditor type that is associated with that
        /// attribute (via the CustomAttributePropertyEditorAttribute annotation).
        /// </summary>
        private static Dictionary<Type, Type> _attributeEditorMappings = new Dictionary<Type, Type>();

        /// <summary>
        /// A map of attribute type to if that property editor should replace all other editors
        /// after it.
        /// </summary>
        private static Dictionary<Type, bool> _attributeReplaceMappings = new Dictionary<Type, bool>();

        static AttributePropertyEditor() {
            // fetch all CustomAttributePropertyEditorAttribute types
            _attributeEditorMappings = new Dictionary<Type, Type>();
            _attributeReplaceMappings = new Dictionary<Type, bool>();

            foreach (Type attributeHolder in
                from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()

                where type.IsAbstract == false
                where type.IsInterface == false

                let attribute = type.GetAttribute<CustomAttributePropertyEditorAttribute>()
                where attribute != null

                select type) {

                if (typeof(IAttributePropertyEditor).IsAssignableFrom(attributeHolder) == false) {
                    Debug.LogWarning(string.Format("{0} has a {1} attribute but does not extend {2}",
                        attributeHolder, typeof(CustomAttributePropertyEditorAttribute).Name,
                        typeof(IAttributePropertyEditor).Name));
                    continue;
                }

                var attribute = attributeHolder.GetAttribute<CustomAttributePropertyEditorAttribute>();

                _attributeEditorMappings[attribute.AttributeActivator] = attributeHolder;
                _attributeReplaceMappings[attribute.AttributeActivator] = attribute.ReplaceOthers;
            }
        }

        public PropertyEditorChain EditorChain {
            get;
            set;
        }

        private List<IAttributePropertyEditor> _editors;
        private bool _showPrimary;

        private AttributePropertyEditor(List<IAttributePropertyEditor> editors, bool showPrimary) {
            _editors = editors;
            _showPrimary = showPrimary;
        }

        public object OnSceneGUI(object element) {
            for (int i = 0; i < _editors.Count; ++i) {
                element = _editors[i].OnSceneGUI(element);
            }

            if (_showPrimary) {
                element = NextEditor.OnSceneGUI(element);
            }

            return element;
        }

        private IPropertyEditor NextEditor {
            get {
                return EditorChain.GetNextEditor(this);
            }
        }

        public object Edit(Rect region, GUIContent label, object element) {
            var heights = new List<float>(_editors.Count);
            for (int i = 0; i < _editors.Count; ++i) {
                heights.Add(_editors[i].GetElementHeight(label, element));
            }

            Rect editRegion = region;
            for (int i = 0; i < _editors.Count; ++i) {
                editRegion.height = heights[i];
                element = _editors[i].Edit(editRegion, label, element);
                editRegion.y += editRegion.height;
            }

            if (_showPrimary) {
                editRegion.height = NextEditor.GetElementHeight(label, element);
                element = NextEditor.Edit(editRegion, label, element);
            }

            return element;
        }

        public float GetElementHeight(GUIContent label, object element) {
            float height = 0;

            for (int i = 0; i < _editors.Count; ++i) {
                height += _editors[i].GetElementHeight(label, element);
            }

            if (_showPrimary) {
                height += NextEditor.GetElementHeight(label, element);
            }

            return height;
        }

        public GUIContent GetFoldoutHeader(GUIContent label, object element) {
            return label;
        }

        public bool CanEdit(Type dataType) {
            throw new NotSupportedException();
        }

        public static IPropertyEditor TryCreate(Type editedType, ICustomAttributeProvider editedAttributes) {
            if (editedAttributes == null) {
                return null;
            }

            List<IAttributePropertyEditor> editors = new List<IAttributePropertyEditor>();
            bool replace = false;

            foreach (object attribute in
                editedAttributes.GetCustomAttributes(/*inherit:*/ true)) {

                if (_attributeEditorMappings.ContainsKey(attribute.GetType())) {
                    Type editorType = _attributeEditorMappings[attribute.GetType()];
                    IPropertyEditor editor = PropertyEditorTools.TryCreateEditor(editedType, editorType);
                    if (editor == null) {
                        Debug.LogWarning(string.Format("Failed to create attribute property " +
                            "editor {0} for {1}", editedAttributes, editorType));
                        continue;
                    }

                    IAttributePropertyEditor attributeEditor = (IAttributePropertyEditor)editor;
                    attributeEditor.Attribute = (Attribute)attribute;
                    editors.Add(attributeEditor);

                    // this attribute editor replaces the other ones
                    if (_attributeReplaceMappings[attribute.GetType()]) {
                        replace = true;
                    }
                }
            }

            if (editors.Count == 0) {
                return null;
            }

            // Sort the editors by their opt-in ordering
            editors = editors.OrderBy(editor => {
                var attributeOrder = editor.Attribute as IInspectorAttributeOrder;
                if (attributeOrder != null) {
                    return attributeOrder.Order;
                }

                return double.MaxValue;
            }).ToList();

            return new AttributePropertyEditor(editors, replace == false);
        }
    }
}