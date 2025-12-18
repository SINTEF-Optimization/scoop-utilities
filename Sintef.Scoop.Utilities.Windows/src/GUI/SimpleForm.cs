//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Threading;
using System.Windows.Forms;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Generic simple form class that takes the user control of your choice as input and displays this.
	/// Has a "close" button. The idea is that for simple visualisation forms, there is no need to develop the form
	/// itself. Instead, one can develop a user control that can be used with SimpleForm, and which can
	/// also be embedded in other forms.
	/// </summary>
	public partial class SimpleForm : Form
	{
		/// <summary>
		/// The user control to display
		/// </summary>
		public Control Control { get; set; }

		/// <summary>
		/// Default constructor. Don't use this.
		/// </summary>
		protected SimpleForm()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Constructor, taking a user control to display as input
		/// </summary>
		/// <param name="control">The control to display</param>
		/// <param name="title">Title for the form. If null, the control's Name is used.</param>
		public SimpleForm(Control control, string title)
		{
			InitializeComponent();
			InitializeUseOfControl(control);
			if (title != null)
				Text = title;
		}

		/// <summary>
		/// Replaces the control shown in the form
		/// </summary>
		/// <param name="control">The new control to show</param>
		public void ReplaceControl(Control control)
		{
			_splitContainerMain.Panel1.Controls.Clear();

			InitializeUseOfControl(control);
		}

		/// <summary>
		/// Closes the form
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _buttonClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Sets the form up to use the given control
		/// </summary>
		/// <param name="ctrl"></param>
		private void InitializeUseOfControl(Control ctrl)
		{
			this.Width = ctrl.Width + 10;
      this.Height = ctrl.Height + 100;
			Control = ctrl;
			_splitContainerMain.Panel1.Controls.Add(Control);
			Control.Dock = System.Windows.Forms.DockStyle.Fill;
			Control.Location = new System.Drawing.Point(0, 0);
			Control.Name = "_userControl";
			Control.Size = new System.Drawing.Size(675, 464);
			Control.TabIndex = 0;
		}

		/// <summary>
		/// Copies an bitmap image of the contents of the form to the clipboard, in a separate thread.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _buttonCopyImage_Click(object sender, EventArgs e)
		{
			System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(Control.Width, Control.Height);
			Control.DrawToBitmap(bmp, Control.ClientRectangle);
			ThreadStart ts = new ThreadStart(() => { Clipboard.SetImage(bmp); } );
			Thread t = new Thread(ts);
			t.SetApartmentState(ApartmentState.STA);
			t.Start();
		}
	}
}
