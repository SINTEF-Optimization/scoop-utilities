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
	public class ComparerTest
	{
		readonly int[] list1 = new int[0];
		readonly int[] list2 = new int[] { 1, 2 };
		readonly int[] list3 = new int[] { 2 };
		readonly int[] list4 = new int[] { 2, 2 };
		readonly int[] list5 = new int[] { 2, 4 };

		class ReverseIntComparer : IComparer<int>
		{
			readonly IComparer<int> _cmp = Comparer<int>.Default;

			public int Compare(int x, int y)
			{
				return _cmp.Compare(y, x);
			}
		}


		[TestMethod]
		public void TestCompareByProperty()
		{
			Tuple<int, string> x = new(5, "h");
			Tuple<int, string> y = new(7, "h");
			Tuple<int, string> z = new(6, "ah");

			var all = new[] { x, y, z }.ToList();

			Func<string> status = () => all.Select(t => string.Format("{0}{1}", t.Item1, t.Item2)).Concatenate(",");

			var byInt = x.CompareBy(t => t.Item1);
			var byIntReverse = x.CompareByReverse(t => t.Item1);
			var byString = Compare<Tuple<int, string>>.By(t => t.Item2);
			var byStringThenInt = byString.ThenBy(t => t.Item1);
			var byStringThenIntReverse = byString.ThenByReverse(t => t.Item1);

			var firstShortThen7 = x.ComparePrefer(t => t.Item2.Length == 1).ThenPrefer(t => t.Item1 == 7);
			var first5ThenShort = Compare<Tuple<int, string>>.Prefer(t => t.Item1 == 5).ThenPrefer(t => t.Item2.Length == 1);

			CheckSort(all, status, byInt, "5h,6ah,7h");
			CheckSort(all, status, byIntReverse, "7h,6ah,5h");
			CheckSort(all, status, byStringThenInt, "6ah,5h,7h");
			CheckSort(all, status, byStringThenIntReverse, "6ah,7h,5h");
			CheckSort(all, status, firstShortThen7, "7h,5h,6ah");
			CheckSort(all, status, first5ThenShort, "5h,7h,6ah");
		}

		private static void CheckSort(List<Tuple<int, string>> all, Func<string> status, IComparerAndGenericComparer<Tuple<int, string>> comparer, string expectedStatus)
		{
			all.Sort(comparer);

			Assert.AreEqual(expectedStatus, status());

			all.Reverse();
			all.Sort(comparer);

			Assert.AreEqual(expectedStatus, status());
		}

		[TestMethod]
		public void TestLexicalCompare()
		{
			LexicalComparer<int> cmp = new();

			var listsInOrder = new int[][] { list1, list2, list3, list4, list5 };

			VerifyOrder(cmp, listsInOrder);
		}

		[TestMethod]
		public void TestLexicalCompareWithCustomElementComparer()
		{
			LexicalComparer<int> cmp = new(new ReverseIntComparer());

			var listsInOrder = new int[][] { list1, list3, list5, list4, list2 };

			VerifyOrder(cmp, listsInOrder);
		}

		[TestMethod]
		public void TestLexicalCompareWithEmptySequenceLast()
		{
			LexicalComparer<int> cmp = new(emptySequenceIsLargest: true);

			var listsInOrder = new int[][] { list2, list3, list4, list5, list1 };

			VerifyOrder(cmp, listsInOrder);
		}

		[TestMethod]
		public void TestLexicalCompareWithCustomElementComparerAndEmptySequenceLast()
		{
			LexicalComparer<int> cmp = new(new ReverseIntComparer(), emptySequenceIsLargest: true);

			var listsInOrder = new int[][] { list3, list5, list4, list2, list1 };

			VerifyOrder(cmp, listsInOrder);
		}

		private static void VerifyOrder(LexicalComparer<int> cmp, int[][] listsInOrder)
		{
			int n = listsInOrder.Length;
			for (int i = 0; i < n; ++i)
			{
				for (int j = 0; j < n; ++j)
				{
					var cmpVal = cmp.Compare(listsInOrder[i], listsInOrder[j]);
					if (i < j)
						Assert.AreEqual(-1, cmpVal);
					else if (i > j)
						Assert.AreEqual(1, cmpVal);
					else
						Assert.AreEqual(0, cmpVal);
				}
			}
		}
	}
}
