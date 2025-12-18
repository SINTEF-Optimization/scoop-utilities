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
	/// A dictionary based on weak references on the keys so that they can be garbage collected when no longer in use.
	/// When there no longer is a strong reference to a key in the dictionary, the garbage collector may collect the
	/// key which will make the corresponding entry in the dictionary unreachable.
	/// 
	/// The dead references will still contribute to the <see cref="WeakDictionary{TKey, TValue}.Count"/> property
	/// until the dictionary is purged of dead keys.
	/// 
	/// Dead keys can be purged explicitly by calling <see cref="WeakDictionary{TKey, TValue}.RemoveDeadKeys"/>.
	/// This dictionary will also purge itself from dead references automatically when it is enumerated, or after a
	/// certain number of calls to <see cref="WeakDictionary{TKey, TValue}.Add(TKey, TValue)"/> or
	/// <see cref="WeakDictionary{TKey, TValue}.Remove(TKey)"/> depending on the number of living elements in the
	/// collection (at the time of the last purge).
	/// </summary>
	/// <typeparam name="TKey">The type used to be as key in this dictionary. Must be a class.</typeparam>
	/// <typeparam name="TValue">The type used as value in this dictionary.</typeparam>
	public class WeakDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : class
	{

		/// <summary>
		/// Constructs an instance of the dictionary.
		/// </summary>
		public WeakDictionary()
		{
			_comparer = new KeyComparer<TKey>();
			_dictionary = new Dictionary<Key<TKey>, TValue>(_comparer);
			_numberOfCollectionAlterationsToNextPurge = 16;
			_insertAndRemovesSinceLastPurge = 0;
		}

		/// <inheritdoc/>
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			List<Key<TKey>> deadKeys = new();
			foreach (var pair in _dictionary)
			{
				var target = pair.Key.Target;
				if (target != null)
				{
					yield return new KeyValuePair<TKey, TValue>(target, pair.Value);
				}
				else
				{
					deadKeys.Add(pair.Key);
				}
			}
			deadKeys.ForEach(k => _dictionary.Remove(k));
		}

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <inheritdoc/>
		public void Add(KeyValuePair<TKey, TValue> item)
		{
			_dictionary.Add(new Key<TKey>(item.Key), item.Value);
		}

		/// <inheritdoc/>
		public void Clear()
		{
			_dictionary.Clear();
		}

		/// <summary>
		/// Not implemented by this class.
		/// </summary>
		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Not implemented by this class.
		/// </summary>
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Not implemented by this class.
		/// </summary>
		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Returns the number of elements in the dictionary. This may be greater than the actual number of elements if some keys have died since the last purge.
		/// If an accurate number is important purge the collection before calling this, <see cref="RemoveDeadKeys"/>.
		/// </summary>
		public int Count => _dictionary.Count;

		/// <summary>
		/// Not implemented by this class.
		/// </summary>
		public bool IsReadOnly => throw new NotImplementedException();

		/// <inheritdoc/>
		public void Add(TKey key, TValue value)
		{
			RemoveDeadKeysIfNeeded();
			_dictionary.Add(new Key<TKey>(key), value);
		}

		/// <inheritdoc/>
		public bool ContainsKey(TKey key)
		{
			return _dictionary.ContainsKey(new Key<TKey>(key));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public bool Remove(TKey key)
		{
			RemoveDeadKeysIfNeeded();
			return _dictionary.Remove(new Key<TKey>(key));
		}

		/// <inheritdoc/>
		public bool TryGetValue(TKey key, out TValue value)
		{
			return _dictionary.TryGetValue(new Key<TKey>(key), out value);
		}

		/// <inheritdoc/>
		public TValue this[TKey key]
		{
			get
			{
				return _dictionary[new Key<TKey>(key)];
			}
			set
			{
				_dictionary[new Key<TKey>(key)] = value;
				RemoveDeadKeysIfNeeded();
			}
		}

		/// <inheritdoc/>
		public ICollection<TKey> Keys
		{
			get
			{
				List<TKey> keys = new();
				List<Key<TKey>> deadKeys = new();
				foreach (var key in _dictionary.Keys)
				{
					var target = key.Target;
					if (target != null)
					{
						keys.Add(target);
					}
					else
					{
						deadKeys.Add(key);
					}
				}

				deadKeys.ForEach(k => _dictionary.Remove(k));
				DeadKeysAreRemoved();

				return keys;
			}
		}

		/// <inheritdoc/>
		public ICollection<TValue> Values
		{
			get
			{
				List<TValue> values = new();
				List<Key<TKey>> deadKeys = new();
				foreach (var kvp in _dictionary)
				{
					var target = kvp.Key.Target;
					if (target != null)
					{
						values.Add(kvp.Value);
					}
					else
					{
						deadKeys.Add(kvp.Key);
					}
				}
				
				deadKeys.ForEach(k => _dictionary.Remove(k));
				DeadKeysAreRemoved();

				return values;
			}
		}

		/// <summary>
		/// Removes all entries where the key has been garbage collected.
		/// </summary>
		public void RemoveDeadKeys()
		{
			List<Key<TKey>> deadKeys = new();

			foreach(var key in _dictionary.Keys)
			{
				if (key.Target == null)
				{
					deadKeys.Add(key);
				}
			}

			deadKeys.ForEach(k => _dictionary.Remove(k));
			DeadKeysAreRemoved();
		}

		/// <summary>
		/// Purges the dead keys if there have been enough additions or removals to the collections since the last purge.
		/// </summary>
		private void RemoveDeadKeysIfNeeded()
		{
			_insertAndRemovesSinceLastPurge++;
			if (_insertAndRemovesSinceLastPurge > _numberOfCollectionAlterationsToNextPurge)
			{
				RemoveDeadKeys();
			}
		}

		/// <summary>
		/// Signals the dictionary that the dead keys have just been purged.
		/// </summary>
		private void DeadKeysAreRemoved()
		{
			_numberOfCollectionAlterationsToNextPurge = Math.Min(Count, 10);
			_insertAndRemovesSinceLastPurge = 0;
		}

		/// <summary>
		/// Comparer used to look up entries in the dictionary based on the key.
		/// </summary>
		private readonly KeyComparer<TKey> _comparer;

		/// <summary>
		/// The dictionary
		/// </summary>
		private readonly Dictionary<Key<TKey>, TValue> _dictionary;

		/// <summary>
		/// Counts the number of add and remove calls since the last time dead keys were purged.
		/// </summary>
		private int _insertAndRemovesSinceLastPurge;

		/// <summary>
		/// The number of insert and remove calls before purging dead keys.
		/// </summary>
		private int _numberOfCollectionAlterationsToNextPurge;

		/// <summary>
		/// A weak key reference to an object in the dictionary. Does not support references to null objects.
		/// This is designed to be used in a weak dictionary, so the hash code is based on the initially referenced object,
		/// so it will not end up in the wrong bucket if the referenced key is garbage collected.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		private class Key<T> where T : class
		{

			public Key(T value)
			{
				if (value == null)
				{
					throw new ArgumentNullException("Does not support references to null values");
				}
				_reference = new WeakReference<T>(value);
				// Store the hashcode now so the reference does not change hashcode when the referred object is collected.
				_hashCode = value.GetHashCode();
			}

			/// <summary>
			/// Returns a strong reference to the referenced key object if it is alive or null if it is no longer alive.
			/// </summary>
			public T Target
			{
				get
				{
					if (_reference.TryGetTarget(out var value))
					{
						return value;
					}
					return null;
				}
			}

			/// <inheritdoc/>
			public override int GetHashCode()
			{
				return _hashCode;
			}

			/// <summary>
			/// A weak reference to the key object.
			/// </summary>
			private readonly WeakReference<T> _reference;

			/// <summary>
			/// The hash code of the original object before it gets collected.
			/// </summary>
			private readonly int _hashCode;
		}

		/// <summary>
		/// A comparer designed for the weak dictionary.
		/// If both objects are alive, then the comparison is based on the referenced instance.
		/// If one or both objects are dead, then it is based on the key instance.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		private class KeyComparer<T> : IEqualityComparer<Key<T>> where T : class
		{
			/// <summary>
			/// If both key references are alive, compare the objects referenced by the keys and return true if they are the same.
			/// If both key references are dead, compare the keys themselves and return true if they are the same key.
			/// Otherwise, return false.
			/// </summary>
			/// <param name="first"></param>
			/// <param name="second"></param>
			/// <returns></returns>
			public bool Equals(Key<T> first, Key<T> second)
			{
				if (first == null || second == null)
				{
					return false;
				}

				var firstTarget = first.Target;
				var secondTarget = second.Target;

				if (firstTarget != null && secondTarget != null)
				{
					return ReferenceEquals(firstTarget, secondTarget);
				}

				return ReferenceEquals(first, second);
			}

			/// <inheritdoc/>
			public int GetHashCode(Key<T> obj)
			{
				return obj.GetHashCode();
			}
		}

	}
}