//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System.ComponentModel;
using System.Windows.Forms;

namespace Sintef.Scoop.Utilities.GUI
{
	/// <summary>
	/// Double Buffered table layout panel - removes flicker during resize operations.
	/// </summary>
	public partial class DoubleBufferedTableLayoutPanel : TableLayoutPanel
	{
		/// <summary>
		/// Initializes a control
		/// </summary>
		public DoubleBufferedTableLayoutPanel()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint |
			  ControlStyles.OptimizedDoubleBuffer |
			  ControlStyles.UserPaint, true);
		}

		/// <summary>
		/// Initializes a control
		/// </summary>
		public DoubleBufferedTableLayoutPanel(IContainer container)
		{
			container.Add(this);
			SetStyle(ControlStyles.AllPaintingInWmPaint |
			  ControlStyles.OptimizedDoubleBuffer |
			  ControlStyles.UserPaint, true);
		}
	}
}