namespace Sintef.Scoop.Utilities.GUI
{
	partial class MultiViewControl
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
			this._tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.SuspendLayout();
			// 
			// _tableLayoutPanel
			// 
			this._tableLayoutPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
			this._tableLayoutPanel.ColumnCount = 1;
			this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this._tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
			this._tableLayoutPanel.Name = "_tableLayoutPanel";
			this._tableLayoutPanel.RowCount = 1;
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._tableLayoutPanel.Size = new System.Drawing.Size(984, 622);
			this._tableLayoutPanel.TabIndex = 2;
			// 
			// MultiViewControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._tableLayoutPanel);
			this.Name = "MultiViewControl";
			this.Size = new System.Drawing.Size(984, 622);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel _tableLayoutPanel;
	}
}
