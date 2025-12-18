//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

namespace Sintef.Scoop.Utilities.Tests
{
	/// <summary>
	/// Class that mimics some external context or data related to the
	/// finite-state machine, but that should not be part of it.
	/// </summary>
	public class ExternalContext
	{
		#region Public properties

		/// <summary>
		/// Number to be checked when making a transition.
		/// </summary>
		public int NumberToCheck { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Empty constructor
		/// </summary>
		public ExternalContext()
		{
			NumberToCheck = 0;
		}

		#endregion
	}
}
