namespace Sintef.Scoop.Utilities
{
	partial class TextInputForm
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
			this._labelQuestion = new System.Windows.Forms.Label();
			this._textBoxInput = new System.Windows.Forms.TextBox();
			this._buttonOK = new System.Windows.Forms.Button();
			this._buttonCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// _labelQuestion
			// 
			this._labelQuestion.AutoSize = true;
			this._labelQuestion.Location = new System.Drawing.Point(12, 9);
			this._labelQuestion.Name = "_labelQuestion";
			this._labelQuestion.Size = new System.Drawing.Size(35, 13);
			this._labelQuestion.TabIndex = 0;
			this._labelQuestion.Text = "label1";
			// 
			// _textBoxInput
			// 
			this._textBoxInput.Location = new System.Drawing.Point(12, 38);
			this._textBoxInput.Name = "_textBoxInput";
			this._textBoxInput.Size = new System.Drawing.Size(558, 20);
			this._textBoxInput.TabIndex = 1;
			// 
			// _buttonOK
			// 
			this._buttonOK.Location = new System.Drawing.Point(516, 78);
			this._buttonOK.Name = "_buttonOK";
			this._buttonOK.Size = new System.Drawing.Size(53, 26);
			this._buttonOK.TabIndex = 2;
			this._buttonOK.Text = "OK";
			this._buttonOK.UseVisualStyleBackColor = true;
			this._buttonOK.Click += new System.EventHandler(this._buttonOK_Click);
			// 
			// _buttonCancel
			// 
			this._buttonCancel.Location = new System.Drawing.Point(452, 78);
			this._buttonCancel.Name = "_buttonCancel";
			this._buttonCancel.Size = new System.Drawing.Size(58, 26);
			this._buttonCancel.TabIndex = 3;
			this._buttonCancel.Text = "Cancel";
			this._buttonCancel.UseVisualStyleBackColor = true;
			this._buttonCancel.Click += new System.EventHandler(this._buttonCancel_Click);
			// 
			// TextInputForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(585, 122);
			this.Controls.Add(this._buttonCancel);
			this.Controls.Add(this._buttonOK);
			this.Controls.Add(this._textBoxInput);
			this.Controls.Add(this._labelQuestion);
			this.Name = "TextInputForm";
			this.Text = "TextInputForm";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label _labelQuestion;
		private System.Windows.Forms.TextBox _textBoxInput;
		private System.Windows.Forms.Button _buttonOK;
		private System.Windows.Forms.Button _buttonCancel;
	}
}