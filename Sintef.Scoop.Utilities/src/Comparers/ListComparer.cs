//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System.Collections.Generic;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// A comparer that compares two lists. The lists are equal if
	/// they have the same length and elements at corresponding positions
	/// compare as Equals().
	/// </summary>
	/// <typeparam name="T">The type of list elements</typeparam>
	public class ListComparer<T> : EqualityComparer<List<T>> 
	{
		/// <inheritdoc/>
		public override bool Equals(List<T> x, List<T> y)
		{
			if (x.Count != y.Count)
				return false;

			for (int i = 0; i < x.Count; ++i)
				if (!x[i].Equals(y[i]))
					return false;

			return true;
		}

		/// <inheritdoc/>
		public override int GetHashCode(List<T> obj)
		{
			int hash = 0;
			foreach (T c in obj)
				hash = hash * 7 + c.GetHashCode();
			return hash;
		}

		/// <summary>
		/// In this default implementation, 'x' dominates 'y' if
		/// the elements of 'x' is a struct subset of the elements of 'y'
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public virtual bool Dominates(List<T> x, List<T> y)
		{
			return x.IsSubsetOf(y);
		}
	}

}
