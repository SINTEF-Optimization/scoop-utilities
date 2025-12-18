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
	/// An non-directional edge in a spatial graph. 
	/// If StartNode == EndNode, and there are no geometry points, the length of the edge is zero.
	/// </summary>
	//[TypeConverter(typeof(GenericObjectConverter<SpatialEdge>))]
	public class SpatialEdge
	{
		#region Private members

		/// <summary>
		/// The geometrical properties of the graph.
		/// </summary>
		Polyline _geometry;

		#endregion

		#region Public properties

		/// <summary>
		/// The geometrical properties of the graph.
		/// </summary>
		public Polyline Geometry
		{
			get { return _geometry; }
			set
			{
				_geometry = value;
				UpdateLength();
			}
		}

		/// <summary>
		/// The graph that the segment belongs to.
		/// </summary>
		public SpatialGraph Graph { get; private set; }

		/// <summary>
		/// The coordinate system of the graph that the edge is in.
		/// </summary>
		internal CoordinateSystem CoordinateSystem => Graph.CoordinateSystem;


		/// <summary>
		/// The edge's ID. This member is for external reference, e.g. for route explanation
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// A unique index among edges in the graph. Edge indices for a graph
		/// start at 0 and are contiguous.
		/// 
		/// Edge indices are guaranteed to be constant as long as no nodes
		/// are removed from the graph. Edge indices do generally NOT
		/// coincide with indices in the Graph.Edges list.
		/// </summary>
		public int Index { get; internal set; }

		/// <summary>
		/// The first node of the edge (not that this does not indicate anything about direction of travel, it is just a name).
		/// </summary>
		public SpatialNode From { get; internal set; }

		/// <summary>
		/// The other node of the  edge (not that this does not indicate anything about direction of travel, it is just a name).
		/// </summary>
		public SpatialNode To { get; internal set; }

		/// <summary>
		/// The Zlevel of the edge.
		/// A value of zero means that the road is at terrain surface level, i.e., the default
		/// A negative value means that the road is below the terrain, e.g., a tunnel
		/// A positive value means that the road is above the terrain, e.g., a bridge
		/// </summary>
		public virtual int ZLevel { get; set; }

		/// <summary>
		/// The euclidian distance from <see cref="From"/> to <see cref="To"/>.
		/// </summary>
		public double StraightDistance { get { return From.Coordinate.DistanceTo(To.Coordinate); } }

		/// <summary>
		/// True if and only iff the edge starts or ends at the given node.
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		internal bool HasNode(SpatialNode node) => From.Id == node.Id || To.Id == node.Id;

		/// <summary>
		/// The length of the segment, along its geometry.
		/// </summary>
		public double Length { get; private set; }

		/// <summary>
		/// The average rate of climb along the geometrical distance of the edge.
		/// </summary>
		public double Incline
		{
			get
			{
				if (Length == 0)
					return 0;
				ICoordinate from = From.Coordinate;
				if (from == null || double.IsNaN(from.Z))
					return 0;
				ICoordinate to = To.Coordinate;
				if (to == null || double.IsNaN(to.Z))
					return 0;

				double incline = (to.Z - from.Z) / Length;
				return incline;
			}
		}

		/// <summary>
		/// Returns the node that is common for the  this edge and the given one,
		/// or null if they have no node in common.
		/// </summary>
		/// <param name="secondEdge"></param>
		/// <returns></returns>
		public SpatialNode CommonNodeWith(SpatialEdge secondEdge)
		{
			if (From == secondEdge.From || From == secondEdge.To)
				return From;
			else if (To == secondEdge.From || To == secondEdge.To)
				return To;
			else
				return null;
		}


		/// <summary>
		/// Returns true if the two edges do not have a common node and intersects in the same Z-level.
		/// </summary>
		/// <param name="edge">The other edge</param>
		/// <param name="tolerance">Error tolerance in comparing positions (in meters). Optional.</param>		
		/// <returns></returns>
		public bool Intersects(SpatialEdge edge, double tolerance)
		{
			return Intersection(edge, tolerance) != null;
		}

		/// <summary>
		/// All coordinates associated with the segment, in the order of <see cref="From"/> -> <see cref="To"/>, including the start and end points.
		/// </summary>
		public IEnumerable<ICoordinate> Coordinates
		{
			get
			{
				foreach (var p in Geometry.Coordinates)
				{
					yield return p;
				}
			}
		}

		/// <summary>
		/// The coordinate halfway along the edge.
		/// </summary>
		public ICoordinate MidPoint => Geometry.PointAtDistance(Length / 2.0);

		/// <summary>
		/// String representation of the edge
		/// </summary>
		public override string ToString()
		{
			return "[" + "edge" + " )" + Id + " from " + From.Coordinate + " to " + To.Coordinate + "]";
		}

		/// <summary>
		/// Split this edge in several other edges on the intersection with the given edge.
		/// For each found intersection, creates all resulting new edges and delete the old split ones from the graph.
		/// </summary>
		/// <param name="otherEdge"></param>
		/// <param name="distanceSlack">Uncertainty in position in input data.</param>
		/// <returns></returns>
		public IEnumerable<SpatialEdge> SplitOnIntersectionsWith(SpatialEdge otherEdge, double distanceSlack)
		{
			return SplitOnCoordinates(Intersections(otherEdge, distanceSlack), distanceSlack).edges;
		}

		/// <summary>
		/// Split this edge in several other edges on the intersection with the given edge.
		/// Adds the new edges and nodes to the graph, and removes remove the original one.
		/// </summary>
		/// <param name="splittingPoints"></param>
		/// <param name="distanceSlack">Uncertainty in position in input data.</param>
		/// <returns>The new edges and nodes that were created.</returns>
		public (IEnumerable<SpatialEdge> edges, IEnumerable<SpatialNode> nodes) SplitOnCoordinates(IEnumerable<ICoordinate> splittingPoints, double distanceSlack)
		{
			//First simple implementation
			List<SpatialEdge> newEdges = new List<SpatialEdge>();
			List<SpatialNode> newNodes = new List<SpatialNode>();

			if (!splittingPoints.NullOrEmpty())
			{
				SpatialNode prevNode = From;
				int counter = 0;
				IEnumerable<ICoordinate> intermediatepoints = Enumerable.Empty<ICoordinate>();
				int segmentCounter = 0;
				foreach (var c in splittingPoints)
				{
					intermediatepoints = Geometry.GetCoordinatesBetween(CoordinateSystem, prevNode.Coordinate, c, distanceSlack, distanceSlack);

					//Is there already a node here? If not, create a new one
					SpatialNode splitNode = Graph.ClosestNode(c);
					if (c.DistanceTo(splitNode.Coordinate) > distanceSlack)
					{
						splitNode = Graph.AddNode(c, $"Sp_{Id}_{++counter}");
						newNodes.Add(splitNode);
					}
					else
					{
						Debug.WriteLine($"Reusing node {splitNode.Id}");
						//if (splitNode == To || splitNode == From) //This is problematic when the end of a edge is splitting by being on one of its earlier segments
						//	continue;
					}

					if (prevNode != splitNode)
						newEdges.Add(Graph.AddEdge(prevNode, splitNode, intermediatepoints, $"{Id}_{segmentCounter++}"));
					prevNode = splitNode;
				}
				intermediatepoints = Geometry.GetCoordinatesBetween(CoordinateSystem, prevNode.Coordinate, To.Coordinate, distanceSlack, distanceSlack);

				//Trimming off any too close intermediate points
				int size = intermediatepoints.Count();
				if (size > 1 && intermediatepoints.First().DistanceTo(prevNode.Coordinate) < distanceSlack)
				{
					intermediatepoints = intermediatepoints.Skip(1);
					size--;
				}
				if (size > 1 && intermediatepoints.Last().DistanceTo(To.Coordinate) < distanceSlack)
					intermediatepoints = intermediatepoints.Take(size - 1);


				if (prevNode != To || intermediatepoints.Any())
					newEdges.Add(Graph.AddEdge(prevNode, To, intermediatepoints, $"{Id}_{segmentCounter}"));
			}

			if (newEdges.Any())
				Graph.RemoveEdge(this);
			return (newEdges, newNodes);
		}

		/// <summary>
		/// The orientation of the edge in the given direction, ignoring the edge's internal 
		/// geometry (considering it to be a straight line).
		/// </summary>
		/// <param name="dir"></param>
		/// <returns></returns>
		internal double StraightOrientation(EdgeTraversalDirection dir)
		{
			if (dir == EdgeTraversalDirection.Forward)
				return From.Coordinate.StraightLineDirectionTo(To.Coordinate);
			else
				return To.Coordinate.StraightLineDirectionTo(From.Coordinate);
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Creates an edge between the given nodes and with the given ID. 
		/// </summary>
		/// <param name="id">The edge's ID. This member is for external reference, e.g. for route explanation</param>
		/// <param name="node1">The first node of the edge (not that this does not indicate anything about direction of travel, it is just a name).</param>
		/// <param name="node2">The other node of the edge (not that this does not indicate anything about direction of travel, it is just a name).</param>
		/// <param name="coordinates">The coordinates of any geometrical points between <paramref name="node1"/> and <paramref name="node2"/> 
		/// (exclusive), in the order from node1 to node2.</param>
		public SpatialEdge(SpatialNode node1, SpatialNode node2, string id, IEnumerable<ICoordinate> coordinates)
		{
			if (!coordinates.NullOrEmpty())
			{
				if (coordinates.First().Equals(node1.Coordinate))
				{
					throw new ArgumentException("First internal edge coordinate is the same as the From node coordinate. These internal points should be used to give the geometry BETWEEN the node end points.");
				}
				if (coordinates.Last().Equals(node2.Coordinate))
				{
					throw new ArgumentException("Last internal edge coordinate is the same as the To node coordinate. These internal points should be used to give the geometry BETWEEN the node end points.");
				}
			}

			Graph = node1.Graph;
			From = node1;
			To = node2;
			Id = id;
			var points = coordinates.Prepend(node1.Coordinate).Append(node2.Coordinate);
			Geometry = new Polyline(points);
		}

		#endregion

		#region Public methods


		#region Geometry tools

		/// <summary>
		/// Returns the altitude at the given factor of length along the edge
		/// </summary>
		public double InterpolateAltitude(double fraction)
		{
			double a1 = From.Coordinate.Z;
			double a2 = To.Coordinate.Z;
			return a1 + fraction * (a2 - a1);
		}

		/// <summary>
		/// Returns the coordinate closest to the coordinate on the edge.
		/// </summary>
		/// <param name="coordinate"></param>
		/// <param name="tolerance">A tolerance in meters, which is used to decide if borderline projections
		/// fall on, or outside, the segment (interpreted as inside if it is less than <paramref name="tolerance"/> 
		/// meters outside). Optional, the default value is zero.</param>
		public ProjectionResult ClosestPoint(ICoordinate coordinate, double tolerance = 0) => Geometry.ClosestPoint(coordinate, tolerance);

		/// <summary>
		/// Get the other node, assuming the input node is either <see cref="From"/> or <see cref="To"/>.
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public SpatialNode GetOtherNode(SpatialNode node)
		{
			if (node == From)
				return To;
			else if (node == To)
				return From;
			else
				throw new Exception("SpatialEdge.GetOtherNode: the given node is not a start or end node.");
		}

		/// <summary>
		/// Returns the coordinate closest to the coordinate on the edge. 
		/// </summary>
		/// <param name="coordinate"></param>
		/// <param name="distFraction">Is set to the fraction along the edge where the closest coordinate is found</param>
		/// <param name="tolerance">A tolerance in meters, which is used to decide if borderline projections
		/// fall on, or outside, the segment (interpreted as inside if it is less than <paramref name="tolerance"/> 
		/// meters outside). Optional, the default value is zero.</param>
		public ProjectionResult GetClosestPoint(ICoordinate coordinate, out double distFraction, double tolerance = 0)
		{
			ProjectionResult result = Geometry.ClosestPoint(coordinate, tolerance);
			distFraction = result.DistanceAlong / Length;
			return result;
		}

		/// <summary>
		/// Finds the coordinate of the intersection between this edge and the given edge.
		/// Assumes there is only one such intersection.
		/// </summary>
		/// <param name="edge"></param>
		/// <param name="tolerance">Error tolerance in comparing positions (in meters). Optional.</param>		
		/// <returns></returns>
		internal ICoordinate Intersection(SpatialEdge edge, double tolerance)
		{
			if (ZLevel != edge.ZLevel || SharesNodeWith(edge))
				return default;

			//Calculations are done in cartesian coordinates, no matter the type of ICoordinate.
			Coordinate myStartCoordinate = CoordinateSystem.GetCoordinate(From.Coordinate);
			Coordinate otherStartCoordinate = CoordinateSystem.GetCoordinate(edge.From.Coordinate);

			if (myStartCoordinate.DistanceTo(otherStartCoordinate) > this.Length + edge.Length)
				// Arcs are too far apart to intersect
				return default;

			Coordinate result;
			if (Geometry == null)
			{
				Coordinate e = CoordinateSystem.GetCoordinate(To.Coordinate);
				result = edge.IntersectionXY(myStartCoordinate, e);
			}
			else
			{
				result = Geometry.IntersectionXY(CoordinateSystem, edge, tolerance);
			}

			if (result == null)
				return default;

			return (ICoordinate)CoordinateSystem.GetCoordinateOfType<ICoordinate>(result);
		}

		/// <summary>
		/// Finds the coordinates of the (possible several) intersections between this edge and the given edge.
		/// Note that if no geometry is given for this edge or the other, there will be only one intersection between their respective
		/// straight lines.
		/// Note that if the given edge is the edge itself, intersection is interpreted as that the edge crosses itself, or has an end node on its geometry (not if the
		/// end node is the same as the start node.
		/// </summary>
		/// <param name="edge"></param>
		/// <param name="tolerance">Error tolerance in comparing positions (in meters)</param>
		/// <returns></returns>
		internal IEnumerable<ICoordinate> Intersections(SpatialEdge edge, double tolerance)
		{
			if (ZLevel != edge.ZLevel)// || SharesNodeWith(edge))
				yield break;

			Coordinate myStartCoordinate = CoordinateSystem.GetCoordinate(From.Coordinate);
			Coordinate otherStartCoordinate = CoordinateSystem.GetCoordinate(edge.From.Coordinate);

			if (myStartCoordinate.DistanceTo(otherStartCoordinate) > this.Length + edge.Length)
				// Arcs are too far apart to intersect
				yield break;

			IEnumerable<ICoordinate> result;
			if (Geometry == null)
			{
				if (edge == this)
					result = Enumerable.Empty<ICoordinate>();
				else
				{
					Coordinate e = CoordinateSystem.GetCoordinate(To.Coordinate);
					result = edge.IntersectionsXY(myStartCoordinate, e);
				}
			}
			else
			{
				result = (edge == this) ? Geometry.IntersectionsWithSelfXY(CoordinateSystem, tolerance) : Geometry.IntersectionsXY(CoordinateSystem, edge, tolerance);
			}

			if (result == null)
				yield break;

			foreach (var c in result)
			{
				yield return CoordinateSystem.GetGeoCoordinate(c);
			}
		}

		/// <summary>
		/// Finds the carthesian coordinate of the intersection between this edge and a straigth line between the two given coordinates
		/// Assumes there is only one such intersection. If a non-zero tolerance is given, then it counts as an intersection even if only one
		/// of the two coordinates is closer to the edge than the tolerance, or if any of the point on the Edge is closer to
		/// the line between c1 and c2 than the tolerance.
		/// </summary>
		/// <param name="c1"></param>
		/// <param name="c2"></param>
		/// <param name="tolerance">Error tolerance in comparing positions (in meters). Optional.</param>
		/// <returns></returns>
		internal Coordinate IntersectionXY(Coordinate c1, Coordinate c2, double tolerance = 0)
		{
			if (Geometry == null)
			{
				Coordinate s = CoordinateSystem.GetCoordinate(From.Coordinate);
				Coordinate e = CoordinateSystem.GetCoordinate(To.Coordinate);
				Coordinate intersection = Coordinate.IntersectionInXYPlane(s, e, c1, c2);

				//If we have a non-zero tolerance, still check if any of the ned points are close to the other line segment.
				if (tolerance != 0)
				{
					intersection = intersection ?? s.GetProjectionCloserThan(c1, c2, tolerance);
					intersection = intersection ?? e.GetProjectionCloserThan(c1, c2, tolerance);
					intersection = intersection ?? c1.GetProjectionCloserThan(s, e, tolerance);
					intersection = intersection ?? c2.GetProjectionCloserThan(s, e, tolerance);
				}
				return intersection;
			}
			else
			{
				return Geometry.IntersectionXY(CoordinateSystem, c1, c2, tolerance);
			}
		}


		/// <summary>
		/// Finds the carthesian coordinates of the intersections between this edge and a straight line between the two given coordinates.
		/// </summary>
		/// <returns></returns>
		internal IEnumerable<Coordinate> IntersectionsXY(Coordinate c1, Coordinate c2)
		{
			if (Geometry == null)
			{
				Coordinate s = CoordinateSystem.GetCoordinate(From.Coordinate);
				Coordinate e = CoordinateSystem.GetCoordinate(To.Coordinate);
				Coordinate nextIntersection = Coordinate.IntersectionInXYPlane(s, e, c1, c2);

				yield return Coordinate.IntersectionInXYPlane(s, e, c1, c2);
			}
			else
			{
				foreach (var c in Geometry.IntersectionsXY(CoordinateSystem, c1, c2))
				{
					yield return c1;
				}
			}
		}


		/// <summary>
		/// The coordinate corresponding to a certain distance along the geometry of the edge
		/// (measured from <see cref="From"/>).
		/// </summary>
		/// <param name="distanceAlongEdge"></param>
		/// <returns></returns>
		public ICoordinate PositionAlongGeometry(double distanceAlongEdge)
		{
			if (Geometry == null)
			{
				double distanceFraction = distanceAlongEdge / Length;
				return From.Coordinate.InterpolatedCoordinate(To.Coordinate, distanceFraction, 1e-6);
			}
			else
				return Geometry.PointAtDistance(distanceAlongEdge);
		}

		/// <summary>
		/// The distance along the edge corresponding to a certain coordinate (measured from <see cref="From"/>).
		/// The coordinate is assumed to be on the geometry (or within a tolerance of 1E-6 meters).
		/// If(the projection of the coordinate onto the edge is behind the edge, the function return zero.
		/// If the projection is after the edge (the function returns the length of the edge).
		/// </summary>
		/// <param name="coordinate"></param>
		/// <returns>Distance, in meters.</returns>
		public double DistanceToPoint(ICoordinate coordinate)
		{
			Polyline geometry = Geometry ?? new Polyline(new[] { From.Coordinate, To.Coordinate }); //Using a straight line if no geometry is given.
			double dist = geometry.DistanceToPoint(CoordinateSystem, coordinate, 1e-6, out double closestPoint);
			if (dist < 0)
				return 0;
			else if (dist > Length)
				return Length;
			else
				return dist;
		}

		/// <summary>
		/// Orientation at the start of the edge, when traversing the edge.
		/// </summary>
		/// <param name="forward">If true, we assume we are traversing the edge from the start node to the end node, if false then vice versa.</param>
		/// <returns>Angle clockwise from North, in degrees.</returns>
		public double GetOrientationAtStart(bool forward)
		{
			ICoordinate c1 = From.Coordinate;
			ICoordinate c2 = Coordinates.Skip(1).First();
			return forward ? GetOrientation(c1, c2) : GetOrientation(c2, c1);
		}

		/// <summary>
		/// Orientation at the end of the edge, when traversing the edge.
		/// </summary>
		/// <param name="forward">If true, we assume we are traversing the edge from the start node to the end node, if false then vice versa.</param>
		/// <returns>Angle clockwise from North, in degrees.</returns>
		public double GetOrientationAtEnd(bool forward)
		{
			ICoordinate c1 = Coordinates.Skip(Coordinates.Count() - 2).First();
			ICoordinate c2 = To.Coordinate;
			return forward ? GetOrientation(c1, c2) : GetOrientation(c2, c1);
		}

		/// <summary>
		/// Returns the orientation standing in the <paramref name="fromCoordinate"/> and facing the <paramref name="toCoordinate"/>.
		/// Angle clockwise from North, in degrees.
		/// </summary>
		/// <param name="fromCoordinate"></param>
		/// <param name="toCoordinate"></param>
		/// <returns></returns>
		private static double GetOrientation(ICoordinate fromCoordinate, ICoordinate toCoordinate) => fromCoordinate.StraightLineDirectionTo(toCoordinate);

		#endregion


		#endregion

		#region Private functions

		/// <summary>
		/// Updates the cached length
		/// </summary>
		public void UpdateLength()
		{
			if (Geometry == null)
				Length = From.Coordinate.DistanceTo(To.Coordinate);
			else
				Length = Geometry.Length;
		}

		/// <summary>
		/// Thue if and only if this edge shares at least one node with the given edge.
		/// </summary>
		/// <param name="edge"></param>
		/// <returns></returns> 
		private bool SharesNodeWith(SpatialEdge edge)
		{
			return From == edge.From
				|| From == edge.To
				|| To == edge.From
				|| To == edge.To;
		}
		#endregion
	}

	/// <summary>
	/// A direction an edge is traversed (relative to its start-end definition).
	/// </summary>
	public enum EdgeTraversalDirection
	{
		/// <summary>
		/// Direction undefined (or not yet decided).
		/// </summary>
		Undefined,
		/// <summary>
		/// The edge is traversed from its <see cref="SpatialEdge.From"/> to <see cref="SpatialEdge.To"/>.
		/// </summary>
		Forward,
		/// <summary>
		/// The edge is traversed from its <see cref="SpatialEdge.To"/> to <see cref="SpatialEdge.From"/>.
		/// </summary>
		Reversed
	}

}
