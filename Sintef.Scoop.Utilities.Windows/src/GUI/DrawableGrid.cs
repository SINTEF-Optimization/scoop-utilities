//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Sintef.Scoop.Utilities.GeoCoding;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Sintef.Scoop.Utilities.GUI
{
	/// <summary>
	/// A drawable lat/lon grid
	/// </summary>
	public class DrawableGrid : DrawableItem<GeoCoordinate>
	{
		/// <summary>
		/// The colour of the grid
		/// </summary>
		public Color Colour { get; set; }
		/// <summary>
		/// The font of the text
		/// </summary>
		public Font Font { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public DrawableGrid(int layer)
			: base(layer)
		{
			Colour = Color.LightGray;
			Font = new Font("Arial", 8, FontStyle.Bold); //Font = SystemFonts.SmallCaptionFont;
		}

		/// <summary>
		/// Draws a latitude/longitude grid in the view.
		/// The grid covers the extent of the other drawable objects.
		/// </summary>
		public override void Draw(NetworkViewControlGeneric<GeoCoordinate> view)
		{
			try
			{
				if (!(view is NetworkViewControl nvc))
					throw new ArgumentException($"Expected control of type NetworkViewControl");

				// Resolution depending on zoom factor:
				double pixelsPerLatitude = nvc.PixelsPerYUnit();
				int dec = (int)(1 - Math.Log10(600 / pixelsPerLatitude));
				if (dec < 0 || 1 < dec)
					return;
				double stp = Math.Pow(10, -dec);

				// where to print text
				double halfStp = stp / 2;
				double minLat, maxLat, minLon, maxLon;
				view.Extent(out minLat, out maxLat, out minLon, out maxLon);
				double midLat = Math.Round((minLat + maxLat + halfStp) / 2, dec) - halfStp;
				double midLon = Math.Round((minLon + maxLon + halfStp) / 2, dec) - halfStp;

				// Get the extent of the view
				GeoCoordinate[] corners =
				{
					view.FromView(new PointF(0, 0)),
					view.FromView(new PointF(0, view.Height)),
					view.FromView(new PointF(view.Width, 0)),
					view.FromView(new PointF(view.Width, view.Height)) as GeoCoordinate,
					view.FromView(new PointF(view.Width/2, 0)) as GeoCoordinate,
					view.FromView(new PointF(view.Width, view.Height/2)) as GeoCoordinate,
					view.FromView(new PointF(view.Width/2, view.Height)) as GeoCoordinate,
					view.FromView(new PointF(0, view.Height/2)) as GeoCoordinate
				};
				double viewMinLat = corners.Min(x => x.Latitude);
				double viewMaxLat = corners.Max(x => x.Latitude);
				double viewMinLon = corners.Min(x => x.Longitude);
				double viewMaxLon = corners.Max(x => x.Longitude);
				double firstLat = Math.Round(viewMinLat, dec);
				double firstLon = Math.Round(viewMinLon, dec);
				double lastLat = viewMaxLat + stp;
				double lastLon = viewMaxLon + stp;

				// Draw horizontal lines
				GeoCoordinate c1, c2;
				Pen myPen = new Pen(Colour);
				for (double lat = firstLat; lat < lastLat; lat += stp)
				{
					c1 = new GeoCoordinate(lat, midLon);
					string txt = Math.Abs(lat).ToString("F" + dec) + char.ConvertFromUtf32(176);
					view.DrawText(txt, c1, Colour, Font, new SizeF(0, -13));
					c1 = new GeoCoordinate(lat, firstLon - stp);
					for (double lon = firstLon; lon < lastLon; lon += stp)
					{
						c2 = new GeoCoordinate(lat, lon);
						view.DrawLine(c1, c2, myPen);
						c1 = c2;
					}
				}

				// Draw vertical lines
				for (double lon = firstLon; lon < lastLon; lon += stp)
				{
					c1 = new GeoCoordinate(midLat, lon);
					string txt = Math.Abs(lon).ToString("F" + dec) + char.ConvertFromUtf32(176);
					view.DrawText(txt, c1, Colour, Font, new SizeF(-10, 2));
					c1 = new GeoCoordinate(GeoCoordinate.AdjLat(firstLat - stp), lon);
					for (double lat = firstLat; lat < lastLat; lat += stp)
					{
						c2 = new GeoCoordinate(lat, lon);
						view.DrawLine(c1, c2, myPen);
						c1 = c2;
					}
				}

				myPen.Dispose();
			}
			catch (ArgumentException)
			{ }
		}

		/// <summary>
		/// The grid does not return any extent
		/// </summary>
		public override IEnumerable<GeoCoordinate> Extent
		{
			get
			{
				yield break;
			}
		}
	}
}
