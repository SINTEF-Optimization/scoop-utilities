//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sintef.Scoop.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities.Tests
{
	[TestClass]
	public class TestBufferEnumerator
	{
		string log = "";
		IEnumerable<int> source;
		IEnumerable<int> bufferedSource;

		[TestInitialize]
		public void Setup()
		{
			source = Enumerable.Range(1, 5).Select(i => { log += $"Extract {i} "; return i; });
			bufferedSource = source.Buffer();
		}

		[TestMethod]
		public void UnbufferedSourceIsEnumeratedTwice()
		{
			var taken = source.Take(2).Concat(source.Take(2)).ToList();

			Assert.AreEqual("Extract 1 Extract 2 Extract 1 Extract 2 ", log);
		}


		[TestMethod]
		public void BufferedSourceIsEnumeratedOnce()
		{
			var taken = bufferedSource.Take(2).Concat(bufferedSource.Take(2)).ToList();

			Assert.AreEqual("Extract 1 Extract 2 ", log);

			log = "";
			bufferedSource.Take(3).ToList();

			Assert.AreEqual("Extract 3 ", log);
		}
	}
}
