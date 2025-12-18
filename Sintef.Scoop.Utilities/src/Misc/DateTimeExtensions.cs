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
	/// DateTime related extension methods.
	/// </summary>
	public static partial class Extensions
	{
		/// <summary>
		/// Converts a DateTime to a DateTimeOffset with proper handling of MinValue and MaxValue
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static DateTimeOffset ToDateTimeOffset(this DateTime dt)
		{
			if (dt == DateTime.MinValue)
				return DateTimeOffset.MinValue;

			if (dt == DateTime.MaxValue)
				return DateTimeOffset.MaxValue;

			return new DateTimeOffset(dt);
		}

		/// <summary>
		/// Converts a nullable DateTime to a nullable DateTimeOffset with proper handling of MinValue and MaxValue
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static DateTimeOffset? ToDateTimeOffset(this DateTime? dt)
		{
			return dt.HasValue ? dt.Value.ToDateTimeOffset() : (DateTimeOffset?)null;
		}

		/// <summary>
		/// Converts a DateTimeOffset to a DateTime with proper handling of MinValue and MaxValue.
		/// A DateTimeOffset different from MinValue and MaxValue will be converted to a DateTime of Kind Utc
		/// </summary>
		/// <param name="dto"></param>
		/// <returns></returns>
		public static DateTime ToDateTime(this DateTimeOffset dto)
		{
			if (dto == DateTimeOffset.MinValue)
				return DateTime.MinValue;

			if (dto == DateTimeOffset.MaxValue)
				return DateTime.MaxValue;

			return dto.UtcDateTime;
		}

		/// <summary>
		/// Converts a nullable DateTimeOffset to a nullable DateTime with proper handling of MinValue and MaxValue.
		/// A nullable DateTimeOffset different from null, MinValue and MaxValue will be converted to a DateTime of Kind Utc
		/// </summary>
		/// <param name="dto"></param>
		/// <returns></returns>
		public static DateTime? ToDateTime(this DateTimeOffset? dto)
		{
			return dto.HasValue ? dto.Value.ToDateTime() : (DateTime?)null;
		}

		/// <summary>
		/// Return the week day after a given week day.
		/// </summary>
		public static DayOfWeek NextDay(DayOfWeek day)
		{
			if (day == DayOfWeek.Saturday)
				return DayOfWeek.Sunday;
			else
				return ++day;
		}

		/// <summary>
		/// Return the week day before a given week day.
		/// </summary>
		public static DayOfWeek PreviousDay(DayOfWeek day)
		{
			if (day == DayOfWeek.Sunday)
				return DayOfWeek.Saturday;
			else
				return --day;
		}

		/// <summary>
		/// Returns the minimum of two DateTimes
		/// </summary>
		public static DateTime Min(this DateTime first, DateTime second)
		{
			return first < second ? first : second;
		}

		/// <summary>
		/// Returns the maximum of two DateTimes
		/// </summary>
		public static DateTime Max(this DateTime first, DateTime second)
		{
			return first > second ? first : second;
		}

		/// <summary>
		/// Returns the minimum of two DateTimeOffsets
		/// </summary>
		public static DateTimeOffset Min(this DateTimeOffset first, DateTimeOffset second)
		{
			return first < second ? first : second;
		}

		/// <summary>
		/// Returns the maximum of two DateTimeOffsets
		/// </summary>
		public static DateTimeOffset Max(this DateTimeOffset first, DateTimeOffset second)
		{
			return first > second ? first : second;
		}

		/// <summary>
		/// Returns the sum of TimeSpans
		/// </summary>
		public static TimeSpan Sum(this IEnumerable<TimeSpan> source)
		{
			return TimeSpan.FromTicks(source.Sum(x => x.Ticks));
		}

		/// <summary>
		/// Returns the sum of TimeSpans
		/// </summary>
		/// <param name="source">The source enumerable</param>
		/// <param name="selector">The selector that finds the TimeSpan for a source item</param>
		public static TimeSpan Sum<T>(this IEnumerable<T> source, Func<T, TimeSpan> selector)
		{
			return TimeSpan.FromTicks(source.Sum(x => selector(x).Ticks));
		}

		/// <summary>
		/// Returns the absolute value of the given time span
		/// </summary>
		public static TimeSpan Abs(this TimeSpan span)
		{
			return span > TimeSpan.Zero ? span : -span;
		}

		/// <summary>
		/// Returns the minimum of two TimeSpans
		/// </summary>
		public static TimeSpan Min(this TimeSpan first, TimeSpan second)
		{
			return first < second ? first : second;
		}

		/// <summary>
		/// Returns the maximum of two TimeSpans
		/// </summary>
		public static TimeSpan Max(this TimeSpan first, TimeSpan second)
		{
			return first > second ? first : second;
		}

		/// <summary>
		/// Returns the product of a time span times an integer factor
		/// </summary>
		/// <param name="span">The time span</param>
		/// <param name="factor">The integer factor</param>
		/// <returns>The product</returns>
		public static TimeSpan Times(this TimeSpan span, int factor)
		{
			return TimeSpan.FromTicks(span.Ticks * factor);
		}

		/// <summary>
		/// Returns the product of a time span times a real factor
		/// </summary>
		/// <param name="span">The time span</param>
		/// <param name="factor">The real factor</param>
		/// <returns>The product</returns>
		public static TimeSpan Times(this TimeSpan span, double factor)
		{
			return TimeSpan.FromTicks((long)(span.Ticks * factor));
		}

		/// <summary>
		/// Returns the product of a time span times a real factor, rounded up
		/// </summary>
		/// <param name="span">The time span</param>
		/// <param name="factor">The real factor</param>
		/// <returns>The product</returns>
		public static TimeSpan TimesCeil(this TimeSpan span, double factor)
		{
			return TimeSpan.FromTicks((long)Math.Ceiling(span.Ticks * factor));
		}

		/// <summary>
		/// Compares two DateTime's with a tolerance in milliseconds.
		/// Returns true if this value is definitely less than the other value
		/// within the tolerance.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="toleranceInMilliseconds">Non-negative tolerance, in milliseconds.</param>
		public static bool LessThanWithTolerance(this DateTime x, DateTime y, double toleranceInMilliseconds)
		{
			return (y - x).TotalMilliseconds > toleranceInMilliseconds;
		}

		/// <summary>
		/// Compares two DateTime's with a tolerance in milliseconds.
		/// Returns true if this value is definitely not larger than the other value
		/// within the tolerance.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="toleranceInMilliseconds">Non-negative tolerance, in milliseconds.</param>
		public static bool LessOrEqualWithTolerance(this DateTime x, DateTime y, double toleranceInMilliseconds)
		{
			return x < y || x.EqualsWithTolerance(y, toleranceInMilliseconds);
		}

		/// <summary>
		/// Compares two DateTime's with a tolerance in milliseconds.
		/// Returns true if this value is definitely greater than the other value
		/// within the tolerance.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="toleranceInMilliseconds">Non-negative tolerance, in milliseconds.</param>
		public static bool GreaterThanWithTolerance(this DateTime x, DateTime y, double toleranceInMilliseconds)
		{
			return (x - y).TotalMilliseconds > toleranceInMilliseconds;
		}

		/// <summary>
		/// Compares two DateTime's with a tolerance in milliseconds.
		/// Returns true if this value is definitely not smaller than the other value
		/// within the tolerance.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="toleranceInMilliseconds">Non-negative tolerance, in milliseconds.</param>
		public static bool GreaterOrEqualWithTolerance(this DateTime x, DateTime y, double toleranceInMilliseconds)
		{
			return x > y || x.EqualsWithTolerance(y, toleranceInMilliseconds);
		}

		/// <summary>
		/// Compares two DateTime's with a tolerance in milliseconds.
		/// Returns true if this value equals the other value
		/// within the tolerance.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="toleranceInMilliseconds">Non-negative tolerance, in milliseconds.</param>
		public static bool EqualsWithTolerance(this DateTime x, DateTime y, double toleranceInMilliseconds)
		{
			return Math.Abs((y - x).TotalMilliseconds) < toleranceInMilliseconds;
		}

		/// <summary>
		/// Rounds the given DateTime down to the nearest multiple of the given TimeSpan.
		/// Use e.g. like this: myTruncatedDateTime = myDateTime.Truncate(TimeSpan.FromSeconds(1));
		/// </summary>
		/// <param name="dateTime"></param>
		/// <param name="timeSpan"></param>
		/// <returns></returns>
		public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
		{
			if (timeSpan == TimeSpan.Zero) return dateTime; // Or could throw an ArgumentException
			return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
		}

		/// <summary>
		/// Rounds the given TimeSpan down to the nearest multiple of the given <paramref name="resolution"/>.
		/// Use e.g. like this:  myTruncatedTimeSpan = myTimeSpan.Truncate(TimeSpan.FromSeconds(1));
		/// </summary>
		/// <param name="timeSpan"></param>
		/// <param name="resolution"></param>
		/// <returns></returns>
		public static TimeSpan Truncate(this TimeSpan timeSpan, TimeSpan resolution)
		{
			if (resolution == TimeSpan.Zero) return timeSpan; // Or could throw an ArgumentException
			long mod = timeSpan.Ticks % resolution.Ticks;
			if (mod != 0)
			{
				TimeSpan result = timeSpan.Add(new TimeSpan(-mod));
				return result;
			}
			else
				return timeSpan;
		}

	}
}
