//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Xml;

namespace Sintef.Scoop.Utilities.Tests
{
	[TestClass]
	public class TestUtcDateTime
	{
		const string _isoFormat = "yyyy\\-MM\\-dd\\THH\\:mm\\:ss\\.fffffff";

		UtcDateTime _default = default;
		TimeZoneInfo _cet = TimeZoneInfo.GetSystemTimeZones().First(z => z.StandardName == "Central European Standard Time");
		TimeZoneInfo _utc = TimeZoneInfo.FindSystemTimeZoneById("UTC");

		DateTime _2PmInWinter_Undefined = new(2017, 1, 1, 14, 0, 0);
		DateTime _2PmInWinter_Local = new(2017, 1, 1, 14, 0, 0, DateTimeKind.Local);
		DateTime _2PmInWinter_Utc = new(2017, 1, 1, 14, 0, 0, DateTimeKind.Utc);

		UtcDateTime _utcTime = UtcDateTime.FromUtc(new DateTime(2017, 1, 2, 3, 4, 5, DateTimeKind.Utc));

		[TestMethod]
		public void DefaultDateTimeIsMinValue()
		{
			Assert.AreEqual(UtcDateTime.MinValue, _default);
		}

		[TestMethod]
		public void MinValueAndMaxValueMatchDateTimeticks()
		{
			Assert.AreEqual(DateTime.MinValue.Ticks, UtcDateTime.MinValue.InUtc.Ticks);
			Assert.AreEqual(DateTime.MaxValue.Ticks, UtcDateTime.MaxValue.InUtc.Ticks);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void CannotDecreaseMinValue()
		{
			var less = UtcDateTime.MinValue.AddTicks(-1);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void CannotIncreaseMaxValue()
		{
			var more = UtcDateTime.MaxValue.AddTicks(1);
		}

		[TestMethod]
		public void NowChangesAsExpected()
		{
			var dateTime1 = DateTime.Now;
			var utcTime1 = UtcDateTime.Now;
			Thread.Sleep(50);
			var utcTime2 = UtcDateTime.Now;
			var dateTime2 = DateTime.Now;

			var utcDiff = utcTime2 - utcTime1;
			var dateTimeDiff = dateTime2 - dateTime1;
			Assert.IsTrue(utcDiff > TimeSpan.Zero);
			Assert.IsTrue(utcDiff <= dateTimeDiff);
		}

		[TestMethod]
		public void ConstructorsWorkAsExpected()
		{
			var time = new UtcDateTime(_cet, 2017, 1, 2);

			var localTime = time.InTimeZone(_cet);
			Assert.AreEqual(2017, localTime.Year);
			Assert.AreEqual(1, localTime.Month);
			Assert.AreEqual(2, localTime.Day);
			Assert.AreEqual(0, localTime.Hour);
			Assert.AreEqual(0, localTime.Minute);
			Assert.AreEqual(0, localTime.Second);

			time = new UtcDateTime(_cet, 2017, 1, 2, 3, 4, 5);

			localTime = time.InTimeZone(_cet);
			Assert.AreEqual(2017, localTime.Year);
			Assert.AreEqual(1, localTime.Month);
			Assert.AreEqual(2, localTime.Day);
			Assert.AreEqual(3, localTime.Hour);
			Assert.AreEqual(4, localTime.Minute);
			Assert.AreEqual(5, localTime.Second);
		}

		[TestMethod]
		public void FromDateTimeOffsetWorks()
		{
			// 12 PM in UTC+2
			DateTimeOffset offset = new(2017, 1, 1, 12, 0, 0, TimeSpan.FromHours(2));

			UtcDateTime time = UtcDateTime.FromDateTimeOffset(offset);

			// is 10 AM in UTC
			Assert.AreEqual(10, time.InUtc.Hour);
		}

		[TestMethod]
		public void FromTimeZoneWorks()
		{
			var time = UtcDateTime.FromTimeZone(_2PmInWinter_Undefined, _cet);

			Assert.AreEqual(13, time.InUtc.Hour);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void FromTimeRequiresUndefinedKind()
		{
			var time = UtcDateTime.FromTimeZone(_2PmInWinter_Local, _cet);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void FromTimeRequiresUndefinedKind2()
		{
			var time = UtcDateTime.FromTimeZone(_2PmInWinter_Utc, _cet);
		}

		[TestMethod]
		public void FromUtcWorks()
		{
			var time = UtcDateTime.FromUtc(_2PmInWinter_Utc);

			Assert.AreEqual(14, time.InUtc.Hour);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void FromUtcRequiresUtcKind()
		{
			var time = UtcDateTime.FromUtc(_2PmInWinter_Local);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void FromUtcRequiresUtcKind2()
		{
			var time = UtcDateTime.FromUtc(_2PmInWinter_Undefined);
		}

		[TestMethod]
		public void ToDateTimeOffsetWorks()
		{
			var offset = _utcTime.ToDateTimeOffset();

			Assert.AreEqual(2017, offset.Year);
			Assert.AreEqual(1, offset.Month);
			Assert.AreEqual(2, offset.Day);
			Assert.AreEqual(3, offset.Hour);
			Assert.AreEqual(4, offset.Minute);
			Assert.AreEqual(5, offset.Second);
			Assert.AreEqual(TimeSpan.Zero, offset.Offset);
		}

		[TestMethod]
		public void InTimeZoneWorks()
		{
			var time = UtcDateTime.FromUtc(_2PmInWinter_Utc);
			var localTime = time.InTimeZone(_cet);

			Assert.AreEqual(2017, localTime.Year);
			Assert.AreEqual(1, localTime.Month);
			Assert.AreEqual(1, localTime.Day);
			Assert.AreEqual(15, localTime.Hour);
		}

		[TestMethod]
		public void DateWorks()
		{
			var date = _utcTime.Date(_utc);

			Assert.AreEqual(new DateTime(2017, 1, 2), date);
		}

		[TestMethod]
		public void ToStringWorks()
		{
			// Compare with DateTime.ToString to compensate for locale settings
			string expectedUtcDateTime = new DateTime(2017, 1, 2, 3, 4, 5).ToString();  // "2017-01-02 03:04:05" or "02.01.2017 03:04:05" or ...
			string expectedCetDateTime = new DateTime(2017, 1, 2, 4, 4, 5).ToString(); // "2017-01-02 04:04:05" or "02.01.2017 04:04:05" or ...

			// ToString() without arguments:
			Assert.AreEqual($"{expectedUtcDateTime} UTC", _utcTime.ToString());

			// with time zone argument
			Assert.AreEqual(expectedUtcDateTime, _utcTime.ToString(_utc));
			Assert.AreEqual(expectedCetDateTime, _utcTime.ToString(_cet));

			// with format argument
			Assert.AreEqual("03:04:05 - 01/02/17 UTC", _utcTime.ToString(@"hh\:mm\:ss - MM\/dd\/yy"));

			// with format and time zone arguments
			Assert.AreEqual("03:04:05 - 01/02/17", _utcTime.ToString(@"hh\:mm\:ss - MM\/dd\/yy", _utc));
			Assert.AreEqual("04:04:05 - 01/02/17", _utcTime.ToString(@"hh\:mm\:ss - MM\/dd\/yy", _cet));
		}

		[TestMethod]
		public void OperatorsWork()
		{
			var time1 = UtcDateTime.Now;
			var time1b = time1;
			var time2 = time1.AddTicks(1);

			Assert.IsTrue(time1 <= time1b);
			Assert.IsTrue(time1b <= time1);
			Assert.IsTrue(time1 <= time2);
			Assert.IsFalse(time2 <= time1);

			Assert.IsTrue(time1 >= time1b);
			Assert.IsTrue(time1b >= time1);
			Assert.IsFalse(time1 >= time2);
			Assert.IsTrue(time2 >= time1);

			Assert.IsFalse(time1 < time1b);
			Assert.IsFalse(time1b < time1);
			Assert.IsTrue(time1 < time2);
			Assert.IsFalse(time2 < time1);

			Assert.IsFalse(time1 > time1b);
			Assert.IsFalse(time1b > time1);
			Assert.IsFalse(time1 > time2);
			Assert.IsTrue(time2 > time1);

			Assert.IsTrue(time1 == time1b);
			Assert.IsTrue(time1b == time1);
			Assert.IsFalse(time1 == time2);
			Assert.IsFalse(time2 == time1);

			Assert.IsFalse(time1 != time1b);
			Assert.IsFalse(time1b != time1);
			Assert.IsTrue(time1 != time2);
			Assert.IsTrue(time2 != time1);

			var oneTick = TimeSpan.FromTicks(1);
			Assert.AreEqual(oneTick, time2 - time1);
			Assert.AreEqual(time2, time1 + oneTick);
			Assert.AreEqual(time1, time2 - oneTick);
		}

		[TestMethod]
		public void EqualsAndHashCodeWork()
		{
			var time1 = UtcDateTime.Now;
			var time1b = time1;
			var time2 = time1.AddTicks(1);

			Assert.IsTrue(time1.Equals(time1));
			Assert.IsTrue(time1.Equals(time1b));
			Assert.IsTrue(time1b.Equals(time1));

			Assert.IsFalse(time1.Equals(time2));
			Assert.IsFalse(time2.Equals(time1));

			Assert.IsFalse(time1.Equals(null));
			Assert.IsFalse(time1.Equals(1));
			Assert.IsFalse(time1.Equals("hei"));
			Assert.IsFalse(time1.Equals(this));

			Assert.AreEqual(time1.GetHashCode(), time1b.GetHashCode());
			Assert.AreNotEqual(time1.GetHashCode(), time2.GetHashCode());
		}

		[TestMethod]
		public void ArithmeticsWork()
		{
			Assert.AreEqual("2017-01-02T03:04:05.0000001 UTC", _utcTime.AddTicks(1).ToString(_isoFormat));
			// According to the documentation, DateTime.AddMilliseconds should round to the nearest millisecond.
			// From .NET 7 and onwards however this does not happen. If this is changed back in a future version,
			// swap out the test in the line below with the commented one in the line below it.
			Assert.AreEqual("2017-01-02T03:04:05.0015000 UTC", _utcTime.AddMilliseconds(1.5).ToString(_isoFormat));
			//Assert.AreEqual("2017-01-02T03:04:05.0020000 UTC", _utcTime.AddMilliseconds(1.5).ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:04:06.5000000 UTC", _utcTime.AddSeconds(1.5).ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:05:35.0000000 UTC", _utcTime.AddMinutes(1.5).ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T04:34:05.0000000 UTC", _utcTime.AddHours(1.5).ToString(_isoFormat));
			Assert.AreEqual("2017-01-03T15:04:05.0000000 UTC", _utcTime.AddDays(1.5).ToString(_isoFormat));
			Assert.AreEqual("2017-02-02T03:04:05.0000000 UTC", _utcTime.AddMonths(1).ToString(_isoFormat));
			Assert.AreEqual("2018-01-02T03:04:05.0000000 UTC", _utcTime.AddYears(1).ToString(_isoFormat));

			Assert.AreEqual(UtcDateTime.MinValue, _utcTime.Min(UtcDateTime.MinValue));
			Assert.AreEqual(_utcTime, _utcTime.Min(_utcTime));
			Assert.AreEqual(_utcTime, _utcTime.Min(UtcDateTime.MaxValue));

			Assert.AreEqual(_utcTime, _utcTime.Max(UtcDateTime.MinValue));
			Assert.AreEqual(_utcTime, _utcTime.Max(_utcTime));
			Assert.AreEqual(UtcDateTime.MaxValue, _utcTime.Max(UtcDateTime.MaxValue));

			Assert.AreEqual(UtcDateTime.MinValue, UtcDateTime.Min(_utcTime, UtcDateTime.MinValue));
			Assert.AreEqual(_utcTime, UtcDateTime.Min(_utcTime, _utcTime));
			Assert.AreEqual(_utcTime, UtcDateTime.Min(_utcTime, UtcDateTime.MaxValue));
			Assert.AreEqual(_utcTime, UtcDateTime.Min(null, _utcTime));
			Assert.AreEqual(_utcTime, UtcDateTime.Min(_utcTime, null));
			Assert.AreEqual(null, UtcDateTime.Min(null, null));

			Assert.AreEqual(_utcTime, UtcDateTime.Max(_utcTime, UtcDateTime.MinValue));
			Assert.AreEqual(_utcTime, UtcDateTime.Max(_utcTime, _utcTime));
			Assert.AreEqual(UtcDateTime.MaxValue, UtcDateTime.Max(_utcTime, UtcDateTime.MaxValue));
			Assert.AreEqual(_utcTime, UtcDateTime.Max(null, _utcTime));
			Assert.AreEqual(_utcTime, UtcDateTime.Max(_utcTime, null));
			Assert.AreEqual(null, UtcDateTime.Max(null, null));
		}

		[TestMethod]
		public void RoundDownWorks()
		{
			var time = _utcTime.AddSeconds(40).AddMilliseconds(55);

			Assert.AreEqual("2017-01-02T03:04:45.0550000 UTC", time.ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:04:45.0500000 UTC", time.RoundDown(TimeSpan.FromMilliseconds(10)).ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:04:45.0000000 UTC", time.RoundDown(TimeSpan.FromSeconds(1)).ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:04:44.0000000 UTC", time.RoundDown(TimeSpan.FromSeconds(2)).ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:04:40.0000000 UTC", time.RoundDown(TimeSpan.FromSeconds(10)).ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:04:30.0000000 UTC", time.RoundDown(TimeSpan.FromSeconds(30)).ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:04:00.0000000 UTC", time.RoundDown(TimeSpan.FromMinutes(1)).ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:00:00.0000000 UTC", time.RoundDown(TimeSpan.FromHours(1)).ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T00:00:00.0000000 UTC", time.RoundDown(TimeSpan.FromDays(1)).ToString(_isoFormat));

			time = _utcTime;
			Assert.AreEqual("2017-01-02T03:04:05.0000000 UTC", time.ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:04:05.0000000 UTC", time.RoundDown(TimeSpan.FromSeconds(1)).ToString(_isoFormat));
		}

		[TestMethod]
		public void RoundUpWorks()
		{
			var time = _utcTime.AddSeconds(40).AddMilliseconds(55);

			Assert.AreEqual("2017-01-02T03:04:45.0550000 UTC", time.ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:04:45.0600000 UTC", time.RoundUp(TimeSpan.FromMilliseconds(10)).ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:04:46.0000000 UTC", time.RoundUp(TimeSpan.FromSeconds(1)).ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:04:46.0000000 UTC", time.RoundUp(TimeSpan.FromSeconds(2)).ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:04:50.0000000 UTC", time.RoundUp(TimeSpan.FromSeconds(10)).ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:05:00.0000000 UTC", time.RoundUp(TimeSpan.FromSeconds(30)).ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:05:00.0000000 UTC", time.RoundUp(TimeSpan.FromMinutes(1)).ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T04:00:00.0000000 UTC", time.RoundUp(TimeSpan.FromHours(1)).ToString(_isoFormat));
			Assert.AreEqual("2017-01-03T00:00:00.0000000 UTC", time.RoundUp(TimeSpan.FromDays(1)).ToString(_isoFormat));

			time = _utcTime;
			Assert.AreEqual("2017-01-02T03:04:05.0000000 UTC", time.ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:04:05.0000000 UTC", time.RoundUp(TimeSpan.FromSeconds(1)).ToString(_isoFormat));
		}

		[TestMethod]
		public void RoundWorks()
		{
			var time = _utcTime.AddSeconds(40).AddMilliseconds(55);

			Assert.AreEqual("2017-01-02T03:04:45.0550000 UTC", time.ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:04:45.0600000 UTC", time.Round(TimeSpan.FromMilliseconds(10)).ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:04:45.0000000 UTC", time.Round(TimeSpan.FromSeconds(1)).ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:04:46.0000000 UTC", time.Round(TimeSpan.FromSeconds(2)).ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:04:50.0000000 UTC", time.Round(TimeSpan.FromSeconds(10)).ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:05:00.0000000 UTC", time.Round(TimeSpan.FromSeconds(30)).ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:05:00.0000000 UTC", time.Round(TimeSpan.FromMinutes(1)).ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:00:00.0000000 UTC", time.Round(TimeSpan.FromHours(1)).ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T00:00:00.0000000 UTC", time.Round(TimeSpan.FromDays(1)).ToString(_isoFormat));

			time = _utcTime.AddSeconds(25);
			Assert.AreEqual("2017-01-02T03:04:30.0000000 UTC", time.ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:04:30.0000000 UTC", time.Round(TimeSpan.FromSeconds(1)).ToString(_isoFormat));

			time = _utcTime.AddSeconds(25.5);
			Assert.AreEqual("2017-01-02T03:04:30.5000000 UTC", time.ToString(_isoFormat));
			Assert.AreEqual("2017-01-02T03:04:31.0000000 UTC", time.Round(TimeSpan.FromSeconds(1)).ToString(_isoFormat));
		}

		[TestMethod]
		public void IComparableWorks()
		{
			IComparable<UtcDateTime> comparable = _utcTime;

			Assert.AreEqual(0, comparable.CompareTo(_utcTime));
			Assert.AreEqual(1, comparable.CompareTo(UtcDateTime.MinValue));
			Assert.AreEqual(-1, comparable.CompareTo(UtcDateTime.MaxValue));
		}

		[TestMethod]
		public void IXmlSerializableWorks()
		{
			var stringBuilder = new StringBuilder();
			var writer = XmlWriter.Create(stringBuilder);

			writer.WriteStartElement("Wrapper");
			writer.WriteStartElement("Time");
			_utcTime.WriteXml(writer);
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.Flush();

			string xml = stringBuilder.ToString();

			Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-16\"?><Wrapper><Time>2017-01-02T03:04:05Z</Time></Wrapper>", xml);

			var reader = XmlReader.Create(new StringReader(xml));

			UtcDateTime result = new();
			reader.ReadStartElement("Wrapper");
			result.ReadXml(reader);
			reader.ReadEndElement();

			Assert.AreEqual(_utcTime, result);
		}

		[TestMethod]
		public void DataContractSerializationWorks()
		{
			var contract = new TestDataContract
			{
				DateTime = _utcTime,
				MaybeDateTime2 = UtcDateTime.Now
			};

			string serialized = contract.SerializeToString();
			Console.WriteLine(serialized);

			var contract2 = DataContractUtils.ReadFromString<TestDataContract>(serialized);

			Assert.AreEqual(contract.DateTime, contract2.DateTime);
			Assert.AreEqual(contract.MaybeDateTime1, contract2.MaybeDateTime1);
			Assert.AreEqual(contract.MaybeDateTime2, contract2.MaybeDateTime2);
		}

		[TestMethod]
		public void NextTest()
		{
		}


		[DataContract]
		public class TestDataContract
		{
			[DataMember]
			public UtcDateTime DateTime;

			[DataMember]
			public UtcDateTime? MaybeDateTime1;

			[DataMember]
			public UtcDateTime? MaybeDateTime2;
		}
	}
}
