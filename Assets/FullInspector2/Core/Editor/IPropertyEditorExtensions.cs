using System;
using UnityEditor;
using UnityEngine;

namespace FullInspector {
    public static class PropertyEditorExtensions {
        /// <summary>
        /// Helper method to fetch the editing API for an IPropertyEditor.
        /// </summary>
        private static IPropertyEditorEditAPI GetEditingAPI(IPropertyEditor editor) {
            var api = editor as IPropertyEditorEditAPI;

            if (api == null) {
                throw new InvalidOperationException(string.Format("Type {0} needs to extend " +
                    "IPropertyEditorEditAPI", editor.GetType()));
            }

            return api;
        }

        /// <summary>
        /// Display a Unity inspector GUI that provides an editing interface for the given object.
        /// </summary>
        /// <param name="region">The rect on the screen to draw the GUI controls.</param>
        /// <param name="label">The label to label the controls with.</param>
        /// <param name="element">The element itself to edit. This can be mutated directly. For
        /// values which cannot be mutated, such as structs, the return value is used to update the
        /// stored value.</param>
        /// <returns>An updated instance of the element.</returns>
        public static T Edit<T>(this IPropertyEditor editor, Rect region, GUIContent label, T element) {
            var api = GetEditingAPI(editor);

            //--
            // Unity doesn't properly render editing controls when they are highly indented (read:
            // the x value is large).
            //
            // To account for this, we force all controls to render at or near (0, 0) by just
            // wrapping all FI controls within Begin/End Group calls.
            //
            // However, Unity will not render controls identically at (0, 0) compared to near (0, 0)
            // so as a fit and polish issue we only correct the indentation issue when the rect is
            // indented a certain amount so that Unity renders everything properly.
            //

            if (region.x > 20) {
                GUI.BeginGroup(region);
                region.x = 0;
                region.y = 0;
                T updated = (T)api.Edit(region, label, element);
                GUI.EndGroup();
                return updated;
            }

            return (T)api.Edit(region, label, element);
        }

        /// <summary>
        /// Returns the height of the region that needs editing.
        /// </summary>
        /// <param name="label">The label that will be used when editing.</param>
        /// <param name="element">The element that will be edited.</param>
        /// <returns>The height of the region that needs editing.</returns>
        public static float GetElementHeight<T>(this IPropertyEditor editor, GUIContent label, T element) {
            var api = GetEditingAPI(editor);

            return api.GetElementHeight(label, element);
        }

        /// <summary>
        /// Returns a header that should be used for the foldout. An item is displayed within a
        /// foldout when this property editor reaches a certain height.
        /// </summary>
        /// <param name="label">The current foldout label.</param>
        /// <param name="element">The current object element.</param>
        /// <returns>An updated label.</returns>
        public static GUIContent GetFoldoutHeader<T>(this IPropertyEditor editor, GUIContent label,
            T element) {

            var api = GetEditingAPI(editor);

            return api.GetFoldoutHeader(label, element);
        }

        /// <summary>
        /// Draw an optional scene GUI.
        /// </summary>
        /// <param name="element">The object instance to edit using the scene GUI.</param>
        /// <returns>An updated object instance.</returns>
        public static T OnSceneGUI<T>(this IPropertyEditor editor, T element) {
            var api = GetEditingAPI(editor);

            return (T)api.OnSceneGUI(element);
        }

        /// <summary>
        /// This method makes it easy to use a typical property editor as a GUILayout style method,
        /// where the rect is taken care of.
        /// </summary>
        /// <param name="editor">The editor that is being used.</param>
        /// <param name="label">The label to edit the region with.</param>
        /// <param name="element">The element that is being edited.</param>
        public static T EditWithGUILayout<T>(this IPropertyEditor editor, GUIContent label,
            T element) {
            var api = GetEditingAPI(editor);

            float height = api.GetElementHeight(label, element);
            Rect region = EditorGUILayout.GetControlRect(false, height);
            return (T)api.Edit(region, label, element);
        }
    }
}