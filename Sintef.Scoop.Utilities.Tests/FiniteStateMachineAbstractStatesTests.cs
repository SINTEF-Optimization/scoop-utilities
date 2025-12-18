//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sintef.Scoop.Utilities.Tests
{

	/// <summary>
	/// Tests related to a <see cref="FiniteStateMachineAbstractStates{T}"/>
	/// </summary>
	[TestClass]
	public class FiniteStateMachineAbstractStatesTests
	{

		/// <summary>
		/// Tests for the functionalities related to the states
		/// (of different types).
		/// </summary>
		[TestMethod]
		public void FiniteStateMachineAbstractStatesStatesTest() {

			var machine = new FiniteStateMachineAbstractStates<Transition<AbstractState>>();
			var state1 = new StateType1("State 1-1");
			var state2 = new StateType1("State 2-1");
			var state3 = new StateType2("State 3-2");

			machine.AddState(state1);
			machine.AddState(state2);
			machine.AddState(state3);

			var trigger1 = new Trigger1("Trigger 1");
			var trigger2 = new Trigger2("Trigger 2");
			var transition1 = new Transition<AbstractState>(state1, trigger1, state2, "Transition 1");
			var transition2 = new Transition<AbstractState>(state1, trigger2, state2, "Transition 2");
			var transition3 = new Transition<AbstractState>(state2, trigger2, state3, "Transition 3");

			machine.AddTransition(transition1);
			machine.AddTransition(transition2);
			machine.AddTransition(transition3);

			Assert.AreEqual(state2, machine.NextState(state1, trigger1));
			Assert.AreEqual(state2, machine.NextState<StateType1>(state1, trigger1));
			Assert.AreEqual(null, machine.NextState<StateType2>(state1, trigger1));
		}
	}

}
