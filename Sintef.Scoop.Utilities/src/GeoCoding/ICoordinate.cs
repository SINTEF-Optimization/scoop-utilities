//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Xml.Linq;

namespace Sintef.Scoop.Utilities.GeoCoding
{
	/// <summary>
	/// Common interface for coordinate classes.
	/// It is also assumed that coordinate classes have a parameter-less constructor.
	/// </summary>
	public interface ICoordinate : IEquatable<ICoordinate>
	{
		/// <summary>
		/// Right/east coordinate, in whatever measure is used by the coordinate class.
		/// </summary>
		double X { get; set;  }

		/// <summary>
		/// Up/North coordinate, in whatever measure is used by the coordinate class.
		/// </summary>
		double Y { get; set; }

		/// <summary>
		/// Vertical coordinate, in meters.
		/// </summary>
		double Z { get; set; }

		/// <summary>
		/// Returns a string description of the coordinate.
		/// </summary>
		string ToInvariantString(int decimals = 2);

		/// <summary>
		/// Returns the distance to the other coordinate, in meters.
		/// </summary>
		double DistanceTo(ICoordinate other);

		/// <summary>
		/// Test for approximate equality, within the given tolerance. Two coordinates are equal if they are (approximately) at the same position.
		/// I.e., the coordinate in each dimension is equal within the given tolerance (relative or absolute).
		/// </summary>
		/// <param name="other"></param>
		/// <param name="ignoreVertical">If true, then comparison is done only in the horizontal plane</param>
		/// <param name="maxTolerance"></param>
		bool EqualsWithTolerance(ICoordinate other, bool ignoreVertical, double maxTolerance);

		/// <summary>
		/// Returns the closest point to this coordinate on the segment 
		/// between the two endpoints (p1, p2).
		/// 
		/// </summary>
		/// <param name="p1">The start point of the segment</param>
		/// <param name="p2">The end point of the segment</param>
		/// <param name="tolerance">A tolerance in meters, which is used to decide if borderline projections
		/// fall on, or outside, the segment (interpreted as inside if it is less than <paramref name="tolerance"/> 
		/// meters outside).</param>
		ProjectionResult ClosestProjection(ICoordinate p1, ICoordinate p2, double tolerance);

		/// <summary>
		/// Returns the closest point to this coordinate on the segment between the two given coordinates.
		/// </summary>
		ICoordinate ClosestCoordinate(ICoordinate p1, ICoordinate p2);

		/// <summary>
		/// Returns an xml representation of this coordinate
		/// </summary>
		XElement ToXml(string elementName);


		/// <summary>
		/// Returns the direction to move the shortest distance from the present coordinate to the <paramref name="target"/> coordinate, in degrees from north.
		/// </summary>
		/// <param name="target">The coordinate moving towards when calculating the direction</param>
		double StraightLineDirectionTo(ICoordinate target);

		/// <summary>
		/// Returns the coordinate obtained by moving this coordiate
		/// the given fraction of the distance towards the
		/// other coordinate.
		/// </summary>
		/// <param name="other">The coordinate to move towards</param>
		/// <param name="fraction">The fraction of the distance to move</param>
		/// <param name="minAccuracy">The maximum error in the fraction moved</param>
		/// <returns></returns>
		ICoordinate InterpolatedCoordinate(ICoordinate other, double fraction, double minAccuracy);

		/// <summary>
		/// Returns a coordinate that is offset from this coordinate by a given distance in
		/// a given direction.
		/// </summary>
		/// <param name="distance">The distance to offset by, in meters</param>
		/// <param name="azimuth">The direction to offset in, as and angle in degrees wrt North/Up (positivy Y-direction). 
		/// North/Up is 0, west/left -90, east/right 90 and south/down 180/-180.</param>
		/// <returns></returns>
		ICoordinate CoordinateOffsetBy(double distance, double azimuth);

	}

	/// <summary>
	/// Extension methods for <see cref="ICoordinate"/>
	/// </summary>
	public static class ICoordinateExtensions
	{
		/// <summary>
		/// Creates a coordinate of the given generic type, and returns a
		///  ICoordinate reference to it.
		/// </summary>
		/// <typeparam name="C"></typeparam>
		/// <param name="x">East/right coordinate</param>
		/// <param name="y">North/up coordinate</param>
		/// <returns></returns>
		public static C CreateCoordinate<C>(double x, double y) where C :ICoordinate
		{
			C coord = Activator.CreateInstance<C>();
			coord.X = x;
			coord.Y = y;
			return coord;
		}

