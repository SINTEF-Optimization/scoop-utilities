//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Sintef.Scoop.Utilities.Tests
{
	[TestClass]
	public class TestDictionaryExtensions
	{
		[TestMethod]
		public void TestWithDefaultValue()
		{
			Dictionary<int, int> dict = new()
			{
				{ 1, 2 }
			};

			var dict2 = dict.WithDefaultFor(new int[] { 1, 2 }, 4);

			Assert.AreEqual(2, dict2[1]); // 1 had a value -- is untouched
			Assert.AreEqual(4, dict2[2]); // 2 had no value -- got the default

			dict2 = dict.WithDefaultFor(new int[] { 1, 2 });

			Assert.AreEqual(2, dict2[1]);
			Assert.AreEqual(default, dict2[2]);
		}
	}
}
