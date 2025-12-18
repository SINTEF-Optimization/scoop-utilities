//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace Sintef.Scoop.Utilities.GeoCoding
{
	/// <summary>
	/// A node in a spatial graph.
	/// </summary>
	[DataContract]
//	[TypeConverter(typeof(GenericObjectConverter<SpatialNode>))]
	public class SpatialNode
	{
		#region Public properties

		/// <summary>
		/// The graph that the node belongs to.
		/// </summary>
		public SpatialGraph Graph { get; }

		/// <summary>
		/// The node's ID. This member is for external reference, e.g. for route explanation
		/// </summary>
		[DataMember] 
		public string Id { get; set; }

		/// <summary>
		/// A unique index among nodes in the graph. Node indices for a graph
		/// start at 0 and are contiguous.
		/// 
		/// Node indices are guaranteed to be constant as long as no nodes
		/// are removed from the graph. Node indices do generally NOT
		/// coincide with indices in the Graph.Nodes list.
		/// </summary>
		public int Index { get; internal set; }

		/// <summary>
		/// The node's coordinate
		/// </summary>
		public ICoordinate Coordinate { get; set; }

		/// <summary>
		/// The collection of edges where this node is the one of the nodes
		/// </summary>
		public List<SpatialEdge> AllEdges { get; private set; }

		/// <summary>
		/// Enumerates the edges where this node is the From node
		/// </summary>
		public IEnumerable<SpatialEdge> OutEdges => AllEdges.Where(e => e.From == this);

		/// <summary>
		/// Enumerates the edges where this node is the To node
		/// </summary>
		public IEnumerable<SpatialEdge> InEdges => AllEdges.Where(e => e.To == this);

		#endregion

		#region Constructor

		/// <summary>
		/// Creates a node with the given ID and coordinate
		/// </summary>
		public SpatialNode(SpatialGraph graph, string id, ICoordinate coordinate)
		{
			Graph = graph;
			Id = id;
			Coordinate = coordinate;
			AllEdges = new List<SpatialEdge>();
		}

		#endregion

		/// <summary>
		/// String representation of the node
		/// </summary>
		public override string ToString()
		{
			return "[" + Id + " at " + Coordinate.ToString() + "]";
		}

	}
}
