//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;

namespace Sintef.Scoop.Utilities.Tests
{
	/// <summary>
	/// Simple implentation of <see cref="IState"/>
	/// </summary>
    public class State : IState
    {

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="description">Description of the state.</param>
		public State(String description)
		{
			Description = description;
		}

		#endregion

		#region Implementation of IState

		public string Description { get; private set; }

		#endregion

	}

	/// <summary>
	/// State of type 1.
	/// </summary>
	public class StateType1 : AbstractState
	{

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="description">Description of the state.</param>
		public StateType1(String description)
		{
			Description = description;
		}

		#endregion

		#region Implementation of AbstractState

		public override string Description { get; protected set; }

		#endregion
	}

	/// <summary>
	/// State of type 2.
	/// </summary>
	public class StateType2 : AbstractState
	{

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="description">Description of the state.</param>
		public StateType2(String description)
		{
			Description = description;
		}

		#endregion

		#region Implementation of AbstractState

		public override string Description { get; protected set; }

		#endregion
	}
}