		/// <summary>
		/// Returns the intersection of two segments or null if they don't intersect.
		/// </summary>
		/// <param name="start1">The start coordinate of the first segment</param>
		/// <param name="end1">The end coordinate of the first segment</param>
		/// <param name="start2">The start coordinate of the second segment</param>
		/// <param name="end2">The end coordinate of the second segment</param>
		/// <param name="err">A tolerance needed due to floating-point arithmetic</param>
		public static C Intersection<C>(C start1, C end1, C start2, C end2, double err = double.Epsilon)
		{
			if (typeof(C) == typeof(GeoCoordinate))
				throw new NotImplementedException("Intersection not implemented for GeoCoordinates.");
				//return (C)(ICoordinate) GeoCoordinate.Intersects(start1 as GeoCoordinate, end1 as GeoCoordinate, start2 as GeoCoordinate, end2 as GeoCoordinate);
			else if (typeof(C) == typeof(Coordinate))
				return (C)(ICoordinate)Coordinate.IntersectionInXYPlane(start1 as Coordinate, end1 as Coordinate, start2 as Coordinate, end2 as Coordinate, err);
			else
				throw new NotImplementedException("Intersection:Unexpected coordinate type {typeof(C)}");
		}

		/// <summary>
		/// Returns true if the two segments intersect, false if not..
		/// </summary>
		/// <param name="start1">The start coordinate of the first segment</param>
		/// <param name="end1">The end coordinate of the first segment</param>
		/// <param name="start2">The start coordinate of the second segment</param>
		/// <param name="end2">The end coordinate of the second segment</param>
		public static bool Intersects<C>(C start1, C end1, C start2, C end2)
		{
			if (typeof(C) == typeof(GeoCoordinate))
			//	throw new NotImplementedException("Instersection not implemented for GeoCoordinates.");
				return GeoCoordinate.Intersects(start1 as GeoCoordinate, end1 as GeoCoordinate, start2 as GeoCoordinate, end2 as GeoCoordinate);
			else if (typeof(C) == typeof(Coordinate))
				return Coordinate.IntersectionInXYPlane(start1 as Coordinate, end1 as Coordinate, start2 as Coordinate, end2 as Coordinate) != null;
			else
				throw new NotImplementedException("Intersection:Unexpected coordinate type {typeof(C)}");
		}

		/// <summary>
		/// Test for approximate equality, within the given tolerance. Two coordinates are equal if they are (approximately) at the same position.
		/// I.e., the coordinate in each dimension is equal within the given tolerance (relative or absolute).
		/// </summary>
		/// <param name="myself"></param>
		/// <param name="other"></param>
		/// <param name="ignoreZ">If true, then comparison is done only in the horizontal-plane</param>
		/// <param name="maxTolerance"></param>
		public static bool EqualsWithTolerance(this ICoordinate myself, ICoordinate other, bool ignoreZ, double maxTolerance)
		{
			if (myself is GeoCoordinate geo)
				return geo.EqualsWithTolerance(other, ignoreZ, maxTolerance);
			else if (myself is Coordinate co)
				return co.EqualsWithTolerance(other, ignoreZ, maxTolerance);
			else
				throw new NotImplementedException("Intersection:Unexpected coordinate type {typeof(C)}");
		}
	}

	/// <summary>
	/// Small interface for coordinates to implement some functions with
	/// typed signatures (as in, type of coordinate, <typeparamref name="C"/>),
	/// for convenience (less casting) and easier backwards compatibility.
	/// </summary>
	/// <typeparam name="C"></typeparam>
	public interface ICoordinateBase<C> where C : ICoordinate
	{
		/// <summary>
		/// Returns the closest point to this coordinate on the segment between the two given coordinates.
		/// </summary>
		/// <returns>The coordinate, as a <typeparamref name="C"/> reference</returns>
		C ClosestPoint(C p1, C p2);

		/// <summary>
		/// Returns a coordinate that is offset from this coordinate by a given distance in
		/// a given direction.
		/// </summary>
		/// <param name="distance">The distance to offset by, in meters</param>
		/// <param name="azimuth">The direction to offset in, as and angle in degrees wrt North/Up (positivy Y-direction). 
		/// North/Up is 0, west/left -90, east/right 90 and south/down 180/-180.</param>
		/// <returns>The coordinate, as a <typeparamref name="C"/> reference.</returns>
		C OffsetBy(double distance, double azimuth);

		/// <summary>
		/// Returns the coordinate obtained by moving this coordiate
		/// the given fraction of the distance towards the
		/// other coordinate.
		/// </summary>
		/// <param name="other">The coordinate to move towards</param>
		/// <param name="fraction">The fraction of the distance to move</param>
		/// <param name="minAccuracy">The maximum error in the fraction moved</param>
		/// <returns>The coordinate, as a <typeparamref name="C"/> reference.</returns>
		C Interpolated(C other, double fraction, double minAccuracy);
	}

}