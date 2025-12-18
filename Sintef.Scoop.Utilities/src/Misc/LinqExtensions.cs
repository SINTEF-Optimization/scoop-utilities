//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Linq related extension methods.
	/// </summary>
	public static partial class Extensions
	{
		/// <summary>
		/// Executes the given action on each member of the enumerable
		/// </summary>
		public static void Do<T>(this IEnumerable<T> source, Action<T> action)
		{   // note: omitted arg/null checks
			foreach (T item in source) { action(item); }
		}

		/// <summary>
		/// Returns a random element in the collection
		/// </summary>
		public static T RandomElement<T>(
				this IEnumerable<T> source,
				Random r)
		{
			return source.ElementAt(r.Next(source.Count()));
		}

		/// <summary>
		/// Returns the element in the collection at index: index % Count()
		/// </summary>
		public static T CyclicElementAt<T>(
				this IEnumerable<T> source,
				int index)
		{
			return source.ElementAt(index % source.Count());
		}


		/// <inheritdoc cref="IList{T}.IndexOf(T)"/>
		public static int IndexOf<T>(this IReadOnlyList<T> list, T item)
		{
			if (list is IList<T> l)
				return l.IndexOf(item);
			return list.ToList().IndexOf(item);
		}

		/// <summary>
		/// Returns a random element in the collection, or the default value
		/// if the collection is empty
		/// </summary>
		public static T RandomElementOrDefault<T>(
				this IEnumerable<T> collection,
				Random r)
		{
			if (!collection.Any())
				return default;

			return collection.ElementAt(r.Next(collection.Count()));
		}

		/// <summary>
		/// Returns a random element in the collection among those that satisfies the given
		/// predicate, or the default value if there is no such element
		/// </summary>
		public static T RandomElementOrDefault<T>(
				this IList<T> collection,
				Random r,
				Func<T, bool> predicate)
		{
			int count = collection.Count;
			if (count == 0)
				return default;

			// Try finding a random element first and then checking the predicate
			// This is quick if the predicate accepts most elements
			for (int i = 0; i < count / 3; ++i)
			{
				T element = collection[r.Next(count)];
				if (predicate(element))
					return element;
			}

			// Otherwise, we need to filter by the predicate first and then select
			// a random element from the result
			return collection.Where(predicate).ToList().RandomElementOrDefault(r);
		}

		/// <summary>
		/// Returns a list of random element in the collection, of a given size, sorted in the 
		/// order they appear in the collection
		/// If the size is larger or equal to the collection, the whole collection is returned.
		/// </summary>
		/// <param name="collection">The collection to choose from.</param>
		/// <param name="numElements">The number of random elements to choose.</param>
		/// <param name="rand">The random generator to use. Optional. If null, then a new will be created using  <see cref="RandomCreator.GetRandomGenerator"/>.</param>
		public static IEnumerable<T> RandomElements<T>(
				this IEnumerable<T> collection, int numElements,
				Random rand = null)
		{
			int n = collection.Count();
			if (n <= numElements)
			{
				foreach (T e in collection)
				{
					yield return e;
				}
				yield break;
			}

			Random r = rand ?? RandomCreator.GetRandomGenerator();
			int prevIndex = 0;
			for (int i = 0; i < numElements; i++)
			{
				int upperExclusive = n - numElements + i + 1;
				int index = r.Next(prevIndex, upperExclusive);
				yield return collection.ElementAt(index);
				prevIndex = index + 1; //Inclusive lower limit
			}
		}

		/// <summary>
		/// Returns a sequence of the adjacent pairs in the source sequence.
		/// Each element in the result is a Tuple where Item1 is the former
		/// and Item2 the latter of the adjacent pair.
		/// </summary>
		public static IEnumerable<Tuple<T, T>> AdjacentPairs<T>(this IEnumerable<T> source)
		{
			// The implementation was previously:
			// source.Skip(1).Zip(source, (second, first) => new Tuple<T, T>(first, second))
			// But this was not correct for non-deterministic sequences, since it enumerate the source twice.

			var enumerator = source.GetEnumerator();
			if (!enumerator.MoveNext())
				yield break;

			var prev = enumerator.Current;
			while (enumerator.MoveNext())
			{
				var cur = enumerator.Current;
				yield return new Tuple<T, T>(prev, cur);
				prev = cur;
			}
		}

		/// <summary>
		/// Converts a sequence of pair Tuples to the equivalent sequence of value tuples
		/// </summary>
		public static IEnumerable<(T1, T2)> ToValuePairs<T1, T2>(this IEnumerable<Tuple<T1, T2>> source) =>
			source.Select(x => (x.Item1, x.Item2));


		// This version of zip was added to the framework in .NET Core 3.0, so it's no longer needed
#if NETFRAMEWORK || NETSTANDARD

		/// <summary>
		/// Enumerates the pairs of elements at corresponding positions
		/// in the sequences. The result has the same length as the shorter of
		/// the sequences.
		/// </summary>
		public static IEnumerable<Tuple<TFirst, TSecond>> Zip<TFirst, TSecond>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second)
		{
			return first.Zip(second, Tuple.Create);
		}

