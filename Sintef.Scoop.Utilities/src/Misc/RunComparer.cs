//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.IO;
using System.Xml;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// This class helps in comparing two runs of a program to discover at what point they
	/// start to differ. It is intended to be useful for tracking down the root cause of
	/// changes in test results.
	/// 
	/// The main method of the class is Check(), which should be called at any point where you want to
	/// see if something has diverged. This method either writes the value of a 
	/// supplied object to a reference file, or it compares the value of the object to the
	/// value written to the reference file earlier. Call CreateReference() at the start of the
	/// program to build the reference file. Then run again calling CompareWithReference()
	/// instead to do the comparison and get an exception at the first difference.
	/// 
	/// Objects are compared by ToString() values, except doubles, which are compared numerically
	/// using a tolerance.
	/// 
	/// It is also possible to write two runs to difference reference files and compare the files
	/// which are written in xml.
	/// </summary>
	public static class RunComparer
	{
		/// <summary>
		/// The writer to the reference file, or null
		/// </summary>
		static XmlWriter _writer;

		/// <summary>
		/// The reader from the reference file, or null
		/// </summary>
		static XmlReader _reader;

		/// <summary>
		/// Counts how many values have been written to file / checked
		/// </summary>
		static int _counter = 0;

		/// <summary>
		/// The tolerance to use when comparing doubles. The default value ie 1e-12.
		/// </summary>
		public static double DoubleTolerance { get; set; }

		/// <summary>
		/// Initializes the static properties
		/// </summary>
		static RunComparer() { 
			DoubleTolerance = 1e-12; 
		}

		/// <summary>
		/// Opens a reference file with the given name for writing. Each subsequent call to 
		/// Check() will write a record to the file
		/// </summary>
		public static void CreateReference(string filename)
		{
			_writer = XmlWriter.Create(filename, new XmlWriterSettings { Indent = true });
			_writer.WriteStartElement("CompareLog");
		}

		/// <summary>
		/// Opens an existing reference file for comparison. Each subsequence call to Check()
		/// will compare the supplied value with the value in the reference file
		/// </summary>
		public static void CompareWithReference(string filename)
		{
			_reader = XmlReader.Create(new FileStream(filename, FileMode.Open), new XmlReaderSettings { IgnoreWhitespace = true });
			_reader.ReadStartElement("CompareLog");
		}

		/// <summary>
		/// Closes the reference file if open.
		/// </summary>
		public static void Close()
		{
			if (_reader != null)
			{
				_reader.Close();
				_reader = null;
			}

			if (_writer != null)
			{
				_writer.Close();
				_writer = null;
			}
		}

		/// <summary>
		/// Writes a record of the given value to the reference file, or compares it against
		/// the record on the reference file.
		/// </summary>
		/// <param name="value">The value to record/check</param>
		/// <param name="tag">A tag identifying what is being checked</param>
		public static void Check(object value, string tag = "")
		{
			if (_counter == 1402)
			{
				// Here is an opportunity for setting a breakpoint
				++_counter;
				--_counter;
			}

			if (_writer != null)
			{
				// Write counter, object value and tag to file

				_writer.WriteStartElement("Entry");
				_writer.WriteElementString("Counter", _counter.ToString());
				if (tag != "")
					_writer.WriteElementString("Tag", tag);
				_writer.WriteElementString("Value", value == null ? "null" : value.ToString());
				_writer.WriteEndElement();
				_writer.Flush();

				++_counter;

			}
			else if (_reader != null)
			{
				// Read from file and compare

				_reader.ReadStartElement("Entry");

				// Check that counter matches
				int c = _reader.ReadElementContentAsInt("Counter", "");
				if (c != _counter)
					throw new Exception("Counter mismatch, " + _counter);

				if (tag != "")
				{
					// Check that tag matches
					string t = _reader.ReadElementContentAsString("Tag", "");
					if (t != tag)
						throw new Exception(String.Format("Tag mismatch at counter {0}, {1} != {2}", _counter, t, tag));
				}

				// Read correct object value
				string referenceString = _reader.ReadElementContentAsString("Value", "");

				if (value.GetType() == typeof(double))
				{
					// For doubles, compare with tolerance
					double doubleValue = (double)value;
					double doubleReferenceValue = double.Parse(referenceString);

					if (! doubleReferenceValue.EqualsWithTolerance(doubleValue, DoubleTolerance))
						throw new Exception(String.Format("Double mismatch at counter {0}, {1} != {2}. Tag = {3}", _counter, doubleReferenceValue, doubleValue, tag));
				}
				else
				{
					// Everything else, compare by ToString()
					string stringValue = value == null ? "null" : value.ToString();
					if (referenceString != stringValue)
						throw new Exception(String.Format("Value mismatch at counter {0}, {1} != {2}. Tag = {3}", _counter, referenceString, stringValue,tag));
				}

				_reader.ReadEndElement();

				++_counter;
			}
		}
	}
}
