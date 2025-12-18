namespace Sintef.Scoop.Utilities
{
	partial class SimpleGraphForm
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
			this._zoom = new System.Windows.Forms.NumericUpDown();
			this._scollbar = new System.Windows.Forms.HScrollBar();
			((System.ComponentModel.ISupportInitialize)(this._zoom)).BeginInit();
			this.SuspendLayout();
		
			// 
			// SimpleGraphForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.ClientSize = new System.Drawing.Size(675, 511);
			this.Name = "SimpleGraphForm";
			this.Text = "SimpleGraphForm";
			((System.ComponentModel.ISupportInitialize)(this._zoom)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.NumericUpDown _zoom;
		private System.Windows.Forms.HScrollBar _scollbar;

	}
}