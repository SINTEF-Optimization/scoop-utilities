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
	/// An implementation of a continuous piecewise linear function which aims to be efficient at calculating function
	/// values or inverse function values.
	/// </summary>
	public class ContinuousPiecewiseLinearFunction
	{

		/// <summary>
		/// Constructs a new piecewise linear function from the given set of inputs <paramref name="x"/> and values
		/// <paramref name="y"/>. There must be an exact match between inputs and values and together they constitute
		/// the breakpoints of the function.
		///
		/// There must be at least one breakpoint, otherwise the function is undefined.
		/// </summary>
		public ContinuousPiecewiseLinearFunction(IEnumerable<double> x, IEnumerable<double> y)
		{
			_arguments = x.ToArray();
			_values = y.ToArray();

			Initialize(true);
		}

		/// <summary>
		/// Private constructor optionally not creating the inverse.
		/// </summary>
		private ContinuousPiecewiseLinearFunction(IEnumerable<double> x, IEnumerable<double> y, bool makeInverse)
		{
			_arguments = x.ToArray();
			_values = y.ToArray();

			Initialize(makeInverse);
		}

		/// <summary>
		/// Constructs a new piecewise linear function from the given breakpoints. In each breakpoint, X is the input
		/// and Y is the value.
		///
		/// There must be at least one breakpoint, otherwise the function is undefined.
		/// </summary>
		public ContinuousPiecewiseLinearFunction(IEnumerable<(double X, double Y)> breakPoints)
		{
			// ReSharper disable once PossibleMultipleEnumeration
			var size = breakPoints.Count();
			_arguments = new double[size];
			_values = new double[size];

			int i = 0;
			// ReSharper disable once PossibleMultipleEnumeration
			foreach (var (x, y) in breakPoints)
			{
				_arguments[i] = x;
				_values[i++] = y;
			}

			Initialize(true);
		}

		/// <summary>
		/// Set to true if the function is monotonously increasing in the defined interval.
		/// </summary>
		public bool IsIncreasing { get; private set; }

		/// <summary>
		/// Set to true if the function is monotonously decreasing in the defined interval.
		/// </summary>
		public bool IsDecreasing { get; private set; }

		/// <summary>
		/// The minimum value of this function.
		/// </summary>
		public double MinValue { get; private set; } = double.PositiveInfinity;

		/// <summary>
		/// The maximum value of this function.
		/// </summary>
		public double MaxValue { get; private set; } = double.NegativeInfinity;

		/// <summary>
		/// The minimal argument value for which this function is defined.
		/// </summary>
		public double MinArgument { get; private set; } = double.PositiveInfinity;

		/// <summary>
		/// The maximum argument value for which this function is defined.
		/// </summary>
		public double MaxArgument { get; private set; } = double.NegativeInfinity;

		/// <summary>
		/// The values at the breakpoints of the function. The values are ordered identically as the arguments in 
		/// <see cref="BreakPointArguments"/>, so using the same index in both collections gives you the value and
		/// argument for the same breakpoint.
		/// </summary>
		public ReadOnlySpan<double> BreakPointValues => _values;

		/// <summary>
		/// The arguments at the breakpoints of the function. These will be ordered by increasingly. The values will be
		/// ordered correspondingly in <see cref="BreakPointValues"/>, so using the same index in both collections gives
		/// you the value and argument for the same breakpoint.
		/// </summary>
		public ReadOnlySpan<double> BreakPointArguments => _arguments;

		/// <summary>
		/// Returns the value of the function for the given input value. If the given input value is outside the range
		/// of the function then Double.NaN is returned. 
		/// </summary>
		public double Value(double x)
		{
			if (x.IsNanOrInfinity())
			{
				return double.NaN;
			}
			int indexBeforeOrAt = IndexBeforeOrAt(x);
			if (indexBeforeOrAt < 0)
			{
				// Return NaN if x is outside the defined range of the function.
				return double.NaN;
			}

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (_arguments[indexBeforeOrAt] == x)
			{
				return _values[indexBeforeOrAt];
			}

			var xBefore = _arguments[indexBeforeOrAt];
			var xAfter = _arguments[indexBeforeOrAt + 1];
			var yBefore = _values[indexBeforeOrAt];
			var yAfter = _values[indexBeforeOrAt + 1];

			var t = (x - xBefore) / (xAfter - xBefore);

			return yBefore + t * (yAfter - yBefore);
		}

		/// <summary>
		/// If the function is monotonously increasing or decreasing this is the inverse function. Otherwise this will be
		/// null.
		/// </summary>
		public ContinuousPiecewiseLinearFunction InverseFunction => _inverse;

		/// <summary>
		/// Internal initializer during construction. This assumes <see cref="_arguments"/> and <see cref="_values"/>
		/// are already initialized and based on that it initializes the rest of the instance.
		/// </summary>
		/// <exception cref="ArgumentException"></exception>
		private void Initialize(bool makeInverse)
		{
			var size = _values.Length;
			if (_arguments.Length != size)
			{
				throw new ArgumentException(
					"A piecewise linear function requires same number of values and arguments");
			}
			if (_arguments.Length == 0)
			{
				throw new ArgumentException("At least one breakpoint is required for the function to be defined");
			}

			Array.Sort(_arguments, _values);

			var prevX = double.PositiveInfinity;
			foreach (var x in _arguments)
			{
				if (x < MinArgument)
				{
					MinArgument = x;
				}

				if (x > MaxArgument)
				{
					MaxArgument = x;
				}

				if (x.IsNanOrInfinity())
				{
					throw new ArgumentException("NaN or infinity is not supported");
				}

				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (x == prevX)
				{
					throw new ArgumentException("Multiple values for same input is not supported");
				}

				prevX = x;
			}

			foreach (var y in _values)
			{
				if (y < MinValue)
				{
					MinValue = y;
				}

				if (y > MaxValue)
				{
					MaxValue = y;
				}

				if (y.IsNanOrInfinity())
				{
					throw new ArgumentException("NaN or infinity is not supported");
				}
			}

			// Determine if the function is monotonic

			IsIncreasing = true;
			IsDecreasing = true;

			for (int i = 0; i < size; ++i)
			{
				if (i > 0)
				{
					if (_values[i] >= _values[i - 1])
					{
						IsDecreasing = false;
					}
					else if (_values[i] <= _values[i - 1])
					{
						IsIncreasing = false;
					}
				}
			}

			// Determine inverse function if the function is monotonic
			if ((IsIncreasing || IsDecreasing) && makeInverse)
			{
				_inverse = new ContinuousPiecewiseLinearFunction(_values, _arguments, false)
				{
					_inverse = this
				};
			}
		}

		/// <summary>
		/// If the function is defined for the given input value then this returns the index of breakpoint before or at
		/// the given input. If the function is not defined for this input, a negative number is returned.
		/// </summary>
		private int IndexBeforeOrAt(double x)
		{
			var len = _arguments.Length;
			if (x < _arguments[0] || x > _arguments[len - 1])
			{
				return -1;
			}

			int minIndex = 0;
			int maxIndex = len - 1;

			while (true)
			{
				if (maxIndex - minIndex <= 1)
				{
					// ReSharper disable once CompareOfFloatsByEqualityOperator
					if (_arguments[maxIndex] == x)
					{
						return maxIndex;
					}

					return minIndex;
				}

				int mid = (minIndex + maxIndex) >> 1;

				if (_arguments[mid] > x)
				{
					maxIndex = mid;
					continue;
				}

				minIndex = mid;
			}
		}

		/// <summary>
		/// The argument (x) values of the break points, in increasing order.
		/// </summary>
		private readonly double[] _arguments;

		/// <summary>
		/// The values (y) of the break points, each value at the corresponding index as in <see cref="_arguments"/>.
		/// </summary>
		private readonly double[] _values;

		/// <summary>
		/// The inverse function if it exists, otherwise null.
		/// </summary>
		private ContinuousPiecewiseLinearFunction _inverse;
	}
}