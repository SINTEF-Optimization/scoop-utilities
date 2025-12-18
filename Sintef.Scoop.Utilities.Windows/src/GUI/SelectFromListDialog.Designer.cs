namespace Sintef.Scoop.Utilities
{
	partial class SelectFromListDialog
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
			this._listBox = new System.Windows.Forms.ListBox();
			this._cancel = new System.Windows.Forms.Button();
			this._ok = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// _listBox
			// 
			this._listBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._listBox.FormattingEnabled = true;
			this._listBox.Location = new System.Drawing.Point(1, 0);
			this._listBox.Name = "_listBox";
			this._listBox.Size = new System.Drawing.Size(338, 238);
			this._listBox.TabIndex = 0;
			this._listBox.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this._listBox_MouseDoubleClick);
			// 
			// _cancel
			// 
			this._cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancel.Location = new System.Drawing.Point(251, 249);
			this._cancel.Name = "_cancel";
			this._cancel.Size = new System.Drawing.Size(75, 23);
			this._cancel.TabIndex = 2;
			this._cancel.Text = "Cancel";
			this._cancel.UseVisualStyleBackColor = true;
			// 
			// _ok
			// 
			this._ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._ok.Location = new System.Drawing.Point(12, 249);
			this._ok.Name = "_ok";
			this._ok.Size = new System.Drawing.Size(75, 23);
			this._ok.TabIndex = 3;
			this._ok.Text = "OK";
			this._ok.UseVisualStyleBackColor = true;
			// 
			// SelectFromListDialog
			// 
			this.AcceptButton = this._ok;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this._cancel;
			this.ClientSize = new System.Drawing.Size(338, 284);
			this.Controls.Add(this._ok);
			this.Controls.Add(this._cancel);
			this.Controls.Add(this._listBox);
			this.Name = "SelectFromListDialog";
			this.Text = "SelectFromListDialog";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListBox _listBox;
		private System.Windows.Forms.Button _cancel;
		private System.Windows.Forms.Button _ok;
	}
}