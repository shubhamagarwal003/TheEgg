using FullInspector.Internal;
using FullInspector.Modules.Collections;
using FullInspector.Modules.Common;
using FullInspector.Rotorz.ReorderableList;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace FullInspector {
    /// <summary>
    /// An editor that provides a good inspector experience for types which derive from
    /// ISerializedObject.
    /// </summary>
    public class FullInspectorCommonSerializedObjectEditor : Editor {
        public override bool RequiresConstantRepaint() {
            // When we're playing and code modifies the inspector for an object, we want to always
            // show the latest data
            return EditorApplication.isPlaying || fiEditorUtility.ShouldInspectorRedraw;
        }

        /// <summary>
        /// This is accessed by the BaseBehaviorEditor (using reflection) to determine if the editor
        /// should show the value for _serializedState.
        /// </summary>
        private static bool _editorShowSerializedState;

        [MenuItem("Window/Full Inspector/Show Serialized State")]
        protected static void ViewSerializedState() {
            _editorShowSerializedState = !_editorShowSerializedState;
        }

        private void DrawSerializedState(ISerializedObject behavior) {
            if (_editorShowSerializedState) {
                var flags = ReorderableListFlags.HideAddButton;

                EditorGUILayout.HelpBox("The following is raw serialization data. Only change it " +
                    "if you know what you're doing or you could corrupt your object!",
                    MessageType.Warning);

                ReorderableListGUI.Title("Serialized Keys");
                ReorderableListGUI.ListField(new GenericListAdaptorWithDynamicHeight<string>(
                    behavior.SerializedStateKeys, DrawItem, GetItemHeight), flags);

                ReorderableListGUI.Title("Serialized Values");
                ReorderableListGUI.ListField(new GenericListAdaptorWithDynamicHeight<string>(
                    behavior.SerializedStateValues, DrawItem, GetItemHeight), flags);

                ReorderableListGUI.Title("Serialized Object References");
                ReorderableListGUI.ListField(new GenericListAdaptorWithDynamicHeight<UnityObject>(
                    behavior.SerializedObjectReferences, DrawItem, GetItemHeight), flags);
            }
        }

        private float GetItemHeight(string item) {
            return EditorStyles.label.CalcHeight(GUIContent.none, 100);
        }

        private string DrawItem(Rect position, string item) {
            return EditorGUI.TextField(position, item);
        }

        private float GetItemHeight(UnityObject item) {
            return EditorStyles.label.CalcHeight(GUIContent.none, 100);
        }

        private UnityObject DrawItem(Rect position, UnityObject item) {
            return EditorGUI.ObjectField(position, item, typeof(UnityObject),
                /*allowSceneObjects:*/true);
        }

        /// <summary>
        /// Ensures that the given behavior has been restored so that it can be edited with the
        /// proper data populated. This also verifies that the object is displaying the most recent
        /// prefab data.
        /// </summary>
        private void EnsureRestored(ISerializedObject obj) {
            // The object may have not been restored yet.
            if (obj.Restored == false) {
                // We try to avoid calling obj.SaveState() whenever possible, as if the user is just
                // browsing a scene and a SaveState() gets called, then the user will be prompted to
                // save the scene changes to disk, even though they may have not modified anything.
                // This is decidedly bad UX, so we try to avoid it.
                //
                // Notice that we only need to do the initial save when the object has just been
                // created. The initial save is necessary so that the undo system will have a good
                // initial state to revert to.
                bool needsInitialSave = obj.SerializedStateKeys == null ||
                    obj.SerializedStateValues == null;

                obj.RestoreState();
                if (needsInitialSave) {
                    obj.SaveState();
                }

                ObjectModificationDetector.Update(obj);

                // Make sure that the prefab parent is also restored
                var prefab = PrefabUtility.GetPrefabParent(target) as ISerializedObject;
                if (prefab != null) {
                    EnsureRestored(prefab);
                }
            }

            // If the object was modified, then we want to make sure that it fully reflects the most
            // recent serialized state, so we restore the state. Notably, we don't restore while the
            // game is playing, as prefab modifications are not going to be dispatched at this
            // point.
            if (ObjectModificationDetector.WasModified(obj) &&
                EditorApplication.isPlaying == false) {

                obj.RestoreState();
                ObjectModificationDetector.Update(obj);
            }

        }

        public void OnSceneGUI() {
            var inspectedObject = target as ISerializedObject;
            if (inspectedObject == null) {
                if (FullInspectorSettings.EmitWarnings) {
                    Debug.LogError("The object inspected by " + this.GetType() +
                        " (" + target + ") must extend ISerializedObject");
                }
                return;
            }

            EnsureRestored(inspectedObject);
            Undo.RecordObject(target, "Scene GUI Modification");

            EditorGUI.BeginChangeCheck();

            // we don't want to get the IObjectPropertyEditor for the given target, which extends
            // UnityObject, so that we can actually edit the property instead of getting a Unity
            // reference field
            PropertyEditorChain editorChain = PropertyEditor.Get(target.GetType(), null);
            IPropertyEditor editor = editorChain.SkipUntilNot(typeof(IObjectPropertyEditor),
                typeof(AbstractTypePropertyEditor));

            editor.OnSceneGUI(target);

            // If the GUI has been changed, then we want to reserialize the current object state.
            // However, we don't bother doing this if we're currently in play mode, as the
            // serialized state changes will be lost regardless.
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying == false) {
                EditorUtility.SetDirty(target);
                inspectedObject.SaveState();
                ObjectModificationDetector.Update(inspectedObject);
            }
        }

        public override void OnInspectorGUI() {
            // edit the data
            var inspectedObject = target as ISerializedObject;
            if (inspectedObject == null) {
                if (FullInspectorSettings.EmitWarnings) {
                    Debug.LogError("The object inspected by " + this.GetType() +
                        " (" + target + ") must extend ISerializedObject");
                }
                return;
            }

            EnsureRestored(inspectedObject);
            Undo.RecordObject(target, "Inspector Modification");

            //-
            //-
            //-
            // Inspector based off of the property editor
            EditorGUI.BeginChangeCheck();

            // We don't want to get the IObjectPropertyEditor for the given target, which extends
            // UnityObject, so that we can actually edit the property instead of getting a Unity
            // reference field. We also don't want the AbstractTypePropertyEditor, which we will get
            // if the behavior has any derived types.
            PropertyEditorChain editorChain = PropertyEditor.Get(target.GetType(), null);
            IPropertyEditor editor = editorChain.SkipUntilNot(typeof(IObjectPropertyEditor),
                typeof(AbstractTypePropertyEditor));

            // Run the editor
            editor.EditWithGUILayout(GUIContent.none, target);

            // If the GUI has been changed, then we want to reserialize the current object state.
            // However, we don't bother doing this if we're currently in play mode, as the
            // serialized state changes will be lost regardless.
            if (EditorGUI.EndChangeCheck() && EditorApplication.isPlaying == false) {
                EditorUtility.SetDirty(target);
                inspectedObject.SaveState();

                ObjectModificationDetector.Update(inspectedObject);
            }

            //-
            //-
            //-
            // Inspector for the serialized state
            EditorGUI.BeginChangeCheck();
            DrawSerializedState(inspectedObject);
            if (EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(target);
                inspectedObject.RestoreState();

                ObjectModificationDetector.Update(inspectedObject);
            }
        }
    }
}