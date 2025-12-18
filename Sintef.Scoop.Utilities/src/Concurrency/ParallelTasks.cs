//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Provides a mechanism for processing work in parallel threads, where the allowed degree
	/// of parallelism is controlled from a point higher up the call hierarchy.
	/// 
	/// To start code that may run in parallel, call the <see cref="Run(Action, int?)"/> function.
	/// The code will run in parallel if this has been enabled higher up the call hierarchy. 
	/// 
	/// To enable parallel execution for a unit of work, call <see cref="Enable"/> with the action
	/// that runs the work. The total degree of parallelism is limited across all code within the action.
	/// This means that you can call <see cref="Run(Action, int?)"/> in different, unrelated, places, including
	/// recursively, without having to allocate the available parallelism explicitly between them.
	/// 
	/// The system uses a <see cref="TaskScheduler"/> to enforce the degree of parallelism.
	/// It will not work as intended in the presence of code that starts work on a different task scheduler than
	/// the one it runs under itself.
	/// </summary>
	public class ParallelTasks
	{
		/// <summary>
		/// Returns true if parallelism is enabled, i.e. calls to <see cref="Run(Action, int?)"/> and <see cref="Foreach{T}"/>
		/// will actually execute in parallel.
		/// </summary>
		public static bool IsEnabled => TaskScheduler.Current is LimitedConcurrencyLevelTaskScheduler scheduler;

		/// <summary>
		/// Runs <paramref name="action"/> with parallel execution enabled.
		/// 
		/// Calls to <see cref="Run(Action, int?)"/> from within the action are allowed
		/// to start parallel work. The total degree of parallelism within the action will
		/// not exceed <paramref name="maxDegreeOfParallelism"/> at any point.
		/// </summary>
		/// <param name="maxDegreeOfParallelism">The maximum number of parallel tasks to execute in parallel
		///   within the action</param>
		/// <param name="action">The action that may execute work in parallel</param>
		public static void Enable(int maxDegreeOfParallelism, Action action)
		{
			if (maxDegreeOfParallelism <= 1)
			{
				action();
				return;
			}

			var scheduler = new LimitedConcurrencyLevelTaskScheduler(maxDegreeOfParallelism);
			ParallelOptions opt = new() { TaskScheduler = scheduler };

			Parallel.For(0, 1, opt, i => action());
		}

		/// <summary>
		/// Starts work that may run in parallel.
		/// 
		/// If this function is called from an action run by <see cref="Enable"/>, the supplied
		/// <paramref name="action"/> is started in several tasks that may run in parallel. The 
		/// number of parallel tasks is limited by:
		///  - <paramref name="maxTaskCount"/>, if not null
		///  - <see cref="Environment.ProcessorCount"/>
		///  - The maxDegreeOfParallelism given to <see cref="Enable"/>
		///  
		/// If not running within <see cref="Enable"/>, or only one task may be started, the action
		/// is run synchronously on the calling thread.
		/// </summary>
		/// <param name="action">The action that may be run in parallel</param>
		/// <param name="maxTaskCount">The maximum number of parallel tasks to start for the action.
		///   If null, the number of tasks is limited by <see cref="Environment.ProcessorCount"/> and
		///   the argument to <see cref="Enable"/></param>
		public static void Run(Action action, int? maxTaskCount = null)
		{
			if (maxTaskCount <= 1)
			{
				action();
				return;
			}

			if (TaskScheduler.Current is not LimitedConcurrencyLevelTaskScheduler scheduler)
			{
				action();
				return;
			}

			int taskCount = Math.Min(scheduler.MaximumConcurrencyLevel, Environment.ProcessorCount);

			if (maxTaskCount != null)
				taskCount = Math.Min(taskCount, maxTaskCount.Value);

			ParallelOptions opt = new() { TaskScheduler = scheduler };
			Parallel.For(0, taskCount, opt, i => action());
		}

		/// <inheritdoc cref="Run(Action, int?)"/>
		public static void Run(int maxTaskCount, Action action) => Run(action, maxTaskCount);

		/// <summary>
		/// Starts a loop that may run in parallel.
		/// 
		/// If this function is called from an action run by <see cref="Enable"/>, the supplied
		/// <paramref name="action"/> is run in parallel on each element of <paramref name="items"/>.
		/// The degree of parallelism is limited by the parameter to <see cref="Enable"/>.
		/// 
		/// If not called within <see cref="Enable"/>, the action is run for each element sequentially,
		/// in a regular foreach loop.
		/// </summary>
		public static void Foreach<T>(IEnumerable<T> items, Action<T> action)
		{
			if (TaskScheduler.Current is LimitedConcurrencyLevelTaskScheduler scheduler)
			{
				ParallelOptions options = new() { TaskScheduler = scheduler };
				Parallel.ForEach(items, options, action);
			}
			else
			{
				foreach (var item in items)
					action(item);
			}
		}
	}
}
