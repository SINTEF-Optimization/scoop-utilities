//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Numerics;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Implements the Hungarian algorithm.
	/// </summary>
	public static class Hungarian
	{
		/// <summary>
		/// Solves the assignment problem defined by the given cost matrix using the Hungarian algorithm.<br/>
		/// <br/>
		/// Each element costMatrix[i, j] represents the cost of assigning an element i to an element j, where i and j are 0 based indices to elements from two
		/// disjoint sets. The optimal solution is the maximum set of assignments that minimises the total cost of all assignments.<br/>  
		/// <br/>
		/// The cost matrix must have at least as many columns as rows. This will return an optimal solution in polynomial time. The optimal solution is not
		/// always unique, as there may be multiple solutions with the same value. Negative costs in the cost matrix are allowed.<br/>
		/// <br/>
		/// All indices in the cost matrix and resulting vector are 0 based.
		/// </summary>
		/// <typeparam name="T">The value type of the costmatrix. This must be a scalar numeric type.</typeparam>
		/// <param name="costMatrix">The cost matrix where each element <code>costMatrix[i, j]</code> represents the cost of matching the element represented
		/// by row u with the element represented by column v.</param>
		/// <param name="vectorize">If true, it will use SIMD instructions to improve performance. This is currently implemented for <typeparamref name="T"/> of
		/// type float or double. Note that on some architectures or .NET versions this may actually decrease performance, so by default, this parameter value is
		/// false.</param>
		/// <returns>A vector with an element for each row which is the index of the column it should be matched with for minimal cost.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static int[] Solve<T>(T[,] costMatrix, bool vectorize = false) where T: INumber<T>, INumberBase<T>, IMinMaxValue<T>
		{
			var n = costMatrix.GetLength(0);
			var m = costMatrix.GetLength(1);
			if (n == 0 || m == 0)
			{
				return [];
			}
			if (n > m)
			{
				throw new ArgumentException("Number of rows cannot exceed number of columns.");
			}

			return costMatrix switch
			{
				float[,] floatMatrix => Solve(floatMatrix, vectorize),
				double[,] doubleMatrix => Solve(doubleMatrix, vectorize),
				_ => SolveGeneric(costMatrix)
			};
		}

		/// <summary>
		/// Solves the assignment problem defined by the given cost matrix using the Hungarian algorithm.<br/>
		/// <br/>
		/// Each element costMatrix[i, j] represents the cost of assigning an element i to an element j, where i and j are indices of elements from two
		/// disjoint sets. The optimal solution is the maximum set of assignments that minimises the total cost of all assignments. Here i is 0 based but j is 1
		/// based for vector alignment. Column 0 is a dummy column for alignment purposes and is not used by the algorithm.<br/>  
		/// <br/>
		/// The cost matrix must have at least as many columns as rows. This will return an optimal solution in polynomial time. The optimal solution is not
		/// always unique, as there may be multiple solutions with the same value. Negative costs in the cost matrix are allowed.<br/>
		/// <br/>
		/// The indices in the resulting vector are 0 based.
		/// </summary>
		/// <param name="costs">The cost matrix where each element <code>costMatrix[i, j]</code> represents the cost of matching the element represented
		/// by row u with the element represented by column v.</param>
		/// <param name="vectorize">If true, it will use vector instructions like SSE to improve performance. Default and recommended setting is true.</param>
		/// <returns>A vector with an element for each row which is the index of the column it should be matched with for minimal cost.</returns>
		/// <exception cref="ArgumentException"></exception>
		private static int[] Solve(double[,] costs, bool vectorize)
		{
			var costMatrix = ConvertToArrayOfArray(costs);
			
			var n = costMatrix.Length;
			var m = costMatrix[0].Length;
			
			// Note, 1 based indexing
			// Potential per row
			var u = new double[n + 1];
			
			// Potential per column
			var v = new double[m];
			
			// For eah column 1..m, the number of the selected row or 0 if nothing selected yet. For index 0, the current row
			var path = new long[m];
			
			// Stores the previous column in the alternating path, or 0 if there is none
			var prev = new long[m];
			
			// Calculated minima for each column, to calculate delta quick
			var minColumnValuesArray = new double[m];
			var minColumnValues = minColumnValuesArray.AsSpan();

			// Vector length
			var vectorLen = Vector<double>.Count;
			
			// Used to initialize vector for keeping track of the index for each element in the vector
			var indexArr = new long[vectorLen];
			
			for (int k = 0; k < vectorLen; ++k)
			{
				indexArr[k] = k;
			}
			var startIndexVector = new Vector<long>(indexArr, 0);
			
			// Stores the used columns
			var usedArr = new long[m];
			var used = usedArr.AsSpan();
			
			bool useVector = vectorize && Vector.IsHardwareAccelerated && m >= 16;
			
			for (int i = 1; i <= n; ++i)
			{
				path[0] = i;
				long currColumn = 0;

				minColumnValues.Fill(double.MaxValue);
				used.Clear();
				
				// Repeat until we find a free next column
				do
				{
					used[(int)currColumn] = -1;
					int currRow = (int)path[currColumn], nextColumn = -1;
					double delta = double.MaxValue;
					int j = 0;
					
					// Update column values and delta
					// Pick next column
					var row = costMatrix[currRow - 1];

					if (useVector)
					{
						var uCurrRowVector = new Vector<double>(u[currRow]);
						var currColVector = new Vector<long>(currColumn);
						var deltaVector = new Vector<double>(double.MaxValue);
						var minIndexVector = new Vector<long>(-1);

						for (; j <= (m - vectorLen); j += vectorLen)
						{
							var notUsedV = ~(new Vector<long>(usedArr, j));

							var curVector = new Vector<double>(row, j) - uCurrRowVector - new Vector<double>(v, j);

							// Update minimum column values and prev
							var minValueVector = new Vector<double>(minColumnValuesArray, j);
							var mask = Vector.LessThan(curVector, minValueVector) & notUsedV;
							var vectorResult = Vector.ConditionalSelect(mask, curVector, minValueVector);
							vectorResult.CopyTo(minColumnValuesArray, j);
							Vector.ConditionalSelect(mask, currColVector, new Vector<long>(prev, j)).CopyTo(prev, j);

							// Set delta to the smallest column value and nextColumn to index of smallest column value
							mask = Vector.LessThan(vectorResult, deltaVector) & notUsedV;
							deltaVector = Vector.ConditionalSelect(mask, vectorResult, deltaVector);
							minIndexVector = Vector.ConditionalSelect(mask, startIndexVector + new Vector<long>(j), minIndexVector);
						}

						// Gather the lowest delta and set nextColumn accordingly
						for (int k = 0; k < vectorLen; ++k)
						{
							if (deltaVector[k] < delta)
							{
								nextColumn = (int)minIndexVector[k];
								delta = minColumnValues[nextColumn];
							}
						}
					}

					for (; j < m; ++j)
					{
						if (used[j] != 0)
						{
							continue;
						}
						double cur = row[j] - u[currRow] - v[j];
						if (cur < minColumnValues[j])
						{
							minColumnValues[j] = cur;
							prev[j] = currColumn;
						}

						if (minColumnValues[j] < delta)
						{
							delta = minColumnValues[j];
							nextColumn = j;
						}
					}

					// Update potentials
					j = 0;
					for (; j < m; ++j)
					{
						if (used[j] != 0)
						{
							u[path[j]] += delta;
							v[j] -= delta;
						}
						else
						{
							minColumnValues[j] -= delta;
						}
					}

					currColumn = nextColumn;
				} while (path[currColumn] != 0);

				// Update path/matching
				do
				{
					var prevColumn = prev[currColumn];
					path[currColumn] = path[prevColumn];
					currColumn = prevColumn;
				} while (currColumn != 0);
			}

			// Return the result using 0 based indices
			var result = new int[n];
			for (int j = 1; j < m; ++j)
			{
				result[path[j] - 1] = j - 1;
			}

			return result;
		}

		/// <summary>
		/// Solves the assignment problem defined by the given cost matrix using the Hungarian algorithm.<br/>
		/// <br/>
		/// Each element costMatrix[i, j] represents the cost of assigning an element i to an element j, where i and j are indices of elements from two
		/// disjoint sets. The optimal solution is the maximum set of assignments that minimises the total cost of all assignments. Here i is 0 based but j is 1
		/// based for vector alignment. Column 0 is a dummy column for alignment purposes and is not used by the algorithm.<br/>  
		/// <br/>
		/// The cost matrix must have at least as many columns as rows. This will return an optimal solution in polynomial time. The optimal solution is not
		/// always unique, as there may be multiple solutions with the same value. Negative costs in the cost matrix are allowed.<br/>
		/// <br/>
		/// The indices in the resulting vector are 0 based.
		/// </summary>
		/// <param name="costs">The cost matrix where each element <code>costMatrix[i, j]</code> represents the cost of matching the element represented
		/// by row u with the element represented by column v.</param>
		/// <param name="vectorize">If true, it will use vector instructions like SSE to improve performance. Default and recommended setting is true.</param>
		/// <returns>A vector with an element for each row which is the index of the column it should be matched with for minimal cost.</returns>
		/// <exception cref="ArgumentException"></exception>
		private static int[] Solve(float[,] costs, bool vectorize)
		{
			var costMatrix = ConvertToArrayOfArray(costs);
			
			var n = costMatrix.Length;
			var m = costMatrix[0].Length;
			
			// Note, 1 based indexing
			// Potential per row
			var u = new float[n + 1];
			
			// Potential per column
			var v = new float[m];
			
			// For eah column 1..m, the number of the selected row or 0 if nothing selected yet. For index 0, the current row
			var path = new int[m];
			
			// Stores the previous column in the alternating path, or 0 if there is none
			var prev = new int[m];
			
			// Calculated minima for each column, to calculate delta quick
			var minColumnValuesArray = new float[m];
			var minColumnValues = minColumnValuesArray.AsSpan();

			// Vector length
			var vectorLen = Vector<float>.Count;
			
			// Used to initialize vector for keeping track of the index for each element in the vector
			var indexArr = new int[vectorLen];
			
			for (int k = 0; k < vectorLen; ++k)
			{
				indexArr[k] = k;
			}
			var startIndexVector = new Vector<int>(indexArr, 0);
			
			// Stores the used columns
			var usedArr = new int[m];
			var used = usedArr.AsSpan();
			
			bool useVector = vectorize && Vector.IsHardwareAccelerated && m >= 16;
			
			for (int i = 1; i <= n; ++i)
			{
				path[0] = i;
				int currColumn = 0;

				minColumnValues.Fill(float.MaxValue);
				used.Clear();
				
				// Repeat until we find a free next column
				do
				{
					used[currColumn] = -1;
					int currRow = path[currColumn], nextColumn = -1;
					float delta = float.MaxValue;
					int j = 0;
					
					// Update column values and delta
					// Pick next column
					var row = costMatrix[currRow - 1];

					if (useVector)
					{
						var uCurrRowVector = new Vector<float>(u[currRow]);
						var currColVector = new Vector<int>(currColumn);
						var deltaVector = new Vector<float>(float.MaxValue);
						var minIndexVector = new Vector<int>(-1);

						for (; j <= (m - vectorLen); j += vectorLen)
						{
							var notUsedV = ~(new Vector<int>(usedArr, j));

							var vVector = new Vector<float>(v, j);
							var curVector = new Vector<float>(row, j) - uCurrRowVector - vVector;

							// Update minimum column values and prev
							var prevVector = new Vector<int>(prev, j);
							var minValueVector = new Vector<float>(minColumnValuesArray, j);
							var mask = Vector.LessThan(curVector, minValueVector) & notUsedV;
							var vectorResult = Vector.ConditionalSelect(mask, curVector, minValueVector);
							vectorResult.CopyTo(minColumnValuesArray, j);
							var prevResult = Vector.ConditionalSelect(mask,  currColVector, prevVector);
							prevResult.CopyTo(prev, j);

							// Set delta to the smallest column value and nextColumn to index of smallest column value
							var indexVector = startIndexVector + new Vector<int>(j);
							mask = Vector.LessThan(vectorResult, deltaVector) & notUsedV;
							deltaVector = Vector.ConditionalSelect(mask, vectorResult, deltaVector);
							minIndexVector = Vector.ConditionalSelect(mask, indexVector, minIndexVector);
						}

						// Gather the lowest delta and set nextColumn accordingly
						for (int k = 0; k < vectorLen; ++k)
						{
							if (deltaVector[k] < delta)
							{
								nextColumn = minIndexVector[k];
								delta = minColumnValues[nextColumn];
							}
						}
					}

					for (; j < m; ++j)
					{
						if (used[j] != 0)
						{
							continue;
						}
						float cur = row[j] - u[currRow] - v[j];
						if (cur < minColumnValues[j])
						{
							minColumnValues[j] = cur;
							prev[j] = currColumn;
						}

						if (minColumnValues[j] < delta)
						{
							delta = minColumnValues[j];
							nextColumn = j;
						}
					}

					// Update potentials
					j = 0;
					for (; j < m; ++j)
					{
						if (used[j] != 0)
						{
							u[path[j]] += delta;
							v[j] -= delta;
						}
						else
						{
							minColumnValues[j] -= delta;
						}
					}

					currColumn = nextColumn;
				} while (path[currColumn] != 0);

				// Update path/matching
				do
				{
					var prevColumn = prev[currColumn];
					path[currColumn] = path[prevColumn];
					currColumn = prevColumn;
				} while (currColumn != 0);
			}

			// Return the result using 0 based indices
			var result = new int[n];
			for (int j = 1; j < m; ++j)
			{
				result[path[j] - 1] = j - 1;
			}

			return result;
		}

		/// <summary>
		/// Solves the assignment problem defined by the given cost matrix using the Hungarian algorithm.<br/>
		/// <br/>
		/// Each element costMatrix[i, j] represents the cost of assigning an element i to an element j, where i and j are indices of elements from two
		/// disjoint sets. The optimal solution is the maximum set of assignments that minimises the total cost of all assignments. Here i is 0 based but j is 1
		/// based for vector alignment. Column 0 is a dummy column for alignment purposes and is not used by the algorithm.<br/>  
		/// <br/>
		/// The cost matrix must have at least as many columns as rows. This will return an optimal solution in polynomial time. The optimal solution is not
		/// always unique, as there may be multiple solutions with the same value. Negative costs in the cost matrix are allowed.<br/>
		/// <br/>
		/// The indices in the resulting vector are 0 based.
		/// </summary>
		/// <param name="costMatrix">The cost matrix where each element <code>costMatrix[i, j]</code> represents the cost of matching the element represented
		/// by row u with the element represented by column v.</param>
		/// <returns>A vector with an element for each row which is the index of the column it should be matched with for minimal cost.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static int[] SolveGeneric<T>(T[,] costMatrix) where T: INumber<T>, INumberBase<T>, IMinMaxValue<T> 
		{
			var n = costMatrix.GetLength(0);
			var m = costMatrix.GetLength(1) + 1;

			// Note, 1 based indexing
			// Potential per row
			var u = new T[n + 1];

			// Potential per column
			var v = new T[m];

			// For eah column 1..m, the number of the selected row or 0 if nothing selected yet. For index 0, the current row
			var path = new int[m];

			// Stores the previous column in the alternating path, or 0 if there is none
			var prev = new int[m];

			// Calculated minima for each column, to calculate delta quick
			var minColumnValues = new T[m];

			// Stores the used columns
			var used = new int[m];

			for (int i = 1; i <= n; ++i)
			{
				path[0] = i;
				int currColumn = 0;

				for (int k = 0; k < m; ++k)
				{
					minColumnValues[k] = T.MaxValue;
					used[k] = 0;
				}

				// Repeat until we find a free next column
				do
				{
					used[currColumn] = -1;
					int currRow = path[currColumn], nextColumn = -1;
					T delta = T.MaxValue;
					int j = 0;

					// Update minimum column values and prev
					// Pick next column
					for (; j < m; ++j)
					{
						if (used[j] != 0)
						{
							continue;
						}

						T cur = costMatrix[currRow - 1, j - 1] - u[currRow] - v[j];
						if (cur < minColumnValues[j])
						{
							minColumnValues[j] = cur;
							prev[j] = currColumn;
						}

						if (minColumnValues[j] < delta)
						{
							delta = minColumnValues[j];
							nextColumn = j;
						}
					}

					// Update potentials
					j = 0;
					for (; j < m; ++j)
					{
						if (used[j] != 0)
						{
							u[path[j]] += delta;
							v[j] -= delta;
						}
						else
						{
							minColumnValues[j] -= delta;
						}
					}

					currColumn = nextColumn;
				} while (path[currColumn] != 0);

				// Update path/matching
				do
				{
					var prevColumn = prev[currColumn];
					path[currColumn] = path[prevColumn];
					currColumn = prevColumn;
				} while (currColumn != 0);
			}

			// Return the result using 0 based indices
			var result = new int[n];
			for (int j = 1; j < m; ++j)
			{
				result[path[j] - 1] = j - 1;
			}

			return result;
		}

		/// <summary>
		/// Converts a two-dimensional array into an array of arrays where the inner arrays are expanded with a dummy column at index 0.
		/// </summary>
		/// <param name="inputMatrix">The input matrix</param>
		/// <typeparam name="T">The value type.</typeparam>
		/// <returns>The array of arrays.</returns>
		private static T[][] ConvertToArrayOfArray<T>(T[,] inputMatrix)
		{
			var n = inputMatrix.GetLength(0);
			var m = inputMatrix.GetLength(1);
			var a = CreateMatrix<T>(n, m + 1);

			for (int i = 0; i < n; ++i)
			{
				for (int j = 1; j <= m; ++j)
				{
					a[i][j] = inputMatrix[i, j - 1];
				}
			}

			return a;
		}

		/// <summary>
		/// Creates a 2d matrix in the form of an array of <paramref name="n"/> rows of length <paramref name="m"/>.
		/// </summary>
		/// <param name="n">The number of rows.</param>
		/// <param name="m">The numbers of columns.</param>
		/// <param name="initialValue">Initial value of all elements in the matrix.</param>
		/// <typeparam name="T">The type of the value in the matrix.</typeparam>
		/// <returns>The created matrix.</returns>
		private static T[][] CreateMatrix<T>(int n, int m, T initialValue = default)
		{
			var result = new T[n][];

			for (int i = 0; i < n; ++i)
			{
				var row = new T[m];
				result[i] = row;
				row.AsSpan().Fill(initialValue);
			}
			return result;
		}
	}
}