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
	public class TestBoundingBox
	{
		[TestMethod]
		public void TestExpandBoundingBox()
		{
			Random rand = new Random(1);
			for (int i = 0; i < 100000; ++i)
			{
				GeoCoordinate c1 = RandomPoint(rand);
				GeoCoordinate c2 = RandomPoint(rand);
				GeoCoordinate c3 = RandomPoint(rand);
				GeoCoordinate c4 = RandomPoint(rand);

				BoundingBox box = new BoundingBox(c1);
				box.ExpandBy(c2);

				Assert.IsTrue(box.Contains(c1));
				Assert.IsTrue(box.Contains(c2));
				Assert.AreEqual(0.0, box.MinDistance(c1));
				Assert.AreEqual(0.0, box.MinDistance(c2));
				//Assert.AreEqual(0.0, box.DistFromEdge(c1));
				//Assert.AreEqual(0.0, box.DistFromEdge(c2));

				if (box.Contains(c3))
				{
					Assert.AreEqual(0.0, box.MinDistance(c3));
					Assert.IsTrue(box.DistFromEdge(c3) <= c3.DistanceTo(c1));
					Assert.IsTrue(box.DistFromEdge(c3) <= c3.DistanceTo(c2));

					BoundingBox box2 = box;
					box2.ExpandBy(c3);
					Assert.AreEqual(box.MinLatitude, box2.MinLatitude);
					Assert.AreEqual(box.MaxLatitude, box2.MaxLatitude);
					Assert.AreEqual(box.MinLongitude, box2.MinLongitude);
					Assert.AreEqual(box.MaxLongitude, box2.MaxLongitude);
					Assert.AreEqual(box.Area, box2.Area);

					BoundingBox box3 = new BoundingBox(c3);
					Assert.IsTrue(box.Intersects(box3));
				}
				else
				{
					double minDist = box.MinDistance(c3);
					double edgeDist = box.DistFromEdge(c3);
					double d13 = c1.DistanceTo(c3);
					double d23 = c2.DistanceTo(c3);

					Assert.IsTrue(minDist > 0);
					Assert.IsTrue(minDist == edgeDist);
					Assert.IsTrue(edgeDist <= d13 * 1.002);
					Assert.IsTrue(edgeDist <= d23 * 1.002);

					BoundingBox box3 = new BoundingBox(c3);
					Assert.IsFalse(box.Intersects(box3));

					box3.ExpandBy(box);
					Assert.IsTrue(box3.Area > box.Area);
				}

				BoundingBox boxb = new BoundingBox(c3);
				boxb.ExpandBy(c4);

				BoundingBox boxc = box;
				boxc.ExpandBy(boxb);

				Assert.IsTrue(boxc.Contains(box));
				Assert.IsTrue(boxc.Contains(boxb));
			}
		}

		private static GeoCoordinate RandomPoint(Random r)
		{
			var c = new GeoCoordinate(r.NextDouble() * 180 - 90, r.NextDouble() * 360);
			return c;
		}

	}
}
