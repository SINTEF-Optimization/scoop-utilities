//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace Sintef.Scoop.Utilities.Tests
{
	[TestClass]
	public class TestGeneralLoggerParser
	{
		[TestMethod]
		public void TestWriteAndParse()
		{
			string filename = "___tester___blub__testMethods_test_general_logger";
			string[] pname = new string[] { "Prop:A", "Prop=[B]" };
			string[] msg = new string[] { "m[a]", "m:b", "[m=c]" };
			string[] paval = new string[] { null, "a2", "a=3" };
			string[] pbval = new string[] { "b:1", "b:2]", null };

			Stream stream = new FileStream(filename, FileMode.Create);
			GeneralLogger logger = new GeneralLogger();
			logger.LogTargetTextWriter = new StreamWriter(stream);
			logger.LogLevel = int.MaxValue;
			logger.WriteToLogTarget = true;
			for (int i = 0; i < pname.Length; ++i)
				logger.AddOrGetProperty(pname[i], true);

			logger.Log(1, msg[0], new string[,] { { pname[1], pbval[0] } });
			logger.Log(1, msg[1], new string[,] { { pname[0], paval[1] }, { pname[1], pbval[1] } });
			logger.Log(1, msg[2], new string[,] { { pname[0], "" }, { pname[0], paval[2] }, { pname[1], pbval[2] } });

			logger.LogTargetTextWriter.Flush();
			stream.Flush();
			stream.Close();

			List<GeneralLoggerParser.LogEntry> entries = GeneralLoggerParser.Parse(filename);
			Assert.AreEqual(3, entries.Count);
			for (int i = 0; i < entries.Count; ++i)
			{
				Assert.AreEqual(msg[i], entries[i].Message);
			}

			Assert.AreEqual(1, entries[0].Properties.Keys.Count());
			Assert.IsTrue(entries[0].Properties.Keys.Contains(pname[1]));
			Assert.AreEqual(1, entries[0].Properties[pname[1]].Count);
			Assert.IsTrue(entries[0].Properties[pname[1]].Contains(pbval[0]));

			Assert.AreEqual(2, entries[1].Properties.Keys.Count());
			Assert.IsTrue(entries[1].Properties.Keys.Contains(pname[0]));
			Assert.IsTrue(entries[1].Properties.Keys.Contains(pname[1]));
			Assert.AreEqual(1, entries[1].Properties[pname[0]].Count);
			Assert.IsTrue(entries[1].Properties[pname[0]].Contains(paval[1]));
			Assert.AreEqual(1, entries[1].Properties[pname[1]].Count);
			Assert.IsTrue(entries[1].Properties[pname[1]].Contains(pbval[1]));

			Assert.AreEqual(2, entries[2].Properties.Keys.Count());
			Assert.IsTrue(entries[2].Properties.Keys.Contains(pname[0]));
			Assert.IsTrue(entries[2].Properties.Keys.Contains(pname[1]));
			Assert.AreEqual(2, entries[2].Properties[pname[0]].Count);
			Assert.IsTrue(entries[2].Properties[pname[0]].Contains(paval[2]));
			Assert.IsTrue(entries[2].Properties[pname[0]].Contains(null));
			Assert.AreEqual(1, entries[2].Properties[pname[1]].Count);
			Assert.IsTrue(entries[2].Properties[pname[1]].Contains(pbval[2]));
		}

		[TestMethod]
		public void TestParse1()
		{
			string data = "2017-08-30 13:42:17.3449304 garb-]age [P=b] stuff : msg\n";

			List<GeneralLoggerParser.LogEntry> entries = GeneralLoggerParser.Parse(GenerateStreamFromString(data));
			Assert.AreEqual(1, entries.Count);
			Assert.AreEqual(DateTime.Parse("2017-08-30 13:42:17.3449304"), entries[0].Timestamp);
			Assert.AreEqual("msg", entries[0].Message);
			Assert.AreEqual(1, entries[0].Properties.Keys.Count());
			Assert.IsTrue(entries[0].Properties.Keys.Contains("P"));
			Assert.AreEqual(1, entries[0].Properties["P"].Count);
			Assert.IsTrue(entries[0].Properties["P"].Contains("b"));
		}

		[TestMethod]
		public void TestParse2()
		{
			string data = "2017-08-30 13:42:17.3449304 garb-]age [P=b] stuff msg\n";

			List<GeneralLoggerParser.LogEntry> entries = GeneralLoggerParser.Parse(GenerateStreamFromString(data));
			Assert.AreEqual(1, entries.Count);
			Assert.AreEqual(DateTime.Parse("2017-08-30 13:42:17.3449304"), entries[0].Timestamp);
			Assert.AreEqual("stuff msg", entries[0].Message);
			Assert.AreEqual(1, entries[0].Properties.Keys.Count());
			Assert.IsTrue(entries[0].Properties.Keys.Contains("P"));
			Assert.AreEqual(1, entries[0].Properties["P"].Count);
			Assert.IsTrue(entries[0].Properties["P"].Contains("b"));
		}

		[TestMethod]
		public void TestParse2_1()
		{
			string data = "2017-08-30 13:42:17.3449304 garb\\[-]age [P\\=b=c=d] \"\\\"[stuff] msg\"\n";

			List<GeneralLoggerParser.LogEntry> entries = GeneralLoggerParser.Parse(GenerateStreamFromString(data));
			Assert.AreEqual(1, entries.Count);
			Assert.AreEqual(DateTime.Parse("2017-08-30 13:42:17.3449304"), entries[0].Timestamp);
			Assert.AreEqual("\"\\\"[stuff] msg\"", entries[0].Message);
			Assert.AreEqual(1, entries[0].Properties.Keys.Count());
			Assert.IsTrue(entries[0].Properties.Keys.Contains("P=b"));
			Assert.AreEqual(1, entries[0].Properties["P=b"].Count);
			Assert.IsTrue(entries[0].Properties["P=b"].Contains("c=d"));
		}

		[TestMethod]
		public void TestParse3()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("nb-NO");

			string data = "  2017-08-30  13:42:17.3449304 : garb-age [P=b] stuff msg\n";

			List<GeneralLoggerParser.LogEntry> entries = GeneralLoggerParser.Parse(GenerateStreamFromString(data));
			Assert.AreEqual(1, entries.Count);
			Assert.AreEqual(DateTime.Parse("2017-08-30 13:42:17.3449304"), entries[0].Timestamp);
			Assert.AreEqual("garb-age [P=b] stuff msg", entries[0].Message);
			Assert.AreEqual(0, entries[0].Properties.Keys.Count());
		}

		[TestMethod]
		public void TestParse4()
		{
			// Dotnet currently accepts an a character in front of a datetime so this tests fails, switch to using b instead
			//string data = "a2017-08-30 13:42:17.3449304 : garb-age [P=b] stuff msg\n";
			string data = "b2017-08-30 13:42:17.3449304 : garb-age [P=b] stuff msg\n";

			List<GeneralLoggerParser.LogEntry> entries = GeneralLoggerParser.Parse(GenerateStreamFromString(data));
			Assert.AreEqual(0, entries.Count);
		}

		private static Stream GenerateStreamFromString(string s)
		{
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(s);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}
	}
}