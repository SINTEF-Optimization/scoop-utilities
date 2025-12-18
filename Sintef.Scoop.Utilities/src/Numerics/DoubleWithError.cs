//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// A Double whose error is tracked
	/// </summary>
	/// <remarks>
	/// Floating point number accumulate errors, especially when two numbers of similar
	/// magnitude are subtracted. This struct represents a floating point number and keeps
	/// track of how accurate the value is.
	/// </remarks>
	public struct DoubleWithError
	{
		#region Private data members

		/// <summary>
		/// The smallest relative error we can expect
		/// </summary>
		static readonly double _machineError = 1.11e-16;

		/// <summary>
		/// The value
		/// </summary>
		double _value;

		#endregion

		/// <summary>
		/// The value. Setting this member produces a number whose error is
		/// minimal
		/// </summary>
		public double Value
		{
			get { return _value; }
			set { _value = value; Error = ErrorFor(value); }
		}

		/// <summary>
		/// The maximal absolute difference between the value and the "true" number
		/// </summary>
		public double Error { get; private set; }

		/// <summary>
		/// The maximal error, relative to the magnitude of the value.
		/// Special cases: If the value is 0, returns double.Inifinity, unless
		/// the error also is 0, in which case the property returns 0.
		/// </summary>
		public double RelativeError
		{
			get
			{
				if (_value == 0.0)
				{
					if (Error == 0.0)
						return 0.0;
					else
						return double.PositiveInfinity;
				}
				else
					return Error / Math.Abs(_value);
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="value">The value</param>
		/// <param name="error">The error</param>
		private DoubleWithError(double value, double error)
		{
			_value = value;
			Error = Math.Abs(error);
		}

		/// <summary>
		/// Constructor. Creates a value with minimal error.
		/// </summary>
		/// <param name="value">The value</param>
		public DoubleWithError(double value)
			: this(value, ErrorFor(value))
		{
		}

		/// <summary>
		/// Returns a DoubleWithError that could be anything, i.e. the error is infinite.
		/// </summary>
		public static DoubleWithError Anything { get { return new DoubleWithError(0, double.PositiveInfinity); } }

		/// <summary>
		/// Adds two numbers with error
		/// </summary>
		static public DoubleWithError operator +(DoubleWithError one, DoubleWithError other)
		{
			double result = one._value + other._value;

			if (double.IsNaN(result))
				// Adding opposite infinities
				return Anything;
			if (double.IsInfinity(result))
				return new DoubleWithError(result, 0);

			return new DoubleWithError(result, one.Error + other.Error + ErrorFor(result));
		}

		/// <summary>
		/// Adds two numbers with error
		/// </summary>
		static public DoubleWithError operator +(DoubleWithError one, double other)
		{
			return one + new DoubleWithError(other);
		}

		/// <summary>
		/// Subtracts two numbers with error
		/// </summary>
		static public DoubleWithError operator -(DoubleWithError one, DoubleWithError other)
		{
			double result = one._value - other._value;

			if (double.IsNaN(result))
				// Subtracting equal infinities
				return Anything;
			if (double.IsInfinity(result))
				return new DoubleWithError(result, 0);

			return new DoubleWithError(result, one.Error + other.Error + ErrorFor(result));
		}

		/// <summary>
		/// Subtracts two numbers with error
		/// </summary>
		static public DoubleWithError operator -(DoubleWithError one, double other)
		{
			return one - new DoubleWithError(other);
		}

		/// <summary>
		/// Returns true if the numbers values could be equal, within their errors
		/// </summary>
		public bool EqualsWithError(DoubleWithError other)
		{
			double diff = Math.Abs(_value - other._value);
			double err = Error + other.Error;
			return diff <= err;
		}

		/// <summary>
		/// Returns a comparer that considers two values equal if
		/// their absolute difference is no larger than their combined errors
		/// </summary>
		public static IEqualityComparer<DoubleWithError> EqualWithinError
		{
			get { return new EqualWithinError(); }
		}

		/// <summary>
		/// Returns true if the numbers are equal within the error
		/// </summary>
		public bool EqualsWithError(double other)
		{
			return EqualsWithError(new DoubleWithError(other));
		}

		/// <summary>
		/// Returns the error to assume for the given value
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private static double ErrorFor(double value)
		{
			if (double.IsInfinity(value))
				return 0;

			return 2 * _machineError * Math.Abs(value);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Value}±{Error}";
		}
	}

	/// <summary>
	/// A comparer that considers two values equal if
	/// their absolute difference is no larger than their combined errors
	/// </summary>
	public class EqualWithinError : IEqualityComparer<DoubleWithError>
	{
		#region IEqualityComparer<DoubleWithError> Members

		/// <summary>
		/// Returns true if the two values could be equal, within their errors
		/// </summary>
		public bool Equals(DoubleWithError x, DoubleWithError y)
		{
			return x.EqualsWithError(y);
		}

		/// <summary>
		/// Not implemented
		/// </summary>
		public int GetHashCode(DoubleWithError obj)
		{
			throw new NotImplementedException("GetHashCode");
		}

		#endregion
	}


	public static partial class Extensions
	{
		/// <summary>
		/// Converts a double to a DoubleWithError with the minimal error
		/// </summary>
		public static DoubleWithError WithError(this double value)
		{
			return new DoubleWithError(value);
		}

		/// <summary>
		/// Returns the sum of the given sequence
		/// </summary>
		public static DoubleWithError Sum(this IEnumerable<DoubleWithError> source)
		{
			DoubleWithError sum = new DoubleWithError(0);
			foreach (var item in source)
				sum += item;
			return sum;
		}

		/// <summary>
		/// Returns the sum of the values that are obtained
		/// by invoking a transform function on each element of the input sequence.		
		/// </summary>
		public static DoubleWithError Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, DoubleWithError> selector)
		{
			DoubleWithError sum = new DoubleWithError(0);
			foreach (var item in source)
				sum += selector(item);
			return sum;
		}
	}
}
