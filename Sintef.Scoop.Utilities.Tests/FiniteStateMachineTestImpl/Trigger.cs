//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

namespace Sintef.Scoop.Utilities.Tests
{

	/// <summary>
	/// A simple implementation of <see cref="ITrigger"/>
	/// </summary>
	public class Trigger1 : ITrigger
	{
		#region Constructor

		public Trigger1(string description)
		{
			Description = description;
		}

		#endregion

		#region Implementation of IAction

		public string Description { get; private set; }

		#endregion

	}

	/// <summary>
	/// A simple implementation of <see cref="ITrigger"/>
	/// </summary>
	public class Trigger2 : ITrigger
	{
		#region Constructor

		public Trigger2(string description)
		{
			Description = description;
		}

		#endregion

		#region Implementation of IAction

		public string Description { get; private set; }

		#endregion

	}
}
