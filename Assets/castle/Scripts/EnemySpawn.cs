using System.Collections;
using UnityEngine;

namespace Assets.castle.Scripts
{
    public class EnemySpawn : MonoBehaviour
    {

        public Enemy[] EnemiesToSpawn;

        public float SpawnInterval = 1;
        public int SpawnNumber = 200;
        public float SpawnNumVariation = 10;

        // Use this for initialization
        void Start()
        {
            StartCoroutine(Spawn());
        }

        // Update is called once per frame
        void Update()
        {

        }


        IEnumerator Spawn()
        {
            while (true)
            {
                var numToSpawn = SpawnNumber * (1 + Random.Range(-1f, 1f) * SpawnNumVariation * .01f);
                for (int i = 0; i < numToSpawn; i++)
                {
                    Instantiate(EnemiesToSpawn[Random.Range(0, EnemiesToSpawn.Length - 1)], transform.position, Quaternion.identity);
                }
                yield return new WaitForSeconds(SpawnInterval);
            }

        }

    }
}
