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
	/// A piecewise constant function that takes double Y-values and integer X-values.
	/// The function's value is constant between integer arguments, but may jump
	/// at integer arguments. The function's value at an integer need not be
	/// equal to the value on either side.
	/// </summary>
	public class PiecewiseConstFunctionDouble
	{
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
		public int XLength { get { return _points.Last().X - _points[1].X; } }


		/// <summary>
		/// The total range of the function (with non-zero values)
		/// </summary>
		public IntInterval XRange { get { return new IntInterval(_points[1].X, _points.Last().X); } }


		/// <summary>
		/// All x-values (one from each point), from <see cref="FirstXValue"/> to <see cref="LastXValue"/>
		/// </summary>
		public IEnumerable<int> XPoints { get { return _points.Skip(1).Select(p => p.X); } }

		/// <summary>
		/// True if the function has any data points
		/// </summary>
		public bool HasData { get { return _points.Count > 1; } }

		#region Construction

		/// <summary>
		/// Creates a function that is 0 everywhere
		/// </summary>
		public PiecewiseConstFunctionDouble()
		{
			Clear();
		}

		/// <summary>
		/// Creates a copy of the given function
		/// </summary>
		/// <param name="other"></param>
		public PiecewiseConstFunctionDouble(PiecewiseConstFunctionDouble other)
		{
			_points = new List<Point>();
			other._points.ForEach(p => _points.Add(new Point(p.X, p.ValueAtX, p.ValueRightOfX)));
		}

		/// <summary>
		/// Clone
		/// </summary>
		/// <returns></returns>
		public PiecewiseConstFunctionDouble Clone()
		{
			return new PiecewiseConstFunctionDouble(this);
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
			PiecewiseConstFunctionDouble other = obj as PiecewiseConstFunctionDouble;
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
					if (!point.ValuesAreEqual(otherPoint))
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
			double b = 7;
			_points.ForEach(p => b *= 7 * (p.X + p.ValueRightOfX));
			return (int)b;
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
			if (lastIndex < firstIndex)
				return null;

			return _points.Skip(firstIndex).Take(lastIndex - firstIndex + 1);
		}

		/// <summary>
		/// Returns the minimum y-value part of the function that has the x-values within the given
		/// (inclusive) interval. Uses Binary search, exploiting that the point list 
		/// is sorted on ascending x-values. 
		/// </summary>
		/// <param name="window"></param>
		/// <returns>The minimum value, or null if there are no points in the interval.</returns>
		private double? MinValueInInterval(IntInterval window)
		{
			int lower = window.Lower;
			int firstIndex = _points.BinaryFirstIndex<Point>(p => p.X >= lower);
			if (firstIndex == _points.Count)
				return null;

			int upper = window.Upper;
			int lastIndex = _points.BinaryLastIndex<Point>(p => p.X <= upper, firstIndex);
			if (lastIndex < firstIndex)
				return null;

			return _points.Skip(firstIndex).Take(lastIndex - firstIndex + 1).Min(p => p.MinValue);
		}

		/// <summary>
		/// The tolarance used when comparing doubles
		/// </summary>
		double _tolerance = 0.0000000001;

		/// <summary>
		/// Returns the first interval within the given window where the function's value
		/// is not higher than the given max value for at least the given duration.
		/// Returns null if no such interval exists.
		/// 
		/// If minDuration is 0, the result can be a single integer, at a point where the
		/// function value is lower than on either side. This result can equal the start
		/// or the end of the window.
		/// </summary>
		public IntInterval GetFirsIntervalNeverHigherThan(IntInterval window, int minDuration, double maxValue)
		{
			// Find closest point left of or at window start
			int leftPtIndex = GetIndexLeftOfOrAt(window.Lower);
			Point leftPoint = _points[leftPtIndex];

			if (minDuration == 0 && leftPoint.X == window.Lower && leftPoint.ValueAtX.LessOrEqualWithTolerance(maxValue, _tolerance) && leftPoint.ValueRightOfX.GreaterThanWithTolerance(maxValue, _tolerance))
				// Very special case: We found a single point at the window start that works
				return new IntInterval(leftPoint.X, leftPoint.X);

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
						if (point.X - resultStart >= minDuration)
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
					if (window.Upper - point.X < minDuration)
						// We cannot find a long enough interval before the window ends
						return null;

					if (point.ValueRightOfX <= maxValue)
					{
						// A legal interval starts here
						ok = true;
						resultStart = point.X;
					}
					else
					{
						if (minDuration == 0 && point.ValueAtX <= maxValue)
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
		}


		/// <summary>
		/// Returns the first interval within the given window where the function's value
		/// is never lower than the given min value for at least the given duration.
		/// Returns null if no such interval exists.
		/// 
		/// If minDuration is 0, the result can be a single integer, at a point where the
		/// function value is higher than on either side. This result can equal the start
		/// or the end of the window.
		/// </summary>
		public IntInterval GetFirstIntervalNeverLowerThan(IntInterval window, int minDuration, double minValue)
		{
			// Find closest point left of or at window start
			int leftPtIndex = GetIndexLeftOfOrAt(window.Lower);
			Point leftPoint = _points[leftPtIndex];

			if (minDuration == 0 && leftPoint.X == window.Lower && leftPoint.ValueAtX.GreaterOrEqualWithTolerance(minValue, _tolerance) && leftPoint.ValueRightOfX.LessThanWithTolerance(minValue, _tolerance))
				// Very special case: We found a single point at the window start that works
				return new IntInterval(leftPoint.X, leftPoint.X);

			int resultStart = Math.Max(leftPoint.X, window.Lower);
			bool ok = leftPoint.ValueRightOfX >= minValue && window.Upper - resultStart >= minDuration;

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

					if (point.ValueAtX < minValue || point.ValueRightOfX < minValue)
					{
						// The low value interval stops here
						if (point.X - resultStart >= minDuration)
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
					if (window.Upper - point.X < minDuration)
						// We cannot find a long enough interval before the window ends
						return null;

					if (point.ValueRightOfX >= minValue)
					{
						// A legal interval starts here
						ok = true;
						resultStart = point.X;
					}
					else
					{
						if (minDuration == 0 && point.ValueAtX >= minValue)
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
		}


		/// <summary>
		/// Returns the first continuous x interval, within the given window, in which this function is
		/// larger or equal to the input function for all x values in the interval, if the input function is
		/// positioned so that it has it's first non-zero value at the beginning of the interval.
		/// </summary>
		/// <param name="window">The window in which to search</param>
		/// <param name="prof">The function that should be smaller for all x in the returned interval.</param>
		/// <returns></returns>
		public IntInterval GetFirstIntervalWhereLargerThan(IntInterval window, PiecewiseConstFunctionDouble prof)
		{
			PiecewiseConstFunctionDouble croppedToWindow = Crop(window);
			if (croppedToWindow == null)
				return null;

			//First some simple and fast checks, since the following logic takes some time
			if (croppedToWindow.XLength < prof.XLength)
				return null;
			else
			{
				IEnumerable<Point> valuePoints = prof._points.Skip(2).Take(prof._points.Count - 3); //Since we know that the first and the last point has left and right values, respectively,  = 0
				if (valuePoints.Any() && MinValue(croppedToWindow.XRange) < valuePoints.Min(p => p.MinValue))
					return null;
			}
			if (MaxValue(croppedToWindow.XRange) < prof.MaxValue())
				return null;

			foreach (Point point in croppedToWindow._points.Skip(1))// _points.Where(p => window.Contains(p._x)))
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
						if (violatedInBetween || point.Less(adjustedPoint, _tolerance))
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
							if (violatedInBetween || point.Less(adjustedPoint, _tolerance))
							{
								matchWasFound = false;
								break;
							}
						}
					}

					if (matchWasFound)
					{
						IntInterval res = new IntInterval(x - profPoint.X, x - profPoint.X + prof.XLength);
						return res;
					}

				}
			}
			return null;
		}

		/// <summary>
		/// Creates a new function equal to the part of this function to the right of the given input value
		/// </summary>
		/// <param name="start"></param>
		/// <returns>The new function, or null if <paramref name="start"/> was larger or equal to the end of this function.</returns>
		public PiecewiseConstFunctionDouble TakeFrom(int start)
		{
			if (start >= LastXValue)
				return null;
			IntInterval window = new IntInterval(start, LastXValue);
			PiecewiseConstFunctionDouble result = new PiecewiseConstFunctionDouble();

			//If there is not a point exactly at the start, we need to create one.
			Point lastBefore = _points.Last(p => p.X <= start);
			if (lastBefore.X < start)
				result._points.Add(new Point(start, 0, lastBefore.ValueRightOfX));

			PointsInInterval(window).Do(p => result._points.Add(new Point(p.X, p.ValueAtX, p.ValueRightOfX)));
			return result;
		}

		/// <summary>
		/// Creates a new function equal to the part of this function to the left of the given input value
		/// </summary>
		/// <param name="end"></param>
		/// <returns>The new function, or null if <paramref name="end"/> was smaller or equal to the start of this function.</returns>
		public PiecewiseConstFunctionDouble TakeTo(int end)
		{
			if (end <= FirstXValue)
				return null;
			IntInterval window = new IntInterval(FirstXValue, end);
			PiecewiseConstFunctionDouble result = new PiecewiseConstFunctionDouble();
			PointsInInterval(window).Do(p => result._points.Add(new Point(p.X, p.ValueAtX, p.ValueRightOfX)));

			//If there is not a point exactly at the end, we need to create one.
			Point firstAfter = _points.FirstOrDefault(p => p.X >= end);
			if (firstAfter != null && firstAfter.X > end)
				result._points.Add(new Point(end, firstAfter.ValueAtX, 0));

			return result;
		}

		/// <summary>
		/// Creates a new function equal to the part of this function that lies in the given interval (including the limits)
		/// </summary>
		/// <returns>The new function, or null if the cropping result is empty.</returns>
		public PiecewiseConstFunctionDouble Crop(IntInterval window)
		{
			int start = window.Lower;
			int end = window.Upper;

			if (start >= LastXValue || end < FirstXValue)
				return null;
			PiecewiseConstFunctionDouble result = new PiecewiseConstFunctionDouble();

			//If there is not a point exactly at the start, we need to create one.
			Point lastBefore = _points.Last(p => p.X <= start);
			if (lastBefore.X < start)
				result._points.Add(new Point(start, 0, lastBefore.ValueRightOfX));

			IEnumerable<Point> pointsInInterval = PointsInInterval(window);
			if (pointsInInterval != null)
				pointsInInterval.Do(p => result._points.Add(new Point(p.X, p.ValueAtX, p.ValueRightOfX)));

			//If there is not a point exactly at the end, we need to create one.
			Point firstAfter = _points.FirstOrDefault(p => p.X >= end);
			if (firstAfter != null && firstAfter.X > end)
				result._points.Add(new Point(end, firstAfter.ValueAtX, 0));

			return result;
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
		public void AddInOpenInterval(int start, int finish, double offset, double valueLimit, bool limitIsUpper)
		{
			AddInOpenIntervalImpl(start, finish, offset, valueLimit, limitIsUpper);
		}

		/// <summary>
		/// Adds the given offset to the function, from the given
		/// start to the given end, both exclusive.
		/// </summary>
		public void AddInOpenInterval(int start, int finish, double offset)
		{
			AddInOpenIntervalImpl(start, finish, offset, null, true);
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
		private void AddInOpenIntervalImpl(int start, int finish, double offset, double? valueLimit, bool limitIsUpper)
		{
			if (start == finish)
				return;

			int startIndex = GetIndexLeftOfOrAt(start);
			int finishIndex = GetIndexLeftOfOrAt(finish);

			// Make sure there are points at the start and finish

			if (_points[finishIndex].X != finish)
			{
				double valueAtFinish = _points[finishIndex].ValueRightOfX;
				++finishIndex;
				_points.Insert(finishIndex, new Point(finish, valueAtFinish, valueAtFinish));
			}

			if (_points[startIndex].X != start)
			{
				double valueAtStart = _points[startIndex].ValueRightOfX;
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

			if (_points[finishIndex].ValueAtX.EqualsWithTolerance(_points[finishIndex - 1].ValueRightOfX, _tolerance) &&
				_points[finishIndex].ValueAtX.EqualsWithTolerance(_points[finishIndex].ValueRightOfX, _tolerance))
			{
				// No change in value at finish
				_points.RemoveAt(finishIndex);
			}

			if (_points[startIndex].ValueAtX.EqualsWithTolerance(_points[startIndex - 1].ValueRightOfX, _tolerance) &&
				_points[startIndex].ValueAtX.EqualsWithTolerance(_points[startIndex].ValueRightOfX, _tolerance))
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
		private double AddOrSetToLimitIfExceeded(double number, double offset, double valueLimit, bool limitIsUpper)
		{
			double result = number + offset;
			if ((limitIsUpper && result.GreaterThanWithTolerance(valueLimit, _tolerance)) || (!limitIsUpper && result.LessThanWithTolerance(valueLimit, _tolerance)))
				result = valueLimit;
			return result;
		}

		/// <summary>
		/// Returns the index in _points of the first point whose x
		/// is smaller that or equal to the given value
		/// </summary>
		private int GetIndexLeftOfOrAt(int x)
		{
			int indexPassed = _points.BinaryFirstIndex(p => p.X > x);
			return indexPassed - 1;
		}

		/// <summary>
		/// Returns the first point to the left of 'x' in which the
		/// left-hand value is larger or equal to 'valueLimit'
		/// </summary>
		/// <param name="x"></param>
		/// <param name="valueLimit"></param>
		/// <returns>The point in question, or int.MinValue if no such point exists.</returns>
		public int GetLatestEarlierPointGEQ(int x, double valueLimit)
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
		public double Value(int x)
		{
			Point lp = _points.FirstOrDefault(p => p.X >= x);
			if (lp == null)
				return 0;
			else
				return lp.ValueAtX;
		}

		/// <summary>
		/// Explicitly sets the function values at the given X-value.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="valueLeft"></param>
		/// <param name="valueRight"></param>
		public void SetValue(int x, double valueLeft, double valueRight)
		{
			Point lp = _points.FirstOrDefault(p => p.X == x);
			if (lp == null)
			{
				Point newPoint = new Point(x, valueLeft, valueRight);
				Point prevPoint = _points.Last(p => p.X < x); //There will always be one
				_points.Insert(_points.IndexOf(prevPoint) + 1, newPoint);
			}
			else
			{
				lp.ValueAtX = valueLeft;
				lp.ValueRightOfX = valueRight;
			}
		}

		/// <summary>
		/// The function value for argument values immediately larger than ("to the right of") the given argument.
		/// </summary>
		/// <param name="x"></param>
		/// <returns>The value, or 0 if the argument is outside current definitions.</returns>
		public double ValueToTheRight(int x)
		{
			Point lp = _points.FirstOrDefault(p => p.X >= x);
			if (lp == null)
				return 0;
			else
				return lp.X == x ? lp.ValueRightOfX : lp.ValueAtX;
		}

		/// <summary>
		/// The average value at the given x-value.
		/// If x is exactly at a step, this will be the average between the left and right values.
		/// </summary>
		/// <param name="x"></param>
		/// <returns>The value, or 0 if the argument is outside current definitions.</returns>
		public double AverageValue(int x)
		{
			Point lp = _points.FirstOrDefault(p => p.X >= x);
			if (lp == null)
				return 0;
			else
				return lp.X == x ? (lp.ValueRightOfX + lp.ValueAtX) / 2.0 : lp.ValueAtX;
		}


		/// <summary>
		/// Returns the minimum value of the function
		/// </summary>
		/// <param name="filter">Optional. If given, only values valid for x-values in this interval are considered.</param>
		/// <returns></returns>
		public double MinValue(IntInterval filter = null)
		{
			if (filter != null)
			{
				//IEnumerable<Point> filtered = PointsInInterval(filter);// _points.Where(p => filter.Contains(p._x));

				double? minVal = MinValueInInterval(filter);

				if (minVal.HasValue)
					return minVal.Value;
				//if (filtered != null && filtered.Any())
				//	return filtered.Min(p => p.MinValue);
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
		/// <returns></returns>
		private bool Less(double upperLimit, IntInterval filter)
		{
			return MinValue(filter).LessThanWithTolerance(upperLimit, _tolerance);
		}

		/// <summary>
		/// Returns the max value of the function
		/// </summary>
		/// <param name="filter">Optional. If given, only values valid for x-values in this interval are considered.</param>
		/// <returns></returns>
		public double MaxValue(IntInterval filter = null)
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
			/// The function's value at x
			/// </summary>
			private double _valueAtX;

			/// <summary>
			/// The function's value to the right of x, until the next point
			/// </summary>
			private double _valueRightOfX;

			/// <summary>
			/// The x value (function argument)
			/// </summary>
			public int X { get; internal set; }

			/// <summary>
			/// The function's value at x
			/// </summary>
			public double ValueAtX
			{
				get { return _valueAtX; }
				internal set
				{
					_valueAtX = value;
					MinValue = Math.Min(_valueAtX, _valueRightOfX);
				}
			}

			/// <summary>
			/// The function's value to the right of x, until the next point
			/// </summary>
			public double ValueRightOfX
			{
				get { return _valueRightOfX; }
				internal set
				{
					_valueRightOfX = value;
					MinValue = Math.Min(_valueAtX, _valueRightOfX);
				}
			}

			/// <summary>
			/// Creates a new point with the given data
			/// </summary>
			public Point(int x, double valueAtX, double valueRightOfX)
			{
				X = x;
				_valueAtX = valueAtX;
				_valueRightOfX = valueRightOfX;
				MinValue = Math.Min(_valueRightOfX, _valueAtX);
			}

			/// <inheritdoc/>
			public override string ToString()
			{
				return String.Format("{0}:  [{1}]  --{2}->", X, ValueAtX, _valueRightOfX);
			}

			/// <summary>
			/// Returns true iff the value of this point is strictly smaller
			/// than the value of the other point, based on their respective left and right values.
			/// If the _x's are the same, the left value of this is compared with the left value of 'other',
			/// and correspondingly for the right value.
			/// If the _x's are different, the logical comparison is made.
			/// </summary>
			public bool Less(Point other, double tolerance)
			{
				if (X == other.X)
					return ValueAtX.LessThanWithTolerance(other.ValueAtX, tolerance) || _valueRightOfX.LessThanWithTolerance(other._valueRightOfX, tolerance);
				else if (X < other.X)
					return _valueRightOfX.LessThanWithTolerance(other.ValueAtX, tolerance);
				else
					return ValueAtX.LessThanWithTolerance(other._valueRightOfX, tolerance);
			}

			/// <summary>
			/// Returns true iff values to the left and right are equal for this and the given other point.
			/// </summary>
			/// <param name="otherPoint"></param>
			/// <returns></returns>
			internal bool ValuesAreEqual(Point otherPoint)
			{
				return otherPoint.ValueAtX == ValueAtX && otherPoint._valueRightOfX == _valueRightOfX;
			}

			/// <summary>
			/// The minimum of the right and left values. Updated whenever a new left or right value is set.
			/// </summary>
			public double MinValue { get; private set; }
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
			public double Value { get; set; }

			/// <summary>
			/// Constructor
			/// </summary>
			public Interval(int start, int end, double value)
			{
				Start = start;
				End = end;
				Value = value;
			}

		}

		/// <summary>
		/// Multiplies each value in the function by the given scalar.
		/// </summary>
		/// <param name="v"></param>
		public void MultiplyWith(double v)
		{
			foreach (var p in _points.Skip(1))
			{
				p.ValueAtX *= v;
				p.ValueRightOfX *= v;
			}
		}

		#endregion


		/// <summary>
		/// Adds the given function to this one.
		/// </summary>
		/// <param name="start">The start X value (in this function) at which the given function will be inserted (corresponding to zero in the given function).</param>
		/// <param name="prof"></param>
		public void Add(int start, PiecewiseConstFunctionDouble prof)
		{
			Add(start, prof, true);
		}


		/// <summary>
		/// Subtracts the given function from this one.
		/// </summary>
		/// <param name="start">The start X value (in this function) at which the given function will be inserted (corresponding to zero in the given function).</param>
		/// <param name="prof"></param>
		/// <param name="minValue">Optional. If given, the resulting function will have this as it's minimum value where subtraction happened. 
		/// I.e. all values that results from substraction and that are smaller than minValue will be replaced by minValue.
		/// Note that the original function may still have other points whose value exceed the limit, if no subtraction happened for these points.</param>
		public void Subtract(int start, PiecewiseConstFunctionDouble prof, double? minValue = null)
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
		/// I.e. all values resulting from the addition and that are larger (or smaller if the signIsPositive is false) than valueLimit will be replaced by valueLimit.
		/// Note that the original function may still have other points whose value exceed the limit, if no addition happened for these points.</param>
		private void Add(int startX, PiecewiseConstFunctionDouble prof, bool signIsPositive, double? valueLimit = null)
		{
			double sign = signIsPositive ? 1 : -1;

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
		public bool IsSquare { get { return _points.Count == 3 && _points[1].ValueAtX.EqualsWithTolerance(_points[2].ValueRightOfX, _tolerance); } }


		/// <summary>
		/// Computes the integral of the function. Values above zero will be added,
		/// values below zero will be subtracted.
		/// </summary>
		/// <returns></returns>
		public double Integral()
		{
			double sum = NonzeroIntervals.Sum(i => i.Value * ((double)(i.End - i.Start)));
			return sum;
		}

		/// <summary>
		/// Computes the integral of the function within the given (inclusive) x-value interval. Values above zero will be added,
		/// values below zero will be subtracted.
		/// </summary>
		/// <returns></returns>
		public double Integral(IntInterval xInterval)
		{
			//	double sum = NonzeroIntervals.Where(i => xInterval.Contains(i.End-1)// || xInterval.Contains(i.Start+1)).Sum(i => i.Value * (i.End - i.Start));

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

			return sum;
		}

		/// <summary>
		/// Computes the integral of the function within the given x-value interval. Ignores all parts of the
		/// graph that lies below y = 0.
		/// </summary>
		/// <returns></returns>
		public double IntegralIgnoringNegativeValues(IntInterval xInterval)
		{
			double sum = NonzeroIntervals.Where(i => (xInterval.Contains(i.End - 1) || xInterval.Contains(i.Start + 1)) && i.Value > 0).Sum(i => i.Value * (Math.Min(i.End, xInterval.Upper + 1) - Math.Max(i.Start, xInterval.Lower)));
			return sum;
		}

		/// <summary>
		/// Computes the integral of the function within the given x-value interval. Ignores all parts of the
		/// graph that lies above y = 0.
		/// </summary>
		/// <returns></returns>
		public double IntegralIgnoringPositiveValues(IntInterval xInterval)
		{
			double sum = NonzeroIntervals.Where(i => (xInterval.Contains(i.End - 1) || xInterval.Contains(i.Start + 1)) && i.Value.LessThanWithTolerance(0, _tolerance)).Sum(i => i.Value * (Math.Min(i.End, xInterval.Upper + 1) - Math.Max(i.Start, xInterval.Lower)));
			return sum;
		}


		#region Operators

		/// <summary>
		/// Adds the two functions
		/// </summary>
		/// <param name="c1"></param>
		/// <param name="c2"></param>
		/// <returns>Returns a new function that is the sum of the argument functions.</returns>
		public static PiecewiseConstFunctionDouble operator +(PiecewiseConstFunctionDouble c1, PiecewiseConstFunctionDouble c2)
		{
			PiecewiseConstFunctionDouble temp = new PiecewiseConstFunctionDouble(c1);
			temp.Add(0, c2);
			return temp;
		}

		/// <summary>
		/// Subtracts the last function from the first.
		/// </summary>
		/// <param name="c1"></param>
		/// <param name="c2"></param>
		/// <returns>Returns a new function that is the difference of the argument functions.</returns>
		public static PiecewiseConstFunctionDouble operator -(PiecewiseConstFunctionDouble c1, PiecewiseConstFunctionDouble c2)
		{
			PiecewiseConstFunctionDouble temp = new PiecewiseConstFunctionDouble(c1);
			temp.Subtract(0, c2);
			return temp;
		}

		#endregion

	}
}
