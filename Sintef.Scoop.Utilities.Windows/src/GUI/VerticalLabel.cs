//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System.Drawing;
using System.Windows.Forms;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Label drawing text vertically instead of horizontally
	/// </summary>
	public class VerticalLabel : Label
	{
		/// <summary>
		/// ways to orient vertical text
		/// </summary>
		public enum TextOrientations
		{
			/// <summary>
			/// can be read when looking from right
			/// </summary>
			ReadFromRight, 

			/// <summary>
			/// can be read when looking from left
			/// </summary>
			ReadFromLeft
		}

		/// <summary>
		/// which textorentation to use
		/// </summary>
		public TextOrientations TextOrientation { get; set; }

		/// <summary>
		/// Initializes the label
		/// </summary>
		public VerticalLabel()
			: base()
		{
			TextOrientation = TextOrientations.ReadFromRight;
		}

		/// <summary>
		/// Paints the label
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPaint(PaintEventArgs e)
		{
			StringFormat format = new StringFormat();
			format.FormatFlags = StringFormatFlags.DirectionVertical;
			switch(TextAlign)
			{
				case ContentAlignment.BottomCenter:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.TopCenter:
					format.Alignment = StringAlignment.Center;
					break;
				case ContentAlignment.BottomLeft:
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.TopLeft:
					format.Alignment = StringAlignment.Near;
					break;
				case ContentAlignment.BottomRight:
				case ContentAlignment.MiddleRight:
				case ContentAlignment.TopRight:
					format.Alignment = StringAlignment.Far;
					break;
			}
			switch(TextAlign)
			{
				case ContentAlignment.BottomCenter:
				case ContentAlignment.BottomLeft:
				case ContentAlignment.BottomRight:
					format.LineAlignment = StringAlignment.Near;
					break;
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.MiddleRight:
					format.LineAlignment = StringAlignment.Center;
					break;
				case ContentAlignment.TopCenter:
				case ContentAlignment.TopLeft:
				case ContentAlignment.TopRight:
					format.LineAlignment = StringAlignment.Far;
					break;
			}
			Brush brush = new SolidBrush(this.ForeColor);
			if (TextOrientation == TextOrientations.ReadFromRight)
			{
				e.Graphics.RotateTransform(180, System.Drawing.Drawing2D.MatrixOrder.Append);
				e.Graphics.TranslateTransform(ClientRectangle.Width, ClientRectangle.Height, System.Drawing.Drawing2D.MatrixOrder.Append);
			}
			e.Graphics.DrawString(this.Text, this.Font, brush, ClientRectangle, format);
			//e.Graphics.DrawRectangle(new Pen(brush), e.ClipRectangle);
		}

		/// <summary>
		/// Returns the label's preferred size
		/// </summary>
		public override Size GetPreferredSize(Size proposedSize)
		{
			Size s = base.GetPreferredSize(proposedSize);
			return new Size(s.Height, s.Width);
		}
	}
}
