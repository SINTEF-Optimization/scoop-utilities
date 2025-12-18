//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// A helper class for locating directories
	/// </summary>
	public class DirectoryFinder
	{
		/// <summary>
		/// Searches for a directory equal to or above the currect working directory that has the given name
		/// and/or contains subdirectories with the given names.
		/// Throws an exception or returns null if it fails.
		/// </summary>
		/// <param name="dirName">The name of the directory to find. If null, any name will do.</param>
		/// <param name="subdirNames">Names of subdirectories that must be present in the directory to find.</param>
		/// <param name="throwOnFail">If true, the function throws an exception if the directory is not found.
		///   If false, it returns null instead.</param>
		/// <returns>Full path of the target directory</returns>
		public static string FindDirectoryAboveCurrent(string dirName, IEnumerable<string> subdirNames, bool throwOnFail = true)
		{
			DirectoryInfo dir = new DirectoryInfo(Directory.GetCurrentDirectory());

			while (dir != null)
			{
				if (dirName == null || dir.Name == dirName)
				{
					var subDirs = dir.GetDirectories().Select(x => x.Name);
					if (subdirNames.Except(subDirs).Count() == 0)
						return dir.FullName;
				}

				dir = dir.Parent;
			}

			if (throwOnFail)
			{
				string spec = dirName ?? ("containing {" + subdirNames.Concatenate(", ") + "}");

				throw new Exception(string.Format("Unable to find the requested ancestor directory {0}. Current dir is {1}", spec, System.IO.Directory.GetCurrentDirectory()));
			}
			else
				return null;
		}
	}
}
