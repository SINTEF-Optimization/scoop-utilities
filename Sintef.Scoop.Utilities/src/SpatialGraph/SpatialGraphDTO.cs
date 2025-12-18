//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sintef.Scoop.Utilities.SpatialGraphDTO
{
	/// <summary>
	/// A data structure for serialization of a <see cref="SpatialGraph"/> network
	/// </summary>
	public class SpatialGraphContainer
	{
		/// <summary>
		/// The network
		/// </summary>
		[JsonRequired]
		public SpatialGraph Network { get; set; }
	}

	/// <summary>
	/// A data structure for serialization of a network
	/// </summary>
	public class SpatialGraph
	{
		/// <summary>
		/// Id that can be used to identify the graph among a collection of graph objects.
		/// Not required.
		/// </summary>
		public string Id { get; set;  }

		/// <summary>
		/// The nodes
		/// </summary>
		public List<Node> Nodes { get; set; }

		/// <summary>
		/// The edges
		/// </summary>
		public List<Edge> Edges { get; set; }

		/// <summary>
		/// The coordinate system
		/// </summary>
		public CoordinateSystem coordinate_system { get; set; }
	}

	/// <summary>
	/// A coordinate system for serialization of a <see cref="SpatialGraph"/> network
	/// </summary>
	public class CoordinateSystem
	{
		/// <summary>
		/// The longitude and latitude of the coordinate system's origin
		/// </summary>
		public List<double> origoInLongitudeLatitude { get; set; }
	}

	/// <summary>
	/// An edge for serialization of a <see cref="SpatialGraph"/> network
	/// </summary>
	public class Edge
	{
		/// <summary>
		/// The edge's ID
		/// </summary>
		public string id { get; set; }

		/// <summary>
		/// The edge's coordinates
		/// </summary>
		public List<List<double>> coordinates { get; set; }

		/// <summary>
		/// The ID of the edge's start node
		/// </summary>
		public NodeRef Node1 { get; set; }

		/// <summary>
		/// The ID of the edge's end node
		/// </summary>
		public NodeRef Node2 { get; set; }
	}

	/// <summary>
	/// A node reference for serialization of a <see cref="SpatialGraph"/> network
	/// </summary>
	public class NodeRef
	{
		/// <summary>
		/// The node's ID
		/// </summary>
		public string nodeId { get; set; }
	}

	/// <summary>
	/// A node for serialization of a <see cref="SpatialGraph"/> network
	/// </summary>
	public class Node
	{
		/// <summary>
		/// The node's ID
		/// </summary>
		public string id { get; set; }

		/// <summary>
		/// The node's coordinates
		/// </summary>
		public List<double> coordinates { get; set; }
	}
}

