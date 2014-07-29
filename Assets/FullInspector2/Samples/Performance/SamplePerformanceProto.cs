using System.Collections.Generic;
using UnityEngine;

namespace FullInspector.Samples.Performance {
    [AddComponentMenu("Full Inspector Samples/Performance/protobuf-net")]
    public class SamplePerformanceProto : BaseBehavior<ProtoBufNetSerializer> {
        public List<ProtoSerializedClass> SerializedItems;
    }
}