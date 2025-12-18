//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sintef.Scoop.Utilities;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Sintef.Scoop.Utilities.Tests;

[TestClass]
public class NpyParserTests
{
	private static readonly string _projectFolder =
		DirectoryFinder.FindDirectoryAboveCurrent(null, new[] { "Data", "FiniteStateMachineTestImpl", "SpatialGraphTests" });

	private static readonly string _dataFolder = Path.Combine(_projectFolder, "Data");

	// These filename follow this standard:
	// No numbers other that for the shape definition
	// The shape definition are the dimensions separated by the char 'x'
	// The shape definition must be immediately followed by the char 'C' for c order or 'F' for fortran order
	// If the file is big endian it must contain the substring 'bigE', whereas little endian files are not allowed to contain that substring
	private readonly string _17CLittle = Path.Combine(_dataFolder, "test-17C.npy");
	private readonly string _17CBig = Path.Combine(_dataFolder, "test-17C-bigE.npy");

	private readonly string _10X20CLittle = Path.Combine(_dataFolder, "test-10x20C.npy");
	private readonly string _10X20FLittle = Path.Combine(_dataFolder, "test-10x20F.npy");

	private readonly string _5X7X3CLittle = Path.Combine(_dataFolder, "test-5x7x3C.npy");
	private readonly string _5X7X3CBig = Path.Combine(_dataFolder, "test-5x7x3C-bigE.npy");
	private readonly string _5X7X3FLittle = Path.Combine(_dataFolder, "test-5x7x3F.npy");
	private readonly string _5X7X3FBig = Path.Combine(_dataFolder, "test-5x7x3F-bigE.npy");

	private readonly string _b3X7C = Path.Combine(_dataFolder, "test-bool-3x7C.npy");
	private readonly string _by3X7C = Path.Combine(_dataFolder, "test-byte-3x7C.npy");
	private readonly string _ushort3X7C = Path.Combine(_dataFolder, "test-ushort-3x7C.npy");
	
	// These files are for testing too big array sizes and do not follow the naming convention of those above
	// They are also cut down in size, containing little more than the header.
	private readonly string _broken = Path.Combine(_dataFolder, "test-broken.npy");
	private readonly string _broken2 = Path.Combine(_dataFolder, "test-broken2.npy");

	// Invalid file
	private readonly string _broken3 = Path.Combine(_dataFolder, "test-broken3.npy");

	// Sample zip file
	private readonly string _10X20CLittleZip = Path.Combine(_dataFolder, "test-10x20C.zip");
	
	[TestMethod]
	public void HeadersAreParsedCorrectly()
	{
		AssertNpyHeader<float>(_17CLittle, "17CL");
		AssertNpyHeader<float>(_17CBig, "17CB");
		AssertNpyHeader<double>(_10X20CLittle, "10x20CL");
		AssertNpyHeader<double>(_10X20FLittle, "10x20FL");
		AssertNpyHeader<double>(_5X7X3CLittle, "5x7x3CL");
		AssertNpyHeader<double>(_5X7X3CBig, "5x7x3CB");
		AssertNpyHeader<double>(_5X7X3FLittle, "5x7x3FL");
		AssertNpyHeader<double>(_5X7X3FBig, "5x7x3FB");
		AssertNpyHeader<bool>(_b3X7C, "3x7CN");
		AssertNpyHeader<byte>(_by3X7C, "3x7CN");
		AssertNpyHeader<ushort>(_ushort3X7C, "3x7CL");
		AssertNpyHeader<byte>(_broken, "2200000000CN");
		AssertNpyHeader<byte>(_broken2, "2x2200000000CN");
	}

	[TestMethod]
	public void ParserDoesNotAllowTooBigTables()
	{
		foreach (var file in new[] { _broken, _broken2 })
		{
			using var parser = NpyParser.OpenNpyFile(file);

			try
			{
				_ = parser.ParseTable<byte[]>();
				Assert.Fail();
			}
			catch (InvalidOperationException e)
			{
				Assert.AreEqual("Table has an dimension with too many elements to be fit into a .NET array", e.Message);
			}
		}
	}

