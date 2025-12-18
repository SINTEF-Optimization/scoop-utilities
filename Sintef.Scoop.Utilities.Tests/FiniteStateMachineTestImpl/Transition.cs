//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Diagnostics;

namespace Sintef.Scoop.Utilities.Tests
{
	/// <summary>
	/// Simple implementation of <see cref="ITransition{S}"/>
	/// </summary>
	public class Transition<S> : ITransition<S>
	{

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="from">The starting state of the transition.</param>
		/// <param name="trigger">The trigger that causes the transition.</param>
		/// <param name="to">The final state of the transition.</param>
		/// <param name="description">A brief description of the transition.</param>
		public Transition(S from, ITrigger trigger, S to, string description)
		{
			Debug.Assert(from != null);
			Debug.Assert(trigger != null);
			Debug.Assert(to != null);

			FromState = from;
			Trigger = trigger;
			ToState = to;
			Description = description;
		}

		#endregion

		#region Implementation of ITransition

		public S FromState { get; protected set; }

		public ITrigger Trigger { get; protected set; }

		public S ToState { get; protected set; }

		public string Description { get; protected set; }

		public virtual bool IsAllowed
		{
			get { return true; }
		}

		#endregion

	}

	/// <summary>
	/// A transition that makes use of contextual information to decide
	/// whether it is allowed or not.
	/// </summary>
	/// <typeparam name="S"></typeparam>
	public class TransitionContext<S> : Transition<S>
	{

		#region Private data members

		/// <summary>
		/// The IsAllowed function.
		/// </summary>
		private Func<bool> _isAllowed;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="from">The starting state of the transition.</param>
		/// <param name="trigger">The trigger that causes the transition.</param>
		/// <param name="to">The final state of the transition.</param>
		/// <param name="description">A brief description of the transition.</param>
		/// <param name="isAllowedFunc"> Function that verifies if the transition
		/// is allowed or not.</param>
		public TransitionContext(S from, ITrigger trigger, S to, 
			string description, Func<bool> isAllowedFunc)
			: base (from, trigger, to, description)
		{
			Debug.Assert(isAllowedFunc != null);
			_isAllowed = isAllowedFunc;
		}

		#endregion

		#region Overriden implementation of ITransition

		public override bool IsAllowed {
			get { return _isAllowed.Invoke(); }
		}

		#endregion
	}


}
