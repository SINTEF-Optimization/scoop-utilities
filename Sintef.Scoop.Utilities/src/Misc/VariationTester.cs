//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Helper for testing numerous input variations in service API calls.
	/// Checks that valid data for API calls are accepted and invalid data are rejected.
	/// </summary>
	public abstract class VariationTester
	{
		/// <summary>
		/// The action used to fail the test in test framework used
		/// </summary>
		public Action<string> Fail { get; set; } = (message) =>
		{
			throw new Exception(message);
		};

		/// <summary>
		/// Action that classifies where an exception probably originates and unpacks
		/// to get to the root exception if necessary.
		/// If server exceptions and client exceptions are indistinguishable, a server exception
		/// should be reported (since client exceptions fail the test).
		/// </summary>
		public Func<Exception, (ExceptionLocation, Exception)> ClassifyException { get; set; } = null;

		/// <summary>
		/// The action used to write a line of text to the test log
		/// </summary>
		public Action<string> WriteLine { get; set; } = Console.WriteLine;

		/// <summary>
		/// Runs a number of different variations of using a service API.
		/// </summary>
		/// <param name="setupTestAction">Delegate that sets up the test. Returns two action sets,
		///   one with good (success) actions and one with bad (failing) actions.</param>
		/// <param name="testAction">Runs the test after a good or bad action has been applied.
		///   For good actions, the test should not throw an exception.
		///   For bad actions, the test should throw a FaultException or a SerializationException,
		///   possibly wrapped in a CommunicationException. The exception message may not be a generic
		///		exception message (e.g. "Sequence contains no elements") produced by the common runtime and
		///		standard library.</param>
		/// <param name="failingTestMessage">zero or more strings, separated by '/', at least one
		///   of which must be contained in the exception message for each bad action, except
		///   those that produce serialization exceptions</param>
		/// <param name="variationToRun">If given, only the variation with this number is run. Helps debugging.</param>
		/// <param name="requireDescriptionTextInErrorMessage">If true (recommended), the error message caused by a bad action
		///   must contain the description text of that action.</param>
		public void RunVariationChecks(
			Func<ActionSets> setupTestAction,
			Action testAction,
			string failingTestMessage,
			int? variationToRun = null,
			bool requireDescriptionTextInErrorMessage = true
			)
		{
			string[] textsInFailMessage = failingTestMessage.Split('/').ToArray();

			int failedChecksCount = 0;
			string firstFailMessage = null;

			// If true, only the failing checks are reported
			bool reportSuccesses = true;

			int variationNo = variationToRun ?? 0;

			Dictionary<string, int> lastVariationForErrorMessage = new Dictionary<string, int>();

			while (true)
			{
				// Get the actions for creating variations
				var actions = setupTestAction.Invoke();
				ActionSet goodActions = actions.Good;
				ActionSet badActions = actions.Bad;

				if (variationNo == 0)
				{
					// To make finding problems easier, make sure that all good descriptions are distinct and do not
					// match any bad description
					var goodDescriptions = goodActions._actions.Select(x => x.Description).ToList();
					var badDescriptions = badActions._actions.Select(x => x.Description).Distinct().ToList();
					var duplicate = goodDescriptions.Concat(badDescriptions).GroupBy(x => x).FirstOrDefault(group => group.Count() > 1);
					if (duplicate != null)
					{
						Fail($"Problem with unit test setup: Action description '{duplicate.Key}' is not unique");
					}
				}

				bool checkShouldSucceed;
				TestAction theAction;

				// Select the action to apply this time
				if (variationNo < goodActions.Count)
				{
					checkShouldSucceed = true;
					theAction = goodActions._actions[variationNo];
				}
				else
				{
					checkShouldSucceed = false;
					theAction = badActions._actions[variationNo - goodActions.Count];
				}

				Action generateVariationAction = theAction.Action;
				string description = theAction.Description;
				string variationId = variationNo.ToString() + " - " + description;

				// Apply the action to create the variation
				generateVariationAction.Invoke();

				try
				{

					// Do the testing action
					testAction.Invoke();

					// Success
					if (!checkShouldSucceed)
					{
						RegisterFailure($"\n** Failure: Variation '{variationId}' should be invalid, but did not cause an exception.");
					}
					else
					{
						if (reportSuccesses)
							WriteLine($"Success: Variation '{variationId}' was found valid.");
					}

				}
				catch (Exception exception)
				{
					ExceptionLocation type;
					(type, exception) = ClassifyException(exception);


					if (type == ExceptionLocation.InService)
					{
						// Service detected a problem

						string message = exception.Message;

						if (ExceptionIsGeneric(exception))
						{
							RegisterFailure($"\n** Failure: Variation '{variationId}' caused generic exception message '{message}'.");
						}
						else if (checkShouldSucceed)
						{
							RegisterFailure($"\n** Failure: Variation '{variationId}' should be valid, but caused exception message '{message}'.");
						}
						else if (!textsInFailMessage.Any(m => message.Contains(m)))
						{
							RegisterFailure($"\n** Failure: Variation '{variationId}' failed but message is wrong: '{message}'."
								+ $"\n(the message should contain '{failingTestMessage}')");
						}
						else if (requireDescriptionTextInErrorMessage && !message.Contains(description))
						{
							RegisterFailure($"\n** Failure: Variation '{variationId}' failed but message is wrong: '{message}'."
								+ $"\n(the message should contain '{description}')");
						}
						else
						{
							if (reportSuccesses)
								WriteLine($"Success: Variation '{variationId}' should fail and caused exception message '{message}'.");

							if (lastVariationForErrorMessage.ContainsKey(message))
							{
								int otherVariation = lastVariationForErrorMessage[message];
								string sameMessageInfo = string.Format("Same error message for variations {0} and {1}: '{2}'", lastVariationForErrorMessage[message], variationNo, message);
								if (otherVariation < variationNo - 1)
									Fail($"{sameMessageInfo}\nVariations with the same message must follow each other immediately to show it is intended.");
								else
									WriteLine("Info: " + sameMessageInfo);
							}

							lastVariationForErrorMessage[message] = variationNo;
						}
					}

					if (type == ExceptionLocation.InSerialization)
					{
						// Serialization failed. This normally

						if (checkShouldSucceed)
						{
							RegisterFailure($"\n** Failure: Variation '{variationId}' should be valid, but failed in serialization:"
								+ $"\n{exception.Message}");
						}
						else
						{
							if (reportSuccesses)
								WriteLine($"Success: Variation '{variationId}' should fail and failed in serialization: {exception.Message}");
						}
					}


					if (type == ExceptionLocation.InClient)
						// Exception in the client. We do not expect this in these tests, so propagate out and fail the test
						throw;
				}


				// Do next variation
				++variationNo;

				if (variationNo >= goodActions.Count + badActions.Count || variationToRun.HasValue)
					// All variations have been tested -- finished.
					break;
			}

			if (failedChecksCount > 0)
				Fail($"Problem with {failedChecksCount} input checks in the API." 
					+$"\nFirst message was: {firstFailMessage}");



			void RegisterFailure(string message)
			{
				WriteLine(message);

				++failedChecksCount;
				if (firstFailMessage == null)
				firstFailMessage = message;
			}
		}

		/// <summary>
		/// Verifies that two different test actions produce the same result on all variations.
		/// 
		/// On each variation, the actions must either both succeed, or both fail with
		/// the same exception message.
		/// </summary>
		/// <param name="setupAction">Delegate that sets up the test. Returns a tuple,
		///   with good (success) actions as the first element and bad (failing) actions as the second.</param>
		/// <param name="testAction1">The first action to compare</param>
		/// <param name="testAction2">The second action to compare</param>  
		/// <param name="variationToRun">If given, only the variation with this number is run. Helps debugging.</param>
		public void CompareActionsOnVariations(Func<ActionSets> setupAction, Action testAction1, Action testAction2, int? variationToRun = null)
		{
			Func<ActionSets> mySetupAction = () =>
			{
				ActionSets actions = setupAction.Invoke();

				// Move all bad actions to good
				actions.Good._actions.AddRange(actions.Bad._actions);
				actions.Bad._actions = new List<TestAction>();
				return actions;
			};

			Action testAction = () =>
			{
				Exception ex1 = null;
				Exception ex2 = null;

				try
				{
					testAction1();
				}
				catch (Exception ex)
				{
					ex1 = ex;
				}

				try
				{
					testAction2();
				}
				catch (Exception ex)
				{
					ex2 = ex;
				}

				if ((ex1 == null) != (ex2 == null))
					Fail($"One method succeeded, the other failed with message {(ex1 ?? ex2).Message}");
				if (ex1 != null && ex1.Message != ex2.Message)
					Fail($"Methods gave different messages: {ex1.Message} -- {ex2.Message}");
			};

			RunVariationChecks(mySetupAction, testAction, "", variationToRun);
		}


		/// <summary>
		/// Returns true if the given exception was produced by the .NET runtime or standard
		/// library, and not by us, and so does not contain good diagnostics.
		/// </summary>
		/// <param name="exception">The exception</param>
		/// <returns>True if the exception is generic</returns>
		private bool ExceptionIsGeneric(Exception exception)
		{
			string message = exception.Message;
			bool exceptionIsGeneric = false;

			if (message.Contains("Object reference not set to an instance of an object"))
				exceptionIsGeneric = true;
			if (message.Contains("The given key was not present in the dictionary"))
				exceptionIsGeneric = true;
			if (message.Contains("An item with the same key has already been added"))
				exceptionIsGeneric = true;
			if (message.Contains("Value cannot be null"))
				exceptionIsGeneric = true;
			if (message.Contains("Sequence contains no matching element"))
				exceptionIsGeneric = true;
			if (message.Contains("Sequence contains more than one matching element"))
				exceptionIsGeneric = true;
			if (message.Contains("Nullable object must have a value"))
				exceptionIsGeneric = true;
			if (message.Contains("Sequence contains no elements"))
				exceptionIsGeneric = true;

			return exceptionIsGeneric;
		}

		/// <summary>
		/// One set of good actions and one set of bad actions
		/// </summary>
		public class ActionSets
		{
			/// <summary>
			/// The set of good actions
			/// </summary>
			public ActionSet Good;

			/// <summary>
			/// The set of bad actions
			/// </summary>
			public ActionSet Bad;

			/// <summary>
			/// Initializes empty sets of actions
			/// </summary>
			public ActionSets()
			{
				Good = new ActionSet();
				Bad = new ActionSet();
			}
		}

		/// <summary>
		/// A set of actions, with a description for each
		/// </summary>
		public class ActionSet
		{
			/// <summary>
			/// The actions and descriptions
			/// </summary>
			public List<TestAction> _actions = new List<TestAction>();

			/// <summary>
			/// The number of actions in the set
			/// </summary>
			public int Count { get { return _actions.Count; } }

			/// <summary>
			/// Adds the given action and description to the set
			/// </summary>
			public void Add(Action action, string description)
			{
				_actions.Add(new TestAction { Action = action, Description = description });
			}

			/// <summary>
			/// Adds all actions from the other set to this set
			/// </summary>
			/// <param name="other">The other set</param>
			/// <param name="descriptionPrefix">A prefix to add to the description of each action added</param>
			public void AddRange(ActionSet other, string descriptionPrefix = "")
			{
				foreach (var action in other._actions)
					Add(action.Action, descriptionPrefix + action.Description);
			}

			/// <summary>
			/// Removes any action with the given description
			/// </summary>
			public void Remove(string description)
			{
				_actions = _actions.Where(a => a.Description != description).ToList();
			}
		}

		/// <summary>
		/// An action with a description; part of an action set
		/// </summary>
		public class TestAction
		{
			/// <summary>
			/// The action
			/// </summary>
			public Action Action;

			/// <summary>
			/// The description of the action.
			/// For a bad (error-producing) action, this is a part of the expected exception message
			/// </summary>
			public string Description;
		}

		/// <summary>
		/// Identifies where an exception was thrown
		/// </summary>
		public enum ExceptionLocation
		{
			/// <summary>
			/// Exception was thrown in (de)serialization
			/// </summary>
			InSerialization,

			/// <summary>
			/// Exception was thrown in the service
			/// </summary>
			InService,

			/// <summary>
			/// Exception was thrown in the client
			/// </summary>
			InClient
		}
	}
}
