//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sintef.Scoop.Utilities.GeoCoding;
using System;

namespace Sintef.Scoop.Utilities.Tests
{
	[TestClass]
	public class CoordinateTest
	{
		[TestMethod,TestCategory("UnitTest")]
		public void TestClosestPoint()
		{
			Coordinate c0 = new Coordinate(0, 0);
			Coordinate c1 = new Coordinate(4, 0);
			Coordinate c2 = new Coordinate(0, 4);

			// 2D internal point
			Coordinate c3 = c0.ClosestPoint(c1, c2);
			Assert.AreEqual(c3, new Coordinate(2, 2));

			// 2D end point
			c0 = new Coordinate(10, 0);
			c3 = c0.ClosestPoint(c1, c2);
			Assert.AreEqual(c3, c1);

			// 2D other end point
			c0 = new Coordinate(0, 10);
			c3 = c0.ClosestPoint(c1, c2);
			Assert.AreEqual(c3, c2);

			// 3D internal point
			c0 = new Coordinate(0, 0, 0);
			c1 = new Coordinate(3, 3, 0);
			c2 = new Coordinate(0, 0, 6);
			c3 = c0.ClosestPoint(c1, c2);
			Assert.AreEqual(c3, new Coordinate(2, 2, 2));

			// 3D internal point
			c0 = new Coordinate(1, 1, 0);
			c1 = new Coordinate(4, 4, 0);
			c2 = new Coordinate(0, 0, 4);
			c3 = c0.ClosestPoint(c1, c2);
			Assert.AreEqual(c3, new Coordinate(2, 2, 2));

		}

		[TestMethod]
		public void TestInterpolateShort()
		{
			TestInterpolate(0, 0.00001, 5);
			TestInterpolate(0, 0.01, 5);
			TestInterpolate(0, 1, 5);
			TestInterpolate(0.99, 1, 5);
		}

		[TestMethod]
		public void TestInterpolateVeryShort()
		{
			Coordinate origin = new Coordinate(45.647783462088483, 9.8194020261920159);
			Coordinate destination = new Coordinate(45.647783464757467, 9.8194020263595);

			TestInterpolate(origin, destination, 0.20693896000924705, 1e-9);
		}


		[TestMethod]
		public void TestInterpolateMid()
		{
			TestInterpolate(0, 1, 500);
			TestInterpolate(0, 0.01, 500);
			TestInterpolate(0.99, 1, 500);
		}

		[TestMethod]
		public void TestInterpolateFar()
		{
			TestInterpolate(0, 1, 50000);
			TestInterpolate(0, 0.01, 50000);
			TestInterpolate(0.99, 1, 50000);
		}

		private static void TestInterpolate(double minFraction, double maxFraction, double offsetDistance)
		{
			double distanceTolerance = 1e-6;
			Random r = new Random(85);

			Coordinate origin = new Coordinate(60, 10);

			for (int i = 0; i < 5000; ++i)
			{
				// Create destination at distance offsetDistance in a random direction
				Coordinate dest = origin.OffsetBy(offsetDistance, r.NextDouble() * 360);

				// Verify the distance
				double distance = origin.DistanceTo(dest);
				Assert.AreEqual(offsetDistance, distance, distanceTolerance);

				// Select a fraction
				double fraction = minFraction + (maxFraction - minFraction) * r.NextDouble();

				TestInterpolate(origin, dest, fraction, distanceTolerance);

			}
		}

		private static void TestInterpolate(Coordinate origin, Coordinate dest, double fraction, double distanceTolerance)
		{
			ICoordinate originCopy = origin.Interpolated(dest, 0, 1e-100);
			ICoordinate destCopy = origin.Interpolated(dest, 1, 1e-100);
			double distOrigDest = origin.DistanceTo(dest);

			Assert.AreNotEqual(origin, dest);
			Assert.AreEqual(origin, originCopy);
			Assert.AreEqual(dest, destCopy);

			// Create an interpolated point at the fraction of distance
			ICoordinate inter = origin.Interpolated(dest, fraction, 1e-6);

			// Verify distances and azimuths to intermediate point
			double distOrigInter = origin.DistanceTo(inter);
			double distDestInter = dest.DistanceTo(inter);

			Assert.AreEqual(distOrigInter, distOrigDest * fraction, distanceTolerance);
			Assert.AreEqual(distDestInter, distOrigDest * (1 - fraction), distanceTolerance);
		}

