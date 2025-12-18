//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Base class for statistical probability distributions
	/// </summary>
	public abstract class Distribution
	{
		/// <summary>
		/// The mean of the distribution
		/// </summary>
		public abstract double Mean { get; }

		/// <summary>
		/// Get a random value according to the given distribution
		/// </summary>
		/// <param name="random">The pseudo-random number generator</param>
		/// <returns>A random value according to the distribution</returns>
		public abstract double GetRandomDouble(Random random);

		/// <summary>
		/// Get a random time span according to the distribution
		/// </summary>
		/// <param name="random">The pseudo-random number generator</param>
		/// <returns>A random time span according to the distribution</returns>
		public TimeSpan GetRandomTimeSpan(Random random)
		{
			return TimeSpan.FromSeconds(GetRandomDouble(random));
		}

		/// <summary>
		/// Get a statistical probability distribution from XElement.
		/// </summary>
		public static Distribution GetDistribution(XElement element, bool useTimeSpanAsSeconds = false)
		{
			Distribution distribution = null;
			switch (element.RequireAttribute("Name").Value)
			{
				case "Fixed":
					{
						double value;
						if (useTimeSpanAsSeconds)
							value = element.ParseTimeSpanInvariant().TotalSeconds;
						else
							value = element.ParseDoubleInvariant();
						distribution = new FixedValueDistribution(value);
					}
					break;

				case "Uniform":
					{
						double minimum, maximum;
						XElement xMinimum = element.RequireElement("Minimum");
						if (useTimeSpanAsSeconds)
							minimum = xMinimum.ParseTimeSpanInvariant().TotalSeconds;
						else
							minimum = xMinimum.ParseDoubleInvariant();
						XElement xMaximum = element.RequireElement("Maximum");
						if (useTimeSpanAsSeconds)
							maximum = xMaximum.ParseTimeSpanInvariant().TotalSeconds;
						else
							maximum = xMaximum.ParseDoubleInvariant();
						distribution = new UniformDistribution(minimum, maximum);
					}
					break;

				case "Normal":
					{
						double mean, std;
						XElement xMean = element.RequireElement("Mean");
						if (useTimeSpanAsSeconds)
							mean = xMean.ParseTimeSpanInvariant().TotalSeconds;
						else
							mean = xMean.ParseDoubleInvariant();
						XElement xStd = element.RequireElement("StandardDeviation");
						if (useTimeSpanAsSeconds)
							std = xStd.ParseTimeSpanInvariant().TotalSeconds;
						else
							std = xStd.ParseDoubleInvariant();
						distribution = new NormalDistribution(mean, std);
					}
					break;

				case "Exponential":
					{
						double mean;
						XElement xMean = element.RequireElement("Mean");
						if (useTimeSpanAsSeconds)
							mean = xMean.ParseTimeSpanInvariant().TotalSeconds;
						else
							mean = xMean.ParseDoubleInvariant();
						distribution = new ExponentialDistribution(mean);
					}
					break;

				case "Beta":
					{
						double alpha = element.RequireAttribute("Alpha").ParseDoubleInvariant();
						double beta = element.RequireAttribute("Beta").ParseDoubleInvariant();
						double minimum, maximum;
						XElement xMinimum = element.RequireElement("Minimum");
						if (useTimeSpanAsSeconds)
							minimum = xMinimum.ParseTimeSpanInvariant().TotalSeconds;
						else
							minimum = xMinimum.ParseDoubleInvariant();
						XElement xMaximum = element.RequireElement("Maximum");
						if (useTimeSpanAsSeconds)
							maximum = xMaximum.ParseTimeSpanInvariant().TotalSeconds;
						else
							maximum = xMaximum.ParseDoubleInvariant();
						distribution = new BetaDistribution(minimum, maximum, alpha, beta);
					}
					break;

				case "Gamma":
					{ // Expressed as alpha and beta
						double alpha, beta;
						var xAlpha = element.TagAttribute("Alpha");
						if (xAlpha != null)
						{
							var xBeta = element.RequireAttribute("Beta");
							alpha = xAlpha.ParseDoubleInvariant();
							beta = xBeta.ParseDoubleInvariant();

						}
						else
						{ // Expressed as a shape parameter k and a mean parameter μ = k/β.
							double mean;
							var xMean = element.RequireElement("Mean");
							if (useTimeSpanAsSeconds)
								mean = xMean.ParseTimeSpanInvariant().TotalSeconds;
							else
								mean = xMean.ParseDoubleInvariant();
							double shape = element.RequireAttribute("Shape").ParseDoubleInvariant();

							alpha = mean / shape;
							beta = mean / alpha;
						}
						distribution = new GammaDistribution(alpha, beta);
					}
					break;
			}
			return distribution;
		}

		/// <summary>
		/// Get a statistical probability distribution from the contents of XElement.
		/// The distribution is either found from a sub-element with the given name
		/// or from the value of the element.
		/// </summary>
		public static Distribution GetDistributionFromSubElementOrValue(XElement element,
					string nameOfSubElement = "Distribution", bool useTimeSpanAsSeconds = false)
		{
			XElement xDistribution = nameOfSubElement == null ? null : element.TagElement(nameOfSubElement);
			if (xDistribution == null)
			{
				double value;
				if (useTimeSpanAsSeconds)
					value = element.ParseTimeSpanInvariant().TotalSeconds;
				else
					value = element.ParseDoubleInvariant();
				return new FixedValueDistribution(value);
			}
			return GetDistribution(xDistribution, useTimeSpanAsSeconds: true);
		}

		/// <summary>
		/// Get a random time span according to the distribution specified in the element
		/// </summary>
		/// <param name="element">The specification of the statistical probability distribution to use</param>
		/// <param name="random">The pseudo-random number generator</param>
		/// <returns>A random time span according to the distribution</returns>
		public static TimeSpan GetRandomTimeSpan(XElement element, Random random)
		{
			XElement xDistribution = element.TagElement("Distribution");
			if (xDistribution != null)
			{
				Distribution distribution = GetDistribution(xDistribution, useTimeSpanAsSeconds: true);
				return distribution.GetRandomTimeSpan(random);
			}
			return element.ParseTimeSpanInvariant();
		}

		/// <summary>
		/// Get a random double according to the distribution specified in the element
		/// </summary>
		/// <param name="element">The specification of the statistical probability distribution to use</param>
		/// <param name="random">The pseudo-random number generator</param>
		/// <returns>A random double according to the distribution</returns>
		public static double GetRandomDouble(XElement element, Random random)
		{
			XElement xDistribution = element.TagElement("Distribution");
			if (xDistribution != null)
			{
				Distribution distribution = GetDistribution(xDistribution);
				return distribution.GetRandomDouble(random);
			}
			return element.ParseDoubleInvariant();
		}

	}

	/// <summary>
	/// A distribution that always yields a single fixed value
	/// </summary>
	public class FixedValueDistribution : Distribution
	{
		/// <summary>
		/// The value
		/// </summary>
		private double _value;

		/// <inheritdoc/>
		public override double Mean
		{
			get { return _value; }
		}

		/// <summary>
		/// Initializes the disribution
		/// </summary>
		/// <param name="value">The fixed value</param>
		public FixedValueDistribution(double value)
		{
			_value = value;
		}

		/// <inheritdoc/>
		public override double GetRandomDouble(Random random)
		{
			return _value;
		}

	}

	/// <summary>
	/// The uniform probability distribution, i.e., uniform probability between a minimum and a maximum value
	/// </summary>
	public class UniformDistribution : Distribution
	{
		/// <summary>
		/// The smallest value
		/// </summary>
		public double Minimum { get; private set; }
		/// <summary>
		/// The largest value
		/// </summary>
		public double Maximum { get; private set; }

		/// <inheritdoc/>
		public override double Mean
		{
			get { return 0.5 * (Minimum + Maximum); }
		}

		/// <summary>
		/// Initializes the distribution
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		public UniformDistribution(double min, double max)
		{
			Minimum = min;
			Maximum = max;
		}

		/// <summary>
		/// Returns a random number uniformly distributed between <paramref name="min"/> and <paramref name="max"/>
		/// </summary>
		public static double GetRandomDouble(Random random, double min, double max)
		{
			return min + random.NextDouble() * (double.Epsilon + max - min);
		}

		/// <inheritdoc/>
		public override double GetRandomDouble(Random random)
		{
			return GetRandomDouble(random, Minimum, Maximum);
		}

	}

	/// <summary>
	/// The normal probability distribution N(m,s)
	/// </summary>
	public class NormalDistribution : Distribution
	{
		private double _mean;

		/// <inheritdoc/>
		public override double Mean
		{
			get { return _mean; }
		}

		/// <summary>
		/// The distribution's standard deviation
		/// </summary>
		public double StandardDeviation { get; private set; }

		/// <summary>
		/// Initializes the distribution
		/// </summary>
		public NormalDistribution(double mean, double stdDev)
		{
			_mean = mean;
			StandardDeviation = stdDev;
		}

		/// <summary>
		/// returns N(0,1)
		/// </summary>
		public static double GetRandomDoubleN01(Random random)
		{
			double s, v1, v2;
			do
			{
				v1 = UniformDistribution.GetRandomDouble(random, -1, 1);
				v2 = UniformDistribution.GetRandomDouble(random, -1, 1);
				s = v1 * v1 + v2 * v2;
			} while (s >= 1 || s == 0);
			// s is now a random point inside the unit circle
			double n01 = v1 * Math.Sqrt(-2 * Math.Log(s) / s);
			return n01;
		}

		/// <summary>
		/// Returns a normal distributed random number
		/// </summary>
		/// <param name="random"></param>
		/// <param name="mean"></param>
		/// <param name="stdDev"></param>
		/// <returns></returns>
		public static double GetRandomDouble(Random random, double mean, double stdDev)
		{
			return mean + GetRandomDoubleN01(random) * stdDev;
		}

		/// <inheritdoc/>
		public override double GetRandomDouble(Random random)
		{
			return GetRandomDouble(random, Mean, StandardDeviation);
		}

	}

	/// <summary>
	/// The exponential probability distribution.
	/// 
	/// It describes the time between events in a Poisson process, i.e. a process in 
	/// which events occur continuously and independently at a constant average rate.
	/// The mean time between events is called the scale parameter of the distribution.
	/// 
	/// The distribution supports a negative scale parameter, in which case negative
	/// samples are generated.
	/// 
	/// The distribution also supports an offset, which is added to each generated sample.
	/// </summary>
	public class ExponentialDistribution : Distribution
	{
		private double _scale;

		private double _offset;

		/// <inheritdoc/>
		public override double Mean
		{
			get { return _offset + _scale; }
		}

		/// <summary>
		/// Initializes the distribution
		/// </summary>
		public ExponentialDistribution(double scale, bool allowNegativeMean = false, double offset = 0)
		{
			if (scale == 0)
				throw new ArgumentException("scale cannot be zero");
			if (scale < 0 && !allowNegativeMean)
				throw new ArgumentException("scale must be positive");

			_scale = scale;
			_offset = offset;
		}

		/// <inheritdoc/>
		public override double GetRandomDouble(Random random)
		{
			double r = 1.0 - random.NextDouble();
			return _offset - _scale * Math.Log(r);
		}

		/// <summary>
		/// Returns the probability of drawing a random sample smaller than <paramref name="value"/>
		/// </summary>
		public double CumulativeProbability(double value)
		{
			value -= _offset;

			if (_scale > 0)
			{
				if (value < 0)
					return 0.0;

				return 1.0 - Math.Exp(-value / _scale);
			}
			else
			{
				if (value > 0)
					return 1.0;

				return Math.Exp(-value / _scale);
			}
		}

		/// <summary>
		/// Returns the expected value of a sample when the sample is larger than <paramref name="value"/>
		/// </summary>
		public double ExpectedValueWhenGreaterThan(double value)
		{
			if (_scale < 0)
				throw new NotImplementedException("ExpectedValueWhenGreaterThan with negative scale");

			if (value < _offset)
				return _offset + _scale;

			return value + _scale;
		}

		/// <summary>
		/// Estimates an exponential distribution (with zero offset) from the given samples
		/// </summary>
		public static ExponentialDistribution FromSamples(IEnumerable<double> samples)
		{
			double sampleSum = samples.Sum();
			int nSamples = samples.Count();

			double estimatedScale = sampleSum / nSamples;

			if (estimatedScale > 0)
				return new ExponentialDistribution(estimatedScale);
			else
				return null;
		}
	}

	/// <summary>
	/// The beta probability distribution Beta(α, β)
	/// 
	/// m = α / (α+β)
	/// s = sqrt( α*β / ( (α+β)^2 * (α+β+1) ) )
	/// 
	/// </summary>
	public class BetaDistribution : Distribution
	{
		/// <summary>
		/// Shape parameter (normally a value greater then 1)
		/// </summary>
		public double Alpha { get; private set; }
		/// <summary>
		/// Shape parameter (normally a value greater then 1)
		/// </summary>
		public double Beta { get; private set; }
		/// <summary>
		/// The smallest value
		/// </summary>
		public double Minimum { get; private set; }
		/// <summary>
		/// The largest value
		/// </summary>
		public double Maximum { get; private set; }

		/// <summary>
		/// Initializes the distribution
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="alpha"></param>
		/// <param name="beta"></param>
		public BetaDistribution(double min, double max, double alpha, double beta)
		{
			Minimum = min;
			Maximum = max;
			Alpha = alpha;
			Beta = beta;
		}

		/// <inheritdoc/>
		public override double Mean
		{
			get
			{
				return Minimum + (Maximum - Minimum) * Alpha / (Alpha + Beta);
			}
		}

		/// <summary>
		/// Returns a beta-distributed random sample according to the given parameters
		/// </summary>
		public static double GetRandomDouble(Random random, double alpha, double beta)
		{
			double y1 = GammaDistribution.GetRandomDouble(random, alpha);
			return y1 / (y1 + GammaDistribution.GetRandomDouble(random, beta));
		}

		/// <summary>
		/// Returns a beta-distributed random sample according to the given parameters
		/// </summary>
		public static double GetRandomDouble(Random random, double min, double max, double alpha, double beta)
		{
			return min + (max - min) * GetRandomDouble(random, alpha, beta);
		}

		/// <inheritdoc/>
		public override double GetRandomDouble(Random random)
		{
			return GetRandomDouble(random, Minimum, Maximum, Alpha, Beta);
		}

	}

	/// <summary>
	/// The gamma probability distribution
	/// 
	/// m = alpha * beta
	/// s = sqrt(alpha * beta^2)
	/// alpha = (m / s)^2
	/// beta = s^2 / m
	/// 
	/// </summary>
	public class GammaDistribution : Distribution
	{
		/// <summary>
		/// The alpha parameter
		/// </summary>
		public double Alpha { get; private set; }
		/// <summary>
		/// The beta parameter
		/// </summary>
		public double Beta { get; private set; }

		/// <inheritdoc/>
		public override double Mean
		{
			get { return Alpha * Beta; }
		}

		/// <summary>
		/// Initialize the distribution
		/// </summary>
		/// <param name="alpha"></param>
		/// <param name="beta"></param>
		public GammaDistribution(double alpha, double beta)
		{
			Alpha = alpha;
			Beta = beta;
		}

		/// <summary>
		/// Returns a beta distributed sample according to the given parameters and beta=1
		/// </summary>
		public static double GetRandomDouble(Random random, double alpha)
		{
			if (alpha < 1)
			{ // GS algorithm
				while (true)
				{ // step 1
					double b = (Math.E + alpha) / Math.E, u1 = random.NextDouble();
					double p = b * u1;
					if (p <= 1)
					{ // step 2
						double y = Math.Pow(p, 1 / alpha), u2 = random.NextDouble();
						if (u2 <= Math.Exp(-y))
							return y;
					}
					else
					{ // step 3
						double y = -Math.Log((b - p) / alpha), u2 = random.NextDouble();
						if (u2 <= Math.Pow(y, alpha - 1))
							return y;
					}
				}
			}
			else
			{
				double a = 1 / Math.Sqrt(2 * alpha - 1), b = alpha - Math.Log(4), d = 1 + Math.Log(4.5);
				double q = alpha + 1 / a;
				while (true)
				{
					double u1 = random.NextDouble();
					double u2 = random.NextDouble();
					double v = a * Math.Log(u1 / (1 - u1));
					double y = alpha * Math.Exp(v), z = u1 * u1 * u2;
					double w = b + q * v - y;
					if (w * d >= 4.5 * z || w >= Math.Log(z))
						return y;
				}
			}
		}

		/// <summary>
		/// Returns a beta distributed sample according to the given parameters
		/// </summary>
		public static double GetRandomDouble(Random random, double alpha, double beta)
		{
			return GetRandomDouble(random, alpha) * beta;
		}

		/// <inheritdoc/>
		public override double GetRandomDouble(Random random)
		{
			return GetRandomDouble(random, Alpha) * Beta;
		}

	}

	/// <summary>
	/// A statistical distribution that contains two parts: one uniformly distributed, and the other
	/// exponentially distributed. The exponential part is for low values, i.e. it tails off toward negative
	/// infinity.
	/// </summary>
	public class ExponentialThenLinearDistribution
	{
		/// <summary>
		/// The highest value in the uniform part
		/// </summary>
		public double TopValue { get; private set; }

		/// <summary>
		/// The lowest value in uniform part, which is also the highest value in the exponential part
		/// </summary>
		public double BaseValue { get; private set; }

		/// <summary>
		/// The total probability weight in the exponential part
		/// </summary>
		public double ProbabilityOfValueBelowBase { get; private set; }

		/// <summary>
		/// The scale parameter of the exponential part, which is equal to the base value minus
		/// the exponential part's expected value
		/// </summary>
		public double ExpectedDistanceBelowBase { get { return DistributionOfBaseMinusValue.Mean; } }

		/// <summary>
		/// The distribution used to generate the exponential part
		/// </summary>
		private ExponentialDistribution DistributionOfBaseMinusValue;

		/// <summary>
		/// Creates a distribution with the given parameters
		/// </summary>
		public ExponentialThenLinearDistribution(double topValue, double baseValue, double probabilityBelowBase, double expectedDistanceBelowBase)
			: this(topValue, baseValue, probabilityBelowBase, new ExponentialDistribution(expectedDistanceBelowBase))
		{
		}

		/// <summary>
		/// Estimates a distribution based on the given samples
		/// </summary>
		/// <param name="samples">The samples to approximate</param>
		public static ExponentialThenLinearDistribution FromSamples(IEnumerable<double> samples)
		{
			if (samples == null || !samples.Any())
				throw new ArgumentException("samples is null or empty");

			double topValue = samples.Max();

			// Choose the base value as the median or 10th smallest value, whichever is smaller
			var smallSamples = samples.OrderBy(v => v).Take(samples.Count() / 2 + 1).Take(10).ToList();
			double baseValue = smallSamples.Last();

			// Estimate the exponential part from the samples smaller than the base value
			smallSamples = smallSamples.TakeWhile(x => x < baseValue).ToList();

			double probabilityOfValueBelowBase = smallSamples.Count / (double)samples.Count();

			var DistributionOfBaseMinusValue = ExponentialDistribution.FromSamples(smallSamples.Select(x => baseValue - x));

			return new ExponentialThenLinearDistribution(topValue, baseValue, probabilityOfValueBelowBase, DistributionOfBaseMinusValue);
		}

		/// <summary>
		/// Returns the total probability up to, but not including, the given value
		/// </summary>
		public double ProbabilityOfLessThan(double value)
		{
			if (value == BaseValue && BaseValue == TopValue)
				return ProbabilityOfValueBelowBase;
			else
				return CumulativeProbability(value);
		}

		/// <summary>
		/// Returns the total probability up to and including the given value
		/// </summary>
		public double CumulativeProbability(double value)
		{
			if (value >= TopValue)
				return 1.0;

			if (value >= BaseValue)
				return ProbabilityOfValueBelowBase + (value - BaseValue) / (TopValue - BaseValue) * (1 - ProbabilityOfValueBelowBase);

			if (ProbabilityOfValueBelowBase == 0)
				return 0;

			double baseMinusValue = BaseValue - value;

			return ProbabilityOfValueBelowBase * (1.0 - DistributionOfBaseMinusValue.CumulativeProbability(baseMinusValue));
		}

		/// <summary>
		/// Returns the expected value in the part of the distribution that is strictly
		/// smaller than the given value
		/// </summary>
		public double ExpectedValueWhenLessThan(double value)
		{
			if (value <= BaseValue)
			{
				// Only the exponential part contributes

				if (ProbabilityOfValueBelowBase == 0)
					return double.NaN;

				double baseMinusValue = BaseValue - value;

				double expectedBaseMinusValue = DistributionOfBaseMinusValue.ExpectedValueWhenGreaterThan(baseMinusValue);

				return BaseValue - expectedBaseMinusValue;
			}

			if (value > TopValue)
			{
				// We're above the top value

				if (BaseValue < TopValue)
					// Normally, we can ignore the difference
					return ExpectedValueWhenLessThan(TopValue);

				// .. but when the top and base are equal, we must take the discrete probability
				// of the base value into account

				if (ProbabilityOfValueBelowBase == 0)
					return BaseValue;

				return ProbabilityOfValueBelowBase * ExpectedValueWhenLessThan(BaseValue) +
					(1.0 - ProbabilityOfValueBelowBase) * BaseValue;
			}

			// Value between top and base: calculate and weigh contributions from both parts

			double expectedValueAboveBase = (value + BaseValue) / 2;

			if (ProbabilityOfValueBelowBase == 0)
				return expectedValueAboveBase;

			double totalProbability = ProbabilityOfLessThan(value);

			return (ProbabilityOfValueBelowBase * ExpectedValueWhenLessThan(BaseValue) +
				(totalProbability - ProbabilityOfValueBelowBase) * expectedValueAboveBase)
				/ totalProbability;
		}

		/// <summary>
		/// Private constructor
		/// </summary>
		private ExponentialThenLinearDistribution(double topValue, double baseValue, double probabilityBelowBase, ExponentialDistribution exponentialDistribution)
		{
			if (topValue < baseValue)
				throw new ArgumentException("topValue is below baseValue");
			if (probabilityBelowBase < 0 || probabilityBelowBase > 1)
				throw new ArgumentException("probabilityBelowBase is not between 0 and 1");
			if (probabilityBelowBase > 0 && exponentialDistribution == null)
				throw new ArgumentException("exponentialDistribution is null");

			TopValue = topValue;
			BaseValue = baseValue;
			ProbabilityOfValueBelowBase = probabilityBelowBase;
			DistributionOfBaseMinusValue = exponentialDistribution;
		}
	}
}