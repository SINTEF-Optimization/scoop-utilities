//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Sintef.Scoop.Utilities.GeoCoding
{
	/// <summary>
	/// A topological graph with spatial (geographical and geometrical properties)
	/// 
	/// The main contents of a graph are the following:
	///  - a collection of nodes
	///  - a collection of edges
	///  
	/// All coordinates that are used must be of the same coordinate class. This is the responsibility
	/// of the user.
	/// </summary>
	public class SpatialGraph
	{
		#region Public properties

		/// <summary>
		/// Id that can be used to identify the graph among a collection of graph objects.
		/// Not required.
		/// </summary>
		public string Id { get; }

		/// <summary>
		/// The coordinate system that we use in defining the graph members' geometrical properties.
		/// </summary>
		public CoordinateSystem CoordinateSystem { get; private set; }

		/// <summary>
		/// The collection of nodes
		/// </summary>
		public List<SpatialNode> Nodes { get; private set; }

		/// <summary>
		/// The collection of edges
		/// </summary>
		public List<SpatialEdge> Edges { get; private set; }

		/// <summary>
		/// Returns the extent of the graph based on the coordinates of the nodes.
		/// Only the coordinates with the smallest/largest lat/lon are returned.
		/// </summary>
		public IEnumerable<ICoordinate> Extent
		{
			get
			{
				List<ICoordinate> coordinates = Nodes.Where(n => n.Coordinate != null).Select(x => x.Coordinate).ToList();
				if (coordinates.Count == 0)
					yield break;
				yield return coordinates.MaxBy(x => x.Y);
				yield return coordinates.MinBy(x => x.Y);
				yield return coordinates.MaxBy(x => x.X);
				yield return coordinates.MinBy(x => x.X);
			}
		}

		#endregion

		/// <summary>
		/// Constructor.
		/// Creates an empty graph, and a default coordinate system centeret at Lat 60, Lon 10.
		/// </summary>
		/// <param name="id">Id that can be used to identify the graph among a collection of graph objects.
		/// Optional.</param>
		public SpatialGraph(string id = null)
		{
			Id = id;	
			Nodes = new List<SpatialNode>();
			Edges = new List<SpatialEdge>();
			CoordinateSystem = new CoordinateSystem(new GeoCoordinate(60, 10));
			//_coordinateType = coordinateType;
		}

		/// <summary>
		/// Constructor.
		/// Creates an empty graph
		/// </summary>
		/// <param name="id">Id that can be used to identify the graph among a collection of graph objects.
		/// Optional.</param>
		/// <param name="coordinateSystem">The coordinate system used. If null, a default system is constructed.</param>
		public SpatialGraph(CoordinateSystem coordinateSystem, string id = null) : this(id)
		{
			if (coordinateSystem != null)
				CoordinateSystem = coordinateSystem;
		}

		/// <summary>
		/// The base class implementation does nothing.
		/// </summary>
		public virtual void PostReadGraph()
		{
		}

		/// <summary>
		/// Adds a new node to the graph.
		/// </summary>
		/// <param name="coordinate">The new node's coordinate</param>
		/// <param name="id">The new node's external ID</param>
		/// <returns>The new node</returns>
		public SpatialNode AddNode(ICoordinate coordinate, string id = null)
		{
			//		Debug.Assert(coordinate.GetType() == _coordinateType, $"Received coordinate of type {coordinate.GetType()}, expected {_coordinateType}");

			SpatialNode node = new SpatialNode(this, id, coordinate)
			{
				Index = Nodes.Count
			};
			Nodes.Add(node);
			return node;
		}

		/// <summary>
		/// Check if the graph contains a node, and the node refers to the graph.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <returns>True if the graph contains a node, false otherwise.</returns>
		public bool HasNode(SpatialNode node)
		{
			// Input parameter check.
			Debug.Assert(node != null);

			return node.Graph == this && (node.Index <= Nodes.Count - 1
				&& Nodes[node.Index] == node);
		}

		/// <summary>
		/// Removes the given node from the graph.
		/// 
		/// If the removed node does not have the highest index in the graph,
		/// the node with the previously highest index inherits the removed node's index.
		/// 
		/// Throws an exception if the node does not belong to the graph, or
		/// if it has any arcs or stops connected.
		/// </summary>
		public virtual void RemoveNode(SpatialNode node)
		{

			if (!HasNode(node))
				throw new ArgumentException("Node does not belong to this graph", "node");
			if (node.AllEdges.Any())
				throw new InvalidOperationException("Node has one or more arcs connected");

			int maxIndex = Nodes.Count - 1;
			int nodeIndex = node.Index;

			SpatialNode moved = Nodes[maxIndex];
			Nodes[nodeIndex] = moved;
			moved.Index = nodeIndex;

			Nodes.RemoveAt(maxIndex);
		}

		/// <summary>
		/// Adds a new edge to the graph.
		/// </summary>
		/// <param name="from">The node where the edge starts</param>
		/// <param name="to">The node where the edge ends</param>
		/// <param name="id">The arc's external ID</param>
		/// <param name="coordinates">The coordinates of any geometrical points between <paramref name="from"/> and <paramref name="to"/> 
		/// (exclusive), in the order from node1 to node2.</param>
		/// <returns>The new arc</returns>
		public virtual SpatialEdge AddEdge(SpatialNode from, SpatialNode to, IEnumerable<ICoordinate> coordinates = null, string id = "x")
		{
			//	Debug.Assert(coordinates.NullOrEmpty() || coordinates.All(c => c.GetType() == _coordinateType), $"Received coordinates where at least one was of type different from the expected {_coordinateType}");

			if (!HasNode(from))
				throw new ArgumentException("Node does not belong to this graph", "from");
			if (!HasNode(to))
				throw new ArgumentException("Node does not belong to this graph", "to");

			coordinates ??= new ICoordinate[0];

			SpatialEdge edge = new SpatialEdge(from, to, id, coordinates)
			{
				Index = Edges.Count
			};
			Edges.Add(edge);

			from.AllEdges.Add(edge);
			to.AllEdges.Add(edge);

			return edge;
		}

		/// <summary>
		/// Returns the node that has the given <paramref name="id"/>, or null if no such 
		/// node has been defined.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public SpatialNode GetNode(string id) => Nodes.SingleOrDefault(n => n.Id == id);

		/// <summary>
		/// Returns the edge that has the given <paramref name="id"/>, or null if no such 
		/// edge has been defined.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public SpatialEdge GetEdge(string id) => Edges.SingleOrDefault(e => e.Id == id);

		/// <summary>
		/// Returns an edge that links the two given nodes.
		/// </summary>
		/// <returns>The edge, or null if no match was found.</returns>
		public SpatialEdge GetEdge(SpatialNode node1, SpatialNode node2) => node1.AllEdges.SingleOrDefault(e => e.GetOtherNode(node1) == node2);

		/// <summary>
		/// Removes the given edge from the graph.
		/// 
		/// If the removed edge does not have the highest index in the graph,
		/// the edge with the previously highest index inherits the removed edge's index.
		/// 
		/// Throws an exception if the edge does not belong to the graph.
		/// </summary>
		public void RemoveEdge(SpatialEdge edge)
		{
			int maxIndex = Edges.Count - 1;
			int edgeIndex = edge.Index;

			if (edgeIndex > maxIndex || Edges[edgeIndex] != edge)
				throw new ArgumentException("Edge does not belong to this graph", "edge");

			SpatialEdge moved = Edges[maxIndex];
			Edges[edgeIndex] = moved;
			moved.Index = edgeIndex;

			edge.From.AllEdges.Remove(edge);
			edge.To.AllEdges.Remove(edge);

			Edges.RemoveAt(maxIndex);
		}

		/// <summary>
		/// Returns the node in the graph that is closest to the given coordinate
		/// </summary>
		public SpatialNode ClosestNode(ICoordinate c)
		{
			SpatialNode closest = null;
			foreach (SpatialNode n in Nodes)
				if (closest == null || n.Coordinate.DistanceTo(c) < closest.Coordinate.DistanceTo(c))
					closest = n;
			return closest;
		}

		/// <summary>
		/// Returns the edge in the graph that is closest to the given coordinate, the closest point
		/// (i.e. the projection of the input coordinate on the closest edge, or an end point of the edge
		/// if the projection fals outside the edge.). 
		/// </summary>
		public (SpatialEdge edge, ICoordinate closestPoint) ClosestEdge(ICoordinate c)
		{
			var (edge, projectionResult) = ClosestEdge(Edges, c, 0);
			return (edge, projectionResult.ClosestPoint);
		}

		/// <summary>
		/// Returns the edge in the the given collection of <paramref name="edges"/> that is closest to the given coordinate, 
		/// the closest point (i.e. the projection of the input coordinate on that edge, or an end point of the edge
		/// if the projection falls outside the edge.), and the distance between that point and the input coordinate.
		/// </summary>
		/// <param name="edges"></param>
		/// <param name="c"></param>
		/// <param name="tolerance">A tolerance in meters, which is used to decide if borderline projections
		/// fall on, or outside, the segment (interpreted as inside if it is less than <paramref name="tolerance"/> 
		/// meters outside). Optional, the default value is zero.</param>
		/// <returns>The closest edge, and the projection result onto that edge.</returns>
		public static (SpatialEdge edge, ProjectionResult projectionResult) ClosestEdge(IEnumerable<SpatialEdge> edges, ICoordinate c, double tolerance)
		{
			SpatialEdge closestEdge = null;
			double closestDist = double.PositiveInfinity;
			ProjectionResult projResOnClosestEdge = null;
			foreach (SpatialEdge a in edges)
			{
				var projRes = a.ClosestPoint(c, tolerance);
				double dist = c.DistanceTo(projRes.ClosestPoint);
				if (closestEdge == null || dist < closestDist)
				{
					closestEdge = a;
					closestDist = dist;
					projResOnClosestEdge = projRes;
				}
			}
			return (closestEdge, projResOnClosestEdge);
		}

		/// <summary>
		/// Returns the coordinate on some edge that is closest to the given coordinate
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public ICoordinate ClosestPointOnAnyEdge(ICoordinate c) => ClosestEdge(c).closestPoint;

		/// <summary>
		/// Returns true if the line segment between two given coordinates intersects
		/// some arc in the graph
		/// </summary>
		/// <param name="c1">The start of the line segment</param>
		/// <param name="c2">The end of the line segment</param>
		/// <returns></returns>
		public bool IntersectsGraph(ICoordinate c1, ICoordinate c2)
		{
			foreach (SpatialEdge a in Edges)
			{
				if (ICoordinateExtensions.Intersects(c1, c2, a.From.Coordinate, a.To.Coordinate))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Merges another graph into this graph.
		/// </summary>
		/// <remarks>
		/// This operation removes all nodes and arcs from the other
		/// graph and adds them to this graph. No objects are created or deleted, just moved.
		/// 
		/// Indices for the nodes and arcs that are moved are shifted up by the number of
		/// nodes/arcs in this graph. Thus they are updated so that they form
		/// a contiguous range above the range of indices already present in this graph.
		/// </remarks>
		/// <param name="otherGraph"></param>
		public virtual void Merge(SpatialGraph otherGraph)
		{
			// Renumber indices
			int myNodeCount = Nodes.Count;
			int myArcCount = Edges.Count;

			foreach (var node in otherGraph.Nodes)
				node.Index += myNodeCount;
			foreach (var arc in otherGraph.Edges)
				arc.Index += myArcCount;

			// Transfer objects
			Nodes.AddRange(otherGraph.Nodes);
			Edges.AddRange(otherGraph.Edges);

			// Clean out other graph
			otherGraph.Nodes = new List<SpatialNode>();
			otherGraph.Edges = new List<SpatialEdge>();
		}

		/// <summary>
		/// Returns a random edge on the graph
		/// </summary>
		/// <returns></returns>
		public SpatialEdge GetRandomEdge()
		{
			Random rand = RandomCreator.GetRandomGenerator();
			return Edges[rand.Next(0, Edges.Count)];
		}
	}
}
