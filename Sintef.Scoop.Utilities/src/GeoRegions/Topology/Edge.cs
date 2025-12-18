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
	/// An edge between two nodes in the topology graph of a region
	/// </summary>
	internal class Edge
	{
		#region Private classes

		/// <summary>
		/// Inside properties of an edge with respect to a specific region
		/// </summary>
		private class InsideFlags
		{
			/// <summary>
			/// Whether the lefthand side of the edge is inside the region
			/// </summary>
			internal bool LeftInside { get; set; }

			/// <summary>
			/// Whether the righthand side of the edge is inside the region
			/// </summary>
			internal bool RightInside { get; set; }

			/// <summary>
			/// Creates a pair of inside properties of an edge
			/// </summary>
			/// <param name="firstToLeft">Whether firstInside is for the lefthand (true) or righthand (false) side of the edge</param>
			/// <param name="firstInside">The inside property of the first side</param>
			/// <param name="secondInside">The inside property of the second side</param>
			internal InsideFlags(bool firstToLeft, bool firstInside, bool secondInside)
			{
				LeftInside = firstToLeft ? firstInside : secondInside;
				RightInside = firstToLeft ? secondInside : firstInside;
			}

			/// <summary>
			/// Creates a pair of inside properties of an edge, with the same inside property of both sides
			/// </summary>
			/// <param name="inside">The inside property of both sides of the edge</param>
			internal InsideFlags(bool inside)
			{
				LeftInside = inside;
				RightInside = inside;
			}

			/// <summary>
			/// Returns a clone of the inside properties of an edge, possibly null
			/// </summary>
			/// <param name="other">The inside properties to be cloned</param>
			internal static InsideFlags Clone(InsideFlags other)
			{
				return other == null ? null : new InsideFlags(true, other.LeftInside, other.RightInside);
			}

			/// <summary>
			/// Returns whether one ofthe sides of an edge are regarded as inside
			/// </summary>
			/// <param name="leftSide">Whether to return the lefthand (true) or righthand (false) side inside property</param>
			internal bool IsInside(bool leftSide)
			{
				return leftSide ? LeftInside : RightInside;
			}

			/// <summary>
			/// Changes the inside properties to the result of applying an operation type to this and another inside property set.
			/// stores the results in the existing region settings and removes the applied region settings.
			/// </summary>
			/// <param name="other">The other inside property set in the operation</param>
			/// <param name="operationType">The operation type to be applied</param>
			internal void ApplyOperation(InsideFlags other, RegionOperations.OperationType operationType)
			{
				LeftInside = ApplyOperation(LeftInside, other.LeftInside, operationType);
				RightInside = ApplyOperation(RightInside, other.RightInside, operationType);
			}

			/// <summary>
			/// Complements the inside properties.
			/// </summary>
			internal void Complement()
			{
				LeftInside = !LeftInside;
				RightInside = !RightInside;
			}

			/// <summary>
			/// Returns the result of applying an operation type to two inside settings
			/// </summary>
			/// <param name="first">The first inside</param>
			/// <param name="second">The second inside</param>
			/// <param name="operationType">The operation type to be applied</param>
			private static bool ApplyOperation(bool first, bool second, RegionOperations.OperationType operationType)
			{
				switch (operationType)
				{
					case RegionOperations.OperationType.Union:
						return first || second;
					case RegionOperations.OperationType.Intersection:
						return first && second;
					case RegionOperations.OperationType.FirstMinusSecond:
						return first && !second;
					case RegionOperations.OperationType.SecondMinusFirst:
						return !first && second;
					case RegionOperations.OperationType.SymmetricDifference:
						return first != second;

					default:
						throw new Exception($"Unknown operation type: {operationType}");
				}
			}
		}

		#endregion

		#region Private variables

		/// <summary>
		/// The lines this edge is part of, with paramter positions for this edge on the line.
		/// </summary>
		private List<EdgeOnLine> _edgeOnLines = new List<EdgeOnLine>();

		/// <summary>
		/// The inside properties of this edge for the existing region, or null if no inside properties are set.
		/// </summary>
		private InsideFlags _existingRegionInsideFlags;

		/// <summary>
		/// The inside properties of this edge for the applied region, or null if no inside properties are set.
		/// </summary>
		private InsideFlags _appliedRegionInsideFlags;

		/// <summary>
		/// The direction at the start node when moving along the edge from the start node.
		/// North is 0, East is 90, West is -90, South is 180 or -180.
		/// </summary>
		private double _bearingFromStart;

		/// <summary>
		/// The direction at the end node when moving along the edge from the end node.
		/// North is 0, East is 90, West is -90, South is 180 or -180.
		/// </summary>
		private double _bearingFromEnd;

		#endregion

		#region Internal properties

		/// <summary>
		/// The start node of this edge in the topology
		/// </summary>
		internal Node Start { get; private set; }

		/// <summary>
		/// The start node of this edge in the topology
		/// </summary>
		internal Node End { get; private set; }

		/// <summary>
		/// Whether the output region is currently being built, and this edge should be used in the polygons, but has not been used yet.
		/// </summary>
		internal bool PendingForOutputRegion { get; set; }

		/// <summary>
		/// Returns whether the lefthand side of the edge is inside for the existing region
		/// </summary>
		internal bool LeftInside => _existingRegionInsideFlags.LeftInside;

		/// <summary>
		/// Returns whether the righthand side of the edge is inside for the existing region
		/// </summary>
		internal bool RightInside => _existingRegionInsideFlags.RightInside;

		/// <summary>
		/// Whether this edge has set the inside properties for each side for the applied region
		/// </summary>
		internal bool UsedInAppliedRegion => _appliedRegionInsideFlags != null;

		/// <summary>
		/// Whether this edge has set the inside properties for each side for the existing region
		/// </summary>
		internal bool UsedInExistingRegion => _existingRegionInsideFlags != null;

		#endregion

		#region Constructor

		/// <summary>
		/// Creats an edge in the topology graph spanning over an entire polygon line or gluing line for the applied region
		/// </summary>
		/// <param name="line">The line to create the edge for</param>
		/// <param name="leftInside">Whether the lefthand side of the line is inside the applied region</param>
		/// <param name="rightInside">Whether the righthand side of the line is inside the applied region</param>
		/// <param name="edgeType">How the shapes of the edges in the regions are defined</param>
		internal Edge(PolygonLine line, bool leftInside, bool rightInside, RegionEdgeType edgeType)
		{
			SetStart(line.Start);
			SetEnd(line.End);
			InsideFlags insideFlags = new InsideFlags(true, leftInside, rightInside);
			_existingRegionInsideFlags = line.FromExistingRegion ? insideFlags : null;
			_appliedRegionInsideFlags = line.FromExistingRegion ? null : insideFlags;
			SetBearingsAndNodeUsedInRegions(edgeType);
		}

		/// <summary>
		/// Creates an edge in the topology graph, copying the inside properties from another source edge in the same direction
		/// </summary>
		/// <param name="start">The start node of the edge</param>
		/// <param name="end">The end node of the edge</param>
		/// <param name="source">The source edge to copy the inside properties from</param>
		/// <param name="edgeType">How the shapes of the edges in the regions are defined</param>
		internal Edge(Node start, Node end, Edge source, RegionEdgeType edgeType)
		{
			SetStart(start);
			SetEnd(end);
			_existingRegionInsideFlags = InsideFlags.Clone(source._existingRegionInsideFlags);
			_appliedRegionInsideFlags = InsideFlags.Clone(source._appliedRegionInsideFlags);
			SetBearingsAndNodeUsedInRegions(edgeType);
		}

		#endregion

		#region Internal methods

		/// <summary>
		/// Sets the edge inside properties of either the existing or applied region
		/// </summary>
		/// <param name="existingRegion">If true, set the inside properties for the existing region.
		/// If false, set the inside properties for the applied region.</param>
		/// <param name="firstToLeft">Whether firstInside is for the lefthand (true) or righthand (false) side of the edge</param>
		/// <param name="firstInside">The inside property of the first side</param>
		/// <param name="secondInside">The inside property of the second side</param>
		internal void SetInsideFlags(bool existingRegion, bool firstToLeft, bool firstInside, bool secondInside)
		{
			InsideFlags insideFlags = new InsideFlags(firstToLeft, firstInside, secondInside);
			if (existingRegion)
			{
				Start.UsedInExistingRegion = true;
				End.UsedInExistingRegion = true;
				if (_existingRegionInsideFlags == null)
				{
					_existingRegionInsideFlags = insideFlags;
				}
				else
				{
					throw new Exception("Inside edge properties for existing region were already set");
				}
			}
			else
			{
				Start.UsedInAppliedRegion = true;
				End.UsedInAppliedRegion = true;
				if (_appliedRegionInsideFlags == null)
				{
					_appliedRegionInsideFlags = insideFlags;
				}
				else
				{
					throw new Exception("Inside edge properties for applied region were already set");
				}
			}
		}

		/// <summary>
		/// Sets the edge inside properties of the edge from the inside properties of a given source edge.
		/// </summary>
		/// <param name="sameDirection">Whether this edge and the source edge have the same direction</param>
		/// <param name="source">The source edge to set the inside properties from</param>
		internal void SetInsideFlagsFromSourceEdge(bool sameDirection, Edge source)
		{
			if (source._existingRegionInsideFlags != null)
			{
				SetInsideFlags(true, sameDirection, source._existingRegionInsideFlags.LeftInside, source._existingRegionInsideFlags.RightInside);
			}
			if (source._appliedRegionInsideFlags != null)
			{
				SetInsideFlags(false, sameDirection, source._appliedRegionInsideFlags.LeftInside, source._appliedRegionInsideFlags.RightInside);
			}
		}

		/// <summary>
		/// Sets the inside properties for the existing or applied region to be the same on both sides
		/// </summary>
		/// <param name="existingRegion">Whether to set the inside properties for the existing region (true) or the applied region (false)</param>
		/// <param name="inside">The inside property of both sides of the edge</param>
		internal void SetBothSides(bool existingRegion, bool inside)
		{
			if (existingRegion)
			{
				_existingRegionInsideFlags = new InsideFlags(inside);
			}
			else
			{
				_appliedRegionInsideFlags = new InsideFlags(inside);
			}
		}

		/// <summary>
		/// Complements the inside properties for the existing region.
		/// </summary>
		internal void ComplementExisting()
		{
			_existingRegionInsideFlags.Complement();
		}

		/// <summary>
		/// Applies an operation type to the existing and applied region inside settings of the edge,
		/// stores the results in the existing region settings and removes the applied region settings.
		/// </summary>
		/// <param name="operationType">The operation type to be applied</param>
		internal void ApplyOperation(RegionOperations.OperationType operationType)
		{
			_existingRegionInsideFlags.ApplyOperation(_appliedRegionInsideFlags, operationType);
			_appliedRegionInsideFlags = null;
		}

		/// <summary>
		/// Returns whether a given side of the edge is marked as being inside when seen from a given node
		/// </summary>
		/// <param name="node">The node to see the edge from when testing the inside property</param>
		/// <param name="existingRegion">Whether to check the inside flag for the existing region (true) or the applied region (false)</param>
		/// <param name="toLeft">If true, return the inside property of the lefthand side of the edge when seen from the node.
		/// If false, return the inside property of the righthand side of the edge when seen from the node.</param>
		internal bool IsInsideFromNode(Node node, bool existingRegion, bool toLeft)
		{
			return GetInsideFlags(existingRegion)?.IsInside(IsStartNode(node) == toLeft) ?? false;
		}

		/// <summary>
		/// Adds paramter position information for this edge on a line.
		/// </summary>
		/// <param name="edgeOnLine">The edge position information to be added</param>
		internal void AddEdgeOnLine(EdgeOnLine edgeOnLine)
		{
			_edgeOnLines.Add(edgeOnLine);
		}

		/// <summary>
		/// Removes paramter position information for this edge on a line.
		/// </summary>
		/// <param name="edgeOnLine">The edge position information to be removed</param>
		internal void RemoveEdgeOnLine(EdgeOnLine edgeOnLine)
		{
			_edgeOnLines.Remove(edgeOnLine);
		}

		/// <summary>
		/// Splits this edge at a given position and given node, and updates the information for all connected lines.
		/// The existing edge is removed from the topology and replaced by two new edges. The position information of this edge
		/// for each line is replaced by a position information for each of the two new edges.
		/// </summary>
		/// <param name="intersection">The topology node for the splitting point</param>
		/// <param name="position">The relative parameter position of the splitting node on the edge, 0 is the start node, 1 is the end node</param>
		/// <param name="edgeType">How the shapes of the edges in the regions are defined</param>
		internal void Split(Node intersection, double position, RegionEdgeType edgeType)
		{
			List<EdgeOnLine> edgeOnLinesCopy = new List<EdgeOnLine>(_edgeOnLines);

			Start.RemoveEdge(this);
			End.RemoveEdge(this);

			foreach (EdgeOnLine edgeOnLine in edgeOnLinesCopy)
			{
				edgeOnLine.Disconnect();
			}

			Edge edge1 = Start.GetOrCreateEdge(intersection, this, edgeType);
			Edge edge2 = intersection.GetOrCreateEdge(End, this, edgeType);
			bool edge1Forward = edge1.Start == Start;
			bool edge2Forward = edge2.Start == intersection;

			foreach (EdgeOnLine oldEdgeOnLine in edgeOnLinesCopy)
			{
				double startPos = oldEdgeOnLine.EdgeStartParameter;
				double endPos = oldEdgeOnLine.EdgeEndParameter;
				double midPos = startPos + position * (endPos - startPos);
				if (edge1Forward)
				{
					EdgeOnLine.Connect(edge1, oldEdgeOnLine.Line, startPos, midPos);
				}
				else
				{
					EdgeOnLine.Connect(edge1, oldEdgeOnLine.Line, midPos, startPos);
				}
				if (edge2Forward)
				{
					EdgeOnLine.Connect(edge2, oldEdgeOnLine.Line, midPos, endPos);
				}
				else
				{
					EdgeOnLine.Connect(edge2, oldEdgeOnLine.Line, endPos, midPos);
				}
			}
		}

		/// <summary>
		/// Returns the bearing of the edge when going from a given end node
		/// </summary>
		/// <param name="node">THe start or end node of the edge to move from in the direction of the returned bearing</param>
		internal double BearingFromNode(Node node)
		{
			return IsStartNode(node) ? _bearingFromStart : _bearingFromEnd;
		}

		/// <summary>
		/// Returns the other start/end node of the edge than the given input node
		/// </summary>
		/// <param name="node">The opposite edge node of the returned node</param>
		internal Node OppositeNode(Node node)
		{
			return IsStartNode(node) ? End : Start;
		}

		/// <summary>
		/// Creates and returns the closed polygons for the existing region that have not been returned yet, starting at this edge.
		/// Marks all edges in the the returned polygons as having been returned.
		/// The method might return several polygons in case a node is visited several times.
		/// The method shoud only be called if the edge is not used in any returned polygon yet, and if the edge has different inside
		/// properties for its two sides.
		/// If the region has edge type UnitSphereShortestPaths, all returned polygons are positively oriented.
		/// If the region has edge type LatitudeLongitudeStraightLines, the orientation of the returned polygons is the orientation
		/// of the polygon in the latitude/longitude coordinate system.
		/// </summary>
		/// <param name="nmbEdges">The total number of edges in the graph, used to detect if we get an eternal loop error</param>
		/// <param name="edgeType">The edge type of the region</param>
		internal IEnumerable<ClosedGeoPolygon> GetPolygons(int nmbEdges, RegionEdgeType edgeType)
		{
			List<Node> nodes = new List<Node>();
			List<Edge> edges = new List<Edge>();

			Node currentNode = LeftInside ? Start : End;
			Edge currentEdge = this;

			int iterations = 0;
			while (true)
			{
				if (iterations > nmbEdges)
				{
					throw new Exception("Possibly in eternal loop when finding edges for polygons in region topology graph");
				}
				++iterations;

				int idx = nodes.IndexOf(currentNode);
				if (idx >= 0)
				{
					ClosedGeoPolygon polygon = MakePolygon(nodes.Skip(idx), edges.Skip(idx), edgeType);
					if (polygon != null)
					{
						yield return polygon;
					}

					nodes = nodes.Take(idx).ToList();
					edges = edges.Take(idx).ToList();
					if (currentEdge == this)
					{
						break;
					}
				}

				if (!currentEdge.PendingForOutputRegion)
				{
					throw new Exception("Edge is not final region edge");
				}
				currentEdge.PendingForOutputRegion = false;
				nodes.Add(currentNode);
				edges.Add(currentEdge);

				currentNode = currentEdge.OppositeNode(currentNode);
				int nmbEdgesAtNextNode = currentNode.Edges.Count();
				int iterationsAtNextNode = 0;
				do
				{
					if (iterationsAtNextNode > nmbEdges)
					{
						throw new Exception("Possibly in eternal loop when finding next polygon edge in region topology graph");
					}
					++iterationsAtNextNode;

					currentEdge = currentNode.EdgeToRight(currentEdge);
				}
				while (!currentEdge.IsInsideFromNode(currentNode, true, true) || currentEdge.IsInsideFromNode(currentNode, true, false));
			}

			if (nodes.Any() || edges.Any())
			{
				throw new Exception("Unexpected set of nodes and edges after building polygons");
			}
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Creates and returns a closed polygon from the given nodes and edges.
		/// If the region has edge type UnitSphereShortestPaths, the returned polygon is positively oriented.
		/// If the region has edge type LatitudeLongitudeStraightLines, the orientation of the returned polygon is the orientation
		/// of the polygon in the latitude/longitude coordinate system.
		/// </summary>
		/// <param name="nodes">The nodes, giving the corners in the polygon. Some of the nodes might be skipped as corners if the edge before and after belong to the same line.</param>
		/// <param name="edges">The edges, same numer as the nodes. Edge i goes from node i to node i+1.</param>
		/// <param name="edgeType">The edge type of the region</param>
		private static ClosedGeoPolygon MakePolygon(IEnumerable<Node> nodes, IEnumerable<Edge> edges, RegionEdgeType edgeType)
		{
			List<Node> nodesList = nodes.ToList();
			List<Edge> edgesList = edges.ToList();
			List<HashSet<PolygonLine>> edgeLines = edges.Select(edge => new HashSet<PolygonLine>(edge._edgeOnLines.Select(eol => eol.Line))).ToList();

			int offset = 0;
			int nmbEdges = edgesList.Count;
			int prevIdx = nmbEdges - 1;
			foreach (int idx in Enumerable.Range(0, nmbEdges))
			{
				if (!edgeLines[idx].Intersect(edgeLines[prevIdx]).Any())
				{
					offset = idx;
					break;
				}
				prevIdx = idx;
			}

			List<GeoCoordinate> corners = new List<GeoCoordinate>();
			HashSet<PolygonLine> linesFromPrevious = new HashSet<PolygonLine>();

			foreach (int idx in Enumerable.Range(offset, nmbEdges - offset).Concat(Enumerable.Range(0, offset)))
			{
				linesFromPrevious.IntersectWith(edgeLines[idx]);
				if (!linesFromPrevious.Any())
				{
					linesFromPrevious = new HashSet<PolygonLine>(edgeLines[idx]);
					GeoCoordinate newCorner = nodesList[idx].GeoCoordinate;
					if (newCorner == null && edgeType == RegionEdgeType.UnitSphereShortestPaths)
					{
						newCorner = UnitSphereGeometry.UnitSpherePointToGeoCoordinate(nodesList[idx].Coordinate);
					}
					corners.Add(newCorner);
				}
			}

			switch(edgeType)
			{
				case RegionEdgeType.UnitSphereShortestPaths:
					return ClosedGeoPolygon.UnitSphereShortestPathsPolygon(corners, true, ClosedGeoPolygon.BigAreaAction.Keep);
				case RegionEdgeType.LatitudeLongitudeStraightLines:
					return ClosedGeoPolygon.LatitudeLongitudeStraightLinesPolygon(corners);
				default:
					throw new InvalidOperationException($"Can not create polygon when edge type is {edgeType}");
			}
		}

		/// <summary>
		/// Returns true if the given node is the start node of the edge.
		/// Returns false if the given node is the end node of the edge.
		/// Throws an exception if the given node is neither the start nor the end node.
		/// </summary>
		/// <param name="node">The node</param>
		private bool IsStartNode(Node node)
		{
			if (node == Start)
			{
				return true;
			}
			else if (node == End)
			{
				return false;
			}
			else
			{
				throw new Exception("Node is not Start or End");
			}
		}

		/// <summary>
		/// Returns the inside settings for the existing region or the applied region
		/// </summary>
		/// <param name="existingRegion">If true, return the existing region settings. If false, return the applied region settings</param>
		private InsideFlags GetInsideFlags(bool existingRegion)
		{
			return existingRegion ? _existingRegionInsideFlags : _appliedRegionInsideFlags;
		}

		/// <summary>
		/// Sets the start node of the edge, and adds this edge to the list of edges for the start node.
		/// </summary>
		/// <param name="node">THe start node of the edge</param>
		private void SetStart(Node node)
		{
			Start = node;
			node.AddEdge(this);
		}

		/// <summary>
		/// Sets the end node of the edge, and adds this edge to the list of edges for the end node.
		/// </summary>
		/// <param name="node">THe end node of the edge</param>
		private void SetEnd(Node node)
		{
			End = node;
			node.AddEdge(this);
		}

		/// <summary>
		/// Calculates the bearings for the edge at the start and end node.
		/// Updates whether the start and end nodes are unsed in the existing and applied regions according to
		/// whether the inside properties are set for the two regions.
		/// </summary>
		/// <param name="edgeType">How the shapes of the edges in the regions are defined</param>
		private void SetBearingsAndNodeUsedInRegions(RegionEdgeType edgeType)
		{
			if (edgeType == RegionEdgeType.UnitSphereShortestPaths)
			{
				_bearingFromStart = UnitSphereGeometry.Bearing(Start.Coordinate, End.Coordinate);
				_bearingFromEnd = UnitSphereGeometry.Bearing(End.Coordinate, Start.Coordinate);
			}
			else
			{
				_bearingFromStart = GeoCoordinate.AzimutFromLongitudeLatitudeStraightLine(Start.GeoCoordinate, End.GeoCoordinate);
				_bearingFromEnd = GeoCoordinate.AzimutFromLongitudeLatitudeStraightLine(End.GeoCoordinate, Start.GeoCoordinate);
			}
			if (_existingRegionInsideFlags != null)
			{
				Start.UsedInExistingRegion = true;
				End.UsedInExistingRegion = true;
			}
			if (_appliedRegionInsideFlags != null)
			{
				Start.UsedInAppliedRegion = true;
				End.UsedInAppliedRegion = true;
			}
		}

		#endregion
	}
}
