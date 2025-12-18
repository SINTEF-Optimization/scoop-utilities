//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Sintef.Scoop.Utilities.GeoCoding
{

	/// <summary>
	/// A path in a Spatial graph, given as an alternating sequence of nodes and edges.
	/// Always starts and ends with a node, and is assumed to never visit a node more than once.
	/// As a special case, a path can contain only one node. 
	/// 
	/// </summary>
	public class SpatialPath
	{

		#region Public properties 


		/// <summary>
		/// The first node of the path
		/// </summary>
		/// <returns></returns>
		public SpatialNode StartNode => _nextEdge.First().Key;

		/// <summary>
		/// The last node of the path
		/// </summary>
		/// <returns></returns>
		public SpatialNode EndNode => GetNextNode(_nextEdge.Last().Key);

		/// <summary>
		/// The nodes, in the sequence of traversing the path
		/// </summary>
		public IEnumerable<SpatialNode> Nodes
		{
			get
			{
				SpatialNode lastNodeInKeyList = _nextEdge.Keys.Last();
				SpatialNode lastNode = _nextEdge[lastNodeInKeyList].GetOtherNode(lastNodeInKeyList);
				return _nextEdge.Keys.Concat(lastNode);
			}
		}

		/// <summary>
		/// Returns all coordinates along the path, from <see cref="StartNode"/> to <see cref="EndNode"/>.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ICoordinate> Coordinates
		{
			get
			{
				SpatialNode node = StartNode;
				yield return node.Coordinate;
				foreach (var edge in Edges)
				{
					//Forward or backwards?
					if (node == edge.From)
					{
						foreach (var coord in edge.Coordinates.Skip(1))
						{
							yield return coord;
						}
						node = edge.To;
					}
					else
					{
						Debug.Assert(node == edge.To);
						foreach (var coord in edge.Coordinates.Reverse().Skip(1))
						{
							yield return coord;
						}
						node = edge.From;
					}
				}
			}
		}

		/// <summary>
		/// The pairs of all coordinates, in the order of the path.
		/// </summary>
		public IEnumerable<(ICoordinate from, ICoordinate to)> CoordinatePairs
		{
			get
			{
				return Coordinates.Zip(Coordinates.Skip(1)).Select(tup => (tup.Item1, tup.Item2));
			}
		}

		/// <summary>
		/// The edges, in the sequence of traversing the path
		/// </summary>
		public IEnumerable<SpatialEdge> Edges => _nextEdge.Values;

		/// <summary>
		/// The edges of the path, in the order of traversal, along with the direction the edge is traversed
		/// relative to it's defined direction.
		/// </summary>
		public IEnumerable<(SpatialEdge edge, EdgeTraversalDirection direction)> EdgesWithDirection
		{
			get
			{
				SpatialNode n = StartNode;
				foreach (var edge in Edges)
				{
					if (n == edge.From)
						yield return (edge, EdgeTraversalDirection.Forward);
					else
						yield return (edge, EdgeTraversalDirection.Reversed);

					n = edge.GetOtherNode(n);
				}
			}
		}

		/// <summary>
		/// The length of the path, as the sum of the length of the edges
		/// </summary>
		public double Length => Edges.Sum(e => e.Length);

		/// <summary>
		/// The coordinate half way along the path.
		/// </summary>
		public ICoordinate MidPoint => PointAtDistance(Length / 2.0);

		/// <summary>
		/// Orientation at the start of the path (when starting to follow the path). 
		/// </summary>
		/// <returns>Angle clockwise from North, in degrees.</returns>
		public double OrientationAtStart => GetOrientationAtNode(StartNode);

		/// <summary>
		/// Orientation at the end of the path (when follow the path). 
		/// </summary>
		/// <returns>Angle clockwise from North, in degrees.</returns>
		public double OrientationAtEnd => GetOrientationAtNode(EndNode);


		#endregion

		#region Private data members

		/// <summary>
		/// The edge following the each node in the graph. If the path is a single point,
		/// this will have only one entry, with a null edge value.
		/// The node keys are given in the order or traversal along the path.
		/// </summary>
		private Dictionary<SpatialNode, SpatialEdge> _nextEdge;

		#endregion

		#region Construction

		/// <summary>
		/// Private default constructor.
		/// </summary>
		private SpatialPath()
		{
			_nextEdge = new Dictionary<SpatialNode, SpatialEdge>();
		}

		/// <summary>
		/// Constructor, based on a sequence of nodes. 
		/// </summary>
		/// <param name="graph">The graph that the path is in.</param>
		/// <param name="nodes">The sequence of nodes. It is assumed that each pair of nodes 
		/// are connected be an edge defined in the <paramref name="graph"/>.</param>
		public SpatialPath(SpatialGraph graph, IEnumerable<SpatialNode> nodes) : this()
		{
			if (nodes.CountIs(1))
				throw new ArgumentException("SpatialPath.ctor taking a sequence of nodes: For single element sequences, use the constructor taking a single node");
			else
				nodes.AdjacentPairs().Do(tup => _nextEdge[tup.Item1] = graph.GetEdge(tup.Item1, tup.Item2));
		}

		/// <summary>
		/// Constructs a single node path, of length zero.
		/// </summary>
		/// <param name="node"></param>
		public SpatialPath(SpatialNode node) : this()
		{
			_nextEdge[node] = new SpatialEdge(node, node, string.Empty, Enumerable.Empty<ICoordinate>());
		}

		/// <summary>
		/// Constructor, based on a sequence of edges. If there is assumed exactly one edge given,
		/// the order in which the nodes are stored is with the "From" node first. Otherwise, the ordering of nodes
		/// are deduced from how the edges are connected.
		/// Constructor, based on a sequence of edges. Assumes that the sequence has at least two edges, since this is necessary to
		/// deduce the direction in which each edge is traversed. To construct a SpatialPath from a single edge, use another constructor.
		/// </summary>
		/// <param name="edgesInOrderOfTraversal">The sequence of edges, given in the order of traversal along the path.</param>
		public SpatialPath(IEnumerable<SpatialEdge> edgesInOrderOfTraversal) : this()
		{
			if (edgesInOrderOfTraversal.CountIs(1))
				throw new ArgumentException("SpatialPath.ctor taking a sequence of edges: For single element sequences, use the constructor taking a single edge");
			BuildPath(edgesInOrderOfTraversal);
		}

		/// <summary>
		/// Constructor taking a single edge, and the direction in which this is to be traversed.
		/// </summary>
		public SpatialPath(SpatialEdge edge, EdgeTraversalDirection direction) : this()
		{
			SpatialNode start = direction == EdgeTraversalDirection.Forward ? edge.From : edge.To;
			_nextEdge[start] = edge;
		}

		/// <summary>
		/// Returns a clone of the path. Edge/Node objects are not cloned, only their references.
		/// </summary>
		/// <returns></returns>
		public SpatialPath Clone()
		{
			SpatialPath path = new SpatialPath();
			_nextEdge.Do(kvp => path._nextEdge.Add(kvp.Key, kvp.Value));
			return path;
		}

		/// <summary>
		/// Creates a new path that is the reverse of the this one.
		/// </summary>
		/// <returns></returns>
		public SpatialPath Reverse()
		{
			SpatialPath reversed = new SpatialPath();
			reversed.BuildPath(Edges.Reverse(), EndNode);
			return reversed;
		}

		#endregion

		#region Public methods


		/// <summary>
		/// Returns the orientation of the given edge, in the given direction, in degrees, clockwise from North.
		/// </summary>
		/// <param name="tup"></param>
		/// <returns></returns>
		public double OrientationOfEdge((SpatialEdge edge, EdgeTraversalDirection dir) tup) => tup.edge.StraightOrientation(tup.dir);

		/// <summary>
		/// Returns the orientation vector (the direction of travel) that corresponds to the given distance
		/// into the path.
		/// </summary>
		/// <param name="distance"></param>
		/// <returns>The orientation in degrees clockwise from North</returns>
		public double OrientationAtDistance(double distance)
		{
			if (distance <= 0)
				return OrientationOfEdge(EdgesWithDirection.First());
			else if (distance >= Length)
				return OrientationOfEdge(EdgesWithDirection.Last());
			else
			{
				double lengthSoFar = 0;
				foreach ((SpatialEdge edge, EdgeTraversalDirection dir) in EdgesWithDirection)
				{
					if (lengthSoFar + edge.Length > distance)
						return OrientationOfEdge((edge, dir));
					else
						lengthSoFar += edge.Length;
				}
				//Should not get here, but to please the compiler
				throw new Exception("should not get her");
			}
		}


		/// <summary>
		/// Orientation at the given node (when following the path). 
		/// </summary>
		/// <returns>Angle clockwise from North, in degrees.</returns>

		public double GetOrientationAtNode(SpatialNode node)
		{
			if (node == StartNode)
			{
				SpatialEdge firstedge = GetNextEdge(StartNode);
				if (StartNode == firstedge.From)
					return firstedge.GetOrientationAtStart(true);
				else
					return firstedge.GetOrientationAtEnd(false);
			}
			else if (node == EndNode)
			{
				SpatialEdge lastedge = Edges.Last();
				if (EndNode == lastedge.To)
					return lastedge.GetOrientationAtEnd(true);
				else
					return lastedge.GetOrientationAtStart(false);
			}
			else
			{
				//Node interior to the path.
				//Use the average orientations of the last segment of the previous edge and the first segment of the next edge
				var prevEdge = GetPreviousEdge(node);
				double orientOfPrev = (node == prevEdge.To) ? prevEdge.GetOrientationAtEnd(true) : prevEdge.GetOrientationAtStart(false);
				var nextEdge = GetNextEdge(node);
				double orientOfNext = (node == nextEdge.From) ? nextEdge.GetOrientationAtStart(true) : nextEdge.GetOrientationAtEnd(false);
				return GetAverageOrientation(orientOfPrev, orientOfNext);
			}
		}

		/// <summary>
		/// Returns the average between the two given orientation angles. Returns the angle
		/// with the smallest absolute value that represents the correct orientation.
		/// </summary>
		/// <param name="orientOfPrev"></param>
		/// <param name="orientOfNext"></param>
		/// <returns></returns>
		public static double GetAverageOrientation(double orientOfPrev, double orientOfNext)
		{
			double diff = orientOfNext - orientOfPrev;
			while (diff < -180)
			{
				diff += 360;
			}
			while (diff >= 180)
			{
				diff -= 360;
			}

			double angle = orientOfPrev + diff / 2;

			// This is correct, but we choose to return the angle with the smaller absolute value
			if (angle < -180)
				angle += 360;
			if (angle > 180)
				angle -= 360;

			return angle;
		}


		/// <summary>
		/// The node following the given node in the path, null if there are no more nodes.
		/// </summary>
		public SpatialNode GetNextNode(SpatialNode node) => _nextEdge.ItemOrDefault(node)?.GetOtherNode(node);

		/// <summary>
		/// The edge following the given node in the path.
		/// </summary>
		public SpatialEdge GetNextEdge(SpatialNode node) => _nextEdge.ItemOrDefault(node);

		/// <summary>
		/// The edge preceeding the given node in the path. Returns null if the given node is the start node.
		/// </summary>
		public SpatialEdge GetPreviousEdge(SpatialNode node) => node == StartNode ? null : Edges.Single(e => (e.From == node || e.To == node) && e != _nextEdge[node]);

		/// <summary>
		/// For the given <paramref name="coordinate"/>, returns the closest point
		/// (i.e. the projection of the input coordinate onto the closest edge, or an end point of that edge
		/// if the projection falls outside the edge.).
		/// </summary>
		/// <param name="coordinate"></param>
		/// <param name="tolerance">A tolerance in meters, which is used to decide if borderline projections
		/// fall on, or outside, the segment (interpreted as inside if it is less than <paramref name="tolerance"/> 
		/// meters outside). Optional, the default value is zero.</param>
		/// <returns>The closest edge, and the projection result onto that edge, and the distance to the edge.</returns>
		public ICoordinate GetClosestPoint(ICoordinate coordinate, double tolerance) 
			=> SpatialGraph.ClosestEdge(Edges, coordinate, tolerance).projectionResult.ClosestPoint;

		/// <summary>
		/// For the given <paramref name="coordinate"/>, returns the closest edge, along with the 
		/// projection of the input coordinate onto that edge.
		/// </summary>
		/// <param name="coordinate"></param>
		/// <param name="tolerance">A tolerance in meters, which is used to decide if borderline projections
		/// fall on, or outside, the segment (interpreted as inside if it is less than <paramref name="tolerance"/> 
		/// meters outside). Optional, the default value is zero.</param>
		/// <returns>The closest edge, and the projection result onto that edge, and the distance to the edge.</returns>
		public (SpatialEdge edge, ProjectionResult projectionResult) GetClosestEdge(ICoordinate coordinate, double tolerance)
			=> SpatialGraph.ClosestEdge(Edges, coordinate, tolerance);

		/// <summary>
		/// Gets the coordinate associated with the point at <paramref name="distance"/> along
		/// the edge. If distance > Length (&lt; 0), then the last (first) point is returned. 
		/// </summary>
		/// <param name="distance"></param>
		/// <returns></returns>
		public ICoordinate PointAtDistance(double distance)
		{
			if (distance <= 0)
				return _nextEdge.First().Key.Coordinate;
			else if (distance >= Length)
				return EndNode.Coordinate;
			else
			{
				double lengthSoFar = 0;
				foreach ((SpatialEdge edge, EdgeTraversalDirection dir) in EdgesWithDirection)
				{
					if (lengthSoFar + edge.Length > distance)
					{
						if (dir == EdgeTraversalDirection.Forward)
							return edge.Geometry.PointAtDistance(distance - lengthSoFar);
						else
							return edge.Geometry.PointAtDistance(edge.Length - (distance - lengthSoFar));
					}
					else
						lengthSoFar += edge.Length;
				}
				//Should not get here, but to please the compiler
				return EndNode.Coordinate;
			}
		}

		/// <summary>
		/// Returns the distance along the path to the given coordinate,
		/// assuming it lies on (or very close to) the path.
		/// </summary>
		/// <param name="coordinate"></param>
		/// <param name="tolerance">A tolerance in meters, which is used to decide if borderline projections
		/// fall on, or outside, the segment (interpreted as inside if it is less than <paramref name="tolerance"/> 
		/// meters outside). Optional, the default value is zero.</param>
		/// <returns>The projection result</returns>
		public ProjectionResult DistanceToPoint(ICoordinate coordinate, double tolerance)
		{
			//Instead of doing a lot of logic here, we create a GeoPolyLine, and then use the logic in Geometry.

			List<ICoordinate> coordinates = new List<ICoordinate>(), distinctCoordinates = new List<ICoordinate>();
			var (edge, direction) = EdgesWithDirection.First();
			coordinates.Add(direction == EdgeTraversalDirection.Forward ? edge.From.Coordinate : edge.To.Coordinate);

			foreach (var dirEdge in EdgesWithDirection)
			{
				if (dirEdge.direction == EdgeTraversalDirection.Forward)
				{
					coordinates.AddRange(dirEdge.edge.Geometry.Coordinates);
				}
				else
				{
					coordinates.AddRange(dirEdge.edge.Geometry.Coordinates.ToArray().Reverse());
				}
			}

			ICoordinate previous = null;
			foreach (var current in coordinates)
			{
				if (current != previous)
				{
					distinctCoordinates.Add(current);
				}
				previous = current;
			}
			
			Polyline pl = new Polyline(distinctCoordinates);
			ProjectionResult projectionResult = pl.ClosestPoint(coordinate, tolerance);
			return projectionResult;
		}

		/// <summary>
		/// Cecks if this path equal the <paramref name="other"/> topologically.
		/// I.e., goes through the same sequence of nodes.
		/// </summary>
		/// <param name="other"></param>
		public bool EqualsTopologically(SpatialPath other) => Nodes.SetEquals(other.Nodes);

		/// <summary>
		/// Overrides GetHashCode based on the sequence of node id's
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			var hashCode = -96829410;
			foreach (var n in Nodes)
			{
				hashCode = hashCode * -1521134295 + n.Id.GetHashCode();
			}
			return hashCode;
		}


		#endregion

		#region Private methods


		/// <summary>
		/// Builds the path based on the edges in the order given.
		/// </summary>
		/// <param name="edgesInOrderOfTraversal">The edges</param>
		/// <param name="startNode">If given, this must be one of the two end nodes of the first edge in <paramref name="edgesInOrderOfTraversal"/>.
		/// This will be the start node of the path. The need to supply this is only there if the list of edges contains 
		/// only one edge. In other cases, the start node can be deduced. If the startNode is still given, and no consistent with this
		/// deduction, an exception is thrown.</param>
		private void BuildPath(IEnumerable<SpatialEdge> edgesInOrderOfTraversal, SpatialNode startNode = null)
		{
			SpatialEdge firstEdge = edgesInOrderOfTraversal.First();
			SpatialNode currentNode = firstEdge.From;
			if (edgesInOrderOfTraversal.Skip(1).Any())
			{
				SpatialEdge secondEdge = edgesInOrderOfTraversal.Skip(1).First();
				SpatialNode commonNode = firstEdge.CommonNodeWith(secondEdge);
				if (commonNode == null)
					throw new ArgumentException($"Cannot construct path from edges that are not connected. The edges {firstEdge.Id} and {secondEdge.Id} have no common nodes");
				currentNode = firstEdge.GetOtherNode(commonNode);

				if (startNode != null && startNode != currentNode)
					throw new Exception("A start node was (un-necessarily) given together with a list of more than one edge, and not consistent with the list of edges.");
			}
			else if (startNode != null)
			{
				if (!firstEdge.HasNode(startNode))
					throw new Exception("A start node was given that is not a node on the single edge in the path");
				else
					currentNode = startNode;
			}

			List<SpatialNode> nodesInGivenOrder = new List<SpatialNode>() { currentNode };
			foreach (SpatialEdge e in edgesInOrderOfTraversal)
			{
				if (!e.HasNode(currentNode))
					throw new ArgumentException($"Cannot construct path from edges that are not connected. The edges {e.Id} does not start or end at node {currentNode.Id}");
				if (_nextEdge.ContainsKey(currentNode))
					throw new ArgumentException($"BuildPath called with a sequence of edges that visited node {currentNode} more than once. This is contrary to the assumptions of a SpatialPath.");
				_nextEdge[currentNode] = e;
				currentNode = e.GetOtherNode(currentNode); ;
			}
		}


		#endregion
	}
}
