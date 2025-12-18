//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using C = System.Windows.Forms.DataVisualization.Charting;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Simple control for plotting data series
	/// </summary>
	public partial class SimpleGraph : UserControl
	{
		#region Properties and members

		/// <summary>
		/// Flags wether to display each series.
		/// </summary>
		Dictionary<Series, bool> _displaySeries;

		/// <summary>
		/// For checking that the user calls Initialize
		/// </summary>
		bool _isInitialized = false;

		/// <summary>
		/// The minimum range that you want to see on the X-value when zooming in.
		/// </summary>
		int _minXValueZoomRange;

		/// <summary>
		/// The minimum x-value that we consider.
		/// </summary>
		int _minXValue;

		/// <summary>
		/// The minimum x-value if the X-axis has dates.
		/// </summary>
		DateTime _startTime;

		/// <summary>
		/// X-axis DateTime or numeric?
		/// </summary>
		bool _xAxisIsDateTime = false;

		/// <summary>
		/// Start y-axis at zero, or automatically decide where to start.
		/// </summary>
		public bool StartYAtZero
		{
			get { return _chart.ChartAreas.Single().AxisY.IsStartedFromZero; }
			set { _chart.ChartAreas.Single().AxisY.IsStartedFromZero = value; }
		}

		/// <summary>
		/// Gets the chart
		/// </summary>
		public Chart Chart { get { return _chart; } }

		#endregion

		/// <summary>
		/// Initializes the graph
		/// </summary>
		public SimpleGraph()
		{
			InitializeComponent();
			_displaySeries = new Dictionary<Series, bool>();

		}

		/// <summary>
		/// Initialises the graph for a "numerical X-axis" plot. Call this (or one of the overloads) after construction, and before
		/// use. 
		/// </summary>
		/// <param name="minXvalue">The minimum X-axis value that will be plotted</param>
		/// <param name="maxXvalue">The maximum X-axis value that will be plotted</param>
		/// <param name="minXValueZoomRange">The minimum range that you want to see on the X-value when zooming in.</param>
		public void Initialize(int minXvalue, int maxXvalue, int minXValueZoomRange)
		{
			_zoom.Value = 0;
			_scollbar.Minimum = 0;
			_scollbar.Maximum = (Math.Min(int.MaxValue - 1, maxXvalue) - minXvalue);
			_scollbar.LargeChange = _scollbar.Maximum;
			_minXValueZoomRange = minXValueZoomRange;
			_xAxisIsDateTime = false;
			_minXValue = minXvalue;
			_isInitialized = true;
		}

		/// <summary>
		/// Initialises the graph for a "DateTime X-axis" plot. Call this (or one of the overloads) after construction, and before
		/// use. 
		/// </summary>
		/// <param name="minXvalue">The minimum X-axis value that will be plotted</param>
		/// <param name="maxXvalue">The maximum X-axis value that will be plotted</param>
		/// <param name="minXValueZoomRange">The minimum range that you want to see on the X-value when zooming in.</param>
		public void Initialize(DateTime minXvalue, DateTime maxXvalue, int minXValueZoomRange)
		{
			_zoom.Value = 0;
			_scollbar.Minimum = 0;
			_scollbar.Maximum = (int)(maxXvalue - minXvalue).TotalMinutes;
			_scollbar.LargeChange = _scollbar.Maximum;
			_minXValueZoomRange = minXValueZoomRange;
			_xAxisIsDateTime = true;
			_startTime = minXvalue;
			_isInitialized = true;
		}

		/// <summary>
		/// Adds a data series to show
		/// </summary>
		public Series AddSeries(string name)
		{
			if (!_isInitialized)
				throw new Exception("SimpleGraph used without being initialised. Call Initialize after construction");

			Series s = _chart.Series.Add(name);
			s.ChartType = SeriesChartType.Line;
			s.BorderWidth = 3;
			_displaySeries[s] = true;
			return s;
		}

		/// <summary>
		/// Adds a data point. If any value is double.maxvalue, the function does nothing.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="refresh">Optional. The default value = true, which means that axis values are 
		/// re-calculated and the graph is refreshed after adding the point. Set to false if you want to delay this.</param>
		/// <returns></returns>
		public C.DataPoint AddDataPoint(Series s, double x, double y, bool refresh = true)
		{
			if (s.Points != null && !x.IsNanOrInfinity() && !y.IsNanOrInfinity())
			{
				C.DataPoint dp = new C.DataPoint(x, y);
				s.Points.Add(dp);
				if (refresh)
				{
					try
					{
						_chart.ChartAreas[0].RecalculateAxesScale();
						Refresh();
					}
					catch (Exception) { }
				}
				return dp;
			}
			else
				return null;
		}

		internal void RemoveDataPoint(Series s, double x, double y)
		{
			if (s.Points != null)
			{
				C.DataPoint toDie = s.Points.FirstOrDefault(p => p.XValue == x & p.YValues.Single() == y);
				if (toDie == default(C.DataPoint))
					return;

				s.Points.Remove(toDie);
				try
				{
					_chart.ChartAreas[0].RecalculateAxesScale();
					Refresh();
				}
				catch (Exception) { }
			}
		}

		internal void RemoveDataPoint(Series s, C.DataPoint dp)
		{
			if (s.Points != null)
			{
				s.Points.Remove(dp);
				try
				{
					_chart.ChartAreas[0].RecalculateAxesScale();
					Refresh();
				}
				catch (Exception) { }
			}
		}

		private void _buttonClear_Click(object sender, EventArgs e)
		{
			Clear();
		}



		/// <summary>
		/// Clears the points from all data series
		/// </summary>
		public void Clear()
		{
			SuspendLayout();
			foreach (var s in _chart.Series)
			{
				s.Points.Clear();
			}
			ResumeLayout();

			try
			{
				_chart.ChartAreas[0].RecalculateAxesScale();
				Refresh();
			}
			catch (Exception) { }
		}

		/// <summary>
		/// Removes all series.
		/// </summary>
		public void ClearAllSeries()
		{
			SuspendLayout();
			_chart.Series.Clear();
			_displaySeries.Clear();
			ResumeLayout();

			try
			{
				_chart.ChartAreas[0].RecalculateAxesScale();
				Refresh();
			}
			catch (Exception) { }
		}

		/// <summary>
		/// Which series to view
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _buttonChooseStatistics_Click(object sender, EventArgs e)
		{
			if (!_isInitialized)
				throw new Exception("SimpleGraph used without being initialised. Call Initialize after construction");

			// Create a small form with only a checked list box over the statistic aggregates

			Form form = new Form();
			System.Windows.Forms.CheckedListBox box = new CheckedListBox();
			form.Controls.Add(box);

			box.FormattingEnabled = true;
			box.Dock = System.Windows.Forms.DockStyle.Fill;
			box.CheckOnClick = true;
			box.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

			foreach (var obj in _displaySeries)
			{
				box.Items.Add(obj.Key, obj.Value);
			}

			form.Size = new System.Drawing.Size(_displaySeries.Keys.Max(k => k.Name.Length) * 8, 150);
			form.FormBorderStyle = FormBorderStyle.None;
			form.ShowInTaskbar = false;

			// Pop up over button
  		Control control = FindForm();// Parent.Parent.Parent.Parent;
			var pos = _buttonChooseSeries.PointToScreen(new Point(0, 0));
			if (pos.Y + form.Height > control.Location.Y + control.Height)
				pos.Y = control.Location.Y + control.Height - form.Height;
			pos.X -= (form.Width - _buttonChooseSeries.Width);
			if (pos.X < control.Location.X)
				pos.X = control.Location.X;
			form.Location = pos;
			form.StartPosition = FormStartPosition.Manual;

			// Kill form on mouse leave
			System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer
			{
				Interval = 100
			};
			timer.Tick += (s, e2) =>
			{
				if (!form.DisplayRectangle.Contains(form.PointToClient(MousePosition)))
				{
					timer.Dispose();
					form.Dispose();
				}
			};

			// Update graph on check

			box.ItemCheck += (s, e2) =>
			{
				Series ser = box.Items[e2.Index] as Series;
				bool newValue = e2.NewValue == CheckState.Checked;
				_displaySeries[ser] = newValue;

				SuspendLayout();
				if (newValue)
					_chart.Series.Add(ser);
				else
					_chart.Series.Remove(ser);
				ResumeLayout();

				try
				{
					_chart.ChartAreas[0].RecalculateAxesScale();
					Refresh();
				}
				catch (Exception) { }

			};

			form.Show();
			timer.Enabled = true;
		}

		/// <summary>
		/// Updates the view when the graph zoom level changes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _zoom_ValueChanged(object sender, EventArgs e)
		{
			try
			{
				
			if (!_isInitialized)
				throw new Exception("SimpleGraph used without being initialised. Call Initialize after construction");

			if (_zoom.Value < 0)
			{
				_zoom.Value = 0;
				return;
			}

			// Record old center
			int center = _scollbar.Value + _scollbar.LargeChange / 2;
			int oldValue = _scollbar.Value;

			// Calculate new view length
			int newLength = _scollbar.Maximum;
			for (int i = 0; i < _zoom.Value; ++i)
				newLength /= 2;

			if (newLength < _minXValueZoomRange)
				return;

			// Update scollbar length
			_scollbar.LargeChange = newLength;
			_scollbar.SmallChange = newLength / 20;

			int newValue;
			if (oldValue == 0)
				// Maintain snap to start
				newValue = 0;
			else
				// Preserve center
				newValue = center - newLength / 2;

			// Avoid over/underflow
			if (newValue < 0)
				newValue = 0;
			if (newValue + newLength > _scollbar.Maximum)
				newValue = _scollbar.Maximum - newLength;

			// Update position
			_scollbar.Value = newValue;

			UpdateGraphZoom();

			}
			catch (Exception)
			{
				throw;
			}
		}

		/// <summary>
		/// Updates the view when the scrollbar is moved
		/// </summary>
		private void _scollbar_Scroll(object sender, ScrollEventArgs e)
		{
			if (!_isInitialized)
				throw new Exception("SimpleGraph used without being initialised. Call Initialize after construction");

			UpdateGraphZoom();
		}

		private void UpdateGraphZoom()
		{
			int min = _scollbar.Value;
			int max = min + _scollbar.LargeChange;
			
			IEnumerable<Series> seriesToDisplay = _displaySeries.Where(kvp => kvp.Value).Select(kvp => kvp.Key);
			if(seriesToDisplay.Any())
			{

				IEnumerable<Series> sers = seriesToDisplay.Where(s => s.Points.Any());

				if(sers.Any())
				{
					double xStartValue = sers.Min(s => s.Points.Min(p => p.XValue));
					if (_xAxisIsDateTime)
					{
						_chart.ChartAreas[0].AxisX.Minimum = _startTime.AddMinutes(min).ToOADate();
						_chart.ChartAreas[0].AxisX.Maximum = _startTime.AddMinutes(max).ToOADate();
					}
					else
					{
						_chart.ChartAreas[0].AxisX.Minimum = xStartValue + min;
						_chart.ChartAreas[0].AxisX.Maximum = xStartValue + max;
					}
					double xmin = _chart.ChartAreas[0].AxisX.Minimum;
					double xmax = _chart.ChartAreas[0].AxisX.Maximum;


					_chart.ChartAreas[0].AxisY.Minimum = sers.Min(s => GetMinimumValueInInterval(s, xmin,xmax));
					_chart.ChartAreas[0].RecalculateAxesScale();
				}
			}
		}

		/// <summary>
		/// The minimum value in the interval. If no values are defined in the interval, the function returns 0.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="xmin"></param>
		/// <param name="xmax"></param>
		/// <returns></returns>
		private double GetMinimumValueInInterval(Series s, double xmin, double xmax)
		{
			IEnumerable<System.Windows.Forms.DataVisualization.Charting.DataPoint> points = s.Points.Where(p => (p.XValue >= xmin && p.XValue <= xmax));
			if (points.Any())
				return points.Min(p => p.YValues.Min());
			else
				return 0;
		}

		/// <summary>
		/// Call this to explicitly re-calculate axis values and refresh the control.
		/// </summary>
		public void RecalcAxisAndRefreshControl()
		{
		//	_chart.ChartAreas[0].RecalculateAxesScale();
			Refresh();
		}

		private void copyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CopyImageToClipBoard();
		}

		private void CopyImageToClipBoard()
		{
			System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(Width, Height);
			DrawToBitmap(bmp, ClientRectangle);
			ThreadStart ts = new ThreadStart(() => { Clipboard.SetImage(bmp); });
			Thread t = new Thread(ts);
			t.SetApartmentState(ApartmentState.STA);
			t.Start();
		}
	}
}
