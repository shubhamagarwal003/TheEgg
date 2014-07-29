using UnityEditor;
using UnityEngine;

namespace FullInspector.Samples.BinaryFormatterSample {
    [CustomPropertyEditor(typeof(SampleBinaryFormatterCustomBehaviorEditor))]
    public class SampleBinaryFormatterCustomBehaviorEditorEditor :
        PropertyEditor<SampleBinaryFormatterCustomBehaviorEditor> {

        public override SampleBinaryFormatterCustomBehaviorEditor Edit(Rect region, GUIContent label, SampleBinaryFormatterCustomBehaviorEditor element) {
            EditorGUI.HelpBox(region, "This is the custom editor for SampleBinaryFormatterCustomBehaviorEditor", MessageType.Info);
            return element;
        }

        public override float GetElementHeight(GUIContent label, SampleBinaryFormatterCustomBehaviorEditor element) {
            return 30;
        }
    }

}