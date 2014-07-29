using System;
using UnityEditor;

namespace FullInspector.Serializers.ProtoBufNet {
    /// <summary>
    /// Detects when Unity is recompiling code. Before a recompile happens, we update the
    /// precompiled protobuf-net DLL.
    /// </summary>
    [InitializeOnLoad]
    public static class ProtoBufNetRecompiler {
        static ProtoBufNetRecompiler() {
#pragma warning disable 0162 // disable unreachable code detected warning
            if (ProtoBufNetSettings.AutomaticDllRecompilation) {
                EditorApplication.update += Update;
            }
#pragma warning restore
        }

        /// <summary>
        /// True if we have detected a compile but have already recompiled. This is set to false by
        /// Unity after a compilation has finished as it is not serialized and gets reset to the
        /// default value.
        /// </summary>
        [NonSerialized]
        private static bool _saved = false;

        private static void Update() {
            if (!_saved && EditorApplication.isCompiling) {
                // We set _saved to true before CompileSerializationDLL so that if
                // CompileSerializationDLL fails then we don't continuously retry

                _saved = true;
                ProtoBufNetMenus.CompileSerializationDLL();
            }
        }
    }
}