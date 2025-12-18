//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Sintef.Scoop.Utilities.GeoCoding;
using Sintef.Scoop.Utilities.GeoGeometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities.GeoRegions
{
	/// <summary>
	/// A closed polygon on the Earth surface defining a region limited by the polygon.
	/// The polygon is defined by corners and the paths between the corners (the polygon edges).
	/// The shape of the paths is defined by the EdgeType property.
	/// The edges must be well-defined in all cases, and the ploygons must not be self intersecting.
	/// </summary>
	public class ClosedGeoPolygon
	{
		/// <summary>
		/// What to do when creating a polygon defining an area that is bigger than the complement area
		/// </summary>
		public enum BigAreaAction
		{
			/// <summary>
			/// Keep the polygon as it is
			/// </summary>
			Keep,

			/// <summary>
			/// Change the polygon to define the complement area, keep the originally wanted orientation for this area.
			/// This means the HasPositiveOrientation property is unchanged, while the corner order is reversed.
			/// </summary>
			SameOrientationOnComplement,

			/// <summary>
			/// Change the polygon to define the complement area, use the opposite of the originally wanted orientation for this area.
			/// This means the HasPositiveOrientation property is changed to the opposite, while the corner order is unchanged.
			/// </summary>
			OppositeOrientationOnComplement,
		}

		/// <summary>
		/// The corners of the polygon
		/// </summary>
		private List<GeoCoordinate> _corners;

		/// <summary>
		/// The corners of the polygon given as unit sphere points. Only used if EdgeType is UnitSphereShortestPaths.
		/// </summary>
		private List<Coordinate> _cornersCartesian;

		/// <summary>
		/// The number of corners in the polygon
		/// </summary>
		int _nmbCorners;

		/// <summary>
		/// The minimum latitude of the region defined by the polygon
		/// </summary>
		private double _boundingBoxMinLatitude;

		/// <summary>
		/// The maximum latitude of the region defined by the polygon
		/// </summary>
		private double _boundingBoxMaxLatitude;

		/// <summary>
		/// The minimum longitude of the region defined by the polygon
		/// </summary>
		private double _boundingBoxMinLongitude;

		/// <summary>
		/// The maximum longitude of the region defined by the polygon
		/// </summary>
		private double _boundingBoxMaxLongitude;

		/// <summary>
		/// True if this polygon defines a region on the left side of the polygon when moving in corners order.
		/// False if it defines a region on the right side of the polygon when moving in corners order.
		/// </summary>
		public bool HasPositiveOrientation { get; private set; }

		/// <summary>
		/// The area of the region in km^2
		/// </summary>
		public double Area { get; private set; }

		/// <summary>
		/// How the shape of the edges in the polygon are defined. The edge type can not be NoEdges.
		/// </summary>
		public RegionEdgeType EdgeType { get; }

		/// <summary>
		/// The corners of the polygon
		/// </summary>
		public IEnumerable<GeoCoordinate> Corners => _corners.AsReadOnly();

		/// <summary>
		/// The corners of the polygon given as unit sphere points. Only used if EdgeType is UnitSphereShortestPaths.
		/// </summary>
		public IEnumerable<Coordinate> CornersCartesian => _cornersCartesian.AsReadOnly();

		/// <summary>
		/// Returns all edges in the polygon as pairs of GeoCoordinates
		/// </summary>
		public IEnumerable<(GeoCoordinate From, GeoCoordinate To)> Edges => Enumerable.Range(0, _nmbCorners).Select(idx => (_corners[idx], _corners[(idx + 1) % _nmbCorners]));

		/// <summary>
		/// Returns all edges in the polygon as pairs of unit sphere points. Only works if EdgeType is UnitSphereShortestPaths.
		/// </summary>
		public IEnumerable<(Coordinate From, Coordinate To)> EdgesCartesian => Enumerable.Range(0, _nmbCorners).Select(idx => (_cornersCartesian[idx], _cornersCartesian[(idx + 1) % _nmbCorners]));

		/// <summary>
		/// Creates a closed polygon on the Earth surface defining a region limited by the polygon, where the edges are the shortest paths on the Earth,
		/// assumed to be a perfectly round sphere.
		/// </summary>
		/// <param name="corners">The corners of the polygon</param>
		/// <param name="hasPositiveOrientation">Whether this polygon defines a region on its left side (true) or right side (false) when moving in corners order.</param>
		/// <param name="bigAreaAction">What to do if the region defined by the polygon is bigger than the complement. Only relevant if edgeType is UnitSphereShortestPaths.</param>
		private ClosedGeoPolygon(IEnumerable<GeoCoordinate> corners, bool hasPositiveOrientation, BigAreaAction bigAreaAction)
		{
			EdgeType = RegionEdgeType.UnitSphereShortestPaths;
			_corners = new List<GeoCoordinate>(corners);
			_cornersCartesian = _corners.Select(coord => coord.AsUnitSpherePoint()).ToList();
			_nmbCorners = _corners.Count;
			HasPositiveOrientation = hasPositiveOrientation;
			CalculateBoudningBox();

			CalculateArea();
			if (bigAreaAction != BigAreaAction.Keep && Area > 253000000)
			{
				// It is probable that the region area is bigger than the complements area, in that case should use the complement instead

				// To calculate the area of the complement, we change HasPositiveOrientation og recalculate the area.
				// This also the action to be done if the complement region is smaller and bigAreaAction == OppositeOrientationOnComplement
				HasPositiveOrientation = !hasPositiveOrientation;
				double oldArea = Area;
				CalculateArea();

				if (oldArea <= Area)
				{
					// The complement was not smaller, use original region
					HasPositiveOrientation = hasPositiveOrientation;
					Area = oldArea;
				}
				else if (bigAreaAction == BigAreaAction.SameOrientationOnComplement)
				{
					// The complement was smaller so we use it, but according to bigAreaAction, we should not
					// change HasPositiveOrientation, but reverse the order of the corners
					HasPositiveOrientation = hasPositiveOrientation;
					_corners.Reverse();
					_cornersCartesian.Reverse();
				}
			}
		}

		/// <summary>
		/// Creates a closed polygon on the Earth surface defining a region limited by the polygon, where the edges are straight lines in
		/// the longitude/latitude coordinate system. The HasPositiveOrientation property will be set according the orientation of the polygon.
		/// </summary>
		/// <param name="corners">The corners of the polygon</param>
		/// <param name="forcedOrientation">If true, HasPositiveOrientation will be set to true, the corners order will be reversed if they had negative orientation.
		/// If false, HasPositiveOrientation will be set to false, the corners order will be reversed if they had positive orientation.
		/// If null, HasPositiveOrientation property will be set according the orientation of the corners, the corners order will remain unchanged.</param>
		private ClosedGeoPolygon(IEnumerable<GeoCoordinate> corners, bool? forcedOrientation)
		{
			EdgeType = RegionEdgeType.LatitudeLongitudeStraightLines;
			_corners = new List<GeoCoordinate>(corners);
			_cornersCartesian = null;
			_nmbCorners = _corners.Count;

			CalculateBoudningBox();
			CalculateAreaAndOrientation();

			if (forcedOrientation.HasValue && (forcedOrientation.Value != HasPositiveOrientation))
			{
				_corners.Reverse();
				HasPositiveOrientation = forcedOrientation.Value;
			}
		}

		/// <summary>
		/// Creates a closed polygon on the Earth surface defining a region limited by the polygon, where the edges are the
		/// shortest paths on the Earth, assumed to be a perfectly round sphere.
		/// </summary>
		/// <param name="corners">The corners of the polygon</param>
		/// <param name="hasPositiveOrientation">Whether this polygon defines a region on its left side (true) or right side (false) when moving in corners order.</param>
		/// <param name="bigAreaAction">What to do if the region defined by the polygon is bigger than the complement.</param>
		public static ClosedGeoPolygon UnitSphereShortestPathsPolygon(IEnumerable<GeoCoordinate> corners, bool hasPositiveOrientation, BigAreaAction bigAreaAction)
		{
			return new ClosedGeoPolygon(corners, hasPositiveOrientation, bigAreaAction);
		}

		/// <summary>
		/// Creates a closed polygon on the Earth surface defining a region limited by the polygon, where the edges are the
		/// shortest paths on the Earth, assumed to be a perfectly round sphere.
		/// The region will be smaller than (or equal to) its complement, and the orientation is set to positive or negative,
		/// depending on whether the region is to the left or right of the polygon when running in the corners order
		/// </summary>
		/// <param name="corners">The corners of the polygon</param>
		public static ClosedGeoPolygon UnitSphereShortestPathsPolygon(IEnumerable<GeoCoordinate> corners)
		{
			return new ClosedGeoPolygon(corners, true, BigAreaAction.OppositeOrientationOnComplement);
		}

		/// <summary>
		/// Creates a closed polygon on the Earth surface defining a region limited by the polygon, where the edges are straight lines in
		/// the longitude/latitude coordinate system.
		/// </summary>
		/// <param name="corners">The corners of the polygon</param>
		/// <param name="forcedOrientation">If true, HasPositiveOrientation will be set to true, the corners order will be reversed if they had negative orientation.
		/// If false, HasPositiveOrientation will be set to false, the corners order will be reversed if they had positive orientation.
		/// If null, HasPositiveOrientation property will be set according the orientation of the corners, the corners order will remain unchanged.</param>
		public static ClosedGeoPolygon LatitudeLongitudeStraightLinesPolygon(IEnumerable<GeoCoordinate> corners, bool? forcedOrientation)
		{
			return new ClosedGeoPolygon(corners, forcedOrientation);
		}

		/// <summary>
		/// Creates a closed polygon on the Earth surface defining a region limited by the polygon, where the edges are straight lines in
		/// the longitude/latitude coordinate system. The HasPositiveOrientation property will be set according the orientation of the polygon.
		/// </summary>
		/// <param name="corners">The corners of the polygon</param>
		public static ClosedGeoPolygon LatitudeLongitudeStraightLinesPolygon(IEnumerable<GeoCoordinate> corners)
		{
			return new ClosedGeoPolygon(corners, null);
		}

		/// <summary>
		/// Returns a new closed polygon defining the same region, but with the corners in reversed order and with the opposite orientation than this polygon.
		/// </summary>
		public ClosedGeoPolygon ReversedOrientation()
		{
			if (EdgeType == RegionEdgeType.UnitSphereShortestPaths)
			{
				return UnitSphereShortestPathsPolygon(Corners.Reverse(), !HasPositiveOrientation, BigAreaAction.Keep);
			}
			else
			{
				return LatitudeLongitudeStraightLinesPolygon(Corners.Reverse());
			}
		}

		/// <summary>
		/// Returns a cloned copy of the polygon.
		/// </summary>
		public ClosedGeoPolygon Clone()
		{
			if (EdgeType == RegionEdgeType.UnitSphereShortestPaths)
			{
				return UnitSphereShortestPathsPolygon(_corners.Select(coord => new GeoCoordinate(coord.Latitude, coord.Longitude, coord.Altitude)), HasPositiveOrientation, BigAreaAction.Keep);
			}
			else
			{
				return LatitudeLongitudeStraightLinesPolygon(_corners.Select(coord => new GeoCoordinate(coord.Latitude, coord.Longitude, coord.Altitude)));
			}
		}

		/// <summary>
		/// Change the polygon so that the region limited by the polygon is the complement of the current region.
		/// </summary>
		/// <param name="keepPositiveOrientation">If true, the defined region will still be on the same side when moving in the corners order.
		/// I.e. HasPositiveOrientation is unchanged, the corners order is reversed.
		/// If flase, the defined region will be on the opposite side as it was before when moving in the corners order.
		/// I.e. HasPositiveOrientation is changed, the corners order is unchanged.</param>
		internal void UseComplementRegion(bool keepPositiveOrientation)
		{
			if (EdgeType != RegionEdgeType.UnitSphereShortestPaths)
			{
				throw new InvalidOperationException("It is only possible to change to the complement region for polygons of edge type UnitSphereShortestPaths");
			}

			if (keepPositiveOrientation)
			{
				_corners.Reverse();
				_cornersCartesian.Reverse();
			}
			else
			{
				HasPositiveOrientation = !HasPositiveOrientation;
			}
			CalculateArea();
		}

		/// <summary>
		/// Returns whether the region defined by the polygon contains a given coordinate
		/// </summary>
		/// <param name="coordinate">The coordinate</param>
		/// <param name="acceptOnEdge">Tells if a coordinate on the edge is considered as contained in the region</param>
		public bool Contains(GeoCoordinate coordinate, bool acceptOnEdge = true)
		{
			if (coordinate.Latitude < _boundingBoxMinLatitude || coordinate.Latitude > _boundingBoxMaxLatitude || coordinate.Longitude < _boundingBoxMinLongitude || coordinate.Longitude > _boundingBoxMaxLongitude)
			{
				return false;
			}

			if (EdgeType == RegionEdgeType.UnitSphereShortestPaths)
			{
				// Missing extra handling for the case where a polygon corner is antopodal to the coordinate to be tested for being inside

				double area = 0;
				double angleChange = 0;
				Coordinate coordCartesian = coordinate.AsUnitSpherePoint();
				foreach ((Coordinate c1, Coordinate c2) in EdgesCartesian)
				{
					if (coordCartesian.Equals(c1))
					{
						return acceptOnEdge;
					}

					// Missing extra handling when coordinate lies on the line between the corners
					double angle = UnitSphereGeometry.SurfaceAngle(coordCartesian, c1, c2);
					double areaChange = UnitSphereGeometry.GetSignedArea(coordCartesian, c1, c2);

					area += areaChange;
					if (areaChange >= 0)
					{
						angleChange += angle;
					}
					else
					{
						angleChange -= angle;
					}
				}

				bool insideForPositiveOrientation = angleChange > Math.PI || (Math.Abs(angleChange) < Math.PI && area < 0);
				return insideForPositiveOrientation == HasPositiveOrientation;
			}
			else if (EdgeType == RegionEdgeType.LatitudeLongitudeStraightLines)
			{
				if (_corners.Count == 0)
					return false;

				double cLon = coordinate.Longitude;
				double cLat = coordinate.Latitude;

				// The algorithm is to run through the corners and sum up the changes in
				// the quadrant they lie in relative to the point
				int quadrantChanges = 0;
				int previousQuadrant = 0;
				double previousLon = 0.0;
				double previousLat = 0.0;

				for (int i = 0; i <= _corners.Count; ++i)
				{
					int idx = i == _corners.Count ? 0 : i;
					double lon = _corners[idx].Longitude - cLon;
					double lat = _corners[idx].Latitude - cLat;
					int quadrant = 0;
					if (lon == 0.0 && lat == 0.0)
						return acceptOnEdge;  // The point lies on a corner
					else if (lon >= 0.0 && lat >= 0.0)
						quadrant = 0;
					else if (lon <= 0.0 && lat <= 0.0)
						quadrant = 2;
					else if (lon < 0.0)
						quadrant = 1;
					else
						quadrant = 3;
					if (i > 0)
					{
						int changeQuadrant = quadrant - previousQuadrant;
						if (changeQuadrant == 3)
							changeQuadrant = -1;
						else if (changeQuadrant == -3)
							changeQuadrant = 1;
						else if (changeQuadrant == 2 || changeQuadrant == -2)
						{
							// Goes from one quadrant to the oposite. Use cross product to determine which side
							// the edge lies on
							double crossProd = previousLon * lat - previousLat * lon;
							if (crossProd == 0.0)
								return acceptOnEdge;   // The point lies on the edge
							else if (crossProd > 0.0)
								changeQuadrant = 2;
							else
								changeQuadrant = -2;
						}
						quadrantChanges += changeQuadrant;
					}
					previousQuadrant = quadrant;
					previousLon = lon;
					previousLat = lat;
				}

				// quadrantChanges is either
				// 0 (point is outside),
				// 4 (inside, polygon orientation is positive) or
				// -4 (inside, polygon orientation is negative)
				return quadrantChanges != 0;
			}
			else
			{
				throw new InvalidOperationException($"Can not test if coordinate is contained in polygon of type {EdgeType}");
			}
		}

		/// <summary>
		/// Returns whether a part of a segment is contained in the region defined by the polygon (including the edges)
		/// </summary>
		/// <param name="start">The start point of the segment</param>
		/// <param name="end">The end point of the segment</param>
		public bool ContainsPartOfSegment(GeoCoordinate start, GeoCoordinate end)
		{
			// Check if start or end of segment is inside
			if (Contains(start, true) || Contains(end, true))
				return true;

			if (EdgeType == RegionEdgeType.UnitSphereShortestPaths)
			{
				Coordinate startCartesian = start.AsUnitSpherePoint();
				Coordinate endCartesian = end.AsUnitSpherePoint();

				// Check if the segment hits any of the polygon edges
				return EdgesCartesian.Any(edge => UnitSphereGeometry.LinesIntersect(startCartesian, endCartesian, edge.From, edge.To));
			}
			else if (EdgeType == RegionEdgeType.LatitudeLongitudeStraightLines)
			{
				// Check if the segment hits any of the polygon edges
				return Edges.Any(edge => LongitudeLatitudeStraightLinesGeometry.LinesIntersect(start, end, edge.From, edge.To));
			}
			else
			{
				throw new InvalidOperationException($"Can not test segment containment for polygon of type {EdgeType}");
			}
		}

		/// <summary>
		/// Calculcates the area of the region defined by the polygon, and determines the orientation.
		/// </summary>
		private void CalculateAreaAndOrientation()
		{
			if (EdgeType != RegionEdgeType.LatitudeLongitudeStraightLines)
			{
				throw new InvalidOperationException("Area and orientation together are only calculated for polygons of edge type LatitudeLongitudeStraightLines");
			}

			double signedArea = GeoCoordinate.SignedArea(_corners);
			HasPositiveOrientation = signedArea >= 0.0;
			Area = Math.Abs(signedArea) / 1000000.0;
		}

		/// <summary>
		/// Calculates the area of the region defined by the polygon
		/// </summary>
		private void CalculateArea()
		{
			if (EdgeType == RegionEdgeType.LatitudeLongitudeStraightLines)
			{
				Area = Math.Abs(GeoCoordinate.SignedArea(_corners)) / 1000000.0;
			}
			else if (EdgeType == RegionEdgeType.UnitSphereShortestPaths)
			{
				// We first sum up the unit sphere areas of the triangles from the first corner and two adjacent corners in the polygon,
				// and subtract when the area if the corners in the triangle have a negative orientation
				// We can skip the cases when the first corner is one of the adjacent corners.
				double area = EdgesCartesian
				.Skip(1)
				.Take(_nmbCorners - 2)
				.Sum(edge => UnitSphereGeometry.GetSignedArea(_cornersCartesian[0], edge.From, edge.To));

				// If orientation is negative, we have found the area (mod 4*Pi) of the complement region, so we fix this
				if (!HasPositiveOrientation)
				{
					area = 4 * Math.PI - area;
				}

				// The calculated area is only the same as the correct area mod 4*Pi, fix it to be the correct area
				while (area < 0)
				{
					area += 4 * Math.PI;
				}
				while (area >= 4 * Math.PI)
				{
					area -= 4 * Math.PI;
				}

				// Convert from unit sphere area to an estimated Earth area in km^2 by using the Earth radius at the mid latitude of the bounding box.
				double midLatitude = 0.5 * (_boundingBoxMinLatitude + _boundingBoxMaxLatitude);
				double earthRadKm = GeoCoordinate.DistanceFromEarthCenter(midLatitude) / 1000;
				Area = area * earthRadKm * earthRadKm;
			}
			else
			{
				throw new InvalidOperationException($"Can not calculate area for polygon of type {EdgeType}");
			}
		}

		/// <summary>
		/// Calculates the minimum and maximum latitude and longitude values. Currently this does not work correct for areas containing the North
		/// or South pole if the edge type is UnitSphereShortestPaths.
		/// </summary>
		private void CalculateBoudningBox()
		{
			// Longitude range of the area is the same as the longitude range of the corners
			_boundingBoxMinLongitude = _corners.Min(c => c.Longitude);
			_boundingBoxMaxLongitude = _corners.Max(c => c.Longitude);

			if (EdgeType == RegionEdgeType.LatitudeLongitudeStraightLines)
			{
				// Latitude range of the area is the same as the longitude range of the corners when the edges are straight lines in the lon/lat coordinate system
				_boundingBoxMinLatitude = _corners.Min(c => c.Latitude);
				_boundingBoxMaxLatitude = _corners.Max(c => c.Latitude);
			}
			else if (EdgeType == RegionEdgeType.UnitSphereShortestPaths)
			{
				// When the edge typs is UnitSphereShortestPaths, the Latitude range is more complicated because a shortest path between two points
				// may reach latitudes outside the range between the latitudes of the two points
				_boundingBoxMinLatitude = Double.MaxValue;
				_boundingBoxMaxLatitude = Double.MinValue;

				foreach ((GeoCoordinate c1, GeoCoordinate c2) in Edges)
				{
					// List of latitudes that will be reached by the line between polygon corners c1 and c2,
					// so that the maximum and minimum latitudes for the line will be in the list.
					double latitude1 = c1.Latitude;
					double latitude2 = c2.Latitude;
					List<double> latitudeExtremumCandidates = new List<double>() { latitude1, latitude2 };

					// To simplify expressions, we assume the longitude of c1 is 0 and the longitude of c2 is the length of the range of longitudes travelled.
					// The longitude of c2 is then in the range from 0 to 180

					double longitudeRange = Math.Abs(c2.Longitude - c1.Longitude);
					if (longitudeRange > 180)
					{
						longitudeRange = 360 - longitudeRange;
					}

					double latRad1 = latitude1 * Math.PI / 180;
					double latRad2 = latitude2 * Math.PI / 180;
					double lonRangeRad = longitudeRange * Math.PI / 180;
					double sinLat1 = Math.Sin(latRad1);
					double cosLat1 = Math.Cos(latRad1);
					double sinLat2 = Math.Sin(latRad2);
					double cosLat2 = Math.Cos(latRad2);
					double sinLonRange = Math.Sin(lonRangeRad);
					double cosLonRange = Math.Cos(lonRangeRad);

					// For a point on the line from c1 to c2 with latitude Lat and longidute Lon we have
					// tan(Lat) = cos(Lon) * tan(c1.Latitude) + sin(Lon) * (tan(c2.Latitude) / sin(longitudeRange) - tan(c1.Latitude) * cotan(longitudeRange))
					// It has a maximum/minimum when Lat differentiated w.r.t. Lon is zero, i.e. when
					// tan(Lon) = tan(c2.Latitude) / (sin(longitudeRange) * tan(c1.Latitude)) - cotan(longitudeRange)
					// See if this maximum/minimum latitude occurs for a longitude between c1 and c2

					// Some special case handlings to avoid divisions by zero
					if (sinLonRange == 0)
					{
						if (cosLonRange < 0)
						{
							if (latitude1 + latitude2 >= 0)
							{
								// The line crosses the North pole (or is not defined because it connets two antipodal points, this should not occur)
								latitudeExtremumCandidates.Add(90);
							}
							else
							{
								// The line crosses the South pole
								latitudeExtremumCandidates.Add(-90);
							}
						}
						// If cosLonRang > 0, the line connects two points on the same longitude, nothing more to add to the latitude extremum candidates
					}
					else if (cosLat1 == 0 || cosLat2 == 0)
					{
						// One of the points is the North or South pole, nothing more to add to the latitude extremum candidates
					}
					else
					{
						double tanLat1 = sinLat1 / cosLat1;
						double tanLat2 = sinLat2 / cosLat2;
						double cotLonRange = cosLonRange / sinLonRange;

						// tanLonTop is tan(longitude) where the latitude reaches a maximum/minimum on the prolonged line
						double tanLonTop = tanLat2 / (sinLonRange * tanLat1) - cotLonRange;
						if (tanLonTop > 0 && tanLonTop * cotLonRange < 1)
						{
							// The top/bottom point of the prolonged line is inside the line from c1 to c2, add its latitude to the latitude extremum candidates
							double lonRadTop = Math.Atan(tanLonTop);
							double tanLatTop = Math.Cos(lonRadTop) * tanLat1 + Math.Sin(lonRadTop) * (tanLat2 / sinLonRange - tanLat1 * cotLonRange);
							double latRadTop = Math.Atan(tanLatTop);
							latitudeExtremumCandidates.Add(latRadTop * 180 / Math.PI);
						}
					}

					_boundingBoxMinLatitude = Math.Min(_boundingBoxMinLatitude, latitudeExtremumCandidates.Min());
					_boundingBoxMaxLatitude = Math.Max(_boundingBoxMaxLatitude, latitudeExtremumCandidates.Max());
				}
			}
			else
			{
				throw new InvalidOperationException($"Can not calculate bounding box for polygon of type {EdgeType}");
			}
		}
	}
}
