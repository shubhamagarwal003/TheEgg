using UnityEditor;
using UnityEngine;

namespace FullInspector.Internal {
    internal static class fiEditorUtility {
        private static int _renderCount;

        public static bool ShouldInspectorRedraw {
            get {
                return _renderCount > 0;
            }
        }

        public static void PushInspectorRedraw() {
            ++_renderCount;
        }

        public static void PopInspectorRedraw() {
            --_renderCount;

            if (_renderCount < 0) {
                _renderCount = 0;
            }
        }

        /// <summary>
        /// Attempts to fetch a MonoScript that is associated with the given obj.
        /// </summary>
        /// <param name="obj">The object to fetch the script for.</param>
        /// <param name="script">The script, if found.</param>
        /// <returns>True if there was a script, false otherwise.</returns>
        public static bool TryGetMonoScript(object obj, out MonoScript script) {
            script = null;

            if (obj is MonoBehaviour) {
                var behavior = (MonoBehaviour)obj;
                script = MonoScript.FromMonoBehaviour(behavior);
            }

            else if (obj is ScriptableObject) {
                var scriptable = (ScriptableObject)obj;
                script = MonoScript.FromScriptableObject(scriptable);
            }

            return script != null;
        }

        /// <summary>
        /// Returns true if the given obj has a MonoScript associated with it.
        /// </summary>
        public static bool HasMonoScript(object obj) {
            MonoScript script;
            return TryGetMonoScript(obj, out script);
        }
    }
}