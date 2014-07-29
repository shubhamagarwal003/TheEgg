using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullInspector.Samples.Games.Arena {
    public interface ISpawnLocation {
        Vector3 GetSpawnLocation();
    }

    public class AbsoluteLocation : ISpawnLocation {
        public Vector3 SpawnLocation;

        public Vector3 GetSpawnLocation() {
            return SpawnLocation;
        }
    }

    public class AroundObjectLocation : ISpawnLocation {
        [Comment("The object to get the spawn position from")]
        public GameObject TrackedObject;

        [Comment("The radius around the parent's location")]
        public float Radius = 1;

        public Vector3 GetSpawnLocation() {
            var pos = new Vector3();
            if (TrackedObject != null) {
                pos = TrackedObject.transform.position;
            }

            Vector2 unitCircle = UnityEngine.Random.insideUnitCircle * Radius;
            return new Vector3(pos.x + unitCircle.x, pos.y, pos.z + unitCircle.y);
        }
    }
}