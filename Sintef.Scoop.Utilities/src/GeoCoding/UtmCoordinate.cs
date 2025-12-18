//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.ComponentModel;

namespace Sintef.Scoop.Utilities.GeoCoding
{
	/// <summary>
	/// The UTM system is not a single map projection. The system instead divides the Earth into sixty zones, 
	/// each a six-degree band of longitude, and uses a secant transverse Mercator projection in each zone.
	/// </summary>
	[TypeConverter(typeof(GenericObjectConverter<UtmCoordinate>))]
	public struct UtmCoordinate
	{
		/// <summary>
		/// 10000000 meter offset for southern hemisphere
		/// </summary>
		public static double SouthernHemisphereOffset = 10000000.0; 

		#region Public properties

		/// <summary>
		/// Easting refers to the eastward-measured distance (or the x-coordinate)
		/// </summary>
		public double Easting { get; set; }

		/// <summary>
		/// If the coordinate is on the norhtern hemisphere, this is the distance from the equator 
		/// (or the y-coordinate). If it is on the southern hemisphere (<see cref="NorthernHemisphere"/> = false),
		/// this is 10 000 000 - the distance from the equator.
		/// </summary>
		public double Northing { get; set; }

		/// <summary>
		/// Northing is the northward-measured distance from the equator (or the y-coordinate).
		/// It is negative if the coordinate is south of the equator (<see cref="NorthernHemisphere"/> = false).
		/// </summary>
		public double SignedNorthing
		{
			get
			{
				if (NorthernHemisphere)
					return Northing;
				else
					return Northing - SouthernHemisphereOffset;
			}
		}


		/// <summary>
		/// The UTM Zone
		/// </summary>
		public int Zone { get; set; }

		/// <summary>
		/// True if the coordinate is in the northern hemisphere
		/// </summary>
		public bool NorthernHemisphere { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a coordinate
		/// </summary>
		/// <param name="easting">The easting</param>
		/// <param name="northing">The northing</param>
		/// <param name="utmZone">The UTM Zone</param>
		/// <param name="northernHemisphere">True if the coordinate is in the northern hemisphere</param>
		public UtmCoordinate(double easting, double northing, int utmZone, bool northernHemisphere)
			: this()
		{
			Easting = easting;
			Northing = northing;
			Zone = utmZone;
			NorthernHemisphere = northernHemisphere;

			/*  if (easting < 0 || easting > 1000000)
					throw BtException("Illegal UTM easting (must be 0 - 1 000 000)");
				if (northing < 0 || northing > 10000000)
					throw BtException("Illegal UTM northing (must be 0 - 10 000 000)");*/

			if (Zone < 1 || Zone > 60)
				throw new ArgumentException("Illegal UTM zone (must be 1 - 60)");
		}

		/// <summary>
		/// Initializes a coordinate
		/// </summary>
		/// <param name="easting">The easting</param>
		/// <param name="signedNorthing">The signed northing. Positive for a point on the northern hemisphere, 
		///   negative for the southers hemisphere.</param>
		/// <param name="utmZone">The UTM Zone</param>
		public static UtmCoordinate FromSignedNorthing(double easting, double signedNorthing, int utmZone)
		{
			if (signedNorthing > 0)
				return new UtmCoordinate(easting, signedNorthing, utmZone, true);
			else
				return new UtmCoordinate(easting, signedNorthing + SouthernHemisphereOffset, utmZone, false);
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Returns this coordinate as a lat/lon coordinate
		/// </summary>
		public GeoCoordinate ToGeoCoordinate()
		{
			return CoordinateSystem.ToGeoCoordinate(this);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return ToString("F2");
		}

		/// <summary>
		/// Returns a string representation of the coordinate, using the given number format
		/// </summary>
		public string ToString(string numberFormat)
		{
			return Zone.ToString() + ' ' + Easting.ToString(numberFormat) + ' ' + Northing.ToString(numberFormat);
		}

		#endregion

	}
}
