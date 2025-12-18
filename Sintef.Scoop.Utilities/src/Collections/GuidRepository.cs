//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// A class storing objects of type T. The objects are referenced by a Guid that is issued when
	/// adding the object.
	/// </summary>
	/// <typeparam name="T">The class of objects to be stored in the repository.</typeparam>
	public class GuidRepository<T>
	{
		private readonly Dictionary<Guid, T> _objects = new Dictionary<Guid, T>();

		/// <summary>
		/// Adds a new object to the storage.
		/// </summary>
		/// <param name="o">Object to add.</param>
		/// <returns><see cref="Guid"/> created for given object.</returns>
		public Guid Add(T o)
		{
			return Add(o, Guid.NewGuid());
		}

		/// <summary>
		/// Retrieves the object with given <paramref name="guid"/>.
		/// </summary>
		/// <param name="guid">Guid to retrive.</param>
		/// <returns>Object T.</returns>
		public T Retrieve(Guid guid)
		{
			return _objects[guid];
		}

		/// <summary>
		/// Adds a new object to the storage with the given <paramref name="guid"/>.
		/// </summary>
		/// <param name="o">Object to add.</param>
		/// <param name="guid">Guid for the object.</param>
		/// <returns><paramref name="guid"/></returns>
		private Guid Add(T o, Guid guid)
		{
			_objects[guid] = o;
			return guid;
		}
	}
}
