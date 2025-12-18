//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Sintef.Scoop.Utilities.GeoCoding
{
	/// <summary>
	/// A spatial index organizes the collection of SpatialGraphNode's in a Graph spatially.
	/// It makes operations such as finding the nodes and arcs in a geographic region or
	/// finding the closest node or arc to some point, much faster.
	/// </summary>
	public class SpatialIndex
	{
		/// <summary>
		/// The type of coordinates that the index will use. Must be the same for all nodes.
		/// </summary>
		Type _coordinateType;

		#region Public properties

		/// <summary>
		/// The maximum number of nodes this index will allow in a region before
		/// splitting it in subregions
		/// </summary>
		public int MaxNodesInARegion { get; private set; }

		/// <summary>
		/// The root of the region tree
		/// </summary>
		public Region RootRegion { get; private set; }

		#endregion

		/// <summary>
		/// Initializes the index with the given nodes.
		/// Creates an index that organizes the nodes in the given collection
		/// </summary>
		/// <param name="coordinateType">The type of coordinates that the index will use. Must be the same for all nodes added later.</param>
		/// <param name="nodes"></param>
		/// <param name="maxNodesInRegion"></param>
		public SpatialIndex(Type coordinateType, IEnumerable<SpatialNode> nodes, int maxNodesInRegion)
		{
			_coordinateType = coordinateType;
			MaxNodesInARegion = maxNodesInRegion;
			RootRegion = new Region(nodes, this);

			if (nodes.Any())
				Debug.Assert(nodes.All(n => n.Coordinate.GetType() == _coordinateType), $"At least one node was given with an unexpected coordinate type. Expected {_coordinateType.Name}");
		}

		#region Public methods

		/// <summary>
		/// Creates a list of pairs of nodes that have the same coordinate.
		/// The first node in the pair is from this index, while the second is
		/// from the other spatial index.
		/// If more than one node in the same spatial index have the same coordinate,
		/// (and the same coordinate is found in the other index,)
		/// only one (arbitrary) of them will be reported in a node pair.
		/// </summary>
		public List<Tuple<SpatialNode, SpatialNode>> GetNodePairsWithSameCoordinate(SpatialIndex otherSpatialIndex)
		{
			List<Tuple<SpatialNode, SpatialNode>> result = new List<Tuple<SpatialNode, SpatialNode>>();

			RootRegion.FindNodePairsWithSameCoordinate(otherSpatialIndex.RootRegion, result);

			return result;
		}

		/// <summary>
		/// Creates a list of pairs of nodes that are at least as close as the given distance.
		/// The first node in the pair is from this index, while the second is
		/// from the other spatial index.
		/// If more than one node in the same spatial index is close enough to the same
		/// node in the other spatial index,
		/// only one (arbitrary) of them will be reported in a node pair.
		/// </summary>
		/// <param name="otherSpatialIndex"></param>
		/// <param name="distanceTolerance">The maxium distance between nodes in a pair, in meters</param>
		public List<Tuple<SpatialNode, SpatialNode>> GetCloseNodePairs(SpatialIndex otherSpatialIndex, double distanceTolerance)
		{
			List<Tuple<SpatialNode, SpatialNode>> result = new List<Tuple<SpatialNode, SpatialNode>>();
			Dictionary<Region, List<SpatialNode>> matchedNodes = new Dictionary<Region, List<SpatialNode>>();

			RootRegion.FindCloseNodePairs(new List<Region>() { otherSpatialIndex.RootRegion }, distanceTolerance, result, matchedNodes);

			return result;
		}
		/// <summary>
		/// Creates a list of all pairs of nodes that are at least as close as the given distance.
		/// The first node in the pair is from this index, while the second is
		/// from the other spatial index.
		/// If more than one node in the same spatial index is close enough to the same
		/// node in the other spatial index,
		/// all of them will be reported in a node pair.
		/// </summary>
		/// <param name="otherSpatialIndex"></param>
		/// <param name="distanceTolerance">The maxium distance between nodes in a pair, in meters</param>
		public List<Tuple<SpatialNode, SpatialNode>> GetAllCloseNodePairs(SpatialIndex otherSpatialIndex, double distanceTolerance)
		{
			List<Tuple<SpatialNode, SpatialNode>> result = new List<Tuple<SpatialNode, SpatialNode>>();

			RootRegion.FindAllCloseNodePairs(new List<Region>() { otherSpatialIndex.RootRegion }, distanceTolerance, result);
			return result;
		}
		/// <summary>
		/// Comparer that sorts on the second item of the tuple (which is distance)
		/// </summary>
		private class Item2Comparer : IComparer<Tuple<Region, double>>
		{
			#region IComparer<Tuple<Region,double>> Members

			public int Compare(Tuple<Region, double> x, Tuple<Region, double> y)
			{
				if (x.Item2 < y.Item2)
					return -1;
				if (x.Item2 > y.Item2)
					return 1;
				return 0;
			}

			#endregion
		}

		/// <summary>
		/// Returns n arc-distance pairs for the arcs closest to c. The list is ordered by ascending distance.
		/// </summary>
		/// <param name="coordinate">The coordinate to find the closest arcs to</param>
		/// <param name="n">The number of arcs to find</param>
		/// <param name="zLevel">The ZLevel of the arcs to find</param>
		/// <returns>The closest arcs, with distance</returns>
		public List<Tuple<SpatialEdge, double>> ClosestArcsWithDistance(ICoordinate coordinate, int n, int zLevel = int.MinValue)
		{
			List<Tuple<SpatialEdge, double>> result = new List<Tuple<SpatialEdge, double>>();

			foreach (var arcDist in ArcsOrderedByDistance(coordinate))
			{
				SpatialEdge arc = arcDist.Item1;

				if (zLevel != int.MinValue && arc.ZLevel != zLevel)
					continue;

				result.Add(arcDist);
				if (result.Count == n)
					break;
			}

			return result;
		}

		/// <summary>
		/// Returns SpatialArc-distance-pairs of all arcs ordered by closest distance from a coordinate.
		/// </summary>
		/// <param name="coordinate">The coordinate</param>
		/// <returns></returns>
		public IEnumerable<Tuple<SpatialEdge, double>> ArcsOrderedByDistance(ICoordinate coordinate)
		{
			// List for arcs found in visited regions, and that have not been returned yet.
			List<Tuple<SpatialEdge, double>> arcsToBeReturned = new List<Tuple<SpatialEdge, double>>();

			// Keep track of arcs that are on arcsToBeReturned or have been returned,
			// to ensure no arc is returned twice
			Dictionary<SpatialEdge, bool> checkedArcs = new Dictionary<SpatialEdge, bool>();

			// <Region, lower bound on distance from coordinate to an arc with a node in region>
			C5.IntervalHeap<Tuple<Region, double>> candidateRegions = new C5.IntervalHeap<Tuple<Region, double>>(new Item2Comparer());

			// List for candidate regions with zero distance
			List<Region> zeroDistanceRegions = new List<Region>();

			// Index of next region in list to check
			int zeroIndex = 0;

			double d = RootRegion.DistanceToArcLowerBound(coordinate);
			if (d > 0)
				candidateRegions.Add(new Tuple<Region, double>(RootRegion, d));
			else
				zeroDistanceRegions.Add(RootRegion);

			while (zeroIndex < zeroDistanceRegions.Count || candidateRegions.Count > 0)
			{
				// Extract a region with the lowest distance bound
				Region region;
				double regionLowerBound = 0;
				if (zeroIndex < zeroDistanceRegions.Count)
				{
					region = zeroDistanceRegions[zeroIndex++];
				}
				else
				{
					Tuple<Region, double> bestItem = candidateRegions.DeleteMin();

					region = bestItem.Item1;
					regionLowerBound = bestItem.Item2;
				}

				// Test if the best arc in arcsToBeReturned is closer than what is possible for arcs in unchecked regions
				while (arcsToBeReturned.Count > 0 && arcsToBeReturned[0].Item2 < regionLowerBound)
				{
					yield return arcsToBeReturned[0];
					arcsToBeReturned.RemoveAt(0);
				}

				// Test if the region should be split into sub regions
				if (region.HasSubRegions)
				{
					foreach (var sub in region.SubRegions)
					{
						double dd = sub.DistanceToArcLowerBound(coordinate);
						if (dd > 0)
							candidateRegions.Add(new Tuple<Region, double>(sub, dd));
						else
							zeroDistanceRegions.Add(sub);
					}
					continue;
				}

				// Leaf region.
				foreach (var node in region.Nodes)
				{
					foreach (var edge in node.AllEdges)
					{
						if (checkedArcs.ContainsKey(edge))
							continue;
						checkedArcs.Add(edge, true);

						var projection = edge.ClosestPoint(coordinate);
						arcsToBeReturned.Add(new Tuple<SpatialEdge, double>(edge, coordinate.DistanceTo(projection.ClosestPoint)));
					}
				}

				// Resort arcsToBeReturned by distance
				arcsToBeReturned = arcsToBeReturned.OrderBy(x => x.Item2).ToList();
			}

			// No more regions to check, return rest of arcs
			foreach (var v in arcsToBeReturned)
				yield return v;
		}

		/// <summary>
		/// Returns n node-distance pairs for the nodes closest to c. The list is ordered by ascending distance.
		/// </summary>
		/// <param name="coordinate">The coordinate to find the closest nodes to</param>
		/// <param name="n">The number of nodes to find</param>
		/// <returns>The closest nodes, with distance</returns>
		public List<Tuple<SpatialNode, double>> ClosestNodesWithDistance(ICoordinate coordinate, int n)
		{
			List<Tuple<SpatialNode, double>> result = new List<Tuple<SpatialNode, double>>();

			// <Region, lower bound on distance from coordinate to a node in region>
			List<Tuple<Region, double>> candidateRegions = new List<Tuple<Region, double>>();

			candidateRegions.Add(new Tuple<Region, double>(RootRegion, RootRegion.BoundingBox.MinDistance(coordinate)));
			double nthBestBound = double.PositiveInfinity; // Upper bound on n'th best distance

			while (candidateRegions.Count > 0)
			{
				Tuple<Region, double> bestItem = candidateRegions.MinBy(x => x.Item2);
				candidateRegions.Remove(bestItem);

				Region region = bestItem.Item1;
				double regionLowerBound = bestItem.Item2;

				if (regionLowerBound > nthBestBound)
					// Region cannot contain a node that is better than any we have
					continue;

				if (region.HasSubRegions)
				{
					// Split into subregions
					foreach (var sub in region.SubRegions)
						candidateRegions.Add(new Tuple<Region, double>(sub, sub.BoundingBox.MinDistance(coordinate)));
					continue;
				}

				// Leaf region.
				foreach (var node in region.Nodes)
				{
					result.Add(new Tuple<SpatialNode, double>(node, node.Coordinate.DistanceTo(coordinate)));
				}

				// Eliminate nodes and update best bound
				result = result.OrderBy(x => x.Item2).Take(n).ToList();
				if (result.Count == n)
					nthBestBound = result.Last().Item2;
			}

			return result;
		}

		/// <summary>
		/// Returns SpatialGraphNode-distance-pairs of all nodes ordered by closest distance from a coordinate
		/// </summary>
		/// <param name="coordinate">The coordinate</param>
		/// <returns></returns>
		public IEnumerable<Tuple<SpatialNode, double>> NodesOrderedByDistance(ICoordinate coordinate)
		{
			// List for nodes found in visited regions, and that have not been returned yet.
			List<Tuple<SpatialNode, double>> nodesToBeReturned = new List<Tuple<SpatialNode, double>>();

			// <Region, lower bound on distance from coordinate to a node in region>
			List<Tuple<Region, double>> candidateRegions = new List<Tuple<Region, double>>();

			candidateRegions.Add(new Tuple<Region, double>(RootRegion, RootRegion.BoundingBox.MinDistance(coordinate)));

			while (candidateRegions.Count > 0)
			{
				// Get region with the lowest distance bound
				Tuple<Region, double> bestItem = candidateRegions.MinBy(x => x.Item2);
				candidateRegions.Remove(bestItem);

				Region region = bestItem.Item1;
				double regionLowerBound = bestItem.Item2;

				// Test if the best node in nodesToBeReturned is closer than what is possible for arcs in unchecked regions
				while (nodesToBeReturned.Count > 0 && nodesToBeReturned[0].Item2 < regionLowerBound)
				{
					yield return nodesToBeReturned[0];
					nodesToBeReturned.RemoveAt(0);
				}

				// Test if the region should be split into subregions
				if (region.HasSubRegions)
				{
					foreach (var sub in region.SubRegions)
						candidateRegions.Add(new Tuple<Region, double>(sub, sub.BoundingBox.MinDistance(coordinate)));
					continue;
				}

				// Leaf region.
				foreach (var node in region.Nodes)
				{
					nodesToBeReturned.Add(new Tuple<SpatialNode, double>(node, node.Coordinate.DistanceTo(coordinate)));
				}

				// Resort nodesToBeReturned by distance
				nodesToBeReturned = nodesToBeReturned.OrderBy(x => x.Item2).ToList();
			}

			// No more regions to check, return rest of nodes
			foreach (var v in nodesToBeReturned)
				yield return v;
		}

		/// <summary>
		/// Returns the node closest to the coordinate
		/// </summary>
		public SpatialNode ClosestNode(ICoordinate c)
		{
			return ClosestNodesWithDistance(c, 1)[0].Item1;
		}

		/// <summary>
		/// Returns the arc with the closest point to the coordinate
		/// </summary>
		public SpatialEdge ClosestArc(ICoordinate c)
		{
			return ClosestArcsWithDistance(c, 1)[0].Item1;
		}

		/// <summary>
		/// Returns whether any arc in the spatial index intersects the line
		/// from c1 to c2
		/// </summary>
		public bool IntersectsAnyArc(ICoordinate c1, ICoordinate c2)
		{
			return RootRegion.IntersectsAnyArc(c1, c2);
		}

		/// <summary>
		/// Merges the other spatial index into this index.
		/// 
		/// The other index is made empty.
		/// </summary>
		/// <param name="otherIndex"></param>
		public void Merge(SpatialIndex otherIndex)
		{
			// Merge the smallest tree into the other tree

			if (RootRegion.NodeCount > otherIndex.RootRegion.NodeCount)
			{
				if (otherIndex.RootRegion.NodeCount > 0)
					RootRegion.Merge(otherIndex.RootRegion);
			}
			else if (RootRegion.NodeCount > 0)
			{
				otherIndex.RootRegion.Merge(RootRegion);
				RootRegion = otherIndex.RootRegion;
			}

			// Clean out other index
			otherIndex.RootRegion = new Region(new SpatialNode[0], otherIndex);

			RootRegion.SetOwner(this);
		}

		/// <summary>
		/// Updates the spatial index after the given nodes have been added to the graph.
		/// </summary>
		public void AddNodes(List<SpatialNode> addedNodes)
		{
			// Find each node's closest leaf region
			List<Tuple<SpatialNode, Region>> closestRegions = addedNodes.Select(n => new Tuple<SpatialNode, Region>(n, ClosestLeafRegion(n.Coordinate))).ToList();

			// Process nodes grouped by closest region
			foreach (var group in closestRegions.GroupBy(x => x.Item2))
			{
				List<SpatialNode> nodes = group.Select(x => x.Item1).ToList();
				Region leaf = group.Key;
				var pathFromLeaf = RootRegion.RegionsInPathFrom(leaf);

				leaf.AddNodes(nodes);
				leaf.Rebalance();

				if (pathFromLeaf != null)
				{
					foreach (Region r in pathFromLeaf)
					{
						r.UpdateBoundingBoxAndNodeCount();
						r.UpdateArcBoundingBox();
					}
				}
			}

			foreach (var n in addedNodes)
			{
				if (RootRegion.LeafRegionContaining(n) == null)
					throw new Exception("shds");
			}
		}

		/// <summary>
		/// Updates the spatial index after the given nodes have been removed to the graph.
		/// </summary>
		public void RemoveNodes(List<SpatialNode> removedNodes)
		{
			// Find each node's leaf region
			List<Tuple<SpatialNode, Region>> containingRegions = removedNodes.Select(n => new Tuple<SpatialNode, Region>(n, RootRegion.LeafRegionContaining(n))).ToList();

			List<Region> parents = new List<Region>();

			// Process nodes grouped by containing leaf region
			foreach (var group in containingRegions.GroupBy(x => x.Item2))
			{
				List<SpatialNode> nodes = group.Select(x => x.Item1).ToList();
				Region leaf = group.Key;
				if (leaf == null)
					throw new Exception("A node to remove was not found in spatial index");

				var pathFromLeaf = RootRegion.RegionsInPathFrom(leaf);
				Region parent = pathFromLeaf[1];
				int nRemovedRegions = 0;

				if (nodes.Count < leaf.NodeCount)
					// Remove nodes from region, leaving some
					leaf.RemoveNodes(nodes);
				else
				{
					// Remove all nodes and prune empty regions from tree
					parent.RemoveSubRegion(leaf);
					++nRemovedRegions;
					while (parent.SubRegions.Count() == 0)
					{
						Region region = parent;
						parent = pathFromLeaf[++nRemovedRegions];
						parent.RemoveSubRegion(region);
					}
				}

				parents.Add(parent);

				// Update accumulated data up to root
				foreach (Region r in pathFromLeaf.Skip(nRemovedRegions))
				{
					r.UpdateBoundingBoxAndNodeCount();
					r.UpdateArcBoundingBox();
				}
			}

			// Rebalance affected parts of tree
			foreach (Region parent in parents.Distinct())
				parent.Rebalance();
		}

		/// <summary>
		/// Updates the spatial index after the given nodes have been modified by addition or removal of arcs.
		/// </summary>
		public void UpdateNodes(List<SpatialNode> updatedNodes)
		{
			// Find each node's leaf region
			List<Tuple<SpatialNode, Region>> containingRegions = updatedNodes.Select(n => new Tuple<SpatialNode, Region>(n, RootRegion.LeafRegionContaining(n))).ToList();

			// Process nodes grouped by containing leaf region
			foreach (var group in containingRegions.GroupBy(x => x.Item2))
			{
				List<SpatialNode> nodes = group.Select(x => x.Item1).ToList();
				Region leaf = group.Key;
				if (leaf == null)
					throw new Exception("A node to update was not found in spatial index");

				var pathFromLeaf = RootRegion.RegionsInPathFrom(leaf);

				foreach (Region r in pathFromLeaf)
				{
					r.UpdateBoundingBoxAndNodeCount();
					r.UpdateArcBoundingBox();
				}
			}
		}

		/// <summary>
		/// Updates the spatial index after the given arcs have been added to the graph.
		/// 
		/// Requires that the nodes at either end of the added arcs
		/// have already been added in the index.
		/// </summary>
		public void AddArcs(List<SpatialEdge> addedArcs)
		{
			// We need to update the max arc extension in all regions that contain a node
			// at one end of an added arc

			// Collect nodes at either end of added arcs
			List<SpatialNode> nodes = addedArcs.Select(a => a.From).Concat(addedArcs.Select(a => a.To)).Distinct().ToList();

			// Find leaf regions with these nodes
			var leafRegions = nodes.Select(n => RootRegion.LeafRegionContaining(n)).Distinct().ToList();

			//if (leafRegions.Contains(null))
			//	throw new Exception("SpatialGraphNode at arc to be added to spatial index was not present in index");

			// Process
			foreach (Region leaf in leafRegions)
			{
				if (leaf != null)
				{
					List<Region> leafPath = RootRegion.RegionsInPathFrom(leaf);
					if (leafPath != null)
					{
						foreach (Region r in leafPath)
							r.UpdateArcBoundingBox();
					}
				}
			}
		}

		/// <summary>
		/// Updates the bounding box for any region affected by the addition of the given arcs
		/// </summary>
		public void AddTransferArcs(List<SpatialEdge> addedArcs)
		{
			// We need to update the max arc extension in all regions that contain a node
			// at one end of an added arc

			// Collect nodes at either end of added arcs
			List<SpatialNode> nodes = addedArcs.Select(a => a.From).Concat(addedArcs.Select(a => a.To)).Distinct().ToList();

			// Find leaf regions with these nodes
			var leafRegions = nodes.Select(n => RootRegion.LeafRegionContaining(n)).Distinct().ToList();

			//if (leafRegions.Contains(null))
			//	throw new Exception("SpatialGraphNode at arc to be added to spatial index was not present in index");

			// Process
			foreach (Region leaf in leafRegions)
			{
				if (leaf != null)
				{
					List<Region> leafPath = RootRegion.RegionsInPathFrom(leaf);
					if (leafPath != null)
					{
						foreach (Region r in leafPath)
							r.UpdateArcBoundingBox();
					}
				}
			}
		}
		/// <summary>
		/// Updates the index after the given arcs have been removed from the graph.
		/// 
		/// Requires that the removed arcs still reference the nodes that the connected, 
		/// and that these nodes are still present in the index
		/// </summary>
		/// <param name="removedArcs"></param>
		public void RemoveArcs(List<SpatialEdge> removedArcs)
		{
			// We need to update the max arc extension in all regions that contain a node
			// at one end of a removed

			// Collect nodes at either end of added arcs
			List<SpatialNode> nodes = removedArcs.Select(a => a.From).Concat(removedArcs.Select(a => a.To)).ToList();

			// Find leaf regions with these nodes
			var leafRegions = nodes.Distinct().Select(n => RootRegion.LeafRegionContaining(n)).Distinct().ToList();

			if (leafRegions.Contains(null))
				throw new Exception("SpatialGraphNode at arc to be removed from spatial index was not present in index");

			// Process
			foreach (Region leaf in leafRegions)
			{
				foreach (Region r in RootRegion.RegionsInPathFrom(leaf))
					r.UpdateArcBoundingBox();
			}
		}

		/// <summary>
		/// Verifies that this spatial index is correct for the nodes and arcs in the given 
		/// graph. Throws an exception if an inconsistency is found.
		/// </summary>
		public void Verify(SpatialGraph graph)
		{
			if (RootRegion.NodeCount != graph.Nodes.Count)
				throw new Exception("SpatialGraphNode count in spatial index is incorrect");

			HashSet<SpatialNode> nodesDiff = new HashSet<SpatialNode>(RootRegion.AllNodes);
			nodesDiff.SymmetricExceptWith(graph.Nodes);

			if (nodesDiff.Count != 0)
				throw new Exception("Nodes in spatial index do not match nodes in graph");

			RootRegion.Verify(this);
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Returns the leaf region closest to the given coordinate. If the coordinate
		/// is covered by several leaf nodes, one is arbitrarily chosen.
		/// </summary>
		private Region ClosestLeafRegion(ICoordinate coordinate)
		{
			// <Region, distance from coordinate region bounding box>
			List<Tuple<Region, double>> candidateRegions = new List<Tuple<Region, double>>();

			candidateRegions.Add(new Tuple<Region, double>(RootRegion, RootRegion.BoundingBox.MinDistance(coordinate)));

			while (true)
			{
				Tuple<Region, double> bestItem = candidateRegions.MinBy(x => x.Item2);
				candidateRegions.Remove(bestItem);

				Region region = bestItem.Item1;
				if (region.IsLeaf)
					return region;

				// Split into subregions
				foreach (var sub in region.SubRegions)
					candidateRegions.Add(new Tuple<Region, double>(sub, sub.BoundingBox.MinDistance(coordinate)));
			}
		}

		#endregion

		#region Inner classes

		/// <summary>
		/// A region in a spatial index. 
		/// A region contains a subset of the nodes, possibly organized in subregions.
		/// Each region keeps track of how many nodes it contains, a bounding box for
		/// the nodes and how far outside this bounding box any arc connected to one
		/// of the nodes can reach.
		/// </summary>
		public class Region
		{
			#region Private data members

			/// <summary>
			/// The type of coordinate that the owning index uses.
			/// </summary>
			Type _coordinateType => _owner._coordinateType;

			/// <summary>
			/// The spatial index we're part of
			/// </summary>
			SpatialIndex _owner;

			/// <summary>
			/// If we're a leaf region, the nodes we contain.
			/// Otherwise null.
			/// </summary>
			List<SpatialNode> _nodes;

			/// <summary>
			/// If we're not a leaf region, the subregions we contain.
			/// Otherwise null.
			/// </summary>
			List<Region> _subRegions;

			/// <summary>
			/// A bounding box that fully contains all arcs connected to a node in the region 
			/// </summary>
			public IBoundingBox _arcBoundingBox;

			#endregion

			#region Public properties

			/// <summary>
			/// A bounding box for the nodes and subregions we contain
			/// </summary>
			public IBoundingBox BoundingBox { get; private set; }

			/// <summary>
			/// The total number of nodes in the region an subregions
			/// </summary>
			public int NodeCount { get; private set; }

			/// <summary>
			/// The nodes in the region, if it is a leaf region.
			/// Throws an exception it this is not a leaf region.
			/// </summary>
			public IEnumerable<SpatialNode> Nodes
			{
				get
				{
					if (_nodes == null)
						throw new InvalidOperationException("Region is not a leaf region");
					return _nodes;
				}
			}

			/// <summary>
			/// All the nodes in the region.
			/// This method collects nodes from all subregions. If the region is
			/// high in the tree, the result can be a very large number of nodes.
			/// </summary>
			public IEnumerable<SpatialNode> AllNodes
			{
				get
				{
					if (_nodes == null)
						return _subRegions.SelectMany(x => x.AllNodes);
					return _nodes;
				}
			}

			/// <summary>
			/// True if this region is subdivided in subregions
			/// </summary>
			public bool HasSubRegions
			{
				get { return _subRegions != null; }
			}

			/// <summary>
			/// True if this region is a leaf region, i.e. not subdivided in subregions
			/// </summary>
			public bool IsLeaf
			{
				get { return _subRegions == null; }
			}

			/// <summary>
			/// The region's subregions, if any
			/// </summary>
			public IEnumerable<Region> SubRegions { get { return _subRegions ?? new List<Region>(); } }

			#endregion

			/// <summary>
			/// Constructor.
			/// Creates a region containing the given nodes. 
			/// If there are more nodes than allowed in a leaf region, subdivides into subregions.
			/// </summary>
			public Region(IEnumerable<SpatialNode> nodes, SpatialIndex owner)
			{
				_owner = owner;

#if DEBUG
				if (_owner._coordinateType != typeof(GeoCoordinate) &&
						_owner._coordinateType != typeof(Coordinate))
					throw new NotImplementedException($"Unexpected coordinate type {_coordinateType.Name}");
#endif

				Balance(nodes);
			}

			/// <summary>
			/// Constructor.
			/// Creates a region consisting of the given subregions.
			/// </summary>
			public Region(IEnumerable<Region> subRegions, SpatialIndex owner)
			{
				_owner = owner;

#if DEBUG
				if (_owner._coordinateType != typeof(GeoCoordinate) &&
						_owner._coordinateType != typeof(Coordinate))
					throw new NotImplementedException($"Unexpected coordinate type {_coordinateType.Name}");
#endif

				_subRegions = subRegions.ToList();
				UpdateBoundingBoxAndNodeCount();
				UpdateArcBoundingBox();
			}

			#region Public methods

			/// <summary>
			/// Updates the bounding box to the smallest box that contains all
			/// nodes or subregions.
			/// Updates the node count.
			/// </summary>
			public void UpdateBoundingBoxAndNodeCount()
			{
				if (_nodes != null)
				{
					IBoundingBox bb;
					if (!_nodes.Any())
					{
						if (_coordinateType == typeof(GeoCoordinate))
							bb = new BoundingBox();
						else 
							bb = new BoundingBoxCartesian();
					}
					else {
						if (_coordinateType == typeof(GeoCoordinate))
							bb = new BoundingBox(_nodes.First().Coordinate as GeoCoordinate);
						else 
							bb = new BoundingBoxCartesian(_nodes.First().Coordinate as Coordinate);
					}
					foreach (var node in _nodes)
						bb.ExpandBy(node.Coordinate);
					BoundingBox = bb;
				}
				else
				{
					var bb = _subRegions.First().BoundingBox;
					foreach (var region in _subRegions)
						bb.ExpandBy(region.BoundingBox);
					BoundingBox = bb;
				}

				if (_nodes != null)
					NodeCount = _nodes.Count;
				else
					NodeCount = _subRegions.Sum(r => r.NodeCount);
			}

			/// <summary>
			/// Updates the _arcBoundingBox member based on the nodes/arcs or subregions
			/// </summary>
			public void UpdateArcBoundingBox()
			{
				if (_nodes != null)
				{
					if (_coordinateType == typeof(Coordinate))
						_arcBoundingBox = new BoundingBoxCartesian(BoundingBox as BoundingBoxCartesian);
					else
						_arcBoundingBox = new BoundingBox(BoundingBox as BoundingBox);
					foreach (SpatialNode node in _nodes)
					{
						foreach (SpatialEdge arc in node.AllEdges)
						{
							_arcBoundingBox.ExpandBy(arc.To.Coordinate);
							_arcBoundingBox.ExpandBy(arc.From.Coordinate);
						}
					}
				}
				else
				{
					if (_coordinateType == typeof(Coordinate))
						_arcBoundingBox = new BoundingBoxCartesian(_subRegions[0]._arcBoundingBox as BoundingBoxCartesian);
					else
						_arcBoundingBox = new BoundingBox(_subRegions[0]._arcBoundingBox as BoundingBox);
					foreach (Region region in _subRegions.Skip(1))
						_arcBoundingBox.ExpandBy(region._arcBoundingBox);
				}
			}

			/// <summary>
			/// Worker method for SpatialIndex.GetNodePairsWithSameCoordinate, for a particular
			/// region pair
			/// </summary>
			public void FindNodePairsWithSameCoordinate(Region otherRegion, List<Tuple<SpatialNode, SpatialNode>> result)
			{
				if (!this.BoundingBox.Intersects(otherRegion.BoundingBox))
					// No shared coordinates
					return;

				if (_subRegions != null || otherRegion._subRegions != null)
				{
					// Recurse and conquer. 

					bool recurseThisFirst = otherRegion._subRegions == null ||
						(_subRegions != null && BoundingBox.Area > otherRegion.BoundingBox.Area);

					if (recurseThisFirst)
					{
						foreach (var subRegion in _subRegions)
							subRegion.FindNodePairsWithSameCoordinate(otherRegion, result);
						return;
					}
					else
					{
						foreach (var otherSubRegion in otherRegion._subRegions)
							FindNodePairsWithSameCoordinate(otherSubRegion, result);
						return;
					}
				}

				// Base case: Two overlapping leaf regions

				List<ICoordinate> mergedCoordinates = new List<ICoordinate>();

				foreach (var myNode in _nodes)
				{
					foreach (var otherNode in otherRegion._nodes)
					{
						if (myNode.Coordinate.Equals(otherNode.Coordinate) && !mergedCoordinates.Contains(myNode.Coordinate))
						{
							result.Add(new Tuple<SpatialNode, SpatialNode>(myNode, otherNode));
							mergedCoordinates.Add(myNode.Coordinate);
							break;
						}
					}
				}
			}

			/// <summary>
			/// Worker method for SpatialIndex.GetCloseNodePairs, for a particular subset of regions
			/// </summary>
			/// <param name="otherRegions">The set of regions in the other tree to look for close nodes in</param>
			/// <param name="distanceTolerance"></param>
			/// <param name="result"></param>
			/// <param name="matchedNodes">The set of nodes that have already been matched, keyed on the leaf region
			/// in the 'other' region tree</param>
			public void FindCloseNodePairs(List<Region> otherRegions, double distanceTolerance, List<Tuple<SpatialNode, SpatialNode>> result, Dictionary<Region, List<SpatialNode>> matchedNodes)
			{
				IBoundingBox testBox = BoundingBox;
				testBox.ExpandBy(distanceTolerance);

				// Eliminate regions where no node can be close enough
				otherRegions = otherRegions.Where(r => testBox.Intersects(r.BoundingBox)).ToList();

				if (otherRegions.Count == 0)
					return;

				bool othersHaveSubregions = otherRegions.Any(r => r.HasSubRegions);

				if (HasSubRegions || othersHaveSubregions)
				{
					// Recurse and conquer. 

					bool recurseThisFirst = !othersHaveSubregions ||
						(HasSubRegions && BoundingBox.Area > otherRegions.Sum(x => x.BoundingBox.Area));

					if (recurseThisFirst)
					{
						// Expand this region's subregions
						foreach (var subRegion in _subRegions)
							subRegion.FindCloseNodePairs(otherRegions, distanceTolerance, result, matchedNodes);
						return;
					}
					else
					{
						// Expand other regions' subregions
						otherRegions = otherRegions.SelectMany(r => r.HasSubRegions ? r._subRegions : new List<Region>() { r }).ToList();
						FindCloseNodePairs(otherRegions, distanceTolerance, result, matchedNodes);
						return;
					}
				}

				// Base case: A leaf region and its close leaf regions in the other tree

				// Find nodes in other regions that have not been matched yet
				List<SpatialNode> matchCandidates = otherRegions.SelectMany(r => r._nodes).ToList();
				foreach (var region in otherRegions)
				{
					List<SpatialNode> matchedNodesInRegion = null;
					if (matchedNodes.TryGetValue(region, out matchedNodesInRegion))
						matchCandidates.RemoveAll(n => matchedNodesInRegion.Contains(n));
				}

				foreach (var myNode in _nodes)
				{
					double bestDist = double.PositiveInfinity;
					SpatialNode best = null;
					Region bestRegion = null;

					foreach (var otherRegion in otherRegions)
					{
						SpatialNode bestInRegion = otherRegion._nodes.MinBy(node => myNode.Coordinate.DistanceTo(node.Coordinate));

						if (myNode.Coordinate.DistanceTo(bestInRegion.Coordinate) < bestDist)
						{
							bestDist = myNode.Coordinate.DistanceTo(bestInRegion.Coordinate);
							best = bestInRegion;
							bestRegion = otherRegion;
						}
					}

					List<SpatialNode> matchedNodesInRegion = null;
					if (matchedNodes.TryGetValue(bestRegion, out matchedNodesInRegion) && matchedNodesInRegion.Contains(best))
						// Best node was already matched -- cannot use
						continue;

					if (bestDist <= distanceTolerance)
					{
						// Match!
						result.Add(new Tuple<SpatialNode, SpatialNode>(myNode, best));

						if (matchedNodesInRegion == null)
						{
							matchedNodesInRegion = new List<SpatialNode>();
							matchedNodes.Add(bestRegion, matchedNodesInRegion);
						}
						matchedNodesInRegion.Add(best);
					}
				}
			}

			/// <summary>
			/// Worker method for SpatialIndex.GetAllCloseNodePairs, for a particular subset of regions
			/// </summary>
			/// <param name="otherRegions">The set of regions in the other tree to look for close nodes in</param>
			/// <param name="distanceTolerance">The distance tolerance used to find close nodes</param>
			/// <param name="result">The set of nodes pairs identified in the 'other' region tree</param>
			public void FindAllCloseNodePairs(List<Region> otherRegions, double distanceTolerance, List<Tuple<SpatialNode, SpatialNode>> result)
			{
				IBoundingBox testBox = BoundingBox;
				testBox.ExpandBy(distanceTolerance);

				// Eliminate regions where no node can be close enough
				otherRegions = otherRegions.Where(r => testBox.Intersects(r.BoundingBox)).ToList();

				if (otherRegions.Count == 0)
					return;

				bool othersHaveSubregions = otherRegions.Any(r => r.HasSubRegions);

				if (HasSubRegions || othersHaveSubregions)
				{
					if(HasSubRegions)
					{
						// Expand the public transit region's subregions
						foreach (var subRegion in _subRegions)
							subRegion.FindAllCloseNodePairs(otherRegions, distanceTolerance, result);
						return;
					}
					else
					{
						// Expand the network region's subregions
						otherRegions = otherRegions.SelectMany(r => r.HasSubRegions ? r._subRegions : new List<Region>() { r }).ToList();
						FindAllCloseNodePairs(otherRegions, distanceTolerance, result);
						return;
					}
				}

				//When both are on a leaf region, find the nodes that are close enough
				foreach (var myNode in _nodes)
				{
					foreach (var otherRegion in otherRegions)
					{
						List<SpatialNode> candidatesInRegion = otherRegion._nodes.Where(r => myNode.Coordinate.DistanceTo(r.Coordinate) < distanceTolerance).ToList();
						foreach (var connectToNode in candidatesInRegion)
						{
							result.Add(new Tuple<SpatialNode, SpatialNode>(myNode, connectToNode));
						}
					}
				}
				return;
			}


			/// <summary>
			/// Returns true if any arc connected to a node in this
			/// region intersects the line from c1 to c2
			/// </summary>
			public bool IntersectsAnyArc(ICoordinate c1, ICoordinate c2)
			{
				double maxArcsLength = c1.DistanceTo(c2) / 2;

				if (_arcBoundingBox.MinDistance(c1) > maxArcsLength && _arcBoundingBox.MinDistance(c2) > maxArcsLength)
					// Endpoints are far enough from bounding box that nothing can intersect
					return false;

				if (HasSubRegions)
					return SubRegions.Any(r => r.IntersectsAnyArc(c1, c2));

				var arcs = _nodes.SelectMany(n => n.AllEdges);
				return (arcs.Any(a => ICoordinateExtensions.Intersects(c1, c2, a.From.Coordinate, a.To.Coordinate)));
			}

			/// <summary>
			/// Merges the given region into this region tree, at a level with similar node counts
			/// </summary>
			public void Merge(Region region)
			{
				// Find the subregion that overlaps best with the region to merge

				Region best = _subRegions.MinBy(r => r.BoundingBox.ExpansionArea(region.BoundingBox));

                if (best.NodeCount > region.NodeCount && best.HasSubRegions)
				{
					// This subregion has more nodes than the region to merge, so merge at subregion level
					best.Merge(region);
					UpdateBoundingBoxAndNodeCount();
					UpdateArcBoundingBox();
					return;
				}

				// Merge at this level.
				// Replace best subregion with a new subregion that is the union of the best subregion and the region to merge
				Region newRegion = new Region(new Region[] { best, region }, _owner);
				_subRegions.Remove(best);
				_subRegions.Add(newRegion);

				UpdateBoundingBoxAndNodeCount();
				UpdateArcBoundingBox();
			}

			/// <summary>
			/// Sets the given owner for this region and subregions
			/// </summary>
			/// <param name="owner"></param>
			public void SetOwner(SpatialIndex owner)
			{
				_owner = owner;
				foreach (var subRegion in SubRegions)
					subRegion.SetOwner(owner);
			}

			/// <summary>
			/// Returns a lower bound on the distance between the given coordinate and any
			/// point on an arc that connects to a node in the region
			/// </summary>
			public double DistanceToArcLowerBound(ICoordinate coordinate)
			{
				return _arcBoundingBox.MinDistance(coordinate);
			}

			/// <summary>
			/// Adds the given nodes to the region.
			/// May only be used for a leaf region.
			/// </summary>
			public void AddNodes(List<SpatialNode> nodes)
			{
				if (HasSubRegions)
					throw new InvalidOperationException("Not a leaf region");

				_nodes.AddRange(nodes);
				UpdateBoundingBoxAndNodeCount();
				UpdateArcBoundingBox();
			}

			/// <summary>
			/// Removes the given nodes from the region.
			/// May only be used for a leaf region.
			/// </summary>
			public void RemoveNodes(List<SpatialNode> nodes)
			{
				if (nodes.Count == _nodes.Count)
					throw new InvalidOperationException("Cannot remove all nodes from a leaf node");

				if (HasSubRegions)
					throw new InvalidOperationException("Not a leaf region");

				bool c = BoundingBox.Contains(nodes[0].Coordinate);

				foreach (SpatialNode node in nodes)
					_nodes.Remove(node);

				UpdateBoundingBoxAndNodeCount();
				UpdateArcBoundingBox();
			}

			/// <summary>
			/// Returns the sequence of regions in the tree from the given descendant region to
			/// this region, inclusive. Returns null if the given region is not a descendant of this
			/// region.
			/// </summary>
			public List<Region> RegionsInPathFrom(Region region)
			{
				if (this == region)
				{
					return new List<Region> { region };
				}

				if (!BoundingBox.Contains(region.BoundingBox))
					// region is not on this path
					return null;

				if (IsLeaf)
					return null;

				List<Region> subpath = _subRegions.Select(sub => sub.RegionsInPathFrom(region)).SingleOrDefault(path => path != null);

				if (subpath != null)
					subpath.Add(this);

				return subpath;
			}

			/// <summary>
			/// Rebalances the region tree below this region. All regions below
			/// are created anew as new objects.
			/// </summary>
			public void Rebalance()
			{
				Balance(AllNodes.ToList());
			}

			/// <summary>
			/// Returns the leaf region that contains the given node.
			/// If no such region is found in the subtree below this node, returns null.
			/// </summary>
			public Region LeafRegionContaining(SpatialNode n)
			{
				if (!BoundingBox.Contains(n.Coordinate))
					return null;

				if (!HasSubRegions)
					return _nodes.Contains(n) ? this : null;

				foreach (var sub in _subRegions)
				{
					Region leafInSub = sub.LeafRegionContaining(n);
					if (leafInSub != null)
						return leafInSub;
				}

				return null;
			}

			#endregion

			#region Private methods

			/// <summary>
			/// Fills this region object with the given nodes, subdividing into
			/// subregions if necessary
			/// </summary>
			private void Balance(IEnumerable<SpatialNode> nodes)
			{
				if (nodes.Count() > _owner.MaxNodesInARegion)
				{
					// Subdivide
					double maxLat = nodes.Max(x => x.Coordinate.Y);
					double minLat = nodes.Min(x => x.Coordinate.Y);
					double maxLon = nodes.Max(x => x.Coordinate.X);
					double minLon = nodes.Min(x => x.Coordinate.X);

					if (maxLat - minLat > maxLon - minLon)
					{
						// Divide by latitude
						double divideLat = (maxLat + minLat) / 2.0;

						List<SpatialNode> north = nodes.Where(x => x.Coordinate.Y >= divideLat).ToList();
						List<SpatialNode> south = nodes.Where(x => x.Coordinate.Y < divideLat).ToList();
						_nodes = null;

						_subRegions = new List<Region>() {
							new Region(north, _owner),
							new Region(south, _owner)
						};
					}
					else if (maxLon == minLon)
					{
						// All nodes have same coordinate
						_nodes = nodes.ToList();
						_subRegions = null;
					}
					else
					{
						// Divide by longitude
						double divideLon = (maxLon + minLon) / 2.0;

						List<SpatialNode> east = nodes.Where(x => x.Coordinate.X >= divideLon).ToList();
						List<SpatialNode> west = nodes.Where(x => x.Coordinate.X < divideLon).ToList();
						_nodes = null;

						_subRegions = new List<Region>() {
							new Region(east, _owner),
							new Region(west, _owner)
						};
					}

				}
				else
				{
					// Create leaf region
					_nodes = nodes.ToList();
					_subRegions = null;
				}

				UpdateBoundingBoxAndNodeCount();
				UpdateArcBoundingBox();
			}

			/// <summary>
			/// Removes the given subregion from this region
			/// </summary>
			/// <param name="region"></param>
			internal void RemoveSubRegion(Region region)
			{
				if (_nodes != null)
					throw new InvalidOperationException("Cannot remove subregion from a leaf node");

				if (_subRegions.Contains(region))
					_subRegions.Remove(region);
				else
					throw new InvalidOperationException("Region does not contain the given subregion");
			}

			#endregion

			/// <summary>
			/// Verifies that the data in this region and subregions is correct.
			/// Throws an exception if an inconsistency is found.
			/// </summary>
			public void Verify(SpatialIndex owner)
			{
				if (_owner != owner)
					throw new Exception("Region has incorrect owner");

				if ((_nodes == null) == (_subRegions == null))
					throw new Exception("Region has both nodes and subregions or neither");

				if (_nodes != null && _nodes.Count == 0)
					throw new Exception("List of nodes in leaf region is empty");

				if (_subRegions != null && _subRegions.Count == 0)
					throw new Exception("List of subregions in non-leaf region is empty");

				IBoundingBox oldBb = BoundingBox;
				int oldCount = NodeCount;
				UpdateBoundingBoxAndNodeCount();
				if (!oldBb.Equals(BoundingBox))
					throw new Exception("Region has incorrect bounding box");
				if (oldCount != NodeCount)
					throw new Exception("Region has incorrect node count");

				oldBb = _arcBoundingBox;
				UpdateArcBoundingBox();
				if (!oldBb.Equals(_arcBoundingBox))
					throw new Exception("Region has incorrect arc bounding box");

				if (_nodes != null)
				{
					// Leaf region

					if (Nodes.Any(n => !BoundingBox.Contains(n.Coordinate)))
						throw new Exception("Region has a node outside bounding box");
					if (NodeCount != Nodes.Count())
						throw new Exception("Region has incorrect node count");
					if (Nodes.SelectMany(n => n.AllEdges).Any(arc => !_arcBoundingBox.Contains(arc.From.Coordinate) || !_arcBoundingBox.Contains(arc.To.Coordinate)))
						throw new Exception("Region has an arc outside arc bounding box");

				}

				if (_subRegions != null)
					foreach (var subRegion in _subRegions)
						subRegion.Verify(owner);
			}
		}

		#endregion

	}
}
