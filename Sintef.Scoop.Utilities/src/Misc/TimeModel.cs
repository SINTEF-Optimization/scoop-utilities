//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Xml.Linq;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// A generic time model, which can be based either on 'int' or 'long'.
	/// 
	/// The time model coverts between model time, which is counted in integers (int or long), 
	/// and external time, represented as DateTime.
	/// Model time can never be negative.
	/// Model time 0 corresponds to the external time TimeZero, and the length between 
	/// one model time and the next is a TimeUnit.
	/// 
	/// The time model also converts between external time and model days. Model day 0 starts at TimeZero.
	/// </summary>
	public abstract class TimeModel<INTTYPE> where INTTYPE : struct
	{
		#region Public properties

		/// <summary>
		/// The external time corresponding to model time 0. Is always at midnight (00:00) of some date.
		/// </summary>
		public DateTime TimeZero { get; private set; }

		/// <summary>
		/// The time unit -- the distance between one model time and the next.
		/// </summary>
		public TimeSpan TimeUnit { get; private set; }

		/// <summary>
		/// The number of time units in a day.
		/// </summary>
		public int TimeUnitsPerDay { get { return (int)(TimeSpan.FromDays(1).Ticks / TimeUnit.Ticks); } }


		/// <summary>
		/// True if the model has converted an external time that was before TimeZero
		/// </summary>
		public Boolean HasConvertedNegativeTime { get; private set; }

		/// <summary>
		/// The earliest external time converted by this time model.
		/// The default is DateZero, but it is updated if times earlier than TimeZero are converted.
		/// </summary>
		public DateTime EarliestConvertedTime { get; private set; }

		/// <summary>
		/// The earliest external time converted by this time model.
		/// The default is DateZero, but it is updated if times later than TimeZero are converted.
		/// </summary>
		public DateTime LatestConvertedTime { get; private set; }

		/// <summary>
		/// Returns the latest external time this time mode supports.
		/// </summary>
		public DateTime LatestTime { get { return GetExternalTime(MaxTime); } }

		/// <summary>
		/// Seconds per time unit.
		/// </summary>
		public double SecondsPerTimeUnit { get { return TimeUnit.TotalSeconds; } }

		/// <summary>
		/// An upper limit of the size of INTTYPE. Used for checking against overflow.
		/// </summary>
		public long MaxNumericLimit { get; private set; }

		/// <summary>
		/// The largest internal time this time model supports.
		/// </summary>
		public INTTYPE MaxTime
		{
			get
			{
				double unitsToDateTimeMax = (DateTime.MaxValue - TimeZero).TotalSeconds / SecondsPerTimeUnit;
				unitsToDateTimeMax -= 10; // Safety

				long maxTime = MaxNumericLimit;

				if (unitsToDateTimeMax < maxTime)
				{
					// MaxNumericLimit maps outside the range of DateTime.
					// Use a value that maps to close to DateTime.MaxValue
					maxTime = (long)unitsToDateTimeMax;
				}

				return CastToType(maxTime);
			}
		}

		/// <summary>
		/// If true, the time model accepts times that are earlier than the reference time,
		/// and negative time differences.
		/// </summary>
		public bool AllowTimesEarlierThanReference { get; private set; }

		#endregion

		#region Private members

		/// <summary>
		/// If true, the time model will where necessary round times to the nearest time unit.
		/// If false, all times (and time spans) are required to be an integer number of time unit,
		/// and an exception will be thrown if this is not the case.
		/// </summary>
		bool _roundToClosestTimeUnit;

		#endregion

		#region Construction

		/// <summary>
		/// Not to be used.
		/// </summary>
		private TimeModel()
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="timeZero">The external time corresponding to model time = 0.		
		///   If it is not at midnight, it is snapped to the preceding midnigth.</param>
		/// <param name="timeUnit">The time unit</param>
		/// <param name="maxNumericLimit"></param>
		/// <param name="allowTimesEarlierThanReference">If true, the time model accepts times that are earlier than the reference time,
		/// and negative time differences.</param>
		/// <param name="roundToClosestTimeUnit">If true, the time model will where necessary round times to the nearest time unit.
		/// If false, all times (and time spans) are required to be an integer number of time unit,
		/// and an exception will be thrown if this is not the case. Optional, the default value is false.</param>
		protected TimeModel(DateTime timeZero, TimeSpan timeUnit, long maxNumericLimit, bool allowTimesEarlierThanReference, bool roundToClosestTimeUnit)// = false)
		{
			TimeZero = timeZero.Date;
			TimeUnit = timeUnit;
			MaxNumericLimit = maxNumericLimit;

			EarliestConvertedTime = TimeZero;
			LatestConvertedTime = TimeZero;
			AllowTimesEarlierThanReference = allowTimesEarlierThanReference;
			_roundToClosestTimeUnit = roundToClosestTimeUnit;
		}

		/// <summary>
		/// Copy constructor.
		/// </summary>
		/// <param name="other">The time model to copy from</param>
		protected TimeModel(TimeModel<INTTYPE> other)
		{
			TimeZero = EarliestConvertedTime = other.TimeZero;
			TimeUnit = other.TimeUnit;
			_roundToClosestTimeUnit = other._roundToClosestTimeUnit;
			AllowTimesEarlierThanReference = other.AllowTimesEarlierThanReference;

			EarliestConvertedTime = TimeZero;
			LatestConvertedTime = TimeZero;
		}



		#endregion

		#region Public methods

		/// <summary>
		/// Returns the model time corresponding to the given external time.
		/// If the given external time is before TimeZero, the result is 0, and the
		/// flag HasConvertedNegativeTime is set.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="attributeDescription"></param>
		/// <returns></returns>
		public INTTYPE GetModelTime(DateTime time, string attributeDescription = "")
		{
			if (time < EarliestConvertedTime)
			{
				HasConvertedNegativeTime = true;
				EarliestConvertedTime = time;
			}

			if (time > LatestConvertedTime)
				LatestConvertedTime = time;

			if ((!AllowTimesEarlierThanReference) && time < TimeZero)
				return CastToType(0);

			TimeSpan timeAfterZero = time - TimeZero;
			long result;
			if (timeAfterZero.Ticks % TimeUnit.Ticks != 0)
			{
				if (_roundToClosestTimeUnit)
					result = (long)Math.Round(((double)timeAfterZero.Ticks) / ((double)TimeUnit.Ticks));
				else
					throw new ArgumentException(String.Format("Time {0}, given as {1}, is not a whole number of time units", time, attributeDescription));
			}
			else
				result = timeAfterZero.Ticks / TimeUnit.Ticks;

			if (Math.Abs(result) > MaxNumericLimit)
				throw new ArgumentException("The problem's time unit is too small (absolute value) compared to the time range of the problem (i.e. the span of " +
					"availability start/end times, time windows etc.). Try increasing the time unit or reducing the time horizon");

			return CastToType(result);
		}

		/// <summary>
		/// Returns the external time corresponding to the given model time.
		/// </summary>
		/// <param name="modelTime"></param>
		/// <returns></returns>
		public DateTime GetExternalTime(INTTYPE modelTime)
		{
			long modTime = CastFromType(modelTime);
			if ((!AllowTimesEarlierThanReference) && modTime < 0)
				throw new ArgumentException("Trying to convert a negative model time to external time");

			return TimeZero.AddTicks(TimeUnit.Ticks * modTime);
		}

		/// <summary>
		/// Returns the external duration corresponding to the given 
		/// number of model time units.
		/// </summary>
		public TimeSpan GetExternalDuration(INTTYPE units)
		{
			return GetExternalTime(units) - GetExternalTime(default);
		}

		/// <summary>
		/// Returns the model day corresponding to the given external time. 
		/// Day zero starts at the model's reference date, TimeZero.
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		public INTTYPE GetModelDayForExternalTime(DateTime time)
		{
			return CastToType((time - TimeZero).Days);
		}

		/// <summary>
		/// Returns the external time corresponding to the given model day.
		/// </summary>
		public DateTime GetExternalTimeForModelDay(INTTYPE day)
		{
			return TimeZero.AddDays(CastFromType(day));
		}

		/// <summary>
		/// Returns the model day corresponding to the given model time.
		/// </summary>
		/// <param name="modelTime"></param>
		/// <param name="allowNegativeModelTime">If false (the default), then negative model times will result in an exception. In general, the time model
		/// requires all model times to be non-negative. Set to true only if you know what you are doing.</param>
		/// <returns></returns>
		public INTTYPE GetDay(INTTYPE modelTime, bool allowNegativeModelTime = false)
		{
			if (allowNegativeModelTime)
				return GetModelDayForExternalTime(TimeZero.AddTicks(TimeUnit.Ticks * CastFromType(modelTime)));
			else
				return GetModelDayForExternalTime(GetExternalTime(modelTime));
		}

		/// <summary>
		/// Returns the number of minutes since midnight corresponding to the
		/// given model time. Rounded down to the closest minute.
		/// </summary>
		/// <param name="modelTime"></param>
		/// <returns></returns>
		public int GetMinutesSinceMidnight(INTTYPE modelTime)
		{
			DateTime date = GetExternalTime(modelTime);
			return (int)(date - date.Date).TotalMinutes;
		}

		/// <summary>
		/// Returns the number of time units since midnight corresponding to the
		/// given model time. Rounded down to the closest time unit.
		/// </summary>
		/// <param name="modelTime"></param>
		/// <returns></returns>
		public INTTYPE GetTimeUnitsSinceMidnight(INTTYPE modelTime)
		{
			DateTime date = GetExternalTime(modelTime);
			return GetNumberOfTimeUnits(date - date.Date, null);
		}

		/// <summary>
		/// Returns the model time corresponding to the start of the given model day
		/// </summary>
		/// <param name="day"></param>
		/// <returns></returns>
		public INTTYPE GetModelTimeAtStartOfDay(INTTYPE day)
		{
			return CastToType(CastFromType(day) * TimeUnitsPerDay); //Since _dateZero is at midnight.
		}

		/// <summary>
		/// Returns the model time corresponding to the start of the day that the given time falls on
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		public INTTYPE GetModelTimeAtStartOfDay(DateTime time)
		{
			return CastToType((time.Date - TimeZero).Days * TimeUnitsPerDay);
		}

		/// <summary>
		/// Returns the number of time units in the given time span
		/// </summary>
		/// <param name="timeSpan"></param>
		/// <param name="attributeDescription"></param>
		/// <param name="roundToNearestTimeUnit"></param>
		/// <returns></returns>
		public INTTYPE GetNumberOfTimeUnits(TimeSpan timeSpan, string attributeDescription, bool roundToNearestTimeUnit = false)
		{
			if ((!AllowTimesEarlierThanReference) && timeSpan.Ticks < 0)
				throw new ArgumentException(String.Format("Negative time span {0}, given as {1}, is not allowed", timeSpan, attributeDescription));

			long rest = timeSpan.Ticks % TimeUnit.Ticks;
			if (rest != 0)
			{
				if (roundToNearestTimeUnit)
				{
					if (Math.Abs(rest) < TimeUnit.Ticks / 2)
						timeSpan = new TimeSpan(timeSpan.Ticks - rest);
					else
						timeSpan = timeSpan.Add(new TimeSpan(TimeUnit.Ticks - rest));

					if (timeSpan.Ticks % TimeUnit.Ticks != 0)
						throw new Exception("Something is wrong in the logic here...");
				}
				else
					throw new ArgumentException(String.Format("Time span {0}, given as {1}, is not a whole number of time units", timeSpan, attributeDescription));
			}

			return CastToType(timeSpan.Ticks / TimeUnit.Ticks);
		}



		/// <summary>
		/// Returns the number of time units in the given time span, or null if
		/// the argument is null
		/// </summary>
		public INTTYPE? GetNumberOfTimeUnits(TimeSpan? timeSpan, string attributeDescription = "")
		{
			if (timeSpan == null)
				return null;
			else
				return GetNumberOfTimeUnits(timeSpan.Value, attributeDescription);
		}

		/// <summary>
		/// </summary>
		/// <param name="modelTime"></param>
		/// <returns></returns>
		public TimeSpan GetTimeSpanFromModelTime(INTTYPE modelTime)
		{
			return TimeSpan.FromTicks(TimeUnit.Ticks * CastFromType(modelTime));
		}


		/// <summary>
		/// Returns the number of time units in the given time span as a real number
		/// </summary>
		/// <param name="timeSpan"></param>
		/// <param name="attributeDescription"></param>
		/// <returns></returns>
		public double GetRealNumberOfTimeUnits(TimeSpan timeSpan, string attributeDescription)
		{
			if ((!AllowTimesEarlierThanReference) && timeSpan.Ticks < 0)
				throw new ArgumentException(String.Format("Negative time span {0}, given as {1}, is not allowed", timeSpan, attributeDescription));

			return ((double)timeSpan.Ticks / (double)TimeUnit.Ticks);
		}


		#endregion

		#region Protected methods

		/// <summary>
		/// Must be implemented in sub classes to cast from long to the
		/// value type.
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		protected abstract INTTYPE CastToType(long v);

		/// <summary>
		/// Must be implemented in sub classes to cast from long to the
		/// value type.
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		protected abstract long CastFromType(INTTYPE v);

		#endregion

		#region File IO

		/// <summary>
		/// Reads time model data from the given XElement
		/// </summary>
		/// <param name="timeModeEl"></param>
		/// <returns></returns>
		public static TimeModelInt CreateFromXElement(XElement timeModeEl)
		{
			DateTime dz = timeModeEl.TagAttribute("DateZero").Value.ParseInvariantDateTime();
			TimeSpan tu = timeModeEl.TagAttribute("TimeUnit").Value.ParseInvariantTimeSpan();

			XAttribute allowNegativeTimes = timeModeEl.TagAttribute("AllowTimesLessThanDateZero");
			bool allowNegative = (allowNegativeTimes != null) ? Convert.ToBoolean(allowNegativeTimes.Value) : false; //False for backward compatibility.
			return new TimeModelInt(dz, tu, allowNegative);
		}

		/// <summary>
		/// Writes the time model's data to the given xml writer.
		/// </summary>
		/// <param name="writer"></param>
		public void WriteToXML(System.Xml.XmlWriter writer)
		{
			writer.WriteStartElement("TimeModel");
			writer.WriteAttributeString("DateZero", TimeZero.ToInvariantString());
			writer.WriteAttributeString("TimeUnit", TimeUnit.ToInvariantString());
			writer.WriteAttributeString("AllowTimesLessThanDateZero", AllowTimesEarlierThanReference.ToString());
			writer.WriteEndElement();//TimeModel
		}

		#endregion
	}

	/// <summary>
	/// A time model based on int. It contains checks to ensure that time differences
	/// and time units are not set up so that the size of 'int' is exceeded.
	/// 
	/// The time model coverts between model time, which is counted in integers, 
	/// and external time, represented as DateTime.
	/// Model time can never be negative.
	/// Model time 0 corresponds to the external time TimeZero, and the length between 
	/// one model time and the next is a TimeUnit.
	/// 
	/// The time model also converts between external time and model days. Model day 0 starts at TimeZero.
	/// </summary>
	public class TimeModelInt : TimeModel<int>
	{
		#region Construction

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="timeZero">The external time corresponding to model time = 0.		
		///   If it is not at midnight, it is snapped to the preceding midnigth.</param>
		/// <param name="timeUnit">The time unit</param>
		/// <param name="allowTimesEarlierThanReference">If true, the time model accepts times that are earlier than the reference time,
		/// and negative time differences.</param>
		/// <param name="roundToClosestTimeUnit">If true, the time model will where necessary round times to the nearest time unit.
		/// If false, all times (and time spans) are required to be an integer number of time unit,
		/// and an exception will be thrown if this is not the case. Optional, the default value is false.</param>
		public TimeModelInt(DateTime timeZero, TimeSpan timeUnit, bool allowTimesEarlierThanReference = true, bool roundToClosestTimeUnit=false) : base(timeZero, timeUnit, int.MaxValue, allowTimesEarlierThanReference, roundToClosestTimeUnit)
		{
		}

		/// <summary>
		/// Copy constructor.
		/// </summary>
		/// <param name="other">The time model to copy from</param>
		public TimeModelInt(TimeModelInt other) : base(other)
		{
		}

		/// <summary>
		/// Returns a clone of the time model
		/// </summary>
		/// <returns></returns>
		public TimeModelInt Clone()
		{
			return new TimeModelInt(this);
		}

		#endregion

		#region Casts

		/// <summary>
		/// Must be implemented in sub classes to cast from long to the
		/// value type.
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		protected override int CastToType(long v)
		{
			return (int)v;
		}

		/// <summary>
		/// Must be implemented in sub classes to cast from long to the
		/// value type.
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		protected override long CastFromType(int v)
		{
			return v;
		}

		#endregion

	}


	/// <summary>
	/// A time model based on 'long'. 
	/// 
	/// The time model coverts between model time, which is counted in (long) integers, 
	/// and external time, represented as DateTime.
	/// Model time can never be negative.
	/// Model time 0 corresponds to the external time TimeZero, and the length between 
	/// one model time and the next is a TimeUnit.
	/// 
	/// The time model also converts between external time and model days. Model day 0 starts at TimeZero.
	/// </summary>
	public class TimeModelLong : TimeModel<long>
	{
		#region Construction

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="timeZero">The external time corresponding to model time = 0.		
		///   If it is not at midnight, it is snapped to the preceding midnigth.</param>
		/// <param name="timeUnit">The time unit</param>
		/// <param name="allowTimesEarlierThanReference">If true, the time model accepts times that are earlier than the reference time,
		/// and negative time differences.</param>
		/// <param name="roundToClosestTimeUnit">If true, the time model will where necessary round times to the nearest time unit.
		/// If false, all times (and time spans) are required to be an integer number of time unit,
		/// and an exception will be thrown if this is not the case. Optional, the default value is false.</param>
		public TimeModelLong(DateTime timeZero, TimeSpan timeUnit, bool allowTimesEarlierThanReference=true, bool roundToClosestTimeUnit = false) : base(timeZero, timeUnit, long.MaxValue, allowTimesEarlierThanReference,roundToClosestTimeUnit)
		{
		}

		/// <summary>
		/// Copy constructor.
		/// </summary>
		/// <param name="other">The time model to copy from</param>
		public TimeModelLong(TimeModelLong other) : base(other)
		{
		}

		/// <summary>
		/// Returns a clone of the time model
		/// </summary>
		/// <returns></returns>
		public TimeModelLong Clone()
		{
			return new TimeModelLong(this);
		}

		#endregion

		#region Casts

		/// <summary>
		/// Must be implemented in sub classes to cast from long to the
		/// value type.
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		protected override long CastToType(long v)
		{
			return v;
		}

		/// <summary>
		/// Must be implemented in sub classes to cast from long to the
		/// value type.
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		protected override long CastFromType(long v)
		{
			return v;
		}

		#endregion

	}
}
