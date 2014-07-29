using FullInspector.Rotorz.ReorderableList;
using System;
using UnityEngine;

namespace FullInspector.Internal {
    /// <summary>
    /// Provides a property editor for arrays, or a type T[].
    /// </summary>
    public class ArrayPropertyEditor<T> : PropertyEditor<T[]> {
        private PropertyEditorChain _propertyEditor = PropertyEditor.Get(typeof(T), typeof(T));

        public override T[] Edit(Rect region, GUIContent label, T[] elements) {
            if (elements == null) {
                elements = new T[0];
            }

            ArrayListAdaptor<T> adapter = GetAdapter(elements);

            Rect titleRect = new Rect(region);
            titleRect.height = GetTitleHeight();
            ReorderableListGUI.Title(titleRect, label);

            Rect bodyRect = new Rect(region);
            bodyRect.y += GetTitleHeight();
            bodyRect.height -= GetTitleHeight();
            ReorderableListGUI.ListFieldAbsolute(bodyRect, adapter, DrawEmpty);

            return adapter.StoredArray;
        }

        private T DrawItem(Rect rect, T item) {
            return (T)_propertyEditor.FirstEditor.Edit(rect, GUIContent.none, item);
        }

        private ArrayListAdaptor<T> GetAdapter(T[] elements) {
            return new ArrayListAdaptor<T>(elements, DrawItem, GetItemHeight);
        }

        private void DrawEmpty(Rect rect) {
        }

        private float GetItemHeight(T element) {
            float height = _propertyEditor.FirstEditor.GetElementHeight(GUIContent.none, element);
            if (height < ReorderableListGUI.DefaultItemHeight) {
                return ReorderableListGUI.DefaultItemHeight;
            }
            return height;
        }

        private float GetTitleHeight() {
            return ReorderableListGUI.CalculateTitleHeight();
        }

        public override float GetElementHeight(GUIContent label, T[] elements) {
            if (elements == null || elements.Length == 0) {
                const int heightOfEmpty = 20;
                return GetTitleHeight() + heightOfEmpty;
            }

            return GetTitleHeight() + ReorderableListGUI.CalculateListFieldHeight(GetAdapter(elements));
        }

        public override GUIContent GetFoldoutHeader(GUIContent label, object element) {
            if (element == null) {
                return label;
            }

            return new GUIContent(label.text + " (" + ((T[])element).Length + " elements)",
                label.tooltip);
        }
    }

    public static class ArrayPropertyEditor {
        public static IPropertyEditor TryCreate(Type dataType) {
            if (dataType.IsArray == false) {
                return null;
            }

            Type elementType = dataType.GetElementType();

            Type editorType = typeof(ArrayPropertyEditor<>).MakeGenericType(elementType);
            return (IPropertyEditor)Activator.CreateInstance(editorType);
        }
    }
}