namespace Sintef.Scoop.Utilities.GUI
{
	partial class LogControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this._textBoxConsole = new System.Windows.Forms.TextBox();
			this._splitContainerLog = new System.Windows.Forms.SplitContainer();
			this._flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.panel1 = new System.Windows.Forms.Panel();
			this._logLevelShown = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this._logFilterText = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.panel3 = new System.Windows.Forms.Panel();
			this._useRegExpInFilter = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this._splitContainerLog)).BeginInit();
			this._splitContainerLog.Panel1.SuspendLayout();
			this._splitContainerLog.Panel2.SuspendLayout();
			this._splitContainerLog.SuspendLayout();
			this._flowLayoutPanel1.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._logLevelShown)).BeginInit();
			this.panel2.SuspendLayout();
			this.panel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// _textBoxConsole
			// 
			this._textBoxConsole.Dock = System.Windows.Forms.DockStyle.Fill;
			this._textBoxConsole.Location = new System.Drawing.Point(0, 0);
			this._textBoxConsole.Margin = new System.Windows.Forms.Padding(4);
			this._textBoxConsole.Multiline = true;
			this._textBoxConsole.Name = "_textBoxConsole";
			this._textBoxConsole.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this._textBoxConsole.Size = new System.Drawing.Size(715, 619);
			this._textBoxConsole.TabIndex = 0;
			// 
			// _splitContainerLog
			// 
			this._splitContainerLog.Dock = System.Windows.Forms.DockStyle.Fill;
			this._splitContainerLog.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this._splitContainerLog.Location = new System.Drawing.Point(0, 0);
			this._splitContainerLog.Margin = new System.Windows.Forms.Padding(4);
			this._splitContainerLog.Name = "_splitContainerLog";
			this._splitContainerLog.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// _splitContainerLog.Panel1
			// 
			this._splitContainerLog.Panel1.Controls.Add(this._flowLayoutPanel1);
			// 
			// _splitContainerLog.Panel2
			// 
			this._splitContainerLog.Panel2.Controls.Add(this._textBoxConsole);
			this._splitContainerLog.Size = new System.Drawing.Size(715, 649);
			this._splitContainerLog.SplitterDistance = 25;
			this._splitContainerLog.SplitterWidth = 5;
			this._splitContainerLog.TabIndex = 2;
			// 
			// _flowLayoutPanel1
			// 
			this._flowLayoutPanel1.Controls.Add(this.panel1);
			this._flowLayoutPanel1.Controls.Add(this.panel2);
			this._flowLayoutPanel1.Controls.Add(this.panel3);
			this._flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this._flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this._flowLayoutPanel1.Name = "_flowLayoutPanel1";
			this._flowLayoutPanel1.Size = new System.Drawing.Size(715, 25);
			this._flowLayoutPanel1.TabIndex = 0;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this._logLevelShown);
			this.panel1.Controls.Add(this.label2);
			this.panel1.Location = new System.Drawing.Point(3, 3);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(203, 22);
			this.panel1.TabIndex = 0;
			// 
			// _logLevelShown
			// 
			this._logLevelShown.Location = new System.Drawing.Point(149, 0);
			this._logLevelShown.Margin = new System.Windows.Forms.Padding(4);
			this._logLevelShown.Name = "_logLevelShown";
			this._logLevelShown.Size = new System.Drawing.Size(52, 22);
			this._logLevelShown.TabIndex = 4;
			this._logLevelShown.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
			this._logLevelShown.ValueChanged += new System.EventHandler(this._logLevelShown_ValueChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(4, 2);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(137, 17);
			this.label2.TabIndex = 5;
			this.label2.Text = "Max log level shown:";
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this._logFilterText);
			this.panel2.Controls.Add(this.label4);
			this.panel2.Location = new System.Drawing.Point(212, 3);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(249, 22);
			this.panel2.TabIndex = 1;
			// 
			// _logFilterText
			// 
			this._logFilterText.Location = new System.Drawing.Point(82, 0);
			this._logFilterText.Margin = new System.Windows.Forms.Padding(4);
			this._logFilterText.Name = "_logFilterText";
			this._logFilterText.Size = new System.Drawing.Size(156, 22);
			this._logFilterText.TabIndex = 8;
			this._logFilterText.TextChanged += new System.EventHandler(this._logFilterText_TextChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(4, 2);
			this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(70, 17);
			this.label4.TabIndex = 7;
			this.label4.Text = "Text filter:";
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this._useRegExpInFilter);
			this.panel3.Location = new System.Drawing.Point(467, 3);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(191, 22);
			this.panel3.TabIndex = 2;
			// 
			// _useRegExpInFilter
			// 
			this._useRegExpInFilter.AutoSize = true;
			this._useRegExpInFilter.Location = new System.Drawing.Point(0, 1);
			this._useRegExpInFilter.Margin = new System.Windows.Forms.Padding(4);
			this._useRegExpInFilter.Name = "_useRegExpInFilter";
			this._useRegExpInFilter.Size = new System.Drawing.Size(183, 21);
			this._useRegExpInFilter.TabIndex = 9;
			this._useRegExpInFilter.Text = "Use regular expressions";
			this._useRegExpInFilter.UseVisualStyleBackColor = true;
			this._useRegExpInFilter.CheckedChanged += new System.EventHandler(this._useRegExpInFilter_CheckedChanged);
			// 
			// LogControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._splitContainerLog);
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "LogControl";
			this.Size = new System.Drawing.Size(715, 649);
			this._splitContainerLog.Panel1.ResumeLayout(false);
			this._splitContainerLog.Panel2.ResumeLayout(false);
			this._splitContainerLog.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this._splitContainerLog)).EndInit();
			this._splitContainerLog.ResumeLayout(false);
			this._flowLayoutPanel1.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this._logLevelShown)).EndInit();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TextBox _textBoxConsole;
		private System.Windows.Forms.SplitContainer _splitContainerLog;
		private System.Windows.Forms.TextBox _logFilterText;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.NumericUpDown _logLevelShown;
		private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox _useRegExpInFilter;
		private System.Windows.Forms.FlowLayoutPanel _flowLayoutPanel1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Panel panel3;
	}
}
