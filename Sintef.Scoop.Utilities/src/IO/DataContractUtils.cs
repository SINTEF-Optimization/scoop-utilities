//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Helper functions for deserializing data contracts
	/// </summary>
	public class DataContractUtils
	{
		/// <summary>
		/// Reads and returns a data contract object of the type T from a file
		/// </summary>
		/// <typeparam name="T">The type of the data contract</typeparam>
		/// <param name="filename">The name of the file</param>
		/// <returns>The deserialized object</returns>
		public static T ReadFromFile<T>(string filename) where T : class
		{
			using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				return ReadFromStream<T>(stream);
			}
		}

		/// <summary>
		/// Reads and returns a data contract object of the type T from a stream
		/// </summary>
		/// <typeparam name="T">The type of the data contract</typeparam>
		/// <param name="stream">The stream</param>
		/// <returns>The deserialized object</returns>
		public static T ReadFromStream<T>(Stream stream) where T : class
		{
			System.Runtime.Serialization.DataContractSerializer serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
			return (T)serializer.ReadObject(stream);
		}

		/// <summary>
		/// Reads and returns a data contract object of the type T from a string
		/// </summary>
		/// <typeparam name="T">The type of the data contract</typeparam>
		/// <param name="str">The string</param>
		/// <returns>The deserialized object</returns>
		public static T ReadFromString<T>(string str) where T : class
		{
			System.Runtime.Serialization.DataContractSerializer serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
			XmlReader reader = XmlReader.Create(new StringReader(str));
			return (T)serializer.ReadObject(reader);
		}

		/// <summary>
		/// Returns true if two data contract objects are equal.
		/// 
		/// Compares two data contract objects by serializing each and comparing the serialization.
		/// The objects may be of different classes, but are more commonly of the same class.
		/// </summary>
		/// <param name="dataContract1">The first object to compare</param>
		/// <param name="dataContract2">The second object to compare</param>
		/// <param name="showFirstDiff">If true, writes information about the first difference found to the console</param>
		/// <param name="showAllIfDiff">If true, writes both contracts to the console if there is a difference</param>
		/// <returns></returns>
		public static bool ContractsAreEqual(object dataContract1, object dataContract2, bool showFirstDiff = false, bool showAllIfDiff = false)
		{
			// Serialize each object
			MemoryStream stream1 = new MemoryStream();
			dataContract1.SerializeToStream(stream1, false);
			stream1.Seek(0, SeekOrigin.Begin);

			MemoryStream stream2 = new MemoryStream();
			dataContract2.SerializeToStream(stream2, false);
			stream2.Seek(0, SeekOrigin.Begin);

			// Compare
			if (StreamUtilities.StreamsAreEqual(stream1, stream2))
				return true;

			if (showAllIfDiff)
			{
				dataContract1.SerializeToWriter(Console.Out);
				Console.WriteLine();
				dataContract2.SerializeToWriter(Console.Out);
			}

			if (!showFirstDiff)
				return false;

			// There is a difference and we're asked to display it

			// Reserialize in a readable format
			stream1 = new MemoryStream();
			dataContract1.SerializeToStream(stream1, true);
			stream1.Seek(0, SeekOrigin.Begin);

			stream2 = new MemoryStream();
			dataContract2.SerializeToStream(stream2, true);
			stream2.Seek(0, SeekOrigin.Begin);

			TextReader reader1 = new StreamReader(stream1, Encoding.UTF8);
			TextReader reader2 = new StreamReader(stream2, Encoding.UTF8);

			// Find the first line that differs
			int line = 0;
			while (true)
			{
				string l1 = reader1.ReadLine();
				string l2 = reader2.ReadLine();
				++line;

				if (l1 != l2)
				{
					Console.WriteLine("First difference at line " + line);
					Console.WriteLine("Contract 1: " + l1);
					Console.WriteLine("Contract 2: " + l2);
					return false;
				}
			}
		}

	}

	public static partial class Extensions
	{
		/// <summary>
		/// Converts the given data contract object to another that has a compatible XML serialization.
		/// 
		/// If the contract types are the same, this amounts to creating a copy by serialization/deserialization.
		/// </summary>
		/// <typeparam name="T">The type of the data contract to return</typeparam>
		/// <param name="dataContract">The data contract to covert</param>
		/// <returns></returns>
		public static T ConvertToContract<T>(this object dataContract) where T : class
		{
			if (dataContract == null)
				return null;

			MemoryStream myStream = new MemoryStream();
			dataContract.SerializeToStream(myStream);

			myStream.Seek(0, SeekOrigin.Begin);
			return DataContractUtils.ReadFromStream<T>(myStream);
		}

		/// <summary>
		/// Seralizes the data contract to file in XML
		/// </summary>
		/// <param name="dataContract">The object to serialize</param>
		/// <param name="filename">The file to serialize to</param>
		/// <param name="pretty">If true, formats the output for human readability</param>
		public static void SerializeToFile(this object dataContract, string filename, bool pretty = false)
		{
			using (FileStream stream = new FileStream(filename, FileMode.Create))
			{
				if (!pretty)
				{
					dataContract.SerializeToStream(stream);
				}
				else
				{
					MemoryStream myStream = new MemoryStream();
					dataContract.SerializeToStream(myStream);

					myStream.Seek(0, SeekOrigin.Begin);
					XElement root = XElement.Load(myStream);

					root.Save(stream);
				}
			}
		}

		/// <summary>
		/// Seralizes the data contract to the given stream in XML, UTF8
		/// </summary>
		/// <param name="dataContract">The object to serialize</param>
		/// <param name="stream">The stream to serialize to</param>
		/// <param name="pretty">If true, formats the output for human readability</param>
		public static void SerializeToStream(this object dataContract, Stream stream, bool pretty = false)
		{
			if (!IsDataContract(dataContract))
				throw new ArgumentException("Object to serialize must be a data contract");

			if (!pretty)
			{
				XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8);
				DataContractSerializer serializer = new DataContractSerializer(dataContract.GetType());
				serializer.WriteObject(writer, dataContract);
				writer.Flush();
			}
			else
			{
				MemoryStream myStream = new MemoryStream();
				SerializeToStream(dataContract, myStream, false);

				myStream.Seek(0, SeekOrigin.Begin);
				XElement root = XElement.Load(myStream);

				root.Save(stream);
			}
		}

		/// <summary>
		/// Seralizes the data contract to the given textwriter, in a human readable format
		/// </summary>
		/// <param name="dataContract">The object to serialize</param>
		/// <param name="writer">The writer to serialize to</param>
		public static void SerializeToWriter(this object dataContract, TextWriter writer)
		{
			if (!IsDataContract(dataContract))
				throw new ArgumentException("Object to serialize must be a data contract");

			MemoryStream myStream = new MemoryStream();
			SerializeToStream(dataContract, myStream, false);

			myStream.Seek(0, SeekOrigin.Begin);
			XElement root = XElement.Load(myStream);

			root.Save(writer);
		}

		/// <summary>
		/// Seralizes the data contract as a string, in a human readable format
		/// </summary>
		/// <param name="dataContract">The object to serialize</param>
		/// <returns>The string containing the serialized object</returns>
		public static string SerializeToString(this object dataContract)
		{
			StringWriter writer = new StringWriter();

			SerializeToWriter(dataContract, writer);

			writer.Close();

			return writer.ToString();
		}

		/// <summary>
		/// Returns true if the given object has a [DataContract] attribute
		/// </summary>
		public static bool IsDataContract(this object dataContract)
		{
			return System.Attribute.GetCustomAttributes(dataContract.GetType()).Any(attr => attr is System.Runtime.Serialization.DataContractAttribute);
		}

	}
}
