//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Sintef.Scoop.Utilities.GeoCoding;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;


namespace Sintef.Scoop.Utilities.GUI
{
	/// <summary>
	/// A drawable item that draws all the segments  in a graph.
	/// </summary>
	/// <typeparam name="C">The type of coordinates used in the spatial graph.</typeparam>
	public class SpatialGraphDrawer<C> : DrawableItem<C> where C : ICoordinate
	{
		#region Private members

		/// <summary>
		/// The <see cref="SpatialIndex "/> used in rendering the graph.
		/// </summary>
		SpatialIndex _spatialIndex;

		#endregion

		#region Public properties

		/// <summary>
		/// The graph to show
		/// </summary>
		public SpatialGraph MyGraph { get; set; }
				
		/// <summary>
		/// The function that selects the pen used to draw a road link. The selection
		/// can be made based on arbitrary properties of the road link.
		/// The function may be null or return null, in which case the default drawing style is used.
		/// Input: Arc, default width the arc would be drawn with
		/// </summary>
		public Func<SpatialEdge, float, Pen> PenSelector { get; set; }

		/// <summary>
		/// The colour per road category to use for road links, if no pen selector is set
		/// </summary>
		public Dictionary<int, Color> Colour { get; }

		/// <summary>
		/// The width of the line per road category, if no pen selector is set
		/// </summary>
		public Dictionary<int, float> Width { get; }

		///// <summary>
		///// If true, oneway links are marked by colouring their last part blue, if no pen selector is set
		///// </summary>
		//public bool ShowOneway { get; set; }

		///// <summary>
		///// Which traveller changes to display.
		///// </summary>
		//public bool ShowCarParking { get; set; }

		///// <summary>
		///// Which traveller changes to display.
		///// </summary>
		//public bool ShowBicycleParking { get; set; }

		///// <summary>
		///// Show nodes with no roads
		///// </summary>
		//public bool ShowRoadlessNodes { get; set; }

		/// <summary>
		/// The font of the text
		/// </summary>
		public Font Font { get; set; }

		/// <summary>
		/// If true, additional test is shown.
		/// </summary>
		public bool ShowText { get; set; }

