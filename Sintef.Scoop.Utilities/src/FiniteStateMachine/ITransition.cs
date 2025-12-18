//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// A transition between two<see cref="IState"/>s of type 
	/// <typeparamref name="S"/> in a <see cref="FiniteStateMachine{S, T}"/>.
	/// </summary>
	public interface ITransition<S>
	{
		#region Properties

		/// <summary>
		/// The initial <see cref="IState"/> of the transition.
		/// </summary>
		S FromState { get; }

		/// <summary>
		/// Trigger that causes the transition.
		/// </summary>
		ITrigger Trigger { get; }

		/// <summary>
		/// The final <see cref="IState"/> of the transition.
		/// </summary>
		S ToState { get; }

		/// <summary>
		/// String with a brief description of the transition.
		/// </summary>
		string Description { get; }

		/// <summary>
		/// A function that determines if the transition is allowed to 
		/// take place or not.
		/// </summary>
		bool IsAllowed { get; }

		#endregion
	}
}
