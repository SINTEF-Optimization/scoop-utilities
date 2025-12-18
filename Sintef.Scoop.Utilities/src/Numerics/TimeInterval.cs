//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Sintef.Scoop.Utilities.OpenClosedInterval;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// An interval of time
	/// </summary>
	public class TimeInterval : Interval<DateTime>
	{
		/// <summary>
		/// The start time of the interval
		/// </summary>
		public DateTime StartTime => Start.Bound;

		/// <summary>
		/// The end time of the interval
		/// </summary>
		public DateTime EndTime => End.Bound;

		/// <summary>
		/// The center of the interval
		/// </summary>
		public DateTime Center { get { return Start.Bound + TimeSpan.FromHours(0.5 * Length.TotalHours); } }

		/// <summary>
		/// Returns the length of the time interval
		/// </summary>
		public TimeSpan Length { get { return End.Bound - Start.Bound; } }

		/// <summary>
		/// Enumerates the dates that are (partly or completely) contained in the interval
		/// </summary>
		public IEnumerable<DateTime> Dates
		{
			get
			{
				DateTime date = StartTime.Date;
				do
				{
					yield return date;
					date = date.AddDays(1);
				}
				while (this.Contains(date));
			}
		}

		/// <summary>
		/// Creates a time interval
		/// </summary>
		/// <param name="start">The start time</param>
		/// <param name="end">The end time</param>
		public TimeInterval(DateTime start, DateTime end)
			: base(start, end, true, false)
		{ }

		/// <summary>
		/// Creates a copy of the other time interval
		/// </summary>
		public TimeInterval(Interval<DateTime> other)
			: base(other)
		{
		}

		/// <summary>
		/// Creates a time interval
		/// </summary>
		/// <param name="start">The start time</param>
		/// <param name="end">The end time</param>
		/// <param name="inclusiveStart">whether start value gives an inclusive bound</param>
		/// <param name="inclusiveEnd">whether end value gives an inclusive bound</param>
		protected TimeInterval(DateTime start, DateTime end, bool inclusiveStart, bool inclusiveEnd)
			: base(start, end, inclusiveStart, inclusiveEnd)
		{ }

		/// <summary>
		/// Returns the largest time interval that is contained in both
		/// <see langword="this"/> and <paramref name="other"/>. If there is no such interval,
		/// returns null.
		/// </summary>
		public TimeInterval Intersection(TimeInterval other)
		{
			Interval<DateTime> intersection = base.Intersection(other);
			if (intersection == null)
				return null;
			return new TimeInterval(intersection);
		}

		/// <summary>
		/// Returns an extended version of this time interval, where the <see cref="StartTime"/> and <see cref="EndTime"/> have
		/// been moved earlier and later, respectively.
		/// In case <paramref name="buffer"/> is negative, returns a contracted time interval.
		/// </summary>
		/// <param name="buffer">The amount to move <see cref="StartTime"/> and <see cref="EndTime"/> by</param>
		public TimeInterval WithBuffer(TimeSpan buffer)
		{
			return new TimeInterval(StartTime - buffer, EndTime + buffer, Start.IsInclusive, End.IsInclusive);
		}

		/// <summary>
		/// Returns a time interval that has been moved the given offset
		/// </summary>
		public TimeInterval WithOffset(TimeSpan offset)
		{
			return new TimeInterval(StartTime + offset, EndTime + offset, Start.IsInclusive, End.IsInclusive);
		}

		/// <summary>
		/// Formats a TimeInterval using a readable, culture invariant format.
		/// A typical example is 2001-05-04 12:00-13:00.
		/// Seconds and the end date are included if necessary.
		/// </summary>
		public string ToStandardString()
		{
			string endTime;
			if (StartTime.Date == EndTime.Date)
			{
				if (EndTime.Second == 0)
					endTime = EndTime.ToString("HH\\:mm");
				else
					endTime = EndTime.ToString("HH\\:mm\\:ss");
			}
			else
				endTime = EndTime.ToStandardString();

			return $"{StartTime.ToStandardString()}-{endTime}";
		}
	}

	/// <summary>
	/// An interval of time that includes both the start and end time
	/// </summary>
	public class ClosedTimeInterval : TimeInterval
	{
		/// <summary>
		/// A closed interval with the maximum possible length.
		/// In practice, this is an unbounded interval.
		/// The DateTimes used have Kind Unspecified.
		/// </summary>
		public static ClosedTimeInterval UnboundedInterval { get; } = new ClosedTimeInterval(DateTime.MinValue, DateTime.MaxValue);

		/// <summary>
		/// A closed interval with the maximum possible length.
		/// In practice, this is an unbounded interval.
		/// The DateTimes used are in UTC.
		/// </summary>
		public static ClosedTimeInterval UnboundedIntervalUtc
		{
			get { return new ClosedTimeInterval(
				new DateTime(DateTime.MinValue.Ticks, DateTimeKind.Utc),
				new DateTime(DateTime.MaxValue.Ticks, DateTimeKind.Utc)); }
		}

		/// <summary>
		/// Returns true if the interval is less than [MinValue, MaxValue]
		/// </summary>
		public bool IsBounded => StartTime.Ticks != DateTime.MinValue.Ticks || EndTime.Ticks != DateTime.MaxValue.Ticks;

		/// <summary>
		/// Creates a closed time interval
		/// </summary>
		/// <param name="start">The start time</param>
		/// <param name="end">The end time</param>
		public ClosedTimeInterval(DateTime start, DateTime end)
			: base(start, end, true, true)
		{
		}

		/// <summary>
		/// Returns the intersection between this and the other interval, i.e. the
		/// interval they have in common. If there is no intersection, returns null.
		/// 
		/// As a special case, if one interval ends and the other starts at the same time,
		/// the result is an interval of zero length.
		/// </summary>
		public ClosedTimeInterval Intersection(ClosedTimeInterval other)
		{
			Interval<DateTime> intersection = base.Intersection(other);
			if (intersection == null)
				return null;

			return new ClosedTimeInterval(intersection.Start.Bound, intersection.End.Bound);
		}

		/// <summary>
		/// Returns an extended version of this time interval, where the Start and End have
		/// been moved earlier and later, respectively.
		/// In case <paramref name="buffer"/> is negative, returns a contracted time interval.
		/// </summary>
		/// <param name="buffer">The amount to move Start and End by</param>
		public new ClosedTimeInterval WithBuffer(TimeSpan buffer)
		{
			return new ClosedTimeInterval(StartTime - buffer, EndTime + buffer);
		}

		/// <summary>
		/// Returns a time interval that has been moved the given offset
		/// </summary>
		public new ClosedTimeInterval WithOffset(TimeSpan offset)
		{
			return new ClosedTimeInterval(StartTime + offset, EndTime + offset);
		}

		/// <summary>
		/// Returns the smallest time interval that covers both this interval and
		/// the given <paramref name="time"/>
		/// </summary>
		public ClosedTimeInterval ExtendedTo(DateTime time)
		{
			if (time < StartTime)
				return new ClosedTimeInterval(time, EndTime);

			if (time > EndTime)
				return new ClosedTimeInterval(StartTime, time);

			return this;
		}

		/// <summary>
		/// Returns a time interval equal to this, except that one end has been moved to enlarge it.
		/// If <paramref name="extraTime"/> is positive, it moves the end later; if negative, it moves the start earlier.
		/// </summary>
		public ClosedTimeInterval ExtendedBy(TimeSpan extraTime)
		{
			if (extraTime > TimeSpan.Zero)
				return new ClosedTimeInterval(StartTime, EndTime + extraTime);
			else
				return new ClosedTimeInterval(StartTime + extraTime, EndTime);
		}

		/// <summary>
		/// Returns the time interval from the earlier of the given times to the later.
		/// </summary>
		public static ClosedTimeInterval Between(DateTime time1, DateTime time2)
		{
			return new ClosedTimeInterval(time1.Min(time2), time1.Max(time2));
		}

		/// <summary>
		/// Returns the intersection of this time interval and the interval from
		/// <paramref name="time"/> to <see cref="DateTime.MaxValue"/>.
		/// Returns null if the intersection is empty.
		/// </summary>
		public ClosedTimeInterval ExcludedBefore(DateTime time)
		{
			if (time <= StartTime)
				return this;
			if (time > EndTime)
				return null;

			return new ClosedTimeInterval(time, EndTime);
		}

		/// <summary>
		/// Returns the intersection of this time interval and the interval from
		/// <see cref="DateTime.MinValue"/> to <paramref name="time"/>.
		/// Returns null if the intersection is empty.
		/// </summary>
		public ClosedTimeInterval ExcludedAfter(DateTime time)
		{
			if (time >= EndTime)
				return this;
			if (time < StartTime)
				return null;

			return new ClosedTimeInterval(StartTime, time);
		}
	}

	/// <summary>
	/// A list of time intervals
	/// </summary>
	public class TimeIntervals : SortedIntervals<TimeInterval, DateTime>
	{
		/// <summary>
		/// Returns the sum of the lengths of the intervals in this list
		/// </summary>
		public TimeSpan TotalLength { get { return TimeSpan.FromTicks(Intervals.Sum(x => x.Length.Ticks)); } }

		/// <summary>
		/// Creates a list of time intervals
		/// </summary>
		public TimeIntervals(IEnumerable<TimeInterval> intervals)
			: base(intervals)
		{
		}

		/// <summary>
		/// Creates a copy of the other time interval list
		/// </summary>
		/// <param name="other"></param>
		public TimeIntervals(TimeIntervals other)
			: base(other)
		{
		}

		/// <summary>
		/// Creates a list containing one time interval
		/// </summary>
		/// <param name="interval">The single time interval</param>
		public TimeIntervals(TimeInterval interval)
			: this(new TimeInterval[] { interval })
		{
		}

		/// <summary>
		/// Creates a list containing one time interval
		/// </summary>
		/// <param name="start">The start of the interval</param>
		/// <param name="end">The end of the interval</param>
		public TimeIntervals(DateTime start, DateTime end)
			: this(new TimeInterval[] { new TimeInterval(start, end) })
		{
		}

		/// <summary>
		/// Creates a list containing one time interval
		/// </summary>
		/// <param name="start">The start of the interval</param>
		/// <param name="end">The end of the interval</param>
		public TimeIntervals(IntervalLowerBound<DateTime> start, IntervalUpperBound<DateTime> end)
			: this(new TimeInterval[] { new TimeInterval(start.Bound, end.Bound) })
		{
		}

		/// <summary>
		/// Creates a time interval list that is the union of the given intervals.
		/// </summary>
		/// <param name="intervals">The intervals to create the unit of. They do not have to be 
		///   in chronological order and may overlap</param>
		/// <returns></returns>
		public static TimeIntervals UnionOf(IEnumerable<TimeInterval> intervals)
		{
			return new TimeIntervals(intervals.CreateUnion<TimeInterval, DateTime>((a, b, start, end) => new TimeInterval(start.Bound, end.Bound)));
		}

		/// <summary>
		/// Returns the first time in the time intervals where a time span can fit.
		/// Returns null if there is no such time.
		/// </summary>
		/// <param name="earliestStartTime">The earliest time that the span may start</param>
		/// <param name="requiredLength">The length of the time span; zero if null</param>
		/// <returns>The start time of the span, or null if it does not fit</returns>
		public DateTime? FirstAvailableTime(DateTime earliestStartTime, TimeSpan? requiredLength = null)
		{
			TimeSpan length = requiredLength ?? TimeSpan.Zero;

			foreach (var interval in Intervals)
			{
				DateTime start = interval.Start.Bound;
				if (start > earliestStartTime)
					earliestStartTime = start;

				DateTime end = interval.End.Bound;
				if (earliestStartTime + length <= end)
					return earliestStartTime;
			}
			return null;
		}

		/// <summary>
		/// Returns true if the given interval is fully contained in this interval list.
		/// (The interval does not need to equal one of the intervals in the list)
		/// </summary>
		public bool ContainsInterval(TimeInterval timeInterval)
		{
			return this.IntersectionWith(timeInterval).TotalLength == timeInterval.Length;
		}

		/// <summary>
		/// Returns the intersection of this interval list with the given one.
		/// </summary>
		public TimeIntervals IntersectionWith(TimeIntervals other)
		{
			TimeIntervals temp = new TimeIntervals(other); //Maybe not terribly efficient
			temp.IntersectWith(this);
			return temp;
		}

		/// <summary>
		/// Returns the union of this interval list with the given one.
		/// </summary>
		public TimeIntervals UnionWith(TimeIntervals other)
		{
			return TimeIntervals.UnionOf(this.Concat(other));
		}

		/// <summary>
		/// Returns the intersection between this interval list and the given time interval
		/// </summary>
		public TimeIntervals IntersectionWith(TimeInterval timeInterval)
		{
			return IntersectionWith(new TimeIntervals(new TimeInterval[] { timeInterval }));
		}

		/// <summary>
		/// Returns true if this interval list and the given time interval have a nonempty intersection
		/// </summary>
		public bool Intersects(TimeInterval interval)
		{
			return IntersectionWith(interval).Any();
		}

		/// <summary>
		/// Removes the given interval from this list of invervals.
		/// Intervals that intersect wholly are removed, those who intersect partially are replaced by the complement and invervals
		/// that do not intersect at all are unchanged.
		/// </summary>
		public void RemoveInterval(DateTime start, DateTime end)
		{
			RemoveInterval(new TimeInterval(start, end));
		}

		/// <inheritdoc/>
		public override SortedIntervals<TimeInterval, DateTime> Create(IEnumerable<TimeInterval> intervals)
		{
			return new TimeIntervals(intervals);
		}

		/// <inheritdoc/>
		protected override TimeInterval CreateSubInterval(TimeInterval interval, IntervalLowerBound<DateTime> start, IntervalUpperBound<DateTime> end)
		{
			return new TimeInterval(start.Bound, end.Bound);
		}
	}

	/// <summary>
	/// Extension methods for <see cref="TimeInterval"/>
	/// </summary>
	public static class TimeIntervalExtensions
	{
		/// <summary>
		/// Returns whether there exists a time interval in list with length at least of span
		/// </summary>
		/// <param name="intervals"></param>
		/// <param name="span"></param>
		/// <returns></returns>
		public static bool ProvidesTimeSpan(this IEnumerable<TimeInterval> intervals, TimeSpan span)
		{
			foreach (TimeInterval i in intervals)
			{
				if (i.Length >= span)
					return true;
			}
			return false;
		}
	}
}