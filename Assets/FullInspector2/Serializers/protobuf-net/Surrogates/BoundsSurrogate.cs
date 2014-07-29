using ProtoBuf;
using UnityEngine;

namespace FullInspector.Serializers.ProtoBufNet {
    [ProtoContract]
    [ProtoSurrogate(typeof(Bounds))]
    public class BoundsSurrogate {
        [ProtoMember(1)]
        public Vector3 center;

        [ProtoMember(2)]
        public Vector3 size;

        public static explicit operator Bounds(BoundsSurrogate surrogate) {
            return new Bounds(surrogate.center, surrogate.size);
        }

        public static explicit operator BoundsSurrogate(Bounds bounds) {
            return new BoundsSurrogate() {
                center = bounds.center,
                size = bounds.size
            };
        }
    }
}