#endif

		/// <summary>
		/// Splits the source enumerable into groups of size <paramref name="groupSize"/>, except the last group, which
		/// may be smaller.
		/// <code>x.InGroupsOf(y).SelectMany(g => g)</code> is equvalent to <code>x</code> for
		/// any <see cref="IEnumerable"/> x and positive integer y.
		/// </summary>
		public static IEnumerable<IEnumerable<T>> InGroupsOf<T>(this IEnumerable<T> source, int groupSize)
		{
			if (groupSize <= 0)
				throw new ArgumentException("Group size must be positive");

			var currentGroup = new List<T>(groupSize);
			foreach (var item in source)
			{
				currentGroup.Add(item);
				if (currentGroup.Count == groupSize)
				{
					yield return currentGroup;
					currentGroup = new List<T>(groupSize);
				}
			}
			if (currentGroup.Count != 0)
				yield return currentGroup;
		}

		/// <summary>
		/// Joins a set of enumerables to a single enumerable. The function considers the first element in each enumerable,
		/// and returns the one selected (ordered first by) using the given <paramref name="selector"/>. The process is repeated until all elements 
		/// in all enumerables have been returned.
		/// This can be used, e.g., to return a sorted enumerable of elements from many sorted enumerables of elements.
		/// </summary>
		/// <param name="sequences"></param>
		/// <param name="selector"></param>
		/// <returns></returns>
		public static IEnumerable<T> Combine<T, Key>(this IEnumerable<IEnumerable<T>> sequences, Func<T, Key> selector) where Key : IComparable<Key>
		{
			List<IEnumerator<T>> enumerators = sequences.Where(s => s.Any()).Select(s => s.GetEnumerator()).ToList();
			enumerators.Do(e => e.MoveNext());
			while (enumerators.Any())
			{
				IEnumerator<T> selectedEnumerator = enumerators.MinBy(x => selector(x.Current));
				yield return selectedEnumerator.Current;
				if (!selectedEnumerator.MoveNext())
					enumerators.Remove(selectedEnumerator);
			}
		}

		/// <summary>
		/// Returns true if all elements in the enumerable are equal. Comparing
		/// elements by calling Equals
		/// </summary>
		/// <param name="source"></param>
		/// <returns>True iff all are equal, or if the enumerable is empty.</returns>
		public static bool AllEqual<T>(this IEnumerable<T> source)
		{
			if (!source.Any())
				return true;
			else
			{
				T first = source.First();
				var cmp = EqualityComparer<T>.Default;
				return source.Skip(1).All(e => cmp.Equals(e, first));
			}
		}

		/// <summary>
		/// Returns true if all elements in the enumerable are different. Comparing
		/// elements by calling Equals
		/// </summary>
		/// <param name="source"></param>
		/// <returns>True iff all are different, or if the enumerable is empty.</returns>
		public static bool AllDifferent<T>(this IEnumerable<T> source)
		{
			if (!source.Any())
				return true;
			else
			{
				IEnumerable<T> distinct = source.Distinct();
				return distinct.SetEquals(source);
			}
		}

		/// <summary>
		/// Returns true if all elements in the enumerable are equal. Comparing
		/// elements by calling Equals.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="selector">Function delegate mapping whatever is in the enumerable to a E.</param>
		/// <returns>True iff all are equal, or if the enumerable is empty.</returns>
		public static bool AllEqual<T, E>(this IEnumerable<T> source, Func<T, E> selector)
		{
			if (!source.Any())
				return true;
			else
			{
				E first = selector(source.First());
				var cmp = EqualityComparer<E>.Default;
				return source.Skip(1).All(e => cmp.Equals(selector(e), first));
			}
		}

		/// <summary>
		/// Returns a sequence where even-index elements are taken from first
		/// and odd-index are from second. The result has twice the length of the
		/// shorter of the input sequences.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<T> Interleave<T>(this IEnumerable<T> first, IEnumerable<T> second)
		{
			var iter1 = first.GetEnumerator();
			var iter2 = second.GetEnumerator();
			while (iter1.MoveNext() && iter2.MoveNext())
			{
				yield return iter1.Current;
				yield return iter2.Current;
			}
		}

		/// <summary>
		/// Returns a sequence where the given element has been inserted between adjacent
		/// elements of the source sequence.
		/// The result is one shorter than twice the length of the source sequence
		/// (unless the source sequence is empty).
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<T> Interleave<T>(this IEnumerable<T> source, T interleaveElement)
		{
			if (!source.Any())
				yield break;

			yield return source.First();

			foreach (var element in source.Skip(1))
			{
				yield return interleaveElement;
				yield return element;
			}
		}

		/// <summary>
		/// Returns a sequence that contains every n'th element of the source
		/// sequence. So, if n = 5, we return element no. 0, 5, 10, etc.
		/// </summary>
		public static IEnumerable<T> TakeEvery<T>(this IEnumerable<T> source, int step)
		{
			var iter = source.GetEnumerator();
			int i = 0;
			while (iter.MoveNext())
			{
				if (i == 0)
				{
					yield return iter.Current;
					i = step;
				}
				--i;
			}
		}

		/// <summary>
		/// Returns elements from the source sequence up to and including the first element
		/// that satisfies the predicate (or the whole sequence if none does).
		/// </summary>
		public static IEnumerable<T> TakeUntil<T>(this IEnumerable<T> source, Func<T, bool> predicate)
		{
			foreach (var item in source)
			{
				yield return item;
				if (predicate(item))
					break;
			}
		}

		/// <summary>
		/// Enumerates the subsequences of consecutive items from the source that satisfy the given predicate.
		/// </summary>
		public static IEnumerable<IEnumerable<T>> Subsequences<T>(this IEnumerable<T> source, Func<T, bool> predicate)
		{
			List<T> currentGroup = new();
			foreach (var item in source)
			{
				if (predicate(item))
				{
					currentGroup.Add(item);
				}
				else if (currentGroup.Count != 0)
				{
					yield return currentGroup;
					currentGroup = new();
				}
			}
			if (currentGroup.Count != 0)
				yield return currentGroup;
		}

		/// <summary>
		/// Removes repetitions of the same value, but not duplicates occuring later.
		/// </summary>
		/// <remarks>
		/// Consider { 5, 7, 7, 8, 8, 8, 2, 5, 5, 8, 8, 1, 1, 9 };
		/// Applying <see cref="WithoutRepetitions{T}"/> yields
		/// { 5, 7, 8, 2, 5, 8, 1, 9 }.
		/// Applying <see cref="Enumerable.Distinct{TSource}(IEnumerable{TSource})"/> yields
		/// { 5, 7, 8, 2, 1, 9 }.
		/// </remarks>
		public static IEnumerable<T> WithoutRepetitions<T>(this IEnumerable<T> set)
		{
			var comparer = EqualityComparer<T>.Default;

			if (!set.Any())
				yield break;

			T last = set.First();
			foreach (T element in set.Skip(1))
			{
				if (comparer.Equals(element, last))
					continue;
				yield return last;
				last = element;
			}
			yield return last;
		}

		/// <summary>
		///  Returns elements from a sequence that have distinct keys, by using the default equality comparer
		///  to compare values.
		/// </summary>
		public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			HashSet<TKey> seenKeys = new();
			foreach (TSource element in source)
			{
				if (seenKeys.Add(keySelector(element)))
				{
					yield return element;
				}
			}
		}

		/// <summary>
		/// Returns true if the sequence contains two or more elements that compare equal, false otherwise
		/// </summary>
		/// <param name="source">The sequence</param>
		/// <param name="duplicate">Contains one of the equal elements if the function returns true.
		///   Otherwise is set to <typeparamref name="T"/>'s default value</param>
		public static bool TryFindDuplicate<T>(this IEnumerable<T> source, out T duplicate)
			=> source.TryFindDuplicateBy(x => x, out duplicate);

		/// <summary>
		/// Returns true if the sequence contains two or more elements for which <paramref name="keySelector"/> 
		/// produces equal values, false otherwise
		/// </summary>
		/// <param name="source">The sequence</param>
		/// <param name="keySelector"></param>
		/// <param name="duplicate">Contains one of the equal elements if the function returns true.
		///   Otherwise is set to <typeparamref name="T"/>'s default value</param>
		public static bool TryFindDuplicateBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, out T duplicate)
		{
			var groups = source.GroupBy(keySelector);
			var groupWithDuplicate = groups.FirstOrDefault(g => g.Count() >= 2);

			if (groupWithDuplicate != null)
			{
				duplicate = groupWithDuplicate.First();
				return true;
			}

			duplicate = default;
			return false;
		}

		/// <summary>
		/// Returns a sequence that is identical to the source sequence, except that
		/// enumeration terminates after the given time (counted from when the first element
		/// is enumerated).
		/// </summary>
		public static IEnumerable<T> StopAfter<T>(this IEnumerable<T> source, TimeSpan time)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();

			foreach (var element in source)
			{
				yield return element;

				if (sw.Elapsed > time)
					yield break;
			}
		}

		/// <summary>
		/// Returns a sequence from which a single element has been removed, if it is
		/// present in the source sequence
		/// </summary>
		public static IEnumerable<T> Except<T>(this IEnumerable<T> source, T element)
		{
			return source.Except(new[] { element });
		}

		/// <summary>
		/// Returns an enumerable from a value.
		/// </summary>
		public static IEnumerable<T> Yield<T>(this T item)
		{
			yield return item;
		}

		/// <summary>
		/// Returns the only element of a sequence, and throws the given exception if there is not
		/// exactly one element in the sequence.
		/// </summary>
		/// <param name="source">Sequence of which the only element to return</param>
		/// <param name="e">Exception to throw if sequence has no or more than 1 element.</param>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is null</exception>
		public static T Single<T>(this IEnumerable<T> source, Exception e)
		{
			try
			{
				return source.Single();
			}
			catch (InvalidOperationException)
			{
				throw e;
			}
		}

		/// <summary>
		/// Returns the only element of a sequence that satisfies a specified condition,
		/// and throws the given exception if more than one such element exists.
		/// </summary>
		/// <param name="source">Sequence of which the only element to return</param>
		/// <param name="predicate">A function to test an element for a condition.</param>
		/// <param name="e">Exception to throw if sequence has no or more than 1 element.</param>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is null</exception>
		public static T Single<T>(this IEnumerable<T> source, Func<T, bool> predicate, Exception e)
		{
			try
			{
				return source.Single(predicate);
			}
			catch (InvalidOperationException)
			{
				throw e;
			}
		}

		/// <summary>
		/// Returns the only element of a sequence, or a default value if the sequence is
		/// empty; this method throws the given exception if there is more than one element in the
		/// sequence.
		/// </summary>
		/// <param name="source">Sequence of which the only element to return or default value</param>
		/// <param name="e">Exception to throw if sequence has more than 1 element.</param>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is null</exception>
		public static T SingleOrDefault<T>(this IEnumerable<T> source, Exception e)
		{
			try
			{
				return source.SingleOrDefault();
			}
			catch (InvalidOperationException)
			{
				throw e;
			}
		}
		/// <summary>
		/// Evaluates whether the count of items equals the value using lazy evaluation.
		/// </summary>
		/// <param name="source">The enumerable to evaluate</param>
		/// <param name="value">The value to evaluate the count against</param>
		/// <returns>Whether the count of the source equals the value</returns>
		public static bool CountIs<T>(this IEnumerable<T> source, int value)
		{
			foreach (var i in source)
			{
				if (--value < 0)
					break;
			}
			return value == 0;
		}


		/// <summary>
		/// Evaluates whether the count of items is less than the value using lazy evaluation.
		/// </summary>
		/// <param name="source">The enumerable to evaluate</param>
		/// <param name="value">The value to evaluate the count against</param>
		/// <returns>Whether the count of the source equals the value</returns>
		public static bool CountIsLessThan<T>(this IEnumerable<T> source, int value) => CountIsLessOrEqual<T>(source, value - 1);


		/// <summary>
		/// Evaluates whether the count of items is less or equals the value using lazy evaluation.
		/// </summary>
		/// <param name="source">The enumerable to evaluate</param>
		/// <param name="value">The value to evaluate the count against</param>
		/// <returns>Whether the count of the source equals the value</returns>
		public static bool CountIsLessOrEqual<T>(this IEnumerable<T> source, int value)
		{
			foreach (var i in source)
			{
				if (--value < 0)
					break;
			}
			return value >= 0;
		}

		/// <summary>
		/// Returns true if <paramref name="subsequence"/> occurs as a 
		/// (not necessarrily contiguous)
		/// subsequence of <paramref name="sequence"/>, false otherwise
		/// </summary>
		/// <param name="sequence">The sequence to look for a subsequence in</param>
		/// <param name="subsequence">The subsequence to look for</param>
		public static bool HasSubsequence<T>(this IEnumerable<T> sequence, IEnumerable<T> subsequence)
		{
			return sequence.Where(x => subsequence.Contains(x)).SequenceEqual(subsequence);
		}

		/// <summary>
		/// Performs like ToDictionary, but leaves out all but the first in each set of items with equal keys
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TElement"></typeparam>
		/// <param name="source"></param>
		/// <param name="keySelector"></param>
		/// <param name="elementSelector"></param>
		/// <returns></returns>
		public static Dictionary<TKey, TElement> ToDictionaryIgnoringDuplicates<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
		{
			Dictionary<TKey, TElement> dict = new Dictionary<TKey, TElement>();
			foreach (var item in source)
			{
				var key = keySelector.Invoke(item);
				if (!dict.ContainsKey(key))
				{
					var value = elementSelector.Invoke(item);
					dict.Add(key, value);
				}
			}
			return dict;
		}

		/// <summary>
		/// Returns a hash set containing all elements in the source sequence.
		/// 
		/// This function used to be called ToHashSet, but was renamed to avoid colliding with
		/// the function that now exists from
		/// .NET Standard 2.1, Core 2.0 and Framework 4.7.2.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <returns></returns>
		public static HashSet<T> ToHashSetScoop<T>(this IEnumerable<T> source) => new HashSet<T>(source);

		/// <summary>
		/// Returns a new IEnumerable where the given element is added directly behind the given
		/// previous element which is assumed to be in the given sequence (comparison is performed
		/// by <see cref="Object.Equals(object)"/> such that it can be overridden). If
		/// previousElement is null, it will be added as the first element in the returned sequence.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sequence">An enumerable sequence to add an element to.</param>
		/// <param name="elementToInsert">The element to add to the enumeration.</param>
		/// <param name="previousElement">The element which it is added after.</param>
		/// <returns>A new IEnumerable with the added element.</returns>
		public static IEnumerable<T> InsertAfter<T>(this IEnumerable<T> sequence, T elementToInsert, T previousElement) where T : class
		{
			T prev = null;

			foreach (var element in sequence)
			{
				if ((previousElement == null && prev == null) || (previousElement != null && previousElement.Equals(prev)))
				{
					yield return elementToInsert;
				}
				prev = element;
				yield return element;
			}
			if ((previousElement == null && prev == null) || (previousElement != null && previousElement.Equals(prev)))
			{
				yield return elementToInsert;
			}
		}

		/// <summary>
		/// Enumerates all subsets of size <paramref name="k"/> from <paramref name="elements"/>.
		/// 
		/// Snipped from https://stackoverflow.com/questions/33336540/how-to-use-linq-to-find-all-combinations-of-n-items-from-a-set-of-numbers
		/// </summary>
		public static IEnumerable<IEnumerable<T>> DifferentCombinations<T>(this IEnumerable<T> elements, int k)
		{
			return k == 0
					? new[] { Enumerable.Empty<T>() }
					: elements.SelectMany((e, i) =>
							elements.Skip(i + 1)
									.DifferentCombinations(k - 1)
									.Select(c => new[] { e }.Concat(c)));
		}

		/// <summary>
		/// Concatenates a sequence and a single element
		/// </summary>
		/// <typeparam name="T">The type of the elements of the input sequence</typeparam>
		/// <param name="sequence">The sequence to concatenate</param>
		/// <param name="element">The element to concatenate to the sequence</param>
		public static IEnumerable<T> Concat<T>(this IEnumerable<T> sequence, T element)
		{
			return sequence.Concat(new[] { element });
		}

		/// <summary>
		/// Enumerates the integers from 0 to <paramref name="enumerable"/>.Count() - 1.
		/// </summary>
		public static IEnumerable<int> IndexRange<T>(this IEnumerable<T> enumerable)
		{
			return Enumerable.Range(0, enumerable.Count());
		}

		/// <summary>
		/// Returns true in case enumerable is null or has no entries, false otherwise
		/// </summary>
		public static bool NullOrEmpty<T>(this IEnumerable<T> e)
		{
			return (e == null || !e.Any());
		}

		/// <summary>
		/// Returns a hash code for the sequence, that depends on the hash codes of the
		/// elements and their order
		/// </summary>
		public static int GetSequenceHashCode<T>(this IEnumerable<T> sequence)
		{
			int hash = 0;
			foreach (T t in sequence)
			{
				hash *= 173;
				hash += t.GetHashCode();
			}
			return hash;
		}

	}
}
