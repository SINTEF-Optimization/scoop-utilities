//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Sintef.Scoop.Utilities.GeoCoding;
using Sintef.Scoop.Utilities.GeoGeometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities.GeoRegions.Topology
{
	/// <summary>
	/// A topology object used to get the result of applying boolean operations (union or intersection) on a set of regions.
	/// The object holds the nodes and edges from the regions, together with information on wether each side of each edge is inside or outside the region
	/// Before and after the operation has been applied to a region, the graph will be connected.
	/// </summary>
	internal class RegionTopology
	{
		/// <summary>
		/// The state of the topology regions
		/// </summary>
		private enum RegionState
		{
			/// <summary>
			/// No regions are added yet
			/// </summary>
			Empty,

			/// <summary>
			/// The topology only holds the existing region, i.e. all edges have inside flags set for the existing region only
			/// </summary>
			ExistingOnly,

			/// <summary>
			/// The topology only holds the applied region and the existing region, i.e. all edges have inside flags set for both the existing and applied region.
			/// This is an intermediate state that only exists 
			/// </summary>
			ExistingAndApplied,
		}

		/// <summary>
		/// The state of the topology regions. This should be evaluated before any call to an internal method, and updated at the end of the method call.
		/// </summary>
		private RegionState _state = RegionState.Empty;

		/// <summary>
		/// The lines from polygon edges or gluing edges from the applied regions.
		/// This is different from the edges in the topology, because an intersection of two lines will split the crossing edges, but keep the lines.
		/// </summary>
		private List<PolygonLine> _polygonLines = new List<PolygonLine>();

		/// <summary>
		/// The nodes in the topology, representing GeoCoordinate points given as Cartesian points on the unit sphere.
		/// </summary>
		private List<Node> _nodes = new List<Node>();

		/// <summary>
		/// How the shapes of the edges in the regions are defined. All applied regions must have the same edge type.
		/// Before any region is applied, the edge type is set to NoEdges.
		/// </summary>
		private RegionEdgeType _edgeType = RegionEdgeType.NoEdges;

		/// <summary>
		/// The maximum distance between two GeoCoordinates, if they should be treated as the same point
		/// </summary>
		internal static readonly double SameGeoCoordinateTolerance = 1.0;

		/// <summary>
		/// The maximum distance between two unit sphere points, if they should be treated as the same point
		/// </summary>
		internal static readonly double SameCoordinateTolerance = 1e-7;

		/// <summary>
		/// The edges in the topology
		/// </summary>
		private IEnumerable<Edge> Edges => _nodes.SelectMany(node => node.Edges.Where(edge => edge.Start == node));

		/// <summary>
		/// Updates with the topolgy from a new region.
		/// The properties region.IsAll and region.IsEmpty must be false.
		/// The topology state must be either Empty or ExistingOnly.
		/// If the state is Empty, the region topology is stored into the existing region, and the state will be changed to ExistingOnly.
		/// If the state is ExistingOnly, the region topology is stored into the applied region, and the state will be changed to ExistingAndApplied.
		/// </summary>
		/// <param name="region">The the region to be applied to the topology</param>
		internal void ApplyRegion(GeoRegion region)
		{
			if (region.IsAll)
			{
				throw new Exception("RegionTopology.ApplyRegion(): Region can not be the entire Earth surface");
			}
			if (region.IsEmpty)
			{
				throw new Exception("RegionTopology.ApplyRegion(): Region can not be empty");
			}
			if (_state != RegionState.Empty && _state != RegionState.ExistingOnly)
			{
				throw new Exception($"RegionTopology.ApplyRegion(): State was {_state}, should be Empty or ExistingOnly");
			}

			if (_edgeType == RegionEdgeType.NoEdges)
			{
				_edgeType = region.EdgeType;
			}
			else if (_edgeType != region.EdgeType)
			{
				throw new InvalidOperationException($"RegionTopology.ApplyRegion(): Edge type was {_edgeType}, new region has edge type {region.EdgeType}");
			}

			GluedGeoPolygons gluedPolygon = GluedGeoPolygons.Glue(region);

			// Apply the polygon lines from the region
			foreach (ClosedGeoPolygon polygon in gluedPolygon.Polygons)
			{
				List<GeoCoordinate> geoCoords = new List<GeoCoordinate>(polygon.Corners);
				int nmbCorners = geoCoords.Count;
				List<Coordinate> coords;
				if (_edgeType == RegionEdgeType.UnitSphereShortestPaths)
				{
					coords = new List<Coordinate>(polygon.CornersCartesian);
				}
				else
				{
					coords = Enumerable.Repeat<Coordinate>(null, nmbCorners).ToList();
				}
				foreach (int idx in Enumerable.Range(0, nmbCorners))
				{
					int nextIdx = (idx + 1) % nmbCorners;
					ApplyPolygonLine(coords[idx], geoCoords[idx], coords[nextIdx], geoCoords[nextIdx], true, false);
				}
			}

			// Apply the gluing edges from the region. They are needed for possible intersections with the existing region to settle the insideness of edges according to
			// whether they are inside the existing region or not.
			foreach (GluedGeoPolygons.GlueEdge edge in gluedPolygon.GluingEdges)
			{
				if (_edgeType == RegionEdgeType.UnitSphereShortestPaths)
				{
					ApplyPolygonLine(edge.FromCoordinate, null, edge.ToCoordinate, null, edge.InsideRegion, edge.InsideRegion);
				}
				else
				{
					ApplyPolygonLine(null, edge.FromGeoCoordinate, null, edge.ToGeoCoordinate, edge.InsideRegion, edge.InsideRegion);
				}
			}

			switch (_state)
			{
				case RegionState.Empty:
					SortEdges();
					_state = RegionState.ExistingOnly;
					break;

				case RegionState.ExistingOnly:
					EnsureExistingAndAppliedConnected(region);
					SortEdges();
					SetMissingEdgeInsides();
					_state = RegionState.ExistingAndApplied;
					break;
			}
		}

		/// <summary>
		/// Complements the inside properties in all edges for the existing region.
		/// The topology state must be either ExistingOnly or ExistingAndApplied, and will not be changed.
		/// The edge type must be UnitSphereShortestPaths.
		/// </summary>
		internal void ComplementExisting()
		{
			if (_edgeType != RegionEdgeType.UnitSphereShortestPaths)
			{
				throw new InvalidOperationException($"RegionTopology.ComplementExisting(): Edge type was {_edgeType}, must be UnitSphereShortestPaths");
			}

			if (_state != RegionState.ExistingOnly && _state != RegionState.ExistingAndApplied)
			{
				throw new Exception($"RegionTopology.ComplementExisting(): State was {_state}, should be ExistingOnly or ExistingAndApplied");
			}

			foreach (Edge edge in Edges)
			{
				edge.ComplementExisting();
			}
		}

		/// <summary>
		/// Applies an operation type to the existing and applied region, stores the result in the existing region and clears the applied region.
		/// The topology state must be ExistingAndApplied, and will be changed to ExistingOnly.
		/// </summary>
		/// <param name="operationType">The operation type to be applied to the two regions</param>
		internal void ApplyOperation(RegionOperations.OperationType operationType)
		{
			if (_state != RegionState.ExistingOnly && _state != RegionState.ExistingAndApplied)
			{
				throw new Exception($"RegionTopology.ApplyOperation(): State was {_state}, should be ExistingAndApplied");
			}

			foreach (Node node in _nodes)
			{
				node.ApplyOperation(operationType);
			}
			foreach (PolygonLine line in _polygonLines)
			{
				line.FromExistingRegion = true;
			}

			_state = RegionState.ExistingOnly;
		}

		/// <summary>
		/// Returns the GeoRegion object defined by the topology
		/// </summary>
		internal GeoRegion ToRegion()
		{
			int nmbEdges = Edges.Count();
			foreach (Edge edge in Edges)
			{
				edge.PendingForOutputRegion = edge.LeftInside != edge.RightInside;
			}

			if (_edgeType == RegionEdgeType.UnitSphereShortestPaths)
			{
				// The list of all single closed polygons in the region that are not used in any GeoRegionComponent yet. They are sorted by area, with the smallest first.
				List<ClosedGeoPolygon> positivePolygons = _nodes
					.SelectMany(node => node.Edges.Where(edge => edge.LeftInside != edge.RightInside && edge.PendingForOutputRegion).SelectMany(edge => edge.GetPolygons(nmbEdges, _edgeType)))
					.OrderBy(pol => pol.Area)
					.ToList();

				if (!positivePolygons.Any())
				{
					return Edges.First().LeftInside ? GeoRegion.All() : new GeoRegion();
				}

				GeoRegion region = new GeoRegion();

				// To build a new region component, use the polygon with the smallest area as the outside polygon of the component, and the polygons inside that polygon as the inner loops.
				while (positivePolygons.Any())
				{
					ClosedGeoPolygon outerLoop = positivePolygons[0];
					positivePolygons.Remove(outerLoop);

					List<ClosedGeoPolygon> innerLoops = positivePolygons
						.Where(pol => outerLoop.Contains(pol.Corners.First().Interpolated(pol.Corners.Skip(1).First(), 0.5, 0.01)))
						.ToList();
					foreach (ClosedGeoPolygon pol in innerLoops)
					{
						positivePolygons.Remove(pol);
						pol.UseComplementRegion(false);
					}

					region.AddRegion(new GeoRegionComponent(outerLoop, innerLoops));
				}

				return region;
			}
			else
			{
				List<ClosedGeoPolygon> polygons = _nodes
					.SelectMany(node => node.Edges.Where(edge => edge.LeftInside != edge.RightInside && edge.PendingForOutputRegion).SelectMany(edge => edge.GetPolygons(nmbEdges, _edgeType)))
					.ToList();

				if (!polygons.Any())
				{
					return Edges.First().LeftInside ? GeoRegion.All() : new GeoRegion();
				}

				if (_edgeType != RegionEdgeType.LatitudeLongitudeStraightLines)
				{
					throw new InvalidOperationException($"Failed to create region from edge type {_edgeType}");
				}

				return new GeoRegion(polygons);
			}
		}

		/// <summary>
		/// Adds a line from the currently applied region to the topology. The line can be either a polygon edge or a gluing line between the polygons.
		/// The line is applied to the existing region if the current topology state is Empty, or to the applied region if the current topolgy state is ExistingOnly.
		/// </summary>
		/// <param name="from">The start coordinate of the line, null if the edge type is not UnitSphereShortestPaths</param>
		/// <param name="fromGeo">The start GeoCoordinate of the line</param>
		/// <param name="to">The end coordinate of the line, null if the edge type is not UnitSphereShortestPaths</param>
		/// <param name="toGeo">The end GeoCoordinate of the line</param>
		/// <param name="leftInside">Whether the lefthand side of the line is inside the applied region</param>
		/// <param name="rightInside">Whether the righthand side of the line is inside the applied region</param>
		private void ApplyPolygonLine(Coordinate from, GeoCoordinate fromGeo, Coordinate to, GeoCoordinate toGeo, bool leftInside, bool rightInside)
		{
			ApplyPolygonLine(GetNode(from, fromGeo), GetNode(to, toGeo), leftInside, rightInside);
		}

		/// <summary>
		/// Adds a line from the currently applied region to the topology. The line can be either a polygon edge or a gluing line between the polygons.
		/// The line is applied to the existing region if the current topology state is Empty, or to the applied region if the current topolgy state is ExistingOnly.
		/// </summary>
		/// <param name="startNode">The start node of the line</param>
		/// <param name="endNode">The end node of the line</param>
		/// <param name="leftInside">Whether the lefthand side of the line is inside the applied region</param>
		/// <param name="rightInside">Whether the righthand side of the line is inside the applied region</param>
		private void ApplyPolygonLine(Node startNode, Node endNode, bool leftInside, bool rightInside)
		{
			bool addToExistingRegion = _state == RegionState.Empty;

			PolygonLine existingLineSameNodes = addToExistingRegion ? null : startNode.Lines.FirstOrDefault(l => l.FromExistingRegion && l.Nodes.Contains(endNode));
			if (existingLineSameNodes == null)
			{
				// There is no former polygon line connecting the start and end node of the edge (then there also is no edge between them),
				// so we create a new.
				PolygonLine line = new PolygonLine(startNode, endNode, addToExistingRegion);
				_polygonLines.Add(line);
				Edge edge = new Edge(line, leftInside, rightInside, _edgeType);
				EdgeOnLine.Connect(edge, line, 0, 1);

				// Look for intersections with the lines from the existing region, and split crossing edges in the common intersection point
				foreach (PolygonLine otherLine in _polygonLines.Where(l => l.FromExistingRegion))
				{
					Node intersection = Intersect(line, otherLine, out double parLine, out double parOtherLine);
					if (intersection != null)
					{
						line.SplitEdge(intersection, parLine, _edgeType);
						otherLine.SplitEdge(intersection, parOtherLine, _edgeType);
					}
				}
			}
			else
			{
				// There is a former polygon line connection the start and end node, update the topology edges for that line between the nodes
				// with insideness information of this line.
				double startParOnExistingLine = existingLineSameNodes.Parameter(startNode);
				double endParOnExistingLine = existingLineSameNodes.Parameter(endNode);
				bool existingAndAppliedLineHaveSameDirection = startParOnExistingLine <= endParOnExistingLine;
				double minParOnExistingLine = existingAndAppliedLineHaveSameDirection ? startParOnExistingLine : endParOnExistingLine;
				double maxParOnExistingLine = existingAndAppliedLineHaveSameDirection ? endParOnExistingLine : startParOnExistingLine;

				foreach (EdgeOnLine edgeOnExistingLine in existingLineSameNodes.EdgeOnLines.Where(eol => eol.StartLineParameter >= minParOnExistingLine && eol.EndLineParameter <= maxParOnExistingLine))
				{
					edgeOnExistingLine.Edge.SetInsideFlags(false, edgeOnExistingLine.GoesForward == existingAndAppliedLineHaveSameDirection, leftInside, rightInside);
				}
			}
		}

		/// <summary>
		/// Returns the intersection of two lines as a topology node, or null if they do not intersect
		/// </summary>
		/// <param name="line1">The first line tested for intersection</param>
		/// <param name="line2">The second line tested for intersection</param>
		/// <param name="parLine1">Set to the relative parameter position on the first line of the intersection (0 for the start node, 1 for the end node)</param>
		/// <param name="parLine2">Set to the relative parameter position on the second line of the intersection (0 for the start node, 1 for the end node)</param>
		private Node Intersect(PolygonLine line1, PolygonLine line2, out double parLine1, out double parLine2)
		{
			if (_edgeType == RegionEdgeType.UnitSphereShortestPaths)
			{
				Coordinate coordinate = UnitSphereGeometry.LineIntersection(line1.Start.Coordinate, line1.End.Coordinate, line2.Start.Coordinate, line2.End.Coordinate, out parLine1, out parLine2, SameCoordinateTolerance);
				if (coordinate != null)
				{
					Node node = GetNode(coordinate, null);
					node.UsedInExistingRegion = true;
					return node;
				}
			}
			else
			{
				GeoCoordinate coordinate = LongitudeLatitudeStraightLinesGeometry.LineIntersection(line1.Start.GeoCoordinate, line1.End.GeoCoordinate, line2.Start.GeoCoordinate, line2.End.GeoCoordinate, out parLine1, out parLine2, SameGeoCoordinateTolerance);
				if (coordinate != null)
				{
					Node node = GetNode(null, coordinate);
					node.UsedInExistingRegion = true;
					return node;
				}
			}

			return null;
		}

		/// <summary>
		/// Returns the topology node at the given coordinate.
		/// If there is no node at the given coordinate, a new node is created and marked as (so far) only used by the applied region, not the existing region.
		/// </summary>
		/// <param name="coordinate">The position of the node, given as a unit sphere point</param>
		/// <param name="geoCoordinate">The node's geocoordinate</param>
		private Node GetNode(Coordinate coordinate, GeoCoordinate geoCoordinate)
		{
			if (_edgeType == RegionEdgeType.UnitSphereShortestPaths)
			{
				Node node = _nodes.FirstOrDefault(n => n.Coordinate.DistanceTo(coordinate) <= SameCoordinateTolerance);
				if (node == null)
				{
					node = new Node(coordinate);
					_nodes.Add(node);
				}
				if (geoCoordinate != null && node.GeoCoordinate == null)
				{
					node.GeoCoordinate = geoCoordinate;
				}
				return node;
			}
			else
			{
				Node node = _nodes.FirstOrDefault(n => n.GeoCoordinate.DistanceTo(geoCoordinate) <= SameGeoCoordinateTolerance);
				if (node == null)
				{
					node = new Node(geoCoordinate);
					_nodes.Add(node);
				}
				return node;
			}
		}

		/// <summary>
		/// Sorts the topology edges for all the nodes according to their bearing from the node.
		/// </summary>
		private void SortEdges()
		{
			foreach (Node node in _nodes)
			{
				node.SortEdges();
			}
		}

		/// <summary>
		/// Sets the existing or applied region inside properties of the edges if they are missing.
		/// </summary>
		private void SetMissingEdgeInsides()
		{
			// Create entire list of starting nodes first, since the UsedInExistingRegion and UsedInAppliedRegion properties will change when we run the algorithm.
			List<Node> nodesUsedInBothRegions = _nodes.Where(node => node.UsedInExistingRegion && node.UsedInAppliedRegion).ToList();

			foreach (Node node in nodesUsedInBothRegions)
			{
				node.SetMissingEdgeInsidesBothRegions();
			}
		}

		/// <summary>
		/// Makes sure the topology graph is connected. Since both the part of the graph for the existing region, and the part of the graph for the applied region are connected,
		/// the only thing to do is to add an line between the two parts if they are disjoint.
		/// </summary>
		/// <param name="appliedRegion">The applied input region</param>
		private void EnsureExistingAndAppliedConnected(GeoRegion appliedRegion)
		{
			if (_nodes.Any(node => node.UsedInExistingRegion && node.UsedInAppliedRegion))
			{
				return;
			}

			// The topology graph parts for the existing region and applied region are disjoint, so we add a new line connecting them without intersecting the edges in the applied region
			if (_edgeType == RegionEdgeType.UnitSphereShortestPaths)
			{
				Node existingRegionNode = _nodes.First(node => node.UsedInExistingRegion);
				Coordinate existingRegionCoord = existingRegionNode.Coordinate;
				List<PolygonLine> appliedLines = _polygonLines.Where(line => !line.FromExistingRegion).ToList();

				foreach (Node appliedRegionNode in _nodes.Where(node => node.UsedInAppliedRegion))
				{
					Coordinate appliedRegionCoord = appliedRegionNode.Coordinate;
					if (appliedLines.All(line => !UnitSphereGeometry.LinesIntersectAndHaveDifferentEndPoints(appliedRegionCoord, existingRegionCoord, line.Start.Coordinate, line.End.Coordinate, SameCoordinateTolerance)))
					{
						bool newLineInside = appliedRegion.Contains(UnitSphereGeometry.UnitSpherePointToGeoCoordinate(existingRegionCoord));
						ApplyPolygonLine(appliedRegionNode, existingRegionNode, newLineInside, newLineInside);
						return;
					}
				}
			}
			else
			{
				Node existingRegionNode = _nodes.First(node => node.UsedInExistingRegion);
				GeoCoordinate existingRegionCoord = existingRegionNode.GeoCoordinate;
				List<PolygonLine> appliedLines = _polygonLines.Where(line => !line.FromExistingRegion).ToList();

				foreach (Node appliedRegionNode in _nodes.Where(node => node.UsedInAppliedRegion))
				{
					GeoCoordinate appliedRegionCoord = appliedRegionNode.GeoCoordinate;
					if (appliedLines.All(line => !LongitudeLatitudeStraightLinesGeometry.LinesIntersectAndHaveDifferentEndPoints(appliedRegionCoord, existingRegionCoord, line.Start.GeoCoordinate, line.End.GeoCoordinate, SameGeoCoordinateTolerance)))
					{
						bool newLineInside = appliedRegion.Contains(existingRegionCoord);
						ApplyPolygonLine(appliedRegionNode, existingRegionNode, newLineInside, newLineInside);
						return;
					}
				}
			}

			throw new Exception("Could not build connecting edge for existing and applied region");
		}
	}
}
