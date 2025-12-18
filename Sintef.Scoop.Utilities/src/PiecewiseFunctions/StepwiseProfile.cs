//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

//This namespace contains simple functionality for handling simple
//functions.
namespace Sintef.Scoop.Utilities.Functions
{
	/// <summary>
	/// A class that represents a piecewise constant function, where data points
	/// mark the transition from one function value to the next. The function is not 
	/// (in general) continuous in the data points, and we use the convention that
	/// for a data point (X*,Y*), Y(X) = Y* for X>X* (until the next data point).
	/// </summary>
	public class StepwiseProfile<E> : PiecewiseLinearFunction<E> where E : FunctionPoint, new()
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public StepwiseProfile()
		{
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="other"></param>
		public StepwiseProfile(StepwiseProfile<E> other)
						: base(other)
		{
		}

		/// <summary>
		/// Removes an event from the profile. If there is no event at the given
		/// time, the function does nothing.
		/// </summary>
		/// <param name="start">The variable value at which to remove the event.</param>
		/// <param name="levelReduction">The reduction in profile level as a result of the removal.</param>
		/// <param name="finish">The last variable value for which the reduction is valid. Optional; if not provided (or if negative),
		/// the reduction is done through to the end of the profile. If positive, it is required to be greater than 'start'</param>
		public void Remove(int start, double levelReduction, int finish = -1)
		{
			if (HasPointWithX(start))
			{
				//Remove
				RemovePointAtX(start);

				//Adjust later profile values
				AdjustLevel(start, -levelReduction, finish);
			}
		}


		/// <summary>
		/// Adds a point to the profile at the given startX value, where all Y-values to the right of this new 
		/// point (or up to "finishX" if this is given) is calculated from the previous Y value at startX by adding "levelAddition". 
		/// If a point already exists at startX, the function does not add a new one, but still adjusts Y_right for the point.
		/// If "finishX" is provided (and non-negative), and does not already exists, then another point is added at "finishX". Y-values
		/// are left un-changes for all points with X less than startX or X  greater or equal to finishX.
		/// </summary>
		/// <param name="startX">The variable value at which to remove the event.</param>
		/// <param name="yAddition">The addition in profile level as a result of the removal.</param>
		/// <param name="finishX">The last variable value for which the reduction is valid. Optional; if not provided (or if negative),
		/// the reduction is done through to the end of the profile. If positive, it is required to be greater than 'start'</param>
		public void AddYinInterval(double startX, double yAddition, double finishX = -1)
		{
			if (!HasPointWithX(startX))
			{
				E lastbef = GetLastPointBeforeX(startX) as E;
				double levelBef = (lastbef != null) ? lastbef.Y_right : 0;
				AddPoint(startX, levelBef, levelBef);//Right-value to be adjusted below
			}

			//Adjust later profile values
			AdjustLevel(startX, yAddition, finishX);
		}

		/// <summary>
		/// Adjusts the level of the profile, from start to finish. Assumes there is an event at
		/// "start".
		/// </summary>
		/// <param name="start">The variable value at which to start level adjustment.</param>
		/// <param name="adjustment">The adjustment in profile level/value.</param>
		/// <param name="finish">The last variable value for which the adjustment is valid. Optional; if not provided (or if negative),
		/// the reduction is done through to the end of the profile. If positive, it is required to be greater than 'start'. </param>
		public void AdjustLevel(double start, double adjustment, double finish = -1)
		{
			if (adjustment == 0)
				return;

			bool isRenewable = finish >= 0;
#if DEBUG
			if (isRenewable)
				Debug.Assert(finish >= start);
#endif

			double valueAtFinish = -1;
			if (isRenewable)
			{
				valueAtFinish = GetValue(finish);
				if (valueAtFinish == double.NegativeInfinity)
					valueAtFinish = 0;
			}

			IEnumerable<E> futurepoints = isRenewable ? GetPointsInXInterval(start, finish) // _points.Keys.Where(x => x >= start && x <= finish).ToList() //finish is inclusive
: GetPointsInXInterval(start, double.PositiveInfinity); // _events.Keys.Where(x => x >= start).ToList();
			foreach (E fpoint in futurepoints)
			{

				if (fpoint.X > start)
					fpoint.Y_left += adjustment;
				if (fpoint.X < finish)
					fpoint.Y_right += adjustment;
#if DEBUG
				if (fpoint.Y_left < 0 || fpoint.Y_right < 0)
				{
					Console.WriteLine("resource profile reduced to sub-zero...");
				}
#endif
			}

			if (isRenewable && adjustment > 0)
			{
				if (!HasPointWithX(finish))
				{
					AddPoint(finish, valueAtFinish + adjustment, valueAtFinish);
				}
			}

		}

		/// <summary>
		/// Returns the first positive-length interval during which the profile's value is no larger than the
		/// given maxValue, and which is in the given window and is no shorter than minDuration.
		/// 
		/// This function only works when all X values in the profile are integers.
		/// </summary>
		/// <param name="window">The window in which to search, end points inclusive</param>
		/// <param name="minLength">The minimum allowed length of the result</param>
		/// <param name="maxValue">The maximum allowed value of the profile in the result interval</param>
		/// <returns>The found interval. End points are inclusive</returns>
		public IntInterval GetFirstFreeInterval(IntInterval window, int minLength, int maxValue)
		{
			if (minLength < 0)
				throw new ArgumentException("minLength must be nonnegative");
			if (maxValue < 0)
				throw new ArgumentException("maxValue must be nonnegative");
			if (window == null)
				throw new ArgumentNullException("window");
			if (window.Length < 1)
				throw new ArgumentException("window must have length at least 1");

			if (NumberOfPoints == 0)
			{
				return window;
			}

			List<double> timepoints = GetXs();
			int windowStart = window.Lower;
			int windowEnd = window.Upper;
			int windowLastPeriod = window.Upper - 1;

			int earliestMatchIndex = timepoints.BinaryFirstIndex(x => x >= windowStart); //TODO make start index functionality in BinaryFirstIndex to make more efficient when intersection.count > 1.
			if (earliestMatchIndex == timepoints.Count)
			{
				//The profile contained no point with X >= firstStart => The whole profile lies to the left of the interval, and we return the interval.
				return window;
			}

			if (earliestMatchIndex == timepoints.Count - 1)
			{
				double lx = timepoints[earliestMatchIndex];
				if ((int)lx > windowStart && GetLeftValue(lx) <= maxValue)
				{
					return window;
				}
				else if (windowLastPeriod - lx + 1 >= minLength)
					return new IntInterval((int)lx, windowEnd);
				else
					return null;
			}

			if (timepoints[earliestMatchIndex] > windowLastPeriod)
			{
				if (earliestMatchIndex == 0)    //In this case, the entire resource profile lies to "the right" of the input interval, and we return the input interval.
				{
					return window;
				}
				else
				{
					FunctionPoint p = GetPointAtIndex(earliestMatchIndex);
					if (p.Y_left <= maxValue) //In which case [firstStart, firstEnd] lies between the point, and the first point to the left of point. 
																		//					|| firstStart == firstEnd && p.Y_right <= capBuf) //In which case the point corresponds to the single period in first, and we're happy if the right value is small enough.
					{
						return window;
					}
					else
						return null;
				}
			}
			else //Search on trough the profile
			{
				int matchIndex = earliestMatchIndex;
				for (int i = matchIndex; i < timepoints.Count - 1; i++)
				{
					double firstX = timepoints[i];
					FunctionPoint pointI = GetPointAtX(firstX);
					if (firstX > windowLastPeriod)
					{
						return null;
					}

					if (pointI.Y_right <= maxValue)
					{
						//If this was the first point, then we should be calculating from the start of the interval
						if (i == earliestMatchIndex && pointI.Y_left <= maxValue)
							firstX = windowStart;
						double lastX = firstX;

						//Promising. Now, for how long does this last.
						for (int j = i + 1; j < timepoints.Count; j++)
						{
							lastX = timepoints[j];
							if (lastX > windowLastPeriod + 1)
							{
								lastX = windowLastPeriod + 1;
								//So far, no longer
								if (lastX - firstX >= minLength)
								{
									return new IntInterval((int)firstX, (int)lastX);
								}
								else
									return null;
							}
							else if (GetValue(lastX) > maxValue)
							{
								//So far, no longer?
								if (lastX - firstX >= minLength)
									return new IntInterval((int)firstX, (int)lastX);
								else
								{
									//Continue the search
									matchIndex = j + 1;
									break;
								}
							}
							else if (j == timepoints.Count - 1 && lastX <= windowLastPeriod + 1)
							{
								if (windowLastPeriod + 1 - firstX >= minLength)
									return new IntInterval((int)firstX, windowEnd);
								else
									return null;
							}
						}
					}
					else
					{
						//No luck. However, if this was the first point, then we might have an interval between
						//the start of the interval and this point
						if (i == earliestMatchIndex && pointI.Y_left <= maxValue)
						{
							if ((firstX > windowStart) && firstX - windowStart >= minLength)
								return new IntInterval(windowStart, (int)firstX);

							//Otherwise, continue the search
						}
					}
				}
				//No luck, anywhere
				if (windowLastPeriod - timepoints.Last() + 1 >= minLength)
					return new IntInterval((int)timepoints.Last(), windowEnd);
				else
					return null;
			}
		}

		/// <summary>
		/// Gets the first X value corresponding to the given Y value.
		/// If no value is defined (e.g. if the given value is outside of the function value range), 
		/// the method returns double.NegativeInfinity. May be overridden in sub classes.
		/// This implementation is for the step function.
		/// </summary>
		/// <param name="y"></param>
		/// <returns></returns>
		public override double GetFirstXWithFunctionValue(double y)
		{
			foreach (FunctionPoint p in _points.Values)
			{
				//Check if crossing y
				if ((p.Y_left < y && p.Y_right >= y) || (p.Y_left >= y && p.Y_right < y))
				{
					//Yes, crossing
					return p.X;
				}
			}
			return double.NegativeInfinity;
		}


	}
}

