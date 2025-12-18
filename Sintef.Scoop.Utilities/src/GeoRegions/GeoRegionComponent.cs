//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Sintef.Scoop.Utilities.GeoCoding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities.GeoRegions
{
	/// <summary>
	/// A connected region on the Earth defined by one positively oriented outer loop,
	/// and some possible negatively oriented inner loops representing holes.
	/// </summary>
	public class GeoRegionComponent
	{
		/// <summary>
		/// The inner loops of the region component
		/// </summary>
		private List<ClosedGeoPolygon> _innerLoops;

		/// <summary>
		/// The outer loop of the region component
		/// </summary>
		public ClosedGeoPolygon OuterLoop { get; }

		/// <summary>
		/// The inner loops of the region component
		/// </summary>
		public IEnumerable<ClosedGeoPolygon> InnerLoops => _innerLoops.AsReadOnly();

		/// <summary>
		/// How the shape of the edges in the region component are defined. The ourter loop and inner loops of the region
		/// component must have the same edge type. The edge type can not be NoEdges.
		/// </summary>
		public RegionEdgeType EdgeType => OuterLoop.EdgeType;

		/// <summary>
		/// The (positive) area limited by the region in km^2
		/// </summary>
		public double Area => OuterLoop.Area - InnerLoops.Sum(pol => pol.Area);

		/// <summary>
		/// A connected region with no holes
		/// </summary>
		/// <param name="outerLoop">The outer loop of the region</param>
		public GeoRegionComponent(ClosedGeoPolygon outerLoop)
		{
			OuterLoop = outerLoop;
			_innerLoops = new List<ClosedGeoPolygon>();
		}

		/// <summary>
		/// A connected region, possibly with holes
		/// </summary>
		/// <param name="outerLoop">The outer loop of the region</param>
		/// <param name="innerLoops">The inner loops defining the holes</param>
		public GeoRegionComponent(ClosedGeoPolygon outerLoop, IEnumerable<ClosedGeoPolygon> innerLoops)
		{
			OuterLoop = outerLoop;
			_innerLoops = new List<ClosedGeoPolygon>(innerLoops);
			if (_innerLoops.Any(pol => pol.EdgeType != EdgeType))
			{
				throw new InvalidOperationException("The outer loop and all inner loops of a region component must have the same edge type");
			}
		}

		/// <summary>
		/// Returns a cloned copy of the connected region.
		/// </summary>
		public GeoRegionComponent Clone()
		{
			return new GeoRegionComponent(OuterLoop.Clone(), InnerLoops.Select(pol => pol.Clone()));
		}

		/// <summary>
		/// Adds an inner loop to the region
		/// </summary>
		/// <param name="loop">The loop to be added</param>
		internal void AddInnerLoop(ClosedGeoPolygon loop)
		{
			if (loop.EdgeType != EdgeType)
			{
				throw new InvalidOperationException($"Cannot add inner loop of edge type {loop.EdgeType} to a region component of edge type {EdgeType}");
			}
			_innerLoops.Add(loop);
		}

		/// <summary>
		/// Returns whether the region contains a given coordinate
		/// </summary>
		/// <param name="coordinate">The coordinate</param>
		/// <param name="acceptOnEdge">Tells if a coordinate on the edge is considered as contained in the region</param>
		internal bool Contains(GeoCoordinate coordinate, bool acceptOnEdge = true)
		{
			if (!OuterLoop.Contains(coordinate, acceptOnEdge))
				return false;

			return InnerLoops.All(pol => !pol.Contains(coordinate, !acceptOnEdge));
		}
	}
}
