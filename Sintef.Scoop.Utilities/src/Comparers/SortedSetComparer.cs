using System.Collections.Generic;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// A comparer that compares two sorted sets. The lists are equal if
	/// they have the same length and elements at corresponding positions
	/// compare as Equals().
	/// </summary>
	/// <typeparam name="T">The type of set elements</typeparam>
	public class SortedSetComparer<T> : EqualityComparer<SortedSet<T>>
	{
		/// <inheritdoc/>
		public override bool Equals(SortedSet<T> x, SortedSet<T> y)
		{
			return x.SetEquals(y);
		}

		/// <inheritdoc/>
		public override int GetHashCode(SortedSet<T> obj)
		{
			int hash = 0;
			foreach (T c in obj)
				hash = hash * 7 + c.GetHashCode();
			return hash;
		}
	}

}
