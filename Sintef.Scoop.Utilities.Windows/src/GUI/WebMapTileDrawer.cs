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
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sintef.Scoop.Utilities.GUI
{
	/// <summary>
	/// Draws a map, which is fetched dynamically from a Web Map Tile Service.
	/// 
	/// The drawer maintains a collection of tiles and draws those appropriate for the
	/// current view. A background task (driven by a Forms timer) fetches a new tile regularly.
	/// The tile to fetch is chosen as the centermost missing tile at the appropriate zoom level.
	/// </summary>
	public class WebMapTileDrawer : DrawableItem<GeoCoordinate>
	{
		/// <summary>
		/// An tile with an image
		/// </summary>
		private class ImageTile
		{
			/// <summary>
			/// The tile reference
			/// </summary>
			public WebMercatorTile Tile;

			/// <summary>
			/// The image, or null if not ready
			/// </summary>
			public Image image;

			/// <summary>
			/// The url that produces the image
			/// </summary>
			public string url;
		}

		/// <summary>
		/// The next tile(s) to fetch the image for
		/// </summary>
		ImageTile[] _tilesToFetch;

		/// <summary>
		/// The tiles that have been generated
		/// </summary>
		Dictionary<WebMercatorTile, ImageTile> _tiles;

		/// <summary>
		/// A message to show in the view (mostly for reporting errors)
		/// </summary>
		string _message;

		/// <summary>
		/// If not null, status information is show in this label
		/// </summary>
		private Label _statusLabel;

		/// <summary>
		/// The cookie received from the web map service, if any
		/// </summary>
		private string _cookie;

		/// <summary>
		/// True if the background tile fetching has been started
		/// </summary>
		bool _hasStartedFetching = false;

		/// <summary>
		/// If true, the tile being fetched is drawn as a red rectangle
		/// </summary>
		public bool ShowTileBeingFetched { get; set; }

		/// <summary>
		/// Creates a web map tile drawer
		/// </summary>
		/// <param name="view">The view to draw in</param>
		/// <param name="statusLabel">If not null, status information is show in this label</param>
		public WebMapTileDrawer(NetworkViewControl view, Label statusLabel = null)
			: base(1)
		{
			_tiles = new Dictionary<WebMercatorTile, ImageTile>();
			_message = "No image yet";
			_statusLabel = statusLabel;
			ShowTileBeingFetched = true;

		}

		/// <summary>
		/// Draws the web map in the view
		/// </summary>
		/// <param name="view"></param>
		public override void Draw(NetworkViewControlGeneric<GeoCoordinate> view)
		{
			if (!_hasStartedFetching)
			{
				StartTileFetchingProcess(view);
				_hasStartedFetching = true;
			}

			// Get the view dimensions and coordinates

			var size = view.ViewPanel.ClientSize;
			//var cs = view.CoordinateSystem;

			PointF botLeft = new PointF(0, size.Height);
			PointF topRight = new PointF(size.Width, 0);
			PointF topLeft = new PointF(0, 0);
			PointF botRight = new PointF(size.Width, size.Height);
			PointF center = new PointF(size.Width / 2, size.Height / 2);

			var geoTopLeft = view.FromView(topLeft);
			var geoBotRight = view.FromView(botRight);
			var geoBotLeft = view.FromView(botLeft);
			var geoTopRight = view.FromView(topRight);
			var geoCenter = view.FromView(center);

			// Find the appropriate zoom
			int zoom = BestZoom(geoTopLeft, geoBotRight, Math.Max(size.Height, size.Width));

			// Find the tiles we should show but can't
			var points = new[] { geoTopLeft, geoBotRight, geoBotLeft, geoTopRight, geoCenter };
			var requiredTiles = WebMercatorTile.TilesThatCover(zoom, points, maxTiles: 100);

			var missingTiles = requiredTiles.Except(_tiles.Values.Where(x => x.image != null).Select(x => x.Tile)).ToList();
			missingTiles = missingTiles.OrderBy(x => geoCenter.DistanceTo(x.Center)).ToList();

			if (missingTiles.Any() && _tilesToFetch == null)
			{
				// Create the most important missing tiles and schedule fetching their images

				var newTiles = new List<ImageTile>();

				foreach (var id in missingTiles.Take(4))
				{
					var tile = new ImageTile
					{
						Tile = id,
						url = Url(id)
					};

					_tiles[id] = tile;
					newTiles.Add(tile);
				}

				_tilesToFetch = newTiles.ToArray();
			}

			// Select the tiles to draw: From low zoom level up to the appropriate level

			var toDraw = _tiles.Values
				.Where(v => v.image != null)
				.Where(x => x.Tile.ZoomLevel <= zoom)
				.OrderBy(x => x.Tile.ZoomLevel);

			// Draw them

			foreach (var image in toDraw)
			{
				view.DrawImage(image.image, image.Tile.TopLeft, image.Tile.TopRight, image.Tile.BottomLeft);
			}

			// Draw tile being fetched, if requested

			var tilesToFetch = _tilesToFetch;
			if (ShowTileBeingFetched && tilesToFetch != null)
			{
				foreach (var tile in _tilesToFetch.Where(t => t.image == null))
				{
					Brush b = new SolidBrush(Color.Red);
					view.FillParallelogram(tile.Tile.TopLeft, tile.Tile.BottomLeft, tile.Tile.TopRight, b);
				}
			}

			// Draw the message, if present

			if (!string.IsNullOrEmpty(_message))
			{
				// Fade out the image to ensure message is visible
				Brush brush = new SolidBrush(Color.FromArgb(128, Color.White));
				view.FillRectangle(geoTopLeft, geoBotRight, brush);
				var colour = Color.Black;
				var font = new Font("Arial", 8, FontStyle.Bold);
				view.DrawText(_message, 3, 3, colour, font);
			}
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

		/// <summary>
		/// Returns the zoom level that is appropriate for the view
		/// </summary>
		/// <param name="topLeft">The view's top left coordinate</param>
		/// <param name="bottomRight">The view's bottom right coordinate</param>
		/// <param name="pixelsSize">The view's size in pixels</param>
		/// <returns></returns>
		private int BestZoom(GeoCoordinate topLeft, GeoCoordinate bottomRight, int pixelsSize)
		{
			// Assuming a zoom level, find the difference between the view height
			// and the number of Web Mercator points between the top and bottom

			Func<int, int> diffForLevel = (zoom) =>
			{
				WebMercatorProjection.Project(topLeft, zoom, out int xl, out int yt);
				WebMercatorProjection.Project(bottomRight, zoom, out int xr, out int yb);
				int height = Math.Abs(yb - yt);
				int width = Math.Abs(xr - xl);
				int size = Math.Max(height, width);

				int diff = Math.Abs(size - pixelsSize);

				return diff;
			};

			// Choose the zoom level that given the smallest difference

			return Enumerable.Range(0, 20).MinBy(l => diffForLevel(l));
		}

		/// <summary>
		/// Returns the URL to fetch the given tile
		/// </summary>
		private static string Url(WebMercatorTile tile)
		{
			// See https://wiki.openstreetmap.org/wiki/Tiles

			string server = "";
			//string server = "a.";
			//string server = "b.";
			//string server = "c.";

			string url = string.Format("https://{0}tile.openstreetmap.org/{1}/{2}/{3}.png", server, tile.ZoomLevel, tile.X, tile.Y);
			return url;
		}

#pragma warning disable SYSLIB0014

		/// <summary>
		/// Updates the image in the given tile, by getting it from the url
		/// </summary>
		/// <param name="tile"></param>
		private void FetchImage(ImageTile tile)
		{
			WebRequest request = WebRequest.Create(tile.url);
			request.Timeout = 5000;
			request.Method = "GET";
			if (_cookie != null)
				request.Headers.Add(HttpRequestHeader.Cookie, _cookie);
			((HttpWebRequest)request).UserAgent = "SintefTestApp/0.1 (okl@sintef.no)";


			WebResponse response = request.GetResponse();
			Image img = Image.FromStream(response.GetResponseStream());

			tile.image = img;

			// Set cookie, if web map service says so
			string setCookie = response.Headers[HttpResponseHeader.SetCookie];
			if (setCookie != null)
			{
				_cookie = setCookie.Split(';')[0];
			}

			response.Close();
		}

#pragma warning restore SYSLIB0014

		/// <summary>
		/// Starts the background process that fetches tile images
		/// </summary>
		/// <param name="view"></param>
		private void StartTileFetchingProcess(NetworkViewControlGeneric<GeoCoordinate> view)
		{
			Action<string> ShowStatus = (message) => { };
			if (_statusLabel != null)
				ShowStatus = (message) => { view.Invoke((Action)delegate { _statusLabel.Text = message; }); };

			// Set up the background fetching process

			// We drive it using a timer that ticks every 200 milliseconds.
			Timer timer = new Timer() { Interval = 200 };
			timer.Tick += (s, e) =>
			{
				// Tick: this takes place on the GUI thread
				//ShowStatus("Tick");

				if (_tilesToFetch == null)
					// There is no image to fetch
					return;

				// We need to fetch new images

				// Stop the timer in the meantime
				//ShowStatus("Stopping timer");
				timer.Stop();

				List<Task> tasks = new List<Task>();

				foreach (ImageTile tileToFetch in _tilesToFetch)
				{
					// Do the actual fetching on a background thread to avoid hanging the GUI

					var task = new Task(() =>
					{
						ShowStatus("Getting " + tileToFetch.url);
						try
						{
							FetchImage(tileToFetch);

							ShowStatus("");
							_message = "";
						}
						catch (Exception ex)
						{
							ShowStatus("Exception: " + ex.Message);
							_message = ex.Message;
						}

						// Update view
						view.Invoke((Action)delegate
							{
								view.RefreshStaticLayers();
								view.RefreshViewPanel();
							});

					});
					task.Start();
					tasks.Add(task);
				}

				Task.WhenAll(tasks).ContinueWith(t =>
				{
					_tilesToFetch = null;

					// Update view and restart timer
					view.Invoke((Action)delegate
					{
						timer.Start();
						//ShowStatus("Started timer");

						view.RefreshStaticLayers();
						view.RefreshViewPanel();
					});
				});
			};

			timer.Start();
		}
	}
}
