//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;

namespace Sintef.Scoop.Utilities.GeoCoding
{
	/// <summary>
	/// A GeoCoordinate bounding box.
	/// </summary>
	/// <remarks>
	/// 
	/// This bounding box represents an area of the surface of the 
	/// Earth. The area is defined as the area between a pair of 
	/// latitudes and a pair of longitudes, inclusive. See 
	/// GeoCoordinate for more details about the coordinate system.
	/// 
	/// The following restrictions on the coordinates apply:
	/// -90 &lt;= min_latitude &lt;= max_latitude &lt;= 90
	/// -180 &lt;= min_longitude &lt; 180
	/// min_longitude &lt;= max_longitude &lt;= min_longitude + 360
	/// The range [180, 540) of (max_)longitude is equivalent to the 
	/// range [-180, 180).
	/// </remarks>
	public class BoundingBox : BoundingBoxBase<GeoCoordinate>
	{
		/// <summary>
		/// The minimum latitude covered by the bounding box, in [-90, 90]
		/// </summary>
		public double MinLatitude => MinY;

		/// <summary>
		/// The maximum latitude covered by the bounding box, in [-90, 90]
		/// </summary>
		public double MaxLatitude => MaxY;

		/// <summary>
		/// The minimum longitude covered by the bounding box, in [-180, 180)
		/// </summary>
		public double MinLongitude => MinX;

		/// <summary>
		/// The maximum longitude covered by the bounding box, in [MinLongitude, MinLongitude + 360)
		/// </summary>
		public double MaxLongitude => MaxX;

		/// <summary>
		/// Default constructor. Creates a bounding box of zero extent, at (0,0).
		/// </summary>
		public BoundingBox() : base()
		{
		}

		/// <summary>
		/// Constructor. Creates a bounding box that covers the given coordinate 
		/// (and nothing else)
		/// </summary>
		public BoundingBox(GeoCoordinate coordinate) : base(coordinate)
		{
		}

