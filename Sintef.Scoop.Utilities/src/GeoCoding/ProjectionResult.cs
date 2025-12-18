//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

namespace Sintef.Scoop.Utilities.GeoCoding
{
	/// <summary>
	/// Describes the result of an attempted projection of a coordinate point
	/// onto a line segment or polyline.
	/// </summary>
	public class ProjectionResult
	{
		/// <summary>
		/// The point closest to the original coordinate.
		/// If both <see cref="OutsideBefore"/> and <see cref="OutsideAfter"/> are false, this closest point
		/// is a projection onto the segment. If <see cref="OutsideBefore"/> (<see cref="OutsideAfter"/>) == true, 
		/// the closest point is at the beginning (end) of the poly-line that the projection is on.
		/// </summary>
		public ICoordinate ClosestPoint { get; private set; }

		/// <summary>
		/// True if the projection falls outside the segment, and "before" it.
		/// </summary>
		public bool OutsideBefore { get; private set; }

		/// <summary>
		/// True if the projection falls outside the segment, and "after" it.
		/// </summary>
		public bool OutsideAfter { get; private set; }

		/// <summary>
		/// True if and only if the projection falls within the segment (including
		/// falling exactly on any of the end points).
		/// </summary>
		public bool ProjectionOK => (!OutsideBefore) && (!OutsideAfter);

		/// <summary>
		/// The distance along whatever poly-line the projection is on, measured in meters
		/// from the start. If OutsideBefore == true, this is zero. If OutsideAfter == true, this 
		/// is the length of whatever polyline the projection was on.
		/// </summary>
		public double DistanceAlong { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="closestPoint">The point closest to the original coordinate.
		/// If both <paramref name="outsideBefore"/> and <paramref name="outsideAfter"/> are false, this closest point
		/// is a projection onto the segment. If <paramref name="outsideBefore"/> (<paramref name="outsideAfter"/>) == true, 
		/// the projection falls outside the segment, before (after) it.</param>
		/// <param name="outsideBefore"> True if the projection falls outside the segment, and "before" it.</param>
		/// <param name="outsideAfter">True if the projection falls outside the segment, and "after" it.</param>
		/// <param name="distanceAlong">The distance along whatever poly-line the projection is on, measured in meters
		/// from the start. If <paramref name="outsideBefore"/>, is true, this will be ignored
		/// (and the corresponding property set to 0). If <paramref name="outsideAfter"/> is true, then the value should be the length of whatever
		/// polyline the projection was on.</param>
		public ProjectionResult(ICoordinate closestPoint, bool outsideBefore, bool outsideAfter, double distanceAlong)
		{
			ClosestPoint = closestPoint;
			OutsideAfter = outsideAfter;
			OutsideBefore = outsideBefore;
			DistanceAlong = outsideBefore ? 0 : distanceAlong;
		}
	}
}
