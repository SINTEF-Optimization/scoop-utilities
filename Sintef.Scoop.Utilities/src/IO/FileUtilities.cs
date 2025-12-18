//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// A collection of file handling and IO utilities.
	/// </summary>
	public static class FileUtilities
	{
		/// <summary>
		/// returns true if a given string represents a full path including volume letter
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool IsFullPath(string path)
		{
			return !String.IsNullOrWhiteSpace(path)
				&& path.IndexOfAny(System.IO.Path.GetInvalidPathChars().ToArray()) == -1
				&& Path.IsPathRooted(path)
				&& !Path.GetPathRoot(path).Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal);
		}

		/// <summary>
		/// Given a path and a base directory to use for relative paths as full path, creates the full path for the given path
		/// 
		/// Similar to Path.GetFullPath(path), but uses the given fullBasePathForRelativePaths as base directory, not the current
		/// environment directory.
		/// </summary>
		/// <param name="fullBasePathForRelativePaths">full path for the base directory to use for relative paths</param>
		/// <param name="path">the path to make full</param>
		/// <returns>The full path</returns>
		public static string MakeFullPath(string fullBasePathForRelativePaths, string path)
		{
			if (IsFullPath(path))
				return path;

			if (!IsFullPath(fullBasePathForRelativePaths))
				throw new ArgumentException("base path not full", fullBasePathForRelativePaths);

			if (Path.IsPathRooted(path))
				return Path.GetPathRoot(fullBasePathForRelativePaths) + path.Substring(Path.GetPathRoot(path).Length);

			return Path.Combine(fullBasePathForRelativePaths, path);
		}

		/// <summary>
		/// Returns true if <paramref name="possibleSubdir"/> is the same directory or a subdirectory
		/// of <paramref name="referenceDir"/>, false otherwise.
		/// 
		/// This method is only supported on .NET 5 and later, since it is based on Path.GetRelativePath.
		/// </summary>
#if NET5_0_OR_GREATER

		public static bool IsSubdirOfOrSame(this string possibleSubdir, string referenceDir)
		{
			string pathFromReference = Path.GetRelativePath(referenceDir, possibleSubdir);

			if (pathFromReference == ".")
				// The directories are the same
				return true;

			string parent = Path.Combine(referenceDir, "..");

			if (Path.GetRelativePath(parent, referenceDir) == ".")
			{
				// referenceDir is a file system root.
				// Check if possibleSubdir is under the same root
				return !Path.IsPathRooted(pathFromReference);
			}

			string pathFromParent = Path.GetRelativePath(parent, possibleSubdir);

			return pathFromParent.Length > pathFromReference.Length;
		}

#else

		[Obsolete("This method is only available from .NET 5 onward")]
		public static bool IsSubdirOfOrSame(this string possibleSubdir, string referenceDir)
		{
			throw new NotSupportedException("This method is only available from .NET 5 onward");
		}

