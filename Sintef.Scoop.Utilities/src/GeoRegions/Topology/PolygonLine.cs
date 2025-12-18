//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities.GeoRegions.Topology
{
	/// <summary>
	/// A line from a region polygon or one of its gluing lines, with information on its edges and nodes in the topology.
	/// It might be split into several Edge objects on the topology graph of the region.
	/// </summary>
	internal class PolygonLine
	{
		/// <summary>
		/// The topology edges with parameter positions on the line.
		/// </summary>
		private List<EdgeOnLine> _edgeOnLines = new List<EdgeOnLine>();

		/// <summary>
		/// Whether this line comes from the existing region (true) or the applied region (false)
		/// </summary>
		internal bool FromExistingRegion { get; set; }

		/// <summary>
		/// The start topology node of the line
		/// </summary>
		internal Node Start { get; }

		/// <summary>
		/// The end node topology of the line
		/// </summary>
		internal Node End { get; }

		/// <summary>
		/// The topology edges with parameter positions on the line.
		/// </summary>
		internal IEnumerable<EdgeOnLine> EdgeOnLines => _edgeOnLines.AsReadOnly();

		/// <summary>
		/// All topology nodes visited by this line
		/// </summary>
		internal IEnumerable<Node> Nodes => EdgeOnLines.Select(eol => eol.StartNodeLineDirection).Concat(Enumerable.Repeat(End, 1));

		/// <summary>
		/// Creates a line from a region polygon or one of its gluing lines, with information on its edges and nodes in the topology.
		/// </summary>
		/// <param name="start">The start topology node of the line</param>
		/// <param name="end">The end topology node of the line</param>
		/// <param name="fromExistingRegion">Whether this line comes from the existing region (true) or the applied region (false)</param>
		internal PolygonLine(Node start, Node end, bool fromExistingRegion)
		{
			Start = start;
			End = end;
			FromExistingRegion = fromExistingRegion;
		}

		/// <summary>
		/// Adds a topology edge on the line with its with parameter positions
		/// </summary>
		/// <param name="edgeOnLine">The edge to be added</param>
		internal void AddEdgeOnLine(EdgeOnLine edgeOnLine)
		{
			_edgeOnLines.Add(edgeOnLine);
		}

		/// <summary>
		/// Removes information about a topology edge on the line
		/// </summary>
		/// <param name="edgeOnLine">The edge to be removed</param>
		internal void RemoveEdgeOnLine(EdgeOnLine edgeOnLine)
		{
			_edgeOnLines.Remove(edgeOnLine);
		}

		/// <summary>
		/// Returns the relative parameter position of a node on the line (0 for the start node, 1 for the end node).
		/// Throws an exception of the node is not on the line.
		/// </summary>
		/// <param name="node">The node to get the parameter position for</param>
		internal double Parameter(Node node)
		{
			if (node == End)
			{
				return 1;
			}
			foreach (EdgeOnLine edge in EdgeOnLines)
			{
				if (edge.StartNodeLineDirection == node)
				{
					return edge.StartLineParameter;
				}
			}

			throw new Exception("Parameter not found for node");
		}

		/// <summary>
		/// Splits a topology edge of the line at the given position, if that position is on the interior of the edge.
		/// </summary>
		/// <param name="intersection">The topology node for the splitting point</param>
		/// <param name="position">The parameter position on the line of the splitting point</param>
		/// <param name="edgeType">How the shapes of the edges in the regions are defined</param>
		internal void SplitEdge(Node intersection, double position, RegionEdgeType edgeType)
		{
			EdgeOnLine edgeOnLine = EdgeOnLines.FirstOrDefault(eol => eol.StartLineParameter <= position && eol.EndLineParameter >= position);

			if (edgeOnLine == null || edgeOnLine.Edge.Start == intersection || edgeOnLine.Edge.End == intersection)
			{
				return;
			}

			if (edgeOnLine.StartLineParameter == position || edgeOnLine.EndLineParameter == position)
			{
				throw new Exception("New intersection point was not at edge interior");
			}

			double edgePosition = (position - edgeOnLine.EdgeStartParameter) / (edgeOnLine.EdgeEndParameter - edgeOnLine.EdgeStartParameter);
			edgeOnLine.Edge.Split(intersection, edgePosition, edgeType);
		}
	}

}
