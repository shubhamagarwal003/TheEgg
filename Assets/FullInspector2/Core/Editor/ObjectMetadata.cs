using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FullInspector.Internal {
    /// <summary>
    /// Returns an associated object for another object.
    /// </summary>
    internal static class ObjectMetadata<T> where T : new() {
        private static ObjectIDGenerator _ids = new ObjectIDGenerator();
        private static Dictionary<long, T> _items = new Dictionary<long, T>();
        private static T _nullValue = new T();

        public static T Get(object item) {
            if (item == null) {
                return _nullValue;
            }

            bool firstTime;
            long id = _ids.GetId(item, out firstTime);

            if (firstTime) {
                T value = new T();
                _items[id] = value;
            }

            return _items[id];
        }
    }
}