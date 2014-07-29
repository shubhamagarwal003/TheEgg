using System;
using System.Collections.Generic;

namespace FullInspector.Internal {
    /// <summary>
    /// Provides access to an arbitrary set of singleton objects such that the objects can be
    /// accessed in generic functions.
    /// </summary>
    internal static class SingletonCache {
        /// <summary>
        /// The singleton instances.
        /// </summary>
        private static Dictionary<Type, object> _instances = new Dictionary<Type, object>();

        /// <summary>
        /// Retrieve a singleton of the given type. This method creates the object if it has not
        /// already been created.
        /// </summary>
        /// <typeparam name="T">The type of object to fetch an instance of.</typeparam>
        /// <returns>An object of the given type.</returns>
        public static T Get<T>() {
            object result;

            if (_instances.TryGetValue(typeof(T), out result) == false) {
                result = Activator.CreateInstance<T>();
                _instances[typeof(T)] = result;
            }

            return (T)result;
        }
    }
}