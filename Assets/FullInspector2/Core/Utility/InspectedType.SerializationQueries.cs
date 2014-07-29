using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace FullInspector.Internal {
    /// <summary>
    /// This partial implementation contains methods which are used to determine if a MemberInfo can
    /// be serialized by Unity or if it can be serialized by Full Inspector.
    /// </summary>
    public partial class InspectedType {
        /// <summary>
        /// A simple type is a type that is either primitive, a string, or a non-generic
        /// non-abstract class composed of other simple types.
        /// </summary>
        private static bool IsSimpleType(Type type) {
            if (type.IsPrimitive) {
                return true;
            }

            if (type == typeof(string)) {
                return true;
            }

            // TODO: Enable more complex detection for types which Unity can serialize by
            //       uncommenting this block. First, test it, though.
            /*
            if (type.IsClass && type.IsGenericType == false && type.IsAbstract == false) {
                BindingFlags flags =
                        BindingFlags.Instance |
                        BindingFlags.FlattenHierarchy |
                        BindingFlags.Public |
                        BindingFlags.NonPublic;

                // The type might contain a property that should be serialized.
                if (type.GetProperties(flags).Length > 0) {
                    return false;
                }

                // The type can have fields composed only of other simple types.
                // TODO: will this recurse infinitely for recursive types?
                foreach (var field in type.GetFields(flags)) {
                    if (IsSimpleType(field.FieldType) == false) {
                        return false;
                    }
                }

                return true;
            }
            */

            return false;
        }

        /// <summary>
        /// Returns true if the given type can be serialized by Unity. This function is conservative
        /// and may not return true if the type can be serialized by unity. However, it will *not*
        /// return true if the type cannot be serialized by unity.
        /// </summary>
        private static bool IsSerializedByUnity(InspectedProperty property) {
            // Properties are *not* serialized by Unity
            if (property.MemberInfo is PropertyInfo) {
                return false;
            }

            Type type = property.StorageType;

            return
                // Basic primitive types
                IsSimpleType(type) ||

                // A UnityObject derived type
                typeof(UnityObject).IsAssignableFrom(type) ||

                // Array (but not a multidimensional one)
                (type.IsArray && type.GetElementType().IsArray == false && IsSimpleType(type.GetElementType())) ||

                // Lists of already serializable types
                (
                    type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(List<>) &&
                    IsSimpleType(type.GetGenericArguments()[0])
                );
        }

        /// <summary>
        /// Returns true if the given property should be serialized.
        /// </summary>
        private static bool IsSerializedByFullInspector(InspectedProperty property) {
            MemberInfo member = property.MemberInfo;

            // if it has NonSerialized, then we *don't* serialize it
            if (member.GetAttribute<NonSerializedAttribute>() != null ||
                member.GetAttribute<NotSerializedAttribute>() != null) {
                return false;
            }

            // if we have a [SerializeField] or [Serializable] attribute, then we *do* serialize
            if (member.GetAttribute<SerializeField>() != null ||
                member.GetAttribute<SerializableAttribute>() != null) {
                return true;
            }

            // otherwise we serialize by default only if it's public
            return property.IsPublic;
        }
    }
}