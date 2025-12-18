//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Interface for classes that can run an action in the background.
	/// While the obvious implementation is in terms of <see cref="System.Threading.Tasks.Task"/>,
	/// there is also an implementation that allows you to run background tasks
	/// (e.g. optimization) in the context of a Scoop Simulator while ensuring that the
	/// simulation does not run quicker than real time while a background task is
	/// running.
	/// </summary>
	public interface IBgTaskRunner
	{
		/// <summary>
		/// Starts running the given action in a background task.
		/// </summary>
		/// <param name="taskAction">The action to execute in the background.</param>
		/// <returns>The task executing the action</returns>
		IBgTask Run(Action<IBgTaskContext> taskAction);
	}

	/// <summary>
	/// Context for a task running in the background
	/// </summary>
	public interface IBgTaskContext
	{
		/// <summary>
		/// Starts invoking the given action.
		/// The action is executed in the task runner's foreground
		/// context, where such a concept applies.
		/// Does not wait for the action to finish (or even start).
		/// </summary>
		/// <param name="action">The action to invoke</param>
		void BeginInvoke(Action action);
	}

	/// <summary>
	/// A task run by an <see cref="IBgTaskRunner"/>
	/// </summary>
	public interface IBgTask
	{
		/// <summary>
		/// Returns true if the task has completed (with success or faulted).
		/// </summary>
		bool IsCompleted { get; }

		/// <summary>
		/// Waits for the task to finish. If the task faults, the
		/// resulting exception is thrown by this function.
		/// </summary>
		void Wait();
	}
}
