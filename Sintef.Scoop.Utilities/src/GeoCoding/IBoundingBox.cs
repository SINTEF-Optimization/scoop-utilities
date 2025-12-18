//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;

namespace Sintef.Scoop.Utilities.GeoCoding
{
	/// <summary>
	/// Interface for bounding boxes. Agnostic as to the type of coordinates that are used.
	/// In general, it is assumed that the x-axis points right/east, and the y-axis points up/north.
	/// </summary>
	public interface IBoundingBox
	{
		/// <summary>
		/// The area (in square coordinate units, whatever they are) of the bounding box
		/// </summary>
		double Area { get; }

		/// <summary>
		/// The minimum east/right covered by the bounding box (can be negative, which means west/left).
		/// </summary>
		double MinX { get; }

		/// <summary>
		/// The maximum east/right covered by the bounding box (can be negative, which means west/left).
		/// </summary>
		double MaxX { get; }

		/// <summary>
		/// The minimum north/up coordinate.
		/// </summary>
		double MinY { get; }

		/// <summary>
		/// The maximum north/up coordinate covered by the bounding box
		/// </summary>
		double MaxY { get; }

		/// <summary>
		/// Returns a random coordinate within the bounding box.
		/// </summary>
		ICoordinate GetRandomCoordinate(Random r);

		/// <summary>
		/// Returns the minimum distance, in meters, along the surface,
		/// between the given coordinate and the closest point in 
		/// the bounding box. If the point is inside the bounding 
		/// box, returns 0.
		/// </summary>
		double MinDistance(ICoordinate coord);

		/// <summary>
		/// Expands the bounding box, if necessary, to ensure that 
		/// the given coordinate is contained by this bounding box.
		/// </summary>
		void ExpandBy(ICoordinate coord);


		/// <summary>
		/// Expands the bounding box, if necessary, to ensure that 
		/// the given bounding box is entirely contained by this 
		/// bounding box.
		/// </summary>
		void ExpandBy(IBoundingBox other);

		/// <summary>
		/// Expands the bounding box to include any point within
		/// distance \p d (in meters) of the original box
		/// </summary>
		void ExpandBy(double d);

		/// <summary>
		/// Returns true if the given cooridinate is inside the 
		/// bounding box, false if not
		/// </summary>
		bool Contains(ICoordinate coord);

		/// <summary>
		/// Returns true if the given bounding box is entirely inside this
		/// bounding box, false if not
		/// </summary>
		bool Contains(IBoundingBox other);

		/// <summary>
		/// Returns true if this bounding box and the given
		/// bounding box have any coordinate in common.
		/// </summary>
		bool Intersects(IBoundingBox other);

		/// <summary>
		/// Returns the area added if this bounding box is expanded by the
		/// other bounding box
		/// </summary>
		double ExpansionArea(IBoundingBox other);


	}
}