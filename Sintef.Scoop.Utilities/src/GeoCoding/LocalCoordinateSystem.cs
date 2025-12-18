//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;

namespace Sintef.Scoop.Utilities.GeoCoding
{
	/// <summary>
	/// A local coordinate system.
	/// 
	/// Once the local coordinate system is establiched the method ToGlobal converts a local coordinate to a global coordinate.
	/// 
	/// \todo Implement in 3D
	/// 
	/// </summary>
	public class LocalCoordinateSystem
	{
		/// <summary>
		/// Origin of the local coordinate system in global coordinates
		/// </summary>
		Coordinate _origin;

		/// <summary>
		/// Unit vector for the directions of the local x-axis
		/// </summary>
		Coordinate _unitX;

		/// <summary>
		/// Unit vector for the directions of the local y-axis
		/// </summary>
		Coordinate _unitY;

		//Coordinate _unitZ;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="origin">The origin of the local coordinate system in the global coordinate system</param>
		/// <param name="unitX">The direction of the local x-axis in the global coordinate system</param>
		/// <param name="unitY">The direction of the local y-axis in the global coordinate system</param>
		public LocalCoordinateSystem(Coordinate origin, Coordinate unitX, Coordinate unitY)
		{
			_origin = origin;
			_unitX = unitX;
			_unitY = unitY;
		}

		/// <summary>
		/// The length in 2D
		/// </summary>
		public static double Length(double x, double y, double z = double.NaN)
		{
			double rtn = Math.Sqrt(x * x + y * y);
			if (!double.IsNaN(z))
				rtn = Math.Sqrt(rtn * rtn + z * z);
			return rtn;
		}

		/// <summary>
		/// The length in 2D
		/// </summary>
		public static double Length(Coordinate vector)
		{
			return Length(vector.X, vector.Y, vector.Z);
		}

		/// <summary>
		/// Returns a unit vector in the specified direction
		/// </summary>
		public static Coordinate ToUnitVector(double x, double y, double z = double.NaN)
		{
			double length = Length(x, y, z);
			return new Coordinate(x / length, y / length, double.IsNaN(z) ? double.NaN : z / length);
		}

		/// <summary>
		/// Returns a unit vector in the specified direction
		/// </summary>
		public static Coordinate ToUnitVector(Coordinate vector)
		{
			return ToUnitVector(vector.X, vector.Y);
		}

		/// <summary>
		/// Returns the cross product between two vectors
		/// </summary>
		public static Coordinate CrossProduct(Coordinate u, Coordinate v)
		{
			double x = u.Y * v.Z - u.Z * v.Y;
			double y = u.Z * v.X - u.X * v.Z;
			double z = u.X * v.Y - u.Y * v.X;
			return new Coordinate(x, y, z);
		}

		/// <summary>
		/// Converts a local coordinate to a global coordinate
		/// </summary>
		public Coordinate ToGlobal(Coordinate local)
		{
			//
			// |ux.x uy.x| |l.x|  =  |g.x|
			// |ux.y uy.y| |l.y|     |g.y|
			//
			double globalX = _origin.X + _unitX.X * local.X + _unitY.X * local.Y;
			double globalY = _origin.Y + _unitX.Y * local.X + _unitY.Y * local.Y;
			return new Coordinate(globalX, globalY);
		}

		/// <summary>
		/// Converts a sequence of local coordinates to a sequence of global coordinates
		/// </summary>
		public IEnumerable<Coordinate> ToGlobal(IEnumerable<Coordinate> localCoordinates)
		{
			foreach (var c in localCoordinates)
				yield return ToGlobal(c);
		}
	}
}
