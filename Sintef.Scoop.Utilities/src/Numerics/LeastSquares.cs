//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Functions for least squares regression for linear and
	/// exponentially decaying functions
	/// </summary>
	public static class LeastSquares
	{
		/// <summary>
		/// Returns the exponential decay function that minimzes the sum of squares of the
		/// vertial errors to the given points.
		/// </summary>
		/// <param name="data">The data to fit</param>
		/// <returns></returns>
		public static ExponentialFunction BestExponentialFit(DataPoints data)
		{
			if (data.Select(pt => pt.X).Distinct().Count() < 3)
				throw new ArgumentException("Cannot fit an exponential to less than 3 points with different x");

			double minHalfRange = data.XRange / 100;
			double maxHalfRange = data.XRange * 100;

			double minB = Math.Log(2) / maxHalfRange;
			double maxB = Math.Log(2) / minHalfRange;

			Func<double, double> errorDerivative = (double trialB) => DerivativeOfExpFitError(data, trialB);

			double bestB;

			if (errorDerivative(maxB) <= 0)
				bestB = maxB;
			else if (errorDerivative(minB) >= 0)
				bestB = minB;
			else
				bestB = NumericZero.ZeroOf(errorDerivative, minB, maxB, argumentTolerance: maxB * 1e-10);

			double a, c;

			ExpFitError(data, bestB, out a, out c);

			return new ExponentialFunction(10, c, bestB);
		}

		/// <summary>
		/// Calculates the parameters for the linear function that minimizes the sum of squares of the
		/// vertial errors to the given points.
		/// 
		/// The function has the form a * x + b.
		/// </summary>
		/// <param name="data">The data to fit</param>
		public static LinearFunction BestLinearFit(DataPoints data)
		{
			double a, b;

			BestLinearFit(data, out a, out b);

			return new LinearFunction(b, a);
		}

		/// <summary>
		/// Calculates the parameters for the linear function that minimizes the sum of squares of the
		/// vertial errors to the given points.
		/// 
		/// The function has the form a * x + c.
		/// </summary>
		/// <param name="data">The data to fit</param>
		/// <param name="a">On exit, the best value for a</param>
		/// <param name="c">On exit, the best value for c</param>
		public static void BestLinearFit(DataPoints data, out double a, out double c)
		{
			// The formulas used here can be looked up in a textbook

			int n = data.Count();

			double sumX = data.Sum(d => d.X);
			double sumXX = data.Sum(d => d.X * d.X);
			double sumY = data.Sum(d => d.Y);
			double sumYY = data.Sum(d => d.Y * d.Y);
			double sumXY = data.Sum(d => d.X * d.Y);

			double u_a = n * sumXY - sumX * sumY;
			double u_c = sumY * sumXX - sumXY * sumX;
			double v = n * sumXX - sumX * sumX;

			a = u_a / v;
			c = u_c / v;
		}

		/// <summary>
		/// Returns the error (sum of squared deviations) of the best exponential decay fit
		/// for a fixed decay parameter
		/// </summary>
		/// <param name="data">The data to fit</param>
		/// <param name="b">The decay parameter to use</param>
		/// <returns></returns>
		public static double ExpFitError(DataPoints data, double b)
		{
			double a, c;
			return ExpFitError(data, b, out a, out c);
		}

		/// <summary>
		/// Returns the error (sum of squared deviations) of the best exponential decay fit
		/// for a fixed decay parameter
		/// </summary>
		/// <param name="data">The data to fit</param>
		/// <param name="b">The decay parameter to use</param>
		/// <param name="a"></param>
		/// <param name="c"></param>
		/// <returns></returns>
		private static double ExpFitError(DataPoints data, double b, out double a, out double c)
		{
			int n = data.Count();

			// First, transform the x values to z = exp(-b * x)

			Func<DataPoint, double> Z = d => Math.Exp(-b * d.X);
			Func<DataPoint, double> Y = d => d.Y;

			// Adjust data by subtracting means of z and y

			double yMean = data.Sum(d => Y(d)) / data.Count();
			double zMean = data.Sum(d => Z(d)) / data.Count();

			Func<DataPoint, double> Za = d => Math.Exp(-b * d.X) - zMean;
			Func<DataPoint, double> Ya = d => d.Y - yMean;

			// Find a and c using standard linear least squares between z and y

			double sumZZ = data.Sum(d => Za(d) * Za(d));
			double sumYY = data.Sum(d => Ya(d) * Ya(d));
			double sumZY = data.Sum(d => Za(d) * Ya(d));

			double u_a = n * sumZY;
			double v = n * sumZZ;

			a = u_a / v;
			c = 0;

			// Compute the error

			double E = sumYY - 2 * a * sumZY + a * a * sumZZ;

			// Adjust c to compensate for subtracting means. a is unchanged by adjustment

			c += yMean - a * zMean;

			return E;
		}

		/// <summary>
		/// Returns the derivative of ExpFitError(data, b) with respect to b
		/// </summary>
		public static double DerivativeOfExpFitError(DataPoints data, double b)
		{
			// Redo the ExpFitErrorUsingZeroMean calculation

			int n = data.Count();

			Func<DataPoint, double> Z = d => Math.Exp(-b * d.X);
			Func<DataPoint, double> Y = d => d.Y;

			double yMean = data.Sum(d => Y(d)) / data.Count();
			double zMean = data.Sum(d => Z(d)) / data.Count();

			Func<DataPoint, double> Za = d => Math.Exp(-b * d.X) - zMean;
			Func<DataPoint, double> Ya = d => d.Y - yMean;

			double sumZZ = data.Sum(d => Za(d) * Za(d));
			double sumYY = data.Sum(d => Ya(d) * Ya(d));
			double sumZY = data.Sum(d => Za(d) * Ya(d));

			double u_a = n * sumZY;
			double v = n * sumZZ;

			double a = u_a / v;

			// double E = sumYY - 2 * a * sumZY + a * a * sumZZ;

			// Calculate the derivative wrt. b, using product, division and chain rules.
			// The variable name convention is that p means 'primed', i.e. derivative wrt. b
			// sumZpZ means Sum(dz/db * z), while sumZSqp means Sum(d/db (z*z)).

			Func<DataPoint, double> Zp = d => -d.X * Math.Exp(-b * d.X);
			double meanZp = data.Sum(d => Zp(d)) / n;
			Func<DataPoint, double> Zap = d => -d.X * Math.Exp(-b * d.X) - meanZp;

			double sumZpY = data.Sum(d => Zap(d) * Ya(d));
			double sumZpZ = data.Sum(d => Zap(d) * Za(d));
			double sumZSqp = data.Sum(d => 2 * Zap(d) * Za(d));

			double u_ap = n * sumZpY;
			double vp = n * sumZSqp;

			double ap = (u_ap * v - vp * u_a) / (v * v);

			double Ep_half = -a * sumZpY + a * a * sumZpZ - ap * sumZY + a * ap * sumZZ;

			return Ep_half * 2;
		}
	}

	/// <summary>
	/// An (x, y) data point
	/// </summary>
	public class DataPoint
	{
		/// <summary>
		/// The X value
		/// </summary>
		public double X;

		/// <summary>
		/// The Y value
		/// </summary>
		public double Y;

		/// <summary>
		/// Creates a new data point
		/// </summary>
		/// <param name="x">The x value</param>
		/// <param name="y">The y value</param>
		public DataPoint(double x, double y)
		{
			X = x;
			Y = y;
		}
	}

	/// <summary>
	/// A sequence of data points
	/// </summary>
	public class DataPoints : IEnumerable<DataPoint>
	{
		/// <summary>
		/// The minimum x value of a data point
		/// </summary>
		public double MinX { get; private set; }

		/// <summary>
		/// The maximum x value of a data point
		/// </summary>
		public double MaxX { get; private set; }

		/// <summary>
		/// The difference between MaxX and MinX
		/// </summary>
		public double XRange { get { return MaxX - MinX; } }


		/// <summary>
		/// The data point sequence
		/// </summary>
		private List<DataPoint> _points;

		/// <summary>
		/// Creates a DataPoints with the given point sequence
		/// </summary>
		public DataPoints(IEnumerable<DataPoint> points)
		{
			_points = points.ToList();

			MaxX = _points.Max(pt => pt.X);
			MinX = _points.Min(pt => pt.X);
		}

		/// <summary>
		/// Creates a DataPoints from a sequence of (x1, y1, x2, y2, ...) values
		/// </summary>
		public static DataPoints Points(params double[] points)
		{
			List<DataPoint> pts = new List<DataPoint>();

			for (int i = 0; i < points.Length; i += 2)
			{
				pts.Add(new DataPoint(points[i], points[i + 1]));
			}

			return new DataPoints(pts);
		}

		/// <summary>
		/// Creates a data sequence by sampling the given function. The function is sampled at
		/// x values of 0, 1, 2, ...
		/// </summary>
		/// <param name="function">The function to sample</param>
		/// <param name="nPoints">The number of points to sample</param>
		/// <param name="randomOffsetScale">The size of random noise to add to the sequence's y values</param>
		/// <returns>The new data sequence</returns>
		public static DataPoints Sample(Func<double, double> function, int nPoints, double randomOffsetScale = 0)
		{
			double interval = 1;
			double maxX = nPoints - 1;

			return Sample(function, maxX, interval: interval, randomOffsetScale: randomOffsetScale);
		}

		/// <summary>
		/// Creates a data sequence by sampling the given function at regular intervals, plus at the end.
		/// 
		/// If minX = 0, maxX = 1.7 and interval = 0.5, the sequence will contain 5 points, at x = 0, 0.5, 1, 1.5 and 1.7.
		/// </summary>
		/// <param name="function">The function to sample</param>
		/// <param name="minX">The minimum x to sample</param>
		/// <param name="maxX">The maximum x to sample</param>
		/// <param name="interval">The offset between x samples</param>
		/// <param name="randomOffsetScale">The size of random noise to add to the sequence's y values</param>
		/// <returns>The new data sequence</returns>
		public static DataPoints Sample(Func<double, double> function, double maxX, double minX = 0, double interval = 1, double randomOffsetScale = 0)
		{
			if (randomOffsetScale > 0)
			{
				Random r = new Random(42);

				function = (xx) =>
				{
					double offset = randomOffsetScale * (r.NextDouble() * 2 - 1);

					return function(xx) + offset;
				};
			}

			double x = 0;

			var data = new List<DataPoint>();

			while (x < maxX)
			{
				data.Add(new DataPoint(x, function(x)));

				x += interval;
			}

			data.Add(new DataPoint(maxX, function(maxX)));

			return new DataPoints(data);
		}

		/// <inheritdoc/>
		public IEnumerator<DataPoint> GetEnumerator()
		{
			return _points.GetEnumerator();
		}

		/// <inheritdoc/>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _points.GetEnumerator();
		}
	}

	/// <summary>
	/// A function that decays exponentially toward a certain value at infinity.
	/// It has the form a * exp(-b * x) + c.
	/// 
	/// b is the decay factor, equal to ln(2) / halving distance.
	/// c is the value at infinity.
	/// a + c is the value at 0.
	/// </summary>
	public class ExponentialFunction
	{
		/// <summary>
		/// The value as zero
		/// </summary>
		public double ValueAtZero { get; private set; }

		/// <summary>
		/// The decay factor, b
		/// </summary>
		public double Decay { get; private set; }

		/// <summary>
		/// The halving time of the exponential
		/// </summary>
		public double HalfTime { get; private set; }

		/// <summary>
		/// The value at infinity
		/// </summary>
		public double ValueAtInfinity { get; private set; }

		/// <summary>
		/// Creates a function with the given parameters
		/// </summary>
		public ExponentialFunction(double valueAtZero, double valueAtInfinity, double decay)
		{
			if (decay <= 0)
				throw new ArgumentException("decay must be positive");

			ValueAtZero = valueAtZero;
			Decay = decay;
			HalfTime = Math.Log(2) / decay;
			ValueAtInfinity = valueAtInfinity;
		}

		/// <summary>
		/// Creates a function with the given parameters
		/// </summary>
		public static ExponentialFunction FromHalfTime(double valueAtZero, double valueAtInfinity, double halfTime)
		{
			double decay = Math.Log(2) / halfTime;

			ExponentialFunction f = new ExponentialFunction(valueAtZero, valueAtInfinity, decay);
			return f;
		}

		/// <summary>
		/// Returns the function's value at the given parameter value
		/// </summary>
		public double Value(double x)
		{
			return ValueAtInfinity + (ValueAtZero - ValueAtInfinity) * Math.Exp(-x * Decay);
		}
	}

	/// <summary>
	/// A linear function, of the form a * x + b
	/// 
	/// a is the value at 0.
	/// b is the gradient.
	/// </summary>
	public class LinearFunction
	{
		/// <summary>
		/// The value at x = 0
		/// </summary>
		public double ValueAtZero { get; private set; }

		/// <summary>
		/// The slope of the function
		/// </summary>
		public double Gradient { get; private set; }

		/// <summary>
		/// Creates a function with the given parameters
		/// </summary>
		public LinearFunction(double valueAtZero, double gradient)
		{
			ValueAtZero = valueAtZero;
			Gradient = gradient;
		}

		/// <summary>
		/// Returns the function's value at the given parameter value
		/// </summary>
		public double Value(double x)
		{
			return ValueAtZero + x * Gradient;
		}
	}
}