using System;
using UnityEditor;
using UnityEngine;

namespace FullInspector.Modules.Common {
    [CustomPropertyEditor(typeof(DateTime))]
    public class DateTimePropertyEditor : PropertyEditor<DateTime> {
        public override DateTime Edit(Rect region, GUIContent label, DateTime element) {
            string updated = EditorGUI.TextField(region, label, element.ToString());

            DateTime result;
            if (DateTime.TryParse(updated, out result)) {
                return result;
            }

            return element;
        }

        public override float GetElementHeight(GUIContent label, DateTime element) {
            return EditorStyles.label.CalcHeight(label, 100);
        }
    }
}