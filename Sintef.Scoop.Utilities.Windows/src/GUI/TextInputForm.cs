//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Windows.Forms;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Form that gets a short input string from the user.
	/// </summary>
	public partial class TextInputForm : Form
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public TextInputForm()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Opens this form and returns the result. Return null
		/// if the user cancels.
		/// </summary>
		/// <param name="question">Question to the user.</param>
		/// <param name="title">Text box title</param>
		/// <param name="allowCancel">If true, there will be a cancel button.</param>
		/// <returns></returns>
		public static string GetInput(string question, bool allowCancel = false, string title = null)
		{
			TextInputForm form = new TextInputForm();
			if (title != null)
				form.Text = title;
			int questionLength = question.Length;
			form._buttonCancel.Visible = allowCancel;
			form._labelQuestion.Text = question;
			form.Width = Math.Max(600, (int)( 1.5 * questionLength));
			form._textBoxInput.Width = Math.Max(500, questionLength);
			DialogResult res = form.ShowDialog();
			if (res == DialogResult.OK)
				return form._textBoxInput.Text;
			else
				return null;
		}

		private void _buttonOK_Click(object sender, EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.OK;
			Close();
		}

		private void _buttonCancel_Click(object sender, EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.Cancel;
			Close();
		}
	}
}
