using FullInspector.Rotorz.ReorderableList;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullInspector.Modules.Collections {
    /// <summary>
    /// Reorderable list adapter for generic list.
    /// </summary>
    /// <remarks>
    /// <para>This adapter can be subclassed to add special logic to item height calculation. You
    /// may want to implement a custom adapter class where specialized functionality is
    /// needed.</para>
    /// </remarks>
    public class GenericListAdaptorWithDynamicHeight<T> : IReorderableListAdaptor {
        private Func<T, float> _itemHeight;
        private IList<T> _list;
        private ReorderableListControl.ItemDrawer<T> _itemDrawer;
        private Func<T> _itemGenerator;

        private static T DefaultItemGenerator() {
            return default(T);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="GenericListAdaptor{T}"/>.
        /// </summary>
        /// <param name="list">The list which can be reordered.</param>
        /// <param name="itemDrawer">Callback to draw list item.</param>
        /// <param name="itemHeight">Height of list item in pixels.</param>
        /// <param name="itemGenerator">The function used to generate new items. If null, then the
        /// default item generator is used.</param>
        public GenericListAdaptorWithDynamicHeight(IList<T> list,
            ReorderableListControl.ItemDrawer<T> itemDrawer, Func<T, float> itemHeight,
            Func<T> itemGenerator = null) {
            _list = list;
            _itemDrawer = itemDrawer ?? ReorderableListGUI.DefaultItemDrawer;
            _itemHeight = itemHeight;
            _itemGenerator = itemGenerator ?? DefaultItemGenerator;
        }

        public int Count {
            get { return _list.Count; }
        }
        public virtual bool CanDrag(int index) {
            return true;
        }
        public virtual bool CanRemove(int index) {
            return true;
        }
        public void Add() {
            T item = _itemGenerator();
            _list.Add(item);
        }
        public void Insert(int index) {
            T item = _itemGenerator();
            _list.Insert(index, item);
        }
        public void Duplicate(int index) {
            _list.Insert(index + 1, _list[index]);
        }
        public void Remove(int index) {
            _list.RemoveAt(index);
        }
        public void Move(int sourceIndex, int destIndex) {
            if (destIndex > sourceIndex)
                --destIndex;

            T item = _list[sourceIndex];
            _list.RemoveAt(sourceIndex);
            _list.Insert(destIndex, item);
        }
        public void Clear() {
            _list.Clear();
        }
        public virtual void DrawItem(Rect position, int index) {
            // Rotorz seems to sometimes give an index of -1, not sure why.
            if (index < 0) {
                return;
            }

            _list[index] = _itemDrawer(position, _list[index]);
        }
        public virtual float GetItemHeight(int index) {
            return _itemHeight(_list[index]);
        }
    }
}