﻿using FullInspector.Rotorz.ReorderableList;
using System.Collections.Generic;
using UnityEngine;

namespace FullInspector.Modules.Collections {
    /// <summary>
    /// Convenience class that wraps other IPropertyEditors and allows them to be List[T] to be
    /// edited as a reorderable list.
    /// </summary>
    /// <remarks>
    /// This uses the open type property editor injection system to automatically inject itself for
    /// those ElementTypes which have PropertyEditors.
    /// </remarks>
    [CustomPropertyEditor(typeof(IList<>), Inherit = true)]
    public class IListPropertyEditor<TList, TData> : PropertyEditor<TList>
            where TList : IList<TData>, new() {

        private PropertyEditorChain _propertyEditor = PropertyEditor.Get(typeof(TData), null);

        public override TList OnSceneGUI(TList elements) {
            if (elements != null) {
                for (int i = 0; i < elements.Count; ++i) {
                    elements[i] = (TData)_propertyEditor.FirstEditor.OnSceneGUI(elements[i]);
                }
            }

            return elements;
        }

        public override TList Edit(Rect region, GUIContent label, TList elements) {
            if (elements == null) {
                elements = new TList();
            }

            Rect titleRect = new Rect(region);
            titleRect.height = GetTitleHeight();
            ReorderableListGUI.Title(titleRect, label);

            Rect bodyRect = new Rect(region);
            bodyRect.y += GetTitleHeight();
            bodyRect.height -= GetTitleHeight();
            ReorderableListGUI.ListFieldAbsolute(bodyRect, GetAdapter(elements), DrawEmpty);

            return elements;
        }

        private TData DrawItem(Rect rect, TData item) {
            return (TData)_propertyEditor.FirstEditor.Edit(rect, GUIContent.none, item);
        }

        private IReorderableListAdaptor GetAdapter(TList elements) {
            return new GenericListAdaptorWithDynamicHeight<TData>(elements, DrawItem, GetItemHeight);
        }

        private void DrawEmpty(Rect rect) {
        }

        private float GetItemHeight(TData element) {
            float height = _propertyEditor.FirstEditor.GetElementHeight(GUIContent.none, element);
            if (height < ReorderableListGUI.DefaultItemHeight) {
                return ReorderableListGUI.DefaultItemHeight;
            }
            return height;
        }

        private float GetTitleHeight() {
            return ReorderableListGUI.CalculateTitleHeight();
        }

        public override float GetElementHeight(GUIContent label, TList elements) {
            if (elements == null || elements.Count == 0) {
                const int heightOfEmpty = 20;
                return GetTitleHeight() + heightOfEmpty;
            }

            return GetTitleHeight() + ReorderableListGUI.CalculateListFieldHeight(GetAdapter(elements));
        }

        public override GUIContent GetFoldoutHeader(GUIContent label, object element) {
            if (element == null) {
                return label;
            }

            return new GUIContent(label.text + " (" + ((TList)element).Count + " elements)",
                label.tooltip);
        }
    }
}