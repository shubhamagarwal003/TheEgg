using UnityEditor;
using UnityEngine;

namespace FullInspector.Samples.JsonNet {
    [CustomPropertyEditor(typeof(SampleJsonCustomBehaviorEditor))]
    public class SampleJsonCustomBehaviorEditorEditor :
        PropertyEditor<SampleJsonCustomBehaviorEditor> {

        public override SampleJsonCustomBehaviorEditor Edit(Rect region, GUIContent label, SampleJsonCustomBehaviorEditor element) {
            EditorGUI.HelpBox(region, "This is the custom editor for SampleJsonCustomBehaviorEditor", MessageType.Info);
            return element;
        }

        public override float GetElementHeight(GUIContent label, SampleJsonCustomBehaviorEditor element) {
            return 30;
        }
    }

}