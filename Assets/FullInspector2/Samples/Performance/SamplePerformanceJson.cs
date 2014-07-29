using System.Collections.Generic;
using UnityEngine;

namespace FullInspector.Samples.Performance {
    [AddComponentMenu("Full Inspector Samples/Performance/Json.NET")]
    public class SamplePerformanceJson : BaseBehavior<JsonNetSerializer> {
        public List<JsonSerializedClass> SerializedItems;
    }
}