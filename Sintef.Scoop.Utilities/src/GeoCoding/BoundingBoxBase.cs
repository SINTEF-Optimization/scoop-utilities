//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;

namespace Sintef.Scoop.Utilities.GeoCoding
{
	/// <summary>
	/// A coordinate bounding box.
	/// </summary>
	/// <remarks>
	/// A bounding box represents an area on the "horisontal" or ground surface of a coordinate system.
	/// </remarks>
	/// <typeparam name="C">They type of coordinates that are used.</typeparam>
	public abstract class BoundingBoxBase<C> : IBoundingBox where C : ICoordinate
	{
		#region Public properties

		/// <summary>
		/// The minimum north/up coordinate.
		/// </summary>
		public double MinY { get { return _min_y; } protected set { _min_y = value; } }

		/// <summary>
		/// The maximum north/up coordinate covered by the bounding box
		/// </summary>
		public double MaxY { get { return _max_y; } protected set { _max_y = value; } }

		/// <summary>
		/// The minimum east/right covered by the bounding box (can be negative, which means west/left).
		/// </summary>
		public double MinX { get { return _min_x; } protected set { _min_x = value; } }

		/// <summary>
		/// The maximum east/right covered by the bounding box (can be negative, which means west/left).
		/// </summary>
		public double MaxX { get { return _max_x; } protected set { _max_x = value; } }

		/// <summary>
		/// The area (in square coordinate units, whatever they are) of the bounding box
		/// </summary>
		public double Area
		{
			get
			{
				// Not correct! This assumes planar geometry (equirectangular projection)
				return (MaxY - MinY) * (MaxX - MinX);
			}
		}

		#endregion

		#region Private data members

		/// <summary>
		/// The north/up defining the southern/lower border of the bounding box
		/// </summary>
		double _min_y;

		/// <summary>
		/// The north/up defining the northern/upper border of the 
		/// bounding box
		/// </summary>
		double _max_y;

		/// <summary>
		/// The rigth/east defining the western/leftmost border of the 
		/// bounding box
		/// </summary>
		double _min_x;

		/// <summary>
		/// The right/east defining the eastern/rightmost border of the 
		/// bounding box
		/// </summary>
		double _max_x;

		/// <summary>
		/// This tolerance is used when checking for containment
		/// (both for coordinates and boxes),
		/// to avoid numerical problems. Thus some points slightly
		/// outside a bounding box may be categorised as inside.
		/// </summary>
		static protected double contain_tol_ = 360 * 1e-14;

		#endregion

		/// <summary>
		/// Default constructor. Creates a bounding box of zero extent, at (0,0).
		/// </summary>
		public BoundingBoxBase()
		{
			_min_y = _max_y = 0;
			_min_x = _max_x = 0;
		}

		/// <summary>
		/// Constructor. Creates a bounding box that covers the given coordinate 
		/// (and nothing else)
		/// </summary>
		public BoundingBoxBase(C coordinate)
		{
			_min_y = _max_y = coordinate.Y;
			_min_x = _max_x = coordinate.X;
		}

		/// <summary>
		/// Constructor. Creates a bounding box that covers the given intervals
		/// </summary>
		public BoundingBoxBase(double minY, double maxY,
									 double minX, double maxX)
		{
			if (typeof(C) == typeof(GeoCoordinate))
			{
				if (minY < -90 || maxY < minY || 90 < maxY)
					throw new ArgumentException(string.Format("Illegal latitude interval [{0}, {1}]", minY, maxY));

				if (minX < -180 || minX >= 180 || maxX < minX ||
						maxX > minX + 360)
					throw new ArgumentException(string.Format("Illegal longitude interval [{0}, {1}]", minX, maxX));
			}

			_min_y = minY;
			_max_y = maxY;
			_min_x = minX;
			_max_x = maxX;
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		public BoundingBoxBase(BoundingBoxBase<C> other)
			: this(other._min_y, other._max_y, other._min_x, other._max_x)
		{
		}

		/// <summary>
		/// Creates a clone of this bounding box
		/// </summary>
		public abstract BoundingBoxBase<C> Clone();

		/// <summary>
		/// Returns true if the given cooridinate is inside the 
		/// bounding box, false if not
		/// </summary>
		public abstract bool Contains(ICoordinate coord);

		/// <summary>
		/// Returns true if the given bounding box is entirely inside this
		/// bounding box, false if not
		/// </summary>
		public abstract bool Contains(IBoundingBox other);

		/// <summary>
		/// Returns true if this bounding box and the given
		/// bounding box have any coordinate in common.
		/// </summary>
		public abstract bool Intersects(IBoundingBox other);

		/// <summary>
		/// Returns the minimum distance, in meters, along the surface,
		/// between the given coordinate and the closest point in 
		/// the bounding box. If the point is inside the bounding 
		/// box, returns 0.
		/// </summary>
		public double MinDistance(ICoordinate coord)
		{
			if (Contains(coord))
				return 0.0;
			else
				return OutsideDistance((C) coord);
		}

		/// <summary>
		/// Returns the distance, in meters, along a Great Circle, 
		/// between the given coordinate and the closest point on
		/// the edge of the bounding box.
		/// </summary>
		public double DistFromEdge(C coord)
		{
			if (Contains(coord))
				return InsideDistance(coord);
			else
				return OutsideDistance(coord);
		}

		/// <summary>
		/// Returns the minimum distance, in meters,  
		/// between the given coordinate and the closest point on
		/// the edge of the bounding box. The coordinate must lie
		/// inside the bounding box.
		/// </summary>
		protected abstract double InsideDistance(C coord);

		/// <summary>
		/// Returns the minimum distance, in meters, 
		/// between the given coordinate and the closest point on
		/// the edge of the bounding box. The coordinate must lie
		/// outside the bounding box.
		/// </summary>
		protected abstract double OutsideDistance(C coord);

		/// <summary>
		/// Expands the bounding box, if necessary, to ensure that 
		/// the given coordinate is contained by this bounding box.
		/// </summary>
		public abstract void ExpandBy(ICoordinate coord);


		/// <summary>
		/// Expands the bounding box, if necessary, to ensure that 
		/// the given bounding box is entirely contained by this 
		/// bounding box.
		/// </summary>
		public abstract void ExpandBy(IBoundingBox other);


		/// <summary>
		/// Expands the bounding box to include any point within
		/// distance \p d (in meters) of the original box
		/// </summary>
		public abstract void ExpandBy(double d);

		/// <summary>
		/// Returns the area added if this bounding box is expanded by the
		/// other bounding box
		/// </summary>
		public double ExpansionArea(IBoundingBox other)
		{
			BoundingBoxBase<C> tmp = Clone();
			tmp.ExpandBy(other);
			return tmp.Area - Area;
		}

		/// <summary>
		/// Returns a random coordinate within the bounding box.
		/// </summary>
		public ICoordinate GetRandomCoordinate(Random r)
		{
			double y = MinY + r.NextDouble() * (MaxY - MinY);
			double x = MinX + r.NextDouble() * (MaxX - MinX);
			return ICoordinateExtensions.CreateCoordinate<C>(x, y);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is BoundingBoxBase<C> other &&
				_min_x == other._min_x &&
				_max_x == other._max_x &&
				_min_y == other._min_y &&
				_max_y == other._max_y;
		}

		/// <summary>
		/// Returns a hash code for the bounding box
		/// </summary>
		public override int GetHashCode()
		{
			return _min_x.GetHashCode() + _max_x.GetHashCode() + _min_y.GetHashCode() + _max_y.GetHashCode();
		}
	}

}
