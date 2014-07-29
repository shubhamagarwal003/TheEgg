using UnityEngine;

namespace FullInspector.Samples.ItemDatabase {
    [AddComponentMenu("Full Inspector Samples/Other/ScriptableObjects")]
    public class ItemDatabaseBehavior : BaseBehavior<JsonNetSerializer> {
        public ItemDatabaseSample ScriptableObject;
    }
}