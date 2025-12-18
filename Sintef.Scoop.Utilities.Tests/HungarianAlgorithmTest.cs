//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sintef.Scoop.Utilities;
using System;
using System.Linq;
using System.Numerics;

namespace Sintef.Scoop.Utilities.Tests
{
	[TestClass]
	public class HungarianAlgorithmTest
	{
		private readonly uint[,] _costMatrixUint =
		{
			{ 45, 87, 22, 24 },
			{ 67, 89, 85, 9 },
			{ 2, 14, 75, 42 },
			{ 90, 84, 30, 94 }
		};

		private readonly double[,] _costMatrix =
		{
			{ 45, 87, 22, 24 },
			{ 67, 89, 85, 9 },
			{ 2, 14, 75, 42 },
			{ 90, 84, 30, 94 }
		};

		private readonly (int, int)[] _expectedPairs = [(0, 0), (2, 1), (3, 2), (1, 3)];

		private readonly double[,] _costMatrix2 =
		{
			{ 45, 87, 5, 44 },
			{ 67, 89, 85, 9 },
			{ 2, 43, 75, 42 },
			{ 90, 4, 50, 94 }
		};

		private readonly (int, int)[] _expectedPairs2 = [(2, 0), (3, 1), (0, 2), (1, 3)];
		private readonly double _expectedValue2 = 20;

		private readonly double[,] _costMatrix3 =
		{
			{ 35, 36, 36, 25 },
			{ 31, 31, 35, 23 },
			{ 33, 35, 39, 26 },
			{ 32, 38, 33, 24 }
		};
		private readonly (int, int)[] _expectedPairs3 = [(2,0), (1,1), (0,3), (3,2)];
		private readonly double _expectedValue3 = 122;
		

		private readonly double[,] _bigMatrix =
		{
			{ 93, 79, 21, 93, 62, 88, 76, 42, 21, 29 },
			{ 40, 60, 19, 36, 79, 63, 81, 28, 56, 4 },
			{ 20, 80, 72, 1, 65, 6, 7, 16, 4, 95 },
			{ 52, 42, 80, 98, 37, 7, 92, 29, 23, 75 },
			{ 81, 3, 70, 46, 9, 53, 55, 5, 37, 38 },
			{ 47, 31, 5, 96, 5, 90, 3, 77, 80, 40 },
			{ 18, 68, 71, 34, 85, 55, 81, 23, 5, 12 },
			{ 69, 78, 66, 89, 70, 44, 83, 35, 94, 51 },
			{ 63, 99, 2, 24, 74, 58, 27, 57, 88, 21 },
			{ 84, 93, 11, 2, 10, 7, 64, 3, 32, 75 }
		};

		private readonly (int, int)[] _expectedPairsBig =
		[
			(0, 8), (1, 9), (2, 6), (3, 5), (4, 1), (5, 4), (6, 0), (7, 7), (8, 2), (9, 3)
		];

		private readonly double _expectedCostBig = 104;

		private readonly double[,] _bigMatrix2 =
		{
			{ 48, 81, 36, 57, 49, 79, 46, 78, 10, 59 },
			{ 27, 94, 76, 34, 91, 70, 9, 47, 91, 50 },
			{ 20, 28, 12, 70, 92, 13, 10, 66, 5, 4 },
			{ 21, 42, 74, 65, 6, 29, 31, 87, 96, 71 },
			{ 98, 86, 80, 56, 87, 83, 88, 10, 60, 77 },
			{ 36, 97, 66, 64, 20, 99, 60, 78, 81, 81 },
			{ 10, 98, 36, 42, 7, 76, 64, 41, 46, 55 },
			{ 76, 21, 15, 19, 54, 96, 31, 80, 75, 88 },
			{ 94, 36, 9, 49, 16, 93, 98, 75, 89, 4 },
			{ 53, 7, 84, 85, 18, 9, 27, 28, 79, 11 }
		};

