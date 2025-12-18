//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sintef.Scoop.Utilities.Tests
{
	/// <summary>
	/// Tests related to <see cref="FiniteStateMachine{S, T}"/>
	/// </summary>
	[TestClass]
	public class FiniteStateMachineTests
	{
		/// <summary>
		/// Tests for the functionalities related to the states.
		/// </summary>
		[TestMethod]
		public void FiniteStateMachineStatesTest()
		{
			var machine = new FiniteStateMachine<State, Transition<State>>();
			var state1 = new State("State 1");
			var state2 = new State("State 2");
			var state3 = new State("State 3");

			// Addition of states.

			Assert.IsFalse(machine.HasState(state1));
			Assert.IsFalse(machine.HasState(state2));
			Assert.IsFalse(machine.HasState(state3));

			machine.AddState(state1);
			machine.AddState(state2);
			machine.AddState(state3);

			Assert.IsTrue(machine.HasState(state1));
			Assert.IsTrue(machine.HasState(state2));
			Assert.IsTrue(machine.HasState(state3));

			machine.IniState = state1;
			Assert.AreEqual(state1, machine.IniState);

			// Is an end state.
	
			var trigger = new Trigger1("Trigger 1");
			var transition = new Transition<State>(state1, trigger, state2, "Transition 1");

			machine.AddTransition(transition);

			Assert.IsFalse(machine.IsEndState(state1));
			Assert.IsTrue(machine.IsEndState(state2));
			Assert.IsTrue(machine.IsEndState(state3));
		}

		/// <summary>
		/// Tests for the functionalities related to the triggers.
		/// </summary>
		[TestMethod]
		public void FiniteStateMachineTriggersTest()
		{
			var machine = new FiniteStateMachine<State, Transition<State>>();
			var state1 = new State("State 1");
			var state2 = new State("State 2");
			var state3 = new State("State 3");

			machine.AddState(state1);
			machine.AddState(state2);
			machine.AddState(state3);

			var trigger1 = new Trigger1("Trigger 1");
			var trigger2 = new Trigger1("Trigger 2");
			var transition1 = new Transition<State>(state1, trigger1, state2, "Transition 1");
			var transition2 = new Transition<State>(state1, trigger2, state2, "Transition 2");

			machine.AddTransition(transition1);
			machine.AddTransition(transition2);

			Assert.IsTrue(machine.NumTriggerTypes == 1);
			Assert.IsTrue(machine.TriggerCausesTransition(state1, trigger1));
			Assert.IsTrue(machine.TriggerCausesTransition(state1, trigger2));
			Assert.IsFalse(machine.TriggerCausesTransition(state2, trigger1));
			Assert.IsFalse(machine.TriggerCausesTransition(state2, trigger2));
			Assert.IsTrue(machine.HasTriggersOfType(typeof(Trigger1)));
			Assert.IsFalse(machine.HasTriggersOfType(typeof(Trigger2)));
		}

		/// <summary>
		/// Tests for the functionalities related to transitions.
		/// </summary>
		[TestMethod]
		public void FiniteStateMachineTransitionTest()
		{
			var machine = new FiniteStateMachine<State, Transition<State>>();
			var state1 = new State("State 1");
			var state2 = new State("State 2");
			var state3 = new State("State 3");

			machine.AddState(state1);
			machine.AddState(state2);
			machine.AddState(state3);

			var trigger1 = new Trigger1("Trigger 1");
			var trigger2 = new Trigger2("Trigger 2");
			var transition1 = new Transition<State>(state1, trigger1, state2, "Transition 1");
			var transition2 = new Transition<State>(state1, trigger2, state2, "Transition 2");
			var transition3 = new Transition<State>(state2, trigger2, state3, "Transition 3");

			Assert.IsFalse(machine.HasTransition(transition1));
			Assert.IsFalse(machine.HasTransition(transition2));
			Assert.IsFalse(machine.HasTransition(transition3));

			machine.AddTransition(transition1);
			machine.AddTransition(transition2);
			machine.AddTransition(transition3);

			Assert.IsTrue(machine.HasTransition(transition1));
			Assert.IsTrue(machine.HasTransition(transition2));
			Assert.IsTrue(machine.HasTransition(transition3));

			Assert.AreEqual(state2, machine.NextState(state1, trigger1));
			Assert.AreEqual(state2, machine.NextState(state1, trigger2));
			Assert.AreEqual(state3, machine.NextState(state2, trigger2));
			Assert.AreEqual(null, machine.NextState(state2, trigger1));
		}

		/// <summary>
		/// Tests for the functionalities related to transitions that are 
		/// allowed or not.
		/// </summary>
		[TestMethod]
		public void FiniteStateMachineIsAllowedTest()
		{
			var machine = new FiniteStateMachine<State, TransitionContext<State>>();
			var state1 = new State("State 1");
			var state2 = new State("State 2");
			var state3 = new State("State 3");

			machine.AddState(state1);
			machine.AddState(state2);
			machine.AddState(state3);

			var trigger1 = new Trigger1("Trigger 1");
			var trigger2 = new Trigger2("Trigger 2");

			var context = new ExternalContext();
			context.NumberToCheck = 0;

			var transition1 = new TransitionContext<State>(state1, trigger1,
				state2, "Transition 1", () => { return context.NumberToCheck == 0; });
			var transition2 = new TransitionContext<State>(state1, trigger2, 
				state2, "Transition 2", () => { return context.NumberToCheck != 0; });

			machine.AddTransition(transition1);
			machine.AddTransition(transition2);

			Assert.AreEqual(state2, machine.NextState(state1, trigger1));
			Assert.AreEqual(null, machine.NextState(state1, trigger2));

			context.NumberToCheck = 1;

			Assert.AreEqual(null, machine.NextState(state1, trigger1));
			Assert.AreEqual(state2, machine.NextState(state1, trigger2));
		}

	}
}
