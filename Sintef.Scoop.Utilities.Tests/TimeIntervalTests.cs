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
	[TestClass]
	public class TimeIntervalTests
	{
		[TestMethod]
		public void TestIntervalsMinus()
		{
			Action<TimeIntervals, TimeIntervals, TimeIntervals> DoTest = (interval, minus, result) =>
			{
				interval.RemoveIntervals(minus);
				Assert.IsTrue(Equal(interval, result));
			};

			DoTest(
				CreateIntervals(10, 20),
				CreateIntervals(1, 3),
				CreateIntervals(10, 20));

			DoTest(
				CreateIntervals(10, 20),
				CreateIntervals(8, 15),
				CreateIntervals(15, 20));

			DoTest(
				CreateIntervals(10, 20),
				CreateIntervals(12, 15),
				CreateIntervals(10,12, 15, 20));

			DoTest(
				CreateIntervals(10, 20),
				CreateIntervals(10, 20),
				CreateIntervals());

			DoTest(
				CreateIntervals(10, 20),
				CreateIntervals(9, 20),
				CreateIntervals());

			DoTest(
				CreateIntervals(10, 20),
				CreateIntervals(10, 21),
				CreateIntervals());

			DoTest(
				CreateIntervals(10, 20),
				CreateIntervals(9, 21),
				CreateIntervals());

			DoTest(
				CreateIntervals(10, 20),
				CreateIntervals(15, 25),
				CreateIntervals(10, 15));

			DoTest(
				CreateIntervals(10, 20),
				CreateIntervals(20, 25),
				CreateIntervals(10, 20));

			DoTest(
				CreateIntervals(10, 20),
				CreateIntervals(22, 25),
				CreateIntervals(10, 20));

			DoTest(
				CreateIntervals(10,20, 30,40, 50,60, 70,80, 90,100),
				CreateIntervals(1,3, 8,15, 33,37, 55,65, 70,80, 110,120),
				CreateIntervals(15,20, 30,33, 37,40, 50,55, 90,100));

			DoTest(
				CreateIntervals(10, 20, 30, 40),
				CreateIntervals(15, 40),
				CreateIntervals(10, 15));

			DoTest(
				CreateIntervals(10, 20, 30, 40),
				CreateIntervals(1, 35),
				CreateIntervals(35, 40));
		}

		[TestMethod]
		public void UnionOfZeroIntervalsIsEmpty()
		{
			IEnumerable<TimeInterval> intervals = new TimeInterval[0];
			var union = TimeIntervals.UnionOf(intervals);

			Assert.AreEqual(0, union.TotalLength.TotalSeconds);
		}

		[TestMethod]
		public void WithBufferWorksCorrectly()
		{
			DateTime zero = new DateTime(2010,1,1);
			TimeInterval interval = new TimeInterval(zero, zero.AddHours(1));

			var withBuffer = interval.WithBuffer(TimeSpan.FromHours(0.5));

			Assert.AreEqual(zero.AddHours(-0.5), withBuffer.StartTime);
			Assert.AreEqual(zero.AddHours(1.5), withBuffer.EndTime);
		}

		[TestMethod]
		public void IntersectionWithUnboundedIsEqualToTheOriginal()
		{
			DateTime zero = new DateTime(2010, 1, 1);
			var interval = new ClosedTimeInterval(zero, zero.AddHours(1));

			var unboundedInterval = ClosedTimeInterval.UnboundedInterval;

			var intersection = unboundedInterval.Intersection(interval);

			Assert.AreEqual(interval, intersection);
		}
		
		/// <summary>
		/// Defines a zero point in time used in Utils
		/// </summary>
		public static DateTime TimeZero { get { return new DateTime(2017, 1, 1); } }
		
		/// <summary>
		/// Creates TimeIntervals representing the intervals
		/// </summary>
		/// <param name="intervals">Offset in minutes from zero point for start-end of intervals. Must be even number and increasing.</param>
		/// <returns>Generated TimeIntevals</returns>
		public static TimeIntervals CreateIntervals(params int[] intervals)
		{
			if (intervals.Length % 2 != 0 || intervals.AdjacentPairs().Any(p => p.Item1 >= p.Item2))
				throw new ArgumentException("intervals must have an even number of increasing elements");

			var windows = new List<TimeInterval>(intervals.Length / 2);
			for (int i = 0; i < intervals.Length / 2; ++i)
			{
				windows.Add(new TimeInterval(TimeZero.AddMinutes(intervals[i * 2]), TimeZero.AddMinutes(intervals[i * 2 + 1])));
			}

			return new TimeIntervals(windows);
		}

		/// <summary>
		/// Compares two TimeIntervals whether they are equal
		/// </summary>
		/// <param name="a">Intervals A</param>
		/// <param name="b">Intervals B</param>
		/// <returns>Whether A == B </returns>
		public static bool Equal(TimeIntervals a, TimeIntervals b)
		{
			if (a.Count() != b.Count())
				return false;
			if (a.Count() == 0)
				return true;

			var aIt = a.GetEnumerator();
			aIt.MoveNext();
			var bIt = b.GetEnumerator();
			bIt.MoveNext();
			while (true)
			{
				if (aIt.Current.Start != bIt.Current.Start)
					return false;
				if (aIt.Current.End != bIt.Current.End)
					return false;

				if (!aIt.MoveNext())
					return true;
				bIt.MoveNext();
			}
		}
		
	} 
}