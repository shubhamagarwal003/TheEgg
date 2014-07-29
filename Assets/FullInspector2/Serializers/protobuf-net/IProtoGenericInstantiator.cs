using System;
using System.Collections.Generic;

namespace FullInspector {
    /// <summary>
    /// This interface contains the methods necessary to instantiate a [ProtoGenericInstantiator]
    /// type with the associated generic type parameters in the compiled protobuf type model.
    /// </summary>
    public interface IProtoGenericInstantiator {
        /// <summary>
        /// Every generic type parameter that will be used in the generic ProtoContract type.
        /// </summary>
        IEnumerable<Type[]> GenericTypeArguments { get; }
    }
}