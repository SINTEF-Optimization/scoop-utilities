//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Utility class for partial ordering.
	/// </summary>
	public static class PartialOrdering
	{
		/// <summary>
		/// Partially orders the given input sequence based on the given key selector method using default the comparer on the keys.
		/// 
		/// This is basically a sort based on key and thus obsolete, use OrderBy instead.
		/// </summary>
		/// <param name="sequence">The sequence to order.</param>
		/// <param name="keySelector">A method providing the associated keys for each element.</param>
		/// <typeparam name="T">The type of the elements in the sequence.</typeparam>
		/// <typeparam name="TKey">The type of the keys used for determining precedence.</typeparam>
		/// <returns>The partially ordered sequence.</returns>
		[Obsolete]
		public static IEnumerable<T> PartialOrderBy<T, TKey>(this IEnumerable<T> sequence, Func<T, TKey> keySelector)
		{
			return PartialOrderBy(sequence, keySelector, Comparer<TKey>.Default);
		}

		/// <summary>
		/// Performs a partial ordering on the given sequence. The ordering is based on the given comparer.
		/// </summary>
		/// <typeparam name="T">The type of the elements in the sequence.</typeparam>
		/// <param name="sequence">The sequence to order.</param>
		/// <param name="comparer">A comparer which defines precedence based on the values. Returns a negative value if the first parameter precedes the second,
		/// 0 if they are unrelated or a positive number if the second parameter precedes the first.</param>
		/// <returns>The partially ordered sequence.</returns>
		public static IEnumerable<T> PartialOrderBy<T>(this IEnumerable<T> sequence, IComparer<T> comparer)
		{
			return PartialOrderBy(sequence, x => x, comparer);
		}

		/// <summary>
		/// Performs a partial ordering on the given sequence. The ordering is based on the given comparer used on the keys provided by the given method.
		/// </summary>
		/// <param name="sequence">The sequence to order.</param>
		/// <param name="keySelector">A method providing the associated keys for each element.</param>
		/// <param name="comparer">A comparer which defines precedence based on the associated keys. Returns a negative value if the first parameter precedes
		/// the second, 0 if they are unrelated or a positive number if the second parameter precedes the first.</param>
		/// <typeparam name="T">The type of the elements in the sequence.</typeparam>
		/// <typeparam name="TKey">The type of the keys used for determining precedence.</typeparam>
		/// <returns>The partially ordered sequence.</returns>
		public static IEnumerable<T> PartialOrderBy<T, TKey>(this IEnumerable<T> sequence, Func<T, TKey> keySelector, IComparer<TKey> comparer)
		{
			var actualComparer = comparer ?? Comparer<TKey>.Default;

			var elements = sequence.ToArray();
			var remaining = elements.Length;
			if (remaining == 0)
			{
				yield break;
			}
			var keys = elements.Select(keySelector).ToArray();

			do
			{
				int minIndex = 0;
				for (int i = 1; i < remaining; i++)
				{
					if (actualComparer.Compare(keys[i], keys[minIndex]) < 0)
					{
						minIndex = i;
					}
				}
				yield return elements[minIndex];
				elements[minIndex] = elements[remaining - 1];
				keys[minIndex] = keys[remaining - 1];
				remaining--;
			} while (remaining > 0);
		}
	}
}
