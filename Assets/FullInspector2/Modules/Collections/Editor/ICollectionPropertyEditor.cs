using FullInspector.Internal;
using FullInspector.Rotorz.ReorderableList;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullInspector.Modules.Collections {
    /// <summary>
    /// Provides a property editor for all types which derive from ICollection{T}.
    /// </summary>
    [CustomPropertyEditor(typeof(ICollection<>), Inherit = true)]
    public class ICollectionPropertyEditor<TDerived, T> : PropertyEditor<ICollection<T>>
        where TDerived : ICollection<T> {

        /// <summary>
        /// The editor that is used for editing instances of type T.
        /// </summary>
        private static readonly PropertyEditorChain TEditor = PropertyEditor.Get(typeof(T), null);

        /// <summary>
        /// The current value for the next item that we will insert into the collection. We show a
        /// custom property editor for this value below the list GUI.
        /// </summary>
        private T _nextValue;

        public override ICollection<T> Edit(Rect region, GUIContent label, ICollection<T> elements) {
            EnsureNotNull(ref elements);

            //-
            //-
            //-
            // calculate the rectangles that we use in the editor
            Rect titleRect = new Rect(region);
            titleRect.height = ReorderableListGUI.CalculateTitleHeight();

            Rect bodyRect = new Rect(region);
            bodyRect.y += titleRect.height;
            bodyRect.height -= titleRect.height + GetBottomHeight();

            Rect addKeyRectButton, addKeyRectValue;
            {
                Rect baseKeyRect = new Rect(region);
                baseKeyRect.y += titleRect.height + bodyRect.height;
                baseKeyRect.height = GetBottomHeight();

                SplitRectAbsolute(baseKeyRect,
                    /*leftWidth:*/ ReorderableListGUI.defaultAddButtonStyle.fixedWidth,
                    /*margin:*/ 5, out addKeyRectButton, out addKeyRectValue);
                addKeyRectButton.height = ReorderableListGUI.defaultAddButtonStyle.fixedHeight;
            }

            //-
            //-
            //-
            // draw the title
            ReorderableListGUI.Title(titleRect, label);

            // draw the dictionary elements
            ReorderableListGUI.ListFieldAbsolute(bodyRect, GetAdapter(elements), DrawEmpty,
                ReorderableListFlags.DisableReordering | ReorderableListFlags.HideAddButton);

            // draw the next key
            if (GUI.Button(addKeyRectButton, "", ReorderableListGUI.defaultAddButtonStyle)) {
                elements.Add(_nextValue);
                GUI.FocusControl(null);
                _nextValue = default(T);
            }
            EnsureValueDefaults(ref _nextValue);
            _nextValue = (T)TEditor.FirstEditor.Edit(addKeyRectValue, GUIContent.none, _nextValue);

            return elements;
        }

        /// <summary>
        /// Returns the height of the bottom element of the dictionary property editor. More
        /// precisely, the bottom element is the "Add Key" button and the "Add Key" property
        /// inspector.
        /// </summary>
        private float GetBottomHeight() {
            EnsureValueDefaults(ref _nextValue);

            int buttonHeight = (int)ReorderableListGUI.defaultAddButtonStyle.fixedHeight;
            float keyEditorHeight = TEditor.FirstEditor.GetElementHeight(GUIContent.none, _nextValue);

            return Math.Max(buttonHeight, keyEditorHeight);
        }

        /// <summary>
        /// Splits the given rect into two rects that are divided horizontally.
        /// </summary>
        /// <param name="rect">The rect to split</param>
        /// <param name="leftWidth">The horizontal size of the left rect</param>
        /// <param name="margin">How much space that should be between the two rects</param>
        /// <param name="left">The output left-hand side rect</param>
        /// <param name="right">The output right-hand side rect</param>
        private static void SplitRectAbsolute(Rect rect, float leftWidth, float margin, out Rect left, out Rect right) {
            left = new Rect(rect);
            left.width = leftWidth;

            right = new Rect(rect);
            right.x += left.width + margin;
            right.width -= left.width + margin;
        }

        /// <summary>
        /// Callback that draws an empty dictionary.
        /// </summary>
        private static void DrawEmpty(Rect rect) {
        }

        /// <summary>
        /// Callback from Rotorz that draws an item in the collection.
        /// </summary>
        private static T DrawItem(Rect rect, T element) {
            return (T)TEditor.FirstEditor.Edit(rect, GUIContent.none, element);
        }

        /// <summary>
        /// Callback from Rotorz that returns the height of an item in the collection.
        /// </summary>
        private static float GetItemHeight(T element) {
            float height = TEditor.FirstEditor.GetElementHeight(GUIContent.none, element);
            if (height < ReorderableListGUI.DefaultItemHeight) {
                return ReorderableListGUI.DefaultItemHeight;
            }
            return height;
        }

        /// <summary>
        /// Gets a reorderable list adapter for a dictionary. The implementation is somewhat hacky
        /// and slow, but it works.
        /// </summary>
        private static IReorderableListAdaptor GetAdapter(ICollection<T> collection) {
            return new CollectionAdaptor<T>(collection, DrawItem, GetItemHeight);
        }

        /// <summary>
        /// Ensures that the given value has a valid default value. Some collection types dislike
        /// null strings, for example.
        /// </summary>
        private void EnsureValueDefaults(ref T key) {
            if (typeof(T) == typeof(string) && key == null) {
                key = (T)(object)"";
            }
        }

        /// <summary>
        /// Ensures that the given collection reference is not null. If it is currently null, then a
        /// value is constructed and stored inside of the reference.
        /// </summary>
        private static void EnsureNotNull(ref ICollection<T> elements) {
            if (elements == null) {
                elements = (ICollection<T>)InspectedType.Get(typeof(TDerived)).CreateInstance();
            }
        }

        public override float GetElementHeight(GUIContent label, ICollection<T> elements) {
            EnsureNotNull(ref elements);

            // height of the title
            float titleHeight = ReorderableListGUI.CalculateTitleHeight();

            // height of the actual Rotorz list editor
            float listHeight;
            if (elements == null || elements.Count == 0) {
                const int heightOfEmpty = 4;
                listHeight = heightOfEmpty;
            }
            else {
                IReorderableListAdaptor adaptor = GetAdapter(elements);
                listHeight = ReorderableListGUI.CalculateListFieldHeight(adaptor,
                    ReorderableListFlags.HideAddButton);
            }

            // title + editor + new item editor
            return titleHeight + listHeight + GetBottomHeight();
        }

        public override GUIContent GetFoldoutHeader(GUIContent label, object element) {
            if (element == null) {
                return label;
            }

            return new GUIContent(label.text + " (" + ((ICollection<T>)element).Count + " elements)",
                label.tooltip);
        }
    }
}