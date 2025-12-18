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
	/// How the shape of the edges on regions and closed polygons are defined
	/// </summary>
	public enum RegionEdgeType
	{
		/// <summary>
		/// The edge between two corners is given as a straight line in the Cartesian Longitude/Latitude coordinate system.
		/// If the two corners have the same latitude, the edge between them will also lie entirely on that common latitude.
		/// </summary>
		LatitudeLongitudeStraightLines,

		/// <summary>
		/// The edge between two corners is given as the shortest path on the Earth surface between the corners,
		/// assuming the Earth is a perfect sphere (the equatorial and polar radius are the same).
		/// If the two corners have the same latitude on the Nothern hemisphere, the interior of the edge between them will
		/// go further North that that common latitude.
		/// </summary>
		UnitSphereShortestPaths,

		/// <summary>
		/// There are noe edges in the region. Used when there are noe edges defined yet, like for a region that is empty
		/// or the entire Earth surface.
		/// </summary>
		NoEdges
	}

	/// <summary>
	/// A region on the Earth, defined by a set of disjoint connexe region components
	/// </summary>
	public class GeoRegion
	{
		/// <summary>
		/// The area of the entire Earth surface, in km^2
		/// </summary>
		private static readonly double _entireEarthArea = 510072000;

		/// <summary>
		/// The components of the region. If null, this region represents the entire Earth surface
		/// </summary>
		private List<GeoRegionComponent> _regionComponents;

		/// <summary>
		/// How the shape of the edges in the region are defined. All region components must have the same edge type.
		/// If the region is empty, the edge type is set to NoEdges, and might be changed later when components are added.
		/// If the region is the entire Earth surface, the types is also set to NoEdges.
		/// </summary>
		public RegionEdgeType EdgeType { get; private set; }

		/// <summary>
		/// The components of the region, or null if this is the entire Earth surface
		/// </summary>
		public IEnumerable<GeoRegionComponent> RegionComponents => _regionComponents?.AsReadOnly();

		/// <summary>
		/// The (positive) area limited by the region in km^2
		/// </summary>
		public double Area => IsAll ? _entireEarthArea : RegionComponents.Sum(comp => comp.Area);

		/// <summary>
		/// Whether this region represents the entire Earth surface
		/// </summary>
		public bool IsAll => _regionComponents == null;

		/// <summary>
		/// Whether this region is empty
		/// </summary>
		public bool IsEmpty => _regionComponents != null && !_regionComponents.Any();

		/// <summary>
		/// An empty region with no components
		/// </summary>
		public GeoRegion()
		{
			_regionComponents = new List<GeoRegionComponent>();
			EdgeType = RegionEdgeType.NoEdges;
		}

		/// <summary>
		/// A region of one component only without any holes
		/// </summary>
		/// <param name="polygon">The outer loop of the region</param>
		public GeoRegion(ClosedGeoPolygon polygon)
		{
			_regionComponents = new List<GeoRegionComponent>() { new GeoRegionComponent(polygon) };
			EdgeType = polygon.EdgeType;
		}

		/// <summary>
		/// A region given by a set of polygons that might be outer polygons (positive orientation) or holes (negative orientation)
		/// </summary>
		/// <param name="polygons">The polygons</param>
		public GeoRegion(IEnumerable<ClosedGeoPolygon> polygons)
		{
			if (!polygons.Any())
			{
				EdgeType = RegionEdgeType.NoEdges;
			}
			else
			{
				EdgeType = polygons.First().EdgeType;
				if (polygons.Skip(1).Any(pol => pol.EdgeType != EdgeType))
				{
					throw new InvalidOperationException("Can not define region with polygons of different edge types");
				}
			}

			// Each positively oriented polygon becomes the outer polygon of a region component
			_regionComponents = polygons
				.Where(pol => pol.HasPositiveOrientation)
				.Select(pol => new GeoRegionComponent(pol))
				.ToList();

			// Each negatively oriented polygon is an inner loop in the smallest region containing the polygon
			foreach (ClosedGeoPolygon pol in polygons.Where(pol => !pol.HasPositiveOrientation))
			{
				// Use mid point of first two corners, to avoid special cases where a hole and outer loop share a corner
				GeoCoordinate mid;
				if (EdgeType == RegionEdgeType.UnitSphereShortestPaths)
				{
					mid = pol.Corners.First().Interpolated(pol.Corners.Skip(1).First(), 0.5, 0.01);
				}
				else
				{
					GeoCoordinate gc1 = pol.Corners.First();
					GeoCoordinate gc2 = pol.Corners.Skip(1).First();
					mid = new GeoCoordinate(0.5 * (gc1.Latitude + gc2.Latitude), 0.5 * (gc1.Longitude + gc2.Longitude));
				}

				GeoRegionComponent smalestContainingComponent = null;
				double smalestContainingArea = Double.MaxValue;

				foreach (GeoRegionComponent comp in RegionComponents)
				{
					ClosedGeoPolygon outer = comp.OuterLoop;
					if (outer.Contains(mid, false))
					{
						if (outer.Area < smalestContainingArea)
						{
							smalestContainingComponent = comp;
							smalestContainingArea = outer.Area;
						}
					}
				}

				if (smalestContainingComponent == null)
				{
					throw new InvalidOperationException("Hole in resulting region is not contained in any outer loop");
				}

				smalestContainingComponent.AddInnerLoop(pol);
			}
		}

		/// <summary>
		/// Returns a region defining the entire Earth surface
		/// </summary>
		public static GeoRegion All()
		{
			GeoRegion region = new GeoRegion();
			region._regionComponents = null;
			return region;
		}

		/// <summary>
		/// Returns a cloned copy of the region.
		/// </summary>
		public GeoRegion Clone()
		{
			if (IsAll)
			{
				return All();
			}
			else
			{
				GeoRegion clonedRegion = new GeoRegion();
				clonedRegion._regionComponents.AddRange(_regionComponents.Select(comp => comp.Clone()));
				return clonedRegion;
			}
		}

		/// <summary>
		/// Adds a region connexe component to the region
		/// </summary>
		/// <param name="component">The component to be added</param>
		public void AddRegion(GeoRegionComponent component)
		{
			if (_regionComponents != null)
			{
				_regionComponents.Add(component);
				if (EdgeType == RegionEdgeType.NoEdges)
				{
					EdgeType = component.EdgeType;
				}
				else if (EdgeType != component.EdgeType)
				{
					throw new InvalidOperationException($"Cannot add region component of edge type {component.EdgeType} to region of edge type {EdgeType}");
				}
			}
		}

		/// <summary>
		/// Returns whether the region contains a coordinate
		/// </summary>
		/// <param name="coordinate">The coordinate</param>
		/// <param name="acceptOnEdge">Tells if a coordinate on the edge is considered as contained in the region</param>
		public bool Contains(GeoCoordinate coordinate, bool acceptOnEdge = true)
		{
			return IsAll ? true : RegionComponents.Any(comp => comp.Contains(coordinate, acceptOnEdge));
		}
	}
}
