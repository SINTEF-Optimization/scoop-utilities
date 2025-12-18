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
	/// Utility functions for straight lines in the Longitude/Latitude coordinate system
	/// </summary>
	public static class LongitudeLatitudeStraightLinesGeometry
	{
		/// <summary>
		/// Returns whether two straight lines in the Longitude/Latitude coordinate system have different start and end points within a tolerance, and intersect.
		/// </summary>
		/// <param name="from1">The start coordinate of the first line</param>
		/// <param name="to1">The end coordinate of the first line</param>
		/// <param name="from2">The start coordinate of the second line</param>
		/// <param name="to2">The end coordinate of the second line</param>
		/// <param name="tolerance">An tolerance on how close two coordinate must be if they should be regarded as the same</param>
		public static bool LinesIntersectAndHaveDifferentEndPoints(GeoCoordinate from1, GeoCoordinate to1, GeoCoordinate from2, GeoCoordinate to2, double tolerance = 0)
		{
			if (from1.DistanceTo(from2) <= tolerance || from1.DistanceTo(to2) <= tolerance || to1.DistanceTo(from2) <= tolerance || to1.DistanceTo(to2) <= tolerance)
			{
				return false;
			}

			return LinesIntersect(from1, to1, from2, to2);
		}

		/// <summary>
		/// Returns whether two lines in the Longitude/Latitude coordinate system intersect.
		/// </summary>
		/// <param name="from1">The start coordinate of the first line</param>
		/// <param name="to1">The end coordinate of the first line</param>
		/// <param name="from2">The start coordinate of the second line</param>
		/// <param name="to2">The end coordinate of the second line</param>
		public static bool LinesIntersect(GeoCoordinate from1, GeoCoordinate to1, GeoCoordinate from2, GeoCoordinate to2)
		{
			return LineIntersection(from1, to1, from2, to2, out double _, out double _) != null;
		}

		/// <summary>
		/// Returns the intersection point of two lines in the Longitude/Latitude coordinate system, if they intersect.
		/// Returns null if they do not intersect.
		/// </summary>
		/// <param name="from1">The start coordinate of the first line</param>
		/// <param name="to1">The end coordinate of the first line</param>
		/// <param name="from2">The start coordinate of the second line</param>
		/// <param name="to2">The end coordinate of the second line</param>
		/// <param name="pos1">Set to be the relative position from the start point on the first line of the intersection point, it will be in the range 0 (intersection is start point) to 1 (intersection is end point)</param>
		/// <param name="pos2">Set to be the relative position from the start point on the second line of the intersection point, it will be in the range 0 (intersection is start point) to 1 (intersection is end point)</param>
		/// <param name="tolerance">An tolerance on how close an intersection point must be to one of the end points on the line, to be treated as being the same coordinate</param>
		public static GeoCoordinate LineIntersection(GeoCoordinate from1, GeoCoordinate to1, GeoCoordinate from2, GeoCoordinate to2, out double pos1, out double pos2, double tolerance = 0)
		{
			pos1 = 0;
			pos2 = 0;

			double longLine1 = to1.Longitude - from1.Longitude;
			double latLine1 = to1.Latitude - from1.Latitude;
			double longLine2 = to2.Longitude - from2.Longitude;
			double latLine2 = to2.Latitude - from2.Latitude;

			double lineCross = longLine1 * latLine2 - longLine2 * latLine1;
			if (lineCross == 0)
			{
				// Special case when line segments are parallell
				return null;
			}

			double longFrom = from2.Longitude - from1.Longitude;
			double latFrom = from2.Latitude - from1.Latitude;

			pos1 = (longFrom * latLine2 - longLine2 * latFrom) / lineCross;
			pos2 = (longFrom * latLine1 - longLine1 * latFrom) / lineCross;

			if (pos1 < -0.1 || pos1 > 1.1 || pos2 < -0.1 || pos2 > 1.1)
			{
				pos1 = 0;
				pos2 = 0;
				return null;
			}

			double calculatedLat = Math.Max(-90, Math.Min(90, from1.Latitude + pos1 * latLine1));
			double calculatedLon = Math.Max(-180, Math.Min(180, from1.Longitude + pos1 * longLine1));

			// The intersection point of the prolonged lines, might be outside the line segments
			GeoCoordinate calculatedIntersection = new GeoCoordinate(calculatedLat, calculatedLon);

			// The intersection point to be returned, could be replaced by one of the path end points if they are closer than the tolerance
			GeoCoordinate actualIntersection = calculatedIntersection;

			// Snap the intersection point to a line end point if it is closer than the tolerance
			if (calculatedIntersection.DistanceTo(from1) <= tolerance)
			{
				pos1 = 0;
				actualIntersection = from1;
			}
			else if (calculatedIntersection.DistanceTo(to1) <= tolerance)
			{
				pos1 = 1;
				actualIntersection = to1;
			}

			if (calculatedIntersection.DistanceTo(from2) <= tolerance)
			{
				pos2 = 0;
				actualIntersection = from2;
			}
			else if (calculatedIntersection.DistanceTo(to2) <= tolerance)
			{
				pos2 = 1;
				actualIntersection = to2;
			}

			// Check if the (maybe snapped) intersection point is on the intersection of the lines
			if (pos1 < 0 || pos1 > 1 || pos2 < 0 || pos2 > 1)
			{
				pos1 = 0;
				pos2 = 0;
				return null;
			}

			return actualIntersection;
		}

		/// <summary>
		/// Returns a coordinate on a line in the Longitude/Latitude coordinate system
		/// </summary>
		/// <param name="start">The start coordinate of the line</param>
		/// <param name="end">The end coordinate of the line</param>
		/// <param name="fraction">The relative position on the line of the point to be returned,
		/// 0.0 returns the start coordinate, 1.0 returns the end coordinate</param>
		public static GeoCoordinate CoordinateOnLine(GeoCoordinate start, GeoCoordinate end, double fraction)
		{
			double latitude = start.Latitude + fraction * (end.Latitude - start.Latitude);
			double longitude = start.Longitude + fraction * (end.Longitude - start.Longitude);
			double altitude = double.NaN;
			if (start.Altitude != double.NaN && end.Altitude != double.NaN)
			{
				altitude = start.Altitude + fraction * (end.Altitude - start.Altitude);
			}
			return new GeoCoordinate(latitude, longitude, altitude);
		}
	}
}
