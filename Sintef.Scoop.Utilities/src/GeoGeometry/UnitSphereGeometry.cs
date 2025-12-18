//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Sintef.Scoop.Utilities.GeoCoding;
using System;

namespace Sintef.Scoop.Utilities.GeoGeometry
{
	/// <summary>
	/// Utility functions for points on the unit sphere
	/// </summary>
	public static class UnitSphereGeometry
	{
		/// <summary>
		/// Returns the area of the unit sphere triangle with the three given points as corners if the points are in anti-clockwise order.
		/// Returns the negative of the area if the points are in clockwise order.
		/// The triangle is defined to be the smallest of the two regions the sphere is split into by the shortest unit sphere paths between the points.
		/// The method should be numerically stable if the three points are close compared to the sphere radius
		/// </summary>
		/// <param name="c1">The first triangle corner, expected to be on the unit sphere</param>
		/// <param name="c2">The second triangle corner, expected to be on the unit sphere</param>
		/// <param name="c3">The third triangle corner, expected to be on the unit sphere</param>
		public static double GetSignedArea(Coordinate c1, Coordinate c2, Coordinate c3)
		{
			if (c1.Equals(c2) || c1.Equals(c3) || c2.Equals(c3))
			{
				return 0;
			}
			double positivArea = SurfaceAngleMinusPlaneAngle(c1, c2, c3) + SurfaceAngleMinusPlaneAngle(c2, c1, c3) + SurfaceAngleMinusPlaneAngle(c3, c1, c2);
			if (ClosePointsDeterminant(c1, c2, c3) >= 0)
			{
				return positivArea;
			}
			else
			{
				return -positivArea;
			}
		}

		/// <summary>
		/// Returns the angle at a given point between two shortes unit sphere paths both starting at the point and ending in two given end points
		/// </summary>
		/// <param name="start">The common start point of the paths, and the point where the angle is measured, expected to be on the unit sphere</param>
		/// <param name="end1">The end point of the first path, expected to be on the unit sphere</param>
		/// <param name="end2">The end point of the second path, expected to be on the unit sphere</param>
		public static double SurfaceAngle(Coordinate start, Coordinate end1, Coordinate end2)
		{
			// The tangent vectors of the paths at the start point
			Coordinate tan1 = end1 - (end1 * start) * start;
			Coordinate tan2 = end2 - (end2 * start) * start;

			double cosAngle = tan1 * tan2 / (tan1.Length * tan2.Length);
			cosAngle = Math.Min(1.0, Math.Max(-1.0, cosAngle));
			return Math.Acos(cosAngle);
		}

		/// <summary>
		/// Returns the difference between the angle between the shortest unit sphere paths from a start coordinate to two end coordiantes,
		/// and the angle between the straight lines from the start coordinate to the end coordinates in the plane spanned by the three coordiantes.
		/// The method is written to be numerically stable when the three points are very close compared to the sphere radius.
		/// </summary>
		/// <param name="start">The common start point of the paths and linse, and the point where the angles are measured, expected to be on the unit sphere</param>
		/// <param name="end1">The end point of the first path and the first line, expected to be on the unit sphere</param>
		/// <param name="end2">The end point of the second path and the second line, expected to be on the unit sphere</param>
		private static double SurfaceAngleMinusPlaneAngle(Coordinate start, Coordinate end1, Coordinate end2)
		{
			// The direct lines from the start point to each end point
			Coordinate dirLine1 = end1 - start;
			Coordinate dirLine2 = end2 - start;
			double dir1_len = dirLine1.Length;
			double dir2_len = dirLine2.Length;

			// The tangent line when moving on the sphere from the start point to one of the end points
			Coordinate tang1 = end1 - (end1 * start) * start;
			Coordinate tang2 = end2 - (end2 * start) * start;
			double tang1_len = tang1.Length;
			double tang2_len = tang2.Length;
			double tang1_2 = tang1 * tang1;
			double tang2_2 = tang2 * tang2;

			// The ratio of the lengths of the direct lines and the tangent lines. This is very close to 1 for close input points.
			double dir_to_tan1 = dir1_len / tang1_len;
			double dir_to_tan2 = dir2_len / tang2_len;

			// The ratio between the length from an end point, to the end of the tanget line when starting in the start point,
			// and the length of the tangent line.
			// For the first end point, the denominator is the same as the length of tang1 - dirLine1 (which is parallell to the start vector),
			// but we use a more numerically stable way to calculate it.
			double r1 = tang1_len / (1 + Math.Sqrt(Math.Max(0, 1 - tang1_2)));
			double r2 = tang2_len / (1 + Math.Sqrt(Math.Max(0, 1 - tang2_2)));
			double r1_2 = r1 * r1;
			double r2_2 = r2 * r2;

			// Cosine and sine for the unit sphere surface angle and the triangle angle. The difference between the two angles is what should be returned from this function,
			// but a direct calculation of the difference is numerically unstable for close input points.
			double cos_surface = tang1 * tang2 / (tang1_len * tang2_len);
			double sin_surface = tang1.CrossProduct(tang2).Length / (tang1_len * tang2_len);
			double cos_triangle = dirLine1 * dirLine2 / (dir1_len * dir2_len);
			double sin_triangle = dirLine1.CrossProduct(dirLine2).Length / (dir1_len * dir2_len);

			// Calculate the difference of the angles via the sine of the angle
			double cos_surf_2 = cos_surface * cos_surface;
			double sin_diff_numerator = 2 * r1 * r2 * cos_surface + (1 - cos_surf_2) * r1_2 * r2_2 - cos_surf_2 * (r1_2 + r2_2);
			double sin_diff_denominator = dir_to_tan1 * dir_to_tan1 * dir_to_tan2 * dir_to_tan2 * (sin_surface * cos_triangle + sin_triangle * cos_surface);
			double sin_diff = sin_diff_numerator / sin_diff_denominator;

			return Math.Asin(Math.Min(1, Math.Max(-1, sin_diff)));
		}

