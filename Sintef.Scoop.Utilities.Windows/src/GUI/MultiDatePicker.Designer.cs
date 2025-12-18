namespace Sintef.Scoop.Utilities.GUI
{
	partial class MultiDatePicker
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
			this._date = new System.Windows.Forms.DateTimePicker();
			this._dateList = new System.Windows.Forms.ListBox();
			this._add = new System.Windows.Forms.Button();
			this._remove = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// _date
			// 
			this._date.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._date.Location = new System.Drawing.Point(0, 340);
			this._date.Name = "_date";
			this._date.Size = new System.Drawing.Size(138, 20);
			this._date.TabIndex = 14;
			// 
			// _dateList
			// 
			this._dateList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._dateList.FormattingEnabled = true;
			this._dateList.Location = new System.Drawing.Point(0, 0);
			this._dateList.Name = "_dateList";
			this._dateList.Size = new System.Drawing.Size(326, 329);
			this._dateList.TabIndex = 15;
			// 
			// _add
			// 
			this._add.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._add.Location = new System.Drawing.Point(144, 341);
			this._add.Name = "_add";
			this._add.Size = new System.Drawing.Size(75, 23);
			this._add.TabIndex = 16;
			this._add.Text = "Add";
			this._add.UseVisualStyleBackColor = true;
			this._add.Click += new System.EventHandler(this._add_Click);
			// 
			// _remove
			// 
			this._remove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._remove.Location = new System.Drawing.Point(251, 341);
			this._remove.Name = "_remove";
			this._remove.Size = new System.Drawing.Size(75, 23);
			this._remove.TabIndex = 17;
			this._remove.Text = "Remove";
			this._remove.UseVisualStyleBackColor = true;
			this._remove.Click += new System.EventHandler(this._remove_Click);
			// 
			// MultiDatePicker
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._remove);
			this.Controls.Add(this._add);
			this.Controls.Add(this._dateList);
			this.Controls.Add(this._date);
			this.Name = "MultiDatePicker";
			this.Size = new System.Drawing.Size(326, 364);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DateTimePicker _date;
		private System.Windows.Forms.ListBox _dateList;
		private System.Windows.Forms.Button _add;
		private System.Windows.Forms.Button _remove;
	}
}
