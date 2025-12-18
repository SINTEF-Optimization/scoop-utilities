//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections;
using System.Collections.Generic;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// A list that supports removing items in O(1) time.
	/// 
	/// The differences from a regular List are:
	///  - Contains() and IndexOf() are constant time
	///  - Remove() is constant time
	///  - The order of the remaining items may change when an item is removed.
	///  - The list may not contain duplicate items. In this respect, it behaves more 
	///    like a HashSet.
	///  - Insert(T, int) is not supported
	/// </summary>
	/// <typeparam name="T">The type of items in the list</typeparam>
	public class QuickList<T> : IList<T>
	{
		/// <summary>
		/// The list of items
		/// </summary>
		private List<T> _list = new List<T>();

		/// <summary>
		/// Map from item to its index in the list
		/// </summary>
		private Dictionary<T, int> _index = new Dictionary<T, int>();

		/// <summary>
		/// Returns the item at the given <paramref name="index"/>.
		/// Setting is not implementing.
		/// </summary>
		public T this[int index]
		{
			get => _list[index];
			set => throw new NotImplementedException();
		}

		/// <summary>
		/// Returns the number of items in the list
		/// </summary>
		public int Count => _list.Count;

		/// <summary>
		/// Returns false
		/// </summary>
		public bool IsReadOnly => false;

		/// <summary>
		/// Adds the given item to the list, if it not there already
		/// </summary>
		/// <param name="item">The item to add</param>
		/// <returns>True if the item was added, false if it already was in the list</returns>
		public bool Add(T item)
		{
			if (Contains(item))
				return false;

			_index[item] = _list.Count;
			_list.Add(item);

			return true;
		}

		void ICollection<T>.Add(T item) => Add(item);

		/// <summary>
		/// Removes all items from the list
		/// </summary>
		public void Clear()
		{
			_list.Clear();
			_index.Clear();
		}

		/// <summary>
		/// Returns true if the given item is in the list
		/// </summary>
		public bool Contains(T item) => _index.ContainsKey(item);

		/// <summary>
		/// See List.CopyTo
		/// </summary>
		public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

		/// <summary>
		/// Returns an enumerator for the list
		/// </summary>
		/// <returns></returns>
		public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

		/// <summary>
		/// Returns the index of the given item
		/// </summary>
		public int IndexOf(T item) => _index[item];

		/// <summary>
		/// Throws an exception
		/// </summary>
		public void Insert(int index, T item)
		{
			throw new NotImplementedException("This does not make sense, as indices are not preserved");
		}

		/// <summary>
		/// Removes the given item from the list, if it is there
		/// </summary>
		/// <param name="item">The item to remove</param>
		/// <returns>True if the item was removed</returns>
		public bool Remove(T item)
		{
			if (Contains(item))
			{
				RemoveAt(_index[item]);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Removes the item at the given index in the list
		/// </summary>
		public void RemoveAt(int index)
		{
			int lastIndex = Count - 1;

			_index.Remove(_list[index]);

			if (index != lastIndex)
			{
				T movedItem = _list[lastIndex];
				_list[index] = movedItem;
				_index[movedItem] = index;
			}

			_list.RemoveAt(lastIndex);
		}

		IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
	}

}
