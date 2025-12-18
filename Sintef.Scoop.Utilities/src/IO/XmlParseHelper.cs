//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Helper class for parsing XML.
	/// Contains convenience extension functions for parsing data in xml elements and
	/// tagging all elements and attributes that have been parsed.
	/// All checks throw XmlParseException, which specifies at which element the 
	/// exception originated.
	/// After parsing is completed, the CheckAllParsed function can be used to throw
	/// an exception if the xml contained elements or attributes that we did not parse.
	/// </summary>
	public static class XmlParseHelper
	{
		/// <summary>
		/// Tag that we give as annotation to any XML element and attribute we've read
		/// </summary>
		static object tag = new object();

		/// <summary>
		/// Throws an exception if the given element's name is not equal to the given name
		/// </summary>
		public static void RequireName(this XElement el, XName name)
		{
			if (!el.Name.Equals(name))
				throw new XmlParseException(el, "Found element '" + el.Name + "', expected '" + name + "'");
		}

		/// <summary>
		/// Returns the named subelement. Throws an exception if not present. Tags the subelement
		/// </summary>
		public static XElement RequireElement(this XElement el, XName name)
		{
			var result = el.Element(name);
			if (result == null)
				throw new XmlParseException(el, "Element " + el.Name + " missing subelement " + name);
			Tag(result);
			return result;
		}

		/// <summary>
		/// Returns the named subelement, or null if not present. Tags the subelement
		/// </summary>
		public static XElement TagElement(this XElement el, XName name)
		{
			var result = el.Element(name);
			if (result != null)
				Tag(result);
			return result;
		}

		/// <summary>
		/// If the named subelement exists, tags it and all its subelements and all
		/// associated attributes, effectively ignoring it.
		/// </summary>
		/// <param name="el">The XML element</param>
		/// <param name="name"></param>
		public static void IgnoreElement(this XElement el, XName name)
		{
			var subEl = el.Element(name);
			if (subEl != null)
			{
				subEl.IgnoreElement();
			}
		}

		/// <summary>
		/// If the named attribute exists, it tags the attribute, effectively ignoring it.
		/// </summary>
		public static void IgnoreAttribute(this XElement el, XName name)
		{
			el.Attribute(name)?.Tag();
		}

		/// <summary>
		/// Tags the element and all its subelements and all
		/// associated attributes, effectively ignoring it.
		/// </summary>
		/// <param name="el">The XML element</param>
		public static void IgnoreElement(this XElement el)
		{
			foreach (var node in el.DescendantsAndSelf())
			{
				node.Tag();
				foreach (var attr in node.Attributes())
					attr.Tag();
			}
		}

		/// <summary>
		/// Returns the named attribute. Throws an exception if not present. Tags the attribute
		/// </summary>
		public static XAttribute RequireAttribute(this XElement el, XName name)
		{
			var result = el.Attribute(name);
			if (result == null)
				throw new XmlParseException(el, "Missing attribute " + name);
			Tag(result);
			return result;
		}

		/// <summary>
		/// Returns the named attribute, or null if not present. Tags the attribute
		/// </summary>
		public static XAttribute TagAttribute(this XElement el, XName name)
		{
			var result = el.Attribute(name);
			if (result != null)
				Tag(result);
			return result;
		}

		/// <summary>
		/// Returns the element's children while tagging them
		/// </summary>
		public static IEnumerable<XElement> TagElements(this XElement el)
		{
			var result = el.Elements();
			foreach (var sub in result)
				Tag(sub);
			return result;
		}

		/// <summary>
		/// Returns the element's children with the given name while tagging them
		/// </summary>
		public static IEnumerable<XElement> TagElements(this XElement el, XName name)
		{
			var result = el.Elements(name);
			foreach (var sub in result)
				Tag(sub);
			return result;
		}

		/// <summary>
		/// Returns the element's first child, tagging it
		/// </summary>
		public static XElement TagFirstElement(this XElement el)
		{
			XElement sub = el.Elements().FirstOrDefault();
			if (sub == null)
				throw new XmlParseException(el, "Expected element to contain at least one sub-element");

			sub.Tag();
			return sub;
		}

		/// <summary>
		/// Adds the given key/value to the dictionary.
		/// Throws an exception at the given element if the key already exists.
		/// </summary>
		public static void AddUnique<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value, XElement el, string typeName)
		{
			if (dict.ContainsKey(key))
				throw new XmlParseException(el, "Duplicate " + typeName + " ID '" + key + "'");
			dict.Add(key, value);
		}

		/// <summary>
		/// Tags the object by adding an annotation
		/// </summary>
		public static void Tag(this XObject obj)
		{
			obj.AddAnnotation(tag);
		}

		/// <summary>
		/// Returns true if the object has been tagged by adding an annotation
		/// </summary>
		public static bool IsTagged(this XObject obj)
		{
			return obj.Annotations(typeof(object)).Contains(tag);
		}

		/// <summary>
		/// Returns a description of the XML path to the given element
		/// </summary>
		public static string Path(this XElement el)
		{
			if (el.Parent == null)
				return el.Name.ToString();
			else
				return Path(el.Parent) + " -> " + el.Name.ToString();
		}

		/// <summary>
		/// Returns a description of the XML path to the given attribute
		/// </summary>
		public static string Path(this XAttribute attr)
		{
			return Path(attr.Parent) + " -> " + attr.Name.ToString();
		}

		/// <summary>
		/// Verifies that all elements and attriubutes under the given element
		/// have been tagged, or throws an XmlParseException
		/// </summary>
		/// <param name="root"></param>
		public static void CheckAllParsed(this XElement root)
		{
			// Check that we've tagged all leaf nodes, meaning we've parsed them
			foreach (XElement el in root.Descendants())
			{
				if (!el.IsTagged())
					// No attributes, not tagged
					throw new XmlParseException(el, "XML node '" + Path(el) + "' not recognized or given more than once");
			}
			// Check that we've tagged all attributes, meaning we've parsed them
			foreach (XElement el in root.Descendants())
			{
				foreach (XAttribute attr in el.Attributes())
				{
					if (!attr.IsTagged())
						// Attribute not tagged
						throw new XmlParseException(el, "XML attribute '" + Path(attr) + "' not recognized");
				}
			}
		}

		/// <summary>
		/// Returns value if XElement is not null, else returns defaultValue
		/// </summary>
		/// <param name="el">The XElement to parse</param>
		/// <param name="defaultValue">Default value to return if element is null</param>
		/// <returns>Value if XElement is not null, otherwise returns defaultValue</returns>
		public static string ValueOrDefault(this XElement el, string defaultValue)
		{
			Debug.Assert(defaultValue != null || defaultValue != "",
				"Default value should not be null or empty.");
			return el == null ? defaultValue : el.Value;
		}

		/// <summary>
		/// Parses and returns the element's value as a double.
		/// Uses the Invariant culture, so the decimal separator is always '.'
		/// </summary>
		/// <param name="el">The XML element</param>
		/// <param name="requireNonnegative">If true, an exception is thrown instead of returning a negative value</param>
		/// <param name="defaultValue">If given, this value is returned when the element is null.
		///  Example: <code>element.TagElement("value").ParseDoubleInvariant(defaultValue: 5)</code> </param>
		public static double ParseDoubleInvariant(this XElement el, bool requireNonnegative = false, double? defaultValue = null)
		{
			if (el == null && defaultValue.HasValue)
				return defaultValue.Value;

			double value;
			try
			{
				value = el.Value.ParseInvariantDouble();
			}
			catch (Exception ex)
			{
				throw new XmlParseException(el, ex);
			}

			if (requireNonnegative && value < 0)
				throw new XmlParseException(el, "Negative value not legal");

			return value;
		}

		/// <summary>
		/// Parses and returns the attribute's value as a double.
		/// Uses the Invariant culture, so the decimal separator is always '.'
		/// </summary>
		/// <param name="at">The XML attribute</param>
		/// <param name="requireNonnegative">If true, an exception is thrown instead of returning a negative value</param>
		/// <param name="defaultValue">If given, this value is returned when the attribute is null.
		///  Example: <code>element.TagAttribute("value").ParseDoubleInvariant(defaultValue: 5)</code> </param>
		public static double ParseDoubleInvariant(this XAttribute at, bool requireNonnegative = false, double? defaultValue = null)
		{
			if (at == null && defaultValue.HasValue)
				return defaultValue.Value;

			double value;
			try
			{
				value = at.Value.ParseInvariantDouble();
			}
			catch (Exception ex)
			{
				throw new XmlParseException(at, ex);
			}

			if (requireNonnegative && value < 0)
				throw new XmlParseException(at.Parent, "Negative value not legal for attribute " + at.Name);

			return value;
		}

		/// <summary>
		/// Parses and returns the element's value as a double.
		/// Note that the format accepted depends on the current locale
		/// </summary>
		/// <param name="el">The XML element</param>
		/// <param name="requireNonnegative">If true, an exception is thrown instead of returning a negative value</param>
		/// <param name="defaultValue">If given, this value is returned when the element is null.
		///  Example: <code>element.TagElement("value").ParseDoubleUsingLocale(defaultValue: 5)</code> </param>
		public static double ParseDoubleUsingLocale(this XElement el, bool requireNonnegative = false, double? defaultValue = null)
		{
			if (el == null && defaultValue.HasValue)
				return defaultValue.Value;

			double value;
			try
			{
				value = double.Parse(el.Value);
			}
			catch (Exception ex)
			{
				throw new XmlParseException(el, ex);
			}

			if (requireNonnegative && value < 0)
				throw new XmlParseException(el, "Negative value not legal");

			return value;
		}



		/// <summary>
		/// Parses and returns the element's value as an integer
		/// </summary>
		/// <param name="el">The XML element</param>
		/// <param name="requireNonnegative">If true, an exception is thrown instead of returning a negative value</param>
		/// <param name="defaultValue">If given, this value is returned when the element is null.
		///  Example: <code>element.TagElement("value").ParseInt(defaultValue: 5)</code> </param>
		public static int ParseInt(this XElement el, bool requireNonnegative = false, int? defaultValue = null)
		{
			if (el == null && defaultValue.HasValue)
				return defaultValue.Value;

			int value;
			try
			{
				value = int.Parse(el.Value);
			}
			catch (Exception ex)
			{
				throw new XmlParseException(el, ex);
			}

			if (requireNonnegative && value < 0)
				throw new XmlParseException(el, "Negative value not legal");

			return value;
		}

		/// <summary>
		/// Parses and returns the attribute's value as a long
		/// </summary>
		/// <param name="at">The XML attribute</param>
		/// <param name="requireNonnegative">If true, an exception is thrown instead of returning a negative value</param>
		/// <param name="defaultValue">If given, this value is returned when the element is null.
		///  Example: <code>element.TagElement("value").ParseInt(defaultValue: 5)</code> </param>
		public static long ParseLong(this XAttribute at, bool requireNonnegative = false, long? defaultValue = null)
		{
			if (at == null && defaultValue.HasValue)
				return defaultValue.Value;

			long value;
			try
			{
				value = long.Parse(at.Value);
			}
			catch (Exception ex)
			{
				throw new XmlParseException(at, ex);
			}

			if (requireNonnegative && value < 0)
				throw new XmlParseException(at.Parent, "Negative value not legal for attribute " + at.Name);

			return value;
		}

		/// <summary>
		/// Parses and returns the attribute's value as an integer
		/// </summary>
		/// <param name="at">The XML attribute</param>
		/// <param name="requireNonnegative">If true, an exception is thrown instead of returning a negative value</param>
		/// <param name="defaultValue">If given, this value is returned when the attribute is null.
		///  Example: <code>element.TagAttribute("value").ParseInt(defaultValue: 5)</code> </param>
		public static int ParseInt(this XAttribute at, bool requireNonnegative = false, int? defaultValue = null)
		{
			if (at == null && defaultValue.HasValue)
				return defaultValue.Value;

			int value;
			try
			{
				value = int.Parse(at.Value);
			}
			catch (Exception ex)
			{
				throw new XmlParseException(at, ex);
			}

			if (requireNonnegative && value < 0)
				throw new XmlParseException(at.Parent, "Negative value not legal for attribute " + at.Name);

			return value;
		}

		/// <summary>
		/// Parses and returns the attribute's value as an uinteger
		/// </summary>
		/// <param name="at">Attribute containing the value</param>
		/// <param name="defaultValue">If given, this value is returned when teh attribute is null</param>
		/// <returns>uint of value</returns>
		/// Example: <code>element.TagAttribute("value").ParseUint(defaultValue: 1)</code>
		public static uint ParseUInt(this XAttribute at, uint? defaultValue = null)
		{
			if (at == null && defaultValue.HasValue)
				return defaultValue.Value;

			uint value;
			try
			{
				value = uint.Parse(at.Value);
			}
			catch (Exception ex)
			{
				throw new XmlParseException(at, ex);
			}
			return value;
		}

		/// <summary>
		/// Parses and returns the boolean in the given element's value.
		/// Accepted values are "true", "false" (both case neutral), "0" and "1"
		/// </summary>
		/// <param name="el">The XML element</param>
		/// <param name="defaultValue">If given, this value is returned when the element is null.
		///  Example: <code>element.TagElement("value").ParseBool(defaultValue: true)</code> </param>
		public static bool ParseBool(this XElement el, bool? defaultValue = null)
		{
			if (el == null && defaultValue.HasValue)
				return defaultValue.Value;

			bool result;
			if (ParseBool(el.Value, out result))
				return result;
			throw new XmlParseException(el, "Illegal boolean value");
		}

		/// <summary>
		/// Parses and returns the boolean in the given attribute's value.
		/// Accepted values are "true", "false" (both case neutral), "0" and "1"
		/// </summary>
		/// <param name="at">The XML attribute</param>
		/// <param name="defaultValue">If given, this value is returned when the attribute is null.
		///  Example: <code>element.TagAttribute("value").ParseBool(defaultValue: true)</code> </param>
		public static bool ParseBool(this XAttribute at, bool? defaultValue = null)
		{
			if (at == null && defaultValue.HasValue)
				return defaultValue.Value;

			bool result;
			if (ParseBool(at.Value, out result))
				return result;
			throw new XmlParseException(at.Parent, "Illegal boolean value for attribute " + at.Name);
		}


		private static bool ParseBool(string value, out bool result)
		{
			if (value.ToLower() == "false" || value == "0")
			{
				result = false;
				return true;
			}
			if (value.ToLower() == "true" || value == "1")
			{
				result = true;
				return true;
			}
			result = false;
			return false;
		}

		/// <summary>
		/// Parses and returns the datetime in the given element's value.
		/// Note that the format accepted depends on the current locale
		/// </summary>
		/// <param name="el">The XML element</param>
		/// <param name="defaultValue">If given, this value is returned when the element is null.
		///  Example: <code>element.TagElement("value").ParseDateTimeUsingLocale(defaultValue: DateTime.MinValue)</code> </param>
		public static DateTime ParseDateTimeUsingLocale(this XElement el, DateTime? defaultValue = null)
		{
			if (el == null && defaultValue.HasValue)
				return defaultValue.Value;

			string value = el.Value;

			try
			{
				return DateTime.Parse(value);
			}
			catch (Exception ex)
			{
				throw new XmlParseException(el, ex);
			}
		}

		/// <summary>
		/// Parses and returns the datetime in the given element's value.
		/// Uses the Invariant culture
		/// </summary>
		/// <param name="el">The XML element</param>
		/// <param name="defaultValue">If given, this value is returned when the element is null.
		///  Example: <code>element.TagElement("value").ParseDateTimeInvariant(defaultValue: DateTime.MinValue)</code> </param>
		public static DateTime ParseDateTimeInvariant(this XElement el, DateTime? defaultValue = null)
		{
			if (el == null && defaultValue.HasValue)
				return defaultValue.Value;

			string value = el.Value;

			try
			{
				return value.ParseInvariantDateTime();
			}
			catch (Exception ex)
			{
				throw new XmlParseException(el, ex);
			}
		}

		/// <summary>
		/// Parses and returns the datetime in the given element's value as UTC time.
		/// </summary>
		/// <param name="el">The XML element</param>
		/// <param name="defaultValue">If given, this value is returned when the element is null.
		///  Example: <code>element.TagElement("value").ParseAsUniversalDateTime(defaultValue: DateTime.MinValue)</code> </param>
		public static DateTime ParseAsUniversalDateTime(this XElement el, DateTime? defaultValue = null)
		{
			if (el == null && defaultValue.HasValue)
				return defaultValue.Value;

			string value = el.Value;

			try
			{
				return value.ParseUniversalDateTime();
			}
			catch (Exception ex)
			{
				throw new XmlParseException(el, ex);
			}
		}

		/// <summary>
		/// Parses and returns the datetime in the given element's value.
		/// Uses the Invariant culture
		/// </summary>
		/// <param name="el">The XML element</param>
		/// <param name="defaultValue">If given, this value is returned when the element is null.
		///  Example: <code>element.TagElement("value").ParseDateTimeInvariant(defaultValue: DateTimeOffset.MinValue)</code> </param>
		public static DateTimeOffset ParseDateTimeOffsetInvariant(this XElement el, DateTimeOffset? defaultValue = null)
		{
			if (el == null && defaultValue.HasValue)
				return defaultValue.Value;

			string value = el.Value;

			try
			{
				return value.ParseInvariantDateTimeOffset();
			}
			catch (Exception ex)
			{
				throw new XmlParseException(el, ex);
			}
		}

		/// <summary>
		/// Parses and returns the datetime in the given attribute's value.
		/// Uses the Invariant culture
		/// </summary>
		/// <param name="at">The XML attribute</param>
		/// <param name="defaultValue">If given, this value is returned when the element is null.
		///  Example: <code>element.RequireAttribute("value").ParseDateTimeInvariant(defaultValue: DateTime.MinValue)</code> </param>
		public static DateTime ParseDateTimeInvariant(this XAttribute at, DateTime? defaultValue = null)
		{
			if (at == null && defaultValue.HasValue)
				return defaultValue.Value;

			string value = at.Value;

			try
			{
				return value.ParseInvariantDateTime();
			}
			catch (Exception ex)
			{
				throw new XmlParseException(at, ex);
			}
		}

		/// <summary>
		/// Parses and returns the datetime in the given attribute's value as UTC time.
		/// Uses the Invariant culture
		/// </summary>
		/// <param name="at">The XML attribute</param>
		/// <param name="defaultValue">If given, this value is returned when the element is null.
		///  Example: <code>element.RequireAttribute("value").ParseAsUniversalDateTime(defaultValue: DateTime.MinValue)</code> </param>
		public static DateTime ParseAsUniversalDateTime(this XAttribute at, DateTime? defaultValue = null)
		{
			if (at == null && defaultValue.HasValue)
				return defaultValue.Value;

			string value = at.Value;

			try
			{
				return value.ParseUniversalDateTime();
			}
			catch (Exception ex)
			{
				throw new XmlParseException(at, ex);
			}
		}

		/// <summary>
		/// Parses and returns the datetime in the given element's value.
		/// Uses the Invariant culture
		/// </summary>
		/// <param name="el">The XML element</param>
		/// <param name="defaultValue">If given, this value is returned when the element is null.
		///  Example: <code>element.TagElement("value").ParseTimeSpanInvariant(defaultValue: TimeSpan.Zero)</code> </param>
		public static TimeSpan ParseTimeSpanInvariant(this XElement el, TimeSpan? defaultValue = null)
		{
			if (el == null && defaultValue.HasValue)
				return defaultValue.Value;

			string value = el.Value;

			try
			{
				return value.ParseInvariantTimeSpan();
			}
			catch (Exception ex)
			{
				throw new XmlParseException(el, ex);
			}
		}

		/// <summary>
		/// Parses and returns the Enum in the given element's value.
		/// </summary>
		/// <typeparam name="T">This should be of type enum. Otherwhise, an ArgumentException is thrown </typeparam>
		/// <param name="el">The XML element</param>
		/// <param name="defaultValue">If given, this value is returned when the element is null. If not given, when the attribute is null, then a NullReferenceException is thrown. 
		/// Example: <code>element.TagElement("value").ParseTimeSpanInvariant(defaultValue: TimeSpan.Zero)</code></param>
		/// <returns></returns>
		public static T ParseEnum<T>(this XElement el, T? defaultValue = null) where T : struct, IConvertible
		{
			if (!typeof(T).IsEnum)
				throw new ArgumentException("T must be an enumerated type");

			if (el == null && defaultValue.HasValue)
				return defaultValue.Value;

			var value = el.Value;

			try
			{
				return (T)Enum.Parse(typeof(T), value);
			}
			catch (Exception ex)
			{
				throw new XmlParseException(el, ex);
			}
		}

		/// <summary>
		/// Parses and returns the Enum in the given attributes's value.
		/// </summary>
		/// <typeparam name="T">This should be of type enum. Otherwhise, an ArgumentException is thrown </typeparam>
		/// <param name="at">The XML attribute</param>
		/// <param name="defaultValue">If given, this value is returned when the attribute is null. If not given, when the attribute is null, then a NullReferenceException is thrown.
		/// Example: <code>element.TagAttribute("value").ParseTimeSpanInvariant(defaultValue: TimeSpan.Zero)</code></param>
		/// <returns></returns>
		public static T ParseEnum<T>(this XAttribute at, T? defaultValue = null) where T : struct, IConvertible
		{
			if (!typeof(T).IsEnum)
				throw new ArgumentException("T must be an enumerated type");

			if (at == null && defaultValue.HasValue)
				return defaultValue.Value;

			var value = at.Value;

			try
			{
				return (T)Enum.Parse(typeof(T), value);
			}
			catch (Exception ex)
			{
				throw new XmlParseException(at, ex);
			}
		}

		/// <summary>
		/// Parses and returns an enumerable of T from the enumerable of elements.
		/// </summary>
		/// <typeparam name="T">The type of the result of the given parseMethod</typeparam>
		/// <param name="elements">The underlying enumerable of elements that is parsed. If is is null an empty enumerable of T is returned</param>
		/// <param name="parseMethod">The parsing method to use when parsing the individual XElement to objects of type T</param>
		/// <returns></returns>
		public static IEnumerable<T> ParseEnumerable<T>(this IEnumerable<XElement> elements, Func<XElement, T> parseMethod)
		{
			return elements?.Select(parseMethod) ?? Enumerable.Empty<T>();
		}

		/// <summary>
		/// Parses and returns the TimeSpan in the given element's value which is assumed to be in ISO 8601 format.
		/// Elements with non zero month or year values will cause an exception since they are not well defined.
		/// </summary>
		/// <param name="el">The XML element</param>
		/// <param name="defaultValue">If given, this value is returned when the element is null. Otherwise, NullReferenceException is thrown </param>
		/// <returns></returns>
		public static TimeSpan ParseISOTimeSpan(this XElement el, TimeSpan? defaultValue = null)
		{
			if (el == null && defaultValue.HasValue)
				return defaultValue.Value;

			string value = el.Value;

			try
			{
				return value.ParseISOTimeSpan();
			}
			catch (Exception ex)
			{
				throw new XmlParseException(el, ex);
			}
		}

		/// <summary>
		/// Parses and returns the TimeSpan in the given attribute's value which is assumed to be in ISO 8601 format.
		/// Attributes with non zero month or year values will cause an exception since they are not well defined.
		/// </summary>
		/// <param name="at">The XML attribute</param>
		/// <param name="defaultValue">If given, this value is returned when the attribute is null. Otherwise, NullReferenceException is thrown </param>
		/// <returns></returns>
		public static TimeSpan ParseISOTimeSpan(this XAttribute at, TimeSpan? defaultValue = null)
		{
			if (at == null && defaultValue.HasValue)
				return defaultValue.Value;

			string value = at.Value;

			try
			{
				return value.ParseISOTimeSpan();
			}
			catch (Exception ex)
			{
				throw new XmlParseException(at, ex);
			}
		}
	}

	/// <summary>
	/// An exception during parsing of an XML problem file.
	/// Contains a reference to the offending xml object (XElement or XAttribute)
	/// </summary>
	public class XmlParseException : Exception
	{
		/// <summary>
		/// The XObject being parsed when the exception occurred
		/// </summary>
		public XObject XObject { get; set; }

		/// <summary>
		/// The XElement being parsed when the exception occurred
		/// </summary>
		public XElement Element
		{
			get
			{
				if (XObject is XElement)
					return (XElement)XObject;
				if (XObject is XAttribute)
					return XObject.Parent;
				return null;
			}
		}

		/// <summary>
		/// The XAttribute being parsed when the exception occurred
		/// or null if the exception if the exception is from an XElement
		/// </summary>
		public XAttribute Attribute { get { return XObject as XAttribute; } }

		/// <summary>
		/// Initializes an exception
		/// </summary>
		/// <param name="xmlObject">The XObject being parsed when the exception occurred</param>
		/// <param name="message">The exception message</param>
		public XmlParseException(XObject xmlObject, string message)
			: base(message)
		{
			XObject = xmlObject;
		}

		/// <summary>
		/// Initializes an exception
		/// </summary>
		/// <param name="xmlObject">The XObject being parsed when the exception occurred</param>
		/// <param name="inner">The inner exception</param>
		public XmlParseException(XObject xmlObject, Exception inner)
			: base(inner.Message, inner)
		{
			XObject = xmlObject;
		}
	}

}
