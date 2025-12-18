//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System.Collections.Generic;
using System.Xml.Linq;

namespace Sintef.Scoop.Utilities.GeoCoding
{
	/// <summary>
	/// A connected region in a cartesian 2D coordinate system with one positively oriented outer loop,
	/// and some possible negatively oriented inner loops representing holes.
	/// </summary>
	public class RegionComponent2D
	{
		/// <summary>
		/// The min x value of the bounding box of the region component
		/// </summary>
		public double BoundingBoxMinX
		{
			get { return OuterLoop.BoundingBoxMinX; }
		}

		/// <summary>
		/// The max x value of the bounding box of the region component
		/// </summary>
		public double BoundingBoxMaxX
		{
			get { return OuterLoop.BoundingBoxMaxX; }
		}

		/// <summary>
		/// The min y value of the bounding box of the region component
		/// </summary>
		public double BoundingBoxMinY
		{
			get { return OuterLoop.BoundingBoxMinY; }
		}

		/// <summary>
		/// The max y value of the bounding box of the region component
		/// </summary>
		public double BoundingBoxMaxY
		{
			get { return OuterLoop.BoundingBoxMaxY; }
		}

		/// <summary>
		/// The (positive) area limited by the region
		/// </summary>
		public double Area
		{
			get
			{
				double a = OuterLoop.Area;
				foreach (ClosedPolygon cp in InnerLoops)
					a -= cp.Area;
				return a;
			}
		}

		/// <summary>
		/// The outer loop of the region component
		/// </summary>
		public ClosedPolygon OuterLoop { get; private set; }

		/// <summary>
		/// The inner loops of the region component
		/// </summary>
		public List<ClosedPolygon> InnerLoops { get; private set; }

		/// <summary>
		/// A connected region with no holes
		/// </summary>
		/// <param name="outerLoop">The outer loop of the region</param>
		public RegionComponent2D(ClosedPolygon outerLoop)
		{
			OuterLoop = outerLoop;
			InnerLoops = new List<ClosedPolygon>();
		}

		/// <summary>
		/// A connected region, possibly with holes
		/// </summary>
		/// <param name="outerLoop">The outer loop of the region</param>
		/// <param name="innerLoops">The inner loops defining the holes</param>
		public RegionComponent2D(ClosedPolygon outerLoop, IEnumerable<ClosedPolygon> innerLoops)
		{
			OuterLoop = outerLoop;
			InnerLoops = new List<ClosedPolygon>(innerLoops);
		}

		/// <summary>
		/// A connected region, possibly with holes
		/// </summary>
		/// <param name="element">XElement containing xml representation</param>
		public RegionComponent2D(XElement element)
		{
			OuterLoop = new ClosedPolygon(element.RequireElement("OuterLoop"));
			InnerLoops = new List<ClosedPolygon>();
			XElement xInner = element.TagElement("InnerLoops");
			if (xInner != null)
			{
				foreach (XElement e in xInner.TagElements("ClosedPolygon"))
					InnerLoops.Add(new ClosedPolygon(e));
			}
		}

		/// <summary>
		/// A region representing a rectangular axis parallell box in a cartesian coordinate system
		/// </summary>
		/// <param name="minX">The minimum x value of the box</param>
		/// <param name="maxX">The maximum x value of the box</param>
		/// <param name="minY">The minimum y value of the box</param>
		/// <param name="maxY">The maximum y value of the box</param>
		public RegionComponent2D(double minX, double maxX, double minY, double maxY)
		{
			OuterLoop = new ClosedPolygon(minX, maxX, minY, maxY, true);
			InnerLoops = new List<ClosedPolygon>();
		}

		/// <summary>
		/// Adds the given polygon as an inner loop in the region
		/// </summary>
		public void AddInnerLoop(ClosedPolygon loop)
		{
			InnerLoops.Add(loop);
		}

		/// <summary>
		/// Returns an xml representation of this region component
		/// </summary>
		public XElement ToXml(string elementName)
		{
			XElement element = new XElement(elementName);
			element.Add(OuterLoop.ToXml("OuterLoop"));
			XElement iLoops = new XElement("InnerLoops");
			foreach (ClosedPolygon inner in InnerLoops)
				iLoops.Add(inner.ToXml("ClosedPolygon"));
			element.Add(iLoops);
			return element;
		}


		/// <summary>
		/// Returns whether the region contains a coordinate
		/// </summary>
		/// <param name="coordinate">The coordinate</param>
		/// <param name="acceptOnEdge">Tells if a coordinate on the edge is considered as contained in the region</param>
		public bool ContainsCoordinate(Coordinate coordinate, bool acceptOnEdge = true)
		{
			if (!OuterLoop.ContainsCoordinate(coordinate, acceptOnEdge))
				return false;

			foreach (ClosedPolygon pol in InnerLoops)
			{
				if (pol.ContainsCoordinate(coordinate, !acceptOnEdge))
					return false;
			}

			return true;
		}
	}
}
