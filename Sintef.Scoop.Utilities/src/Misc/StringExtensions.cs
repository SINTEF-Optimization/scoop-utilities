//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// String related extension methods.
	/// </summary>
	public static partial class Extensions
	{
		/// <summary>
		/// Small utility for writing out a list of things to a string, with a comma or other separator between them.
		/// </summary>
		/// <param name="things">The list of things, each of which will be represented by something in the output string.</param>
		/// <param name="addSpaceAfterSeparator">If true (the default value), a blank will be added after the separator.</param>
		/// <param name="elementToStringConverter">Optional converter. If not given, objecto.ToString() is used.</param>
		/// <param name="separator">Optional separator, the default is ','</param>
		/// <returns></returns>
		public static string ToSeparatedString<T>(this IEnumerable<T> things, bool addSpaceAfterSeparator = true, Func<T, string> elementToStringConverter = null, char separator = ',')
		{
			if (!things.Any())
				return string.Empty;

			string result = ConvertToString(things.First());
			foreach (var el in things.Skip(1))
			{
				result += separator;
				if (addSpaceAfterSeparator)
					result += ' ';
				result += ConvertToString(el);
			}
			return result;

			//Local utility conversion function
			string ConvertToString(T v)
			{
				if (elementToStringConverter != null)
					return elementToStringConverter(v);
				else
					return v.ToString();
			}
		}

		/// <summary>
		/// Concatenates all strings in the given sequence, with the given separator
		/// between each pair of strings
		/// </summary>
		public static string Concatenate(this IEnumerable<String> list, String separator) => string.Join(separator, list);

		/// <summary>
		/// Concatenates all strings in the given sequence, with the given separator
		/// between each pair of strings
		/// </summary>
		public static string Join(this IEnumerable<string> list, string separator) => string.Join(separator, list);

		/// <summary>
		/// Concatenates the string representation of all items in the given sequence, with the given separator
		/// between each pair of strings
		/// </summary>
		public static string JoinStrings<T>(this IEnumerable<T> list, string separator) => list.Select(x => x?.ToString() ?? "<null>").Join(separator);

		/// <summary>
		/// Concatenates the string representation of all items in the given sequence, with the given separator
		/// between each pair of strings
		/// </summary>
		/// <param name="list">The items to convert to strings</param>
		/// <param name="separator">The separator between adjacent strings</param>
		/// <param name="cultureInfoForReals">The culture to use for formatting doubles and floats</param>
		public static string JoinStrings<T>(this IEnumerable<T> list, string separator, CultureInfo cultureInfoForReals) =>
			list.Select(x => x switch
			{
				null => "<null>",
				float f => f.ToString(cultureInfoForReals),
				double d => d.ToString(cultureInfoForReals),
				object o => o.ToString()
			})
			.Join(separator);

		/// <summary>
		/// Returns the part of the source string before the first occurrence of the given substring
		/// </summary>
		public static string Before(this string source, string substring) => source.Substring(0, source.IndexOf(substring));

		/// <summary>
		/// Returns up to <paramref name="maxCount"/> items from the input list in one string, comma-separated.
		/// If there are more items in the input list, adds '...' at the end.
		/// </summary>
		public static string ListSome(this IEnumerable<string> list, int maxCount = 3)
		{
			if (list.CountIsLessOrEqual(maxCount))
				return string.Join(", ", list);
			else
				return string.Join(", ", list.Take(maxCount).Concat("..."));
		}

		/// <summary>
		/// Returns a measure of the similarity between the two strings.
		/// 
		/// It is computed as the number of length 2 substrings they have in common.
		/// E.g. "ababc" and "abcab" have similarity 3, because they share "ab" twice
		/// and "bc" once.
		/// </summary>
		/// <param name="s1">One string</param>
		/// <param name="s2">The other string</param>
		/// <returns></returns>
		public static int SimilarityWith(this string s1, string s2)
		{
			Dictionary<string, int> pairsInA = PairsIn(s1);
			Dictionary<string, int> pairsInB = PairsIn(s2);

			return pairsInA.Sum(kv => Math.Min(kv.Value, pairsInB.TryGetValue(kv.Key, out int n) ? n : 0));

			// Local function

			Dictionary<string, int> PairsIn(string s)
			{
				return Enumerable.Range(0, s.Length - 1)
								.Select(i => s.Substring(i, 2))
								.GroupBy(x => x)
								.ToDictionary(g => g.Key, g => g.Count());
			}
		}

	}
}
