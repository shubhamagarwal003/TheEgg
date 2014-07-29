using System;
using UnityEngine;

namespace FullInspector.Samples.Games.Arena {
    /// <summary>
    /// Moves an object along the x axis in a sinusoidal wave.
    /// </summary>
    [AddComponentMenu("Full Inspector Samples/Games/Arena/Tick-Tock Movement Behavior")]
    public class SampleArenaTickTockMovementBehavior : BaseBehavior {
        public float Scale = .1f;

        protected void FixedUpdate() {
            var pos = transform.position;
            transform.position = new Vector3(pos.x + (Scale * (float)Math.Sin(Time.time)), pos.y, pos.z);
        }
    }
}