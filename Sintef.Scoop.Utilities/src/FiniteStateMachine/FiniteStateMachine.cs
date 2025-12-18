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
	/// A finite-state machine. A machine is composed of a collection of states
	/// (<see cref="IState"/>) of type <typeparamref name="S"/>, along with a 
	/// collection of transitions (<see cref="ITransition{S}"/>) between them. 
	/// 
	/// It is assumed that the <see cref="ITrigger"/>s that cause the transitions
	/// are not relevant. Therefore, they can be of any type as long as they
	/// implement the corresponding interface. Nevertheless, the class keeps track
	/// of the <see cref="Type"/>s of the triggers considered, just in case they 
	/// are needed in a future use case.
	/// </summary>
	public class FiniteStateMachine<S, T> 
		where S : class, IState
		where T : class, ITransition<S>
	{
		#region Private data members

		/// <summary>
		/// Initial state of the machine.
		/// </summary>
		private S _iniState;

		/// <summary>
		/// States in the machine.
		/// </summary>
		private List<S> _states;

		/// <summary>
		/// Collection of transitions starting from a state.
		/// </summary>
		private Dictionary<S, List<T>> _transitionsFrom;

		/// <summary>
		/// Collection of transitions finishing at a state.
		/// </summary>
		private Dictionary<S, List<T>> _transitionsTo;

		/// <summary>
		/// The types of triggers in the machine.
		/// </summary>
		private List<Type> _triggerTypes;

		#endregion

		#region Public properties

		/// <summary>
		/// Number of states in the machine.
		/// </summary>
		public int NumStates
		{
			get { return _states.Count; }
		}

		/// <summary>
		/// Number of trigger types in the machine.
		/// </summary>
		public int NumTriggerTypes
		{
			get { return _triggerTypes.Count; }
		}

		/// <summary>
		/// Read-only list of states in the machine.
		/// </summary>
		public IReadOnlyList<S> States
		{
			get { return _states.AsReadOnly(); }
		}

		/// <summary>
		/// The initial state of the machine.
		/// </summary>
		public S IniState
		{
			get
			{
				Debug.Assert(_iniState != null);
				return _iniState;
			}
			set
			{
				Debug.Assert(value != null);
				Debug.Assert(HasState(value));
				_iniState = value;
			}
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Empty constructor.
		/// </summary>
		public FiniteStateMachine()
		{
			_iniState = null;
			_states = new List<S>();
			_transitionsFrom = new Dictionary<S, List<T>>();
			_transitionsTo = new Dictionary<S, List<T>>();
			_triggerTypes = new List<Type>();
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Determine if the machine has a state.
		/// </summary>
		/// <param name="state">The state.</param>
		/// <returns>True if the machine has the state, false otherwise.</returns>
		public bool HasState(S state)
		{
			// Input parameter check.
			Debug.Assert(state != null);

			return _states.Contains(state);
		}

		/// <summary>
		/// Add a state to the machine.
		/// </summary>
		/// <param name="state">The State.</param>
		public virtual void AddState(S state)
		{
			// Input parameter check.
			Debug.Assert(state != null);
			Debug.Assert(!HasState(state));

			_states.Add(state);
			_transitionsFrom.Add(state, new List<T>());
			_transitionsTo.Add(state, new List<T>());
		}

		/// <summary>
		/// Determine if a state is an end state in the machine.
		/// </summary>
		/// <param name="state">The state.</param>
		/// <returns>True if the state is an end state, false otherwise.</returns>
		public bool IsEndState(S state)
		{
			// Input parameter check.
			Debug.Assert(state != null);
			Debug.Assert(HasState(state));

			bool noTransitionsFrom = _transitionsFrom[state].Count == 0;

			return noTransitionsFrom; 
		}

		/// <summary>
		/// Determine if the state machine has any transitions that are triggered
		/// by <see cref="ITrigger"/> of a certain type.
		/// </summary>
		/// <param name="type">The type/class of the trigger.</param>
		/// <returns>True if there is at least one transition with a trigger
		/// of that type, false otherwise.</returns>
		public bool HasTriggersOfType(Type type)
		{
			// Input parameter check.
			Debug.Assert(type != null);
			Debug.Assert(IsDerivedOrImplements(type, typeof(ITrigger)));

			return _triggerTypes.Contains(type);
		}

		/// <summary>
		/// Determine if a trigger causes a transition from a state.
		/// </summary>
		/// <param name="state">The state.</param>
		/// <param name="trigger">The trigger.</param>
		/// <returns>True if the action causes a transition, false otherwise.
		/// </returns>
		public bool TriggerCausesTransition(S state, ITrigger trigger)
		{
			// Input parameter check.
			Debug.Assert(state != null);
			Debug.Assert(HasState(state));
			Debug.Assert(trigger != null);

			foreach (T transition in _transitionsFrom[state])
				if (transition.Trigger == trigger)
					return true;

			return false;
		}

		/// <summary>
		/// Determine if the machine has a transition.
		/// </summary>
		/// <param name="transition">The transition.</param>
		/// <returns>True if the machine has the transition, false otherwise.
		/// </returns>
		public bool HasTransition(T transition)
		{
			//  Input parameter check.
			Debug.Assert(transition != null);
			Debug.Assert(HasState(transition.FromState));
			Debug.Assert(HasState(transition.ToState));

			return _transitionsFrom[transition.FromState].Contains(transition);
		}

		/// <summary>
		/// Add a transition to the machine.
		/// </summary>
		/// <param name="transition">The transition.</param>
		public void AddTransition(T transition)
		{
			// Input parameter check.
			Debug.Assert(transition != null);
			Debug.Assert(HasState(transition.FromState));
			Debug.Assert(HasState(transition.ToState));
			Debug.Assert(!HasTransition(transition));
			Debug.Assert(!TriggerCausesTransition(transition.FromState,
				transition.Trigger));

			_transitionsFrom[transition.FromState].Add(transition);
			_transitionsTo[transition.ToState].Add(transition);

			Type triggerType = transition.Trigger.GetType();
			if (!HasTriggersOfType(triggerType))
				_triggerTypes.Add(triggerType);
		}

		/// <summary>
		/// Determine the next state in the machine, given a starting state
		/// and a trigger.
		/// </summary>
		/// <param name="state">The starting state.</param>
		/// <param name="trigger">The trigger.</param>
		/// <returns>The next state in the machine, or null if there is no 
		/// transition caused by the trigger, or the transition is not available.
		/// </returns>
		public S NextState(S state, ITrigger trigger)
		{
			// Input parameter check.
			Debug.Assert(state != null);
			Debug.Assert(trigger != null);
			Debug.Assert(HasState(state));

			if (!TriggerCausesTransition(state, trigger))
				return null;

			foreach (T transition in _transitionsFrom[state])
				if (transition.Trigger == trigger)
				{
					if (transition.IsAllowed)
						return transition.ToState;
					else
						return null;
				}

			return null;
		}

		#endregion

		#region Protected methods

		/// <summary>
		/// Determine if a type implements or is derived from another type.
		/// </summary>
		/// <param name="sub">The type that implements or is derived.</param>
		/// <param name="super">The type of the base class or the interface.
		/// </param>
		/// <returns>True if the sub type is derived or implements 
		/// the super type, false otherwise.</returns>
		protected bool IsDerivedOrImplements(Type sub, Type super)
		{
			// Input parameter check.
			Debug.Assert(sub != null);
			Debug.Assert(super != null);

			return super.IsAssignableFrom(sub);
		}

		#endregion
	}
}
