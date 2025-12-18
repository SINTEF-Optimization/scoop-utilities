namespace Sintef.Scoop.Utilities
{
  partial class ProgressDialog
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this._textBoxExplanation = new System.Windows.Forms.TextBox();
      this._progressBar = new System.Windows.Forms.ProgressBar();
      this._webBrowser = new System.Windows.Forms.WebBrowser();
      this._labelEntertainment = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // _textBoxExplanation
      // 
      this._textBoxExplanation.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
      this._textBoxExplanation.Cursor = System.Windows.Forms.Cursors.Default;
      this._textBoxExplanation.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this._textBoxExplanation.ForeColor = System.Drawing.Color.Yellow;
      this._textBoxExplanation.Location = new System.Drawing.Point(22, 17);
      this._textBoxExplanation.Multiline = true;
      this._textBoxExplanation.Name = "_textBoxExplanation";
      this._textBoxExplanation.ReadOnly = true;
      this._textBoxExplanation.Size = new System.Drawing.Size(908, 191);
      this._textBoxExplanation.TabIndex = 0;
      // 
      // _progressBar
      // 
      this._progressBar.Location = new System.Drawing.Point(22, 228);
      this._progressBar.Name = "_progressBar";
      this._progressBar.Size = new System.Drawing.Size(908, 20);
      this._progressBar.Step = 1;
      this._progressBar.TabIndex = 1;
      // 
      // _webBrowser
      // 
      this._webBrowser.Location = new System.Drawing.Point(22, 291);
      this._webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
      this._webBrowser.Name = "_webBrowser";
      this._webBrowser.Size = new System.Drawing.Size(926, 603);
      this._webBrowser.TabIndex = 2;
      this._webBrowser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this._webBrowser_DocumentCompleted);
      // 
      // _labelEntertainment
      // 
      this._labelEntertainment.AutoSize = true;
      this._labelEntertainment.Location = new System.Drawing.Point(22, 260);
      this._labelEntertainment.Name = "_labelEntertainment";
      this._labelEntertainment.Size = new System.Drawing.Size(211, 13);
      this._labelEntertainment.TabIndex = 3;
      this._labelEntertainment.Text = "Some entertainment while you are waiting...";
      // 
      // ProgressDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(961, 914);
      this.Controls.Add(this._labelEntertainment);
      this.Controls.Add(this._webBrowser);
      this.Controls.Add(this._progressBar);
      this.Controls.Add(this._textBoxExplanation);
      this.Name = "ProgressDialog";
      this.Text = "Progress Dialog";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox _textBoxExplanation;
    private System.Windows.Forms.ProgressBar _progressBar;
    private System.Windows.Forms.WebBrowser _webBrowser;
    private System.Windows.Forms.Label _labelEntertainment;
  }
}