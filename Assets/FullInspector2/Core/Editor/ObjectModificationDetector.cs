using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FullInspector.Internal {
    /// <summary>
    /// A helper class that identifies when an ISerializedObject has been modified for some reason.
    /// </summary>
    /// <remarks>
    /// To determine if the object state has changed, this hashes the serialized state of the
    /// object. If the hash of the serialized state has changed, then the object has been modified.
    /// </remarks>
    internal static class ObjectModificationDetector {
        private static Dictionary<long, int> _serializedStates = new Dictionary<long, int>();
        private static ObjectIDGenerator _ids = new ObjectIDGenerator();
        private static ObjectIDGenerator _unityObjectIds = new ObjectIDGenerator();

        /// <summary>
        /// Computes a hash code for the given serialized object.
        /// </summary>
        private static int GetHash(ISerializedObject obj) {
            int hash = 27;

            foreach (var unityObj in obj.SerializedObjectReferences) {
                if (unityObj != null) {
                    bool firstTime;
                    long id = _unityObjectIds.GetId(unityObj, out firstTime);
                    hash = (13 * hash) + (int)id;
                }
            }

            foreach (var str in obj.SerializedStateKeys) {
                hash = (13 * hash) + str.GetHashCode();
            }
            foreach (var str in obj.SerializedStateValues) {
                if (str != null) {
                    hash = (13 * hash) + str.GetHashCode();
                }
            }

            return hash;
        }

        /// <summary>
        /// Update the stored serialized state of the given object.
        /// </summary>
        public static void Update(ISerializedObject obj) {
            bool firstTime;
            long id = _ids.GetId(obj, out firstTime);

            _serializedStates[id] = GetHash(obj);
        }

        /// <summary>
        /// Returns true if the given object has been modified since its last update.
        /// </summary>
        public static bool WasModified(ISerializedObject obj) {
            bool firstTime;
            long id = _ids.GetId(obj, out firstTime);

            int savedHash;
            if (_serializedStates.TryGetValue(id, out savedHash) == false) {
                // We want to reset if we don't have any data already stored, because the object may
                // have already been modified from it's initial prefab state.
                return true;
            }

            return savedHash != GetHash(obj);
        }
    }

}