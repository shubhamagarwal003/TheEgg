using UnityEngine;

namespace FullInspector.Modules.Attributes {
    [CustomAttributePropertyEditor(typeof(MarginAttribute), ReplaceOthers = false)]
    public class MarginAttributeEditor<T> : AttributePropertyEditor<T, MarginAttribute> {
        protected override T Edit(Rect region, GUIContent label, T element, MarginAttribute attribute) {
            return element;
        }

        protected override float GetElementHeight(GUIContent label, T element, MarginAttribute attribute) {
            return attribute.Margin;
        }
    }
}