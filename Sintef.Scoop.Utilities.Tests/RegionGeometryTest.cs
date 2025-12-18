//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sintef.Scoop.Utilities.GeoCoding;
using Sintef.Scoop.Utilities.GeoGeometry;
using Sintef.Scoop.Utilities.GeoRegions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities.Tests
{
	[TestClass]
	public class RegionGeometryTest
	{
		private static readonly double _toleranceBearing = 1e-5;

		/// <summary>
		/// Tests if two closed polygons are the same. In order to be the same, they must have the same orientation
		/// and the same coordinates in the same circular order, but the two corner lists do not need to
		/// start at the same coordiante
		/// </summary>
		/// <param name="pol0">Polygon to be compared</param>
		/// <param name="pol1">The other polygon to be compared</param>
		/// <param name="tolerance">The tolerance in meter when testing if two GeoCoordinates are considered to be the same</param>
		private bool ClosedPolygonsSame(ClosedGeoPolygon pol0, ClosedGeoPolygon pol1, double tolerance = 0)
		{
			List<GeoCoordinate> corners0 = pol0.Corners.ToList();
			List<GeoCoordinate> corners1 = pol1.Corners.ToList();
			int nmbCorners = corners0.Count;

			// Test if orientations are the same
			if (pol0.HasPositiveOrientation != pol1.HasPositiveOrientation)
			{
				return false;
			}

			// Test if number of corners are the same
			if (nmbCorners != corners1.Count)
			{
				return false;
			}

			for (int startIdx1 = 0; startIdx1 < nmbCorners; ++startIdx1)
			{
				// Test if polygon corner lists are the same when starting at corner 0 and corner startIdx1
				// for the two polygons

				int firstNotEqual = 0;
				while (firstNotEqual < nmbCorners)
				{
					GeoCoordinate c0 = corners0[firstNotEqual];
					GeoCoordinate c1 = corners1[(firstNotEqual + startIdx1) % nmbCorners];
					bool same = tolerance == 0
						? c0.Latitude == c1.Latitude && c0.Longitude == c1.Longitude
						: c0.DistanceTo(c1) < tolerance;
					if (!same)
					{
						break;
					}
					++firstNotEqual;
				}

				if (firstNotEqual == nmbCorners)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Tests if two GeoRegionComponents are the same, i.e. if they have the same outer loop
		/// and the same inner loops (in arbitrary order)
		/// </summary>
		/// <param name="comp0">The region component to be compared</param>
		/// <param name="comp1">The other region component to be compared</param>
		/// <param name="tolerance">The tolerance in meter when testing if two GeoCoordinates are considered to be the same</param>
		private bool RegionComponentsSame(GeoRegionComponent comp0, GeoRegionComponent comp1, double tolerance = 0)
		{
			// Test outer loops
			if (!ClosedPolygonsSame(comp0.OuterLoop, comp1.OuterLoop, tolerance))
			{
				return false;
			}

			// Test list of inner loops. For each inner loop in first compoenent, see if the
			// loop exists in the other component, if so, remove it from list of loops to be compared to
			List<ClosedGeoPolygon> inner1Copy = new List<ClosedGeoPolygon>(comp1.InnerLoops);
			List<ClosedGeoPolygon> inner0 = new List<ClosedGeoPolygon>(comp0.InnerLoops);

			if (inner0.Count != inner1Copy.Count)
			{
				return false;
			}

			foreach (ClosedGeoPolygon pol0 in inner0)
			{
				ClosedGeoPolygon sameInInner1 = null;
				foreach (ClosedGeoPolygon pol1 in inner1Copy)
					if (ClosedPolygonsSame(pol0, pol1, tolerance))
					{
						sameInInner1 = pol1;
						break;
					}

				if (sameInInner1 == null)
				{
					return false;
				}

				inner1Copy.Remove(sameInInner1);
			}

			return true;
		}

		/// <summary>
		/// Test if two GeoRegions are the same, i.e. if they have the same components (in arbitrary order)
		/// </summary>
		/// <param name="expected">The other region to be compared</param>
		/// <param name="actual">The region to be compared</param>
		/// <param name="tolerance">The tolerance in meter when testing if two GeoCoordinates are considered to be the same</param>
		private bool RegionsSame(GeoRegion expected, GeoRegion actual, double tolerance = 0)
		{
			if (actual.IsAll)
			{
				return expected.IsAll;
			}
			else if (expected.IsAll)
			{
				return false;
			}

			// Test list of components. For each inner loop in first region, see if the
			// component exists in the other component, if so, remove it from list of components to be compared to
			List<GeoRegionComponent> expectedComponents = new List<GeoRegionComponent>(expected.RegionComponents);
			List<GeoRegionComponent> actualComponents = new List<GeoRegionComponent>(actual.RegionComponents);

			if (actualComponents.Count != expectedComponents.Count)
			{
				return false;
			}

			foreach (GeoRegionComponent actualComp in actualComponents)
			{
				GeoRegionComponent sameInExpected = null;
				foreach (GeoRegionComponent comp in expectedComponents)
					if (RegionComponentsSame(actualComp, comp, tolerance))
					{
						sameInExpected = comp;
						break;
					}

				if (sameInExpected == null)
				{
					return false;
				}

				expectedComponents.Remove(sameInExpected);
			}

			return true;
		}

		/// <summary>
		/// Returns whether a polygon region is (approximately) less than half of the Earth surface area
		/// </summary>
		/// <param name="polygon">The polygon tested for holding a small area</param>
		private bool PolygonRegionIsSmall(ClosedGeoPolygon polygon)
		{
			return polygon.Area < 255050000;
		}

		private ClosedGeoPolygon ClosedPolygonFromLatLon(IList<double> latLon, bool expectedOrientationPositive, RegionEdgeType edgeType)
		{
			List<GeoCoordinate> coordinates = new List<GeoCoordinate>();
			int latLonCount = latLon.Count;
			for (int i = 0; i < latLonCount; i += 2)
			{
				coordinates.Add(new GeoCoordinate(latLon[i], latLon[i + 1]));
			}
			ClosedGeoPolygon polygon;
			if (edgeType == RegionEdgeType.UnitSphereShortestPaths)
			{
				polygon = ClosedGeoPolygon.UnitSphereShortestPathsPolygon(coordinates, expectedOrientationPositive, ClosedGeoPolygon.BigAreaAction.OppositeOrientationOnComplement);
			}
			else
			{
				polygon = ClosedGeoPolygon.LatitudeLongitudeStraightLinesPolygon(coordinates);
			}
			if (polygon.HasPositiveOrientation != expectedOrientationPositive)
			{
				throw new Exception("Created polygon did not have expected orientation");
			}
			return polygon;
		}

		private ClosedGeoPolygon Remove180(ClosedGeoPolygon oldPolygon)
		{
			List<GeoCoordinate> corners = oldPolygon.Corners.ToList();
			int nmbCorners = corners.Count;

			if (oldPolygon.EdgeType == RegionEdgeType.UnitSphereShortestPaths)
			{
				List<Coordinate> cornersCartesian = oldPolygon.CornersCartesian.ToList();

				int idx = 0;
				while (idx < nmbCorners)
				{
					double bearingNext = UnitSphereGeometry.Bearing(cornersCartesian[idx], cornersCartesian[(idx + 1) % nmbCorners]);
					double bearingPrev = UnitSphereGeometry.Bearing(cornersCartesian[idx], cornersCartesian[(idx + nmbCorners - 1) % nmbCorners]);
					if (Math.Abs(180 - Math.Abs(bearingNext - bearingPrev)) < _toleranceBearing)
					{
						corners.RemoveAt(idx);
						cornersCartesian.RemoveAt(idx);
						--nmbCorners;
					}
					else
					{
						++idx;
					}
				}

				return ClosedGeoPolygon.UnitSphereShortestPathsPolygon(corners, oldPolygon.HasPositiveOrientation, ClosedGeoPolygon.BigAreaAction.Keep);
			}
			else
			{
				int idx = 0;
				while (idx < nmbCorners)
				{
					double bearingNext = GeoCoordinate.AzimutFromLongitudeLatitudeStraightLine(corners[idx], corners[(idx + 1) % nmbCorners]);
					double bearingPrev = GeoCoordinate.AzimutFromLongitudeLatitudeStraightLine(corners[idx], corners[(idx + nmbCorners - 1) % nmbCorners]);
					if (Math.Abs(180 - Math.Abs(bearingNext - bearingPrev)) < _toleranceBearing)
					{
						corners.RemoveAt(idx);
						--nmbCorners;
					}
					else
					{
						++idx;
					}
				}

				return ClosedGeoPolygon.LatitudeLongitudeStraightLinesPolygon(corners);
			}
		}

		private GeoRegionComponent Remove180(GeoRegionComponent oldComponent)
		{
			return new GeoRegionComponent(Remove180(oldComponent.OuterLoop), oldComponent.InnerLoops.Select(pol => Remove180(pol)));
		}

		private GeoRegion Remove180(GeoRegion oldRegion)
		{
			if (oldRegion.IsAll)
			{
				return oldRegion;
			}

			GeoRegion region = new GeoRegion();
			foreach (GeoRegionComponent component in oldRegion.RegionComponents)
			{
				region.AddRegion(Remove180(component));
			}
			return region;
		}


		private void AssertEqualByTolerance(double expected, double actual, double tolerance, string errorMessage = null)
		{
			bool test = Math.Abs(actual - expected) < tolerance;

			if (errorMessage == null)
			{
				Assert.IsTrue(test);
			}
			else
			{
				string parsedErrorMessage = errorMessage
					.Replace("%Exp%", expected.ToInvariantString())
					.Replace("%Act%", actual.ToInvariantString())
					.Replace("%Tol%", tolerance.ToInvariantString());
				Assert.IsTrue(test, parsedErrorMessage);
			}
		}

		[TestMethod]
		public void RegionAreaTest()
		{
			foreach (RegionEdgeType edgeType in new[] { RegionEdgeType.UnitSphereShortestPaths, RegionEdgeType.LatitudeLongitudeStraightLines })
			{
				ClosedGeoPolygon outer1 = ClosedPolygonFromLatLon(new double[] { 0, 0, 0, 2, 2, 2, 2, 0 }, true, edgeType);
				ClosedGeoPolygon inner1 = ClosedPolygonFromLatLon(new double[] { 0.5, 0.5, 1.5, 0.5, 1.5, 1.5, 0.5, 1.5 }, false, edgeType);
				ClosedGeoPolygon outer2 = ClosedPolygonFromLatLon(new double[] { 0, 3, 0, 4, 1, 4, 1, 3 }, true, edgeType);

				GeoRegion regionFromPolygons = new GeoRegion(new ClosedGeoPolygon[] { outer1, outer2, inner1 });
				GeoRegion regionFromComponentAdding = new GeoRegion();
				regionFromComponentAdding.AddRegion(new GeoRegionComponent(outer1, new ClosedGeoPolygon[] { inner1 }));
				regionFromComponentAdding.AddRegion(new GeoRegionComponent(outer2));

				Assert.IsTrue(RegionsSame(regionFromPolygons, regionFromComponentAdding), "Regions are not identical");
				double expectedArea = edgeType == RegionEdgeType.UnitSphereShortestPaths ? 49564.4 : 49227.9;
				AssertEqualByTolerance(expectedArea, regionFromPolygons.Area, 0.1, "Region area expected to be %Exp% by tolerance of %Tol%, was %Act%.");
			}
		}

		[TestMethod]
		public void RegionContainsCoordinate()
		{
			foreach (RegionEdgeType edgeType in new[] { RegionEdgeType.UnitSphereShortestPaths, RegionEdgeType.LatitudeLongitudeStraightLines })
			{
				GeoRegion region = new GeoRegion(ClosedPolygonFromLatLon(new double[] { 60, 0, 60, 10, 61, 10, 61, 0 }, true, edgeType));

				GeoCoordinate coordOutside = new GeoCoordinate(60.5, 11);
				GeoCoordinate coordInside = new GeoCoordinate(60.5, 5);
				GeoCoordinate coordInsideButAboveCornerLatitudes = new GeoCoordinate(61.05, 5);
				GeoCoordinate coordOutsideAboveTopEdge = new GeoCoordinate(61.05, 9.5);
				GeoCoordinate coordCorner = new GeoCoordinate(60, 0);

				Assert.IsFalse(region.Contains(coordOutside, true));
				Assert.IsFalse(region.Contains(coordOutside, false));
				Assert.IsTrue(region.Contains(coordInside, true));
				Assert.IsTrue(region.Contains(coordInside, false));
				if (edgeType == RegionEdgeType.UnitSphereShortestPaths)
				{
					Assert.IsTrue(region.Contains(coordInsideButAboveCornerLatitudes, true));
					Assert.IsTrue(region.Contains(coordInsideButAboveCornerLatitudes, false));
				}
				else
				{
					Assert.IsFalse(region.Contains(coordInsideButAboveCornerLatitudes, true));
					Assert.IsFalse(region.Contains(coordInsideButAboveCornerLatitudes, false));
				}
				Assert.IsFalse(region.Contains(coordOutsideAboveTopEdge, true));
				Assert.IsFalse(region.Contains(coordOutsideAboveTopEdge, false));
				Assert.IsTrue(region.Contains(coordCorner, true));
				Assert.IsFalse(region.Contains(coordCorner, false));
			}
		}

		[TestMethod]
		public void ClosedPolygonOrientationTest()
		{
			ClosedGeoPolygon checkPolygonPositive = ClosedPolygonFromLatLon(new double[] { 0, 0, 0, 4, 2, 2 }, true, RegionEdgeType.UnitSphereShortestPaths);
			ClosedGeoPolygon checkPolygonNegative = ClosedPolygonFromLatLon(new double[] { 0, 0, 2, 2, 0, 4 }, false, RegionEdgeType.UnitSphereShortestPaths);

			// Coordinates in negative orientation order
			GeoCoordinate c0 = new GeoCoordinate(0, 0);
			GeoCoordinate c1 = new GeoCoordinate(2, 2);
			GeoCoordinate c2 = new GeoCoordinate(0, 4);
			GeoCoordinate[] coords = new GeoCoordinate[] { c0, c1, c2 };

			// Check polygon built with BigAreaAction.Keep
			ClosedGeoPolygon polKeep = ClosedGeoPolygon.UnitSphereShortestPathsPolygon(coords, true, ClosedGeoPolygon.BigAreaAction.Keep);
			Assert.IsTrue(polKeep.HasPositiveOrientation, "polKeep should have positive orientation");
			Assert.IsFalse(PolygonRegionIsSmall(polKeep), "polKeep should be big");

			// Check polygon built with BigAreaAction.SameOrientationOnComplement
			ClosedGeoPolygon polSameOrder = ClosedGeoPolygon.UnitSphereShortestPathsPolygon(coords, true, ClosedGeoPolygon.BigAreaAction.SameOrientationOnComplement);
			Assert.IsTrue(polSameOrder.HasPositiveOrientation, "polSameOrder should have positive orientation");
			Assert.IsTrue(PolygonRegionIsSmall(polSameOrder), "polSameOrder should be small");
			Assert.IsTrue(ClosedPolygonsSame(polSameOrder, checkPolygonPositive), "polSameOrder should be same as checkPolygonPositive");
			Assert.IsFalse(ClosedPolygonsSame(polSameOrder, checkPolygonNegative), "polSameOrder should be not same as checkPolygonNegative");

			// Check polygon built with BigAreaAction.OppositeOrientationOnComplement
			ClosedGeoPolygon polReverseOrder = ClosedGeoPolygon.UnitSphereShortestPathsPolygon(coords, true, ClosedGeoPolygon.BigAreaAction.OppositeOrientationOnComplement);
			Assert.IsFalse(polReverseOrder.HasPositiveOrientation, "polReverseOrder should have negative orientation");
			Assert.IsTrue(PolygonRegionIsSmall(polReverseOrder), "polReverseOrder should be small");
			Assert.IsTrue(ClosedPolygonsSame(polReverseOrder, checkPolygonNegative), "polReverseOrder should be same as checkPolygonNegative");
			Assert.IsFalse(ClosedPolygonsSame(polReverseOrder, checkPolygonPositive), "polReverseOrder should be same as checkPolygonPositive");
		}

		[TestMethod]
		public void ClosedPolygonContainsSegment()
		{
			foreach (RegionEdgeType edgeType in new[] { RegionEdgeType.UnitSphereShortestPaths, RegionEdgeType.LatitudeLongitudeStraightLines })
			{
				ClosedGeoPolygon pol = ClosedPolygonFromLatLon(new double[] { 0.0, 0.0, 0, 1, 1, 1, 1, 0 }, true, edgeType);

				// All of segment is inside polygon, even if it does not hit edge
				GeoCoordinate startAllInside = new GeoCoordinate(0.3, 0.3);
				GeoCoordinate endAllInside = new GeoCoordinate(0.7, 0.7);
				Assert.IsTrue(pol.ContainsPartOfSegment(startAllInside, endAllInside));

				// Part of segment is inside polygon, even if start/end points are outside
				GeoCoordinate startPartInside = new GeoCoordinate(-1, 0);
				GeoCoordinate endPartInside = new GeoCoordinate(3, 0.1);
				Assert.IsTrue(pol.ContainsPartOfSegment(startPartInside, endPartInside));

				// All of segment is outside polygon, even if it hits some of the prolonged edge lines outside the edges,
				// and some of the edges hit the segment line outside the segment
				GeoCoordinate startOutside = new GeoCoordinate(-1, -0.1);
				GeoCoordinate endOutside = new GeoCoordinate(-0.1, 0.1);
				Assert.IsFalse(pol.ContainsPartOfSegment(startOutside, endOutside));
			}
		}

		[TestMethod]
		public void SimpleRegionOperationTest()
		{
			foreach (RegionEdgeType edgeType in new[] { RegionEdgeType.UnitSphereShortestPaths, RegionEdgeType.LatitudeLongitudeStraightLines })
			{
				// Test all possible binary operations on any combination of the two given regions
				GeoRegion region1 = new GeoRegion(ClosedPolygonFromLatLon(new double[] { 1, 1, 2, 5, 5, 2 }, true, edgeType));
				GeoRegion region2 = new GeoRegion(ClosedPolygonFromLatLon(new double[] { 1, -2, 0, 2, 3, 3 }, true, edgeType));

				// region1 and region2 will split the entire area in four parts. A number Idx from 0 to 15 describes all possible ways to put these parts together by
				// Idx = (1 if the part inside region1 and inside region2 is included)
				//     + (2 if the part inside region1 and outside region2 is included)
				//     + (4 if the part outside region1 and inside region2 is included)
				//     + (8 if the part outside region1 and outside region2 is included), only if edgeType is UnitSphereShortestPaths

				GeoCoordinate lineIntersect1;
				GeoCoordinate lineIntersect2;

				if (edgeType == RegionEdgeType.UnitSphereShortestPaths)
				{
					lineIntersect1 = UnitSphereGeometry.LineIntersection(
						new GeoCoordinate(1, 1), new GeoCoordinate(2, 5),
						new GeoCoordinate(0, 2), new GeoCoordinate(3, 3),
						out double _, out double _);
					lineIntersect2 = UnitSphereGeometry.LineIntersection(
						new GeoCoordinate(1, 1), new GeoCoordinate(5, 2),
						new GeoCoordinate(1, -2), new GeoCoordinate(3, 3),
						out double _, out double _);
				}
				else
				{
					lineIntersect1 = LongitudeLatitudeStraightLinesGeometry.LineIntersection(
						new GeoCoordinate(1, 1), new GeoCoordinate(2, 5),
						new GeoCoordinate(0, 2), new GeoCoordinate(3, 3),
						out double _, out double _);
					lineIntersect2 = LongitudeLatitudeStraightLinesGeometry.LineIntersection(
						new GeoCoordinate(1, 1), new GeoCoordinate(5, 2),
						new GeoCoordinate(1, -2), new GeoCoordinate(3, 3),
						out double _, out double _);
				}
				double lat1 = lineIntersect1.Latitude;
				double lon1 = lineIntersect1.Longitude;
				double lat2 = lineIntersect2.Latitude;
				double lon2 = lineIntersect2.Longitude;

				ClosedGeoPolygon polIdx1 = ClosedPolygonFromLatLon(new double[] { lat1, lon1, 3, 3, lat2, lon2, 1, 1 }, true, edgeType);
				ClosedGeoPolygon polIdx2 = ClosedPolygonFromLatLon(new double[] { lat1, lon1, 2, 5, 5, 2, lat2, lon2, 3, 3 }, true, edgeType);
				ClosedGeoPolygon polIdx4 = ClosedPolygonFromLatLon(new double[] { 0, 2, lat1, lon1, 1, 1, lat2, lon2, 1, -2 }, true, edgeType);
				ClosedGeoPolygon polIdx7 = ClosedPolygonFromLatLon(new double[] { 0, 2, lat1, lon1, 2, 5, 5, 2, lat2, lon2, 1, -2 }, true, edgeType);


				GeoRegion regionIdx6 = new GeoRegion();
				regionIdx6.AddRegion(new GeoRegionComponent(polIdx2));
				regionIdx6.AddRegion(new GeoRegionComponent(polIdx4));

				// The 8 different regions possible to get by combining the three limited parts, region Idx in the array corresponds to the region described by the number Idx above
				List<GeoRegion> allCombinedRegions = new List<GeoRegion>
				{
					new GeoRegion(),
					new GeoRegion(polIdx1),
					new GeoRegion(polIdx2),
					region1,
					new GeoRegion(polIdx4),
					region2,
					regionIdx6,
					new GeoRegion(polIdx7)
				};
				if (edgeType == RegionEdgeType.UnitSphereShortestPaths)
				{
					// Extend to 16 different regions possible to get by combining the four parts, i.e. the three limited parts pluss the part of everything outside both original regions
					ClosedGeoPolygon polIdx8 = ClosedGeoPolygon.UnitSphereShortestPathsPolygon(polIdx7.Corners.Reverse(), true, ClosedGeoPolygon.BigAreaAction.Keep);
					GeoRegion regionIdx9 = new GeoRegion();
					regionIdx9.AddRegion(new GeoRegionComponent(polIdx1));
					regionIdx9.AddRegion(new GeoRegionComponent(polIdx8));

					allCombinedRegions.Add(new GeoRegion(polIdx8));
					allCombinedRegions.Add(regionIdx9);
					allCombinedRegions.Add(new GeoRegion(ClosedGeoPolygon.UnitSphereShortestPathsPolygon(region2.RegionComponents.First().OuterLoop.Corners.Reverse(), true, ClosedGeoPolygon.BigAreaAction.Keep)));
					allCombinedRegions.Add(new GeoRegion(ClosedGeoPolygon.UnitSphereShortestPathsPolygon(polIdx4.Corners.Reverse(), true, ClosedGeoPolygon.BigAreaAction.Keep)));
					allCombinedRegions.Add(new GeoRegion(ClosedGeoPolygon.UnitSphereShortestPathsPolygon(region1.RegionComponents.First().OuterLoop.Corners.Reverse(), true, ClosedGeoPolygon.BigAreaAction.Keep)));
					allCombinedRegions.Add(new GeoRegion(ClosedGeoPolygon.UnitSphereShortestPathsPolygon(polIdx2.Corners.Reverse(), true, ClosedGeoPolygon.BigAreaAction.Keep)));
					allCombinedRegions.Add(new GeoRegion(ClosedGeoPolygon.UnitSphereShortestPathsPolygon(polIdx1.Corners.Reverse(), true, ClosedGeoPolygon.BigAreaAction.Keep)));
					allCombinedRegions.Add(GeoRegion.All());
				}

				int nmbRegions = allCombinedRegions.Count;

				// Test all complements
				if (edgeType == RegionEdgeType.UnitSphereShortestPaths)
				{
					foreach (int idx in Enumerable.Range(0, nmbRegions))
					{
						Assert.IsTrue(RegionsSame(allCombinedRegions[15 - idx], Remove180(RegionOperations.Complement(allCombinedRegions[idx])), 1), $"Unexpected complement of region {idx}");
					}
				}

				// Test all binary operations
				foreach (int idx1 in Enumerable.Range(0, nmbRegions))
				{
					foreach (int idx2 in Enumerable.Range(0, nmbRegions))
					{
						Assert.IsTrue(RegionsSame(allCombinedRegions[idx1 | idx2], Remove180(RegionOperations.Union(allCombinedRegions[idx1], allCombinedRegions[idx2])), 1),
							$"Unexpected union of region {idx1} and {idx2}");
						Assert.IsTrue(RegionsSame(allCombinedRegions[idx1 & idx2], Remove180(RegionOperations.Intersection(allCombinedRegions[idx1], allCombinedRegions[idx2])), 1),
							$"Unexpected intersection of region {idx1} and {idx2}");
						Assert.IsTrue(RegionsSame(allCombinedRegions[idx1 & (15 - idx2)], Remove180(RegionOperations.Minus(allCombinedRegions[idx1], allCombinedRegions[idx2])), 1),
							$"Unexpected subtraction of region {idx1} from region {idx2}");
						Assert.IsTrue(RegionsSame(allCombinedRegions[(idx1 | idx2) - (idx1 & idx2)], Remove180(RegionOperations.SymmetricDifference(allCombinedRegions[idx1], allCombinedRegions[idx2])), 1),
							$"Unexpected symmetric differenc between region {idx1} and {idx2}");
					}
				}

			}
		}

		[TestMethod]
		public void NeighbourRegionUnionTest()
		{
			foreach (RegionEdgeType edgeType in new[] { RegionEdgeType.UnitSphereShortestPaths, RegionEdgeType.LatitudeLongitudeStraightLines })
			{
				GeoRegion region1 = new GeoRegion(ClosedPolygonFromLatLon(new double[] { 1, 1, 2, 5, 5, 2 }, true, edgeType));
				GeoRegion region2 = new GeoRegion(ClosedPolygonFromLatLon(new double[] { 1, 1, 5, 2, 4, 0 }, true, edgeType));

				GeoRegion calculatedUnion = RegionOperations.Union(region1, region2);
				GeoRegion expectedUnion = new GeoRegion(ClosedPolygonFromLatLon(new double[] { 4, 0, 1, 1, 2, 5, 5, 2 }, true, edgeType));
				Assert.IsTrue(RegionsSame(expectedUnion, calculatedUnion, 10), "Did not get expected union");
			}
		}

		[TestMethod]
		public void DisjointPolygonRegionUnionsAndIntersections()
		{
			foreach (RegionEdgeType edgeType in new[] { RegionEdgeType.UnitSphereShortestPaths, RegionEdgeType.LatitudeLongitudeStraightLines })
			{
				// region1 is limited by the square with latitude and longitude corners +/- 2,
				// and with a hole defined by the square with latitude and longitude corners +/- 1
				ClosedGeoPolygon outsideRegion1 = ClosedPolygonFromLatLon(new double[] { -2, -2, -2, 2, 2, 2, 2, -2 }, true, edgeType);
				ClosedGeoPolygon holeRegion1 = ClosedPolygonFromLatLon(new double[] { -1, -1, 1, -1, 1, 1, -1, 1 }, false, edgeType);
				GeoRegion region1 = new GeoRegion();
				region1.AddRegion(new GeoRegionComponent(outsideRegion1, new[] { holeRegion1 }));

				// Union and intersection with a square outside region 1
				GeoRegion region2Outside = new GeoRegion(ClosedPolygonFromLatLon(new double[] { -2.5, -2.5, -2.5, 2.5, 2.5, 2.5, 2.5, -2.5 }, true, edgeType));

				Assert.IsTrue(RegionsSame(region2Outside, RegionOperations.Union(region1, region2Outside), 1), "Did not get expected union with region2Outside");
				Assert.IsTrue(RegionsSame(region2Outside, RegionOperations.Union(region2Outside, region1), 1), "Did not get expected union with region2Outside");
				Assert.IsTrue(RegionsSame(region1, RegionOperations.Intersection(region1, region2Outside), 1), "Did not get expected intersection with region2Outside");
				Assert.IsTrue(RegionsSame(region1, RegionOperations.Intersection(region2Outside, region1), 1), "Did not get expected intersection with region2Outside");

				// Union and intersection with a square between the outer and inner loop of region 1
				ClosedGeoPolygon polygonRegion2Between = ClosedPolygonFromLatLon(new double[] { -1.5, -1.5, -1.5, 1.5, 1.5, 1.5, 1.5, -1.5 }, true, edgeType);
				GeoRegion region2Between = new GeoRegion(polygonRegion2Between);
				GeoRegion expectedUnionBetween = new GeoRegion(outsideRegion1);
				GeoRegion expectedIntersectionBetween = new GeoRegion();
				expectedIntersectionBetween.AddRegion(new GeoRegionComponent(polygonRegion2Between, new[] { holeRegion1 }));

				GeoRegion reg1 = RegionOperations.Union(region1, region2Outside);
				Assert.IsTrue(RegionsSame(expectedUnionBetween, RegionOperations.Union(region1, region2Between), 1), "Did not get expected union with region2Between");
				Assert.IsTrue(RegionsSame(expectedUnionBetween, RegionOperations.Union(region2Between, region1), 1), "Did not get expected union with region2Between");
				Assert.IsTrue(RegionsSame(expectedIntersectionBetween, RegionOperations.Intersection(region1, region2Between), 1), "Did not get expected intersection with region2Between");
				Assert.IsTrue(RegionsSame(expectedIntersectionBetween, RegionOperations.Intersection(region2Between, region1), 1), "Did not get expected intersection with region2Between");

				// Union and intersection with a square inside the hole in region 1
				ClosedGeoPolygon polygonRegion2Inside = ClosedPolygonFromLatLon(new double[] { -0.5, -0.5, -0.5, 0.5, 0.5, 0.5, 0.5, -0.5 }, true, edgeType);
				GeoRegion region2Inside = new GeoRegion(polygonRegion2Inside);
				GeoRegion expectedUnionInside = new GeoRegion();
				expectedUnionInside.AddRegion(new GeoRegionComponent(outsideRegion1, new[] { holeRegion1 }));
				expectedUnionInside.AddRegion(new GeoRegionComponent(polygonRegion2Inside));

				Assert.IsTrue(RegionsSame(expectedUnionInside, RegionOperations.Union(region1, region2Inside), 1), "Did not get expected union with region2Inside");
				Assert.IsTrue(RegionsSame(expectedUnionInside, RegionOperations.Union(region2Inside, region1), 1), "Did not get expected union with region2Inside");
				Assert.IsTrue(RegionsSame(new GeoRegion(), RegionOperations.Intersection(region1, region2Inside), 1), "Did not get expected intersection with region2Inside");
				Assert.IsTrue(RegionsSame(new GeoRegion(), RegionOperations.Intersection(region2Inside, region1), 1), "Did not get expected intersection with region2Inside");
			}
		}

		[TestMethod]
		public void TwoRegionsWithHoleUnionTest()
		{
			foreach (RegionEdgeType edgeType in new[] { RegionEdgeType.UnitSphereShortestPaths, RegionEdgeType.LatitudeLongitudeStraightLines })
			{
				ClosedGeoPolygon outsideRegion1 = ClosedPolygonFromLatLon(new double[] { 1, 0, 0, 1, 1, 2, 3, 2, 4, 1, 3, 0 }, true, edgeType);
				ClosedGeoPolygon holeRegion1 = ClosedPolygonFromLatLon(new double[] { 1, 0.2, 3, 0.2, 3.8, 1, 3, 1.8, 1, 1.8, 0.2, 1 }, false, edgeType);
				GeoRegion region1 = new GeoRegion();
				region1.AddRegion(new GeoRegionComponent(outsideRegion1, new[] { holeRegion1 }));

				ClosedGeoPolygon outsideRegion2 = ClosedPolygonFromLatLon(new double[] { 1, 1, 0, 2, 1, 3, 3, 3, 4, 2, 3, 1 }, true, edgeType);
				ClosedGeoPolygon holeRegion2 = ClosedPolygonFromLatLon(new double[] { 1, 1.2, 3, 1.2, 3.8, 2, 3, 2.8, 1, 2.8, 0.2, 2 }, false, edgeType);
				GeoRegion region2 = new GeoRegion();
				region2.AddRegion(new GeoRegionComponent(outsideRegion2, new[] { holeRegion2 }));

				ClosedGeoPolygon outsideExpected = ClosedPolygonFromLatLon(new double[] { 1, 0, 0, 1, 0.5, 1.5, 0, 2, 1, 3, 3, 3, 4, 2, 3.5, 1.5, 4, 1, 3, 0 }, true, edgeType);
				ClosedGeoPolygon hole1Expected = ClosedPolygonFromLatLon(new double[] { 1, 0.2, 3, 0.2, 3.8, 1, 3.4, 1.4, 3, 1, 1, 1, 0.6, 1.4, 0.2, 1 }, false, edgeType);
				ClosedGeoPolygon hole2Expected = ClosedPolygonFromLatLon(new double[] { 1, 1.2, 3, 1.2, 3.3, 1.5, 3, 1.8, 1, 1.8, 0.7, 1.5 }, false, edgeType);
				ClosedGeoPolygon hole3Expected = ClosedPolygonFromLatLon(new double[] { 0.2, 2, 0.6, 1.6, 1, 2, 3, 2, 3.4, 1.6, 3.8, 2, 3, 2.8, 1, 2.8 }, false, edgeType);

				// More precise intersection coordinates:
				//ClosedGeoPolygon outsideExpected = ClosedPolygonFromLatLon(new double[] { 1, 0, 0, 1, 0.500057119753402, 1.5, 0, 2, 1, 3, 3, 3, 4, 2, 3.50039983894236, 1.5, 4, 1, 3, 0 }, true);
				//ClosedGeoPolygon hole1Expected = ClosedPolygonFromLatLon(new double[] { 1, 0.2, 3, 0.2, 3.8, 1, 3.40031440033168, 1.39993418054617, 3, 1, 1, 1, 0.600051178146445, 1.40000731110017, 0.2, 1 }, false);
				//ClosedGeoPolygon hole2Expected = ClosedPolygonFromLatLon(new double[] { 1, 1.2, 3, 1.2, 3.30023078974505, 1.5, 3, 1.8, 1, 1.8, 0.700043408701892, 1.5 }, false);
				//ClosedGeoPolygon hole3Expected = ClosedPolygonFromLatLon(new double[] { 0.2, 2, 0.600051178146444, 1.59999268889983, 1, 2, 3, 2, 3.40031440033168, 1.60006581945383, 3.8, 2, 3, 2.8, 1, 2.8 }, false);

				GeoRegion calculatedUnion = RegionOperations.Union(region1, region2);
				GeoRegion expectedUnion = new GeoRegion();
				expectedUnion.AddRegion(new GeoRegionComponent(outsideExpected, new[] { hole1Expected, hole2Expected, hole3Expected }));

				Assert.IsTrue(RegionsSame(expectedUnion, RegionOperations.Union(region1, region2), 50), "Did not get expected union of two regions with one hole each");
				Assert.IsTrue(RegionsSame(expectedUnion, RegionOperations.Union(region2, region1), 50), "Did not get expected union of two regions with one hole each");
			}
		}
	}
}
