//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Sintef.Scoop.Utilities.Tests
{
	[TestClass]
	public class CartesianProductTest
	{
		[TestMethod]
		public void CartesianProductWorks()
		{
			char[] a = ['a', 'b', 'c'], b = ['x', 'y'], c = ['u', 'v', 'w'];
			char[][] sets = [a, b, c];

			var result = sets.CartesianProduct().Select(x => new string(x.ToArray())).ToArray();
			
			Assert.AreEqual(3 * 2 * 3, result.Length);

			char[] combo = new char[3];
			for (int i = 0; i < 3; i++)
			{
				combo[0] = a[i];
				for (int j = 0; j < 2; j++)
				{
					combo[1] = b[j];
					for (int k = 0; k < 3; k++)
					{
						combo[2] = c[k];
						Assert.IsTrue(result.Contains(new string(combo)));
					}
				}
			}
			
			sets = [['a', 'b', 'c', 'd', 'e'], ['x', 'y'], ['u', 'v', 'w'], ['m', 'n', 'o', 'p', 'q'], ['r'], ['s', 't']];
			
			result = sets.CartesianProduct().Select(x => new string(x.ToArray())).ToArray();

			Assert.AreEqual(5 * 2 * 3 * 5 * 1 * 2, result.Length);
			
			combo = new char[6];
			for (int i = 0; i < 5; i++)
			{
				combo[0] = sets[0][i];
				for (int j = 0; j < 2; j++)
				{
					combo[1] = sets[1][j];
					for (int k = 0; k < 3; k++)
					{
						combo[2] = sets[2][k];
						for (int l = 0; l < 5; l++)
						{
							combo[3] = sets[3][l];
							for (int m = 0; m < 1; m++)
							{
								combo[4] = sets[4][m];
								for (int n = 0; n < 2; n++)
								{
									combo[5] = sets[5][n];
									Assert.IsTrue(result.Contains(new string(combo)));
								}
							}
						}
					}
				}
			}
		}
	}
}