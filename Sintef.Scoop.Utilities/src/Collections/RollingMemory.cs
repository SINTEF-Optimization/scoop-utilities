//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Simple utility class, which 
	/// rempresents a rolling memory of the last 'n' T's.
	/// T must be int, long, double, or float (this is checked in the constructor).
	/// Can give average. Thread safe.
	/// </summary>
	public class RollingMemory<T>
	{
		#region Properties and fields
		/// <summary>
		/// The improvement rates that we remember
		/// </summary>
		Queue<T> _observations;

		/// <summary>
		/// Max memory length
		/// </summary>
		public int MemoryLength { get; private set; }


		/// <summary>
		/// The current number of recorded overvations.
		/// </summary>
		public int Length { get { return _observations.Count; } }


		#endregion

		#region Construction

		/// <summary>
		/// Constructor
		/// </summary>'
		/// <param name="memoryLength"></param>
		public RollingMemory(int memoryLength)
		{
			Type t = typeof(T);
			if (t != typeof(int) && t != typeof(long) && t != typeof(double) && t != typeof(float))
				throw new Exception("Type " + t.ToString() + " is not allowed as the generic argument for RollingMemory");

			_observations = new Queue<T>();
			MemoryLength = memoryLength;
		}


		#endregion

		#region Public members
		/// <summary>
		/// Adds an observation
		/// </summary>
		/// <param name="p"></param>
		public void Add(T p)
		{
			lock (_observations)
			{
				_observations.Enqueue(p);
				if (_observations.Count > MemoryLength)
					_observations.Dequeue();
			}
		}

		/// <summary>
		/// Returns the average of the remembered values
		/// </summary>
		/// <returns></returns>
		public double Average()
		{
			if (_observations.Count > 0)
			{
				lock (_observations)
				{
					return _observations.Cast<double>().Average();
				}
			}
			else
				throw new InvalidOperationException("RollingMemory: Average called on empty history (Length == 0)");
		}

		/// <summary>
		/// Returns the sum of the remembered values.
		/// </summary>
		/// <returns>The average as a double, or 0.0 if there are no observations.</returns>
		public double Sum()
		{
			if (_observations.Count > 0)
			{
				lock (_observations)
				{
					return _observations.Cast<double>().Sum();
				}
			}
			else
				return 0.0;
		}

		#endregion

		#region Private members

		#endregion



	}
}
