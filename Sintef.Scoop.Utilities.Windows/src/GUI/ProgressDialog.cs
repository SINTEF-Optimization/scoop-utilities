//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Drawing;
using System.Windows.Forms;

#pragma warning disable 1591

namespace Sintef.Scoop.Utilities
{
  /// <summary>
  /// A progress dialog. No multithreading supported.
  /// </summary>
  public partial class ProgressDialog : Form
  {
    public enum LayOut
    {
      PLAIN,
      WITH_TEXT_BOX,
      WITH_ENTERTAINMENT,
      WITH_TEXT_AND_ENTERTAINMENT
    }

    public ProgressDialog()
    {
      InitializeComponent();
    }

    public void Open(int maxSteps, string title, string explanation, LayOut layout)
    {
			base.Show();

			//Invoke((Action)delegate
			{
				Reset(maxSteps, explanation);
				this.Text = title;
				this.BringToFront();

				if (layout == LayOut.WITH_ENTERTAINMENT || layout == LayOut.WITH_TEXT_AND_ENTERTAINMENT)
				{
					_webBrowser.Navigate("http://xkcd.com/909/");
					_webBrowser.Refresh();

					if (layout == LayOut.WITH_TEXT_AND_ENTERTAINMENT)
					{
						_textBoxExplanation.Visible = true;
					}
					else
						_textBoxExplanation.Visible = false;
				}
				else if (layout == LayOut.WITH_TEXT_BOX)
				{
					Height = 300;
					_webBrowser.Visible = false;
					_labelEntertainment.Visible = false;
					_textBoxExplanation.Visible = true;
					_textBoxExplanation.Visible = true;
				}
				else
				{
					Width = 1000;
					Height = 100;
					_progressBar.Location = new Point(_progressBar.Location.X, 20);
					_webBrowser.Visible = false;
					_labelEntertainment.Visible = false;
					_textBoxExplanation.Visible = false;
				}

			//});
    }
		}

    public void Step()
    {
      Invoke((Action)delegate
     {
       _progressBar.PerformStep();
     });
    }

    public void Write(string text)
    {
      Invoke((Action)delegate
     {
       _textBoxExplanation.AppendText(text);
     });
    }

    public void WriteLine(string text)
    {
      Invoke((Action)delegate
      {
        _textBoxExplanation.AppendText(text + "\n");
      });
    }

    
    public new void Show()
    {
       throw new Exception("ProgressDialog.Show: Use Open instead");
    }

    private void _webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
    {

    }

    /// <summary>
    /// Resets the progress bar, and set's it's max value to maxSteps
    /// </summary>
    public void Reset(int maxSteps, string text)
    {
      Invoke((Action)delegate
     {
       Write(text);
       _progressBar.Value = 0;
       _progressBar.Maximum = maxSteps;
       _progressBar.Minimum = 0;
     });
    }
  }
}

#pragma warning restore 1591
