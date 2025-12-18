//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Sintef.Scoop.Utilities.GUI
{
	/// <summary>
	/// A user control that allows selecting a set of dates
	/// </summary>
	public partial class MultiDatePicker : UserControl
	{
		/// <summary>
		/// Raised when the selection of dates has changed
		/// </summary>
		public event EventHandler SelectionChanged;

		/// <summary>
		/// Initializes the control
		/// </summary>
		public MultiDatePicker()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Returns the dates selected in the control
		/// </summary>
		public IEnumerable<DateTime> SelectedDates => _dateList.Items.Cast<DateTime>();

		/// <summary>
		/// Updates the control to show the given dates
		/// </summary>
		/// <param name="dates"></param>
		public void Show(IEnumerable<DateTime> dates)
		{
			_dateList.Items.Clear();
			_dateList.Items.AddRange(dates.Cast<object>().ToArray());

			SelectionChanged?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Event handler for Add button
		/// </summary>
		private void _add_Click(object sender, EventArgs e)
		{
			var dates = SelectedDates.ToList();
			dates.Add(_date.Value.Date);

			Show(dates.Distinct().OrderBy(d => d));
		}

		/// <summary>
		/// Event handler for Remove buttor
		/// </summary>
		private void _remove_Click(object sender, EventArgs e)
		{
			if (_dateList.SelectedItems.Count == 1)
				_dateList.Items.Remove(_dateList.SelectedItem);

			SelectionChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
