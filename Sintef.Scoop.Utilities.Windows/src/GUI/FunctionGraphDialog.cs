//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Sintef.Scoop.Utilities.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Functions for visually plotting piecewise constant or linear functions.
	/// </summary>
	public static class FunctionGraphDialog
	{
		/// <summary>
		/// Opens a dialog to show a PiecewiseLinearFunction visually.
		/// Does nothing if the function has no data.
		/// </summary>
		public static void Open<E>(PiecewiseLinearFunction<E> function, string label) where E : FunctionPoint, new()
		{
			if (function.Points.Take(2).Count() < 2)
				return;
			List<KeyValuePair<double, E>> points = function.Points.ToList();
			int min = (int)Math.Round(points.First().Key);// points.Skip(1).Min(p => p._x);
			int max = 1 + (int)Math.Round(points.Last().Key);// points.Skip(1).Max(p => p._x);
			SimpleGraphForm sf = new SimpleGraphForm("Piecewise Linear Function", min, max, 3);
			Series ser = sf.AddSeries(label);
			ser.ChartType = SeriesChartType.Line;
			foreach (KeyValuePair<double, E> kvp in points)
			{
				FunctionPoint p = kvp.Value as FunctionPoint;
				sf.AddDataPoint(ser, p.X, p.Y_left);
				//	sf.AddDataPoint(ser, p.X, p.Y_right);
			}
			sf.ShowDialog();
		}

		/// <summary>
		/// Opens a dialog to show a PiecewiseConstFunction visually.
		/// Does nothing if there are less than 2 points in the
		/// profile, or the function has no data.
		/// </summary>
		public static void Open(PiecewiseConstFunction function, string label)
		{
			if (function.Points.Take(2).Count() < 2)
				return;
			int min = function.Points.Skip(1).Min(p => p.X);
			int max = function.Points.Skip(1).Max(p => p.X);
			SimpleGraphForm sf = new SimpleGraphForm("Piecewise Constant Function", min, max, 3);
			Series ser = sf.AddSeries(label);
			ser.ChartType = SeriesChartType.StepLine;
			foreach (PiecewiseConstFunction.Point p in function.Points.Skip(1)) //Skipping int.MinValue
			{
				sf.AddDataPoint(ser, p.X, p.ValueAtX);
				sf.AddDataPoint(ser, p.X, p.ValueRightOfX);
			}
			sf.ShowDialog();
		}

		/// <summary>
		/// Opens a dialog to show a PiecewiseConstFunction visually, cropped to the given time interval.
		/// </summary>
		public static void Open(PiecewiseConstFunction function, string label, IntInterval ti)
		{
			if (function.Points.Any())
			{
				SimpleGraphForm sf = new SimpleGraphForm("Piecewise Constant Function", ti.Lower, ti.Upper, 3);
				Series ser = sf.AddSeries(label);
				ser.ChartType = SeriesChartType.StepLine;
				foreach (PiecewiseConstFunction.Point p in function.Points.Skip(1).Where(p => ti.Contains(p.X))) //Skipping int.MinValue
				{
					sf.AddDataPoint(ser, p.X, p.ValueAtX);
					sf.AddDataPoint(ser, p.X, p.ValueRightOfX);
				}
				sf.ShowDialog();
			}
		}

		/// <summary>
		/// Opens a dialog to show a PiecewiseConstFunction visually.
		/// </summary>
		public static void OpenGraphDialog(PiecewiseConstFunctionDouble function, string label)
		{
			if (function.Points.Take(2).Count() <= 1)
			{
				System.Windows.Forms.MessageBox.Show("Now points to plot");
				return;
			}
			int min = function.Points.Skip(1).Min(p => p.X);
			int max = function.Points.Skip(1).Max(p => p.X);
			SimpleGraphForm sf = new SimpleGraphForm("Piecewise Constant Function", min, max, 3);
			Series ser = sf.AddSeries(label);
			ser.ChartType = SeriesChartType.StepLine;
			foreach (PiecewiseConstFunctionDouble.Point p in function.Points.Skip(1)) //Skipping int.MinValue
			{
				sf.AddDataPoint(ser, p.X, p.ValueAtX);
				sf.AddDataPoint(ser, p.X, p.ValueRightOfX);
			}
			sf.ShowDialog();
		}

		/// <summary>
		/// Opens a dialog to show a PiecewiseConstFunction visually, cropped to the given time interval.
		/// </summary>
		public static void OpenGraphDialog(PiecewiseConstFunctionDouble function, string label, IntInterval ti)
		{
			if (function.Points.Any())
			{
				SimpleGraphForm sf = new SimpleGraphForm("Piecewise Constant Function", ti.Lower, ti.Upper, 3);
				Series ser = sf.AddSeries(label);
				ser.ChartType = SeriesChartType.StepLine;
				foreach (PiecewiseConstFunctionDouble.Point p in function.Points.Skip(1).Where(p => ti.Contains(p.X))) //Skipping int.MinValue
				{
					sf.AddDataPoint(ser, p.X, p.ValueAtX);
					sf.AddDataPoint(ser, p.X, p.ValueRightOfX);
				}
				sf.ShowDialog();
			}
		}

		/// <summary>
		/// Utility function plotting PiecewiseConstFunction points. Does not refresh the graph (or re-calculate axis values). This must be done explicitly by the caller.
		/// </summary>
		/// <param name="function"></param>
		/// <param name="ser"></param>
		/// <param name="sf"></param>
		/// <param name="xOffset"></param>
		/// <param name="plotValuesForEachXInThis">If not null, a data value will be added for all integer x-values in this interval (inclusive).
		/// Use this with stacked chart types.</param>
		/// <param name="includeEndPoints">If true (the default value), then the end points with value zero to the left and right, respectively, will be plottet. If false,
		/// these will be omitted.</param>
		public static void PlotPointsToDataSeries(PiecewiseConstFunction function, int xOffset, Series ser, SimpleGraph sf, IntInterval plotValuesForEachXInThis, bool includeEndPoints = true)
		{
			List<PiecewiseConstFunction.Point> points = function.Points.ToList();
			if (plotValuesForEachXInThis != null)
			{
				int nextPointCounter = 1;
				PiecewiseConstFunction.Point previousPoint = points[0];
				PiecewiseConstFunction.Point nextPoint = points[nextPointCounter];
				for (int x = plotValuesForEachXInThis.Lower; x <= plotValuesForEachXInThis.Upper; x++)
				{
					if (nextPointCounter < points.Count && x == nextPoint.X + xOffset)
					{
						if (includeEndPoints || x > points[1].X)
							sf.AddDataPoint(ser, x, nextPoint.ValueAtX, false);
						if (includeEndPoints || nextPointCounter < points.Count - 1)
							sf.AddDataPoint(ser, x, nextPoint.ValueRightOfX, false);

						previousPoint = nextPoint;
						if (++nextPointCounter < points.Count)
							nextPoint = points[nextPointCounter];
					}
					else if (includeEndPoints || x < points.Last().X)
					{
						//Twice, to mimic left/right (to let every series have the same number of points).
						sf.AddDataPoint(ser, x, previousPoint.ValueRightOfX, false);
						sf.AddDataPoint(ser, x, previousPoint.ValueRightOfX, false);
					}
				}
			}
			else
			{
				//Plot only the data points
				for (int i = 1; i < points.Count; i++)//Skipping int.MinValue
				{
					PiecewiseConstFunction.Point p = points[i];
					if (includeEndPoints || i > 1)
						sf.AddDataPoint(ser, xOffset + p.X, p.ValueAtX, false);
					if (includeEndPoints || i < points.Count - 1)
						sf.AddDataPoint(ser, xOffset + p.X, p.ValueRightOfX, false);
				}
			}
		}

		/// <summary>
		/// Utility function. Does not refresh the graph (or re-calculate axis values). This must be done explicitly by the caller.
		/// </summary>
		/// <param name="function"></param>
		/// <param name="xOffset"></param>
		/// <param name="ser"></param>
		/// <param name="sf"></param>
		/// <param name="plotValuesForEachXInThis">If not null, a data value will be added for all integer x-values in this interval (inclusive).
		/// Use this with stacked chart types.</param>
		/// <param name="includeEndPoints">If true (the default value), then the end points with value zero to the left and right, respectively, will be plottet. If false,
		/// these will be omitted.</param>
		public static void PlotPointsToDataSeries(PiecewiseConstFunctionDouble function, int xOffset, Series ser, SimpleGraph sf, IntInterval plotValuesForEachXInThis, bool includeEndPoints = true)
		{
			List<PiecewiseConstFunctionDouble.Point> points = function.Points.ToList();
			if (plotValuesForEachXInThis != null)
			{
				int nextPointCounter = 1;
				PiecewiseConstFunctionDouble.Point previousPoint = points[0];
				PiecewiseConstFunctionDouble.Point nextPoint = points[nextPointCounter];
				for (int x = plotValuesForEachXInThis.Lower; x <= plotValuesForEachXInThis.Upper; x++)
				{
					if (nextPointCounter < points.Count && x == nextPoint.X + xOffset)
					{
						if (includeEndPoints || x > points[1].X)
							sf.AddDataPoint(ser, x, nextPoint.ValueAtX, false);
						if (includeEndPoints || nextPointCounter < points.Count - 1)
							sf.AddDataPoint(ser, x, nextPoint.ValueRightOfX, false);

						previousPoint = nextPoint;
						if (++nextPointCounter < points.Count)
							nextPoint = points[nextPointCounter];
					}
					else if (includeEndPoints || x < points.Last().X)
					{
						//Twice, to mimic left/right (to let every series have the same number of points).
						sf.AddDataPoint(ser, x, previousPoint.ValueRightOfX, false);
						sf.AddDataPoint(ser, x, previousPoint.ValueRightOfX, false);
					}
				}
			}
			else
			{
				//Plot only the data points
				for (int i = 1; i < points.Count; i++)//Skipping int.MinValue
				{
					PiecewiseConstFunctionDouble.Point p = points[i];
					if (includeEndPoints || i > 1)
						sf.AddDataPoint(ser, xOffset + p.X, p.ValueAtX, false);
					if (includeEndPoints || i < points.Count - 1)
						sf.AddDataPoint(ser, xOffset + p.X, p.ValueRightOfX, false);
				}
			}
		}
	}
}
