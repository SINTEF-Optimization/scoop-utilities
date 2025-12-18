//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Sintef.Scoop.Utilities.GeoCoding
{
	/// <summary>
	/// A closed region bounded by edges that are straight lines in a cartesian coordinate system
	/// </summary>
	public class ClosedPolygon
	{
		/// <summary>
		/// The bounding box edges of the polygon
		/// </summary>
		private double[] _boundingBox = new double[4];

		/// <summary>
		/// The polygon corners
		/// </summary>
		private List<Coordinate> _corners;

		/// <summary>
		/// The min x value of the bounding box of the polygon
		/// </summary>
		public double BoundingBoxMinX
		{
			get { return _boundingBox[0]; }
			private set { _boundingBox[0] = value; }
		}

		/// <summary>
		/// The max x value of the bounding box of the polygon
		/// </summary>
		public double BoundingBoxMaxX
		{
			get { return _boundingBox[1]; }
			private set { _boundingBox[1] = value; }
		}

		/// <summary>
		/// The min y value of the bounding box of the polygon
		/// </summary>
		public double BoundingBoxMinY
		{
			get { return _boundingBox[2]; }
			private set { _boundingBox[2] = value; }
		}

		/// <summary>
		/// The max y value of the bounding box of the polygon
		/// </summary>
		public double BoundingBoxMaxY
		{
			get { return _boundingBox[3]; }
			private set { _boundingBox[3] = value; }
		}

		/// <summary>
		/// The polygon corners
		/// </summary>
		public IList<Coordinate> Corners { get { return _corners; } }

		/// <summary>
		/// The orientation of the polygon. Positive orientation represents outer edge of a region.
		/// Negative orientation represents holes inside a region.
		/// </summary>
		public bool HasPositiveOrientation { get; private set; }

		/// <summary>
		/// The (positive) area limited by the polygon
		/// </summary>
		public double Area { get; private set; }

		/// <summary>
		/// The number of polygon corners
		/// </summary>
		public int Count { get { return _corners.Count; } }

		/// <summary>
		/// Returns a corner in the polygon
		/// </summary>
		/// <param name="key">The index of the corner</param>
		public Coordinate this[int key] { get { return _corners[key]; } }

		/// <summary>
		/// Sets the orienation of the polygon
		/// </summary>
		private void CalculateOrientationAndBoundingBox()
		{
			// We calculate the double of the area by summing up the cross-products corner[i] x corner[i+1]
			// The area and orientation have the same sign
			double area = 0.0;
			for (int i = 0; i < _corners.Count; ++i)
			{
				int nextIdx = (i + 1) % _corners.Count;
				area += _corners[i].X * _corners[nextIdx].Y - _corners[i].Y * _corners[nextIdx].X;
			}
			HasPositiveOrientation = area >= 0.0;
			Area = 0.5 * Math.Abs(area);

			BoundingBoxMinX = Double.MaxValue;
			BoundingBoxMaxX = Double.MinValue;
			BoundingBoxMinY = Double.MaxValue;
			BoundingBoxMaxY = Double.MinValue;

			foreach (Coordinate gc in _corners)
			{
				BoundingBoxMinX = Math.Min(BoundingBoxMinX, gc.X);
				BoundingBoxMaxX = Math.Max(BoundingBoxMaxX, gc.X);
				BoundingBoxMinY = Math.Min(BoundingBoxMinY, gc.Y);
				BoundingBoxMaxY = Math.Max(BoundingBoxMaxY, gc.Y);
			}
		}

		/// <summary>
		/// A polygon region
		/// </summary>
		/// <param name="corners">The corners as Coordinate objects</param>
		/// <param name="orientation">The expected orientation, true for positive, false for negative</param>
		/// <param name="setOrderFromOrientation">If true, the orientation will be set according to the
		/// 'orientation' parameter, this might reverse the order of the corners.
		/// If false, an error will be raised if expected orientation differs from calculated orientation</param>
		public ClosedPolygon(IEnumerable<Coordinate> corners, bool orientation, bool setOrderFromOrientation = false)
		{
			_corners = new List<Coordinate>();
			_corners.AddRange(corners);
			CalculateOrientationAndBoundingBox();
			if (orientation != HasPositiveOrientation)
			{
				if (setOrderFromOrientation)
				{
					_corners.Reverse();
					HasPositiveOrientation = !HasPositiveOrientation;
				}
				else
					throw new ArgumentException("Polygon corners do not have expected orientation", "corners");
			}
		}

		/// <summary>
		/// A polygon region, where the orientation is defined from the corners
		/// </summary>
		/// <param name="corners">The corners as Coordinate objects</param>
		public ClosedPolygon(IEnumerable<Coordinate> corners)
		{
			_corners = new List<Coordinate>();
			_corners.AddRange(corners);
			CalculateOrientationAndBoundingBox();
		}

		/// <summary>
		/// A polygon region
		/// </summary>
		/// <param name="element">XElement containing xml representation</param>
		public ClosedPolygon(XElement element)
		{
			_corners = new List<Coordinate>();
			XElement corners = element.RequireElement("Corners");
			foreach (XElement c in corners.TagElements("Corner"))
				_corners.Add(new Coordinate(c));
			XElement xOrientation = element.TagElement("HasPositiveOrientation");

			CalculateOrientationAndBoundingBox();
			if (xOrientation != null)
			{
				if (Convert.ToBoolean(xOrientation.Value) != HasPositiveOrientation)
					throw new XmlParseException(element, "computed orientation based on corners does note equal specified orientation");
			}
		}

		/// <summary>
		/// A polygon representing a rectangular axis parallell box in a cartesian coordinate system
		/// </summary>
		/// <param name="minX">The minimum x value of the box</param>
		/// <param name="maxX">The maximum x value of the box</param>
		/// <param name="minY">The minimum y value of the box</param>
		/// <param name="maxY">The maximum y value of the box</param>
		/// <param name="orientation">The orientation of the box</param>
		public ClosedPolygon(double minX, double maxX, double minY, double maxY, bool orientation)
		{
			var lowLeft = new Coordinate(minX, minY);
			var lowRight = new Coordinate(maxX, minY);
			var highRight = new Coordinate(maxX, maxY);
			var highLeft = new Coordinate(minX, maxY);
			if (orientation)
			{
				_corners = new List<Coordinate>() { lowLeft, lowRight, highRight, highLeft };
			}
			else
			{
				_corners = new List<Coordinate>() { lowLeft, highLeft, highRight, lowRight };
			}
			CalculateOrientationAndBoundingBox();
			if (HasPositiveOrientation != orientation)
				throw new Exception("ClosedPolygon with unexpected orientation");
		}

		/// <summary>
		/// Returns an xml representation of this polygon
		/// </summary>
		public XElement ToXml(string elementName)
		{
			XElement orientation = new XElement("HasPositiveOrientation");
			orientation.Value = HasPositiveOrientation.ToString();
			XElement corners = new XElement("Corners");
			foreach(Coordinate corner in _corners)
				corners.Add(corner.ToXml("Corner"));

			XElement element = new XElement(elementName);
			element.Add(orientation);
			element.Add(corners);
			return element;
		}


		/// <summary>
		/// Returns whether the region contains a coordinate
		/// </summary>
		/// <param name="coordinate">The coordinate</param>
		/// <param name="acceptOnEdge">Tells if a coordinate on the edge is considered as contained in the region</param>
		public bool ContainsCoordinate(Coordinate coordinate, bool acceptOnEdge = true)
		{
			if (_corners.Count == 0)
				return false;

			// First check if the point is outside the bounding box
			double cX = coordinate.X;
			double cY = coordinate.Y;
			if (cX < BoundingBoxMinX || cX > BoundingBoxMaxX || cY < BoundingBoxMinY || cY > BoundingBoxMaxY)
				return false;

			// The algorithm is to run through the corners and sum up the changes in
			// the quadrant they lie in relative to the point
			int quadrantChanges = 0;
			int previousQuadrant = 0;
			double previousX = 0.0;
			double previousY = 0.0;

			for (int i = 0; i <= _corners.Count; ++i)
			{
				int idx = i == _corners.Count ? 0 : i;
				double x = _corners[idx].X - cX;
				double y = _corners[idx].Y - cY;
				int quadrant = 0;
				if (x == 0.0 && y == 0.0)
					return acceptOnEdge;  // The point lies on a corner
				else if (x >= 0.0 && y >= 0.0)
					quadrant = 0;
				else if (x <= 0.0 && y <= 0.0)
					quadrant = 2;
				else if (x < 0.0)
					quadrant = 1;
				else
					quadrant = 3;
				if (i > 0)
				{
					int changeQuadrant = quadrant - previousQuadrant;
					if (changeQuadrant == 3)
						changeQuadrant = -1;
					else if (changeQuadrant == -3)
						changeQuadrant = 1;
					else if (changeQuadrant == 2 || changeQuadrant == -2)
					{
						// Goes from one quadrant to the oposite. Use cross product to determine which side
						// the edge lies on
						double crossProd = previousX * y - previousY * x;
						if (crossProd == 0.0)
							return acceptOnEdge;   // The point lies on the edge
						else if (crossProd > 0.0)
							changeQuadrant = 2;
						else
							changeQuadrant = -2;
					}
					quadrantChanges += changeQuadrant;
				}
				previousQuadrant = quadrant;
				previousX = x;
				previousY = y;
			}

			// quadrantChanges is either
			// 0 (point is outside),
			// 4 (inside, polygon orientation is positive) or
			// -4 (inside, polygon orientation is negative)
			return quadrantChanges != 0;
		}

		/// <summary>
		/// Returns whether a part of a segment is contained in the polygon (including the edges)
		/// </summary>
		/// <param name="start">The start point of the segment</param>
		/// <param name="end">The end point of the segment</param>
		public bool ContainsPartOfSegment(Coordinate start, Coordinate end)
		{
			// Check if start or end of segment is inside
			if (ContainsCoordinate(start, true) || ContainsCoordinate(end, true))
				return true;

			// Check if the segment hits any of the polygon edges
			for (int i = 0; i < _corners.Count; ++i)
			{
				int nextIdx = i + 1;
				if (nextIdx == _corners.Count)
					nextIdx = 0;

				Coordinate c0 = _corners[i];
				Coordinate c1 = _corners[nextIdx];

				if (Coordinate.IntersectionInXYPlane(c0, c1, start, end) != null)
					return true;
			}

			return false;
		}
	}
}
