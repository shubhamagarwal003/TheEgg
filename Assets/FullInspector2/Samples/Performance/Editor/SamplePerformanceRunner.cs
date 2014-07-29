using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FullInspector.Samples.Performance {
    public class SamplePerformanceRunner {
        private static int SerializedItemCount = 1000;
        private static int UnityObjectFrequency = 100;
        private static int TestRuns = 500;

        private static void PrepareObjects(out GameObject unityObject, out GameObject protoObject,
            out GameObject jsonObject, out GameObject binaryObject) {

            unityObject = new GameObject("Performance Object (Unity)");
            protoObject = new GameObject("Performance Object (Proto)");
            jsonObject = new GameObject("Performance Object (Json)");
            binaryObject = new GameObject("Performance Object (Binary)");

            var unityComponent = unityObject.AddComponent<SamplePerformanceUnity>();
            var protoComponent = protoObject.AddComponent<SamplePerformanceProto>();
            var jsonComponent = jsonObject.AddComponent<SamplePerformanceJson>();
            var binaryComponent = binaryObject.AddComponent<SamplePerformanceBinary>();

            unityComponent.SerializedItems = new List<UnitySerializedClass>();
            protoComponent.SerializedItems = new List<ProtoSerializedClass>();
            jsonComponent.SerializedItems = new List<JsonSerializedClass>();
            binaryComponent.Proxy.SerializedItems = new List<BinarySerializedClass>();

            for (int i = 0; i < SerializedItemCount; ++i) {
                var Int = i;
                var GameObject = i % UnityObjectFrequency == 0 ? unityObject : null;
                var String = i.ToString();

                unityComponent.SerializedItems.Add(new UnitySerializedClass() {
                    Int = Int,
                    GameObject = GameObject,
                    String = String
                });

                protoComponent.SerializedItems.Add(new ProtoSerializedClass() {
                    Int = Int,
                    GameObject = GameObject,
                    String = String
                });

                jsonComponent.SerializedItems.Add(new JsonSerializedClass() {
                    Int = Int,
                    GameObject = GameObject,
                    String = String
                });

                binaryComponent.Proxy.SerializedItems.Add(new BinarySerializedClass() {
                    Int = Int,
                    GameObject = GameObject,
                    String = String
                });
            }

            protoComponent.SaveState();
            jsonComponent.SaveState();
            binaryComponent.SaveState();
        }

        [MenuItem("Window/Full Inspector/Run Performance Sample")]
        public static void RunPerformanceSample() {
            if (Application.isPlaying == false) {
                Debug.LogError("Please enter Play mode before running the performance sample");
                return;
            }

            GameObject unityObject, protoObject, jsonObject, binaryObject;
            PrepareObjects(out unityObject, out protoObject, out jsonObject, out binaryObject);

            long averageInstantiateUnity = RunBenchmark(unityObject);
            long averageInstantiateProto = RunBenchmark(protoObject);
            long averageInstantiateJson = RunBenchmark(jsonObject);
            long averageInstantiateBinary = RunBenchmark(binaryObject);

            //Object.DestroyImmediate(unityObject);
            //Object.DestroyImmediate(protoObject);
            //Object.DestroyImmediate(jsonObject);
            //Object.DestroyImmediate(binaryObject);

            Debug.Log(string.Format(
                "SerializedItemCount: {0}\n" +
                "UnityObjectFrequency: {1}\n" +
                "TestRuns: {2}\n" +
                "\n" +
                "Average Unity Instantiate Time:  {3}\n" +
                "Average Proto Instantiate Time:  {4}\n" +
                "Average Json Instantiate Time:   {5}\n" +
                "Average Binary Instantiate Time: {6}\n",
                SerializedItemCount,
                UnityObjectFrequency,
                TestRuns,
                averageInstantiateUnity,
                averageInstantiateProto,
                averageInstantiateJson,
                averageInstantiateBinary));
        }

        private static long RunBenchmark(GameObject gameObject) {
            var ticks = new List<long>();

            // run the tests
            for (int i = 0; i < TestRuns; ++i) {
                var stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
                var spawned = Object.Instantiate(gameObject);
                stopwatch.Stop();
                ticks.Add(stopwatch.ElapsedTicks);
                Object.DestroyImmediate(spawned);
            }

            // compute the average
            long total = 0;
            foreach (long tick in ticks) {
                total += tick;
            }
            return total / ticks.Count;
        }
    }
}