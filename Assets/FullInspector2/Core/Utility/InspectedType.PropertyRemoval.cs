using System;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace FullInspector.Internal {
    public partial class InspectedType {
        private static void InitializePropertyRemoval() {
            // We need to remove some properties that when viewed using reflection either
            // a) are not pretty/necessary
            // b) hard-crash Unity

            // NOTE: Because of the way the property resolution system works, we have to make sure
            //       that we remove properties from the highest level items in the inheritance
            //       hierarchy first. Otherwise, the property will show up in derived types that
            //       have already had their properties resolved.

            RemoveProperty<IntPtr>("m_value");

            RemoveProperty<UnityObject>("m_UnityRuntimeReferenceData");
            RemoveProperty<UnityObject>("m_UnityRuntimeErrorString");
            RemoveProperty<UnityObject>("name");
            RemoveProperty<UnityObject>("hideFlags");

            RemoveProperty<Component>("active");
            RemoveProperty<Component>("tag");

            RemoveProperty<Behaviour>("enabled");

            RemoveProperty<MonoBehaviour>("useGUILayout");
        }

        /// <summary>
        /// Attempts to remove the property with the given name.
        /// </summary>
        /// <param name="propertyName">The name of the property to remove.</param>
        private static void RemoveProperty<T>(string propertyName) {
            // There are currently three points where properties are cached.
            // 1. The inspected property list
            // 2. The serialized property list.
            // 3. The name to property map
            // The property has to be removed from all three storage locations.

            var type = InspectedType.Get(typeof(T));

            for (int i = 0; i < type._localInspectedProperties.Count; ++i) {
                if (type._localInspectedProperties[i].Name == propertyName) {
                    type._localInspectedProperties.RemoveAt(i);
                    break;
                }
            }

            for (int i = 0; i < type._inspectedProperties.Count; ++i) {
                if (type._inspectedProperties[i].Name == propertyName) {
                    type._inspectedProperties.RemoveAt(i);
                    break;
                }
            }

            for (int i = 0; i < type._serializedProperties.Count; ++i) {
                if (type._serializedProperties[i].Name == propertyName) {
                    type._serializedProperties.RemoveAt(i);
                    break;
                }
            }

            type._nameToProperty.Remove(propertyName);
        }
    }
}