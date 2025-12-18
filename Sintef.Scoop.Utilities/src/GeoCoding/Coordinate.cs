//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Xml.Linq;
using System.Globalization;

namespace Sintef.Scoop.Utilities.GeoCoding
{
	/// <summary>
	/// Local cartesian coordinates.
	/// The unit is one meter.
	/// </summary>
	/// <seealso cref="UtmCoordinate"/>
	[DataContract]
	[TypeConverter(typeof(GenericObjectConverter<Coordinate>))]
	public class Coordinate : ICoordinate, ICoordinateBase<Coordinate>
	{

		#region Public properties

		/// <summary>
		/// X coordinate in meters relative to an origin defined by the corresponding CoordinateSystem.
		/// </summary>
		[DataMember]
		[ReadOnly(true)]
		public double X { get; set; }

		/// <summary>
		/// Y coordinate in meters relative to an origin defined by the corresponding CoordinateSystem.
		/// </summary>
		[DataMember]
		[ReadOnly(true)]
		public double Y { get; set; }

		/// <summary>
		/// Z coordinate (e.g., altitude) in meters relative to an origin defined by the corresponding CoordinateSystem.
		/// </summary>
		[DataMember]
		[ReadOnly(true)]
		public double Z { get; set; }

		/// <summary>
		/// The distance from origo
		/// </summary>
		[Browsable(false)]
		public double Length
		{
			get
			{
				double sqrDist = X * X + Y * Y;
				if (!double.IsNaN(Z))
					sqrDist += Z * Z;
				return Math.Sqrt(sqrDist);
			}
		}

		/// <summary>
		/// The origin of the coordinate system
		/// </summary>
		public static Coordinate Origin = new Coordinate(0, 0, 0);

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor, for use e.g. with deserialization. Sets all values to zero.
		/// </summary>
		public Coordinate()
		{
			X = 0;
			Y = 0;
			Z = 0;
		}

		/// <summary>
		/// Creates a coordinate at the given x and y, both given in meters.
		/// 
		/// </summary>
		/// <param name="x">the X coordinate</param>
		/// <param name="y">the Y coordinate</param>
		/// <param name="z">the Z coordinate</param>
		public Coordinate(double x, double y, double z = double.NaN)
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
		/// creates a coordinate
		/// </summary>
		/// <param name="element">XElement containing xml representation</param>
		public Coordinate(XElement element)
		{
			X = Convert.ToDouble(element.RequireElement("X").Value);
			Y = Convert.ToDouble(element.RequireElement("Y").Value);
			XElement xZ = element.TagElement("Z");
			if (xZ == null)
				Z = Double.NaN;
			else
				Z = Convert.ToDouble(xZ.Value);
		}

		#endregion

		#region Public methods

		#region ICoordinateBase implementation

		/// <summary>
		/// Returns the closest point to this coordinate on the segment between the two given coordinates.
		/// </summary>
		/// <returns>The coordinate, as a <see cref="Coordinate"/> reference.</returns>
		public Coordinate ClosestPoint(Coordinate p1, Coordinate p2) => ClosestCoordinate(p1, p2) as Coordinate;

		/// <summary>
		/// Returns a coordinate that is offset from this coordinate by a given distance in
		/// a given direction.
		/// </summary>
		/// <param name="distance">The distance to offset by, in meters</param>
		/// <param name="azimuth">The direction to offset in, as and angle in degrees wrt Up (positivy Y-direction). 
		/// Up is 0, left -90, right 90 and down 180/-180.</param>
		/// <returns>The coordinate, as a <see cref="Coordinate"/> reference.</returns>
		public Coordinate OffsetBy(double distance, double azimuth) => CoordinateOffsetBy(distance, azimuth) as Coordinate;

		/// <summary>
		/// Returns the coordinate obtained by moving this coordiate
		/// the given fraction of the distance towards the
		/// other coordinate.
		/// </summary>
		/// <param name="other">The coordinate to move towards</param>
		/// <param name="fraction">The fraction of the distance to move</param>
		/// <param name="minAccuracy">The maximum error in the fraction moved</param>
		/// <returns>The coordinate, as a <see cref="Coordinate"/> reference.</returns>
		public Coordinate Interpolated(Coordinate other, double fraction, double minAccuracy) => InterpolatedCoordinate(other, fraction, minAccuracy) as Coordinate;

