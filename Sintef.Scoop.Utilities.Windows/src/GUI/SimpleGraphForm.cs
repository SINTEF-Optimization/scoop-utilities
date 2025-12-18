//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Windows.Forms.DataVisualization.Charting;
using C = System.Windows.Forms.DataVisualization.Charting;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// A SimpleForm sub class that takes uses a SimpleGraph as it's control.
	/// </summary>
	public partial class SimpleGraphForm : SimpleForm
	{
		/// <summary>
		/// The simple graph control of the form.
		/// </summary>
		public SimpleGraph Graph { get { return Control as SimpleGraph; } }
		
		/// <summary>
		/// Default constructor. Don't use this.
		/// </summary>
		protected SimpleGraphForm():base()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Constructor, for graphs with "numerical" (int, double,...) X-axis.
		/// </summary>
		/// <param name="title">Title for the form. If null, the control's Name is used.</param>
		/// <param name="minXvalue">The minimum X-axis value that will be plotted</param>
		/// <param name="maxXvalue">The maximum X-axis value that will be plotted</param>
		/// <param name="minXValueZoomRange">The minimum range that you want to see on the X-value when zooming in.</param>
		public SimpleGraphForm(string title,int minXvalue, int maxXvalue, int minXValueZoomRange):base(new SimpleGraph(),title)
		{
			InitializeComponent();
			if (title != null)
				Text = title;
			Graph.Initialize(minXvalue,maxXvalue, minXValueZoomRange);
		}

		/// <summary>
		/// Constructor, for graphs with DateTime values on the X-axis
		/// </summary>
		/// <param name="title">Title for the form. If null, the control's Name is used.</param>
		/// <param name="minXvalue">The minimum X-axis value that will be plotted</param>
		/// <param name="maxXvalue">The maximum X-axis value that will be plotted</param>
		/// <param name="minXValueZoomRange">The minimum range that you want to see on the X-value when zooming in.</param>
		public SimpleGraphForm(string title, DateTime minXvalue, DateTime maxXvalue, int minXValueZoomRange)
			: base(new SimpleGraph(), title)
		{
			InitializeComponent();
			if (title != null)
				Text = title;
			Graph.Initialize(minXvalue, maxXvalue, minXValueZoomRange);
		}

		/// <summary>
		/// Adds a series to the underlying graph.
		/// </summary>
		public Series AddSeries(string name)
		{
			return Graph.AddSeries(name);
		}

		/// <summary>
		/// Adds a data point to the underlying graph.
		/// </summary>
		public C.DataPoint AddDataPoint(Series ser, double x, double y)
		{
			return Graph.AddDataPoint(ser, x, y);
		}

		//public void AddDataPoint(Series ser, int x, int y)
		//{
		//	Graph.AddDataPoint(ser, x, y);
		//}

		/// <summary>
		/// Removes a data point from the underlying graph.
		/// </summary>
		public void RemoveDataPoint(Series ser, double x, double y)
		{
			Graph.RemoveDataPoint(ser, x, y);
		}

		/// <summary>
		/// Removes a data point from the underlying graph.
		/// </summary>
		public void RemoveDataPoint(Series ser, C.DataPoint dp)
		{
			Graph.RemoveDataPoint(ser, dp);
		}

		/// <summary>
		/// Clears the graph
		/// </summary>
		public void ClearGraph()
		{
			Graph.Clear();
		}
	}
}
