using System.Collections;
using UnityEngine;

namespace FullInspector.Samples.Games.Arena {
    [AddComponentMenu("Full Inspector Samples/Games/Arena/Spawn Behavior")]
    public class SampleArenaSpawnBehavior : BaseBehavior {
        [Comment("Where objects are spawned at")]
        public ISpawnLocation SpawnLocation;

        [Comment("Controls how objects are spawned")]
        public ISpawnProvider SpawnProvider;

        protected override void Awake() {
            base.Awake();

            if (SpawnLocation == null) {
                Debug.LogError("Please assign a spawn location", this);
                return;
            }

            if (SpawnProvider == null) {
                Debug.LogError("Please assign a spawn provider", this);
                return;
            }

            StartCoroutine(SpawnProvider.Update(SpawnLocation));
        }
    }
}