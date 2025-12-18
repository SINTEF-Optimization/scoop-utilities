//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sintef.Scoop.Utilities.OpenClosedInterval;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities.Tests
{
	[TestClass]
	public class BoundedIntervalTests
	{
		public class TestIntervals<T> : SortedIntervals<Interval<T>, T>
		{
			public TestIntervals(IEnumerable<Interval<T>> intervals) : base(intervals)
			{
			}

			public override SortedIntervals<Interval<T>, T> Create(IEnumerable<Interval<T>> intervals)
			{
				return new TestIntervals<T>(intervals);
			}

			protected override Interval<T> CreateSubInterval(Interval<T> interval, IntervalLowerBound<T> start, IntervalUpperBound<T> end)
			{
				return new Interval<T>(start, end);
			}
		}

		/// <summary>
		/// Creates Intervals representing the intervals
		/// </summary>
		/// <param name="intervals">Must be even number and increasing.If negative then exclusive bound.</param>
		/// <returns>Generated integer Intevals</returns>
		public static IEnumerable<Interval<int>> CreateUnsortedIntervalEnumerable(params int[] intervals)
		{
			if (intervals.Length % 2 != 0)
				throw new ArgumentException("intervals must have an even number of elements");

			var windows = new List<Interval<int>>(intervals.Length / 2);
			for (int i = 0; i < intervals.Length / 2; ++i)
			{
				windows.Add(new Interval<int>(Math.Abs(intervals[i * 2]), Math.Abs(intervals[i * 2 + 1]), intervals[i * 2] > 0, intervals[i * 2 + 1] > 0));
			}

			return windows;
		}

		/// <summary>
		/// Creates Intervals representing the intervals
		/// </summary>
		/// <param name="intervals">Must be even number and increasing.If negative then exclusive bound.</param>
		/// <returns>Generated integer Intevals</returns>
		public static IEnumerable<Interval<int>> CreateIntervalEnumerable(params int[] intervals)
		{
			if (intervals.Length % 2 != 0 || intervals.AdjacentPairs().Any(p => Math.Abs(p.Item1) > Math.Abs(p.Item2)))
				throw new ArgumentException("intervals must have an even number of increasing elements");

			var windows = new List<Interval<int>>(intervals.Length / 2);
			for (int i = 0; i < intervals.Length / 2; ++i)
			{
				windows.Add(new Interval<int>(Math.Abs(intervals[i * 2]), Math.Abs(intervals[i * 2 + 1]), intervals[i * 2] > 0, intervals[i * 2 + 1] > 0));
			}

			return windows;
		}

		/// <summary>
		/// Creates Intervals representing the intervals
		/// </summary>
		/// <param name="intervals">Must be even number and increasing.If negative then exclusive bound.</param>
		/// <returns>Generated integer Intevals</returns>
		public static TestIntervals<int> CreateIntervals(params int[] intervals)
		{
			return new TestIntervals<int>(CreateIntervalEnumerable(intervals));
		}

		/// <summary>
		/// Compares two TimeIntervals whether they are equal
		/// </summary>
		/// <param name="a">Intervals A</param>
		/// <param name="b">Intervals B</param>
		/// <returns>Whether A == B </returns>
		public static bool Equal<T>(SortedIntervals<Interval<T>, T> a, SortedIntervals<Interval<T>, T> b)
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


		[TestMethod]
		public void TestIntervalLowerBounds()
		{
			IComparer<double> cmp = Comparer<double>.Default;

			IntervalLowerBound<double> lowerZeroIn = new(0, true, cmp);
			IntervalLowerBound<double> lowerZeroIn2 = new(0, true, cmp);
			IntervalLowerBound<double> lowerZeroEx = new(0, false, cmp);

			IntervalLowerBound<double> lowerTenIn = new(10, true, cmp);
			IntervalLowerBound<double> lowerTenEx = new(10, false, cmp);

			Assert.AreEqual(0, lowerZeroEx.Compare(lowerZeroEx));
			Assert.AreEqual(0, lowerZeroIn.Compare(lowerZeroIn));
			Assert.AreEqual(0, lowerZeroIn.Compare(lowerZeroIn2));
			Assert.AreEqual(0, lowerZeroIn2.Compare(lowerZeroIn));
			Assert.AreEqual(-1, lowerZeroIn.Compare(lowerZeroEx));
			Assert.AreEqual(+1, lowerZeroEx.Compare(lowerZeroIn));

			var tmp = lowerZeroEx;
			Assert.IsTrue(lowerZeroEx == tmp);
			tmp = lowerZeroIn;
			Assert.IsTrue(lowerZeroIn == tmp);
			Assert.IsTrue(lowerZeroIn == lowerZeroIn2);
			Assert.IsTrue(lowerZeroIn2 == lowerZeroIn);
			Assert.IsTrue(lowerZeroIn < lowerZeroEx);
			Assert.IsTrue(lowerZeroEx > lowerZeroIn);

			Assert.AreEqual(-1, lowerZeroIn.Compare(lowerTenIn));
			Assert.AreEqual(-1, lowerZeroIn.Compare(lowerTenEx));
			Assert.AreEqual(+1, lowerTenIn.Compare(lowerZeroIn));
			Assert.AreEqual(+1, lowerTenIn.Compare(lowerZeroEx));

			Assert.IsTrue(lowerZeroIn < lowerTenIn);
			Assert.IsTrue(lowerZeroIn < lowerTenEx);
			Assert.IsTrue(lowerTenIn > lowerZeroIn);
			Assert.IsTrue(lowerTenIn > lowerZeroEx);

			Assert.AreEqual(-1, lowerZeroEx.Compare(lowerTenIn));
			Assert.AreEqual(-1, lowerZeroEx.Compare(lowerTenEx));
			Assert.AreEqual(+1, lowerTenEx.Compare(lowerZeroIn));
			Assert.AreEqual(+1, lowerTenEx.Compare(lowerZeroEx));

			Assert.IsTrue(lowerZeroEx < lowerTenIn);
			Assert.IsTrue(lowerZeroEx < lowerTenEx);
			Assert.IsTrue(lowerTenEx > lowerZeroIn);
			Assert.IsTrue(lowerTenEx > lowerZeroEx);
		}

		[TestMethod]
		public void TestIntervalUpperBounds()
		{
			IComparer<double> cmp = Comparer<double>.Default;

			IntervalUpperBound<double> UpperZeroIn = new(0, true, cmp);
			IntervalUpperBound<double> UpperZeroIn2 = new(0, true, cmp);
			IntervalUpperBound<double> UpperZeroEx = new(0, false, cmp);

			IntervalUpperBound<double> UpperTenIn = new(10, true, cmp);
			IntervalUpperBound<double> UpperTenEx = new(10, false, cmp);

			Assert.AreEqual(0, UpperZeroEx.Compare(UpperZeroEx));
			Assert.AreEqual(0, UpperZeroIn.Compare(UpperZeroIn));
			Assert.AreEqual(0, UpperZeroIn.Compare(UpperZeroIn2));
			Assert.AreEqual(0, UpperZeroIn2.Compare(UpperZeroIn));
			Assert.AreEqual(+1, UpperZeroIn.Compare(UpperZeroEx));
			Assert.AreEqual(-1, UpperZeroEx.Compare(UpperZeroIn));

			var tmp = UpperZeroEx;
			Assert.IsTrue(UpperZeroEx == tmp);
			tmp = UpperZeroIn;
			Assert.IsTrue(UpperZeroIn == tmp);
			Assert.IsTrue(UpperZeroIn == UpperZeroIn2);
			Assert.IsTrue(UpperZeroIn2 == UpperZeroIn);
			Assert.IsTrue(UpperZeroIn > UpperZeroEx);
			Assert.IsTrue(UpperZeroEx < UpperZeroIn);

			Assert.AreEqual(-1, UpperZeroIn.Compare(UpperTenIn));
			Assert.AreEqual(-1, UpperZeroIn.Compare(UpperTenEx));
			Assert.AreEqual(+1, UpperTenIn.Compare(UpperZeroIn));
			Assert.AreEqual(+1, UpperTenIn.Compare(UpperZeroEx));

			Assert.IsTrue(UpperZeroIn < UpperTenIn);
			Assert.IsTrue(UpperZeroIn < UpperTenEx);
			Assert.IsTrue(UpperTenIn > UpperZeroIn);
			Assert.IsTrue(UpperTenIn > UpperZeroEx);

			Assert.AreEqual(-1, UpperZeroEx.Compare(UpperTenIn));
			Assert.AreEqual(-1, UpperZeroEx.Compare(UpperTenEx));
			Assert.AreEqual(+1, UpperTenEx.Compare(UpperZeroIn));
			Assert.AreEqual(+1, UpperTenEx.Compare(UpperZeroEx));

			Assert.IsTrue(UpperZeroEx < UpperTenIn);
			Assert.IsTrue(UpperZeroEx < UpperTenEx);
			Assert.IsTrue(UpperTenEx > UpperZeroIn);
			Assert.IsTrue(UpperTenEx > UpperZeroEx);
		}

		[TestMethod]
		public void TestIntervalBounds()
		{
			IComparer<double> cmp = Comparer<double>.Default;

			IntervalLowerBound<double> lowerZeroIn = new(0, true, cmp);
			IntervalLowerBound<double> lowerZeroEx = new(0, false, cmp);

			IntervalUpperBound<double> UpperZeroIn = new(0, true, cmp);
			IntervalUpperBound<double> UpperZeroEx = new(0, false, cmp);

			IntervalUpperBound<double> UpperTenIn = new(10, true, cmp);
			IntervalUpperBound<double> UpperTenEx = new(10, false, cmp);

			IntervalLowerBound<double> lowerTenIn = new(10, true, cmp);
			IntervalLowerBound<double> lowerTenEx = new(10, false, cmp);

			Assert.AreEqual(0, lowerZeroIn.Compare(UpperZeroIn));
			Assert.AreEqual(0, UpperZeroIn.Compare(lowerZeroIn));

			Assert.IsTrue(lowerZeroIn == UpperZeroIn);
			Assert.IsTrue(UpperZeroIn == lowerZeroIn);

			Assert.AreEqual(+1, lowerZeroEx.Compare(UpperZeroEx));
			Assert.AreEqual(-1, UpperZeroEx.Compare(lowerZeroEx));

			Assert.IsTrue(lowerZeroEx > UpperZeroEx);
			Assert.IsTrue(UpperZeroEx < lowerZeroEx);

			Assert.AreEqual(+1, lowerZeroIn.Compare(UpperZeroEx));
			Assert.AreEqual(-1, UpperZeroEx.Compare(lowerZeroIn));

			Assert.IsTrue(lowerZeroIn > UpperZeroEx);
			Assert.IsTrue(UpperZeroEx < lowerZeroIn);

			Assert.AreEqual(+1, lowerZeroEx.Compare(UpperZeroIn));
			Assert.AreEqual(-1, UpperZeroIn.Compare(lowerZeroEx));

			Assert.IsTrue(lowerZeroEx > UpperZeroIn);
			Assert.IsTrue(UpperZeroIn < lowerZeroEx);

			Assert.AreEqual(-1, lowerZeroEx.Compare(UpperTenEx));
			Assert.AreEqual(+1, UpperTenEx.Compare(lowerZeroEx));

			Assert.IsTrue(lowerZeroEx < UpperTenEx);
			Assert.IsTrue(UpperTenEx > lowerZeroEx);
		}

		[TestMethod]
		public void TestInterval()
		{
			Interval<double> interval_i0_0i = new(0, 0, true, true);
			Interval<double> interval_i0_10i = new(0, 10, true, true);
			Interval<double> interval_i0_10e = new(0, 10, true, false);
			Interval<double> interval_e0_10i = new(0, 10, false, true);
			Interval<double> interval_e0_10e = new(0, 10, false, false);

			Interval<double> interval_i5_15i = new(5, 15, true, true);
			Interval<double> interval_i5_15e = new(5, 15, true, false);
			Interval<double> interval_e5_15i = new(5, 15, false, true);
			Interval<double> interval_e5_15e = new(5, 15, false, false);

			Assert.IsTrue(interval_i0_0i.HasZeroLength);
			Assert.IsFalse(interval_i0_10i.HasZeroLength);

			Assert.IsTrue(interval_i0_10i.Contains(10));
			Assert.IsTrue(interval_i0_10i.Contains(8));
			Assert.IsTrue(interval_i0_10i.Contains(0));
			Assert.IsFalse(interval_i0_10i.Contains(-1));
			Assert.IsFalse(interval_i0_10i.Contains(10.1));

			Assert.IsTrue(interval_i0_0i.Contains(0));
			Assert.IsFalse(interval_i0_0i.Contains(-1e-16));
			Assert.IsFalse(interval_i0_0i.Contains(1e-16));

			Assert.IsTrue(interval_i0_0i.Contains(new IntervalLowerBound<double>(0, true)));
			Assert.IsTrue(interval_i0_0i.Contains(new IntervalUpperBound<double>(0, true)));
			Assert.IsFalse(interval_i0_0i.Contains(new IntervalLowerBound<double>(0, false)));
			Assert.IsFalse(interval_i0_0i.Contains(new IntervalUpperBound<double>(0, false)));

			Assert.IsTrue(interval_i0_10i.Contains(interval_i0_0i));
			Assert.IsTrue(interval_i0_10i.Contains(interval_i0_10e));
			Assert.IsTrue(interval_i0_10i.Contains(interval_e0_10i));
			Assert.IsTrue(interval_i0_10i.Contains(interval_e0_10e));

			Assert.IsFalse(interval_i0_10e.Contains(interval_i0_10i));

			Assert.IsTrue(interval_i0_10i.Intersects(interval_i5_15i));

			Interval<double> a = interval_i0_10e.Intersection(interval_i5_15e);
			Assert.AreEqual(5, a.Start.Bound);
			Assert.AreEqual(true, a.Start.IsInclusive);
			Assert.AreEqual(10, a.End.Bound);
			Assert.AreEqual(false, a.End.IsInclusive);

			a = interval_i0_10i.Intersection(interval_i5_15e);
			Assert.AreEqual(5, a.Start.Bound);
			Assert.AreEqual(true, a.Start.IsInclusive);
			Assert.AreEqual(10, a.End.Bound);
			Assert.AreEqual(true, a.End.IsInclusive);

			a = interval_i0_10e.Intersection(interval_e5_15e);
			Assert.AreEqual(5, a.Start.Bound);
			Assert.AreEqual(false, a.Start.IsInclusive);
			Assert.AreEqual(10, a.End.Bound);
			Assert.AreEqual(false, a.End.IsInclusive);

			a = interval_i0_10i.Intersection(interval_e5_15e);
			Assert.AreEqual(5, a.Start.Bound);
			Assert.AreEqual(false, a.Start.IsInclusive);
			Assert.AreEqual(10, a.End.Bound);
			Assert.AreEqual(true, a.End.IsInclusive);
		}


		[TestMethod]
		public void TestUnionOf()
		{
			Action<IEnumerable<Interval<int>>, TestIntervals<int>> DoTest = (intervals, result) =>
			{
				TestIntervals<int> interval = new(intervals.CreateUnion<Interval<int>, int>((a, b, start, end) => new Interval<int>(start, end)));
				Assert.IsTrue(Equal<int>(interval, result));
			};

			DoTest(
				CreateUnsortedIntervalEnumerable(10, 20),
				CreateIntervals(10, 20));

			DoTest(
				CreateUnsortedIntervalEnumerable(10, 20, -30, 40),
				CreateIntervals(10, 20, -30, 40));

			DoTest(
				CreateUnsortedIntervalEnumerable(-30, 40, 10, 20),
				CreateIntervals(10, 20, -30, 40));

			DoTest(
				CreateUnsortedIntervalEnumerable(-30, 40, 10, 20, 15, -25),
				CreateIntervals(10, -25, -30, 40));

			DoTest(
				CreateUnsortedIntervalEnumerable(-30, 40, 10, 20, 15, -25, 28, 30),
				CreateIntervals(10, -25, 28, 40));

			DoTest(
				CreateUnsortedIntervalEnumerable(30, 40, 10, 20, 15, -25, 28, 30),
				CreateIntervals(10, -25, 28, 40));

			DoTest(
				CreateUnsortedIntervalEnumerable(-30, 40, 10, 20, 15, -25, 28, -30),
				CreateIntervals(10, -25, 28, -30, -30, 40));

			DoTest(
				CreateUnsortedIntervalEnumerable(30, 40, 10, 20, 12, 16, 28, -30),
				CreateIntervals(10, 20, 28, 40));
		}


		[TestMethod]
		public void TestIntersectWith()
		{
			Action<TestIntervals<int>, TestIntervals<int>, TestIntervals<int>> DoTest = (interval, intersect, result) =>
			{
				interval.IntersectWith(intersect);
				Assert.IsTrue(Equal<int>(interval, result));
			};

			DoTest(
				CreateIntervals(10, 20),
				CreateIntervals(1, 3),
				CreateIntervals());

			DoTest(
				CreateIntervals(10, 20),
				CreateIntervals(12, 15),
				CreateIntervals(12, 15));

			DoTest(
				CreateIntervals(10, 20, 30, 40),
				CreateIntervals(15, 35),
				CreateIntervals(15, 20, 30, 35));

			DoTest(
				CreateIntervals(10, -20, -30, 40),
				CreateIntervals(20, 30),
				CreateIntervals());

			DoTest(
				CreateIntervals(10, 20, -30, 40),
				CreateIntervals(20, 30),
				CreateIntervals(20, 20));

			DoTest(
				CreateIntervals(10, 20, 30, 40),
				CreateIntervals(20, 30),
				CreateIntervals(20, 20, 30, 30));
		}


		[TestMethod]
		public void TestIntervalsMinus()
		{
			Action<TestIntervals<int>, TestIntervals<int>, TestIntervals<int>> DoTest = (interval, minus, result) =>
			{
				interval.RemoveIntervals(minus);
				Assert.IsTrue(Equal<int>(interval, result));
			};

			DoTest(
				CreateIntervals(10, 20),
				CreateIntervals(1, 3),
				CreateIntervals(10, 20));

			DoTest(
				CreateIntervals(10, 20),
				CreateIntervals(8, 15),
				CreateIntervals(-15, 20));

			DoTest(
				CreateIntervals(10, 20),
				CreateIntervals(8, -15),
				CreateIntervals(+15, 20));

			DoTest(
				CreateIntervals(10, 20),
				CreateIntervals(12, 15),
				CreateIntervals(10, -12, -15, 20));

			DoTest(
				CreateIntervals(10, 20),
				CreateIntervals(12, 12),
				CreateIntervals(10, -12, -12, 20));

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
				CreateIntervals(-15, 25),
				CreateIntervals(10, 15));

			DoTest(
				CreateIntervals(10, 20),
				CreateIntervals(-20, 25),
				CreateIntervals(10, 20));

			DoTest(
				CreateIntervals(10, 20),
				CreateIntervals(20, 25),
				CreateIntervals(10, -20));

			DoTest(
				CreateIntervals(-10, 20),
				CreateIntervals(22, 25),
				CreateIntervals(-10, 20));

			DoTest(
				CreateIntervals(10, 20, 30, 40, 50, 60, 70, 80, 90, 100),
				CreateIntervals(1, 3, 8, 15, 33, 37, 55, 65, 70, 80, 110, 120),
				CreateIntervals(-15, 20, 30, -33, -37, 40, 50, -55, 90, 100));

			DoTest(
				CreateIntervals(10, 20, -30, 40, 50, 60, -70, 80, 90, 100),
				CreateIntervals(1, 3, 8, -15, -33, 37, -55, 65, 70, -80, 110, 120),
				CreateIntervals(15, 20, -30, 33, -37, 40, 50, 55, 80, 80, 90, 100));

			DoTest(
				CreateIntervals(10, 20, 30, 40),
				CreateIntervals(-15, 40),
				CreateIntervals(10, 15));

			DoTest(
				CreateIntervals(10, 20, 30, 40),
				CreateIntervals(15, -40),
				CreateIntervals(10, -15, 40, 40));

			DoTest(
				CreateIntervals(10, 20, 30, 40),
				CreateIntervals(1, 35),
				CreateIntervals(-35, 40));

			DoTest(
				CreateIntervals(10, 20, 30, 40),
				CreateIntervals(1, -35),
				CreateIntervals(35, 40));
		}

	}
}
