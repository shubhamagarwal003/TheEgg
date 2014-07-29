using FullInspector.Internal;
using System;
using UnityEditor;
using UnityEngine;

namespace FullInspector.Modules.StaticInspector {
    public class StaticInspectorWindow : EditorWindow {
        public static int MyStaticInt;

        [MenuItem("Window/Full Inspector/Static Inspector (beta - may crash Unity)")]
        public static void Activate() {
            // We try to dock next to the inspector window. This is a hack, since Unity doesn't
            // actually expose the UnityEditor.InspectorWindow type -- we look it up ourselves.
            // Hopefully Unity doesn't change the type name in any of the future releases.

            Type inspectorWindow = TypeCache.FindType("UnityEditor.InspectorWindow");
            var window = EditorWindow.GetWindow<StaticInspectorWindow>(inspectorWindow);
            window.minSize = new Vector2(300, 400);
            window.title = "Static Inspector";
        }

        /// <summary>
        /// The current scrolling position for the static inspector.
        /// </summary>
        private Vector2 _inspectorScrollPosition;

        /// <summary>
        /// The type that we are currently viewing the statics for. Unfortunately, we have to store
        /// this type as a string so that Unity can serialize it. It would be awesome to have FI
        /// serialization on EditorWindows, but oh well :P.
        /// </summary>
        private string _serializedInspectedType;
        private Type _inspectedType {
            get {
                return TypeCache.FindType(_serializedInspectedType, null, /*willThrow:*/false);
            }
            set {
                if (value == null) {
                    _serializedInspectedType = string.Empty;
                }
                else {
                    _serializedInspectedType = value.FullName;
                }
            }
        }

        public void OnGUI() {
            try {
                // For some reason, the type selection popup window cannot force the rest of the
                // Unity GUI to redraw. We do it here instead -- this removes any delay after
                // selecting the type in the popup window and the type actually being displayed.
                if (fiEditorUtility.ShouldInspectorRedraw) {
                    Repaint();
                }


                GUILayout.Label("Static Inspector", EditorStyles.boldLabel);

                EditorGUILayout.HelpBox("The static inspector allows you view the static " +
                    "variables of any type. This feature is still in beta but in testing has " +
                    "been stable. Please make sure to backup your work before using this Full " +
                    "Inspector experimental feature.", MessageType.Warning);
                EditorGUILayout.HelpBox("Changing variables that are structs does not always work",
                    MessageType.Info);

                {
                    var label = new GUIContent("Inspected Type");
                    var typeEditor = PropertyEditor.Get(typeof(Type), null);

                    _inspectedType = typeEditor.FirstEditor.EditWithGUILayout(label, _inspectedType);
                }

                fiEditorGUILayout.Splitter(2);

                if (_inspectedType != null) {
                    _inspectorScrollPosition = EditorGUILayout.BeginScrollView(_inspectorScrollPosition);

                    var inspectedType = InspectedType.Get(_inspectedType);
                    foreach (InspectedProperty staticProperty in inspectedType.StaticProperties) {
                        var editorChain = PropertyEditor.Get(staticProperty.StorageType, staticProperty.MemberInfo);
                        IPropertyEditor editor = editorChain.FirstEditor;

                        GUIContent label = new GUIContent(staticProperty.Name);
                        object currentValue = staticProperty.Read(null);
                        object updatedValue = editor.EditWithGUILayout(label, currentValue);

                        staticProperty.Write(null, updatedValue);
                    }

                    EditorGUILayout.EndScrollView();
                }
            }
            catch (ExitGUIException) { }
            catch (Exception e) {
                Debug.LogError(e);
            }
        }
    }
}