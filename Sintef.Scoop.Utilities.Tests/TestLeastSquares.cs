//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities.Tests
{
	/// <summary>
	/// Tests for gathering statistics on optimizer performance
	/// </summary>
	[TestClass]
	public class TestLeastSquares
	{
		public class RunParameters
		{
			public double StartValue { get; set; }
			public double ConvergeValue { get; set; }
			public double HalfTime { get; set; }
			public int NPoints { get; set; }

			public double Decay { get { return Math.Log(2) / HalfTime; } }
			public override string ToString()
			{
				return string.Format("{0} / {1}", HalfTime, ConvergeValue);
			}
		}

		List<RunParameters> testData;

		[TestInitialize]
		public void Setup()
		{
			// Create some fixed parameter sets to cover simple variations

			testData = new List<RunParameters> {
				new RunParameters { StartValue = 10, ConvergeValue = 0, HalfTime = 2, NPoints = 10 },
				new RunParameters{ StartValue = 100, ConvergeValue = 0, HalfTime = 2, NPoints = 10 },
				new RunParameters{ StartValue = 10, ConvergeValue = 5, HalfTime = 2, NPoints = 10 },
				new RunParameters{ StartValue = 10, ConvergeValue = 0, HalfTime = 4, NPoints = 10 },
				new RunParameters{ StartValue = 10, ConvergeValue = 0, HalfTime = 2, NPoints = 3 },
			};

			// Add randomly generated parameter sets

			Random r = new Random(67);
			for (int i = 0; i < 100; ++i)
			{
				RunParameters randomParameters = new RunParameters
				{
					ConvergeValue = -100 + 200 * r.NextDouble(),
					HalfTime = 0.1 + 10 * r.NextDouble(),
					NPoints = 3 + r.Next(10)
				};
				randomParameters.StartValue = randomParameters.ConvergeValue + 100 * r.NextDouble();

				testData.Add(randomParameters);
			}
		}

		[TestMethod]
		public void IdealExponentialIsEstimatedCorrectly()
		{
			foreach (var test in testData)
			{
				DataPoints data = ExponentialSequence(test);

				Assert.AreEqual(test.StartValue, data.First().Y);

				ExponentialFunction estimate = LeastSquares.BestExponentialFit(data);

				Assert.AreEqual(test.ConvergeValue, estimate.ValueAtInfinity, 1e-5);
				Assert.AreEqual(test.Decay, estimate.Decay, 1e-8);
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void ExponentialFitRequiresThreeXValues()
		{
			var testData = DataPoints.Points(0, 5, 0, 3, 1, 3);

			var f = LeastSquares.BestExponentialFit(testData);
		}

		[TestMethod]
		public void ExponentialHalfTimeCannotBeTooShort()
		{
			// This test data is best approximated using a very large decay, i.e. short half interval

			var testData = DataPoints.Points(0, 5, 0.001, 0, 2, 0);

			var f = LeastSquares.BestExponentialFit(testData);

			// We don't allow halving intervals shorter than 1% of the data domain length

			Assert.AreEqual(0.01 * testData.XRange, f.HalfTime);
		}

		[TestMethod]
		public void ExponentialHalfTimeCannotBeTooLong()
		{
			// This test data is best approximated using an infinitesimal decay, i.e. the limit to a linear function

			var testData = DataPoints.Points(0, 4, 1, 2, 2, 0);

			var f = LeastSquares.BestExponentialFit(testData);

			// We don't allow halving intervals longer than 100 times the data domain length

			Assert.AreEqual(100 * testData.XRange, f.HalfTime);
		}

		[TestMethod]
		public void LinearRegressionWorks()
		{
			DataPoints data = DataPoints.Points(0, 0, 1, 1);

			// y = ax + c
			double a, c;

			LeastSquares.BestLinearFit(data, out a, out c);

			Assert.AreEqual(0, c);
			Assert.AreEqual(1, a);

			data = DataPoints.Points(0, 1, 1, 0);

			LeastSquares.BestLinearFit(data, out a, out c);

			Assert.AreEqual(1, c);
			Assert.AreEqual(-1, a);

			data = DataPoints.Points(0, 10, 1, 9, 2, 9, 3, 8);

			LeastSquares.BestLinearFit(data, out a, out c);

			Assert.AreEqual(9.9, c);
			Assert.AreEqual(-0.6, a);
		}

		[TestMethod]
		public void ExponentialFitErrorIsCorrectForIdealData()
		{
			foreach (var test in testData)
			{
				DataPoints data = ExponentialSequence(test);

				// The error is zero for the correct b

				double correctB = test.Decay;
				double error = LeastSquares.ExpFitError(data, correctB);

				Assert.AreEqual(0, error, 1e-11);

				// Increasing b increases the error

				double error1 = LeastSquares.ExpFitError(data, correctB * 1.1);
				double error2 = LeastSquares.ExpFitError(data, correctB * 1.2);
				Assert.AreNotEqual(0, error1, 1e-10);
				Assert.IsTrue(error2 > error1);

				// Decreasing b increases the error

				error1 = LeastSquares.ExpFitError(data, correctB * 0.9);
				error2 = LeastSquares.ExpFitError(data, correctB * 0.8);
				Assert.AreNotEqual(0, error1, 1e-10);
				Assert.IsTrue(error2 > error1);

			}
		}

		[TestMethod]
		public void DerivativeOfExponentialFitErrorIsCorrect()
		{
			foreach (var test in testData)
			{
				DataPoints data = ExponentialSequence(test);

				double testB0 = test.Decay;
				double testB1 = testB0 * 1.01;

				VerifyDerivativeOfExponentialFitError(data, testB0, testB1);

				VerifyDerivativeOfExponentialFitError(data, testB0 / 10, testB1 / 10);

				VerifyDerivativeOfExponentialFitError(data, testB0 * 10, testB1 * 10);
			}
		}

		private static void VerifyDerivativeOfExponentialFitError(DataPoints data, double testB0, double testB1)
		{
			double error0 = LeastSquares.ExpFitError(data, testB0);
			double error1 = LeastSquares.ExpFitError(data, testB1);

			double errorDerivative0 = LeastSquares.DerivativeOfExpFitError(data, testB0);
			double errorDerivative1 = LeastSquares.DerivativeOfExpFitError(data, testB1);

			double minDerivative = Math.Min(errorDerivative0, errorDerivative1);
			double maxDerivative = Math.Max(errorDerivative0, errorDerivative1);

			double deltaB = testB1 - testB0;

			Assert.IsTrue(error1 >= error0 + deltaB * minDerivative - 0.01 * Math.Abs(deltaB * minDerivative));
			Assert.IsTrue(error1 <= error0 + deltaB * maxDerivative + 0.01 * Math.Abs(deltaB * maxDerivative));
		}

		[TestMethod]
		public void ExponentialFunctionWorks()
		{
			ExponentialFunction f = ExponentialFunction.FromHalfTime(valueAtZero: 10, valueAtInfinity: 2, halfTime: 2);

			Assert.AreEqual(10, f.Value(0));
			Assert.AreEqual(6, f.Value(2));
			Assert.AreEqual(4, f.Value(4));
			Assert.AreEqual(3, f.Value(6));
			Assert.AreEqual(2, f.Value(1000));
		}

		/// <summary>
		/// Creates an exponentional sequence of points based on the given parameters
		/// </summary>
		public static DataPoints ExponentialSequence(RunParameters test, double randomOffsetScale = 0)
		{
			ExponentialFunction f = ExponentialFunction.FromHalfTime(test.StartValue, test.ConvergeValue, test.HalfTime);

			return DataPoints.Sample(f.Value, test.NPoints, randomOffsetScale);
		}

		/// <summary>
		/// Creates a linear sequence of points
		/// </summary>
		private static DataPoints LinearSequence(double valueAtZero, double gradient, int nPoints)
		{
			var f = new LinearFunction(valueAtZero, gradient);

			return DataPoints.Sample(f.Value, 10);
		}

	}
}