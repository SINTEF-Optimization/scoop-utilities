//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.ComponentModel;
using System.Globalization;
using Newtonsoft.Json;

namespace Sintef.Scoop.Utilities.GeoCoding
{
	/// <summary>
	/// A geographical coordinate.
	/// 
	/// The coordinate specifies a point on the surface of the 
	/// Earth, using geographical coordinates (latitude, longitude)
	/// in degrees and datum WGS84.
	/// </summary>
	[DataContract]
	[TypeConverter(typeof(GenericObjectConverter<GeoCoordinate>))]
	public class GeoCoordinate : ICoordinate
	{
		#region Public properties

		/// <summary>
		/// The latitude.
		/// 
		/// Latitudes can range from -90 to 90. Negative numbers 
		/// identify the Southern hemisphere and positive numbers 
		/// identify the Northern hemisphere. 0 is the equator.
		/// </summary>
		[DataMember]
		[JsonProperty(PropertyName = "latitude", Required = Required.Default)]
		public double Latitude { get; private set; }

		/// <summary>
		/// The longitude.
		/// 
		/// Longitudes can range from -180 (inclusive) to 180 
		/// (exclusive). Negative numbers identify the Western 
		/// hemisphere and positive numbers identify the Eastern 
		/// hemisphere. 0 is the Greenwich meridian in England. 
		/// </summary>
		[DataMember]
		[JsonProperty(PropertyName = "longitude", Required = Required.Default)]
		public double Longitude { get; private set; }

		/// <summary>
		/// The altitude.
		/// 
		/// the elevation in meters from the Earth's surface.
		/// </summary>
		[DataMember]
		[JsonProperty(PropertyName = "altitude", Required = Required.Default)]
		public double Altitude { get; private set; }

		#region ICoordinate implementation

		/// <summary>
		/// Implements vertical coordinate component, a property from ICoordinate.
		/// </summary>
		public double Z { get => Altitude; set => Altitude = value; }

		/// <summary>
		/// East coordinate, in degrees, a property from ICoordinate.
		/// </summary>
		public double X { get => Longitude; set => Longitude = value; }

		/// <summary>
		/// North coordinate, in degrees, a property from ICoordinate.
		/// </summary>
		public double Y { get => Latitude; set => Latitude = value; }

		#endregion

		#endregion

		#region WGS84 ellipsoid parameters

		private const double a_ = 6378137.0;
		private const double f_ = 1 / 298.257223563;
		private const double e2_ = 1 - (1 - f_) * (1 - f_);
		private const double tolerance = 1e-12;

		#endregion

		#region Constructors 

