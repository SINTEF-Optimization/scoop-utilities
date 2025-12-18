//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Extension methods for GUI components
	/// </summary>
	public static class GuiExtensions
	{
		/// <summary>
		/// Returns all subcontrols in the given control, recursively
		/// </summary>
		/// <param name="root"></param>
		/// <returns></returns>
		public static IEnumerable<Control> GetAllChildren(this Control root)
		{
			var stack = new Stack<Control>();
			stack.Push(root);

			while (stack.Any())
			{
				var next = stack.Pop();
				foreach (Control child in next.Controls)
					stack.Push(child);
				yield return next;
			}
		}

		/// <summary>
		/// Appends the text in a new line (unless old text is empty)
		/// </summary>
		/// <param name="box">Box whose text to append to</param>
		/// <param name="text">text to append</param>
		public static void AppendTextInNewLine(this TextBox box, string text)
		{
			if (box.Text.Length == 0)
				box.AppendText(text);
			else
				box.AppendText("\r\n" + text);
		}

		/// <summary>
		/// Sets the DoubleBuffered property of <paramref name="control"/> to <paramref name="value"/>
		/// </summary>
		public static void SetDoubleBuffered(this Control control, bool value)
		{
			// Get the DoubleBuffered property through reflection, since it's protected
			System.Reflection.PropertyInfo property = typeof(Control).GetProperty("DoubleBuffered",
							System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			property.SetValue(control, value, null);
		}
	}
}