	[TestMethod]
	public void TryingToParseIntoArraysWithWrongDimensionsThrowsProperException()
	{
		using var parser = NpyParser.OpenNpyFile(_by3X7C);

		Action[] actions =
		[
			// ReSharper disable once AccessToDisposedClosure
			() => _ = parser.ParseTable<byte[]>(),
			// ReSharper disable once AccessToDisposedClosure
			() => _ = parser.ParseTable<byte[][][]>()
		];

		foreach (var action in actions)
		{
			try
			{
				action();
				Assert.Fail();
			}
			catch (ArgumentException e)
			{
				Assert.AreEqual("The given type does not have the correct number of dimensions to store the table", e.Message);
			}
		}
	}

	[TestMethod]
	public void OpeningNonNpyFileThrowsProperException()
	{
		try
		{
			using var parser = NpyParser.OpenNpyFile(_broken3);
			Assert.Fail();
		}
		catch (ArgumentException e)
		{
			Assert.AreEqual("Given file is not a valid numpy file", e.Message);
		}
	}

	[TestMethod]
	public void TryingToParseIntoWrongTypeThrowsProperException()
	{
		using var parser = NpyParser.OpenNpyFile(_by3X7C);

		try
		{
			_ = parser.ParseTable<short[][]>();
			Assert.Fail();
		}
		catch (ArgumentException e)
		{
			Assert.AreEqual("The given element type (System.Int16) does not match the contents of parsed table (System.Byte)", e.Message);
		}
	}

	[TestMethod]
	public void TryingToParseAfterClosingThrowsProperException()
	{
		using var parser = NpyParser.OpenNpyFile(_by3X7C);

		parser.Close();
		
		try
		{
			_ = parser.ParseTable<short[][]>();
			Assert.Fail();
		}
		catch (InvalidOperationException e)
		{
			Assert.AreEqual("Can't parse the table after the parser has been closed!", e.Message);
		}
	}

	[TestMethod]
	public void CanNotParseIntoMultipleArrays()
	{
		using var parser = NpyParser.OpenNpyFile(_b3X7C);

		var result1 = parser.ParseTable<bool[][]>();
		try
		{
			var result2 = parser.ParseTable<bool[][]>();
			Assert.Fail();
		}
		catch (InvalidOperationException e)
		{
			Assert.AreEqual("Stream does not contain enough data", e.Message);
		}
	}

	[TestMethod]
	public void CanParseStreamFromZipStream()
	{
		using var zip = ZipFile.Open(_10X20CLittleZip, ZipArchiveMode.Read);

		var stream = zip.Entries.Single().Open();

		using var parser = NpyParser.OpenNpyFile(stream);
		
		Assert.IsTrue(parser.IsLittleEndian);
		Assert.IsFalse(parser.HasFortranOrder);
		
		Assert.AreEqual(parser.Shape.Length, 2);
		Assert.AreEqual(10UL, parser.Shape[0]);
		Assert.AreEqual(20UL, parser.Shape[1]);
	}
	
	[TestMethod]
	public void BoolIsParsedCorrectly()
	{
		using var parser = NpyParser.OpenNpyFile(_b3X7C);

		var result = parser.ParseTable<bool[][]>();
		Assert.IsNotNull(result);
		Assert.AreEqual(2, parser.Shape.Length);
		Assert.AreEqual(3, result.Length);
		for (int i = 0; i < 3; ++i)
		{
			var row = result[i];
			Assert.AreEqual(7, row.Length);
			for (int j = 0; j < 7; ++j)
			{
				Assert.AreEqual(((i + j) & 1) != 0, row[j]);
			}
		}
	}
	
	[TestMethod]
	public void ByteIsParsedCorrectly()
	{
		using var parser = NpyParser.OpenNpyFile(_by3X7C);

		var result = parser.ParseTable<byte[][]>();
		Assert.IsNotNull(result);
		Assert.AreEqual(2, parser.Shape.Length);
		Assert.AreEqual(3, result.Length);
		for (int i = 0; i < 3; ++i)
		{
			var row = result[i];
			Assert.AreEqual(7, row.Length);
			for (int j = 0; j < 7; ++j)
			{
				Assert.AreEqual((byte)i + 10 * j, row[j]);
			}
		}
	}

	[TestMethod]
	public void UShortIsParsedCorrectly()
	{
		using var parser = NpyParser.OpenNpyFile(_ushort3X7C);

		var result = parser.ParseTable<ushort[][]>();
		Assert.IsNotNull(result);
		Assert.AreEqual(2, parser.Shape.Length);
		Assert.AreEqual(3, result.Length);
		for (int i = 0; i < 3; ++i)
		{
			var row = result[i];
			Assert.AreEqual(7, row.Length);
			for (int j = 0; j < 7; ++j)
			{
				Assert.AreEqual((ushort)i + 10 * j, row[j]);
			}
		}
	}

