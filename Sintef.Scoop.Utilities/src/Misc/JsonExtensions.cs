using System;
using System.Text.Json;

namespace Sintef.Scoop.Utilities;

/// <summary>
/// Extension methods for JSON parsing.
/// </summary>
public static class JsonExtensions
{
	/// <summary>
	/// <para>Parses the given JSON element for the value of the property <paramref name="name"/> as the given type <typeparamref name="T"/>. This currently
	/// supports the following basic C# types: string, bool, byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal, DateTime, Guid.</para>
	/// 
	/// <para>Attempting to use any other type will result in an exception. DateTime and DateTimeOffsets must conform to the ISO 8601-1 extended format in the
	/// JSON element.</para>
	/// 
	/// <para>If a property with the given name is not present on the given element, this method will throw an exception.</para>
	///
	/// <para>This method is provided for convenience and will be somewhat slower than using the built in <see cref="JsonElement"/> methods directly.</para>
	/// </summary>
	/// <param name="element">The JSON element that may contain the property.</param>
	/// <param name="name">The name of the property.</param>
	/// <typeparam name="T">The type of the value of the property.</typeparam>
	/// <returns>The parsed value.</returns>
	/// <exception cref="InvalidOperationException">The property is not present or the type is not supported.</exception>
	public static T GetPropertyValue<T>(this JsonElement element, string name)
	{
		if (!element.TryGetProperty(name, out var propertyElement) || propertyElement.ValueKind == JsonValueKind.Null)
		{
			throw new InvalidOperationException($"Missing property {name}");
		}

		return TryConvertValue<T>(propertyElement, name);
	}

	/// <summary>
	/// <para>Parses the given JSON element for the value of the property <paramref name="name"/> as the given type <typeparamref name="T"/>. This currently
	/// supports the following basic C# types: string, bool, byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal, DateTime, Guid.</para>
	/// 
	/// <para>Attempting to use any other type will result in an exception. DateTime and DateTimeOffsets must conform to the ISO 8601-1 extended format in the
	/// JSON element.</para>
	/// 
	/// <para>If a property with the given name is not present on the given element, this method will return <paramref name="defaultValue"/>.</para>
	///
	/// <para>This method is provided for convenience and will be somewhat slower than using the built in <see cref="JsonElement"/> methods directly.</para>
	/// </summary>
	/// <param name="element">The JSON element that may contain the property.</param>
	/// <param name="name">The name of the property.</param>
	/// <param name="defaultValue">The default value to return if the property is not present in the provided element.</param>
	/// <typeparam name="T">The type of the value of the property.</typeparam>
	/// <returns>The parsed value or default value if property not present in the provided element.</returns>
	/// <exception cref="InvalidOperationException">The type is not supported.</exception>
	public static T GetPropertyValueOrDefault<T>(this JsonElement element,
		string name,
		T defaultValue = default)
	{
		if (!element.TryGetProperty(name, out var propertyElement) || propertyElement.ValueKind == JsonValueKind.Null)
			return defaultValue;

		return TryConvertValue<T>(propertyElement, name);
	}
	
	/// <summary>
	/// Helper method for <see cref="GetPropertyValue{T}"/> and see <see cref="GetPropertyValueOrDefault{T}"/>.
	///
	/// It does the conversion or throws an exception if the conversion fails.
	/// </summary>
	/// <param name="propertyElement">The element holding the property value.</param>
	/// <param name="name">The name of the element.</param>
	/// <typeparam name="T">The type of the value we are trying to convert to.</typeparam>
	/// <returns>The converted value.</returns>
	/// <exception cref="InvalidOperationException">The conversion failed for some reason.</exception>
	private static T TryConvertValue<T>(JsonElement propertyElement, string name)
	{
		try
		{
			if (typeof(T) == typeof(string))
			{
				return (T)(object)propertyElement.GetString();
			}

			if (typeof(T) == typeof(bool))
			{
				return (T)(object)propertyElement.GetBoolean();
			}

			if (typeof(T) == typeof(byte))
			{
				return (T)(object)propertyElement.GetByte();
			}

			if (typeof(T) == typeof(sbyte))
			{
				return (T)(object)propertyElement.GetSByte();
			}

			if (typeof(T) == typeof(short))
			{
				return (T)(object)propertyElement.GetInt16();
			}

			if (typeof(T) == typeof(ushort))
			{
				return (T)(object)propertyElement.GetUInt16();
			}
			
			if (typeof(T) == typeof(int))
			{
				return (T)(object)propertyElement.GetInt32();
			}

			if (typeof(T) == typeof(uint))
			{
				return (T)(object)propertyElement.GetUInt32();
			}

			if (typeof(T) == typeof(long))
			{
				return (T)(object)propertyElement.GetInt64();
			}

			if (typeof(T) == typeof(ulong))
			{
				return (T)(object)propertyElement.GetUInt64();
			}

			if (typeof(T) == typeof(float))
			{
				return (T)(object)propertyElement.GetSingle();
			}

			if (typeof(T) == typeof(double))
			{
				return (T)(object)propertyElement.GetDouble();
			}

			if (typeof(T) == typeof(decimal))
			{
				return (T)(object)propertyElement.GetDecimal();
			}

			if (typeof(T) == typeof(DateTime))
			{
				return (T)(object)propertyElement.GetDateTime();
			}

			if (typeof(T) == typeof(DateTimeOffset))
			{
				return (T)(object)propertyElement.GetDateTimeOffset();
			}

			if (typeof(T) == typeof(Guid))
			{
				return (T)(object)propertyElement.GetGuid();
			}

			throw new InvalidOperationException($"Currently does not support parsing to type {typeof(T)}");
		}
		catch (Exception ex) when (ex is JsonException or FormatException or OverflowException or InvalidOperationException)
		{
			throw new InvalidOperationException(
				$"Unable to convert JSON property '{name}' to {typeof(T)}.", ex);
		}
		
	}

		
}