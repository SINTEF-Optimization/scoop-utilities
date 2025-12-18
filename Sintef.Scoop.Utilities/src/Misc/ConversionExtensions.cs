//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Conversion related extension methods.
	/// </summary>
	public static partial class Extensions
	{
		/// <summary>
		/// A culture invariant number format
		/// </summary>
		static private IFormatProvider _format = CultureInfo.InvariantCulture.NumberFormat;

		/// <summary>
		/// Formats a double using a culture invariant format and full ("roundtrip") precision.
		/// Reading the double back using ParseInvariantDouble is guaranteed to produce the exact
		/// same value, regardless of the locale.
		/// The decimal separator is '.'.
		/// </summary>
		public static string ToInvariantString(this System.Double x)
		{
			return x.ToString("R", _format);
		}

		/// <summary>
		/// Parses a double using a culture invariant format.
		/// This function is guaranteed to reproduce a double written using ToInvariantString exactly,
		/// regardless of the locale.
		/// The decimal separator is '.'.
		/// </summary>
		public static double ParseInvariantDouble(this System.String x)
		{
			return Double.Parse(x, _format);
		}

		/// <summary>
		/// Formats a DateTime using a culture invariant format.
		/// Reading the DateTime back using ParseInvariantDateTime is guaranteed to produce the exact
		/// same value, regardless of the locale.
		/// The format used is "yyyy-MM-ddTHH:mm:ss.fffffff[Z]", which corresponds to ISO 8601. A 'Z' is added to the end,
		/// if DateTime is UTC. 
		/// </summary>
		public static string ToInvariantString(this DateTime x)
		{
			return x.ToString("o");
		}

		/// <summary>
		/// Formats a DateTime using a readable, culture invariant format.
		/// The format used is "yyyy-MM-dd HH:mm:ss", or "yyyy-MM-dd HH:mm" if seconds is 0. 
		/// </summary>
		public static string ToStandardString(this DateTime x)
		{
			if (x.Second == 0)
				return x.ToString("yyyy-MM-dd HH\\:mm");
			else
				return x.ToString("yyyy-MM-dd HH\\:mm\\:ss");
		}

		/// <summary>
		/// Parses a DateTime using a culture invariant format.
		/// This function is guaranteed to reproduce a DateTime written using ToInvariantString exactly,
		/// regardless of the locale.
		/// The format used is "yyyy-MM-ddTHH:mm:ss.fffffff", which corresponds to ISO 8601.
		/// </summary>
		public static DateTime ParseInvariantDateTime(this System.String x)
		{
			return DateTime.Parse(x, null, DateTimeStyles.RoundtripKind);
		}

		/// <summary>
		/// Parses a DateTime using assuming it is is a universal time.
		/// </summary>
		public static DateTime ParseUniversalDateTime(this System.String x)
		{
			return DateTime.Parse(x, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
		}

		/// <summary>
		/// Formats a DateTimeOffset using a culture invariant format.
		/// Reading the DateTime back using ParseInvariantDateTime is guaranteed to produce the exact
		/// same value, regardless of the locale.
		/// The format used is "yyyy-MM-ddTHH:mm:ss.fffffff+HH:mm", which corresponds to ISO 8601.
		/// </summary>
		public static string ToInvariantString(this DateTimeOffset x)
		{
			return x.ToString("o");
		}

		/// <summary>
		/// Parses a DateTimeOffset using a culture invariant format.
		/// This function is guaranteed to reproduce a DateTime written using ToInvariantString exactly,
		/// regardless of the locale.
		/// The format expected is "yyyy-MM-ddTHH:mm:ss.fffffff+HH:mm", which corresponds to ISO 8601.
		/// </summary>
		public static DateTimeOffset ParseInvariantDateTimeOffset(this System.String x)
		{
			return DateTimeOffset.Parse(x, null, DateTimeStyles.RoundtripKind);
		}


		/// <summary>
		/// Formats a TimeSpan using a culture invariant format
		/// Reading the TimeSpan back using ParseInvariantTimeSpan is guaranteed to produce the exact
		/// same value, regardless of the locale.
		/// </summary>
		public static string ToInvariantString(this TimeSpan x)
		{
			return string.Format("{0:c}", x);
		}

		/// <summary>
		/// Parses a TimeSpan using a culture invariant format
		/// </summary>
		public static TimeSpan ParseInvariantTimeSpan(this System.String x)
		{
			return TimeSpan.Parse(x);
		}

		/// <summary>
		/// Formats a TimeSpan using the ISO 8601 format
		/// </summary>
		public static string ToISOString(this TimeSpan x)
		{
			return XmlConvert.ToString(x);
		}

		/// <summary>
		/// Parses a TimeSpan using the ISO 8601 format. This conversion does not accept non zero year or month
		/// specifiers since those are not well defined on a timespan by itself.
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static TimeSpan ParseISOTimeSpan(this System.String x)
		{
			int posOfTime = x.IndexOf('T');
			if (posOfTime >= 0)
			{
				bool fail = false;
				int posOfMonth = x.IndexOf('M');
				if (posOfMonth >= 0 && posOfMonth < posOfTime)
				{
					int i = posOfMonth - 1;
					string months = "";
					while (i > 0 && Char.IsDigit(x[i]))
						months = x[i--] + months;
					if (int.Parse(months) > 0)
						fail = true;
				}
				int posOfYear = x.IndexOf('Y');
				if (posOfYear >= 0)
				{
					int i = posOfYear - 1;
					string years = "";
					while (i > 0 && Char.IsDigit(x[i]))
						years = x[i--] + years;
					if (int.Parse(years) > 0)
						fail = true;
				}
				if (fail)
					throw new InvalidOperationException("'" + x + "' does not define an exact time span");
			}

			return XmlConvert.ToTimeSpan(x);
		}

		/// <summary>
		/// Formats a TimeSpan on the form "1d 12h 45m 2s", possibly also including milliseconds.
		/// </summary>
		/// <param name="span">The time span to format</param>
		/// <param name="includeMilliseconds">If true, milliseconds are also reported</param>
		/// <returns></returns>
		public static string ToReadableString(this TimeSpan span, bool includeMilliseconds = false)
		{
			if (span < TimeSpan.Zero)
				return $"-{(-span).ToReadableString(includeMilliseconds)}";

			List<string> parts = new();

			if (span.Days > 0)
				parts.Add($"{span.Days}d");

			if (span.Hours > 0)
				parts.Add($"{span.Hours}h");

			if (span.Minutes > 0)
				parts.Add($"{span.Minutes}m");

			if ((parts.Count == 0 && !includeMilliseconds) || span.Seconds > 0)
				parts.Add($"{span.Seconds}s");

			if (includeMilliseconds && (parts.Count == 0 || span.Milliseconds > 0))
				parts.Add($"{span.Milliseconds}ms");

			return parts.Join(" ");
		}

		/// <summary>
		/// Transforms the given sequence by calling <see cref="object.ToString"/>() on each element
		/// </summary>
		public static IEnumerable<string> ToStrings(this IEnumerable<object> source)
			=> source.Select(x => x.ToString());

		/// <summary>
		/// Returns the exception's messages, plus the messages of any inner exceptions, separated by newlines.
		/// </summary>
		public static string FullMessage(this Exception exception)
		{
			StringBuilder b = new StringBuilder(exception.Message);

			while (exception.InnerException != null)
			{
				exception = exception.InnerException;
				b.Append(Environment.NewLine).Append(exception.Message);
			}

			return b.ToString();
		}

	}
}
