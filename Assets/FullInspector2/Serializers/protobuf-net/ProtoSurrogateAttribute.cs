using System;

namespace FullInspector {
    /// <summary>
    /// Marks the annotated type as a surrogate for another type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class ProtoSurrogateAttribute : Attribute {
        /// <summary>
        /// The type that this annotated type is a surrogate for.
        /// </summary>
        public Type SurrogateFor;

        /// <summary>
        /// Marks this type as a surrogate for another non-generic type.
        /// </summary>
        /// <param name="surrogateFor">The type that this type acts as a protobuf surrogate
        /// for.</param>
        public ProtoSurrogateAttribute(Type surrogateFor) {
            if (surrogateFor.IsGenericTypeDefinition) {
                throw new InvalidOperationException("Use the two argument constructor for " +
                    "generic type definition surrogates");
            }

            SurrogateFor = surrogateFor;
        }
    }
}