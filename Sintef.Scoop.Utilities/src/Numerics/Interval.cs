//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities.OpenClosedInterval
{
	/// <summary>
	/// A bounded interval of T
	/// </summary>
	public class Interval<T>
	{
		/// <summary>
		/// Comparer used
		/// </summary>
		public IComparer<T> Comparer { get { return Start.Comparer; } }

		/// <summary>
		/// The start of the interval
		/// </summary>
		public readonly IntervalLowerBound<T> Start;

		/// <summary>
		/// The end of the interval
		/// </summary>
		public readonly IntervalUpperBound<T> End;

		/// <summary>
		/// Creates an interval
		/// </summary>
		/// <param name="start">The start bound value</param>
		/// <param name="end">The end bound value</param>
		/// <param name="inclusiveStart">whether start value gives an inclusive bound</param>
		/// <param name="inclusiveEnd">whether end value gives an inclusive bound</param>
		/// <param name="cmp">Comparer to use for type T</param>
		public Interval(T start, T end, bool inclusiveStart, bool inclusiveEnd, IComparer<T> cmp = null)
			: this(new IntervalLowerBound<T>(start, inclusiveStart, cmp), new IntervalUpperBound<T>(end, inclusiveEnd, cmp))
		{ }

		/// <summary>
		/// Creates an interval
		/// </summary>
		/// <param name="start">The lower bound</param>
		/// <param name="end">The upper bound</param>
		public Interval(IntervalLowerBound<T> start, IntervalUpperBound<T> end)
		{
			if (start.Comparer != end.Comparer)
				throw new ArgumentException("Bounds must use same comparerer");

			Start = start;
			End = end;

			if (Start > End)
				throw new ArgumentException("interval start is after end");
		}

		/// <summary>
		/// Creates a copy of the other interval
		/// </summary>
		public Interval(Interval<T> other)
		{
			Start = other.Start;
			End = other.End;
		}

		/// <summary>
		/// Wether interval has zero length
		/// </summary>
		public bool HasZeroLength
		{
			get
			{
				return Start == End;
			}
		}

		/// <summary>
		/// Returns true if this interval contains the given value at the inside (excluding Start and End)
		/// </summary>
		public bool ContainsNotOnBoundary(T value)
		{
			return Start < value && value < End;
		}

		/// <summary>
		/// Returns true if this interval contains the given value at the inside (including Start but excluding End)
		/// </summary>
		public bool ContainsNotOnUpperBoundary(T value)
		{
			return Start <= value && value < End;
		}

		/// <summary>
		/// Returns true if this interval contains the given value 
		/// </summary>
		public bool Contains(T value)
		{
			return Start <= value && value <= End;
		}

		/// <summary>
		/// Returns true if this interval contains the given bound 
		/// </summary>
		public bool Contains(IntervalLowerBound<T> bound)
		{
			return Start <= bound && bound <= End;
		}

		/// <summary>
		/// Returns true if this interval contains the given bound 
		/// </summary>
		public bool Contains(IntervalUpperBound<T> bound)
		{
			return Start <= bound && bound <= End;
		}

		/// <summary>
		/// Returns true if this interval contains the given value or if the given value is the exclusive end or start value of the interval
		/// </summary>
		public bool ContainsOrHasBoundary(T value)
		{
			return Start <= value && value <= End;
		}

		/// <summary>
		/// Returns true if this interval contains the whole of the given interval
		/// </summary>
		public bool Contains(Interval<T> interval)
		{
			return Contains(interval.Start) && Contains(interval.End);
		}

		/// <summary>
		/// Returns true if this interval and the other interval have a
		/// nonzero intersection, that is, this interval starts no later than
		/// other ends, and the other starts no later than this interval ends.
		/// </summary>
		public bool Intersects(Interval<T> other)
		{
			return other.Start <= End && Start <= other.End;
		}

		/// <summary>
		/// Returns the intersection between this and the other interval, i.e. the
		/// interval they have in common. If there is no intersection, returns null.
		/// 
		/// As a special case, if one interval ends and the other starts at the same bound,
		/// the result is an interval of zero length.
		/// </summary>
		public Interval<T> Intersection(Interval<T> other)
		{
			if (!Intersects(other))
				return null;

			return new Interval<T>(this.Start.Max(other.Start), this.End.Min(other.End));
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("{0} - {1}", Start, End);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return Start.GetHashCode() + End.GetHashCode();
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (obj is not Interval<T> other)
				return false;

			return Start == other.Start && End == other.End;
		}
	}

	/// <summary>
	/// The upper bound of an interval
	/// </summary>
	/// <typeparam name="T">Type of bound</typeparam>
	public class IntervalUpperBound<T>
	{
		/// <summary>
		/// Comparer to use for T
		/// </summary>
		public readonly IComparer<T> Comparer;

		/// <summary>
		/// The bound
		/// </summary>
		public readonly T Bound;

		/// <summary>
		/// Wether the bound is inclusive or not
		/// </summary>
		public readonly bool IsInclusive;

		/// <summary>
		/// Constructs an upper bound
		/// </summary>
		/// <param name="bound">The bound</param>
		/// <param name="inclusive">Whether bound is inclusive or not</param>
		/// <param name="comparer">Comparer to use</param>
		public IntervalUpperBound(T bound, bool inclusive, IComparer<T> comparer=null)
		{
			Comparer = comparer;
			Comparer ??= Comparer<T>.Default;

			Bound = bound;
			IsInclusive = inclusive;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return Bound.ToString() + (IsInclusive ? "]" : ")");
		}

		/// <summary>
		/// Compares this bound to the given value using the given comparer for type T
		/// </summary>
		/// <param name="value">Value to compare bound to</param>
		/// <returns>
		/// less than zero if bound is smaller, 
		/// 0 if bound equals value (must be inclusive), 
		/// greater than zero if bound is greater than value
		/// </returns>
		public int Compare(T value)
		{
			int t_cmp = Comparer.Compare(Bound, value);
			if (t_cmp != 0)
				return t_cmp;
			if (IsInclusive)
				return 0;
			return -1;
		}

		/// <summary>
		/// Compares a <see cref="IntervalUpperBound{T}"/> and a value
		/// </summary>
		public static bool operator<(IntervalUpperBound<T> bound, T value)
		{
			return bound.Compare(value) < 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalUpperBound{T}"/> and a value
		/// </summary>
		public static bool operator >(IntervalUpperBound<T> bound, T value)
		{
			return bound.Compare(value) > 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalUpperBound{T}"/> and a value
		/// </summary>
		public static bool operator <=(IntervalUpperBound<T> bound, T value)
		{
			return bound.Compare(value) <= 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalUpperBound{T}"/> and a value
		/// </summary>
		public static bool operator >=(IntervalUpperBound<T> bound, T value)
		{
			return bound.Compare(value) >= 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalUpperBound{T}"/> and a value
		/// </summary>
		public static bool operator ==(IntervalUpperBound<T> bound, T value)
		{
			return bound.Compare(value) == 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalUpperBound{T}"/> and a value
		/// </summary>
		public static bool operator !=(IntervalUpperBound<T> bound, T value)
		{
			return bound.Compare(value) != 0;
		}

		/// <summary>
		/// Compares a <see cref="IntervalUpperBound{T}"/> and a value
		/// </summary>
		public static bool operator <(T value, IntervalUpperBound<T> bound)
		{
			return bound.Compare(value) > 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalUpperBound{T}"/> and a value
		/// </summary>
		public static bool operator >(T value, IntervalUpperBound<T> bound)
		{
			return bound.Compare(value) < 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalUpperBound{T}"/> and a value
		/// </summary>
		public static bool operator <=(T value, IntervalUpperBound<T> bound)
		{
			return bound.Compare(value) >= 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalUpperBound{T}"/> and a value
		/// </summary>
		public static bool operator >=(T value, IntervalUpperBound<T> bound)
		{
			return bound.Compare(value) <= 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalUpperBound{T}"/> and a value
		/// </summary>
		public static bool operator ==(T value, IntervalUpperBound<T> bound)
		{
			return bound.Compare(value) == 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalUpperBound{T}"/> and a value
		/// </summary>
		public static bool operator !=(T value, IntervalUpperBound<T> bound)
		{
			return bound.Compare(value) != 0;
		}

		/// <summary>
		/// Compares this bound to the given bound using the given comparer for type T
		/// </summary>
		/// <param name="lower">Bound to compare this bound to</param>
		/// <returns>
		/// less than zero if this bound is smaller than given one, 
		/// 0 if bounds are equal (both must be inclusive and have same value), 
		/// greater than zero if this bound is greater than given one
		/// </returns>
		public int Compare(IntervalLowerBound<T> lower)
		{
			if (lower.Comparer != Comparer)
				throw new ArgumentException("Must have same comparer for T");
			return -lower.Compare(this);
		}

		/// <summary>
		/// Compares a <see cref="IntervalLowerBound{T}"/> and a <see cref="IntervalUpperBound{T}"/>
		/// </summary>
		public static bool operator <(IntervalUpperBound<T> bound, IntervalLowerBound<T> lower)
		{
			return bound.Compare(lower) < 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalLowerBound{T}"/> and a <see cref="IntervalUpperBound{T}"/>
		/// </summary>
		public static bool operator >(IntervalUpperBound<T> bound, IntervalLowerBound<T> lower)
		{
			return bound.Compare(lower) > 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalLowerBound{T}"/> and a <see cref="IntervalUpperBound{T}"/>
		/// </summary>
		public static bool operator <=(IntervalUpperBound<T> bound, IntervalLowerBound<T> lower)
		{
			return bound.Compare(lower) <= 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalLowerBound{T}"/> and a <see cref="IntervalUpperBound{T}"/>
		/// </summary>
		public static bool operator >=(IntervalUpperBound<T> bound, IntervalLowerBound<T> lower)
		{
			return bound.Compare(lower) >= 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalLowerBound{T}"/> and a <see cref="IntervalUpperBound{T}"/>
		/// </summary>
		public static bool operator ==(IntervalUpperBound<T> bound, IntervalLowerBound<T> lower)
		{
			return bound.Compare(lower) == 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalLowerBound{T}"/> and a <see cref="IntervalUpperBound{T}"/>
		/// </summary>
		public static bool operator !=(IntervalUpperBound<T> bound, IntervalLowerBound<T> lower)
		{
			return bound.Compare(lower) != 0;
		}

		/// <summary>
		/// Compares this bound to the given bound using the given comparer for type T
		/// </summary>
		/// <param name="other">Bound to compare this bound to</param>
		/// <returns>
		/// less than zero if this bound is smaller than given one, 
		/// 0 if bounds are equal,
		/// greater than zero if this bound is greater than given one
		/// </returns>
		public int Compare(IntervalUpperBound<T> other)
		{
			if (other.Comparer != Comparer)
				throw new ArgumentException("Must have same comparer for T");
			int t_cmp = Comparer.Compare(Bound, other.Bound);
			if (t_cmp != 0)
				return t_cmp;
			if (IsInclusive)
			{
				if (other.IsInclusive)
					return 0;
				return +1;
			}
			if (other.IsInclusive)
				return -1;
			return 0;
		}

		/// <summary>
		/// Compares two <see cref="IntervalUpperBound{T}"/>s
		/// </summary>
		public static bool operator <(IntervalUpperBound<T> bound, IntervalUpperBound<T> upper)
		{
			return bound.Compare(upper) < 0;
		}
		/// <summary>
		/// Compares two <see cref="IntervalUpperBound{T}"/>s
		/// </summary>
		public static bool operator >(IntervalUpperBound<T> bound, IntervalUpperBound<T> upper)
		{
			return bound.Compare(upper) > 0;
		}
		/// <summary>
		/// Compares two <see cref="IntervalUpperBound{T}"/>s
		/// </summary>
		public static bool operator <=(IntervalUpperBound<T> bound, IntervalUpperBound<T> upper)
		{
			return bound.Compare(upper) <= 0;
		}
		/// <summary>
		/// Compares two <see cref="IntervalUpperBound{T}"/>s
		/// </summary>
		public static bool operator >=(IntervalUpperBound<T> bound, IntervalUpperBound<T> upper)
		{
			return bound.Compare(upper) >= 0;
		}
		/// <summary>
		/// Compares two <see cref="IntervalUpperBound{T}"/>s
		/// </summary>
		public static bool operator ==(IntervalUpperBound<T> bound, IntervalUpperBound<T> upper)
		{
			return bound.Compare(upper) == 0;
		}
		/// <summary>
		/// Compares two <see cref="IntervalUpperBound{T}"/>s
		/// </summary>
		public static bool operator !=(IntervalUpperBound<T> bound, IntervalUpperBound<T> upper)
		{
			return bound.Compare(upper) != 0;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (this.GetType().Equals(obj.GetType()))
				return Compare(obj as IntervalUpperBound<T>) == 0;
			return base.Equals(obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return Bound.GetHashCode() + IsInclusive.GetHashCode();
		}

		/// <summary>
		/// Returns the smaller one of this bound and given other bound
		/// </summary>
		/// <param name="other">Other bound</param>
		/// <returns>Returns the smaller one of this bound and given other bound</returns>
		public IntervalUpperBound<T> Min(IntervalUpperBound<T> other)
		{
			if (other.Comparer != Comparer)
				throw new ArgumentException("Must have same comparer for T");
			return (Compare(other) <= 0 ? this : other);
		}

		/// <summary>
		/// Returns the greater one of this bound and given other bound
		/// </summary>
		/// <param name="other">Other bound</param>
		/// <returns>Returns the greater one of this bound and given other bound</returns>
		public IntervalUpperBound<T> Max(IntervalUpperBound<T> other)
		{
			if (other.Comparer != Comparer)
				throw new ArgumentException("Must have same comparer for T");
			return (Compare(other) >= 0 ? this : other);
		}
	}

	/// <summary>
	/// The lower bound of an interval
	/// </summary>
	/// <typeparam name="T">Type of bound</typeparam>
	public class IntervalLowerBound<T>
	{
		/// <summary>
		/// Comparer to use for T
		/// </summary>
		public readonly IComparer<T> Comparer;

		/// <summary>
		/// the bound
		/// </summary>
		public readonly T Bound;

		/// <summary>
		/// Whether bound is inclusive or not
		/// </summary>
		public readonly bool IsInclusive;

		/// <summary>
		/// Constructs a lower bound
		/// </summary>
		/// <param name="bound">The bound</param>
		/// <param name="inclusive">Whether bound is inclusive or not</param>
		/// <param name="comparer"></param>
		public IntervalLowerBound(T bound, bool inclusive, IComparer<T> comparer=null)
		{
			Comparer = comparer;
			Comparer ??= Comparer<T>.Default;
			Bound = bound;
			IsInclusive = inclusive;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return (IsInclusive ? "[" : "(") + Bound.ToString();
		}

		/// <summary>
		/// Compares this bound to the given value using the given comparer for type T
		/// </summary>
		/// <param name="value">Value to compare bound to</param>
		/// <returns>
		/// less than zero if bound is smaller, 
		/// 0 if bound equals value (must be inclusive), 
		/// greater than zero if bound is greater than value
		/// </returns>
		public int Compare(T value)
		{
			int t_cmp = Comparer.Compare(Bound, value);
			if (t_cmp != 0)
				return t_cmp;
			if (IsInclusive)
				return 0;
			return +1;
		}

		/// <summary>
		/// Compares a <see cref="IntervalLowerBound{T}"/> and a value
		/// </summary>
		public static bool operator <(IntervalLowerBound<T> bound, T value)
		{
			return bound.Compare(value) < 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalLowerBound{T}"/> and a value
		/// </summary>
		public static bool operator >(IntervalLowerBound<T> bound, T value)
		{
			return bound.Compare(value) > 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalLowerBound{T}"/> and a value
		/// </summary>
		public static bool operator <=(IntervalLowerBound<T> bound, T value)
		{
			return bound.Compare(value) <= 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalLowerBound{T}"/> and a value
		/// </summary>
		public static bool operator >=(IntervalLowerBound<T> bound, T value)
		{
			return bound.Compare(value) >= 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalLowerBound{T}"/> and a value
		/// </summary>
		public static bool operator ==(IntervalLowerBound<T> bound, T value)
		{
			return bound.Compare(value) == 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalLowerBound{T}"/> and a value
		/// </summary>
		public static bool operator !=(IntervalLowerBound<T> bound, T value)
		{
			return bound.Compare(value) != 0;
		}

		/// <summary>
		/// Compares a <see cref="IntervalLowerBound{T}"/> and a value
		/// </summary>
		public static bool operator <(T value, IntervalLowerBound<T> bound)
		{
			return bound.Compare(value) > 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalLowerBound{T}"/> and a value
		/// </summary>
		public static bool operator >(T value, IntervalLowerBound<T> bound)
		{
			return bound.Compare(value) < 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalLowerBound{T}"/> and a value
		/// </summary>
		public static bool operator <=(T value, IntervalLowerBound<T> bound)
		{
			return bound.Compare(value) >= 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalLowerBound{T}"/> and a value
		/// </summary>
		public static bool operator >=(T value, IntervalLowerBound<T> bound)
		{
			return bound.Compare(value) <= 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalLowerBound{T}"/> and a value
		/// </summary>
		public static bool operator ==(T value, IntervalLowerBound<T> bound)
		{
			return bound.Compare(value) == 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalLowerBound{T}"/> and a value
		/// </summary>
		public static bool operator !=(T value, IntervalLowerBound<T> bound)
		{
			return bound.Compare(value) != 0;
		}

		/// <summary>
		/// Compares this bound to the given bound using the given comparer for type T
		/// </summary>
		/// <param name="other">Bound to compare this bound to</param>
		/// <returns>
		/// less than zero if this bound is smaller than given one, 
		/// 0 if bounds are equal
		/// greater than zero if this bound is greater than given one
		/// </returns>
		public int Compare(IntervalLowerBound<T> other)
		{
			if (other.Comparer != Comparer)
				throw new ArgumentException("Must have same comparer for T");
			int t_cmp = Comparer.Compare(Bound, other.Bound);
			if (t_cmp != 0)
				return t_cmp;
			if (IsInclusive)
			{
				if (other.IsInclusive)
					return 0;
				return -1;
			}
			if (other.IsInclusive)
				return +1;
			return 0;
		}

		/// <summary>
		/// Compares a <see cref="IntervalLowerBound{T}"/>s
		/// </summary>
		public static bool operator <(IntervalLowerBound<T> bound, IntervalLowerBound<T> lower)
		{
			return bound.Compare(lower) < 0;
		}
		/// <summary>
		/// Compares two <see cref="IntervalLowerBound{T}"/>s
		/// </summary>
		public static bool operator >(IntervalLowerBound<T> bound, IntervalLowerBound<T> lower)
		{
			return bound.Compare(lower) > 0;
		}
		/// <summary>
		/// Compares two <see cref="IntervalLowerBound{T}"/>s
		/// </summary>
		public static bool operator <=(IntervalLowerBound<T> bound, IntervalLowerBound<T> lower)
		{
			return bound.Compare(lower) <= 0;
		}
		/// <summary>
		/// Compares two <see cref="IntervalLowerBound{T}"/>s
		/// </summary>
		public static bool operator >=(IntervalLowerBound<T> bound, IntervalLowerBound<T> lower)
		{
			return bound.Compare(lower) >= 0;
		}
		/// <summary>
		/// Compares two <see cref="IntervalLowerBound{T}"/>s
		/// </summary>
		public static bool operator ==(IntervalLowerBound<T> bound, IntervalLowerBound<T> lower)
		{
			return bound.Compare(lower) == 0;
		}
		/// <summary>
		/// Compares two <see cref="IntervalLowerBound{T}"/>s
		/// </summary>
		public static bool operator !=(IntervalLowerBound<T> bound, IntervalLowerBound<T> lower)
		{
			return bound.Compare(lower) != 0;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (this.GetType().Equals(obj.GetType()))
				return Compare(obj as IntervalLowerBound<T>) == 0;
			return base.Equals(obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return Bound.GetHashCode() + IsInclusive.GetHashCode();
		}

		/// <summary>
		/// Compares this bound to the given bound using the given comparer for type T
		/// </summary>
		/// <param name="upper">Bound to compare this bound to</param>
		/// <returns>
		/// less than zero if this bound is smaller than given one, 
		/// 0 if bounds are equal (both must be inclusive and have same value), 
		/// greater than zero if this bound is greater than given one
		/// </returns>
		public int Compare(IntervalUpperBound<T> upper)
		{
			if (upper.Comparer != Comparer)
				throw new ArgumentException("Must have same comparer for T");
			int t_cmp = Comparer.Compare(Bound, upper.Bound);
			if (t_cmp != 0)
				return t_cmp;
			if (IsInclusive && upper.IsInclusive)
				return 0;
			return +1;
		}

		/// <summary>
		/// Compares a <see cref="IntervalLowerBound{T}"/> and a <see cref="IntervalUpperBound{T}"/>
		/// </summary>
		public static bool operator <(IntervalLowerBound<T> bound, IntervalUpperBound<T> upper)
		{
			return bound.Compare(upper) < 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalLowerBound{T}"/> and a <see cref="IntervalUpperBound{T}"/>
		/// </summary>
		public static bool operator >(IntervalLowerBound<T> bound, IntervalUpperBound<T> upper)
		{
			return bound.Compare(upper) > 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalLowerBound{T}"/> and a <see cref="IntervalUpperBound{T}"/>
		/// </summary>
		public static bool operator <=(IntervalLowerBound<T> bound, IntervalUpperBound<T> upper)
		{
			return bound.Compare(upper) <= 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalLowerBound{T}"/> and a <see cref="IntervalUpperBound{T}"/>
		/// </summary>
		public static bool operator >=(IntervalLowerBound<T> bound, IntervalUpperBound<T> upper)
		{
			return bound.Compare(upper) >= 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalLowerBound{T}"/> and a <see cref="IntervalUpperBound{T}"/>
		/// </summary>
		public static bool operator ==(IntervalLowerBound<T> bound, IntervalUpperBound<T> upper)
		{
			return bound.Compare(upper) == 0;
		}
		/// <summary>
		/// Compares a <see cref="IntervalLowerBound{T}"/> and a <see cref="IntervalUpperBound{T}"/>
		/// </summary>
		public static bool operator !=(IntervalLowerBound<T> bound, IntervalUpperBound<T> upper)
		{
			return bound.Compare(upper) != 0;
		}

		/// <summary>
		/// Returns the smaller one of this bound and given other bound
		/// </summary>
		/// <param name="other">Other bound</param>
		/// <returns>Returns the smaller one of this bound and given other bound</returns>
		public IntervalLowerBound<T> Min(IntervalLowerBound<T> other)
		{
			if (other.Comparer != Comparer)
				throw new ArgumentException("Must have same comparer for T");
			return (Compare(other) <= 0 ? this : other);
		}

		/// <summary>
		/// Returns the greater one of this bound and given other bound
		/// </summary>
		/// <param name="other">Other bound</param>
		/// <returns>Returns the greater one of this bound and given other bound</returns>
		public IntervalLowerBound<T> Max(IntervalLowerBound<T> other)
		{
			if (other.Comparer != Comparer)
				throw new ArgumentException("Must have same comparer for T");
			return (Compare(other) >= 0 ? this : other);
		}
	}

	/// <summary>
	/// A list of bounded intervals
	/// </summary>
	public abstract class SortedIntervals<TInterval, T> : IEnumerable, IEnumerable<TInterval> 
		where TInterval : Interval<T> 
	{
		/// <summary>
		/// Creates a list of intervals
		/// </summary>
		/// <param name="intervals">The intervals</param>
		public abstract SortedIntervals<TInterval, T> Create(IEnumerable<TInterval> intervals);

		/// <summary>
		/// Creates a sub interval
		/// </summary>
		/// <param name="interval">Interval for which the subinterval to create</param>
		/// <param name="start">New lower bound</param>
		/// <param name="end">New upper bound</param>
		protected abstract TInterval CreateSubInterval(TInterval interval, IntervalLowerBound<T> start, IntervalUpperBound<T> end);

		/// <summary>
		/// Returns true if the list contains no intervals
		/// </summary>
		public bool IsEmpty()
		{
			return !_intervals.Any();
		}

		/// <summary>
		/// The comparer
		/// </summary>
		public IComparer<T> Comparer { get { return _intervals.FirstOrDefault()?.Comparer; } }

		/// <summary>
		/// The start of the first interval in the list
		/// </summary>
		public IntervalLowerBound<T> Start { get { return _intervals.First().Start; } }

		/// <summary>
		/// The end of the last interval in the list
		/// </summary>
		public IntervalUpperBound<T> End { get { return _intervals.Last().End; } }

		/// <summary>
		/// Enumerates the intervals
		/// </summary>
		public IEnumerable<TInterval> Intervals { get { return _intervals; } }

		/// <summary>
		/// The intervals
		/// </summary>
		private IList<TInterval> _intervals;

		/// <summary>
		/// Creates a list of intervals
		/// </summary>
		/// <param name="intervals">The intervals</param>
		public SortedIntervals(IEnumerable<TInterval> intervals)
		{
			if (intervals.AdjacentPairs().Any((p => p.Item2.Start <= p.Item1.End)))
				throw new ArgumentException("Intervals are not increasing and non-overlapping");

			IComparer<T> cmp = intervals.FirstOrDefault()?.Comparer;
			foreach(TInterval interval in intervals)
			{
				if (interval.Comparer != cmp)
					throw new ArgumentException("Intervals must use same comparer");
			}

			_intervals = intervals.ToList();
		}

		/// <summary>
		/// Updates this list to equal the intersection of this list and the other list.
		/// 
		/// Removes those parts of the list that do not intersect the other list.
		/// Intervals that do not intersect at all are removed, while those that intersect
		/// wholly or partially are repaced by their intersection with the other list.
		/// </summary>
		public void IntersectWith(SortedIntervals<TInterval, T> other)
		{
			// Eliminate trivial cases with one empty list
			if (!_intervals.Any())
				return;
			if (!other.Intervals.Any())
			{
				_intervals = new List<TInterval>();
				return;
			}

			IList<TInterval> result = new List<TInterval>(_intervals.Count);

			// Start with first interval in each list
			var myIt = _intervals.GetEnumerator();
			myIt.MoveNext();
			var otherIt = other.Intervals.GetEnumerator();
			otherIt.MoveNext();

			while (true)
			{
				// Check current intervals for intersection
				var intersection = myIt.Current.Intersection(otherIt.Current);
				if (intersection != null)
					result.Add(CreateSubInterval(myIt.Current, intersection.Start, intersection.End));

				// Move iterator(s) whose interval ends first to next interval
				bool moveMe = myIt.Current.End <= otherIt.Current.End;
				bool moveOther = myIt.Current.End >= otherIt.Current.End;
				if (moveMe)
				{
					if (!myIt.MoveNext())
						// End of my list
						break;
				}
				if (moveOther)
				{
					if (!otherIt.MoveNext())
						// End of other list
						break;
				}
			}

			_intervals = result;
		}

		/// <summary>
		/// Updates this list to equal the intersection of this list and the interval.
		/// 
		/// Removes those parts of the list that do not intersect the interval.
		/// Intervals that do not intersect at all are removed, while those that intersect
		/// wholly or partially are repaced by their intersection with the interval.
		/// </summary>
		public void IntersectWith(TInterval interval)
		{
			IntersectWith(Create(new TInterval[] { interval }));
		}

		/// <summary>
		/// Updates this list by removing the intervals in the other list.
		/// 
		/// Removes those parts of the list that intersect the other list.
		/// Intervals that intersect wholly are removed, those who intersect partially are replaced by the complement and invervals
		/// that do not intersect at all are unchanged.
		/// </summary>
		public void RemoveIntervals(SortedIntervals<TInterval, T> intervalsToRemove)
		{
			// Eliminate trivial cases with one empty list
			if (!_intervals.Any() || !intervalsToRemove.Intervals.Any())
				return;

			// Start with first interval in each list
			int ii_me = 0;
			var otherIt = intervalsToRemove.GetEnumerator();
			otherIt.MoveNext();

			while (true)
			{
				var mine = _intervals[ii_me];
				var other = otherIt.Current;

				// Check current intervals for intersection and update
				// ------------------------------------------
				if (mine.Start < other.Start)
				{
					// other completely part of mine without touching ends
					if (other.End < mine.End)
					{
						TInterval left = CreateSubInterval(mine, mine.Start, new IntervalUpperBound<T>(other.Start.Bound, !other.Start.IsInclusive, Comparer));
						TInterval right = CreateSubInterval(mine, new IntervalLowerBound<T>(other.End.Bound, !other.End.IsInclusive, Comparer), mine.End);
						_intervals.Insert(ii_me, left);
						++ii_me;
						_intervals[ii_me] = right;
					}
					else //other.End >= mine.End
					{
						// intersecting at end of mine
						if (other.Start <= mine.End)
						{
							_intervals[ii_me] = CreateSubInterval(mine, mine.Start, new IntervalUpperBound<T>(other.Start.Bound, !other.Start.IsInclusive, Comparer));
						}
						// else: other on right of mine, no intersection
					}
				}
				else // mine.Start >= other.Start 
				{
					if (other.End < mine.End)
					{
						// other intersects at start of mine
						if (mine.Start < other.End)
						{
							_intervals[ii_me] = CreateSubInterval(mine, new IntervalLowerBound<T>(other.End.Bound, !other.End.IsInclusive, Comparer), mine.End);
						}
						// else: other on left of mine, no intersection, maybe touching
					}
					// mine completely part of other => remove whole thing
					else
					{
						_intervals.RemoveAt(ii_me);
						if (ii_me >= _intervals.Count)
							// End of my list
							break;
						continue; // do not move anything, continue with same ii_me as it points to next interval now
					}
				}

				// Move iterator(s) whose interval ends first to next interval
				mine = _intervals[ii_me];
				bool moveMe = mine.End <= other.End;
				bool moveOther = mine.End >= other.End;
				if (moveMe)
				{
					++ii_me;
					if (ii_me >= _intervals.Count)
						// End of my list
						break;
				}
				if (moveOther)
				{
					if (!otherIt.MoveNext())
						// End of other list
						break;
				}
			}
		}

		/// <summary>
		/// Removes the given interval from this list of invervals.
		/// Intervals that intersect wholly are removed, those who intersect partially are replaced by the complement and invervals
		/// that do not intersect at all are unchanged.
		/// </summary>
		public void RemoveInterval(TInterval intervalToRemove)
		{
			RemoveIntervals(Create(new[] { intervalToRemove }));
		}

		#region IEnumerable implementation

		/// <inheritdoc/>
		public IEnumerator<TInterval> GetEnumerator()
		{
			return _intervals.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _intervals.GetEnumerator();
		}

		#endregion

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.Select(i => i.ToString()).Concatenate(" / ");
		}
	}

	/// <summary>
	/// Extension methods for 
	/// </summary>
	public static class IntervalExtensions
	{
		/// <summary>
		/// Creates a interval list that is the union of the given intervals.
		/// </summary>
		/// <param name="intervals">The intervals to create the unit of. They do not have to be 
		///   in chronological order and may overlap</param>
		/// <param name="mergeIntervals">Merges two intervals</param>
		public static IEnumerable<TInterval> CreateUnion<TInterval, T>(this IEnumerable<TInterval> intervals,
			Func<TInterval, TInterval, IntervalLowerBound<T>, IntervalUpperBound<T>, TInterval> mergeIntervals)
			where TInterval : Interval<T>
		{
			if (!intervals.Any())
				return new TInterval[0];

			IComparer<T> cmp = intervals.FirstOrDefault()?.Comparer;
			foreach (TInterval i in intervals)
				if (i.Comparer != cmp)
					throw new ArgumentException("All intervals must have same comparer");

			// Sort intervals
			var intervalsInOrder = intervals.OrderBy(x => x,
				Comparer<TInterval>.Create((x, y) => x.Start.Compare(y.Start))
				).ToList();

			List<TInterval> union = new();
			TInterval nextIntervalToAdd = intervalsInOrder.First();

			foreach (var interval in intervalsInOrder.Skip(1))
			{
				if (interval.Start <= nextIntervalToAdd.End ||
					(cmp.Compare(nextIntervalToAdd.End.Bound, interval.Start.Bound) == 0 && (nextIntervalToAdd.End.IsInclusive || interval.Start.IsInclusive)))
				{
					// Intervals overlap: merge them
					nextIntervalToAdd = mergeIntervals(nextIntervalToAdd, interval, nextIntervalToAdd.Start, nextIntervalToAdd.End.Max(interval.End));
					continue;
				}

				// Intervals do not overlap: commit one to the result
				union.Add(nextIntervalToAdd);
				nextIntervalToAdd = interval;
			}

			// Add the last interval in the result are return
			union.Add(nextIntervalToAdd);
			return union;
		}

	}
}
