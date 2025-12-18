//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System.Collections.Generic;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// And thread safe wrapper around an <see cref="IGeneralLogger"/>. This invokes the underlying methods on the <see cref="IGeneralLogger"/> and blocks
	/// simultaneous execution through mutexes. Care must be taken to not invoke the underlying logger directly as that can bypass the mutexes.
	/// </summary>
	public class ThreadSafeLogger : IGeneralLogger
	{
		/// <summary>
		/// addition to underlying loggers offset
		/// </summary>
		int _logLevelOffsetAddition;

		/// <summary>
		/// lock for locking
		/// </summary>
		private object _lock;

		/// <summary>
		/// underlying logger to use
		/// </summary>
		private IGeneralLogger _logger;

		/// <summary>
		/// Constructs at thread safe logger around the given <see cref="IGeneralLogger"/>. The underlying <paramref name="logger"/> should not be invoked
		/// directly as that will bypass the thread safety.
		/// </summary>
		public ThreadSafeLogger(IGeneralLogger logger)
		{
			_lock = new object();
			_logLevelOffsetAddition = 0;
			_logger = logger;
		}

		/// <inheritdoc/>
		public int LogLevelOffset
		{
			get
			{
				lock (_lock)
				{
					return _logger.LogLevelOffset + _logLevelOffsetAddition;
				}
			}

			set
			{
				lock(_lock)
				{
					_logLevelOffsetAddition = value - _logger.LogLevelOffset;
				}
			}
		}

		/// <inheritdoc/>
		public GLogProperty AddOrGetProperty(string name, bool propertyAllowsNoValues = true, IEqualityComparer<string> valuesComparer = null)
		{
			lock(_lock)
			{
				return _logger.AddOrGetProperty(name, propertyAllowsNoValues, valuesComparer);
			}
		}

		/// <inheritdoc/>
		public void Log(int level, string message, string[,] properties = null)
		{
			lock(_lock)
			{
				_logger.Log(level, message, properties);
			}
		}
	}
}
