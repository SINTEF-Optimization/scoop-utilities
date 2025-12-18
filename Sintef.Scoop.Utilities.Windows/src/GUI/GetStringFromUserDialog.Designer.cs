namespace Sintef.Scoop.Utilities.GUI
{
	partial class GetStringFromUserDialog
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
			this._textBoxInput = new System.Windows.Forms.TextBox();
			this._textBoxDescription = new System.Windows.Forms.TextBox();
			this._buttonCancel = new System.Windows.Forms.Button();
			this._buttonOK = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// _textBoxInput
			// 
			this._textBoxInput.Location = new System.Drawing.Point(12, 112);
			this._textBoxInput.Name = "_textBoxInput";
			this._textBoxInput.Size = new System.Drawing.Size(525, 20);
			this._textBoxInput.TabIndex = 0;
			// 
			// _textBoxDescription
			// 
			this._textBoxDescription.Location = new System.Drawing.Point(12, 15);
			this._textBoxDescription.Multiline = true;
			this._textBoxDescription.Name = "_textBoxDescription";
			this._textBoxDescription.ReadOnly = true;
			this._textBoxDescription.Size = new System.Drawing.Size(524, 86);
			this._textBoxDescription.TabIndex = 1;
			// 
			// _buttonCancel
			// 
			this._buttonCancel.Location = new System.Drawing.Point(413, 146);
			this._buttonCancel.Name = "_buttonCancel";
			this._buttonCancel.Size = new System.Drawing.Size(55, 35);
			this._buttonCancel.TabIndex = 2;
			this._buttonCancel.Text = "Cancel";
			this._buttonCancel.UseVisualStyleBackColor = true;
			// 
			// _buttonOK
			// 
			this._buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._buttonOK.Location = new System.Drawing.Point(481, 146);
			this._buttonOK.Name = "_buttonOK";
			this._buttonOK.Size = new System.Drawing.Size(55, 35);
			this._buttonOK.TabIndex = 3;
			this._buttonOK.Text = "OK";
			this._buttonOK.UseVisualStyleBackColor = true;
			// 
			// GetStringFromUserDialog
			// 
			this.AcceptButton = this._buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this._buttonCancel;
			this.ClientSize = new System.Drawing.Size(551, 196);
			this.Controls.Add(this._buttonOK);
			this.Controls.Add(this._buttonCancel);
			this.Controls.Add(this._textBoxDescription);
			this.Controls.Add(this._textBoxInput);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "GetStringFromUserDialog";
			this.Text = "GetStringFromUserDialog";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox _textBoxInput;
		private System.Windows.Forms.TextBox _textBoxDescription;
		private System.Windows.Forms.Button _buttonCancel;
		private System.Windows.Forms.Button _buttonOK;
	}
}