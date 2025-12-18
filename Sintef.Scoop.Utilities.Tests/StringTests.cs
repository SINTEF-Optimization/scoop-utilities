//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities.Tests
{
	[TestClass]
	public class StringTests
	{
		[TestMethod]
		public void TestStringPairSplit1()
		{
			string data = null;
			List<StringPairManipulation.Pair> delimeters = new List<StringPairManipulation.Pair>();
			List<StringPairManipulation.Pair> nonsplit = new List<StringPairManipulation.Pair>();
			StringPairManipulation.SplitOptions options = new StringPairManipulation.SplitOptions();

			nonsplit.AddRange(StringPairManipulation.DefinePairs(new char[,] { { '<', '>' }, { '"', '"' }, { '\'', '\'' } }));
			delimeters.Add(new StringPairManipulation.Pair('[', ']'));
			delimeters.Add(new StringPairManipulation.Pair('{', '}'));

			data = "This {is [a } fancy] test} \\'with [some]\\' stuff";
			options.NestedInsideDefinition = StringPairManipulation.SplitOptions.NestedInsideDefinitions.WithRespectToParent;
			options.IncludeDelimeters = true;
			options.EnableNestingInDelimeterPairs = true;
			options.EnableDelimeterNestingInNonSplitPairs = false;
			options.EnableNonSplitNestingInNonSplitPairs = false;
			var result = StringPairManipulation.Split(data, delimeters, nonsplit, options);
			Assert.AreEqual(5, result.Count);
			Assert.AreEqual("This ", "" + result[0]);
			Assert.AreEqual("{is [a } fancy] test}", "" + result[1]);
			Assert.AreEqual(" \\'with ", "" + result[2]);
			Assert.AreEqual("[some]", "" + result[3]);
			Assert.AreEqual("\\' stuff", "" + result[4]);

			data = "This \\{is [a } fancy] test} 'with [some]' stuff";
			options.NestedInsideDefinition = StringPairManipulation.SplitOptions.NestedInsideDefinitions.WithRespectToParent;
			options.IncludeDelimeters = true;
			options.EnableNestingInDelimeterPairs = true;
			options.EnableDelimeterNestingInNonSplitPairs = false;
			options.EnableNonSplitNestingInNonSplitPairs = false;
			result = StringPairManipulation.Split(data, delimeters, nonsplit, options);
			Assert.AreEqual(3, result.Count);
			Assert.AreEqual("This \\{is ", "" + result[0]);
			Assert.AreEqual("[a } fancy]", "" + result[1]);
			Assert.AreEqual(" test} 'with [some]' stuff", "" + result[2]);

			data = "This \\{is [a } fancy] test} 'with [some]' and [more] stuff";
			options.NestedInsideDefinition = StringPairManipulation.SplitOptions.NestedInsideDefinitions.WithRespectToParent;
			options.IncludeDelimeters = true;
			options.EnableNestingInDelimeterPairs = true;
			options.EnableDelimeterNestingInNonSplitPairs = false;
			options.EnableNonSplitNestingInNonSplitPairs = false;
			result = StringPairManipulation.Split(data, delimeters, nonsplit, options);
			Assert.AreEqual(5, result.Count);
			Assert.AreEqual("This \\{is ", "" + result[0]);
			Assert.AreEqual("[a } fancy]", "" + result[1]);
			Assert.AreEqual(" test} 'with [some]' and ", "" + result[2]);
			Assert.AreEqual("[more]", "" + result[3]);
			Assert.AreEqual(" stuff", "" + result[4]);

			data = "This \\{is [a } fancy] test} 'with [some' and [more] stuff";
			options.NestedInsideDefinition = StringPairManipulation.SplitOptions.NestedInsideDefinitions.WithRespectToParent;
			options.IncludeDelimeters = true;
			options.EnableNestingInDelimeterPairs = true;
			options.EnableDelimeterNestingInNonSplitPairs = false;
			options.EnableNonSplitNestingInNonSplitPairs = false;
			result = StringPairManipulation.Split(data, delimeters, nonsplit, options);
			Assert.AreEqual(5, result.Count);
			Assert.AreEqual("This \\{is ", "" + result[0]);
			Assert.AreEqual("[a } fancy]", "" + result[1]);
			Assert.AreEqual(" test} 'with [some' and ", "" + result[2]);
			Assert.AreEqual("[more]", "" + result[3]);
			Assert.AreEqual(" stuff", "" + result[4]);

			data = "This \\{is [a } fancy] test} 'with [some' and [more] stuff";
			options.NestedInsideDefinition = StringPairManipulation.SplitOptions.NestedInsideDefinitions.WithRespectToParent;
			options.IncludeDelimeters = true;
			options.EnableNestingInDelimeterPairs = true;
			options.EnableDelimeterNestingInNonSplitPairs = true;
			options.EnableNonSplitNestingInNonSplitPairs = false;
			result = StringPairManipulation.Split(data, delimeters, nonsplit, options);
			Assert.AreEqual(3, result.Count);
			Assert.AreEqual("This \\{is ", "" + result[0]);
			Assert.AreEqual("[a } fancy]", "" + result[1]);
			Assert.AreEqual(" test} 'with [some' and [more] stuff", "" + result[2]);

			data = "This \\{is [a } fancy] test} 'with \"some' and [more] stuff";
			options.NestedInsideDefinition = StringPairManipulation.SplitOptions.NestedInsideDefinitions.WithRespectToParent;
			options.IncludeDelimeters = true;
			options.EnableNestingInDelimeterPairs = true;
			options.EnableDelimeterNestingInNonSplitPairs = false;
			options.EnableNonSplitNestingInNonSplitPairs = false;
			result = StringPairManipulation.Split(data, delimeters, nonsplit, options);
			Assert.AreEqual(5, result.Count);
			Assert.AreEqual("This \\{is ", "" + result[0]);
			Assert.AreEqual("[a } fancy]", "" + result[1]);
			Assert.AreEqual(" test} 'with \"some' and ", "" + result[2]);
			Assert.AreEqual("[more]", "" + result[3]);
			Assert.AreEqual(" stuff", "" + result[4]);

			data = "This \\{is [a } fancy] test} 'with \"some' and [more] stuff";
			options.NestedInsideDefinition = StringPairManipulation.SplitOptions.NestedInsideDefinitions.WithRespectToParent;
			options.IncludeDelimeters = true;
			options.EnableNestingInDelimeterPairs = true;
			options.EnableDelimeterNestingInNonSplitPairs = true;
			options.EnableNonSplitNestingInNonSplitPairs = true;
			result = StringPairManipulation.Split(data, delimeters, nonsplit, options);
			Assert.AreEqual(3, result.Count);
			Assert.AreEqual("This \\{is ", "" + result[0]);
			Assert.AreEqual("[a } fancy]", "" + result[1]);
			Assert.AreEqual(" test} 'with \"some' and [more] stuff", "" + result[2]);

			data = "This {is 'a [ fancy' {te}st} 'with [some]] and' [more] }stuff";
			options.NestedInsideDefinition = StringPairManipulation.SplitOptions.NestedInsideDefinitions.WithRespectToParent;
			options.IncludeDelimeters = true;
			options.EnableNestingInDelimeterPairs = true;
			options.EnableDelimeterNestingInNonSplitPairs = false;
			options.EnableNonSplitNestingInNonSplitPairs = false;
			result = StringPairManipulation.Split(data, delimeters, nonsplit, options);
			Assert.AreEqual(5, result.Count);
			Assert.AreEqual("This ", "" + result[0]);
			Assert.AreEqual("{is 'a [ fancy' {te}st}", "" + result[1]);
			Assert.AreEqual(" 'with [some]] and' ", "" + result[2]);
			Assert.AreEqual("[more]", "" + result[3]);
			Assert.AreEqual(" }stuff", "" + result[4]);

			data = "This {is 'a [ fancy' {te}st} 'with [some]] and' [more] }stuff";
			options.NestedInsideDefinition = StringPairManipulation.SplitOptions.NestedInsideDefinitions.WithRespectToParent;
			options.IncludeDelimeters = true;
			options.EnableNestingInDelimeterPairs = true;
			options.EnableDelimeterNestingInNonSplitPairs = true;
			options.EnableNonSplitNestingInNonSplitPairs = false;
			result = StringPairManipulation.Split(data, delimeters, nonsplit, options);
			Assert.AreEqual(3, result.Count);
			Assert.AreEqual("This ", "" + result[0]);
			Assert.AreEqual("{is 'a [ fancy' {te}st} 'with [some]] and' [more] }", "" + result[1]);
			Assert.AreEqual("stuff", "" + result[2]);

			data = "This { is < a [ fancy < {te}st} with [some]] and > [more] }stuff";
			options.NestedInsideDefinition = StringPairManipulation.SplitOptions.NestedInsideDefinitions.WithRespectToParent;
			options.IncludeDelimeters = true;
			options.EnableNestingInDelimeterPairs = true;
			options.EnableDelimeterNestingInNonSplitPairs = true;
			options.EnableNonSplitNestingInNonSplitPairs = true;
			result = StringPairManipulation.Split(data, delimeters, nonsplit, options);
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual("This ", "" + result[0]);
			Assert.AreEqual("{ is < a [ fancy < {te}st} with [some]] and > [more] }stuff", "" + result[1]);

			data = "This { is < a [ fancy < {te}st} with [some]] and > [more] }stuff";
			options.NestedInsideDefinition = StringPairManipulation.SplitOptions.NestedInsideDefinitions.WithRespectToParent;
			options.IncludeDelimeters = true;
			options.EnableNestingInDelimeterPairs = true;
			options.EnableDelimeterNestingInNonSplitPairs = true;
			options.EnableNonSplitNestingInNonSplitPairs = false;
			result = StringPairManipulation.Split(data, delimeters, nonsplit, options);
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual("This ", "" + result[0]);
			Assert.AreEqual("{ is < a [ fancy < {te}st} with [some]] and > [more] }stuff", "" + result[1]);

			data = "This { is < a [ fancy < {te}st} with [some]] and > [more] }stuff";
			options.NestedInsideDefinition = StringPairManipulation.SplitOptions.NestedInsideDefinitions.Total;
			options.IncludeDelimeters = true;
			options.EnableNestingInDelimeterPairs = true;
			options.EnableDelimeterNestingInNonSplitPairs = true;
			options.EnableNonSplitNestingInNonSplitPairs = false;
			result = StringPairManipulation.Split(data, delimeters, nonsplit, options);
			Assert.AreEqual(3, result.Count);
			Assert.AreEqual("This ", "" + result[0]);
			Assert.AreEqual("{ is < a [ fancy < {te}st} with [some]] and > [more] }", "" + result[1]);
			Assert.AreEqual("stuff", "" + result[2]);

			data = "This {is 'a [ fancy' {te}st} 'with [some]] and' [more] }stuff";
			options.NestedInsideDefinition = StringPairManipulation.SplitOptions.NestedInsideDefinitions.WithRespectToParent;
			options.IncludeDelimeters = true;
			options.EnableNestingInDelimeterPairs = false;
			options.EnableDelimeterNestingInNonSplitPairs = false;
			options.EnableNonSplitNestingInNonSplitPairs = false;
			result = StringPairManipulation.Split(data, delimeters, nonsplit, options);
			Assert.AreEqual(5, result.Count);
			Assert.AreEqual("This ", "" + result[0]);
			Assert.AreEqual("{is 'a [ fancy' {te}", "" + result[1]);
			Assert.AreEqual("st} 'with [some]] and' ", "" + result[2]);
			Assert.AreEqual("[more]", "" + result[3]);
			Assert.AreEqual(" }stuff", "" + result[4]);

			data = "This \\{is [a } fancy] test} with [some] stuff";
			options.NestedInsideDefinition = StringPairManipulation.SplitOptions.NestedInsideDefinitions.WithRespectToParent;
			options.IncludeDelimeters = true;
			options.EnableNestingInDelimeterPairs = true;
			options.EnableDelimeterNestingInNonSplitPairs = false;
			options.EnableNonSplitNestingInNonSplitPairs = false;
			result = StringPairManipulation.Split(data, delimeters, nonsplit, options);
			Assert.AreEqual(5, result.Count);
			Assert.AreEqual("This \\{is ", "" + result[0]);
			Assert.AreEqual("[a } fancy]", "" + result[1]);
			Assert.AreEqual(" test} with ", "" + result[2]);
			Assert.AreEqual("[some]", "" + result[3]);
			Assert.AreEqual(" stuff", "" + result[4]);

			data = "This \\{is [a } fancy] test} with [some] stuff : and [some] : more";
			delimeters.Add(new StringPairManipulation.Pair(':'));
			options.NestedInsideDefinition = StringPairManipulation.SplitOptions.NestedInsideDefinitions.WithRespectToParent;
			options.IncludeDelimeters = true;
			options.EnableNestingInDelimeterPairs = true;
			options.EnableDelimeterNestingInNonSplitPairs = false;
			options.EnableNonSplitNestingInNonSplitPairs = false;
			result = StringPairManipulation.Split(data, delimeters, nonsplit, options);
			Assert.AreEqual(11, result.Count);
			Assert.AreEqual("This \\{is ", "" + result[0]);
			Assert.AreEqual("[a } fancy]", "" + result[1]);
			Assert.AreEqual(" test} with ", "" + result[2]);
			Assert.AreEqual("[some]", "" + result[3]);
			Assert.AreEqual(" stuff ", "" + result[4]);
			Assert.AreEqual(":", "" + result[5]);
			Assert.AreEqual(" and ", "" + result[6]);
			Assert.AreEqual("[some]", "" + result[7]);
			Assert.AreEqual(" ", "" + result[8]);
			Assert.AreEqual(":", "" + result[9]);
			Assert.AreEqual(" more", "" + result[10]);

			data = "This \\{is [a } fancy] test} with [s[o]me] stuff : and [some] : more";
			options.NestedInsideDefinition = StringPairManipulation.SplitOptions.NestedInsideDefinitions.WithRespectToParent;
			options.IncludeDelimeters = false;
			options.EnableNestingInDelimeterPairs = true;
			options.EnableDelimeterNestingInNonSplitPairs = false;
			options.EnableNonSplitNestingInNonSplitPairs = false;
			result = StringPairManipulation.Split(data, delimeters, nonsplit, options);
			Assert.AreEqual(9, result.Count);
			Assert.AreEqual("This \\{is ", "" + result[0]);
			Assert.AreEqual("a } fancy", "" + result[1]);
			Assert.AreEqual(" test} with ", "" + result[2]);
			Assert.AreEqual("s[o]me", "" + result[3]);
			Assert.AreEqual(" stuff ", "" + result[4]);
			Assert.AreEqual(" and ", "" + result[5]);
			Assert.AreEqual("some", "" + result[6]);
			Assert.AreEqual(" ", "" + result[7]);
			Assert.AreEqual(" more", "" + result[8]);

			data = "This : is [a : fancy] test} with [some] stuff";
			options.NestedInsideDefinition = StringPairManipulation.SplitOptions.NestedInsideDefinitions.WithRespectToParent;
			options.IncludeDelimeters = true;
			options.EnableNestingInDelimeterPairs = true;
			options.EnableDelimeterNestingInNonSplitPairs = false;
			options.EnableNonSplitNestingInNonSplitPairs = false;
			result = StringPairManipulation.Split(data, delimeters, nonsplit, options);
			Assert.AreEqual(7, result.Count);
			Assert.AreEqual("This ", "" + result[0]);
			Assert.AreEqual(":", "" + result[1]);
			Assert.AreEqual(" is ", "" + result[2]);
			Assert.AreEqual("[a : fancy]", "" + result[3]);
			Assert.AreEqual(" test} with ", "" + result[4]);
			Assert.AreEqual("[some]", "" + result[5]);
			Assert.AreEqual(" stuff", "" + result[6]);

			data = "This \n is [a \n fancy] test} with [some] stuff";
			delimeters.Add(new StringPairManipulation.Pair('\n'));
			options.NestedInsideDefinition = StringPairManipulation.SplitOptions.NestedInsideDefinitions.WithRespectToParent;
			options.IncludeDelimeters = true;
			options.EnableNestingInDelimeterPairs = true;
			options.EnableDelimeterNestingInNonSplitPairs = false;
			options.EnableNonSplitNestingInNonSplitPairs = false;
			result = StringPairManipulation.Split(data, delimeters, nonsplit, options);
			Assert.AreEqual(7, result.Count);
			Assert.AreEqual("This ", "" + result[0]);
			Assert.AreEqual("\n", "" + result[1]);
			Assert.AreEqual(" is ", "" + result[2]);
			Assert.AreEqual("[a \n fancy]", "" + result[3]);
			Assert.AreEqual(" test} with ", "" + result[4]);
			Assert.AreEqual("[some]", "" + result[5]);
			Assert.AreEqual(" stuff", "" + result[6]);

			delimeters.Add(new StringPairManipulation.Pair('*', '*'));
			data = "This * is [a * fancy] test} with [some] stuff";
			options.NestedInsideDefinition = StringPairManipulation.SplitOptions.NestedInsideDefinitions.WithRespectToParent;
			options.IncludeDelimeters = true;
			options.EnableNestingInDelimeterPairs = true;
			options.EnableDelimeterNestingInNonSplitPairs = false;
			options.EnableNonSplitNestingInNonSplitPairs = false;
			result = StringPairManipulation.Split(data, delimeters, nonsplit, options);
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual("This ", "" + result[0]);
			Assert.AreEqual("* is [a * fancy] test} with [some] stuff", "" + result[1]);

			data = "This {is [a } fancy] test} with [some] stuff";
			options.NestedInsideDefinition = StringPairManipulation.SplitOptions.NestedInsideDefinitions.WithRespectToParent;
			options.IncludeDelimeters = true;
			options.EnableNestingInDelimeterPairs = false;
			options.EnableDelimeterNestingInNonSplitPairs = false;
			options.EnableNonSplitNestingInNonSplitPairs = false;
			result = StringPairManipulation.Split(data, delimeters, nonsplit, options);
			Assert.AreEqual(5, result.Count);
			Assert.AreEqual("This ", "" + result[0]);
			Assert.AreEqual("{is [a }", "" + result[1]);
			Assert.AreEqual(" fancy] test} with ", "" + result[2]);
			Assert.AreEqual("[some]", "" + result[3]);
			Assert.AreEqual(" stuff", "" + result[4]);

			data = "This : is [a : fancy] test} with [some] stuff";
			options.NestedInsideDefinition = StringPairManipulation.SplitOptions.NestedInsideDefinitions.WithRespectToParent;
			options.IncludeDelimeters = true;
			options.EnableNestingInDelimeterPairs = false;
			options.EnableDelimeterNestingInNonSplitPairs = false;
			options.EnableNonSplitNestingInNonSplitPairs = false;
			result = StringPairManipulation.Split(data, delimeters, nonsplit, options);
			Assert.AreEqual(7, result.Count);
			Assert.AreEqual("This ", "" + result[0]);
			Assert.AreEqual(":", "" + result[1]);
			Assert.AreEqual(" is ", "" + result[2]);
			Assert.AreEqual("[a : fancy]", "" + result[3]);
			Assert.AreEqual(" test} with ", "" + result[4]);
			Assert.AreEqual("[some]", "" + result[5]);
			Assert.AreEqual(" stuff", "" + result[6]);

			data = "This * is [a * fancy] test} with [some] stuff";
			options.NestedInsideDefinition = StringPairManipulation.SplitOptions.NestedInsideDefinitions.WithRespectToParent;
			options.IncludeDelimeters = true;
			options.EnableNestingInDelimeterPairs = false;
			options.EnableDelimeterNestingInNonSplitPairs = false;
			options.EnableNonSplitNestingInNonSplitPairs = false;
			result = StringPairManipulation.Split(data, delimeters, nonsplit, options);
			Assert.AreEqual(5, result.Count);
			Assert.AreEqual("This ", "" + result[0]);
			Assert.AreEqual("* is [a *", "" + result[1]);
			Assert.AreEqual(" fancy] test} with ", "" + result[2]);
			Assert.AreEqual("[some]", "" + result[3]);
			Assert.AreEqual(" stuff", "" + result[4]);

		}

		[TestMethod]
		public void TestStringPairSplit2()
		{
			string data = null;
			List<StringPairManipulation.Pair> delimeters = new List<StringPairManipulation.Pair>();
			List<StringPairManipulation.Pair> nonsplit = new List<StringPairManipulation.Pair>();
			StringPairManipulation.SplitOptions options = new StringPairManipulation.SplitOptions();

			nonsplit.Add(new StringPairManipulation.Pair('<', '>'));
			StringPairManipulation.Pair brackets = new StringPairManipulation.Pair('[', ']');
			StringPairManipulation.Pair start = new StringPairManipulation.Pair(
				new StringPairManipulation.ExtendedChar(StringPairManipulation.ExtendedChar.CharacterTypes.StartOfString),
				new StringPairManipulation.ExtendedChar('¤'));
			StringPairManipulation.Pair end = new StringPairManipulation.Pair(
				new StringPairManipulation.ExtendedChar(':'),
				new StringPairManipulation.ExtendedChar(StringPairManipulation.ExtendedChar.CharacterTypes.EndOfString));
			delimeters.Add(brackets);
			delimeters.Add(start);
			delimeters.Add(end);

			data = "This [is ¤ a <:> ] fancy ¤ test ¤ [with] : some : [other] stuff";
			options.NestedInsideDefinition = StringPairManipulation.SplitOptions.NestedInsideDefinitions.WithRespectToParent;
			options.IncludeDelimeters = true;
			options.EnableNestingInDelimeterPairs = true;
			options.EnableDelimeterNestingInNonSplitPairs = false;
			options.EnableNonSplitNestingInNonSplitPairs = true;
			var result = StringPairManipulation.Split(data, delimeters, nonsplit, options);
			Assert.AreEqual(5, result.Count);
			Assert.AreEqual("This [is ¤ a <:> ] fancy ¤", "" + result[0]);
			Assert.AreEqual(" test ¤ ", "" + result[1]);
			Assert.AreEqual("[with]", "" + result[2]);
			Assert.AreEqual(" ", "" + result[3]);
			Assert.AreEqual(": some : [other] stuff", "" + result[4]);
			Assert.AreEqual(start, result[0].DefiningPair);
			Assert.AreEqual(null, result[1].DefiningPair);
			Assert.AreEqual(brackets, result[2].DefiningPair);
			Assert.AreEqual(null, result[3].DefiningPair);
			Assert.AreEqual(end, result[4].DefiningPair);
			Assert.AreEqual(1, result.EntriesForDelimeter(start).Count);
			Assert.AreEqual(result[0], result.EntriesForDelimeter(start)[0]);
			Assert.AreEqual(1, result.EntriesForDelimeter(brackets).Count);
			Assert.AreEqual(result[2], result.EntriesForDelimeter(brackets)[0]);
			Assert.AreEqual(1, result.EntriesForDelimeter(end).Count);
			Assert.AreEqual(result[4], result.EntriesForDelimeter(end)[0]);
			Assert.AreEqual(2, result.EntriesForDelimeter(null).Count);
			Assert.AreEqual(result[1], result.EntriesForDelimeter(null)[0]);
			Assert.AreEqual(result[3], result.EntriesForDelimeter(null)[1]);

			data = "This [is ¤ a <:> ] fancy ¤ test ¤ [with] : some : [other] stuff";
			options.NestedInsideDefinition = StringPairManipulation.SplitOptions.NestedInsideDefinitions.WithRespectToParent;
			options.IncludeDelimeters = true;
			options.EnableNestingInDelimeterPairs = false;
			options.EnableDelimeterNestingInNonSplitPairs = false;
			options.EnableNonSplitNestingInNonSplitPairs = true;
			result = StringPairManipulation.Split(data, delimeters, nonsplit, options);
			Assert.AreEqual(5, result.Count);
			Assert.AreEqual("This [is ¤", "" + result[0]);
			Assert.AreEqual(" a <:> ] fancy ¤ test ¤ ", "" + result[1]);
			Assert.AreEqual("[with]", "" + result[2]);
			Assert.AreEqual(" ", "" + result[3]);
			Assert.AreEqual(": some : [other] stuff", "" + result[4]);
			Assert.AreEqual(start, result[0].DefiningPair);
			Assert.AreEqual(null, result[1].DefiningPair);
			Assert.AreEqual(brackets, result[2].DefiningPair);
			Assert.AreEqual(null, result[3].DefiningPair);
			Assert.AreEqual(end, result[4].DefiningPair);
			Assert.AreEqual(1, result.EntriesForDelimeter(start).Count);
			Assert.AreEqual(result[0], result.EntriesForDelimeter(start)[0]);
			Assert.AreEqual(1, result.EntriesForDelimeter(brackets).Count);
			Assert.AreEqual(result[2], result.EntriesForDelimeter(brackets)[0]);
			Assert.AreEqual(1, result.EntriesForDelimeter(end).Count);
			Assert.AreEqual(result[4], result.EntriesForDelimeter(end)[0]);
			Assert.AreEqual(2, result.EntriesForDelimeter(null).Count);
			Assert.AreEqual(result[1], result.EntriesForDelimeter(null)[0]);
			Assert.AreEqual(result[3], result.EntriesForDelimeter(null)[1]);
		}

		[TestMethod]
		public void TestStringPairEscape1()
		{
			string data = null;
			List<char> escapeChars = (new char[] { '[', ']', '=', '\n', '\t'}).ToList<char>();
			List<StringPairManipulation.Pair> noOp = new List<StringPairManipulation.Pair>();
			StringPairManipulation.ManipulateOptions options = new StringPairManipulation.ManipulateOptions();

			noOp.AddRange(StringPairManipulation.DefinePairs(new char[,] { { '<', '>' }, { '"', '"' }, { '\'', '\'' } }));

			data = "Please [escape] this 'with [more] < text' = 'other > data' and [more]";
			options.EnableNoOpNesting = false;
			string result = StringPairManipulation.Escape(data, escapeChars, noOp, options);
			Assert.AreEqual("Please \\[escape\\] this 'with [more] < text' \\= 'other > data' and \\[more\\]", result);

			data = "Please [escape] this 'with [more] < text' = 'other > data' and [more]";
			options.EnableNoOpNesting = true;
			result = StringPairManipulation.Escape(data, escapeChars, noOp, options);
			Assert.AreEqual("Please \\[escape\\] this 'with [more] < text' = 'other > data' and \\[more\\]", result);

			data = "Please [escape] this \n and \t more";
			result = StringPairManipulation.Escape(data, escapeChars, noOp, options);
			Assert.AreEqual("Please \\[escape\\] this \\\n and \\\t more", result);

			data = "Please [escape] this \\\n and \\= more";
			result = StringPairManipulation.Escape(data, escapeChars, noOp, options);
			Assert.AreEqual("Please \\[escape\\] this \\\\\n and \\\\= more", result);

			data = "Please [escape] this \\\n and \\= more \\{ and \\} other \b stuff [\\";
			result = StringPairManipulation.Escape(data, escapeChars, noOp, options);
			Assert.AreEqual("Please \\[escape\\] this \\\\\n and \\\\= more \\{ and \\} other \b stuff \\[\\", result);

			data = "Please [escape] this \\\n and \\= more \\{ and \\} other \b stuff <[\\";
			result = StringPairManipulation.Escape(data, escapeChars, noOp, options);
			Assert.AreEqual("Please \\[escape\\] this \\\\\n and \\\\= more \\{ and \\} other \b stuff <[\\", result);

			try {
				escapeChars = (new char[] { '[', ']', '\\' }).ToList<char>();
				data = "Please [escape] this \\ and \\< this [as] well, and \\[this] too";
				result = StringPairManipulation.Escape(data, escapeChars, noOp, options);
				Assert.Fail();
			}
			catch (ArgumentException)
			{ }
		}

		[TestMethod]
		public void TestStringPairUnEscape1()
		{
			string data = null;
			List<char> escapeChars = (new char[] { '[', ']', '=', '\n', '\t' }).ToList<char>();
			List<StringPairManipulation.Pair> noOp = new List<StringPairManipulation.Pair>();
			StringPairManipulation.ManipulateOptions options = new StringPairManipulation.ManipulateOptions();

			noOp.AddRange(StringPairManipulation.DefinePairs(new char[,] { { '<', '>' }, { '"', '"' }, { '\'', '\'' } }));

			data = "Please [escape] this 'with [more] < text' = 'other > data' and [more]";
			options.EnableNoOpNesting = false;
			string result = StringPairManipulation.Escape(data, escapeChars, noOp, options);
			Assert.AreEqual("Please \\[escape\\] this 'with [more] < text' \\= 'other > data' and \\[more\\]", result);
			Assert.AreEqual(data, StringPairManipulation.Unescape(result, escapeChars, noOp, options));

			data = "Please [escape] this 'with [more] < text' = 'other > data' and [more]";
			options.EnableNoOpNesting = true;
			result = StringPairManipulation.Escape(data, escapeChars, noOp, options);
			Assert.AreEqual("Please \\[escape\\] this 'with [more] < text' = 'other > data' and \\[more\\]", result);
			Assert.AreEqual(data, StringPairManipulation.Unescape(result, escapeChars, noOp, options));

			data = "Please [escape] this \n and \t more";
			result = StringPairManipulation.Escape(data, escapeChars, noOp, options);
			Assert.AreEqual("Please \\[escape\\] this \\\n and \\\t more", result);
			Assert.AreEqual(data, StringPairManipulation.Unescape(result, escapeChars, noOp, options));

			data = "Please [escape] this \\\n and \\= more";
			result = StringPairManipulation.Escape(data, escapeChars, noOp, options);
			Assert.AreEqual("Please \\[escape\\] this \\\\\n and \\\\= more", result);
			Assert.AreEqual(data, StringPairManipulation.Unescape(result, escapeChars, noOp, options));

			data = "Please [escape] this \\\n and \\= more \\{ and \\} other \b stuff [\\";
			result = StringPairManipulation.Escape(data, escapeChars, noOp, options);
			Assert.AreEqual("Please \\[escape\\] this \\\\\n and \\\\= more \\{ and \\} other \b stuff \\[\\", result);
			Assert.AreEqual(data, StringPairManipulation.Unescape(result, escapeChars, noOp, options));

			data = "Please [escape] this \\\n and \\= more \\{ and \\} other \b stuff <[\\";
			result = StringPairManipulation.Escape(data, escapeChars, noOp, options);
			Assert.AreEqual("Please \\[escape\\] this \\\\\n and \\\\= more \\{ and \\} other \b stuff <[\\", result);
			Assert.AreEqual(data, StringPairManipulation.Unescape(result, escapeChars, noOp, options));

			result = "Please \\unescape\\] this \\ and <some\\] more>";
			Assert.AreEqual("Please \\unescape] this \\ and <some\\] more>", StringPairManipulation.Unescape(result, escapeChars, noOp, options));

			try
			{
				escapeChars = (new char[] { '[', ']', '\\' }).ToList<char>();
				data = "Please [unescape] this \\ and \\< this [as] well, and \\[this] too";
				StringPairManipulation.Unescape(data, escapeChars, noOp, options);
				Assert.Fail();
			}
			catch (ArgumentException)
			{ }
		}
	}
}
