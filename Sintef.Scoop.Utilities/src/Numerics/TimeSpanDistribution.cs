//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Xml.Linq;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// A random distribution of time spans.
	/// </summary>
	public class TimeSpanDistribution
	{
		/// <summary>
		/// The underlying real-valued distribution, which produces the
		/// time span lengths in seconds.
		/// </summary>
		public Distribution SecondsDistribution { get; private set; }

		/// <summary>
		/// Creates a random time span distribution
		/// </summary>
		/// <param name="secondsDistribution">The underlying distribution, which produces the
		/// time span lengths in seconds</param>
		public TimeSpanDistribution(Distribution secondsDistribution)
		{
			SecondsDistribution = secondsDistribution;
		}

		/// <summary>
		/// Creates a time span distribution by parsing the given xml element.
		/// 
		/// Three formats are supported:
		/// - If the element name is Distribution, its contents is parsed
		/// - If the element contains a Distribution sub-element, the sub-element is parsed
		/// - Otherwise, the element's text value is parsed as a constant time span
		/// </summary>
		/// <param name="xElement">The element to parse</param>
		/// <returns>The time span distribution</returns>
		public static TimeSpanDistribution FromXml(XElement xElement)
		{
			Distribution secondsDistribution;
			if (xElement.Name == "Distribution")
				secondsDistribution = Distribution.GetDistribution(xElement, useTimeSpanAsSeconds: true);
			else
				secondsDistribution = Distribution.GetDistributionFromSubElementOrValue(xElement, useTimeSpanAsSeconds: true);

			return new TimeSpanDistribution(secondsDistribution);
		}

		/// <summary>
		/// Draws a time span from the distribution
		/// </summary>
		/// <param name="random">The random generator to use</param>
		/// <returns>The drawn time span</returns>
		public TimeSpan DrawTimeSpan(Random random)
		{
			return SecondsDistribution.GetRandomTimeSpan(random);
		}
	}
}
