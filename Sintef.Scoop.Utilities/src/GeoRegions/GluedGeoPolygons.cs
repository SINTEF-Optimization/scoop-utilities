//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Sintef.Scoop.Utilities.GeoCoding;
using Sintef.Scoop.Utilities.GeoGeometry;
using Sintef.Scoop.Utilities.GeoRegions.Topology;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities.GeoRegions
{
	/// <summary>
	/// An object holding a collection of closed polygons defining a region, together with some gluing edges between the polygons,
	/// so that the entire system of polygon edges and gluing edges forms a connected graph, where edges do not intersect except in common start/end points.
	/// For all polygons, the lefthand side when moving in polygon corners order is regarded as inside the region and the righthand side is regarded as outside the region.
	/// </summary>
	internal class GluedGeoPolygons
	{
		/// <summary>
		/// A gluing edge between two polygons in the region.
		/// </summary>
		internal class GlueEdge
		{
			/// <summary>
			/// The polygon corner this edge goes from, given as a GeoCoordinate.
			/// If null, this is to represent that two GluedGeoPolygons share the same coordinate, and therefore
			/// already are connected. This is only used as a return value when looking for connecting edges.
			/// Only used if EdgeType is LatitudeLongitudeStraightLines.
			/// </summary>
			internal GeoCoordinate FromGeoCoordinate { get; }

			/// <summary>
			/// The polygon corner this edge goes to, given as a GeoCoordinate.
			/// Only used if EdgeType is LatitudeLongitudeStraightLines.
			/// </summary>
			internal GeoCoordinate ToGeoCoordinate { get; }

			/// <summary>
			/// The polygon corner this edge goes from, given as unit sphere coordinates.
			/// If null, this is to represent that two GluedGeoPolygons share the same coordinate, and therefore
			/// already are connected. This is only used as a return value when looking for connecting edges.
			/// Only used if EdgeType is UnitSphereShortestPaths.
			/// </summary>
			internal Coordinate FromCoordinate { get; }

			/// <summary>
			/// The polygon corner this edge goes to, given as unit sphere coordinates.
			/// Only used if EdgeType is UnitSphereShortestPaths.
			/// </summary>
			internal Coordinate ToCoordinate { get; }

			/// <summary>
			/// Whether this edge is on the inside of the entire region.
			/// </summary>
			internal bool InsideRegion { get; }

			/// <summary>
			/// Creates a gluing edge between two polygons in the region of edge type LatitudeLongitudeStraightLines.
			/// </summary>
			/// <param name="fromGeoCoordinate">The polygon corner the edge goes from, given as a GeoCoordinate.</param>
			/// <param name="toGeoCoordinate">The polygon corner the edge goes to, given as a GeoCoordinate.</param>
			/// <param name="insideRegion">Whether the edge is on the inside of the entire region.</param>
			internal GlueEdge(GeoCoordinate fromGeoCoordinate, GeoCoordinate toGeoCoordinate, bool insideRegion)
			{
				FromGeoCoordinate = fromGeoCoordinate;
				ToGeoCoordinate = toGeoCoordinate;
				InsideRegion = insideRegion;
			}

			/// <summary>
			/// Creates a gluing edge between two polygons in the region of edge type UnitSphereShortestPaths.
			/// </summary>
			/// <param name="fromCoordinate">The polygon corner the edge goes from, given as unit sphere coordinates.</param>
			/// <param name="toCoordinate">The polygon corner the edge goes to, given as unit sphere coordinates.</param>
			/// <param name="insideRegion">Whether the edge is on the inside of the entire region.</param>
			internal GlueEdge(Coordinate fromCoordinate, Coordinate toCoordinate, bool insideRegion)
			{
				FromCoordinate = fromCoordinate;
				ToCoordinate = toCoordinate;
				InsideRegion = insideRegion;
			}

			/// <summary>
			/// Creates a gluing edge where all coordinates are null.
			/// </summary>
			/// <param name="insideRegion">Whether the edge is on the inside of the entire region.</param>
			internal GlueEdge(bool insideRegion)
			{
				InsideRegion = insideRegion;
				FromGeoCoordinate = null;
				FromCoordinate = null;
				ToGeoCoordinate = null;
				ToCoordinate = null;
			}
		}

		/// <summary>
		/// The polygons defining the entire region.
		/// </summary>
		private List<ClosedGeoPolygon> _polygons;

		/// <summary>
		/// The edges gluing all the region polygons together into one connected graph.
		/// </summary>
		private List<GlueEdge> _gluingEdges;

		/// <summary>
		/// The type of the edges in the region
		/// </summary>
		private RegionEdgeType _edgeType;

		/// <summary>
		/// All the polygon edges and gluing edges, given by their start and end GeoCoordinates.
		/// Only used if EdgeType is LatitudeLongitudeStraightLines.
		/// </summary>
		private IEnumerable<(GeoCoordinate From, GeoCoordinate To)> Edges => _polygons
			.SelectMany(pol => pol.Edges)
			.Concat(_gluingEdges.Select(ge => (ge.FromGeoCoordinate, ge.ToGeoCoordinate)));

		/// <summary>
		/// All the polygon edges and gluing edges, given by their start and end coordinates.
		/// Only used if EdgeType is UnitSphereShortestPaths.
		/// </summary>
		private IEnumerable<(Coordinate From, Coordinate To)> EdgesCartesian => _polygons
			.SelectMany(pol => pol.EdgesCartesian)
			.Concat(_gluingEdges.Select(ge => (ge.FromCoordinate, ge.ToCoordinate)));

		/// <summary>
		/// The polygons defining the entire region.
		/// </summary>
		internal IEnumerable<ClosedGeoPolygon> Polygons => _polygons.AsReadOnly();

		/// <summary>
		/// The edges gluing all the region polygons together into one connected graph.
		/// </summary>
		internal IEnumerable<GlueEdge> GluingEdges => _gluingEdges.AsReadOnly();

		/// <summary>
		/// Creates a GluedGeoPolygons object holding only one closed polygon
		/// </summary>
		/// <param name="polygon">The single polygon in the created object</param>
		private GluedGeoPolygons(ClosedGeoPolygon polygon)
		{
			_edgeType = polygon.EdgeType;
			_polygons = new List<ClosedGeoPolygon>() { polygon };
			_gluingEdges = new List<GlueEdge>();
		}

		/// <summary>
		/// Creates a GluedGeoPolygons object as a union of two GluedGeoPolygons objects together with an edge gluing the two graphs together.
		/// </summary>
		/// <param name="gluedPolygons1">The first GluedGeoPolygons object in the union</param>
		/// <param name="gluedPolygons2">The second GluedGeoPolygons object in the union</param>
		/// <param name="gluingEdge">The gluing edge going from a polygon corner in the first object to a polygon corner in the second object.
		/// If null, the two GluedGeoPolygons objects already have a common polygon corner, and no gluing edge is needed</param>
		private GluedGeoPolygons(GluedGeoPolygons gluedPolygons1, GluedGeoPolygons gluedPolygons2, GlueEdge gluingEdge)
		{
			if (gluedPolygons1._edgeType != gluedPolygons2._edgeType)
			{
				throw new InvalidOperationException($"Can not glue two glued polygons of edge type {gluedPolygons1._edgeType} and {gluedPolygons2._edgeType}");
			}
			_edgeType = gluedPolygons1._edgeType;
			_polygons = new List<ClosedGeoPolygon>();
			_polygons.AddRange(gluedPolygons1.Polygons);
			_polygons.AddRange(gluedPolygons2.Polygons);
			_gluingEdges = new List<GlueEdge>();
			_gluingEdges.AddRange(gluedPolygons1.GluingEdges);
			_gluingEdges.AddRange(gluedPolygons2.GluingEdges);
			if (gluingEdge != null)
			{
				_gluingEdges.Add(gluingEdge);
			}
		}

		/// <summary>
		/// Creates and returns a GluedGeoPolygons object for a region.
		/// </summary>
		/// <param name="region">The region to create the object for</param>
		internal static GluedGeoPolygons Glue(GeoRegion region)
		{
			return Glue(region.RegionComponents.Select(comp => Glue(comp)), false);
		}

		/// <summary>
		/// Creates and returns a GluedGeoPolygons object for a connected region component.
		/// </summary>
		/// <param name="component">The region to create the object for</param>
		private static GluedGeoPolygons Glue(GeoRegionComponent component)
		{
			GluedGeoPolygons outerGlued = new GluedGeoPolygons(component.OuterLoop);

			return Glue(Enumerable.Repeat(new GluedGeoPolygons(component.OuterLoop), 1).Concat(component.InnerLoops.Select(pol => new GluedGeoPolygons(pol))), true);
		}

		/// <summary>
		/// Creates and returns a GluedGeoPolygons object as the union of some given GluedGeoPolygons objects
		/// </summary>
		/// <param name="gluedPolygons">The objects to get the union for</param>
		/// <param name="expectGluingEdgesInside">Whether the gluing edges connecting the objects should all be on the inside (true) or outside (false) of the entire end region</param>
		private static GluedGeoPolygons Glue(IEnumerable<GluedGeoPolygons> gluedPolygons, bool expectGluingEdgesInside)
		{
			// The list of objects to create the union for. We will repeat to search for two objects that can be glued together by a gluing edge,
			// and replace the two objects with a new union, untill we only have one object in the list, this is the final result.
			List<GluedGeoPolygons> polygonsToBeGlued = new List<GluedGeoPolygons>(gluedPolygons);
			int nmbPolygons = polygonsToBeGlued.Count;
			if (nmbPolygons == 0)
			{
				throw new Exception("GluedGeoPolygons can not be created for empty region");
			}

			// Keeps track of GluedGeoPolygons objects that can not be glued together without using gluing edges that intersects with other edges.
			// This is for caching, so we do not waste time by repeating the calculations
			Dictionary<GluedGeoPolygons, HashSet<GluedGeoPolygons>> cannotGlue = new Dictionary<GluedGeoPolygons, HashSet<GluedGeoPolygons>>();

			foreach (GluedGeoPolygons ggp in polygonsToBeGlued)
			{
				cannotGlue.Add(ggp, new HashSet<GluedGeoPolygons>());
			}

			while (nmbPolygons > 1)
			{
				bool edgeFound = false;
				foreach (int idx1 in Enumerable.Range(0, nmbPolygons - 1))
				{
					GluedGeoPolygons glued1 = polygonsToBeGlued[idx1];
					HashSet<GluedGeoPolygons> cannotGlue1 = cannotGlue[glued1];
					foreach (int idx2 in Enumerable.Range(idx1 + 1, nmbPolygons - (idx1 + 1)).Where(idx2 => !cannotGlue1.Contains(polygonsToBeGlued[idx2])))
					{
						GluedGeoPolygons glued2 = polygonsToBeGlued[idx2];

						// Trying to glue glued1 and glued2 together
						GlueEdge gluingEdge = GetGluingEdge(glued1, glued2, polygonsToBeGlued);

						if (gluingEdge == null)
						{
							// No gluing edge between the two GluedGeoPolygons objects was found
							cannotGlue1.Add(glued2);
							cannotGlue[glued2].Add(glued1);
						}
						else
						{
							// We have an edge gluing the two GluedGeoPolygons objects together
							GluedGeoPolygons newGlued;

							if (gluingEdge.FromGeoCoordinate == null && gluingEdge.FromCoordinate == null)
							{
								// The two GluedGeoPolygons objects are already connected in a common polygon corner.
								newGlued = new GluedGeoPolygons(glued1, glued2, null);
							}
							else if (gluingEdge.InsideRegion != expectGluingEdgesInside)
							{
								string insideDescr = gluingEdge.InsideRegion ? "inside" : "outside";
								throw new Exception($"Gluing edge was {insideDescr}, should have been opposite");
							}
							else
							{
								newGlued = new GluedGeoPolygons(glued1, glued2, gluingEdge);
							}

							// Remove the old two GluedGeoPolygons objects from our list and add the new one
							polygonsToBeGlued.Remove(glued1);
							polygonsToBeGlued.Remove(glued2);
							polygonsToBeGlued.Add(newGlued);
							--nmbPolygons;

							// Objects that can not be glued to any of the two GluedGeoPolygons objects, can not be glued to the new object either
							HashSet<GluedGeoPolygons> cannotGlueNew = new HashSet<GluedGeoPolygons>(cannotGlue1);
							cannotGlueNew.IntersectWith(cannotGlue[glued2]);
							cannotGlue.Add(newGlued, cannotGlueNew);
							foreach (GluedGeoPolygons ggp in cannotGlueNew)
							{
								cannotGlue[ggp].Add(newGlued);
							}

							edgeFound = true;
							break;
						}
					}

					if (edgeFound)
					{
						break;
					}
				}

				if (!edgeFound)
				{
					throw new Exception("Did not find any pair of polygons that could be glued");
				}
			}

			// We are down to only one object in the list, this is the result.
			return polygonsToBeGlued.First();
		}

		/// <summary>
		/// Tries to find a gluing edge between two GluedGeoPolygons objects. The edge should not intersect any of the edges in given collection of GluedGeoPolygons objects,
		/// which inludes the two objects to find the gluing edge for.
		/// If a fitting edge is found, it is returned.
		/// If a common polygon corner of the two GluedGeoPolygons objects is found, an edge with FromGeoCoordinate and FromCoordinate set to null is returned.
		/// Otherwise, if no edge is found, null is returned.
		/// </summary>
		/// <param name="gluedPolygons1">The first GluedGeoPolygons object to find the gluing edge for</param>
		/// <param name="gluedPolygons2">The second GluedGeoPolygons object to find the gluing edge for</param>
		/// <param name="allPolygons">The GluedGeoPolygons objects for which the polygon and gluing edges should not intersect the returned edge</param>
		private static GlueEdge GetGluingEdge(GluedGeoPolygons gluedPolygons1, GluedGeoPolygons gluedPolygons2, List<GluedGeoPolygons> allPolygons)
		{
			switch(gluedPolygons1._edgeType)
			{
				case RegionEdgeType.LatitudeLongitudeStraightLines:
					return GetGluingEdgeLatitudeLongitudeStraightLines(gluedPolygons1, gluedPolygons2, allPolygons);

				case RegionEdgeType.UnitSphereShortestPaths:
					return GetGluingEdgeUnitSphereShortestPaths(gluedPolygons1, gluedPolygons2, allPolygons);

				default:
					throw new InvalidOperationException($"Can not find gluing edge for polygons of type {gluedPolygons1._edgeType}");
			}
		}

		/// <summary>
		/// Tries to find a gluing edge between two GluedGeoPolygons objects of edge type LatitudeLongitudeStraightLines. The edge should not intersect any of the
		/// edges in given collection of GluedGeoPolygons objects, which inludes the two objects to find the gluing edge for. If a fitting edge is found, it is returned.
		/// If a common polygon corner of the two GluedGeoPolygons objects is found, an edge with FromGeoCoordinate and FromCoordinate set to null is returned.
		/// Otherwise, if no edge is found, null is returned.
		/// </summary>
		/// <param name="gluedPolygons1">The first GluedGeoPolygons object to find the gluing edge for</param>
		/// <param name="gluedPolygons2">The second GluedGeoPolygons object to find the gluing edge for</param>
		/// <param name="allPolygons">The GluedGeoPolygons objects for which the polygon and gluing edges should not intersect the returned edge</param>
		private static GlueEdge GetGluingEdgeLatitudeLongitudeStraightLines(GluedGeoPolygons gluedPolygons1, GluedGeoPolygons gluedPolygons2, List<GluedGeoPolygons> allPolygons)
		{
			// First test if some corners in the two GluedGeoPolygons are essentially the same
			List<(ClosedGeoPolygon ToPolygon, GeoCoordinate ToCoordinate)> toCoordinates = gluedPolygons2._polygons.SelectMany(pol => pol.Corners.Select(coord => (pol, coord))).ToList();
			if (gluedPolygons1
				._polygons
				.SelectMany(pol => pol.Corners)
				.Any(coordFrom => toCoordinates.Any(pair => coordFrom.DistanceTo(pair.ToCoordinate) <= RegionTopology.SameGeoCoordinateTolerance)))
			{
				return new GlueEdge(true);
			}

			// Next runs through all pairs of polygon corners, trying to create a gluing edge between them.
			List<(GeoCoordinate From, GeoCoordinate To)> allGluedGeoPolygonEdges = allPolygons.SelectMany(ggp => ggp.Edges).ToList();
			foreach (ClosedGeoPolygon fromPol in gluedPolygons1._polygons)
			{
				foreach (GeoCoordinate coordFrom in fromPol.Corners)
				{
					foreach ((ClosedGeoPolygon toPol, GeoCoordinate coordTo) in toCoordinates)
					{
						if (allGluedGeoPolygonEdges.All(pair => !LongitudeLatitudeStraightLinesGeometry.LinesIntersectAndHaveDifferentEndPoints(coordFrom, coordTo, pair.From, pair.To, RegionTopology.SameGeoCoordinateTolerance)))
						{
							// The edge between the corners does not intersect with any other edge in the list of GluedGeoPolygons objects, use this.
							GeoCoordinate mid = new GeoCoordinate(0.5 * (coordFrom.Latitude + coordTo.Latitude), 0.5 * (coordFrom.Longitude + coordTo.Longitude));
							bool inside1 = fromPol.HasPositiveOrientation ? fromPol.Contains(mid, false) : !fromPol.Contains(mid, true);
							bool inside2 = toPol.HasPositiveOrientation ? toPol.Contains(mid, false) : !toPol.Contains(mid, true);
							if (inside1 != inside2)
							{
								throw new Exception("Mid point of polygon connection edge was inside one polygon and outside the other");
							}

							return new GlueEdge(coordFrom, coordTo, inside1);
						}
					}
				}
			}

			// No connecting edge was found
			return null;
		}

		/// <summary>
		/// Tries to find a gluing edge between two GluedGeoPolygons objects of edge type UnitSphereShortestPaths. The edge should not intersect any of the
		/// edges in given collection of GluedGeoPolygons objects, which inludes the two objects to find the gluing edge for. If a fitting edge is found, it is returned.
		/// If a common polygon corner of the two GluedGeoPolygons objects is found, an edge with FromGeoCoordinate and FromCoordinate set to null is returned.
		/// Otherwise, if no edge is found, null is returned.
		/// </summary>
		/// <param name="gluedPolygons1">The first GluedGeoPolygons object to find the gluing edge for</param>
		/// <param name="gluedPolygons2">The second GluedGeoPolygons object to find the gluing edge for</param>
		/// <param name="allPolygons">The GluedGeoPolygons objects for which the polygon and gluing edges should not intersect the returned edge</param>
		private static GlueEdge GetGluingEdgeUnitSphereShortestPaths(GluedGeoPolygons gluedPolygons1, GluedGeoPolygons gluedPolygons2, List<GluedGeoPolygons> allPolygons)
		{
			// First test if some corners in the two GluedGeoPolygons are essentially the same
			List<(ClosedGeoPolygon ToPolygon, Coordinate ToCoordinate)> toCoordinates = gluedPolygons2._polygons.SelectMany(pol => pol.CornersCartesian.Select(coord => (pol, coord))).ToList();
			if (gluedPolygons1
				._polygons
				.SelectMany(pol => pol.CornersCartesian)
				.Any(coordFrom => toCoordinates.Any(pair => coordFrom.DistanceTo(pair.ToCoordinate) <= RegionTopology.SameCoordinateTolerance)))
			{
				return new GlueEdge(true);
			}

			// Next runs through all pairs of polygon corners, trying to create a gluing edge between them.
			List<(Coordinate From, Coordinate To)> allGluedGeoPolygonEdges = allPolygons.SelectMany(ggp => ggp.EdgesCartesian).ToList();
			foreach (ClosedGeoPolygon fromPol in gluedPolygons1._polygons)
			{
				foreach (Coordinate coordFrom in fromPol.CornersCartesian)
				{
					foreach ((ClosedGeoPolygon toPol, Coordinate coordTo) in toCoordinates)
					{
						if (allGluedGeoPolygonEdges.All(pair => !UnitSphereGeometry.LinesIntersectAndHaveDifferentEndPoints(coordFrom, coordTo, pair.From, pair.To, RegionTopology.SameCoordinateTolerance)))
						{
							// The edge between the corners does not intersect with any other edge in the list of GluedGeoPolygons objects, use this.
							Coordinate mid = coordFrom + coordTo;
							mid /= mid.Length;
							GeoCoordinate midGeo = UnitSphereGeometry.UnitSpherePointToGeoCoordinate(mid);
							bool inside1 = fromPol.HasPositiveOrientation ? fromPol.Contains(midGeo, false) : !fromPol.Contains(midGeo, true);
							bool inside2 = toPol.HasPositiveOrientation ? toPol.Contains(midGeo, false) : !toPol.Contains(midGeo, true);
							if (inside1 != inside2)
							{
								throw new Exception("Mid point of polygon connection edge was inside one polygon and outside the other");
							}

							return new GlueEdge(coordFrom, coordTo, inside1);
						}
					}
				}
			}

			// No connecting edge was found
			return null;
		}
	}
}
