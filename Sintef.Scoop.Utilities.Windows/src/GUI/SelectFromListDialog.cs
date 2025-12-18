//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System.Collections.Generic;
using System.Windows.Forms;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// A dialog that shows a list of items and allows the user to select one
	/// </summary>
	public partial class SelectFromListDialog : Form
	{
		/// <summary>
		/// The item that was selected
		/// </summary>
		public object SelectedItem => _listBox.SelectedItem;

		/// <summary>
		/// Initializes the dialog
		/// </summary>
		/// <param name="items">The items to show in the list</param>
		/// <param name="title">The title of the dialog</param>
		public SelectFromListDialog(IEnumerable<object> items, string title = null)
		{
			InitializeComponent();

			Text = title ?? "Please select an item";

			foreach (var item in items)
				_listBox.Items.Add(item);
		}

		private void _listBox_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
