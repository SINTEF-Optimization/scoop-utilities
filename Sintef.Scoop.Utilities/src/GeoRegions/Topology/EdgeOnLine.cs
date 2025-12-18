//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

namespace Sintef.Scoop.Utilities.GeoRegions.Topology
{
	/// <summary>
	/// The position of a topology Edge on a PolygonLine
	/// </summary>
	internal class EdgeOnLine
	{
		/// <summary>
		/// The topology edge
		/// </summary>
		internal Edge Edge { get; }

		/// <summary>
		/// The line
		/// </summary>
		internal PolygonLine Line { get; }

		/// <summary>
		/// The relative position on the line for the Start node of the edge when going in the edge direction, i.e. the line position of Edge.Start.
		/// </summary>
		internal double EdgeStartParameter { get; }

		/// <summary>
		/// The relative position on the line for the End node of the edge when going in the edge direction, i.e. the line position of Edge.End.
		/// </summary>
		internal double EdgeEndParameter { get; }

		/// <summary>
		/// The relative position on the line for the start of the edge, when going in the same direction as the line.
		/// I.e. the position of the Start node of the edge if GoesForward = true,
		/// or the position of the End node of the edge if GoesForward = false.
		/// Smaller than EndParameter.
		/// </summary>
		internal double StartLineParameter => GoesForward ? EdgeStartParameter : EdgeEndParameter;

		/// <summary>
		/// The relative position on the line for the end of the edge, when going in the same direction as the line.
		/// I.e. the position of the End node of the edge if GoesForward = true,
		/// or the position of the Start node of the edge if GoesForward = false.
		/// Greater than StartParameter.
		/// </summary>
		internal double EndLineParameter => GoesForward ? EdgeEndParameter : EdgeStartParameter;

		/// <summary>
		/// Whether the edge and line go in the same direction
		/// </summary>
		internal bool GoesForward => EdgeStartParameter < EdgeEndParameter;

		/// <summary>
		/// The start node of the edge when going in the same direction as the line.
		/// I.e. the Start node of the edge if GoesForward = true,
		/// or the End node of the edge if GoesForward = false.
		/// </summary>
		internal Node StartNodeLineDirection => GoesForward ? Edge.Start : Edge.End;

		/// <summary>
		/// The end node of the edge when going in the same direction as the line.
		/// I.e. the End node of the edge if GoesForward = true,
		/// or the Start node of the edge if GoesForward = false.
		/// </summary>
		internal Node EndNodeLineDirection => GoesForward ? Edge.End : Edge.Start;

		/// <summary>
		/// Creates an object with information on the position of a topology Edge on a PolygonLine
		/// </summary>
		/// <param name="edge">The topology edge</param>
		/// <param name="line">The line</param>
		/// <param name="parEdgeStart">The parameter position on the line of edge.Start</param>
		/// <param name="parEdgeEnd">The parameter position on the line of edge.End</param>
		private EdgeOnLine(Edge edge, PolygonLine line, double parEdgeStart, double parEdgeEnd)
		{
			Edge = edge;
			Line = line;
			EdgeStartParameter = parEdgeStart;
			EdgeEndParameter = parEdgeEnd;
		}

		/// <summary>
		/// Connects a topology edge an a line by creating a RegionTopologyEdgeOnLine object and adds it to the the edge and line.
		/// Return the object with the connection information.
		/// </summary>
		/// <param name="edge">The topology edge</param>
		/// <param name="line">The line</param>
		/// <param name="parEdgeStart">The parameter position on the line of edge.Start</param>
		/// <param name="parEdgeEnd">The parameter position on the line of edge.End</param>
		internal static EdgeOnLine Connect(Edge edge, PolygonLine line, double parEdgeStart, double parEdgeEnd)
		{
			EdgeOnLine edgeOnLine = new EdgeOnLine(edge, line, parEdgeStart, parEdgeEnd);
			line.AddEdgeOnLine(edgeOnLine);
			edge.AddEdgeOnLine(edgeOnLine);
			edge.Start.AddLine(line);
			edge.End.AddLine(line);
			return edgeOnLine;
		}

		/// <summary>
		/// Disconnects this EdgeOnLine object from its topology edge and its line
		/// </summary>
		internal void Disconnect()
		{
			Edge.RemoveEdgeOnLine(this);
			Line.RemoveEdgeOnLine(this);
		}
	}
}
