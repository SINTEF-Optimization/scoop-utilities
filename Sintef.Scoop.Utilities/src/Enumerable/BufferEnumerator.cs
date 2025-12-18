//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections;
using System.Collections.Generic;

namespace Sintef.Scoop.Utilities
{
	public static partial class Extensions
	{
		/// <summary>
		/// Returns an enumerable that buffers the source enumerable. 
		/// Elements from the source enumerable are enumerated only when needed, and
		/// at most once, no matter how many times the result is enumerated.
		/// 
		/// Enumeration is not thread-safe: If you enumerate the result on multiple
		/// threads, concurrent calls to the source's MoveNext may occur.
		/// </summary>
		public static IEnumerable<T> Buffer<T>(this IEnumerable<T> source)
		{
			return new BufferEnumerable<T>(source);
		}
	}

	/// <summary>
	/// Enumerator that buffers a source enumerator, making sure it is only
	/// enumerated at most once.
	/// This implementation lazily buffers elements as they are actually enumerated.
	///
	/// Because of the lazy nature of the buffering, it is not safe to enumerate this
	/// enumerable simultaneously from multiple threads.
	///
	/// Similar functionality is provided by <see cref="CachedIEnumerable{T}"/>, but
	/// that implementation always caches the entire enumerable before it is
	/// enumerated.
	/// </summary>
	public class BufferEnumerable<T> : IEnumerable<T>, IDisposable
	{
		private readonly IEnumerator<T> _source;
		private readonly List<T> _buffer;

		/// <summary>
		/// Initializes the buffered enumerator
		/// </summary>
		/// <param name="source">The sequence to buffer</param>
		public BufferEnumerable(IEnumerable<T> source)
		{
			_source = source.GetEnumerator();
			_buffer = new List<T>();
		}

		/// <inheritdoc/>
		public IEnumerator<T> GetEnumerator()
		{
			return new BufferEnumerator(_source, _buffer);
		}

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new BufferEnumerator(_source, _buffer);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			_source.Dispose();
		}

		private class BufferEnumerator : IEnumerator<T>
		{
			private readonly IEnumerator<T> _source;
			private readonly List<T> _buffer;
			private int _i = -1;

			public BufferEnumerator(IEnumerator<T> source, List<T> buffer)
			{
				_source = source;
				_buffer = buffer;
			}

			public T Current
			{
				get { return _buffer[_i]; }
			}

			object IEnumerator.Current => Current;

			public bool MoveNext()
			{
				_i++;
				if (_i < _buffer.Count)
					return true;
				if (!_source.MoveNext())
					return false;
				_buffer.Add(_source.Current);
				return true;
			}

			public void Reset()
			{
				_i = -1;
			}

			public void Dispose()
			{
			}
		}
	}
}