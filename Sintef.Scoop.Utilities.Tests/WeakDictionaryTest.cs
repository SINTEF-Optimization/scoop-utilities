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
	/// These tests rely on making the garbage collector to collect objects no longer referenced on demand.
	/// Future version of .NET may potentionally change how this works and these tests may break.
	/// </summary>
	[TestClass]
	public class WeakDictionaryTest
	{
		private Key[] _keys;

		private class Key(int key)
		{
			public int Value => key;
		}

		//[TestInitialize]
		public void Initialize()
		{
			int numberOfKeys = 10;

			_keys = new Key[numberOfKeys];

			for (int i = 0; i < numberOfKeys; i++)
			{
				_keys[i] = new Key(i);
			}
		}

		private WeakDictionary<Key, string> CreateAndDelete(params int[] keysToDelete)
		{
			Initialize();
			WeakDictionary<Key, string> dict = new();
			foreach (var key in _keys)
			{
				dict.Add(key, key.Value.ToString());
			}

			DeleteKeys(keysToDelete);

			return dict;
		}

		[TestMethod]
		public void EnumeratingKeysPurgeDeleted()
		{
			var dict = CreateAndDelete(4, 8);

			Collect();

			Assert.AreEqual(10, dict.Count);
			Assert.AreEqual(8, dict.Keys.Count);
			Assert.AreEqual(8, dict.Count);

			CheckThatAllNonNullKeysSurvive(dict);
		}

		[TestMethod]
		public void EnumeratingValuesPurgeDeleted()
		{
			var dict = CreateAndDelete(3, 2, 8);

			Collect();

			Assert.AreEqual(10, dict.Count);
			Assert.AreEqual(7, dict.Values.Count);
			Assert.AreEqual(7, dict.Count);

			CheckThatAllNonNullKeysSurvive(dict);
		}

		[TestMethod]
		public void EnumeratingDictionaryPurgeDeleted()
		{
			var dict = CreateAndDelete(1, 5, 9);

			Collect();

			Assert.AreEqual(10, dict.Count);

			var alive = dict.Select(x => x).Count();

			Assert.AreEqual(7, alive);
			Assert.AreEqual(7, dict.Count);

			CheckThatAllNonNullKeysSurvive(dict);
		}

		[TestMethod]
		public void PurgingDeadReferences()
		{
			var dict = CreateAndDelete(1, 5, 9);

			Collect();

			Assert.AreEqual(10, dict.Count);
			dict.RemoveDeadKeys();
			Assert.AreEqual(7, dict.Count);

			CheckThatAllNonNullKeysSurvive(dict);
		}

		[TestMethod]
		public void IndexingOperations()
		{
			var dict = CreateAndDelete([]);

			var newKey5String = "54545";
			dict[_keys[5]] = newKey5String;
			Assert.AreEqual(10, dict.Count);

			var newKey1 = new Key(333);
			dict[newKey1] = "333";
			var newKey2 = new Key(555);
			dict[newKey2] = "555";

			Assert.AreEqual(12, dict.Count);

			Assert.AreEqual(newKey5String, dict[_keys[5]]);

			Assert.AreEqual("333", dict[newKey1]);
			Assert.AreEqual("555", dict[newKey2]);
			for (int i = 0; i < _keys.Length; i++)
			{
				if (i != 5)
				{
					Assert.AreEqual(i.ToString(), dict[_keys[i]]);
				}
			}
		}

		[TestMethod]
		public void AddAndRemove()
		{
			var dict = CreateAndDelete([3]);

			Assert.AreEqual(10, dict.Count);

			// Add another key, the old key 3 is dead
			var newKey = new Key(11);
			dict.Add(newKey, "10");
			Assert.AreEqual(11, dict.Count);

			Collect();
			dict.RemoveDeadKeys();

			Assert.AreEqual(10, dict.Count);

			dict.Remove(_keys[0]);
			dict.Remove(_keys[7]);

			Assert.AreEqual(8, dict.Count);

			Assert.AreEqual("1", dict[_keys[1]]);
			Assert.AreEqual("2", dict[_keys[2]]);
			Assert.AreEqual("4", dict[_keys[4]]);
			Assert.AreEqual("5", dict[_keys[5]]);
			Assert.AreEqual("6", dict[_keys[6]]);
			Assert.AreEqual("8", dict[_keys[8]]);
			Assert.AreEqual("9", dict[_keys[9]]);
			Assert.AreEqual("10", dict[newKey]);
		}

		[TestMethod]
		public void SufficientAddAndRemoveTriggersDeadKeyRemoval()
		{
			var dict = CreateAndDelete([3, 5, 7]);

			Collect();

			Assert.AreEqual(10, dict.Count);

			var keys = new Key[10];
			for (int i = 0; i < 4; i++)
			{
				var key = new Key(i);
				keys[i] = key;
				dict.Add(key, i.ToString());
			}

			for (int i = 0; i < 4; ++i)
			{
				dict.Remove(keys[i]);
			}

			// While adding and removing there should be a collection of dead keys at some point.
			// This test depend on the knowledge that the dictionary collects dead keys for the first time after 16 add and remove calls.
			// It is difficult to test further collection since it proves difficult to force the GC to do further collection, at least if the
			// test is run in debug configuration.
			Assert.AreEqual(7, dict.Count);
		}

		/// <summary>
		/// Deletes the given set of keys.
		/// </summary>
		/// <param name="keyIndex"></param>
		private void DeleteKeys(params int[] keyIndex)
		{
			foreach (var index in keyIndex)
			{
				_keys[index] = null;
			}
		}

		/// <summary>
		/// Forces a garbage collection.
		/// </summary>
		private void Collect()
		{
			GC.Collect(2, GCCollectionMode.Forced, true);
		}

		/// <summary>
		/// Test that all keys that are not set to null in <see cref="_keys"/> collection are present and have correct value. Also tests that no
		/// other keys are present.
		/// </summary>
		/// <param name="dict">The dictionary to test.</param>
		private void CheckThatAllNonNullKeysSurvive(WeakDictionary<Key, string> dict)
		{
			int numberOfAliveKeys = 0;
			foreach (var (key, value) in dict)
			{
				Assert.IsNotNull(key);
				Assert.IsTrue(_keys.Contains(key));
				numberOfAliveKeys++;
			}
			int numberOfAliveReferences = _keys.Where(x => x is not null).Count();

			Assert.AreEqual(numberOfAliveKeys, numberOfAliveReferences);

			foreach (var key in _keys)
			{
				if (key is not null)
				{
					Assert.AreEqual(key.Value.ToString(), dict[key]);
				}
			}

			string keyString = string.Empty;
			_keys.Where(x => x is not null).Do(x => keyString += x.Value.ToString() + " ");
			Console.WriteLine($"{numberOfAliveKeys} keys still alive: {keyString}");
		}
	}
}
