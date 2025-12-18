namespace Sintef.Scoop.Utilities.GUI
{
	partial class TextForm
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
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this._buttonClose = new System.Windows.Forms.Button();
			this._textBox = new System.Windows.Forms.TextBox();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this._textBox);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this._buttonClose);
			this.splitContainer1.Size = new System.Drawing.Size(727, 614);
			this.splitContainer1.SplitterDistance = 557;
			this.splitContainer1.TabIndex = 0;
			// 
			// _buttonClose
			// 
			this._buttonClose.Location = new System.Drawing.Point(640, 9);
			this._buttonClose.Name = "_buttonClose";
			this._buttonClose.Size = new System.Drawing.Size(79, 36);
			this._buttonClose.TabIndex = 0;
			this._buttonClose.Text = "Close";
			this._buttonClose.UseVisualStyleBackColor = true;
			this._buttonClose.Click += new System.EventHandler(this._buttonClose_Click);
			// 
			// _textBox
			// 
			this._textBox.BackColor = System.Drawing.SystemColors.ButtonHighlight;
			this._textBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this._textBox.Location = new System.Drawing.Point(0, 0);
			this._textBox.Multiline = true;
			this._textBox.Name = "_textBox";
			this._textBox.ReadOnly = true;
			this._textBox.Size = new System.Drawing.Size(727, 557);
			this._textBox.TabIndex = 0;
			// 
			// TextForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(727, 614);
			this.Controls.Add(this.splitContainer1);
			this.Name = "TextForm";
			this.Text = "TextForm";
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.TextBox _textBox;
		private System.Windows.Forms.Button _buttonClose;
	}
}