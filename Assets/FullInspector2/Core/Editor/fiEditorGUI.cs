using UnityEditor;
using UnityEngine;

namespace FullInspector.Internal {
    public class fiEditorGUI {
        // see http://answers.unity3d.com/questions/216584/horizontal-line.html

        private static readonly GUIStyle splitter;

        static fiEditorGUI() {
            splitter = new GUIStyle();
            splitter.normal.background = EditorGUIUtility.whiteTexture;
            splitter.stretchWidth = true;
            splitter.margin = new RectOffset(0, 0, 7, 7);
        }

        private static readonly Color splitterColor = EditorGUIUtility.isProSkin ?
            new Color(0.157f, 0.157f, 0.157f) : new Color(0.5f, 0.5f, 0.5f);


        // GUI Style
        public static void Splitter(Rect position) {
            if (Event.current.type == EventType.Repaint) {
                Color restoreColor = GUI.color;
                GUI.color = splitterColor;
                splitter.Draw(position, false, false, false, false);
                GUI.color = restoreColor;
            }
        }
    }
}