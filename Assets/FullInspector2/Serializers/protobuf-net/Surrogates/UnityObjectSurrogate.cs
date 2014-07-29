using FullInspector.Internal;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Linq;
using System.Runtime.Serialization;
using UnityObject = UnityEngine.Object;

namespace FullInspector.Serializers.ProtoBufNet {
    // iOS and WebPlayer have AOT compilers; MakeGenericType doesn't work.
    public class UnityObjectModelWorker : IProtoModelWorker {
        public void Work(RuntimeTypeModel model) {
#if !UNITY_IOS && !UNITY_WEBPLAYER
            foreach (Type unityObjectType in RuntimeReflectionUtilities.GetUnityObjectTypes()) {
                var surrogateType = typeof(UnityObjectSurrogate<>).MakeGenericType(unityObjectType);

                model.Add(unityObjectType, false)
                    .SetSurrogate(surrogateType);
            }
#endif
        }
    }

    [ProtoContract]
    public class UnityObjectSurrogate<TObject> where TObject : UnityObject {
        [ProtoMember(1, IsRequired = true)]
        public int StorageId;

        public UnityObject Reference;

        [OnDeserializing]
        public void OnDeserializing(StreamingContext context) {
            var ops = (ISerializationOperator)context.Context;
            Reference = ops.RetrieveObjectReference(StorageId);
        }

        [OnSerialized]
        public void OnSerialized(StreamingContext context) {
            var ops = (ISerializationOperator)context.Context;
            StorageId = ops.StoreObjectReference(Reference);
        }

        public static explicit operator TObject(UnityObjectSurrogate<TObject> surrogate) {
            return (TObject)surrogate.Reference;
        }

        public static explicit operator UnityObjectSurrogate<TObject>(TObject reference) {
            return new UnityObjectSurrogate<TObject>() {
                Reference = reference
            };
        }
    }
}