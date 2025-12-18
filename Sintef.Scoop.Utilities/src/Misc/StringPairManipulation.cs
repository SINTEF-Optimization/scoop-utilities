//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Class containing string manipulation methods. The special about the class is the pair concept
	/// </summary>
	/// <remarks>
	/// The pair concept i central in this class. A pair consists of
	/// two characters, one defining the start and the other defining the end of the pair. In this way a pair defines an area
	/// within a string. If a pair does not have an end specified (end is null), than we have a classical single character delimeter.
	/// 
	/// Pairs can be nested, if the start and end characters are different. Consider for example the '[' character as start and
	/// the ']' as end. Then we can have nesting like 
	/// "In this example the [first pair contains a [subpair] in itself] and more text."
	/// The whole pair concept is quite powerful, but also leads to questions of what is a desireable behaviour. Consider both
	/// curly and square brackets as pairs. In the string
	/// "This is [a {difficult] [example} with] some} more] text."
	/// there are several ways of defining areas specified by pairs. For example one can deny nesting, 
	/// leading to the ares "[a {difficult]", "[example} with]" defined by the square brackets. Or we allow nesting and we get
	/// "[a {difficult] [example} with] some} more]" as a single area, with the subarea "{difficult] [example} with] some}" and
	/// the subsubarea "[example} with]". The options for each method give the user the ability to define how the pair concept 
	/// is applied and used.
	/// </remarks>
	public partial class StringPairManipulation
	{
		#region public classes

		/// <summary>
		/// Class represents an extended character, that can in addition to the "normal" characters also represent
		/// some specials like start and end of string.
		/// 
		/// The special 'characters' are defined by an enum
		/// 
		/// Immutable
		/// </summary>
		public class ExtendedChar
		{
			#region public enums and properties

			/// <summary>
			/// the types of extended characters
			/// </summary>
			public enum CharacterTypes
			{
				/// <summary>
				/// A simple, classical character, the value of the character is stored in the Character property
				/// </summary>
				Character,

				/// <summary>
				/// ExtendedChar represents the start of a string. Value of Character is uninteresting and undefined
				/// </summary>
				StartOfString,

				/// <summary>
				/// ExtendedChar represents the end of a string. Value of Character is uninteresting and undefined
				/// </summary>
				EndOfString
			}

			/// <summary>
			/// The 'classical character' ExtendedChar represents in case of Type == Character
			/// </summary>
			public char Character { get; private set; }

			/// <summary>
			/// The type of ExtendedChar this ExtendedChar represents
			/// </summary>
			public CharacterTypes Type { get; private set; }

			#endregion

			#region construction

			/// <summary>
			/// Defines an ExtendedChar representing the given 'classical' char
			/// </summary>
			public ExtendedChar(char c)
			{
				Type = CharacterTypes.Character;
				Character = c;
			}

			/// <summary>
			/// creates an extended char representing either the given type or character c if type is 'Character'
			/// </summary>
			public ExtendedChar(CharacterTypes type, char c = ' ')
			{
				Type = type;
				Character = c;
			}

			#endregion

			/// <summary>
			/// Overwriting the ToString method to either return the Character or an empty string
			/// </summary>
			public override string ToString()
			{
				if (Type == CharacterTypes.Character)
					return Character.ToString();
				else
					return string.Empty;
			}

			#region overwriting equal and == and corresponding operators as ExtendedChar is immutable

			/// <inheritdoc/>	
			public static bool operator ==(ExtendedChar a, ExtendedChar b)
			{
				if (ReferenceEquals(a, null))
					return (ReferenceEquals(b, null));
				if (ReferenceEquals(b, null))
					return false;

				if (a.Type != b.Type)
					return false;
				if (a.Type == CharacterTypes.Character)
					return a.Character == b.Character;
				return true;
			}

			/// <inheritdoc/>	
			public static bool operator !=(ExtendedChar a, ExtendedChar b)
			{
				return !(a == b);
			}

			/// <inheritdoc/>	
			public override bool Equals(object obj)
			{
				if (!(obj is ExtendedChar))
					return base.Equals(obj);
				return (obj as ExtendedChar) == this;
			}

			/// <inheritdoc/>	
			public override int GetHashCode()
			{
				if (Type == CharacterTypes.Character)
					return Character.GetHashCode();
				return Type.GetHashCode();
			}

			#endregion
		}

		/// <summary>
		/// A character pair, defining an area in a string
		/// 
		/// Immutable
		/// </summary>
		public class Pair
		{
			#region start and end definition

			/// <summary>
			/// The character starting the area
			/// </summary>
			public ExtendedChar Start { get; private set; }

			/// <summary>
			/// the character ending the area, can be null to specify more classic delimeter instead of area
			/// </summary>
			public ExtendedChar End { get; private set; }

			#endregion

			#region construction

			/// <summary>
			/// constructor for a classic delimeter character
			/// </summary>
			public Pair(char delimeter)
			{
				Start = new ExtendedChar(delimeter);
				End = null;
			}

			/// <summary>
			/// constructor for an area defining character pair
			/// </summary>
			public Pair(char start, char end)
			{
				Start = new ExtendedChar(start);
				End = new ExtendedChar(end);
			}

			/// <summary>
			/// constructor for a delimeter character with no end defined
			/// </summary>
			public Pair(ExtendedChar start)
			{
				Start = start;
				End = null;
			}

			/// <summary>
			/// constructor for an area defining character pair
			/// </summary>
			public Pair(ExtendedChar start, ExtendedChar end)
			{
				Start = start;
				End = end;
			}

			#endregion

			#region overwriting equal and == and corresponding operators as Pair is immutable

			/// <inheritdoc/>	
			public static bool operator ==(Pair a, Pair b)
			{
				if (ReferenceEquals(a, null))
					return (ReferenceEquals(b, null));
				if (ReferenceEquals(b, null))
					return false;

				if (a.Start != b.Start)
					return false;
				if (a.End != b.End)
					return false;
				return true;
			}

			/// <inheritdoc/>	
			public static bool operator !=(Pair a, Pair b)
			{
				return !(a == b);
			}

			/// <inheritdoc/>	
			public override bool Equals(object obj)
			{
				if (!(obj is Pair))
					return base.Equals(obj);
				return (obj as Pair) == this;
			}

			/// <inheritdoc/>	
			public override int GetHashCode()
			{
				int hashCode = 0;
				if (Start != null)
					hashCode = Start.GetHashCode();
				if (End != null)
					hashCode = hashCode ^ End.GetHashCode();
				return hashCode;
			}
			#endregion
		}

		/// <summary>
		/// Options for manipulating strings
		/// </summary>
		public class ManipulateOptions
		{
			/// <summary>
			/// Whether no-operation pairs can be nested or not
			/// 
			/// Default: false
			/// </summary>
			public bool EnableNoOpNesting { get; set; }

			/// <summary>
			/// Creates ManipulateOptions with default setup
			/// </summary>
			public ManipulateOptions()
			{
				EnableNoOpNesting = false;
			}
		}

		#endregion

		#region private classes and enums

		/// <summary>
		/// States of the escape state machine
		/// </summary>
		private enum EscapeStates {
			Unescaped, // next character is not escaped
			Escaped, // next character is escaped
		};

		/// <summary>
		/// State object for Unescape Method
		/// </summary>
		private class UnescapeMethod_CallerState
		{
			/// <summary>
			/// number of relevant '\' occured after each other so far
			/// </summary>
			public int EscapeCount;

			/// <summary>
			/// initializes state with zero
			/// </summary>
			public UnescapeMethod_CallerState()
			{
				EscapeCount = 0;
			}
		}

		#endregion

		#region public methods

		/// <summary>
		/// Escapes the specified characters in the given string, avoiding no-operation areas defined by noOpPairs.
		/// </summary>
		/// <remarks>
		/// The Escape and the Unescape methods are designed as counterparts. That means that the following is (should) be true:
		///       data = Unescape(Escape(data, e,n,o), e,n,o)
		/// with e being the characters to escape/unescape, n the noOpPairs, and o the options used. Note that those need to be
		/// identical for both methods for the above to be true.
		/// </remarks>
		/// <param name="data">The string where the characters shall be escaped</param>
		/// <param name="escapeThese">The characters to escape</param>
		/// <param name="noOpPairs">The pairs defining areas where no escaping shall occur. If null, no such pairs are specified.</param>
		/// <param name="options">Options for configuring how the function shall behave. If not specified, default options are used.</param>
		/// <returns>the escaped string</returns>
		public static string
			Escape(string data, IEnumerable<char> escapeThese, IEnumerable<Pair> noOpPairs=null, ManipulateOptions options=null)
		{
			if (escapeThese.Contains('\\'))
				throw new ArgumentException("can not escape the escape character'");

			Action<ExtendedChar, StringBuilder, object> unescapedAction = (c,b,o) =>
			{
				if (c.Type == ExtendedChar.CharacterTypes.Character)
				{
					if (escapeThese.Contains(c.Character))
					{
						b.Append("\\" + c.Character);
					}
					else
					{
						b.Append(c.Character);
					}
				}
			};

			Func<ExtendedChar, StringBuilder, object, EscapeStates> escapedAction = (c,b,o) =>
			{
				if (c.Type == ExtendedChar.CharacterTypes.Character)
				{
					if (escapeThese.Contains(c.Character))
					{
						b.Append("\\\\" + c.Character);
					}
					else
					{
						b.Append("\\" + c.Character);
					}
				}
				else
					b.Append("\\");
				return EscapeStates.Unescaped;
			};

			return Manipulate<object>(data, unescapedAction, escapedAction, noOpPairs, options);
		}

		/// <summary>
		/// Unescapes the specified characters in the given string, avoiding no-operation areas defined by noOpPairs.
		/// </summary>
		/// <remarks>
		/// The Escape and the Unescape methods are designed as counterparts. That means that the following is (should) be true:
		///       data = Unescape(Escape(data, e,n,o), e,n,o)
		/// with e being the characters to escape/unescape, n the noOpPairs, and o the options used. Note that those need to be
		/// identical for both methods for the above to be true.
		/// </remarks>
		/// <param name="data">The string where the characters shall be escaped</param>
		/// <param name="unescapeThese">The characters to unescape</param>
		/// <param name="noOpPairs">The pairs defining areas where no unescaping shall occur. If null, no such pairs are specified.</param>
		/// <param name="options">Options for configuring how the function shall behave. If not specified, default options are used.</param>
		/// <returns>the unescaped string</returns>
		public static string
			Unescape(string data, IEnumerable<char> unescapeThese, IEnumerable<Pair> noOpPairs=null, ManipulateOptions options=null)
		{
			if (unescapeThese.Contains('\\'))
				throw new ArgumentException("can not unescape the escape character'");

			Action<ExtendedChar, StringBuilder, UnescapeMethod_CallerState> unescapedAction = (c, b, s) =>
			{
				b.Append(c);
			};

			Func<ExtendedChar, StringBuilder, UnescapeMethod_CallerState, EscapeStates> escapedAction = (c, b, s) =>
			{
				++s.EscapeCount;

				// we got another \
				if (c.Type == ExtendedChar.CharacterTypes.Character && c.Character == '\\')
					return EscapeStates.Escaped;
			
				if (c.Type == ExtendedChar.CharacterTypes.Character && unescapeThese.Contains(c.Character))
					--s.EscapeCount;

				for (int i = 0; i < s.EscapeCount; ++i)
					b.Append("\\");
				b.Append(c);
				s.EscapeCount = 0;
				return EscapeStates.Unescaped;
			};

			return Manipulate<UnescapeMethod_CallerState>(data, unescapedAction, escapedAction, noOpPairs, options);
		}

		#endregion

		#region private generic manipulate function

		/// <summary>
		/// A generic string manipulation function respecting escaped characters and no-op areas
		/// </summary>
		/// <typeparam name="T">Type of status object for unescapedAction and escapedAction</typeparam>
		/// <param name="data">string to manipulate on/from</param>
		/// <param name="unescapedAction">Action applied when an unescaped character is encountered. The function
		/// gets the character encountered, the string builder building the result and the current state as input.</param>
		/// <param name="escapedAction">Action applied when an escaped character is encountered. The function
		/// gets the character encountered, the string builder building the result and the current state as input.
		/// The function also has to determine the escape-state used afterwards (needs to return that).</param>
		/// <param name="noOpPairs">pairs defining no-op zones, if null no such zones are defined</param>
		/// <param name="options">options to be used, if null defaults are used</param>
		/// <returns>The string derived from input data</returns>
		private static string
			Manipulate<T>(string data, Action<ExtendedChar, StringBuilder, T> unescapedAction, 
			    Func<ExtendedChar, StringBuilder, T, EscapeStates> escapedAction,
				IEnumerable<Pair> noOpPairs = null, ManipulateOptions options=null) where T : class, new()
		{
			if (data == null)
				return null;

			if (options == null)
				options = new ManipulateOptions();

			StringBuilder result = new StringBuilder();
			T callerState = new T();

			Stack<ExtendedPair> activePairs = new Stack<ExtendedPair>();
			EscapeStates escapeState = EscapeStates.Unescaped;

			Manipulate_ParseNextUnescapedCharacter(new ExtendedChar(ExtendedChar.CharacterTypes.StartOfString), unescapedAction, noOpPairs, options, result, activePairs, callerState);
			for (int i = 0; i <= data.Length; ++i)
			{
				ExtendedChar c = new ExtendedChar(ExtendedChar.CharacterTypes.EndOfString);
				if (i < data.Length)
					c = new ExtendedChar(data[i]);
				switch (escapeState)
				{
					case EscapeStates.Escaped:
						// in no-op zone
						if (activePairs.Count > 0)
						{
							result.Append("\\" + c);
							escapeState = EscapeStates.Unescaped;
						}
						// in do op zone
						else
						{
							escapeState = escapedAction(c, result, callerState);
						}
						break;

					case EscapeStates.Unescaped:
					default:
						if (c.Type == ExtendedChar.CharacterTypes.Character && c.Character == '\\')
						{
							escapeState = EscapeStates.Escaped;
						}
						else
						{
							Manipulate_ParseNextUnescapedCharacter(c, unescapedAction, noOpPairs, options, result, activePairs, callerState);
						}
						break;
				}
			}
			return result.ToString();
		}

		/// <summary>
		/// A helper function for the generic string manipulation function 
		/// </summary>
		/// <typeparam name="T">Type of status object for unescapedAction and escapedAction</typeparam>
		/// <param name="c">current unescaped character</param>
		/// <param name="unescapedAction">Action applied when an unescaped character is encountered. The function
		/// gets the character encountered, the string builder building the result and the current state as input.</param>
		/// <param name="noOpPairs">pairs defining no-op zones, if null no such zones are defined</param>
		/// <param name="options">options to be used, if null defaults are used</param>
		/// <param name="result">String builder building resulting string</param>
		/// <param name="activePairs">active no-op pairs at this moment</param>
		/// <param name="callerState">state provided to escape und unescape actions</param>
		private static void Manipulate_ParseNextUnescapedCharacter<T>(ExtendedChar c,
			Action<ExtendedChar, StringBuilder, T> unescapedAction,
			IEnumerable<Pair> noOpPairs, ManipulateOptions options,
			StringBuilder result, Stack<ExtendedPair> activePairs,
			T callerState) where T : class
		{
			ExtendedPair topPair = null;
			if (activePairs.Count > 0)
				topPair = activePairs.Peek();

			// --------------------------
			// end current no-op pair?
			// --------------------------
			if (topPair != null && c == topPair.Pair.End)
			{
				result.Append(c);
				activePairs.Pop();
				return;
			}

			// --------------------------
			// nesting not allowed?
			// --------------------------
			if (topPair != null)
			{
				if (!options.EnableNoOpNesting)
				{
					result.Append(c);
					return;
				}
			}

			// --------------------------
			// start a new pair?
			// --------------------------
			ExtendedPair startedPair = null;
			foreach (Pair p in noOpPairs ?? Enumerable.Empty<Pair>())
			{
				if (c == p.Start)
				{
					startedPair = new ExtendedPair(p, false, topPair);
					break;
				}
			}
			if (startedPair != null)
			{
				result.Append(c);
				activePairs.Push(startedPair);
				return;
			}

			// --------------------------
			// Plain character inside a no-op area
			// --------------------------
			if (topPair != null)
			{
				result.Append(c);
				return;
			}

			// --------------------------
			// Plain character outside a no-op area
			// --------------------------
			unescapedAction(c, result, callerState);
			return;
		}

		#endregion
	}
}
