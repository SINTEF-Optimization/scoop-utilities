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
	/// A control for viewing cartesian coordinate data.
	/// 
	/// The control uses three coordinate systems:
	///  - Cartesian world coordinates.
	///  - Map. Euclidean. These are the same as the world coordinates (all conversions are trivial)
	///  - View. Euclidean. This is the Windows-defined coordinate system of the view panel. The unit is pixels.
	///    (0, 0) is in the upper left corner of the view, and the Y axis increases downward.
	///  
	/// Conversion between the map and view coordinate systems is a simple scale/translate/rotate
	/// operation.
	/// </summary>
	public partial class NetworkViewControlCartesian : NetworkViewControlGeneric<Coordinate>
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public NetworkViewControlCartesian():base()
		{
		}

		/// <summary>
		/// Returns the number of pixels per coordinate unit (m) in the Y direction.
		/// The base implementation assumes the W-coordinate system is cartesian.
		/// </summary>
		public override double PixelsPerYUnit()
		{
			Coordinate c1 = FromView(ViewCenter);
			Coordinate c2 = new Coordinate(c1.X, c1.Y + 1);

			return ViewDistance(c1, c2);
		}


		#region Coordinate conversion

		/// <summary>
		/// Converts from cartesian world coordinates to map coordinates. These are simply the same, the conversion
		/// is trivial.
		/// </summary>
		public override void ToMap(Coordinate coordinate, out double mapX, out double mapY)
		{
			if (!(coordinate is Coordinate carco))
				throw new ArgumentException($"Expected Coordinate, got {coordinate.GetType()}");
			mapX = carco.X;
			mapY = carco.Y;
		}

		/// <summary>
		/// Converts from map to cartesian world coordinates (trivial, they are the same.
		/// </summary>
		protected override Coordinate FromMap(double mapX, double mapY) => new Coordinate(mapX, mapY);

		#endregion


		/// <summary>
		/// Converts from view to cartesian world coordinates
		/// </summary>
		public new Coordinate FromView(PointF point) => base.FromView(point) as Coordinate;

		///// <summary>
		///// Converts from geographical to view coordinates
		///// </summary>
		//public PointF ToView(ICoordinate coordinate)
		//{
		//	ToMap(coordinate, out double mapX, out double mapY);

		//	return ToView(mapX, mapY);
		//}

		///// <summary>
		///// Converts from view to geographical coordinates
		///// </summary>
		//public ICoordinate FromView(PointF point)
		//{
		//	ToMap(point, out double mapX, out double mapY);

		//	return FromMap(mapX, mapY);
		//}

		///// <summary>
		///// Converts from map to view coordinates
		///// </summary>
		//public PointF ToView(double mapX, double mapY)
		//{
		//	if (!ViewIsSet)
		//		return new PointF(0, 0);

		//	// Convert to pixel offset from center (unrotated)
		//	double x = (mapX - _xCenter) / ZoomScale;
		//	double y = -(mapY - _yCenter) / ZoomScale;

		//	// Rotate around center
		//	double sin = Math.Sin(_rotationAngleInRadians);
		//	double cos = Math.Cos(_rotationAngleInRadians);
		//	double rx = x * cos + y * sin;
		//	double ry = y * cos - x * sin;

		//	// Translate relative to view origin
		//	Size viewSize = _viewPanel.ClientSize;
		//	x = rx + viewSize.Width / 2;
		//	y = ry + viewSize.Height / 2;

		//	return new PointF((float)x, (float)y);
		//}

		///// <summary>
		///// Converts from view to map coordinates
		///// </summary>
		//public void ToMap(PointF point, out double mapX, out double mapY)
		//{
		//	// Translate relative to view center
		//	Size viewSize = _viewPanel.ClientSize;
		//	double x = point.X - viewSize.Width / 2;
		//	double y = point.Y - viewSize.Height / 2;

		//	// Rotate around center
		//	double sin = Math.Sin(_rotationAngleInRadians);
		//	double cos = Math.Cos(_rotationAngleInRadians);
		//	double rx = x * cos - y * sin;
		//	double ry = y * cos + x * sin;

		//	// Convert to map
		//	mapX = rx * ZoomScale + _xCenter;
		//	mapY = -ry * ZoomScale + _yCenter;
		//}

		///// <summary>
		///// Scales a distance in map coordinates to a distance in the view
		///// </summary>
		//public float ToView(double mapDistance)
		//{
		//	return (float)(mapDistance / ZoomScale);
		//}

		//#endregion

		//#region Methods only for use by DrawableItems during a Paint event

		///// <summary>
		///// Evaluates the size and position of the given bounding box with respect to
		///// the area currently being repainted.
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		///// <param name="boundingBox">The bounding box to evaluate</param>
		///// <param name="smallDiagonal">The number of pixels a diagonal can be to be considered small</param>
		///// <returns></returns>
		//public BoundingBoxStatus GetBoundingBoxStatus(BoundingBox<ICoordinate> boundingBox, int smallDiagonal)
		//{
		//	ICoordinate upRight = new ICoordinate(boundingBox.MaxLatitude, boundingBox.MaxLongitude);
		//	ICoordinate downLeft = new ICoordinate(boundingBox.MinLatitude, boundingBox.MinLongitude);

		//	PointF viewUpRight = ToView(upRight);
		//	PointF viewDownLeft = ToView(downLeft);

		//	if (Math.Abs(viewDownLeft.X - viewUpRight.X) < smallDiagonal && Math.Abs(viewDownLeft.Y - viewUpRight.Y) < smallDiagonal)
		//		return BoundingBoxStatus.Small;

		//	ICoordinate downRight = new ICoordinate(boundingBox.MinLatitude, boundingBox.MaxLongitude);
		//	ICoordinate upLeft = new ICoordinate(boundingBox.MaxLatitude, boundingBox.MinLongitude);

		//	PointF viewDownRight = ToView(downRight);
		//	PointF viewUpLeft = ToView(upLeft);

		//	Rectangle area;

		//	if (_paintEventArgs == null)
		//	{
		//		area = new Rectangle(0, 0, _bitmaps[_currentBitmap].Width, _bitmaps[_currentBitmap].Height);
		//	}
		//	else
		//	{
		//		area = _paintEventArgs.ClipRectangle;
		//	}

		//	if (Math.Max(viewDownLeft.X, Math.Max(viewDownRight.X, Math.Max(viewUpLeft.X, viewUpRight.X))) < area.Left)
		//		return BoundingBoxStatus.OutsideView;

		//	if (Math.Max(viewDownLeft.Y, Math.Max(viewDownRight.Y, Math.Max(viewUpLeft.Y, viewUpRight.Y))) < area.Top)
		//		return BoundingBoxStatus.OutsideView;

		//	if (Math.Min(viewDownLeft.X, Math.Min(viewDownRight.X, Math.Min(viewUpLeft.X, viewUpRight.X))) > area.Right)
		//		return BoundingBoxStatus.OutsideView;

		//	if (Math.Min(viewDownLeft.Y, Math.Min(viewDownRight.Y, Math.Min(viewUpLeft.Y, viewUpRight.Y))) > area.Bottom)
		//		return BoundingBoxStatus.OutsideView;

		//	// While scrolling, most of the screen doesnt need to be repainted
		//	if (_validRect.HasValue)
		//	{
		//		// Here there can be done further optimizations, currently removing only areas that are entirely inside the non repainted
		//		// area

		//		if (_validRect.Value.Contains(viewUpLeft) && _validRect.Value.Contains(viewUpRight) && _validRect.Value.Contains(viewDownLeft) && _validRect.Value.Contains(viewDownRight))
		//			return BoundingBoxStatus.OutsideView;
		//	}

		//	return BoundingBoxStatus.Normal;
		//}

		///// <summary>
		///// Fills a polygon in the view panel.
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		//public void FillPolygon(IList<ICoordinate> coordinates, Brush brush)
		//{
		//	PointF[] points = coordinates.Select(x => ToView(x)).ToArray();

		//	FillPolygon(points, brush);
		//}

		///// <summary>
		///// Draws a polyline in the view panel.
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		//public void DrawPolyline(IList<ICoordinate> coordinates, Pen pen)
		//{
		//	GraphicsPath path = new GraphicsPath();

		//	for (int i = 1; i < coordinates.Count; ++i)
		//	{
		//		var line = LineInView(coordinates[i - 1], coordinates[i], pen.Width);

		//		if (line != null)
		//			path.AddLine(line.From, line.To);
		//		else
		//			path.StartFigure();
		//	}

		//	_currentGraphics?.DrawPath(pen, path);

		//	if (CalculateBoundingRectangle)
		//	{
		//		float minX = float.MaxValue, maxX = float.MinValue;
		//		float minY = float.MaxValue, maxY = float.MinValue;

		//		foreach (ICoordinate c in coordinates)
		//		{
		//			PointF p = ToView(c);

		//			if (p.X < minX)
		//				minX = p.X;
		//			if (p.X > maxX)
		//				maxX = p.X;
		//			if (p.Y < minY)
		//				minY = p.Y;
		//			if (p.Y > maxY)
		//				maxY = p.Y;
		//		}
		//		float width = pen.Width;
		//		UpdateBoundingRectangle(minX - width, minY - width);
		//		UpdateBoundingRectangle(maxX + width, maxY + width);
		//	}
		//}

		///// <summary>
		///// Draws a line in the view panel.
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		//public void DrawLine(ICoordinate from, ICoordinate to, Pen pen)
		//{
		//	try
		//	{
		//		if (from == to)
		//			return;

		//		var line = LineInView(from, to, pen.Width);

		//		if (line == null)
		//			return;

		//		_currentGraphics.DrawLine(pen, line.From, line.To);

		//		if (CalculateBoundingRectangle)
		//		{
		//			float width = pen.Width;
		//			PointF from1 = line.From;
		//			PointF to1 = line.To;

		//			UpdateBoundingRectangle(Math.Min(from1.X, to1.X) - width, Math.Min(from1.Y, to1.Y) - width);
		//			UpdateBoundingRectangle(Math.Max(from1.X, to1.X) + width, Math.Max(from1.Y, to1.Y) + width);
		//		}
		//	}
		//	catch (Exception) { }
		//}

		///// <summary>
		///// Draws a line with fixed length in the view panel.
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		///// <param name="c">ICoordinate to orientate line</param>
		///// <param name="startXOffset">x coordinate offset of start point in view panel distance of coordinate</param>
		///// <param name="startYOffset">y coordinate offset of start point in view panel distance of coordinate</param>
		///// <param name="endXOffset">x coordinate offset of end point in view panel distance of coordinate</param>
		///// <param name="endYOffset">y coordinate offset of end point in view panel distance of coordinate</param>
		///// <param name="pen">pen to use</param>
		//public void DrawFixedViewLine(ICoordinate c, float startXOffset, float startYOffset,
		//	float endXOffset, float endYOffset, Pen pen)
		//{
		//	try
		//	{
		//		PointF from1 = ToView(c);
		//		PointF to1 = from1;
		//		from1.X += startXOffset;
		//		from1.Y += startYOffset;
		//		to1.X += endXOffset;
		//		to1.Y += endYOffset;

		//		_currentGraphics.DrawLine(pen, from1, to1);

		//		if (CalculateBoundingRectangle)
		//		{
		//			float width = pen.Width;
		//			UpdateBoundingRectangle(Math.Min(from1.X, to1.X) - width, Math.Min(from1.Y, to1.Y) - width);
		//			UpdateBoundingRectangle(Math.Max(from1.X, to1.X) + width, Math.Max(from1.Y, to1.Y) + width);
		//		}
		//	}
		//	catch (Exception) { }
		//}

		///// <summary>
		///// Draws a split line in the view panel.
		///// The first part (fraction splitpoint) is drawn using pen1.
		///// The second pair (fraction 1-splitpoint) is drawn using pen2.
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		//public void DrawSplitLine(ICoordinate from, ICoordinate to, Pen pen1, double splitPoint, Pen pen2)
		//{
		//	try
		//	{
		//		PointF from1 = ToView(from);
		//		PointF to1 = ToView(to);
		//		PointF mid = Interpolate(from1, to1, splitPoint);

		//		_currentGraphics.DrawLine(pen1, from1, mid);
		//		_currentGraphics.DrawLine(pen2, mid, to1);

		//		if (CalculateBoundingRectangle)
		//		{
		//			float width = Math.Max(pen1.Width, pen2.Width);
		//			UpdateBoundingRectangle(Math.Min(from1.X, to1.X) - width, Math.Min(from1.Y, to1.Y) - width);
		//			UpdateBoundingRectangle(Math.Max(from1.X, to1.X) + width, Math.Max(from1.Y, to1.Y) + width);
		//		}
		//	}
		//	catch (Exception) { }
		//}

		///// <summary>
		///// Draws an arrowhead in the view panel.
		///// The arrowhead is placed on the line from <paramref name="lineStart"/> to <paramref name="lineEnd"/>,
		///// pointing toward <paramref name="lineEnd"/>.
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		///// <param name="pen">The pen to draw with</param>
		///// <param name="size">The arrowhead size, in pixels</param>
		///// <param name="position">The arrowhead's position along the line, as a fraction of
		/////   distance from start to end. 1.0 is at the end, 0.5 in the middle.</param>
		//public void DrawArrowhead(ICoordinate lineStart, ICoordinate lineEnd, Pen pen, int size, double position = 1.0)
		//{
		//	try
		//	{
		//		// Convert to view coordinates
		//		PointF startPoint = ToView(lineStart);
		//		PointF endPoint = ToView(lineEnd);

		//		// Find the direction from start to end and the direction at right angles
		//		SizeF direction = new SizeF(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y);
		//		direction = Scale(direction, 1 / Length(direction));

		//		SizeF rightDirection = new SizeF(direction.Height, -direction.Width);

		//		// Find the points defining the arrowhead
		//		PointF arrowPoint = Interpolate(startPoint, endPoint, position);
		//		PointF left = arrowPoint + Scale(direction, -size) - Scale(rightDirection, size);
		//		PointF right = arrowPoint + Scale(direction, -size) + Scale(rightDirection, size);

		//		// Draw
		//		_currentGraphics.DrawLine(pen, left, arrowPoint);
		//		_currentGraphics.DrawLine(pen, arrowPoint, right);
		//	}
		//	catch (Exception) { }
		//}

		///// <summary>
		///// Draws a circle in the view panel.
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		//public void DrawCircle(ICoordinate center, float radius, Pen pen)
		//{
		//	try
		//	{
		//		PointF from = ToView(center);

		//		_currentGraphics.DrawEllipse(pen, from.X - radius, from.Y - radius, radius + radius, radius + radius);
		//		if (CalculateBoundingRectangle)
		//		{
		//			float width = pen.Width;
		//			UpdateBoundingRectangle(from.X - radius - width, from.Y - radius - width);
		//			UpdateBoundingRectangle(from.X + radius + width, from.Y + radius + width);
		//		}
		//	}
		//	catch (Exception) { }
		//}

		///// <summary>
		///// Draws an ellipse circle in the view panel.
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		//public void DrawEllipse(ICoordinate center, float width, float height, Pen pen)
		//{
		//	try
		//	{
		//		PointF from = ToView(center);

		//		float top = from.Y - (height * 0.5f);
		//		float bottom = from.Y + (height * 0.5f);
		//		float left = from.X - (width * 0.5f);
		//		float right = from.X - (width * 0.5f);

		//		_currentGraphics.DrawEllipse(pen, left, top, width, height);
		//		if (CalculateBoundingRectangle)
		//		{
		//			float penWidth = pen.Width;
		//			UpdateBoundingRectangle(left - penWidth, top - penWidth);
		//			UpdateBoundingRectangle(right + penWidth, bottom + penWidth);
		//		}
		//	}
		//	catch (Exception) { }
		//}

		///// <summary>
		///// Draws a filled circle in the view panel.
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		//public void DrawFilledCircle(ICoordinate center, float radius, Brush brush)
		//{
		//	FillCircle(center, radius, brush);
		//}

		///// <summary>
		///// Draws a filled circle in the view panel.
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		//public void FillCircle(ICoordinate center, float radius, Brush brush)
		//{
		//	try
		//	{
		//		PointF from = ToView(center);

		//		_currentGraphics.FillEllipse(brush, from.X - radius, from.Y - radius, radius + radius, radius + radius);
		//		if (CalculateBoundingRectangle)
		//		{
		//			UpdateBoundingRectangle(from.X - radius, from.Y - radius);
		//			UpdateBoundingRectangle(from.X + radius, from.Y + radius);
		//		}
		//	}
		//	catch (Exception) { }
		//}

		///// <summary>
		///// Draws a filled ellipse in the view panel.
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		//public void FillEllipse(ICoordinate center, float width, float height, Color centerColor, Color surroundColor)
		//{
		//	try
		//	{
		//		PointF from = ToView(center);

		//		float x = from.X - 0.5F * width;
		//		float y = from.Y - 0.5F * height;
		//		Brush brush = EllipseGradientBrush(x, y, width, height, centerColor, surroundColor);
		//		_currentGraphics.FillEllipse(brush, x, y, width, height);
		//		if (CalculateBoundingRectangle)
		//		{
		//			UpdateBoundingRectangle(x, y);
		//			UpdateBoundingRectangle(x + width, y + height);
		//		}
		//	}
		//	catch (Exception) { }
		//}

		///// <summary>
		///// Draws a line whose colour is a gradient between two given colours
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		///// <param name="widthIs|Meters">If true, the width parameter is given in pixels. 
		/////  If false, the width is given in map units</param>
		//public void DrawGradientLine(ICoordinate start, ICoordinate end, double width, Color startColor, Color endColor, bool widthIsInMap = true)
		//{
		//	try
		//	{
		//		PointF p1 = ToView(start);
		//		PointF p2 = ToView(end);

		//		float vx = p2.X - p1.X, vy = p2.Y - p1.Y;

		//		// Translate (mx,my) to origo
		//		float mx = (p1.X + p2.X) / 2, my = (p1.Y + p2.Y) / 2;
		//		_currentGraphics.TranslateTransform(mx, my);

		//		// Rotate according to direction unit vector
		//		float h = (float)Math.Sqrt(vx * vx + vy * vy);
		//		float ux = vx / h, uy = vy / h;
		//		_currentGraphics.MultiplyTransform(new Matrix(ux, uy, -uy, ux, 0, 0));

		//		// Determine width:
		//		float w = (float)width;
		//		if (widthIsInMap)
		//			w *= (float)PixelsPerMeter();

		//		// Draw
		//		float a = h / 2;
		//		float b = w / 2;

		//		LinearGradientBrush brush = new LinearGradientBrush(new PointF(-a, -b), new PointF(a, b), startColor, endColor);
		//		_currentGraphics.FillRectangle(brush, -a, -b, h, w);

		//		if (CalculateBoundingRectangle)
		//		{
		//			float r = Math.Max(a, b); // is this the worst case? 
		//			UpdateBoundingRectangle(mx - r, my - r);
		//			UpdateBoundingRectangle(mx + r, my + r);
		//		}
		//	}
		//	finally
		//	{
		//		_currentGraphics.ResetTransform();
		//	}
		//}

		///// <summary>
		///// Draws a filled ellipse in the view panel.
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		///// <param name="end1">One end of the ellipse</param>
		///// <param name="end2">The other end of the ellipse</param>
		///// <param name="width">The width (in meters)</param>
		///// <param name="brush">The brush</param>
		//public void FillEllipse(ICoordinate end1, ICoordinate end2, double width, Brush brush)
		//{
		//	try
		//	{
		//		PointF p1 = ToView(end1);
		//		PointF p2 = ToView(end2);

		//		float vx = p2.X - p1.X, vy = p2.Y - p1.Y;

		//		// Translate (mx,my) to origo
		//		float mx = (p1.X + p2.X) / 2, my = (p1.Y + p2.Y) / 2;
		//		_currentGraphics.TranslateTransform(mx, my);

		//		// Rotate according to direction unit vector
		//		float h = (float)Math.Sqrt(vx * vx + vy * vy);
		//		float ux = vx / h, uy = vy / h;
		//		_currentGraphics.MultiplyTransform(new Matrix(ux, uy, -uy, ux, 0, 0));

		//		// Determine width:
		//		float w = (float)(width * PixelsPerMeter());

		//		// Draw
		//		float a = h / 2;
		//		float b = w / 2;
		//		_currentGraphics.FillEllipse(brush, -a, -b, h, w);

		//		if (CalculateBoundingRectangle)
		//		{
		//			float r = Math.Max(a, b); // is this the worst case? 
		//			UpdateBoundingRectangle(mx - r, my - r);
		//			UpdateBoundingRectangle(mx + r, my + r);
		//		}
		//	}
		//	finally
		//	{
		//		_currentGraphics.ResetTransform();
		//	}
		//}

		///// <summary>
		///// Draws a rectangle in the view panel.
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		//public void DrawRectangle(ICoordinate center, int width, int height, Pen pen)
		//{
		//	try
		//	{
		//		PointF middle = ToView(center);

		//		Rectangle r = new Rectangle((int)(middle.X - width / 2.0), (int)(middle.Y - height / 2.0), width, height);
		//		DrawRectangle(r, pen);
		//	}
		//	catch (Exception) { }
		//}

		///// <summary>
		///// Draws a rectangle in the view panel.
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		///// <param name="rectangle">The rectangle, in view coordinates</param>
		///// <param name="pen">The pen to use</param>
		//private void DrawRectangle(Rectangle rectangle, Pen pen)
		//{
		//	try
		//	{
		//		_currentGraphics.DrawRectangle(pen, rectangle);
		//		if (CalculateBoundingRectangle)
		//		{
		//			float pwidth = pen.Width;
		//			UpdateBoundingRectangle(rectangle.X - pwidth, rectangle.X + rectangle.Width - pwidth);
		//			UpdateBoundingRectangle(rectangle.Y + pwidth, rectangle.Y + rectangle.Height + pwidth);
		//		}
		//	}
		//	catch (Exception) { }
		//}

		///// <summary>
		///// Draws a rectangle in the view panel.
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		//public void DrawRectangle(ICoordinate corner1, ICoordinate corner2, Pen pen)
		//{
		//	try
		//	{
		//		PointF c1 = ToView(corner1);
		//		PointF c2 = ToView(corner2);

		//		_currentGraphics.DrawRectangle(pen,
		//			Math.Min(c1.X, c2.X),
		//			Math.Min(c1.Y, c2.Y),
		//			Math.Abs(c1.X - c2.X),
		//			Math.Abs(c1.Y - c2.Y));

		//		if (CalculateBoundingRectangle)
		//		{
		//			float pwidth = pen.Width;
		//			UpdateBoundingRectangle(c1.X, c1.Y);
		//			UpdateBoundingRectangle(c2.X, c2.Y);
		//		}
		//	}
		//	catch (Exception) { }
		//}

		///// <summary>
		///// Draws a filled rectangle in the view panel.
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		//public void FillRectangle(ICoordinate center, int width, int height, Brush brush)
		//{
		//	try
		//	{
		//		PointF from = ToView(center);

		//		_currentGraphics.FillRectangle(brush, (int)from.X - width / 2, (int)from.Y - height / 2, width, height);
		//		if (CalculateBoundingRectangle)
		//		{
		//			UpdateBoundingRectangle(from.X - width / 2, from.Y - height / 2);
		//			UpdateBoundingRectangle(from.X + width / 2, from.Y + height / 2);
		//		}
		//	}
		//	catch (Exception) { }
		//}

		///// <summary>
		///// Draws a filled rectangle in the view panel.
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		//public void FillRectangle(ICoordinate corner1, ICoordinate corner2, Brush brush)
		//{
		//	try
		//	{
		//		PointF c1 = ToView(corner1);
		//		PointF c2 = ToView(corner2);

		//		_currentGraphics.FillRectangle(brush,
		//			Math.Min(c1.X, c2.X),
		//			Math.Min(c1.Y, c2.Y),
		//			Math.Abs(c1.X - c2.X),
		//			Math.Abs(c1.Y - c2.Y));

		//		if (CalculateBoundingRectangle)
		//		{
		//			UpdateBoundingRectangle(c1.X, c1.Y);
		//			UpdateBoundingRectangle(c2.X, c2.Y);
		//		}
		//	}
		//	catch (Exception) { }
		//}

		///// <summary>
		///// Draws a filled parallelogram in the view panel.
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		//public void FillParallelogram(ICoordinate corner1, ICoordinate corner2, ICoordinate corner3, Brush brush)
		//{
		//	try
		//	{
		//		PointF c1 = ToView(corner1);
		//		PointF c2 = ToView(corner2);
		//		PointF c3 = ToView(corner3);

		//		var c4 = new PointF(c2.X + c3.X - c1.X, c2.Y + c3.Y - c1.Y);

		//		FillPolygon(new[] { c1, c2, c4, c3 }, brush);
		//	}
		//	catch (Exception) { }
		//}

		///// <summary>
		///// Draws a text string.
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		///// <param name="s">The text to draw</param>
		///// <param name="upperLeft">The coordinate of the upper left corner of the text</param>
		///// <param name="c">The color of the text</param>
		//public void DrawText(string s, ICoordinate upperLeft, Color c, Font font, SizeF offset)
		//{
		//	try
		//	{
		//		Brush b = new SolidBrush(c);
		//		PointF p = ToView(upperLeft) + offset;
		//		_currentGraphics.DrawString(s, font, b, p);
		//		if (CalculateBoundingRectangle)
		//		{
		//			SizeF size = MeasureString(s, font);
		//			UpdateBoundingRectangle(p.X, p.Y);
		//			UpdateBoundingRectangle(p.X + size.Width, p.Y + size.Height);
		//		}
		//	}
		//	catch (Exception) { }
		//}

		///// <summary>
		///// Draws a text string with a separate outline color.
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		///// <param name="s">>The text to draw</param>
		///// <param name="upperLeft">The coordinate of the upper left corner of the text</param>
		///// <param name="fillColor">The fill color of the text</param>
		///// <param name="outlineColor">The outline color of the text</param>
		///// <param name="font">The font of the text</param>
		///// <param name="offset">The 'view offset' from the view position of the 'upperLeft' coordinate</param>
		//public void DrawOutlinedText(string s, ICoordinate upperLeft, Color fillColor, Color outlineColor, Font font, SizeF offset)
		//{
		//	try
		//	{
		//		PointF p = ToView(upperLeft) + offset;

		//		// Outline
		//		GraphicsPath path = new GraphicsPath();
		//		float emSize = _currentGraphics.DpiY * font.SizeInPoints / 72;
		//		float outlineWidth = .2f * emSize;
		//		path.AddString(s, font.FontFamily, (int)font.Style,
		//							emSize, p, new StringFormat());
		//		Pen pen = new Pen(outlineColor, outlineWidth);
		//		_currentGraphics.DrawPath(pen, path);

		//		// Fill
		//		Brush brush = new SolidBrush(fillColor);
		//		_currentGraphics.FillPath(brush, path);
		//		//_currentGraphics.DrawString(s, font, Brushes.Green, p);

		//		if (CalculateBoundingRectangle)
		//		{
		//			SizeF size = MeasureString(s, font);
		//			UpdateBoundingRectangle(p.X, p.Y);
		//			UpdateBoundingRectangle(p.X + size.Width, p.Y + size.Height);
		//		}
		//	}
		//	catch (Exception) { }
		//}

		///// <summary>
		///// Draws a text in the control panel. Font is hardcoded to Arial
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		///// <param name="s">The text to draw</param>
		///// <param name="c">The color of the text</param>
		//public void DrawText(string s, float x, float y, Color c, Font font)
		//{
		//	try
		//	{
		//		_currentGraphics.DrawString(s, font, new SolidBrush(c), x, y);
		//		if (CalculateBoundingRectangle)
		//		{
		//			SizeF size = MeasureString(s, font);
		//			UpdateBoundingRectangle(x, y);
		//			UpdateBoundingRectangle(x + size.Width, y + size.Height);
		//		}
		//	}
		//	catch (Exception) { }
		//}

		///// <summary>
		///// Draws an image between the given coordinates.
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		///// <param name="srcRegion">If not null, identifies the part (in pixels) of the image that is to be drawn.
		///// This part will be drawn between topLeft and bottomRight. If null, the whole image is drawn. </param>
		//public void DrawImage(Image image, ICoordinate topLeft, ICoordinate bottomRight, RectangleF? srcRegion = null)
		//{
		//	try
		//	{
		//		PointF p1 = ToView(topLeft);
		//		PointF p2 = ToView(bottomRight);
		//		float width = Math.Abs(p1.X - p2.X);
		//		float height = Math.Abs(p1.Y - p2.Y);
		//		RectangleF rectangle = new RectangleF(p1.X, p1.Y, width, height);

		//		if (srcRegion != null)
		//			_currentGraphics.DrawImage(image, rectangle, srcRegion.Value, GraphicsUnit.Pixel);
		//		else
		//			_currentGraphics.DrawImage(image, rectangle);

		//		if (CalculateBoundingRectangle)
		//		{
		//			UpdateBoundingRectangle(p1.X, p1.Y);
		//			UpdateBoundingRectangle(p2.X, p2.Y);
		//		}
		//	}
		//	catch (Exception) { }
		//}

		///// <summary>
		///// Draws an image between centered at the given coordinate, with the given size in pixels, and with the given rotation.
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		///// <param name="center">Center coordinate</param>
		///// <param name="image">The image</param>
		///// <param name="theta">The rotation angle, counter-clockwise, from "east"</param>
		///// <param name="size">Size on screen, in pixels</param>
		//public void DrawImage(Image b, ICoordinate center, double theta, int width, int height)
		//{
		//	try
		//	{
		//		var resizedB = ResizeImage(b, width, height);

		//		PointF centerF = ToView(center);
		//		var bitmp = RotateImage(resizedB, -(float) theta);
		//		PointF topLeft = new PointF(centerF.X - bitmp.Width / 2, centerF.Y - bitmp.Height / 2);
		//		_currentGraphics.DrawImage(bitmp, topLeft);
		//		resizedB.Dispose();
		//		bitmp.Dispose();			
		//	}
		//	catch (Exception e) { }
		//}

		///// <summary>
		///// Resize the image to the specified width and height.
		///// </summary>
		///// <param name="image">The image to resize.</param>
		///// <param name="width">The width to resize to.</param>
		///// <param name="height">The height to resize to.</param>
		///// <returns>The resized image.</returns>
		//public static Bitmap ResizeImage(Image image, int width, int height)
		//{
		//	var destRect = new Rectangle(0, 0, width, height);
		//	var destImage = new Bitmap(width, height);

		//	destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

		//	using (var graphics = Graphics.FromImage(destImage))
		//	{
		//		graphics.CompositingMode = CompositingMode.SourceCopy;
		//		graphics.CompositingQuality = CompositingQuality.HighQuality;
		//		graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
		//		graphics.SmoothingMode = SmoothingMode.HighQuality;
		//		graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

		//		using (var wrapMode = new ImageAttributes())
		//		{
		//			wrapMode.SetWrapMode(WrapMode.TileFlipXY);
		//			graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
		//		}
		//	}

		//	return destImage;
		//}

		///// <summary>
		///// Returns a larger bitmap containing a rotated version of the given image.
		///// </summary>
		///// <param name="rotateMe"></param>
		///// <param name="angle"></param>
		///// <returns></returns>
		//private Bitmap RotateImage(Image rotateMe, float angle)
		//{
		//	//First, re-center the image in a larger image that has a margin/frame
		//	//to compensate for the rotated image's increased size

		//	var bmp = new Bitmap(rotateMe.Width + (rotateMe.Width / 2), rotateMe.Height + (rotateMe.Height / 2));

		//	using (Graphics g = Graphics.FromImage(bmp))
		//		g.DrawImageUnscaled(rotateMe, (rotateMe.Width / 4), (rotateMe.Height / 4), bmp.Width, bmp.Height);

		//	//			bmp.Save("moved.png");
		//	rotateMe = bmp;

		//	//Now, actually rotate the image
		//	Bitmap rotatedImage = new Bitmap(rotateMe.Width, rotateMe.Height);

		//	using (Graphics g = Graphics.FromImage(rotatedImage))
		//	{
		//		g.TranslateTransform(rotateMe.Width / 2, rotateMe.Height / 2);   //set the rotation point as the center into the matrix
		//		g.RotateTransform(angle);                                        //rotate
		//		g.TranslateTransform(-rotateMe.Width / 2, -rotateMe.Height / 2); //restore rotation point into the matrix
		//		g.DrawImage(rotateMe, new Point(0, 0));                          //draw the image on the new bitmap
		//	}

		//	//		rotatedImage.Save("rotated.png");
		//	rotateMe.Dispose();
		//	return rotatedImage;
		//}

		///// <summary>
		///// Draws an image in a parallelogram defined by the given coordinates.
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		///// <param name="srcRegion">If not null, identifies the part (in pixels) of the image that is to be drawn.
		///// If null, the whole image is drawn. </param>
		//public void DrawImage(Image image, ICoordinate topLeft, ICoordinate topRight, ICoordinate bottomLeft, RectangleF? srcRegion = null)
		//{
		//	try
		//	{
		//		PointF p1 = ToView(topLeft);
		//		PointF p2 = ToView(topRight);
		//		PointF p3 = ToView(bottomLeft);

		//		PointF[] pts = new[] { p1, p2, p3 };

		//		if (srcRegion != null)
		//			_currentGraphics.DrawImage(image, pts, srcRegion.Value, GraphicsUnit.Pixel);
		//		else
		//			_currentGraphics.DrawImage(image, pts);

		//		if (CalculateBoundingRectangle)
		//		{
		//			UpdateBoundingRectangle(p1.X, p1.Y);
		//			UpdateBoundingRectangle(p2.X, p2.Y);
		//		}
		//	}
		//	catch (Exception) { }
		//}

		///// <summary>
		///// Draws an image at the given coordinate.
		///// May only be used by a DrawableItem during a paint event.
		///// </summary>
		///// <param name="bitmap">The bitmap</param>
		///// <param name="upperLeft">The coordinate of the upper left corner of the bitmap</param>
		//public void DrawImage(Image image, ICoordinate upperLeft)
		//{
		//	try
		//	{
		//		PointF p = ToView(upperLeft);
		//		_currentGraphics.DrawImage(image, p);
		//		if (CalculateBoundingRectangle)
		//		{
		//			UpdateBoundingRectangle(p.X, p.Y);
		//			UpdateBoundingRectangle(p.X + image.Size.Width, p.Y + image.Size.Height);
		//		}
		//	}
		//	catch (Exception) { }
		//}

		///// <summary>
		///// Measures the specified string when drawn with the specified Font.
		///// </summary>
		//public SizeF MeasureString(string text, Font font)
		//{
		//	return _viewPanel.CreateGraphics().MeasureString(text, font);
		//}

		//#endregion

		//#endregion

		////#region Public events

		/////// <summary>
		/////// Passes the MouseUp event further up
		/////// </summary>
		////public new event MouseEventHandler MouseUp
		////{
		////	add { _viewPanel.MouseUp += value; }
		////	remove { _viewPanel.MouseUp -= value; }
		////}

		/////// <summary>
		/////// Passes the MouseClick event further up if not used to drag
		/////// </summary>
		////public new event MouseEventHandler MouseClick;

		/////// <summary>
		/////// Passes the MouseMove event further up
		/////// </summary>
		////public new event MouseEventHandler MouseMove
		////{
		////	add { _viewPanel.MouseMove += value; }
		////	remove { _viewPanel.MouseMove -= value; }
		////}

		/////// <summary>
		/////// Passes the KeyDown event further up
		/////// </summary>
		////public new event KeyEventHandler KeyDown
		////{
		////	add { (_viewPanel as Control).KeyDown += value; }
		////	remove { (_viewPanel as Control).KeyDown -= value; }
		////}

		/////// <summary>
		/////// Passes the KeyUp event further up
		/////// </summary>
		////public new event KeyEventHandler KeyUp
		////{
		////	add { (_viewPanel as Control).KeyUp += value; }
		////	remove { (_viewPanel as Control).KeyUp -= value; }
		////}

		/////// <summary>
		/////// Passes the KeyPress event further up
		/////// </summary>
		////public new event KeyPressEventHandler KeyPress
		////{
		////	add { (_viewPanel as Control).KeyPress += value; }
		////	remove { (_viewPanel as Control).KeyPress -= value; }
		////}

		/////// <summary>
		/////// Passes the PreviewKeyDown event further up
		/////// </summary>
		////public new event PreviewKeyDownEventHandler PreviewKeyDown
		////{
		////	add { (_viewPanel as Control).PreviewKeyDown += value; }
		////	remove { (_viewPanel as Control).PreviewKeyDown -= value; }
		////}

		/////// <summary>
		/////// Raised when the user has selected a rectangle in the view using
		/////// click and drag.
		/////// </summary>
		////public event EventHandler<RectangleEventArgs> RectangleSelected;

		////#endregion

		//#region Private methods

		///// <summary>
		///// Enable arrow keys for key down
		///// </summary>
		///// <param name="keyData"></param>
		///// <returns></returns>
		//protected override bool ProcessDialogKey(Keys keyData)
		//{
		//	if (EnableArrowKeys)
		//	{
		//		if (keyData.HasFlag(Keys.Up) ||
		//			keyData.HasFlag(Keys.Down) ||
		//			keyData.HasFlag(Keys.Left) ||
		//			keyData.HasFlag(Keys.Right))
		//			return false;
		//	}
		//	return base.ProcessDialogKey(keyData);
		//}

		//private static Brush EllipseGradientBrush(float x, float y, float width, float height, Color centerColor, Color surroundColor)
		//{
		//	GraphicsPath path = new GraphicsPath();
		//	path.AddEllipse(x, y, width, height);

		//	PathGradientBrush brush = new PathGradientBrush(path);
		//	//brush.CenterPoint = new PointF(0f, 0f);
		//	brush.CenterColor = centerColor;
		//	Color[] surroundColors = { surroundColor };
		//	brush.SurroundColors = surroundColors;
		//	return brush;
		//}

		//private void FillPolygon(PointF[] points, Brush brush)
		//{
		//	_currentGraphics?.FillPolygon(brush, points);

		//	if (CalculateBoundingRectangle)
		//	{
		//		float minX = float.MaxValue, maxX = float.MinValue;
		//		float minY = float.MaxValue, maxY = float.MinValue;

		//		foreach (var p in points)
		//		{
		//			if (p.X < minX)
		//				minX = p.X;
		//			if (p.X > maxX)
		//				maxX = p.X;
		//			if (p.Y < minY)
		//				minY = p.Y;
		//			if (p.Y > maxY)
		//				maxY = p.Y;
		//		}
		//		UpdateBoundingRectangle(minX, minY);
		//		UpdateBoundingRectangle(maxX, maxY);
		//	}
		//}

		///// <summary>
		///// Extends the bounding rectangle to include the given point
		///// </summary>
		//private void UpdateBoundingRectangle(float x, float y)
		//{
		//	if (x < _minX)
		//		_minX = x;
		//	if (x > _maxX)
		//		_maxX = x;
		//	if (y < _minY)
		//		_minY = y;
		//	if (y < _maxY)
		//		_maxY = y;
		//}

		///// <summary>
		///// Converts the line segment between the given geocoordinates to view coordinates and
		///// excludes parts that are far outside the view
		///// </summary>
		///// <param name="from">The line's start point</param>
		///// <param name="to">The line's end point</param>
		///// <returns>The line in view coordinates, or null if nothing is visible</returns>
		//private LineSegmentInView LineInView(ICoordinate from, ICoordinate to, float width)
		//{
		//	if (_currentGraphics?.IsVisibleClipEmpty ?? true)
		//		return null;

		//	PointF from1 = ToView(from);
		//	PointF to1 = ToView(to);

		//	// To account for the curvature of long lines due to projection, we add
		//	// a tolerance of 10% of the line length

		//	float dx = from1.X - to1.X;
		//	float dy = from1.Y - to1.Y;
		//	float viewLengh = (float)Math.Sqrt(dx * dx + dy * dy);

		//	float tolerance = width + viewLengh * 0.1f;

		//	if (!LineMayIntersectVisibleBounds(from1, to1, tolerance))
		//		return null;

		//	double scale = _currentGraphics.VisibleClipBounds.Height + _currentGraphics.VisibleClipBounds.Width;

		//	if (viewLengh < scale)
		//		// The line is not long compared to the view size. Keep all
		//		return new LineSegmentInView(from1, to1);

		//	// The line is long compared to the view size. 
		//	// This has been seen to affect drawing performance.
		//	// Split it in two.

		//	ICoordinate mid = from.Interpolated(to, 0.5, 0.1);

		//	PointF mid1 = ToView(mid);

		//	// Collect the visible part of each half

		//	LineSegmentInView line1 = LineInView(from, mid, width);
		//	LineSegmentInView line2 = LineInView(mid, to, width);

		//	if (line1 == null)
		//		return line2;
		//	if (line2 == null)
		//		return line1;

		//	return new LineSegmentInView(line1.From, line2.To);
		//}

		///// <summary>
		///// Returns true if the straight line between p1 and p2 (in view coordinates) possibly
		///// intersects the view (_currentGraphics.VisibleClipBounds).
		///// If the function returns false, the line is definitely not visible
		///// </summary>
		//private bool LineMayIntersectVisibleBounds(PointF p1, PointF p2, float width)
		//{
		//	var bounds = _currentGraphics.VisibleClipBounds;

		//	if (p1.X < bounds.Left - width && p2.X < bounds.Left - width)
		//		return false;

		//	if (p1.X > bounds.Right + width && p2.X > bounds.Right + width)
		//		return false;

		//	if (p1.Y < bounds.Top - width && p2.Y < bounds.Top - width)
		//		return false;

		//	if (p1.Y > bounds.Bottom + width && p2.Y > bounds.Bottom + width)
		//		return false;

		//	return true;
		//}

		//private static PointF Interpolate(PointF from, PointF to, double fraction)
		//{
		//	return new PointF((float)(from.X * (1 - fraction) + to.X * fraction), (float)(from.Y * (1 - fraction) + to.Y * fraction));
		//}

		//private static float Length(SizeF vector)
		//{
		//	return (float)Math.Sqrt(vector.Width * vector.Width + vector.Height * vector.Height);
		//}

		//private static SizeF Scale(SizeF vector, float scale)
		//{
		//	return new SizeF(vector.Width * scale, vector.Height * scale);
		//}

		//private static SizeF Diff(PointF from, PointF to)
		//{
		//	return new SizeF(to.X - from.X, to.Y - from.Y);
		//}

		//private static double DistanceToLine(PointF from, PointF to, PointF point)
		//{
		//	double ptdist = Math.Min(DistanceBetween(from, point), DistanceBetween(to, point));

		//	if (DistanceBetween(from, to) == 0)
		//		return ptdist;

		//	var l = Diff(from, to);
		//	var d = Diff(from, point);

		//	var s = InnerProduct(l, d);
		//	var ll = InnerProduct(l, l);

		//	if (s < 0 || s > ll)
		//		// Point is outside line segment
		//		return ptdist;

		//	var n = new SizeF(l.Height, -l.Width);

		//	return Math.Abs(InnerProduct(d, n) / Length(n));
		//}

		//private static double InnerProduct(SizeF l1, SizeF l2)
		//{
		//	return l1.Width * l2.Width + l1.Height * l2.Height;
		//}

		//private static double DistanceBetween(PointF p1, PointF p2)
		//{
		//	var diff = Diff(p2, p1);
		//	return Length(diff);
		//}

		///// <summary>
		///// Scrolls the static layers as necessary to match the view parameters, invalidating
		///// the parts that become exposed
		///// </summary>
		//private void ScrollStaticLayers()
		//{
		//	// If the scale is 0 then do nothing
		//	if (ZoomScale == 0)
		//		return;

		//	// If the panel size is 0 either in width or height, then do nothing (This prevents an error in minimizing windows that contain the control)
		//	if (_viewPanel.Size.Width <= 0 || _viewPanel.Size.Height <= 0)
		//		return;

		//	Bitmap oldBitMap = _bitmaps[_currentBitmap];
		//	if (_currentBitmap == 0)
		//		_currentBitmap = 1;
		//	else
		//		_currentBitmap = 0;

		//	Bitmap currentBitmap = _bitmaps[_currentBitmap];
		//	if (currentBitmap == null || currentBitmap.Size != _viewPanel.Size && !_viewPanel.Size.IsEmpty)
		//		currentBitmap = _bitmaps[_currentBitmap] = new Bitmap(_viewPanel.Size.Width, _viewPanel.Size.Height);

		//	if (oldBitMap != null)
		//	{
		//		Graphics g = Graphics.FromImage(currentBitmap);

		//		Rectangle rect = new Rectangle();

		//		var delta = ToView(_bitmapOriginX, _bitmapOriginY);
		//		rect.X = (int)delta.X;
		//		rect.Y = (int)delta.Y;
		//		rect.Width = oldBitMap.Width;
		//		rect.Height = oldBitMap.Height;

		//		g.DrawImageUnscaledAndClipped(oldBitMap, rect);

		//		Region updateRegion = new Region();
		//		updateRegion.Xor(rect);

		//		_validRect = rect;

		//		RefreshStaticLayers(updateRegion);

		//		_validRect = null;
		//	}
		//	else
		//		RefreshStaticLayers();

		//	_viewPanel.Refresh();
		//}

		//private void RefreshAll()
		//{
		//	RefreshStaticLayers();
		//	_viewPanel.Refresh();
		//}

		//#endregion

		//#region GUI event handlers

		///// <summary>
		///// Event handler: Control is loaded
		///// </summary>
		//private void NetworkViewControl_Load(object sender, EventArgs e)
		//{

		//}

		///// <summary>
		///// Event handler: View panel being painted
		///// </summary>
		//private void _viewPanel_Paint(object sender, PaintEventArgs e)
		//{
		//	if (!ViewIsSet)
		//		SetScaleToFitAllDrawers();

		//	if (SuppressRedraw)
		//		return;

		//	// Check if any of the static layers have changed shown status since they were last repainted
		//	int i = 0;
		//	bool redrawStaticLayers = false;
		//	foreach (var item in _drawableItems)
		//	{
		//		if (item.Layer <= _highestStaticLayer)
		//		{
		//			if (i >= _staticLayerShown.Count)
		//			{
		//				redrawStaticLayers = true;
		//				break;
		//			}
		//			if (item.Show ^ _staticLayerShown[i])
		//			{
		//				redrawStaticLayers = true;
		//				break;
		//			}
		//			++i;
		//		}
		//	}

		//	// Redraw the static layers when some of them has changed value of Show property
		//	if (redrawStaticLayers)
		//		RefreshStaticLayers();

		//	// Paint event args are only in use in GetBoundingBoxStatus currently, consider removing it entirely
		//	_paintEventArgs = e;
		//	_currentGraphics = e.Graphics;

		//	// Draw static layer bitmap to screen
		//	if (_bitmaps[_currentBitmap] != null)
		//		_currentGraphics.DrawImage(_bitmaps[_currentBitmap], new Point(0, 0));

		//	// Draw dynamic layers on top of the static
		//	foreach (var item in _drawableItems)
		//	{
		//		if (item.Layer > _highestStaticLayer)
		//		{
		//			if (item.Show)
		//				item.Draw(this);
		//		}
		//	}

		//	_paintEventArgs = null;
		//	_currentGraphics = null;
		//}

		///// <summary>
		///// Event handler: Pan up button clicked
		///// </summary>
		//private void _panUp_Click(object sender, EventArgs e)
		//{
		//	Pan(0, -50);
		//}

		///// <summary>
		///// Event handler: Pan down button clicked
		///// </summary>
		//private void _panDown_Click(object sender, EventArgs e)
		//{
		//	Pan(0, 50);
		//}

		///// <summary>
		///// Event handler: Pan left button clicked
		///// </summary>
		//private void _panLeft_Click(object sender, EventArgs e)
		//{
		//	Pan(-50, 0);
		//}

		///// <summary>
		///// Event handler: Pan right button clicked
		///// </summary>
		//private void _panRight_Click(object sender, EventArgs e)
		//{
		//	Pan(50, 0);
		//}

		///// <summary>
		///// Event handler: Zoom in button clicked
		///// </summary>
		//private void _zoomIn_Click(object sender, EventArgs e)
		//{
		//	ZoomIn();
		//}

		///// <summary>
		///// Event handler: The view panel catches the mouse wheel event
		///// </summary>
		//private void ViewPanelMouseWheelEvent(object sender, MouseEventArgs e)
		//{
		//	if (!_allowZoom)
		//		return;

		//	if (ModifierKeys.HasFlag(Keys.Control))
		//	{
		//		double deltaAngle = e.Delta / 12;
		//		RotateTo(RotationAngle + deltaAngle);
		//	}
		//	else
		//	{
		//		const double zoomSpeed = 120;
		//		double deltaFactor = Math.Pow(1.2, e.Delta / zoomSpeed);
		//		Zoom(e.Location, deltaFactor);
		//	}
		//}

		///// <summary>
		///// Event handler: View panel resized
		///// </summary>
		//private void _viewPanel_Resize(object sender, EventArgs e)
		//{
		//	ScrollStaticLayers();
		//}

		///// <summary>
		///// Event handler: Zoom out button clicked
		///// </summary>
		//private void _zoomOut_Click(object sender, EventArgs e)
		//{
		//	ZoomOut();
		//}

		///// <summary>
		///// Event handler: Mouse button pressed in view
		///// </summary>
		//private void _viewPanel_MouseDown(object sender, MouseEventArgs e)
		//{
		//	_mouseDownEventArgs = e;
		//	_mouseDownXCenter = _xCenter;
		//	_mouseDownYCenter = _yCenter;

		//	if (e.Button == System.Windows.Forms.MouseButtons.Left)
		//	{
		//		bool shiftPressed = ModifierKeys.HasFlag(Keys.Shift);
		//		bool controlPressed = ModifierKeys.HasFlag(Keys.Control);

		//		_pendingDragOperation = DragOperationSelector(shiftPressed, controlPressed);
		//		_dragOperation = DragOperation.None;
		//	}

		//	_lastClickWasDrag = false;
		//}

		///// <summary>
		///// Event handler: Mouse button released in view
		///// </summary>
		//private void _viewPanel_MouseUp(object sender, MouseEventArgs e)
		//{
		//	if (e.Button == System.Windows.Forms.MouseButtons.Left)
		//	{
		//		if (_dragOperation == DragOperation.SelectRectangle)
		//		{
		//			RectangleSelected?.Invoke(this, new RectangleEventArgs(_rectangleDrawer.Rectangle));

		//			RemoveDrawableItem(_rectangleDrawer);
		//			_rectangleDrawer = null;
		//		}

		//		_pendingDragOperation = DragOperation.None;
		//		_dragOperation = DragOperation.None;
		//	}

		//	if (!_lastClickWasDrag && MouseClick != null)
		//		MouseClick(this, _mouseDownEventArgs);
		//}

		///// <summary>
		///// Returns the size of a rectangle (in pixels) as it should be displayed in the view,
		///// based on the given physical size (in m).
		///// </summary>
		///// <param name="realSize"></param>
		///// <returns></returns>
		//public Size ToView(SizeF realSize)
		//{
		//	double ppm = PixelsPerMeter();
		//	Size c = new Size((int)(realSize.Width * ppm), (int)(realSize.Height * ppm));
		//	return c;
		//}

		///// <summary>
		///// Event handler: Mouse moved in view
		///// </summary>
		//private void _viewPanel_MouseMove(object sender, MouseEventArgs e)
		//{
		//	int mouseDx = e.X - _mouseDownEventArgs?.Location.X ?? 0;
		//	int mouseDy = e.Y - _mouseDownEventArgs?.Location.Y ?? 0;

		//	if (_pendingDragOperation != DragOperation.None)
		//	{
		//		bool mouseHasMoved = (Math.Abs(mouseDx) > 5 || Math.Abs(mouseDy) > 5);

		//		if (mouseHasMoved)
		//		{
		//			_dragOperation = _pendingDragOperation;
		//			_lastClickWasDrag = true;
		//		}
		//	}

		//	if (_dragOperation == DragOperation.Pan)
		//	{
		//		_viewPanel.Cursor = System.Windows.Forms.Cursors.Hand;

		//		var startCenter = ToView(_mouseDownXCenter, _mouseDownYCenter);
		//		var center = ToView(_xCenter, _yCenter);

		//		// This is how much we have panned previously during this drag operation:
		//		var viewDx = startCenter.X - center.X;
		//		var viewDy = startCenter.Y - center.Y;

		//		Pan(viewDx - mouseDx, viewDy - mouseDy);
		//	}
		//	else if (_dragOperation == DragOperation.SelectRectangle)
		//	{
		//		if (_rectangleDrawer == null)
		//		{
		//			int maxLayer = _drawableItems?.Max(item => item.Layer) ?? 0;
		//			_rectangleDrawer = new SelectionRectangleDrawer(Math.Max(_highestStaticLayer, maxLayer) + 1);
		//			AddDrawableItem(_rectangleDrawer, false);
		//		}

		//		_rectangleDrawer.StartCorner = _mouseDownEventArgs.Location;
		//		_rectangleDrawer.EndCorner = e.Location;

		//		_viewPanel.Refresh();
		//	}
		//	else
		//	{
		//		_viewPanel.Cursor = System.Windows.Forms.Cursors.Cross;
		//	}

		//	// Update coordinate text boxes

		//	ICoordinate c = FromView(e.Location);
		//	_currentMouseLocation = c;
		//	_lat.Text = c.Y.ToString();
		//	_lon.Text = c.X.ToString();

		//	if (_coordinateSystem == null)
		//		_utmE.Text = _utmN.Text = "";
		//	else
		//	{
		//		_utmLabel.Text = "UTM " + _coordinateSystem.UtmZone + " E:";
		//		double east, north;
		//		ToMap(c, out east, out north);
		//		_utmE.Text = ((int)east).ToString();
		//		_utmN.Text = ((int)north).ToString();
		//	}
		//}

		///// <summary>
		///// Event handler: Mouse enters view
		///// </summary>
		//private void _viewPanel_MouseEnter(object sender, EventArgs e)
		//{
		//	_allowZoom = true;
		//}

		///// <summary>
		///// Event handler: Mouse leaves view
		///// </summary>
		//private void _viewPanel_MouseLeave(object sender, EventArgs e)
		//{
		//	_pendingDragOperation = DragOperation.None;
		//	_dragOperation = DragOperation.None;
		//	_allowZoom = false;
		//}

		//private void _viewPanel_MouseClick(object sender, MouseEventArgs e)
		//{
		//	_viewPanel.Focus();
		//}

		//private void _viewPanel_MouseDoubleClick(object sender, MouseEventArgs e)
		//{
		//	// Zooms out is shift key is held
		//	double zoomfactor = 1.5;
		//	if (ModifierKeys.HasFlag(Keys.Shift))
		//		zoomfactor = 1 / zoomfactor;

		//	Zoom(e.Location, zoomfactor);
		//}

		//#endregion

		//	#region Inner types

		///// <summary>
		///// A status for a bounding box
		///// </summary>
		//public enum BoundingBoxStatus
		//{
		//	/// <summary>
		//	/// The box is small; its diagonal is not larger than a given number of pixels
		//	/// </summary>
		//	Small,
		//	/// <summary>
		//	/// The box is completely outside the part of the view being redrawn
		//	/// </summary>
		//	OutsideView,
		//	/// <summary>
		//	/// The box is not small and at least part of it may overlap the arew being redrawn
		//	/// </summary>
		//	Normal
		//}

		///// <summary>
		///// A type of drag operation
		///// </summary>
		//public enum DragOperation
		//{
		//	/// <summary>
		//	/// Nothing happens
		//	/// </summary>
		//	None,

		//	/// <summary>
		//	/// The view is panned
		//	/// </summary>
		//	Pan,

		//	/// <summary>
		//	/// A rectangle is selected in the view
		//	/// </summary>
		//	SelectRectangle
		//}

		///// <summary>
		///// A line segment in view coordinates
		///// </summary>
		//private class LineSegmentInView
		//{
		//	/// <summary>
		//	/// The line's start point
		//	/// </summary>
		//	public PointF From;

		//	/// <summary>
		//	/// The line's end point
		//	/// </summary>
		//	public PointF To;

		//	/// <summary>
		//	/// Constructor
		//	/// </summary>
		//	public LineSegmentInView(PointF from, PointF to)
		//	{
		//		From = from;
		//		To = to;
		//	}
		//}

		///// <summary>
		///// Event arguments containing a rectangle
		///// </summary>
		//public class RectangleEventArgs : EventArgs
		//{
		//	/// <summary>
		//	/// The rectangle
		//	/// </summary>
		//	public Rectangle Rectangle { get; }

		//	public RectangleEventArgs(Rectangle rectangle)
		//	{
		//		Rectangle = rectangle;
		//	}
		//}

		///// <summary>
		///// Draws the selection rectangle
		///// </summary>
		//private class SelectionRectangleDrawer : DrawableItem
		//{
		//	public Point StartCorner { get; internal set; }

		//	public Point EndCorner { get; internal set; }

		//	public Rectangle Rectangle
		//	{
		//		get
		//		{
		//			int minX = Math.Min(StartCorner.X, EndCorner.X);
		//			int maxX = Math.Max(StartCorner.X, EndCorner.X);
		//			int minY = Math.Min(StartCorner.Y, EndCorner.Y);
		//			int maxY = Math.Max(StartCorner.Y, EndCorner.Y);

		//			return new Rectangle(minX, minY, maxX - minX, maxY - minY);
		//		}
		//	}

		//	Pen _pen = new Pen(Color.Black);

		//	public SelectionRectangleDrawer(int layer) : base(layer)
		//	{
		//	}

		//	public override IEnumerable<ICoordinate> Extent => new ICoordinate[0];

		//	public override void Draw(NetworkViewControl view)
		//	{
		//		view.DrawRectangle(Rectangle, _pen);
		//	}
		//}


		//#endregion
	}
}

