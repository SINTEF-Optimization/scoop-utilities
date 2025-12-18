//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Excension methods related to collections.
	/// </summary>
	public static partial class Extensions
	{
		/// <summary>
		/// If the dictionary contains the given key, the element is added to the value list.
		/// If not, a new entry is created by creating a new value list and adding the element to it.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="A"></typeparam>
		/// <param name="dict"></param>
		/// <param name="key"></param>
		/// <param name="element"></param>
		/// <param name="uniqueEntriesOnly">If true, the element is only added if it does not already exist in the list for the given key.</param>
		/// <returns></returns>
		public static void AddOrNew<T, A>(this IDictionary<T, List<A>> dict, T key, A element, bool uniqueEntriesOnly = false)
		{
			if (dict.ContainsKey(key))
			{
				if (!uniqueEntriesOnly || !dict[key].Contains(element))
					dict[key].Add(element);
			}
			else
				dict[key] = new List<A>() { element };
		}

		/// <summary>
		/// If the dictionary contains the given key, the element is removed from the value list.
		/// If that list becomes empty, the key is removed from the dictionary.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="A"></typeparam>
		/// <param name="dict"></param>
		/// <param name="key"></param>
		/// <param name="element"></param>
		/// <returns>True if the key was deleted from the dictionary</returns>
		public static bool RemoveAndDelete<T, A>(this IDictionary<T, List<A>> dict, T key, A element)
		{
			if (dict.ContainsKey(key))
			{
				dict[key].Remove(element);
				if (dict[key].Count == 0)
					return dict.Remove(key);
			}
			return false;
		}



		/// <summary>
		/// If the dictionary contains the given key, the element is added to the value HashSet.
		/// If not, a new entry is created by creating a new value HashSet and adding the element to it.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="A"></typeparam>
		/// <param name="dict"></param>
		/// <param name="key"></param>
		/// <param name="element"></param>
		/// <param name="uniqueEntriesOnly">If true, the element is only added if it does not already exist in the HashSet for the given key.</param>
		/// <returns></returns>
		public static void AddOrNew<T, A>(this IDictionary<T, HashSet<A>> dict, T key, A element, bool uniqueEntriesOnly = false)
		{
			if (dict.ContainsKey(key))
			{
				if (!uniqueEntriesOnly || !dict[key].Contains(element))
					dict[key].Add(element);
			}
			else
				dict[key] = new HashSet<A>() { element };
		}

		/// <summary>
		/// If the dictionary contains the given key, the element is removed from the value HashSet.
		/// If that HashSet becomes empty, the key is removed from the dictionary.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="A"></typeparam>
		/// <param name="dict"></param>
		/// <param name="key"></param>
		/// <param name="element"></param>
		/// <returns>True if the key was deleted from the dictionary</returns>
		public static bool RemoveAndDelete<T, A>(this IDictionary<T, HashSet<A>> dict, T key, A element)
		{
			if (dict.ContainsKey(key))
			{
				dict[key].Remove(element);
				if (dict[key].Count == 0)
					return dict.Remove(key);
			}
			return false;
		}

		/// <summary>
		/// If the sorted dictionary contains the given key, the element is added to the value list.
		/// If not, a new entry is created by creating a new value list and adding the element to it.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="A"></typeparam>
		/// <param name="dict"></param>
		/// <param name="key"></param>
		/// <param name="element"></param>
		/// <param name="uniqueEntriesOnly">If true, the element is only added if it does not already exist in the list for the given key.</param>
		/// <returns></returns>
		public static void AddOrNew<T, A>(this SortedDictionary<T, List<A>> dict, T key, A element, bool uniqueEntriesOnly = false)
		{
			if (dict.ContainsKey(key))
			{
				if (!uniqueEntriesOnly || !dict[key].Contains(element))
					dict[key].Add(element);
			}
			else
				dict[key] = new List<A>() { element };
		}

		/// <summary>
		/// If the sorted dictionary contains the given key, the element is removed from the value list.
		/// If that list becomes empty, the key is removed from the dictionary.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="A"></typeparam>
		/// <param name="dict"></param>
		/// <param name="key"></param>
		/// <param name="element"></param>
		/// <returns>True if the key was deleted from the dictionary</returns>
		public static bool RemoveAndDelete<T, A>(this SortedDictionary<T, List<A>> dict, T key, A element)
		{
			if (dict.ContainsKey(key))
			{
				dict[key].Remove(element);
				if (dict[key].Count == 0)
					return dict.Remove(key);
			}
			return false;
		}

		/// <summary>
		/// If the sorted dictionary contains the given key, the element is added to the value HashSet.
		/// If not, a new entry is created by creating a new value HashSet and adding the element to it.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="A"></typeparam>
		/// <param name="dict"></param>
		/// <param name="key"></param>
		/// <param name="element"></param>
		/// <param name="uniqueEntriesOnly">If true, the element is only added if it does not already exist in the HashSet for the given key.</param>
		/// <returns></returns>
		public static void AddOrNew<T, A>(this SortedDictionary<T, HashSet<A>> dict, T key, A element, bool uniqueEntriesOnly = false)
		{
			if (dict.ContainsKey(key))
			{
				if (!uniqueEntriesOnly || !dict[key].Contains(element))
					dict[key].Add(element);
			}
			else
				dict[key] = new HashSet<A>() { element };
		}

		/// <summary>
		/// If the sorted dictionary contains the given key, the element is removed from the value HashSet.
		/// If that HashSet becomes empty, the key is removed from the dictionary.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="A"></typeparam>
		/// <param name="dict"></param>
		/// <param name="key"></param>
		/// <param name="element"></param>
		/// <returns>True if the key was deleted from the dictionary</returns>
		public static bool RemoveAndDelete<T, A>(this SortedDictionary<T, HashSet<A>> dict, T key, A element)
		{
			if (dict.ContainsKey(key))
			{
				dict[key].Remove(element);
				if (dict[key].Count == 0)
					return dict.Remove(key);
			}
			return false;
		}

		/// <summary>
		/// If the dictionary contains the given key, the element list is added to the value list.
		/// If not, a new entry is created by creating a new value list and adding the element list to it.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="A"></typeparam>
		/// <param name="dict"></param>
		/// <param name="key"></param>
		/// <param name="element"></param>
		/// <returns></returns>
		public static void AddOrNew<T, A>(this IDictionary<T, List<A>> dict, T key, IEnumerable<A> element)
		{
			dict.ItemOrAdd(key, () => new List<A>()).AddRange(element);
		}

		/// <summary>
		/// If the dictionary contains the given key, the 'val' is added to the value.
		/// If not, a new entry is created by creating a new entry with the given value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dict"></param>
		/// <param name="key"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		public static void AddOrNew<T>(this IDictionary<T, int> dict, T key, int val)
		{
			if (dict.ContainsKey(key))
				dict[key] += val;
			else
				dict[key] = val;
		}

		/// <summary>
		/// If the dictionary contains the given key, the 'val' is added to the value.
		/// If not, a new entry is created by creating a new entry with the given value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dict"></param>
		/// <param name="key"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		public static void AddOrNew<T>(this IDictionary<T, long> dict, T key, long val)
		{
			if (dict.ContainsKey(key))
				dict[key] += val;
			else
				dict[key] = val;
		}

		/// <summary>
		/// If the dictionary contains the given key, the 'val' is added to the value.
		/// If not, a new entry is created by creating a new entry with the given value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dict"></param>
		/// <param name="key"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		public static void AddOrNew<T>(this IDictionary<T, double> dict, T key, double val)
		{
			if (dict.ContainsKey(key))
				dict[key] += val;
			else
				dict[key] = val;
		}

		/// <summary>
		/// If the dictionary contains the given key, the 'val' is added to the value.
		/// If not, a new entry is created by creating a new entry with the given value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dict"></param>
		/// <param name="key"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		public static void AddOrNew<T>(this IDictionary<T, Complex> dict, T key, Complex val)
		{
			if (dict.ContainsKey(key))
				dict[key] += val;
			else
				dict[key] = val;
		}

		/// <summary>
		/// If the dictionary contains the given key, <paramref name="valueToAdd"/> is added to the value.
		/// If not, a new entry is created with value <paramref name="initialValue"/>+<paramref name="valueToAdd"/>
		/// </summary>
		/// <param name="dictionary">The dictionary to update</param>
		/// <param name="key">The key of the entry to update</param>
		/// <param name="initialValue">The initial value to give to the entry if it does not exist</param>
		/// <param name="valueToAdd">The value to add to the entry</param>
		public static void AddOrNew<T>(this IDictionary<T, int> dictionary, T key, int valueToAdd, int initialValue)
		{
			if (dictionary.ContainsKey(key))
				dictionary[key] += valueToAdd;
			else
				dictionary[key] = initialValue + valueToAdd;
		}


		/// <summary>
		/// Returns the dictionary's value corresponding to the given key, if the key exists (like '[key]').
		/// If the key does not exists, it return default(V).
		/// </summary>
		/// <typeparam name="K"></typeparam>
		/// <typeparam name="V"></typeparam>
		/// <param name="dict"></param>
		/// <param name="key"></param>
		public static V ItemOrDefault<K, V>(this IDictionary<K, V> dict, K key)
		{
			if (dict.ContainsKey(key))
				return dict[key];
			else
				return default;
		}


		/// <summary>
		/// Returns the dictionary's value corresponding to the given key, if the key exists (like '[key]').
		/// If the key does not exists, it returns the given default value.
		/// </summary>
		public static V ItemOrDefaultValue<K, V>(this IDictionary<K, V> dict, K key, V defaultvalue)
		{
			if (dict.ContainsKey(key))
				return dict[key];
			else
				return defaultvalue;
		}

		/// <summary>
		/// Returns the dictionary's value corresponding to the given key, if the key exists (like '[key]').
		/// If the key does not exist, evaluates the given function to get a value that is added to the
		/// dictionary for the given key, and returns the value.
		/// </summary>
		/// <typeparam name="K"></typeparam>
		/// <typeparam name="V"></typeparam>
		/// <param name="dict"></param>
		/// <param name="key"></param>
		/// <param name="newValueFunction">The function that returns the new value if the key does not exist in the dictionary</param>
		public static V ItemOrAdd<K, V>(this IDictionary<K, V> dict, K key, Func<V> newValueFunction)
		{
			if (!dict.TryGetValue(key, out V value))
			{
				dict[key] = value = newValueFunction();
			}
			return value;
		}

		/// <summary>
		/// If the dictionary contains the given key, the element is added to the value dictionary.
		/// If not, a new entry is created by creating a new value dictionary and adding the element to it.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="A"></typeparam>
		/// <typeparam name="B"></typeparam>
		/// <param name="dict"></param>
		/// <param name="key"></param>
		/// <param name="elementKey"></param>
		/// <param name="elementValue"></param>
		public static void AddOrNew<T, A, B>(this IDictionary<T, Dictionary<A, B>> dict, T key, A elementKey, B elementValue)
		{
			dict.ItemOrAdd(key, () => new Dictionary<A, B>())[elementKey] = elementValue;
		}

		/// <summary>
		/// If the dictionary contains the given key, the element is added to the value dictionary's value list 
		/// If not, a new entry is created.
		/// If the second dictionary
		/// does not contain the elementKey, this is added with a new value list before adding the elementValueItem to  that list.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="A"></typeparam>
		/// <typeparam name="B"></typeparam>
		/// <param name="dict"></param>
		/// <param name="key"></param>
		/// <param name="elementKey">The key of the entry to add or update</param>
		/// <param name="elementValueItem"></param>
		public static void AddOrNew<T, A, B>(this IDictionary<T, Dictionary<A, List<B>>> dict, T key, A elementKey, B elementValueItem)
		{
			dict.ItemOrAdd(key, () => new Dictionary<A, List<B>>()).ItemOrAdd(elementKey, () => new List<B>()).Add(elementValueItem);
		}

		/// <summary>
		/// Removes the <paramref name="elementValueItem"/> from the value list of the inner dictionary. If this leaves the list empty, removes the entry
		/// indicated by <paramref name="elementKey"/> from the inner dictionary. If this leaves the inner dictionary empty,
		/// the element indicated by <paramref name="key"/> is removed from the outer dictionary.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="A"></typeparam>
		/// <typeparam name="B"></typeparam>
		/// <param name="dict"></param>
		/// <param name="key"></param>
		/// <param name="elementKey"></param>
		/// <param name="elementValueItem"></param>
		/// <returns>True if the element was removed, false if it was not found.</returns>
		public static bool Remove<T, A, B>(this IDictionary<T, Dictionary<A, List<B>>> dict, T key, A elementKey, B elementValueItem)
		{
			bool removed = false;
			if (dict.ContainsKey(key))
			{
				Dictionary<A, List<B>> temp = dict[key];
				removed = temp.Remove(elementKey, elementValueItem);
				if (removed)
				{
					if (!temp.Any())
						dict.Remove(key);
				}
			}
			return removed;
		}

		/// <summary>
		/// Removes the <paramref name="elementValueItem"/> from the value list. If this leaves the list empty, removes the entry
		/// indicated by <paramref name="elementKey"/> from the dictionary. 
		/// </summary>
		/// <typeparam name="A"></typeparam>
		/// <typeparam name="B"></typeparam>
		/// <param name="temp"></param>
		/// <param name="elementKey"></param>
		/// <param name="elementValueItem"></param>
		/// <returns>True if the element was removed, false if it was not found.</returns>
		public static bool Remove<A, B>(this IDictionary<A, List<B>> temp, A elementKey, B elementValueItem)
		{
			bool removed = false;
			if (temp.ContainsKey(elementKey))
			{
				if (temp[elementKey].Remove(elementValueItem))
				{
					if (!temp[elementKey].Any())
						removed = temp.Remove(elementKey);
				}
			}
			return removed;
		}

		/// <summary>
		/// Compares this dictionary of lists of items with another, and returns true 
		/// if both dictionaries have the same keys (regardless of order), and for
		/// each key, the two dictionaries have the same values (as determined by standard comparer), 
		/// regardless of order.
		/// </summary>
		/// <typeparam name="A">The type of key</typeparam>
		/// <typeparam name="B">The type of list item</typeparam>
		/// <param name="myself">This dictionary</param>
		/// <param name="other">The other dictionary.</param>
		/// <returns></returns>
		public static bool KeysAndValueEqual<A, B>(this IDictionary<A, List<B>> myself, IDictionary<A, List<B>> other)
		{
			return myself.Keys.SetEquals(other.Keys) && myself.All(kvp => kvp.Value.SetEquals(other[kvp.Key]));
		}

		/// <summary>
		/// Attempts to find an element value item indicated by two keys in a nested pair of dictionaries, in which the inner one has a list
		/// of elements as its value. Returns true if successful, and the found value list in the output parameter.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="A"></typeparam>
		/// <typeparam name="B"></typeparam>
		/// <param name="dict"></param>
		/// <param name="key"></param>
		/// <param name="elementKey"></param>
		/// <param name="elementValueList">On output, this is the found list of values. null is returned if the element was not found.</param>
		/// <returns>True if the element was found, false otherwise. </returns>
		public static bool TryGetValue<T, A, B>(this IDictionary<T, Dictionary<A, List<B>>> dict, T key, A elementKey, out List<B> elementValueList)
		{
			elementValueList = null;
			if (dict.ContainsKey(key))
			{
				Dictionary<A, List<B>> temp = dict[key];
				if (temp.ContainsKey(elementKey))
				{
					elementValueList = temp[elementKey];
					return true;
				}
			}

			//No joy
			return false;
		}

		/// <summary>
		/// Deconstructs a KeyValuePair into the key and the value. Allows you to write e.g.:
		/// <code>foreach (var (key, value) in dictionary) {...}</code>
		/// </summary>
		public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> source, out TKey key, out TValue value)
		{
			key = source.Key;
			value = source.Value;
		}

		/// <summary>
		/// Deconstructs an array of length 2 into two variables. Allows you to write e.g.:
		/// <code>var (name, value) = myString.Split('=');</code>
		/// </summary>
		public static void Deconstruct<T>(this T[] array, out T element1, out T element2)
		{
			if (array.Length != 2)
				throw new ArgumentException("The array must have length 2");

			element1 = array[0];
			element2 = array[1];
		}

		/// <summary>
		/// Deconstructs an enumerable of length 2 into two variables. Allows you to write e.g.:
		/// <code>var (name, value) = list.Take(2);</code>
		/// </summary>
		public static void Deconstruct<T>(this IEnumerable<T> enumerable, out T element1, out T element2)
		{
			(element1, element2) = enumerable.ToArray();
		}

		/// <summary>
		/// Deconstructs an array of length 3 into three variables.
		/// </summary>
		public static void Deconstruct<T>(this T[] array, out T element1, out T element2, out T element3)
		{
			if (array.Length != 3)
				throw new ArgumentException("The array must have length 3");

			element1 = array[0];
			element2 = array[1];
			element3 = array[2];
		}

		/// <summary>
		/// Deconstructs an enumerable of length 3 into three variables.
		/// </summary>
		public static void Deconstruct<T>(this IEnumerable<T> enumerable, out T element1, out T element2, out T element3)
		{
			(element1, element2, element3) = enumerable.ToArray();
		}

		/// <summary>
		/// Returns true if the two enumarations contains the same elements
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="myList"></param>
		/// <param name="otherList"></param>
		/// <param name="eqComp">Equality comparer used to compare elements. If not given, the default comparer is used.</param>
		public static bool SetEquals<T>(this IEnumerable<T> myList, IEnumerable<T> otherList, IEqualityComparer<T> eqComp = null)
		{
			if (eqComp == null)
				return new HashSet<T>(myList).SetEquals(otherList);

			return new HashSet<T>(myList, eqComp).SetEquals(otherList);
		}

		/// <summary>
		/// Returns true if the 'a' enumarations is a strict sub set of the 'b' enumeration
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		/// <param name="b"></param>
		public static bool IsProperSubsetOf<T>(this IEnumerable<T> a, IEnumerable<T> b)//, IEqualityComparer<T> eqComp = null)
		{
			return (new HashSet<T>(a)).IsProperSubsetOf(b);

			//int acount = a.Count();
			//if(acount >= b.Count())
			//	return false;

			//IEnumerable<T> intersection = eqComp == null ? a.Intersect(b) : a.Intersect(b,eqComp);
			//return acount == intersection.Count();
		}

		/// <summary>
		/// Returns true if the 'a' enumarations is a sub set, or set equal, of the 'b' enumeration.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		/// <param name="b"></param>
		public static bool IsSubsetOf<T>(this IEnumerable<T> a, IEnumerable<T> b)//, IEqualityComparer<T> eqComp = null)
		{
			return new HashSet<T>(a).IsSubsetOf(b); //This includes Set Equals, according to the ISet documentation.
		}

		/// <summary>
		/// Returns true if the 'a' collection is a sub set, or set equal, of the 'b' collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		/// <param name="b"></param>
		public static bool IsSubsetOf<T>(this ICollection<T> a, ICollection<T> b)//, IEqualityComparer<T> eqComp = null)
		{
			int acount = a.Count;
			if (acount > b.Count)
				return false;
			else if (acount == b.Count)
				return new HashSet<T>(a).SetEquals(b);//, eqComp);
			else
				return new HashSet<T>(a).IsProperSubsetOf(b);//, eqComp);
		}

		/// <summary>
		/// Returns true if the 'a' set is a sub set, or set equal, of the 'b' set.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="eqComp">Equality comparer used to compare elements. If not given, the default comparer is used.</param>
		public static bool SubSetEq<T>(this ISet<T> a, ISet<T> b, IEqualityComparer<T> eqComp = null)
		{
			return a.IsSubsetOf(b);
		}

		/// <summary>
		/// Returns true if this list is a sublist of <paramref name="b"/>.
		/// That is, if all elements in this list occur in <paramref name="b"/>, and in the same order,
		/// but not necessarily consecutively.
		/// If both lists are equal, this is also considered to be a sublist.
		/// </summary>
		/// <param name="a">The sublist to check for</param>
		/// <param name="b">The list to check whether this list is a sublist of</param>
		public static bool IsSublistOf<T>(this IReadOnlyList<T> a, IReadOnlyList<T> b)
		{
			int ii_b = 0;

			for (int ii_a = 0; ii_a < a.Count(); ++ii_a)
			{
				if (ii_b == b.Count)
					return false;

				while (!a[ii_a].Equals(b[ii_b]))
				{
					++ii_b;
					if (ii_b == b.Count)
						return false;
				}
				++ii_b;
			}
			return true;
		}

		/// <summary>
		/// Clones the dictionary with shallow copies.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TElement"></typeparam>
		/// <param name="dict"></param>
		/// <returns></returns>
		public static Dictionary<TKey, TElement> Clone<TKey, TElement>(this Dictionary<TKey, TElement> dict) => dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

		/// <summary>
		/// Returns a dictionary that is a copy of the source dictionary, plus that a default value has been added
		/// for those of the given keys that are not in the source dictionary
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TElement"></typeparam>
		/// <param name="source">The dictionary to copy</param>
		/// <param name="keys">The keys to ensure have a value</param>
		/// <param name="defaultValue">The value to give to keys not in the input dictionary</param>
		/// <returns></returns>
		public static Dictionary<TKey, TElement> WithDefaultFor<TKey, TElement>(this IDictionary<TKey, TElement> source, IEnumerable<TKey> keys, TElement defaultValue = default)
		{
			Dictionary<TKey, TElement> dict = new Dictionary<TKey, TElement>(source);
			foreach (var key in keys)
			{
				if (!dict.ContainsKey(key))
					dict.Add(key, defaultValue);
			}
			return dict;
		}

		/// <summary>
		/// Returns new array that contains the elements of a range in the argument
		/// </summary>
		/// <typeparam name="T">The type of elements in the array</typeparam>
		/// <param name="array">The source array</param>
		/// <param name="startIndex">The index of the first element to copy</param>
		/// <param name="length">The number of elements to copy</param>
		/// <returns>A new array containing the range</returns>
		public static T[] SubArray<T>(this T[] array, int startIndex, int length)
		{
			T[] result = new T[length];

			for (int i = 0; i < length; ++i)
				result[i] = array[startIndex + i];
			return result;
		}

		/// <summary>
		/// Efficiently creates a clone of the given array
		/// </summary>
		public static T[] CloneArray<T>(this T[] source)
		{
			var result = Array.CreateInstance(typeof(T), source.Length);
			Array.Copy(source, result, source.Length);
			return (T[])result;
		}

		/// <summary>
		/// Returns the range of indices for which the corresponding element in the sorted list compares
		/// equal to the given item, according to the comparer.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">The list to search. Must be sorted according to the comparer</param>
		/// <param name="item">The item to search for</param>
		/// <param name="comparer">The comparer to use</param>
		/// <returns>The range of indices. The first index is inclusive, the second
		/// is exclusive. If the indices are equal, there are no matches, and the indices
		/// indicate the first larger element in the list</returns>
		public static Tuple<int, int> EqualRange<T>(this IList<T> list, T item, IComparer<T> comparer)
		{
			int firstEqualOrLargerIndex = list.BinaryFirstIndex(x => comparer.Compare(x, item) >= 0);
			int firstLargerIndex = list.BinaryFirstIndex(x => comparer.Compare(x, item) > 0, firstEqualOrLargerIndex);

			return new Tuple<int, int>(firstEqualOrLargerIndex, firstLargerIndex);
		}

	}
}
