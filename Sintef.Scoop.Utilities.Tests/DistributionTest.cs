//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Sintef.Scoop.Utilities.Tests
{
	[TestClass]
	public class DistributionTest
	{
		[TestMethod]
		public void TestDistributionParsing()
		{
			string xml;
			Distribution distribution;
			
			xml = "<X>83</X>";
			distribution = Distribution.GetDistributionFromSubElementOrValue(Parse(xml));
			Assert.IsTrue(distribution is FixedValueDistribution);
			Assert.AreEqual(83, ((FixedValueDistribution)distribution).Mean);

			xml = "<Distribution Name='Uniform'><Minimum>12</Minimum><Maximum>83</Maximum></Distribution>";
			distribution = Distribution.GetDistribution(Parse(xml));
			Assert.IsTrue(distribution is UniformDistribution);
			Assert.AreEqual(12, ((UniformDistribution)distribution).Minimum);
			Assert.AreEqual(83, ((UniformDistribution)distribution).Maximum);
			Assert.AreEqual(0.5 * (83.0 + 12.0), ((UniformDistribution)distribution).Mean);

			xml = "<Distribution Name='Normal'><Mean>120</Mean><StandardDeviation>60</StandardDeviation></Distribution>";
			distribution = Distribution.GetDistribution(Parse(xml));
			Assert.IsTrue(distribution is NormalDistribution);
			Assert.AreEqual(120, ((NormalDistribution)distribution).Mean);
			Assert.AreEqual(60, ((NormalDistribution)distribution).StandardDeviation);

			xml = "<Distribution Name='Beta' Alpha='2' Beta='4'><Minimum>120</Minimum><Maximum>480</Maximum></Distribution>";
			distribution = Distribution.GetDistribution(Parse(xml));
			Assert.IsTrue(distribution is BetaDistribution);
			Assert.AreEqual(2, ((BetaDistribution)distribution).Alpha);
			Assert.AreEqual(4, ((BetaDistribution)distribution).Beta);
			Assert.AreEqual(120, ((BetaDistribution)distribution).Minimum);
			Assert.AreEqual(480, ((BetaDistribution)distribution).Maximum);
			Assert.AreEqual(240, ((BetaDistribution)distribution).Mean);

			xml = "<Distribution Name='Gamma' Shape='2'><Mean>120</Mean></Distribution>";
			distribution = Distribution.GetDistribution(Parse(xml));
			Assert.IsTrue(distribution is GammaDistribution);
			Assert.AreEqual(120, ((GammaDistribution)distribution).Mean);
			Assert.AreEqual(60, ((GammaDistribution)distribution).Alpha);
			Assert.AreEqual(2, ((GammaDistribution)distribution).Beta);
		}

		private static XElement Parse(string xml)
		{
			TextReader stream;
			using (stream = new StringReader(xml))
			{
				return XElement.Load(stream);
			}
		}

		[TestMethod]
		public void TestNormalDistribution()
		{
			double mean = 2.71;
			double variance = 1.2;
			double stdDev = Math.Sqrt(variance);
			var d = new NormalDistribution(mean, stdDev);

			VerifyMeanAndStdDev(d, mean, stdDev);
		}

		[TestMethod]
		public void TestExponentialDistribution()
		{
			double scale = 3.14;
			var d = new ExponentialDistribution(scale);

			VerifyMeanAndStdDev(d, scale, scale, samples:100000, tolerance: 0.01);

			Assert.AreEqual(scale, d.Mean);
			Assert.AreEqual(0, d.CumulativeProbability(-1));
			Assert.AreEqual(0, d.CumulativeProbability(0));
			Assert.AreEqual(0.63, d.CumulativeProbability(scale), 0.005);
			Assert.AreEqual(1.0, d.CumulativeProbability(1000), 0.0001);
			Assert.AreEqual(scale, d.ExpectedValueWhenGreaterThan(-1));
			Assert.AreEqual(scale + 1, d.ExpectedValueWhenGreaterThan(1));
			
			d = new ExponentialDistribution(-scale, allowNegativeMean: true);

			VerifyMeanAndStdDev(d, -scale, scale, samples: 100000, tolerance: 0.01);
		
			Assert.AreEqual(-scale, d.Mean);
			Assert.AreEqual(0, d.CumulativeProbability(-1000), 0.001);
			Assert.AreEqual(0.37, d.CumulativeProbability(-scale), 0.005);
			Assert.AreEqual(1.0, d.CumulativeProbability(0));
			Assert.AreEqual(1.0, d.CumulativeProbability(1));

			double offset = 5;
			d = new ExponentialDistribution(scale, offset: offset);

			VerifyMeanAndStdDev(d, offset + scale, scale, samples: 100000, tolerance: 0.01);

			Assert.AreEqual(scale + offset, d.Mean);
			Assert.AreEqual(0, d.CumulativeProbability(offset - 1));
			Assert.AreEqual(0, d.CumulativeProbability(offset));
			Assert.AreEqual(0.63, d.CumulativeProbability(offset + scale), 0.005);
			Assert.AreEqual(1.0, d.CumulativeProbability(1000), 0.0001);
			Assert.AreEqual(offset + scale, d.ExpectedValueWhenGreaterThan(offset - 1));
			Assert.AreEqual(offset + 1 + scale, d.ExpectedValueWhenGreaterThan(offset + 1));

			d = new ExponentialDistribution(-scale, allowNegativeMean: true, offset: offset);

			VerifyMeanAndStdDev(d, offset - scale, scale, samples: 100000, tolerance: 0.01);

			Assert.AreEqual(offset - scale, d.Mean);
			Assert.AreEqual(0, d.CumulativeProbability(-1000), 0.001);
			Assert.AreEqual(0.37, d.CumulativeProbability(offset - scale), 0.005);
			Assert.AreEqual(1.0, d.CumulativeProbability(offset));
			Assert.AreEqual(1.0, d.CumulativeProbability(offset + 1));
		}

		[TestMethod]
		public void TestBetaDistribution()
		{
			Random random = new Random(0);
			double min = 5.23;
			double max = 8.54;
			for (double alpha = 1.0; alpha < 3.5; alpha += 1)
			{
				for (double beta = 1.0; beta < 2.5; beta += 1)
				{
					var d = new BetaDistribution(min, max, alpha, beta);
					double expectedMean = min + (max - min) * (alpha / (alpha + beta));
					double expectedSdev = (max - min) * Math.Sqrt(alpha * beta / (Math.Pow(alpha + beta, 2) * (alpha + beta + 1)));

					VerifyMeanAndStdDev(d, expectedMean, expectedSdev, samples: 5000);
				}
			}
		}

		private static void VerifyMeanAndStdDev(Distribution distribution, double expectedMean, double expectedStdDev, int samples = 10000, double tolerance= 0.02)
		{
			Random random = new Random(0);
			int count = 0;
			double sum = 0;
			double sqrsum = 0;
			for (double x = 0; x <= samples; ++x)
			{
				double r = distribution.GetRandomDouble(random);
				count++;
				sum += r;
				sqrsum += Math.Pow(r - expectedMean, 2);
			}
			double observedMean = sum / count;
			double observedVariance = sqrsum / count;
			double observedStdDev = Math.Sqrt(observedVariance);
			Assert.AreEqual(expectedMean, observedMean, tolerance);
			Assert.AreEqual(expectedStdDev, observedStdDev, tolerance);
		}

		[TestMethod]
		public void TestTimeSpanDistribution()
		{
			Random random = new Random(42);

			TimeSpanDistribution dist = TimeSpanDistribution.FromXml(Parse("<X>00:01:23</X>"));
			Assert.AreEqual(60 + 23, dist.DrawTimeSpan(random).TotalSeconds);

			dist = TimeSpanDistribution.FromXml(Parse("<X><Distribution Name='Uniform'><Minimum>00:00:14</Minimum><Maximum>00:00:14</Maximum></Distribution></X>"));
			Assert.AreEqual(14, dist.DrawTimeSpan(random).TotalSeconds);

			dist = TimeSpanDistribution.FromXml(Parse("<Distribution Name='Uniform'><Minimum>00:00:12</Minimum><Maximum>00:00:12</Maximum></Distribution>"));
			Assert.AreEqual(12, dist.DrawTimeSpan(random).TotalSeconds);

			dist = TimeSpanDistribution.FromXml(Parse("<Distribution Name='Normal'><Mean>00:02:00</Mean><StandardDeviation>00:00:00</StandardDeviation></Distribution>"));
			Assert.AreEqual(120, dist.DrawTimeSpan(random).TotalSeconds);

			dist = TimeSpanDistribution.FromXml(Parse("<Distribution Name='Beta' Alpha='2' Beta='4'><Minimum>00:01:00</Minimum><Maximum>00:01:00</Maximum></Distribution>"));
			Assert.AreEqual(60, dist.DrawTimeSpan(random).TotalSeconds);

			dist = TimeSpanDistribution.FromXml(Parse("<Distribution Name='Gamma' Shape='2'><Mean>00:00:05</Mean></Distribution>"));
			Assert.AreEqual(2.3, dist.DrawTimeSpan(random).TotalSeconds, 0.1);
		}

		[TestMethod]
		public void TestExponentialThenLinearDistribution()
		{
			// Create a test distribution. Half of the probability weight is distributed uniformly between 0 and 10.
			// The other half decays exponentially below zero, with mean -2.
			
			ExponentialThenLinearDistribution d = new ExponentialThenLinearDistribution(10, 0, 0.5, 2);

			// Check cumulative probabilities
			Assert.AreEqual(0.025, d.CumulativeProbability(-6), 0.001);
			Assert.AreEqual(0.07, d.CumulativeProbability(-4), 0.01);
			Assert.AreEqual(0.18, d.CumulativeProbability(-2), 0.01);
			Assert.AreEqual(0.5, d.CumulativeProbability(-0.01), 0.01);
			Assert.AreEqual(0.5, d.CumulativeProbability(0));
			Assert.AreEqual(0.75, d.CumulativeProbability(5));
			Assert.AreEqual(1.0, d.CumulativeProbability(10));
			Assert.AreEqual(1.0, d.CumulativeProbability(1000));

			// Check expectation values
			Assert.AreEqual(-4, d.ExpectedValueWhenLessThan(-2));
			Assert.AreEqual(-2, d.ExpectedValueWhenLessThan(0));
			Assert.AreEqual(1.5, d.ExpectedValueWhenLessThan(10));
			Assert.AreEqual(1.5, d.ExpectedValueWhenLessThan(20));

			// Create another distribution with all weight in the exponential part
			
			d = new ExponentialThenLinearDistribution(150, 50, 1.0, 2);

			// Check cumulative probabilities
			Assert.AreEqual(0.05, d.CumulativeProbability(44), 0.001);
			Assert.AreEqual(0.14, d.CumulativeProbability(46), 0.01);
			Assert.AreEqual(0.36, d.CumulativeProbability(48), 0.01);
			Assert.AreEqual(1.0, d.CumulativeProbability(49.99), 0.01);
			Assert.AreEqual(1.0, d.CumulativeProbability(50));
			Assert.AreEqual(1.0, d.CumulativeProbability(1000));

			// Check expectation values
			Assert.AreEqual(46, d.ExpectedValueWhenLessThan(48));
			Assert.AreEqual(48, d.ExpectedValueWhenLessThan(50));
			Assert.AreEqual(48, d.ExpectedValueWhenLessThan(500));

			// And one with all weight in the uniform part (no exponential part)
			
			d = new ExponentialThenLinearDistribution(1, 0, 0.0, 1);

			// Check cumulative probabilities
			Assert.AreEqual(0.0, d.CumulativeProbability(-1));
			Assert.AreEqual(0.0, d.CumulativeProbability(0));
			Assert.AreEqual(0.5, d.CumulativeProbability(0.5));
			Assert.AreEqual(1.0, d.CumulativeProbability(1));
			Assert.AreEqual(1.0, d.CumulativeProbability(2));

			// Check expectation values
			Assert.IsTrue(double.IsNaN(d.ExpectedValueWhenLessThan(0)));
			Assert.AreEqual(0.25, d.ExpectedValueWhenLessThan(0.5));
			Assert.AreEqual(0.5, d.ExpectedValueWhenLessThan(1));
			Assert.AreEqual(0.5, d.ExpectedValueWhenLessThan(10));

			// Create one with Top=Base

			d = new ExponentialThenLinearDistribution(1, 1, 0.5, 1);

			// Check cumulative probabilities
			Assert.AreEqual(1.0, d.CumulativeProbability(1));
			Assert.AreEqual(0.5, d.CumulativeProbability(0.999), 0.001);

			// Check expectation values
			Assert.AreEqual(0.5, d.ExpectedValueWhenLessThan(2));
			Assert.AreEqual(0.0, d.ExpectedValueWhenLessThan(1));


			// Create one with Top=Base and no weight in the exponential part

			d = new ExponentialThenLinearDistribution(1, 1, 0.0, 1.1);

			// Check cumulative probabilities
			Assert.AreEqual(1.0, d.CumulativeProbability(1));
			Assert.AreEqual(0.0, d.CumulativeProbability(0.999));

			// Check expectation values
			Assert.AreEqual(1.0, d.ExpectedValueWhenLessThan(2));
			Assert.IsTrue(double.IsNaN(d.ExpectedValueWhenLessThan(1)));


		}

		[TestMethod]
		public void TestExponentialThenLinearDistributionFromSamples()
		{
			// Create distributions from known sample data and verify the parameters

			double[] samples = new double[] { 1 };

			ExponentialThenLinearDistribution d = ExponentialThenLinearDistribution.FromSamples(samples);

			Assert.AreEqual(1, d.TopValue);
			Assert.AreEqual(1, d.BaseValue);
			Assert.AreEqual(0, d.ProbabilityOfValueBelowBase);


			samples = new double[] { 1, 2, 3 };

			d = ExponentialThenLinearDistribution.FromSamples(samples);

			Assert.AreEqual(3, d.TopValue);
			Assert.AreEqual(2, d.BaseValue);
			Assert.AreEqual(1 / 3.0, d.ProbabilityOfValueBelowBase);
			Assert.AreEqual(1, d.ExpectedDistanceBelowBase);


			samples = new double[] { 1, 1, 1, 2, 3 };

			d = ExponentialThenLinearDistribution.FromSamples(samples);

			Assert.AreEqual(3, d.TopValue);
			Assert.AreEqual(1, d.BaseValue);
			Assert.AreEqual(0, d.ProbabilityOfValueBelowBase);

			// Create from generated sample data

			var correctExponential = new ExponentialDistribution(scale: 4.5);
			Random r = new Random(73);
			samples = Enumerable.Repeat(1, 1000).Select(x => -correctExponential.GetRandomDouble(r)).ToArray();
			d = ExponentialThenLinearDistribution.FromSamples(samples);

			// We expect an exponential scale of 4.5, but the variance is large. For
			// the chosen random seed, 0.8 is sufficient tolerance.
			Assert.AreEqual(4.5, d.ExpectedDistanceBelowBase, 0.8);
		}
	}
}
