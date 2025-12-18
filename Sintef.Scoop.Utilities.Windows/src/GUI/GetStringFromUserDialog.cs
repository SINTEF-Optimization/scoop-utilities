//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System.Windows.Forms;

namespace Sintef.Scoop.Utilities.GUI
{
	/// <summary>
	/// Small dialog to retrieve a string from the user.
	/// Inherits caption, text, etc. from MessageBox.
	/// </summary>
	public partial class GetStringFromUserDialog : Form
	{
		/// <summary>
		/// The description/explanation to the user
		/// </summary>
		public string Description { get { return _textBoxDescription.Text; } set { _textBoxDescription.Text = value; } }

		/// <summary>
		/// The input given by the user
		/// </summary>
		public string UserInput { get { return _textBoxInput.Text; } set { _textBoxInput.Text = value; } }

		/// <summary>
		/// Initializes the dialog
		/// </summary>
		public GetStringFromUserDialog()
		{
			InitializeComponent();
		}


	}
}
