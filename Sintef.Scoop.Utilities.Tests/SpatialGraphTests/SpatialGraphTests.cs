//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sintef.Scoop.Utilities.GeoCoding;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Sintef.Scoop.Utilities.Tests.SpatialGraphTests
{
	[TestClass]
	public class SpatialGraphTests
	{
		/// <summary>
		/// The graph the tests are working on.
		/// </summary>
		SpatialGraph _g;

		/// <summary>
		/// Initialization that creates a simple graph.
		/// </summary>
		[TestInitialize]
		public void InitTests()
		{
			var coSys = new CoordinateSystem(new GeoCoordinate(59.9561773, 10.7540324), 32);
			_g = new SpatialGraph(coSys, "graph1");
		}

		[TestMethod, TestCategory("UnitTest")]
		public void TestClosestPointOnEdgeGeo() => TestClosestPointOnEdge(CoGeo);

		[TestMethod, TestCategory("UnitTest")]
		public void TestClosestPointOnEdgeLocal() => TestClosestPointOnEdge((x, y) => new Coordinate(x, y));

		private void TestClosestPointOnEdge(Func<double, double, ICoordinate> coordinateCreator)
		{
			Dictionary<string, ICoordinate> coords = new()
			{
				{ "c0", coordinateCreator(0, 0) },
				{ "c1", coordinateCreator(10, 0) },
				{ "c2", coordinateCreator(10, 10) },
				{ "c3", coordinateCreator(10, 20) },
				{ "c4", coordinateCreator(5, 20) }
			};
			var n1 = _g.AddNode(coords["c0"], "n0");
			var n2 = _g.AddNode(coords["c4"], "n4");
			SpatialEdge edge = _g.AddEdge(n1, n2, coords.Values.Skip(1).Take(3));

			//Outside before
			double tolerance = 0.01; //1 cm
			var proj = edge.ClosestPoint(coordinateCreator(-10, 5), tolerance);
			Assert.IsTrue(proj.OutsideBefore);
			Assert.IsFalse(proj.OutsideAfter);
			Assert.IsFalse(proj.ProjectionOK);
			Assert.AreEqual(0, proj.DistanceAlong);

			//Outside after
			proj = edge.ClosestPoint(coordinateCreator(2, 30), tolerance);
			Assert.IsTrue(proj.OutsideAfter);
			Assert.IsFalse(proj.OutsideBefore);
			Assert.IsFalse(proj.ProjectionOK);
			Assert.AreEqual(edge.Length, proj.DistanceAlong);

			//Inside along a segment, inside turn
			proj = edge.ClosestPoint(coordinateCreator(12, 15), tolerance);
			Assert.IsFalse(proj.OutsideAfter);
			Assert.IsFalse(proj.OutsideBefore);
			Assert.IsTrue(proj.ProjectionOK);
			Assert.IsTrue(Math.Abs(proj.DistanceAlong - 25) <= tolerance);

			//Inside, but on an outside turn, so that the projection is not on any line segment of the geometry.
			//First, distance equal to both segments
			proj = edge.ClosestPoint(coordinateCreator(12, 22), tolerance);
			Assert.IsFalse(proj.OutsideAfter);
			Assert.IsFalse(proj.OutsideBefore);
			Assert.IsTrue(proj.ProjectionOK);
			Assert.IsTrue(Math.Abs(proj.DistanceAlong - 30) <= tolerance);

			//...Then distance shorter to the first
			proj = edge.ClosestPoint(coordinateCreator(12, 21), tolerance);
			Assert.IsFalse(proj.OutsideAfter);
			Assert.IsFalse(proj.OutsideBefore);
			Assert.IsTrue(proj.ProjectionOK);
			Assert.IsTrue(Math.Abs(proj.DistanceAlong - 30) <= tolerance);

			//...and finally, distance shorter to the second.
			proj = edge.ClosestPoint(coordinateCreator(11, 22), tolerance);
			Assert.IsFalse(proj.OutsideAfter);
			Assert.IsFalse(proj.OutsideBefore);
			Assert.IsTrue(proj.ProjectionOK);
			Assert.IsTrue(Math.Abs(proj.DistanceAlong - 30) <= tolerance);
		}


		[TestMethod, TestCategory("UnitTest")]
		public void TestProjectionOnEdgeGeo() => TestProjectionOnEdge(CoGeo);

		[TestMethod, TestCategory("UnitTest")]
		public void TestProjectionOnEdgeLocal() => TestProjectionOnEdge((x, y) => new Coordinate(x, y));

		[TestMethod, TestCategory("UnitTest")]
		public void TestAverageOrientation()
		{
			Assert.AreEqual(0, SpatialPath.GetAverageOrientation(0, 0));
			Assert.AreEqual(90, SpatialPath.GetAverageOrientation(90, 90));
			Assert.AreEqual(180, SpatialPath.GetAverageOrientation(180, 180));
			Assert.AreEqual(0, SpatialPath.GetAverageOrientation(360, 360));
			Assert.AreEqual(45, SpatialPath.GetAverageOrientation(0, 90));
			Assert.AreEqual(90, Math.Abs(SpatialPath.GetAverageOrientation(0, 180)));
			Assert.AreEqual(-85, SpatialPath.GetAverageOrientation(0, 190));
			Assert.AreEqual(-45, SpatialPath.GetAverageOrientation(0, 270));
			Assert.AreEqual(0, SpatialPath.GetAverageOrientation(0, 360));
			Assert.AreEqual(45, SpatialPath.GetAverageOrientation(90, 0));
			Assert.AreEqual(90, SpatialPath.GetAverageOrientation(180, 0));
			Assert.AreEqual(-85, SpatialPath.GetAverageOrientation(190, 0));
			Assert.AreEqual(-45, SpatialPath.GetAverageOrientation(270, 0));
			Assert.AreEqual(0, SpatialPath.GetAverageOrientation(360, 0));
			Assert.AreEqual(30, SpatialPath.GetAverageOrientation(20, 40));
			Assert.AreEqual(110, SpatialPath.GetAverageOrientation(100, 120));
			Assert.AreEqual(195 - 360, SpatialPath.GetAverageOrientation(190, 200));
			Assert.AreEqual(285 - 360, SpatialPath.GetAverageOrientation(280, 290));
			Assert.AreEqual(0, SpatialPath.GetAverageOrientation(-5, 5));
			Assert.AreEqual(10, SpatialPath.GetAverageOrientation(-10, 30));
			Assert.AreEqual(179, SpatialPath.GetAverageOrientation(-180, 178));
			Assert.AreEqual(-1, SpatialPath.GetAverageOrientation(-90, 88));

		}


		private void TestProjectionOnEdge(Func<double, double, ICoordinate> coordinateCreator)
		{
			Dictionary<string, ICoordinate> coords = new()
			{
				{ "c0", coordinateCreator(0, 0) },
				{ "c1", coordinateCreator(10, 0) },
				{ "c2", coordinateCreator(10, 10) },
				{ "c3", coordinateCreator(10, 20) },
				{ "c4", coordinateCreator(5, 20) }
			};
			var n1 = _g.AddNode(coords["c0"], "n0");
			var n2 = _g.AddNode(coords["c4"], "n4");
			SpatialEdge edge = _g.AddEdge(n1, n2, coords.Values.Skip(1).Take(3));

			//Outside before
			double tolerance = 0.01; //1cm.
			var proj = edge.Geometry.ClosestProjection(coordinateCreator(-10, -5), tolerance);
			Assert.IsTrue(proj.OutsideBefore);
			Assert.IsFalse(proj.OutsideAfter);
			Assert.IsFalse(proj.ProjectionOK);
			Assert.AreEqual(0, proj.DistanceAlong);

			//Outside after
			proj = edge.Geometry.ClosestProjection(coordinateCreator(-2, 30), tolerance);
			Assert.IsTrue(proj.OutsideAfter);
			Assert.IsFalse(proj.OutsideBefore);
			Assert.IsFalse(proj.ProjectionOK);
			Assert.AreEqual(edge.Length, proj.DistanceAlong);
			var cpRes = edge.ClosestPoint(coordinateCreator(-2, 30), tolerance);
			Assert.AreEqual(proj.OutsideAfter, cpRes.OutsideAfter);
			Assert.AreEqual(proj.OutsideBefore, cpRes.OutsideBefore);
			Assert.AreEqual(proj.ProjectionOK, cpRes.ProjectionOK);
			Assert.IsTrue(proj.ClosestPoint.DistanceTo(cpRes.ClosestPoint) <= tolerance);

			//Projected on the edge that is furthest away.
			proj = edge.Geometry.ClosestProjection(coordinateCreator(2, 30), tolerance);
			Assert.IsFalse(proj.OutsideAfter);
			Assert.IsFalse(proj.OutsideBefore);
			Assert.IsTrue(proj.ProjectionOK);
			Assert.IsTrue(Math.Abs(2 - proj.DistanceAlong) <= tolerance);

			//Inside along a segment
			proj = edge.Geometry.ClosestProjection(coordinateCreator(12, 15), tolerance);
			Assert.IsFalse(proj.OutsideAfter);
			Assert.IsFalse(proj.OutsideBefore);
			Assert.IsTrue(proj.ProjectionOK);
			Assert.IsTrue(Math.Abs(25 - proj.DistanceAlong) <= tolerance);
		}


		[TestMethod, TestCategory("UnitTest")]
		public void TestClosestEdgeGeo() => TestClosestEdge(CoGeo);

		[TestMethod, TestCategory("UnitTest")]
		public void TestClosestEdgeLocal() => TestClosestEdge((x, y) => new Coordinate(x, y));

		private static void TestClosestEdge(Func<double, double, ICoordinate> coordinateCreator)
		{
			var g = SimpleGraph(coordinateCreator);
			var tup = g.ClosestEdge(coordinateCreator(-10, -10));
			Assert.AreEqual("0->1", tup.edge.Id);
			tup = g.ClosestEdge(coordinateCreator(1000, -10));
			Assert.AreEqual("8->9", tup.edge.Id);
			tup = g.ClosestEdge(coordinateCreator(14, 5.8333333));
			Assert.AreEqual("1->2", tup.edge.Id);
			Assert.IsTrue(Math.Abs(tup.edge.DistanceToPoint(tup.closestPoint) - 5.2201532) <= 1e-5);
			Assert.IsTrue(Math.Abs(3.4801018503 - tup.closestPoint.DistanceTo(coordinateCreator(14, 5.8333333))) <= 1e-2);
		}

		[TestMethod, TestCategory("UnitTest")]
		public void TestPositionsProjectedOnPathGeo() => TestPositionsProjectedOnPath(CoGeo);


		[TestMethod, TestCategory("UnitTest")]
		public void TestPositionsProjectedOnPathLocal() => TestPositionsProjectedOnPath((x, y) => new Coordinate(x, y));

		private static void TestPositionsProjectedOnPath(Func<double, double, ICoordinate> coordinateCreator)
		{
			var g = SimpleGraph(coordinateCreator);
			SpatialPath path = new(g.Edges); //A path that traverses the linear simple graph.
			double tolerance = 0.03; //3 cm

			//Position near first edge
			ICoordinate positionNearFirstEdge = coordinateCreator(5, 0.5);
			ProjectionResult projRes = path.DistanceToPoint(positionNearFirstEdge, tolerance);
			Assert.IsTrue(Math.Abs(5 - projRes.DistanceAlong) <= tolerance);
			Assert.AreEqual(path.Edges.First(), g.ClosestEdge(positionNearFirstEdge).edge);
			Assert.IsTrue(projRes.ProjectionOK);

			//Position before route
			ICoordinate positionBeforeRoute = coordinateCreator(-5, 1);
			projRes = path.DistanceToPoint(positionBeforeRoute, tolerance);
			Assert.IsTrue(projRes.DistanceAlong == 0);
			Assert.IsTrue(projRes.OutsideBefore);

			//Projection at start
			ICoordinate positionAtStart = coordinateCreator(-1, 10);
			projRes = path.DistanceToPoint(positionAtStart, tolerance);
			Assert.IsTrue(projRes.DistanceAlong <= tolerance);
			Assert.AreEqual(path.Edges.First(), g.ClosestEdge(positionAtStart).edge);
			Assert.IsTrue(projRes.ProjectionOK);

			//Projection after route
			ICoordinate positionAfterRoute = coordinateCreator(105, 90);
			projRes = path.DistanceToPoint(positionAfterRoute, tolerance);
			Assert.IsTrue(projRes.DistanceAlong.EqualsWithTolerance(path.Length, 1e-9));
			Assert.IsTrue(projRes.OutsideAfter);

			//Projection after end
			ICoordinate positionAtEnd = coordinateCreator(91, 81 - 0.58823529411764705882352941176471);
			projRes = path.DistanceToPoint(positionAtEnd, tolerance);
			Assert.IsTrue(Math.Abs(path.Length - projRes.DistanceAlong) <= tolerance);
			Assert.AreEqual(path.Edges.Last(), g.ClosestEdge(positionAtEnd).edge);

			//Position at node distance
			ICoordinate positionAtNodec = coordinateCreator(10, 1);
			projRes = path.DistanceToPoint(positionAtNodec, tolerance);
			Assert.AreEqual("0->1", g.ClosestEdge(positionAtNodec).edge.Id);
			Assert.IsTrue(Math.Abs(10.049875 - projRes.DistanceAlong) <= tolerance);
			Assert.IsTrue(projRes.ProjectionOK);

			//Position with projection exactly at node distance, inside turn
			ICoordinate positionAtNode = coordinateCreator(9, 11);
			projRes = path.DistanceToPoint(positionAtNode, tolerance);
			Assert.IsTrue((Math.Abs(projRes.DistanceAlong - 10.04987562) - projRes.DistanceAlong) < tolerance);
			Assert.IsTrue(projRes.ProjectionOK);

			//Position in outside turn
			ICoordinate positionInOutsideTurn = coordinateCreator(11.2, -9);
			projRes = path.DistanceToPoint(positionInOutsideTurn, tolerance);
			Assert.IsTrue(Math.Abs(10.049875 - projRes.DistanceAlong) <= tolerance);
			Assert.IsTrue(projRes.ProjectionOK);


		}

		[TestMethod, TestCategory("UnitTest")]
		public void TestReverse()
		{
			var g = SimpleGraph((x, y) => new Coordinate(x, y));
			SpatialPath path = new(g.Edges); //A path that traverses the linear simple graph.
			SpatialPath doubleReversed = path.Reverse().Reverse();

			int i = 0;
			foreach (var origNode in path.Nodes)
			{
				var revNode = doubleReversed.Nodes.ElementAt(i);
				Assert.AreEqual(origNode.Id, revNode.Id);
				++i;
			}

		}


		private static SpatialGraph SimpleGraph(Func<double, double, ICoordinate> coConstructor)
		{
			var coSys = new CoordinateSystem(new GeoCoordinate(59.9561773, 10.7540324), 32);
			int numberOfSteps = 10;
			double step = 10;

			//Basic coordinate system test.
			Coordinate coordinate = new(10, 10);
			Coordinate doubleTransformed = coSys.GetCoordinate(coSys.GetGeoCoordinate(coordinate));
			Assert.IsTrue(coordinate.X.EqualsWithTolerance(doubleTransformed.X, 1e-4));
			Assert.IsTrue(coordinate.Y.EqualsWithTolerance(doubleTransformed.Y, 1e-4));

			SpatialGraph g = new(coSys,"simpleGraph");

			SpatialNode prev = null;
			for (int i = 0; i < numberOfSteps; i++)
			{
				var node = g.AddNode(coConstructor(i * step, i * i), i.ToString());
				Debug.WriteLine($"({i * step},{i * i})");
				if (prev != null)
					g.AddEdge(prev, node, Enumerable.Empty<ICoordinate>(), $"{prev.Id}->{node.Id}");
				prev = node;
			}

			return g;
		}

		/// <summary>
		/// Returns the GeoCoordinate corresponding to the given carthesian coordinates.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		private GeoCoordinate CoGeo(double x, double y)
		{
			var coSys = _g.CoordinateSystem;
			return coSys.GetGeoCoordinate(new Coordinate(x, y));
		}
	}
}