		private readonly (int, int)[] _expectedPairsBig2 =
		[
			(0, 8), (1, 6), (2, 9), (3, 5), (4, 7), (5, 4), (6, 0), (7, 3), (8, 2), (9, 1)
		];

		private readonly double _expectedCostBig2 = 127;

		private readonly double[,] _bigMatrix3 =
		{
			{ 39, 89, 85, 33, 79, 56, 33, 33, 29, 68 },
			{ 67, 11, 59, 56, 35, 92, 8, 14, 98, 96 },
			{ 47, 76, 39, 30, 21, 48, 35, 41, 1, 41 },
			{ 75, 51, 40, 17, 56, 41, 32, 62, 48, 36 },
			{ 85, 99, 69, 99, 69, 29, 96, 5, 41, 80 },
			{ 56, 74, 13, 69, 54, 52, 92, 62, 13, 52 },
			{ 14, 49, 18, 34, 29, 25, 19, 77, 1, 44 },
			{ 46, 86, 43, 64, 64, 48, 4, 19, 90, 20 },
			{ 68, 59, 41, 62, 69, 79, 30, 77, 90, 67 },
			{ 15, 85, 98, 21, 10, 87, 6, 61, 68, 20 }
		};

		private readonly (int, int)[] _expectedPairsBig3 =
		[
			(0, 0), (1, 1), (2, 8), (3, 3), (4, 7), (5, 2), (6, 5), (7, 9), (8, 6), (9, 4)
		];

		private readonly double _expectedCostBig3 = 171;
		
