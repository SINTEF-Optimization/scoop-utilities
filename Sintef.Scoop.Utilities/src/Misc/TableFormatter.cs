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
	/// Formats a two-dimensional table of strings so that entries in the same column appear under each other when printed using a monospaced font, for instance
	/// to a console window.
	///
	/// Example:
	/// <code>
	/// TableFormatter f = new TableFormatter();
	///
	/// f.AddLine("a", "b", "c");
	/// List&lt;string&gt; ss = ["gg", "looooooong"];
	/// f.AddLine(ss);
	/// f.AddLine("jjj");
	///
	/// f.Show();
	/// </code>
	///
	/// Output:
	/// <code>
	/// a     b            c
	/// gg    looooooong    
	/// jjj
	/// </code>
	/// </summary>
	public class TableFormatter
	{
		/// <summary>
		/// The number of spaces to put between columns
		/// </summary>
		public int MarginWidth { get; set; } = 3;

		private readonly List<List<string>> _entries = new List<List<string>>();

		/// <summary>
		/// Initializes a table formatter
		/// </summary>
		/// <param name="entries">If not empty, these entries are added as the first line of the table</param>
		public TableFormatter(params string[] entries)
		{
			if (entries.Length > 0)
				AddLine(entries);
		}

		/// <summary>
		/// Adds a line to the table
		/// </summary>
		/// <param name="entries">The entries in the line</param>
		public void AddLine(params string[] entries)
		{
			_entries.Add(entries.ToList());
		}

		/// <summary>
		/// Adds a line to the table
		/// </summary>
		/// <param name="entries">The entries in the line</param>
		public void AddLine(IEnumerable<string> entries)
		{
			_entries.Add(entries.ToList());
		}

		/// <summary>
		/// Writes the table to <see cref="Console.Out"/>
		/// </summary>
		public void Show()
		{
			foreach (var line in Format(ToArray(), MarginWidth))
				Console.WriteLine(line);
		}

		/// <summary>
		/// Returns the lines of the formatted table
		/// </summary>
		public IEnumerable<string> FormattedLines => Format(ToArray(), MarginWidth);

		/// <summary>
		/// Shows the given strings in a table layout, padding short string so that
		/// strings in the same column appear under each other
		/// </summary>
		/// <param name="table"></param>
		/// <param name="marginWidth"></param>
		public static void ShowTable(string[,] table, int marginWidth = 3)
		{
			foreach (var line in Format(table, marginWidth))
				Console.WriteLine(line);
		}

		private string[,] ToArray()
		{
			int maxEntries = _entries.Max(line => line.Count);

			var table = new string[maxEntries, _entries.Count];

			foreach (int y in _entries.IndexRange())
			{
				foreach (int x in _entries[y].IndexRange())
					table[x, y] = _entries[y][x];
			}

			return table;
		}

		private static IEnumerable<string> Format(string[,] table, int marginWidth)
		{
			marginWidth = Math.Max(marginWidth, 1);

			int xsize = table.GetLength(0);
			int ysize = table.GetLength(1);

			int[] width = new int[xsize];

			for (int y = 0; y < ysize; ++y)
			{
				for (int x = 0; x < xsize; ++x)
				{
					width[x] = Math.Max(width[x], table[x, y]?.Length ?? 0);
				}
			}

			for (int y = 0; y < ysize; ++y)
			{
				string line = "";
				for (int x = 0; x < xsize; ++x)
				{
					int totalWidth = width[x];
					if (x != xsize - 1)
						totalWidth += marginWidth;

					line += (table[x, y] ?? "").PadRight(totalWidth);
				}

				yield return line;
			}
		}
	}
}
