using UnityEditor;
using UnityEngine;

namespace FullInspector.Samples.ProtoBufNet {
    [CustomPropertyEditor(typeof(SampleProtoCustomBehaviorEditor))]
    public class SampleProtoCustomBehaviorEditorEditor :
        PropertyEditor<SampleProtoCustomBehaviorEditor> {

        public override SampleProtoCustomBehaviorEditor Edit(Rect region, GUIContent label, SampleProtoCustomBehaviorEditor element) {
            EditorGUI.HelpBox(region, "This is the custom editor for SampleProtoCustomBehaviorEditor", MessageType.Info);
            return element;
        }

        public override float GetElementHeight(GUIContent label, SampleProtoCustomBehaviorEditor element) {
            return 30;
        }
    }

}