//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// An trigger that causes a <see cref="ITransition{S}"/> between two 
	/// <see cref="IState"/>s in a <see cref="FiniteStateMachine{S, T}"/>.
	/// </summary>
	public interface ITrigger
	{
		#region Properties

		/// <summary>
		/// String with a brief description of the action.
		/// </summary>
		string Description { get; }

		#endregion

	}
}
