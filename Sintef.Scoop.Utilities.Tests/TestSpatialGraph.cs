//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sintef.Scoop.Utilities.GeoCoding;
using System;
using System.Linq;

namespace Sintef.Scoop.Utilities.Tests
{
	[TestClass]
	public class TestSpatialGraph
	{
		[TestMethod]
		public void TestNodeCreationAndDeletion()
		{
			SpatialGraph myGraph = new SpatialGraph();
			Assert.IsNotNull(myGraph);

			GeoCoordinate dummyCoord = new GeoCoordinate(0, 0);

			SpatialNode myNode = myGraph.AddNode(dummyCoord);
			SpatialNode myNode2 = myGraph.AddNode(dummyCoord);
			SpatialNode myNode3 = myGraph.AddNode(dummyCoord);

			Assert.IsNotNull(myNode);
			Assert.IsNotNull(myNode2);
			Assert.IsNotNull(myNode3);

			CheckIndices(myGraph);

			Assert.IsTrue(myGraph.Nodes.Contains(myNode2));
			Assert.IsTrue(myGraph.HasNode(myNode2));
			myGraph.RemoveNode(myNode2);
			Assert.IsFalse(myGraph.Nodes.Contains(myNode2));
			Assert.IsFalse(myGraph.HasNode(myNode2));
			CheckIndices(myGraph);

			myNode2 = myGraph.AddNode(dummyCoord);
			CheckIndices(myGraph);

			SpatialGraph graph2 = new SpatialGraph();

			ExpectException(() => { graph2.RemoveNode(myNode2); }, typeof(ArgumentException));

			SpatialEdge arc = myGraph.AddEdge(myNode, myNode2);
			ExpectException(() => { myGraph.RemoveNode(myNode2); }, typeof(InvalidOperationException));
		}

		[TestMethod]
		public void TestArcCreationAndDeletion()
		{
			GeoCoordinate dummyCoord = new GeoCoordinate(0, 0);

			SpatialGraph g = new SpatialGraph();
			SpatialNode node1 = g.AddNode(dummyCoord);
			SpatialNode node2 = g.AddNode(dummyCoord);
			SpatialNode node3 = g.AddNode(dummyCoord);

			SpatialEdge arc = g.AddEdge(node1, node2);
			Assert.AreEqual(node1, arc.From);
			Assert.AreEqual(node2, arc.To);

			Assert.AreEqual(arc, node1.OutEdges.Single());
			Assert.AreEqual(0, node1.InEdges.Count());
			Assert.AreEqual(arc, node2.InEdges.Single());
			Assert.AreEqual(0, node2.OutEdges.Count());

			Assert.AreEqual(arc, g.Edges.Single());

			SpatialEdge arc2 = g.AddEdge(node1, node3);
			Assert.AreEqual(2, node1.OutEdges.Count());
			Assert.AreEqual(0, node1.InEdges.Count());
			Assert.AreEqual(arc2, node3.InEdges.Single());
			Assert.AreEqual(0, node3.OutEdges.Count());

			Assert.AreEqual(2, g.Edges.Count);

			CheckIndices(g);

			SpatialGraph g2 = new SpatialGraph();

			ExpectException(() => g2.RemoveEdge(arc), typeof(ArgumentException));

			Assert.IsTrue(g.Edges.Contains(arc));
			g.RemoveEdge(arc);
			Assert.AreEqual(arc2, g.Edges.Single());

			Assert.AreEqual(0, node1.InEdges.Count());
			Assert.AreEqual(arc2, node1.OutEdges.Single());
			Assert.AreEqual(0, node2.InEdges.Count());
			Assert.AreEqual(0, node2.OutEdges.Count());
		}



		private void CheckIndices(SpatialGraph myGraph)
		{
			// Each node has index equal to its position in the node list
			Assert.IsTrue(myGraph.Nodes.Select((node, i) => node.Index == i).All(tf => tf));

			// Each arc has index equal to its position in the arc list
			Assert.IsTrue(myGraph.Edges.Select((arc, i) => arc.Index == i).All(tf => tf));
		}

		public void ExpectException(Action action, Type exceptionType)
		{
			try
			{
				action.Invoke();
			}
			catch (Exception ex)
			{
				Type t = ex.GetType();
				if (t != exceptionType)
					throw new Exception("Action did not throw an exception of type " + exceptionType, ex);
				return;
			}
			throw new Exception("Action did not throw an exception");
		}
	}
}
