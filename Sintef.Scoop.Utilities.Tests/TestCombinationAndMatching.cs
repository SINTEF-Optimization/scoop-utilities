//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sintef.Scoop.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities.Tests
{
	/// <summary>
	/// Summary description for TestOrderingAndMatching
	/// </summary>
	[TestClass]
	public class TestCombinationAndMatching
	{
		public TestCombinationAndMatching()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get;
			set;
		}

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void TestWeightedMatching()
		{
			//RandomCreator.SeedType oldSeedType = RandomCreator.GlobalSeedUsageForRandomCreation;
			//RandomCreator.GlobalSeedUsageForRandomCreation = RandomCreator.SeedType.FIXED;
			try
			{

				List<int> ints = new List<int>();
				List<char> chars = new List<char>();
				int max = 5;
				for (int i = 0; i < max; i++)
				{
					ints.Add(i);
					chars.Add((char)(Convert.ToInt32('a') + i));
				}

				//Make score matrix
				Dictionary<int, Dictionary<char, double>> scoreTable = new Dictionary<int, Dictionary<char, double>>();
				for (int i = 0; i < max; i++)
				{
					scoreTable[ints[i]] = new Dictionary<char, double>();
					for (int j = 0; j < max; j++)
					{
						scoreTable[ints[i]][chars[j]] = max - Math.Abs(i - j);
					}
				}
				ClassForScoringFunction<int, char> sfsf = new ClassForScoringFunction<int, char>(scoreTable);

				//Shuffle both lists
				RandomShuffle.Shuffle(ints);
				RandomShuffle.Shuffle(chars);

				//Get all matches
				List<List<Combination<int, char>>> allCombinations = CombinationsFromTwoLists<int, char>.GetAllCombinations(ints, chars);
				if (allCombinations.Count != Math.Min(ints.Count, chars.Count).Factorial())
					throw new Exception("The total number of combinations was not correct.");

		
				//Get all matches with weights
				List<Dictionary<Combination<int, char>, double>> allMatches = CombinationsFromTwoLists<int, char>.GetAllCombinationsWithWeights(ints, chars, sfsf.GetWeights);
				if (allMatches.Count != Math.Min(ints.Count,chars.Count).Factorial())
					throw new Exception("The total number of combinations with weights was not correct.");

				//Get best match
				double matchvalue;
				List<Combination<int, char>> bestMatch = CombinationsFromTwoLists<int, char>.GetOptimalMatching(ints, chars, sfsf.GetWeights, out matchvalue);

				//Check value
				if (matchvalue != 1)
					throw new Exception("The total value of the returned (expected perfect) match is wrong.");

				//Check match
				foreach (Combination<int, char> combo in bestMatch)
				{
					if (combo.GetSElement() != ((char)(Convert.ToInt32('a') + combo.GetTElement())))
						throw new Exception("The expected combination was not returned");
				}

			}
			finally
			{
//				RandomCreator.GlobalSeedUsageForRandomCreation = oldSeedType;
			}
		}

		class ClassForScoringFunction<T,S>
		{

			Dictionary<T, Dictionary<S, double>> _scoreTable;
			double _maxScore;

			public ClassForScoringFunction(Dictionary<T,Dictionary<S,double>> scoreTable)
			{
				_scoreTable = scoreTable;
				_maxScore = scoreTable.Values.Max(kvp => kvp.Values.Max());
			}

			/// <summary>
			/// Returns the score table value divided by the maximum value in that table, hence a value in [0,1].
			/// </summary>
			/// <param name="t"></param>
			/// <param name="s"></param>
			/// <returns></returns>
			public double GetWeights(T t, S s)
			{
				return _scoreTable[t][s] / _maxScore;
			}
		}

	}
}
