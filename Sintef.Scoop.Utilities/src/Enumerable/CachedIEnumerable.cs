//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities
{
	public static partial class Extensions
	{
		/// <summary>
		/// Returns an enumerable that caches the source enumerable. 
		/// All elements from the source enumerable are enumerated and cached when the
		/// cached enumerable is enumerated for the first time. Further enumerations on
		/// the cached enumerable will be cached and more efficient if the underlying
		/// enumerable is costly.
		/// 
		/// The resulting enumerable is thread safe in the sense that multiple threads
		/// may enumerate it independently.
		/// </summary>
		public static IEnumerable<T> Cache<T>(this IEnumerable<T> source)
		{
			return new CachedIEnumerable<T>(source);
		}
	}

	/// <summary>
	/// An enumerable that caches the result of an underlying enumerable.
	/// All elements of the enumerable are cached before this cached enumerable is enumerated for the first time.
	/// Can improve efficiency when you generate a query that is heavy enough that
	/// it should not be evaluated more than once, but it may not be evaluated at all.
	///
	/// A similar mechanism is available in <see cref="BufferEnumerable{T}"/>, but that implementation only
	/// lazily buffers elements from the underlying enumerable when needed.
	/// Which can be more efficient when only parts of the enumerable are enumerated.
	///
	/// This provides some thread safety in that multiple threads may enumerate the resulting cached enumerable
	/// independently, but multiple threads can't safely iterate using the same enumerator.
	/// </summary>
	/// <typeparam name="T">The type of the values in the enumerable.</typeparam>
	public class CachedIEnumerable<T>: IEnumerable<T>
	{
		/// <summary>
		/// The query whose result we cache
		/// </summary>
		private readonly IEnumerable<T> _query;
		
		/// <summary>
		/// The cached result. Null if not computed
		/// </summary>
		private List<T> _cache;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="query">The query to cache</param>
		public CachedIEnumerable(IEnumerable<T> query)
		{
			_query = query;
		}

		#region IEnumerable<T> Members

		/// <summary>
		/// Returns an enumerator for the collection. If the query has not yet been
		/// evaluated, it is evaluated now.
		/// </summary>
		/// <returns></returns>
		public IEnumerator<T> GetEnumerator()
		{
			if (_cache == null)
			{
				lock (_query)
				{
					_cache = _query.ToList();
				}
			}

			return _cache.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		/// <summary>
		/// Returns an enumerator for the collection
		/// </summary>
		/// <returns></returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<T>)this).GetEnumerator();
		}

		#endregion
	}
}