		/// <summary>
		/// Default constructor, which exists only to enable automatic serialization.
		/// Sets Latitute, Longitude and Altitude to zero.
		/// </summary>
		public GeoCoordinate()
		{
			Latitude = 0;
			Longitude = 0;
			Altitude = 0;
		}
		/// <summary>
		/// Creates a coordinate at the given latitude, longitude and altitude
		/// </summary>
		/// <param name="latitude">The latitude. Must be in the range [-90, 90]</param>
		/// <param name="longitude">The longitude. Must be in the range [-180, 540) and is normalized to [-180, 180).</param>
		/// <param name="altitude">The elevation from the Earth's surface.</param>
		public GeoCoordinate(double latitude, double longitude, double altitude = double.NaN)
		{
			if (latitude < -90 || latitude > 90)
				throw new ArgumentException("The latitude must be in the range [-90, 90]");

			if (longitude < -180 || longitude >= 540)
				throw new ArgumentException("The longitude must be in the range [-180, 540)");

			if (longitude >= 180)
				longitude -= 360;

			Latitude = latitude;
			Longitude = longitude;
			Altitude = altitude;
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="other"></param>
		public GeoCoordinate(GeoCoordinate other)
		{
			Latitude = other.Latitude;
			Longitude = other.Longitude;
			Altitude = other.Altitude;
		}

		/// <summary>
		/// Creates a geo coordinate from degrees minutes seconds values. Of the digits left of the comma separator,
		/// digits 5 and above represents the integer number of degrees, digit 4 and 3 represents the integer number
		/// of minutes and the remaining digits including right side of the comma separator represents the seconds.
		/// </summary>
		/// <param name="latitude">Latitude in the degminsec format</param>
		/// <param name="longitude">Longitude in the degminsec format</param>
		/// <param name="altitude">Altitude or NaN if unspecified</param>
		/// <returns></returns>
		public static GeoCoordinate CreateGeoCoordinateFromDegMinSec(double latitude, double longitude, double altitude = double.NaN)
		{
			double lat, lon, reminder;

			reminder = latitude;
			lat = (reminder - reminder % 10000) / 10000;
			reminder -= 10000 * lat;
			double lat_min = (reminder - reminder % 100) / 100;
			reminder -= 100 * lat_min;
			lat += lat_min / 60 + reminder / 3600;

			reminder = longitude;
			lon = (reminder - reminder % 10000) / 10000;
			reminder -= 10000 * lon;
			double lon_min = (reminder - reminder % 100) / 100; ;
			reminder -= 100 * lon_min;
			lon += lon_min / 60 + reminder / 3600;

			return new GeoCoordinate(lat, lon, altitude);
		}

		/// <summary>
		/// Converts the given latitude and longitude from degrees to degminsec format (DDMMSS)
		/// </summary>
		public static void ConvertToDegMinSec(double latitude, double longitude, out double dmsLatitude, out double dmsLongitude)
		{
			dmsLatitude = 0;
			dmsLongitude = 0;

			double remainder1 = latitude - Math.Floor(latitude);
			dmsLatitude = Math.Floor(latitude) * 10000;
			remainder1 *= 60;
			double remainder2 = remainder1 - Math.Floor(remainder1);
			dmsLatitude += Math.Floor(remainder1) * 100;
			dmsLatitude += remainder2 * 60;

			remainder1 = longitude - Math.Floor(longitude);
			dmsLongitude = Math.Floor(longitude) * 10000;
			remainder1 *= 60;
			remainder2 = remainder1 - Math.Floor(remainder1);
			dmsLongitude += Math.Floor(remainder1) * 100;
			dmsLongitude += remainder2 * 60;
		}

		/// <summary>
		/// Creates a coordinat by reading the latitude and longitude from the given xml element
		/// </summary>
		public GeoCoordinate(XElement el)
			: this(el.RequireElement("Latitude").ParseDoubleInvariant(), el.RequireElement("Longitude").ParseDoubleInvariant())
		{
			XElement z = el.TagElement("Altitude");
			if (z == null)
				Altitude = double.NaN;
			else
				Altitude = z.ParseDoubleInvariant();
		}

		#endregion

		#region Private and internal utility methods

		/// <summary>
		/// Adjusts a latitude to [-90, 90] by truncation
		/// </summary>
		public static double AdjLat(double lat)
		{
			if (lat < -90)
				return -90;
			if (lat > 90)
				return 90;
			return lat;
		}


		/// <summary>
		/// Adjusts a longitude to [-180, 180)
		/// </summary>
		public static double AdjLon(double lon)
		{
			if (Math.Abs(lon) <= 180) return (lon);
			lon += 180;  /* adjust to 0..2pi rad */
			lon -= 360 * Math.Floor(lon / 360); /* remove integral # of 'revolutions'*/
			lon -= 180;  /* adjust back to -pi..pi rad */
			return (lon);
		}

		private static double N(double latitude)
		{
			latitude = Math.Sin(latitude * Math.PI / 180);
			double W = Math.Sqrt(1 - e2_ * latitude * latitude);
			return a_ / W;
		}

		private void GetCartesianEarth(out double x, out double y, out double z)
		{
			double onef = 1 - f_;
			double n = N(Latitude);
			double coslat = Math.Cos(Latitude * Math.PI / 180);
			double sinlat = Math.Sin(Latitude * Math.PI / 180);
			double coslon = Math.Cos(Longitude * Math.PI / 180);
			double sinlon = Math.Sin(Longitude * Math.PI / 180);
			x = n * coslat * coslon;
			y = n * coslat * sinlon;
			z = n * sinlat * onef * onef;
		}

		/// <summary>
		/// Returns the distance to the other point, in meters.
		/// If setAngles is true, sets az12 and az21 to the azimuths toward the
		/// other point
		/// </summary>
		private double DistanceTo(GeoCoordinate other, out double az12, out double az21, bool setAngles)
		{
			az12 = 0;
			az21 = 0;

			double onef = 1 - f_;
			double f2 = f_ / 2;
			double f4 = f_ / 4;
			double f64 = f_ * f_ / 64;

			double phi1 = Latitude * Math.PI / 180;
			double phi2 = other.Latitude * Math.PI / 180;
			double dlam = AdjLon(other.Longitude - Longitude) * Math.PI / 180;

			double th1 = Math.Atan(onef * Math.Tan(phi1));
			double th2 = Math.Atan(onef * Math.Tan(phi2));

			double thm = .5 * (th1 + th2);
			double dthm = .5 * (th2 - th1);
			double dlamm = .5 * dlam;
			if (dlam == 0 && phi1 == phi2)
				return 0;

			double sindlamm = Math.Sin(dlamm);
			double costhm = Math.Cos(thm);
			double sinthm = Math.Sin(thm);
			double cosdthm = Math.Cos(dthm);
			double sindthm = Math.Sin(dthm);
			double L = sindthm * sindthm + (cosdthm * cosdthm - sinthm * sinthm) * sindlamm * sindlamm;
			if (L == 0)
				return 0;

			double cosd = 1 - L - L;
			double E = cosd + cosd;
			double Y = sinthm * cosdthm;
			Y *= (Y + Y) / (1.0 - L);
			double T0 = sindthm * costhm;
			T0 *= (T0 + T0) / L;
			double X = Y + T0;
			Y -= T0;

			double sind = 2 * Math.Sqrt(L * (1 - L));
			double T;
			if (sind == 0)
				T = 1;
			else if (sind < cosd)
				T = Math.Asin(sind) / sind;
			else
				T = Math.Acos(cosd) / sind;

			double D = 4.0 * T * T;
			double A = D * E;
			double B = D + D;

			if (setAngles)
			{
				double tandlammp = Math.Tan(.5 * (dlam - .25 * (Y + Y - E * (4.0 - X)) *
					(f2 * T + f64 * (32.0 * T - (20.0 * T - A)
					* X - (B + 4.0) * Y)) * Math.Tan(dlam)));
				double u = Math.Atan2(sindthm, (tandlammp * costhm));
				double v = Math.Atan2(cosdthm, (tandlammp * sinthm));
				az12 = AdjLon((Math.PI * 2 + v - u) * 180 / Math.PI);
				az21 = AdjLon((Math.PI * 2 - v - u) * 180 / Math.PI);
			}
			return a_ * sind * (T - f4 * (T * X - Y) +
				 f64 * (X * (A + (T - .5 * (A - E)) * X) -
				 Y * (B + E * Y) + D * X * Y));
		}

		/// <summary>
		/// Returns the distance along a meridian from the equator to the given latitude, in meters.
		/// (Values are negative for the southern hemisophere.)
		/// </summary>
		internal static double MeridianLength(double lat)
		{
			lat *= Math.PI / 180;
			double e4 = e2_ * e2_, e6 = e4 * e2_;
			double sin2lat = Math.Sin(lat * 2);
			double sin4lat = Math.Sin(lat * 4);
			double sin6lat = Math.Sin(lat * 6);
			double mlen = (1 - e2_ / 4 - 3 * e4 / 64 - 5 * e6 / 256) * lat;
			mlen -= (3 * e2_ / 8 + 3 * e4 / 32 + 45 * e6 / 1024) * sin2lat;
			mlen += (15 * e4 / 256 + 45 * e6 / 1024) * sin4lat;
			mlen -= (35 * e6 / 3072) * sin6lat;

			return a_ * mlen;
		}

		/// <summary>
		/// Returns the coordinate's distance from the meridian of the given longitude, in meters.
		/// </summary>
		internal double DistFromMeridian(double lon, out double northing)
		{
			lon = AdjLon(Longitude - lon) * Math.PI / 180;
			double coslat = Math.Cos(Latitude * Math.PI / 180);
			double sinlat = Math.Sin(Latitude * Math.PI / 180);
			double A = lon * coslat;
			double T = sinlat / coslat; T *= T;
			double C = coslat * coslat * e2_ / (1 - e2_);
			double N = a_ / Math.Sqrt(1 - e2_ * sinlat * sinlat);

			{
				northing = MeridianLength(Latitude);
				northing += N * sinlat / coslat * A * A * (0.5 + (5 - T + 6 * C) * A * A / 24);
			}
			return N * (A - T * A * A * A / 6 - (8 - T + 8 * C) * T * A * A * A * A * A / 120);
		}

		/// <summary>
		/// Returns the distance in meters from the Earth center to an Earth surface point at a given latitude
		/// </summary>
		/// <param name="latitude">The latitude to return the Earth center dinstance from</param>
		internal static double DistanceFromEarthCenter(double latitude)
		{
			double sinLat = Math.Sin(latitude * Math.PI / 180);
			return N(latitude) * Math.Sqrt(1 - e2_ * (2 - e2_) * sinLat * sinLat);
		}

		/// <summary>
		/// Helper function for NewInterpolated.
		/// Calculates an approximate result, which can be refined by numerical iteration
		/// </summary>
		private GeoCoordinate InaccurateInterpolated(GeoCoordinate other, double fraction, bool snap)
		{
			if (fraction <= 0)
				return this;

			if (fraction >= 1)
				return other;

			GeoCoordinate pt;
			if (Math.Abs(this.Latitude - other.Latitude) + Math.Abs(this.Longitude - other.Longitude) < 2)
			{
				pt = new GeoCoordinate(this.Latitude * (1 - fraction) + other.Latitude * fraction,
					this.Longitude * (1 - fraction) + other.Longitude * fraction);
			}
			else
			{
				double x1, y1, z1, x2, y2, z2;
				GetCartesianEarth(out x1, out y1, out z1);
				other.GetCartesianEarth(out x2, out y2, out z2);

				// Interpolate 3d vectors linearly
				double xx = (1 - fraction) * x1 + fraction * x2;
				double yy = (1 - fraction) * y1 + fraction * y2;
				double zz = (1 - fraction) * z1 + fraction * z2;

				// Convert back to lat/lon
				double dd = Math.Sqrt(yy * yy + xx * xx);

				double ssinlon = yy / dd;
				double ccoslon = xx / dd;
				double onef = 1 - f_;

				double lon = Math.Atan2(yy, xx) * 180 / Math.PI;
				double lat = Math.Atan2(zz, onef * onef * dd) * 180 / Math.PI;

				pt = new GeoCoordinate(lat, lon);
			}
			// Snap to great circle
			if (snap)
				pt = pt.ClosestPoint(this, other);

			return pt;
		}

		/// <summary>
		/// Helper function for Offset.
		/// Calculates an approximate result, which can be refined by numerical iteration
		/// </summary>
		private GeoCoordinate InaccurateOffset(double distance, double azimuth)
		{
			if (distance == 0)
				return this;

			GeoCoordinate east = new GeoCoordinate(Latitude, Longitude + 1);
			double eastDistance = DistanceTo(east);

			if (distance > eastDistance)
				throw new ArgumentException(string.Format("Cannot offset by more than one degree longitude, which at this latitude ({0}) is {1}m", Latitude, eastDistance));

			GeoCoordinate north = new GeoCoordinate(Latitude + 1, Longitude);

			east = Interpolated_old(east, distance / eastDistance, 1e-6);
			north = Interpolated_old(north, distance / DistanceTo(north), 1e-6);

			double sn = Math.Sin(azimuth * Math.PI / 180);
			double cs = Math.Cos(azimuth * Math.PI / 180);

			return new GeoCoordinate(Latitude + cs * (north.Latitude - Latitude),
													 Longitude + sn * (east.Longitude - Longitude));
		}

		/// <summary>
		/// Returns the coordinate obtained by moving this coordiate
		/// the given fraction of the distance towards the
		/// other coodinate.
		/// This function is an older alternative to Interpolated, but has numeric issues. However, it's kept
		/// because it performs better in the eOffset method.
		/// </summary>
		/// <param name="other">The coordinate to move towards</param>
		/// <param name="fraction">The fraction of the distance to move</param>
		/// <param name="minAccuracy">The maximum error in the fraction moved</param>
		/// <returns></returns>
		private GeoCoordinate Interpolated_old(GeoCoordinate other, double fraction, double minAccuracy)
		{
			if (fraction < 0 || fraction > 1)
				throw new ArgumentException("Interpolation fraction must be between 0 and 1");
			if (minAccuracy <= 0)
				throw new ArgumentException("minAccuracy must be >= 0");

			double dtot = other.DistanceTo(this);
			if (dtot == 0)
				return this;

			double x1, y1, z1, x2, y2, z2;
			GetCartesianEarth(out x1, out y1, out z1);
			other.GetCartesianEarth(out x2, out y2, out z2);

			// Interpolate 3d vectors linearly
			double xx = (1 - fraction) * x1 + fraction * x2;
			double yy = (1 - fraction) * y1 + fraction * y2;
			double zz = (1 - fraction) * z1 + fraction * z2;

			// Convert back to lat/lon
			double dd = Math.Sqrt(yy * yy + xx * xx);

			double ssinlon = yy / dd;
			double ccoslon = xx / dd;
			double onef = 1 - f_;

			double lon = Math.Atan2(yy, xx) * 180 / Math.PI;
			double lat = Math.Atan2(zz, onef * onef * dd) * 180 / Math.PI;

			var pt = new GeoCoordinate(lat, lon);

			// Snap to great circle
			pt = pt.ClosestPoint(this, other);

			double d = pt.DistanceTo(this);
			double actualFrac = d / dtot;

			if (Math.Abs(fraction - actualFrac) > minAccuracy)
			{
				// Secant method
				if (actualFrac > fraction)
					return Interpolated_old(pt, fraction / actualFrac, minAccuracy / actualFrac);
				else
					return other.Interpolated_old(pt, (1 - fraction) / (1 - actualFrac), minAccuracy / (1 - actualFrac));
			}

			// Deviation is within accuracy
			return pt;
		}

		/// <summary>
		/// Calculates the latitude term for a corner used in the calculation of the area of a region.
		/// </summary>
		/// <param name="latitude">The latitude (in radians) to get the latitude term for</param>
		private static double LatitudeAreaTerm(double latitude)
		{
			if (latitude == 0.0)
			{
				return 0.0;
			}

			double result = 0;
			double subtractTerm = 0;
			double nextTerm = 0;
			double sinLat = Math.Sin(latitude);
			double cosLat = Math.Cos(latitude);
			foreach (int k in Enumerable.Range(0, 6))
			{
				if (k == 0)
				{
					nextTerm = (1 - cosLat) / latitude;
					subtractTerm = cosLat / latitude;
				}
				else
				{
					double denom = 1.0 / ((2 * k + 1) * (2 * k + 1));
					subtractTerm *= e2_ * sinLat * sinLat;
					nextTerm *= e2_ * denom * 2 * (k + 1) * (2 * k - 1);
					nextTerm -= subtractTerm * (k + 1) * denom;
				}

				result += nextTerm;
			}

			return result;
		}

		/// <summary>
		/// Calculates the common latitude term for two corner with the same latitude used in the calculation of the area of a region.
		/// </summary>
		/// <param name="latitude">The latitude (in radians) to get the common latitude term for</param>
		private static double CommonLatitudeAreaTerm(double latitude)
		{
			if (latitude == 0.0)
			{
				return 0.0;
			}

			double result = 0;
			double nextTermNumerator = 0;
			double sinLat = Math.Sin(latitude);

			foreach (int k in Enumerable.Range(0, 6))
			{
				if (k == 0)
				{
					nextTermNumerator = sinLat;
				}
				else
				{
					nextTermNumerator *= e2_ * sinLat * sinLat;
				}

				result += nextTermNumerator * (k + 1) / (2 * k + 1);
			}

			result -= LatitudeAreaTerm(latitude);
			return result;
		}

		/// <summary>
		/// Calculates the area in square meters of a region limited by origo (Latitude = Longitude = 0) and two given
		/// corners, and the curves between the corners given as straight lines in the longitude/latitude coordinate system.
		/// The area is scaled to an Earth where the Polar Radius is 1.
		/// If the orientation of the corners is negative, the returned area will also be negative.
		/// </summary>
		/// <param name="corner1">The first of the two given corners</param>
		/// <param name="corner2">The second of the two given corners</param>
		private static double TriangleUnitArea(GeoCoordinate corner1, GeoCoordinate corner2)
		{
			double c1LongRad = corner1.Longitude * Math.PI / 180.0;
			double c1LatRad = corner1.Latitude * Math.PI / 180.0;
			double c2LongRad = corner2.Longitude * Math.PI / 180.0;
			double c2LatRad = corner2.Latitude * Math.PI / 180.0;

			if (c1LatRad == c2LatRad)
			{
				return (c1LongRad - c2LongRad) * CommonLatitudeAreaTerm(c1LatRad);
			}
			else
			{
				return (c1LongRad * c2LatRad - c2LongRad * c1LatRad) * (LatitudeAreaTerm(c2LatRad) - LatitudeAreaTerm(c1LatRad)) / (c2LatRad - c1LatRad);
			}
		}

		#endregion

		#region Public methods

		#region ICoordinateBase implementation

		/// <summary>
		/// Returns the closest point to this coordinate on the segment between the two given coordinates.
		/// </summary>
		/// <returns>The coordinate, as a <see cref="GeoCoordinate "/> reference.</returns>
		public GeoCoordinate ClosestPoint(GeoCoordinate p1, GeoCoordinate p2) => ClosestCoordinate(p1, p2) as GeoCoordinate;

		/// <summary>
		/// Returns a coordinate that is offset from this coordinate by a given distance in
		/// a given direction.
		/// </summary>
		/// <param name="distance">The distance to offset by, in meters</param>
		/// <param name="azimuth">The direction to offset in, as and angle in degrees wrt North (positivy Y-direction). 
		/// North is 0, west -90, east 90 and south 180/-180.</param>
		/// <returns>The coordinate, as a <see cref="GeoCoordinate "/> reference.</returns>
		public GeoCoordinate OffsetBy(double distance, double azimuth) => CoordinateOffsetBy(distance, azimuth) as GeoCoordinate;

		/// <summary>
		/// Returns the coordinate obtained by moving this coordiate
		/// the given fraction of the distance towards the
		/// other coordinate.
		/// </summary>
		/// <param name="other">The coordinate to move towards</param>
		/// <param name="fraction">The fraction of the distance to move</param>
		/// <param name="minAccuracy">The maximum error in the fraction moved</param>
		/// <returns>The coordinate, as a <see cref="GeoCoordinate "/> reference.</returns>
		public GeoCoordinate Interpolated(GeoCoordinate other, double fraction, double minAccuracy) => InterpolatedCoordinate(other, fraction, minAccuracy) as GeoCoordinate;

		#endregion

		/// <summary>
		/// Returns the distance to the other coordinate, in meters.
		/// </summary>
		public double DistanceTo(ICoordinate other)
		{
			if (other.GetType() != GetType())
				throw new Exception($"GeoCoordinate.DistanceTo: argument not of expected type {GetType()}, but of type {other.GetType()}");

			double tmpa, tmpb;
			return DistanceTo(other as GeoCoordinate, out tmpa, out tmpb, false);
		}

		/// <summary>
		/// Test for approximate equality, within the given tolerance. Two coordinates are equal if they are (approximately) at the same position.
		/// I.e., the coordinate in each dimension is equal within the given tolerance (relative or absolute).
		/// </summary>
		/// <param name="other"></param>
		/// <param name="ignoreAltitude">If true, then comparison is done only in the "horizontal"-plane</param>
		/// <param name="maxTolerance"></param>
		public bool EqualsWithTolerance(ICoordinate other, bool ignoreAltitude, double maxTolerance)
		{
			if (other.GetType() != GetType())
				throw new Exception($"GeoCoordinate.EqualsWithTolerance: argument not of expected type {GetType()}, but of type {other.GetType()}");

			GeoCoordinate c2 = other as GeoCoordinate;
			if (c2 == null)
				return false;

			if (!Latitude.EqualsWithTolerance(c2.Latitude, maxTolerance) || !Longitude.EqualsWithTolerance(c2.Longitude, maxTolerance))
				return false;

			if (double.IsNaN(Altitude) && double.IsNaN(c2.Altitude))
				return true;

			if (ignoreAltitude)
				return true;
			else
				return Altitude.EqualsWithTolerance(c2.Altitude, maxTolerance);
		}

		/// <summary>
		/// Returns the distance to the other coordinate
		/// </summary>
		/// <param name="other">The other coordinate.</param>
		/// <param name="az12">Is set to the azimuth at this coordinate toward the other coordinate.
		/// North is 0, west -90, east 90 and south 180/-180.</param>
		/// <param name="az21">Is set to the azimuth toward this coordinate at the other coordinate.
		/// North is 0, west -90, east 90 and south 180/-180.</param>
		public double DistanceTo(GeoCoordinate other, out double az12, out double az21)
		{
			return DistanceTo(other, out az12, out az21, true);
		}

		/// <summary>
		/// Returns the azimuth of a great cicle at a certain point
		/// </summary>
		/// <param name="start">One point on the great circle</param>
		/// <param name="end">Another point on the great circle</param>
		/// <param name="fractionAlongSegment">The position of the point, given by a fraction
		/// of distance from start to end</param>
		/// <returns>The azimuth looking from the point toward end.
		/// North is 0, west -90, east 90 and south 180/-180.</returns>
		public static double AzimuthAtPointOnSegment(GeoCoordinate start, GeoCoordinate end, double fractionAlongSegment)
		{
			GeoCoordinate pointOnSegment = start.Interpolated(end, fractionAlongSegment, 1e-6) as GeoCoordinate;

			double azimuth, dummy;
			if (fractionAlongSegment < 0.5)
				pointOnSegment.DistanceTo(end, out azimuth, out dummy);
			else
			{
				pointOnSegment.DistanceTo(start, out azimuth, out dummy);
				if (azimuth < 0)
					azimuth += 180;
				else
					azimuth -= 180;
			}
			return azimuth;
		}

		/// <summary>
		/// Returns the direction to move the shortest distance from the present coordinate to the <paramref name="target"/> coordinate, in degrees from north.
		/// </summary>
		/// <param name="target">The coordinate moving towards when calculating the direction</param>
		public double StraightLineDirectionTo(ICoordinate target)
		{
			if (target.GetType() != GetType())
				throw new Exception($"Coordinate.StraightLineDirectionTo: argument not of expected type {GetType()}, but of type {target.GetType()}");


			return GeoCoordinate.AzimutFromLongitudeLatitudeStraightLine(this, target as GeoCoordinate);
		}

		/// <summary>
		/// Returns the direction at a given start coordinate of the path on the Earth surface given by
		/// the straight line in the Longitude/Latitude coordinate system towards a given end coordinate.
		/// North is 0, East is 90, West is -90, South is 180 or -180.
		/// </summary>
		/// <param name="start">The coordinate where the direction is calculated</param>
		/// <param name="end">The coordinate moving towards when calculating the direction</param>
		public static double AzimutFromLongitudeLatitudeStraightLine(GeoCoordinate start, GeoCoordinate end)
		{
			double diffLong = end.Longitude - start.Longitude;
			double diffLat = end.Latitude - start.Latitude;
			double latRadian = start.Latitude * Math.PI / 180.0;
			double sinLat = Math.Sin(latRadian);
			double cosLat = Math.Cos(latRadian);

			double azimutRad = Math.Atan2(diffLong * cosLat * (1 - e2_ * sinLat * sinLat), diffLat * (1 - e2_));
			return azimutRad * 180.0 / Math.PI;
		}

		/// <summary>
		/// Returns the closest point to this coordinate on the segment 
		/// of a great circle between the two endpoints (p1, p2)
		/// 
		/// \todo Calculate altitude
		/// 
		/// </summary>
		public ICoordinate ClosestCoordinate(ICoordinate p1, ICoordinate p2) => ClosestProjection(p1, p2, 0).ClosestPoint;

		/// <summary>
		/// Returns the closest point to this coordinate on the segment 
		/// of a great circle between the two endpoints (p1, p2).
		/// 
		/// \todo Calculate altitude
		/// 
		/// </summary>
		/// <param name="p1">The start point of the segment</param>
		/// <param name="p2">The end point of the segment</param>
		/// <param name="tolerance">A tolerance in meters, which is used to decide if borderline projections
		/// fall on, or outside, the segment (interpreted as inside if it is less than <paramref name="tolerance"/> 
		/// meters outside).</param>
		public ProjectionResult ClosestProjection(ICoordinate p1, ICoordinate p2, double tolerance)
		{
			if (p1.GetType() != GetType())
				throw new Exception($"GeoCoordinate.ClosestProjection: argument 1 not of expected type {GetType()}, but of type {p1.GetType()}");
			if (p2.GetType() != GetType())
				throw new Exception($"GeoCoordinate.ClosestProjection: argument 2 not of expected type {GetType()}, but of type {p2.GetType()}");

			double x, y, z, x1, y1, z1, x2, y2, z2;
			GetCartesianEarth(out x, out y, out z);
			(p1 as GeoCoordinate).GetCartesianEarth(out x1, out y1, out z1);
			(p2 as GeoCoordinate).GetCartesianEarth(out x2, out y2, out z2);
			bool outBeforeOrAtBeginning = false;
			bool outAfterOrAtEnd = false;
		
			// vector n = normal to C, the great circle through p1 and p2
			double nx = y1 * z2 - z1 * y2;
			double ny = z1 * x2 - x1 * z2;
			double nz = x1 * y2 - y1 * x2;

			// vector k = normal to great circle through n and p
			double kx = ny * z - nz * y;
			double ky = nz * x - nx * z;
			double kz = nx * y - ny * x;

			double p1zz = kx * x1 + ky * y1 + kz * z1;
			double p2zz = kx * x2 + ky * y2 + kz * z2;
			if (p1zz >= 0 || p2zz <= 0)
			{
				// An endpoint is closest
				if (p2zz > -p1zz)
					outBeforeOrAtBeginning = true;
				else
					outAfterOrAtEnd = true;
			}

			// result vector = normal to great circle through n and k,
			// i.e. a point both on C and on the great circle through p normal to C
			double xx = ky * nz - kz * ny;
			double yy = kz * nx - kx * nz;
			double zz = kx * ny - ky * nx;

			// Convert coordinates
			double dd = Math.Sqrt(yy * yy + xx * xx);

			double ssinlon = yy / dd;
			double ccoslon = xx / dd;
			double onef = 1 - f_;

			double lon = (xx == 0 && yy == 0) ? 0 : Math.Atan2(yy, xx) * 180 / Math.PI;
			double lat = Math.Atan2(zz, onef * onef * dd) * 180 / Math.PI;

			//If the projection falls outside the segment, we note this and return the 
			//corresponding end point. If not, we return the projection as OK.
			//Note that the projection may fall further outside than the distance between the original (this)
			//coordinate and the end point. In this instance we know that the real projection (not suffering our numerical
			//insequirities) is closer to the end point, and we compare that instead with the tolerance to determine if
			//we are really outside (as defined by the tolerance).
			GeoCoordinate candidate = new GeoCoordinate(lat, lon);
			if (outBeforeOrAtBeginning)
			{
				bool outBefore = Math.Min(candidate.DistanceTo(p1), DistanceTo(p1)) > tolerance;
				return new ProjectionResult(p1 as GeoCoordinate, outBefore, false, 0);
			}
			else if (outAfterOrAtEnd)
			{
				bool outafter = Math.Min(candidate.DistanceTo(p2), DistanceTo(p2)) > tolerance;
				return new ProjectionResult(p2 as GeoCoordinate, false, outafter, p1.DistanceTo(p2));
			}
			else
				return new ProjectionResult(candidate, false, false, p1.DistanceTo(candidate));
		}

		/// <summary>
		/// Returns the coordinate obtained by moving this coordiate
		/// the given fraction of the distance towards the
		/// other coordinate.
		/// </summary>
		/// <param name="other">The coordinate to move towards</param>
		/// <param name="fraction">The fraction of the distance to move</param>
		/// <param name="minAccuracy">The maximum error in the fraction moved</param>
		/// <returns></returns>
		public ICoordinate InterpolatedCoordinate(ICoordinate other, double fraction, double minAccuracy)
		{
			if (!(other is GeoCoordinate otherGeo))
				throw new Exception($"GeoCoordinate.InterpolatedCoordinate: argument 1 not of expected type {GetType()}, but of type {other.GetType()}");

			if (fraction < 0 || fraction > 1)
				throw new ArgumentException("Interpolation fraction must be between 0 and 1");
			if (minAccuracy <= 0)
				throw new ArgumentException("minAccuracy must be >= 0");

			if (fraction == 0)
				return this;
			if (fraction == 1)
				return other;

			double dtot = other.DistanceTo(this);
			if (dtot == 0)
				return this;

			GeoCoordinate testPoint = null;

			Func<double, double> distanceErrorForFraction = (testFraction) =>
			{
				testPoint = InaccurateInterpolated(otherGeo, testFraction, dtot > 1000);
				var testDistance = this.DistanceTo(testPoint);
				double actualFraction = testDistance / dtot;
				var diff = actualFraction - fraction;
				return diff;
			};

			double bestFraction = NumericZero.ZeroOf(distanceErrorForFraction, 0, 1.0, valueTolerance: minAccuracy, throwOnFailure: false);

			// Interpolate altitude as well
			if (!double.IsNaN(this.Altitude) && !double.IsNaN(otherGeo.Altitude))
				testPoint.Altitude = (1 - fraction) * this.Altitude + fraction * otherGeo.Altitude;

			return testPoint;
		}
	
		/// <summary>
		/// Returns a coordinate that is offset from this coordinate by a given distance in
		/// a given direction.
		/// The result is accurate only for small distances.
		/// The offset distance cannot be larger than the length of one degree of longitude at this point.
		/// </summary>
		/// <param name="distance">The distance to offset by, in meters</param>
		/// <param name="azimuth">The direction to offset in. North is 0, west -90, east 90 and south 180/-180.</param>
		/// <returns></returns>
		public ICoordinate CoordinateOffsetBy(double distance, double azimuth)
		{
			if (distance == 0)
				return this;

			Func<double, double> offsetError = (testDistance) =>
			{
				var testCoordinate = InaccurateOffset(testDistance, azimuth);
				double foundDistance = DistanceTo(testCoordinate);
				double relativeError = (foundDistance - distance) / distance;
				return relativeError;
			};

			double maxDistanceGuess = distance * 1.1;

			// Handle very small distances, where offsetError will have numerical problems.
			if (offsetError(maxDistanceGuess) < 0)
				return this;

			double bestArgument = NumericZero.ZeroOf(offsetError, 0, maxDistanceGuess, 1e-6, throwOnFailure: false);
			return InaccurateOffset(bestArgument, azimuth);
		}

		/// <summary>
		/// Returns this coordinate as an UTM coordinate in the given zone
		/// </summary>
		public UtmCoordinate ToUtmCoordinate(int utmZone)
		{
			return CoordinateSystem.ToUtmCoordinate(this, utmZone);
		}

		/// <summary>
		/// Tests if the coordinates are the same
		/// </summary>
		/// <param name="other">The other coordinate</param>
		/// <returns>True when the two coordinates are the same</returns>
		public override bool Equals(object other)
		{
			GeoCoordinate geo = other as GeoCoordinate;

			return (this == geo);
		}

		/// <summary>
		/// Tests if the coordinates are the same
		/// </summary>
		public bool Equals(ICoordinate other) => Equals(other as object);

		/// <summary>
		/// Overridden hash function unsing bitwise exclusive-OR on the Latitude and Longitude. Ignores Altitude.
		/// </summary>
		public override int GetHashCode()
		{
			int intLat = (int)Latitude;
			int intLon = (int)Longitude;
			int hashCode = intLat ^ intLon;
			return hashCode;
		}

		/// <summary>
		/// Compares two coordinates for equality. They are equal if both have the same Latitude and Longitude,
		/// and either both have no Altitude (i.e. NaN) or they have the same Altitude.
		/// </summary>
		static public bool operator ==(GeoCoordinate x, GeoCoordinate y)
		{
			if (ReferenceEquals(x, null))
			{
				return ReferenceEquals(y, null);
			}
			if (ReferenceEquals(y, null))
				return false;

			return x.Latitude == y.Latitude
				&& x.Longitude == y.Longitude
				&& (double.IsNaN(x.Altitude) && double.IsNaN(y.Altitude) || x.Altitude == y.Altitude);
		}

		/// <summary>
		/// Compares two coordinates for inequality. Returns the negation of ==.
		/// </summary>
		static public bool operator !=(GeoCoordinate x, GeoCoordinate y)
		{
			return !(x == y);
		}

		/// <summary>
		/// Returns a string description of the coordinate.
		/// Note that the decimal separator depends on the locale. 
		/// </summary>
		public override string ToString()
		{
			return Math.Abs(Latitude).ToString("F2") +
				(Latitude >= 0 ? "N " : "S ") +
				Math.Abs(Longitude).ToString("F2") +
				(Longitude >= 0 ? "E" : "W");
		}

		/// <summary>
		/// Returns a locale-independent string description of the coordinate.
		/// </summary>
		public string ToInvariantString(int decimals = 2)
		{
			string format = $"F{decimals}";
			return Math.Abs(Latitude).ToString(format, CultureInfo.InvariantCulture) +
				(Latitude >= 0 ? "N " : "S ") +
				Math.Abs(Longitude).ToString(format, CultureInfo.InvariantCulture) +
				(Longitude >= 0 ? "E" : "W");
		}

		/// <summary>
		/// Whether the two segments crosses each other or not.
		/// </summary>
		/// <param name="start1">The start coordinate of the first segment</param>
		/// <param name="end1">The end coordinate of the first segment</param>
		/// <param name="start2">The start coordinate of the second segment</param>
		/// <param name="end2">The end coordinate of the second segment</param>
		/// <returns>True when the two segments crosses each other.</returns>
		public static bool Intersects(GeoCoordinate start1, GeoCoordinate end1, GeoCoordinate start2, GeoCoordinate end2)
		{
			// Quasi Cartesian coordinates:
			double quasi = Math.Cos((start1.Latitude + end1.Latitude + start2.Latitude + end2.Latitude) * Math.PI / (180 * 4));
			double px = start1.Latitude;
			double py = start1.Longitude * quasi;
			double rx = end1.Latitude - px;
			double ry = end1.Longitude * quasi - py;
			double qx = start2.Latitude;
			double qy = start2.Longitude * quasi;
			double sx = end2.Latitude - qx;
			double sy = end2.Longitude * quasi - qy;

			double r_cross_s = rx * sy - ry * sx;
			double q_minus_p_x = (qx - px);
			double q_minus_p_y = (qy - py);

			double q_minus_p_cross_s = q_minus_p_x * sy - q_minus_p_y * sx;
			if (r_cross_s == 0)
				return false; // if (q_minus_p_cross_s == 0) then the two lines are collinear otherwise they are parallell

			// t = (q − p) × s / (r × s)
			double t = q_minus_p_cross_s / r_cross_s;

			//u = (q − p) × r / (r × s)
			double q_minus_p_cross_r = q_minus_p_x * ry - q_minus_p_y * rx;
			double u = q_minus_p_cross_r / r_cross_s;

			// if both t and u is in the range <0, 1> then there is an intersection
			double err = 0.001; // necessary because we are using quasi Cartesian coordinates
			return (err < t && t < 1 - err) && (err < u && u < 1 - err);
		}

		/// <summary>
		/// Calculates the area in square meters of a region limited by a sequence of corners and the curves between the
		/// corners given as straight lines in the longitude/latitude coordinate system. If the orientation of the corners
		/// is negative, the returned area will also be negative.
		/// </summary>
		/// <param name="corners">The corners of the region</param>
		public static double SignedArea(IEnumerable<GeoCoordinate> corners)
		{
			double unitPolarRadArea = 0;   // The area, scaled down to an Earth where the Polar Radius is 1.
			List<GeoCoordinate> cornersList = new List<GeoCoordinate>(corners);
			int nmbCorners = cornersList.Count;
			foreach (int i in Enumerable.Range(0, nmbCorners))
			{
				unitPolarRadArea += TriangleUnitArea(cornersList[i], cornersList[(i + 1) % nmbCorners]);
			}
			return a_ * a_ * (1 - e2_) * unitPolarRadArea;
		}

		/// <summary>
		/// Creates and returns an xml representation of this coordinate
		/// </summary>
		public XElement ToXml(string elementName)
		{
			XElement element = new XElement(elementName);
			element.Add(new XElement("Latitude") { Value = Latitude.ToInvariantString() });
			element.Add(new XElement("Longitude") { Value = Longitude.ToInvariantString() });
			if (!double.IsNaN(Altitude))
				element.Add(new XElement("Altitude") { Value = Altitude.ToInvariantString() });
			return element;
		}

		#endregion
	}
}
