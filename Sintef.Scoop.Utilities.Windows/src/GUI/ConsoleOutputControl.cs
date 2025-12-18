//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.IO;
using System.Windows.Forms;

namespace Sintef.Scoop.Utilities.GUI
{
	/// <summary>
	/// A simple control that on construction captures console output, and displays
	/// this efficiently during program execution.
	/// Resets the console output on destruction.
	/// </summary>
	public partial class ConsoleOutputControl : UserControl
	{
		/// <summary>
		/// The original output for the console.
		/// </summary>
		TextWriter _originalOutput;

		/// <summary>
		/// Initializes a control
		/// </summary>
		public ConsoleOutputControl()
		{
			InitializeComponent();

			// Redirect the out Console stream
			_originalOutput = Console.Out;
			TextBoxStreamWriter writer = new TextBoxStreamWriter(_textBox);
			Console.SetOut(writer);
		}

		/// <summary>
		/// Finalizes a control by restoring console output
		/// </summary>
		~ConsoleOutputControl()
		{
			Console.SetOut(_originalOutput);
		}
	}
}
