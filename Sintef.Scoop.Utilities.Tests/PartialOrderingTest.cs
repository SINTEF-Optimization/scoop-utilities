//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities.Tests;

[TestClass]
public class PartialOrderingTests
{
	[TestMethod]
	[Obsolete]
	public void DefaultComparerOrdersCorrectly()
	{
		// Arrange
		var source = new List<int> { 5, 3, 8, 1, 2 };
		var expected = new List<int> { 1, 2, 3, 5, 8 };

		// Act
		var result = source.PartialOrderBy(x => x).ToList();

		// Assert
		CollectionAssert.AreEqual(expected, result);
	}

	[TestMethod]
	[Obsolete]
	public void OrdersCorrectlyByKey()
	{
		// Arrange
		List<string> source = [ "apple", "banana", "cherry", "orange", "strawberry" ];

		Func<string, int> keySelector = (x) => x switch
		{
			"apple" => 1,
			"cherry" => -1,
			_ => 0
		};

		// Act
		var result = source.PartialOrderBy(keySelector).ToList();

		// Assert
		Assert.AreEqual("cherry", result.First());
		Assert.AreEqual("apple", result.Last());
	}

	[TestMethod]
	public void CustomComparerOrdersCorrectly()
	{
		// Arrange
		var source = new List<int> { 5, 3, 8, 1, 2 };
		var comparer = Comparer<int>.Create((x, y) =>
		{
			if (x == 1 && y == 3) return -1;
			if (x == 3 && y == 1) return 1;

			if (x == 5 && y == 2) return 1;
			if (x == 2 && y == 5) return -1;

			return 0;
		});
		// Act
		var result = source.PartialOrderBy(x => x, comparer).ToList();

		// Assert
		var indexOf1 = result.IndexOf(1);
		var indexOf2 = result.IndexOf(2);
		var indexOf3 = result.IndexOf(3);
		var indexOf5 = result.IndexOf(5);

		Assert.IsTrue(indexOf1 < indexOf3);
		Assert.IsTrue(indexOf2 < indexOf5);
	}

	[TestMethod]
	public void EmptySequenceReturnsEmpty()
	{
		// Arrange
		List<int> sequence = new();

		// Act
		var result = sequence.PartialOrderBy(Comparer<int>.Default).ToList();

		// Assert
		Assert.AreEqual(0, result.Count);
	}

	[TestMethod]
	public void SingleElementReturnsSingleElement()
	{
		// Arrange
		List<int> source = [42];

		// Act
		var result = source.PartialOrderBy(Comparer<int>.Default).ToList();

		// Assert
		Assert.AreEqual(1, result.Count);
		Assert.AreEqual(42, result[0]);
	}

	[TestMethod]
	public void DuplicateKeysOrdersCorrectly()
	{
		// Arrange
		var source = new List<int> { 5, 3, 8, 1, 2, 3, 1, 2 };
		var comparer = Comparer<int>.Create((x, y) =>
		{
			if (x == 1 && y == 3) return -1;
			if (x == 3 && y == 1) return 1;

			if (x == 5 && y == 2) return 1;
			if (x == 2 && y == 5) return -1;

			return 0;
		});

		// Act
		var result = source.PartialOrderBy(x => x, comparer).ToList();

		// Assert
		var firstIndexOf1 = result.IndexOf(1);
		var lastIndexOf1 = result.LastIndexOf(1);
		var firstIndexOf2 = result.IndexOf(2);
		var lastIndexOf2 = result.LastIndexOf(2);
		var firstIndexOf3 = result.IndexOf(3);
		var firstIndexOf5 = result.IndexOf(5);

		Assert.IsTrue(firstIndexOf1 < firstIndexOf3);
		Assert.IsTrue(lastIndexOf1 < firstIndexOf3);
		Assert.IsTrue(firstIndexOf2 < firstIndexOf5);
		Assert.IsTrue(lastIndexOf2 < firstIndexOf5);
	}
}
