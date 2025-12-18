//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Sintef.Scoop.Utilities.GeoCoding;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sintef.Scoop.Utilities.GUI
{
	/// <summary>
	/// Draws a map, which is fetched dynamically from a Web Map Service (WMS).
	/// 
	/// The drawer maintains a current image that is drawn. When the displayed area changes,
	/// a new image is fetched on a background thread. The view is refreshed when the new image
	/// is ready.
	/// 
	/// Only works with network view drawers based on <see cref="GeoCoordinate"/>.
	/// </summary>
	public class WebMapDrawer : DrawableItem<GeoCoordinate>
	{
		/// <summary>
		/// Data about an image
		/// </summary>
		private class ImageData
		{
			/// <summary>
			/// The image, or null if not ready
			/// </summary>
			public Image image;

			/// <summary>
			/// Coordinate of the image's top left corner
			/// </summary>
			public GeoCoordinate topLeft;

			/// <summary>
			/// Coordinate of the image's bottom right corner
			/// </summary>
			public GeoCoordinate botRight;

			/// <summary>
			/// The WMS url that produces the image
			/// </summary>
			public string url;
		}

		/// <summary>
		/// Data for the correct image for the current view
		/// </summary>
		ImageData _correctImage;

		/// <summary>
		/// Data for the last image that was fetched
		/// </summary>
		ImageData _currentImage;

		/// <summary>
		/// A message to show in the view (mostly for reporting errors)
		/// </summary>
		string _message;

		/// <summary>
		/// Creates a web map drawer
		/// </summary>
		/// <param name="view">The view to draw in</param>
		/// <param name="statusLabel">If not null, status information is show in this label</param>
		public WebMapDrawer(NetworkViewControl view, Label statusLabel = null)
			: base(1)
		{
			_message = "No image yet";

			Action<string> ShowStatus = (message) => { };
			if (statusLabel != null)
				ShowStatus = (message) => { view.Invoke((Action)delegate { statusLabel.Text = message; }); };

			// Set up the background fetching process

			// We drive it using a timer that ticks every half second.
			Timer timer = new Timer() { Interval = 500 };
			timer.Tick += (s, e) =>
			{
				// Tick: this takes place on the GUI thread
				ShowStatus("Tick");

				if (_correctImage == null)
					// There is no image to fetch
					return;

				ImageData correct = _correctImage;

				if (_currentImage != null && _currentImage.url == correct.url)
					// The current image is identical to the corect
					return;

				// We need to fetch a new image

				// Stop the timer in the meantime
				ShowStatus("Stopping timer");
				timer.Stop();

				// Do the actual fetching on a background thread to avoid hanging the GUI

				new Task(() =>
				{
					ShowStatus("Fetching image");
					try
					{
						FetchImage(correct);
						_currentImage = correct;

						ShowStatus("Fetched");
						_message = "";
					}
					catch (Exception ex)
					{
						ShowStatus("Exception: " + ex.Message);
						_message = ex.Message;
					}

					// Update view and restart timer
					view.Invoke((Action)delegate
					{
						view.RefreshStaticLayers();
						view.RefreshViewPanel();
						timer.Start();
					});

					ShowStatus("Started timer");

				}).Start();
			};

			timer.Start();
		}

		/// <summary>
		/// Draws the web map image in the view
		/// </summary>
		/// <param name="view"></param>
		public override void Draw(NetworkViewControlGeneric<GeoCoordinate> view)
		{
			// Get the view dimensions and coordinates

			var size = view.ViewPanel.ClientSize;
			var cs = view.CoordinateSystem;

			PointF botLeft = new PointF(0, size.Height);
			PointF topRight = new PointF(size.Width, 0);
			PointF topLeft = new PointF(0, 0);
			PointF botRight = new PointF(size.Width, size.Height);

			var geoTopLeft = view.FromView(topLeft);
			var geoBotRight = view.FromView(botRight);
			var geoBotLeft = view.FromView(botLeft);
			var geoTopRight = view.FromView(topRight);

			// Update info about the correct image for the view.

			string url = GetImageUrl(size, cs, geoBotLeft, geoTopRight);

			_correctImage = new ImageData
			{
				topLeft = geoTopLeft,
				botRight = geoBotRight,
				url = url
			};

			// Draw the current image, if present

			ImageData image = _currentImage;
			if (image != null)
			{
				view.DrawImage(image.image, image.topLeft, image.botRight);
			}

			// Draw the message, if present

			if (!string.IsNullOrEmpty(_message))
			{
				if (image != null)
				{
					// Fade out the image to ensure message is visible

					Brush brush = new SolidBrush(Color.FromArgb(128, Color.White));
					view.FillRectangle(image.topLeft, image.botRight, brush);
				}

				var colour = Color.Black;
				var font = new Font("Arial", 8, FontStyle.Bold);

				view.DrawText(_message, geoTopLeft, colour, font, new SizeF(0, 0));
			}
		}

#pragma warning disable SYSLIB0014

		/// <summary>
		/// Updates the image in the given ImageData, by getting it from the url
		/// </summary>
		/// <param name="data"></param>
		private void FetchImage(ImageData data)
		{
			WebRequest request = WebRequest.Create(data.url);
			request.Timeout = 5000;
			request.Method = "GET";

			WebResponse response = request.GetResponse();
			Image img = Image.FromStream(response.GetResponseStream());
			response.Close();

			data.image = img;
		}

#pragma warning restore SYSLIB0014

		/// <summary>
		/// Returns the url for the given image parameters
		/// </summary>
		/// <param name="size">The image size, in pixels</param>
		/// <param name="cs">The (UTM) coordinate system to use</param>
		/// <param name="geoBotLeft">The coordinate of the image's bottom left corner</param>
		/// <param name="geoTopRight">The coordinate of the image's top right corner</param>
		/// <returns>The url</returns>
		private string GetImageUrl(Size size, CoordinateSystem cs, GeoCoordinate geoBotLeft, GeoCoordinate geoTopRight)
		{
			var utmBotLeft = cs.GetUtmCoordinate(geoBotLeft);
			var utmTopRight = cs.GetUtmCoordinate(geoTopRight);

			string bbox = string.Format("{0},{1},{2},{3}", utmBotLeft.Easting, utmBotLeft.Northing, utmTopRight.Easting, utmTopRight.Northing);

			// This is pretty hardcoded for now -- should be made more general
			// The url is for the WORLD OSM WMS, http://www.osm-wms.de/
			// See also http://wiki.openstreetmap.org/wiki/WMS#OSM_WMS_Servers

			string url = "http://129.206.228.72/cached/osm?"
				//				string url = "http://ows.terrestris.de/osm/service?"
				+ "LAYERS=osm_auto:all"
				+ "&STYLES="
				+ "&SRS=EPSG%3A32632"
				+ "&FORMAT=image%2Fpng"
				+ "&SERVICE=WMS"
				+ "&VERSION=1.1.1"
				+ "&REQUEST=GetMap"
				//+ "&BBOX=500000,6499000,501000,6500000"
				+ "&BBOX=" + bbox
				+ "&WIDTH=" + size.Width
				+ "&HEIGHT=" + size.Height;
			return url;
		}

		/// <summary>
		/// Returns an empty enumeration
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
