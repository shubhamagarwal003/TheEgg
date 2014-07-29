using System;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityObject = UnityEngine.Object;
using FullInspector.Internal;

namespace FullInspector {
    public static class FullInspectorSaveManager {
        /// <summary>
        /// Forcibly save the state of all objects which derive from ISerializedObject.
        /// ISerializedObject saving is managed automatically when you use the editor (and can be
        /// customized in FullInspectorSettings). However, if you're playing a game and make a save
        /// level, ensure to call FullInspectorSaveManager.SaveAll()
        /// </summary>
#if UNITY_EDITOR
        [MenuItem("Window/Full Inspector/Save All")]
#endif
        public static void SaveAll() {
            foreach (Type serializedObjectType in
                RuntimeReflectionUtilities.AllSimpleTypesDerivingFrom(typeof(ISerializedObject))) {

                UnityObject[] objects = UnityObject.FindObjectsOfType(serializedObjectType);
                for (int i = 0; i < objects.Length; ++i) {
                    var obj = (ISerializedObject)objects[i];
                    obj.SaveState();
                }
            }
        }

        /// <summary>
        /// Forcibly restore the state of all objects which derive from ISerializedObject.
        /// </summary>
#if UNITY_EDITOR
        [MenuItem("Window/Full Inspector/Restore All")]
#endif
        public static void RestoreAll() {
            foreach (Type serializedObjectType in
                RuntimeReflectionUtilities.AllSimpleTypesDerivingFrom(typeof(ISerializedObject))) {

                UnityObject[] objects = UnityObject.FindObjectsOfType(serializedObjectType);
                for (int i = 0; i < objects.Length; ++i) {
                    var obj = (ISerializedObject)objects[i];
                    obj.RestoreState();
                }
            }
        }

    }
}