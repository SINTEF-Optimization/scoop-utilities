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
	/// Utility class for random ordering of collections.
	/// </summary>
	public static class RandomShuffle
	{
		/// <summary>
		/// Returns a copy of the enumerable, shuffled pseudo-randomly
		/// </summary>
		/// <param name="source"></param>
		/// <param name="r">The random generator to use. If null, uses a random generator created
		/// by RandomCreator.CreateRandomGenerator()</param>
		public static List<T> Shuffled<T>(this IEnumerable<T> source, Random r = null)
		{
			var list = source.ToList();

			Shuffle(list, r);

			return list;
		}


		/// <summary>
		/// Shuffles the list pseudo-randomly
		/// </summary>
		/// <param name="list"></param>
		/// <param name="r">The random generator to use. If null, uses a random generator created
		/// by RandomCreator.CreateRandomGenerator()</param>
		public static void ShuffleMe<T>(this List<T> list, Random r = null)
		{
			Shuffle(list, r);
		}

		/// <summary>
		/// Shuffles the list pseudo-randomly
		/// </summary>
		/// <param name="list"></param>
		/// <param name="r">The random generator to use. If null, uses a random generator created
		/// by RandomCreator.CreateRandomGenerator()</param>
		public static void Shuffle<T>(List<T> list, Random r = null)
		{
			if (r == null)
				r = RandomCreator.GetRandomGenerator();

			for (int i = 0; i < list.Count; ++i)
			{
				T x = list[i];

				int index = r.Next(list.Count - i) + i;
				System.Diagnostics.Debug.Assert(index < list.Count);
				list[i] = list[index];
				list[index] = x;
			}
		}

	}
}
