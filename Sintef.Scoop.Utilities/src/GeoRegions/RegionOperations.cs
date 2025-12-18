//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Sintef.Scoop.Utilities.GeoRegions.Topology;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities.GeoRegions
{
	/// <summary>
	/// Utility methods for doing set operations on regions.
	/// </summary>
	public static class RegionOperations
	{
		/// <summary>
		/// A binary operation type that to be applied on two regions to get a new one
		/// </summary>
		internal enum OperationType
		{
			/// <summary>
			/// The union of the two regions
			/// </summary>
			Union,

			/// <summary>
			/// The intersection of the two regions
			/// </summary>
			Intersection,

			/// <summary>
			/// The part inside the first region and outisde the second
			/// </summary>
			FirstMinusSecond,

			/// <summary>
			/// The part inside the second region and outisde the first
			/// </summary>
			SecondMinusFirst,

			/// <summary>
			/// The symmetric difference of the two regions, i.e. the part that is contained in exactly one of the regions, and not in the other.
			/// </summary>
			SymmetricDifference
		}

		/// <summary>
		/// Returns a region representing the complement of a region
		/// </summary>
		/// <param name="region">The region to get the complement of</param>
		public static GeoRegion Complement(GeoRegion region)
		{
			if (region.IsAll)
			{
				return new GeoRegion();
			}
			else if (region.IsEmpty)
			{
				return GeoRegion.All();
			}
			else
			{
				RegionTopology regTop = new RegionTopology();
				regTop.ApplyRegion(region);
				regTop.ComplementExisting();
				return regTop.ToRegion();
			}
		}

		/// <summary>
		/// Returns a region representing the union of two regions.
		/// </summary>
		/// <param name="region1">The first region in the union</param>
		/// <param name="region2">The second region in the union</param>
		public static GeoRegion Union(GeoRegion region1, GeoRegion region2)
		{
			return BinaryOperation(region1, region2, OperationType.Union);
		}

		/// <summary>
		/// Returns a region representing the intersection of two regions.
		/// </summary>
		/// <param name="region1">The first region in the intersection</param>
		/// <param name="region2">The second region in the intersection</param>
		public static GeoRegion Intersection(GeoRegion region1, GeoRegion region2)
		{
			return BinaryOperation(region1, region2, OperationType.Intersection);
		}

		/// <summary>
		/// Returns a region representing the difference of two regions, i.e. the region that is inside the first and outside the second
		/// </summary>
		/// <param name="region1">The region to subtract the other region from</param>
		/// <param name="region2">The region to be subtracted from the first region</param>
		public static GeoRegion Minus(GeoRegion region1, GeoRegion region2)
		{
			return BinaryOperation(region1, region2, OperationType.FirstMinusSecond);
		}

		/// <summary>
		/// Returns a region representing the symmetric difference of two regions, i.e. the region that is inside one of the two regions, and outside the other
		/// </summary>
		/// <param name="region1">The first region in the symetric difference</param>
		/// <param name="region2">The second region in the symetric difference</param>
		public static GeoRegion SymmetricDifference(GeoRegion region1, GeoRegion region2)
		{
			return BinaryOperation(region1, region2, OperationType.SymmetricDifference);
		}

		/// <summary>
		/// Returns a region representing the union of a collection of regions.
		/// </summary>
		/// <param name="regions">The regions to take the union of</param>
		public static GeoRegion Union(IEnumerable<GeoRegion> regions)
		{
			return UnionOrIntersection(regions, true);
		}

		/// <summary>
		/// Returns a region representing the intersection of a collection of regions.
		/// </summary>
		/// <param name="regions">The regions to take the intersection of</param>
		public static GeoRegion Intersection(IEnumerable<GeoRegion> regions)
		{
			return UnionOrIntersection(regions, false);
		}

		/// <summary>
		/// Returns a region representing the result of applying a binary operation on two input regions
		/// </summary>
		/// <param name="region1">The first region to apply the operation to</param>
		/// <param name="region2">The second region to apply the operation to</param>
		/// <param name="operationType">The operation type to be applied to the two input regions</param>
		private static GeoRegion BinaryOperation(GeoRegion region1, GeoRegion region2, OperationType operationType)
		{
			if (region1.IsAll)
			{
				switch (operationType)
				{
					case OperationType.Union:
						return GeoRegion.All();
					case OperationType.Intersection:
						return region2.Clone();
					case OperationType.FirstMinusSecond:
					case OperationType.SymmetricDifference:
						return Complement(region2);
					case OperationType.SecondMinusFirst:
						return new GeoRegion();
					default:
						return null;
				}
			}
			else if (region1.IsEmpty)
			{
				switch (operationType)
				{
					case OperationType.Union:
					case OperationType.SecondMinusFirst:
					case OperationType.SymmetricDifference:
						return region2.Clone();
					case OperationType.Intersection:
					case OperationType.FirstMinusSecond:
						return new GeoRegion();
					default:
						return null;
				}
			}

			if (region2.IsAll)
			{
				switch (operationType)
				{
					case OperationType.Union:
						return GeoRegion.All();
					case OperationType.Intersection:
						return region1.Clone();
					case OperationType.FirstMinusSecond:
						return new GeoRegion();
					case OperationType.SecondMinusFirst:
					case OperationType.SymmetricDifference:
						return Complement(region1);
					default:
						return null;
				}
			}
			else if (region2.IsEmpty)
			{
				switch (operationType)
				{
					case OperationType.Union:
					case OperationType.FirstMinusSecond:
					case OperationType.SymmetricDifference:
						return region1.Clone();
					case OperationType.Intersection:
					case OperationType.SecondMinusFirst:
						return new GeoRegion();
					default:
						return null;
				}
			}

			RegionTopology regTop = new RegionTopology();
			regTop.ApplyRegion(region1);
			regTop.ApplyRegion(region2);
			regTop.ApplyOperation(operationType);
			return regTop.ToRegion();
		}

		/// <summary>
		/// Returns a region representing the union or intersection applied on a collection of regions.
		/// </summary>
		/// <param name="regions">The regions to apply the union or intersection to</param>
		/// <param name="isUnion">Whether the operation to be applied is union (true) or intersection (false)</param>
		private static GeoRegion UnionOrIntersection(IEnumerable<GeoRegion> regions, bool isUnion)
		{
			IEnumerable<GeoRegion> regionsToApply;

			if (isUnion)
			{
				if (regions.Any(reg => reg.IsAll))
				{
					return GeoRegion.All();
				}
				regionsToApply = regions.Where(reg => !reg.IsEmpty);
			}
			else
			{
				if (regions.Any(reg => reg.IsEmpty))
				{
					return new GeoRegion();
				}
				regionsToApply = regions.Where(reg => !reg.IsAll);
			}

			if (!regionsToApply.Any())
			{
				return isUnion ? new GeoRegion() : GeoRegion.All();
			}

			if (regionsToApply.Take(2).Count() == 1)
			{
				return regionsToApply.Single().Clone();
			}

			RegionTopology regTop = new RegionTopology();
			OperationType operationType = isUnion ? OperationType.Union : OperationType.Intersection;
			bool isFirst = true;
			foreach (GeoRegion region in regionsToApply)
			{
				regTop.ApplyRegion(region);
				if (isFirst)
				{
					isFirst = false;
				}
				else
				{
					regTop.ApplyOperation(operationType);
				}
			}
			return regTop.ToRegion();
		}
	}
}
