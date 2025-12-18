//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Newtonsoft.Json;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// A date-time expressed in UTC.
	/// 
	/// This type is mostly interchangeable with a DateTime with Kind=Utc. 
	/// However, it is more strict on how an instance may be constructed or
	/// converted to a DateTime or DateTimeOffset:
	/// All construction/conversion is either explicitly in UTC, or explicitly
	/// references a time zone.
	/// Also, some members, like Hour, are not available, as they normally only make
	/// sense relative to a specific time zone.
	/// 
	/// If you use this type for the internal representation of exact points
	/// in time, the compiler will catch most (if not all) cases of
	/// unsafe time conversions and force explicit use of a time zone where
	/// local time is appropriate.
	/// </summary>
	public struct UtcDateTime : IComparable<UtcDateTime>, IXmlSerializable
	{
		#region Public properties

		/// <summary>
		/// Gets a UtcDateTime object that is set to the current UTC date and time
		/// according to the clock on the current computer
		/// </summary>
		public static UtcDateTime Now { get { return FromDateTimeOffset(DateTimeOffset.Now); } }

		/// <summary>
		/// Represents the smallest possible value of UtcDateTime
		/// </summary>
		public static UtcDateTime MinValue { get { return new UtcDateTime(0); } }

		/// <summary>
		/// Represents the largest possible value of UtcDateTime
		/// </summary>
		public static UtcDateTime MaxValue { get { return new UtcDateTime(DateTime.MaxValue.Ticks); } }

		/// <summary>
		/// Returns the DateTime of Kind=Utc that is equivalent to this UtcDateTime
		/// </summary>
		public DateTime InUtc => _utcDateTime;

		/// <summary>
		/// Returns the date component of this <see cref="UtcDateTime"/>
		/// </summary>
		[JsonIgnore]
		public UtcDateTime UtcDate => new(_utcDateTime.Date);

		/// <summary>
		/// Returns the time of day of this <see cref="UtcDateTime"/>
		/// </summary>
		[JsonIgnore]
		public TimeSpan TimeOfDay => _utcDateTime.TimeOfDay;

		#endregion

		#region Private data members

		/// <summary>
		/// The DateTime member that holds our value. 
		/// 
		/// This member always has Kind=Utc, except in
		/// the single case where this struct has been created using the default constructor.
		/// In that case, it equals DateTime.MinValue, with Kind=Unspecified. There is no way to prevent
		/// such construction, which is why this member has an extra underscore and we only ever
		/// read it through _utcDateTime.
		/// </summary>
		private DateTime _internal_utcDateTime;

		/// <summary>
		/// Returns the value of __utcDateTime, except if the struct has been default constructed.
		/// In this case, __utcDateTime is DateTime.MinValue with Kind=Unspecified, but we return
		/// it with Kind=Utc.
		/// </summary>
		private DateTime _utcDateTime
		{
			get
			{
				if (_internal_utcDateTime.Kind == DateTimeKind.Utc)
					return _internal_utcDateTime;

				if (_internal_utcDateTime == default)
				{
					// This struct was default constructed.
					// Replace the Unspecified DateTime with the proper Utc DateTime
					_internal_utcDateTime = new DateTime(_internal_utcDateTime.Ticks, DateTimeKind.Utc);

					return _internal_utcDateTime;
				}

				throw new Exception("Internal error: Found a non-UTC DateTime that is not default(DateTime)");
			}
		}

		#endregion

		#region Construction

		/// <summary>
		/// Creates a UTC time corresponding to the given local time values in the given time zone
		/// </summary>
		/// <param name="timeZone"></param>
		/// <param name="year"></param>
		/// <param name="month"></param>
		/// <param name="day"></param>
		/// <param name="hour"></param>
		/// <param name="minute"></param>
		/// <param name="second"></param>
		public UtcDateTime(TimeZoneInfo timeZone, int year, int month, int day, int hour, int minute, int second)
		{
			DateTime localDateTime = new(year, month, day, hour, minute, second, DateTimeKind.Unspecified);
			_internal_utcDateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone);
		}

		/// <summary>
		/// Creates a UTC time corresponding to the start of the given day in the given time zone
		/// </summary>
		/// <param name="timeZone"></param>
		/// <param name="year"></param>
		/// <param name="month"></param>
		/// <param name="day"></param>
		public UtcDateTime(TimeZoneInfo timeZone, int year, int month, int day)
			: this(timeZone, year, month, day, 0, 0, 0)
		{
		}

		/// <summary>
		/// Creates a UTC time corresponding to a given number of ticks
		/// </summary>
		/// <param name="utcTicks">The number of ticks since UtcDateTime.MinValue</param>
		private UtcDateTime(long utcTicks)
		{
			_internal_utcDateTime = new DateTime(utcTicks, DateTimeKind.Utc);
		}

		/// <summary>
		/// Creates a UTC time from a DateTime
		/// </summary>
		/// <param name="utcDateTime">The DateTime. Must have Kind=Utc</param>
		private UtcDateTime(DateTime utcDateTime)
		{
			if (utcDateTime.Kind != DateTimeKind.Utc)
				throw new ArgumentException("Expected a UTC time");

			_internal_utcDateTime = utcDateTime;
		}

		#endregion

		#region Conversion

		/// <summary>
		/// Returns the UtcDateTime that is equivalent to the given DateTimeOffset
		/// </summary>
		public static UtcDateTime FromDateTimeOffset(DateTimeOffset time)
		{
			return FromUtc(time.UtcDateTime);
		}

		/// <summary>
		/// Returns the UtcDateTime that is equivalent to the given local DateTime in the given time zone.
		/// 
		/// Note that the mapping between UTC and local time is not well defined in time zones
		/// that have daylight saving time.
		/// </summary>
		public static UtcDateTime FromTimeZone(DateTime localDateTime, TimeZoneInfo timeZone)
		{
			DateTime utcDateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone);
			return new UtcDateTime(utcDateTime);
		}

		/// <summary>
		/// Returns the UtcDateTime that is equivalent to the given DateTime, which must have Kind=Utc.
		/// </summary>
		public static UtcDateTime FromUtc(DateTime utcDateTime)
		{
			return new UtcDateTime(utcDateTime);
		}

		/// <summary>
		/// Returns the UtcDateTime date that is equivalent to the given date.
		/// The input and the result are not necessarily the same point in time, but
		/// represent midnight of the same date, possibly in different time zones.
		/// </summary>
		/// <param name="date">A date, that is, a <see cref="DateTime"/> whose TimeOfDay is zero</param>
		public static UtcDateTime FromDate(DateTime date)
		{
			if (date.TimeOfDay != TimeSpan.Zero)
				throw new ArgumentException($"{date} is not a date. It should have no time of day component");

			return FromUtc(new DateTime(date.Ticks, DateTimeKind.Utc));
		}

		/// <summary>
		/// Returns a DateTimeOffset that is equivalent to this UtcDateTime
		/// </summary>
		public DateTimeOffset ToDateTimeOffset()
		{
			return new DateTimeOffset(_utcDateTime);
		}

		/// <summary>
		/// Returns a DateTime that is equivalent to this UtcDateTime in the given time zone.
		///
		/// Note that the mapping between UTC and local time is not well defined in time zones
		/// that have daylight saving time.
		/// </summary>
		public DateTime InTimeZone(TimeZoneInfo timeZone)
		{
			return TimeZoneInfo.ConvertTimeFromUtc(_utcDateTime, timeZone);
		}

		/// <summary>
		/// Returns the date corresponding to this UtcDateTime in the given time zone.
		/// 
		/// Equivalent to InTimeZone(timeZone).Date
		/// </summary>
		public DateTime Date(TimeZoneInfo timeZone)
		{
			return InTimeZone(timeZone).Date;
		}

		/// <summary>
		/// Formats this UtcDateTime as a local time in the given time zone
		/// </summary>
		public string ToString(TimeZoneInfo timeZone)
		{
			return InTimeZone(timeZone).ToString();
		}

		/// <summary>
		/// Formats this UtcDateTime as a local time in the given time zone using the given format
		/// </summary>
		public string ToString(string format, TimeZoneInfo timeZone)
		{
			return InTimeZone(timeZone).ToString(format);
		}

		/// <summary>
		/// Returns a string representation of the time, using the given format
		/// </summary>
		public string ToString(string format)
		{
			return _utcDateTime.ToString(format) + " UTC";
		}


		#endregion

		#region Operators

		/// <summary>
		/// Compares two times
		/// </summary>
		public static bool operator <=(UtcDateTime x, UtcDateTime y) { return x._utcDateTime <= y._utcDateTime; }
		/// <summary>
		/// Compares two times
		/// </summary>
		public static bool operator >=(UtcDateTime x, UtcDateTime y) { return x._utcDateTime >= y._utcDateTime; }
		/// <summary>
		/// Compares two times
		/// </summary>
		public static bool operator <(UtcDateTime x, UtcDateTime y) { return x._utcDateTime < y._utcDateTime; }
		/// <summary>
		/// Compares two times
		/// </summary>
		public static bool operator >(UtcDateTime x, UtcDateTime y) { return x._utcDateTime > y._utcDateTime; }
		/// <summary>
		/// Compares two times
		/// </summary>
		public static bool operator ==(UtcDateTime x, UtcDateTime y) { return x._utcDateTime == y._utcDateTime; }
		/// <summary>
		/// Compares two times
		/// </summary>
		public static bool operator !=(UtcDateTime x, UtcDateTime y) { return x._utcDateTime != y._utcDateTime; }

		/// <summary>
		/// Adds a time span to a time
		/// </summary>
		public static UtcDateTime operator +(UtcDateTime x, TimeSpan y) { return new UtcDateTime(x._utcDateTime + y); }
		/// <summary>
		/// Subtracts a time span to a time
		/// </summary>
		public static UtcDateTime operator -(UtcDateTime x, TimeSpan y) { return new UtcDateTime(x._utcDateTime - y); }
		/// <summary>
		/// Returns the difference of two times
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public static TimeSpan operator -(UtcDateTime x, UtcDateTime y) { return x._utcDateTime - y._utcDateTime; }

		#endregion

		#region Standard funtions

		/// <summary>
		/// Returns true if obj is a UtcDateTime with the same time
		/// </summary>
		public override bool Equals(object obj)
		{
			if (!(obj is UtcDateTime))
				return false;

			return _utcDateTime.Equals(((UtcDateTime)obj)._utcDateTime);
		}

		/// <summary>
		/// Returns the hash code of the underlying DateTime
		/// </summary>
		public override int GetHashCode() { return _utcDateTime.GetHashCode(); }

		/// <summary>
		/// Returns a string representation of the time
		/// </summary>
		public override string ToString() { return _utcDateTime.ToString() + " UTC"; }

		#endregion

		#region Arithmetic

		/// <summary>
		/// Returns a new <see cref="UtcDateTime"/> that adds the specified number of ticks to the value of this instance.
		/// </summary>
		public UtcDateTime AddTicks(long value) { return new UtcDateTime(_utcDateTime.AddTicks(value)); }
		/// <summary>
		/// Returns a new <see cref="UtcDateTime"/> that adds the specified number of milliseconds to the value of this instance.
		/// </summary>
		public UtcDateTime AddMilliseconds(double value) { return new UtcDateTime(_utcDateTime.AddMilliseconds(value)); }
		/// <summary>
		/// Returns a new <see cref="UtcDateTime"/> that adds the specified number of seconds to the value of this instance.
		/// </summary>
		public UtcDateTime AddSeconds(double value) { return new UtcDateTime(_utcDateTime.AddSeconds(value)); }
		/// <summary>
		/// Returns a new <see cref="UtcDateTime"/> that adds the specified number of minutes to the value of this instance.
		/// </summary>
		public UtcDateTime AddMinutes(double value) { return new UtcDateTime(_utcDateTime.AddMinutes(value)); }
		/// <summary>
		/// Returns a new <see cref="UtcDateTime"/> that adds the specified number of hours to the value of this instance.
		/// </summary>
		public UtcDateTime AddHours(double value) { return new UtcDateTime(_utcDateTime.AddHours(value)); }
		/// <summary>
		/// Returns a new <see cref="UtcDateTime"/> that adds the specified number of days to the value of this instance.
		/// </summary>
		public UtcDateTime AddDays(double value) { return new UtcDateTime(_utcDateTime.AddDays(value)); }
		/// <summary>
		/// Returns a new <see cref="UtcDateTime"/> that adds the specified number of months to the value of this instance.
		/// </summary>
		public UtcDateTime AddMonths(int months) { return new UtcDateTime(_utcDateTime.AddMonths(months)); }
		/// <summary>
		/// Returns a new <see cref="UtcDateTime"/> that adds the specified number of years to the value of this instance.
		/// </summary>
		public UtcDateTime AddYears(int value) { return new UtcDateTime(_utcDateTime.AddYears(value)); }

		/// <summary>
		/// Returns the later of two UtcDateTimes
		/// </summary>
		public UtcDateTime Max(UtcDateTime other) { return this > other ? this : other; }

		/// <summary>
		/// Returns the earlier of two UtcDateTimes
		/// </summary>
		public UtcDateTime Min(UtcDateTime other) { return this < other ? this : other; }

		/// <summary>
		/// Returns the later of two UtcDateTimes if both have a value, or the one that is not null, or null if both are null
		/// </summary>
		public static UtcDateTime? Max(UtcDateTime? one, UtcDateTime? other) { return other == null || one > other ? one : other; }

		/// <summary>
		/// Returns the earlier of two UtcDateTimes if both have a value, or the one that is not null, or null if both are null
		/// </summary>
		public static UtcDateTime? Min(UtcDateTime? one, UtcDateTime? other) { return other == null || one < other ? one : other; }

		/// <summary>
		/// Returns this UtcDateTime rounded down to the nearest unit e.g. hour
		/// </summary>
		/// <param name="granularity">The unit to round to</param>
		public UtcDateTime RoundDown(TimeSpan granularity)
		{
			return new UtcDateTime(new DateTime((_utcDateTime.Ticks / granularity.Ticks) * granularity.Ticks, DateTimeKind.Utc));
		}

		/// <summary>
		/// Returns this UtcDateTime rounded up to the nearest unit e.g. hour
		/// </summary>
		/// <param name="granularity">The unit to round to</param>
		public UtcDateTime RoundUp(TimeSpan granularity)
		{
			UtcDateTime roundedDown = RoundDown(granularity);
			if (roundedDown == this)
				return this;

			return roundedDown + granularity;
		}

		/// <summary>
		/// Returns this UtcDateTime rounded (up or down) to the nearest unit e.g. hour
		/// </summary>
		/// <param name="granularity">The unit to round to</param>
		public UtcDateTime Round(TimeSpan granularity) => (this + granularity.Times(0.5)).RoundDown(granularity);

		#endregion

		#region  IComparable<UtcDateTime> implementation

		/// <summary>
		/// Compares two UtcDateTimes
		/// </summary>
		/// <param name="other">the UtcDateTime to compare with</param>
		/// <returns>-1 if this UtcDateTime is earlier, 0 if they are equal, 1 if the other UtcDateTime is earlier</returns>
		public int CompareTo(UtcDateTime other)
		{
			return _utcDateTime.CompareTo(other._utcDateTime);
		}

		#endregion

		#region IXmlSerializable implementation

		/// <summary>
		/// Not implemented
		/// </summary>
		public XmlSchema GetSchema()
		{
			throw new NotImplementedException("Xml schema for UtmDateTime is not implemented");
		}

		/// <summary>
		/// Reads a date time and converts to UTC if necessary.
		/// (If the date time was written by WriteXml(), no conversion is necessary.)
		/// </summary>
		/// <param name="reader"></param>
		public void ReadXml(XmlReader reader)
		{
			// For unknown reasons, reader.ReadElementContentAsDateTime() sometimes parses "...Z" as DateTimeKind.Unknown
			// However, DateTime.Parse seems to work correctly

			string str = reader.ReadElementContentAsString();
			var time = DateTime.Parse(str);
			var utcTime = time.ToUniversalTime();

			_internal_utcDateTime = utcTime;
		}

		/// <summary>
		/// Writes the UTC date time
		/// </summary>
		/// <param name="writer"></param>
		public void WriteXml(XmlWriter writer)
		{
			writer.WriteValue(_utcDateTime);
		}

		#endregion
	}
}

