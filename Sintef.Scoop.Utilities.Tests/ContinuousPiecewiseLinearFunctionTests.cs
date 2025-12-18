//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sintef.Scoop.Utilities;
using Sintef.Scoop.Utilities.Functions;
using System;
using System.Linq;

namespace Sintef.Scoop.Utilities.Tests;

[TestClass]
public class ContinuousPiecewiseLinearFunctionTests
{
	[TestMethod]
	[ExpectedException(typeof(ArgumentException))]
	public void ArgumentsAndValuesMustHaveSameCount()
	{
		double[] x = new[] { 1.0, 2.0, 3.0 };
		double[] y = new[] { 1.0, 2.0 };

		ContinuousPiecewiseLinearFunction f = new(x, y);
	}

	[TestMethod]
	[ExpectedException(typeof(ArgumentException))]
	public void ArgumentsMustBeDistinct()
	{
		double[] x = new[] { 1.0, 2.0, 2.0 };
		double[] y = new[] { 1.0, 2.0, 3.0 };

		ContinuousPiecewiseLinearFunction f = new(x, y);
	}
	
	[TestMethod]
	[ExpectedException(typeof(ArgumentException))]
	public void ValuesMustBeFinite()
	{
		double[] x = new[] { 1.0, 2.0, 3.0 };
		double[] y = new[] { 1.0, double.NegativeInfinity, 3.0 };

		ContinuousPiecewiseLinearFunction f = new(x, y);
	}
	
	[TestMethod]
	[ExpectedException(typeof(ArgumentException))]
	public void ValuesMustBeDefined()
	{
		double[] x = new[] { 1.0, 2.0, 3.0 };
		double[] y = new[] { double.NaN, 2.0, 3.0 };

		ContinuousPiecewiseLinearFunction f = new(x, y);
	}
	
	[TestMethod]
	[ExpectedException(typeof(ArgumentException))]
	public void ArgumentsMustBeFinite()
	{
		double[] x = new[] { 1.0, 2.0, double.PositiveInfinity };
		double[] y = new[] { 1.0, 2.0, 3.0 };

		ContinuousPiecewiseLinearFunction f = new(x, y);
	}

	[TestMethod]
	[ExpectedException(typeof(ArgumentException))]
	public void ArgumentsMustBeDefined()
	{
		double[] x = new[] { 1.0, double.NaN, 3.0 };
		double[] y = new[] { 1.0, 2.0, 3.0 };

		ContinuousPiecewiseLinearFunction f = new(x, y);
	}

	[TestMethod]
	public void CanConstructFromTuples()
	{
		var breakpoints = new[] { (1.0, 1.0), (2.0, 2.0), (3.0, 3.0) };

		ContinuousPiecewiseLinearFunction f = new(breakpoints);
		
		Assert.AreEqual(1.0, f.MinArgument);
		Assert.AreEqual(3.0, f.MaxArgument);
		
		Assert.AreEqual(1.0, f.MinValue);
		Assert.AreEqual(3.0, f.MaxValue);
		
		Assert.AreEqual(1.0, f.Value(1.0));
		Assert.AreEqual(2.0, f.Value(2.0));
		Assert.AreEqual(3.0, f.Value(3.0));
	}
	
	[TestMethod]
	public void ValuesAreCalculatedCorrectly()
	{
		var (x, y) = SampleFunctionBreakpoints();

		ContinuousPiecewiseLinearFunction f = new(x, y);

		var (intermediateInputs,  expectedIntermediateOutputs) = SampleIntermediateValues();

		var testInputs = x.Concat(intermediateInputs).ToArray();
		var expectedOutputs = y.Concat(expectedIntermediateOutputs).ToArray();
		
		for (int i = 0; i < testInputs.Length; ++i)
		{
			Assert.AreEqual(expectedOutputs[i], f.Value(testInputs[i]), 1E-14);
		}
	}

	[TestMethod]
	public void InputOutOfRangeIsHandled()
	{
		var (x, y) = SampleFunctionBreakpoints();

		ContinuousPiecewiseLinearFunction f = new(x, y);

		double[] outOfRangeInputs = new[] { double.NaN, Double.PositiveInfinity, -3.0, 3.00000001, double.MaxValue };

		foreach (var input in outOfRangeInputs)
		{
			Assert.AreEqual(double.NaN, f.Value(input));
		}
	}

	[TestMethod]
	public void InverseFunctionWorksAsIntended()
	{
		var (x, y) = SampleFunctionBreakpoints();

		ContinuousPiecewiseLinearFunction f = new(x, y);
		
		Assert.IsTrue(f.IsIncreasing);
		Assert.IsFalse(f.IsDecreasing);

		var fi = f.InverseFunction;
		
		Assert.IsNotNull(fi);
		Assert.IsTrue(fi.IsIncreasing);
		Assert.IsFalse(fi.IsDecreasing);

		var (intermediateInputs,  expectedIntermediateOutputs) = SampleIntermediateValues();

		var testInputs = x.Concat(intermediateInputs).ToArray();
		var expectedOutputs = y.Concat(expectedIntermediateOutputs).ToArray();
		
		for (int i = 0; i < testInputs.Length; ++i)
		{
			Assert.AreEqual(testInputs[i], fi.Value(expectedOutputs[i]), 1E-14);
		}
	}

