//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities.Functions
{
	/// <summary>
	/// A piecewise constant function that takes integer values.
	/// The function's value is constant between integer arguments, but may jump
	/// at integer arguments. The function's value at an integer need not be
	/// equal to the value on either side.
	/// </summary>
	public class PiecewiseConstFunction
	{
		#region Fields and Properties

		/// <summary>
		/// The points defining the jumps in the function. The points are sorted on 
		/// x, and the first point is always
		/// at (int.MinValue, 0, 0).
		/// </summary>
		private List<Point> _points;

		/// <summary>
		/// The points defining the jumps in the function. The points are sorted on 
		/// x, and the first point is always
		/// at (int.MinValue, 0, 0).
		/// </summary>
		public IEnumerable<Point> Points => _points.AsReadOnly();

		/// <summary>
		/// The last point's x-value
		/// </summary>
		/// <returns></returns>
		public int LastXValue { get { return _points.Last().X; } }

		/// <summary>
		/// The first real point's x-value. Assumes that at least one data point has been added.
		/// </summary>
		public int FirstXValue
		{
			get
			{
				if (_points.Count > 1)
					return _points[1].X;
				else
					throw new IndexOutOfRangeException("PiecewiseConstFunction.FirstXValue called when the function has no data");
			}
		}

		/// <summary>
		/// The total range of the function (with non-zero values)
		/// </summary>
		public int XRange { get { return _points.Last().X - _points[1].X; } }

		/// <summary>
		/// All x-values (one from each point), from <see cref="FirstXValue"/> to <see cref="LastXValue"/>
		/// </summary>
		public IEnumerable<int> XPoints { get { return _points.Skip(1).Select(p => p.X); } }

		#endregion

		#region Construction

		/// <summary>
		/// Creates a function that is 0 everywhere
		/// </summary>
		public PiecewiseConstFunction()
		{
			Clear();
		}

		/// <summary>
		/// Creates a copy of the given function
		/// </summary>
		/// <param name="other"></param>
		public PiecewiseConstFunction(PiecewiseConstFunction other)
		{
			_points = new List<Point>();
			other._points.ForEach(p => _points.Add(new Point(p.X, p.ValueAtX, p.ValueRightOfX)));
		}

		/// <summary>
		/// Clone
		/// </summary>
		/// <returns></returns>
		public PiecewiseConstFunction Clone()
		{
			return new PiecewiseConstFunction(this);
		}

		#endregion

		/// <summary>
		/// Resets the function to 0 everywhere
		/// </summary>
		public void Clear()
		{
			_points = new List<Point> { new Point(int.MinValue, 0, 0) };
		}

		/// <summary>
		/// Returns true if the functions have the same points and values
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			PiecewiseConstFunction other = obj as PiecewiseConstFunction;
			if (other == null)
				return false;

			if (_points.Count != other._points.Count)
				return false;
			foreach (Point point in _points)
			{
				Point otherPoint = other._points.FirstOrDefault(p => p.X == point.X);
				if (otherPoint == null)
					return false;
				else
				{
					if (otherPoint.ValueAtX != point.ValueAtX)
						return false;
					else if (otherPoint.ValueRightOfX != point.ValueRightOfX)
						return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Returns a hash code based on the points in the function.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			int b = 7;
			_points.ForEach(p => b *= 7 * (p.X + p.ValueRightOfX));
			return b;
		}

		/// <summary>
		/// Writes the points to console
		/// </summary>
		public void WriteToConsole()
		{
			Console.Write("PieceWiseConstFunc: ");
			int min = _points.Skip(1).Min(p => p.X);
			int max = _points.Skip(1).Max(p => p.X);
			foreach (Point p in _points.Skip(1)) //Skipping int.MinValue
			{
				Console.Write(", (" + p.X + ", " + p.ValueAtX + "|" + p.ValueRightOfX + ")");
			}
			Console.WriteLine();

		}

		/// <summary>
		/// Returns the part of the list that has the x-values within the given
		/// (inclusive) interval. Uses Binary search, exploiting that the point list 
		/// is sorted on ascending x-values. 
		/// </summary>
		/// <param name="window"></param>
		/// <returns>An enumeration of the points, or null if there are no points in the interval.</returns>
		private IEnumerable<Point> PointsInInterval(IntInterval window)
		{
			int lower = window.Lower;
			int firstIndex = _points.BinaryFirstIndex<Point>(p => p.X >= lower);
			if (firstIndex == _points.Count)
				return null;

			int upper = window.Upper;
			int lastIndex = _points.BinaryLastIndex<Point>(p => p.X <= upper, firstIndex);
			if (lastIndex < 0)
				return null;

			return _points.Skip(firstIndex).Take(lastIndex - firstIndex + 1);
		}

		/// <summary>
		/// Returns the first interval within the given window where the function's value
		/// is not higher than the given max value for at least the given duration.
		/// Returns null if no such interval exists.
		/// 
		/// If minDuration is 0, the result can be a single integer, at a point where the
		/// function value is lower than on either side. This result can equal the start
		/// or the end of the window.
		/// </summary>
		/// <param name="window"></param>
		/// <param name="minDuration"></param>
		/// <param name="maxValue"></param>
		/// <param name="mustOverlap">If given, then the returned interval must have a non-zero overlap with this time interval.</param>
		public IntInterval GetFirstFreeInterval(IntInterval window, int minDuration, int maxValue, IntInterval mustOverlap = null)
		{
			if (mustOverlap != null && !window.Overlaps(mustOverlap))
				return null;

			// Find closest point left of or at window start
			int leftPtIndex = GetIndexLeftOfOrAt(window.Lower);
			Point leftPoint = _points[leftPtIndex];

			if (leftPoint.X == window.Lower && minDuration == 0 && leftPoint.ValueAtX <= maxValue && leftPoint.ValueRightOfX > maxValue)
			{
				// Very special case: We found a single point at the window start that works
				if (OverlapOK(leftPoint.X, leftPoint.X))
					return new IntInterval(leftPoint.X, leftPoint.X);
			}

			int resultStart = Math.Max(leftPoint.X, window.Lower);
			bool ok = leftPoint.ValueRightOfX <= maxValue && window.Upper - resultStart >= minDuration;

			// Invariant from here: if ok is true, a permissible interval starts at resultStart
			//  and continues at least to the current point.
			//  if ok is false, no permissible interval was found starting before the current point.

			int ptIndex = leftPtIndex + 1;
			while (ptIndex < _points.Count)
			{
				Point point = _points[ptIndex];

				if (ok) // We are in a promising interval
				{
					if (point.X >= window.Upper)
						// Found time window end -- stop here
						return new IntInterval(resultStart, window.Upper);

					if (point.ValueAtX > maxValue || point.ValueRightOfX > maxValue)
					{
						// The low value interval stops here
						if (point.X - resultStart >= minDuration && OverlapOK(resultStart, point.X))
							// ...and is long enough. Return.
							return new IntInterval(resultStart, point.X);
						else
							// Too short. Look for a new, later interval
							ok = false;
					}
					// and else, the legal interval continues to the next point

				}
				else // We have no candidate interval so far
				{
					if (window.Upper - point.X < minDuration || !OverlapOK(point.X, window.Upper))
						// We cannot find a long enough interval before the window ends, or an interval that contains the required time.
						return null;

					if (point.ValueRightOfX <= maxValue)
					{
						// A legal interval starts here
						ok = true;
						resultStart = point.X;
					}
					else
					{
						if (minDuration == 0 && point.ValueAtX <= maxValue && OverlapOK(point.X, point.X))
							// Special case: There is a single legal point here
							return new IntInterval(point.X, point.X);
					}
				}

				++ptIndex;

			} // end while(ptIndex < _points.Count)

			// Passed the last point. The function is now constant until the end
			if (ok)
				return new IntInterval(resultStart, window.Upper);
			else
				return null;

			// <summary>
			// Returns true if mustContain either has no value, or is between (inclusive)
			// the two limits.
			// </summary>
			// <param name="resultStart"></param>
			// <param name="x"></param>
			// <returns></returns>
			bool OverlapOK(int a, int b) => (mustOverlap == null || mustOverlap.Overlaps(a, b));

		}



		/// <summary>
		/// Returns the first continuous x interval, within the given window, in which this function is
		/// larger than the input function for all x values in the interval, if the input function is
		/// positioned so that it has it's first non-zero value at the beginning of the interval.
		/// </summary>
		/// <param name="window">The window in which to search</param>
		/// <param name="prof">The function that should be smaller for all x in the returned interval.</param>
		/// <returns></returns>
		public IntInterval GetFirstIntervalWhereLargerThan(IntInterval window, PiecewiseConstFunction prof)
		{
			IEnumerable<Point> pointsInWindow = PointsInInterval(window);
			if (pointsInWindow == null)
				return null;

			foreach (Point point in pointsInWindow)// _points.Where(p => window.Contains(p._x)))
			{
				int x = point.X;

				//We can match each point in prof with this one (so that x correspons to 0 in prof coordinates).
				for (int i = 1; i < prof._points.Count; i++)
				{

					Point profPoint = prof._points[i];

					//Check first time window
					int xOfEarliestPoint = x + (prof.FirstXValue - profPoint.X);
					if (xOfEarliestPoint < window.Lower)
						continue;
					int xOfLatesPoint = x + (prof.LastXValue - profPoint.X);
					if (xOfLatesPoint > window.Upper)
						continue;


					bool matchWasFound = true;

					//Check capacity for earlier points
					foreach (Point earlyPoint in prof._points.Skip(1).Take(i - 1)) //Skipping the first neg.inf. point.
					{
						int earlyXInMyCoords = x + (earlyPoint.X - profPoint.X);
						Point adjustedPoint = new Point(earlyXInMyCoords, earlyPoint.ValueAtX, earlyPoint.ValueRightOfX);
						bool violatedInBetween = false;
						if (x - earlyXInMyCoords - 2 >= 0)
						{
							IntInterval temp = new IntInterval(earlyXInMyCoords + 1, x - 1);
							violatedInBetween = Less(earlyPoint.ValueRightOfX, temp);
						}
						if (violatedInBetween || point.Less(adjustedPoint))
						{
							matchWasFound = false;
							break;
						}
					}

					//Check capacity for this and later points
					if (matchWasFound)
					{
						foreach (Point laterPoint in prof._points.Skip(1 + i)) //Skipping the first neg.inf. point.
						{
							int laterXInMyCoords = x + (laterPoint.X - profPoint.X);
							Point adjustedPoint = new Point(laterXInMyCoords, laterPoint.ValueAtX, laterPoint.ValueRightOfX);
							bool violatedInBetween = false;
							if (laterXInMyCoords - x - 2 >= 0)
							{
								IntInterval temp = new IntInterval(x + 1, laterXInMyCoords - 1);
								violatedInBetween = Less(laterPoint.ValueAtX, temp);
							}
							if (violatedInBetween || point.Less(adjustedPoint))
							{
								matchWasFound = false;
								break;
							}
						}
					}

					if (matchWasFound)
					{
						IntInterval res = new IntInterval(x - profPoint.X, x - profPoint.X + prof.XRange);
						return res;
					}

				}
			}
			return null;
		}

		/// <summary>
		/// Adds the given offset to the function, from the given
		/// start to the given end, both exclusive. Enforces a absolute maximum/minimum limit on the result, within the specified
		/// interval.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="finish"></param>
		/// <param name="offset"></param>
		/// <param name="valueLimit">This will be taken as an upper/lower limit depending on the flag limitIsUpper,
		/// to replace any higher/lower resulting values. Note that this limit is not enforced elsewhere on the x-axis, only in the 
		/// defined interval.</param>
		/// <param name="limitIsUpper"></param>
		public void AddInOpenInterval(int start, int finish, int offset, int valueLimit, bool limitIsUpper)
		{
			AddInOpenIntervalImpl(start, finish, offset, valueLimit, limitIsUpper);
		}


		/// <summary>
		/// Adds the given offset to the function, from the given
		/// start to the given end, both exclusive.
		/// </summary>
		public void AddInOpenInterval(int start, int finish, int offset)
		{
			AddInOpenIntervalImpl(start, finish, offset, null, true);
		}

		/// <summary>
		/// Returns the end of an interval starting at the given <paramref name="start"/> time
		/// and with a value less or equal to the the given <paramref name="maxValue"/>.
		/// Returns null if no such interval exists.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="maxValue"></param>
		public int? GetMaxLengthFreeIntervalFrom(int start, int maxValue)
		{
			var firstInterval = GetFirstFreeInterval(new IntInterval(start, int.MaxValue), 0, maxValue);
			if (firstInterval.Lower == start)
				return firstInterval.Upper;
			else
				return null;
		}

		/// <summary>
		/// Adds the given offset to the function, from the given
		/// start to the given end, both exclusive.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="finish"></param>
		/// <param name="offset"></param>
		/// <param name="valueLimit">Set to null if no limit applies. This will otherwise be taken as an upper/lower limit depending on the flag limitIsUpper,
		/// to replace any higher/lower resulting values. Note that this limit is not enforced elsewhere on the x-axis, only in the 
		/// defined interval.</param>
		/// <param name="limitIsUpper"></param>
		private void AddInOpenIntervalImpl(int start, int finish, int offset, int? valueLimit, bool limitIsUpper)
		{
			if (start == finish)
				return;

			int startIndex = GetIndexLeftOfOrAt(start);
			int finishIndex = GetIndexLeftOfOrAt(finish, startIndex);

			// Make sure there are points at the start and finish

			if (_points[finishIndex].X != finish)
			{
				int valueAtFinish = _points[finishIndex].ValueRightOfX;
				++finishIndex;
				_points.Insert(finishIndex, new Point(finish, valueAtFinish, valueAtFinish));
			}

			if (_points[startIndex].X != start)
			{
				int valueAtStart = _points[startIndex].ValueRightOfX;
				++startIndex;
				++finishIndex;
				_points.Insert(startIndex, new Point(start, valueAtStart, valueAtStart));
			}

			// Update value between start and finish (exclusive)

			if (valueLimit.HasValue)
			{
				_points[startIndex].ValueRightOfX = AddOrSetToLimitIfExceeded(_points[startIndex].ValueRightOfX, offset, valueLimit.Value, limitIsUpper);
				for (int i = startIndex + 1; i < finishIndex; ++i)
				{
					var pt2 = _points[i];
					pt2.ValueRightOfX = AddOrSetToLimitIfExceeded(pt2.ValueRightOfX, offset, valueLimit.Value, limitIsUpper);
					pt2.ValueAtX = AddOrSetToLimitIfExceeded(pt2.ValueAtX, offset, valueLimit.Value, limitIsUpper);
				}
				_points[finishIndex].ValueAtX = AddOrSetToLimitIfExceeded(_points[finishIndex].ValueAtX, offset, valueLimit.Value, limitIsUpper);
			}
			else
			{
				_points[startIndex].ValueRightOfX += offset;
				for (int i = startIndex + 1; i < finishIndex; ++i)
				{
					var pt2 = _points[i];
					pt2.ValueRightOfX += offset;
					pt2.ValueAtX += offset;
				}
				_points[finishIndex].ValueAtX += offset;
			}

			// Remove start and/or finish point if redundant

			if (_points[finishIndex].ValueAtX == _points[finishIndex - 1].ValueRightOfX &&
				_points[finishIndex].ValueAtX == _points[finishIndex].ValueRightOfX)
			{
				// No change in value at finish
				_points.RemoveAt(finishIndex);
			}

			if (_points[startIndex].ValueAtX == _points[startIndex - 1].ValueRightOfX &&
				_points[startIndex].ValueAtX == _points[startIndex].ValueRightOfX)
			{
				// No change in value at start
				_points.RemoveAt(startIndex);
			}
		}

		/// <summary>
		/// Returns number + offset, or value limit if this is exceeded. Note that the
		/// interpretation of valueLimit depends on the flag limitIsUpper
		/// if offset &lt; 0.
		/// </summary>
		/// <param name="number"></param>
		/// <param name="offset"></param>
		/// <param name="valueLimit">Upper or lower limit, depending on the flag limitIsUpper</param>
		/// <param name="limitIsUpper">Determines if valueLimit is upper or lower limit.</param>
		/// <returns></returns>
		private int AddOrSetToLimitIfExceeded(int number, int offset, int valueLimit, bool limitIsUpper)
		{
			int result = number + offset;
			if ((limitIsUpper && result > valueLimit) || (!limitIsUpper && result < valueLimit))
				result = valueLimit;
			return result;
		}

		/// <summary>
		/// Returns the index in _points of the last point whose x
		/// is smaller that or equal to the given value
		/// </summary>
		/// <param name="x">The x value</param>
		/// <param name="startIndex">A start index at which to start the search. We only look at points at, or to the right of, the point with this index.</param>
		private int GetIndexLeftOfOrAt(int x, int startIndex = 0)
		{
			for (int i = startIndex; i < _points.Count - 1; ++i)
				if (_points[i + 1].X > x)
					return i;

			return _points.Count - 1;

		}

		/// <summary>
		/// Returns the first point to the left of 'x' in which the
		/// left-hand value is larger or equal to 'valueLimit'
		/// </summary>
		/// <param name="x"></param>
		/// <param name="valueLimit"></param>
		/// <returns>The point in question, or int.MinValue if no such point exists.</returns>
		public int GetLatestEarlierPointGEQ(int x, int valueLimit)
		{
			int lastIndex = _points.BinaryLastIndex(p => p.X < x);
			if (lastIndex < 0)
				return int.MinValue;
			Point point = _points.Take(lastIndex + 1).Where(p => p.ValueAtX >= valueLimit).LastOrDefault();
			if (point == default(Point))
				return int.MinValue;
			else
				return point.X;
		}

		/// <summary>
		/// The function value of the given argument, defined as the value up to this point. 
		/// </summary>
		/// <param name="x"></param>
		/// <returns>The value, or 0 if the argument is outside current definitions.</returns>
		public int Value(int x)
		{
			Point lp = _points.FirstOrDefault(p => p.X >= x);
			if (lp == null)
				return 0;
			else
				return lp.ValueAtX;
		}

		/// <summary>
		/// The function value for argument values immediately larger than ("to the right of") the given argument.
		/// </summary>
		/// <param name="x"></param>
		/// <returns>The value, or 0 if the argument is outside current definitions.</returns>
		public int ValueToTheRight(int x)
		{
			Point lp = _points.FirstOrDefault(p => p.X >= x);
			if (lp == null)
				return 0;
			else
				return lp.X == x ? lp.ValueRightOfX : lp.ValueAtX;
		}

		/// <summary>
		/// Returns the minimum value of the function
		/// </summary>
		/// <param name="filter">Optional. If given, only values valid for x-values in this interval are considered.</param>
		/// <returns></returns>
		public int MinValue(IntInterval filter = null)
		{
			if (filter != null)
			{
				IEnumerable<Point> filtered = PointsInInterval(filter);// _points.Where(p => filter.Contains(p._x));

				if (filtered != null)
					return filtered.Min(p => Math.Min(p.ValueAtX, p.ValueRightOfX));
				else
				{
					Point earlier = _points.LastOrDefault(p => p.X <= filter.Lower); //THere will always be one
					return earlier.ValueRightOfX;
				}
			}
			else
				return _points.Min(p => p.ValueAtX);
		}

		/// <summary>
		/// Returns true iff there is a point in the given interval that
		/// has a value less than the given limit.
		/// </summary>
		/// <param name="upperLimit"></param>
		/// <param name="filter"></param>
		/// <returns></returns>
		private bool Less(int upperLimit, IntInterval filter)
		{
			if (filter != null)
			{
				IEnumerable<Point> filtered = PointsInInterval(filter);// _points.Where(p => filter.Contains(p._x));

				//There could be that the filter contains no points, but that the value in the interval is still less than the limit
				if (filtered == null)
				{
					int lastEarlierPointIndex = _points.BinaryLastIndex<Point>(p => p.X < filter.Lower);
					return _points[lastEarlierPointIndex].ValueRightOfX < upperLimit;
				}
				else
					return filtered.Any(p => p.MinValue < upperLimit);
			}
			else
				return _points.Any(p => p.MinValue < upperLimit);
		}

		/// <summary>
		/// Returns the max value of the function
		/// </summary>
		/// <param name="filter">Optional. If given, only values valid for x-values in this interval are considered.</param>
		/// <returns></returns>
		public int MaxValue(IntInterval filter = null)
		{
			if (filter != null)
			{
				IEnumerable<Point> filtered = PointsInInterval(filter);// _points.Where(p => filter.Contains(p._x));
				if (filtered != null)
					return filtered.Max(p => Math.Max(p.ValueAtX, p.ValueRightOfX));
				else
				{
					Point earlier = _points.LastOrDefault(p => p.X <= filter.Lower); //THere will always be one
					return earlier.ValueRightOfX;
				}
			}
			else
				return _points.Max(p => p.ValueAtX);
		}

		/// <summary>
		/// Adds the given value to all the X-values.
		/// </summary>
		/// <param name="xShift"></param>
		public void ShiftOnX(int xShift)
		{
			_points.Do(p => p.X += xShift);
		}

		/// <summary>
		/// Enumerates the intervals for which the function has a nonzero value,
		/// ordered by x value.
		/// </summary>
		public IEnumerable<Interval> NonzeroIntervals
		{
			get
			{
				Point prev = null;

				foreach (var p in _points)
				{
					if (prev != null && prev.ValueRightOfX != 0)
					{
						yield return new Interval(prev.X, p.X, prev.ValueRightOfX);
					}

					prev = p;
				}
			}
		}

		#region Inner types

		/// <summary>
		/// A point where the function value changes
		/// </summary>
		public class Point
		{
			/// <summary>
			/// The x value (function argument)
			/// </summary>
			public int X { get; internal set; }

			/// <summary>
			/// The function's value at x
			/// </summary>
			public int ValueAtX { get; internal set; }

			/// <summary>
			/// The function's value to the right of x, until the next point
			/// </summary>
			public int ValueRightOfX { get; internal set; }

			/// <summary>
			/// Creates a new point with the given data
			/// </summary>
			public Point(int x, int valueAtX, int valueRightOfX)
			{
				X = x;
				ValueAtX = valueAtX;
				ValueRightOfX = valueRightOfX;
			}

			/// <inheritdoc/>
			public override string ToString()
			{
				return String.Format("{0}:  [{1}]  --{2}->", X, ValueAtX, ValueRightOfX);
			}

			/// <summary>
			/// Returns true iff the value of this point is strictly smaller
			/// than the value of the other point, based on their respective left and right values.
			/// If the _x's are the same, the left value of this is compared with the left value of 'other',
			/// and correspondingly for the right value.
			/// If the _x's are different, the logical comparison is made.
			/// </summary>
			/// <param name="other"></param>
			/// <returns></returns>
			public bool Less(Point other)
			{
				if (X == other.X)
					return ValueAtX < other.ValueAtX || ValueRightOfX < other.ValueRightOfX;
				else if (X < other.X)
					return ValueRightOfX < other.ValueAtX;
				else
					return ValueAtX < other.ValueRightOfX;

			}

			/// <summary>
			/// The minimum of the right and left values
			/// </summary>
			public int MinValue { get { return Math.Min(ValueAtX, ValueRightOfX); } }
		}

		/// <summary>
		/// An interval during which the function is constant. The limits are
		/// the points where the value change.
		/// </summary>
		public class Interval
		{
			/// <summary>
			/// The interval's start
			/// </summary>
			public int Start { get; set; }

			/// <summary>
			/// The interval's end
			/// </summary>
			public int End { get; set; }

			/// <summary>
			/// The function's value in the interval (start and end points excluded)
			/// </summary>
			public int Value { get; set; }

			/// <summary>
			/// Constructor
			/// </summary>
			public Interval(int start, int end, int value)
			{
				Start = start;
				End = end;
				Value = value;
			}

		}

		#endregion


		/// <summary>
		/// Adds the given function to this one.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="prof"></param>
		public void Add(int start, PiecewiseConstFunction prof)
		{
			Add(start, prof, true);
		}


		/// <summary>
		/// Subtracts the given function from this one.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="prof"></param>
		/// <param name="minValue">Optional. If given, the resulting function will have this as it's minimum value where subtraction happened. 
		/// I.e. all values that results from substraction and that are smaller than minValue will be replaced by minValue.
		/// Note that the original function may still have other points whose value exceed the limit, if no subtraction happened for these points.</param>
		public void Subtract(int start, PiecewiseConstFunction prof, int? minValue = null)
		{
			Add(start, prof, false, minValue);
		}


		/// <summary>
		/// Adds or subtracts the given function to this one, depending on the sign parameter.
		/// </summary>
		/// <param name="startX">The start X value (in this function) at which the given function will be inserted (corresponding to zero in the given function).</param>
		/// <param name="prof"></param>
		/// <param name="signIsPositive">If true, we are adding, if false the sign is negative, and we are subtracting.</param>
		/// <param name="valueLimit">Optional. If given, the resulting function will have this as it's limiting value where addition happens. 
		/// I.e. all values resulting from the addition and that are larger (or smaller if the sign is -1) than valueLimit will be replaced by valueLimit.
		/// Note that the original function may still have other points whose value exceed the limit, if no addition happened for these points.</param>
		private void Add(int startX, PiecewiseConstFunction prof, bool signIsPositive, int? valueLimit = null)
		{
			int sign = signIsPositive ? 1 : -1;

			//for each interval in prof, add/subtract the value in this function
			for (int i = 1; i < prof._points.Count - 1; i++)
			{
				Point point = prof._points[i];
				Point nextPoint = prof._points[i + 1];
				AddInOpenIntervalImpl(startX + point.X, startX + nextPoint.X, point.ValueRightOfX * sign, valueLimit, signIsPositive);
			}
		}

		/// <summary>
		/// Returns true if the function is constant. For debugging only. Remove later.
		/// </summary>
		public bool IsSquare { get { return _points.Count == 3 && _points[1].ValueAtX == _points[2].ValueRightOfX; } }


		/// <summary>
		/// Computes the integral of the function. Values above zero will be added,
		/// values below zero will be subtracted.
		/// </summary>
		/// <returns></returns>
		public int Integral()
		{
			int sum = NonzeroIntervals.Sum(i => i.Value * (i.End - i.Start));
			return sum;
		}

		/// <summary>
		/// Computes the integral of the function within the given (inclusive) x-value interval. Values above zero will be added,
		/// values below zero will be subtracted.
		/// </summary>
		/// <returns></returns>
		public int Integral(IntInterval xInterval)
		{
			double sum = 0.0;
			foreach (Interval iv in NonzeroIntervals)
			{
				if (iv.Start >= xInterval.Upper)
					break;
				if (iv.End <= xInterval.Lower)
					continue;
				if (xInterval.Covers(iv.Start, iv.End))
					sum += (iv.End - iv.Start) * iv.Value;
				else
					sum += ((double)(xInterval.Intersection(iv.Start, iv.End).Length - 1)) * iv.Value;
			}
			return (int)Math.Round(sum);
		}

		/// <summary>
		/// Computes the integral of the function within the given x-value interval. Ignores all parts of the
		/// graph that lies below y = 0.
		/// </summary>
		/// <returns></returns>
		public int IntegralIgnoringNegativeValues(IntInterval xInterval)
		{
			int sum = NonzeroIntervals.Where(i => (xInterval.Contains(i.End - 1) || xInterval.Contains(i.Start + 1)) && i.Value > 0).Sum(i => i.Value * (Math.Min(i.End, xInterval.Upper + 1) - Math.Max(i.Start, xInterval.Lower)));
			return sum;
		}

		/// <summary>
		/// Computes the integral of the function within the given x-value interval. Ignores all parts of the
		/// graph that lies above y = 0.
		/// </summary>
		/// <returns></returns>
		public int IntegralIgnoringPositiveValues(IntInterval xInterval)
		{
			int sum = NonzeroIntervals.Where(i => (xInterval.Contains(i.End - 1) || xInterval.Contains(i.Start + 1)) && i.Value < 0).Sum(i => i.Value * (Math.Min(i.End, xInterval.Upper + 1) - Math.Max(i.Start, xInterval.Lower)));
			return sum;
		}


	}
}
