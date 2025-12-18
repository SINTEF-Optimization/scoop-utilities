//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sintef.Scoop.Utilities
{
	// see comments in StringPairManipulation.cs
	public partial class StringPairManipulation
	{
		#region public classes

		/// <summary>
		/// the options for a split
		/// </summary>
		public class SplitOptions
		{
			#region output style

			/// <summary>
			/// Whether to include the delimeters in the output or not
			/// Default: true
			/// </summary>
			public bool IncludeDelimeters { get; set; }

			#endregion

			#region nesting 

			/// <summary>
			/// Ways of defining "inside" in the nesting rules
			/// </summary>
			public enum NestedInsideDefinitions
			{
				/// <summary>
				/// Whether nesting is allowed or not depends only on the closest pair (the "parent" pair for a new pair)
				/// </summary>
				WithRespectToParent,

				/// <summary>
				/// Whether nesting is allowed or not considers whether any pair of that type is currently in the hierarchy
				/// </summary>
				Total,
			};

			/// <summary>
			/// What is the "inside" definition for the nesting rules
			/// Default: Total
			/// </summary>
			public NestedInsideDefinitions NestedInsideDefinition { get; set; }

			/// <summary>
			/// Inside a delimeter pair, is further nesting allowed or not
			/// Default: true
			/// </summary>
			public bool EnableNestingInDelimeterPairs { get; set; }

			/// <summary>
			/// Inside a non-split defining pair, is nesting with a delimeter pair allowed
			/// Default: false
			/// </summary>
			public bool EnableDelimeterNestingInNonSplitPairs { get; set; }

			/// <summary>
			/// Inside a non-split defining pair, is nesting with a non-split pair allowed
			/// Default: true
			/// </summary>
			public bool EnableNonSplitNestingInNonSplitPairs { get; set; }

			#endregion

			/// <summary>
			/// Defines the default options
			/// </summary>
			public SplitOptions()
			{
				NestedInsideDefinition = NestedInsideDefinitions.Total;
				IncludeDelimeters = true;
				EnableNestingInDelimeterPairs = true;
				EnableNonSplitNestingInNonSplitPairs = true;
				EnableDelimeterNestingInNonSplitPairs = false;
			}
		}

		/// <summary>
		/// Contains a single element after the split
		/// The ToString method will return that string-element
		/// 
		/// Immutable
		/// </summary>
		public class SingleStringPairSplitString
		{
			/// <summary>
			/// The bounding pair that defined the element, or null if it is not defined by an bounding pair
			/// </summary>
			public Pair DefiningPair { get; private set; }

			/// <summary>
			/// The part of the string split off, defining the element
			/// </summary>
			public string String { get; private set; }

			/// <summary>
			/// returns same as String
			/// </summary>
			public override string ToString()
			{
				return String;
			}

			/// <summary>
			/// construction, simply defines DefiningPair and String
			/// </summary>
			public SingleStringPairSplitString(string value, Pair definingPair)
			{
				String = value;
				DefiningPair = definingPair;
			}
		}

		/// <summary>
		/// Interface representing the result of a split, represents the single elements in correct order
		/// </summary>
		public interface IReadOnlyStringPairSplitResult : IReadOnlyList<SingleStringPairSplitString>
		{
			/// <summary>
			/// the original data string that was split
			/// </summary>
			string Original { get; }

			/// <summary>
			/// The options used for the split
			/// </summary>
			SplitOptions Options { get; }

			/// <summary>
			/// the delimeter pairs that contributed to the split
			/// </summary>
			IEnumerable<Pair> DelimeterPairs { get; }

			/// <summary>
			/// the non-split pairs that contributed to the split
			/// </summary>
			IEnumerable<Pair> NonSplitPairs { get; }

			/// <summary>
			/// Returns the elements that the given delimeter pair defined, in order of appearance
			/// If pair is null, than the elements not defined by any delimeter pair are returned, in order of appearance
			/// </summary>
			IReadOnlyList<SingleStringPairSplitString> EntriesForDelimeter(Pair pair);
		}

		/// <summary>
		/// The result of a split
		/// </summary>
		private class StringPairSplitResult : IReadOnlyStringPairSplitResult
		{
			/// <summary>
			/// The entries after the split in correct order
			/// </summary>
			private List<SingleStringPairSplitString> _entries;

			/// <summary>
			/// Dictionary yielding for a delimeter pair the found elements defined by that pair, in order of appearance
			/// </summary>
			private Dictionary<Pair, List<SingleStringPairSplitString>> _entriesPerPair;
			/// <summary>
			/// List of elements not defined by a delimeter pair, in order of appearance
			/// </summary>
			private List<SingleStringPairSplitString> _entriesNotDefinedByAPair;

			/// <summary>
			/// the original data string that was split
			/// </summary>
			public string Original { get; private set; }

			/// <summary>
			/// The options used for the split
			/// </summary>
			public SplitOptions Options { get; private set; }

			/// <summary>
			/// the delimeter pairs that contributed to the split
			/// </summary>
			public IEnumerable<Pair> DelimeterPairs { get; private set; }

			/// <summary>
			/// the non-split pairs that contributed to the split
			/// </summary>
			public IEnumerable<Pair> NonSplitPairs { get; private set; }

			/// <summary>
			/// Access to the n-th element in the result
			/// </summary>
			/// <param name="index">Index of element to access</param>
			/// <returns>the element at given index</returns>
			public SingleStringPairSplitString this[int index]
			{
				get
				{
					return _entries[index];
				}
			}

			/// <summary>
			/// Number of elements in the result
			/// </summary>
			public int Count
			{
				get
				{
					return _entries.Count;
				}
			}

			/// <summary>
			/// enumeration through elements
			/// </summary>
			public IEnumerator<SingleStringPairSplitString> GetEnumerator()
			{
				return _entries.GetEnumerator();
			}

			/// <summary>
			/// enumeration through elements
			/// </summary>
			IEnumerator IEnumerable.GetEnumerator()
			{
				return _entries.GetEnumerator();
			}

			/// <summary>
			/// Returns the elements that the given delimeter pair defined, in order of appearance
			/// If pair is null, than the elements not defined by any delimeter pair are returned, in order of appearance
			/// </summary>
			public IReadOnlyList<SingleStringPairSplitString> EntriesForDelimeter(Pair pair)
			{
				if (pair == null)
					return _entriesNotDefinedByAPair;
				if (_entriesPerPair.ContainsKey(pair))
					return _entriesPerPair[pair];
				return new List<SingleStringPairSplitString>();
			}

			/// <summary>
			/// construction
			/// </summary>
			public StringPairSplitResult(string original,
				IEnumerable<Pair> delimeterPairs, IEnumerable<Pair> nonSplitPairs, SplitOptions options)
			{
				Original = original;
				_entries = new List<SingleStringPairSplitString>();
				_entriesPerPair = new Dictionary<Pair, List<SingleStringPairSplitString>>();
				_entriesNotDefinedByAPair = new List<SingleStringPairSplitString>();
				DelimeterPairs = delimeterPairs;
				NonSplitPairs = nonSplitPairs;
				Options = options;
			}

			/// <summary>
			/// Add an element
			/// </summary>
			public void Add(string value, Pair definingPair)
			{
				Add(new SingleStringPairSplitString(value, definingPair));
			}

			/// <summary>
			/// Add an element
			/// </summary>
			public void Add(SingleStringPairSplitString entry)
			{
				_entries.Add(entry);
				if (entry.DefiningPair == null)
				{
					_entriesNotDefinedByAPair.Add(entry);
				}
				else
				{
					if (!_entriesPerPair.ContainsKey(entry.DefiningPair))
					{
						_entriesPerPair[entry.DefiningPair] = new List<SingleStringPairSplitString>();
					}
					_entriesPerPair[entry.DefiningPair].Add(entry);
				}
			}
		}

		#endregion

		#region private classes and enums

		/// <summary>
		/// A pair with additional information updated and needed during the splitting
		/// Represents the specific occurance of a pair in the string
		/// 
		/// Immutable
		/// </summary>
		private class ExtendedPair
		{
			/// <summary>
			/// The pair that occurs
			/// </summary>
			public Pair Pair { get; private set; }

			/// <summary>
			/// Is it a delimeter pair (true) or a non-split pair (false)
			/// </summary>
			public bool IsDelimeter { get; private set; }

			/// <summary>
			/// Does it occur somewhere within a delimeter pair or 
			/// starts a delimeter area itself
			/// </summary>
			public bool StartsOrIsInDelimeter { get; private set; }

			/// <summary>
			/// Does it occur somewhere within a non-split pair or starts
			/// a non-split area itself
			/// </summary>
			public bool StartsOrIsInNonSplit { get; private set; }

			/// <summary>
			/// construction
			/// </summary>
			/// <param name="pair">which pair occurs</param>
			/// <param name="delimeter">Is it a delimeter pair?</param>
			/// <param name="topPair">The pair at the top of the active pairs stack at the time the pair starts</param>
			public ExtendedPair(Pair pair, bool delimeter, ExtendedPair topPair)
			{
				Pair = pair;
				IsDelimeter = delimeter;
				StartsOrIsInDelimeter = delimeter || (topPair == null ? false : topPair.StartsOrIsInDelimeter);
				StartsOrIsInNonSplit = !delimeter || (topPair == null ? false : topPair.StartsOrIsInNonSplit);
			}
		}

		#endregion

		#region the main public split method

		/// <summary>
		/// Splits a given string at defined delimeters
		/// 
		/// The split considers not just single delimeters as the split in c# string, but it considers delimeter pairs and 
		/// allows and respects nesting of such pairs. Of course single delimeters can also be specified. In addition, the
		/// split respects non-split areas, which are areas defined by given characters, where no splitting will occur, independend
		/// of whether delimeters occur in that area or not.
		/// </summary>
		/// <param name="data">The string to split</param>
		/// <param name="delimeterPairs">The pairs defining the delimeters, hence the splits</param>
		/// <param name="nonsplittablePairs">The pairs defining areas where no splitting shall occur. If null, no such pairs are specified.</param>
		/// <param name="options">Options for configuring how the split functino shall behave. If not specified, default options are used.</param>
		/// <returns>the result of the split</returns>
		public static IReadOnlyStringPairSplitResult
			Split(string data, IEnumerable<Pair> delimeterPairs, IEnumerable<Pair> nonsplittablePairs = null, SplitOptions options = null)
		{
			if (options == null)
				options = new SplitOptions();

			StringPairSplitResult result = new StringPairSplitResult(data, delimeterPairs, nonsplittablePairs, options);
			if (data == null)
				return result;

			Stack<ExtendedPair> activePairs = new Stack<ExtendedPair>();
			StringBuilder builder = new StringBuilder();
			EscapeStates escapeState = EscapeStates.Unescaped;

			Split_ParseNextUnescapedCharacter(new ExtendedChar(ExtendedChar.CharacterTypes.StartOfString), delimeterPairs, nonsplittablePairs, options, result, activePairs, builder);
			for (int i = 0; i < data.Length; ++i)
			{
				char c = data[i];
				switch (escapeState)
				{
					case EscapeStates.Escaped:
						builder.Append(c);
						escapeState = EscapeStates.Unescaped;
						break;

					case EscapeStates.Unescaped:
					default:
						if (c == '\\')
						{
							builder.Append(c);
							escapeState = EscapeStates.Escaped;
						}
						else
						{
							Split_ParseNextUnescapedCharacter(new ExtendedChar(c), delimeterPairs, nonsplittablePairs, options, result, activePairs, builder);
						}
						break;
				}
			}
			Split_ParseNextUnescapedCharacter(new ExtendedChar(ExtendedChar.CharacterTypes.EndOfString), delimeterPairs, nonsplittablePairs, options, result, activePairs, builder);

			if (builder.Length > 0)
			{
				Pair definingPair = null;
				while (activePairs.Count > 1)
					activePairs.Pop();
				if (activePairs.Count == 1)
				{
					ExtendedPair basePair = activePairs.Pop();
					if (basePair.IsDelimeter)
						definingPair = basePair.Pair;
				}
				result.Add(builder.ToString(), definingPair);
			}
			return result;
		}

		#endregion

		#region public convenience methods and classes

		/// <summary>
		/// returns a standard set of pairs describing non split areas
		/// Pairs are started and ended by same characters. Characters defining non-split are:
		/// '"', '\''
		/// </summary>
		public static IEnumerable<Pair> StandardNonSplitPairs
		{
			get { return DefineStartEndPairs(new char[] { '"', '\'' }); }
		}

		/// <summary>
		/// Defines pairs with only start specified. One pair for each char in collecion
		/// </summary>
		public static IEnumerable<Pair> DefineSinglePairs(char[] c)
		{
			return DefineSinglePairs(c.AsEnumerable<char>());
		}

		/// <summary>
		/// Defines pairs with only start specified. One pair for each char in collecion
		/// </summary>
		public static IEnumerable<Pair> DefineSinglePairs(IEnumerable<char> c)
		{
			List<Pair> pairs = new List<Pair>();
			foreach (char x in c)
				pairs.Add(new Pair(x));
			return pairs;
		}

		/// <summary>
		/// Defines pairs with only start and end the same character. One pair for each char in collecion
		/// </summary>
		public static IEnumerable<Pair> DefineStartEndPairs(char[] c)
		{
			return DefineStartEndPairs(c.AsEnumerable<char>());
		}

		/// <summary>
		/// Defines pairs with only start and end the same character. One pair for each char in collecion
		/// </summary>
		public static IEnumerable<Pair> DefineStartEndPairs(IEnumerable<char> c)
		{
			List<Pair> pairs = new List<Pair>();
			foreach (char x in c)
				pairs.Add(new Pair(x, x));
			return pairs;
		}

		/// <summary>
		/// If second dimension is 1, then defines sinlge pairs where end is not specified.
		/// If second dimension is 2 or higher, defines start end pairs where c[,0] is start and c[,1] is end
		/// One pair for each entry in first dimension
		/// </summary>
		public static IEnumerable<Pair> DefinePairs(char[,] c)
		{
			List<Pair> pairs = new List<Pair>();
			bool haveEndChars = c.GetLength(1) > 1;
			for (int i = 0; i < c.GetLength(0); ++i)
			{
				if (haveEndChars)
					pairs.Add(new Pair(c[i, 0], c[i, 1]));
				else
					pairs.Add(new Pair(c[i, 0]));
			}
			return pairs;
		}

		/// <summary>
		/// If second dimension is 1, then defines sinlge pairs where end is not specified.
		/// If second dimension is 2 or higher, defines start end pairs where c[,0] is start and c[,1] is end and both have value,
		/// and a single pair if c[,1] does not have a value.
		/// One pair for each entry in first dimension where c[,0] has value
		/// </summary>
		public static IEnumerable<Pair> DefinePairs(char?[,] c)
		{
			List<Pair> pairs = new List<Pair>();
			bool haveEndChars = c.GetLength(1) > 1;
			for (int i = 0; i < c.GetLength(0); ++i)
			{
				if (!c[i, 0].HasValue)
					continue;
				if (haveEndChars && c[i, 1].HasValue)
					pairs.Add(new Pair(c[i, 0].Value, c[i, 1].Value));
				else
					pairs.Add(new Pair(c[i, 0].Value));
			}
			return pairs;
		}

		/// <summary>
		/// If second dimension is 1, then defines sinlge pairs where end is not specified.
		/// If second dimension is 2 or higher, defines pairs where c[,0] is start and c[,1] is end 
		/// One pair for each entry in first dimension where c[,0] is not null
		/// </summary>
		public static IEnumerable<Pair> DefinePairs(ExtendedChar[,] c)
		{
			List<Pair> pairs = new List<Pair>();
			bool haveEndChars = c.GetLength(1) > 1;
			for (int i = 0; i < c.GetLength(0); ++i)
			{
				if (c[i, 0] == null)
					continue;
				if (haveEndChars)
					pairs.Add(new Pair(c[i, 0], c[i, 1]));
				else
					pairs.Add(new Pair(c[i, 0]));
			}
			return pairs;
		}

		#endregion

		#region private helper methods

		/// <summary>
		/// Parses one unescaped character
		/// 
		/// Basically does all the pair related parsing, whereas the escape consideration is taken by the calling function
		/// 
		/// Updates builder, result and active pairs accordingly
		/// </summary>
		/// <param name="c">The character to pars</param>
		/// <param name="delimeterPairs">The pairs defining the delimeters</param>
		/// <param name="nonsplittablePairs">Pairs defining non-split areas</param>
		/// <param name="options">Options for the splitting</param>
		/// <param name="result">The result of the split, is updated by this function</param>
		/// <param name="activePairs">The currently active pairs, is updated by this function</param>
		/// <param name="builder">The string builder for the current element, updated by this function</param>
		private static void Split_ParseNextUnescapedCharacter(ExtendedChar c,
			IEnumerable<Pair> delimeterPairs, IEnumerable<Pair> nonsplittablePairs, SplitOptions options,
			StringPairSplitResult result, Stack<ExtendedPair> activePairs, StringBuilder builder)
		{
			ExtendedPair topPair = null;
			if (activePairs.Count > 0)
				topPair = activePairs.Peek();

			// --------------------------
			// end current pair?
			// --------------------------
			if (topPair != null && c == topPair.Pair.End)
			{
				// finished a split?
				if (activePairs.Count == 1 && topPair.IsDelimeter)
				{
					if (options.IncludeDelimeters)
						builder.Append(c);

					result.Add(builder.ToString(), topPair.Pair);
					builder.Clear();
				}
				else
				{
					builder.Append(c);
				}

				activePairs.Pop();
				return;
			}

			// --------------------------
			// nesting not allowed?
			// --------------------------
			if (topPair != null)
			{
				switch (options.NestedInsideDefinition)
				{
					case SplitOptions.NestedInsideDefinitions.Total:
						{
							if ((topPair.StartsOrIsInDelimeter && !options.EnableNestingInDelimeterPairs) ||
								(topPair.StartsOrIsInNonSplit && !options.EnableDelimeterNestingInNonSplitPairs && !options.EnableNonSplitNestingInNonSplitPairs))
							{
								// add c
								builder.Append(c);
								return;
							}
						}
						break;

					case SplitOptions.NestedInsideDefinitions.WithRespectToParent:
					default:
						{
							if ((topPair.IsDelimeter && !options.EnableNestingInDelimeterPairs) ||
								(!topPair.IsDelimeter && !options.EnableDelimeterNestingInNonSplitPairs && !options.EnableNonSplitNestingInNonSplitPairs))
							{
								// add c
								builder.Append(c);
								return;
							}
						}
						break;

				}
			}

			// --------------------------
			// start a new pair?
			// --------------------------
			ExtendedPair startedPair = null;
			foreach (Pair p in nonsplittablePairs ?? Enumerable.Empty<Pair>())
			{
				if (c == p.Start)
				{
					startedPair = new ExtendedPair(p, false, topPair);
					break;
				}
			}
			if (startedPair == null)
			{
				foreach (Pair p in delimeterPairs ?? Enumerable.Empty<Pair>())
				{
					if (c == p.Start)
					{
						startedPair = new ExtendedPair(p, true, topPair);
						break;
					}
				}
			}
			if (startedPair != null)
			{
				// nesting with this pair not allowed
				// - - - - - - - - - - - - - - - - - - - - - -  
				if (topPair != null)
				{
					switch (options.NestedInsideDefinition)
					{
						case SplitOptions.NestedInsideDefinitions.Total:
							if (topPair.StartsOrIsInNonSplit)
							{
								if ((startedPair.IsDelimeter && !options.EnableDelimeterNestingInNonSplitPairs) ||
									(!startedPair.IsDelimeter && !options.EnableNonSplitNestingInNonSplitPairs))
								{
									// add c
									builder.Append(c);
									return;
								}
							}
							break;
						case SplitOptions.NestedInsideDefinitions.WithRespectToParent:
						default:
							if (!topPair.IsDelimeter)
							{
								if ((startedPair.IsDelimeter && !options.EnableDelimeterNestingInNonSplitPairs) ||
									(!startedPair.IsDelimeter && !options.EnableNonSplitNestingInNonSplitPairs))
								{
									// add c
									builder.Append(c);
									return;
								}
							}
							break;
					}
				}

				// just a simple delimeter, no pair?
				bool justASingleDelimeter = startedPair.IsDelimeter && startedPair.Pair.End == null;

				// first pair
				// - - - - - - - - - - 
				if (topPair == null)
				{
					// have a new split?
					if (startedPair.IsDelimeter)
					{
						if (builder.Length > 0)
							result.Add(builder.ToString(), null);
						builder.Clear();

						if (justASingleDelimeter)
						{
							if (options.IncludeDelimeters)
								result.Add("" + c, null);
						}
						else
						{
							if (options.IncludeDelimeters)
								builder.Append(c);
							activePairs.Push(startedPair);
						}

						return;
					}

					// new first pair that is not a delimeter
					builder.Append(c);
					activePairs.Push(startedPair);
					return;
				}

				// already inside a pair
				// - - - - - - - - - - 
				builder.Append(c);
				if (!justASingleDelimeter)
					activePairs.Push(startedPair);
				return;
			}

			// --------------------------
			// just a plain character
			// --------------------------
			builder.Append(c);
			return;
		}

		#endregion
	}
}