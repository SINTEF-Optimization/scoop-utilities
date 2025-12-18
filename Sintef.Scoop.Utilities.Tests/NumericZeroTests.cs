//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Sintef.Scoop.Utilities.Tests
{
	[TestClass]
	public class NumericZeroTests
	{
		Func<double, double> Identity = x => x;
		Func<double, double> Zero = x => 0;
		Func<double, double> Sigmoid = x => Math.Atan(x);
		Func<double, double> OffsetSigmoid = x => Math.Atan(x - 4.56);

		Func<double, double> PiecewiseLinear = x => Math.Max(x, -1e-6);

		Func<double, double> ExponentialSigmoid = x => Math.Atan(x) * Math.Exp(-x * x);

		Func<double, double> Step = x => x < 0 ? -1 : 1;

		Func<double, double> StepAt(double y) => (x => x <= y ? -1 : 1);


		[TestMethod]
		public void IdentityHasZeroAtZero()
		{
			double zero = NumericZero.ZeroOf(Identity, -1, 2);

			Assert.AreEqual(0, zero);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void XMinCannotBeGreaterThanXMax()
		{
			double zero = NumericZero.ZeroOf(Zero, 2, 1);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void ValueAtXMinMustBeNegative()
		{
			double zero = NumericZero.ZeroOf(Identity, 1, 2);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void ValueAtXMaxMustBePositive()
		{
			double zero = NumericZero.ZeroOf(Identity, -2, -1);
		}

		[TestMethod]
		public void SigmoidHasZeroAtZero()
		{
			double zero = NumericZero.ZeroOf(Sigmoid, -10, 20);

			Assert.AreEqual(0, zero);
		}

		[TestMethod]
		public void OffsetSigmoidHasKnownZero()
		{
			double zero = NumericZero.ZeroOf(Show(OffsetSigmoid), -10, 20);

			Assert.AreEqual(4.56, zero);
		}

		[TestMethod]
		public void PiecewiseLinearHasZeroAtZero()
		{
			double zero = NumericZero.ZeroOf(Show(LimitCalls(PiecewiseLinear, 50)), -10, 20);

			Assert.AreEqual(0, zero);
		}

		[TestMethod]
		public void ExponentialSigmoidHasZeroAtZero()
		{
			double zero = NumericZero.ZeroOf(Show(ExponentialSigmoid), -12.938726, 20);

			Assert.AreEqual(0, zero);
		}

		[TestMethod]
		public void StepTransitionsAtZero()
		{
			double zero = NumericZero.ZeroOf(LimitCalls(Step, 1000), -1, 1.2, argumentTolerance: 1e-6);

			Assert.AreEqual(0, zero, 1e-6);
		}

		[TestMethod]
		[ExpectedException(typeof(Exception))]
		public void NoZeroGivesException()
		{
			// There is no zero within the given tolerances
			double zero = NumericZero.ZeroOf(StepAt(1e40), 1e30, 1e42, argumentTolerance: 1);
		}

		[TestMethod]
		public void NoZeroSucceedsAtNumericalPrecision()
		{
			Random r = new Random(64);

			for (int i = 0; i < 500; ++i)
			{
				double stepValue = Math.Exp(100 * (1 - r.NextDouble() * 2));
				double zero = NumericZero.ZeroOf(StepAt(stepValue), stepValue / 7, stepValue * 13, throwOnFailure: false);

				Assert.AreEqual(zero, stepValue);
			}
		}

		private Func<double, double> LimitCalls(Func<double, double> function, int maxCalls)
		{
			int calls = 0;
			return (x) =>
			{
				++calls;
				Assert.IsTrue(calls <= maxCalls, "Max number of function invocations exceeded");
				return function(x);
			};
		}

		private Func<double, double> Show(Func<double, double> function)
		{
			return (x) =>
			{
				double value = function(x);
				Console.WriteLine("{0}: {1}", x, value);
				return value;
			};
		}


	}
}
