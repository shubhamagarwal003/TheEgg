using System;

namespace FullInspector {
    /// <summary>
    /// Adding [InspectorHidePrimary] will cause the primary inspector for the target to not be
    /// displayed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class InspectorHidePrimaryAttribute : Attribute, IInspectorAttributeOrder {
        double IInspectorAttributeOrder.Order {
            get {
                return double.MaxValue;
            }
        }
    }
}