		private readonly double[,] _bigMatrix4 =
		{
			{ 0, 0, 60, 9, 50, 77, 10, 88, 39, 27 },
			{ 0, 0, 97, 57, 37, 91, 80, 4, 99, 83 },
			{ 0, 0, 52, 99, 53, 94, 26, 64, 45, 10 },
			{ 0, 0, 13, 63, 92, 85, 78, 26, 66, 54 },
			{ 0, 0, 91, 63, 29, 92, 28, 63, 10, 32 },
			{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
			{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
			{ 0, 0, 46, 59, 33, 60, 33, 90, 19, 85 },
			{ 0, 0, 32, 25, 85, 46, 80, 89, 10, 80 },
			{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
		};

		private readonly (int, int)[] _expectedPairsBig4 =
		[
			(0, 3), (1, 7), (2, 9), (3, 2), (4, 1), (5, 6), (6, 4), (7, 0), (8, 8), (9, 5)
		];

		private readonly double _expectedCostBig4 = 46;
		
		[TestMethod]
		public void Test4X4Case()
		{
			double cost;

			(_, _) = TestSolve(_costMatrix, _expectedPairs, true);
			(_, cost) = TestSolve(_costMatrix2, _expectedPairs2, true);
			Assert.AreEqual(_expectedValue2, cost);
			(_, cost) = TestSolve(_costMatrix3, _expectedPairs3, true);
			Assert.AreEqual(_expectedValue3, cost);
		}

		[TestMethod]
		public void TestBig()
		{
			double cost;

			(_, cost) = TestSolve(_bigMatrix, _expectedPairsBig, false); // Multiple best solutions, don't compare the solution itself just the value
			Assert.AreEqual(_expectedCostBig, cost);
			(_, cost) = TestSolve(_bigMatrix2, _expectedPairsBig2, true);
			Assert.AreEqual(_expectedCostBig2, cost);
			(_, cost) = TestSolve(_bigMatrix3, _expectedPairsBig3, true);
			Assert.AreEqual(_expectedCostBig3, cost);
			(_, cost) = TestSolve(_bigMatrix4, _expectedPairsBig4, false);
			Assert.AreEqual(_expectedCostBig4, cost);
		}

		[TestMethod]
		[DataRow(100)]
		[DataRow(101)]
		[DataRow(102)]
		[DataRow(103)]
		[DataRow(104)]
		[DataRow(105)]
		[DataRow(106)]
		[DataRow(107)]
		[DataRow(108)]
		[DataRow(109)]
		[DataRow(110)]
		[DataRow(997)]
		public void TestVeryLarge(int size)
		{
			var problem = CreateRandomProblem(size, size);
			
			// Test different implementations against each other
			var result = Hungarian.Solve(problem, false);
			var result2 = Hungarian.Solve(problem);
			var result3 = Hungarian.SolveGeneric(problem);
			
			Assert.AreEqual(size, result.Length);
			Assert.AreEqual(size, result2.Length);
			Assert.AreEqual(size, result3.Length);

			for (int i = 0; i < size; i++)
			{
				Assert.AreEqual(result[i], result2[i]);
				Assert.AreEqual(result[i], result3[i]);
			}
			
			// Cost best solution
			var bestCost = Cost(problem, result);
			
			// Create a new problem with a known best solution
			// Calculate value for the desired best solution (diagonal)
			
			var diagonalSolution = Enumerable.Range(0, size).ToArray();
			
			var diagonalCost = Cost(problem, diagonalSolution);
			
			// Adjust matrix to make diagonal optimal
			var delta = 2 * (diagonalCost - bestCost) / size;
			for (int i = 0; i < size; i++)
			{
				problem[i, i] -= delta;
			}
			
			result = Hungarian.Solve(problem, false);

			var newDiagonalCost = Cost(problem, diagonalSolution);

			double costResult = Cost(problem, result);

			if (costResult >= newDiagonalCost)
			{
				// Test that the best solution is the diagonal on the adjusted matrix as long as it did not find an even better solution
				Assert.AreEqual(newDiagonalCost, costResult, 1E-10);
			
				for (int i = 0; i < size; i++)
				{
					Assert.AreEqual(i, result[i]);
				}
			}
		}

		[TestMethod]
		public void TestNegative()
		{
			double cost;
			
			var negativeMatrix = Subtract(_bigMatrix, 50);
			(_, cost) = TestSolve(negativeMatrix, _expectedPairsBig, false); // Multiple best solutions, don't compare the solution itself just the value
			Assert.AreEqual(_expectedCostBig, cost + 50*10);
			var negativeMatrix2 = Subtract(_bigMatrix2, 50);
			(_, cost) = TestSolve(negativeMatrix2, _expectedPairsBig2, true);
			Assert.AreEqual(_expectedCostBig2, cost + 50*10);
			var negativeMatrix3 = Subtract(_bigMatrix3, 50);
			(_, cost) = TestSolve(negativeMatrix3, _expectedPairsBig3, true);
			Assert.AreEqual(_expectedCostBig3, cost + 50*10);
			var negativeMatrix4 = Subtract(_bigMatrix4, 50);
			(_, cost) = TestSolve(negativeMatrix4, _expectedPairsBig4, false); // Multiple best solutions, don't compare the solution itself just the value
			Assert.AreEqual(_expectedCostBig4, cost + 50*10);
		}

		[TestMethod]
		public void TestFloatVsDouble()
		{
			int size = 1000;
			var floatProblem = CreateRandomFloatProblem(size, size);
			var doubleProblem = CreateRandomProblem(size, size);
			
			var result = Hungarian.Solve(floatProblem);
			var result2 = Hungarian.Solve(doubleProblem);
			
			Assert.AreEqual(size, result.Length);
			Assert.AreEqual(size, result2.Length);

			for (int i = 0; i < size; i++)
			{
				Assert.AreEqual(result[i], result2[i]);
			}
		}

		[TestMethod]
		public void TestUint()
		{
			TestSolve(_costMatrixUint, _expectedPairs, true);
		}

		/// <summary>
		/// Creates a new matrix with the same dimensions as the source matrix, and sets each element to the value of the original matrix minus the given value.
		/// </summary>
		/// <param name="matrix">The original matrix.</param>
		/// <param name="value">The value to subtract from each element.</param>
		/// <returns>The resulting matrix with the subtracted values.</returns>
		private static double[,] Subtract(double[,] matrix, double value)
		{
			var result = new double[matrix.GetLength(0), matrix.GetLength(1)];

			for (int i = 0; i < matrix.GetLength(0); i++)
			{
				for (int j = 0; j < matrix.GetLength(1); j++)
				{
					result[i, j] = matrix[i, j] - value;
				}
			}
			
			return result;
		}

		/// <summary>
		/// Creates a random 2d double cost matrix at the given sizes.
		/// </summary>
		/// <param name="rows">The number of rows in the created matrix</param>
		/// <param name="columns">The number of columns in the created matrix</param>
		/// <returns>The created matrix</returns>
		public static double[,] CreateRandomProblem(int rows, int columns)
		{
			var result = new double[rows, columns];
			var rnd = new Random(42);

			for (int i = 0; i < rows; i++)
			{
				for (int j = 0; j < columns; j++)
				{
					result[i, j] = rnd.NextDouble() * 1000 - 500;
				}
			}
			
			return result;
		}

		/// <summary>
		/// Creates a random 2d float cost matrix at the given sizes.
		/// </summary>
		/// <param name="rows">The number of rows in the created matrix</param>
		/// <param name="columns">The number of columns in the created matrix</param>
		/// <returns>The created matrix</returns>
		public static float[,] CreateRandomFloatProblem(int rows, int columns)
		{
			var result = new float[rows, columns];
			var rnd = new Random(42);

			for (int i = 0; i < rows; i++)
			{
				for (int j = 0; j < columns; j++)
				{
					result[i, j] = (float) (rnd.NextDouble() * 1000 - 500);
				}
			}
			
			return result;
		}

		/// <summary>
		/// Returns the cost of the given matching for the given cost matrix.
		/// </summary>
		/// <param name="matrix">The cost matrix.</param>
		/// <param name="solution">The matching to calculate cost for.</param>
		/// <returns>The cost</returns>
		private static T Cost<T>(T[,] matrix, int[] solution) where T : INumber<T>, INumberBase<T>, IMinMaxValue<T>
		{
			T cost = T.Zero;

			for (int i = 0; i < solution.Length; i++)
			{
				cost += matrix[i, solution[i]];
			}

			return cost;
		}

		/// <summary>
		/// Test that the solution to the given problem satisfies some basic properties and return the solution and cost of the solution.
		/// </summary>
		/// <param name="costMatrix"></param>
		/// <param name="expectedPairs2"></param>
		/// <param name="compareSolution"></param>
		/// <returns></returns>
		private static (int[], T) TestSolve<T>(T[,] costMatrix, (int, int)[] expectedPairs2, bool compareSolution) where T : INumber<T>, INumberBase<T>, IMinMaxValue<T>
		{
			var result = Hungarian.Solve(costMatrix);

			var jobs = costMatrix.GetLength(0);
			Assert.IsTrue(result.All(x => x >= 0));
			Assert.AreEqual(jobs, result.Length);
			Assert.AreEqual(jobs, result.Distinct().Count());
			Assert.AreEqual(jobs, result.Max() + 1);

			T expectedCost = T.Zero;

			var cost = Cost(costMatrix, result);

			for (int i = 0; i < jobs; i++)
			{
				var pair = expectedPairs2[i];
				expectedCost += costMatrix[pair.Item1, pair.Item2];
			}

			Console.WriteLine($"Expected cost: {expectedCost}, actual cost: {cost}");
			Assert.AreEqual(expectedCost, cost);

			// Skip this test as there are sometimes multiple best solutions
			if (compareSolution)
			{
				for (int i = 0; i < expectedPairs2.Length; i++)
				{
					var pair = expectedPairs2[i];
					Assert.AreEqual(pair.Item2, result[pair.Item1]);
				}
			}

			return (result, cost);
		}
	}
}