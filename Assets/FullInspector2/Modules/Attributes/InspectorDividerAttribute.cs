using System;
using UnityEngine;

namespace FullInspector {
    /// <summary>
    /// Adds a margin of whitespace above the given field or property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class InspectorDividerAttribute : Attribute, IInspectorAttributeOrder {
        public double Order = 300;

        double IInspectorAttributeOrder.Order {
            get {
                return Order;
            }
        }
    }
}