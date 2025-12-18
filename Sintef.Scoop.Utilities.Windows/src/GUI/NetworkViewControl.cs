//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Sintef.Scoop.Utilities.GeoCoding;
using System;
using System.Drawing;


namespace Sintef.Scoop.Utilities.GUI
{
	/// <summary>
	/// A control for viewing geographical data.
	/// 
	/// The control uses three coordinate systems:
	///  - Geographical. A spherical coordinate system that represents points on the Earth using lat/lon in the Coordinate struct
	///  - Map. Euclidean. This is a planar projection of geographical coordinates, using
	///    either UTM or a Pseudo-Mercator projection
	///  - View. Euclidean. This is the Windows-defined coordinate system of the view panel. The unit is pixels.
	///    (0, 0) is in the upper left corner of the view, and the Y axis increases downward.
	///  
	/// Conversion between the map and view coordinate systems is a simple scale/translate/rotate
	/// operation.
	/// </summary>
	public partial class NetworkViewControl : NetworkViewControlGeneric<GeoCoordinate>
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public NetworkViewControl() : base()
		{
		}

		/// <summary>
		/// Returns the number of pixels per degree latitude at the view center
		/// </summary>
		public override double PixelsPerYUnit()
		{
			GeoCoordinate c1 = FromView(ViewCenter);
			double latitude = c1.Y + 1;
			if (latitude > 50)
				latitude -= 2;
			GeoCoordinate c2 = new GeoCoordinate(latitude, c1.X);

			return ViewDistance(c1, c2);
		}

		#region Coordinate conversion

		/// <summary>
		/// Converts from geographical to map coordinates
		/// </summary>
		public override void ToMap(GeoCoordinate coordinate, out double mapX, out double mapY)
		{
			if (!(coordinate is GeoCoordinate geo))
				throw new ArgumentException($"Expected GeoCoordinate, got {coordinate.GetType()}");

			if (CoordinateSystem == null)
			{
				mapX = geo.X;
				mapY = geo.Y;
			}
			else if (CoordinateSystem.IsMercatorProjection)
			{
				mapX = geo.X;
				mapY = CoordinateSystem.LatitudeToMercatorY(geo.Latitude);
			}
			else
			{
				UtmCoordinate utmCoordinate = CoordinateSystem.GetUtmCoordinate(geo);
				mapX = utmCoordinate.Easting;
				mapY = utmCoordinate.SignedNorthing;
			}
		}

		/// <summary>
		/// Converts from map to geographical coordinates
		/// </summary>
		protected override GeoCoordinate FromMap(double mapX, double mapY)
		{
			if (CoordinateSystem == null)
			{
				return new GeoCoordinate(mapY, mapX);
			}
			if (CoordinateSystem.IsMercatorProjection)
			{
				return new GeoCoordinate(CoordinateSystem.MercatorYToLatitude(mapY), mapX);
			}
			return UtmCoordinate.FromSignedNorthing(mapX, mapY, CoordinateSystem.UtmZone).ToGeoCoordinate();
		}

		#endregion


		/// <summary>
		/// Converts from view to geographical coordinates
		/// </summary>
		public new GeoCoordinate FromView(PointF point) => base.FromView(point) as GeoCoordinate;
	}
}

