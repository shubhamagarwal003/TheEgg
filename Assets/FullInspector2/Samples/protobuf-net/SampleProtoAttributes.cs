using UnityEngine;

namespace FullInspector.Samples.ProtoBufNet {
    [AddComponentMenu("Full Inspector Samples/protobuf-net/Attributes")]
    public class SampleProtoAttributes : BaseBehavior<ProtoBufNetSerializer> {
        [Comment("Welcome! Please click on the game object of choice to see how to serialize and inspect objects relating to the specified Unity type.")]
        public int ReadMeFirst;

        [Tooltip("Hey, there. This is a tooltip from an attribute!")]
        public int HoverOverMe;

        [Margin(50)]
        [Comment("Wow! I'm pretty far away! Thanks Margin!")]
        public int WayBelow;
    }
}