		/// <summary>
		/// Constructor. Creates a bounding box that covers the given intervals
		/// </summary>
		public BoundingBox(double minY, double maxY, double minX, double maxX)
			: base(minY, maxY, minX, maxX)
		{
			if (minY < -90 || maxY < minY || 90 < maxY)
				throw new ArgumentException(string.Format("Illegal latitude interval [{0}, {1}]", minY, maxY));

			if (minX < -180 || minX >= 180 || maxX < minX ||
					maxX > minX + 360)
				throw new ArgumentException(string.Format("Illegal longitude interval [{0}, {1}]", minX, maxX));
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		public BoundingBox(BoundingBox other)
			: this(other.MinY, other.MaxY, other.MinX, other.MaxX)
		{
		}

		/// <summary>
		/// Creates a clone of this bounding box
		/// </summary>
		public override BoundingBoxBase<GeoCoordinate> Clone()
		{
			return new BoundingBox(this);
		}

		/// <summary>
		/// Returns true if the given cooridinate is inside the 
		/// bounding box, false if not
		/// </summary>
		public override bool Contains(ICoordinate coord)
		{
			if (MinY > coord.Y + contain_tol_)
				return false;
			if (MaxY < coord.Y - contain_tol_)
				return false;

			if (MaxX < coord.X - contain_tol_)
				return false;
			if (MinX > coord.X + contain_tol_ && coord.X > MaxX - 360 + contain_tol_)
				return false;

			return true;
		}

		/// <summary>
		/// Returns true if the given bounding box is entirely inside this
		/// bounding box, false if not
		/// </summary>
		public override bool Contains(IBoundingBox other)
		{
			if (MinY > other.MinY + contain_tol_)
				return false;
			if (MaxY < other.MaxY - contain_tol_)
				return false;

			if (MinX == 0 && MaxX == 360)
				return true;

			if (other.MinX < MinX - contain_tol_ &&
					other.MaxX > MaxX - 360 + contain_tol_)
				return false;
			if (other.MaxX > MaxX + contain_tol_)
				return false;

			return true;
		}

		/// <summary>
		/// Returns true if this bounding box and the given
		/// bounding box have any coordinate in common.
		/// </summary>
		public override bool Intersects(IBoundingBox other)
		{
			if (MinY > other.MaxY)
				return false;
			if (MaxY < other.MinY)
				return false;

			if (MaxX < other.MinX && MinX > other.MaxX - 360)
				return false;
			if (MinX > other.MaxX && MaxX - 360 < other.MinX)
				return false;

			return true;
		}

		/// <summary>
		/// Returns the distance, in meters, along a Great Circle, 
		/// between the given coordinate and the closest point on
		/// the edge of the bounding box. The coordinate must lie
		/// inside the bounding box.
		/// </summary>
		protected override double InsideDistance(GeoCoordinate coord)
		{
			double latDist = 1e200;

			BoundingBox latSlice = new BoundingBox(-90, 90, MinX, MaxX);
			if (latSlice.Contains(coord))
			{
				double y = GeoCoordinate.MeridianLength(coord.Latitude);
				double minY = GeoCoordinate.MeridianLength(MinY);
				double maxY = GeoCoordinate.MeridianLength(MaxY);

				latDist = Math.Min(Math.Abs(y - minY), Math.Abs(y - maxY));
			}

			GeoCoordinate edgePoint1 = coord.ClosestPoint(new GeoCoordinate(MinY, MinX), new GeoCoordinate(MaxY, MinX)) as GeoCoordinate;
			double d1 = coord.DistanceTo(edgePoint1);
			GeoCoordinate edgePoint2 = coord.ClosestPoint(new GeoCoordinate(MinY, MaxX), new GeoCoordinate(MaxY, MaxX)) as GeoCoordinate;
			double d2 = coord.DistanceTo(edgePoint2);

			return Math.Min(Math.Min(d1, d2), latDist);
		}

		/// <summary>
		/// Returns the distance, in meters, along a Great Circle, 
		/// between the given coordinate and the closest point on
		/// the edge of the bounding box. The coordinate must lie
		/// outside the bounding box.
		/// </summary>
		protected override double OutsideDistance(GeoCoordinate coord)
		{
			BoundingBox latSlice = new BoundingBox(-90, 90, MinX, MaxX);
			if (latSlice.Contains(coord))
			{
				double y = GeoCoordinate.MeridianLength(coord.Latitude);

				if (coord.Latitude > MaxY)
					return y - GeoCoordinate.MeridianLength(MaxY);
				else
					return GeoCoordinate.MeridianLength(MinY) - y;
			}

			GeoCoordinate edgePoint1;
			GeoCoordinate edgePoint2;
			{
				edgePoint1 = coord.ClosestPoint(new GeoCoordinate(MinY, MinX), new GeoCoordinate(MaxY, MinX)) as GeoCoordinate;
				edgePoint2 = coord.ClosestPoint(new GeoCoordinate(MinY, MaxX), new GeoCoordinate(MaxY, MaxX)) as GeoCoordinate;
			}

			double d1 = coord.DistanceTo(edgePoint1);
			double d2 = coord.DistanceTo(edgePoint2);

			return Math.Min(d1, d2);
		}

		/// <summary>
		/// Expands the bounding box, if necessary, to ensure that 
		/// the given coordinate is contained by this bounding box.
		/// </summary>
		public override void ExpandBy(ICoordinate coord)
		{
			double latitude = coord.Y;
			if (latitude > MaxY)
				MaxY = latitude;
			if (latitude < MinY)
				MinY = latitude;

			double longitude = coord.X;
			if (longitude > MaxX)
				MaxX = longitude;

			if (longitude < MinX && longitude > MaxX - 360)
			{
				// Choose smallest expansion 
				if (MinX - longitude <= longitude - (MaxX - 360))
					// Expand west
					MinX = longitude;
				else
					// Expand east
					MaxX = longitude + 360;
			}

			/*
			assert(Contains(coord));
			assert(-90 <= MinY);
			assert(MinY <= MaxY);
			assert(MaxY <= 90);
			assert(-180 <= MinX);
			assert(MinX < 180);
			assert(MinX <= MaxX);
			assert(MaxX <= MinX + 360);
			 */
		}

		/// <summary>
		/// Expands the bounding box, if necessary, to ensure that 
		/// the given bounding box is entirely contained by this 
		/// bounding box.
		/// </summary>
		public override void ExpandBy(IBoundingBox other)
		{
			if (other.MinY < MinY)
				MinY = other.MinY;
			if (other.MaxY > MaxY)
				MaxY = other.MaxY;

			if (MaxX - MinX >= 360)
				// Cannot expand
				return;

			double other_min = other.MinX;
			double other_max = other.MaxX;

			if (other_max - other_min >= 360)
			{
				// Other covers 360 degrees
				MinX = other_min;
				MaxX = other_max;
				return;
			}

			// Adjust so that western end of other is east of western end of this
			if (other_min < MinX)
			{
				other_min += 360;
				other_max += 360;
			}

			if (other_min >= MinX && other_max <= MaxX)
				// Other completely contained by this
				return;

			if (other_min - 360 <= MinX && other_max - 360 >= MaxX)
			{
				// This completely contained by other
				MinX = other.MinX;
				MaxX = other.MaxX;
				return;
			}

			if (other_min <= MaxX && other_max - 360 >= MinX)
			{
				// This and other cover entire 360 degrees
				MinX = 0;
				MaxX = 360;
				return;
			}

			if (other_min <= MaxX)
			{
				// East of this and west of other overlap
				MaxX = other_max;
				return;
			}

			if (other_max - 360 >= MinX)
			{
				// West of this and east of other overlap
				MinX = other_min - 360;
				if (MinX < -180)
				{
					MinX += 360;
					MaxX += 360;
				}
				return;
			}

			// No overlap.

			if (other_min - MaxX < MinX - (other_max - 360))
			{
				// Eastern expansion is smaller
				MaxX = other_max;

			}
			else
			{
				// Western expansion is smaller
				MinX = other_min - 360;
				if (MinX < -180)
				{
					MinX += 360;
					MaxX += 360;
				}
			}
		}

		/// <summary>
		/// Expands the bounding box to include any point within
		/// distance \p d (in meters) of the original box
		/// </summary>
		public override void ExpandBy(double d)
		{
			double extreme_latitude = Math.Max(MaxY, -MinY);
			double test_meridian = MaxX - 1;

			double dist_to_test = new GeoCoordinate(extreme_latitude, MaxX).DistFromMeridian(test_meridian, out double dummy);
			double add_to_long = d / dist_to_test;

			MinX -= add_to_long;
			MaxX += add_to_long;

			if (MaxX - MinX >= 360)
			{
				MinX = 0;
				MaxX = 360;
			}
			else if (MinX < -180)
			{
				MinX += 360;
				MaxX += 360;
			}

			if (MaxY < 90)
			{
				double dist_to_n_pole = GeoCoordinate.MeridianLength(90)
																- GeoCoordinate.MeridianLength(MaxY);
				double add_to_max_lat = d * (90 - MaxY) / dist_to_n_pole;

				MaxY += add_to_max_lat;
				if (MaxY > 90)
					MaxY = 90;
			}

			if (MinY > -90)
			{
				double dist_to_s_pole = GeoCoordinate.MeridianLength(MinY)
																- GeoCoordinate.MeridianLength(-90);
				double sub_from_min_lat = d * (MinY + 90) / dist_to_s_pole;

				MinY -= sub_from_min_lat;
				if (MinY < -90)
					MinY = -90;
			}
		}
	}
}
