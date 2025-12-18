//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Extension methods related to binary search.
	/// </summary>
	public static partial class Extensions
	{
		/// <summary>
		/// Uses binary search to find the smallest integer in the range
		/// (Item1 and Item2 exclusive!) that satisfies
		/// the given predicate. If no integer satisfies the predicate, returns range.Item2.
		/// The algorithm only works correctly if all integers that do not satisfy the predicate
		/// are smaller than those that do satisfy it.
		/// </summary>
		public static int BinaryFirst(this Tuple<int, int> range, Func<int, bool> predicate)
		{
			return BinaryFirst(range.Item1, range.Item2, predicate);
		}

		/// <summary>
		/// Uses binary search to find the smallest integer in the range
		/// (Item1 and Item2 exclusive!) that satisfies
		/// the given predicate. If no integer satisfies the predicate, returns range.Item2.
		/// The algorithm only works correctly if all integers that do not satisfy the predicate
		/// are smaller than those that do satisfy it.
		/// </summary>
		public static int BinaryFirst(this (int, int) range, Func<int, bool> predicate)
		{
			return BinaryFirst(range.Item1, range.Item2, predicate);
		}

		/// <summary>
		/// Uses binary search to find the smallest integer in the range
		/// (Item1 and Item2 exclusive!) that satisfies
		/// the given predicate. If no integer satisfies the predicate, returns range.Item2.
		/// The algorithm only works correctly if all integers that do not satisfy the predicate
		/// are smaller than those that do satisfy it.
		/// </summary>
		public static long BinaryFirst(this (long, long) range, Func<long, bool> predicate)
		{
			return BinaryFirst(range.Item1, range.Item2, predicate);
		}

		/// <summary>
		/// Uses binary search to find the first index in the list whose element satisfies
		/// the given predicate. If no element satisfies the predicate, returns list.Count.
		/// The algorithm only works correctly if all elements that do not satisfy the predicate
		/// precede those that do satisfy it in the list.
		/// </summary>
		/// <param name="list">The list to search in</param>
		/// <param name="predicate">The predicate</param>
		/// <param name="firstIndex">Optional. If given, we only search the list from this index and onwards.</param>
		public static int BinaryFirstIndex<T>(this IList<T> list, Func<T, bool> predicate, int firstIndex = 0)
		{
			int min = firstIndex - 1;
			int max = list.Count;

			while (max - min > 1)
			{
				int mid = (max + min) / 2;
				if (!predicate.Invoke(list[mid]))
					min = mid;
				else
					max = mid;
			}
			return max;
		}

		/// <summary>
		/// Uses binary search to find the first index in the list whose element satisfies
		/// the given predicate. If no element satisfies the predicate, returns list.Count.
		/// The algorithm only works correctly if all elements that do not satisfy the predicate
		/// precede those that do satisfy it in the list.
		/// </summary>
		/// <param name="list">The list to search in</param>
		/// <param name="predicate">The predicate</param>
		/// <param name="firstIndex">Optional. If given, we only search the list from this index and onwards.</param>
		public static int BinaryFirstIndex<T>(this IReadOnlyList<T> list, Func<T, bool> predicate, int firstIndex = 0)
		{
			int min = firstIndex - 1;
			int max = list.Count;

			while (max - min > 1)
			{
				int mid = (max + min) / 2;
				if (!predicate.Invoke(list[mid]))
					min = mid;
				else
					max = mid;
			}
			return max;
		}

		/// <summary>
		/// Uses binary search to find the first index in the list whose element satisfies
		/// the given predicate. If no element satisfies the predicate, returns list.Count.
		/// The algorithm only works correctly if all elements that do not satisfy the predicate
		/// precede those that do satisfy it in the list.
		/// </summary>
		/// <param name="list">The list to search in</param>
		/// <param name="predicate">The predicate</param>
		/// <param name="firstIndex">Optional. If given, we only search the list from this index and onwards.</param>
		public static int BinaryFirstIndex<T>(this List<T> list, Func<T, bool> predicate, int firstIndex = 0)
		 => ((IReadOnlyList<T>)list).BinaryFirstIndex(predicate, firstIndex);

		/// <summary>
		/// Uses binary search to find the first index in the array whose element satisfies
		/// the given predicate. If no element satisfies the predicate, returns array.Length.
		/// The algorithm only works correctly if all elements that do not satisfy the predicate
		/// precede those that do satisfy it in the array.
		/// </summary>
		/// <param name="array">The array to search in</param>
		/// <param name="predicate">The predicate</param>
		/// <param name="firstIndex">Optional. If given, we only search the array from this index and onwards.</param>
		public static int BinaryFirstIndex<T>(this T[] array, Func<T, bool> predicate, int firstIndex = 0)
		{
			int min = firstIndex - 1;
			int max = array.Length;

			while (max - min > 1)
			{
				int mid = (max + min) / 2;
				if (!predicate.Invoke(array[mid]))
					min = mid;
				else
					max = mid;
			}
			return max;
		}

		/// <summary>
		/// Uses binary search to find the largest integer in the range
		/// (Item1 and Item2 exclusive!) that satisfies
		/// the given predicate. If no integer satisfies the predicate, returns range.Item1.
		/// The algorithm only works correctly if all integers that do not satisfy the predicate
		/// are smaller than those that do satisfy it.
		/// </summary>
		public static int BinaryLast(this (int, int) range, Func<int, bool> predicate)
		{
			return BinaryLast(range.Item1, range.Item2, predicate);
		}

		/// <summary>
		/// Uses binary search to find the largest integer in the range
		/// (Item1 and Item2 exclusive!) that satisfies
		/// the given predicate. If no integer satisfies the predicate, returns range.Item1.
		/// The algorithm only works correctly if all integers that do not satisfy the predicate
		/// are smaller than those that do satisfy it.
		/// </summary>
		public static long BinaryLast(this (long, long) range, Func<long, bool> predicate)
		{
			return BinaryLast(range.Item1, range.Item2, predicate);
		}

		/// <summary>
		/// Uses binary search to find the last index in the list whose element satisfies
		/// the given predicate. If no element satisfies the predicate, returns <paramref name="firstIndex"/> - 1.
		/// The algorithm only works correctly if all elements that satisfy the predicate
		/// precede those that do not satisfy it in the list.
		/// </summary>
		/// <param name="list">The list to search in</param>
		/// <param name="predicate">The predicate</param>
		/// <param name="firstIndex">Optional. If given, we only search the list from this index and onwards.</param>
		public static int BinaryLastIndex<T>(this IList<T> list, Func<T, bool> predicate, int firstIndex = 0)
		{
			int min = firstIndex - 1;
			int max = list.Count;

			while (max - min > 1)
			{
				int mid = (max + min) / 2;
				if (predicate.Invoke(list[mid]))
					min = mid;
				else
					max = mid;
			}
			return min;
		}

		/// <summary>
		/// Uses binary search to find the last index in the list whose element satisfies
		/// the given predicate. If no element satisfies the predicate, returns <paramref name="firstIndex"/> - 1.
		/// The algorithm only works correctly if all elements that satisfy the predicate
		/// precede those that do not satisfy it in the list.
		/// </summary>
		/// <param name="list">The list to search in</param>
		/// <param name="predicate">The predicate</param>
		/// <param name="firstIndex">Optional. If given, we only search the list from this index and onwards.</param>
		public static int BinaryLastIndex<T>(this IReadOnlyList<T> list, Func<T, bool> predicate, int firstIndex = 0)
		{
			int min = firstIndex - 1;
			int max = list.Count;

			while (max - min > 1)
			{
				int mid = (max + min) / 2;
				if (predicate.Invoke(list[mid]))
					min = mid;
				else
					max = mid;
			}
			return min;
		}

		/// <summary>
		/// Uses binary search to find the last index in the list whose element satisfies
		/// the given predicate. If no element satisfies the predicate, returns <paramref name="firstIndex"/> - 1.
		/// The algorithm only works correctly if all elements that satisfy the predicate
		/// precede those that do not satisfy it in the list.
		/// </summary>
		/// <param name="list">The list to search in</param>
		/// <param name="predicate">The predicate</param>
		/// <param name="firstIndex">Optional. If given, we only search the list from this index and onwards.</param>
		public static int BinaryLastIndex<T>(this List<T> list, Func<T, bool> predicate, int firstIndex = 0)
		 => ((IReadOnlyList<T>)list).BinaryLastIndex(predicate, firstIndex);

		/// <summary>
		/// Uses binary search to find the last index in the array whose element satisfies
		/// the given predicate. If no element satisfies the predicate, returns <paramref name="firstIndex"/> - 1.
		/// The algorithm only works correctly if all elements that satisfy the predicate
		/// precede those that do not satisfy it in the array.
		/// </summary>
		/// <param name="array">The array to search in</param>
		/// <param name="predicate">The predicate</param>
		/// <param name="firstIndex">Optional. If given, we only search the array from this index and onwards.</param>
		public static int BinaryLastIndex<T>(this T[] array, Func<T, bool> predicate, int firstIndex = 0)
		{
			int min = firstIndex - 1;
			int max = array.Length;

			while (max - min > 1)
			{
				int mid = (max + min) / 2;
				if (predicate.Invoke(array[mid]))
					min = mid;
				else
					max = mid;
			}
			return min;
		}

		#region These methods will be moved into a separate class for binary search, but for now we avoid breaking changes

		/// <summary>
		/// Uses binary search to find the smallest integer x where 
		/// <paramref name="min"/> &lt; x &lt; <paramref name="max"/> that satisfies
		/// the given predicate. If no integer satisfies the predicate, returns <paramref name="max"/>.
		/// The algorithm only works correctly if all integers that do not satisfy the predicate
		/// are smaller than those that do satisfy it.
		/// </summary>
		public static int BinaryFirst(int min, int max, Func<int, bool> predicate)
		{
			if (max <= min)
				throw new ArgumentException("Invalid range");

			while (max - 1 > min)
			{
				int mid;
				if (min >= 0 || max <= 0)
					mid = min + (max - min) / 2;
				else
					mid = (min + max) / 2;

				if (!predicate.Invoke(mid))
					min = mid;
				else
					max = mid;
			}
			return max;
		}

		/// <summary>
		/// Uses binary search to find the smallest integer x where 
		/// <paramref name="min"/> &lt; x &lt; <paramref name="max"/> that satisfies
		/// the given predicate. If no integer satisfies the predicate, returns <paramref name="max"/>.
		/// The algorithm only works correctly if all integers that do not satisfy the predicate
		/// are smaller than those that do satisfy it.
		/// </summary>
		public static long BinaryFirst(long min, long max, Func<long, bool> predicate)
		{
			if (max <= min)
				throw new ArgumentException("Invalid range");

			while (max - 1 > min)
			{
				long mid;
				if (min >= 0 || max <= 0)
					mid = min + (max - min) / 2;
				else
					mid = (min + max) / 2;

				if (!predicate.Invoke(mid))
					min = mid;
				else
					max = mid;
			}
			return max;
		}

		/// <summary>
		/// Uses binary search to find the largest integer in the range
		/// (Item1 and Item2 exclusive!) that satisfies
		/// the given predicate. If no integer satisfies the predicate, returns range.Item1.
		/// The algorithm only works correctly if all integers that do not satisfy the predicate
		/// are smaller than those that do satisfy it.
		/// </summary>
		public static int BinaryLast(this Tuple<int, int> range, Func<int, bool> predicate)
		{
			return BinaryLast(range.Item1, range.Item2, predicate);
		}

		/// <summary>
		/// Uses binary search to find the largest integer x where 
		/// <paramref name="min"/> &lt; x &lt; <paramref name="max"/> that satisfies
		/// the given predicate. If no integer satisfies the predicate, returns <paramref name="min"/>.
		/// The algorithm only works correctly if all integers that do not satisfy the predicate
		/// are smaller than those that do satisfy it.
		/// </summary>
		public static int BinaryLast(int min, int max, Func<int, bool> predicate)
		{
			if (max <= min)
				throw new ArgumentException("Invalid range");

			while (max - 1 > min)
			{
				int mid;
				if (min >= 0 || max <= 0)
					mid = min + (max - min) / 2;
				else
					mid = (min + max) / 2;
				if (predicate.Invoke(mid))
					min = mid;
				else
					max = mid;
			}
			return min;
		}

		/// <summary>
		/// Uses binary search to find the largest integer x where 
		/// <paramref name="min"/> &lt; x &lt; <paramref name="max"/> that satisfies
		/// the given predicate. If no integer satisfies the predicate, returns <paramref name="min"/>.
		/// The algorithm only works correctly if all integers that do not satisfy the predicate
		/// are smaller than those that do satisfy it.
		/// </summary>
		public static long BinaryLast(long min, long max, Func<long, bool> predicate)
		{
			if (max <= min)
				throw new ArgumentException("Invalid range");

			while (max - 1 > min)
			{
				long mid;
				if (min >= 0 || max <= 0)
					mid = min + (max - min) / 2;
				else
					mid = (min + max) / 2;
				if (predicate.Invoke(mid))
					min = mid;
				else
					max = mid;
			}
			return min;
		}


		#endregion
	}
}
