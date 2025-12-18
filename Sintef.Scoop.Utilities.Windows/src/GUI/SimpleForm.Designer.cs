namespace Sintef.Scoop.Utilities
{
	partial class SimpleForm
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
			this._splitContainerMain = new System.Windows.Forms.SplitContainer();
			this._buttonClose = new System.Windows.Forms.Button();
			this._buttonCopyImage = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this._splitContainerMain)).BeginInit();
			this._splitContainerMain.Panel2.SuspendLayout();
			this._splitContainerMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// _splitContainerMain
			// 
			this._splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this._splitContainerMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this._splitContainerMain.IsSplitterFixed = true;
			this._splitContainerMain.Location = new System.Drawing.Point(0, 0);
			this._splitContainerMain.Name = "_splitContainerMain";
			this._splitContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// _splitContainerMain.Panel2
			// 
			this._splitContainerMain.Panel2.Controls.Add(this._buttonCopyImage);
			this._splitContainerMain.Panel2.Controls.Add(this._buttonClose);
			this._splitContainerMain.Size = new System.Drawing.Size(675, 511);
			this._splitContainerMain.SplitterDistance = 477;
			this._splitContainerMain.TabIndex = 0;
			// 
			// _buttonClose
			// 
			this._buttonClose.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this._buttonClose.Location = new System.Drawing.Point(290, 2);
			this._buttonClose.Name = "_buttonClose";
			this._buttonClose.Size = new System.Drawing.Size(92, 25);
			this._buttonClose.TabIndex = 0;
			this._buttonClose.Text = "Close";
			this._buttonClose.UseVisualStyleBackColor = true;
			this._buttonClose.Click += new System.EventHandler(this._buttonClose_Click);
			// 
			// _buttonCopyImage
			// 
			this._buttonCopyImage.Location = new System.Drawing.Point(14, 5);
			this._buttonCopyImage.Name = "_buttonCopyImage";
			this._buttonCopyImage.Size = new System.Drawing.Size(84, 21);
			this._buttonCopyImage.TabIndex = 1;
			this._buttonCopyImage.Text = "Copy image";
			this._buttonCopyImage.UseVisualStyleBackColor = true;
			this._buttonCopyImage.Click += new System.EventHandler(this._buttonCopyImage_Click);
			// 
			// SimpleForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(675, 511);
			this.Controls.Add(this._splitContainerMain);
			this.Name = "SimpleForm";
			this.Text = "SimpleForm";
			this._splitContainerMain.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this._splitContainerMain)).EndInit();
			this._splitContainerMain.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer _splitContainerMain;
		private System.Windows.Forms.Button _buttonClose;
		private System.Windows.Forms.Button _buttonCopyImage;
	}
}