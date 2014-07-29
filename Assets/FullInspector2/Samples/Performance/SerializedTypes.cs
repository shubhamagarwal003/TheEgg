using Newtonsoft.Json;
using ProtoBuf;
using System;
using UnityEngine;

namespace FullInspector.Samples.Performance {
    [ProtoContract]
    public class ProtoSerializedClass {
        [ProtoMember(1)]
        public int Int;

        [ProtoMember(2)]
        public GameObject GameObject;

        [ProtoMember(3)]
        public string String;
    }

    [Serializable]
    public class UnitySerializedClass {
        public int Int;
        public GameObject GameObject;
        public string String;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class JsonSerializedClass {
        [JsonProperty]
        public int Int;

        [JsonProperty]
        public GameObject GameObject;

        [JsonProperty]
        public string String;
    }

    [Serializable]
    public class BinarySerializedClass {
        public int Int;
        public GameObject GameObject;
        public string String;
    }
}