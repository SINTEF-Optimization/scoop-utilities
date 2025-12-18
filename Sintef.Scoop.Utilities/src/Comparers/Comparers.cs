//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Compares two sequences lexically.
	/// 
	/// Compares corresponding elements of two sequences. The sequence whose first differing element is smaller,
	/// or that ends first, is judged smaller.
	/// </summary>
	/// <typeparam name="T">The type of the elements in the sequences</typeparam>
	public class LexicalComparer<T> : IComparer<IEnumerable<T>>
	{
		/// <summary>
		/// The comparer used to compare elements
		/// </summary>
		public IComparer<T> ElementComparer { get; private set; }

		/// <summary>
		/// If true, an empty sequence is put last in the sort order, instead of first
		/// </summary>
		public bool EmptySequenceIsLargest { get; set; }

		/// <summary>
		/// Creates a lexical comparer that uses the default element comparer <see cref="Comparer{T}"/>
		/// </summary>
		/// <param name="emptySequenceIsLargest">If true, an empty sequence is put last in the sort order, instead of first</param>
		public LexicalComparer(bool emptySequenceIsLargest = false)
		{
			ElementComparer = Comparer<T>.Default;
			EmptySequenceIsLargest = emptySequenceIsLargest;
		}

		/// <summary>
		/// Creates a lexical comparer that uses the given comparer to compare elements
		/// </summary>
		/// <param name="elementComparer">The element comparer</param>
		/// <param name="emptySequenceIsLargest">If true, an empty sequence is put last in the sort order, instead of first</param>
		public LexicalComparer(IComparer<T> elementComparer, bool emptySequenceIsLargest = false)
		{
			ElementComparer = elementComparer;
			EmptySequenceIsLargest = emptySequenceIsLargest;
		}

		/// <summary>
		/// Compares the given sequences. Returns a negative number if x is smaller, a positive number if
		/// y is smaller, and 0 if the sequences are equal.
		/// </summary>
		public int Compare(IEnumerable<T> x, IEnumerable<T> y)
		{
			var xit = x.GetEnumerator();
			var yit = y.GetEnumerator();

			int smallerResult = -1;
			if (EmptySequenceIsLargest)
				// If one sequence ends in the first iteration, we will return the opposite result of normal,
				// thus making the empty sequence largest instead of smallest
				smallerResult = 1;

			while (true)
			{
				// Check for a next element in each sequence
				bool hasX = xit.MoveNext();
				bool hasY = yit.MoveNext();

				if (!hasX)
				{
					if (!hasY)
						// Both sequences ended. They are equal
						return 0;
					else
						// x has ended and is smaller
						return smallerResult;
				}
				if (!hasY)
				{
					// y has ended and is smaller
					return -smallerResult;
				}

				// Compare the elements
				var cmp = ElementComparer.Compare(xit.Current, yit.Current);

				if (cmp != 0)
					// Elements differ
					return cmp;

				// Elements are equal -- go to next

				// From now on, the sequence to end first is smaller
				smallerResult = -1;
			}
		}
	}

	/// <summary>
	/// Interface for classes that implement both IComparer and IComparer&lt;T>
	/// </summary>
	/// <typeparam name="T">The type compared</typeparam>
	public interface IComparerAndGenericComparer<T> : IComparer, IComparer<T> { }

	/// <summary>
	/// Compares two objects using a comparer. If the result is zero (the objects are equal), 
	/// tries to break the tie by using a second comparer.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SequentialComparer<T> : IComparerAndGenericComparer<T>
	{
		private IComparer<T> _mainComparer;
		private IComparer<T> _tieBreakComparer;

		/// <summary>
		/// Creates a comparer
		/// </summary>
		/// <param name="mainComparer">The comparer to use</param>
		/// <param name="tieBreakComparer">The comparer to use if the main comparer does not differentiate</param>
		public SequentialComparer(IComparer<T> mainComparer, IComparer<T> tieBreakComparer)
		{
			_mainComparer = mainComparer;
			_tieBreakComparer = tieBreakComparer;
		}

		/// <summary>
		/// Implements IComparer&lt;TSource>.Compare()
		/// </summary>
		public int Compare(object x, object y)
		{
			return Compare((T)x, (T)y);
		}

		/// <summary>
		/// Implements IComparer.Compare()
		/// </summary>
		public int Compare(T x, T y)
		{
			int cmp = _mainComparer.Compare(x, y);

			if (cmp == 0)
				cmp = _tieBreakComparer.Compare(x, y);

			return cmp;
		}
	}

	/// <summary>
	/// Compares two objects by their key, which is extracted by a selector function.
	/// Typically, the selector extracts a property of the objects.
	/// This is analogous to LINQ OrderBy.
	/// </summary>
	/// <typeparam name="T">The type of objects to compare</typeparam>
	/// <typeparam name="TKey">The type of the key</typeparam>
	public class BySelectorComparer<T, TKey> : IComparerAndGenericComparer<T>
	{
		private Func<T, TKey> _selector;
		private IComparer<TKey> _comparer;
		private bool _reverse;

		/// <summary>
		/// Creates a comparer
		/// </summary>
		/// <param name="selector">The selector that extracts the objects' keys</param>
		/// <param name="reverse">If true, keys are compared in reversed order</param>
		public BySelectorComparer(Func<T, TKey> selector, bool reverse = false)
		{
			this._selector = selector;
			_comparer = Comparer<TKey>.Default;
			this._reverse = reverse;
		}

		/// <summary>
		/// Implements IComparer&lt;TSource>.Compare()
		/// </summary>
		public int Compare(T x, T y)
		{
			if (_reverse)
				return _comparer.Compare(_selector(y), _selector(x));
			else
				return _comparer.Compare(_selector(x), _selector(y));
		}

		/// <summary>
		/// Implements IComparer.Compare()
		/// </summary>
		public int Compare(object x, object y)
		{
			return Compare((T)x, (T)y);
		}
	}

	/// <summary>
	/// Methods for creating comparers.
	/// 
	/// Example: <code>Comparer&lt;MyType> cmp = Compare&lt;MyType>.By(x => x.Id).ThenByReverse(x => x.Age)</code>
	/// </summary>
	/// <typeparam name="T">The type of objects to compare</typeparam>
	public class Compare<T>
	{
		/// <summary>
		/// Creates a comparer that compares according to a key
		/// </summary>
		/// <typeparam name="TKey">The key type</typeparam>
		/// <param name="selector">Function that extracts the key of an object</param>
		/// <returns>The comparer</returns>
		public static IComparerAndGenericComparer<T> By<TKey>(Func<T, TKey> selector)
		{
			return new BySelectorComparer<T, TKey>(selector);
		}

		/// <summary>
		/// Creates a comparer that compares according to a key, in reverse order
		/// </summary>
		/// <typeparam name="TKey">The key type</typeparam>
		/// <param name="selector">Function that extracts the key of an object</param>
		/// <returns>The comparer</returns>
		public static IComparerAndGenericComparer<T> ByReverse<TKey>(Func<T, TKey> selector)
		{
			return new BySelectorComparer<T, TKey>(selector, true);
		}

		/// <summary>
		/// Creates a comparer that places objects that satisfy the predicate (the predicate returns true)
		/// before those that do not (the predicate returns false)
		/// </summary>
		/// <param name="predicate">Function that returns whether to place the object first</param>
		/// <returns>The comparer</returns>
		public static IComparerAndGenericComparer<T> Prefer(Func<T, bool> predicate)
		{
			return By(x => predicate(x) ? 0 : 1);
		}
	}

	/// <summary>
	/// Extension methods for creating comparers
	/// 
	/// Example: <code>Comparer&lt;MyType> cmp = myObj.CompareBy(x => x.Id).ThenByReverse(x => x.Age)</code>
	/// (myObj is a dummy to determine generic types.)
	/// </summary>
	public static partial class Extensions
	{
		/// <summary>
		/// Creates a comparer that compares according to a key
		/// </summary>
		/// <typeparam name="T">The type of objects to compare</typeparam>
		/// <typeparam name="TKey">The key type</typeparam>
		/// <param name="dummy">An object of the type to compare. Not used.</param>
		/// <param name="selector">Function that extracts the key of an object</param>
		/// <returns>The comparer</returns>
		public static IComparerAndGenericComparer<T> CompareBy<T, TKey>(this T dummy, Func<T, TKey> selector)
		{
			return new BySelectorComparer<T, TKey>(selector);
		}

		/// <summary>
		/// Creates a comparer that compares according to a key, in reverse order
		/// </summary>
		/// <typeparam name="T">The type of objects to compare</typeparam>
		/// <typeparam name="TKey">The key type</typeparam>
		/// <param name="dummy">An object of the type to compare. Not used.</param>
		/// <param name="selector">Function that extracts the key of an object</param>
		/// <returns>The comparer</returns>
		public static IComparerAndGenericComparer<T> CompareByReverse<T, TKey>(this T dummy, Func<T, TKey> selector)
		{
			return new BySelectorComparer<T, TKey>(selector, true);
		}

		/// <summary>
		/// Creates a comparer that places objects that satisfy a predicate before those that do not
		/// </summary>
		/// <typeparam name="T">The type of objects to compare</typeparam>
		/// <param name="dummy"></param>
		/// <param name="predicate">Function that returns whether to place the object first</param>
		/// <returns>The comparer</returns>
		public static IComparerAndGenericComparer<T> ComparePrefer<T>(this T dummy, Func<T, bool> predicate)
		{
			return Compare<T>.Prefer(predicate);
		}

		/// <summary>
		/// Creates a comparer that compares first according to the main comparer. If the result
		/// is a tie, compares according to a key
		/// </summary>
		/// <typeparam name="T">The type of objects to compare</typeparam>
		/// <typeparam name="TKey">The key type</typeparam>
		/// <param name="mainComparer">The comparer to use first</param>
		/// <param name="selector">Function that extracts the key of an object</param>
		/// <returns>The comparer</returns>
		public static IComparerAndGenericComparer<T> ThenBy<T, TKey>(this IComparer<T> mainComparer, Func<T, TKey> selector)
		{
			return new SequentialComparer<T>(mainComparer, Compare<T>.By(selector));
		}

		/// <summary>
		/// Creates a comparer that compares first according to the main comparer. If the result
		/// is a tie, compares according to a key, in reverse order
		/// </summary>
		/// <typeparam name="T">The type of objects to compare</typeparam>
		/// <typeparam name="TKey">The key type</typeparam>
		/// <param name="mainComparer">The comparer to use first</param>
		/// <param name="selector">Function that extracts the key of an object</param>
		/// <returns>The comparer</returns>
		public static IComparerAndGenericComparer<T> ThenByReverse<T, TKey>(this IComparer<T> mainComparer, Func<T, TKey> selector)
		{
			return new SequentialComparer<T>(mainComparer, Compare<T>.ByReverse(selector));
		}

		/// <summary>
		/// Creates a comparer that compares first according to the main comparer. If the result
		/// is a tie, places objects that satisfy a predicate before those that do not
		/// </summary>
		/// <typeparam name="T">The type of objects to compare</typeparam>
		/// <param name="mainComparer">The comparer to use first</param>
		/// <param name="predicate">Function that returns whether to place the object first</param>
		/// <returns>The comparer</returns>
		public static IComparerAndGenericComparer<T> ThenPrefer<T>(this IComparer<T> mainComparer, Func<T, bool> predicate)
		{
			return new SequentialComparer<T>(mainComparer, Compare<T>.Prefer(predicate));
		}
	}

	/// <summary>
	/// An implementation of EqualityComparer and IEqualityComparer^lt;T> that
	/// always compares by reference
	/// </summary>
	public sealed class ReferenceEqualityComparer<T>
			: IEqualityComparer, IEqualityComparer<T>
	{
		/// <summary>
		/// Returns the single comparer for the generic argument type
		/// </summary>
		public static readonly ReferenceEqualityComparer<T> Default
				= new ReferenceEqualityComparer<T>();

		private ReferenceEqualityComparer() { }

		bool IEqualityComparer.Equals(object x, object y)
		{
			return ReferenceEquals(x, y);
		}

		bool IEqualityComparer<T>.Equals(T x, T y)
		{
			return ReferenceEquals(x, y);
		}

		/// <inheritdoc/>
		public int GetHashCode(object obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}

		/// <inheritdoc/>
		public int GetHashCode(T obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}

	/// <summary>
	/// A generic comparer that compares tow <typeparamref name="TItem"/>'s by their respective keys (of type <typeparamref name="TKey"/>).
	/// The class takes a property extraction function that 
	/// extracts the key to be compared, and a comparison function for comparing keys. 
	/// If no comparison function is given, the default comparer for the key type is used.
	/// Useful for constructing e.g. <see cref="SortedDictionary{T,V}"/>'s.
	/// </summary>
	/// <typeparam name="TItem">The item type</typeparam>
	/// <typeparam name="TKey">the key type</typeparam>
	public class KeyComparer<TItem, TKey> : Comparer<TItem>
	{
		/// <summary>
		/// The key extraction function.
		/// </summary>
		private readonly Func<TItem, TKey> _extract;

		/// <summary>
		/// The key comparer
		/// </summary>
		private readonly IComparer<TKey> _comparer;

		/// <summary>
		/// Constructor taking a property extraction function.
		/// The default comparer for the key type will be used.
		/// </summary>
		/// <param name="extract">Key extration function.</param>
		public KeyComparer(Func<TItem, TKey> extract)
				: this(extract, Comparer<TKey>.Default)
		{ }

		/// <summary>
		/// Constructor taking a property extraction function and a key comparer.
		/// </summary>
		/// <param name="extract">Key extration function.</param>
		/// <param name="comparer">The key comparer.</param>
		public KeyComparer(Func<TItem, TKey> extract, IComparer<TKey> comparer)
		{
			_extract = extract;
			_comparer = comparer;
		}

		/// <summary>
		/// Compares two items by comparing their respective key's.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public override int Compare(TItem x, TItem y)
		{
			// need to handle nulls
			TKey xKey = _extract(x);
			TKey yKey = _extract(y);
			return _comparer.Compare(xKey, yKey);
		}
	}

	/// <summary>
	/// A generic equality comparer for easy creation of anonymous delegate comparers
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class GenericEqualityComparer<T> : IEqualityComparer<T>
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="equals">Equals delegate function</param>
		/// <param name="getHashCode">Get has code delegate function</param>
		public GenericEqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
		{
			this.equals = equals;
			this.getHashCode = getHashCode;
		}

		readonly Func<T, T, bool> equals;

		/// <inheritdoc/>
		public bool Equals(T x, T y)
		{
			return equals(x, y);
		}

		readonly Func<T, int> getHashCode;

		/// <inheritdoc/>
		public int GetHashCode(T obj)
		{
			return getHashCode(obj);
		}
	}

}
