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
	/// The class represents a list of <see cref="List{T}"/>, where no list
	/// is dominated by any other list. To determine this, a list comparer
	/// can be given in the constructor. If this is not given,
	/// the class assumes that a strict subset A of B dominates B.
	/// Duplicates of existing lists are ignored.
	/// </summary>
	public class NonDominatedSetOfLists<T>  
	{

		#region Properties and fields

		/// <summary>
		/// The lists
		/// </summary>
		public List<List<T>> Lists { get; private set; }

		/// <summary>
		/// The optional list comparer
		/// </summary>
		ListComparer<T> _listComparer;

		#endregion

		#region Construction

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="listComparer">Used to compare two lists for dominant. If(a.compareTo(b) &lt; 0), 'a' dominates by definition. If this argument is not given,
		/// or is null, the class assumes that a strict subset A of B dominates B.</param>
		public NonDominatedSetOfLists(ListComparer<T> listComparer = null)
		{
			Lists = new List<List<T>>();
			if (listComparer == null)
				_listComparer = new ListComparer<T>();
			else
				_listComparer = listComparer;
		}

		#endregion

		#region Public members

		/// <summary>
		/// Adds the given list, and removes any existing lists that are dominated 
		/// (i.e. that are supersets of the given list).
		/// If the new list is dominated by any in the existing set, it is not added.
		/// </summary>
		/// <param name="newList"></param>
		public void Add(List<T> newList)
		{
			if (!Lists.Contains(newList,_listComparer) && !HasMemberThatDominates(newList))
			{
				Lists.RemoveAll(l => Dominates(newList, l));
				Lists.Add(newList);
			}
		}

		/// <summary>
		/// Returns true iff the given list is a superset of any of
		/// the existing lists.
		/// </summary>
		/// <param name="candidate"></param>
		/// <returns></returns>
		public bool HasMemberThatDominates(List<T> candidate)
		{
			return Lists.Any(l => Dominates(l,candidate));
		}

		/// <summary>
		/// Returns the first found member that dominates the given list,
		/// or null if no such exists.
		/// </summary>
		/// <param name="candidate"></param>
		/// <returns></returns>
		public List<T> FirstMemberThatDominates(List<T> candidate)
		{
			return Lists.FirstOrDefault(l => Dominates(l, candidate));
		}

		/// <summary>
		/// Returns true iff the given list is a superset of any of
		/// the existing lists.
		/// </summary>
		/// <param name="candidate"></param>
		/// <returns></returns>
		public bool HasMemberThatDominates(SortedSet<T> candidate)
		{
			return Lists.Any(l => l.IsSubsetOf<T>(candidate));
		}

		#endregion

		#region Private members

		/// <summary>
		/// Returns true iff 'a' dominates 'b'
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		private bool Dominates(List<T> a, List<T> b)
		{
			return _listComparer.Dominates(a, b);
		}

		#endregion
	}
}
