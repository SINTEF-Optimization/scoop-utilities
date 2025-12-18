namespace Sintef.Scoop.Utilities
{
	partial class SimpleGraph
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
			System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
			System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
			this._chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
			this._buttonChooseSeries = new System.Windows.Forms.Button();
			this._buttonClear = new System.Windows.Forms.Button();
			this._splitContainerChartAndScroll = new System.Windows.Forms.SplitContainer();
			this._splitContainerZoomAndDisplay = new System.Windows.Forms.SplitContainer();
			this._zoom = new System.Windows.Forms.NumericUpDown();
			this._scollbar = new System.Windows.Forms.HScrollBar();
			this._contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this._chart)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this._splitContainerChartAndScroll)).BeginInit();
			this._splitContainerChartAndScroll.Panel1.SuspendLayout();
			this._splitContainerChartAndScroll.Panel2.SuspendLayout();
			this._splitContainerChartAndScroll.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._splitContainerZoomAndDisplay)).BeginInit();
			this._splitContainerZoomAndDisplay.Panel1.SuspendLayout();
			this._splitContainerZoomAndDisplay.Panel2.SuspendLayout();
			this._splitContainerZoomAndDisplay.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._zoom)).BeginInit();
			this._contextMenuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// _chart
			// 
			chartArea1.Name = "ChartArea1";
			this._chart.ChartAreas.Add(chartArea1);
			this._chart.ContextMenuStrip = this._contextMenuStrip1;
			this._chart.Dock = System.Windows.Forms.DockStyle.Fill;
			legend1.Name = "Legend1";
			this._chart.Legends.Add(legend1);
			this._chart.Location = new System.Drawing.Point(0, 0);
			this._chart.Name = "_chart";
			this._chart.Size = new System.Drawing.Size(1044, 517);
			this._chart.TabIndex = 0;
			this._chart.Text = "chart1";
			// 
			// _buttonChooseSeries
			// 
			this._buttonChooseSeries.Location = new System.Drawing.Point(81, 8);
			this._buttonChooseSeries.Name = "_buttonChooseSeries";
			this._buttonChooseSeries.Size = new System.Drawing.Size(72, 24);
			this._buttonChooseSeries.TabIndex = 3;
			this._buttonChooseSeries.Text = "Show...";
			this._buttonChooseSeries.UseVisualStyleBackColor = true;
			this._buttonChooseSeries.Click += new System.EventHandler(this._buttonChooseStatistics_Click);
			// 
			// _buttonClear
			// 
			this._buttonClear.Location = new System.Drawing.Point(3, 7);
			this._buttonClear.Name = "_buttonClear";
			this._buttonClear.Size = new System.Drawing.Size(72, 25);
			this._buttonClear.TabIndex = 0;
			this._buttonClear.Text = "Clear";
			this._buttonClear.UseVisualStyleBackColor = true;
			this._buttonClear.Click += new System.EventHandler(this._buttonClear_Click);
			// 
			// _splitContainerChartAndScroll
			// 
			this._splitContainerChartAndScroll.Dock = System.Windows.Forms.DockStyle.Fill;
			this._splitContainerChartAndScroll.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this._splitContainerChartAndScroll.Location = new System.Drawing.Point(0, 0);
			this._splitContainerChartAndScroll.Name = "_splitContainerChartAndScroll";
			this._splitContainerChartAndScroll.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// _splitContainerChartAndScroll.Panel1
			// 
			this._splitContainerChartAndScroll.Panel1.Controls.Add(this._chart);
			// 
			// _splitContainerChartAndScroll.Panel2
			// 
			this._splitContainerChartAndScroll.Panel2.Controls.Add(this._splitContainerZoomAndDisplay);
			this._splitContainerChartAndScroll.Size = new System.Drawing.Size(1044, 559);
			this._splitContainerChartAndScroll.SplitterDistance = 517;
			this._splitContainerChartAndScroll.TabIndex = 2;
			// 
			// _splitContainerZoomAndDisplay
			// 
			this._splitContainerZoomAndDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
			this._splitContainerZoomAndDisplay.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this._splitContainerZoomAndDisplay.Location = new System.Drawing.Point(0, 0);
			this._splitContainerZoomAndDisplay.Name = "_splitContainerZoomAndDisplay";
			// 
			// _splitContainerZoomAndDisplay.Panel1
			// 
			this._splitContainerZoomAndDisplay.Panel1.Controls.Add(this._zoom);
			this._splitContainerZoomAndDisplay.Panel1.Controls.Add(this._scollbar);
			// 
			// _splitContainerZoomAndDisplay.Panel2
			// 
			this._splitContainerZoomAndDisplay.Panel2.Controls.Add(this._buttonChooseSeries);
			this._splitContainerZoomAndDisplay.Panel2.Controls.Add(this._buttonClear);
			this._splitContainerZoomAndDisplay.Size = new System.Drawing.Size(1044, 38);
			this._splitContainerZoomAndDisplay.SplitterDistance = 880;
			this._splitContainerZoomAndDisplay.TabIndex = 5;
			// 
			// _zoom
			// 
			this._zoom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._zoom.Location = new System.Drawing.Point(817, 10);
			this._zoom.Name = "_zoom";
			this._zoom.Size = new System.Drawing.Size(54, 20);
			this._zoom.TabIndex = 4;
			this._zoom.ValueChanged += new System.EventHandler(this._zoom_ValueChanged);
			// 
			// _scollbar
			// 
			this._scollbar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._scollbar.Location = new System.Drawing.Point(10, 8);
			this._scollbar.Name = "_scollbar";
			this._scollbar.Size = new System.Drawing.Size(796, 25);
			this._scollbar.TabIndex = 3;
			this._scollbar.Scroll += new System.Windows.Forms.ScrollEventHandler(this._scollbar_Scroll);
			// 
			// _contextMenuStrip1
			// 
			this._contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem});
			this._contextMenuStrip1.Name = "contextMenuStrip1";
			this._contextMenuStrip1.Size = new System.Drawing.Size(145, 26);
			// 
			// copyToolStripMenuItem
			// 
			this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
			this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.copyToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
			this.copyToolStripMenuItem.Text = "Copy";
			this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
			// 
			// SimpleGraph
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._splitContainerChartAndScroll);
			this.Name = "SimpleGraph";
			this.Size = new System.Drawing.Size(1044, 559);
			((System.ComponentModel.ISupportInitialize)(this._chart)).EndInit();
			this._splitContainerChartAndScroll.Panel1.ResumeLayout(false);
			this._splitContainerChartAndScroll.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this._splitContainerChartAndScroll)).EndInit();
			this._splitContainerChartAndScroll.ResumeLayout(false);
			this._splitContainerZoomAndDisplay.Panel1.ResumeLayout(false);
			this._splitContainerZoomAndDisplay.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this._splitContainerZoomAndDisplay)).EndInit();
			this._splitContainerZoomAndDisplay.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this._zoom)).EndInit();
			this._contextMenuStrip1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataVisualization.Charting.Chart _chart;
		private System.Windows.Forms.Button _buttonClear;
		private System.Windows.Forms.Button _buttonChooseSeries;
		private System.Windows.Forms.SplitContainer _splitContainerChartAndScroll;
		private System.Windows.Forms.NumericUpDown _zoom;
		private System.Windows.Forms.HScrollBar _scollbar;
		private System.Windows.Forms.SplitContainer _splitContainerZoomAndDisplay;
		private System.Windows.Forms.ContextMenuStrip _contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
	}
}
