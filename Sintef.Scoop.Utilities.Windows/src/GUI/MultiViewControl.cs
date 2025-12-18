//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Sintef.Scoop.Utilities.GUI
{
	/// <summary>
	/// This control position controls in a TableLayoutPanel.
	/// The controls are given a context menu allowing change their size and position.
	/// 
	/// </summary>
	public partial class MultiViewControl : UserControl
	{
		#region Private variables

		private List<Control> _controls = new List<Control>();
		private int _rows = 1;
		private int _columns = 1;

		#endregion

		#region Public properties

		/// <summary>
		/// The number of columns wanted in the control.
		/// The number of columns displayed may be less
		/// depending on the number and size of sub-controls.
		/// 
		/// Setting Columns has no effect when GrowStyle = AddColumns  
		/// </summary>
		private int Columns
		{
			get
			{
				return _columns;

			}
			set
			{
				_columns = Math.Max(1, value);
				TableColumnCount = _columns;
				LayoutControls();
			}
		}

		/// <summary>
		/// The number of rows wanted in the control.
		/// The number of rows displayed may be more or less
		/// depending on the number and size of sub-controls.
		/// 
		/// Setting Rows has no effect when GrowStyle = AddRows  
		/// </summary>
		private int Rows
		{
			get
			{
				// not stored in a separate variable since it is a consequence of the sub-controls
				return _rows;
			}
			set
			{
				_rows = Math.Max(1, value);
				TableRowCount = _rows;
				LayoutControls();
			}
		}

		#endregion

		#region Protected properties

		/// <summary>
		/// The number of rows currently in the table
		/// </summary>
		protected int TableRowCount
		{
			get
			{
				return _tableLayoutPanel.RowCount;
			}
			set
			{
				if (TableRowCount == value)
					return;
				_tableLayoutPanel.RowCount = value;
			}
		}

		/// <summary>
		/// The number of columns currently in the table
		/// </summary>
		protected int TableColumnCount
		{
			get
			{
				return _tableLayoutPanel.ColumnCount;
			}
			set
			{
				if (TableColumnCount == value)
					return;
				_tableLayoutPanel.ColumnCount = value;
			}
		}

		#endregion

		#region Constructor and initialization

		/// <summary>
		/// Initializes the control
		/// </summary>
		public MultiViewControl()
		{
			InitializeComponent();
			AddContextMenu(this);
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Adds a control to the view.
		/// </summary>
		/// <param name="control">The control to add</param>
		/// <param name="cols">The width of the control in columns</param>
		/// <param name="rows">The height of the control in rows</param>
		public void Add(Control control, int rows = 1, int cols = 1)
		{
			AddContextMenu(control);
			_controls.Add(control);
			_tableLayoutPanel.SetRowSpan(control, rows);
			_tableLayoutPanel.SetColumnSpan(control, cols);
			LayoutControls();
		}

		/// <summary>
		/// Removes a control from the view.
		/// </summary>
		/// <param name="control"></param>
		public void Remove(Control control)
		{
			_controls.Remove(control);
			LayoutControls();
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Adds elements to the context menu strip of the control.
		/// </summary>
		/// <param name="control"></param>
		private void AddContextMenu(Control control)
		{
			ToolStripMenuItem subMenu, item;
			var mainMenu = control.ContextMenuStrip;
			if (mainMenu == null)
			{
				mainMenu = new ContextMenuStrip();
				control.ContextMenuStrip = mainMenu;
			}
			else if (mainMenu.Items.Count > 0)
			{
				mainMenu.Items.Add(new ToolStripSeparator());
			}

			#region Grid menu

			subMenu = new ToolStripMenuItem("Grid");

			item = new ToolStripMenuItem("Add one column");
			subMenu.DropDownItems.Add(item);
			item.Click += delegate (object sender, EventArgs e)
			{
				++Columns;
			};

			item = new ToolStripMenuItem("Remove one column");
			subMenu.DropDownItems.Add(item);
			item.Click += delegate (object sender, EventArgs e)
			{
				--Columns;
			};

			mainMenu.Items.Add(subMenu);

			#endregion

			// The below is added to the context menu only for sub controls
			if (control == this)
				return;

			#region Resize menu for sub control

			subMenu = new ToolStripMenuItem("Resize");
			item = new ToolStripMenuItem("Increase Width");
			subMenu.DropDownItems.Add(item);
			item.Click += delegate (object sender, EventArgs e)
			{
				int cols = _tableLayoutPanel.GetColumnSpan(control) + 1;
				_tableLayoutPanel.SetColumnSpan(control, cols);
				if (cols > Columns)
					Columns = cols;
				else
					LayoutControls();
			};

			item = new ToolStripMenuItem("Decrease Width");
			subMenu.DropDownItems.Add(item);
			item.Click += delegate (object sender, EventArgs e)
			{
				int cols = _tableLayoutPanel.GetColumnSpan(control) - 1;
				if (cols > 0)
				{
					_tableLayoutPanel.SetColumnSpan(control, cols);
					LayoutControls();
				}
			};

			item = new ToolStripMenuItem("Increase Height");
			subMenu.DropDownItems.Add(item);
			item.Click += delegate (object sender, EventArgs e)
			{
				int rows = _tableLayoutPanel.GetRowSpan(control);
				_tableLayoutPanel.SetRowSpan(control, rows + 1);
				LayoutControls();
			};

			item = new ToolStripMenuItem("Decrease Height");
			subMenu.DropDownItems.Add(item);
			item.Click += delegate (object sender, EventArgs e)
			{
				int rows = _tableLayoutPanel.GetRowSpan(control);
				if (rows > 1)
				{
					_tableLayoutPanel.SetRowSpan(control, rows - 1);
					LayoutControls();
				}
			};
			mainMenu.Items.Add(subMenu);

			#endregion

			#region Move menu

			subMenu = new ToolStripMenuItem("Move");
			item = new ToolStripMenuItem("Move Earlier");
			subMenu.DropDownItems.Add(item);
			item.Click += delegate (object sender, EventArgs e)
			{
				int pos = _controls.IndexOf(control);
				if (pos > 0)
				{
					_controls.Remove(control);
					_controls.Insert(pos - 1, control);
					LayoutControls();
				}
			};

			item = new ToolStripMenuItem("Move Later");
			subMenu.DropDownItems.Add(item);
			item.Click += delegate (object sender, EventArgs e)
			{
				int pos = _controls.IndexOf(control);
				if (pos < _controls.Count - 1)
				{
					_controls.Remove(control);
					_controls.Insert(pos + 1, control);
					LayoutControls();
				}
			};

			item = new ToolStripMenuItem("Remove view");
			subMenu.DropDownItems.Add(item);
			item.Click += delegate (object sender, EventArgs e)
			{
				Remove(control);
			};

			mainMenu.Items.Add(subMenu);

			#endregion
		}

		/// <summary>
		/// Rebuilds the layout of sub-controls.
		/// </summary>
		private void LayoutControls()
		{
			_tableLayoutPanel.Controls.Clear();
			foreach (var control in _controls)
				_tableLayoutPanel.Controls.Add(control);
			// Now there
			EnsureTableSize();
		}

		/// <summary>
		/// Ensures that there are enough rows, columns, row styles and column styles
		/// </summary>
		private void EnsureTableSize()
		{
			EnsureEnoughRows();
			EnsureEnoughColumns();
		}

		/// <summary>
		/// Ensures that there are enough rows and row styles
		/// </summary>
		private void EnsureEnoughRows()
		{
			int requiredRows = _controls.Any()
				? _controls.Max(c => _tableLayoutPanel.GetPositionFromControl(c).Row + _tableLayoutPanel.GetRowSpan(c))
				: 1;
			TableRowCount = requiredRows;
			while (_tableLayoutPanel.RowStyles.Count < TableRowCount)
				_tableLayoutPanel.RowStyles.Add(new RowStyle());
			for (int i = 0; i < _tableLayoutPanel.RowStyles.Count; ++i)
			{
				var style = _tableLayoutPanel.RowStyles[i];
				style.Height = 1;
				style.SizeType = SizeType.Percent;
			}
		}

		/// <summary>
		/// Ensures that there are enough columns and column styles
		/// </summary>
		private void EnsureEnoughColumns()
		{
			int requiredCols = _controls.Any()
				? _controls.Max(c => _tableLayoutPanel.GetPositionFromControl(c).Column + _tableLayoutPanel.GetColumnSpan(c))
				 : 1;
			TableColumnCount = requiredCols;
			while (_tableLayoutPanel.ColumnStyles.Count < TableColumnCount)
				_tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
			for (int i = 0; i < _tableLayoutPanel.ColumnStyles.Count; ++i)
			{
				var style = _tableLayoutPanel.ColumnStyles[i];
				style.Width = 1;
				style.SizeType = SizeType.Percent;
			}
		}

		#endregion
	}
}