#endif

		/// <summary>
		/// Opens a stream from a regular file or a file in a zip archive.
		/// 
		/// If <paramref name="zipFile"/> is given, opens the zip archive and then
		/// opens the entry in the archive indicated by <paramref name="filename"/>.
		/// If <paramref name="zipFile"/> is null, opens the <paramref name="filename"/>
		/// as a regular file.
		/// </summary>
		/// <param name="filename">The path of the file</param>
		/// <param name="zipFile">The path to the zip archive, or null</param>
		public static Stream OpenFileMaybeInZipFile(string filename, string zipFile)
		{
			if (zipFile == null)
				return File.OpenRead(filename);
			else
				return OpenFileInZipFile(zipFile, filename);
		}

		/// <summary>
		/// Opens a stream in a zip archive
		/// </summary>
		/// <param name="zipFile">The path to the zip archive</param>
		/// <param name="pathInZipFile">The path of the file within the zip archive</param>
		/// <returns>The stream</returns>
		public static Stream OpenFileInZipFile(string zipFile, string pathInZipFile)
		{
			ZipArchive archive = ZipFile.OpenRead(zipFile);
			ZipArchiveEntry entry = archive.Entries.SingleOrDefault(e => e.FullName == pathInZipFile);

			if (entry == null)
			{
				string bestMatch = archive.Entries
					.MaxBy(e => e.FullName.SimilarityWith(pathInZipFile))
					.FullName;

				throw new FileNotFoundException($"Did not find path '{pathInZipFile}' in zip file '{zipFile}'.\n(Best match was '{bestMatch}')");
			}

			return entry.Open();
		}

		/// <summary>
		/// Compares the contents of two files
		/// </summary>
		/// <param name="filename1">The name of the first file</param>
		/// <param name="filename2">The name of the second file</param>
		/// <returns>True if the files are equal, false if not</returns>
		public static bool FilesAreEqual(string filename1, string filename2)
		{
			FileStream fs1;
			FileStream fs2;

			// Determine if the same file was referenced two times.
			if (filename1 == filename2)
			{
				// Return true to indicate that the files are the same.
				return true;
			}

			// Open the two files.
			using (fs1 = new FileStream(filename1, FileMode.Open))
			{
				using (fs2 = new FileStream(filename2, FileMode.Open))
				{
					// Compare the streams
					return StreamUtilities.StreamsAreEqual(fs1, fs2);
				}
			}
		}

		/// <summary>
		/// Returns the first mismatching line from each of two text files.
		/// If the files are equal, returns (null, null).
		/// </summary>
		/// <param name="filename1">The name of the first file</param>
		/// <param name="filename2">The name of the second file</param>
		/// <param name="acceptedDifferences">If not null, line differences that are listed in this
		///   enumerable, are not counted as a mismatch. Each string pair may match at most
		///   one pair of differing lines.</param>
		public static (string, string) FirstDifferingLines(string filename1, string filename2,
			IEnumerable<(string, string)> acceptedDifferences = null)
		{
			return FirstDifferingLines(filename1, filename2, acceptedDifferences, null);
		}

		/// <summary>
		/// Returns the first mismatching line from each of two text files.
		/// If the files are equal, returns (null, null).
		/// </summary>
		/// <param name="filename1">The name of the first file</param>
		/// <param name="filename2">The name of the second file</param>
		/// <param name="acceptedDifferences">If not null, line differences that are listed in this
		///   enumerable, are not counted as a mismatch. Each string pair may match at most
		///   one pair of differing lines.</param>
		/// <param name="accept">If not null, a function that, given one line from each file, returns true if the
		///   lines are not considered a mismatch.</param>
		public static (string, string) FirstDifferingLines(string filename1, string filename2,
			IEnumerable<(string, string)> acceptedDifferences, Func<string, string, bool> accept = null)
		{
			// Open the two files.
			using (var reader1 = new StreamReader(filename1))
			{
				using (var reader2 = new StreamReader(filename2))
				{
					return FirstDifferingLines(reader1, reader2, acceptedDifferences, accept);
				}
			}
		}

		/// <summary>
		/// Returns the first mismatching line from each of two text readers.
		/// If the files are equal, returns (null, null).
		/// </summary>
		/// <param name="reader1">The first reader</param>
		/// <param name="reader2">The second reader</param>
		/// <param name="acceptedDifferences">If not null, line differences that are listed in this
		///   enumerable, are not counted as a mismatch. Each string pair may match at most
		///   one pair of differing lines.</param>
		/// <param name="accept">If not null, a function that, given one line from each reader, returns true if the
		///   lines are not considered a mismatch.</param>
		public static (string, string) FirstDifferingLines(TextReader reader1, TextReader reader2,
			IEnumerable<(string, string)> acceptedDifferences = null, Func<string, string, bool> accept = null)
		{
			List<(string, string)> remainingDifferences = acceptedDifferences?.ToList()
				?? new List<(string, string)>();
			accept ??= (_, _) => false;

			while (true)
			{
				string line1 = reader1.ReadLine();
				string line2 = reader2.ReadLine();

				if (line1 == null && line2 == null)
					return (null, null);

				line1 ??= "<eof>";
				line2 ??= "<eof>";

				if (line1 != line2)
				{
					if (accept(line1, line2))
						continue;

					if (remainingDifferences.Any(pair => pair.Item1 == line1 && pair.Item2 == line2))
					{
						remainingDifferences.Remove((line1, line2));
						continue;
					}
					if (remainingDifferences.Any(pair => pair.Item2 == line1 && pair.Item1 == line2))
					{
						remainingDifferences.Remove((line2, line1));
						continue;
					}

					return (line1, line2);
				}
			}
		}

		/// <summary>
		/// Creates a zip archive that contains the files and directories from the specified directory.
		/// </summary>
		public static void ZipAllFilesInDirectory(string sourceDirectoryName, string destinationArchiveFileName)
		{
			ZipFile.CreateFromDirectory(sourceDirectoryName, destinationArchiveFileName);
		}

		/// <summary>
		/// Compares the contents of two zip files
		/// </summary>
		/// <param name="filename1">The name of the first file</param>
		/// <param name="filename2">The name of the second file</param>
		/// <param name="reason">If the files are not equal, this string contains an explanation on exit</param>
		/// <returns>True if the files are equal, false if not</returns>
		public static bool ZipFilesAreEqual(string filename1, string filename2, out string reason)
		{
			if (!File.Exists(filename1))
				throw new ArgumentException($"File 1 ({filename1}) does not exist");
			if (!File.Exists(filename2))
				throw new ArgumentException($"File 2 ({filename2}) does not exist");

			reason = "";

			// Determine if the same file was referenced two times.
			if (filename1 == filename2)
			{
				// Return true to indicate that the files are the same.
				return true;
			}

			// Open the two files.
			using (ZipArchive zip1 = new ZipArchive(new FileStream(filename1, FileMode.Open)))
			{
				using (ZipArchive zip2 = new ZipArchive(new FileStream(filename2, FileMode.Open)))
				{
					var entries1 = zip1.Entries.Select(e => e.Name).ToList();
					var entries2 = zip2.Entries.Select(e => e.Name).ToList();
					var missing = entries1.Except(entries2);
					if (missing.Any())
					{
						reason = $"file '{missing.First()}' is only present in the first file";
						return false;
					}

					missing = entries2.Except(entries1);
					if (missing.Any())
					{
						reason = $"file '{missing.First()}' is only present in the second file";
						return false;
					}

					foreach (var entry in zip1.Entries)
					{
						using (var stream1 = entry.Open())
						{
							using (var stream2 = zip2.GetEntry(entry.Name).Open())
							{
								if (!StreamUtilities.StreamsAreEqual(stream1, stream2))
								{
									reason = $"File {entry.Name} differs";
									return false;
								}
							}
						}
					}
				}


				// All files are equal
				return true;
			}
		}

		/// <summary>
		/// Converts an enumerable of filenames to an enumerable of <see cref="FileStream"/>.
		/// </summary>
		/// <param name="filenames">Enumerable of filenames.</param>
		/// <returns>Enumerable of filestreams.</returns>
		public static IEnumerable<FileStream> ToFilestreams(IEnumerable<string> filenames)
		{
			return filenames.Select(filename => new FileStream(filename, FileMode.Open));
		}

		/// <summary>
		/// Returns a concatenated stream of all the files specified by the enumerable of filenames.
		/// </summary>
		/// <param name="filenames">Enumerable of filenames.</param>
		/// <returns>Concatenated stream of all the files.</returns>
		public static Stream ToStream(IEnumerable<string> filenames)
		{
			return ToFilestreams(filenames).Concat();
		}

		/// <summary>
		/// REMOVE IN FUTURE VERSION.
		/// USE <see cref="StreamUtilities.StreamsAreEqual"/> instead.
		/// 
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
			return StreamUtilities.StreamsAreEqual(stream1, stream2);
		}

		/// <summary>
		/// Returns a memory stream whose contents is the given string
		/// </summary>
		public static MemoryStream ToMemoryStream(this string s)
		{
			return new MemoryStream(Encoding.UTF8.GetBytes(s));
		}
	}
}