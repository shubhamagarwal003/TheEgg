using FullInspector.Rotorz.ReorderableList;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FullInspector.Modules.Collections {
    /// <summary>
    /// Reorderable list adapter for generic collections.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class DictionaryAdapter<TKey, TValue> : IReorderableListAdaptor {
        private IDictionary<TKey, TValue> _dictionary;
        private KeyValuePair<TKey, TValue>[] _dictionaryCache;

        private Func<KeyValuePair<TKey, TValue>, float> _itemHeight;
        private ReorderableListControl.ItemDrawer<TKey> _keyDrawer;
        private ReorderableListControl.ItemDrawer<TValue> _valueDrawer;
        private Func<KeyValuePair<TKey, TValue>> _itemGenerator;

        private static KeyValuePair<TKey, TValue> DefaultItemGenerator() {
            return new KeyValuePair<TKey, TValue>();
        }

        /// <summary>
        /// For performance reasons, the DictionaryAdapter stores an array version of the
        /// dictionary. If the adapted dictionary has been structurally modified, for example, an
        /// item has been added, then the local cache is invalid. Calling this method updates the
        /// cache, which will restore proper adapter semantics.
        /// </summary>
        public void InvalidateCache() {
            _dictionaryCache = _dictionary.ToArray();
        }

        /// <summary>
        /// Initializes a new instance of DictionaryAdapter.
        /// </summary>
        /// <param name="dictionary">The dictionary that will be adapter for Rotorz</param>
        /// <param name="keyDrawer">The function that will edit keys in the dictionary</param>
        /// <param name="valueDrawer">The function that will edit values in the dictionary</param>
        /// <param name="itemHeight">The function that computes the height of dictionary
        /// items</param>
        /// <param name="itemGenerator">The function used to generate new items. If null, then the
        /// default item generator is used</param>
        public DictionaryAdapter(IDictionary<TKey, TValue> dictionary,
            ReorderableListControl.ItemDrawer<TKey> keyDrawer,
            ReorderableListControl.ItemDrawer<TValue> valueDrawer,
            Func<KeyValuePair<TKey, TValue>, float> itemHeight,
            Func<KeyValuePair<TKey, TValue>> itemGenerator = null) {

            _dictionary = dictionary;
            _keyDrawer = keyDrawer;
            _valueDrawer = valueDrawer;
            _itemHeight = itemHeight;
            _itemGenerator = itemGenerator ?? DefaultItemGenerator;

            InvalidateCache();
        }

        public int Count {
            get {
                return _dictionary.Count;
            }
        }

        public virtual bool CanDrag(int index) {
            return false;
        }

        public virtual bool CanRemove(int index) {
            return true;
        }

        public void Add() {
            KeyValuePair<TKey, TValue> item = _itemGenerator();

            if (_dictionary.ContainsKey(item.Key)) {
                Debug.LogWarning("Dictionary already contains an item for key=" + item.Key);
                return;
            }

            _dictionary.Add(item);
            InvalidateCache();
        }

        public void Insert(int index) {
            // oh well, can't insert into a collection
            Debug.LogWarning("Cannot insert into a specific index into a collection; adding to the end");
            Add();
            InvalidateCache();
        }

        public void Duplicate(int index) {
            Debug.LogError("Cannot duplicate a Dictionary element");
        }

        public void Remove(int index) {
            var item = _dictionaryCache[index];
            _dictionary.Remove(item);
            InvalidateCache();
        }

        public void Move(int sourceIndex, int destIndex) {
            Debug.LogError("Cannot move elements in a dictionary");
        }

        public void Clear() {
            _dictionary.Clear();
        }

        public virtual void DrawItem(Rect position, int index) {
            // Rotorz seems to sometimes give an index of -1, not sure why.
            if (index < 0) {
                return;
            }

            KeyValuePair<TKey, TValue> item = _dictionaryCache[index];

            TKey key = _keyDrawer(position, item.Key);
            TValue value = _valueDrawer(position, item.Value);

            // has the key been edited?
            if (EqualityComparer<TKey>.Default.Equals(key, item.Key) == false) {
                // we already contain an element for the new key; lets just update the old key
                // in-case the value also changed
                if (_dictionary.ContainsKey(key)) {
                    _dictionary[item.Key] = value;
                }

                // we don't have a slot for the new key, so lets swap the values over from the old
                // key to the new key
                else {
                    _dictionary.Remove(item.Key);
                    _dictionary[key] = value;
                }

                InvalidateCache();
            }

            // The key was not changed, so we can just update the value in the dictionary. However,
            // we only need to do this if the value has actually been modified and the modification
            // is not reflected inside of the dictionary.
            else if (EqualityComparer<TValue>.Default.Equals(_dictionary[key], value) == false) {
                _dictionary[key] = value;
                InvalidateCache();
            }
        }

        public virtual float GetItemHeight(int index) {
            return _itemHeight(_dictionaryCache[index]);
        }
    }
}