		/// <summary>
		/// Returns whether two unit sphere shortest paths intersect in a common point.
		/// The method should be numerically stable if the points are very close compared to the sphere radius.
		/// </summary>
		/// <param name="from1">The start point of the first path, expected to be on the unit sphere</param>
		/// <param name="to1">The end point of the first path, expected to be on the unit sphere</param>
		/// <param name="from2">The start point of the second path, expected to be on the unit sphere</param>
		/// <param name="to2">The end point of the second path, expected to be on the unit sphere</param>
		public static bool LinesIntersect(Coordinate from1, Coordinate to1, Coordinate from2, Coordinate to2)
		{
			return LineIntersection(from1, to1, from2, to2, out double _, out double _) != null;
		}

		/// <summary>
		/// Returns whether two segments on the Earth intersect.
		/// </summary>
		/// <param name="from1">The start of the first segment</param>
		/// <param name="to1">The end of the first segment</param>
		/// <param name="from2">The start of the second segment</param>
		/// <param name="to2">The end of the second segment</param>
		public static bool LinesIntersect(GeoCoordinate from1, GeoCoordinate to1, GeoCoordinate from2, GeoCoordinate to2)
		{
			return LinesIntersect(from1.AsUnitSpherePoint(), to1.AsUnitSpherePoint(), from2.AsUnitSpherePoint(), to2.AsUnitSpherePoint());
		}

