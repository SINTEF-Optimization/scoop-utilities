//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Threading.Tasks;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Implements <see cref="IBgTaskRunner"/> by running the action in a <see cref="System.Threading.Tasks.Task"/>,
	/// </summary>
	public class SimpleBgTaskRunner : IBgTaskRunner
	{
		/// <summary>
		/// The delegate that executes <see cref="IBgTaskContext.BeginInvoke"/>
		/// </summary>
		internal Action<Action> BeginInvoke { get; private set; }

		/// <summary>
		/// Initializes the runner
		/// </summary>
		/// <param name="beginInvoke">The delegate to use to execute <see cref="IBgTaskContext.BeginInvoke"/>.
		///   If null, actions are run on the thread pool.</param>
		public SimpleBgTaskRunner(Action<Action> beginInvoke = null)
		{
			BeginInvoke = beginInvoke ?? (x => Task.Run(x));
		}

		/// <summary>
		/// Starts running the given action in a background task.
		/// </summary>
		/// <param name="taskAction">The action to execute in the background.</param>
		/// <returns>The task executing the action</returns>
		public IBgTask Run(Action<IBgTaskContext> taskAction)
		{
			return new BgTask(taskAction, this);
		}
	}

	/// <summary>
	/// A task run by a <see cref="SimpleBgTaskRunner"/>
	/// </summary>
	public class BgTask : IBgTask, IBgTaskContext
	{
		/// <summary>
		/// The runner managing this task
		/// </summary>
		private SimpleBgTaskRunner _bgRunner;

		/// <summary>
		/// The system task we forward to
		/// </summary>
		private Task _task;

		/// <summary>
		/// Initializes and starts the task
		/// </summary>
		public BgTask(Action<IBgTaskContext> taskAction, SimpleBgTaskRunner bgTaskRunner)
		{
			_bgRunner = bgTaskRunner;
			_task = Task.Run(() => { taskAction.Invoke(this); });
		}

		/// <summary>
		/// Returns true if the task has completed (with success or faulted).
		/// </summary>
		public bool IsCompleted => _task.IsCompleted;

		/// <summary>
		/// Waits for the task to finish. If the task faults, the
		/// resulting exception is thrown by this function.
		/// </summary>
		public void Wait() => _task.GetAwaiter().GetResult(); 

		/// <summary>
		/// Starts invoking the given action.
		/// The action is executed by <see cref="SimpleBgTaskRunner.BeginInvoke"/>.
		/// Does not wait for the action to finish (or even start).
		/// </summary>
		/// <param name="action">The action to invoke</param>
		void IBgTaskContext.BeginInvoke(Action action) { _bgRunner.BeginInvoke(action); }
	}
}
