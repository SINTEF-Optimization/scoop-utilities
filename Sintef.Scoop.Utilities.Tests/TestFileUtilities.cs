//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sintef.Scoop.Utilities.Tests
{
	/// <summary>
	/// Tests for FileUtilities
	/// </summary>
	[TestClass]
	public class TestFileUtilities
	{
		[TestMethod]
		public void IsSubdirOfOrSameIsCorrect()
		{
			List<string> dirs = [".", "..", "../..", "/", "/folder", "c:/folder", "d:/folder", "folder", "root/dir/other/folder", "root/dir/../folder"];

			if (Path.DirectorySeparatorChar == '\\')
				dirs.AddRange(["..\\..", "\\", "\\folder", "c:\\", "d:\\", "c:\\folder", "d:\\folder", "folder", "root\\dir\\other\\folder", "root\\dir\\..\\folder"]);

			foreach (string dir in dirs)
			{
				string parent = Path.Combine(dir, "..");
				bool dirIsARoot = Path.GetRelativePath(dir, parent) == ".";
				string child = Path.Combine(dir, "child");

				Assert.IsTrue(parent.IsSubdirOfOrSame(parent));
				Assert.IsTrue(dir.IsSubdirOfOrSame(dir));
				Assert.IsTrue(child.IsSubdirOfOrSame(child));

				if (!dirIsARoot)
				{
					Assert.IsTrue(dir.IsSubdirOfOrSame(parent));
					Assert.IsFalse(parent.IsSubdirOfOrSame(dir));
				}

				Assert.IsFalse(dir.IsSubdirOfOrSame(child));
				Assert.IsTrue(child.IsSubdirOfOrSame(dir));

				Assert.IsFalse(parent.IsSubdirOfOrSame(child));
				Assert.IsTrue(child.IsSubdirOfOrSame(parent));

				string descendant = Path.Combine(dir, "child/descendant/baby");

				Assert.IsFalse(dir.IsSubdirOfOrSame(descendant));
				Assert.IsTrue(descendant.IsSubdirOfOrSame(dir));

				if (!dirIsARoot)
				{
					string sibling = Path.Combine(dir, "../otherFolder");

					Assert.IsFalse(dir.IsSubdirOfOrSame(sibling));
					Assert.IsFalse(sibling.IsSubdirOfOrSame(dir));
					Assert.IsFalse(child.IsSubdirOfOrSame(sibling));
					Assert.IsFalse(sibling.IsSubdirOfOrSame(child));
				}

				string alsoDir = Path.Combine(dir, "child/../");

				Assert.IsTrue(dir.IsSubdirOfOrSame(alsoDir));
				Assert.IsTrue(alsoDir.IsSubdirOfOrSame(dir));
				Assert.IsTrue(child.IsSubdirOfOrSame(alsoDir));
				Assert.IsFalse(alsoDir.IsSubdirOfOrSame(child));

				if (dir.EndsWith("folder"))
				{
					alsoDir = Path.Combine(dir, "../folder");

					Assert.IsTrue(dir.IsSubdirOfOrSame(alsoDir));
					Assert.IsTrue(alsoDir.IsSubdirOfOrSame(dir));
					Assert.IsTrue(child.IsSubdirOfOrSame(alsoDir));
					Assert.IsFalse(alsoDir.IsSubdirOfOrSame(child));
					Assert.IsTrue(alsoDir.IsSubdirOfOrSame(parent));
					Assert.IsFalse(parent.IsSubdirOfOrSame(alsoDir));
				}

				string path1 = Path.Combine(Environment.CurrentDirectory, "here");
				string path2 = "here";

				Assert.IsTrue(path1.IsSubdirOfOrSame(path2));
				Assert.IsTrue(path2.IsSubdirOfOrSame(path1));

				path1 = "c:/there";
				path2 = "d:/there";

				Assert.IsFalse(path1.IsSubdirOfOrSame(path2));
				Assert.IsFalse(path2.IsSubdirOfOrSame(path1));
			}
		}

		[TestMethod]
		public void ExqualTextsAreAccepted()
		{
			var diff = Diff("qq\naaa\nqq", "qq\naaa\nqq");

			Assert.AreEqual((null, null), diff);
		}

		[TestMethod]
		public void DifferingLinesAreIdentified()
		{
			var diff = Diff("qq\naaa\nqq", "qq\nbbb\nqq");

			Assert.AreEqual(("aaa", "bbb"), diff);
		}

		[TestMethod]
		public void DifferingTextLengthsAreIdentified()
		{
			var diff = Diff("qq\naaa\nqq", "qq\naaa\nqq\ncc");

			Assert.AreEqual(("<eof>", "cc"), diff);
		}

		[TestMethod]
		public void DifferingLinesAreAcceptedByList()
		{
			var diff = Diff("qq\naaa\nqq", "qq\nbbb\nqq", acceptedDifferences: new[] { ("aaa", "bbb") });

			Assert.AreEqual((null, null), diff);
		}

		[TestMethod]
		public void EachDifferenceIsAcceptedOnlyOnce()
		{
			var diff = Diff("qq\naaa\naaa\nqq", "qq\nbbb\nbbb\nqq", acceptedDifferences: new[] { ("aaa", "bbb") });

			Assert.AreEqual(("aaa", "bbb"), diff);
		}

		[TestMethod]
		public void DifferingLinesAreAcceptedByFunction()
		{
			var diff = Diff("qq\naaa\naaa\nqq", "qq\nbbb\nccc\nqq", accept: (s1, s2) => s1 == "aaa");

			Assert.AreEqual((null, null), diff);
		}

		private static (string, string) Diff(string text1, string text2, (string, string)[] acceptedDifferences = null, Func<string, string, bool> accept = null)
		{
			using var reader1 = new StringReader(text1);
			using var reader2 = new StringReader(text2);

			return FileUtilities.FirstDifferingLines(reader1, reader2, acceptedDifferences, accept);
		}
	}
}
