//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// A comparer that compares doubles with a given tolerance
	/// </summary>
	public class TolerantComparer : IEqualityComparer<double>
	{
		/// <summary>
		/// The tolerance
		/// </summary>
		double _tolerance;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="tolerance">The tolerance to use</param>
		public TolerantComparer(double tolerance)
		{
			_tolerance = tolerance;
		}

		#region IEqualityComparer<double> Members

		/// <summary>
		/// Returns true if the absolute or relative difference between the numbers is
		/// less than the tolerance
		/// </summary>
		public bool Equals(double x, double y)
		{
			return Math.Abs(x - y) <= _tolerance * Math.Max(1.0, Math.Abs(x));
		}

		/// <summary>
		/// Simple and fast..
		/// </summary>
		public int GetHashCode(double obj)
		{
			return (int)obj;
		}

		#endregion
	}
}
