//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.ComponentModel;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Enables expanding an object of type T in a PropertyGrid.
	/// Use by adding the attribute <code>[TypeConverter(typeof(GenericObjectConverter&lt;X>))]</code>
	/// to your class X.
	/// </summary>
	public class GenericObjectConverter<T> : ExpandableObjectConverter
	{
		/// <inheritdoc/>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(T))
				return true;

			/*
			 * Using [TypeConverter(typeof(GenericObjectConverter<T>))] leads to problems in deserialization from JSON
			 * using e.g. Newtonsoft. Apperently if object can be converted to string, deserializer assumes that the object
			 * is presented as string in json. Hence we do not want this.
			 */
			if (destinationType == typeof(string))
				return false;

			return base.CanConvertTo(context, destinationType);
		}

		/// <inheritdoc/>
		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(System.String) && value is T)
			{
				return value.ToString();
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
