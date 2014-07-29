using FullInspector.Internal;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace FullInspector {
    /// <summary>
    /// Set the display order of an field or property of an object. A field or property without an
    /// order defaults to order double.MaxValue. Each inheritance level receives its own order
    /// group.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class InspectorOrderAttribute : Attribute {
        /// <summary>
        /// The ordering of this member relative to other ordered fields/properties.
        /// </summary>
        public double Order;

        /// <summary>
        /// Set the order.
        /// </summary>
        /// <param name="order">The order in which to display this field or property. A field or
        /// property without an InspectorOrderAttribute defaults to order double.MaxValue.</param>
        public InspectorOrderAttribute(double order) {
            Order = order;
        }

        /// <summary>
        /// Helper method to determine the inspector order for the given member. If the
        /// member does not have an [InspectorOrder] attribute, then the default order is returned.
        /// </summary>
        public static double GetInspectorOrder(MemberInfo memberInfo) {
            var attr = memberInfo.GetAttribute<InspectorOrderAttribute>();
            if (attr != null) {
                return attr.Order;
            }
            return double.MaxValue;
        }
    }
}