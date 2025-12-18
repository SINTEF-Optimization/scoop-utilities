//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Numeric extensions methods.
	/// </summary>
	public static partial class Extensions
	{
		/// <summary>
		/// Returns the factorial of the given integer.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public static int Factorial(this int a)
		{
			int fact = 1;
			for (int i = 2; i <= a; i++)
				fact *= i;
			return fact;
		}

		/// <summary>
		/// Returns the <see cref="Complex"/> sum of an enumerable of <see cref="Complex"/> numbers.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		public static Complex ComplexSum(this IEnumerable<Complex> e)
		{
			double real = e.Sum(c => c.Real);
			double imag = e.Sum(c => c.Imaginary);
			return new Complex(real, imag);
		}

		/// <summary>
		/// Returns the <see cref="Complex"/> sum of complex numbers produce by the given function <paramref name="f"/>
		/// from an enumerable of objects of type <typeparamref name="K"/>.
		/// </summary>
		public static Complex ComplexSum<K>(this IEnumerable<K> e, Func<K, Complex> f)
		{
			return e.Select(c => f(c)).ComplexSum();
		}

		/// <summary>
		/// Returns the <see cref="Complex"/> sum of complex numbers produce by the given function <paramref name="f"/>
		/// from an enumerable of <see cref="KeyValuePair{K,V}"/>'s.
		/// </summary>
		public static Complex ComplexSum<K, V>(this IEnumerable<KeyValuePair<K, V>> e, Func<KeyValuePair<K, V>, Complex> f)
		{
			return e.Select(c => f(c)).ComplexSum();
		}

		/// <summary>
		/// Converts a vector given as an (x,y) tuple to an angle (as measured in degrees, counterclockwise from the positive x-axis direction).
		/// </summary>
		/// <returns></returns>
		public static double ToAngle(this (double x, double y) vec)
		{
			double length = Math.Sqrt(vec.x * vec.x + vec.y * vec.y);
			if (length == 0)
				return 0;
			else
			{
				double sign = vec.y >= 0 ? 1 : -1;
				return sign * Math.Acos(vec.x / length) * (180 / Math.PI);
			}
		}

		/// <summary>
		/// Compares two doubles with tolerance.
		/// Returns true if the values may be equal within the relative or absolute tolerance
		/// </summary>
		public static bool EqualsWithTolerance(this System.Double x, System.Double y, System.Double tolerance)
		{
			if (Double.IsInfinity(x) || Double.IsInfinity(y))
				return x == y;
			else
				return Math.Abs(x - y) <= tolerance * Math.Max(Math.Abs(x), 1.0);
		}

		/// <summary>
		/// Compares two nullable doubles with tolerance. Checks first on whether they are equal in
		/// terms of having a value, and second on the double values.
		/// Returns true if the values may be equal within the relative or absolute tolerance
		/// </summary>
		public static bool EqualsWithTolerance(this System.Double? x, System.Double? y, System.Double tolerance)
		{
			if (x.HasValue != y.HasValue)
				return false;
			else if (!x.HasValue)
				return true;
			else
				return EqualsWithTolerance(x.Value, y.Value, tolerance);
		}

		/// <summary>
		/// Returns 1.0 if x > 0, -1.0 if x &lt; 0, and 0 if x == 0.
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static System.Double Sign(this System.Double x)
		{
			if (x > 0)
				return 1.0;
			else if (x < 0)
				return -1.0;
			else
				return 0;
		}

		/// <summary>
		/// Compares two <see cref="Complex"/> numbers with tolerance.
		/// Returns true if both the real and imaginary part may be equal within the relative or absolute tolerance
		/// </summary>
		public static bool ComplexEqualsWithTolerance(this Complex x, Complex y, System.Double tolerance)
		{
			return x.Real.EqualsWithTolerance(y.Real, tolerance) && x.Imaginary.EqualsWithTolerance(y.Imaginary, tolerance);
		}

		/// <summary>
		/// Compares two doubles with tolerance.
		/// Returns true if this value is definitely less than the other value
		/// within the relative or absolute tolerance
		/// </summary>
		public static bool LessThanWithTolerance(this System.Double x, System.Double y, System.Double tolerance)
		{
			if (Double.IsInfinity(x) || Double.IsInfinity(y))
				return x < y;
			else
				return (y - x) > tolerance * Math.Max(Math.Abs(x), 1.0);
		}

		/// <summary>
		/// Compares two doubles with tolerance.
		/// </summary>
		public static bool LessOrEqualWithTolerance(this System.Double x, System.Double y, System.Double tolerance)
		{
			if (Double.IsInfinity(x) || Double.IsInfinity(y))
				return x <= y;
			else
				return x < y || EqualsWithTolerance(x, y, tolerance);
		}

		/// <summary>
		/// Compares two doubles with tolerance.
		/// Returns true if this value is definitely greater than the other value
		/// within the relative or absolute tolerance
		/// </summary>
		public static bool GreaterThanWithTolerance(this System.Double x, System.Double y, System.Double tolerance)
		{
			if (Double.IsInfinity(x) || Double.IsInfinity(y))
				return x > y;
			else
				return (x - y) > tolerance * Math.Max(Math.Abs(x), 1.0);
		}

		/// <summary>
		/// Compares two doubles with tolerance.
		/// </summary>
		public static bool GreaterOrEqualWithTolerance(this System.Double x, System.Double y, System.Double tolerance)
		{
			if (Double.IsInfinity(x) || Double.IsInfinity(y))
				return x >= y;
			else
				return x > y || EqualsWithTolerance(x, y, tolerance);
		}

		/// <summary>
		/// Checks whether value of double is infinity or NAN
		/// </summary>
		/// <param name="x">double to check</param>
		/// <returns>whether value of double is infinity or NAN</returns>
		public static bool IsNanOrInfinity(this System.Double x)
		{
			return (Double.IsInfinity(x) || Double.IsNaN(x));
		}

	}
}