		[TestMethod]
		public void OffsetByWorksForVerySmallDistances()
		{
			Coordinate c = new Coordinate(5, 8);
			// This produced an exception before:
			c.OffsetBy(1e-8, 45);
		}


		private static Coordinate RandomPoint(Random r)
		{
			var c = new Coordinate(r.NextDouble(), r.NextDouble());
			return c;
		}

		[TestMethod]
		public void TestClosestProjectionWhenEndPoint()
		{
			GeoCoordinate p1 = new GeoCoordinate(5.7824121882718158, 58.767336460029775);
			GeoCoordinate p2 = new GeoCoordinate(5.7824121938937179, 58.7673346544144);

			//1 mm, anything smaller breaks because of numerical uncertainty.
			double tolerance = 0.001;

			//Project p1 onto the line segment in which p1 is also an end point
			ProjectionResult projectionResult = p1.ClosestProjection(p1, p2, tolerance);
			Assert.IsTrue(projectionResult.ProjectionOK);
			Assert.IsTrue(projectionResult.ClosestPoint.DistanceTo(p1) <= tolerance);
		}

		[TestMethod, TestCategory("UnitTest")]
		public void TestClosestProjection()
		{
			var coSys = new CoordinateSystem(new GeoCoordinate(59.9561773, 10.7540324), 32);
			double tolerance = 0.0001; //0.1 mm.
			Coordinate c0 = new Coordinate(0, 0);
			Coordinate c1 = new Coordinate(4, 0);
			Coordinate c2 = new Coordinate(0, 4);

			var assertTolerance = 1E-13;
			
			// 2D internal point
			ProjectionResult projRes = c0.ClosestProjection(c1, c2, tolerance);
			Assert.IsTrue(projRes.ProjectionOK);
			Assert.AreEqual(Math.Sqrt(8), projRes.DistanceAlong, assertTolerance); 
			Assert.AreEqual(2, projRes.ClosestPoint.X, assertTolerance);
			Assert.AreEqual(2, projRes.ClosestPoint.Y, assertTolerance);
			
			// 2D end point before
			c0 = new Coordinate(10, 0);
			projRes = c0.ClosestProjection(c1, c2, tolerance);
			Assert.IsFalse(projRes.ProjectionOK);
			Assert.IsTrue(projRes.OutsideBefore);
			Assert.IsFalse(projRes.OutsideAfter);
			Assert.AreEqual(0.0, projRes.DistanceAlong);

			// 2D end point after
			c0 = new Coordinate(0, 10);
			projRes = c0.ClosestProjection(c1, c2, tolerance);
			Assert.IsFalse(projRes.ProjectionOK);
			Assert.IsFalse(projRes.OutsideBefore);
			Assert.IsTrue(projRes.OutsideAfter);
			Assert.AreEqual(Math.Sqrt(32), projRes.DistanceAlong, assertTolerance);

			//Border line, projection on c1
			c0 = new Coordinate(6, 2);
			projRes = c0.ClosestProjection(c1, c2, tolerance);
			Assert.IsTrue(projRes.ProjectionOK);
			Assert.IsTrue(c1.DistanceTo(projRes.ClosestPoint) <= tolerance);
			Assert.IsTrue(c0.DistanceTo(projRes.ClosestPoint) - Math.Sqrt(4 + 4) <= tolerance*10);

			//Border line, projection on c2
			c0 = new Coordinate(2, 6);
			projRes = c0.ClosestProjection(c1, c2, tolerance);
			Assert.IsTrue(projRes.ProjectionOK);
			Assert.IsTrue(c2.DistanceTo(projRes.ClosestPoint) <= tolerance);

			//Turning the segment around to check
			projRes = c0.ClosestProjection(c2, c1, tolerance);
			Assert.IsTrue(projRes.ProjectionOK);
			Assert.IsTrue(c2.DistanceTo(projRes.ClosestPoint) <= tolerance);
		}

