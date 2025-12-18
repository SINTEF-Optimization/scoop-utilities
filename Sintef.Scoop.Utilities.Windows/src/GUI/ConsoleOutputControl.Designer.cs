namespace Sintef.Scoop.Utilities.GUI
{
	partial class ConsoleOutputControl
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
			this._textBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// _textBox
			// 
			this._textBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this._textBox.Location = new System.Drawing.Point(0, 0);
			this._textBox.Multiline = true;
			this._textBox.Name = "_textBox";
			this._textBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this._textBox.Size = new System.Drawing.Size(700, 657);
			this._textBox.TabIndex = 0;
			// 
			// ConsoleOutputControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._textBox);
			this.Name = "ConsoleOutputControl";
			this.Size = new System.Drawing.Size(700, 657);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox _textBox;
	}
}
