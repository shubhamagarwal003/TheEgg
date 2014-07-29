using FullInspector.Samples.MinMaxSample;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace FullInspector.Samples.Games.Arena {
    public interface ISpawnProvider {
        IEnumerator<YieldInstruction> Update(ISpawnLocation spawnLocation);
    }

    /// <summary>
    /// Spawns an object every n seconds.
    /// </summary>
    public class FixedCountSpawner : ISpawnProvider {
        [Comment("The spawned object")]
        public GameObject SpawnedPrefab;

        [Comment("Seconds between each spawn")]
        public float Delay;

        public IEnumerator<YieldInstruction> Update(ISpawnLocation spawnLocation) {
            while (true) {
                yield return new WaitForSeconds(Delay);
                UnityObject.Instantiate(SpawnedPrefab, spawnLocation.GetSpawnLocation(), Quaternion.identity);
            }
        }
    }

    /// <summary>
    /// Spawns objects in a burst
    /// </summary>
    public class BurstSpawner : ISpawnProvider {
        [Comment("The spawned object")]
        public GameObject SpawnedPrefab;

        [Comment("Seconds between burst spawns")]
        public float IntermissionDelay;

        [Comment("Seconds between spawns when in burst mode")]
        public float BurstDelay;

        [Comment("The number of spawned objects per burst")]
        public MinMax<int> BurstCount = new MinMax<int>() {
            MinLimit = 0,
            MaxLimit = 50
        };

        public IEnumerator<YieldInstruction> Update(ISpawnLocation spawnLocation) {
            while (true) {
                int burstCount = UnityEngine.Random.Range(BurstCount.Min, BurstCount.Max);
                for (int i = 0; i < burstCount; ++i) {
                    UnityObject.Instantiate(SpawnedPrefab, spawnLocation.GetSpawnLocation(), Quaternion.identity);
                    yield return new WaitForSeconds(BurstDelay);
                }

                yield return new WaitForSeconds(IntermissionDelay);
            }
        }
    }
}