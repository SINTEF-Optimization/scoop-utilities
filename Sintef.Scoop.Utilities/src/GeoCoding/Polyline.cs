//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Sintef.Scoop.Utilities.GeoCoding
{
	/// <summary>
	/// A polyline, described by a sequence of ICoordinates.
	/// It is up to the user to make sure that all added coordinates are of the same type (e.g. all are 
	/// <see cref="GeoCoordinate"/>'s).
	/// </summary>
	public class Polyline
	{
		#region Public properties

		/// <summary>
		/// The coordinates of the poly line
		/// </summary>
		public IReadOnlyList<ICoordinate> Coordinates => _coordinates;

		/// <summary>
		/// The length of the polyline.
		/// </summary>
		public double Length => Coordinates.AdjacentPairs().Sum(pair => pair.Item1.DistanceTo(pair.Item2));

		/// <summary>
		/// The internal points of the poly line, that is all points excluding the first and the last. 
		/// </summary>
		public IEnumerable<ICoordinate> InternalPoints
		{
			get
			{
				for (int i = 1; i < Coordinates.Count - 1; i++)
				{
					yield return Coordinates[i];
				}
			}
		}

		#endregion

		#region Private data members

		private List<ICoordinate> _coordinates;
		
		#endregion

		#region Construction

		/// <summary>
		/// Constructor, taking a sequence of coordinates. More coordinates can be added later
		/// by using the <see cref="Add(ICoordinate)"/> function.
		/// </summary>
		/// <param name="coordinates">The coordinates.</param>
		public Polyline(IEnumerable<ICoordinate> coordinates)
		{
			_coordinates = coordinates.ToList();
		}

		/// <summary>
		/// Constructor, taking a single coordinate as starting point. More coordinates can be added later
		/// by using the <see cref="Add(ICoordinate)"/> function.
		/// </summary>
		/// <param name="coordinate">The coordinate.</param>
		public Polyline(ICoordinate coordinate)
		{
			_coordinates = new List<ICoordinate>();
			Add(coordinate);
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Adds the given coordinate to the end of the polyline
		/// </summary>
		/// <param name="coord"></param>
		public void Add(ICoordinate coord) => _coordinates.Add(coord);

		/// <summary>
		/// Adds the given coordinates to the end of the polyline
		/// </summary>
		/// <param name="coords"></param>
		public void Add(IEnumerable<ICoordinate> coords) => _coordinates.AddRange(coords);

		/// <summary>
		/// Returns the closest projection of the given coordinate onto any segment of the polyline. If there is no projection that falls
		/// within any segment, this is reflected
		/// in that the returned ProjectionResult has either OutsideBefore or OutsideAfter set to true.
		/// If the projection is outside before the first segment, and also outside after the last, then OutsideBefore = true != OutsideAfter 
		/// if the first segment is closer to the <paramref name="coordinate"/> than the last segment, and vice versa.
		/// 
		/// Note that this function is different from ClosestPoint, in that it gives the closest point among all
		/// projections (if there is a projection). ClosestPoint gives the closest point on the poly line, and information about 
		/// whether this is a projection onto the poly line. If there is no projection, the two functions returns the same.
		/// </summary>
		/// <param name="coordinate">The coordinate to find the closest point to</param>
		/// <param name="tolerance">A tolerance in meters, which is used to decide if borderline projections
		/// fall on, or outside, the segment (interpreted as inside if it is less than <paramref name="tolerance"/> 
		/// meters outside).</param>
		/// <returns>The projection of the given coordinate onto the polyline</returns>
		public ProjectionResult ClosestProjection(ICoordinate coordinate, double tolerance)
		{
			ICoordinate start = Coordinates.First();
			ICoordinate end = Coordinates.Last();
			ICoordinate closest = start;
			double distanceFromPolyline = double.MaxValue;
			double distanceAlongPolyline = 0;
			ICoordinate prev = closest;
			double distanceToPrev = 0;
			bool projectionFound = false;
			bool outsideBefore = false;
			bool outsideAfter = false;

			foreach (var point in Coordinates.Skip(1))
			{
				ProjectionResult projres = coordinate.ClosestProjection(prev, point, tolerance);
				if (projres.ProjectionOK)
				{
					projectionFound = true;
					outsideAfter = false;
					outsideBefore = false;
					double dist = projres.ClosestPoint.DistanceTo(coordinate);
					if (dist < distanceFromPolyline)
					{
						closest = projres.ClosestPoint;
						distanceFromPolyline = dist;
						distanceAlongPolyline = distanceToPrev + prev.DistanceTo(closest);
					}
				}
				else if (projres.OutsideBefore && !projectionFound)
					outsideBefore = true;
				else if (projres.OutsideAfter && !projectionFound)
					outsideAfter = true;

				distanceToPrev += prev.DistanceTo(point);
				prev = point;
			}

			if (outsideAfter && outsideBefore)
			{
				if (coordinate.DistanceTo(start) < coordinate.DistanceTo(end))
				{
					outsideAfter = false;
					closest = start;
					distanceAlongPolyline = 0;
				}
				else
				{
					outsideBefore = false;
					closest = end;
					distanceAlongPolyline = Length;
				}
			}

			return new ProjectionResult(closest, outsideBefore, outsideAfter, distanceAlongPolyline);
		}

		/// <summary>
		/// Returns the closest point on the polyline to the given coordinate. The projection is taken onto the closest
		/// line segment that contains the closest point. If the projection falls outside this line segment, this is reflected
		/// in that the returned ProjectionResult has eigher OutsideBefore or OutsideAfter set to true.
		/// </summary>
		/// <param name="coordinate">The coordinate to find the closest point to</param>
		/// <param name="tolerance">A tolerance in meters, which is used to decide if borderline projections
		/// fall on, or outside, the segment (interpreted as inside if it is less than <paramref name="tolerance"/> 
		/// meters outside).</param>
		/// <returns>The closest point on the polyline</returns>
		public ProjectionResult ClosestPoint(ICoordinate coordinate, double tolerance)
		{
			//TODO How is this function different from ClosestProjection?


			ICoordinate start = Coordinates.First();
			ICoordinate end = Coordinates.Last();
			ICoordinate closest = start;
			double length = Length;
			double distanceFromPolyline = double.MaxValue;
			double distanceAlongPolyline = 0;
			ICoordinate prev = closest;
			double distanceToPrev = 0;
			bool outsideBefore = false;
			bool outsideAfter = false;
			bool prevOutsideAfter = false;
			bool previousWasSelected = false;
			foreach (var point in Coordinates.Skip(1))
			{
				ProjectionResult projres = coordinate.ClosestProjection(prev, point, tolerance);

				//Check for outside turn where the previous was closest
				if (previousWasSelected && prevOutsideAfter && projres.OutsideBefore)
				{
					//The projection falls outside two subsequent edges, on an "outside turn" in the poly line"
					//We then still consider the projection to be on the geometry
					outsideAfter = false;
					outsideBefore = false;
				}

				double dist = projres.ClosestPoint.DistanceTo(coordinate);
				if (dist < distanceFromPolyline)
				{
					closest = projres.ClosestPoint;
					distanceFromPolyline = dist;
					distanceAlongPolyline = distanceToPrev + prev.DistanceTo(closest);

					//Check outside turn when this is closest
					if (prevOutsideAfter && projres.OutsideBefore)
					{
						//The projection falls outside two subsequent edges, on an "outside turn" in the poly line"
						//We then still consider the projection to be on the geometry
						outsideAfter = false;
						outsideBefore = false;
					}
					else
					{
						outsideAfter = projres.OutsideAfter;
						outsideBefore = projres.OutsideBefore;
					}
					previousWasSelected = true;
				}
				else
					previousWasSelected = false;

				//Moving on
				prevOutsideAfter = projres.OutsideAfter;
				distanceToPrev += prev.DistanceTo(point);
				prev = point;
			}

			if (distanceAlongPolyline == 0)
				closest = start;
			if (distanceAlongPolyline > length)
			{
				distanceAlongPolyline = length;
				closest = end;
			}
			return new ProjectionResult(closest, outsideBefore, outsideAfter, distanceAlongPolyline);
		}

		/// <summary>
		/// Returns the point that lies at the given distance along the polyline
		/// </summary>
		/// <param name="distanceAlongArc">The distance along the polyline, in meters</param>
		public ICoordinate PointAtDistance(double distanceAlongArc)
		{
			if (distanceAlongArc < 0)
			{
				throw new ArgumentException("distanceAlongArc cannot be less than zero");
			}

			double remainingDistance = distanceAlongArc;
			ICoordinate prev = null;

			foreach (var point in Coordinates)
			{
				if (prev != null)
				{
					double lengthOfSegment = prev.DistanceTo(point);
					
					if (remainingDistance <= lengthOfSegment)
					{
						double fractionAlongSegment = remainingDistance / lengthOfSegment;
						return prev.InterpolatedCoordinate(point, fractionAlongSegment, 1e-6);
					}
					
					remainingDistance -= lengthOfSegment;
				}

				prev = point;
			}

			if (distanceAlongArc > 0)
			{
				throw new ArgumentException("distanceAlongArc cannot be greater than Length"); 
			}

			return Coordinates.Last();
		}

		/// <summary>
		/// Returns the smallest distance along the Edge, corresponding to a cartesian coordinate and its distance to
		/// the nearest part of the Edge. Returnes the best match distance based on the distance between the coordinate and the edge.
		/// However, if a distance is found within the positionUncertainty, we return this without looking further.
		/// </summary>
		/// <param name="coordinateSystem">A coordinate system which can be used to convert GeoCoordinates to a 2d coordinate system</param>
		/// <param name="coordinate">The coordinate</param>
		/// <param name="positionUncertainty"></param>
		/// <param name="distanceToClosestPointOnSegment">On return, this is the distance to the closest point on the segment (from the given
		/// coordinate).</param>
		/// <param name="minDistance">Minimum distance to look for. Projections that fall on the segment before this are ignored.</param>
		/// <returns>Position in length along the item (in meters), or -1 if the coordinate's projection falls outside the segment 
		/// (or more outside than is permitted by the given uncertainty).</returns>
		public double DistanceToPoint(CoordinateSystem coordinateSystem, ICoordinate coordinate, double positionUncertainty, out double distanceToClosestPointOnSegment, double minDistance = 0)
		{
			if (coordinateSystem == null)
			{
				throw new ArgumentNullException(nameof(coordinateSystem));
			}

			if (coordinate == null)
			{
				throw new ArgumentNullException(nameof(coordinate));
			}

			double posOnSegment = 0;
			double bestDeltaPos = -1;
			bool someProjectionWasFound = false;
			distanceToClosestPointOnSegment = double.PositiveInfinity;
			IList<Coordinate> points = Coordinates.Select(c => coordinateSystem.GetCoordinate(c)).ToList();
			Coordinate coord = coordinateSystem.GetCoordinate(coordinate);
			for (int i = 0; i < points.Count - 1; i++)
			{
				Coordinate p = points[i];
				Coordinate np = points[i + 1];
				Coordinate projection = coord.ProjectionOnLineSegment(p, np, positionUncertainty);
				double del;
				double resultingPosOnSegment = posOnSegment;

				if (projection != null)
				{
					del = projection.DistanceTo(coord);
					resultingPosOnSegment += p.DistanceTo(projection);
					if (resultingPosOnSegment >= minDistance)
						someProjectionWasFound = true;
				}
				else
				{
					del = p.DistanceTo(coord);
				}

				if (someProjectionWasFound && del < distanceToClosestPointOnSegment)
				{
					distanceToClosestPointOnSegment = del;
					bestDeltaPos = resultingPosOnSegment;
					if (distanceToClosestPointOnSegment <= positionUncertainty)
						return bestDeltaPos;
				}

				//Continue searching. There may be more line segments that have a projection from coord.
				posOnSegment += p.DistanceTo(np);

				//Distance to last point...
				if (i == points.Count - 2)
				{
					del = np.DistanceTo(coord);
					if (del < distanceToClosestPointOnSegment)
					{
						distanceToClosestPointOnSegment = del;
						bestDeltaPos = Length;
					}
				}
			}

			return someProjectionWasFound ? bestDeltaPos : -1;
		}

		/// <summary>
		/// The IntermediatePoints that lies on the edge, and that are between the two given coordinates,
		/// both assumed to also lie on the edge, with <paramref name="c1"/> closer to Edge.From.
		/// It is also assumed that the distances along the Geometry of c1 and c2, respectively, are at least diff apart. This can be used
		/// e.g. to find the the intermediate points between the two positions associated with the intersection at a 
		/// loop in the geometry (or when the <see cref="SpatialEdge"/> as the same 
		/// start and end node).
		/// </summary>
		/// <param name="coordinateSystem">A coordinate system which can be used to convert GeoCoordinates to a 2d coordinate system</param>
		/// <param name="c1"></param>
		/// <param name="c2"></param>
		/// <param name="minDiff">Optional, if given it is taken as the minimum distance between the two distances along the Geometry. If the two distances
		/// are closer apart, the function throws an exception.</param>
		/// <param name="tolerance">The uncertainty in positions that we tolerate.</param>
		/// <returns></returns>
		public IEnumerable<ICoordinate> GetCoordinatesBetween(CoordinateSystem coordinateSystem, ICoordinate c1, ICoordinate c2, double tolerance = 0.5, double minDiff = 0)
		{
			double distToC1 = DistanceToPoint(coordinateSystem, c1, tolerance, out double distToEdge);
			double distToC2 = DistanceToPoint(coordinateSystem, c2, tolerance, out distToEdge, distToC1 + minDiff);
			return GetCoordinatesBetween(distToC1, distToC2);
		}


		/// <summary>
		/// The IntermediatePoints that lies on the geometry, and that are between or at the two given distances from the From node.
		/// Assumes d1 &lt; d2.
		/// </summary>
		/// <param name="d1"></param>
		/// <param name="d2"></param>
		/// <returns></returns>
		public IEnumerable<ICoordinate> GetCoordinatesBetween(double d1, double d2)
		{
			double dist = 0;
			ICoordinate prev = null;
			foreach (ICoordinate c in Coordinates)
			{
				if (prev != null)
				{
					dist += prev.DistanceTo(c);
					if (dist >= d1)
					{
						if (dist <= d2)
						{
							yield return c;
						}
						else
							yield break;
					}
				}
				prev = c;
			}
		}

		/// <summary>
		/// Returns true if the geometry intersects the edge in the XY plane
		/// </summary>
		/// <param name="coordinateSystem">Coordinate system used to map coordinates to a XY plane.</param>
		/// <param name="edge">The other edge</param>
		/// <returns></returns>
		public bool IntersectsXY(CoordinateSystem coordinateSystem, SpatialEdge edge)
		{
			return IntersectsXY(coordinateSystem, edge);
		}

		/// <summary>
		/// Returns the point where the geometry intersects the edge in the XY plane, or
		/// null if there is no intersection.
		/// Assumes the edges have only one intersecting point.
		/// </summary>
		/// <param name="coordinateSystem">Coordinate system used to map coordinates to a XY plane.</param>
		/// <param name="edge">The other edge</param>
		/// <param name="tolerance">Error tolerance in comparing positions (in meters). Optional.</param>
		/// <returns></returns>
		public Coordinate IntersectionXY(CoordinateSystem coordinateSystem, SpatialEdge edge, double tolerance = 0)
		{
			Coordinate prev = null;
			
			foreach (var curr in Coordinates)
			{
				var next = coordinateSystem.GetCoordinate(curr);
				if (prev != null)
				{
					var inter = edge.IntersectionXY(prev, next, tolerance);
					if (inter != null)
						return inter;
				}
				prev = next;
			}
			
			return null;
		}

		/// <summary>
		/// Returns the points where the geometry intersects the given edge in the XY plane, or
		/// null if there are no intersections.
		/// </summary>
		/// <param name="coordinateSystem">Coordinate system used to map coordinates to a XY plane.</param>
		/// <param name="edge">The other edge</param>
		/// <param name="tolerance">Error tolerance in comparing positions (in meters).
		/// Optional, if not given a very small tolerance
		/// is used only to avoid numerical errors.</param>
		/// <returns></returns>
		public IEnumerable<ICoordinate> IntersectionsXY(CoordinateSystem coordinateSystem, SpatialEdge edge, double tolerance = double.Epsilon)
		{
			Coordinate prev = null;
			Coordinate prevReturned = null;

			foreach (var curr in Coordinates)
			{
				var next = coordinateSystem.GetCoordinate(curr);
				if (prev != null)
				{
					var inter = edge.IntersectionXY(prev, next, tolerance);

					//If this point is the same as prev, then it has already been added
					if (inter != null && (prevReturned == null || inter.DistanceTo(prevReturned) > tolerance))
					{
						prevReturned = inter;
						yield return inter;
					}
				}

				prev = next;
			}
		}


		/// <summary>
		/// Returns the points where the geometry intersects itself in the XY plane, or null if there are no intersections. Note that starting and ending at the
		/// same place are not considered an intersection.
		///
		/// This method does not handle overlapping lines in the poly line and will throw an exception if that is encountered.
		///
		/// If any pair of endpoints (excluding the first and last in the polyline) are within the tolerance of each other, it is counted as intersection. If
		/// there are multiple intersections inside the tolerance of each other, the closest one to the polyline is returned.
		/// </summary>
		/// <param name="coordinateSystem">Coordinate system used to map coordinates to a XY plane.</param>
		/// <param name="tolerance">Distance measuring tolerance, in meters.</param>
		/// <returns>An enumeration of intersections, in carthesian coordinates.</returns>
		[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
		public IEnumerable<Coordinate> IntersectionsWithSelfXY(CoordinateSystem coordinateSystem, double tolerance)
		{
			int numberOfCoordinates = _coordinates.Count;
			Coordinate prevOuter = null, prevInner, first = null;

			List<Coordinate> internalIntersections = new();
			Dictionary<Coordinate, Projection> endPointIntersections = new();
			
			for (int i = 0; i < numberOfCoordinates - 2; ++i)
			{
				var currentOuter = coordinateSystem.GetCoordinate(_coordinates[i]);
				if (prevOuter != null)
				{
					//If prevInner is closer to the outer segment than the tolerance, we have
					//an Edge that doubles back on itself, which we cannot tolerate.
					prevInner = coordinateSystem.GetCoordinate(_coordinates[i + 1]);
					if (prevInner.GetProjectionCloserThan(prevOuter, currentOuter, tolerance) != null)
						throw new Exception($"An edge in the polyline is doubling back upon itself, within the tolerance {tolerance}");

					for (int j = i + 2; j < numberOfCoordinates; ++j)
					{
						var currentInner = coordinateSystem.GetCoordinate(_coordinates[j]);

						// First check for exact intersection
						var intersection = Coordinate.IntersectionInXYPlane(prevOuter, currentOuter, prevInner, currentInner);
						
						if (intersection != null)
						{
							// If at an endpoint, update the endpoint dictionary
							if (intersection.DistanceTo(currentInner) == 0)
							{
								var projection = endPointIntersections.ItemOrAdd(currentInner, () => new());
								projection.ProjectionPoint = currentInner;
								projection.Distance = 0;
								continue;
							}

							if (intersection.DistanceTo(prevInner) == 0)
							{
								var projection = endPointIntersections.ItemOrAdd(prevInner, () => new());
								projection.ProjectionPoint = prevInner;
								projection.Distance = 0;
								continue;
							}

							internalIntersections.Add(intersection);
							continue;
						}
						
						//There was no direct intersection, check for intersections within the tolerance
						intersection = currentInner.GetProjectionCloserThan(prevOuter, currentOuter, tolerance);

						// Return the intersection as long as it is not ending at the start of the polyline which by definition is not an intersection.
						if (intersection != null && !(j == numberOfCoordinates - 1 && intersection.DistanceTo(first) < tolerance))
						{

							var distanceCurrent = intersection.DistanceTo(currentInner); 
							if (distanceCurrent < tolerance)
							{
								var projection = endPointIntersections.ItemOrAdd(currentInner, () => new());
								if (distanceCurrent < projection.Distance)
								{
									projection.Distance = distanceCurrent;
									projection.ProjectionPoint = currentInner;
								}
							}
							var distancePrev = intersection.DistanceTo(prevInner);
							if (distancePrev < tolerance)
							{
								var projection = endPointIntersections.ItemOrAdd(prevInner, () => new());
								if (distancePrev < projection.Distance)
								{
									projection.Distance = distancePrev;
									projection.ProjectionPoint = prevInner;
								}
							}
						}

						prevInner = currentInner;
					}
				}
				prevOuter = currentOuter;
				first ??= currentOuter;
			}

			return internalIntersections.Union(endPointIntersections.Values.Select(c => c.ProjectionPoint));
		}

		/// <summary>
		/// Returns true if the geometry intersects the line between the two coordinates in the XY plane
		/// </summary>
		/// <returns></returns>
		public bool IntersectsXY(CoordinateSystem coordinateSystem, Coordinate c1, Coordinate c2)
		{
			return IntersectionXY(coordinateSystem, c1, c2) != null;
		}

		/// <summary>
		/// Returns the point where the geometry intersects the line between the two coordinates in the XY plane,
		/// or null if there is no intersection. Assumes there is at most one intersection.
		/// </summary>
		/// <param name="coordinateSystem"></param>
		/// <param name="c1"></param>
		/// <param name="c2"></param>
		/// <param name="tolerance">Error tolerance in comparing positions (in meters).
		/// Optional, by default, a tolerance of 0 is used.</param>
		/// <returns></returns>
		public Coordinate IntersectionXY(CoordinateSystem coordinateSystem, Coordinate c1, Coordinate c2, double tolerance = 0)
		{
			Coordinate prev = null;

			foreach (var curr in _coordinates)
			{
				Coordinate next = coordinateSystem.GetCoordinate(curr);
				if (prev != null)
				{
					var intersection = Coordinate.IntersectionInXYPlane(prev, next, c1, c2);

					//Even if there is no clear intersection, it may be that one of the coordinates is on
					//the line between the other pair of coordinates (within the tolerance).
					if (tolerance != 0)
					{
						intersection ??= prev.GetProjectionCloserThan(c1, c2, tolerance);
						intersection ??= next.GetProjectionCloserThan(c1, c2, tolerance);
						intersection ??= c1.GetProjectionCloserThan(prev, next, tolerance);
						intersection ??= c2.GetProjectionCloserThan(prev, next, tolerance);
					}

					if (intersection != null)
						return intersection;
				}

				prev = next;
			}

			return null;
		}

		/// <summary>
		/// Returns the points where the geometry intersects the line between the two coordinates in the XY plane,
		/// or null if there are no intersections. Each intersection will only be reported once, even if it intersects
		/// multiple lines in the polyline.
		/// </summary>
		//[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
		[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
		public IEnumerable<Coordinate> IntersectionsXY(CoordinateSystem coordinateSystem, Coordinate c1, Coordinate c2)
		{

			List<Coordinate> intersections = new();
			Coordinate prevCoordinate = null; //, prevIntersection = null;
			foreach (var curr in _coordinates)
			{
				var coordinate = coordinateSystem.GetCoordinate(curr);
				if (prevCoordinate != null)
				{
					var intersection = Coordinate.IntersectionInXYPlane(prevCoordinate, coordinate, c1, c2, 0);
					if (intersection != null)
					{
						if (!intersections.Any(x => x.X == intersection.X && x.Y == intersection.Y))
						{
							intersections.Add(intersection);
						}
					}
				}

				prevCoordinate = coordinate;
			}

			return intersections;
		}

		#endregion

		#region Private classes

		/// <summary>
		/// A class used to represent a projection point along with a projection distance.
		/// </summary>
		private class Projection
		{
			/// <summary>
			/// The projection distance.
			/// </summary>
			public double Distance { get; set; } = double.MaxValue;
			
			/// <summary>
			/// The projection point.
			/// </summary>
			public Coordinate ProjectionPoint { get; set; }
		}

		#endregion
	}
}