		[TestMethod]
		public void ClosestProjectionIsCorrect()
		{
			Random random = new(42);

			for (double rotationAngle = 0; rotationAngle < Math.PI; rotationAngle += 0.1)
			{
				Coordinate translation = new(1000 * (random.NextDouble() - 0.5), 1000 * (random.NextDouble() - 0.5));
				TransformAndTest(rotationAngle, translation);
			}

			void TransformAndTest(double d, Coordinate coordinate)
			{
				Coordinate start = new(0, 0);
				Coordinate end = new(1, 0);

				var transformedStart = Transform(start, d, coordinate);
				var transformedEnd = Transform(end, d, coordinate);
				double tolerance = 0.11;
				double assertTolerance = 1E-13;

				for (int yf = -1; yf <= 1; ++yf)
				{
					double y = yf * 0.5;
					for (double x = -0.3; x <= 1.3; x += 0.1)
					{
						Coordinate candidate = Transform(new(x, y), d, coordinate);
						var res = candidate.ClosestProjection(transformedStart, transformedEnd, tolerance);

						var closestPoint = ReverseTransform(res.ClosestPoint as Coordinate, d, coordinate);
					
						Assert.AreEqual(0, closestPoint.Y, assertTolerance);
						if (x < -tolerance)
						{
							Assert.IsTrue(res.OutsideBefore);
							Assert.IsFalse(res.OutsideAfter);
							Assert.IsFalse(res.ProjectionOK);
							Assert.AreEqual(0d, closestPoint.X, assertTolerance);
							continue;
						}
						if (x > 1 + tolerance)
						{
							Assert.IsFalse(res.OutsideBefore);
							Assert.IsTrue(res.OutsideAfter);
							Assert.IsFalse(res.ProjectionOK);
							Assert.AreEqual(1d, closestPoint.X, assertTolerance);
							continue;
						}
						Assert.IsTrue(res.ProjectionOK);
						if (x > -assertTolerance && x < 1 + assertTolerance)
						{
							Assert.AreEqual(x, closestPoint.X, assertTolerance);
						}
					}
				}
			}
		}

		[TestMethod]
		public void TestInterpolate()
		{
			Random r = new Random(1);
			for (int i = 0; i < 1000; ++i)
			{
				var p1 = RandomPoint(r);
				var p2 = RandomPoint(r);
				double d = p1.DistanceTo(p2);

				double lastDist1 = -1;
				double lastDist2 = Double.MaxValue;

				for (int j = 0; j <= 50; ++j)
				{
					double f = j / 50.0;

					var p = p1.Interpolated(p2, f, 0.001);

					double d1 = p1.DistanceTo(p);
					double d2 = p2.DistanceTo(p);

					if (!(d1 > lastDist1 || d2 < lastDist2))
					{
						Assert.IsTrue(d1 > lastDist1 || d2 < lastDist2);
					}

					lastDist1 = d1;
					lastDist2 = d2;

					p = p1.Interpolated(p2, f, 0.1);
					Assert.AreEqual(f, p1.DistanceTo(p) / d, 0.1);

					var close = p.ClosestPoint(p1, p2);
					Assert.AreEqual(0.0, p.DistanceTo(close), 0.1);
					d1 = p1.DistanceTo(p);
					d2 = p2.DistanceTo(p);
					Assert.AreEqual(d, d1 + d2, d * 4e-5);

					p = p1.Interpolated(p2, f, 0.001);
					Assert.AreEqual(f, p1.DistanceTo(p) / d, 0.001);

					p = p1.Interpolated(p2, f, 0.00001);
					Assert.AreEqual(f, p1.DistanceTo(p) / d, 0.00002);

				}
			}
		}

