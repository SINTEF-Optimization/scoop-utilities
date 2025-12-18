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
	[TestClass]
	public class TestShortestPathAlgorithms
	{

		public class Arc(TestShortestPathAlgorithms.Node n1, TestShortestPathAlgorithms.Node n2, double cost)
		{
			public Node Node1 = n1, Node2 = n2;
			public double Cost = cost;

			public override string ToString()
			{
				return $"A({Node1.X}, {Node1.Y})-({Node2.X}, {Node2.Y})";
			}
		}

		public class Node(int x, int y)
		{
			public int X = x, Y = y;
			public Arc Left, Right, Up, Down;

			public override string ToString()
			{
				return $"N({X} {Y})";
			}
		}

		public static Node[,] CreateGrid(int dimension, double arcCost) => CreateGrid(dimension, (x, y, u, v) => arcCost);

		public static Node[,] CreateGrid(int dimension, Func<int, int, int, int, double> arcCost)
		{
			Node[,] grid = new Node[dimension, dimension];

			for (int x = 0;  x < dimension; ++x)
			{
				for (int y = 0; y < dimension; ++y)
				{
					grid[x, y] = new Node(x, y);
				}
			}

			for (int x = 0; x < dimension; ++x)
			{
				for (int y = 0; y < dimension; ++y)
				{
					if (y > 0)
					{
						grid[x, y].Up = new Arc(grid[x, y], grid[x, y - 1], arcCost(x, y, x, y - 1));
						grid[x, y - 1].Down = new Arc(grid[x, y - 1], grid[x, y], arcCost(x, y - 1, x, y));
					}
					if (x > 0)
					{
						grid[x, y].Left = new Arc(grid[x, y], grid[x - 1, y], arcCost(x, y, x - 1, y));
						grid[x - 1, y].Right = new Arc(grid[x - 1, y], grid[x, y], arcCost(x - 1, y, x, y));
					}
				}
			}

			return grid;
		}

		public static ShortestPathCalculatorForTesting<Node, Arc> CreateGraph(Node[,] grid)
		{
			var result = new ShortestPathCalculatorForTesting<Node, Arc>();


			int dimX = grid.GetLength(0);
			int dimY = grid.GetLength(1);

			for (int x = 0; x < dimX; ++x)
			{
				for (int y = 0; y < dimY; ++y)
				{
					result.AddNode(grid[x, y]);
				}
			}
			void addArc(Arc x)
			{
				if (x != null) result.AddEdge(x.Node1, x.Node2, x.Cost, true, x);
			}
			for (int x = 0; x < dimX; ++x)
			{
				for (int y = 0; y < dimY; ++y)
				{
					Node n = grid[x, y];
					addArc(n.Up);
					addArc(n.Down);
					addArc(n.Left);
					addArc(n.Right);
				}
			}

			return result;
		}

		[TestMethod]
		public void TestFindingCycle()
		{
			var grid = CreateGrid(10, 1.0);
			var graph = CreateGraph(grid);

			var start = grid[0, 0];
			var end = grid[9, 9];
			Assert.IsTrue(graph.BellmanFord(start, 0.0, false));

			double dist = graph.GetNodeCost(end);
			Assert.AreEqual(18.0, dist, 1E-10);

			var cycle = graph.ExtractCycle();
			Assert.IsNull(cycle);

			// Introduce a minimal cycle
			grid[5, 5].Up.Cost = -10.0;

			graph = CreateGraph(grid);

			Assert.IsFalse(graph.BellmanFord(start, 0.0, false));
			cycle = graph.ExtractCycle();
			Assert.IsNotNull(cycle);
			Assert.IsTrue(cycle.Select(x => x.ToString()).Contains("N(5 5)"));
			Assert.IsTrue(cycle.Select(x => x.ToString()).Contains("N(5 4)"));

			// Reset cycle
			grid[5, 5].Up.Cost = 1.0;

			// Introduce a big cycle around the whole grid which includes starting point
			for (int i = 0; i < 9; ++i)
			{
				grid[i, 0].Right.Cost = -0.001;
				grid[i + 1, 9].Left.Cost = -0.001;
				grid[9, i].Down.Cost = -0.001;
				grid[0, i + 1].Up.Cost = -0.001;
			}
			graph = CreateGraph(grid);

			Assert.IsFalse(graph.BellmanFord(start, 0.0, false));
			cycle = graph.ExtractCycle();
			Assert.IsNotNull(cycle);
			Assert.AreEqual(36, cycle.Count);
			for (int i = 0; i < 9; ++i)
			{
				Assert.IsTrue(cycle.Select(x => x.ToString()).Contains($"N({i} 0)"));
				Assert.IsTrue(cycle.Select(x => x.ToString()).Contains($"N({i + 1} 9)"));
				Assert.IsTrue(cycle.Select(x => x.ToString()).Contains($"N(9 {i})"));
				Assert.IsTrue(cycle.Select(x => x.ToString()).Contains($"N(0 {i + 1})"));
			}
		}
	}
}
