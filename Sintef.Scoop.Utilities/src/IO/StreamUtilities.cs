//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/


using System;
using System.Collections.Generic;
using System.IO;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	///  A collection of stream handling and IO utilities.
	/// </summary>
	public static class StreamUtilities
	{
		/// <summary>
		/// Compares the contents of two streams.
		/// 
		/// The streams should be positioned at the start when calling this function.
		/// On return, the streams are positioned at the end.
		/// </summary>
		/// <param name="stream1">The first stream</param>
		/// <param name="stream2">The second stream</param>
		/// <returns>True if the contents of the streams are equal, false if not</returns>
		public static bool StreamsAreEqual(Stream stream1, Stream stream2)
		{
			try
			{
				// Check the stream sizes. If they are not the same, the streams 
				// are not the same.
				if (stream1.Length != stream2.Length)
				{
					// Return false to indicate files are different
					return false;
				}
			}
			catch (NotSupportedException)
			{
				// Cannot do the length test; must compare byte by byte
			}

			// Read and compare a byte from each stream until either a
			// non-matching set of bytes is found or until the end of
			// stream1 is reached.
			int stream1Byte;
			int stream2Byte;
			do
			{
				// Read one byte from each stream.
				stream1Byte = stream1.ReadByte();
				stream2Byte = stream2.ReadByte();
			}
			while ((stream1Byte == stream2Byte) && (stream1Byte != -1));

			// Return the success of the comparison. "stream1byte" is 
			// equal to "stream2byte" at this point only if the files are 
			// the same.
			return ((stream1Byte - stream2Byte) == 0);
		}

		/// <summary>
		/// Concatenates enumerable of streams.
		/// </summary>
		/// <param name="streams">Streams to concatenate.</param>
		/// <returns>Concatenated stream.</returns>
		public static Stream Concat(this IEnumerable<Stream> streams)
		{
			return new ConcatenatedStream(streams);
		}
	}
}