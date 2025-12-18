//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Sintef.Scoop.Utilities.GeoCoding;
using System.Collections.Generic;
using System.Drawing;

namespace Sintef.Scoop.Utilities.GUI
{
	/// <summary>
	/// An item that can be drawn in a NetworkViewControl
	/// </summary>
	/// <typeparam name="C">The type of world coordinates that are used in the application.</typeparam>
	public abstract class DrawableItem<C> where C : ICoordinate
	{
		/// <summary>
		/// Initializes an item
		/// </summary>
		public DrawableItem(int layer) { Layer = layer; Show = true; }

		/// <summary>
		/// The layer. Items with larger layer numbers will be drawn on top of items with smaller layer numbers.
		/// </summary>
		public int Layer { get; set; }

		/// <summary>
		/// Whether this item should be shown at all. True by default
		/// </summary>
		public bool Show { get; set; }

		/// <summary>
		/// The number of pixels that the item can draw outside its Extent.
		/// Defaults to 0.
		/// </summary>
		public float Margin { get; set; }

		/// <summary>
		/// Draws the item.
		/// 
		/// This method is called during the repaint event of the view
		/// for the items that have been registered using AddDrawableItem.
		/// The item can then draw itself using the DrawXXX methods in the
		/// supplied view.
		/// </summary>
		public abstract void Draw(NetworkViewControlGeneric<C> view);

		/// <summary>
		/// A collection of points that more or less cover the extent of the item.
		/// </summary>
		/// <see cref="Margin"/>
		public abstract IEnumerable<C> Extent { get; }

        /// <summary>
        /// This is a optional function that returns a region which relatively precisely defines
        /// what area was covered by this layer when it was last drawn. This is intended to be
        /// implemented by dynamic layers so they can redraw very quickly.
        /// </summary>
        /// <returns></returns>
        public virtual Region UpdateRegion()
        {
            return null;
        }
	}
}
