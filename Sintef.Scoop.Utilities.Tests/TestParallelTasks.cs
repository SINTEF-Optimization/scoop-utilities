//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Sintef.Scoop.Utilities.Tests
{
	/// <summary>
	/// Tests for <see cref="ParallelTasks"/>
	/// </summary>
	[TestClass]
	public class TestParallelTasks
	{
		private List<string> _log = new();

		private int _runningTasksCount;

		private int _maxRunningTaskCount;

		[TestMethod]
		public void NoParallelizationIsDoneByDefault()
		{
			ParallelTasks.Run(LogTask);

			Assert.AreEqual(1, _log.Count);
		}

		[TestMethod]
		public void ParallelizationCanBeEnabledForCall()
		{
			ParallelTasks.Enable(2, () =>
			{
				ParallelTasks.Run(LogTask);
			});

			Assert.AreEqual(2, _log.Count);
		}

		[TestMethod]
		public void TasksActuallyRunInParallel()
		{
			ParallelTasks.Enable(2, () =>
			{
				ParallelTasks.Run(DelayAndCountTask);
			});

			Assert.AreEqual(2, _maxRunningTaskCount);
		}

		[TestMethod]
		public void NoParallelizationIsDoneByDefault_Foreach()
		{
			ParallelTasks.Foreach(Enumerable.Range(0, 2), DelayAndCountTaskForInt);

			Assert.AreEqual(1, _maxRunningTaskCount);
		}

		[TestMethod]
		public void TasksActuallyRunInParallel_Foreach()
		{
			ParallelTasks.Enable(2, () =>
			{
				ParallelTasks.Foreach(Enumerable.Range(0, 4), DelayAndCountTaskForInt);
			});

			Assert.AreEqual(2, _maxRunningTaskCount);
		}

		[TestMethod]
		public void ConcurrencyIsLimitedByProcessorCount()
		{
			ParallelTasks.Enable(10000, () =>
			{
				ParallelTasks.Run(LogTask);
			});

			Assert.AreEqual(Environment.ProcessorCount, _log.Count);
		}

		[TestMethod]
		public void ConcurrencyIsLimitedByCallArgument()
		{
			ParallelTasks.Enable(10000, () =>
			{
				ParallelTasks.Run(LogTask, 3);
			});

			Assert.AreEqual(3, _log.Count);
		}

		[TestMethod]
		public void MultiLevelParallelismWorks()
		{
			ParallelTasks.Enable(4, () =>
			{
				// Start 3 tasks
				ParallelTasks.Run(3, () =>
					// That each starts 3 tasks
					ParallelTasks.Run(3, LogTask));
			});

			Assert.AreEqual(9, _log.Count);
		}

		[TestMethod]
		public void MultiLevelParallelTasksActuallyRunInParallel()
		{
			TestUtils.RunTestWithRetries(10, Test);


			void Test()
			{
				DateTime start = DateTime.Now;

				ParallelTasks.Enable(3, () =>
				{
					// Start 2 tasks
					ParallelTasks.Run(2, () =>
						// That each starts 3 tasks
						ParallelTasks.Run(3, DelayAndCountTask));
				});

				TimeSpan duration = DateTime.Now - start;

				Assert.AreEqual(3, _maxRunningTaskCount);

				// 6 tasks of 100ms executed 3 at the time should take around 200ms.
				Assert.AreEqual(200, duration.TotalMilliseconds, 90);
			}
		}

		[TestMethod]
		public void ExceptionsInTaskArePropagatedToCaller()
		{
			void Test()
			{
				ParallelTasks.Enable(2, () =>
				{
					ParallelTasks.Run(2, () =>
						throw new Exception("Hei")
						);
				});
			}

			TestUtils.ExpectException(Test, requiredMessage: "One or more errors occurred. (One or more errors occurred. (Hei) (Hei))");
		}

		[TestMethod]
		public void EnablingCanBeDetected()
		{
			Assert.IsFalse(ParallelTasks.IsEnabled);

			ParallelTasks.Enable(2, () =>
			{
				Assert.IsTrue(ParallelTasks.IsEnabled);
			});

			// Parallelism of 1 does not count
			ParallelTasks.Enable(1, () =>
			{
				Assert.IsFalse(ParallelTasks.IsEnabled);
			});
		}

		private void DelayAndCountTask()
		{
			lock (this)
			{
				//Console.WriteLine("Start");
				++_runningTasksCount;
				_maxRunningTaskCount = Math.Max(_maxRunningTaskCount, _runningTasksCount);
			}

			Thread.Sleep(100);

			lock (this)
			{
				--_runningTasksCount;
				//Console.WriteLine("End");
			}
		}

		private void DelayAndCountTaskForInt(int i) => DelayAndCountTask();

		private void LogTask()
		{
			lock (_log)
			{
				_log.Add("Task run");
			}
		}
	}
}
