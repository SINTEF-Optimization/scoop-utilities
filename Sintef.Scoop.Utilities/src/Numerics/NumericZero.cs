//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Container class for a function that numerically finds the zero of functions
	/// </summary>
	public static class NumericZero
	{

		/// <summary>
		/// Calculates an inverse value of a function.
		/// 
		/// Finds an argument value to a fairly monotonic function such that the corresponding function value is close enough to
		/// a target value within the given tolerances.
		/// 
		/// If no inverse can be found within the tolerances, the function throws an exception by default.
		/// However, if throwOnFailure is false, the function instead returns an x such that f(x) &lt; targetValue, while
		/// f(x') > targetValue, where x' is the next larger double that can be represented.
		/// </summary>
		/// <param name="function">The function to find an inverse of</param>
		/// <param name="targetValue">The function value to find the inverse of</param>
		/// <param name="xMin">The minimum paramer value to consider. The function value must be less than or equal to the target value at this point</param>
		/// <param name="xMax">The maximum paramer value to consider. The function value must be equal to or greater than the target value at this point</param>
		/// <param name="valueTolerance">The absolute tolerance to use for the function value</param>
		/// <param name="argumentTolerance">The absolute tolerance to use for x</param>
		/// <param name="throwOnFailure">If true, an exception is thrown if an inverse cannot be found within the given tolerances. 
		///  If false, the function instead returns the best possible result within the possibilities of numerical precision</param>
		/// <returns>An x for which the function returns targetValue (within valueTolerance), or for which
		///  function(x) is less than targetValue and function(x + argumentTolerance) is greater than targetValue</returns>
		public static double InverseOf(Func<double, double> function, double targetValue, double xMin, double xMax, double valueTolerance = 0, double argumentTolerance = 0, bool throwOnFailure = true)
		{
			return ZeroOf((x) => function(x) - targetValue, xMin, xMax, valueTolerance, argumentTolerance, throwOnFailure);
		}

		/// <summary>
		/// Calculates a zero of a function.
		/// 
		/// If no zero can be found within the tolerances, the function throws an exception by default.
		/// However, if throwOnFailure is false, the function instead returns an x such that f(x) is negative, while
		/// f(x') is positive, where x' is the next larger double that can be represented.
		/// </summary>
		/// <param name="function">The function to find a zero of</param>
		/// <param name="xMin">The minimum paramer value to consider. The function value must be zero or negative at this point</param>
		/// <param name="xMax">The maximum paramer value to consider. The function value must be zero or positive at this point</param>
		/// <param name="valueTolerance">The absolute tolerance to use for the function value</param>
		/// <param name="argumentTolerance">The absolute tolerance to use for x</param>
		/// <param name="throwOnFailure">If true, an exception is thrown if a zero cannot be found within the given tolerances. 
		///  If false, the function instead returns the best possible result within the possibilities of numerical precision</param>
		/// <returns>An x for which abs(function(x)) &lt;= valueTolerance, or for which
		///  function(x) is negative and function(x + argumentTolerance) is positive. 
		/// </returns>
		public static double ZeroOf(Func<double, double> function, double xMin, double xMax, double valueTolerance = 0, double argumentTolerance = 0, bool throwOnFailure = true)
		{
			if (xMin >= xMax)
				throw new ArgumentException("xMin must be less than xMax");
			if (valueTolerance < 0)
				throw new ArgumentException("valueTolerance cannot be negative");
			if (argumentTolerance < 0)
				throw new ArgumentException("argumentTolerance cannot be negative");

			double maxVal = function(xMax);
			if (Math.Abs(maxVal) < valueTolerance)
				return xMax;

			double minVal = function(xMin);
			if (Math.Abs(minVal) < valueTolerance)
				return xMin;

			if (minVal > 0)
				throw new InvalidOperationException("f(xMin) is positive");
			if (maxVal < 0)
				throw new InvalidOperationException("f(xMax) is negative");

			while (true)
			{
				if (xMax - xMin <= argumentTolerance)
					return xMin;

				double xMid = (xMin * maxVal - xMax * minVal) / (maxVal - minVal);

				// When xMin and xMax are close, numeric errors can cause xMid to not fall between them.
				// Avoid this.
				if (xMid > xMax)
					xMid = xMax;
				if (xMid < xMin)
					xMid = xMin;

				double midVal = function(xMid);

				if (Math.Abs(midVal) <= valueTolerance)
					return xMid;

				if (midVal < 0)
				{
					xMin = xMid;
					minVal = midVal;
					if (minVal < midVal * 2)
					{
						continue;
					}
				}
				else
				{
					xMax = xMid;
					maxVal = midVal;
					if (midVal * 2 < maxVal)
					{
						continue;
					}
				}

				if (xMax - xMin <= argumentTolerance)
					return xMin;

				// We did not make enough progress in the last iteration.
				// Do a binary search step to make robust progress
				xMid = (xMin + xMax) / 2.0;

				if (xMid == xMin || xMid == xMax)
				{
					// We have reached the limits of numerical precision without finding a value close enough to zero 
					if (throwOnFailure)
						throw new Exception(string.Format("The function jumps across zero at x = {0}, from {1} to {2}. Use a larger valueTolerance or argumentTolerance, "
							+ "or set throwOnFailure to false to guarantee a result",
							xMid, minVal, maxVal));
					else
						return xMin;
				}

				midVal = function(xMid);

				if (midVal < 0)
				{
					xMin = xMid;
					minVal = midVal;
				}
				else
				{
					xMax = xMid;
					maxVal = midVal;
				}
			}
		}
	}
}
