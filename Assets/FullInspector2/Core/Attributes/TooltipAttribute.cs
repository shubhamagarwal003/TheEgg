﻿using System;

namespace FullInspector {
    /// <summary>
    /// Adds a tooltip to an field or property that is viewable in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class TooltipAttribute : Attribute {
        public string Tooltip;

        public TooltipAttribute(string tooltip) {
            Tooltip = tooltip;
        }
    }
}