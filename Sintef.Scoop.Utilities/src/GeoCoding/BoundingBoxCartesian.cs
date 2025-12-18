//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;

namespace Sintef.Scoop.Utilities.GeoCoding
{
	/// <summary>
	/// A cartesian <see cref="Coordinate"/> bounding box.
	/// </summary>
	/// <remarks>
	/// A bounding box represents an area on the "horisontal" or ground surface of a coordinate system.
	/// </remarks>
	public class BoundingBoxCartesian : BoundingBoxBase<Coordinate>
	{
		/// <summary>
		/// Default constructor. Creates a bounding box of zero extent, at (0,0).
		/// </summary>
		public BoundingBoxCartesian():base()
		{
		}

		/// <summary>
		/// Constructor. Creates a bounding box that covers the given coordinate 
		/// (and nothing else)
		/// </summary>
		public BoundingBoxCartesian(Coordinate coordinate):base(coordinate)
		{
		}

		/// <summary>
		/// Constructor. Creates a bounding box that covers the given intervals
		/// </summary>
		public BoundingBoxCartesian(double minY, double maxY,
									 double minX, double maxX):base(minY,maxY,minX,maxX)
		{
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		public BoundingBoxCartesian(BoundingBoxCartesian other):base(other)
		{
		}

		/// <summary>
		/// Creates a clone of this bounding box
		/// </summary>
		public override BoundingBoxBase<Coordinate> Clone()
		{
			return new BoundingBoxCartesian(this);
		}

		/// <summary>
		/// Returns true if the given coordinate is inside the 
		/// bounding box, false if not
		/// </summary>
		public override bool Contains(ICoordinate coord)
		{
			if (MinY > coord.Y + contain_tol_)
				return false;
			if (MaxY < coord.Y - contain_tol_)
				return false;

			if (MaxX < coord.X - contain_tol_)
				return false;
			if (MinX > coord.X + contain_tol_)
				return false;

			return true;
		}

		/// <summary>
		/// Returns true if the given bounding box is entirely inside this
		/// bounding box, false if not
		/// </summary>
		public override bool Contains(IBoundingBox other)
		{
			if (MinY > other.MinY + contain_tol_)
				return false;
			if (MaxY < other.MaxY - contain_tol_)
				return false;
			if (other.MinX < MinX - contain_tol_)
				return false;
			if (other.MaxX > MaxX + contain_tol_)
				return false;

			return true;
		}

		/// <summary>
		/// Returns true if this bounding box and the given
		/// bounding box have any coordinate in common.
		/// </summary>
		public override bool Intersects(IBoundingBox other)
		{
			if (MinY > other.MaxY)
				return false;
			if (MaxY < other.MinY)
				return false;
			if (MaxX < other.MinX)
				return false;
			if (MinX > other.MaxX)
				return false;

			return true;
		}

		/// <summary>
		/// Returns the straigth line distance, in meters, 
		/// between the given coordinate and the closest point on
		/// the edge of the bounding box. The coordinate must lie
		/// inside the bounding box.
		/// </summary>
		protected override double InsideDistance(Coordinate coord)
		{
			Coordinate edgePoint1 = coord.ClosestPoint(new Coordinate(MinX, MinY), new Coordinate(MinX, MaxY));
			double d1 = coord.DistanceTo(edgePoint1);
			Coordinate edgePoint2 = coord.ClosestPoint(new Coordinate(MaxX, MinY), new Coordinate(MaxX, MaxY));
			double d2 = coord.DistanceTo(edgePoint2);
			Coordinate edgePoint3 = coord.ClosestPoint(new Coordinate(MinX, MinY), new Coordinate(MaxX, MinY));
			double d3 = coord.DistanceTo(edgePoint3);
			Coordinate edgePoint4 = coord.ClosestPoint(new Coordinate(MinX, MaxY), new Coordinate(MaxX, MaxY));
			double d4 = coord.DistanceTo(edgePoint4);

			return Math.Min(Math.Min(d1, d2), Math.Min(d3,d4));
		}

		/// <summary>
		/// Returns the minimum distance, in meters, 
		/// between the given coordinate and the closest point on
		/// the edge of the bounding box. The coordinate must lie
		/// outside the bounding box.
		/// </summary>
		protected override double OutsideDistance(Coordinate coord) => InsideDistance(coord);

		/// <summary>
		/// Expands the bounding box, if necessary, to ensure that 
		/// the given coordinate is contained by this bounding box.
		/// </summary>
		public override void ExpandBy(ICoordinate coord)
		{
			double y = coord.Y;
			if (y > MaxY)
				MaxY = y;
			if (y < MinY)
				MinY = y;

			double x = coord.X;
			if (x > MaxX)
				MaxX = x;
			if (x < MinX)
				MinX = x;
		}

		/// <summary>
		/// Expands the bounding box, if necessary, to ensure that 
		/// the given bounding box is entirely contained by this 
		/// bounding box.
		/// </summary>
		public override void ExpandBy(IBoundingBox other)
		{
			if (other.MinY < MinY)
				MinY = other.MinY;
			if (other.MaxY > MaxY)
				MaxY = other.MaxY;
			if (other.MinX < MinX)
				MinX = other.MinX;
			if (other.MaxX > MaxX)
				MaxX = other.MaxX;
		}

		/// <summary>
		/// Expands the bounding box to include any point within
		/// distance \p d (in meters) of the original box
		/// </summary>
		public override void ExpandBy(double d)
		{
			MinX -= d;
			MinY -= d;
			MaxX += d;
			MaxY += d;
		}
	}
}
