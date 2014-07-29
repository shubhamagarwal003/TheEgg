using FullInspector.Rotorz.ReorderableList;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FullInspector.Modules.Collections {
    /// <summary>
    /// Reorderable list adapter for ICollection types
    /// </summary>
    public class CollectionAdaptor<T> : IReorderableListAdaptor {
        /// <summary>
        /// Returns the height of the given element.
        /// </summary>
        private Func<T, float> _height;

        /// <summary>
        /// Provides an editor for the given element.
        /// </summary>
        private ReorderableListControl.ItemDrawer<T> _drawer;

        /// <summary>
        /// Stores all of the elements
        /// </summary>
        private ICollection<T> _collection;

        /// <summary>
        /// A cached version of the collection optimized for item lookup.
        /// </summary>
        private T[] _collectionCache;

        /// <summary>
        /// For performance reasons, the CollectionAdaptor stores an array version of the
        /// collection. If the adapted collection has been structurally modified, for example, an
        /// item has been added, then the local cache is invalid. Calling this method updates the
        /// cache, which will restore proper adapter semantics.
        /// </summary>
        public void InvalidateCache() {
            _collectionCache = _collection.ToArray();
        }

        public CollectionAdaptor(ICollection<T> collection,
            ReorderableListControl.ItemDrawer<T> drawer, Func<T, float> height) {

            _collection = collection;
            _drawer = drawer ?? ReorderableListGUI.DefaultItemDrawer;
            _height = height;

            InvalidateCache();
        }

        public int Count {
            get { return _collectionCache.Length; }
        }

        public virtual bool CanDrag(int index) {
            return true;
        }

        public virtual bool CanRemove(int index) {
            return true;
        }

        public void Add() {
            T item = default(T);
            _collection.Add(item);

            InvalidateCache();
        }

        public void Insert(int index) {
            throw new NotSupportedException();
        }

        public void Duplicate(int index) {
            T element = _collection.ElementAt(index);

            _collection.Add(element);
            InvalidateCache();
        }

        public void Remove(int index) {
            T element = _collection.ElementAt(index);

            _collection.Remove(element);
            InvalidateCache();
        }

        public void Move(int sourceIndex, int destIndex) {
            throw new NotSupportedException();
        }

        public void Clear() {
            _collection.Clear();
            InvalidateCache();
        }

        public virtual void DrawItem(Rect position, int index) {
            // Rotorz seems to sometimes give an index of -1, not sure why.
            if (index < 0) {
                return;
            }

            T element = _collectionCache[index];
            T updated = _drawer(position, element);

            // If the modified item is equal to the updated item, then we don't have to replace it
            // in the collection.
            if (EqualityComparer<T>.Default.Equals(element, updated) == false) {
                _collection.Remove(element);
                _collection.Add(updated);
                InvalidateCache();
            }
        }

        public virtual float GetItemHeight(int index) {
            T element = _collectionCache[index];

            return _height(element);
        }
    }
}