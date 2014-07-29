using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FullInspector.Modules.Common {
    /// <summary>
    /// The LayerMaskEditor has a fair amount of code because EditorGUI.LayerField shows
    /// a layer editor that does not support multiple layers. However, as a LayerMask can
    /// map to multiple layers, we emulate this behavior using a EditorGUI.MaskField. Things
    /// become more complex because not every layer in the mapping is shown (for example, it
    /// might be empty), so we could have only layer indices 1, 3, and 8 shown. But for a
    /// regular mask field, these three items would then map to powers-of-two 2^0, 2^1, and 2^2,
    /// but in reality we want them mapped to 2^1, 2^3, and 2^8.
    /// </summary>
    [CustomPropertyEditor(typeof(LayerMask))]
    public class LayerMaskEditor : PropertyEditor<LayerMask> {
        /// <summary>
        /// The number of layers that Unity supports. Unity doesn't directly
        /// expose the layers themselves, so we have to go through every layer
        /// index and see if there is a layer there -- hence, we need to know
        /// how many indices to search through.
        /// </summary>
        private const int UnitySupportedLayerCount = 32;

        /// <summary>
        /// The layers that we want to display to the user.
        /// </summary>
        private static string[] DisplayedLayers;

        /// <summary>
        /// Every layer that we can display. This is used so that we
        /// can get the proper power-of-two layer values when some layers
        /// are hidden.
        /// </summary>
        private static string[] AllLayers;

        static LayerMaskEditor() {
            RefreshLayers();

            LastLayerUpdate = DateTime.Now;
            EditorApplication.update += UpdateLayersCallback;
        }

        /// <summary>
        /// The last time that we updated the layer lists at.
        /// </summary>
        private static DateTime LastLayerUpdate;

        /// <summary>
        /// The amount of time between layer updates.
        /// </summary>
        private static TimeSpan LayerUpdateInterval = TimeSpan.FromSeconds(3);

        /// <summary>
        /// This function is called on EditorApplication.update so that the layer
        /// list can be updated when the user changes the layer information.
        /// </summary>
        private static void UpdateLayersCallback() {
            if (DateTime.Now > (LastLayerUpdate + LayerUpdateInterval)) {
                LastLayerUpdate = DateTime.Now;
                RefreshLayers();
            }
        }

        /// <summary>
        /// Updates the contents of the layer arrays based on the current
        /// layer list from Unity.
        /// </summary>
        private static void RefreshLayers() {
            var displayed = new List<string>();
            var all = new List<string>();

            // We have to just query Unity for every possible layer
            // that might be active. This is a slow algorithm, but
            // luckily the layer count is low so the performance impact
            // should be minimal.
            for (int i = 0; i < UnitySupportedLayerCount; i++) {
                var name = LayerMask.LayerToName(i);


                if (string.IsNullOrEmpty(name) == false) {
                    displayed.Add(name);
                    all.Add(name);
                }
                else {
                    all.Add(string.Empty);
                }
            }

            DisplayedLayers = displayed.ToArray();
            AllLayers = all.ToArray();
        }

        /// <summary>
        /// Returns the indices of the bits that are set. For example, if we
        /// have the number 60 (32 + 16 + 8 + 4, or in binary:: 00...0111100), then
        /// the list {5,4,3,2} will be returned.
        /// </summary>
        private static List<int> GetSetBitIndices(int value) {
            var setBitIndices = new List<int>();

            for (int bitIndex = 0; bitIndex < 32; ++bitIndex) {
                bool isSet = value % 2 == 1;
                value >>= 1;
                if (isSet) {
                    //int bitValue = 1 << bitIndex;
                    setBitIndices.Add(bitIndex);
                }
            }

            return setBitIndices;
        }

        /// <summary>
        /// Converts an int from one mask mapping to another mask mapping.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="to"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ConvertMaskMapping(string[] current, string[] to, int value) {
            if (value == -1) return value;

            var setBits = GetSetBitIndices(value);

            int convertedValue = 0;
            foreach (int setBit in setBits) {
                string currentValue = current[setBit];
                int mappedBit = Array.IndexOf(to, currentValue);

                if (mappedBit < 0) {
                    continue;
                }

                convertedValue += 1 << mappedBit;
            }

            return convertedValue;
        }

        public override LayerMask Edit(Rect region, GUIContent label, LayerMask element) {
            //-- 
            // Conversion algorithm:
            // Get the set bits in the layer mask, convert them from AllLayers to DisplayedLayers
            // Show the mask editor on the DisplayedLayers set
            // Convert back from DisplayLayers into AllLayers

            int displayMapping = ConvertMaskMapping(AllLayers, DisplayedLayers, element.value);
            int updated = EditorGUI.MaskField(region, label, displayMapping, DisplayedLayers);
            int unityMapping = ConvertMaskMapping(DisplayedLayers, AllLayers, updated);

            return new LayerMask() {
                value = unityMapping
            };
        }

        public override float GetElementHeight(GUIContent label, LayerMask element) {
            return EditorStyles.layerMaskField.CalcHeight(label, 100);
        }
    }
}