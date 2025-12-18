//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Sintef.Scoop.Utilities.GUI
{
	/// <summary>
	/// Control for displaying a simulation log
	/// </summary>
	public partial class LogControl : UserControl
	{
		#region Properties and members

		/// <summary>
		/// The source of the log entries to display
		/// </summary>
		private GeneralLogger _logger;

		/// <summary>
		/// The log text that has been received but not yet displayed
		/// </summary>
		private StringBuilder _newLogText = new StringBuilder();

		/// <summary>
		/// True if text has been added to the log since the last display update.
		/// </summary>
		private bool _logTextChanged = false;

		/// <summary>
		/// A timer for updating the display.
		/// </summary>
		private Timer _updateTimer = new Timer();

		/// <summary>
		/// The time format to use
		/// </summary>
		public string TimeFormat { get; set; }
		#endregion

		#region Construction

		/// <summary>
		/// Initializes the control
		/// </summary>
		public LogControl()
		{
			InitializeComponent();

			TimeFormat = "yyyy-MM-dd HH\\:mm\\:ss.fffffff";

			_updateTimer.Tick += new EventHandler(UpdateLogEventHandler);
			_updateTimer.Interval = 100;
			_updateTimer.Start();
		}

		#endregion

		#region Public functions

		/// <summary>
		/// Makes the control show log items from the given logger.
		/// The display updates automatically each second.
		/// </summary>
		public void Show(GeneralLogger logger)
		{
			if (_logger != null)
				_logger.EntryDisplayed -= AddLogEntry;

			_logger = logger;

			if (_logger != null)
				_logger.EntryDisplayed += AddLogEntry;

			RefreshLog();
		}

		/// <summary>
		/// Updates the display with the latest log messages, independently
		/// of the timer
		/// </summary>
		public void UpdateLog()
		{
			UpdateLogEventHandler(null, null);
		}

		/// <summary>
		/// Set the level limit of shown entries
		/// </summary>
		/// <param name="limit"></param>
		public void SetLevelLimit(int limit)
		{
			if (limit > 0)
				_logLevelShown.Value = limit;
		}

		#endregion

		#region Private functions

		/// <summary>
		/// Adds a log entry for display on the next timer tick
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void AddLogEntry(object sender, LogEntryEventArgs e)
		{
			GLogEntry entry = e.LogEntry;
			string text = entry.ToString(TimeFormat) + "\r\n";
			lock (_newLogText)
			{
				_newLogText.Append(text);
				_logTextChanged = true;
			}
		}

		/// <summary>
		/// Updates the display when timer events happen.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void UpdateLogEventHandler(Object sender, EventArgs args)
		{
			if (_logTextChanged)
			{
				_updateTimer.Stop();
				lock (_newLogText)
				{
					_textBoxConsole.AppendText(_newLogText.ToString());
					_newLogText = new StringBuilder();
					_logTextChanged = false;
				}
				_updateTimer.Start();
			}
		}

		/// <summary>
		/// Updates the logger's filter settings from the GUI and displays the new set
		/// of filtered log entries 
		/// </summary>
		private void RefreshLog()
		{
			if (_logger == null)
			{
				_textBoxConsole.Text = "";
				return;
			}

			lock (_newLogText)
			{
				try
				{
					_logger.LogLevel = (int)_logLevelShown.Value;
					_logger.FilterText = _logFilterText.Text;
					_logger.FilterRegEx = null;
					if (_useRegExpInFilter.Checked && _logFilterText.Text != null && _logFilterText.Text != "")
					{
						try
						{
							_logger.FilterRegEx = new Regex(_logFilterText.Text);
						}
						catch (Exception)
						{
							_logger.FilterRegEx = null;
						}
					}

					StringBuilder buffer = new StringBuilder();
					foreach (GLogEntry entry in _logger.FilteredEntries)
					{
						buffer.Append(entry.ToString(TimeFormat)).Append("\r\n");
					}

					_textBoxConsole.Text = buffer.ToString();

					//Scroll to bottom
					_textBoxConsole.SelectionStart = _textBoxConsole.Text.Length;
					_textBoxConsole.ScrollToCaret();

					_newLogText = new StringBuilder();
				}
				catch (Exception)
				{
				}
			}
		}

		private void _logLevelShown_ValueChanged(object sender, EventArgs e)
		{
			if (_logLevelShown.Value < 0)
			{
				_logLevelShown.Value = 0;
			}

			RefreshLog();
		}

		private void _logFilterText_TextChanged(object sender, EventArgs e)
		{
			RefreshLog();
		}

		private void _useRegExpInFilter_CheckedChanged(object sender, EventArgs e)
		{
			RefreshLog();
		}

		#endregion

	}
}
