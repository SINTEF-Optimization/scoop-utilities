//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// A collection of keys and values, where iteration is in key entry order.
	/// Values are added, accessed and removed using unique keys for lookup,
	/// just like for a Dictionary, but the iteration order is in the entry order of the keys.
	/// This means that if a value is added with a key that does not exist in the collection,
	/// the value will be placed at the end of the current iteration order, even if a
	/// key/value pair with the same key has existed and been removed from the collection
	/// before. If the value for an existing key is updated, the value keeps the same
	/// position in the iteration order as the old value of the same key.
	/// </summary>
	/// <typeparam name="TKey">The type of keys in the ChronologicalDictionary.</typeparam>
	/// <typeparam name="TValue">The type of values in the ChronologicalDictionary.</typeparam>
	public class ChronologicalDictionary<TKey, TValue> : IEnumerable<TValue>
	{
		#region Private inner classes

		/// <summary>
		/// An object of a specified type together with an index value.
		/// The index is used to order a collection of pairs.
		/// </summary>
		/// <typeparam name="T">The type of the object being indexed.</typeparam>
		private class IndexedObject<T>
		{
			/// <summary>
			/// The index of the object.
			/// </summary>
			public uint Index { get; }

			/// <summary>
			/// The object being indexed.
			/// </summary>
			public T Object { get; set; }

			/// <summary>
			/// Creates an indexed object.
			/// </summary>
			/// <param name="index">The index of the object.</param>
			/// <param name="obj">The object being indexed.</param>
			public IndexedObject(uint index, T obj)
			{
				Index = index;
				Object = obj;
			}
		}

		#endregion

		#region Private variables

		/// <summary>
		/// The keys and values of the ChronologicalDictionary, with an indexing of the values,
		/// defining the order of the iteration of the values.
		/// </summary>
		private readonly Dictionary<TKey, IndexedObject<TValue>> _indexedValues = new();

		/// <summary>
		/// The next index to be assigned to a value in the ChronologicalDictionary for a key that is
		/// not already found in the dictionary.
		/// </summary>
		private uint _nextIndex;

		#endregion

		#region Private methods

		/// <summary>
		/// Orders the KeyValuePair elements of the indexed values in the key inserted order.
		/// </summary>
		/// <returns>Returns the KeyValuePair elements of the indexed values in the key inserted order.</returns>
		private IEnumerable<KeyValuePair<TKey, IndexedObject<TValue>>> KeysAndIndexedValuesSorted()
		{
			return _indexedValues.OrderBy(kvp => kvp.Value.Index);
		}

		#endregion

		#region Implementation of some members of the IDictionary<TKey, TValue> interface

		/// <summary>
		/// Gets or sets the element with the specified key.
		/// </summary>
		/// <param name="key">The key of the element to get or set.</param>
		/// <returns>The element with the specified key.</returns>
		public TValue this[TKey key]
		{
			get => _indexedValues[key].Object;
			set
			{
				if (_indexedValues.TryGetValue(key, out IndexedObject<TValue> existingValue))
					existingValue.Object = value;
				else
					Add(key, value);
			}
		}

		/// <summary>
		/// Gets an enumerator that iterates through the keys in the key inserted order.
		/// </summary>
		public IEnumerable<TKey> Keys => KeysAndIndexedValuesSorted().Select(kvp => kvp.Key);

		/// <summary>
		/// Adds an element with the provided key and value to the ChronologicalDictionary.
		/// </summary>
		/// <param name="key">The object to use as the key of the element to add.</param>
		/// <param name="value">The object to use as the value of the element to add.</param>
		public void Add(TKey key, TValue value)
		{
			uint idx = _nextIndex;
			++_nextIndex;

			_indexedValues.Add(key, new IndexedObject<TValue>(idx, value));
		}

		/// <summary>
		/// Determines whether the ChronologicalDictionary contains an element
		/// with the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the ChronologicalDictionary</param>
		/// <returns>true if the ChronologicalDictionary contains an element with the key;
		/// otherwise, false.</returns>
		public bool ContainsKey(TKey key)
		{
			return _indexedValues.ContainsKey(key);
		}

		/// <summary>
		/// Removes the element with the specified key from the ChronologicalDictionary.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <returns>True if the element is successfully removed; otherwise, false.
		/// This method also returns false if key was not found in the original
		/// ChronologicalDictionary.</returns>
		public bool Remove(TKey key)
		{
			return _indexedValues.Remove(key);
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key whose value to get.</param>
		/// <param name="value">When this method returns, the value associated with the specified key, if the
		/// key is found; otherwise, the default value for the type of the value parameter.
		/// This parameter is passed uninitialized.</param>
		/// <returns>true if the object that implements ChronologicalDictionary contains
		/// an element with the specified key; otherwise, false.</returns>
		public bool TryGetValue(TKey key, out TValue value)
		{
			bool found = _indexedValues.TryGetValue(key, out IndexedObject<TValue> indexedValue);
			value = found ? indexedValue.Object : default;

			return found;
		}

		#endregion

		#region IEnumerable<TValue> members

		/// <summary>
		/// Returns an enumerator that iterates through the values in the key inserted order.
		/// </summary>
		/// <returns>An enumerator that can be used to iterate through the values in the key
		/// inserted order.</returns>
		public IEnumerator<TValue> GetEnumerator()
		{
			return KeysAndIndexedValuesSorted().Select(kvp => kvp.Value.Object).GetEnumerator();
		}

		#endregion

		#region IEnumerable members

		/// <summary>
		/// Returns an enumerator that iterates through the values in the key inserted order.
		/// </summary>
		/// <returns>A System.Collections.IEnumerator that can be used to iterate through the
		/// values in the key inserted order.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

	}
}
