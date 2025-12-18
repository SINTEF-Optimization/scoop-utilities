//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Sintef.Scoop.Utilities.GeoCoding
{
	/// <summary>
	/// A 2D region in a cartesian coordinate system,
	/// defined by a set of disjoint connexe region components
	/// </summary>
	public class Region2D
	{
		/// <summary>
		/// The bounding box edges of the region
		/// </summary>
		private double[] _boundingBox = new double[4];

		/// <summary>
		/// The min x value of the bounding box of the region
		/// </summary>
		public double BoundingBoxMinX
		{
			get { return _boundingBox[0]; }
			private set { _boundingBox[0] = value; }
		}

		/// <summary>
		/// The max x value of the bounding box of the region
		/// </summary>
		public double BoundingBoxMaxX
		{
			get { return _boundingBox[1]; }
			private set { _boundingBox[1] = value; }
		}

		/// <summary>
		/// The min y value of the bounding box of the region
		/// </summary>
		public double BoundingBoxMinY
		{
			get { return _boundingBox[2]; }
			private set { _boundingBox[2] = value; }
		}

		/// <summary>
		/// The max y value of the bounding box of the region
		/// </summary>
		public double BoundingBoxMaxY
		{
			get { return _boundingBox[3]; }
			private set { _boundingBox[3] = value; }
		}

		/// <summary>
		/// The components of the region
		/// </summary>
		public List<RegionComponent2D> RegionComponents { get; private set; }

		/// <summary>
		/// The (positive) area limited by the region
		/// </summary>
		public double Area
		{
			get
			{
				double a = 0.0;
				foreach (RegionComponent2D comp in RegionComponents)
					a += comp.Area;
				return a;
			}
		}

		/// <summary>
		/// Gets if the region is empty, i.e., contains no components and no area
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return !RegionComponents.Any();
			}
		}

		private void UpdateBoundingBox()
		{
			BoundingBoxMinX = Double.MaxValue;
			BoundingBoxMaxX = Double.MinValue;
			BoundingBoxMinY = Double.MaxValue;
			BoundingBoxMaxY = Double.MinValue;

			foreach (RegionComponent2D comp in RegionComponents)
			{
				BoundingBoxMinX = Math.Min(BoundingBoxMinX, comp.BoundingBoxMinX);
				BoundingBoxMaxX = Math.Max(BoundingBoxMaxX, comp.BoundingBoxMaxX);
				BoundingBoxMinY = Math.Min(BoundingBoxMinY, comp.BoundingBoxMinY);
				BoundingBoxMaxY = Math.Max(BoundingBoxMaxY, comp.BoundingBoxMaxY);
			}
		}

		/// <summary>
		/// An empty region with no components
		/// </summary>
		public Region2D()
		{
			RegionComponents = new List<RegionComponent2D>();
			UpdateBoundingBox();
		}

		/// <summary>
		/// A 2D region
		/// </summary>
		/// <param name="element">XElement containing xml representation of a 2d region</param>
		public Region2D(XElement element)
		{
			RegionComponents = new List<RegionComponent2D>();
			foreach (XElement comp in element.TagElements("RegionComponent"))
				RegionComponents.Add(new RegionComponent2D(comp));
			UpdateBoundingBox();
		}

		/// <summary>
		/// A 2D region
		/// </summary>
		/// <param name="components">The components of the region</param>
		public Region2D(IEnumerable<RegionComponent2D> components)
		{
			RegionComponents = new List<RegionComponent2D>(components);
			UpdateBoundingBox();
		}

		/// <summary>
		/// A region representing a rectangular axis parallell box in a cartesian coordinate system
		/// </summary>
		/// <param name="minX">The minimum x value of the box</param>
		/// <param name="maxX">The maximum x value of the box</param>
		/// <param name="minY">The minimum y value of the box</param>
		/// <param name="maxY">The maximum y value of the box</param>
		public Region2D(double minX, double maxX, double minY, double maxY)
		{
			RegionComponents = new List<RegionComponent2D>();
			RegionComponents.Add(new RegionComponent2D(minX, maxX, minY, maxY));
			UpdateBoundingBox();
		}

		/// <summary>
		/// A region of one component only without any holes
		/// </summary>
		/// <param name="polygon">The outer loop of the region</param>
		public Region2D(ClosedPolygon polygon)
		{
			RegionComponents = new List<RegionComponent2D>();
			RegionComponents.Add(new RegionComponent2D(polygon));
			UpdateBoundingBox();
		}

		/// <summary>
		/// A 2D region given by a set of polygons that might be outer polygons (positive orientation) or holes (negative orientation)
		/// </summary>
		/// <param name="polygons">The polygons</param>
		public Region2D(IEnumerable<ClosedPolygon> polygons)
		{
			RegionComponents = new List<RegionComponent2D>();

			// Each positively oriented polygon becomes the outer polygon of a region component
			foreach (ClosedPolygon pol in polygons)
				if (pol.HasPositiveOrientation)
					RegionComponents.Add(new RegionComponent2D(pol));

			// Each negatively oriented polygon is an inner loop in the smallest region containing the polygon
			foreach (ClosedPolygon pol in polygons)
				if (!pol.HasPositiveOrientation)
				{
					// Use mid point of first two corners, to avoid special cases where a hole and outer loop share a corner
					Coordinate mid = 0.5 * (pol.Corners[0] + pol.Corners[1]);
					RegionComponent2D smalestContainingComponent = null;
					double smalestContainingArea = Double.MaxValue;

					foreach (RegionComponent2D comp in RegionComponents)
					{
						ClosedPolygon outer = comp.OuterLoop;
						if (outer.ContainsCoordinate(mid, false))
						{
							if (smalestContainingComponent == null || outer.Area < smalestContainingArea)
							{
								smalestContainingComponent = comp;
								smalestContainingArea = outer.Area;
							}
						}
					}

					if (smalestContainingComponent == null)
						throw new Exception("Hole in resulting region is not contained in any outer loop");

					smalestContainingComponent.AddInnerLoop(pol);
				}
		}

		/// <summary>
		/// Adds the given component to the region
		/// </summary>
		public void AddRegion(RegionComponent2D component)
		{
			RegionComponents.Add(component);
			UpdateBoundingBox();
		}
		
		/// <summary>
		/// Returns an xml representation of this region
		/// </summary>
		public XElement ToXml(string elementName)
		{
			XElement element = new XElement(elementName);
			foreach (RegionComponent2D comp in RegionComponents)
				element.Add(comp.ToXml("RegionComponent"));
			return element;
		}

		/// <summary>
		/// The number of region components
		/// </summary>
		public int Count { get { return RegionComponents.Count; } }

		/// <summary>
		/// Returns a specific region component in the region
		/// </summary>
		/// <param name="key">The index of the polygon</param>
		public RegionComponent2D this[int key] { get { return RegionComponents[key]; } }

		/// <summary>
		/// Returns whether the region contains a coordinate
		/// </summary>
		/// <param name="coordinate">The coordinate</param>
		/// <param name="acceptOnEdge">Tells if a coordinate on the edge is considered as contained in the region</param>
		public bool ContainsCoordinate(Coordinate coordinate, bool acceptOnEdge = true)
		{
			foreach (RegionComponent2D comp in RegionComponents)
			{
				if (comp.ContainsCoordinate(coordinate, acceptOnEdge))
					return true;
			}

			return false;
		}
	}
}