		/// <summary>
		/// Returns the intersection point of two unit sphere shortest paths, if they intersect.
		/// Returns null if they do not intersect.
		/// The method should be numerically stable if the points are very close compared to the sphere radius.
		/// </summary>
		/// <param name="from1">The start point of the first path, expected to be on the unit sphere</param>
		/// <param name="to1">The end point of the first path, expected to be on the unit sphere</param>
		/// <param name="from2">The start point of the second path, expected to be on the unit sphere</param>
		/// <param name="to2">The end point of the second path, expected to be on the unit sphere</param>
		/// <param name="pos1">Set to be the relative position from the start point on the first path of the intersection point, it will be in the range 0 (intersection is start point) to 1 (intersection is end point)</param>
		/// <param name="pos2">Set to be the relative position from the start point on the second path of the intersection point, it will be in the range 0 (intersection is start point) to 1 (intersection is end point)</param>
		/// <param name="tolerance">An tolerance on how close an intersection point must be to one of the end points on the paths, to be treated as being the same point</param>
		public static Coordinate LineIntersection(Coordinate from1, Coordinate to1, Coordinate from2, Coordinate to2, out double pos1, out double pos2, double tolerance = 0)
		{
			pos1 = 0;
			pos2 = 0;

			Coordinate norm1 = from1.CrossProduct(to1);
			Coordinate norm2 = from2.CrossProduct(to2);
			Coordinate intersectionLine = norm1.CrossProduct(norm2);
			if (intersectionLine.Length == 0)
			{
				// Special case when line segments are either undefined or on the same line
				return null;
			}

			// Get coefficients applied to from1 and to1 to sum up to intersectionLine,
			// and coefficients applied to from2 and to2 to sum up to intersectionLine.
			double coefFrom1 = ClosePointsDeterminant(to1, to2, from2);
			double coefTo1 = ClosePointsDeterminant(from1, from2, to2);
			double coefFrom2 = ClosePointsDeterminant(from1, to1, to2);
			double coefTo2 = ClosePointsDeterminant(to1, from1, from2);

			if (coefFrom1 + coefTo1 < 0)
			{
				// If there really is an intersection point, it is found in the opposite direction of intersectionLine, use those coefficients instead
				coefFrom1 = -coefFrom1;
				coefFrom2 = -coefFrom2;
				coefTo1 = -coefTo1;
				coefTo2 = -coefTo2;
			}
			else if (coefFrom1 + coefTo1 == 0)
			{
				return null;
			}

			// The intersection point of the prolonged lines, might be outside the paths
			Coordinate calculatedIntersection = coefFrom1 * from1 + coefTo1 * to1;
			calculatedIntersection /= calculatedIntersection.Length;

			// The intersection point to be returned, could be replaced by one of the path end points if they are closer than the tolerance
			Coordinate actualIntersection = calculatedIntersection;

			bool isEndPoint1 = false;
			bool isEndPoint2 = false;

			// Snap the intersection point to a path end point if it is closer than the tolerance
			if (calculatedIntersection.DistanceTo(from1) <= tolerance)
			{
				coefTo1 = 0;
				calculatedIntersection = from1;
				isEndPoint1 = true;
			}
			else if (calculatedIntersection.DistanceTo(to1) <= tolerance)
			{
				pos1 = 1;
				coefFrom1 = 0;
				actualIntersection = to1;
				isEndPoint1 = true;
			}

			if (calculatedIntersection.DistanceTo(from2) <= tolerance)
			{
				coefTo2 = 0;
				calculatedIntersection = from2;
				isEndPoint2 = true;
			}
			else if (calculatedIntersection.DistanceTo(to2) <= tolerance)
			{
				pos2 = 1;
				coefFrom2 = 0;
				actualIntersection = to2;
				isEndPoint2 = true;
			}

			// Check if the (maybe snapped) intersection point is on the intersection of the paths
			if (coefFrom1 < 0 || coefTo1 < 0 || coefFrom2 < 0 || coefTo2 < 0)
			{
				pos1 = 0;
				pos2 = 0;
				return null;
			}

			if (!isEndPoint1)
			{
				pos1 = Math.Asin(from1.CrossProduct(actualIntersection).Length) / Math.Asin(norm1.Length);
			}
			if (!isEndPoint2)
			{
				pos2 = Math.Asin(from2.CrossProduct(actualIntersection).Length) / Math.Asin(norm2.Length);
			}

			return actualIntersection;
		}

		/// <summary>
		/// Returns the intersection point of two segments on the Earth, if they intersect.
		/// Returns null if they do not intersect.
		/// </summary>
		/// <param name="from1">The start of the first segment</param>
		/// <param name="to1">The end of the first segment</param>
		/// <param name="from2">The start of the second segment</param>
		/// <param name="to2">The end of the second segment</param>
		/// <param name="pos1">Set to be the relative position from the start point on the first segment of the intersection point, it will be in the range 0 (intersection is start point) to 1 (intersection is end point)</param>
		/// <param name="pos2">Set to be the relative position from the start point on the second segment of the intersection point, it will be in the range 0 (intersection is start point) to 1 (intersection is end point)</param>
		public static GeoCoordinate LineIntersection(GeoCoordinate from1, GeoCoordinate to1, GeoCoordinate from2, GeoCoordinate to2, out double pos1, out double pos2)
		{
			Coordinate fromSph1 = from1.AsUnitSpherePoint();
			Coordinate toSph1 = to1.AsUnitSpherePoint();
			Coordinate fromSph2 = from2.AsUnitSpherePoint();
			Coordinate toSph2 = to2.AsUnitSpherePoint();

			Coordinate intersection = LineIntersection(fromSph1, toSph1, fromSph2, toSph2, out pos1, out pos2);
			if (intersection == null)
			{
				return null;
			}
			else
			{
				return intersection.UnitSpherePointToGeoCoordinate();
			}
		}

