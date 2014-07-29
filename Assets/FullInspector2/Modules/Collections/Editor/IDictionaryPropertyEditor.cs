﻿using FullInspector.Rotorz.ReorderableList;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullInspector.Modules.Collections {
    /// <summary>
    /// The base dictionary property editor provides common code for editing dictionaries. To enable
    /// support for editing an IDictionary type, all one needs to do is extend this class with the
    /// appropriate type parameters filled in.
    /// </summary>
    /// <typeparam name="TDictionary">The dictionary type (must extend IDictionary[TKey,
    /// TValue])</typeparam> <typeparam name="TKey">The key type stored in the
    /// dictionary.</typeparam> <typeparam name="TValue">The value type stored in the
    /// dictionary.</typeparam>
    [CustomPropertyEditor(typeof(IDictionary<,>), Inherit = true)]
    public class IDictionaryPropertyEditor<TDictionary, TKey, TValue> : PropertyEditor<TDictionary>
        where TDictionary : IDictionary<TKey, TValue>, new() {

        /// <summary>
        /// The key editor that we use for editing key values.
        /// </summary>
        private static readonly PropertyEditorChain KeyEditor = PropertyEditor.Get(typeof(TKey), null);

        /// <summary>
        /// The value editor that we use for editing values.
        /// </summary>
        private static readonly PropertyEditorChain ValueEditor = PropertyEditor.Get(typeof(TValue), null);

        /// <summary>
        /// The margin between the end of the list and the add key button.
        /// </summary>
        private const float ListKeyAddMargin = 0;

        /// <summary>
        /// The amount of space at the after the add key region.
        /// </summary>
        private const float ListKeyEndMargin = 0;

        /// <summary>
        /// The current value for the next key that we will insert into the dictionary. We show a
        /// custom property editor for this value below the list GUI.
        /// </summary>
        private TKey _nextKey;

        public override TDictionary Edit(Rect region, GUIContent label, TDictionary elements) {
            if (elements == null) {
                elements = new TDictionary();
            }

            // calculate the rectangles that we use in the dictionary editor
            Rect titleRect = new Rect(region);
            titleRect.height = ReorderableListGUI.CalculateTitleHeight();

            Rect bodyRect = new Rect(region);
            bodyRect.y += titleRect.height;
            bodyRect.height -= titleRect.height + ListKeyAddMargin + GetBottomHeight() + ListKeyEndMargin;

            Rect addKeyRectButton, addKeyRectValue;
            {
                Rect baseKeyRect = new Rect(region);
                baseKeyRect.y += titleRect.height + bodyRect.height + ListKeyAddMargin;
                baseKeyRect.height = GetBottomHeight();

                SplitRectAbsolute(baseKeyRect,
                    /*leftWidth:*/ ReorderableListGUI.defaultAddButtonStyle.fixedWidth,
                    /*margin:*/ 5, out addKeyRectButton, out addKeyRectValue);
                addKeyRectButton.height = ReorderableListGUI.defaultAddButtonStyle.fixedHeight;
            }

            // draw the title
            ReorderableListGUI.Title(titleRect, label);

            // draw the dictionary elements
            ReorderableListGUI.ListFieldAbsolute(bodyRect, GetAdapter(elements), DrawEmpty,
                ReorderableListFlags.DisableReordering | ReorderableListFlags.HideAddButton);

            // draw the next key
            //int controlId = EditorGUIUtility.GetControlID(FocusType.Native);
            //var control = GUIUtility.GetStateObject(typeof(ReorderableListControl), controlId) as ReorderableListControl;

            if (GUI.Button(addKeyRectButton, "", ReorderableListGUI.defaultAddButtonStyle)) {
                if (elements.ContainsKey(_nextKey) == false) {
                    elements[_nextKey] = default(TValue);
                    GUI.FocusControl(null);
                    _nextKey = default(TKey);
                }
                else {
                    Debug.LogError("Dictionary already contains key \"" + _nextKey + "\"; ignoring");
                }
            }
            EnsureKeyDefaults(ref _nextKey);

            _nextKey = (TKey)KeyEditor.FirstEditor.Edit(addKeyRectValue, GUIContent.none, _nextKey);

            return elements;
        }

        /// <summary>
        /// Draws a key in the dictionary's KeyValuePair.
        /// </summary>
        private TKey DrawKey(Rect rect, TKey key) {
            Rect keyRect, valueRect;
            SplitRect(rect, /*percentage:*/ .3f, /*margin:*/ 5, out keyRect, out valueRect);
            keyRect.height = KeyEditor.FirstEditor.GetElementHeight(GUIContent.none, key);

            return (TKey)KeyEditor.FirstEditor.Edit(keyRect, GUIContent.none, key);
        }

        /// <summary>
        /// Draws a value in the dictionary's KeyValuePair.
        /// </summary>
        private TValue DrawValue(Rect rect, TValue value) {
            Rect keyRect, valueRect;
            SplitRect(rect, /*percentage:*/ .3f, /*margin:*/ 5, out keyRect, out valueRect);
            valueRect.height = ValueEditor.FirstEditor.GetElementHeight(GUIContent.none, value);

            return (TValue)ValueEditor.FirstEditor.Edit(valueRect, GUIContent.none, value);
        }

        /// <summary>
        /// Splits the given rect into two rects that are divided horizontally.
        /// </summary>
        /// <param name="rect">The rect to split</param>
        /// <param name="percentage">The horizontal percentage that the rects are split at</param>
        /// <param name="margin">How much space that should be between the two rects</param>
        /// <param name="left">The output left-hand side rect</param>
        /// <param name="right">The output right-hand side rect</param>
        private static void SplitRect(Rect rect, float percentage, float margin, out Rect left, out Rect right) {
            left = new Rect(rect);
            left.width *= .3f;

            right = new Rect(rect);
            right.x += left.width + margin;
            right.width -= left.width + margin;
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
        /// Gets a reorderable list adapter for a dictionary. The implementation is somewhat hacky
        /// and slow, but it works.
        /// </summary>
        private IReorderableListAdaptor GetAdapter(IDictionary<TKey, TValue> dict) {
            return new DictionaryAdapter<TKey, TValue>(dict, DrawKey, DrawValue, GetItemHeight);
        }

        /// <summary>
        /// Draws an empty dictionary.
        /// </summary>
        private void DrawEmpty(Rect rect) {
        }

        /// <summary>
        /// Returns the height of an individual item in the dictionary list.
        /// </summary>
        private float GetItemHeight(KeyValuePair<TKey, TValue> element) {
            float height = Math.Max(
                KeyEditor.FirstEditor.GetElementHeight(GUIContent.none, element.Key),
                ValueEditor.FirstEditor.GetElementHeight(GUIContent.none, element.Value));

            if (height < ReorderableListGUI.DefaultItemHeight) {
                return ReorderableListGUI.DefaultItemHeight;
            }
            return height;
        }

        /// <summary>
        /// Ensures that the given key has a valid default value. Dictionaries don't like null
        /// strings.
        /// </summary>
        private void EnsureKeyDefaults(ref TKey key) {
            if (typeof(TKey) == typeof(string) && key == null) {
                key = (TKey)(object)"";
            }
        }

        /// <summary>
        /// Returns the height of the bottom element of the dictionary property editor. More
        /// precisely, the bottom element is the "Add Key" button and the "Add Key" property
        /// inspector.
        /// </summary>
        private float GetBottomHeight() {
            EnsureKeyDefaults(ref _nextKey);

            int buttonHeight = (int)ReorderableListGUI.defaultAddButtonStyle.fixedHeight;
            float keyEditorHeight = KeyEditor.FirstEditor.GetElementHeight(GUIContent.none, _nextKey);

            return Math.Max(buttonHeight, keyEditorHeight);
        }

        public override float GetElementHeight(GUIContent label, TDictionary elements) {
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

            float titleHeight = ReorderableListGUI.CalculateTitleHeight();
            return titleHeight + listHeight + ListKeyAddMargin + GetBottomHeight() + ListKeyEndMargin;
        }

        public override GUIContent GetFoldoutHeader(GUIContent label, object element) {
            if (element == null) {
                return label;
            }

            return new GUIContent(label.text + " (" + ((TDictionary)element).Count + " elements)",
                label.tooltip);
        }
    }
}