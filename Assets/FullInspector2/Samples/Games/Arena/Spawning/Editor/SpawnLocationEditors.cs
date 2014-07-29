using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FullInspector.Samples.Games.Arena {
    [CustomPropertyEditor(typeof(AbsoluteLocation))]
    public class AbsoluteLocationEditor : PropertyEditor<AbsoluteLocation> {
        public override AbsoluteLocation OnSceneGUI(AbsoluteLocation element) {
            float handleSize = 4;
            element.SpawnLocation = Handles.Slider2D(element.SpawnLocation, Vector3.up,
                Vector3.left, Vector3.forward, handleSize * 2, DrawCapFunction, 0);
            return element;
        }

        private void DrawCapFunction(int controlID, Vector3 position, Quaternion rotation, float size) {
            Handles.ConeCap(controlID, position, rotation, size);
        }
    }

    [CustomPropertyEditor(typeof(AroundObjectLocation))]
    public class AroundObjectLocationEditor : PropertyEditor<AroundObjectLocation> {
        public override AroundObjectLocation OnSceneGUI(AroundObjectLocation element) {
            Vector3 position = new Vector3();

            if (element.TrackedObject != null) {
                position = element.TrackedObject.transform.position;
            }

            position = Handles.Slider2D(position, Vector3.up, Vector3.left, Vector3.forward,
                element.Radius, DrawCapFunction, 0);
            element.Radius = Handles.ScaleValueHandle(element.Radius, position, Quaternion.identity, 10, Handles.CubeCap, 0);

            if (element.TrackedObject != null) {
                element.TrackedObject.transform.position = position;
            }

            return element;
        }

        private void DrawCapFunction(int controlID, Vector3 position, Quaternion rotation, float size) {
            Handles.CircleCap(controlID, position, rotation, size);
        }
    }
}