		/// <summary>
		/// Returns whether two unit sphere shortest paths have different start and end points within a tolerance, and intersect.
		/// </summary>
		/// <param name="from1">The start point of the first path, expected to be on the unit sphere</param>
		/// <param name="to1">The end point of the first path, expected to be on the unit sphere</param>
		/// <param name="from2">The start point of the second path, expected to be on the unit sphere</param>
		/// <param name="to2">The end point of the second path, expected to be on the unit sphere</param>
		/// <param name="tolerance">An tolerance on how close two points must be if they should be regarded as the same point</param>
		internal static bool LinesIntersectAndHaveDifferentEndPoints(Coordinate from1, Coordinate to1, Coordinate from2, Coordinate to2, double tolerance = 0)
		{
			if (from1.DistanceTo(from2) <= tolerance || from1.DistanceTo(to2) <= tolerance || to1.DistanceTo(from2) <= tolerance || to1.DistanceTo(to2) <= tolerance)
			{
				return false;
			}

			return LinesIntersect(from1, to1, from2, to2);
		}

		/// <summary>
		/// Returns the determinant of three unit sphere points. The method should be numerically stable if the points are close compared to the sphere radius
		/// </summary>
		/// <param name="c0">The first point in the determinant</param>
		/// <param name="c1">The second point in the determinant</param>
		/// <param name="c2">The third point in the determinant</param>
		public static double ClosePointsDeterminant(Coordinate c0, Coordinate c1, Coordinate c2)
		{
			Coordinate c0xc1 = c0.CrossProduct(c1);
			Coordinate c1xc2 = c1.CrossProduct(c2);
			Coordinate c2xc0 = c2.CrossProduct(c0);
			if (c0xc1.Length == 0 || c1xc2.Length == 0 || c2xc0.Length == 0)
			{
				return 0;
			}

			Coordinate closeC2 = c2 + (c0xc1 * c1xc2 * c0 + c0xc1 * c2xc0 * c1) / (c0xc1 * c0xc1);
			return c0xc1 * closeC2;
		}

		/// <summary>
		/// Returns a GeoCoordinate as a unit sphere point, assuming a perfectly round Earth.
		/// </summary>
		/// <param name="geoCoord">The coordiante to get the unit sphere point for</param>
		public static Coordinate AsUnitSpherePoint(this GeoCoordinate geoCoord)
		{
			double latRadians = geoCoord.Latitude * Math.PI / 180;
			double lonRadians = geoCoord.Longitude * Math.PI / 180;
			double coslat = Math.Cos(latRadians);
			double sinlat = Math.Sin(latRadians);
			double coslon = Math.Cos(lonRadians);
			double sinlon = Math.Sin(lonRadians);
			return new Coordinate(coslat * coslon, coslat * sinlon, sinlat);
		}

		/// <summary>
		/// Returns the GeoCoordinate from a unit sphere point, assuming a perfectly round Earth.
		/// </summary>
		/// <param name="coordinate">The unit sphere point to get the GeoCoordinate for</param>
		public static GeoCoordinate UnitSpherePointToGeoCoordinate(this Coordinate coordinate)
		{
			double latitude = Math.Asin(Math.Max(-1, Math.Min(1, coordinate.Z))) * 180 / Math.PI;
			latitude = Math.Max(-90, Math.Min(90, latitude));
			double longitude = Math.Atan2(coordinate.Y, coordinate.X) * 180 / Math.PI;
			longitude = Math.Max(-180, Math.Min(180, longitude));
			return new GeoCoordinate(latitude, longitude);
		}

		/// <summary>
		/// The direction at a unit sphere point, when moving the shortest path on the unit sphere towards another unit sphere point.
		/// North is 0, East is 90, West is -90, South is 180 or -180.
		/// </summary>
		/// <param name="from">The point where the bearing is measured, assumed to be a unit sphere point</param>
		/// <param name="to">The point moving towards when calculating the bearing, assumed to be a unit sphere point</param>
		public static double Bearing(Coordinate from, Coordinate to)
		{
			Coordinate northAtFrom = new Coordinate(-from.Z * from.X, -from.Z * from.Y, 1 - from.Z * from.Z);
			Coordinate eastAtFrom = northAtFrom.CrossProduct(from);
			return Math.Atan2(eastAtFrom * to, northAtFrom * to) * 180 / Math.PI;
		}
	}
}