		/// <summary>
		/// The offset of from the coordinate to the upper left corner of the text
		/// </summary>
		public SizeF TextOffset { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor.
		/// 
		/// The default colour is SandyBrown, and oneway roads are shown.
		/// </summary>
		/// <param name="graph">The graph to show.</param>
		/// <param name="showText">Flag that states whether the text should be 
		/// displayed or not.</param>
		/// <param name="layerIdx">The index of the layer where the item is 
		/// to be drawn. Items with larger layer indeces will be drawn on top
		/// of items with lower indices.</param>
		public SpatialGraphDrawer(SpatialGraph graph, bool showText, int layerIdx)
			: base(layerIdx)
		{
			MyGraph = graph ?? throw new ArgumentNullException(nameof(graph));
			_spatialIndex = new SpatialIndex(typeof(C), graph.Nodes.Where(n => n.Coordinate != null), 10);

			Colour = new Dictionary<int, Color>();
			Width = new Dictionary<int, float>();
			ShowText = showText;
			TextOffset = new SizeF(5, -15);
			Font = new Font("Arial", 7, FontStyle.Bold);
		}

		#endregion

		#region Implementation of DrawableItem

		/// <summary>
		/// Draws the graph in the given view
		/// </summary>
		public override void Draw(NetworkViewControlGeneric<C> view)
		{
			Pen nodePen = new Pen(Color.Green, 3.0f);
			Pen edgePen = new Pen(Color.Green, 3.0f);
			SolidBrush travellerChangeBrushCar = new SolidBrush(Color.Navy);


			RegionSelectionData state = new RegionSelectionData(view);
			float meter = (float)view.GetDrawSize(1, 10, 4);
			bool showDetails = view.PixelsPerMeter() > 0.2;

			SelectRegionsToDraw(_spatialIndex.RootRegion, state);
			var arcs = state._regionsToDraw.SelectMany(r => r.Nodes.SelectMany(n => n.AllEdges).Distinct());

			//onewayPen.EndCap = LineCap.ArrowAnchor;

			List<Tuple<int, List<SpatialEdge>>> aspecialArcs = new List<Tuple<int, List<SpatialEdge>>>();
			Dictionary<Pen, List<SpatialEdge>> specialArcs = new Dictionary<Pen, List<SpatialEdge>>();

			IEnumerable<SpatialEdge> edges = state._regionsToDraw.SelectMany(r => r.Nodes.SelectMany(n => n.AllEdges)).Distinct();

			foreach (SpatialEdge a in edges)
			{
				Pen selectedPen = null;
				if (PenSelector != null)
					selectedPen = PenSelector(a, edgePen.Width);

				if (selectedPen == null)
					selectedPen = edgePen;

				if (!showDetails)
				{
					DrawSegment(a, selectedPen);
				}
				else
				{
					// Detailed drawing
					DrawSegment(a, selectedPen);
					Pen edgeRoadPen = new Pen(Color.Black, selectedPen.Width)
					{
						StartCap = LineCap.NoAnchor,
						EndCap = LineCap.NoAnchor,
						CompoundArray = new float[] { 0.0f, 0.1f, 0.9f, 1.0f },
					};
					DrawSegment(a, edgeRoadPen);
					edgeRoadPen.Dispose();
				}
			}
			
			//Draw nodes
			var nodes = state._regionsToDraw.SelectMany(r => r.Nodes);
			foreach (SpatialNode n in nodes)
			{
				C cor = (C)n.Coordinate;
				view.DrawCircle(cor, 4.0f, nodePen);
				if (ShowText && n.Id != null)
					view.DrawText(n.Id, cor, nodePen.Color, Font, TextOffset);

			}

			edgePen.Dispose();
			nodePen.Dispose();
	
			// Draws the given segment with the given pen.
			void DrawSegment(SpatialEdge a, Pen selectedPen)
			{
				view.DrawPolyline(a.Coordinates.Cast<C>().ToList(), selectedPen);
			}
		}


		/// <summary>
		/// Enumerates points defining the item's extent
		/// </summary>
		public override IEnumerable<C> Extent
		{
			get
			{
				List<C> coordinates = MyGraph.Nodes.Where(n => n.Coordinate != null).Select(x => (C) x.Coordinate).ToList();
				if (coordinates.Count == 0)
					yield break;
				yield return coordinates.MaxBy(x => x.Y);
				yield return coordinates.MinBy(x => x.Y);
				yield return coordinates.MaxBy(x => x.X);
				yield return coordinates.MinBy(x => x.X);
			}
		}

		#endregion

		#region Private methods 

		/// <summary>
		/// Selects regions to draw from the part of the tree under the given region. Eliminates regions
		/// outside the drawing area and fills the data's lists with the regions selected.
		/// </summary>
		private void SelectRegionsToDraw(SpatialIndex.Region region, RegionSelectionData data)
		{
			NetworkViewControlGeneric<C>.BoundingBoxStatus status = data._view.GetBoundingBoxStatus(region._arcBoundingBox, 8);

			switch (status)
			{
				case NetworkViewControlGeneric<C>.BoundingBoxStatus.Small:
					// Small region: draw simply
					data._smallRegions.Add(region);
					return;

				case NetworkViewControlGeneric<C>.BoundingBoxStatus.OutsideView:
					// Region is outside view: skip
					return;

				case NetworkViewControlGeneric<C>.BoundingBoxStatus.Normal:
					// Region is visible and not small. Add leaf or recurse
					if (region.IsLeaf)
					{
						data._regionsToDraw.Add(region);
						return;
					}

					foreach (var subRegion in region.SubRegions)
					{
						SelectRegionsToDraw(subRegion, data);
					}
					return;
			}
		}

		#endregion

		#region Private class

		/// <summary>
		/// Data used during selection of the regions to draw
		/// </summary>
		private class RegionSelectionData
		{
			/// <summary>
			/// The view we're to draw in
			/// </summary>
			public NetworkViewControlGeneric<C> _view;
			/// <summary>
			/// Regions selected to be drawn with full road network
			/// </summary>
			public List<SpatialIndex.Region> _regionsToDraw;
			/// <summary>
			/// Regions selected to be drawn with no detail
			/// </summary>
			public List<SpatialIndex.Region> _smallRegions;

			/// <summary>
			/// Constructor
			/// </summary>
			public RegionSelectionData(NetworkViewControlGeneric<C> view)
			{
				_view = view;
				_regionsToDraw = new List<SpatialIndex.Region>();
				_smallRegions = new List<SpatialIndex.Region>();
			}
		}

		#endregion

	}

	#region Extensions to NetworkViewControl

	/// <summary>
	/// Extension methods 
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Returns the draw size of an object that shall be drawn with a desired size but with given minimum and maximum size.
		/// </summary>
		/// <param name="view">NetworkView extension is working on</param>
		/// <param name="min">Minimum size to draw</param>
		/// <param name="max">Maximum size to draw</param>
		/// <param name="meters">Desired size in meters to draw</param>
		/// <typeparam name="C">The type of world coordinate that the network view control uses.</typeparam>
		/// <returns>The draw size of an object that shall be drawn with a desired size but with given minimum and maximum size.</returns>
		public static double GetDrawSize<C>(this Sintef.Scoop.Utilities.GUI.NetworkViewControlGeneric<C> view, double min, double max, double meters) where C:ICoordinate
		{
			return Math.Min(Math.Max(min, view.PixelsPerMeter() * meters), max);
		}
	}

	#endregion
}
