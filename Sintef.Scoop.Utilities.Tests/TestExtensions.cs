//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace Sintef.Scoop.Utilities.Tests
{
	[TestClass]
	public class TestExtensions
	{
		private TimeSpan TestTimeSpanString(string s, bool expectSameConvertedBack)
		{
			TimeSpan ts = s.ParseISOTimeSpan();
			string convertedBack = ts.ToISOString();
			if (expectSameConvertedBack)
				Assert.AreEqual(convertedBack, s);
			return ts;
		}

		/// <summary>
		/// Tests the extensions RandomElement and RandomElements.
		/// </summary>
		[TestMethod]
		public void TestRandomElements()
		{
			int n = 10;
			List<int> collection = new List<int>();
			for (int i = 0; i < n; i++)
			{
				collection.Add(i);
			}

			RandomCreator.GlobalSeedUsageForRandomCreation = RandomCreator.SeedType.FIXED;
			Random rand = RandomCreator.GetRandomGenerator();

			int numEls = (int)n / 2;
			for (int i = 0; i < 100; i++)
			{
				int e = collection.RandomElement(rand);
				Assert.IsTrue(e >= 0 && e <= n);

				List<int> es = collection.RandomElements(numEls, rand).ToList();
				Assert.IsTrue(es.Count == numEls);

				Assert.IsTrue(es.AllDifferent());

				//Check that the results are in the same order as the original
				int prev = -1;
				es.Do(el => { Assert.IsTrue(el > prev); prev = el; });


				//Check special cases
				//number of elements >= n:
				List<int> shouldBeAll = collection.RandomElements(n, rand).ToList();
				Assert.IsTrue(shouldBeAll.SetEquals(collection));
				prev = -1;
				shouldBeAll.Do(el => { Assert.IsTrue(el > prev); prev = el; });
				shouldBeAll = collection.RandomElements(n + 1, rand).ToList();
				Assert.IsTrue(shouldBeAll.SetEquals(collection));
				prev = -1;
				shouldBeAll.Do(el => { Assert.IsTrue(el > prev); prev = el; });

			}





		}

		[TestMethod]
		public void TestISOTimeSpan()
		{
			string[] timeSpanStrings = new string[] { "PT10H5M16S", "P15DT7S", "P0MT1M", "P0YT1M", "P0Y0MT1S" };
			TimeSpan[] timeSpans = new TimeSpan[] { new TimeSpan(10, 5, 16), new TimeSpan(15, 0, 0, 7), new TimeSpan(0, 1, 0), new TimeSpan(0, 1, 0), new TimeSpan(0, 0, 1) };
			bool[] expectSameConvertedBack = new bool[] { true, true, false, false, false };
			string[] failTimeSpanStrings = new string[] { "01:23:00", "P1YT1M", "P1MT1M" };

			// Check that the following conversions pass and return same value when converted back when that can be expected
			for (int i = 0; i < timeSpanStrings.Length; ++i)
			{
				TimeSpan result = TestTimeSpanString(timeSpanStrings[i], expectSameConvertedBack[i]);
				Assert.AreEqual(result, timeSpans[i]);
			}

			// Check timespan lasting over month / year, we expect it to not use month / year
			// since those are not well defined
			TimeSpan[] longSpans = new TimeSpan[] { TimeSpan.FromDays(423), TimeSpan.FromDays(45), TimeSpan.FromDays(4356647) };

			foreach (var span in longSpans)
			{
				string s = span.ToISOString();
				Assert.IsFalse(s.Contains('Y'));
				Assert.IsFalse(s.Contains('M'));
			}

			// Check that conversions fail as expected. We want the Y and M date conversions to fail since they are not well defined.
			for (int i = 0; i < failTimeSpanStrings.Length; ++i)
			{
				bool fail = false;
				try
				{
					TimeSpan t = failTimeSpanStrings[i].ParseISOTimeSpan();
					Console.WriteLine(t.ToInvariantString());
				}
				catch (Exception)
				{
					//Console.WriteLine(e.ToString());
					fail = true;
				}
				Assert.IsTrue(fail);
			}
		}

		[TestMethod]
		public void TestTableFormatter()
		{
			TableFormatter f = new TableFormatter();

			f.AddLine("a", "b", "c");
			List<string> ss = ["gg", "looooooong"];
			f.AddLine(ss);
			f.AddLine("jjj");

			var lines = f.FormattedLines.ToList();

			Assert.AreEqual(3, lines.Count);
			Assert.AreEqual("a     b            c", lines[0]);
			Assert.AreEqual("gg    looooooong    ", lines[1]);
			Assert.AreEqual("jjj                 ", lines[2]);

			f.Show();
		}

		[TestMethod]
		public void TestConcatSingleElement()
		{
			var numbers = new List<int> { 1, 2, 3 };

			var moreNumbers = numbers.Concat(4).ToArray();

			CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, moreNumbers);
		}

		[TestMethod]
		public void TestEnumerableCombine()
		{
			List<int> e1 = new List<int>() { 1, 3, 5, 7, 9 };
			List<int> e2 = new List<int>() { 2, 4, 6, 8 };

			List<List<int>> es = new List<List<int>>() { e1, e2 };

			List<int> result = es.Combine(i => i).ToList();
			for (int i = 1; i < 10; i++)
			{
				Assert.AreEqual(i, result[i - 1]);
			}

			//Then, with characters

			List<char> a1 = new List<char>() { 'c', 'e', 'g' };
			List<char> a2 = new List<char>() { 'b', 'd', 'f' };
			List<char> a3 = new List<char>() { 'a', 'h' };

			List<List<char>> aas = new List<List<char>>() { a1, a2, a3 };

			List<char> res = aas.Combine<char, int>(c => Convert.ToInt32(c)).ToList();
			int baseNumber = Convert.ToInt32('a') - 1;
			for (int i = 1; i < 9; i++)
			{
				Assert.AreEqual(Convert.ToChar(i + baseNumber), res[i - 1]);
			}
		}

		[TestMethod]
		public void BinaryFirstWorks()
		{
			// Vanilla cases. Note behaviour at endpoints due to endpoint being excluded
			Assert.AreEqual(1, (0, 5).BinaryFirst(x => true));
			Assert.AreEqual(1, (0, 5).BinaryFirst(x => x >= 1));
			Assert.AreEqual(2, (0, 5).BinaryFirst(x => x >= 2));
			Assert.AreEqual(3, (0, 5).BinaryFirst(x => x >= 3));
			Assert.AreEqual(4, (0, 5).BinaryFirst(x => x >= 4));
			Assert.AreEqual(5, (0, 5).BinaryFirst(x => false));

			var (min, max) = (int.MinValue, int.MaxValue);

			// Cases for maximal range
			Assert.AreEqual(min + 1, (min, max).BinaryFirst(x => true));
			Assert.AreEqual(0, (min, max).BinaryFirst(x => x >= 0));
			Assert.AreEqual(max, (min, max).BinaryFirst(x => false));

			// Cases for range near minInt
			Assert.AreEqual(min + 1, (min, min + 10).BinaryFirst(x => true));
			Assert.AreEqual(min + 5, (min, min + 10).BinaryFirst(x => x >= min + 5));
			Assert.AreEqual(min + 10, (min, min + 10).BinaryFirst(x => false));

			// Cases for range near maxInt
			Assert.AreEqual(max - 9, (max - 10, max).BinaryFirst(x => true));
			Assert.AreEqual(max - 5, (max - 10, max).BinaryFirst(x => x >= max - 5));
			Assert.AreEqual(max, (max - 10, max).BinaryFirst(x => false));

			// Cases for minimal range
			Assert.AreEqual(1, (0, 1).BinaryFirst(x => true));
			Assert.AreEqual(1, (0, 1).BinaryFirst(x => false));

			// Cases for invalid range
			TestUtils.ExpectException(() => (min, min).BinaryFirst(x => true), requiredMessage: "Invalid range");
			TestUtils.ExpectException(() => (0, 0).BinaryFirst(x => true));
			TestUtils.ExpectException(() => (max, max).BinaryFirst(x => true));
		}

		[TestMethod]
		public void BinaryLastWorks()
		{
			// Vanilla cases. Note behaviour at endpoints due to endpoint being excluded
			Assert.AreEqual(0, (0, 5).BinaryLast(x => false));
			Assert.AreEqual(1, (0, 5).BinaryLast(x => x <= 1));
			Assert.AreEqual(2, (0, 5).BinaryLast(x => x <= 2));
			Assert.AreEqual(3, (0, 5).BinaryLast(x => x <= 3));
			Assert.AreEqual(4, (0, 5).BinaryLast(x => x <= 4));
			Assert.AreEqual(4, (0, 5).BinaryLast(x => true));

			var (min, max) = (int.MinValue, int.MaxValue);

			// Cases for maximal range
			Assert.AreEqual(min, (min, max).BinaryLast(x => false));
			Assert.AreEqual(0, (min, max).BinaryLast(x => x <= 0));
			Assert.AreEqual(max - 1, (min, max).BinaryLast(x => true));

			// Cases for range near minInt
			Assert.AreEqual(min, (min, min + 10).BinaryLast(x => false));
			Assert.AreEqual(min + 5, (min, min + 10).BinaryLast(x => x <= min + 5));
			Assert.AreEqual(min + 9, (min, min + 10).BinaryLast(x => true));

			// Cases for range near maxInt
			Assert.AreEqual(max - 10, (max - 10, max).BinaryLast(x => false));
			Assert.AreEqual(max - 5, (max - 10, max).BinaryLast(x => x <= max - 5));
			Assert.AreEqual(max - 1, (max - 10, max).BinaryLast(x => true));

			// Cases for minimal range
			Assert.AreEqual(0, (0, 1).BinaryLast(x => false));
			Assert.AreEqual(0, (0, 1).BinaryLast(x => true));

			// Cases for invalid range
			TestUtils.ExpectException(() => (min, min).BinaryLast(x => true), requiredMessage: "Invalid range");
			TestUtils.ExpectException(() => (0, 0).BinaryLast(x => true));
			TestUtils.ExpectException(() => (max, max).BinaryLast(x => true));
		}

		[TestMethod]
		public void BinaryFirstForLongWorks()
		{
			// Vanilla cases. Note behaviour at endpoints due to endpoint being excluded
			Assert.AreEqual(1, (0L, 5L).BinaryFirst(x => true));
			Assert.AreEqual(1, (0L, 5L).BinaryFirst(x => x >= 1));
			Assert.AreEqual(2, (0L, 5L).BinaryFirst(x => x >= 2));
			Assert.AreEqual(3, (0L, 5L).BinaryFirst(x => x >= 3));
			Assert.AreEqual(4, (0L, 5L).BinaryFirst(x => x >= 4));
			Assert.AreEqual(5, (0L, 5L).BinaryFirst(x => false));

			var (min, max) = (long.MinValue, long.MaxValue);

			// Cases for maximal range
			Assert.AreEqual(min + 1, (min, max).BinaryFirst(x => true));
			Assert.AreEqual(0, (min, max).BinaryFirst(x => x >= 0));
			Assert.AreEqual(max, (min, max).BinaryFirst(x => false));

			// Cases for range near minInt
			Assert.AreEqual(min + 1, (min, min + 10).BinaryFirst(x => true));
			Assert.AreEqual(min + 5, (min, min + 10).BinaryFirst(x => x >= min + 5));
			Assert.AreEqual(min + 10, (min, min + 10).BinaryFirst(x => false));

			// Cases for range near maxInt
			Assert.AreEqual(max - 9, (max - 10, max).BinaryFirst(x => true));
			Assert.AreEqual(max - 5, (max - 10, max).BinaryFirst(x => x >= max - 5));
			Assert.AreEqual(max, (max - 10, max).BinaryFirst(x => false));

			// Cases for minimal range
			Assert.AreEqual(1, (0L, 1L).BinaryFirst(x => true));
			Assert.AreEqual(1, (0L, 1L).BinaryFirst(x => false));

			// Cases for invalid range
			TestUtils.ExpectException(() => (min, min).BinaryFirst(x => true), requiredMessage: "Invalid range");
			TestUtils.ExpectException(() => (0L, 0L).BinaryFirst(x => true));
			TestUtils.ExpectException(() => (max, max).BinaryFirst(x => true));
		}

		[TestMethod]
		public void BinaryLastForLongWorks()
		{
			// Vanilla cases. Note behaviour at endpoints due to endpoint being excluded
			Assert.AreEqual(0, (0L, 5L).BinaryLast(x => false));
			Assert.AreEqual(1, (0L, 5L).BinaryLast(x => x <= 1));
			Assert.AreEqual(2, (0L, 5L).BinaryLast(x => x <= 2));
			Assert.AreEqual(3, (0L, 5L).BinaryLast(x => x <= 3));
			Assert.AreEqual(4, (0L, 5L).BinaryLast(x => x <= 4));
			Assert.AreEqual(4, (0L, 5L).BinaryLast(x => true));

			var (min, max) = (long.MinValue, long.MaxValue);

			// Cases for maximal range
			Assert.AreEqual(min, (min, max).BinaryLast(x => false));
			Assert.AreEqual(0, (min, max).BinaryLast(x => x <= 0));
			Assert.AreEqual(max - 1, (min, max).BinaryLast(x => true));

			// Cases for range near minInt
			Assert.AreEqual(min, (min, min + 10).BinaryLast(x => false));
			Assert.AreEqual(min + 5, (min, min + 10).BinaryLast(x => x <= min + 5));
			Assert.AreEqual(min + 9, (min, min + 10).BinaryLast(x => true));

			// Cases for range near maxInt
			Assert.AreEqual(max - 10, (max - 10, max).BinaryLast(x => false));
			Assert.AreEqual(max - 5, (max - 10, max).BinaryLast(x => x <= max - 5));
			Assert.AreEqual(max - 1, (max - 10, max).BinaryLast(x => true));

			// Cases for minimal range
			Assert.AreEqual(0, (0L, 1L).BinaryLast(x => false));
			Assert.AreEqual(0, (0L, 1L).BinaryLast(x => true));

			// Cases for invalid range
			TestUtils.ExpectException(() => (min, min).BinaryLast(x => true), requiredMessage: "Invalid range");
			TestUtils.ExpectException(() => (0L, 0L).BinaryLast(x => true));
			TestUtils.ExpectException(() => (max, max).BinaryLast(x => true));
		}

		[TestMethod]
		public void BinaryFirstIndexWorks()
		{
			List<int> list = new() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

			for (int i = 0; i <= 10; ++i)
			{
				Assert.AreEqual(i, list.BinaryFirstIndex(x => x >= i));

				Assert.AreEqual(i, ((IList<int>)list).BinaryFirstIndex(x => x >= i));
				Assert.AreEqual(i, ((IReadOnlyList<int>)list).BinaryFirstIndex(x => x >= i));
				Assert.AreEqual(i, list.ToArray().BinaryFirstIndex(x => x >= i));

				Assert.AreEqual(i, list.BinaryFirstIndex(x => true, i));

				Assert.AreEqual(i, ((IList<int>)list).BinaryFirstIndex(x => true, i));
				Assert.AreEqual(i, ((IReadOnlyList<int>)list).BinaryFirstIndex(x => true, i));
				Assert.AreEqual(i, list.ToArray().BinaryFirstIndex(x => true, i));

				Assert.AreEqual(10, list.BinaryFirstIndex(x => false, i));

				Assert.AreEqual(10, ((IList<int>)list).BinaryFirstIndex(x => false, i));
				Assert.AreEqual(10, ((IReadOnlyList<int>)list).BinaryFirstIndex(x => false, i));
				Assert.AreEqual(10, list.ToArray().BinaryFirstIndex(x => false, i));
			}

			Assert.AreEqual(10, list.BinaryFirstIndex(x => false));

			Assert.AreEqual(10, ((IList<int>)list).BinaryFirstIndex(x => false));
			Assert.AreEqual(10, ((IReadOnlyList<int>)list).BinaryFirstIndex(x => false));
			Assert.AreEqual(10, list.ToArray().BinaryFirstIndex(x => false));
		}

		[TestMethod]
		public void BinaryLastIndexWorks()
		{
			List<int> list = new() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

			for (int i = 0; i <= 10; ++i)
			{
				Assert.AreEqual(i - 1, list.BinaryLastIndex(x => x < i));

				Assert.AreEqual(i - 1, ((IList<int>)list).BinaryLastIndex(x => x < i));
				Assert.AreEqual(i - 1, ((IReadOnlyList<int>)list).BinaryLastIndex(x => x < i));
				Assert.AreEqual(i - 1, list.ToArray().BinaryLastIndex(x => x < i));

				Assert.AreEqual(i - 1, list.BinaryLastIndex(x => false, i));

				Assert.AreEqual(i - 1, ((IList<int>)list).BinaryLastIndex(x => false, i));
				Assert.AreEqual(i - 1, ((IReadOnlyList<int>)list).BinaryLastIndex(x => false, i));
				Assert.AreEqual(i - 1, list.ToArray().BinaryLastIndex(x => false, i));

				Assert.AreEqual(9, list.BinaryLastIndex(x => true, i));

				Assert.AreEqual(9, ((IList<int>)list).BinaryLastIndex(x => true, i));
				Assert.AreEqual(9, ((IReadOnlyList<int>)list).BinaryLastIndex(x => true, i));
				Assert.AreEqual(9, list.ToArray().BinaryLastIndex(x => true, i));
			}

			Assert.AreEqual(9, list.BinaryLastIndex(x => true));

			Assert.AreEqual(9, ((IList<int>)list).BinaryLastIndex(x => true));
			Assert.AreEqual(9, ((IReadOnlyList<int>)list).BinaryLastIndex(x => true));
			Assert.AreEqual(9, list.ToArray().BinaryLastIndex(x => true));
		}

		[TestMethod]
		public void FullMessageWorks()
		{
			Exception inner = new Exception("inner message");
			Exception outer = new Exception("outer message", inner);

			Assert.AreEqual($"outer message{Environment.NewLine}inner message", outer.FullMessage());
		}

		[TestMethod]
		public void JoinWorks()
		{
			var strings = new[] { "a", "b", "c" };

			Assert.AreEqual("abc", strings.Join(""));
			Assert.AreEqual("a, b, c", strings.Join(", "));
		}

		[TestMethod]
		public void JoinStringsWorks()
		{
			Console.WriteLine(CultureInfo.CurrentCulture.Name);
			var source = new object[] { 5, "a", null };

			Assert.AreEqual("5 a <null>", source.JoinStrings(" "));
		}

		[TestMethod]
		public void JoinStringsUsesSuppliedCultureForReals()
		{
			var source = new object[] { 5, "a", null, 7.8, 1.2f };

			Assert.AreEqual("5 a <null> 7.8 1.2", source.JoinStrings(" ", CultureInfo.InvariantCulture));
			Assert.AreEqual("5 a <null> 7,8 1,2", source.JoinStrings(" ", CultureInfo.GetCultureInfo("nb-no")));
		}

		[TestMethod]
		public void AdjacentPairsWorks()
		{
			var strings = new[] { "a", "b", "c" };

			Assert.AreEqual("ab, bc", strings.AdjacentPairs().Select(p => $"{p.Item1}{p.Item2}").Join(", "));

			Assert.AreEqual(0, strings.Take(1).AdjacentPairs().Count());
			Assert.AreEqual(0, strings.Take(0).AdjacentPairs().Count());
		}

		[TestMethod]
		public void AdjacentPairsEnumeratesSourceJustOnce()
		{
			int counter = 0;
			var source = Enumerable.Range(0, 3).Select(_ => ++counter);

			Assert.AreEqual("12, 23", source.AdjacentPairs().Select(p => $"{p.Item1}{p.Item2}").Join(", "));
			Assert.AreEqual(3, counter);
		}

		[TestMethod]
		public void ToValuePairsWorks()
		{
			var source = new Tuple<string, string>[] { new("a", "b"), new("b", "c") };

			CollectionAssert.AreEqual(new[] { ("a", "b"), ("b", "c") }, source.ToValuePairs().ToList());
		}

		[TestMethod]
		public void DifferentCombinationsWorks()
		{
			Assert.AreEqual("", Combinations("a", 0));
			Assert.AreEqual("a", Combinations("a", 1));
			Assert.AreEqual("", Combinations("a", 2));

			Assert.AreEqual("", Combinations("ab", 0));
			Assert.AreEqual("a b", Combinations("ab", 1));
			Assert.AreEqual("ab", Combinations("ab", 2));
			Assert.AreEqual("", Combinations("ab", 3));

			Assert.AreEqual("", Combinations("abc", 0));
			Assert.AreEqual("a b c", Combinations("abc", 1));
			Assert.AreEqual("ab ac bc", Combinations("abc", 2));
			Assert.AreEqual("abc", Combinations("abc", 3));
			Assert.AreEqual("", Combinations("abc", 4));

			Assert.AreEqual("ab ac ad bc bd cd", Combinations("abcd", 2));

			static string Combinations(string elements, int size)
			{
				return elements.DifferentCombinations(size)
					.Select(chars => chars.JoinStrings(""))
					.Join(" ");
			}
		}

		[TestMethod]
		public void SubsequencesWorks()
		{
			Assert.AreEqual("abc d d", Subsequences("abc-d---d--"));
			Assert.AreEqual("abc d d", Subsequences("--abc-d---d"));
			Assert.AreEqual("abc", Subsequences("abc"));
			Assert.AreEqual("", Subsequences("--"));
			Assert.AreEqual("", Subsequences(""));

			static string Subsequences(string source)
			{
				return source.Subsequences(c => c != '-')
					.Select(chars => chars.JoinStrings(""))
					.Join(" ");
			}
		}

		[TestMethod]
		public void WithoutRepetitionsWorks()
		{
			Assert.AreEqual("abcd", "abcd".WithoutRepetitions().JoinStrings(""));
			Assert.AreEqual("abcd", "aabbccdd".WithoutRepetitions().JoinStrings(""));
			Assert.AreEqual("abcdcba", "abcddcba".WithoutRepetitions().JoinStrings(""));
			Assert.AreEqual("", "".WithoutRepetitions().JoinStrings(""));
		}

		[TestMethod]
		public void IsSublistOfWorks()
		{
			IsSublist("", "", true);
			IsSublist("", "1", true);
			IsSublist("1", "", false);
			IsSublist("12", "1122", true);
			IsSublist("1221", "1122", false);
			IsSublist("21", "12", false);
			IsSublist("1122", "3113223", true);

			static void IsSublist(string subList, string mainList, bool expectedResult)
			{
				var a = subList.Select(c => int.Parse($"{c}")).ToList();
				var b = mainList.Select(c => int.Parse($"{c}")).ToList();
				Assert.AreEqual(expectedResult, a.IsSublistOf(b));
			}
		}

		[TestMethod]
		public void SingleWithExceptionWorks()
		{
			Exception ex = new Exception();

			int[] source0 = null;
			var source1 = new int[] { };
			var source2 = new int[] { 1 };
			var source3 = new int[] { 1, 2 };

			AssertArgumentNull(() => source0.Single(ex));
			AssertException(() => source1.Single(ex));
			Assert.AreEqual(1, source2.Single(ex));
			AssertException(() => source3.Single(ex));

			AssertArgumentNull(() => source0.Single(x => x == 2, ex));
			AssertException(() => source1.Single(x => x == 2, ex));
			AssertException(() => source2.Single(x => x == 2, ex));
			Assert.AreEqual(2, source3.Single(x => x == 2, ex));

			AssertArgumentNull(() => source0.SingleOrDefault(ex));
			Assert.AreEqual(0, source1.SingleOrDefault(ex));
			Assert.AreEqual(1, source2.SingleOrDefault(ex));
			AssertException(() => source3.SingleOrDefault(ex));

			void AssertException(Func<int> p)
			{
				try
				{
					var x = p();
					Assert.Fail();
				}
				catch (Exception exception)
				{
					Assert.AreSame(ex, exception);
				}
			}

			void AssertArgumentNull(Func<int> p)
			{
				try
				{
					var x = p();
					Assert.Fail();
				}
				catch (ArgumentNullException)
				{
				}
			}
		}

		[TestMethod]
		public void IndexOfWorks()
		{
			IReadOnlyList<int> myList = [1, 2, 3, 4];

			Assert.AreEqual(2, myList.IndexOf(3));
			Assert.AreEqual(-1, myList.IndexOf(5));
		}

		[TestMethod]
		public void ToReadableStringIsCorrect()
		{
			var span = new TimeSpan(1, 2, 3, 4, 5);

			Assert.AreEqual("1d 2h 3m 4s", span.ToReadableString());

			Assert.AreEqual("1d 2h 3m 4s 5ms", span.ToReadableString(includeMilliseconds: true));

			Assert.AreEqual("-1d 2h 3m 4s", (-span).ToReadableString());

			Assert.AreEqual("0s", TimeSpan.Zero.ToReadableString());

			Assert.AreEqual("0ms", TimeSpan.Zero.ToReadableString(includeMilliseconds: true));

			Assert.AreEqual("1d", TimeSpan.FromDays(1).ToReadableString());
			Assert.AreEqual("1h", TimeSpan.FromHours(1).ToReadableString());
			Assert.AreEqual("1m", TimeSpan.FromMinutes(1).ToReadableString());
			Assert.AreEqual("1s", TimeSpan.FromSeconds(1).ToReadableString());

			Assert.AreEqual("0s", TimeSpan.FromSeconds(0.5).ToReadableString());
			Assert.AreEqual("500ms", TimeSpan.FromSeconds(0.5).ToReadableString(includeMilliseconds: true));
			Assert.AreEqual("-500ms", TimeSpan.FromSeconds(-0.5).ToReadableString(includeMilliseconds: true));
		}

		[TestMethod]
		public void SetEqualsIsCorrect()
		{
			var r = new Random();

			for (int i = 0; i < 100; ++i)
			{
				var list1 = Enumerable.Range(1, 5 + r.Next(10)).ToList();
				var list2 = list1.Shuffled(r).ToList();

				// Shuffled list is equivalent to original list
				Assert.IsTrue(list1.SetEquals(list2));
				Assert.IsTrue(list2.SetEquals(list1));

				// Removing an element makes it different
				var list3 = list1.Skip(1).ToList();
				AssertSetEquals(list3, false);

				// So does adding a new element
				var list4 = list1.Append(-4).ToList();
				AssertSetEquals(list4, false);

				// Duplicating an element does not
				var list5 = list1.Append(list1.RandomElement(r)).ToList();
				AssertSetEquals(list5, true);


				void AssertSetEquals(List<int> list, bool expectEqual)
				{
					Assert.AreEqual(expectEqual, list1.SetEquals(list));
					Assert.AreEqual(expectEqual, list2.SetEquals(list));
					Assert.AreEqual(expectEqual, list.SetEquals(list1));
					Assert.AreEqual(expectEqual, list.SetEquals(list2));
				}
			}
		}

		[TestMethod]
		public void SetEqualsIsCorrect_WithComparator()
		{
			var r = new Random();
			var comparator = new CompareLastDigit();

			var list1 = new[] { 1, 2, 3, 4, 5, 12, 13, 14, 15, 23, 24, 25 };
			var list2 = list1.Shuffled(r).ToList();

			// Shuffled list is equivalent to original list
			Assert.IsTrue(list1.SetEquals(list2, comparator));
			Assert.IsTrue(list2.SetEquals(list1, comparator));

			// Removing the only occurence of a last digit makes it different
			var list3 = list1.Skip(1).ToList();
			AssertSetEquals(list3, false);

			// So does adding a new last digit
			var list4 = list1.Append(8).ToList();
			AssertSetEquals(list4, false);

			// Duplicating an element does not
			var list5 = list1.Append(list1.RandomElement(r)).ToList();
			AssertSetEquals(list5, true);

			// Adding an element with same last digit does not
			var list6 = list1.Append(53).ToList();
			AssertSetEquals(list6, true);

			// Removing a digit with multiple occurrences
			var list7 = list1.Except(13).ToList();
			AssertSetEquals(list7, true);


			void AssertSetEquals(List<int> list, bool expectEqual)
			{
				Assert.AreEqual(expectEqual, list1.SetEquals(list, comparator));
				Assert.AreEqual(expectEqual, list2.SetEquals(list, comparator));
				Assert.AreEqual(expectEqual, list.SetEquals(list1, comparator));
				Assert.AreEqual(expectEqual, list.SetEquals(list2, comparator));
			}
		}

		[TestMethod]
		public void JsonGetPropertyValue()
		{
			string json = "{\n  \"username\": \"jane_doe\",\n  \"score\": 42,\n  \"negativescore\": -42,\n \"isActive\": true,\n \"number\": 33.7,\n" + 
			              "  \"eventTime\": \"2023-08-15T13:45:30.1000000Z\"\n,  \"eventTimeOffset\": \"2023-08-15T13:45:30.1000000+03:00\"\n}";
			
			DateTime dateTime = new DateTime(2023, 8, 15, 13, 45, 30, 100, DateTimeKind.Utc);
			DateTimeOffset dateTimeOffset = new DateTimeOffset(2023, 8, 15, 13, 45, 30, 100, TimeSpan.FromHours(3));
			
			using JsonDocument doc = JsonDocument.Parse(json);
			JsonElement root = doc.RootElement;
			
			Assert.AreEqual("jane_doe", root.GetPropertyValue<string>("username"));
			Assert.AreEqual(-42, root.GetPropertyValue<sbyte>("negativescore"));
			Assert.AreEqual(42u, root.GetPropertyValue<byte>("score"));
			Assert.AreEqual(-42, root.GetPropertyValue<short>("negativescore"));
			Assert.AreEqual(42u, root.GetPropertyValue<ushort>("score"));
			Assert.AreEqual(-42, root.GetPropertyValue<int>("negativescore"));
			Assert.AreEqual(42u, root.GetPropertyValue<uint>("score"));
			Assert.AreEqual(-42L, root.GetPropertyValue<long>("negativescore"));
			Assert.AreEqual(42ul, root.GetPropertyValue<ulong>("score"));
			Assert.IsTrue(root.GetPropertyValue<bool>("isActive"));
			Assert.AreEqual(33.7f, root.GetPropertyValue<float>("number"));
			Assert.AreEqual(33.7, root.GetPropertyValue<double>("number"));
			Assert.AreEqual(dateTime, root.GetPropertyValue<DateTime>("eventTime"));
			Assert.AreEqual(dateTime.ToDateTimeOffset(), root.GetPropertyValue<DateTimeOffset>("eventTime"));
			Assert.AreEqual(dateTimeOffset, root.GetPropertyValue<DateTimeOffset>("eventTimeOffset"));
			
			try
			{
				var result = root.GetPropertyValue<byte>("negativescore");
				Assert.Fail();
			}
			catch (InvalidOperationException)
			{
				// This is the expected exception type
			}
			catch (Exception e)
			{
				Assert.Fail("Unexpected exception: " + e);
			}
			
			try
			{
				var result = root.GetPropertyValue<ushort>("negativescore");
				Assert.Fail();
			}
			catch (InvalidOperationException)
			{
				// This is the expected exception type
			}
			catch (Exception e)
			{
				Assert.Fail("Unexpected exception: " + e);
			}
			
			try
			{
				var result = root.GetPropertyValue<uint>("negativescore");
				Assert.Fail();
			}
			catch (InvalidOperationException)
			{
				// This is the expected exception type
			}
			catch (Exception e)
			{
				Assert.Fail("Unexpected exception: " + e);
			}

			try
			{
				var result = root.GetPropertyValue<ulong>("negativescore");
				Assert.Fail();
			}
			catch (InvalidOperationException)
			{
				// This is the expected exception type
			}
			catch (Exception e)
			{
				Assert.Fail("Unexpected exception: " + e);
			}
			
			try
			{
				var result = root.GetPropertyValue<string>("isActive");
				Assert.Fail();
			}
			catch (InvalidOperationException)
			{
				// This is the expected exception type
			}
			catch (Exception e)
			{
				Assert.Fail("Unexpected exception: " + e);
			}

			try
			{
				var val = root.GetPropertyValue<string>("fictive");
				Assert.Fail();
			}
			catch (InvalidOperationException)
			{
				// This is the expected exception type
			}
			catch (Exception e)
			{
				Assert.Fail("Unexpected exception: " + e);
			}
		}

		[TestMethod]
		public void JSonGetPropertyValueOrDefault()
		{
			string json = "{\n  \"username\": \"jane_doe\",\n  \"score\": 42,\n  \"isActive\": true\n}";
			
			using JsonDocument doc = JsonDocument.Parse(json);
			JsonElement root = doc.RootElement;
			
			Assert.AreEqual("jane_doe", root.GetPropertyValueOrDefault<string>("username", ""));
			Assert.AreEqual(42, root.GetPropertyValueOrDefault<int>("score", 90));
			Assert.IsTrue(root.GetPropertyValueOrDefault<bool>("isActive", false));
			
			Assert.IsFalse(root.GetPropertyValueOrDefault<bool>("fictive", false));
			Assert.AreEqual("john_doe", root.GetPropertyValueOrDefault<string>("fictive", "john_doe"));

			try
			{
				var result = root.GetPropertyValueOrDefault<string>("score", "33");
				Assert.Fail();
			}
			catch (InvalidOperationException)
			{
				// This is the expected exception type
			}
			catch (Exception e)
			{
				Assert.Fail("Unexpected exception: " + e);
			}
		}
	}

	public class CompareLastDigit : IEqualityComparer<int>
	{
		public bool Equals(int x, int y)
		{
			return (x - y) % 10 == 0;
		}

		public int GetHashCode(int obj)
		{
			return obj % 10;
		}
	}
}