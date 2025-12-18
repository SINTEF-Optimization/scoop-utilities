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
	public class GeoCodingTest
	{

		[TestMethod]
		public void TestInterpolateShort()
		{
			TestInterpolate(0, 0.00001, 5, 0.01);
			TestInterpolate(0, 0.01, 5, 0.01);
			TestInterpolate(0, 1, 5, 0.01);
			TestInterpolate(0.99, 1, 5, 0.01);
		}

		[TestMethod]
		public void TestInterpolateVeryShort()
		{
			GeoCoordinate origin = new GeoCoordinate(45.647783462088483, 9.8194020261920159);
			GeoCoordinate destination = new GeoCoordinate(45.647783464757467, 9.8194020263595);

			TestInterpolate(origin, destination, 0.20693896000924705, 1e-6);
		}


		[TestMethod]
		public void TestInterpolateMid()
		{
			TestInterpolate(0, 1, 500, 0.1);
			TestInterpolate(0, 0.01, 500, 0.1);
			TestInterpolate(0.99, 1, 500, 0.1);
		}

		[TestMethod]
		public void TestInterpolateFar()
		{
			TestInterpolate(0, 1, 50000, 0.1);
			TestInterpolate(0, 0.01, 50000, 0.1);
			TestInterpolate(0.99, 1, 50000, 0.1);
		}

		private static void TestInterpolate(double minFraction, double maxFraction, double offsetDistance, double distanceTolerance)
		{

			Random r = new Random(85);

			GeoCoordinate origin = new GeoCoordinate(60, 10);

			for (int i = 0; i < 5000; ++i)
			{
				// Create destination at distance offsetDistance in a random direction
				GeoCoordinate dest = origin.OffsetBy(offsetDistance, r.NextDouble() * 360) as GeoCoordinate;

				// Verify the distance
				double distance = origin.DistanceTo(dest, out _, out _);
				Assert.AreEqual(offsetDistance, distance, distanceTolerance);

				// Select a fraction
				double fraction = minFraction + (maxFraction - minFraction) * r.NextDouble();

				TestInterpolate(origin, dest, fraction, distanceTolerance);

			}
		}

		private static void TestInterpolate(GeoCoordinate origin, GeoCoordinate dest, double fraction, double distanceTolerance)
		{
			GeoCoordinate originCopy = origin.Interpolated(dest, 0, 1e-100);
			GeoCoordinate destCopy = origin.Interpolated(dest, 1, 1e-100);
			double azOrigDest, azDestOrig;
			double distOrigDest = origin.DistanceTo(dest, out azOrigDest, out azDestOrig);

			Assert.AreNotEqual(origin, dest);
			Assert.AreEqual(origin, originCopy);
			Assert.AreEqual(dest, destCopy);

			// Create an interpolated point at the fraction of distance
			GeoCoordinate inter = origin.Interpolated(dest, fraction, 1e-6);

			// Verify distances and azimuths to intermediate point
			double azOrigInter, azInterOrig;
			double distOrigInter = origin.DistanceTo(inter, out azOrigInter, out azInterOrig);
			double azDestInter, azInterDest;
			double distDestInter = dest.DistanceTo(inter, out azDestInter, out azInterDest);

			Assert.AreEqual(distOrigInter, distOrigDest * fraction, distanceTolerance);
			Assert.AreEqual(distDestInter, distOrigDest * (1 - fraction), distanceTolerance);
			if (distOrigInter > 0.1)
			{
				Assert.AreEqual(azOrigInter, azOrigDest, 0.1);
				Assert.AreEqual(azInterOrig, GeoCoordinate.AzimuthAtPointOnSegment(dest, origin, 1 - fraction), 0.1);
			}
			if (distDestInter > 0.1)
			{
				Assert.AreEqual(azDestInter, azDestOrig, 0.1);
				Assert.AreEqual(azInterDest, GeoCoordinate.AzimuthAtPointOnSegment(origin, dest, fraction), 0.1);
			}
		}

		[TestMethod]
		public void OffsetByWorksForVerySmallDistances()
		{
			GeoCoordinate c = new GeoCoordinate(5, 8);
			// This produced an exception before:
			c.OffsetBy(1e-8, 45);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void TestConstruct2()
		{
			var c = new GeoCoordinate(90.01, 0);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void TestConstruct3()
		{
			var c = new GeoCoordinate(-90.01, 0);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void TestConstruct4()
		{
			var c = new GeoCoordinate(0, -180.01);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void TestConstruct5()
		{
			var c = new GeoCoordinate(0, 540);
		}

		[TestMethod]
		public void TestUtm1()
		{
			for (int zone = 1; zone <= 60; ++zone)
			{
				TestUtm(0, 0, zone);
				TestUtm(1000, 0, zone);
				TestUtm(0, 1000, zone);
				TestUtm(30000, 50000, zone);
				TestUtm(55000, 6000000, zone);
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void TestUtm2()
		{
			var c = new UtmCoordinate(0, 0, 0, true);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void TestUtm3()
		{
			var c = new UtmCoordinate(0, 0, 61, true);
		}

		private static void TestUtm(double e0, double n0, int zone)
		{
			CoordinateSystem cs = new CoordinateSystem();
			cs.UtmZone = zone;
			var c = new UtmCoordinate(e0, n0, zone, true);
			double e, n;
			bool hemi;

			GeoCoordinate gc = cs.GetGeoCoordinate(c);
			UtmCoordinate cc = cs.GetUtmCoordinate(gc);

			//c.GetUtmCoordinates(zone, out e, out n, out hemi);
			e = cc.Easting;
			n = cc.Northing;
			hemi = cc.NorthernHemisphere;
			Assert.AreEqual(e0, e, 0.5);
			Assert.AreEqual(n0, n, 0.5);
			Assert.AreEqual(hemi, true);
		}

		[TestMethod]
		public void TestDistance()
		{
			Random r = new Random(1);

			// 6 poles
			var c1 = new GeoCoordinate(0, 0);
			var c2 = new GeoCoordinate(90, 0);
			var c3 = new GeoCoordinate(-90, 0);
			var c4 = new GeoCoordinate(0, 90);
			var c5 = new GeoCoordinate(0, -90);
			var c6 = new GeoCoordinate(0, 180);
			GeoCoordinate[] poles = new GeoCoordinate[] { c1, c2, c3, c4, c5, c6 };

			Assert.AreEqual(0.0, c1.DistanceTo(c1));
			Assert.AreEqual(0.0, c2.DistanceTo(c2));
			Assert.AreEqual(0.0, c3.DistanceTo(c3));

			Assert.AreEqual(c1.DistanceTo(c2), c2.DistanceTo(c1));

			for (int i = 0; i < 1000; ++i)
			{
				var c = RandomPoint(r);

				Assert.AreEqual(c.DistanceTo(c), 0.0);
				foreach (var pole in poles)
				{
					Assert.AreEqual(c.DistanceTo(pole), pole.DistanceTo(c), 1e-6);
				}
				double minD = poles.Min(x => c.DistanceTo(x));
				double maxD = poles.Max(x => c.DistanceTo(x));

				Assert.IsTrue(minD < 6500000);
				Assert.IsTrue(maxD > 13500000);
			}

			double az12, az21;
			c1.DistanceTo(c2, out az12, out az21);
			Assert.AreEqual(0, az12);
			c1.DistanceTo(c3, out az12, out az21);
			Assert.AreEqual(-180, az12);
			c1.DistanceTo(c4, out az12, out az21);
			Assert.AreEqual(90, az12);
			Assert.AreEqual(-90, az21);
			c1.DistanceTo(c5, out az12, out az21);
			Assert.AreEqual(-90, az12);
			Assert.AreEqual(90, az21);

			c4.DistanceTo(c2, out az12, out az21);
			Assert.AreEqual(0, az12);
			c4.DistanceTo(c3, out az12, out az21);
			Assert.AreEqual(-180, az12);
			c4.DistanceTo(c6, out az12, out az21);
			Assert.AreEqual(90, az12);
			Assert.AreEqual(-90, az21);
			c4.DistanceTo(c1, out az12, out az21);
			Assert.AreEqual(-90, az12);
			Assert.AreEqual(90, az21);
		}

		private static GeoCoordinate RandomPoint(Random r)
		{
			var c = new GeoCoordinate(r.NextDouble() * 180 - 90, r.NextDouble() * 360);
			return c;
		}

		[TestMethod]
		public void TestClosestProjectionWhenEndPoint()
		{
			GeoCoordinate p1 = new GeoCoordinate(5.7824121882718158, 58.767336460029775);
			GeoCoordinate p2 = new GeoCoordinate(5.7824121938937179, 58.7673346544144);

			//1 mm, anyting smaller breaks because of numerical uncertainty.
			double tolerance = 0.001;

			//Project p1 onto the line segment in which p1 is also an end point
			ProjectionResult projectionResult = p1.ClosestProjection(p1, p2, tolerance);
			Assert.IsTrue(projectionResult.ProjectionOK);
			Assert.IsTrue(projectionResult.ClosestPoint.DistanceTo(p1) <= tolerance);
		}

		[TestMethod]
		public void TestClosestGeoPoint()
		{
			Random r = new Random(1);
			for (int i = 0; i < 1000; ++i)
			{
				var p1 = RandomPoint(r);
				var p2 = RandomPoint(r);
				var p3 = RandomPoint(r);

				double d12 = p1.DistanceTo(p2);
				var close = p3.ClosestPoint(p1, p2);
				var close2 = close.ClosestPoint(p1, p2);

				Assert.AreEqual(0.0, close.DistanceTo(close2), 0.1);

				double d1c = p1.DistanceTo(close);
				double dc2 = close.DistanceTo(p2);

				Assert.AreEqual(d12, d1c + dc2, d12 * 3e-5);

				Assert.IsTrue(p3.DistanceTo(close) <= p3.DistanceTo(p1));
				Assert.IsTrue(p3.DistanceTo(close) <= p3.DistanceTo(p2));
			}
		}



		[TestMethod, TestCategory("UnitTest")]
		public void TestClosestGeoProjection()
		{
			var coSys = new CoordinateSystem(new GeoCoordinate(59.9561773, 10.7540324), 32);
			double tolerance = 0.0001; //0.1 mm.
			GeoCoordinate c0 = Co(0, 0);
			GeoCoordinate c1 = Co(4, 0);
			GeoCoordinate c2 = Co(0, 4);

			// 2D internal point
			ProjectionResult projRes = c0.ClosestProjection(c1, c2, tolerance);
			Assert.IsTrue(projRes.ProjectionOK);
			
			// 2D end point before
			c0 = Co(10, 0);
			projRes = c0.ClosestProjection(c1, c2, tolerance);
			Assert.IsFalse(projRes.ProjectionOK);
			Assert.IsTrue(projRes.OutsideBefore);
			Assert.IsFalse(projRes.OutsideAfter);

			// 2D end point after
			c0 = Co(0, 10);
			projRes = c0.ClosestProjection(c1, c2, tolerance);
			Assert.IsFalse(projRes.ProjectionOK);
			Assert.IsFalse(projRes.OutsideBefore);
			Assert.IsTrue(projRes.OutsideAfter);

			//Border line, projection on c1
			c0 = Co(6, 2);
			projRes = c0.ClosestProjection(c1, c2, tolerance);
			Assert.IsTrue(projRes.ProjectionOK);
			Assert.IsTrue(c1.DistanceTo(projRes.ClosestPoint) <= tolerance);
			Assert.IsTrue(c0.DistanceTo(projRes.ClosestPoint) - Math.Sqrt(4 + 4) <= tolerance*10);

			//Border line, projection on c2
			c0 = Co(2, 6);
			projRes = c0.ClosestProjection(c1, c2, tolerance);
			Assert.IsTrue(projRes.ProjectionOK);
			Assert.IsTrue(c2.DistanceTo(projRes.ClosestPoint) <= tolerance);

			//Turning the segment around to check
			projRes = c0.ClosestProjection(c2, c1, tolerance);
			Assert.IsTrue(projRes.ProjectionOK);
			Assert.IsTrue(c2.DistanceTo(projRes.ClosestPoint) <= tolerance);


			/// <summary>
			/// Returns the GeoCoordinate corresponding to the given carthesian coordinates.
			/// </summary>
			/// <param name="x"></param>
			/// <param name="y"></param>
			/// <returns></returns>
			GeoCoordinate Co(double x, double y) =>coSys.GetGeoCoordinate(new Coordinate(x, y));

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
			GeoCoordinate start1 = new GeoCoordinate(65, 5), end1 = new GeoCoordinate(67, 5);

			GeoCoordinate start2 = new GeoCoordinate(65, 5), end2 = new GeoCoordinate(65, 7); // same end
			Assert.IsFalse(GeoCoordinate.Intersects(start1, end1, start2, end2));

			start2 = new GeoCoordinate(66, 4); end2 = new GeoCoordinate(66, 6); // intersects
			Assert.IsTrue(GeoCoordinate.Intersects(start1, end1, start2, end2));

			start2 = new GeoCoordinate(66, 7); end2 = new GeoCoordinate(67, 6); // does not intersects
			Assert.IsFalse(GeoCoordinate.Intersects(start1, end1, start2, end2));

			start2 = new GeoCoordinate(65, 5); end2 = new GeoCoordinate(67, 5); // identical
			Assert.IsFalse(GeoCoordinate.Intersects(start1, end1, start2, end2));

			start2 = new GeoCoordinate(65.5, 5); end2 = new GeoCoordinate(66.5, 5); // on top of each other
			Assert.IsFalse(GeoCoordinate.Intersects(start1, end1, start2, end2));
			
			start2 = new GeoCoordinate(65, 6); end2 = new GeoCoordinate(67, 6); // parallell
			Assert.IsFalse(GeoCoordinate.Intersects(start1, end1, start2, end2));

			start2 = new GeoCoordinate(65, 6); end2 = new GeoCoordinate(65, 6); // point outside arc
			Assert.IsFalse(GeoCoordinate.Intersects(start1, end1, start2, end2));

			start2 = new GeoCoordinate(66, 5); end2 = new GeoCoordinate(66, 5); // point on arc
			Assert.IsFalse(GeoCoordinate.Intersects(start1, end1, start2, end2));
		}

		[TestMethod]
		public void TestConstruct1()
		{
			TestConstruct(0, 0);
			TestConstruct(90, 0);
			TestConstruct(-90, 0);
			TestConstruct(0, -180);
			TestConstruct(0, 179);

			var c = new GeoCoordinate(0, 539.99);
		}

		private static GeoCoordinate TestConstruct(double lat, double lon)
		{
			var c = new GeoCoordinate(lat, lon);
			Assert.AreEqual(lat, c.Latitude);
			Assert.AreEqual(lon, c.Longitude);
			return c;
		}

		[TestMethod]
		public void TestGeoCoordinateEquality()
		{
			GeoCoordinate x = new GeoCoordinate(1, 2);
			GeoCoordinate y = new GeoCoordinate(1, 2);
			GeoCoordinate z = new GeoCoordinate(1, 3);
			GeoCoordinate nl1 = null;
			GeoCoordinate nl2 = null;

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
	}
}
