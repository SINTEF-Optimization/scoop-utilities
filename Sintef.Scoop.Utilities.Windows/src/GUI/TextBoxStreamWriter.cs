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
	/// A text writer that writes/updates to a given TextBox at regular time intervals.
	/// Handy for (e.g.) writing console output to a text box efficiently.
	/// </summary>
	public class TextBoxStreamWriter : TextWriter
	{
		TextBox _output = null;

		/// <summary>
		/// The console text
		/// </summary>
		string _textBuffer = "";

		/// <summary>
		/// A timer for updating the console.
		/// </summary>
		System.Windows.Forms.Timer _outputUpdateTimer = new System.Windows.Forms.Timer();

		/// <summary>
		/// Flag to signify if anything was written to console since the last update.
		/// </summary>
		bool _textChanged = false;

		/// <summary>
		/// Initializes the writer
		/// </summary>
		public TextBoxStreamWriter(TextBox output)
		{
			_output = output;
			_outputUpdateTimer.Tick += new EventHandler(UpdateTextBox);
			_outputUpdateTimer.Interval = 500;
			_outputUpdateTimer.Start();
		}

		/// <summary>
		/// Writes a character
		/// </summary>
		public override void Write(char value)
		{
			////Disabling for profiling
			//return;


			lock (_textBuffer)
			{
				if (value == '\n')
					_textBuffer += '\r';				
				_textBuffer += value;
				_textChanged = true;
			}
			//_output.BeginInvoke((Action)delegate
			//{
			//  //base.Write(value);
			//  _output.AppendText(value.ToString()); // When character data is written, append it to the text box.
			//});
		}

		/// <summary>
		/// Writes a character and then a newline
		/// </summary>
		public override void WriteLine(char value)
		{
			if (value == '\n')
				Write(value);
			else
				Write(value + "\r\n");
			////base.WriteLine(value);
			//_output.BeginInvoke((Action)delegate
			//{
			//   _output.AppendText(value.ToString()+"\n"); // When character data is written, append it to the text box.
			//});
		}

		/// <summary>
		/// Writes a string
		/// </summary>
		public override void WriteLine(string value)
		{
			string mod = value.Replace("\n", "\r\n");
			Write(mod + "\r\n");
			////base.WriteLine(value);
			//_output.BeginInvoke((Action)delegate
			//{
			//  _output.AppendText(value + "\n"); // When character data is written, append it to the text box.
			//});
		}

		/// <summary>
		/// The writer's encoding
		/// </summary>
		public override System.Text.Encoding Encoding
		{
			get { return System.Text.Encoding.UTF8; }
		}

		/// <summary>
		/// Updates the console when console timer events happen.
		/// </summary>
		/// <param name="myObject"></param>
		/// <param name="args"></param>
		private void UpdateTextBox(Object myObject, EventArgs args)
		{
			if (_textChanged)
			{
				_outputUpdateTimer.Stop();
				lock (_textBuffer)
				{
					_output.Text = _textBuffer;
					_output.SelectionStart = _output.Text.Length;
					_output.ScrollToCaret();
					_textChanged = false;
				}
				_outputUpdateTimer.Start();
			}
		}
	}

}
