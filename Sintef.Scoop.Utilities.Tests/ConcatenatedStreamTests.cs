//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sintef.Scoop.Utilities.Tests
{
	[TestClass]
	public class ConcatenatedStreamTests
	{
		private List<Stream> _streams;
		private byte[] _zeroByteArray;
		private byte[] _onesByteArray;
		private ConcatenatedStream _concatenatedStream;

		[TestInitialize]
		public void Setup()
		{
			_streams = new List<Stream>();

			_zeroByteArray = new byte[] { 0, 0, 0, 0 };
			var streamZeros = new MemoryStream(_zeroByteArray);
			_onesByteArray = new byte[] { 1, 1, 1, 1 };
			var streamOnes = new MemoryStream(_onesByteArray);
			_streams.Add(streamZeros);
			_streams.Add(streamOnes);

			_concatenatedStream = new ConcatenatedStream(_streams);
		}

		[TestMethod,
		 TestCategory("UnitTest")]
		public void Read_ReadConcatenatedStreams_ArrayOfAllInputStreams()
		{
			// Arrange
			var buffer = new byte[8];

			// Act
			var bytesRead = _concatenatedStream.Read(buffer, 0, 8);

			// Assert
			Assert.AreEqual(8, bytesRead, "Bytes read should be match the combined to streams.");
			CollectionAssert.AreEqual(_zeroByteArray, buffer.Take(4).ToArray());
			CollectionAssert.AreEqual(_onesByteArray, buffer.Skip(4).Take(4).ToArray());
		}

		[TestMethod,
		 TestCategory("UnitTest")]
		public void Read_MultipleReadCalls_MaintainStateOfStreams()
		{
			// Arrange
			var buffer = new byte[8];

			// Act - Read the zero stream and one byte the one stream.
			var bytesRead = _concatenatedStream.Read(buffer, 0, 5);

			// Assert
			Assert.AreEqual(5, bytesRead);
			CollectionAssert.AreEqual(_zeroByteArray, buffer.Take(4).ToArray());
			Assert.AreEqual(1, buffer[4]);

			// Act - Read the remaining one stream.
			bytesRead = _concatenatedStream.Read(buffer, 5, 3);

			// Assert
			Assert.AreEqual(3, bytesRead);
			CollectionAssert.AreEqual(_onesByteArray, buffer.Skip(4).Take(4).ToArray());
		}

		[TestMethod,
		 TestCategory("UnitTest")]
		public void Read_ReadMoreThanStreamSize_EntireStreamContent()
		{
			// Arrange
			var buffer = new byte[10];

			// Act
			var bytesRead = _concatenatedStream.Read(buffer, 0, 10);

			// Assert
			Assert.AreEqual(8, bytesRead);
		}

		[TestMethod,
		 TestCategory("UnitTest")]
		public void GetLength_SumOfInputStreamsLength()
		{
			// Act
			var concatenatedLength = _concatenatedStream.Length;

			// Assert
			Assert.AreEqual(8, concatenatedLength);
		}

		[TestMethod,
		 TestCategory("UnitTest")]
		public void GetPosition_PositionInConcatenatedStreams()
		{
			// Arrange
			var _ = _concatenatedStream.Read(new byte[10], 0, 5);

			// Act
			var position = _concatenatedStream.Position;

			// Assert
			Assert.AreEqual(5, position);
		}

		[TestMethod,
		 TestCategory("UnitTest")]
		public void SetPosition_PositionSetInConcatenatedStreams()
		{
			// Act
			_concatenatedStream.Position = 5;
			var position = _concatenatedStream.Position;

			// Assert
			Assert.AreEqual(5, position);
		}

		[TestMethod,
		 TestCategory("UnitTest")]
		public void SetPosition_ReadFromSetPosition_ByteAtGivenPosition()
		{
			// Arrange
			var buffer = new byte[1];

			// Act
			_concatenatedStream.Position = 5;
			var bytesRead = _concatenatedStream.Read(buffer, 0, 1);

			// Assert
			Assert.AreEqual(1, bytesRead);
			Assert.AreEqual(1, buffer[0]);
		}


		[TestMethod,
		 TestCategory("UnitTest"),
		ExpectedException(typeof(ArgumentException))]
		public void Constructor_InputStreamNotSupportingRead_ThrowArgumentException()
		{
			// Arrange
			var moq = new Mock<Stream>();
			moq.SetupGet(s => s.CanRead).Returns(false);

			// Act
			var _ = new ConcatenatedStream(new[] { moq.Object });
		}

		[TestMethod,
		 TestCategory("UnitTest"),
		 ExpectedException(typeof(NotSupportedException))]
		public void Constructor_InputStreamNotSupportLength_ConcatStreamNotSupportLength()
		{
			// Arrange
			var moq = new Mock<Stream>();
			moq.Setup(s => s.Length).Throws<NotSupportedException>();
			moq.SetupGet(s => s.CanRead).Returns(true);
			moq.SetupGet(s => s.CanSeek).Returns(true);
			var stream = new ConcatenatedStream(new[] { moq.Object });

			// Act
			var _ = stream.Length;
		}

		[TestMethod,
		 TestCategory("UnitTest")]
		public void Constructor_InputStreamSupportLength_ConcatStreamSupportLength()
		{
			// Arrange
			var moq = new Mock<Stream>();
			moq.SetupGet(s => s.Length).Returns(0);
			moq.SetupGet(s => s.CanRead).Returns(true);
			moq.SetupGet(s => s.CanSeek).Returns(true);
			var stream = new ConcatenatedStream(new[] { moq.Object });

			// Act
			var length = stream.Length;

			// Assert
			Assert.AreEqual(0, length, "Concat stream should match the sum of the input streams.");
		}

		[TestMethod,
		 TestCategory("UnitTest"),
		 ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void SetPositionOutsideRange_ThrowArgumentOutOfRangeException()
		{
			// Act
			_concatenatedStream.Position = 10;
		}


	}
}
