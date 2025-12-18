//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Sintef.Scoop.Utilities.GeoCoding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities.GeoRegions.Topology
{
	/// <summary>
	/// A node in the region topology structure
	/// </summary>
	internal class Node
	{
		#region Private vaiables

		/// <summary>
		/// All polygon lines or gluing lines that are attached to this node via an Edge object
		/// </summary>
		private HashSet<PolygonLine> _lines = new HashSet<PolygonLine>();

		/// <summary>
		/// All the topology edges going from or to this node
		/// </summary>
		private List<Edge> _edges = new List<Edge>();

		#endregion

		#region Internal properties

		/// <summary>
		/// The coordinate of this node, given as a point on the unit sphere
		/// </summary>
		internal Coordinate Coordinate { get; }

		internal GeoCoordinate GeoCoordinate { get; set;  }

		/// <summary>
		/// Whether this node is used by any edges from the existing region
		/// </summary>
		internal bool UsedInExistingRegion { get; set; }

		/// <summary>
		/// Whether this node is used by any edges from the currently applied region
		/// </summary>
		internal bool UsedInAppliedRegion { get; set; }

		/// <summary>
		/// All polygon lines or gluing lines that are attached to this node via an Edge object
		/// </summary>
		internal IEnumerable<PolygonLine> Lines => _lines;

		/// <summary>
		/// All the topology edges going from or to this node
		/// </summary>
		internal IEnumerable<Edge> Edges => _edges.AsReadOnly();

		#endregion

		#region Constructor

		/// <summary>
		/// Creates a node in the region topology structure
		/// </summary>
		/// <param name="coordinate">The coordinate of the node, given as a point on the unit sphere</param>
		internal Node(Coordinate coordinate)
		{
			Coordinate = coordinate;
			GeoCoordinate = null;
			UsedInExistingRegion = false;
			UsedInAppliedRegion = false;
		}

		/// <summary>
		/// Creates a node in the region topology structure
		/// </summary>
		/// <param name="coordinate">The GeoCoordinate of the node</param>
		internal Node(GeoCoordinate coordinate)
		{
			Coordinate = null;
			GeoCoordinate = coordinate;
			UsedInExistingRegion = false;
			UsedInAppliedRegion = false;
		}

		#endregion

		#region Internal methods

		/// <summary>
		/// Adds a topology edge going to or from this node
		/// </summary>
		/// <param name="edge">The edge to be connected to the node</param>
		internal void AddEdge(Edge edge)
		{
			_edges.Add(edge);

		}

		/// <summary>
		/// Removes a topology edge from this node
		/// </summary>
		/// <param name="edge">The edge to be removed from the node</param>
		internal void RemoveEdge(Edge edge)
		{
			_edges.Remove(edge);
		}

		/// <summary>
		/// Adds a polygon line or gluing lines that is attached to this node via an Edge object
		/// </summary>
		/// <param name="line">The line to be added</param>
		internal void AddLine(PolygonLine line)
		{
			_lines.Add(line);
		}

		/// <summary>
		/// Returns an edge from this node to another node, creates it if it did not exist. Also updates or sets the inside properties
		/// of the edge according to a given source edge.
		/// </summary>
		/// <param name="other">The other node of the returned edge</param>
		/// <param name="source">The source edge used to set or update the inside properties of the returned edge</param>
		/// <param name="edgeType">How the shapes of the edges in the regions are defined</param>
		internal Edge GetOrCreateEdge(Node other, Edge source, RegionEdgeType edgeType)
		{
			Edge edge = _edges.FirstOrDefault(e => (e.Start == this & e.End == other) || (e.Start == other && e.End == this));
			if (edge != null)
			{
				edge.SetInsideFlagsFromSourceEdge(edge.Start == this, source);
				return edge;
			}
			else
			{
				return new Edge(this, other, source, edgeType);
			}
		}

		/// <summary>
		/// Sorts the topology edges from or to this node in positive (anti-clockwise) order when seen from the node.
		/// </summary>
		internal void SortEdges()
		{
			_edges = _edges.OrderBy(edge => -edge.BearingFromNode(this)).ToList();
		}

		/// <summary>
		/// Returns the edge to the left of the given input edge, when seen from the node.
		/// This requires that the edges have been sorted according to their bearing.
		/// </summary>
		/// <param name="edge">The edge</param>
		internal Edge EdgeToLeft(Edge edge)
		{
			return _edges[(IndexOfEdge(edge) + 1) % _edges.Count];
		}

		/// <summary>
		/// Returns the edge to the right of the given input edge, when seen from the node
		/// This requires that the edges have been sorted according to their bearing.
		/// </summary>
		/// <param name="edge">The edge</param>
		internal Edge EdgeToRight(Edge edge)
		{
			int idx = IndexOfEdge(edge) - 1;
			if (idx < 0)
			{
				idx += _edges.Count;
			}
			return _edges[idx];
		}

		/// <summary>
		/// Either sets the existing region inside properties for the edges that are only visited by the applied region,
		/// or sets the applied region inside properties for the edges that are only visited by the existed region.
		/// </summary>
		/// <param name="setForExistingRegion">If true, the nodes and edges to be handled are only used by the applied region, and miss the edge settings for the existing region.
		/// If false, the same rule applies for the opposite region.</param>
		/// <param name="inside">The inside property to be set for both sides in the edges handled</param>
		internal void SetMissingEdgeInsidesOneRegion(bool setForExistingRegion, bool inside)
		{
			if (setForExistingRegion)
			{
				UsedInExistingRegion = true;
			}
			else
			{
				UsedInAppliedRegion = true;
			}

			foreach (Edge edge in _edges.Where(edge => edge.UsedInExistingRegion == !setForExistingRegion && edge.UsedInAppliedRegion == setForExistingRegion))
			{
				edge.SetBothSides(setForExistingRegion, inside);
				Node node = edge.OppositeNode(this);
				if (node.UsedInExistingRegion == !setForExistingRegion && node.UsedInAppliedRegion == setForExistingRegion)
				{
					node.SetMissingEdgeInsidesOneRegion(setForExistingRegion, inside);
				}
			}
		}

		/// <summary>
		/// Sets the existing or applied region inside properties of the edges from this node if they are missing.
		/// This method is only called for a node that is used by both the existing and applied region.
		/// </summary>
		internal void SetMissingEdgeInsidesBothRegions()
		{
			Edge startEdge = _edges[0];

			Edge edgeUsedInRegion = startEdge;
			while (!edgeUsedInRegion.UsedInExistingRegion)
			{
				edgeUsedInRegion = EdgeToRight(edgeUsedInRegion);
				if (edgeUsedInRegion == startEdge)
				{
					throw new Exception("Node is used in existing region, but none of its edges are");
				}
			}
			bool insideExisting = edgeUsedInRegion.IsInsideFromNode(this, true, true);

			edgeUsedInRegion = startEdge;
			while (!edgeUsedInRegion.UsedInAppliedRegion)
			{
				edgeUsedInRegion = EdgeToRight(edgeUsedInRegion);
				if (edgeUsedInRegion == startEdge)
				{
					throw new Exception("Node is used in applied region, but none of its edges are");
				}
			}
			bool insideApplied = edgeUsedInRegion.IsInsideFromNode(this, false, true);

			// Main loop, running through all edges in positive orientation seen from the node
			foreach (Edge edge in _edges)
			{
				if (edge.UsedInExistingRegion)
				{
					insideExisting = edge.IsInsideFromNode(this, true, true);
				}
				if (edge.UsedInAppliedRegion)
				{
					insideApplied = edge.IsInsideFromNode(this, false, true);
				}

				if (!edge.UsedInExistingRegion)
				{
					edge.SetBothSides(true, insideExisting);
					Node node = edge.OppositeNode(this);
					if (!node.UsedInExistingRegion)
					{
						node.SetMissingEdgeInsidesOneRegion(true, insideExisting);
					}
				}
				if (!edge.UsedInAppliedRegion)
				{
					edge.SetBothSides(false, insideApplied);
					Node node = edge.OppositeNode(this);
					if (!node.UsedInAppliedRegion)
					{
						node.SetMissingEdgeInsidesOneRegion(false, insideApplied);
					}
				}
			}
		}

		/// <summary>
		/// Applies an operation type to the inside setting of the edges starting at this node,
		/// stores the results in the existing region settings and removes the applied region settings.
		/// </summary>
		/// <param name="operationType">The operation type to be applied</param>
		internal void ApplyOperation(RegionOperations.OperationType operationType)
		{
			UsedInExistingRegion = true;
			UsedInAppliedRegion = false;
			foreach (Edge edge in Edges.Where(edge => edge.Start == this))
			{
				edge.ApplyOperation(operationType);
			}
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Returns the index of an edge in the list of edges connected to the node
		/// </summary>
		/// <param name="edge">The edge to get the index for</param>
		private int IndexOfEdge(Edge edge)
		{
			int idx = _edges.IndexOf(edge);
			if (idx < 0)
			{
				throw new Exception("Given edge is not among edges at node");
			}
			return idx;
		}

		#endregion
	}
}
