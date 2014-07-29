using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullInspector.Samples.Performance {
    [AddComponentMenu("Full Inspector Samples/Performance/BinaryFormatter")]
    public class SamplePerformanceBinary : BaseBehavior<BinaryFormatterSerializer> {
        [Serializable]
        public struct ProxyContainer {
            public List<BinarySerializedClass> SerializedItems;
        }

        // We have to throw the SerializedItems into a proxy container so that Unity won't serialize
        // them.
        public ProxyContainer Proxy;
    }
}