//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Sintef.Scoop.Utilities.Tests
{
	/// <summary>
	/// Summary description for UnitTest1
	/// </summary>
	[TestClass]
	public class ShortestPathCalculatorTest
	{

		private class Position(double x, double y)
		{
			public double X
			{
				get;
				set;
			} = x;

			public double Y
			{
				get;
				set;
			} = y;

			public double DistanceTo(Position other)
			{
				double dx = other.X - X;
				double dy = other.Y - Y;

				return Math.Sqrt(dx * dx + dy * dy);
			}

			public override string ToString()
			{
				return "(" + X.ToString() + ", " + Y.ToString() + ")";
			}
		}

		private static void BuildGridGraphString(List<Position> nodePath, List<string> edgePath, out string nodePathString, out string edgePathString)
		{
			nodePathString = string.Empty;
			edgePathString = string.Empty;

			bool first = true;

			foreach (Position p in nodePath)
			{
				if (!first) nodePathString += " ";
				nodePathString += p.ToString();
				first = false;
			}

			first = true;

			foreach (string s in edgePath)
			{
				if (!first) edgePathString += " ";
				edgePathString += s;
				first = false;
			}
		}

		[TestMethod]
		public void TestGridGraph()
		{
			int gridSize = 30;

			// Make a grid graph
			ShortestPathCalculator<Position, string, double> gridGraph = new();
			Position[,] coords = new Position[gridSize, gridSize];

			int xp, yp;

			for (yp = 0; yp < gridSize; ++yp)
			{
				for (xp = 0; xp < gridSize; ++xp)
				{
					Position pos = new((double)xp, (double)yp);
					coords[xp, yp] = pos;
					gridGraph.AddNode(pos);
				}
			}

			// Create edges
			for (yp = 0; yp < gridSize; ++yp)
			{
				for (xp = 0; xp < gridSize; ++xp)
				{
					Position gp = coords[xp, yp];
					if (xp > 0)
					{
						Position gp2 = coords[xp - 1, yp];
						gridGraph.AddEdge(gp2, gp, 1, false, gp2.ToString() + " - " + gp.ToString());
					}

					if (yp > 0)
					{
						Position gp2 = coords[xp, yp - 1];
						gridGraph.AddEdge(gp2, gp, 1, false, gp2.ToString() + " - " + gp.ToString());
					}

					// Diagonals
					if (xp > 0 && yp > 0)
					{
						Position gp2 = coords[xp - 1, yp - 1];
						gridGraph.AddEdge(gp2, gp, 1, false, gp2.ToString() + " - " + gp.ToString());
					}
					if (xp > 0 && yp < gridSize - 1)
					{
						Position gp2 = coords[xp - 1, yp + 1];
						gridGraph.AddEdge(gp2, gp, 1, false, gp2.ToString() + " - " + gp.ToString());
					}
				}
			}

			// Do a shortest path
			List<Position> nodePath = [];
			List<string> edgePath = [];


			gridGraph.AStar(coords[0, 0], coords[gridSize - 1, gridSize - 1], (x, y) => x.DistanceTo(y));
			gridGraph.GetNodeAndSegmentPath(coords[gridSize - 1, gridSize - 1], out nodePath, out edgePath);
			BuildGridGraphString(nodePath, edgePath, out string aStarNodes, out string aStarEdges);

			gridGraph.Dijkstra(coords[0, 0], coords[gridSize - 1, gridSize - 1]);
			gridGraph.GetNodeAndSegmentPath(coords[gridSize - 1, gridSize - 1], out nodePath, out edgePath);
			BuildGridGraphString(nodePath, edgePath, out string dijkstraNodes, out string dijkstraEdges);

			gridGraph.BellmanFord(coords[0, 0], 0, false);
			gridGraph.GetNodeAndSegmentPath(coords[gridSize - 1, gridSize - 1], out nodePath, out edgePath);
			BuildGridGraphString(nodePath, edgePath, out string bellmanFordNodes, out string bellmanFordEdges);

			Assert.AreEqual(aStarNodes, bellmanFordNodes);
			Assert.AreEqual(aStarEdges, bellmanFordEdges);

			Assert.AreEqual(aStarNodes, dijkstraNodes);
			Assert.AreEqual(aStarEdges, dijkstraEdges);
		}

		[TestMethod]
		public void TestGridGraphGenericCost()
		{
			int gridSize = 30;

			// Make a grid graph
			ShortestPathCalculatorForTesting<Position, string> gridGraph = new();
			Position[,] coords = new Position[gridSize, gridSize];

			int xp, yp;

			for (yp = 0; yp < gridSize; ++yp)
			{
				for (xp = 0; xp < gridSize; ++xp)
				{
					Position pos = new((double)xp, (double)yp);
					coords[xp, yp] = pos;
					gridGraph.AddNode(pos);
				}
			}

			// Create edges
			for (yp = 0; yp < gridSize; ++yp)
			{
				for (xp = 0; xp < gridSize; ++xp)
				{
					Position gp = coords[xp, yp];
					if (xp > 0)
					{
						Position gp2 = coords[xp - 1, yp];
						gridGraph.AddEdge(gp2, gp, 1, false, gp2.ToString() + " - " + gp.ToString());
					}

					if (yp > 0)
					{
						Position gp2 = coords[xp, yp - 1];
						gridGraph.AddEdge(gp2, gp, 1, false, gp2.ToString() + " - " + gp.ToString());
					}

					// Diagonals
					if (xp > 0 && yp > 0)
					{
						Position gp2 = coords[xp - 1, yp - 1];
						gridGraph.AddEdge(gp2, gp, 1, false, gp2.ToString() + " - " + gp.ToString());
					}
					if (xp > 0 && yp < gridSize - 1)
					{
						Position gp2 = coords[xp - 1, yp + 1];
						gridGraph.AddEdge(gp2, gp, 1, false, gp2.ToString() + " - " + gp.ToString());
					}
				}
			}

			// Do a shortest path
			List<Position> nodePath = [];
			List<string> edgePath = [];


			gridGraph.AStar(coords[0, 0], coords[gridSize - 1, gridSize - 1], (x, y) => x.DistanceTo(y));
			gridGraph.GetNodeAndSegmentPath(coords[gridSize - 1, gridSize - 1], out nodePath, out edgePath);
			BuildGridGraphString(nodePath, edgePath, out string aStarNodes, out string aStarEdges);

			gridGraph.Dijkstra(coords[0, 0], coords[gridSize - 1, gridSize - 1]);
			gridGraph.GetNodeAndSegmentPath(coords[gridSize - 1, gridSize - 1], out nodePath, out edgePath);
			BuildGridGraphString(nodePath, edgePath, out string dijkstraNodes, out string dijkstraEdges);

			gridGraph.BellmanFord(coords[0, 0], 0, false);
			gridGraph.GetNodeAndSegmentPath(coords[gridSize - 1, gridSize - 1], out nodePath, out edgePath);
			BuildGridGraphString(nodePath, edgePath, out string bellmanFordNodes, out string bellmanFordEdges);

			Assert.AreEqual(aStarNodes, bellmanFordNodes);
			Assert.AreEqual(aStarEdges, bellmanFordEdges);

			Assert.AreEqual(aStarNodes, dijkstraNodes);
			Assert.AreEqual(aStarEdges, dijkstraEdges);
		}

		private readonly double _epsilon = 1E-14;

		[TestMethod]
		public void TestGraph()
		{

			// Do some result and cycle tests on Bellman-Ford algorithm
			ShortestPathCalculator<int, string, double> cycleTestGraph = new();

			cycleTestGraph.AddNode(1);
			cycleTestGraph.AddNode(2);
			cycleTestGraph.AddNode(3);
			cycleTestGraph.AddNode(4);
			cycleTestGraph.AddNode(5);

			cycleTestGraph.AddEdge(1, 2, 1.1, true, "edge1");
			cycleTestGraph.AddEdge(2, 3, 1.2, true, "edge2");
			cycleTestGraph.AddEdge(3, 4, 1.3, true, "edge3");
			cycleTestGraph.AddEdge(4, 5, 1.4, true, "edge4");

			// No cycle yet
			bool hasNegativeCycle = !cycleTestGraph.BellmanFord(1, 0, false);

			Assert.IsFalse(hasNegativeCycle);

			cycleTestGraph.GetNodePath(5, out List<int> cycleTestPath, out List<double> cycleCosts);

			// Test output of node path and cost function
			Assert.AreEqual(cycleTestPath.Count, 5);
			Assert.AreEqual(cycleTestPath[0], 1);
			Assert.AreEqual(cycleTestPath[1], 2);
			Assert.AreEqual(cycleTestPath[2], 3);
			Assert.AreEqual(cycleTestPath[3], 4);
			Assert.AreEqual(cycleTestPath[4], 5);

			Assert.AreEqual(cycleCosts.Count, 5);
			Assert.AreEqual(cycleCosts[0], 0, _epsilon);
			Assert.AreEqual(cycleCosts[1], 1.1, _epsilon);
			Assert.AreEqual(cycleCosts[2], 2.3, _epsilon);
			Assert.AreEqual(cycleCosts[3], 3.6, _epsilon);
			Assert.AreEqual(cycleCosts[4], 5, _epsilon);

			// Test the output of the node path and costs and edge path function
			cycleTestGraph.GetNodeAndSegmentPath(5, out cycleTestPath, out cycleCosts, out List<string> cycleEdges);

			Assert.AreEqual(cycleTestPath.Count, 5);
			Assert.AreEqual(cycleTestPath[0], 1);
			Assert.AreEqual(cycleTestPath[1], 2);
			Assert.AreEqual(cycleTestPath[2], 3);
			Assert.AreEqual(cycleTestPath[3], 4);
			Assert.AreEqual(cycleTestPath[4], 5);

			Assert.AreEqual(cycleCosts[0], 0, _epsilon);
			Assert.AreEqual(cycleCosts[1], 1.1, _epsilon);
			Assert.AreEqual(cycleCosts[2], 2.3, _epsilon);
			Assert.AreEqual(cycleCosts[3], 3.6, _epsilon);
			Assert.AreEqual(cycleCosts[4], 5, _epsilon);
			Assert.AreEqual(cycleCosts.Count, 5);

			Assert.AreEqual(cycleEdges.Count, 4);
			Assert.AreEqual(cycleEdges[0], "edge1");
			Assert.AreEqual(cycleEdges[1], "edge2");
			Assert.AreEqual(cycleEdges[2], "edge3");
			Assert.AreEqual(cycleEdges[3], "edge4");

			// Add a negative weight edge, still no negative weight cycles

			cycleTestGraph.AddEdge(3, 1, -2.29999, true, "edge5");

			hasNegativeCycle = !cycleTestGraph.BellmanFord(1, 0, false);

			Assert.IsFalse(hasNegativeCycle);

			// Add a negative weight edge, this time 1-2-3-4-5-1-2-3-4-5-1-.... is a negative cycle

			cycleTestGraph.AddEdge(5, 1, -5.001, true, "edge6");

			hasNegativeCycle = !cycleTestGraph.BellmanFord(1, 0, false);

			Assert.IsTrue(hasNegativeCycle);
		}

		[TestMethod]
		public void TestGraphGenericCost()
		{

			// Do some result and cycle tests on Bellman-Ford algorithm
			ShortestPathCalculatorForTesting<int, string> cycleTestGraph = new();

			cycleTestGraph.AddNode(1);
			cycleTestGraph.AddNode(2);
			cycleTestGraph.AddNode(3);
			cycleTestGraph.AddNode(4);
			cycleTestGraph.AddNode(5);

			cycleTestGraph.AddEdge(1, 2, 1.1, true, "edge1");
			cycleTestGraph.AddEdge(2, 3, 1.2, true, "edge2");
			cycleTestGraph.AddEdge(3, 4, 1.3, true, "edge3");
			cycleTestGraph.AddEdge(4, 5, 1.4, true, "edge4");

			// No cycle yet
			bool hasNegativeCycle = !cycleTestGraph.BellmanFord(1, 0, false);

			Assert.IsFalse(hasNegativeCycle);

			cycleTestGraph.GetNodePath(5, out List<int> cycleTestPath, out List<double> cycleCosts);

			// Test output of node path and cost function
			Assert.AreEqual(cycleTestPath.Count, 5);
			Assert.AreEqual(cycleTestPath[0], 1);
			Assert.AreEqual(cycleTestPath[1], 2);
			Assert.AreEqual(cycleTestPath[2], 3);
			Assert.AreEqual(cycleTestPath[3], 4);
			Assert.AreEqual(cycleTestPath[4], 5);

			Assert.AreEqual(cycleCosts.Count, 5);
			Assert.AreEqual(cycleCosts[0], 0, _epsilon);
			Assert.AreEqual(cycleCosts[1], 1.1, _epsilon);
			Assert.AreEqual(cycleCosts[2], 2.3, _epsilon);
			Assert.AreEqual(cycleCosts[3], 3.6, _epsilon);
			Assert.AreEqual(cycleCosts[4], 5, _epsilon);

			// Test the output of the node path and costs and edge path function
			cycleTestGraph.GetNodeAndSegmentPath(5, out cycleTestPath, out cycleCosts, out List<string> cycleEdges);

			Assert.AreEqual(cycleTestPath.Count, 5);
			Assert.AreEqual(cycleTestPath[0], 1);
			Assert.AreEqual(cycleTestPath[1], 2);
			Assert.AreEqual(cycleTestPath[2], 3);
			Assert.AreEqual(cycleTestPath[3], 4);
			Assert.AreEqual(cycleTestPath[4], 5);

			Assert.AreEqual(cycleCosts[0], 0, _epsilon);
			Assert.AreEqual(cycleCosts[1], 1.1, _epsilon);
			Assert.AreEqual(cycleCosts[2], 2.3, _epsilon);
			Assert.AreEqual(cycleCosts[3], 3.6, _epsilon);
			Assert.AreEqual(cycleCosts[4], 5, _epsilon);
			Assert.AreEqual(cycleCosts.Count, 5);

			Assert.AreEqual(cycleEdges.Count, 4);
			Assert.AreEqual(cycleEdges[0], "edge1");
			Assert.AreEqual(cycleEdges[1], "edge2");
			Assert.AreEqual(cycleEdges[2], "edge3");
			Assert.AreEqual(cycleEdges[3], "edge4");

			// Add a negative weight edge, still no negative weight cycles

			cycleTestGraph.AddEdge(3, 1, -2.29999, true, "edge5");

			hasNegativeCycle = !cycleTestGraph.BellmanFord(1, 0, false);

			Assert.IsFalse(hasNegativeCycle);

			// Add a negative weight edge, this time 1-2-3-4-5-1-2-3-4-5-1-.... is a negative cycle

			cycleTestGraph.AddEdge(5, 1, -5.001, true, "edge6");

			hasNegativeCycle = !cycleTestGraph.BellmanFord(1, 0, false);

			Assert.IsTrue(hasNegativeCycle);
		}

		private static void BuildPathString(int destination, ShortestPathCalculator<int, string, double> graph, out string nodes, out string edges)
		{
			nodes = "";
			edges = "";

			graph.GetNodeAndSegmentPath(destination, out var nodePath, out var edgePath);

			bool first = true;
			foreach (var node in nodePath)
			{
				if (!first)
				{
					nodes += " ";
				}
				nodes += node.ToString();
				first = false;
			}
			first = true;
			foreach (var edge in edgePath)
			{
				if (!first)
				{
					edges += " ";
				}
				edges += edge;
				first = false;
			}
		}

		private static void BuildPathStringGenericCost(int destination, ShortestPathCalculatorForTesting<int, string> graph, out string nodes, out string edges)
		{
			nodes = "";
			edges = "";

			graph.GetNodeAndSegmentPath(destination, out var nodePath, out var edgePath);

			bool first = true;
			foreach (var node in nodePath)
			{
				if (!first)
				{
					nodes += " ";
				}
				nodes += node.ToString();
				first = false;
			}
			first = true;
			foreach (var edge in edgePath)
			{
				if (!first)
				{
					edges += " ";
				}
				edges += edge;
				first = false;
			}
		}

		[TestMethod]
		public void TestDynamicGraph()
		{
			ShortestPathCalculator<int, string> calculator = new()
			{
				CheckGraphIntegrity = true
			};

			var node1 = calculator.AddNode(1);
			var node2 = calculator.AddNode(2);
			var node3 = calculator.AddNode(3);
			var node4 = calculator.AddNode(4);

			_ = calculator.AddEdge(node1, node2, 1, true, "e1_2");
			_ = calculator.AddEdge(node1, node3, 1.5, true, "e1_3");
			_ = calculator.AddEdge(node2, node4, 1.25, true, "e2_4");
			_ = calculator.AddEdge(node3, node4, 1, true, "e3_4");

			Assert.AreEqual(4, calculator.Edges.Count);
			Assert.AreEqual(4, calculator.Nodes.Count);

			calculator.Dijkstra(1);

			BuildPathString(4, calculator, out string nodes, out string edges);

			Assert.AreEqual("1 2 4", nodes);
			Assert.AreEqual("e1_2 e2_4", edges);

			// Add 2 edges which go directly from 1 to 4 cheaper than the previous 2 edges required
			_ = calculator.AddEdge(1, 4, 1.75, true, "e1_4");
			_ = calculator.AddEdge(1, 4, 1.8, true, "e1_4");

			var node5 = calculator.AddNode(5);

			var edge1_5 = calculator.AddEdge(1, 5, 1, true, "e1_5");
			var edge5_4 = calculator.AddEdge(5, 4, 1, true, "e5_4");

			Assert.AreEqual(5, calculator.Nodes.Count);
			Assert.AreEqual(8, calculator.Edges.Count);

			calculator.Dijkstra(1);

			BuildPathString(4, calculator, out nodes, out edges);

			Assert.AreEqual("1 4", nodes);
			Assert.AreEqual("e1_4", edges);

			// Remove both edges and test that we are back to the original situation
			calculator.RemoveEdges("e1_4");

			Assert.AreEqual(6, calculator.Edges.Count);

			calculator.Dijkstra(1);

			BuildPathString(4, calculator, out nodes, out edges);

			Assert.AreEqual("1 5 4", nodes);
			Assert.AreEqual("e1_5 e5_4", edges);

			// Check that trying to remove node 5 while it is still connected causes an exception
			bool thrown = false;
			try
			{
				calculator.RemoveNode(5);
			}
			catch (InvalidOperationException)
			{
				thrown = true;
			}
			Assert.IsTrue(thrown);
			Assert.AreEqual(6, calculator.Edges.Count);
			Assert.AreEqual(5, calculator.Nodes.Count);

			// Remove one of the edges
			calculator.RemoveEdge(edge5_4);

			Assert.AreEqual(5, calculator.Edges.Count);
			Assert.AreEqual(5, calculator.Nodes.Count);

			// Check that trying to remove node 5 while it is still connected still causes an exception
			thrown = false;
			try
			{
				calculator.RemoveNode(node5);
			}
			catch (InvalidOperationException)
			{
				thrown = true;
			}
			Assert.IsTrue(thrown);

			// Remove the last of the edges to 5
			calculator.RemoveEdge(edge1_5);
			Assert.AreEqual(4, calculator.Edges.Count);
			Assert.AreEqual(5, calculator.Nodes.Count);

			// Check that removal of node 5 now passes
			calculator.RemoveNode(node5);
			Assert.AreEqual(4, calculator.Edges.Count);
			Assert.AreEqual(4, calculator.Nodes.Count);

			// Test shortest path again
			calculator.Dijkstra(1);

			BuildPathString(4, calculator, out nodes, out edges);

			Assert.AreEqual("1 2 4", nodes);
			Assert.AreEqual("e1_2 e2_4", edges);
		}

		[TestMethod]
		public void TestDynamicGraphGenericCost()
		{
			ShortestPathCalculatorForTesting<int, string> calculator = new()
			{
				CheckGraphIntegrity = true
			};

			var node1 = calculator.AddNode(1);
			var node2 = calculator.AddNode(2);
			var node3 = calculator.AddNode(3);
			var node4 = calculator.AddNode(4);

			_ = calculator.AddEdge(node1, node2, 1, true, "e1_2");
			_ = calculator.AddEdge(node1, node3, 1.5, true, "e1_3");
			_ = calculator.AddEdge(node2, node4, 1.25, true, "e2_4");
			_ = calculator.AddEdge(node3, node4, 1, true, "e3_4");

			Assert.AreEqual(4, calculator.Edges.Count);
			Assert.AreEqual(4, calculator.Nodes.Count);

			calculator.Dijkstra(1);

			BuildPathStringGenericCost(4, calculator, out string nodes, out string edges);

			Assert.AreEqual("1 2 4", nodes);
			Assert.AreEqual("e1_2 e2_4", edges);

			// Add 2 edges which go directly from 1 to 4 cheaper than the previous 2 edges required
			_ = calculator.AddEdge(1, 4, 1.75, true, "e1_4");
			_ = calculator.AddEdge(1, 4, 1.8, true, "e1_4");

			var node5 = calculator.AddNode(5);

			var edge1_5 = calculator.AddEdge(1, 5, 1, true, "e1_5");
			var edge5_4 = calculator.AddEdge(5, 4, 1, true, "e5_4");

			Assert.AreEqual(5, calculator.Nodes.Count);
			Assert.AreEqual(8, calculator.Edges.Count);

			calculator.Dijkstra(1);

			BuildPathStringGenericCost(4, calculator, out nodes, out edges);

			Assert.AreEqual("1 4", nodes);
			Assert.AreEqual("e1_4", edges);

			// Remove both edges and test that we are back to the original situation
			calculator.RemoveEdges("e1_4");

			Assert.AreEqual(6, calculator.Edges.Count);

			calculator.Dijkstra(1);

			BuildPathStringGenericCost(4, calculator, out nodes, out edges);

			Assert.AreEqual("1 5 4", nodes);
			Assert.AreEqual("e1_5 e5_4", edges);

			// Check that trying to remove node 5 while it is still connected causes an exception
			bool thrown = false;
			try
			{
				calculator.RemoveNode(5);
			}
			catch (InvalidOperationException)
			{
				thrown = true;
			}
			Assert.IsTrue(thrown);
			Assert.AreEqual(6, calculator.Edges.Count);
			Assert.AreEqual(5, calculator.Nodes.Count);

			// Remove one of the edges
			calculator.RemoveEdge(edge5_4);

			Assert.AreEqual(5, calculator.Edges.Count);
			Assert.AreEqual(5, calculator.Nodes.Count);

			// Check that trying to remove node 5 while it is still connected still causes an exception
			thrown = false;
			try
			{
				calculator.RemoveNode(node5);
			}
			catch (InvalidOperationException)
			{
				thrown = true;
			}
			Assert.IsTrue(thrown);

			// Remove the last of the edges to 5
			calculator.RemoveEdge(edge1_5);
			Assert.AreEqual(4, calculator.Edges.Count);
			Assert.AreEqual(5, calculator.Nodes.Count);

			// Check that removal of node 5 now passes
			calculator.RemoveNode(node5);
			Assert.AreEqual(4, calculator.Edges.Count);
			Assert.AreEqual(4, calculator.Nodes.Count);

			// Test shortest path again
			calculator.Dijkstra(1);

			BuildPathStringGenericCost(4, calculator, out nodes, out edges);

			Assert.AreEqual("1 2 4", nodes);
			Assert.AreEqual("e1_2 e2_4", edges);
		}
	}


	/// <summary>
	/// Uses the generic cost version to implement shortest path algorithms with floating point cost values.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="U"></typeparam>
	public class ShortestPathCalculatorForTesting<T, U> : GenericShortestPathCalculator<T, U, double>
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public ShortestPathCalculatorForTesting() :
			base(double.NegativeInfinity, double.PositiveInfinity, (x, y) => x + y.Weight, (x, y) => (x > y) ? 1 : (x == y ? 0 : -1))
		{
		}

		/// <summary>
		/// A* invocation which takes simple heuristic function as argument
		/// </summary>
		public void AStar(T start, T destination, Func<T, T, double> heuristic)
		{
			AStar(start, destination, (x, y, z) => z + heuristic(x, y));
		}

		/// <summary>
		/// A* invocation which takes simple heuristic function as argument
		/// </summary>
		public void AStar(Node start, Node destination, Func<T, T, double> heuristic)
		{
			AStar(start, destination, (x, y, z) => z + heuristic(x, y));
		}
	}


}