	[TestMethod]
	public void DataIsParsedCorrectly1D()
	{
		string[] files = [_17CLittle, _17CBig];
		
		foreach (var file in files)
		{
			using var parser = NpyParser.OpenNpyFile(file);
			var result = parser.ParseTable<float[]>();
			Assert.IsNotNull(result);
			Assert.AreEqual(1, parser.Shape.Length);
			Assert.AreEqual(17, result.Length);
			for (int i = 0; i < result.Length; ++i)
			{
				Assert.AreEqual(i, result[i]);
			}
		}
	}
	
	[TestMethod]
	public void DataIsParsedCorrectly2D()
	{
		string[] files = [_10X20CLittle, _10X20FLittle];
		
		foreach (var file in files)
		{
			using var parser = NpyParser.OpenNpyFile(file);
			var result = parser.ParseTable<double[][]>();
			Assert.IsNotNull(result);
			Assert.AreEqual(2, parser.Shape.Length);
			Assert.AreEqual(10, result.Length);
			for (int i = 0; i < result.Length; ++i)
			{
				var secondArray = result[i];
				Assert.AreEqual(20, secondArray.Length);
				for (int j = 0; j < secondArray.Length; ++j)
				{
					Assert.AreEqual((double) i * 100 + j, result[i][j]);
				}
			}
		}
	}
	
	[TestMethod]
	public void DataIsParsedCorrectly3D()
	{
		string[] files = [_5X7X3CLittle, _5X7X3CBig, _5X7X3FLittle, _5X7X3FBig];
		
		foreach (var file in files)
		{
			using var parser = NpyParser.OpenNpyFile(file);
			var result = parser.ParseTable<double[][][]>();
			Assert.IsNotNull(result);
			Assert.AreEqual(3, parser.Shape.Length);
			Assert.AreEqual(5, result.Length);
			for (int i = 0; i < 5; ++i)
			{
				Assert.AreEqual(7, result[i].Length);
				for (int j = 0; j < 7; ++j)
				{
					Assert.AreEqual(3, result[i][j].Length);
					for (int k = 0; k < 3; ++k)
					{
						Assert.AreEqual((double)i * 100 + j + 10000 * k, result[i][j][k]);
					}
				}
			}
		}
	}

	[TestMethod]
	public void HeaderKvpArrayIsSplitProperly()
	{

		var result = NpyParser.SplitKeyValuePairs("_ ,, ,  a  ,-,(,,,,,,),").ToArray();
		
		Assert.AreEqual(4, result.Length);
		Assert.AreEqual("_", result[0]);
		Assert.AreEqual("a", result[1]);
		Assert.AreEqual("-", result[2]);
		Assert.AreEqual("(,,,,,,)", result[3]);

		result = NpyParser.SplitKeyValuePairs("a").ToArray();
		
		Assert.AreEqual(1, result.Length);
		Assert.AreEqual("a", result[0]);

		result = NpyParser.SplitKeyValuePairs("a,b,c").ToArray();
		
		Assert.AreEqual(3, result.Length);
		Assert.AreEqual("a", result[0]);
		Assert.AreEqual("b", result[1]);
		Assert.AreEqual("c", result[2]);
	}
	
	/// <summary>
	/// Tests that the Npy header is parsed correctly. The layout is parsed to a format which consists of the shape dimensions separated by 'x', directly
	/// followed by 'C' for c order or 'F' for fortran order. Then another character for endianness, 'L' for little, 'B' for big or 'N' for none.
	///
	/// The given layout is then tested against the given string to check if it is identical. 
	/// </summary>
	/// <param name="filename">Full path of the file to test.</param>
	/// <param name="expectedLayout">A string describing the expected layout of the file</param>
	/// <typeparam name="T">The data type expected from the given file, this is not inferred from the filename.</typeparam>
	private void AssertNpyHeader<T>(string filename, string expectedLayout)
	{
		using var parser = NpyParser.OpenNpyFile(filename);

		Assert.AreEqual(typeof(T), parser.DataType);
		
		string layout = parser.Shape.Select(x => x.ToString()).JoinStrings("x") + (parser.HasFortranOrder ? 'F' : 'C') +
		                (parser.IsLittleEndian.HasValue ? (parser.IsLittleEndian.Value ? 'L' : 'B') : 'N');
		
		Assert.AreEqual(expectedLayout, layout);
	}

}