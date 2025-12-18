//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System.Windows.Forms;

namespace Sintef.Scoop.Utilities.GUI
{
	/// <summary>
	/// A text box in windows forms needs \r\n combination to display newline. This
	/// textbox works on \n and automatically translates to/from \r\n.
	/// </summary>
	public class NormalNewlineTextBox : TextBox
	{
		/// <summary>
		/// Text as displayed, thus having \r\n instead of \n
		/// </summary>
		public string TextAsDisplayed { get { return base.Text; } }

		/// <summary>
		/// Text displayed/to display, but with normal \n newline characters inside/accepted 
		/// instead of \r\n.
		/// </summary>
		public override string Text
		{
			get
			{
				return base.Text.Replace("\r\n", "\n");
			}

			set
			{
				if (value == null)
				{
					base.Text = value;
					return;
				}

				base.Text = "";
				string s = value;
				int index = s.IndexOf('\n');
				while(index >= 0)
				{
					if (index > 0 && s[index - 1] == '\r')
						base.Text += s.Substring(0, index - 1);
					else
						base.Text += s.Substring(0, index);
					base.Text += "\r\n";
					s = s.Substring(index + 1);
					index = s.IndexOf('\n');
				}
				base.Text += s;
			}
		}
	}
}
