using FullInspector.Samples.MinMaxSample;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullInspector.Samples.Games.Arena {
    /// <summary>
    /// The main AI logic behind the enemies, TrackingBehavior merely selects the closest
    /// PlayerBehavior and attempts to collide with it.
    /// </summary>
    [AddComponentMenu("Full Inspector Samples/Games/Arena/Tracking Behavior")]
    public class SampleArenaTrackingBehavior : BaseBehavior {
        [Comment("The speed that the object will move at is randomly selected from within this range")]
        public MinMax<float> Speed = new MinMax<float>() {
            MinLimit = 0,
            MaxLimit = 2
        };

        [SerializeField]
        private float _speed;

        protected override void Awake() {
            base.Awake();
            _speed = UnityEngine.Random.Range(Speed.Min, Speed.Max);
        }

        protected void OnTriggerEnter(Collider collider) {
            Destroy(gameObject);
        }

        protected void FixedUpdate() {
            SampleArenaPlayerController controller;

            if (GetClosestController(transform.position, out controller)) {
                Vector3 direction = controller.transform.position - transform.position;
                direction = direction.normalized * _speed;

                transform.position += direction;
            }
        }

        private static bool GetClosestController(Vector3 position, out SampleArenaPlayerController controller) {
            controller = null;
            float minDist = float.MaxValue;

            for (int i = 0; i < SampleArenaPlayerController.AllControllers.Count; ++i) {
                SampleArenaPlayerController possibleController = SampleArenaPlayerController.AllControllers[i];
                float dist = Vector3.Distance(position, possibleController.transform.position);
                if (minDist > dist) {
                    controller = possibleController;
                    minDist = dist;
                }
            }

            return controller != null;
        }
    }
}