	[TestMethod]
	public void DecreasingFunctionIsDetected()
	{
		var (x, y) = SampleFunctionBreakpoints();

		x = x.Reverse().ToArray();
		
		ContinuousPiecewiseLinearFunction f = new(x, y);
		
		Assert.IsFalse(f.IsIncreasing);
		Assert.IsTrue(f.IsDecreasing);
		Assert.IsNotNull(f.InverseFunction);
	}
	
	[TestMethod]
	public void NonMonotonicFunctionIsProperlyDetected()
	{
		var x = new[] { 0.0, 1.0, 2.0, 3.0, 4.0 };
		var y = new[] { 0.0, 2.0, 8.0, 5.0, 3.0 };
		
		ContinuousPiecewiseLinearFunction f = new(x, y);
		
		Assert.IsFalse(f.IsIncreasing);
		Assert.IsFalse(f.IsDecreasing);
		Assert.IsNull(f.InverseFunction);
		
		Assert.IsTrue(double.IsNaN(f.Value(8.00000001)));
		Assert.IsTrue(double.IsNaN(f.Value(-0.00000001)));
		
		for (int i = 0; i < x.Length; ++i)
		{
			Assert.AreEqual(y[i], f.Value(x[i]), 1E-14);
		}
	}

	[TestMethod]
	public void MaxAndMinValueAndArgumentsAreCorrect()
	{
		var x = new[] { 0.0, 1.0, 2.0, 3.0, 4.0 };
		var y = new[] { 0.0, 2.0, 8.0, 5.0, 3.0 };
		
		ContinuousPiecewiseLinearFunction f = new(x, y);

		Assert.AreEqual(0.0, f.MinValue);
		Assert.AreEqual(8.0, f.MaxValue);
		Assert.AreEqual(0.0, f.MinArgument);
		Assert.AreEqual(4.0, f.MaxArgument);
	}

	[TestMethod]
	public void SingleArgumentFunctionIsSupported()
	{
		var x = new[] { 1.0 };
		var y = new[] { 42.0 };
		
		ContinuousPiecewiseLinearFunction f = new(x, y);
		
		Assert.AreEqual(42.0, f.Value(1.0));

		var illegalXValues = new[] { 1.000000001, 0.99999999, 2, -2 };

		foreach (var argument in illegalXValues)
		{
			var value = f.Value(argument);
			Assert.IsTrue(double.IsNaN(value));
		}
	}

	[TestMethod]
	[ExpectedException(typeof(ArgumentException))]
	public void UndefinedFunctionNotSupported()
	{
		ContinuousPiecewiseLinearFunction f = new(Array.Empty<double>(), Array.Empty<double>());
	}

	[TestMethod]
	public void BreakPointsAreReportedCorrectly()
	{
		// Breakpoints ordered in increasing argument value
		var x = new[] { 0.0, 1.0, 2.0, 3.0, 4.0 };
		var y = new[] { 0.0, 2.0, 8.0, 5.0, 3.0 };

		ContinuousPiecewiseLinearFunction f = new(x, y);
		
		Assert.AreEqual(f.BreakPointArguments.Length, f.BreakPointValues.Length);
		
		// Check that the breakpoints are the same and in same order
		CollectionAssert.AreEqual(x, f.BreakPointArguments.ToArray());
		CollectionAssert.AreEqual(y, f.BreakPointValues.ToArray());

		// Define the breakpoints in a random order
		x = new[] { 3.0, 0.0, 1.0, 4.0, 2.0 };
		y = new[] { 5.0, 0.0, 2.0, 3.0, 8.0 };
		
		f = new(x, y);

		Assert.AreEqual(f.BreakPointArguments.Length, f.BreakPointValues.Length);
		
		// Check that the breakpoints are the same but not necessarily in the same order
		CollectionAssert.AreEquivalent(x, f.BreakPointArguments.ToArray());
		CollectionAssert.AreEquivalent(y, f.BreakPointValues.ToArray());
		
		// Check that they are ordered at increasing argument value
		for (int i = 1; i < f.BreakPointArguments.Length; ++i)
		{
			Assert.IsTrue(f.BreakPointArguments[i] > f.BreakPointArguments[i - 1]);
		}
		
		// Check that both collections are ordered correspondingly, ie same index refers to same breakpoint
		for (int i = 0; i < f.BreakPointArguments.Length; ++i)
		{
			var index = x.IndexOf(f.BreakPointArguments[i]);
			Assert.AreEqual(y[index], f.BreakPointValues[i]);
		}
	}
	
	/// <summary>
	/// Returns breakpoints for a sample test function.
	/// </summary>
	private static (double[], double[]) SampleFunctionBreakpoints()
	{
		return (new[] { 0.0, 1.0, 2.0, 3.0 }, new[] { 0.0, 1.0, 4.0, 16.0 });
	}

	/// <summary>
	/// Returns some sample inputs and corresponding values outside the breakpoints for the sample function returned by
	/// <see cref="SampleFunctionBreakpoints"/>.
	/// </summary>
	private static (double[], double[]) SampleIntermediateValues()
	{
		return (new[] { 0.5, 1.5, 2.5 }, new[] { 0.5, 2.5, 10 });
	}
}