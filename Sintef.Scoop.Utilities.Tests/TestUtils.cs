//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Threading;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Helper functions for unit testing
	/// </summary>
	static class TestUtils
	{
		/// <summary>
		/// Throws an exception if invoking the Action does not throw an exception
		/// </summary>
		/// <param name="action">The action to execute</param>
		/// <param name="requiredType">If not null, the exception thrown must be of this type</param>
		/// <param name="requiredMessage">If not null, the exception thrown must contain this message</param>
		public static void ExpectException(this Action action, Type requiredType = null, string requiredMessage = null)
		{
			try
			{
				action.Invoke();
			}
			catch (Exception ex)
			{
				if (requiredType != null)
					Assert.AreEqual(requiredType, ex.GetType());
				if (requiredMessage != null)
					Assert.AreEqual(requiredMessage, ex.Message);

				// Success
				return;
			}
			Assert.Fail("Expected an exception but did not get one");
		}

		/// <summary>
		/// Executes the test until it succeeds (does not throw an exception)
		/// or has failed a maximal number of times.
		/// 
		/// If the test does not succeeed, reports the failure message from all runs.
		/// </summary>
		/// <param name="maxFails">The maximal number of fails to allow before giving up</param>
		/// <param name="executeTest">The action that runs the test</param>
		/// <param name="sleepTime">If not null, the time to sleep after a test fails before retrying</param>
		public static void RunTestWithRetries(int maxFails, Action executeTest, TimeSpan? sleepTime = null)
		{
			int failCount = 0;

			StringBuilder messages = new StringBuilder();

			while (true)
			{
				try
				{
					executeTest();

					// Success!
					return;
				}
				catch (Exception ex)
				{
					messages.AppendLine(ex.Message);

					++failCount;

					if (failCount == maxFails)
						// We don't retry more times. Report the problems.
						Assert.Fail("The test failed {0} times. The error messages were: \n{1}", maxFails, messages);

					Thread.Sleep(sleepTime ?? TimeSpan.Zero);
				}
			}
		}
	}
}
