//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Linq;


namespace Sintef.Scoop.Utilities.GeoCoding
{
	/// <summary>
	/// References a tile in a tiled Web Mercator projection
	/// </summary>
	public struct WebMercatorTile
	{
		static int tileSize = 256;

		/// <summary>
		/// The zoom level (0-19)
		/// </summary>
		public int ZoomLevel;

		/// <summary>
		/// The X tile coordinate
		/// </summary>
		public int X;

		/// <summary>
		/// The X tile coordinate
		/// </summary>
		public int Y;

		/// <summary>
		/// Creates a tile with the given zoom level, x and y
		/// </summary>
		public WebMercatorTile(int zoomLevel, int x, int y)
		{
			ZoomLevel = zoomLevel;
			X = x;
			Y = y;
		}

		/// <summary>
		/// Returns the geocoordinate of the tile's top left corner
		/// </summary>
		public GeoCoordinate TopLeft
		{
			get { return WebMercatorProjection.ProjectReverse(ZoomLevel, tileSize * X, tileSize * Y); }
		}

		/// <summary>
		/// Returns the geocoordinate of the tile's top right corner
		/// </summary>
		public GeoCoordinate TopRight
		{
			get { return WebMercatorProjection.ProjectReverse(ZoomLevel, tileSize * (X + 1), tileSize * Y); }
		}

		/// <summary>
		/// Returns the geocoordinate of the tile's bottom left corner
		/// </summary>
		public GeoCoordinate BottomLeft
		{
			get { return WebMercatorProjection.ProjectReverse(ZoomLevel, tileSize * X, tileSize * (Y + 1)); }
		}

		/// <summary>
		/// Returns the geocoordinate of the tile's bottom right corner
		/// </summary>
		public GeoCoordinate BottomRight
		{
			get { return WebMercatorProjection.ProjectReverse(ZoomLevel, tileSize * (X + 1), tileSize * (Y + 1)); }
		}

		/// <summary>
		/// Returns the geocoordinate of the tile's center
		/// </summary>
		public GeoCoordinate Center
		{
			get { return TopRight.Interpolated(BottomLeft, 0.5, 0.01); }
		}

		/// <summary>
		/// Returns the tile that covers the given geocoordinate at the given zoom level
		/// </summary>
		public static WebMercatorTile? TileThatCovers(int zoomLevel, GeoCoordinate point)
		{
			if (point.Latitude.IsNanOrInfinity() || point.Longitude.IsNanOrInfinity())
				return null;

			int x, y;
			WebMercatorProjection.Project(point, zoomLevel, out x, out y);

			x /= tileSize;
			y /= tileSize;

			return new WebMercatorTile(zoomLevel, x, y);
		}

		/// <summary>
		/// Returns the tiles that cover the rectangle over the given geocoordinates at the given zoom level
		/// </summary>
		public static List<WebMercatorTile> TilesThatCover(int zoomLevel, IEnumerable<GeoCoordinate> points, int maxTiles = int.MaxValue)
		{
			List<WebMercatorTile> result = new List<WebMercatorTile>();

			var tiles = points.Select(p => TileThatCovers(zoomLevel, p))
				.Where(t => t != null)
				.Select(t => t.Value);

			if (!tiles.Any())
				return new List<WebMercatorTile>();

			int x1 = tiles.Min(t => t.X);
			int x2 = tiles.Max(t => t.X);
			int y1 = tiles.Min(t => t.Y); 
			int y2 = tiles.Max(t => t.Y);

			for (int x = x1; x <= x2; ++x)
			{
				for (int y = y1; y <= y2; ++y)
				{
					result.Add(new WebMercatorTile(zoomLevel, x, y));

					if (result.Count >= maxTiles)
						return result;
				}
			}

			return result;
		}
	}

	/// <summary>
	/// Converts between geocoordinates and Web Mercator coordinates
	/// </summary>
	public class WebMercatorProjection
	{
		/// <summary>
		/// The maximum latitude covered by the projection. 
		/// </summary>
		public const double MaxLatitude  = 85.05112878;

		/// <summary>
		/// Converts a geocoordinate to Web Mercator coordinates
		/// </summary>
		/// <param name="coordinate">The coordinate to convert</param>
		/// <param name="zoomLevel">The zoom level</param>
		/// <param name="x">Is set to the Web Mercator x coordinate</param>
		/// <param name="y">Is set to the Web Mercator y coordinate</param>
		public static void Project(GeoCoordinate coordinate, int zoomLevel, out int x, out int y)
		{
			double boundedLatitude = Math.Min(Math.Max(coordinate.Latitude, -MaxLatitude), MaxLatitude);

			double scale = Math.Pow(2, zoomLevel);

			double lon = coordinate.Longitude * Math.PI / 180;
			double lat = boundedLatitude * Math.PI / 180;

			x = (int)(128 * scale / Math.PI * (lon + Math.PI));
			y = (int)(128 * scale / Math.PI * (Math.PI - Math.Log(Math.Tan(Math.PI / 4 + lat / 2))));
		}

		/// <summary>
		/// Converts Web Mercator coordinates to geocoordinates
		/// </summary>
		/// <param name="zoomLevel">The zoom level</param>
		/// <param name="x">The Web Mercator x coordinate</param>
		/// <param name="y">The Web Mercator y coordinate</param>
		/// <returns>The geocoordinate</returns>
		public static GeoCoordinate ProjectReverse(int zoomLevel, int x, int y)
		{
			double scale = Math.Pow(2, zoomLevel);

			double g = y * Math.PI / 128 / scale;
			double f = x * Math.PI / 128 / scale;
			double i = Math.PI - g;
			double h = Math.Atan(Math.Exp(i));

			double lon = f - Math.PI;
			double lat = h * 2 - Math.PI / 2;

			return new GeoCoordinate(lat * 180 / Math.PI, lon * 180 / Math.PI);
		}
	}
}
