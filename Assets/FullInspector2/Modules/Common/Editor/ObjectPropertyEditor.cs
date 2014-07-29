using UnityEditor;
using UnityEngine;

namespace FullInspector.Modules.Common {
    /// <summary>
    /// Used to remove the generic arguments from ObjectPropertyEditor so that it can be used as a
    /// "banned" argument for PropertyEditor.Get
    /// </summary>
    public interface IObjectPropertyEditor {
    }

    /// <summary>
    /// Provides an ObjectField for every type which derives from Object.
    /// </summary>
    /// <typeparam name="ObjectType">The actual type of the derived parameter</typeparam>
    [CustomPropertyEditor(typeof(Object), Inherit = true)]
    public class ObjectPropertyEditor<ObjectType> : PropertyEditor<Object>, IObjectPropertyEditor {

        public override Object Edit(Rect region, GUIContent label, Object element) {
            return EditorGUI.ObjectField(region, label, element, typeof(ObjectType), true);
        }

        public override float GetElementHeight(GUIContent label, Object element) {
            return EditorStyles.objectField.CalcHeight(label, 100);
        }
    }
}