		#endregion

		/// <summary>
		/// Returns a string description of the coordinate.
		/// </summary>
		public string ToInvariantString(int decimals = 2)
		{
			string format = $"F{decimals}";
			return X.ToString(format, CultureInfo.InvariantCulture) + "," +
				Y.ToString(format, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Returns the direction to move the shortest distance from the present coordinate to the <paramref name="target"/> coordinate, in degrees from north.
		/// </summary>
		/// <param name="target">The coordinate moving towards when calculating the direction</param>
		public double StraightLineDirectionTo(ICoordinate target)
		{
			if (!(target is Coordinate tarCo))
				throw new Exception($"Coordinate.EqualsWithTolerance: argument not of expected type {GetType()}, but of type {target.GetType()}");

			return tarCo.Difference(this).GetDirectionInDegrees();
		}

		/// <summary>
		/// Returns a coordinate that is offset from this coordinate by a given distance in
		/// a given direction.
		/// </summary>
		/// <param name="distance">The distance to offset by, in meters</param>
		/// <param name="azimuth">The direction to offset in, as and angle in degrees wrt Up (positivy Y-direction). 
		/// Up is 0, left -90, right 90 and down 180/-180.</param>
		/// <returns></returns>
		public ICoordinate CoordinateOffsetBy(double distance, double azimuth)
		{
			double sn = Math.Sin(azimuth * Math.PI / 180);
			double cs = Math.Cos(azimuth * Math.PI / 180);

			return new Coordinate(X + distance * sn, Y + distance*cs);
		}

		/// <summary>
		/// If this <see cref="Coordinate"/> has a projection on the straight line between <paramref name="lineStart"/> and <paramref name="lineEnd"/>,
		/// and this projection is a distance from point that is less than the given <paramref name="tolerance"/> (in meters), then the projection is returned.
		/// Otherwise, the function returns null.
		/// </summary>
		public Coordinate GetProjectionCloserThan(Coordinate lineStart, Coordinate lineEnd, double tolerance)
		{
			Coordinate projection = ProjectionOnLineSegment(lineStart, lineEnd);
			if (projection?.DistanceTo(this) <= tolerance)
				return projection;
			else
				return null;
		}


		/// <summary>
		/// Overridden hash function using bitwise exclusive-OR on X and Y. Ignores Z.
		/// </summary>
		public override int GetHashCode()
		{
			int intX = (int)X;
			int intY = (int)Y;
			int hashCode = intX ^ intY;
			return hashCode;
		}

		/// <summary>
		/// Tests if the coordinates are the same
		/// </summary>
		/// <param name="other">The other coordinate</param>
		/// <returns>True when the two coordinates are the same</returns>
		public override bool Equals(object other)
		{
			Coordinate geo = other as Coordinate;

			return (this == geo);
		}

		/// <summary>
		/// Returns true if the coordinates are the same
		/// </summary>
		public bool Equals(ICoordinate other) => Equals(other as object);

		/// <summary>
		/// Compares two coordinates for equality. They are equal if both have the same X and Y,
		/// and either both have no Z (i.e. NaN) or they have the same Z.
		/// </summary>
		static public bool operator == (Coordinate c1, Coordinate c2)
		{
			if (ReferenceEquals(c1, null))
			{
				return ReferenceEquals(c2, null);
			}
			if (ReferenceEquals(c2, null))
				return false;

			if (c1.X != c2.X || c1.Y != c2.Y)
				return false;

			if (double.IsNaN(c1.Z) && double.IsNaN(c2.Z))
				return true;

			return c1.Z == c2.Z;

		}

		/// <summary>
		/// Compares two coordinates for inequality. Returns the negation of ==.
		/// </summary>
		static public bool operator !=(Coordinate x, Coordinate y)
		{
			return !(x == y);
		}

		/// <summary>
		/// Test for approximate equality. Two coordinates are equal if they are (approximately) at the same position.
		/// Used in debugging.
		/// </summary>
		/// <param name="other">The coordinate to compare to</param>
		/// <param name="ignoreZ">If true, then comparison is done only in the XY-plane</param>
		public bool EqualsForAllPracticalPurposes(Coordinate other, bool ignoreZ)
		{
			return EqualsWithTolerance(other, ignoreZ, 0.0000000001);
		}

		/// <summary>
		/// Test for approximate equality, within the given tolerance. Two coordinates are equal if they are (approximately) at the same position.
		/// I.e., the coordinate in each dimension is equal within the given tolerance (relative or absolute).
		/// </summary>
		/// <param name="other">The coordinate to compare to</param>
		/// <param name="ignoreZ">If true, then comparison is done only in the XY-plane</param>
		/// <param name="maxTolerance">The tolerance</param>
		public bool EqualsWithTolerance(ICoordinate other, bool ignoreZ, double maxTolerance)
		{
			if (other.GetType() != GetType())
				throw new Exception($"Coordinate.EqualsWithTolerance: argument 1 not of expected type {GetType()}, but of type {other.GetType()}");


			Coordinate c2 = other as Coordinate;
			if (c2 == null)
				return false;

			if (!X.EqualsWithTolerance(c2.X, maxTolerance) || !Y.EqualsWithTolerance(c2.Y, maxTolerance))
				return false;

			if (double.IsNaN(Z) && double.IsNaN(c2.Z))
				return true;

			if (ignoreZ)
				return true;
			else
				return Z.EqualsWithTolerance(c2.Z, maxTolerance);
		}

		/// <summary>
		/// True if either both coordinates are null, or they are both not null and 
		/// the same within the given tolerance (relative or absolute).
		/// </summary>
		/// <param name="c1"></param>
		/// <param name="c2"></param>
		/// <param name="ignoreZ">If true, then comparison is done only in the XY-plane</param>
		/// <param name="tolerance"></param>
		public static bool EqualsWithTolerance(Coordinate c1, Coordinate c2, bool ignoreZ, double tolerance)
		{
			if (c1 == null && c2 == null)
				return true;
			else if ((c1 == null) != (c2 == null))
				return false;
			else
				return c1.EqualsWithTolerance(c2, ignoreZ, tolerance);
		}

		/// <summary>
		/// Returns the closest point to this coordinate on the segment 
		/// between the two endpoints (p1, p2).
		/// 
		/// </summary>
		/// <param name="p1">The start point of the segment</param>
		/// <param name="p2">The end point of the segment</param>
		/// <param name="tolerance">A tolerance in meters, which is used to decide if borderline projections
		/// fall on, or outside, the segment (interpreted as inside if it is less than <paramref name="tolerance"/> 
		/// meters outside).</param>
		public ProjectionResult ClosestProjection(ICoordinate p1, ICoordinate p2, double tolerance)
		{
			if (p1.GetType() != GetType())
				throw new Exception($"Coordinate.ClosestProjection: argument 1 not of expected type {GetType()}, but of type {p1.GetType()}");
			if (p2.GetType() != GetType())
				throw new Exception($"Coordinate.ClosestProjection: argument 2 not of expected type {GetType()}, but of type {p2.GetType()}");

			Coordinate start = p1 as Coordinate;
			Coordinate stop = p2 as Coordinate;
			Coordinate s = stop - start;
			double lengthOfSSquared = s * s;
			double product = (this - start) * s;
			Coordinate projection = (product / lengthOfSSquared) * s;

			var lengthAndTolerance = s.Length + tolerance;
			return product switch
			{
				> 0 when projection * projection > lengthAndTolerance * lengthAndTolerance => new ProjectionResult(p2, false, true, s.Length),
				< 0 when projection * projection > tolerance * tolerance => new ProjectionResult(p1, true, false, 0),
				_ => new ProjectionResult(start + projection, false, false, projection.Length)
			};
		}

		/// <summary>
		/// Returns a new coordinate that represents a projection of this coordinate onto
		/// a line segment defined by the two given coordinates. The Z-coordinate is interpolated between the end points.
		/// </summary>
		/// <param name="start">Start of the line segment</param>
		///<param name="stop">End of line segment</param>
		///<param name="tolerance">If given, projections that are less or equal to this much outside the
		///line segment are still considered as a valid projection. The default value is 0.</param>
		/// <returns>The coordinate of the projection (or null if the projection falls outside the line segment).
		/// </returns>
		public Coordinate ProjectionOnLineSegment(Coordinate start, Coordinate stop, double tolerance = 0)
		{
			var projRes = ClosestProjection(start, stop, tolerance);
			if (projRes.ProjectionOK)
				return projRes.ClosestPoint as Coordinate;
			else
				return null;
		}

		/// <summary>
		/// Returns the distance in the XY plane to the other coordinate.
		/// </summary>
		public double HorizontalDistanceTo(Coordinate other)
		{
			double xDist = X - other.X;
			double yDist = Y - other.Y;
			return Math.Sqrt(xDist * xDist + yDist * yDist);
		}

		/// <summary>
		/// Returns the distance along the Z axis to the other coordinate or Double.NaN if either Z
		/// coordinate is undefined.
		/// </summary>
		public double VerticalDistanceTo(Coordinate other)
		{
			return Math.Abs(Z - other.Z);
		}

		/// <summary>
		/// Returns the total distance to the other coordinate.
		/// </summary>
		public double DistanceTo(ICoordinate other)
		{
			if (!(other is Coordinate))
				throw new Exception($"Coordinate.DistanceTo: argument not of expected type {GetType()}, but of type {other.GetType()}");

			Coordinate oth = other as Coordinate;
			double xyDist = HorizontalDistanceTo(oth);
			double zDist = VerticalDistanceTo(oth);
			return zDist > 0
				? Math.Sqrt(xyDist * xyDist + zDist * zDist)
				: xyDist;
		}

		/// <summary>
		/// Adds the two coordinates.
		/// </summary>
		public Coordinate Translate(Coordinate other, double fraction = 1.0)
		{
			double newX = X + fraction * other.X;
			double newY = Y + fraction * other.Y;
			double newZ = (double.IsNaN(Z) || double.IsNaN(other.Z)) ? double.NaN : Z + fraction * other.Z;
			return new Coordinate(newX, newY, newZ);
		}


		/// <summary>
		/// Interpolates to a coordinate at a given fraction of the line segment
		/// between this and the given coordinate, starting measuring from this coordinate.
		/// </summary>
		/// <param name="other">The coordinate to move towards</param>
		/// <param name="fraction">The fraction of the distance to move</param>
		/// <param name="minAccuracy">The maximum error in the fraction moved. This is ignored in this implementation,
		/// since there for cartesian coordinates, the computation can be made exact.</param>
		public ICoordinate InterpolatedCoordinate(ICoordinate other, double fraction, double minAccuracy)
		{
			if (other.GetType() != GetType())
				throw new Exception($"Coordinate.InterpolatedCoordinate: argument 1 not of expected type {GetType()}, but of type {other.GetType()}");

			var cart = other as Coordinate;

			double newX = X + fraction * (cart.X - X);
			double newY = Y + fraction * (cart.Y - Y);
			double newZ = (double.IsNaN(Z) || double.IsNaN(other.Z)) ? double.NaN : Z + fraction * (other.Z - Z);
			return new Coordinate(newX, newY, newZ);
		}

		/// <summary>
		/// Creates the difference vector by subtracting vector "other".
		/// </summary>
		public Coordinate Difference(Coordinate other)
		{
			double newX = X - other.X;
			double newY = Y - other.Y;
			double newZ = Z - other.Z;
			return new Coordinate(newX, newY, newZ);
		}

		/// <summary>
		/// Creates the difference vector by subtricting vector "other".
		/// </summary>
		public Coordinate Scale(double s)
		{
			return new Coordinate(X * s, Y * s, Z * s);
		}

		/// <summary>
		/// Returns a vector with the same direction but unit length
		/// </summary>
		public Coordinate ToUnitVector()
		{
			double length = Length;
			return length == 0 ? null : new Coordinate(X / length, Y / length, double.IsNaN(Z) ? double.NaN : Z / length);
		}

		/// <summary>
		/// Returns a vector multiplied by the given factor
		/// </summary>
		public static Coordinate operator *(Coordinate vector, double scalar)
		{
			return vector.Scale(scalar);
		}

		/// <summary>
		/// Returns a vector multiplied by the given factor
		/// </summary>
		public static Coordinate operator *(double scalar, Coordinate vector)
		{
			return vector.Scale(scalar);
		}

		/// <summary>
		/// Returns a vector divided by the given value
		/// </summary>
		public static Coordinate operator /(Coordinate vector, double scalar)
		{
			return vector.Scale(1 / scalar);
		}

		/// <summary>
		/// Returns the vector dot product of the given 2 coordinates.
		/// </summary>
		public static double operator *(Coordinate first, Coordinate second)
		{
			double result = first.X * second.X + first.Y * second.Y;
			if (!Double.IsNaN(first.Z) && !Double.IsNaN(second.Z))
				result += first.Z * second.Z;
			return result;
		}

		/// <summary>
		/// Returns the difference of two vectors
		/// </summary>
		public static Coordinate operator -(Coordinate first, Coordinate second)
		{
			return first.Difference(second);
		}

		/// <summary>
		/// Returns the sum of two vectors
		/// </summary>
		public static Coordinate operator +(Coordinate first, Coordinate second)
		{
			double newX = first.X + second.X;
			double newY = first.Y + second.Y;
			double newZ = (double.IsNaN(first.Z) || double.IsNaN(second.Z)) ? double.NaN : first.Z + second.Z;
			return new Coordinate(newX, newY, newZ);
		}

		/// <summary>
		/// Returns the cross product of this coordinate with another coordinate
		/// </summary>
		/// <param name="other">The other coordiante in the cross product</param>
		public Coordinate CrossProduct(Coordinate other)
		{
			if (double.IsNaN(Z) || double.IsNaN(other.Z))
			{
				return new Coordinate(0, 0, X * other.Y - other.X * Y);
			}
			else
			{
				return new Coordinate(Y * other.Z - other.Y * Z, Z * other.X - other.Z * X, X * other.Y - other.X * Y);
			}
		}

		/// <summary>
		/// Returns the closest point to this coordinate on the segment 
		/// </summary>
		public ICoordinate ClosestCoordinate(ICoordinate p1, ICoordinate p2)
		{
			if (p1.GetType() != GetType())
				throw new Exception($"Coordinate.ClosestCoordinate: argument 1 not of expected type {GetType()}, but of type {p1.GetType()}");
			if (p2.GetType() != GetType())
				throw new Exception($"Coordinate.ClosestCoordinate: argument 2 not of expected type {GetType()}, but of type {p2.GetType()}");


			double delta;
			return ClosestPoint(p1 as Coordinate, p2 as Coordinate, out delta);
		}

		/// <summary>
		/// Returns the closest point on the segment from p1 to p2
		/// </summary>
		/// <param name="p1">the start of the segment, assumed to be of type <see cref="Coordinate"/>.</param>
		/// <param name="p2">the end of the segment, assumed to be of type <see cref="Coordinate"/>.</param>
		/// <param name="delta">the position of the closest point along the as a double segment within [0,1].</param>
		/// <returns></returns>
		public Coordinate ClosestPoint(Coordinate p1, Coordinate p2, out double delta)
		{
			Coordinate v12 = (p2 as Coordinate).Difference(p1 as Coordinate);
			Coordinate v10 = Difference(p1 as Coordinate);
			double v10v12 = v10 * v12;
			double v12v12 = v12 * v12;
			delta = v10v12 / v12v12;
			if (delta <= 0)
			{
				delta = 0;
				return p1;

			}
			if (delta >= 1)
			{
				delta = 1;
				return p2;
			}
			return p1.Translate(v12.Scale(delta));
		}

		/// <summary>
		/// Returns the intersection of two segments in the XY plane other or null if they don't intersect.
		/// </summary>
		/// <param name="start1">The start coordinate of the first segment</param>
		/// <param name="end1">The end coordinate of the first segment</param>
		/// <param name="start2">The start coordinate of the second segment</param>
		/// <param name="end2">The end coordinate of the second segment</param>
		/// <param name="err">A tolerance needed due to floating-point arithmetic</param>
		public static Coordinate IntersectionInXYPlane(Coordinate start1, Coordinate end1, Coordinate start2, Coordinate end2, double err = double.Epsilon)
		{
			double px = start1.X;
			double py = start1.Y;
			double rx = end1.X - px;
			double ry = end1.Y - py;
			double qx = start2.X;
			double qy = start2.Y;
			double sx = end2.X - qx;
			double sy = end2.Y - qy;
			double r_cross_s = rx * sy - ry * sx;
			double q_minus_p_x = (qx - px);
			double q_minus_p_y = (qy - py);
			double q_minus_p_cross_s = q_minus_p_x * sy - q_minus_p_y * sx;

			if (r_cross_s == 0)
				return null; // if (q_minus_p_cross_s == 0) then the two lines are collinear otherwise they are parallell

			// t = (q − p) × s / (r × s)
			double t = q_minus_p_cross_s / r_cross_s;

			//u = (q − p) × r / (r × s)
			double q_minus_p_cross_r = q_minus_p_x * ry - q_minus_p_y * rx;
			double u = q_minus_p_cross_r / r_cross_s;

			// if both t and u is in the range <0, 1> then there is an intersection
			if ((err <= t && t <= 1 - err) && (err <= u && u <= 1 - err))
			{
				return new Coordinate(px + rx * t, py + ry * t);
			}
			else
				return null;
		}

		/// <summary>
		/// Checks if the two segments in the XY plane intersects, false if not.
		/// </summary>
		/// <param name="start1">The start coordinate of the first segment</param>
		/// <param name="end1">The end coordinate of the first segment</param>
		/// <param name="start2">The start coordinate of the second segment</param>
		/// <param name="end2">The end coordinate of the second segment</param>
		/// <returns>Returns true if the two segments intersect, false if not.</returns>
		public static bool Intersects(Coordinate start1, Coordinate end1, Coordinate start2, Coordinate end2) => IntersectionInXYPlane(start1, end1, start2, end2) != null;

		/// <summary>
		/// Returns the direction from origo to the coordinate as an angle in degrees.
		/// Straight North is defined as 0 degrees, increasing angles correspond
		/// to clocwise rotation. For example, 90 degrees refers to an angle
		/// pointing towards east.
		/// </summary>
		/// <param name="ensurePositive">If this is true, the return angle
		/// will always be positive.</param>
		/// <returns>Direction in degrees, 0 degrees is North</returns>
		public double GetDirectionInDegrees(bool ensurePositive = true)
		{
			double rad = Math.Atan2(X, Y);
			if (ensurePositive && rad < 0) rad += 2 * Math.PI;
			return rad * 180.0 / Math.PI;
		}

		/// <summary>
		/// Returns a coordinate with inverted coordinate values
		/// </summary>
		public Coordinate GetInverse()
		{
			return new Coordinate(-X, -Y, double.IsNaN(Z) ? double.NaN : -Z);
		}

		/// <summary>
		/// Returns a string representation of the xyz coordinate.
		/// </summary>
		public override string ToString()
		{
			return "(" + X.ToString("F2", CultureInfo.InvariantCulture) + "," + Y.ToString("F2", CultureInfo.InvariantCulture) + (double.IsNaN(Z) ? ")" : ("," + Z.ToString("F2", CultureInfo.InvariantCulture) + ")"));
		}

		/// <summary>
		/// Returns an xml representation of this coordinate
		/// </summary>
		public XElement ToXml(string elementName)
		{
			XElement element = new XElement(elementName);
			XElement x = new XElement("X");
			x.Value = X.ToString("G32");
			element.Add(x);
			XElement y = new XElement("Y");
			y.Value = Y.ToString("G32");
			element.Add(y);
			if (!double.IsNaN(Z))
			{
				XElement z = new XElement("Z");
				z.Value = Z.ToString("G32");
				element.Add(z);
			}
			return element;
		}

		/// <summary>
		/// Returns the geographical center point of the given <paramref name="coordinates"/>.
		/// </summary>
		/// <param name="coordinates"></param>
		/// <returns></returns>
		public static Coordinate CenterPoint(IEnumerable<Coordinate> coordinates)
		{
			return new Coordinate(coordinates.Average(c => c.X), coordinates.Average(c => c.Y));
		}

		/// <summary>
		/// A coordinate half way between the two given coordinates.
		/// </summary>
		/// <param name="c1"></param>
		/// <param name="c2"></param>
		/// <returns></returns>
		public static Coordinate CenterPoint(Coordinate c1, Coordinate c2)
		{
			return new Coordinate((c1.X + c2.X) / 2, (c1.Y + c2.Y) / 2);
		}
		

		#endregion
	}
}
