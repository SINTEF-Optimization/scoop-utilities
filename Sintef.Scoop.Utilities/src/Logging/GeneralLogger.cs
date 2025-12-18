//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Log entries can have properties associated with it, like dimension=large
	/// </summary>
	public class GLogProperty
	{
		/// <summary>
		/// for escaping - the no-op pairs
		/// </summary>
		private static IEnumerable<StringPairManipulation.Pair> _static_noOpPairs = 
			StringPairManipulation.DefineStartEndPairs(new char[] { '"', '\'' });
		/// <summary>
		/// for escaping - the options
		/// </summary>
		private static StringPairManipulation.ManipulateOptions _static_manipulateOptions = 
			new StringPairManipulation.ManipulateOptions() { EnableNoOpNesting = false };
		/// <summary>
		/// for escaping - the characters to escape
		/// </summary>
		private static IEnumerable<char> _static_escapeChars = (new char[] { '[', ']', '='}).ToList<char>();

		/// <summary>
		/// Values property can have
		/// </summary>
		private HashSet<string> _values;

		/// <summary>
		/// Name of the property
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Values the property can have, might be empty (no values)
		/// </summary>
		public IEnumerable<string> Values { get { return _values; } }

		/// <summary>
		/// Whether no value is allowed.
		/// </summary>
		public bool NoValueAllowed { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="comparer"></param>
		public GLogProperty(string name, IEqualityComparer<string> comparer = null)
		{
			if (comparer == null)
				comparer = StringComparer.OrdinalIgnoreCase;

			Name = name;
			_values = new HashSet<string>(comparer);
		}

		/// <summary>
		/// Add a value to values the property can have
		/// </summary>
		/// <param name="value"></param>
		/// <returns>Whether value was added (true) or not because it already exists (false)</returns>
		public bool AddValue(string value)
		{
			return _values.Add(value);
		}

		/// <summary>
		/// Adds values to values the properties can have
		/// </summary>
		/// <param name="values"></param>
		public void AddValues(IEnumerable<string> values)
		{
			foreach (string s in values)
				_values.Add(s);
		}

		/// <summary>
		/// Returns a string representation of the property
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return String.Format("[{0}]", 
				StringPairManipulation.Escape(Name, _static_escapeChars, _static_noOpPairs, _static_manipulateOptions));
		}

		/// <summary>
		/// Returns a string representation of the property with assigned value
		/// </summary>
		/// <returns></returns>
		public string ToString(string value)
		{
			if (value != null)
				return String.Format("[{0}={1}]",
					StringPairManipulation.Escape(Name, _static_escapeChars, _static_noOpPairs, _static_manipulateOptions),
					StringPairManipulation.Escape(value, _static_escapeChars, _static_noOpPairs, _static_manipulateOptions));
			return ToString();
		}
	}

	/// <summary>
	/// An entry in a log.
	/// </summary>
	public class GLogEntry
	{
		/// <summary>
		/// The time of the log entry
		/// </summary>
		public DateTime Time { get; private set; }

		///// <summary>
		///// A related object to the logentry, can be null
		///// </summary>
		//public object RelatedObject { get; private set; }

		/// <summary>
		/// The log message
		/// </summary>
		public string Message { get; private set; }

		/// <summary>
		/// The importance of the entry. Lower levels are more important
		/// </summary>
		public int Level { get; private set; }

		/// <summary>
		/// List of properties the log entry has
		/// Each entry is a tuple of property and value for that property, where value might be null.
		/// </summary>
		public IEnumerable<Tuple<GLogProperty, string>> Properties { get; private set; }

		/// <summary>
		/// Creates a new log entry
		/// </summary>
		public GLogEntry(DateTime time, int level, string message,
			IEnumerable<Tuple<GLogProperty, string>> properties)
		{
			Time = time;
			//RelatedObject = relatedObject;
			Level = level;
			Properties = properties;
			Message = message;
		}

		/// <summary>
		/// Returns a string representation of the entry
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return ToString("yyyy-MM-dd HH\\:mm\\:ss.fffffff");
		}

		/// <summary>
		/// Returns a string representation of the entry
		/// </summary>
		/// <returns></returns>
		public string ToString(string timeFormat)
		{
			StringBuilder str = new StringBuilder();
			if (timeFormat != null || Properties.Any())
			{
				if (timeFormat != null)
					str.Append(Time.ToString(timeFormat));
				foreach (Tuple<GLogProperty, string> p in Properties)
				{
					str.Append(" ");
					str.Append(p.Item1.ToString(p.Item2));
				}
				str.Append(": ");
			}
			//str.Append(Level.ToString());
			str.Append(Message);
			return str.ToString();
		}
	}


	/// <summary>
	/// Interface for a general logger
	/// </summary>
	public interface IGeneralLogger
	{
		/// <summary>
		/// Can specify an offset to loglevel that is added to modify the loglevel of logs.
		/// Default: 0
		/// </summary>
		int LogLevelOffset { get; set; }

		/// <summary>
		/// Adds a property to list of known and accepted properties
		/// </summary>
		/// <returns>Either newly added or existing property if property already exists.</returns>
		GLogProperty AddOrGetProperty(string name,
			bool propertyAllowsNoValues = true, IEqualityComparer<string> valuesComparer = null);

		/// <summary>
		/// Adds/displays a log message.
		/// 
		/// User agents can not call this method directly, but should use the Agent.Log method.
		/// </summary>
		/// <param name="level">The level of the message</param>
		/// <param name="message">The message</param>
		/// <param name="properties">Which properties the log message shall have</param>
		void Log(int level, string message, string[,] properties = null);
	}

	/// <summary>
	/// A logger.
	/// 
	/// The logger loags messages and displays or stores them, according to the settings.
	/// Each message has a level that indicates its importance. Lower levels are more important.
	/// </summary>
	public class GeneralLogger : IGeneralLogger
	{
		/// <summary>
		/// the text writer to log to if configured this way
		/// </summary>
		public TextWriter LogTargetTextWriter { get; set; }

		/// <summary>
		/// Can specify an offset to loglevel that is added to modify the loglevel of logs.
		/// Default: 0
		/// </summary>
		public int LogLevelOffset { get; set; }

		/// <summary>
		/// If true (default), all log messages passing the filter settings at time of logging are also written to LogTargetTextWriter.
		/// </summary>
		public bool WriteToLogTarget { get; set; }

		/// <summary>
		/// The log level. Messages with level higher than this will not pass the filter settings 
		/// </summary>
		public int LogLevel { get; set; }

		/// <summary>
		/// When the number of log entries exceeds this number, the older half of all entries is removed.
		/// </summary>
		public int MaxEntryCount { get; set; }

		/// <summary>
		/// A filter text. If not null or empty, only entries that contain this text are displayed.
		/// </summary>
		public string FilterText { get; set; }

		/// <summary>
		/// A filter regular expression. If not null, only entries which when converted to string match regular expression are displayed.
		/// </summary>
		public Regex FilterRegEx { get; set; }

		/// <summary>
		/// The entries logged so far that match the current filter settings
		/// </summary>
		public IEnumerable<GLogEntry> FilteredEntries
		{
			get
			{
				List<GLogEntry> res = null;
				lock (_allEntries)
				{
					res = _allEntries.Where(e => ShouldDisplay(e)).ToList();
				}
				return res;
			}
		}

		/// <summary>
		/// Event that is raised when an entry is added to the log
		/// </summary>
		public event EventHandler<LogEntryEventArgs> EntryAdded;

		/// <summary>
		/// Event that is raised when an entry is added and is important enough to be
		/// displayed according to the current log levels.
		/// </summary>
		public event EventHandler<LogEntryEventArgs> EntryDisplayed;

		/// <summary>
		/// All entries, in the order in which they are logged.
		/// </summary>
		private List<GLogEntry> _allEntries = new List<GLogEntry>();

		/// <summary>
		/// All properties logger knows about
		/// </summary>
		private Dictionary<string, GLogProperty> _properties;

		/// <summary>
		/// Creates a logger
		/// </summary>
		public GeneralLogger(IEqualityComparer<string> propertyNameComparer = null)
		{
			LogLevelOffset = 0;
			LogTargetTextWriter = Console.Out;
			WriteToLogTarget = true;
			MaxEntryCount = 10000;
			if (propertyNameComparer == null)
				propertyNameComparer = StringComparer.OrdinalIgnoreCase;
			_properties = new Dictionary<string, GLogProperty>(propertyNameComparer);
			LogLevel = 2;
		}

		/// <summary>
		/// Creates a logger with a file named as filename as log target
		/// </summary>
		/// <param name="filename">Name of the logfile</param>
		/// <param name="propertyNameComparer"></param>
		public GeneralLogger(string filename, IEqualityComparer<string> propertyNameComparer = null)
			: this(propertyNameComparer)
		{
			LogTargetTextWriter = new StreamWriter(new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read))
			{
				AutoFlush = true
			};
			WriteToLogTarget = true;
		}

		/// <summary>
		/// Adds a property to list of known and accepted properties
		/// </summary>
		/// <returns>Either newly added or existing property if property already exists.</returns>
		public GLogProperty AddOrGetProperty(string name,
			bool propertyAllowsNoValues = true, IEqualityComparer<string> valuesComparer = null)
		{
			if (_properties.ContainsKey(name))
				return _properties[name];

			GLogProperty p = new GLogProperty(name, valuesComparer);
			p.NoValueAllowed = propertyAllowsNoValues;
			_properties[name] = p;
			return p;
		}

		/// <summary>
		/// Adds/displays a log message.
		/// 
		/// User agents can not call this method directly, but should use the Agent.Log method.
		/// </summary>
		/// <param name="level">The level of the message</param>
		/// <param name="message">The message</param>
		/// <param name="properties">Which properties the log message shall have</param>
		public void Log(int level, string message, string[,] properties = null)
		{
			level += LogLevelOffset;
			if (level < 1)
				throw new ArgumentException("Log level cannot be less than 1");

			List<Tuple<GLogProperty, string>> propList = new List<Tuple<GLogProperty, string>>();
			if (properties != null)
			{
				for (int ii_p = 0; ii_p < properties.GetLength(0); ++ii_p)
				{
					string pname = properties[ii_p, 0].Trim();
					if (pname.Length == 0)
						continue;
					if (!_properties.ContainsKey(pname))
						throw new ArgumentException("Property " + pname + " unkown!");
					GLogProperty p = _properties[pname];

					string value = properties[ii_p, 1];
					if (value != null)
					{
						value = value.Trim();
						if (value.Length == 0)
							value = null;
					}
					if (value == null)
					{
						if (!p.NoValueAllowed)
							throw new ArgumentException(String.Format("Property {0} requires value!", p.Name));
					}
					else
					{
						p.AddValue(value);
					}

					propList.Add(new Tuple<GLogProperty, string>(p, value));
				}
			}

			var entry = new GLogEntry(DateTime.Now, level, message, propList);

			if (ShouldDisplay(entry))
			{
				if (WriteToLogTarget && LogTargetTextWriter != null)
					LogTargetTextWriter.WriteLine(entry);

				if (EntryDisplayed != null)
				{
					var eventArgs = new LogEntryEventArgs { LogEntry = entry };
					EntryDisplayed.Invoke(this, eventArgs);
				}
			}

			if (EntryAdded != null)
			{
				var eventArgs = new LogEntryEventArgs { LogEntry = entry };
				EntryAdded.Invoke(this, eventArgs);
			}

			//Store the entry
			lock (_allEntries)
			{
				_allEntries.Add(entry);

				if (_allEntries.Count > MaxEntryCount)
					// Remove the first half of all messages
					_allEntries = _allEntries.Skip(MaxEntryCount / 2).ToList();
			}
		}

		/// <summary>
		/// Returns whether the given entry should be displayed under the current filter settings
		/// </summary>
		/// <param name="entry"></param>
		/// <returns></returns>
		private bool ShouldDisplay(GLogEntry entry)
		{
			if (entry.Level > LogLevel)
				return false;

			// Check if text filter matches
			if (FilterText != null && FilterText != "")
			{
				if (entry.ToString().Contains(FilterText))
					return true;

				try
				{
					if (FilterRegEx != null && FilterRegEx.IsMatch(entry.ToString()))
						return true;
				}
				catch (TimeoutException) { }

				return false;
			}

			return true;
		}

		/// <summary>
		/// Returns all entries as one string.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			string temp = "";
			foreach (var ent in _allEntries)
			{
				temp += ent.ToString() + "\r\n";
			}
			return temp;
		}

		/// <summary>
		/// Returns all entries as one string, using the given time format
		/// </summary>
		public string ToString(string timeFormat)
		{
			string temp = "";
			foreach (var ent in _allEntries)
			{
				temp += ent.ToString(timeFormat) + "\r\n";
			}
			return temp;
		}
	}


	/// <summary>
	/// Event arguments that contain a log entry
	/// </summary>
	public class LogEntryEventArgs : EventArgs
	{
		/// <summary>
		/// The log entry
		/// </summary>
		public GLogEntry LogEntry { get; set; }
	}
}
