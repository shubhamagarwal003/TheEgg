using ProtoBuf;
using UnityEngine;

namespace FullInspector.Serializers.ProtoBufNet {
    [ProtoContract]
    [ProtoSurrogate(typeof(LayerMask))]
    public class LayerMaskSurrogate {
        [ProtoMember(1)]
        public int value;

        public static explicit operator LayerMask(LayerMaskSurrogate surrogate) {
            return new LayerMask() {
                value = surrogate.value
            };
        }

        public static explicit operator LayerMaskSurrogate(LayerMask mask) {
            return new LayerMaskSurrogate() {
                value = mask.value
            };
        }
    }
}