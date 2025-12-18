//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Sintef.Scoop.Utilities.Functions
{
	/// <summary>
	/// A class that represents a point in the plane, with X, and Y values.
	/// </summary>
	public class FunctionPoint
	{
		/// <summary>
		/// The point's X value
		/// </summary>
		public double X { get; internal set; }

		/// <summary>
		/// The Y-value of the function, immediately to the "left" of the point".
		/// Same as Y_right if the function is continuous at X.
		/// </summary>
		public double Y_left { get; internal set; }

		/// <summary>
		/// The Y-value of the function, immediately to the "right" of the point".
		/// Same as Y_left if the function is continuous at X.
		/// </summary>
		public double Y_right { get; internal set; }

		/// <summary>
		/// Initializes a point at (0,0)
		/// </summary>
		public FunctionPoint()
		{
			X = 0;
			Y_left = 0;
			Y_right = 0;
		}

		/// <summary>
		/// Constructor for points where the function is continuous.
		/// </summary>
		/// <param name="x">Variable value</param>
		/// <param name="y">Function value at this point</param>
		public FunctionPoint(int x, double y)
		{
			Y_left = y;
			Y_right = y;
			X = x;
		}

		/// <summary>
		/// Constructor for points at which the function is not continuous.
		/// </summary>
		/// <param name="x">Variable value</param>
		/// <param name="y_left">Function value up to this point.</param>
		/// <param name="y_right">Function value after this point.</param>
		public FunctionPoint(int x, double y_left, double y_right)
		{
			Y_left = y_left;
			Y_right = y_right;
			X = x;
		}

		/// <summary>
		/// ToString override
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			string result = "X = " + X.ToString() + ((Y_left == Y_right) ? ", Y = " + Y_left.ToString() : ", Y_left = " + Y_left.ToString() + ", Y_right = " + Y_right.ToString());
			return result;
		}
	}
	/// <summary>
	/// A class representing simple linear functions of one variable, Y(X).
	/// The function does not have to be continuous.
	/// </summary>
	public class PiecewiseLinearFunction<E> where E : FunctionPoint, new()
	{
		#region Internal classes

		#endregion

		#region Members/Properties

		/// <summary>
		/// The points in the function. TODO: More efficient data structure?
		/// </summary>
		internal SortedDictionary<double, E> _points;

		/// <summary>
		/// The points of the function, in order of increasing value of X
		/// </summary>
		public IEnumerable<KeyValuePair<double, E>> Points { get { return _points.AsEnumerable<KeyValuePair<double, E>>(); } }

		#endregion

		#region Construction
		/// <summary>
		/// Constructor
		/// </summary>
		public PiecewiseLinearFunction()
		{
			_points = new SortedDictionary<double, E>();
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="other"></param>
		public PiecewiseLinearFunction(PiecewiseLinearFunction<E> other)
		{
			_points = new SortedDictionary<double, E>(other._points);
		}

		#endregion

		#region Point sets, gets, and hass


		/// <summary>
		/// Clears all registered points
		/// </summary>
		public void Clear()
		{
			_points.Clear();
		}
		/// <summary>
		/// Returns true iff the function has apoint at the given X value.
		/// </summary>
		/// <param name="x">X</param>
		/// <returns></returns>
		public bool HasPointWithX(double x)
		{
			return _points.ContainsKey(x);
		}

		/// <summary>
		/// Returns the event of the given variable point, or null if no such is defined.
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public E GetPointAtX(double x) { return _points.ContainsKey(x) ? _points[x] : null; }

		/// <summary>
		/// The X values of all points
		/// </summary>
		public List<double> GetXs() { return _points.Keys.ToList(); }

		/// <summary>
		/// Returns an enumeration of events in the given timeinterval (limits are both inclusive).
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		public IEnumerable<E> GetPointsInXInterval(double start, double end)
		{
			return _points.Where(k => k.Key >= start && k.Key <= end).Select(k => k.Value);
		}

		/// <summary>
		/// Returns an enumeration of X-values for all points in the given timeinterval (limits are both inclusive).
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		public IEnumerable<double> GetXValuesInXInterval(double start, double end)
		{
			return _points.Where(k => k.Key >= start && k.Key <= end).Select(k => k.Key);
		}



		/// <summary>
		/// Returns the last point in the function (the point with the highest X value)
		/// </summary>
		/// <returns></returns>
		public E GetLastPoint()
		{
			return _points.Last().Value;
		}

		/// <summary>
		/// Returns the X value corresponding to the last point in the function,
		/// i.e. the highest X value.
		/// </summary>
		/// <returns></returns>
		public double GetLastX()
		{
			return _points.Last().Key;
		}

		/// <summary>
		/// Returne the last point, before the given X value.
		/// </summary>
		/// <param name="x"></param>
		/// <returns>Null if no such exists.</returns>
		public E GetLastPointBeforeX(double x)
		{
			return _points.LastOrDefault(e => e.Key < x).Value;
		}

		/// <summary>
		/// Returns the first point in the function (or null if no points are defined).
		/// </summary>
		/// <returns></returns>
		public E GetFirstPoint()
		{
			if (_points.Any())
				return _points.First().Value;
			else
				return null;
		}

		/// <summary>
		/// Returne the first point, after the given X value.
		/// </summary>
		/// <param name="x"></param>
		/// <returns>Null if no such exists.</returns>
		public E GetFirstPointAfterX(double x)
		{
			return _points.FirstOrDefault(e => e.Key > x).Value;
		}

		/// <summary>
		/// The number of events
		/// </summary>
		public int NumberOfPoints
		{
			get { return _points.Count; }
		}


		/// <summary>
		/// The (X,E) element at the given index
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public E GetPointAtIndex(int index)
		{
			return _points.ElementAt(index).Value;
		}

		/// <summary>
		/// Add a point for which the function is continuous.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void AddPoint(double x, double y)
		{
			AddPoint(new E() { X = x, Y_left = y, Y_right = y });
		}

		/// <summary>
		/// Add a point for which the function is discontinuous.
		/// </summary>
		/// <param name="x">X value</param>
		/// <param name="y_left">Function value up to the point</param>
		/// <param name="y_right">Function value in and after the point</param>
		public void AddPoint(double x, double y_left, double y_right)
		{
			AddPoint(new E() { X = x, Y_left = y_left, Y_right = y_right });
		}

		/// <summary>
		/// Add a point
		/// </summary>
		/// <param name="point"></param>
		public virtual void AddPoint(E point)
		{
			_points.Add(point.X, point);
		}

		/// <summary>
		/// Remove any point that has the given X value.
		/// </summary>
		/// <param name="x"></param>
		public virtual void RemovePointAtX(int x)
		{
			//if (_points.ContainsKey(x))
			_points.Remove(x);
		}
		#endregion

		#region Some simple math

		/// <summary>
		/// Calculates a determined integral of this piecewise linear function, giving a new
		/// piecewis linear function. Assumes end > start. If the given boundaries are outside
		/// of the function definition, the returned function will have no points.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="integralValue">On return, this contains the area under the original graph, between the given start and end.</param>
		/// <returns></returns>
		public virtual PiecewiseLinearFunction<E> CalculateIntegral(double start, double end, out double integralValue)
		{
			integralValue = 0;
			Debug.Assert(end > start);
			PiecewiseLinearFunction<E> integralFunction = new PiecewiseLinearFunction<E>();
			double xAtStart = _points.Keys.First() < start ? start : _points.Keys.First();
			if (xAtStart >= end)
				return null;
			double yAtStart = GetValue(xAtStart);
			FunctionPoint np = GetFirstPointAfterX(xAtStart);
			if (np != null)
				integralFunction.AddPoint(xAtStart, 0);

			double xAtNp = np.X;
			double yAtNp = np.Y_left;
			bool done = false;
			while (np != null)
			{
				double a = (yAtNp - yAtStart) / (xAtNp - xAtStart);
				double deltaX = xAtNp - xAtStart;
				double contributionFromSegment = (0.5 * a * Math.Pow(deltaX, 2) + yAtStart * deltaX);
				integralValue += contributionFromSegment;
				integralFunction.AddPoint(xAtNp, integralValue);
				if (done)
					break;

				//Moving on
				xAtStart = xAtNp;
				yAtStart = np.Y_right;
				np = GetFirstPointAfterX(xAtStart); //TODO can be more efficent here...
				if (np != null)
				{
					if (np.X < end)
					{
						xAtNp = np.X;
						yAtNp = np.Y_left;
					}
					else
					{
						xAtNp = end;
						yAtNp = GetValue(end);
						done = true;
					}
				}
			}
			return integralFunction;
		}

		/// <summary>
		/// Gets the (left) function value corresponding to the given X value. 
		/// If no value is defined (e.g. if the given value is outside of the function definition), 
		/// the method returns Negative Infinity.
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public double GetLeftValue(double x)
		{
			if (_points.ContainsKey(x))
				return _points[x].Y_left;
			else
				return GetValue(x);
		}

		/// <summary>
		/// Gets the (right) function value corresponding to the given X value. If you want the
		/// "left" value at x, use GetLeftValue(x).
		/// If no value is defined (e.g. if the given value is outside of the function definition), 
		/// the method returns Negative Infinity.
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public double GetValue(double x)
		{
			if (_points.ContainsKey(x))
				return _points[x].Y_right;
			else
			{
				FunctionPoint lp = GetLastPointBeforeX(x);
				if (lp == null)
					return double.NegativeInfinity;
				else
				{
					FunctionPoint np = GetFirstPointAfterX(x);
					if (np == null)
						return double.NegativeInfinity;
					else
					{
						double a = (np.Y_left - lp.Y_right) / (np.X - lp.X);
						return lp.Y_right + a * (x - lp.X);
					}
				}
			}
		}

		/// <summary>
		/// Gets the first X value corresponding to the given function value.
		/// If no value is defined (e.g. if the given value is outside of the function value range), 
		/// the method returns double.NegativeInfinity. May be overridden in sub classes.
		/// This base class implementation does linear interpolation between the 
		/// neighbouring points.
		/// </summary>
		/// <param name="y"></param>
		/// <returns></returns>
		public virtual double GetFirstXWithFunctionValue(double y)
		{
			double lastX = 0;
			double lastY = 0;
			int counter = -1;
			foreach (FunctionPoint p in _points.Values)
			{
				if (++counter > 0)
				{
					//Check if crossing y
					if ((lastY < y && p.Y_left >= y) || (lastY >= y && p.Y_left < y))
					{
						//Yes, crossing
						double a = (p.Y_left - lastY) / (p.X - lastX);
						return (y - lastY) / a;
					}
				}
				lastY = p.Y_right;
				lastX = p.X;
			}
			return double.NegativeInfinity;
		}

		#endregion

		/// <summary>
		/// Debug utility.
		/// </summary>
		internal void PrintToConsole(int start, int finish)
		{
			foreach (FunctionPoint ev in GetPointsInXInterval(start, finish))
			{
				Console.WriteLine(ev.ToString());
			}
		}


		/// <summary>
		/// Computes the integral of the function within the given (inclusive) x-value interval. Values above zero will be added,
		/// values below zero will be subtracted.
		/// </summary>
		/// <returns></returns>
		public double Integral(double start, double end)
		{
			double integralValue;
			CalculateIntegral(start, end, out integralValue);
			return integralValue;
		}

		/// <summary>
		/// Computes the integral of the function. Values above zero will be added,
		/// values below zero will be subtracted.
		/// </summary>
		/// <returns></returns>
		public double Integral()
		{
			return Integral(GetFirstPoint().X, GetLastPoint().X);
		}

		/// <summary>
		/// Returns the gradient at the given x coordinate.
		/// If this is at a "derivate discontinuous point", the function returns the average
		/// of the gradients before and after this point.
		/// If x is outside the defined scope of the function, we return double.NegativeInfinity.
		/// The same happens if the function has not points, or only one point, exactly at x.
		/// If the given x corresponds to an end point of the function, we retrun the gradient of the "other side".
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public double GetGradient(double x)
		{
			if (!_points.Any())
				return double.NegativeInfinity;

			FunctionPoint fp = GetPointAtX(x);
			if (fp != null)
			{
				FunctionPoint before = GetLastPointBeforeX(x);
				FunctionPoint after = GetFirstPointAfterX(x);
				if (before == null || after == null)
				{
					//fp is the earliest point..?
					if (before == null && after != null)
						return (after.Y_left - fp.Y_right) / (after.X - fp.X);

					//fp is the lastest point..?
					else if (before != null && after == null)
						return (fp.Y_left - before.Y_right) / (fp.X - before.X);

					//fp is the only point?
					else
						return double.NegativeInfinity;
				}
				else
				{
					//All three points exists, we take the average
					double gradBefore = (fp.Y_left - before.Y_right) / (fp.X - before.X);
					double gradAfter = (after.Y_left - fp.Y_right) / (after.X - fp.X);
					return (gradBefore + gradAfter) / 2.0;
				}
			}
			else
			{
				FunctionPoint before = GetLastPointBeforeX(x);
				if (before == null)
					return double.NegativeInfinity;

				FunctionPoint after = GetFirstPointAfterX(x);
				if (after == null)
					return double.NegativeInfinity;

				return (after.Y_left - before.Y_right) / (after.X - before.X);
			}
		}
	}

	/// <summary>
	/// Extension methods for <see cref="PiecewiseLinearFunction{E}"/>
	/// </summary>
	public static partial class FunctionExtension
	{
		/// <summary>
		/// Returns a new piecewise linear function which is a sum of this one and the input one.
		/// </summary>
		public static PiecewiseLinearFunction<E> SumWith<E>(this PiecewiseLinearFunction<E> a, PiecewiseLinearFunction<E> b) where E : FunctionPoint, new()
		{
			PiecewiseLinearFunction<E> temp = new PiecewiseLinearFunction<E>();

			//First, add the extreme points (unless they are shared)
			FunctionPoint firstAPoint = a.GetFirstPoint();
			FunctionPoint firstBPoint = b.GetFirstPoint();
			double minX = firstAPoint.X; ;
			if (firstAPoint.X < firstBPoint.X)
			{
				minX = firstBPoint.X;
				double leftY = a.GetLeftValue(minX);
				double rightY = firstBPoint.Y_right + a.GetValue(minX);
				temp.AddPoint(minX, leftY, rightY);
			}
			else if (firstAPoint.X > firstBPoint.X)
			{
				minX = firstAPoint.X;
				double leftY = b.GetLeftValue(minX);
				double rightY = firstAPoint.Y_right + b.GetValue(minX);
				temp.AddPoint(minX, leftY, rightY);
			}

			FunctionPoint lastAPoint = a.GetLastPoint();
			FunctionPoint lastBPoint = b.GetLastPoint();
			double maxX = lastAPoint.X;
			if (lastAPoint.X > lastBPoint.X)
			{
				maxX = lastBPoint.X;
				double leftY = a.GetLeftValue(maxX) + lastBPoint.Y_left;
				double rightY = a.GetValue(maxX);
				temp.AddPoint(maxX, leftY, rightY);
			}
			else if (lastAPoint.X < lastBPoint.X)
			{
				maxX = lastAPoint.X;
				double leftY = b.GetLeftValue(maxX) + lastAPoint.Y_left;
				double rightY = b.GetValue(maxX);
				temp.AddPoint(maxX, leftY, rightY);
			}

			//Then, add the in-between points.
			foreach (double x in a.Points.Union(b.Points).Select(kvp => kvp.Key).Distinct())
			{
				//The sum is only defined where both inputs are defined.
				if (x < minX || x > maxX)
					continue;

				//If this is a (non-common) extreme point, we have already added it, so we skip
				if (temp.HasPointWithX(x))
					continue;

				double leftVal = 0;
				double rightVal = 0;
				bool functionIsDefinedInX = false;

				FunctionPoint aPoint = a.GetPointAtX(x);
				if (aPoint != null)
				{
					if (a.GetFirstPoint().X < x)
						leftVal = aPoint.Y_left; //Do not add up if undefined (first point)
					if (a.GetLastPoint().X > x)
						rightVal = aPoint.Y_right;
					functionIsDefinedInX = true;
				}
				else
				{
					double contrib = a.GetValue(x);
					if (contrib != double.NegativeInfinity)
					{
						leftVal = contrib;
						rightVal = contrib;
						functionIsDefinedInX = true;
					}
				}

				FunctionPoint bPoint = b.GetPointAtX(x);
				if (bPoint != null)
				{
					if (b.GetFirstPoint().X < x)
						leftVal += bPoint.Y_left; //Do not add up if undefined (first point)
					if (a.GetLastPoint().X > x)
						rightVal += bPoint.Y_right;
					functionIsDefinedInX = true;
				}
				else
				{
					double rightcontrib = b.GetValue(x);
					if (rightcontrib != double.NegativeInfinity)
					{
						leftVal += rightcontrib;
						rightVal += rightcontrib;
						functionIsDefinedInX = true;
					}
				}

				if (functionIsDefinedInX)
					temp.AddPoint(x, leftVal, rightVal);
			}


			return temp;

		}

		/// <summary>
		/// If the given t is already a key in the dictionary, the piecewise linear function is added to the corresponding value, using SumWith.
		/// If not, a new entry is added to the dictionary (me[t] = f).
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="E"></typeparam>
		/// <param name="me"></param>
		/// <param name="t"></param>
		/// <param name="f"></param>
		public static void AddOrNew<T, E>(this Dictionary<T, PiecewiseLinearFunction<E>> me, T t, PiecewiseLinearFunction<E> f) where E : FunctionPoint, new()
		{
			if (me.ContainsKey(t))
				me[t] = me[t].SumWith<E>(f);
			else
				me[t] = f;
		}

	}

}
