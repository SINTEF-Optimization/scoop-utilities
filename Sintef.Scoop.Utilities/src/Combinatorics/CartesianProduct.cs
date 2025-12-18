//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities
{
	public static partial class Extensions
	{

		/// <summary>
		/// Returns the cartesian product for a set of sets
		/// </summary>
		/// <typeparam name="T">Type of the set selements.</typeparam>
		/// <param name="sequences">A sequence of sets</param>
		/// <returns>The cartesian product of the input sets.</returns>
		public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
		{
			T[][] array = sequences.Select(x => x.ToArray()).ToArray();
			int[] index = new int[array.Length];
			int lastIndex = index.Length - 1;

			while (true)
			{
				do
				{
					yield return array.Select((x, y) => array[y][index[y]]);
					var currIndex = lastIndex;
					index[currIndex]++;
					while (index[currIndex] == array[currIndex].Length)
					{
						index[currIndex] = 0;
						currIndex--;
						if (currIndex < 0)
						{
							yield break;
						}
						index[currIndex]++;
					}
				} while (true);
			}
		}
	}
}
