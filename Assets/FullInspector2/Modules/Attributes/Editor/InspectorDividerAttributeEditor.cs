using FullInspector.Internal;
using UnityEngine;

namespace FullInspector.Modules.Attributes {
    [CustomAttributePropertyEditor(typeof(InspectorDividerAttribute), ReplaceOthers = false)]
    public class InspectorDividierAttributeEditor<T> : AttributePropertyEditor<T, InspectorDividerAttribute> {
        protected override T Edit(Rect region, GUIContent label, T element, InspectorDividerAttribute attribute) {
            fiEditorGUI.Splitter(region);
            return element;
        }

        protected override float GetElementHeight(GUIContent label, T element, InspectorDividerAttribute attribute) {
            return 2;
        }
    }
}