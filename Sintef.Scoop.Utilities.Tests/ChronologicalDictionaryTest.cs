//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Sintef.Scoop.Utilities.Tests
{
	/// <summary>
	/// Tests for the ChronologicalDictionary class
	/// </summary>
	[TestClass]
	public class ChronologicalDictionaryTest
	{
		/// <summary>
		/// Tests that values in a ChronologicalDictionary can be added, updated and removed,
		/// and that the iteration order is as expected.
		/// </summary>
		[TestMethod]
		public void ChronologicalDictionaryAddsRemovesUpdatesCorrectly()
		{
			ChronologicalDictionary<string, string> _dict = new ChronologicalDictionary<string, string>();

			// Two ways to add, key indexing and Add()
			_dict["Blue"] = "Norway";
			_dict.Add("Green", "Denmark");
			_dict["Red"] = "Sweden";
			_dict.Add("Yellow", "Finland");
			_dict["White"] = "Iceland";

			// Fail to use Add() if key is previously added
			try
			{
				_dict.Add("Red", "England");
				Assert.Fail("ChronologicalDictionary.Add() on existing key did not throw an exception");
			}
			catch (ArgumentException) { }

			// Update value
			Assert.AreEqual("Denmark", _dict["Green"], "Unexpected value in ChronologicalDictionary before update");
			_dict["Green"] = "Germany";
			Assert.AreEqual("Germany", _dict["Green"], "Unexpected value in ChronologicalDictionary after update");

			// Remove value
			Assert.IsTrue(_dict.Remove("Yellow"), "Removing existing key 'Yellow' from ChronologicalDictionary failed");
			Assert.IsFalse(_dict.Remove("Pink"), "Removing non-existing key from ChronologicalDictionary did not fail");

			// Remove and reinsert value
			Assert.IsTrue(_dict.Remove("Red"), "Removing existing key 'Red' from ChronologicalDictionary failed");
			_dict["Red"] = "Netherlands";

			// Test iteration order of values and keys. Should be in the key inserted order, a removed
			// and reinserted key should be at the end of the iteration order at the time of the insertion
			string[] expectedKeysOrdered = new string[] { "Blue", "Green", "White", "Red"};
			string[] expectedValuesOrdered = new string[] { "Norway", "Germany", "Iceland", "Netherlands" };

			CollectionAssert.AreEqual(expectedKeysOrdered, _dict.Keys.ToArray(), "Keys in ChronologicalDictionary are not in expected order");
			CollectionAssert.AreEqual(expectedValuesOrdered, _dict.ToArray(), "Values in ChronologicalDictionary are not in expected order");
		}
	}
}
