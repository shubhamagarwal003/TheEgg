﻿using FullInspector.Internal;
using Newtonsoft.Json;
using System;
using UnityObject = UnityEngine.Object;

namespace FullInspector.Serializers.JsonNet {
    /// <summary>
    /// Converts all types that derive from UnityObject.
    /// </summary>
    public class UnityObjectConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return typeof(UnityObject).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            // null may have been serialized automatically by Json.NET, so we need to recover handle
            // the null case
            if (reader.TokenType == JsonToken.Null) {
                return null;
            }

            var serializationOperator = (ISerializationOperator)serializer.Context.Context;

            int componentId = serializer.Deserialize<int>(reader);
            return serializationOperator.RetrieveObjectReference(componentId);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var obj = (UnityObject)value;

            var serializationOperator = (ISerializationOperator)serializer.Context.Context;

            int id = serializationOperator.StoreObjectReference(obj);
            serializer.Serialize(writer, id);
        }
    }
}