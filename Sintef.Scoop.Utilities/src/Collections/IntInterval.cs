//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Integer interval class. 
	/// </summary>
	public class IntInterval : IEquatable<IntInterval>
	{
		#region Static members

		/// <summary>
		/// The definition of infinity for IntInterval's. Use this to signify that no bound exists
		/// </summary>
		public static int Infinity { get { return int.MaxValue; } }

		#endregion

		#region Private members
		/// <summary>
		/// The values
		/// </summary>
		int2 _values;

		#endregion


		#region Construction

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="lower">Interval lower limit, inclusive</param>
		/// <param name="upper">Interval upper limit, inclusive. Use IntInterval.Infinity if no such boundary exists.</param>
		public IntInterval(int lower, int upper)
		{
			_values = new int2(lower, upper);

			if (lower > upper)
				throw new Exception("Interval constructor: lower > upper");

		}

		/// <summary>
		/// Constructor
		/// </summary>
		public IntInterval(int2 pair)
		{
			_values = pair;

			if (pair.First > pair.Second)
				throw new Exception("Interval constructor: lower > upper");

		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		public IntInterval(IntInterval other)
		{
			_values = other._values;

			if (other.Lower > other.Upper)
				throw new Exception("Interval copy constructor: lower > upper");

		}

		/// <summary>
		/// Clone function
		/// </summary>
		/// <returns></returns>
		public IntInterval Clone()
		{
			return new IntInterval(this);
		}

		#endregion

		/// <summary>
		/// Adds the other interval to this one, possibly extending
		/// the limits.
		/// </summary>
		/// <param name="other"></param>
		public void Add(IntInterval other)
		{
			if (other.Lower < Lower)
				Lower = other.Lower;
			if (other.Upper > Upper)
				Upper = other.Upper;
		}

		///// <summary>
		///// Removes the values between the given (inclusive) boundaries from this interval, by narrowing the limits,
		///// or by splitting the interval in two. If splitting, this interval will keep pointing to
		///// one of the parts, and will return a reference to the other.
		///// Note that this function may leave the interval empty.
		///// </summary>
		///// <param name="other"></param>
		//public IntInterval Remove(int l, int u)
		//{
		//    return Remove(new IntInterval(l, u));
		//}

		/// <summary>
		/// Removes the values in the other interval from this one, by narrowing the limits,
		/// or by splitting the interval in two. If splitting, this interval will keep pointing to
		/// one of the parts (the "lower one"), and will return a reference to the other.
		/// Note that this function may leave the interval empty.
		/// </summary>
		/// <param name="other"></param>
		public IntInterval Remove(IntInterval other)
		{
			return Remove(other.Lower, other.Upper);
			//if (Overlaps(other))
			//{
			//	if (other.Upper < Upper && other.Lower > Lower)
			//	{
			//		//We must split this interval
			//		IntInterval temp = new IntInterval(other.Upper + 1, Upper);
			//		Upper = other.Lower - 1;
			//		return temp;
			//	}
			//	else
			//	{
			//		if (other.Upper >= Upper)
			//		{
			//			Upper = other.Lower - 1;

			//		}
			//		else
			//			Lower = other.Upper + 1;
			//		return null;
			//	}
			//}
			//else return null;
		}

		/// <summary>
		/// Removes the values in the given interval from this one, by narrowing the limits,
		/// or by splitting the interval in two. If splitting, this interval will keep pointing to
		/// the lower one of the parts, and will return a reference to the upper one.
		/// Note that this function may leave the interval empty.
		/// </summary>
		public IntInterval Remove(int l, int u)
		{
			if (Overlaps(l, u))
			{
				if (u < Upper && l > Lower)
				{
					//We must split this interval
					IntInterval temp = new IntInterval(u + 1, Upper);
					Upper = l - 1;
					return temp;
				}
				else
				{
					if (u >= Upper)
						Upper = (l == int.MinValue) ? l : l - 1;
					else
						Lower = (u == int.MaxValue) ? u : u + 1;

					//The above can give a problem if we wrapp arount +/- int.MaxValue
					//So, in that case, clear 

					return null;
				}
			}
			else return null;
		}

		/// <summary>
		/// Returns true if if this interval overlaps a, false otherwise
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public bool Overlaps(IntInterval a)
		{
			return Lower <= a.Upper && Upper >= a.Lower;
		}

		/// <summary>
		/// Returns true if if this interval overlaps the given interval, false otherwise
		/// </summary>
		/// <returns></returns>
		public bool Overlaps(int l, int u)
		{
			return Lower <= u && Upper >= l;
		}

		/// <summary>
		/// Returns true if if this interval overlaps any interval in the given interval list, false otherwise
		/// </summary>
		/// <returns></returns>
		public bool Overlaps(IntIntervalList intList)
		{
			foreach (IntInterval intit in intList)
			{
				if (Overlaps(intit))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Returns a new interval representing the overlap between this and a.
		/// </summary>
		/// <param name="a"></param>
		/// <returns>Returns null if there is no overlap</returns>
		public IntInterval Overlap(IntInterval a)
		{
			int l = Math.Max(Lower, a.Lower);
			int u = Math.Min(Upper, a.Upper);
			if (l <= u)
				return new IntInterval(l, u);
			else
				return null;
		}

		/// <summary>
		/// Returns true if the interval is empty (i.e. <see cref="Upper"/> is less than <see cref="Lower"/>)
		/// </summary>
		public bool Empty() { return Upper < Lower; }

		/// <summary>
		/// Lower limit, inclusive
		/// </summary>
		public int Lower { get { return _values.First; } protected set { _values.First = value; } }

		/// <summary>
		/// Upper limit, inclusive
		/// </summary>
		public int Upper { get { return _values.Second; } protected set { _values.Second = value; } }

		///// <summary>
		///// The number of integer values in the interval.
		///// </summary>
		//public int Size { get { return Upper - Lower + 1; } }

		/// <summary>
		/// Length of interval, Upper - Lower + 1.
		/// </summary>
		public long Length { get { return ((long)Upper) - ((long)Lower) + 1; } }

		/// <summary>
		/// Writes the interval span.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return "[" + Lower.ToString() + " -> " + Upper.ToString() + "]";
		}

		/// <summary>
		/// Returns a description of the values in the interval 
		/// </summary>
		/// <returns></returns>
		public string ToDomainString()
		{
			if (Lower == Upper)
				return Lower.ToString();

			return $"{FormatForDomain(Lower)}-{FormatForDomain(Upper)}";
		}

		/// <summary>
		/// Returns true if the interval has an upper limit.
		/// </summary>
		public bool IsUpperBounded { get { return Upper != Infinity; } }

		/// <summary>
		/// Returns true if the interval has an lower limit.
		/// </summary>
		public bool IsLowerBounded { get { return Lower != -Infinity; } }

		/// <summary>
		/// Checks if the interval covers the given interval
		/// </summary>
		/// <param name="start">Start, inclusive</param>
		/// <param name="end">End, inclusive</param>
		/// <returns></returns>
		public bool Covers(int start, int end)
		{
			return (Lower <= start && Upper >= end);
		}

		/// <summary>
		/// Checks if the interval covers the given interval
		/// </summary>
		/// <returns></returns>
		public bool Covers(IntInterval other)
		{
			return Covers(other.Lower, other.Upper);
		}

		/// <summary>
		/// Checks if the period covers the given interval, both limits inclusive.
		/// </summary>
		/// <returns></returns>
		public bool Covers(int2 interval)
		{
			return Covers(interval.First, interval.Second);
		}

		/// <summary>
		/// True if the interval contains the given value
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public bool Contains(int val)
		{
			return Covers(val, val);
		}

		/// <summary>
		/// Returns true iff the interval limits match
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(IntInterval other)
		{
			return other.Lower == Lower && other.Upper == Upper;
		}


		/// <summary>
		/// Intersects this interval with the given other interval
		/// </summary>
		/// <param name="other"></param>
		/// <returns>false if this leaves the interval empty</returns>
		public bool IntersectWith(IntInterval other)
		{
			Lower = Math.Max(Lower, other.Lower);
			Upper = Math.Min(Upper, other.Upper);
			return Lower <= Upper;
		}

		/// <summary>
		/// Intersects this interval with the given other interval
		/// </summary>
		/// <param name="lower">Lower limit, inclusive</param>
		/// <param name="upper">Upper limit, inclusive</param>
		/// <returns>false if this leaves the interval empty</returns>
		public bool IntersectWith(int lower, int upper)
		{
			Lower = Math.Max(Lower, lower);
			Upper = Math.Min(Upper, upper);
			return Lower <= Upper;
		}

		/// <summary>
		/// Returns the interval's intersection with the given interval, or null
		/// if the intersection is empty. Does not change the interval itself.
		/// </summary>
		/// <param name="b"></param>
		/// <returns></returns>
		public IntInterval Intersection(IntInterval b)
		{
			IntInterval res = Clone();
			if (res.IntersectWith(b))
				return res;
			else
				return null;
		}

		/// <summary>
		///  Returns the interval's intersection with the given interval (given by limits), 
		/// or null if the intersection is empty. Does not change the interval itself.
		/// </summary>
		/// <param name="bLower"></param>
		/// <param name="bUpper"></param>
		/// <returns></returns>
		public IntInterval Intersection(int bLower, int bUpper)
		{
			IntInterval res = Clone();
			if (res.IntersectWith(bLower, bUpper))
				return res;
			else
				return null;
		}

		/// <summary>
		/// Returns the value as a string, with parentheses if it is negative
		/// </summary>
		private static string FormatForDomain(int value)
		{
			return (value < 0) ? $"({value})" : value.ToString();
		}
	}


	/// <summary>
	/// Generic list of disjunct intervals, sorted by increasing interval start times. 
	/// If an interval is added that is
	/// not completely disjunct from others in the list, they are joined.
	/// If an interval is removed from the list that is a sub-interval of an interval
	/// in the list, the remaining part(s) of that interval is kept in the list.
	/// </summary>
	public class IntIntervalList : IEnumerable<IntInterval>, IEquatable<IntIntervalList>
	{
		#region Internal classes
		/// <summary>
		/// Comparer for sorting the intervals by increasing interval lower value
		/// </summary>
		private class Comparer : IComparer<IntInterval>
		{
			public int Compare(IntInterval x, IntInterval y)
			{
				return (int)(x.Lower - y.Lower);
			}

		}
		#endregion

		#region Private members

		/// <summary>
		/// Intervals, sorted by lower limit.
		/// </summary>  
		SortedList<int, IntInterval> _intervals;
		Comparer _comparer;

		#endregion

		#region Construction

		/// <summary>
		/// Default constructor
		/// </summary>
		public IntIntervalList()
		{
			_intervals = new SortedList<int, IntInterval>();
			_comparer = new IntIntervalList.Comparer();
		}

		/// <summary>
		/// Copy (deep) constructor
		/// </summary>
		public IntIntervalList(IntIntervalList other)
		{
			_comparer = new IntIntervalList.Comparer();
			_intervals = new SortedList<int, IntInterval>();
			Copy(other);
		}

		/// <summary>
		/// Constructor that adds a first interval with the given inclusive boundaries.
		/// </summary>
		/// <param name="l"></param>
		/// <param name="u"></param>
		public IntIntervalList(int l, int u)
		{
			_intervals = new SortedList<int, IntInterval>();
			_comparer = new IntIntervalList.Comparer();
			AddAtEnd(l, u, false);
		}

		/// <summary>
		/// Constructor that adds a first interval.
		/// </summary>
		public IntIntervalList(IntInterval interval)
		{
			_intervals = new SortedList<int, IntInterval>();
			_comparer = new IntIntervalList.Comparer();
			AddAtEnd(interval, false);
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public IntIntervalList(List<IntInterval> intlist)
			: this()
		{
			foreach (IntInterval intit in intlist)
			{
				Add(intit);
			}
		}

		/// <summary>
		/// Constructs a list containing the given values
		/// </summary>
		/// <param name="values"></param>
		public IntIntervalList(IEnumerable<int> values)
			: this()
		{
			foreach (var value in values)
				Add(value, value);
		}

		/// <summary>
		/// Deep clone
		/// </summary>
		/// <returns></returns>
		public virtual IntIntervalList Clone()
		{
			return new IntIntervalList(this);
		}

		/// <summary>
		/// Deep copy
		/// </summary>
		/// <param name="o"></param>
		public void Copy(IntIntervalList o)
		{
			foreach (IntInterval it in o._intervals.Values)
			{
				AddAtEnd(it.Clone(), false);
			}
		}
		#endregion

		#region Private functions

		/// <summary>
		/// Appends the given interval to the list.
		/// Always use this instead of adding directly to _intervals.
		/// </summary>
		/// <param name="interval"></param>
		/// <param name="mergeWithAdjoiningPrevious">If true, and if a starts just when the previous interval finishes,
		/// the two intervals are joined instead of adding a as a separate interval.</param>
		private void AddAtEnd(IntInterval interval, bool mergeWithAdjoiningPrevious)
		{
			Debug.Assert(FirstOverLapping(interval) == null);
			if (mergeWithAdjoiningPrevious && _intervals.Count > 0)
			{
				IntInterval prev = _intervals.Values[_intervals.Count - 1];
				if (prev.Upper == interval.Lower - 1)
					_intervals[prev.Lower] = new IntInterval(prev.Lower, interval.Upper);
				else
					_intervals[interval.Lower] = interval;
			}
			else
				_intervals[interval.Lower] = interval;
		}

		/// <summary>
		/// Appends the given interval to the list.
		/// Always use this instead of adding directly to _intervals.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="mergeWithAdjoiningPrevious">If true, and if a starts just when the previous interval finishes,
		/// the two intervals are joined instead of adding a as a separate interval. The default value of this optional parameter is true.</param>
		public void AddAtEnd(int start, int end, bool mergeWithAdjoiningPrevious = true)
		{

			if (mergeWithAdjoiningPrevious && _intervals.Count > 0)
			{
				IntInterval prev = _intervals.Values[_intervals.Count - 1];
				if (prev.Upper == start - 1)
					_intervals[prev.Lower] = new IntInterval(prev.Lower, end);
				else
					_intervals[start] = new IntInterval(start, end);
			}
			else
			{
				IntInterval a = new IntInterval(start, end);
				Debug.Assert(FirstOverLapping(a) == null);
				_intervals[a.Lower] = a;
			}
		}

		/// <summary>
		/// Always use this instead of _intervals.RemoveRange
		/// </summary>
		/// <param name="l">Lower list index, inclusive</param>
		/// <param name="u">Upper list index, inclusive</param>
		private void RemoveRange(int l, int u)
		{
			List<int> temp = _intervals.Keys.Skip(l).Take(u - l + 1).ToList();
			foreach (int start in temp)
			{
				_intervals.Remove(start);
			}
		}

		/// <summary>
		/// Removes the interval from this list, if present.
		/// </summary>
		/// <param name="a"></param>
		private void RemoveFromList(IntInterval a)
		{
			_intervals.Remove(a.Lower);
		}

		#endregion

		#region Properties
		/// <summary>
		/// List indexer
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public IntInterval this[int i] { get { return _intervals.ElementAt(i).Value; } }

		/// <summary>
		/// The number of intervals in the list.
		/// </summary>
		public int Count { get { return _intervals.Count; } }

		/// <summary>
		/// The total number of integer values in the list
		/// </summary>
		public long NumberOfValues { get { return (_intervals.Count == 0) ? 0 : _intervals.Values.Sum(i => i.Length); } }

		/// <summary>
		/// The lowest value in the list (equals the lowest value in the first interval in the list).
		/// Equals -1 if no the list is empty
		/// </summary>
		public int Lower { get { return _intervals != null && _intervals.Count > 0 ? _intervals.ElementAt(0).Key : -1; } }

		/// <summary>
		/// The highest value in the list (equals the highest value in the last interval in the list).
		/// Equals -1 if no the list is empty
		/// </summary>
		public int Upper { get { return _intervals != null && _intervals.Count > 0 ? _intervals.ElementAt(_intervals.Count - 1).Value.Upper : -1; } }

		/// <summary>
		/// The smallest integer interval that contains all values in the list
		/// </summary>
		public IntInterval Envelope { get { return new IntInterval(Lower, Upper); } }

		/// <summary>
		/// Enumerates all values in all the intervals
		/// </summary>
		public IEnumerable<int> Values
		{
			get
			{
				foreach (var interval in _intervals.Values)
				{
					for (int value = interval.Lower; value <= interval.Upper; ++value)
						yield return value;
				}
			}
		}

		#endregion

		#region Public functions

		/// <summary>
		/// Empties the list
		/// </summary>
		public void Clear()
		{
			_intervals.Clear();
		}

		/// <summary>
		/// Adds an interval with the given boundaries to the list, possibly joining it with existing intervals.
		/// </summary>
		public void Add(int lower, int upper)
		{
			Add(new IntInterval(lower, upper));
		}

		/// <summary>
		/// Adds an interval to the list, possibly joining it with existing intervals.
		/// </summary>
		/// <param name="a"></param>
		public void Add(IntInterval a)
		{
			if (_intervals.Count == 0)
			{
				AddAtEnd(a, false);
				return;
			}

			IntInterval fo = FirstOverLapping(a);
			if (fo == null)
			{
				if (_intervals.Count > 0 && _intervals.Last().Key > a.Upper)
					Console.WriteLine("Interval.Add: TODO: Why add at end, even if there are no overlapping? Could it not be that we should insert it somewhere else?");
				AddAtEnd(a, true);
			}
			else
			{
				int fostart = fo.Lower;
				fo.Add(a);
				if (fostart != fo.Lower)
				{
					_intervals[fo.Lower] = fo;
					_intervals.Remove(fostart);
				}

				IntInterval bar = LastOverLapping(a);
				if (bar != fo)
				{
					fo.Add(bar);

					//Remove all those in between + bar
					int bari = _intervals.IndexOfKey(bar.Lower);
					int foi = _intervals.IndexOfKey(fo.Lower);
					RemoveRange(foi + 1, bari);
				}
			}

#if DEBUG
			CheckForInternalOverlaps();
#endif

		}

		/// <summary>
		/// Adds all the intervals in the given list
		/// </summary>
		/// <param name="l"></param>
		public void Add(IntIntervalList l)
		{
			for (int i = 0; i < l.Count; i++)
			{
				Add(l[i]);
			}
		}

		/// <summary>
		/// Removes the values from the given interval from intervals in the list, 
		/// and removes any existing intervals that thus becomes empty.
		/// </summary>
		/// <param name="a"></param>
		public void Remove(IntInterval a)
		{
			Remove(a.Lower, a.Upper);
		}

		/// <summary>
		/// Returns true if the given value is in one of the intervals
		/// </summary>
		public bool ContainsValue(int val)
		{
			IntInterval has = _intervals.Values.FirstOrDefault(i => i.Contains(val));
			return has != null;
		}
		/// <summary>
		/// Removes the values from the given interval from intervals in the list, 
		/// and removes any existing intervals that thus becomes empty or smaller than the given minimum length.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="minimumLength"></param>
		public void RemoveAndKillSmall(IntInterval a, int minimumLength)
		{
			IntInterval foo = FirstOverLapping(a);
			if (foo == null)
			{
				return;
			}
			else
			{
				IntInterval bar = LastOverLapping(a);
				if (bar != foo)
				{

					//Remove all those in between 
					int bari = _intervals.IndexOfKey(bar.Lower);
					int foi = _intervals.IndexOfKey(foo.Lower);
					RemoveRange(foi + 1, bari - 1);

					int oldBaL = bar.Lower;
					IntInterval splitbar = bar.Remove(a);
					if (splitbar != null)
					{
						if (splitbar.Length >= minimumLength)
							_intervals.Add(splitbar.Lower, splitbar); //Can this happen?
					}
					else
					{
						if (bar.Empty() || bar.Length < minimumLength)
							_intervals.Remove(oldBaL);
						else if (oldBaL != bar.Lower)
						{
							_intervals.Remove(oldBaL);
							_intervals[bar.Lower] = bar;
						}
					}
				}

				int oldFooL = foo.Lower;
				IntInterval splitoff = foo.Remove(a);
				if (splitoff != null)
				{
					if (splitoff.Length >= minimumLength)
						_intervals.Add(splitoff.Lower, splitoff);
					if (foo.Length < minimumLength)
						_intervals.Remove(oldFooL);
				}
				else
				{
					if (foo.Empty() || foo.Length < minimumLength)
						_intervals.Remove(oldFooL);
					else if (oldFooL != foo.Lower)
					{
						_intervals.Remove(oldFooL);
						_intervals[foo.Lower] = foo;
					}
				}
			}
		}

		/// <summary>
		/// Removes an the values from the given inclusive interval from intervals in the list, 
		/// and removes any existing intervals that thus becomes empty.
		/// </summary>
		public void Remove(int l, int u)
		{
			IntInterval foo = FirstOverLapping(l, u);
			if (foo == null)
			{
				return;
			}
			else
			{
				int fooStart = foo.Lower;
				IntInterval bar = LastOverLapping(l, u);
				if (bar != foo)
				{
					int barStart = bar.Lower;
					//Remove all those in between
					int bari = _intervals.IndexOfKey(bar.Lower);
					int foi = _intervals.IndexOfKey(foo.Lower);
					RemoveRange(foi + 1, bari - 1);

					IntInterval splitbar = bar.Remove(l, u);
					if (splitbar != null)
						_intervals.Add(splitbar.Lower, splitbar); //Can this happen?
					else
					{
						if (bar.Empty())
							_intervals.Remove(barStart);
						else
						{
							if (barStart != bar.Lower)
							{
								_intervals.Remove(barStart);
								_intervals.Add(bar.Lower, bar);
							}
						}
					}
				}

				int oldFooL = foo.Lower;
				IntInterval splitoff = foo.Remove(l, u);
				if (splitoff != null)
					_intervals.Add(splitoff.Lower, splitoff);
				else
				{
					if (foo.Empty())
						_intervals.Remove(oldFooL);
					else
					{
						if (fooStart != foo.Lower)
						{
							_intervals.Remove(fooStart);
							_intervals.Add(foo.Lower, foo);
						}
					}
				}
			}

#if DEBUG
			CheckForInternalOverlaps();
#endif
		}

		/// <summary>
		/// Removes all intervals that are shorter than the given value
		/// </summary>
		/// <param name="duration"></param>
		public void RemoveSmallerThan(int duration)
		{
			for (int i = 0; i < _intervals.Count;)
			{
				if (_intervals.ElementAt(i).Value.Length < duration)
					_intervals.RemoveAt(i);
				else
					++i;
			}
		}

		/// <summary>
		/// Assumes that current refers to an interval object in the list, and returns the next entry in the list following current.
		/// </summary>
		/// <param name="current"></param>
		/// <returns>If current is the last entry, the function returns null.</returns>
		public IntInterval Next(IntInterval current)
		{
			int index = _intervals.IndexOfKey(current.Lower);
			if (index < 0)
				throw new Exception("Next called with entry that is not in the list");
			if (++index == _intervals.Count)
				return null;
			else
				return _intervals.ElementAt(index).Value;
		}

		/// <summary>
		/// Utility function for internal quality control
		/// </summary>
		private void CheckForInternalOverlaps()
		{
			if (_intervals.Count > 1)
			{
				for (int i = 0; i < _intervals.Count - 1; i++)
				{
					IntInterval a = _intervals.ElementAt(i).Value;
					for (int j = i + 1; j < _intervals.Count; j++)
					{
						IntInterval b = _intervals.ElementAt(j).Value;
						if (a.Overlaps(b))
							throw new Exception("Two intervals in the same list overlaps each other");
					}
				}
			}
		}

		/// <summary>
		/// Returns a reference to the first existing interval that overlaps the input interval, or null if
		/// no overlap exists.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public IntInterval FirstOverLapping(IntInterval a)
		{
			return _intervals.FirstOrDefault(b => b.Key <= a.Upper && b.Value.Upper >= a.Lower).Value;
		}

		/// <summary>
		/// Returns a reference to the first existing interval that overlaps the input interval, or null if
		/// no overlap exists.
		/// </summary>
		/// <returns></returns>
		public IntInterval FirstOverLapping(int l, int u)
		{
			return _intervals.FirstOrDefault(b => b.Key <= u && b.Value.Upper >= l).Value;
		}

		/// <summary>
		/// Returns a reference to the first existing interval that overlaps the given value, or null if
		/// no overlap exists.
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public IntInterval FirstOverLapping(int t)
		{
			return _intervals.FirstOrDefault(b => b.Key <= t && b.Value.Upper >= t).Value;
		}


		/// <summary>
		/// Returns the value in the domain that is closest (in absolute difference) to the given value.
		/// </summary>
		/// <param name="refValue"></param>
		/// <returns>The value, or null if the list is empty.</returns>
		public int? GetClosestValue(int refValue)
		{
			IntInterval lastStartingBeforeOrAt = _intervals.LastOrDefault(i => i.Key <= refValue).Value;
			if (lastStartingBeforeOrAt != null)
			{
				if (lastStartingBeforeOrAt.Upper >= refValue)
					return refValue;
				else
				{
					int diffFromBelow = refValue - lastStartingBeforeOrAt.Upper;

					//The next interval may be closer
					IntInterval next = Next(lastStartingBeforeOrAt);
					int diffFromAbove = next.Lower - refValue;

					if (diffFromBelow < diffFromAbove)
						return lastStartingBeforeOrAt.Upper;
					else
						return next.Lower;
				}
			}
			else if (_intervals.Any())
				return _intervals.First().Value.Lower;
			else
				return null;
		}

		/// <summary>
		/// Returns a pointer to the last existing interval that overlaps the input interval, or null if
		/// no overlap exists. Assume that the search for this can start at startInterval.
		/// </summary>
		/// <param name="a">The interval that we would like to find the last overlap for</param>
		/// <param name="startInterval">The interval at which the search starts (we assume that
		/// startInterval overlaps a.</param>
		/// <returns></returns>
		private IntInterval LastOverLapping(IntInterval a, IntInterval startInterval)
		{
			//Find first that is after a.
			IntInterval temp = null;
			int ret = Extensions.BinaryFirstIndex(_intervals.Keys, b => b > a.Upper);
			if (ret < _intervals.Count)
				temp = _intervals.ElementAt(ret).Value;

			//IntInterval temp = _intervals.FirstOrDefault(b => b.Key > a.Upper).Value;

			int itemp = temp == null ? _intervals.Count : _intervals.IndexOfKey(temp.Lower);
			itemp--;
			if (itemp >= 0)
			{
				IntInterval b = _intervals.ElementAt(itemp).Value;
				if (b.Overlaps(a))
					return b;
				else
					return null;
			}
			else
				return null;
		}


		/// <summary>
		/// Returns a pointer to the last existing interval that overlaps the input interval, or null if
		/// no overlap exists.
		/// </summary>
		/// <param name="a">The interval that we would like to find the last overlap for</param>
		/// <returns></returns>
		private IntInterval LastOverLapping(IntInterval a)
		{
			//Find first that is after a.
			IntInterval temp = null;
			int ret = Extensions.BinaryFirstIndex(_intervals.Keys, b => b > a.Upper);
			if (ret < _intervals.Count)
				temp = _intervals.ElementAt(ret).Value;

			//IntInterval temp = _intervals.FirstOrDefault(b => b.Key > a.Upper).Value;

			int itemp = temp == null ? _intervals.Count : _intervals.IndexOfKey(temp.Lower);
			itemp--;
			if (itemp >= 0)
			{
				IntInterval b = _intervals.ElementAt(itemp).Value;
				if (b.Overlaps(a))
					return b;
				else
					return null;
			}
			else
				return null;
		}

		/// <summary>
		/// Returns a pointer to the last existing interval that overlaps the input interval, or null if
		/// no overlap exists.
		/// </summary>
		/// <returns></returns>
		private IntInterval LastOverLapping(int l, int u)
		{
			//Find first that is after a.
			IntInterval temp = null;
			int ret = Extensions.BinaryFirstIndex(_intervals.Keys, b => b > u);
			if (ret < _intervals.Count)
				temp = _intervals.ElementAt(ret).Value;

			//IntInterval temp = _intervals.FirstOrDefault(b => b.Key > a.Upper).Value;

			int itemp = temp == null ? _intervals.Count : _intervals.IndexOfKey(temp.Lower);
			itemp--;
			if (itemp >= 0)
			{
				IntInterval b = _intervals.ElementAt(itemp).Value;
				if (b.Overlaps(l, u))
					return b;
				else
					return null;
			}
			else
				return null;
		}

		/// <summary>
		/// Return the interval list as an array of int2's
		/// </summary>
		/// <returns></returns>
		public int2[] ToInt2Array()
		{
			List<int2> temp = new List<int2>();
			foreach (IntInterval iv in _intervals.Values)
				temp.Add(new int2(iv.Lower, iv.Upper));
			return temp.ToArray();
		}


		/// <summary>
		/// Enumerates the list of intervals. Valid only as long as nothing
		/// is added or removed during enumeration.
		/// </summary>
		/// <returns></returns>
		public IEnumerator<IntInterval> GetEnumerator()
		{
			foreach (IntInterval intit in _intervals.Values)
			{
				yield return intit;
			}
		}

		/// <summary>
		/// Returns a description of interval list.
		/// </summary>
		public override string ToString()
		{
			return _intervals.Values.Select(x => x.ToString()).Concatenate(", ");
		}

		/// <summary>
		/// Returns a description of the interval list in the form [5, 7, 12-14]
		/// </summary>
		public string ToDomainString()
		{
			return "[" + _intervals.Values.Select(x => x.ToDomainString()).Concatenate(", ") + "]";
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException("Interval.GetEnumerator");
		}

		/// <summary>
		/// Removes the i'th element of the list
		/// </summary>
		/// <param name="i"></param>
		public void RemoveAt(int i)
		{
			_intervals.RemoveAt(i);

		}
		#endregion

		/// <summary>
		/// Produces an interval list that equals the intersection of all intervals 
		/// with the given interval.
		/// </summary>
		/// <param name="interval"></param>
		/// <returns></returns>
		public virtual IntIntervalList Intersection(IntInterval interval)
		{
			IntIntervalList nl = new IntIntervalList();
			FillIntersectionList(nl, interval);


#if DEBUG
			nl.CheckForInternalOverlaps();
#endif

			return nl;
		}

		/// <summary>
		/// Produces an interval list that equals the intersection of all intervals 
		/// with all intervals in the given list.
		/// </summary>
		/// <param name="intList"></param>
		/// <returns></returns>
		public virtual IntIntervalList Intersection(IEnumerable<IntInterval> intList)
		{
			IntIntervalList nl = new IntIntervalList();
			FillIntersectionList(nl, intList);

#if DEBUG
			nl.CheckForInternalOverlaps();
#endif

			return nl;
		}

		/// <summary>
		/// Utility function, fills the given interval list with the intersections between "this" and
		/// the given enumerable of intervals.
		/// </summary>
		/// <param name="nl"></param>
		/// <param name="intList"></param>
		protected void FillIntersectionList(IntIntervalList nl, IEnumerable<IntInterval> intList)
		{
			//TODO: Make more efficient, linear complexity logic here. See InstersecWith...
			foreach (IntInterval intit in this)
			{
				foreach (IntInterval otherint in intList)
				{
					IntInterval overlap = intit.Overlap(otherint);
					if (overlap != null)
						nl.Add(overlap);
				}
			}
		}

		/// <summary>
		/// Utility function, fills the given interval list with the intersections between "this" and
		/// the given interval.
		/// </summary>
		/// <param name="nl"></param>
		/// <param name="otherint"></param>
		protected void FillIntersectionList(IntIntervalList nl, IntInterval otherint)
		{
			foreach (IntInterval intit in this)
			{
				IntInterval overlap = intit.Overlap(otherint);
				if (overlap != null)
					nl.Add(overlap);
			}
		}

		/// <summary>
		/// Reduces this interval list to an intersection with all intervals in the given list.
		/// </summary>
		/// <param name="intList"></param>
		/// <returns></returns>
		public void IntersectWith(IntIntervalList intList)
		{
			if (_intervals.Count == 0)
				return;

			if (intList.Count == 0)
			{
				Clear();
				return;
			}

			IntInterval intit = _intervals.First().Value;
			while (intit != null)
			{
				IntInterval firstOther = intList.FirstOverLapping(intit);
				if (firstOther == null)
				{
					int low = intit.Lower;
					intit = Next(intit);
					_intervals.Remove(low);
					continue;
				}

				while (firstOther != null)
				{
					IntInterval nextOther = intList.Next(firstOther);
					if (nextOther == null || !intit.Overlaps(nextOther))
					{
						int oldstart = intit.Lower;
						intit.IntersectWith(firstOther);
						if (intit.Lower != oldstart)
						{
							_intervals.Remove(oldstart);
							_intervals[intit.Lower] = intit;
						}
						break;
					}
					else
					{
						//Remove gap
						IntInterval newIntit = intit.Remove(firstOther.Upper + 1, nextOther.Lower - 1);
						int oldstart = intit.Lower;
						intit.IntersectWith(firstOther);
						if (intit.Lower != oldstart)
						{
							_intervals.Remove(oldstart);
							_intervals[intit.Lower] = intit;
						}
						intit = newIntit;
						Add(newIntit);
						firstOther = nextOther;
					}
				}
				intit = Next(intit);
			}
		}

		/// <summary>
		/// Reduces this interval list to an intersection with the given interval.
		/// </summary>
		/// <param name="interval"></param>
		/// <returns></returns>
		public void IntersectWith(IntInterval interval)
		{
			if (_intervals.Count == 0)
				return;

			if (interval == null && interval.Length == 0)
			{
				Clear();
				return;
			}

			IntInterval myInt = _intervals.FirstOrDefault().Value;
			//Remove the intervals in the list that comes before "interval"
			while (myInt != null && myInt.Upper < interval.Lower)
			{
				_intervals.Remove(myInt.Lower);
				myInt = _intervals.FirstOrDefault().Value;
			}

			//Intersect overlaps
			int currentIndex = -1;
			while (myInt != null && myInt.Lower <= interval.Upper)
			{
				++currentIndex;
				int newlower = Math.Max(myInt.Lower, interval.Lower);
				if (myInt.Upper <= interval.Upper)
				{
					if (myInt.Lower != newlower)
					{
						_intervals.Remove(myInt.Lower);
						_intervals.Add(newlower, new IntInterval(newlower, myInt.Upper));
					}
					myInt = _intervals.FirstOrDefault(kvp => kvp.Key > myInt.Upper).Value; //Next				
				}
				else //intit.Upper > interval.Upper
				{
					//Done
					if (myInt.Lower != newlower)
					{
						_intervals.Remove(myInt.Lower);
						_intervals.Add(newlower, new IntInterval(newlower, interval.Upper));
					}
					else
						myInt.IntersectWith(interval);
					break;
				}
			}

			//Remove those intervals in this list that come after the given interval.
			if (currentIndex + 1 <= _intervals.Count - 1)
				RemoveRange(currentIndex + 1, _intervals.Count - 1);


#if DEBUG
			CheckForInternalOverlaps();
#endif

		}


		/// <summary>
		/// Equals if contains the same (equal) elements
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(IntIntervalList other)
		{
			if (other == null)
				return false;
			if (Count != other.Count)
				return false;
			foreach (IntInterval intit in this)
			{
				if (!other._intervals.ContainsKey(intit.Lower))
					return false;
				if (!intit.Equals(other._intervals[intit.Lower]))
					return false;
			}
			return true;
		}

		#region Static public functions

		/// <summary>
		/// Finds all possible intersection amongst the given interval lists.
		/// </summary>
		/// <param name="lists"></param>
		/// <returns></returns>
		public static IntIntervalList FindIntersections(IntIntervalList[] lists)
		{
			#region Initialisation
			IntIntervalList result = new IntIntervalList();

			//Index of the current interval in each list
			IntInterval[] current = new IntInterval[lists.Length];

			//Length of each list
			int[] length = new int[lists.Length];

			for (int i = 0; i < lists.Length; i++)
			{
				//Initilize lengths and current intervals
				length[i] = lists[i].Count;
				if (length[i] == 0)
					return result;
				current[i] = lists[i].First();
			}
			#endregion

			//Find all overlaps			
			int start = int.MinValue;
			//int matchindex = -1;

			bool moreArePossible = true;
			do
			{
				bool foundIntersection = true;
				int ilow = int.MinValue;// current[matchindex].Lower;
				int iup = int.MaxValue;// current[matchindex].Upper;
				for (int i = 0; i < lists.Length; i++)
				{
					IntIntervalList interli = lists[i];
					IntInterval cur = current[i];
					do //look for overlapping intervals
					{
						if (cur.Lower <= start && cur.Upper >= start)
						{
							//We can use this
							current[i] = cur;
							ilow = Math.Max(ilow, cur.Lower);
							iup = Math.Min(iup, cur.Upper);
							break;
						}
						else if (cur.Lower > start)
						{
							//We've past start, which means that start cannot be the start of an overlap.
							//We update start and try again.
							current[i] = cur;
							start = cur.Lower;
							foundIntersection = false;
							break;
						}
						//Next interval in list
						cur = interli.Next(cur);
						if (cur == null)
						//No more intersections are possible
						{
							moreArePossible = false;
							foundIntersection = false;
						}
					} while (moreArePossible);

					if (!foundIntersection || !moreArePossible)
						break;
				}
				if (foundIntersection)
				{
					result.AddAtEnd(ilow, iup);
					start = iup + 1; //periods....
				}

			} while (moreArePossible);

			return result;
		}

		/// <summary>
		/// Finds the first possible intersection amongst the given interval lists that has span at least as
		/// large as the given duration.
		/// </summary>
		/// <param name="lists"></param>
		/// <param name="duration"></param>
		/// <returns>Returns the found intersection, or null if no such was found.</returns>
		public static IntInterval FindFirstIntersectionLargerThan(IntIntervalList[] lists, int duration)
		{
			#region Initialisation

			//Index of the current interval in each list
			IntInterval[] current = new IntInterval[lists.Length];
			for (int i = 0; i < lists.Length; i++)
			{
				current[i] = lists[i].First();
			}
			#endregion

			int start = lists.Select(l => l.Lower).Max();

			while (true)
			{
				bool foundIntersection = true;
				int ilow = int.MinValue;// current[matchindex].Lower;
				int iup = int.MaxValue;// current[matchindex].Upper;
				for (int i = 0; i < lists.Length; i++)
				{
					IntIntervalList interli = lists[i];
					IntInterval cur = current[i];

					while (cur != null && (cur.Upper < start || cur.Length + 1 < duration))
						cur = interli.Next(cur);

					if (cur == null)
						return null;

					//do //look for overlapping intervals
					//{
					if (cur.Lower <= start)// && cur.Upper >= start)
					{
						//We can use this
						current[i] = cur;
						ilow = Math.Max(ilow, cur.Lower);
						iup = Math.Min(iup, cur.Upper);
						if (iup - ilow + 1 < duration)
						{
							foundIntersection = false;
							start = iup + 1; //Continue search in time
						}
						//	break;
					}
					else //if (cur.Lower > start)
					{
						//We've past start, which means that start cannot be the start of an overlap.
						//We update start and try again.
						current[i] = cur;
						start = cur.Lower;
						foundIntersection = false;
						//break;
					}

					if (!foundIntersection)// || !continueSearching)
						break;
				}
				if (foundIntersection)
				{
					return new IntInterval(ilow, iup); //Looking for the first possible interval only.
				}

			}

			//return null;

		}


		#endregion



	}
}
