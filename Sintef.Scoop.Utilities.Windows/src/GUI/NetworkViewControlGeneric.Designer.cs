using Sintef.Scoop.Utilities.GeoCoding;

namespace Sintef.Scoop.Utilities.GUI
{
	partial class NetworkViewControlGeneric<C> where C:ICoordinate
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
			this.components = new System.ComponentModel.Container();
			this._viewPanel = new System.Windows.Forms.Panel();
			this._panDown = new System.Windows.Forms.Button();
			this._panLeft = new System.Windows.Forms.Button();
			this._panRight = new System.Windows.Forms.Button();
			this._zoomIn = new System.Windows.Forms.Button();
			this._zoomOut = new System.Windows.Forms.Button();
			this._panUp = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this._toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this._utmLabel = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this._lon = new System.Windows.Forms.TextBox();
			this._lat = new System.Windows.Forms.TextBox();
			this._utmE = new System.Windows.Forms.TextBox();
			this._utmN = new System.Windows.Forms.TextBox();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// _viewPanel
			// 
			this._viewPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._viewPanel.BackColor = System.Drawing.Color.White;
			this._viewPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this._viewPanel.Cursor = System.Windows.Forms.Cursors.Arrow;
			this._viewPanel.Location = new System.Drawing.Point(3, 3);
			this._viewPanel.Name = "_viewPanel";
			this._viewPanel.Size = new System.Drawing.Size(661, 348);
			this._viewPanel.TabIndex = 0;
			this._viewPanel.Paint += new System.Windows.Forms.PaintEventHandler(this._viewPanel_Paint);
			this._viewPanel.MouseClick += new System.Windows.Forms.MouseEventHandler(this._viewPanel_MouseClick);
			this._viewPanel.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this._viewPanel_MouseDoubleClick);
			this._viewPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this._viewPanel_MouseDown);
			this._viewPanel.MouseEnter += new System.EventHandler(this._viewPanel_MouseEnter);
			this._viewPanel.MouseLeave += new System.EventHandler(this._viewPanel_MouseLeave);
			this._viewPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this._viewPanel_MouseMove);
			this._viewPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this._viewPanel_MouseUp);
			this._viewPanel.Resize += new System.EventHandler(this._viewPanel_Resize);
			// 
			// _panDown
			// 
			this._panDown.Location = new System.Drawing.Point(53, 38);
			this._panDown.Name = "_panDown";
			this._panDown.Size = new System.Drawing.Size(46, 23);
			this._panDown.TabIndex = 2;
			this._panDown.Text = "Down";
			this._panDown.UseVisualStyleBackColor = true;
			this._panDown.Click += new System.EventHandler(this._panDown_Click);
			// 
			// _panLeft
			// 
			this._panLeft.Location = new System.Drawing.Point(6, 22);
			this._panLeft.Name = "_panLeft";
			this._panLeft.Size = new System.Drawing.Size(41, 23);
			this._panLeft.TabIndex = 3;
			this._panLeft.Text = "Left";
			this._panLeft.UseVisualStyleBackColor = true;
			this._panLeft.Click += new System.EventHandler(this._panLeft_Click);
			// 
			// _panRight
			// 
			this._panRight.Location = new System.Drawing.Point(105, 22);
			this._panRight.Name = "_panRight";
			this._panRight.Size = new System.Drawing.Size(41, 23);
			this._panRight.TabIndex = 4;
			this._panRight.Text = "Right";
			this._panRight.UseVisualStyleBackColor = true;
			this._panRight.Click += new System.EventHandler(this._panRight_Click);
			// 
			// _zoomIn
			// 
			this._zoomIn.Location = new System.Drawing.Point(14, 22);
			this._zoomIn.Name = "_zoomIn";
			this._zoomIn.Size = new System.Drawing.Size(47, 23);
			this._zoomIn.TabIndex = 5;
			this._zoomIn.Text = "In";
			this._zoomIn.UseVisualStyleBackColor = true;
			this._zoomIn.Click += new System.EventHandler(this._zoomIn_Click);
			// 
			// _zoomOut
			// 
			this._zoomOut.Location = new System.Drawing.Point(69, 22);
			this._zoomOut.Name = "_zoomOut";
			this._zoomOut.Size = new System.Drawing.Size(47, 23);
			this._zoomOut.TabIndex = 6;
			this._zoomOut.Text = "Out";
			this._zoomOut.UseVisualStyleBackColor = true;
			this._zoomOut.Click += new System.EventHandler(this._zoomOut_Click);
			// 
			// _panUp
			// 
			this._panUp.Location = new System.Drawing.Point(53, 9);
			this._panUp.Name = "_panUp";
			this._panUp.Size = new System.Drawing.Size(46, 23);
			this._panUp.TabIndex = 7;
			this._panUp.Text = "Up";
			this._panUp.UseVisualStyleBackColor = true;
			this._panUp.Click += new System.EventHandler(this._panUp_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox1.Controls.Add(this._panRight);
			this.groupBox1.Controls.Add(this._panDown);
			this.groupBox1.Controls.Add(this._panUp);
			this.groupBox1.Controls.Add(this._panLeft);
			this.groupBox1.Location = new System.Drawing.Point(14, 351);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(152, 65);
			this.groupBox1.TabIndex = 8;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Pan";
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox2.Controls.Add(this._zoomOut);
			this.groupBox2.Controls.Add(this._zoomIn);
			this.groupBox2.Location = new System.Drawing.Point(172, 351);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(128, 65);
			this.groupBox2.TabIndex = 9;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Zoom";
			// 
			// _toolTip
			// 
			this._toolTip.AutomaticDelay = 50;
			this._toolTip.AutoPopDelay = 5000;
			this._toolTip.InitialDelay = 50;
			this._toolTip.ReshowDelay = 10;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(358, 365);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(25, 13);
			this.label1.TabIndex = 10;
			this.label1.Text = "Lat:";
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(494, 365);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(28, 13);
			this.label2.TabIndex = 11;
			this.label2.Text = "Lon:";
			// 
			// _utmLabel
			// 
			this._utmLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._utmLabel.AutoSize = true;
			this._utmLabel.Location = new System.Drawing.Point(324, 395);
			this._utmLabel.Name = "_utmLabel";
			this._utmLabel.Size = new System.Drawing.Size(59, 13);
			this._utmLabel.TabIndex = 12;
			this._utmLabel.Text = "UTM 33 E:";
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(504, 395);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(18, 13);
			this.label4.TabIndex = 13;
			this.label4.Text = "N:";
			// 
			// _lon
			// 
			this._lon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._lon.Location = new System.Drawing.Point(528, 362);
			this._lon.Name = "_lon";
			this._lon.ReadOnly = true;
			this._lon.Size = new System.Drawing.Size(100, 20);
			this._lon.TabIndex = 14;
			// 
			// _lat
			// 
			this._lat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._lat.Location = new System.Drawing.Point(389, 362);
			this._lat.Name = "_lat";
			this._lat.ReadOnly = true;
			this._lat.Size = new System.Drawing.Size(100, 20);
			this._lat.TabIndex = 15;
			// 
			// _utmE
			// 
			this._utmE.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._utmE.Location = new System.Drawing.Point(389, 392);
			this._utmE.Name = "_utmE";
			this._utmE.ReadOnly = true;
			this._utmE.Size = new System.Drawing.Size(100, 20);
			this._utmE.TabIndex = 16;
			// 
			// _utmN
			// 
			this._utmN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._utmN.Location = new System.Drawing.Point(528, 392);
			this._utmN.Name = "_utmN";
			this._utmN.ReadOnly = true;
			this._utmN.Size = new System.Drawing.Size(100, 20);
			this._utmN.TabIndex = 17;
			// 
			// NetworkViewControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._viewPanel);
			this.Controls.Add(this._utmN);
			this.Controls.Add(this._utmE);
			this.Controls.Add(this._lat);
			this.Controls.Add(this._lon);
			this.Controls.Add(this.label4);
			this.Controls.Add(this._utmLabel);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Name = "NetworkViewControl";
			this.Size = new System.Drawing.Size(667, 418);
			this.Load += new System.EventHandler(this.NetworkViewControl_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

        private System.Windows.Forms.Button _panDown;
		private System.Windows.Forms.Button _panLeft;
		private System.Windows.Forms.Button _panRight;
		private System.Windows.Forms.Button _zoomIn;
		private System.Windows.Forms.Button _zoomOut;
		private System.Windows.Forms.Button _panUp;
		private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Panel _viewPanel;
		  private System.Windows.Forms.ToolTip _toolTip;
			private System.Windows.Forms.Label label1;
			private System.Windows.Forms.Label label2;
			private System.Windows.Forms.Label _utmLabel;
			private System.Windows.Forms.Label label4;
			private System.Windows.Forms.TextBox _lon;
			private System.Windows.Forms.TextBox _lat;
			private System.Windows.Forms.TextBox _utmE;
			private System.Windows.Forms.TextBox _utmN;
	}
}
