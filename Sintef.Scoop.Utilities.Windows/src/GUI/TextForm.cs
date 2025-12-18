//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Windows.Forms;

namespace Sintef.Scoop.Utilities.GUI
{
	/// <summary>
	/// Simple form for displaying some text, as read only.
	/// </summary>
	public partial class TextForm : Form
	{
		/// <summary>
		/// Initializes the form
		/// </summary>
		public TextForm()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Initializes the form
		/// </summary>
		public void Initialise(string title)
		{
			Text = title;
		}

		/// <summary>
		/// Add some text to the text box.
		/// </summary>
		/// <param name="text"></param>
		public void Write(string text)
		{
			lock (_textBox)
			{
				_textBox.Text += text;
				_textBox.SelectionStart = _textBox.Text.Length;
				_textBox.ScrollToCaret();			
			}
		}

		/// <summary>
		/// Add some text to the text box.
		/// </summary>
		/// <param name="text"></param>
		public void WriteLine(string text)
		{
			Write(text += "\r\n");
		}

		private void _buttonClose_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
