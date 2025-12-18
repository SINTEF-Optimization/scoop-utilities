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
	/// Concatenates multiple streams such that they function as one.
	/// Each stream is disposed after it is read.
	///
	/// Currently only reading functionality is implemented. It is possible to
	/// extend this implementation to support operations such as write and seek.
	/// </summary>
	public class ConcatenatedStream : Stream
	{
		private readonly IEnumerator<Stream> _streamEnumerator;
		private long _lengthReadStreams = 0;
		private readonly long _length;
		private readonly bool _hasLength = true;

		/// <summary>
		/// Initializes a new instance of the <see cref="ConcatenatedStream"/> class.
		/// All the <paramref name="streams"/> must support canRead and canSeek.
		/// </summary>
		/// <param name="streams">Streams to concatenate.</param>
		public ConcatenatedStream(IEnumerable<Stream> streams)
		{
			_streamEnumerator = streams?.GetEnumerator() ?? throw new ArgumentNullException(nameof(streams));

			_length = 0;
			foreach (var stream in streams)
			{
				if (!stream.CanRead)
				{
					throw new ArgumentException(nameof(ConcatenatedStream) + " requires that all streams can be read.");
				}

				if (!_hasLength)
				{
					continue;
				}

				try
				{
					_length += stream.Length;
				}
				catch (NotSupportedException)
				{
					_hasLength = false;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether the concatenated stream can be read.
		/// Should always return true.
		/// </summary>
		public override bool CanRead => true;

		/// <summary>
		/// Gets a value indicating whether the concatenated streams can seek.
		/// This is not supported in the current implemenation.
		/// However, <see cref="Length"/> and <see cref="Position"/> are supported.
		/// </summary>
		public override bool CanSeek => false;

		/// <summary>
		/// Gets a value indicating whether the concatenated streams can write.
		/// This is not supported in the current implemenattion.
		/// </summary>
		public override bool CanWrite => false;

		/// <summary>
		/// Gets the sum of the length of the concatenated streams.
		///
		/// If one of the stream do not support Length,
		/// this will neither.
		/// </summary>
		public override long Length
		{
			get
			{
				if (_hasLength)
				{
					return _length;
				}
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Gets or sets or sets the current position in the concatenated streams.
		/// </summary>
		public override long Position
		{
			get => _position;
			set
			{
				if (value < _position || value >= Length)
				{
					throw new ArgumentOutOfRangeException();
				}

				while (_position != value)
				{
					if (_streamEnumerator.Current != null &&
						value < _streamEnumerator.Current.Length + _lengthReadStreams)
					{
						_streamEnumerator.Current.Position = value - _lengthReadStreams;
						break;
					}

					MovedNext();
				}
			}
		}

		/// <summary>
		/// Help property to get the current position.
		/// </summary>
		private long _position => _lengthReadStreams + (_streamEnumerator.Current?.Position ?? 0);

		/// <summary>
		/// Is not supported.
		/// </summary>
		public override void Flush()
		{
			throw new NotSupportedException("Flush is not supported for concatenaded streams.");
		}

		/// <summary>
		/// Is not supported.
		/// </summary>
		/// <param name="offset">A byte offset relative to the origin parameter.</param>
		/// <param name="origin">A value of type System.IO.SeekOrigin indicating
		/// the reference point used to obtain the new position.</param>
		/// <returns>The new position within the current stream.</returns>
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException("Seek is not supported for concatenaded streams.");
		}

		/// <summary>
		/// Is not supported.
		/// </summary>
		/// <param name="value">The desired length of the current stream in bytes.</param>
		public override void SetLength(long value)
		{
			throw new NotSupportedException("Cannot set length in concatenated streams.");
		}

		/// <summary>
		/// Reads a sequence of bytes from the current stream, advances the position within the stream
		/// by the number of bytes read.
		/// </summary>
		/// <param name="buffer"> An array of bytes. When this method returns, the buffer contains the specified
		/// byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
		/// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read fromn the current stream.</param>
		/// <param name="count">The maximum number of bytes to be read from the current stream.</param>
		/// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if
		/// that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
		public override int Read(byte[] buffer, int offset, int count)
		{
			var bytesRead = 0;
			while (bytesRead < count && !HasReachedEndOfStreams())
			{
				var currentStream = _streamEnumerator.Current;

				if (currentStream != null)
				{
					bytesRead += currentStream.Read(buffer, offset + bytesRead, count - bytesRead);
				}
			}

			return bytesRead;
		}

		/// <summary>
		/// Write is not supported.
		/// </summary>
		/// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
		/// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
		/// <param name="count">The number of bytes to be written to the current stream.</param>
		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException("Write is not supported for concatenaded streams.");
		}

		/// <summary>
		/// Returns true if all the streams have been read.
		///
		/// Note: This function iterates the enumerator if the end of the
		/// current stream is reached.
		/// </summary>
		private bool HasReachedEndOfStreams()
		{
			if (_streamEnumerator.Current != null &&
				_streamEnumerator.Current.Position != _streamEnumerator.Current.Length)
			{
				return false;
			}

			var movedNext = MovedNext();

			return !movedNext;
		}

		/// <summary>
		/// This should be used instead of directly calling the <see cref="_streamEnumerator"/>
		/// MoveNext. It ensures that the Position is maintained.
		/// </summary>
		/// <returns>true if the <see cref="_streamEnumerator"/> was successfully advanced to the next element;
		/// false if the <see cref="_streamEnumerator"/> has passed the end of the last stream.</returns>
		private bool MovedNext()
		{
			var currentStream = _streamEnumerator.Current;

			// Try to iterate the enumerator.
			var movedNext = _streamEnumerator.MoveNext();

			if (movedNext)
			{
				_lengthReadStreams += currentStream?.Length ?? 0;
				currentStream?.Dispose();
			}

			return movedNext;
		}
	}
}