		[TestMethod]
		public void TestIntersects()
		{
			Coordinate start1 = new Coordinate(65, 5), end1 = new Coordinate(67, 5);

			Coordinate start2 = new Coordinate(65, 5), end2 = new Coordinate(65, 7); // same end
			Assert.IsFalse(Coordinate.Intersects(start1, end1, start2, end2));

			start2 = new Coordinate(66, 4); end2 = new Coordinate(66, 6); // intersects
			Assert.IsTrue(Coordinate.Intersects(start1, end1, start2, end2));

			start2 = new Coordinate(66, 7); end2 = new Coordinate(67, 6); // does not intersects
			Assert.IsFalse(Coordinate.Intersects(start1, end1, start2, end2));

			start2 = new Coordinate(65, 5); end2 = new Coordinate(67, 5); // identical
			Assert.IsFalse(Coordinate.Intersects(start1, end1, start2, end2));

			start2 = new Coordinate(65.5, 5); end2 = new Coordinate(66.5, 5); // on top of each other
			Assert.IsFalse(Coordinate.Intersects(start1, end1, start2, end2));
			
			start2 = new Coordinate(65, 6); end2 = new Coordinate(67, 6); // parallell
			Assert.IsFalse(Coordinate.Intersects(start1, end1, start2, end2));

			start2 = new Coordinate(65, 6); end2 = new Coordinate(65, 6); // point outside arc
			Assert.IsFalse(Coordinate.Intersects(start1, end1, start2, end2));

			start2 = new Coordinate(66, 5); end2 = new Coordinate(66, 5); // point on arc
			Assert.IsFalse(Coordinate.Intersects(start1, end1, start2, end2));
		}


		[TestMethod]
		public void TestCoordinateEquality()
		{
			Coordinate x = new Coordinate(1, 2);
			Coordinate y = new Coordinate(1, 2);
			Coordinate z = new Coordinate(1, 3);
			Coordinate nl1 = null;
			Coordinate nl2 = null;

			Assert.AreEqual(x, y);
			Assert.AreNotEqual(x, z);
			Assert.AreNotEqual(x, null);

			Assert.IsTrue(x == y);
			Assert.IsFalse(x == z);
			Assert.IsFalse(x == nl1);
			Assert.IsTrue(nl1 == nl2);
			Assert.IsFalse(x == null);

			Assert.IsFalse(x != y);
			Assert.IsTrue(x != z);
			Assert.IsTrue(x != nl1);
			Assert.IsFalse(nl1 != nl2);
			Assert.IsTrue(x != null);
		}
		
		/// <summary>
		/// Rotates the source coordinate clockwise around origo by the given angle and then translates it by the given vector.
		/// </summary>
		/// <param name="source">The coordinate to transform.</param>
		/// <param name="rotateAngle">The rotation angle in radians.</param>
		/// <param name="translate">The translation vector.</param>
		/// <returns>The transformed coordinate.</returns>
		private Coordinate Transform(Coordinate source, double rotateAngle, Coordinate translate)
		{
			var (sin, cos) = Math.SinCos(rotateAngle);

			Coordinate rotated = new(source.X * cos - source.Y * sin, source.X * sin + source.Y * cos);
			return rotated + translate;
		}

		/// <summary>
		/// Reverses the transformation performed by <see cref="Transform"/> using the same arguments.
		/// </summary>
		/// <param name="source">The transformed coordinate to transform back.</param>
		/// <param name="rotateAngle">The rotation angle in radians.</param>
		/// <param name="translate">The translation vector.</param>
		/// <returns>The coordinate transformed back to original coordinate system.</returns>
		private Coordinate ReverseTransform(Coordinate source, double rotateAngle, Coordinate translate)
		{
			var (sin, cos) = Math.SinCos(-rotateAngle);
			var translated = source - translate;
			return new(translated.X * cos - translated.Y * sin, translated.X * sin + translated.Y * cos);
		}
	}
}
