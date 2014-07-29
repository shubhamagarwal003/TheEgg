using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullInspector {
    /// <summary>
    /// Display the given method as a button in the inspector.
    /// </summary>
    /// <remarks>
    /// It may be useful to also combine this with the InspectorOrder attribute to customize the
    /// order that the button is displayed in w.r.t. other elements.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class InspectorButtonAttribute : Attribute {
        /// <summary>
        /// The name of the button. If this is null or the empty string, then a default name
        /// generated off of the method name that this attribute targets should be used instead.
        /// </summary>
        public string DisplayName;

        /// <summary>
        /// Creates a button with a default name generated based off of the method name.
        /// </summary>
        public InspectorButtonAttribute() : this("") { }

        /// <summary>
        /// Set the name of the button.
        /// </summary>
        public InspectorButtonAttribute(string displayName) {
            DisplayName = displayName;
        }
    }
}