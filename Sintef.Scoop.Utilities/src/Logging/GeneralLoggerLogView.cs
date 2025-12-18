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
	/// A class providing views on collection of log entries parsed by general logger parser
	/// </summary>
	public class GeneralLoggerLogView
	{
		/// <summary>
		/// An extended log entry including information about the source where the log entry came from
		/// </summary>
		public class ExtendedLogEntry
		{
			/// <summary>
			/// the log entry
			/// </summary>
			public GeneralLoggerParser.LogEntry Entry { get; private set; }

			/// <summary>
			/// info string that describes or identifies the source (depending on usage of GeneralLoggerLogView)
			/// </summary>
			public string SourceInfo { get; private set; }

			/// <summary>
			/// construction as extended log entry is immutable
			/// </summary>
			/// <param name="entry"></param>
			/// <param name="info"></param>
			public ExtendedLogEntry(GeneralLoggerParser.LogEntry entry, string info)
			{
				Entry = entry;
				SourceInfo = info;
			}
		}

		/// <summary>
		/// the set of entries
		/// </summary>
		private HashSet<ExtendedLogEntry> _entries;

		/// <summary>
		/// the list of source infos
		/// </summary>
		private HashSet<string> _sourceInfos;

		/// <summary>
		/// the set of property names used in at least one log entry
		/// </summary>
		private HashSet<string> _propertyNames;

		/// <summary>
		/// dictionary that gives for any used property name the collection of values that occur in the entries
		/// </summary>
		private Dictionary<string, HashSet<string>> _propertyValues;

		/// <summary>
		/// construction
		/// </summary>
		public GeneralLoggerLogView()
		{
			_entries = new HashSet<ExtendedLogEntry>();
			_sourceInfos = new HashSet<string>();
			_propertyNames = new HashSet<string>();
			_propertyValues = new Dictionary<string, HashSet<string>>();
		}

		/// <summary>
		/// Add entries from log to view, uses filename as source info
		/// </summary>
		/// <param name="filename">log to add</param>
		public void AddEntries(string filename)
		{
			AddEntries(GeneralLoggerParser.Parse(filename), filename);
		}

		/// <summary>
		/// Add given entries to view
		/// </summary>
		/// <param name="entries">Entries to add</param>
		/// <param name="sourceInfo">string describing / identifying source. If sourceInfo should uniquely identify a source,
		/// than all calls to this method adding entries from the same source, have to use the same sourceInfo string!</param>
		public void AddEntries(IEnumerable<GeneralLoggerParser.LogEntry> entries, string sourceInfo = null)
		{
			_sourceInfos.Add(sourceInfo);

			foreach (var entry in entries)
			{
				_entries.Add(new ExtendedLogEntry(entry, sourceInfo));

				foreach (string name in entry.Properties.Keys)
				{
					_propertyNames.Add(name);
					if (!_propertyValues.ContainsKey(name))
						_propertyValues[name] = new HashSet<string>();

					foreach (string value in entry.Properties[name])
						_propertyValues[name].Add(value);
				}
			}
		}

		/// <summary>
		/// all entries in the view
		/// </summary>
		public IEnumerable<ExtendedLogEntry> Entries { get { return _entries; } }

		/// <summary>
		/// all source infos that have entries added
		/// </summary>
		public IEnumerable<string> SourceInfos { get { return _sourceInfos; } }

		/// <summary>
		/// all property names that occur in the entries
		/// </summary>
		public IEnumerable<string> PropertyNames
		{
			get { return _propertyNames; }
		}

		/// <summary>
		/// all values for a given property name. If property name does not exist, an empty list is returned
		/// </summary>
		public IEnumerable<string> ValuesForProperty(string name)
		{
			if (!_propertyValues.ContainsKey(name))
				return new List<string>();

			return _propertyValues[name];
		}
	}


	/// <summary>
	/// extension methods on <see cref="IEnumerable{T}"/> for <see cref="GeneralLoggerLogView.ExtendedLogEntry"/>"/>
	/// </summary>
	public static class GeneralLoggerLogViewExtensions
	{ 
		/// <summary>
		/// returns entries containing the given property
		/// </summary>
		public static IEnumerable<GeneralLoggerLogView.ExtendedLogEntry> 
			EntriesWithProperty(this IEnumerable<GeneralLoggerLogView.ExtendedLogEntry> entries, string name)
		{
			return entries.Where(x => x.Entry.Properties.ContainsKey(name));
		}

		/// <summary>
		/// returns entries containing all of the given properties
		/// </summary>
		public static IEnumerable<GeneralLoggerLogView.ExtendedLogEntry> 
			EntriesWithProperties(this IEnumerable<GeneralLoggerLogView.ExtendedLogEntry>  entries, IEnumerable<string> names)
		{
			return entries.Where(x =>
			{
				foreach(string name in names)
				{
					if (!x.Entry.Properties.ContainsKey(name))
						return false;
				}
				return true;
			});
		}

		/// <summary>
		/// returns entries containing the given property with the given value
		/// </summary>
		public static IEnumerable<GeneralLoggerLogView.ExtendedLogEntry> 
			EntriesWithPropertyValuePair(this IEnumerable<GeneralLoggerLogView.ExtendedLogEntry> entries, string name, string value)
		{
			return entries.Where(x => x.Entry.Properties.ContainsKey(name) && x.Entry.Properties[name].Contains(value));
		}

		/// <summary>
		/// returns entries which contain at least one property with value that satisfy the choosing function
		/// </summary>
		public static IEnumerable<GeneralLoggerLogView.ExtendedLogEntry> 
			EntriesWithPropertyValuePair(this IEnumerable<GeneralLoggerLogView.ExtendedLogEntry> entries, Func<string, string, bool> valuePairChooseFunction)
		{
			return entries.Where(x =>
			{
				foreach(string key in x.Entry.Properties.Keys)
				{
					foreach(string value in x.Entry.Properties[key])
					{
						if (valuePairChooseFunction(key, value))
							return true;
					}
				}
				return false;
			});
		}
	}
}
