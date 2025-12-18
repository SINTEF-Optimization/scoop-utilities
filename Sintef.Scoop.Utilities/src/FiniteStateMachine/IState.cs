//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// A state in a <see cref="FiniteStateMachine{S, T}"/>.
	/// </summary>
	public interface IState
	{
		#region Properties

		/// <summary>
		/// String with a brief description of the state.
		/// </summary>
		string Description { get; }

		#endregion
	}
}
