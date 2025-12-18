//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sintef.Scoop.Utilities.GeoCoding;
using System;


namespace Sintef.Scoop.Utilities.Tests
{
	[TestClass]
	public class TestWebMercator
	{
		int zoom;
		double maxLat = WebMercatorProjection.MaxLatitude;
		double maxLonCenter { get { return 180 - PixelSizeInDegreesLong / 2; } }
		double maxLonRounded { get { return 180 - PixelSizeInDegreesLong; } }
		int pixelMax { get { return (int)(256 * Math.Pow(2, zoom)); } }
		int pixelCenter { get { return (int)(128 * Math.Pow(2, zoom)); } }

		double PixelSizeInDegreesLong { get { return 360 / (256 * Math.Pow(2, zoom)); } }
		double Tolerance { get { return PixelSizeInDegreesLong / 100; } }

		[TestMethod]
		public void TestForwardProjection()
		{
			foreach (var z in new[] { 0, 1, 17, 18 })
			{
				zoom = z;
				CheckForwardProjection(0, 0, pixelCenter, pixelCenter);
				CheckForwardProjection(-180, maxLat, 0, 0);
				CheckForwardProjection(-180, -maxLat, 0, pixelMax);
				CheckForwardProjection(maxLonCenter, -maxLat, pixelMax - 1, pixelMax);
				CheckForwardProjection(maxLonCenter, maxLat, pixelMax - 1, 0);
			}
		}

		[TestMethod]
		public void TestReverseProjection()
		{
			foreach (var z in new[] { 0, 1, 17, 18 })
			{
				zoom = z;
				CheckReverseProjection(0, 0, pixelCenter, pixelCenter);
				CheckReverseProjection(-180, maxLat, 0, 0);
				CheckReverseProjection(-180, -maxLat, 0, pixelMax);
				CheckReverseProjection(maxLonRounded, -maxLat, pixelMax - 1, pixelMax);
				CheckReverseProjection(maxLonRounded, maxLat, pixelMax - 1, 0);
			}
		}

		[TestMethod]
		public void TestTileCornerCoordinate()
		{
			zoom = 0;
			CheckTileCorner(-180, -maxLat, 0, 0);

			zoom = 1;
			CheckTileCorner(0, 0, 1, 0);

			zoom = 10;
			// Oslo bottom left
			CheckTileCorner(10.546875, 59.88894, 542, 297);
			// Oslo top right
			CheckTileCorner(10.898438, 60.06484, 543, 296);

		}

		private void CheckTileCorner(double lon, double lat, int tileX, int tileY)
		{
			var tile = new WebMercatorTile(zoom, tileX, tileY);
			var coordinate = tile.BottomLeft;
			Assert.AreEqual(lon, coordinate.Longitude, Tolerance);
			Assert.AreEqual(lat, coordinate.Latitude, Tolerance);
		}

		private void CheckReverseProjection(double lon, double lat, int x, int y)
		{
			var coordinate = new GeoCoordinate(lat, lon);
			GeoCoordinate coord = WebMercatorProjection.ProjectReverse(zoom, x, y);

			Assert.AreEqual(lon, coord.Longitude, Tolerance);
			Assert.AreEqual(lat, coord.Latitude, Tolerance);
		}

		private void CheckForwardProjection(double lon, double lat, int xx, int yy)
		{
			var coordinate = new GeoCoordinate(lat, lon);
			int x, y;
			WebMercatorProjection.Project(coordinate, zoom, out x, out y);
			Assert.AreEqual(xx, x);
			Assert.AreEqual(yy, y);
		}
	}
}

