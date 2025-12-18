//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities.GeoCoding
{
	/// <summary>
	/// Some geometric utility functions based on coordinates.
	/// </summary>
	public static class GeometricTools
	{


		#region Public properties 

		#endregion

		#region Private data members

		#endregion

		#region Public methods

		/// <summary>
		/// Finds the intersection of a line segment with a circle. The function operates in the XY-plane. Any Z-coordinates of
		/// the input is ignored, and the resulting coordinate has Z = 0.
		/// </summary>
		///<param name="lineSegmentStart">The start of the line segment</param>
		///<param name="lineSegmentEnd">The end of the line segment</param>
		/// <param name="radiusOfCircle"></param>
		/// <param name="centerOfCircle"></param>
		/// <param name="startAtEntry">If there are two intersections, the following logic applies: If this flag is true, the intersection closest to lineSegmentStart is returned. If false, the intersection closest to lineSegmentEnd is returend.</param>
		/// <returns></returns>
		public static Coordinate IntersectionOfLineSegmentWithCircle(Coordinate lineSegmentStart, Coordinate lineSegmentEnd, double radiusOfCircle, Coordinate centerOfCircle, bool startAtEntry)
		{
			double x2 = lineSegmentEnd.X - centerOfCircle.X;
			double y2 = lineSegmentEnd.Y - centerOfCircle.Y;
			double x1 = lineSegmentStart.X - centerOfCircle.X;
			double y1 = lineSegmentStart.Y - centerOfCircle.Y;
			double dx = x2 - x1;
			double dy = y2 - y1;
			double A = dx * dx + dy * dy;
			double r = radiusOfCircle;
			double signedDy = dy < 0 ? -1 : 1;
			double DD = x1 * y2 - x2 * y1;

			//Result
			double x, y;

			//Discriminant:
			double D = r * r * A - DD * DD;
			if (D < 0)
				throw new Exception("PointOnResourceAtCriticalDistanceFromOtherResource: No intersection found. This should have been caught above.");
			else if (D == 0)
			{
				//Tangent
				x = DD * dy / A;
				y = -DD * dx / A;
				return new Coordinate(x + centerOfCircle.X, y + centerOfCircle.Y); 
			}
			else
			{
				//disc > 0
				//Two intersections for the infinite line, one or two intersections for the finite line segment
				//We only consider the first one (when entering the critical zone), or last one (when leaving).
				//First we calculate both intersections
				List<Coordinate> intersections = new List<Coordinate>();
				for (double sign = -1; sign < 2; sign += 2)
				{
					double xx1 = (DD * dy + sign * signedDy * dx * Math.Sqrt(D)) / A;
					double yy1 = (-DD * dx + sign * Math.Abs(dy) * Math.Sqrt(D)) / A;
					if ((x2 > x1 && x2 > xx1 && xx1 > x1) || (x1 >= x2 && x1 >= xx1 && xx1 >= x2))
					{
						//Between, in x. Now check if between also in y (that is, the intersection of the line is on the line segment).
						if ((y2 > y1 && y2 > yy1 && yy1 > y1) || (y1 >= y2 && y1 >= yy1 && yy1 >= y2))
							intersections.Add(new Coordinate(xx1 + centerOfCircle.X, yy1 + centerOfCircle.Y)); //Translating back to the original coordinate system
					}
				}

				//Choose the intersection that is closest to where we entered the segment
				Coordinate result2 = null;
				if (intersections.Any())
				{
					if (startAtEntry) //Entering the zone
						result2 = intersections.MinBy(c => c.DistanceTo(lineSegmentStart));
					else
						result2 = intersections.MinBy(c => c.DistanceTo(lineSegmentEnd));
				}
				return result2;
			}
		}


		/// <summary>
		/// Finds the intersection of a line segment with a sphere closest to the defined end of the segment. 
		/// </summary>
		///<param name="lineSegmentStart">The start of the line segment</param>
		///<param name="lineSegmentEnd">The end of the line segment</param>
		/// <param name="radiusOfSpere"></param>
		/// <param name="centerOfSpere"></param>
		/// <param name="startAtEntry">If there are two intersections, the following logic applies: If this flag is true, the intersection closest to lineSegmentStart is returned. If false, the intersection closest to lineSegmentEnd is returend.</param>
		/// <returns></returns>

		public static Coordinate IntersectionOfLineSegmentWithSphere(Coordinate lineSegmentStart, Coordinate lineSegmentEnd, double radiusOfSpere, Coordinate centerOfSpere, bool startAtEntry)
		{
			List<Coordinate> intersections = IntersectionsOfLineSegmentWithSphere(lineSegmentStart, lineSegmentEnd, radiusOfSpere, centerOfSpere);
			if (intersections != null)
			{
				return startAtEntry ? intersections.MinBy(c => c.DistanceTo(lineSegmentStart)) : intersections.MinBy(c => c.DistanceTo(lineSegmentEnd));
			}
			else
				return null;
		}


		/// <summary>
		/// Finds the intersections of a line segment with a sphere. 
		/// </summary>
		///<param name="lineSegmentStart">The start of the line segment</param>
		///<param name="lineSegmentEnd">The end of the line segment</param>
		/// <param name="radiusOfSpere"></param>
		/// <param name="centerOfSpere"></param>
		/// <returns>A list of the intersections that were found, or null if there are no intersections.</returns>

		public static List<Coordinate> IntersectionsOfLineSegmentWithSphere(Coordinate lineSegmentStart, Coordinate lineSegmentEnd, double radiusOfSpere, Coordinate centerOfSpere)
		{
			double cx = centerOfSpere.X;
			double cy = centerOfSpere.Y;
			double cz = centerOfSpere.Z;

			double x1 = lineSegmentStart.X;
			double y1 = lineSegmentStart.Y;
			double z1 = lineSegmentStart.Z;

			double x2 = lineSegmentEnd.X;
			double y2 = lineSegmentEnd.Y;
			double z2 = lineSegmentEnd.Z;

			//Assuming we are looking for intersectionspoints (x,y,z) | (x,y,z) = (x1,y1,z1) + u (x2,y2,z2).

			double dx = x2 - x1;
			double dy = y2 - y1;
			double dz = z2 - z1;

			//We are looking for solutions u = -B - Math.Sqrt(D)) / (2.0 * A), to the 2nd order equation  A u^2 + B u + C = 0
			//where:

			double A = dx * dx + dy * dy + dz * dz;
			double B = 2.0 * (x1 * dx + y1 * dy + z1 * dz - dx * cx - dy * cy - dz * cz);
			double C = x1 * x1 - 2 * x1 * cx + cx * cx + y1 * y1 - 2 * y1 * cy + cy * cy +
								 z1 * z1 - 2 * z1 * cz + cz * cz - radiusOfSpere * radiusOfSpere;

			// discriminant 
			double D = B * B - 4 * A * C;

			//No intersection with line (no solutions)
			if (D < 0)
			{
				return null;
			}
			else
			{
				//The circle and line intersects, but what about the line segment?

				//The two solutions/intersections
				double u1 = (-B - Math.Sqrt(D)) / (2.0 * A);
				double u2 = (-B + Math.Sqrt(D)) / (2.0 * A);

				//No intersection, line segment otuside of sphere?
				if ((u1 > 1 || u1 < 0) && (u2 < 0 || u2 > 1))
					return null;
				//No intersection, line segment inside of sphere?
				else if ((u1 > 1 && u2 < 0) || (u2 > 1 && u1 < 0))
					return null;
				//At least one intersection point
				else
				{
						List<Coordinate> coordinates = new List<Coordinate>();
					if (D == 0) //Tangent, u1 = u2.
					{
						coordinates.Add(new Coordinate(x1 * (1 - u1) + u1 * lineSegmentEnd.X,
																								 y1 * (1 - u1) + u1 * lineSegmentEnd.Y,
																								 z1 * (1 - u1) + u1 * lineSegmentEnd.Z));
					}
					else
					{
						if(u1 >= 0 && u1 <= 1)
							coordinates.Add(new Coordinate(x1 * (1 - u1) + u1 * lineSegmentEnd.X,
																		 y1 * (1 - u1) + u1 * lineSegmentEnd.Y,
																		 z1 * (1 - u1) + u1 * lineSegmentEnd.Z));

						if (u2 >= 0 && u2 <= 1)
							coordinates.Add(new Coordinate(x1 * (1 - u2) + u2 * lineSegmentEnd.X,
																						 y1 * (1 - u2) + u2 * lineSegmentEnd.Y,
																						 z1 * (1 - u2) + u2 * lineSegmentEnd.Z));
					}
					return coordinates;
				}
			}
		}

	}

	#endregion

	#region Private methods

	#endregion
}


