//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// A logger that can stack properties so that logger is safe to use in methods
	/// with properties kept
	/// </summary>
	public class StackableLogger : IGeneralLogger
	{
		#region instance variables

		/// <summary>
		/// the underlying logger to use
		/// </summary>
		private IGeneralLogger _logger;

		/// <summary>
		/// the stack of properties
		/// </summary>
		private LinkedList<Tuple<string, string>> _propertiesStack;

		#endregion 

		#region construction

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="logger">Logger stackable logger builds upon</param>
		public StackableLogger(IGeneralLogger logger)
		{
			_logger = logger;
			LogLevelOffset = 0;
			_propertiesStack = new LinkedList<Tuple<string, string>>();
		}

		#endregion

		#region public methods

		/// <summary>
		/// Pushes a property (with value?) on the stack of properties
		/// </summary>
		/// <param name="propertyname"></param>
		/// <param name="value"></param>
		public void Push(string propertyname, string value=null)
		{
			_propertiesStack.AddLast(new Tuple<string, string>(propertyname, value));
		}

		/// <summary>
		/// Pops last property from stack (does nothing if stack empty)
		/// </summary>
		public void Pop()
		{
			if (_propertiesStack.Last != null)
				_propertiesStack.RemoveLast();
		}

		/// <summary>
		/// Clones current stackable logger, useful if used in multithreaded setting
		/// </summary>
		/// <returns></returns>
		public StackableLogger Clone()
		{
			StackableLogger other = new StackableLogger(_logger);
			other._propertiesStack = new LinkedList<Tuple<string, string>>();
			for (LinkedListNode<Tuple<string, string>> node = _propertiesStack.First;
				node != null; node = node.Next)
			{
				other._propertiesStack.AddLast(new Tuple<string,string>(node.Value.Item1, node.Value.Item2));
			}
			return other;
		}

		#endregion

		#region IGeneralLogger

		/// <summary>
		/// </summary>
		public int LogLevelOffset { get; set; }
		
		/// <inheritdoc/>	
		public GLogProperty AddOrGetProperty(string name, bool propertyAllowsNoValues = true, IEqualityComparer<string> valuesComparer = null)
		{
			return _logger.AddOrGetProperty(name, propertyAllowsNoValues, valuesComparer);
		}

		/// <inheritdoc/>	
		public void Log(int level, string message, string[,] properties = null)
		{
			level += LogLevelOffset;
			int n = _propertiesStack.Count;
			int m = 0;
			if (properties != null)
				m = properties.GetLength(0);
			string[,] all = new string[n+m,2];
			{
				int i = 0;
				for (LinkedListNode<Tuple<string, string>> node = _propertiesStack.First;
					node != null; node = node.Next)
				{
					all[i, 0] = node.Value.Item1;
					all[i, 1] = node.Value.Item2;
					++i;
				}
			}
			for(int i = 0; i < m; ++i)
			{
				all[n + i, 0] = properties[i, 0];
				all[n + i, 1] = properties[i, 1];
			}
			_logger.Log(level, message, all);
		}
		#endregion
	}
}
