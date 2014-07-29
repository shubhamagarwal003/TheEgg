using FullInspector.Internal;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FullInspector.Serializers.ProtoBufNet {
    /// <summary>
    /// Manages the dynamic construction of a protobuf-net TypeModel that is used for serialization.
    /// </summary>
    public static class TypeModelCreator {
        private struct NonGenericSurrogate {
            /// <summary>
            /// The type that is being modeled by the surrogate.
            /// </summary>
            public Type ModeledType;

            /// <summary>
            /// The surrogate type that is being used to serialize the model type.
            /// </summary>
            public Type SurrogateType;
        }

        /// <summary>
        /// Non-generic surrogate types are a surrogate for non-generic types
        /// </summary>
        private static IEnumerable<NonGenericSurrogate> GetNonGenericSurrogates() {
            return from assembly in RuntimeReflectionUtilities.GetRuntimeAssemblies()
                   from surrogateType in assembly.GetTypes()

                   let surrogateAttribute = surrogateType.GetAttribute<ProtoSurrogateAttribute>()
                   where surrogateAttribute != null

                   where surrogateAttribute.SurrogateFor.IsGenericTypeDefinition == false

                   select new NonGenericSurrogate() {
                       ModeledType = surrogateAttribute.SurrogateFor,
                       SurrogateType = surrogateType
                   };
        }

        private struct GenericSurrogate {
            /// <summary>
            /// The type that is being modeled by the surrogate.
            /// </summary>
            public Type GenericModeledType;

            /// <summary>
            /// The surrogate type that is being used to serialize the model type.
            /// </summary>
            public Type GenericSurrogateType;

            /// <summary>
            /// The type parameters to instantiate GenericModeledType and GenericSurrogateType with.
            /// </summary>
            public IEnumerable<Type[]> TypeArguments;
        }

        /// <summary>
        /// Types which have a [ProtoContract] attribute but not a [ProtoSurrogate] attribute
        /// </summary>
        private static IEnumerable<Type> GetContracts() {
            return from assembly in RuntimeReflectionUtilities.GetRuntimeAssemblies()
                   from contractType in assembly.GetTypes()

                   where contractType.GetAttribute<ProtoContractAttribute>() != null
                   where contractType.GetAttribute<ProtoSurrogateAttribute>() == null

                   // generic contract types are useless by themselves; they are only used when
                   // requested by some other type that is being serialized, which in that case they
                   // will be instantiated with generic type parameters
                   where contractType.IsGenericTypeDefinition == false

                   select contractType;
        }

        private static Type RunSanityCheck(Type type) {
            if ((type.IsPublic || type.IsNestedPublic) == false) {
                Throw("sanity check -- type is not public for type={0}", type);
            }

            if (type.IsGenericTypeDefinition) {
                Throw("sanity check -- type is a generic definition for type={0}", type);
            }

            return type;
        }

        public static RuntimeTypeModel CreateModel() {
            var model = TypeModel.Create();

            //--
            // We want protobuf-net to serialize default values. Sometimes, protobuf-net will skip
            // serialization when it really shouldn't.
            //
            // An example is a nullable struct; the nullable struct can contain a value, but if
            // that value is the default one then protobuf-net will skip serialization, resulting
            // in a null nullable type after deserialization.
            model.UseImplicitZeroDefaults = false;

            // custom model workers
            foreach (IProtoModelWorker worker in
                RuntimeReflectionUtilities.GetAssemblyInstances<IProtoModelWorker>()) {

                worker.Work(model);
            }

            // non-generic surrogates
            foreach (NonGenericSurrogate surrogate in GetNonGenericSurrogates()) {
                Type surrogateType = surrogate.SurrogateType;

                // the surrogate needs to public
                if (surrogateType.IsVisible == false) {
                    Throw("A surrogate needs to have public visibility for surrogate={0}",
                        surrogateType);
                }

                // the surrogate cannot be a generic type
                if (surrogateType.IsGenericTypeDefinition) {
                    Throw("A generic surrogate also needs to have a [ProtoGenericInstantiator] " +
                        "attribute for surrogate={0}", surrogateType);
                }

                // the surrogate needs a [ProtoContract] attribute
                if (surrogateType.GetAttribute<ProtoContractAttribute>() == null) {
                    Throw("A surrogate is missing a [ProtoContract] attribute for surrogate={0}",
                        surrogateType);
                }

                // register the surrogate
                model.Add(RunSanityCheck(surrogate.ModeledType), false)
                    .SetSurrogate(RunSanityCheck(surrogateType));
            }

            // regular old [ProtoContract] types
            foreach (Type contract in GetContracts()) {
                if (contract.IsVisible == false) {
                    Throw("A ProtoContract type needs to have public visibility for contract={0}",
                        contract);
                }

                model.Add(RunSanityCheck(contract), true);
            }

            // Fields and properties on UnityObject derived types, such as BaseBehavior, are not
            // annotated with serialization annotations. This means that for protobuf-net, if a
            // BaseBehavior contains a Dictionary{string,string} field, then that specific generic
            // instantiation may not be discovered, as the field does not serve as an anchor.
            //
            // In this loop, we go through all UnityObject derived types and ensure that every
            // serialized property is in the RuntimeTypeModel.
            foreach (var behaviorType in RuntimeReflectionUtilities.GetUnityObjectTypes()) {
                // We only want UnityObject types are serializable by ProtoBufNet
                // TODO: support custom base classes
                if (typeof(BaseBehavior<ProtoBufNetSerializer>).IsAssignableFrom(behaviorType) == false) {
                    continue;
                }

                var serializedProperties = InspectedType.Get(behaviorType).SerializedProperties;
                foreach (InspectedProperty property in serializedProperties) {
                    // If the property is generic and the model currently doesn't contain it, make
                    // sure we add it to the model.
                    if (property.StorageType.IsGenericType &&
                        ContainsType(model, property.StorageType) == false) {

                        model.Add(property.StorageType, true);
                    }
                }
            }

            return model;
        }

        /// <summary>
        /// Returns true if the given RuntimeTypeModel contains a definition for the given type.
        /// </summary>
        private static bool ContainsType(RuntimeTypeModel model, Type type) {
            foreach (MetaType metaType in model.GetTypes()) {
                if (metaType.Type == type) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Throws an InvalidOperationException.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="parameters">The parameters in the format string.</param>
        private static void Throw(string format, params object[] parameters) {
            throw new InvalidOperationException(string.Format(format, parameters));
        }

    }
}