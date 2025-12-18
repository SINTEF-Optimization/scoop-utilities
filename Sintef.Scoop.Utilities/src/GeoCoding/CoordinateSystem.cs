//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace Sintef.Scoop.Utilities.GeoCoding
{
	/// <summary>
	/// Creates a local coordinate system, e.g., at the airport.
	/// 
	/// Coordinate conversions:
	/// geo -> xyz: GetCoordinate(GeoCoordinate geo)
	/// utm -> xyz: GetCoordinate(UtmCoordinate utm)
	/// xyz -> geo: GetGeoCoordinate(Coordinate xyz)
	/// utm -> geo: GetGeoCoordinate(UtmCoordinate utm)
	/// xyz -> utm: GetUtmCoordinate(Coordinate xyz)
	/// geo -> utm: GetUtmCoordinate(GeoCoordinate geo)
	/// 
	/// </summary>
	[DataContract]
	public class CoordinateSystem
	{

		#region Private variables

		/// <summary>
		/// The geographical coordinate for the origin of the local XY coordinates, e.g. at an airport.
		/// </summary>
		[DataMember]
		private GeoCoordinate _origin;

		/// <summary>
		/// The UTM zone.
		/// </summary>
		[DataMember]
		private int _utmZone;

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a local coordinate system without a specified origin.
		/// </summary>
		public CoordinateSystem()
			: this(new GeoCoordinate(0, 0))
		{
		}

		/// <summary>
		/// Constructs a local coordinate system with origin at the given geographical coordinate.
		/// </summary>
		/// <param name="origin"></param>
		public CoordinateSystem(GeoCoordinate origin)
		{
			_origin = origin;
			_utmZone = BestUtmZone(origin.Longitude);
		}

		/// <summary>
		/// Constructs a local coordinate system with origin at the given
		/// geographical coordinate and for UTM coordinates in the given utm zone.
		/// </summary>
		/// <param name="origin">The geographical coordinate for the origin of the local coordinate system</param>
		/// <param name="utmZone">The UTM zone</param>
		public CoordinateSystem(GeoCoordinate origin, int utmZone)
		{
			_origin = origin;
			_utmZone = utmZone;
		}

		/// <summary>
		/// Constructs a local coordinate system
		/// </summary>
		/// <param name="element">XElement containing xml representation</param>
		public CoordinateSystem(XElement element)
		{
			_origin = new GeoCoordinate(element.RequireElement("Origin"));
			_utmZone = Convert.ToInt32(element.RequireElement("UtmZone").Value);
		}

		/// <summary>
		/// Creates an UTM coordinate system with no offset (the origin is at UTM 0,0)
		/// </summary>
		/// <param name="zone">The UTM zone</param>
		/// <param name="northernHemisphere">True for the northern hemisphere, false for the southern</param>
		/// <returns>The UTM coordinate system</returns>
		public static CoordinateSystem UTM(int zone, bool northernHemisphere)
		{
			UtmCoordinate origin = new(0, 0, zone, northernHemisphere);
			return new CoordinateSystem(ToGeoCoordinate(origin), zone);
		}

		/// <summary>
		/// Creates a Mercator coordinate system
		/// </summary>
		/// <param name="origin"></param>
		/// <returns></returns>
		public static CoordinateSystem MercatorProjection(GeoCoordinate origin = null)
		{
			if (origin == null)
				origin = new GeoCoordinate(0, 0);
			return new CoordinateSystem(origin, -1);
		}

		#endregion

		#region WGS84 ellipsoid parameters

		private const double a_ = 6378137.0;
		private const double f_ = 1 / 298.257223563;
		private const double e2_ = 1 - (1 - f_) * (1 - f_);
		//private const double tolerance = 1e-12;

		#endregion

		#region Pseudo-Mercator projection

		/// <summary>
		/// Converts a pseudo-Mercator y coordinate to a latitude
		/// </summary>
		public double MercatorYToLatitude(double y)
		{
			return 180.0 / System.Math.PI *
				(2 *
				 System.Math.Atan(
					System.Math.Exp(y * System.Math.PI / 180)) - System.Math.PI / 2);
		}

		/// <summary>
		/// Converts a latitude to a pseudo-Mercator y coordinate
		/// </summary>
		public double LatitudeToMercatorY(double latitude)
		{
			return 180.0 / System.Math.PI *
				System.Math.Log(
					System.Math.Tan(
						System.Math.PI / 4.0 + latitude * (System.Math.PI / 180.0) / 2));
		}

		#endregion

		#region Public methods

		/// <summary>
		/// The geographical coordinate for the origin of the local XY coordinates, e.g. at an airport.
		/// </summary>
		public GeoCoordinate Origin
		{
			get { return _origin; }
		}

		/// <summary>
		/// The UTM zone. -1 = Pseudo-Mercator projection. 1 - 36 = UTM zone
		/// </summary>
		public int UtmZone
		{
			get { return _utmZone; }
			set
			{
				if (value < -1 || value == 0 || value > 60)
					throw new ArgumentException("Illegal UTM zone");
				_utmZone = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool IsMercatorProjection => _utmZone == -1;

		/// <summary>
		/// Returns a representative utm zone.
		/// 
		/// The UTM system divides the surface of Earth between 80°S and 84°N latitude into 60 zones, each 6° of longitude in width.
		/// Zone 1 covers longitude 180° to 174° W; zone numbering increases eastward to zone 60 that covers longitude 174 to 180 East.
		/// </summary>
		public static int BestUtmZone(double longitude)
		{
			return (int)(GeoCoordinate.AdjLon(longitude) + 186) / 6;
		}

		/// <summary>
		/// Returns a coordinate of the generic type, based  based on the given coordinate.
		/// </summary>
		/// <param name="coordinate"></param>
		/// <returns></returns>
		public C GetCoordinateOfType<C>(ICoordinate coordinate) where C : ICoordinate
		{
			Type typc = typeof(C);
			if (typc != typeof(Coordinate) && typc != typeof(GeoCoordinate))
				throw new ArgumentException($"Unexpected coordinate type {typc}");

			if (coordinate is Coordinate cartesian)
			{
				if (typc == typeof(Coordinate))
					return (C)coordinate;
				else if (typc == typeof(GeoCoordinate))
					return (C)(ICoordinate)GetGeoCoordinate(cartesian);
			}
			else if (coordinate is GeoCoordinate geo)
			{
				if (typc == typeof(Coordinate))
					return (C)(ICoordinate)GetCoordinate(geo);
				else if (typc == typeof(GeoCoordinate))
					return (C)coordinate;
			}
			else
				throw new ArgumentException($"Unexpected coordinate type {coordinate.GetType()}");

			return default;
		}


		/// <summary>
		/// Returns a cartesian <see cref="Coordinate"/> based on the given coordinate.
		/// </summary>
		/// <param name="coordinate"></param>
		/// <returns></returns>
		public Coordinate GetCoordinate(ICoordinate coordinate)
		{
			if (coordinate is Coordinate cartesian)
				return cartesian;
			else if (coordinate is GeoCoordinate geo)
				return GetCoordinate(geo);
			else
				throw new ArgumentException($"Unexpected coordinate type {coordinate.GetType()}");
		}

		/// <summary>
		/// Returns a <see cref="GeoCoordinate"/> based on the given coordinate.
		/// </summary>
		/// <param name="coordinate"></param>
		/// <returns></returns>
		public GeoCoordinate GetGeoCoordinate(ICoordinate coordinate)
		{
			if (coordinate is Coordinate cartesian)
				return GetGeoCoordinate(cartesian);
			else if (coordinate is GeoCoordinate geo)
				return geo;
			else
				throw new ArgumentException($"Unexpected coordinate type {coordinate.GetType()}");
		}

		/// <summary>
		/// Returns the local coordinate corrsponding to the geographical coordinate
		/// </summary>
		public Coordinate GetCoordinate(GeoCoordinate geo)
		{
			var utmZone = IsMercatorProjection ? BestUtmZone(Origin.Longitude) : _utmZone;
			UtmCoordinate utm = GetUtmCoordinate(geo, utmZone);
			Coordinate xyz = GetCoordinate(utm);
			xyz.Z = geo.Altitude;
			return xyz;
		}

		/// <summary>
		/// Returns the local coordinate corrsponding to the utm coordinate
		/// </summary>
		public Coordinate GetCoordinate(UtmCoordinate utm)
		{
			UtmCoordinate orig = GetUtmCoordinate(_origin, utm.Zone);
			return new Coordinate(utm.Easting - orig.Easting, utm.SignedNorthing - orig.SignedNorthing);
		}

		/// <summary>
		/// Returns the geographical corrsponding to the local coordinate
		/// </summary>
		public GeoCoordinate GetGeoCoordinate(Coordinate xyz)
		{
			var utmZone = IsMercatorProjection ? BestUtmZone(Origin.Longitude) : _utmZone;
			UtmCoordinate utm = GetUtmCoordinate(xyz, utmZone);
			GeoCoordinate geo = GetGeoCoordinate(utm, xyz.Z);
			return geo;
		}

		/// <summary>
		/// Converts a sequence of coordinates to a sequence of geocoordinates
		/// </summary>
		public IEnumerable<GeoCoordinate> ToGeo(IEnumerable<Coordinate> coordinates)
		{
			foreach (var c in coordinates)
				yield return GetGeoCoordinate(c);
		}

		/// <summary>
		/// Returns the geographical corrsponding to the utm coordinate
		/// Deprecated -- use the static ToGeoCoordinate instead.
		/// </summary>
		public GeoCoordinate GetGeoCoordinate(UtmCoordinate utm, double altitude = double.NaN)
		{
			return ToGeoCoordinate(utm, altitude);
		}

		/// <summary>
		/// Returns the geographical corrsponding to the utm coordinate, optionally
		/// with a given altitude
		/// </summary>
		/// <param name="utm">The UTM coordinate</param>
		/// <param name="altitude">The altitude, if given</param>
		public static GeoCoordinate ToGeoCoordinate(UtmCoordinate utm, double altitude = double.NaN)
		{
			double k0 = 0.9996;
			double a = a_;
			double eccSquared = e2_;
			double eccPrimeSquared;
			double e1 = (1 - Math.Sqrt(1 - eccSquared)) / (1 + Math.Sqrt(1 - eccSquared));
			double N1, T1, C1, R1, D, M;
			double longOrigin;
			double mu, phi1Rad;
			double x, y;
			double latitude;
			double longitude;

			x = utm.Easting - 500000.0; //remove 500,000 meter offset for longitude
			y = utm.Northing;

			if (!utm.NorthernHemisphere)
				y -= 10000000.0;//remove 10,000,000 meter offset used for southern hemisphere

			longOrigin = (utm.Zone - 1) * 6 - 180 + 3;  //+3 puts origin in middle of zone

			eccPrimeSquared = (eccSquared) / (1 - eccSquared);

			M = y / k0;
			mu = M / (a * (1 - eccSquared / 4 - 3 * eccSquared * eccSquared / 64 - 5 * eccSquared * eccSquared * eccSquared / 256));

			phi1Rad = mu + (3 * e1 / 2 - 27 * e1 * e1 * e1 / 32) * Math.Sin(2 * mu)
						+ (21 * e1 * e1 / 16 - 55 * e1 * e1 * e1 * e1 / 32) * Math.Sin(4 * mu)
						+ (151 * e1 * e1 * e1 / 96) * Math.Sin(6 * mu);
			//phi1 = phi1Rad * 180 / Math.PI;

			N1 = a / Math.Sqrt(1 - eccSquared * Math.Sin(phi1Rad) * Math.Sin(phi1Rad));
			T1 = Math.Tan(phi1Rad) * Math.Tan(phi1Rad);
			C1 = eccPrimeSquared * Math.Cos(phi1Rad) * Math.Cos(phi1Rad);
			R1 = a * (1 - eccSquared) / Math.Pow(1 - eccSquared * Math.Sin(phi1Rad) * Math.Sin(phi1Rad), 1.5);
			D = x / (N1 * k0);

			latitude = phi1Rad - (N1 * Math.Tan(phi1Rad) / R1) * (D * D / 2 - (5 + 3 * T1 + 10 * C1 - 4 * C1 * C1 - 9 * eccPrimeSquared) * D * D * D * D / 24
							+ (61 + 90 * T1 + 298 * C1 + 45 * T1 * T1 - 252 * eccPrimeSquared - 3 * C1 * C1) * D * D * D * D * D * D / 720);
			latitude = GeoCoordinate.AdjLat(latitude * 180 / Math.PI);

			longitude = (D - (1 + 2 * T1 + C1) * D * D * D / 6 + (5 - 2 * C1 + 28 * T1 - 3 * C1 * C1 + 8 * eccPrimeSquared + 24 * T1 * T1)
							* D * D * D * D * D / 120) / Math.Cos(phi1Rad);
			longitude = GeoCoordinate.AdjLon(longOrigin + longitude * 180 / Math.PI);

			return new GeoCoordinate(latitude, longitude, altitude);
		}

		/// <summary>
		/// Returns the UTM corrsponding to the local coordinate
		/// </summary>
		public UtmCoordinate GetUtmCoordinate(Coordinate xyz) => GetUtmCoordinate(xyz, _utmZone);

		/// <summary>
		/// Returns the UTM corrsponding to the local coordinate
		/// </summary>
		public UtmCoordinate GetUtmCoordinate(Coordinate xyz, int utmZone)
		{
			UtmCoordinate orig = GetUtmCoordinate(_origin, utmZone);
			return new UtmCoordinate(orig.Easting + xyz.X, orig.Northing + xyz.Y, utmZone, orig.NorthernHemisphere);
		}

		/// <summary>
		/// Returns the UTM corrsponding to the geographical coordinate
		/// </summary>
		public UtmCoordinate GetUtmCoordinate(GeoCoordinate geo)
		{
			return GetUtmCoordinate(geo, _utmZone);
		}

		/// <summary>
		/// Returns the UTM corrsponding to the geographical coordinate.
		/// Deprecated -- use the static version instead.
		/// </summary>
		public UtmCoordinate GetUtmCoordinate(GeoCoordinate geo, int utmZone)
		{
			return ToUtmCoordinate(geo, utmZone);
		}

		/// <summary>
		/// Returns the UTM corrsponding to the geographical coordinate
		/// </summary>
		public static UtmCoordinate ToUtmCoordinate(GeoCoordinate geo, int utmZone)
		{
			double easting;
			double northing;
			bool northHemisphere;

			double a = a_;
			double eccSquared = e2_;
			double k0 = 0.9996;

			double eccPrimeSquared;
			double N, T, C, A, M;

			double LatRad = geo.Latitude * Math.PI / 180;
			//double LongRad = geo.Longitude * Math.PI / 180;

			double LongOrigin = (utmZone - 1) * 6 - 180 + 3;  //+3 puts origin in middle of zone
			double LongDelta = GeoCoordinate.AdjLon(geo.Longitude - LongOrigin);
			double LongDeltaRad = LongDelta * Math.PI / 180;

			eccPrimeSquared = (eccSquared) / (1 - eccSquared);

			N = a / Math.Sqrt(1 - eccSquared * Math.Sin(LatRad) * Math.Sin(LatRad));
			T = Math.Tan(LatRad) * Math.Tan(LatRad);
			C = eccPrimeSquared * Math.Cos(LatRad) * Math.Cos(LatRad);
			A = Math.Cos(LatRad) * LongDeltaRad;

			M = a * ((1 - eccSquared / 4 - 3 * eccSquared * eccSquared / 64 - 5 * eccSquared * eccSquared * eccSquared / 256) * LatRad
						- (3 * eccSquared / 8 + 3 * eccSquared * eccSquared / 32 + 45 * eccSquared * eccSquared * eccSquared / 1024) * Math.Sin(2 * LatRad)
											+ (15 * eccSquared * eccSquared / 256 + 45 * eccSquared * eccSquared * eccSquared / 1024) * Math.Sin(4 * LatRad)
											- (35 * eccSquared * eccSquared * eccSquared / 3072) * Math.Sin(6 * LatRad));

			easting = (double)(k0 * N * (A + (1 - T + C) * A * A * A / 6
							+ (5 - 18 * T + T * T + 72 * C - 58 * eccPrimeSquared) * A * A * A * A * A / 120)
							+ 500000.0);

			northing = (double)(k0 * (M + N * Math.Tan(LatRad) * (A * A / 2 + (5 - T + 9 * C + 4 * C * C) * A * A * A * A / 24
						 + (61 - 58 * T + T * T + 600 * C - 330 * eccPrimeSquared) * A * A * A * A * A * A / 720)));
			if (geo.Latitude < 0)
			{
				northing += UtmCoordinate.SouthernHemisphereOffset; //10000000 meter offset for southern hemisphere
				northHemisphere = false;
			}
			else
				northHemisphere = true;

			return new UtmCoordinate(easting, northing, utmZone, northHemisphere);
		}

		/// <summary>
		/// Returns an xml representation of this system
		/// </summary>
		public XElement ToXml(string elementName)
		{
			XElement element = new(elementName);
			element.Add(Origin.ToXml("Origin"));
			XElement xZone = new("UtmZone")
			{
				Value = UtmZone.ToString()
			};
			element.Add(xZone);
			return element;
		}


		#endregion

	}
}
