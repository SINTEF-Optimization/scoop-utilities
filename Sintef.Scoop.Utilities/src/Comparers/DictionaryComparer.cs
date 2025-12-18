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
	/// A comparer that compares two dictionaries. These  are equal if
	/// they have the same length, the same key collection, and the same value
	/// for each Key, as determined by Equals().
	/// </summary>
	/// <typeparam name="K">The type of key elements</typeparam>
	/// <typeparam name="V">The type of value elements</typeparam>
	public class DictionaryComparer<K,V> : EqualityComparer<Dictionary<K,V>> where V : IComparable
	{
		/// <inheritdoc/>
		public override bool Equals(Dictionary<K, V> x, Dictionary<K, V> y)
		{
			if (x.Count != y.Count)
				return false;

			if (!x.Keys.SetEquals(y.Keys))
				return false;

			bool res = x.All(kvp => kvp.Value.Equals(y[kvp.Key]));
			return res;
		}

		/// <inheritdoc/>
		public override int GetHashCode(Dictionary<K, V> obj)
		{
			int hash = 0;
			foreach (KeyValuePair<K,V> c in obj)
				hash = hash * 7 + c.GetHashCode();
			return hash;
		}

		/// <summary>
		/// Virual function that determines some definition of domination between two dictionaries.
		/// In the default implementation, 'x' dominates 'y' if it's key set contains a sub set of the 
		/// key set of 'y', and for each key 'k' in 'x', x[k].Compare(y[k]) &lt;= 0,  and there is no
		/// key kk such that x[kk].Compare(y[kk]) > 0.
		/// OR,
		/// 'x' contains the same keys as 'y' but x[k].Compare(y[k]) &lt; 0 for at least one key k, and there is no
		/// key kk such that x[kk].Compare(y[kk]) > 0.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public virtual bool Dominates(Dictionary<K, V> x, Dictionary<K, V> y)
		{
			if (x.Keys.IsSubsetOf(y.Keys))
			{
				if (x.Keys.SetEquals(y.Keys))
				{
					bool xImprovesY = x.Any(kvp => kvp.Value.CompareTo(y[kvp.Key]) < 0);
					if (xImprovesY)
					{
						bool yImprovesX = x.Any(kvp => kvp.Value.CompareTo(y[kvp.Key]) > 0);
						return !yImprovesX;
					}
					else
						return false;
				}
				else //Sub set, it is enough to have the same value. I.e. that 'y' does not dominate 'x' for any of x's keys.
				{
						bool yImprovesX = x.Any(kvp => kvp.Value.CompareTo(y[kvp.Key]) > 0);
						return !yImprovesX;
				}
			}
			else
				return false;
		}
	}
}
