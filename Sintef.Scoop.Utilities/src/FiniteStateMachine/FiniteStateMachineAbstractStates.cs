//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// A <see cref="FiniteStateMachine{S, T}"/> with states derived from 
	/// the class <see cref="AbstractState"/>. This allows to support different
	/// types of states (derived from the abstract class, of course). The class
	/// offers additional functionalities that keep track of how many types
	/// (concrete implementations of the abstract class) of states are in the 
	/// machine, and offers parameterized methods to obtain the concrete types
	/// of the states in a safe way (can be checked in compilation time).
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class FiniteStateMachineAbstractStates<T> : 
		FiniteStateMachine<AbstractState, T>
			where T : class, ITransition<AbstractState>
	{

		#region Private data members

		/// <summary>
		/// The types of states in the machine.
		/// </summary>
		private List<Type> _stateTypes;

		#endregion

		#region Public properties

		/// <summary>
		/// Number of state types in the machine.
		/// </summary>
		public int NumStateTypes
		{
			get { return _stateTypes.Count; }
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Empty constructor.
		/// </summary>
		public FiniteStateMachineAbstractStates() : base()
		{
			_stateTypes = new List<Type>();
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Determine if the machine has a state of a certain type.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>True if the machine has a state of a certain type,
		/// false otherwise.</returns>
		public bool HasStatesOfType(Type type)
		{
			// Input parameter check.
			Debug.Assert(type != null);
			Debug.Assert(IsDerivedFromAbstractStateClass(type));

			return _stateTypes.Contains(type);
		}

		/// <summary>
		/// Add a state to the machine. 
		/// The overriden method keeps track of the type of the state added.
		/// </summary>
		/// <param name="state">The state.</param>
		public override void AddState(AbstractState state)
		{
			base.AddState(state);

			Type stateType = state.GetType();
			if (!HasStatesOfType(stateType))
				_stateTypes.Add(stateType);
		}

		/// <summary>
		/// Determine the next state in the machine of a certain type, given a
		/// starting state and a trigger.
		/// </summary>
		/// <typeparam name="S">The type of the state.</typeparam>
		/// <param name="state">The starting state.</param>
		/// <param name="trigger">The trigger.</param>
		/// <returns>The next state in the machine of type 
		/// <typeparamref name="S"/>, or null if there is no transition caused
		/// by the trigger, the transition is not available or the state is of
		/// different type.</returns>
		public S NextState<S>(AbstractState state, ITrigger trigger)
			where S : AbstractState
		{
			// Input parameter check.
			Debug.Assert(state != null);
			Debug.Assert(HasState(state));
			Debug.Assert(trigger != null);

			AbstractState nextState = base.NextState(state, trigger);
			if (nextState != null && nextState is S)
				return nextState as S;

			return null;
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Determine if a type is of a class derived from 
		/// <see cref="AbstractState"/>.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>True if the type is of a derived class, false otherwise.
		/// </returns>
		private bool IsDerivedFromAbstractStateClass(Type type)
		{
			// Input parameter check.
			Debug.Assert(type != null);

			return IsDerivedOrImplements(type, typeof(AbstractState));
		}

		#endregion

	}


	/// <summary>
	/// An abstract class that serves as base for the implementation of
	/// different <see cref="IState"/>s. By inhereting from this class,
	/// it is possible to implement a finite-state machine with states
	/// of different types.
	/// </summary>
	public abstract class AbstractState : IState
	{
		#region Implementation of IState

		/// <inheritdoc/>
		public abstract string Description { get; protected set; }

		#endregion
	}

}
