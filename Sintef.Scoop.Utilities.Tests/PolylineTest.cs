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
	public class PolylineTest
	{
		/// <summary>
		/// A sample polyline used for testing.
		/// </summary>
		private Polyline _poly;
		
		/// <summary>
		/// Coords used for sample polyline.
		/// </summary>
		private Coordinate[] _coords = [new(0, 0), new(1, 0), new(1, 1), new(0, 1)];
		
		/// <summary>
		/// Coords used for a self-intersecting polyline.
		/// </summary>
		private Coordinate[] _selfIntersectingCoords = [new(0, 0), new(1, 0), new(1, 1), new(0, -1)];
		
		/// <summary>
		/// Coords used for a close to self-intersecting polyline. However, it ends at the same placed as it starts which should not be considered an
		/// intersection.
		/// </summary>
		private Coordinate[] _selfIntersectingCoords2 = [new(0, 0), new(1, 0), new(1, 1), new(0, 1), new(-0.001, 0)];
		
		/// <summary>
		/// Coords used for a close to self-intersecting polyline. It should be considered intersecting if the tolerance is large enough.
		/// </summary>
		private Coordinate[] _selfIntersectingCoords3 = [new(0, 0), new(1, 0), new(1, 1), new(0, 1), new(0.999, 0.001)];
		
		/// <summary>
		/// Coords used for a self-intersecting polyline. This one intersects precisely in a duplicate coordinate.
		/// </summary>
		private Coordinate[] _selfIntersectingCoords4 = [new(0, 0), new(1, 0), new(1, 1), new(0, 1), new(1, 0), new(2, 0)];
		
		/// <summary>
		/// Coords used for a self-intersecting polyline. This one intersects precisely at an existing coordinate.
		/// </summary>
		private Coordinate[] _selfIntersectingCoords5 = [new(0, 0), new(1, 0), new(1, 1), new(0, 1), new(2, -1), new(2, -1)];
		
		/// <summary>
		/// Coords used for a close to self-intersecting polyline. It should be considered intersecting if the tolerance is large enough.
		/// </summary>
		private Coordinate[] _selfIntersectingCoords6 = [new(0, 0), new(1, 0), new(1, 1), new(0, 1), new(2, -1)];
		
		/// <summary>
		/// Coords used for a close to self-intersecting polyline. It should be considered intersecting if the tolerance is large enough.
		/// </summary>
		private Coordinate[] _selfIntersectingCoords7 = [new(0, 0), new(1, 0), new(1, 1), new(0, 1), new(1.8, -1)];
		
		/// <summary>
		/// Coords used for a close to self-intersecting polyline. It should be considered intersecting if the tolerance is large enough.
		/// </summary>
		private Coordinate[] _selfIntersectingCoords8 = [new(0, 0), new(1, 0), new(0, 1), new(1, 1), new(1.001, -1)];
		/// <summary>
		/// Coords used for a close to self-intersecting polyline. It should be considered intersecting if the tolerance is large enough.
		/// </summary>
		private Coordinate[] _selfIntersectingCoords9 = [new(0, 0), new(1, 0), new(0, 1), new(1, 1), new(0.999, -1)];
		
		/// <summary>
		/// Coords used for various intersections.
		/// </summary>
		private Coordinate[] _intersectionCoordinates = [new(0.5, -1), new(0.5, 2), new(-1, 0.5), new(2, 0.5), new(0.5, -0.01)];

		/// <summary>
		/// Maximum error tolerated in double comparisons.
		/// </summary>
		private const double _eps = 1E-13;

		/// <summary>
		/// Initializes a sample polygon for testing.
		///
		/// Sample 0 is an ordinary simple polygon.
		/// Sample 1 is a self-intersecting polygon.
		/// Samples 2 and 3 are close to self-intersecting, they are self-intersecting if using a large enough tolerance.
		/// </summary>
		/// <param name="sample">Which sample polygon is initialized.</param>
		/// <exception cref="InvalidOperationException"></exception>
		private void Setup(int sample)
		{
			Coordinate[][] selfintersectingCoords =
			[
				_selfIntersectingCoords, _selfIntersectingCoords2, _selfIntersectingCoords3, _selfIntersectingCoords4, _selfIntersectingCoords5,
				_selfIntersectingCoords6, _selfIntersectingCoords7, _selfIntersectingCoords8, _selfIntersectingCoords9
			];
			_poly = new Polyline(sample switch
			{
				0 => _coords,
				>= 1 and <= 9 => selfintersectingCoords[sample - 1],
				_ => throw new InvalidOperationException("Unexpected sample id")
			});
		}
		
		[TestMethod]
		public void PointAtDistanceWorks()
		{
			Setup(0);
			
			ICoordinate pos;
			
			try
			{
				pos = _poly.PointAtDistance(-1);
				Assert.Fail();
			}
			catch (ArgumentException)
			{
			}
			catch (Exception e)
			{
				Assert.Fail(e.Message);
			}

			pos = _poly.PointAtDistance(0);

			Assert.AreEqual(0.0, pos.X, _eps);
			Assert.AreEqual(0.0, pos.Y, _eps);

			pos = _poly.PointAtDistance(1.5);
			
			Assert.AreEqual(1.0, pos.X, _eps);
			Assert.AreEqual(0.5, pos.Y, _eps);

			pos = _poly.PointAtDistance(2);
			
			Assert.AreEqual(1.0, pos.X, _eps);
			Assert.AreEqual(1.0, pos.Y, _eps);

			try
			{
				pos = _poly.PointAtDistance(4);
				Assert.Fail();
			}
			catch (ArgumentException)
			{
			}
			catch (Exception e)
			{
				Assert.Fail(e.Message);
			}
		}

		[TestMethod]
		public void DistanceToPointWorks()
		{
			CoordinateSystem system = new();

			Setup(0);

			var pos1 = new Coordinate(0.5, 0.1);
			
			var distanceAlongEdge = _poly.DistanceToPoint(system, pos1, 1E-10, out var distanceToClosestPointOnSegment);
			
			Assert.AreEqual(0.5, distanceAlongEdge, _eps);
			Assert.AreEqual(0.1, distanceToClosestPointOnSegment, _eps);

			distanceAlongEdge = _poly.DistanceToPoint(system, pos1, 1E-10, out distanceToClosestPointOnSegment, 1);
			
			Assert.AreEqual(1.1, distanceAlongEdge, _eps);
			Assert.AreEqual(0.5, distanceToClosestPointOnSegment, _eps);

			distanceAlongEdge = _poly.DistanceToPoint(system, new Coordinate(0.5, 0), 1E-10, out distanceToClosestPointOnSegment);
			
			Assert.AreEqual(0.5, distanceAlongEdge, _eps);
			Assert.AreEqual(0.0, distanceToClosestPointOnSegment, _eps);
		}

		[TestMethod]
		public void GetCoordinatesBetweenWorks()
		{
			CoordinateSystem cs = new();
			
			Setup(0);

			var allCoordinates = _poly.Coordinates;
			
			var coordinates = _poly.GetCoordinatesBetween(0.2, 1.1).ToArray();
			
			Assert.AreEqual(allCoordinates[1], coordinates.Single());
		}

		[TestMethod]
		public void CanCorrectlyHandleSelfIntersection()
		{
			CoordinateSystem cs = new();
			
			Setup(1);

			var intersections = _poly.IntersectionsWithSelfXY(cs, 0).ToArray();

			Assert.AreEqual(1, intersections.Length);
			
			Assert.AreEqual(0.5, intersections[0].X, _eps);
			Assert.AreEqual(0.0, intersections[0].Y, _eps);
			
			Setup(2);
			
			intersections = _poly.IntersectionsWithSelfXY(cs, 0.1).ToArray();
			
			Assert.AreEqual(0, intersections.Length);
			
			Setup(3);
			
			intersections = _poly.IntersectionsWithSelfXY(cs, 0).ToArray();
			
			Assert.AreEqual(0, intersections.Length);
			
			intersections = _poly.IntersectionsWithSelfXY(cs, 0.1).ToArray();
			
			// This should be one of 2 possible intersections equally distant.
			Assert.AreEqual(1, intersections.Length);
			
			Assert.IsTrue(intersections[0].DistanceTo(_selfIntersectingCoords3.Last()) < 0.1);

			Setup(4);
			
			intersections = _poly.IntersectionsWithSelfXY(cs, 0).ToArray();
			
			Assert.AreEqual(1, intersections.Length);
			
			Assert.AreEqual(1d, intersections[0].X, _eps);
			Assert.AreEqual(0d, intersections[0].Y, _eps);
			
			Setup(5);
			
			intersections = _poly.IntersectionsWithSelfXY(cs, 0).ToArray();
			
			Assert.AreEqual(1, intersections.Length);
			
			Assert.AreEqual(1d, intersections[0].X, _eps);
			Assert.AreEqual(0d, intersections[0].Y, _eps);
			
			Setup(6);
			
			intersections = _poly.IntersectionsWithSelfXY(cs, 0).ToArray();
			
			Assert.AreEqual(1, intersections.Length);
			Assert.AreEqual(1d, intersections[0].X, _eps);
			Assert.AreEqual(0d, intersections[0].Y, _eps);
			
			intersections = _poly.IntersectionsWithSelfXY(cs, 0.1).ToArray();

			Assert.AreEqual(1, intersections.Length);

			Assert.AreEqual(1d, intersections[0].X, _eps);
			Assert.AreEqual(0d, intersections[0].Y, _eps);

			Setup(7);

			intersections = _poly.IntersectionsWithSelfXY(cs, 0).ToArray();
			
			Assert.AreEqual(1, intersections.Length);
			Assert.AreEqual(0.9d, intersections[0].X, _eps);
			Assert.AreEqual(0d, intersections[0].Y, _eps);
			
			intersections = _poly.IntersectionsWithSelfXY(cs, 0.1).ToArray();
			
			Assert.AreEqual(1, intersections.Length);
			
			Assert.AreEqual(0.9d, intersections[0].X, _eps);
			Assert.AreEqual(0d, intersections[0].Y, _eps);
			
			Setup(8);
			
			intersections = _poly.IntersectionsWithSelfXY(cs, 0).ToArray();
			
			Assert.AreEqual(0, intersections.Length);
			
			// As this method is implemented, it only uses tolerance when checking against endpoints of lines, so this should give no intersections
			intersections = _poly.IntersectionsWithSelfXY(cs, 0.1).ToArray();
			
			Assert.AreEqual(0, intersections.Length);
			
			Setup(9);
			
			intersections = _poly.IntersectionsWithSelfXY(cs, 0).ToArray();
			
			Assert.AreEqual(2, intersections.Length);
			
			Assert.AreEqual(0.9995, intersections[0].X, _eps);
			Assert.IsTrue(Math.Abs(intersections[0].Y) < _eps || Math.Abs(intersections[0].Y - 0.0005) < _eps);
			
			intersections = _poly.IntersectionsWithSelfXY(cs, 0.1).ToArray();
			
			Assert.AreEqual(2, intersections.Length);
			
			Assert.AreEqual(0.9995, intersections[0].X, _eps);
			Assert.IsTrue(Math.Abs(intersections[0].Y) < _eps || Math.Abs(intersections[0].Y - 0.0005) < _eps);
		}

		[TestMethod]
		public void CanCorrectlyDetermineIntersection()
		{
			CoordinateSystem cs = new();
			
			Setup(0);

			Assert.IsFalse(_poly.IntersectsXY(cs, _intersectionCoordinates[0], _intersectionCoordinates[4]));
			Assert.IsTrue(_poly.IntersectsXY(cs, _intersectionCoordinates[0], _intersectionCoordinates[1]));
			Assert.IsTrue(_poly.IntersectsXY(cs, _intersectionCoordinates[2], _intersectionCoordinates[3]));
		}
		
		[TestMethod]
		public void IntersectionInXyPlaneAreCorrect()
		{
			CoordinateSystem cs = new();
			
			Setup(0);

			var intersection = _poly.IntersectionXY(cs, _intersectionCoordinates[0], _intersectionCoordinates[4], 0);
			
			Assert.IsNull(intersection);

			intersection = _poly.IntersectionXY(cs, _intersectionCoordinates[0], _intersectionCoordinates[4], 0.02);
			
			Assert.IsNotNull(intersection);
			
			Assert.AreEqual(0.5, intersection.X, _eps);
			Assert.AreEqual(0, intersection.Y, _eps);
		}
		
		[TestMethod]
		public void IntersectionsInXyPlaneAreCorrect()
		{
			CoordinateSystem cs = new();
			
			Setup(0);

			var intersections = _poly.IntersectionsXY(cs, _intersectionCoordinates[0], _intersectionCoordinates[1]).ToArray();
			
			Assert.AreEqual(2, intersections.Length);
			
			Assert.AreEqual(0.5, intersections[0].X, _eps);
			Assert.AreEqual(0.0, intersections[0].Y, _eps);
			
			Assert.AreEqual(0.5, intersections[1].X, _eps);
			Assert.AreEqual(1, intersections[1].Y, _eps);

			intersections = _poly.IntersectionsXY(cs, _intersectionCoordinates[2], _intersectionCoordinates[3]).ToArray();
			
			Assert.AreEqual(1, intersections.Length);
			
			Assert.AreEqual(1, intersections[0].X, _eps);
			Assert.AreEqual(0.5, intersections[0].Y, _eps);
			
			intersections = _poly.IntersectionsXY(cs, _coords[1], _coords[3]).ToArray();
			
			Assert.AreEqual(2, intersections.Length);
			
			Assert.AreEqual(1, intersections[0].X, _eps);
			Assert.AreEqual(0.0, intersections[0].Y, _eps);
			
			Assert.AreEqual(0, intersections[1].X, _eps);
			Assert.AreEqual(1, intersections[1].Y, _eps);
			
		}

	}
}