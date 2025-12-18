//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using C5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sintef.Scoop.Utilities
{
	/// <summary>
	/// Provides functionality to set up graphs and algorithms to calculate the shortest paths on these graphs using numeric costs.
	/// 
	/// In the graph, nodes are associated with an instance of <typeparamref name="T"/>. Only one node in the graph 
	/// can be associated with this instance.
	/// 
	/// The edges can be both directed or undirected, and can optionally be associated with instances
	/// of <typeparamref name="U"/>. In this implementation the distance of an edge is a double.
	/// 
	/// In this implementation the cost of traversing a path can be any scalar which implements <see cref="INumber{TSelf}"/>
	/// and <see cref="IMinMaxValue{TSelf}"/>. For a more general 
	/// version check <see cref="ShortestPathCalculator{T, U, V}"/>. This version is provided as it may provide slightly improved 
	/// performance over <see cref="ShortestPathCalculator{T, U, V}"/>.
	/// </summary>
	/// <typeparam name="T">A class or value type associated with the nodes in the graph</typeparam>
	/// <typeparam name="U">A class optionally associated with the edges in the graph</typeparam>
	/// <typeparam name="V">The value type used for node weights. This must be a number type which supports min and max value.</typeparam>
	public class ShortestPathCalculator<T, U, V> where V: INumber<V>, IMinMaxValue<V>
	{

		#region Private members

		/// <summary>
		/// The nodes in the graph
		/// </summary>
		private readonly List<Node<V>> _nodes = new();

		/// <summary>
		/// The edges in the graph
		/// </summary>
		private List<Edge<V>> _edges = new();

		/// <summary>
		/// The nodes in the graph by their associated data
		/// </summary>
		private readonly Dictionary<T, Node<V>> _nodesByData = new();

		/// <summary>
		/// The set of nodes, for quick lookup
		/// </summary>
		private readonly Dictionary<Node<V>, int> _setOfNodes = new();

		private bool _lastAlgorithmUsedGCost;

		#endregion

		#region Nested classes

		/// <summary>
		/// A node in the graph. A node contains information about which object it is
		/// associated with, which edges lead to or from this node, and results from
		/// the shortest path calculation in the form of cost and previous node / segment.
		/// </summary>
		/// <typeparam name="W">The value type used for node weights. This must be a number type which supports min and max value.</typeparam>
		public class Node<W> : IComparable<Node<W>> where W : INumber<W>, IMinMaxValue<W>
		{
			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="t">An object which will be associated with this Node</param>
			public Node(T t)
			{
				Data = t;
				Cost = W.MaxValue;
				OutgoingEdges = new();
				IncomingEdges = new();
			}

			/// <summary>
			/// The data associated with this node
			/// </summary>
			public T Data
			{
				get;
				private set;
			}

			/// <summary>
			/// The edges leading out of this node
			/// </summary>
			public List<Edge<W>> OutgoingEdges
			{
				get;
				private set;
			}

			/// <summary>
			/// The edges leading to this node
			/// </summary>
			public List<Edge<W>> IncomingEdges
			{
				get;
				private set;
			}

			/// <summary>
			/// The cheapest cost for reaching this node
			/// 
			/// This property will not be set until you run an actual shortest path algorithm on the graph.
			/// 
			/// When using A* algorithm this is the heuristic cost for this node, GCost will hold the actual
			/// cheapest cost.
			/// </summary>
			public W Cost
			{
				get;
				set;
			}

			/// <summary>
			/// Used by A* algorithm only. When using A*, Cost is the heuristic cost of a node in the
			/// open set and GCost is the cheapest cost for reaching this node.
			/// </summary>
			public W GCost
			{
				get;
				set;
			}

			/// <summary>
			/// The previous node in the cheapest path to this node, or null if this is the start node.
			/// 
			/// This property will not be set until you run an actual shortest path algorithm on the graph,
			/// </summary>
			public Node<W> PreviousNode
			{
				get;
				set;
			}

			/// <summary>
			/// Handle to this node in C5 while running algorithms using C5.
			/// 
			/// Only for use by the algorithms.
			/// </summary>
			public IPriorityQueueHandle<Node<V>> Handle { get; set; }

			/// <summary>
			/// Set to true by A* if this node is part of the closed set, false otherwise.
			/// </summary>
			public bool ClosedSet
			{
				get;
				set;
			}

			/// <summary>
			/// Comparer so C5 can compare nodes with each other
			/// </summary>
			public int CompareTo(Node<W> other)
			{
				if (other.Cost > Cost)
					return -1;
				else if (Cost > other.Cost)
					return 1;
				return 0;
			}

			/// <summary>
			/// Returns the edge leading from this node to the given node or throws an exception
			/// if no such edge exists.
			/// </summary>
			public Edge<W> GetEdgeLeadingToo(Node<W> n)
			{
				foreach (var edge in OutgoingEdges)
					if (edge.OppositeNode(this) == n)
						return edge;
				throw new InvalidOperationException("No edges lead to the given segment");
			}
		}

		/// <summary>
		/// An edge in the graph
		/// </summary>
		/// <typeparam name="W">The value type used for node weights. This must be a number type which supports min and max value.</typeparam>
		public class Edge<W> where W : INumber<W>, IMinMaxValue<W>
		{
			/// <summary>
			/// Constructor.
			/// </summary>
			public Edge(Node<W> n1, Node<W> n2, W weight, bool isDirected, U associatedObject)
			{
				IsDirected = isDirected;
				Node1 = n1;
				Node2 = n2;
				Weight = weight;
				AssociatedObject = associatedObject;
				if (Node1 == Node2)
					throw new InvalidOperationException("Edge has same start and end node");
			}

			/// <summary>
			/// If true this edge can only be traversed from Node1 to Node2
			/// </summary>
			public bool IsDirected
			{
				get;
				private set;
			}

			/// <summary>
			/// The first node this edge is connected to
			/// </summary>
			public Node<W> Node1
			{
				get;
				private set;
			}

			/// <summary>
			/// The second node this edge is connected to
			/// </summary>
			public Node<W> Node2
			{
				get;
				private set;
			}

			/// <summary>
			/// The edge of this weight. The graph itself has no requirements on
			/// the weight of an edge but do note that some algorithms may require
			/// a graph without negative cost cycles and maybe even positive weights.
			/// </summary>
			public W Weight
			{
				get;
				set;
			}

			/// <summary>
			/// Returns the other node associated with this edge
			/// </summary>
			public Node<W> OppositeNode(Node<W> n)
			{
				if (n == Node1)
					return Node2;
				else if (n == Node2)
					return Node1;
				else
					throw new InvalidOperationException("The given node is not connected to this edge");
			}

			/// <summary>
			/// The object of type U associated with this edge, or null if no objects
			/// are associated with this edge.
			/// </summary>
			public U AssociatedObject
			{
				get;
				private set;
			}
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes an empty graph
		/// </summary>
		public ShortestPathCalculator()
		{
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Detaches the given edge from the network by removing it from the relevant nodes list
		/// of incoming and outgoing edges.
		/// </summary>
		/// <param name="edge"></param>
		private static void DetachEdgeFromNetwork(Edge<V> edge)
		{
			if (!edge.IsDirected)
			{
				edge.Node1.IncomingEdges.Remove(edge);
				edge.Node2.OutgoingEdges.Remove(edge);
			}
			edge.Node1.OutgoingEdges.Remove(edge);
			edge.Node2.IncomingEdges.Remove(edge);
		}

		/// <summary>
		/// Checks that the integrity of the graph is intact, throws an exception otherwise
		/// </summary>
		private void CheckNetworkIntegrity()
		{
			if (CheckGraphIntegrity)
			{
				foreach (var edge in _edges)
				{
					if (!edge.IsDirected)
					{
						_ = edge.Node2.OutgoingEdges.Single(x => x == edge);
						_ = edge.Node1.IncomingEdges.Single(x => x == edge);
					}
					_ = edge.Node1.OutgoingEdges.Single(x => x == edge);
					_ = edge.Node2.IncomingEdges.Single(x => x == edge);
					if (!_nodes.Contains(edge.Node1) || !_nodes.Contains(edge.Node2))
						throw new InvalidOperationException("Node has a incoming or outgoing edge which is not part of network");
				}
				if (_nodes.Count != _nodesByData.Count)
					throw new InvalidOperationException("_nodesByData does not have same amount of entries as _nodes");
				if (_nodes.Distinct().Count() != _nodes.Count)
					throw new InvalidOperationException("Same node occurs more than once");
				foreach (var node in _nodes)
					if (!_nodesByData.ContainsKey(node.Data))
						throw new InvalidOperationException("A node in _nodes does not exist in _nodesByData");
				foreach (var kvp in _nodesByData)
				{
					if (!kvp.Value.Data.Equals(kvp.Key))
						throw new InvalidOperationException("A node is not entered in the dictionary by it's correct key");
					if (!_nodes.Contains(kvp.Value))
						throw new InvalidOperationException("A node exists in the dictionary but not in the collection");
				}
				if (_edges.Distinct().Count() != _edges.Count)
					throw new InvalidOperationException("Some edge(s) occur more than once");
				foreach (var node in _nodes)
					foreach (var edge in node.IncomingEdges.Union(node.OutgoingEdges))
						if (!_edges.Contains(edge))
							throw new InvalidOperationException("Edge is registered as incoming or outgoing from a node but is not in the graph");
			}
		}

		#endregion

		#region Public properties

		/// <summary>
		/// The nodes in the graph
		/// </summary>
		public System.Collections.Generic.IList<Node<V>> Nodes
		{
			get
			{
				return _nodes.AsReadOnly();
			}
		}

		/// <summary>
		/// The edges in the graph
		/// </summary>
		public System.Collections.Generic.IList<Edge<V>> Edges
		{
			get
			{
				return _edges.AsReadOnly();
			}
		}

		/// <summary>
		/// If set to true then graph integrity is checked every time a node or edge is added or removed.
		/// Set this to true only when debugging or testing this class, as there is a performance penalty.
		/// </summary>
		public bool CheckGraphIntegrity { get; set; } = false;

		#endregion

		#region Public methods

		/// <summary>
		/// Creates and adds a node to the graph. The node will be associated with the given object.
		/// 
		/// Only one node in the graph can be associated with a given object at a given time or else
		/// an exception will be thrown.
		/// 
		/// Returns the newly created node.
		/// </summary>
		public Node<V> AddNode(T t)
		{
			Node<V> node = new(t);

			if (!_nodesByData.TryAdd(t, node))
				throw new InvalidOperationException("There already exist a node associated with the given object");

			_nodes.Add(node);
			_setOfNodes.Add(node, 0);

			CheckNetworkIntegrity();

			return node;
		}

		/// <summary>
		/// Creates and adds an edge between the given nodes with the given weight. If given,
		/// the edge will also be associated with the given object.
		/// 
		/// The nodes must belong to the graph and have been created with AddNode beforehand or
		/// an exception will be thrown.
		/// 
		/// Returns the newly created edge.
		/// </summary>
		public Edge<V> AddEdge(Node<V> n1, Node<V> n2, V weight, bool isDirected, U associatedObject = default)
		{
			if (n1 == n2)
				throw new InvalidOperationException("Edge has same start and end node");

			if (!_setOfNodes.ContainsKey(n1) || !_setOfNodes.ContainsKey(n2))
				throw new InvalidOperationException("Graph does not contain one (or both) of the given nodes");

			Edge<V> edge = new(n1, n2, weight, isDirected, associatedObject);

			n1.OutgoingEdges.Add(edge);
			n2.IncomingEdges.Add(edge);

			if (!isDirected)
			{
				n1.IncomingEdges.Add(edge);
				n2.OutgoingEdges.Add(edge);
			}

			_edges.Add(edge);

			CheckNetworkIntegrity();

			return edge;
		}


		/// <summary>
		/// Adds and returns an edge with the given weight between the nodes. If given,
		/// the edge will also be associated with the given object.
		/// 
		/// There must exist nodes in the graph associated with the given data before
		/// calling this function or an exception will be thrown.
		/// 
		/// Returns the newly created edge.
		/// </summary>
		public Edge<V> AddEdge(T data1, T data2, V weight, bool isDirected, U associatedObject = default)
		{
			if (!_nodesByData.TryGetValue(data1, out var node1) || !_nodesByData.TryGetValue(data2, out var node2))
				throw new InvalidOperationException("Graph does not contain nodes associated with one (or both) of the given data objects");

			var result = AddEdge(node1, node2, weight, isDirected, associatedObject);

			CheckNetworkIntegrity();

			return result;
		}

		/// <summary>
		/// Removes the given node from the network.
		/// The node must be completely detached before it can be removed. If it is still
		/// connected to an edge, an exception will be thrown,
		/// </summary>
		/// <param name="node"></param>
		public void RemoveNode(Node<V> node)
		{
			if (!_nodesByData.ContainsKey(node.Data))
				throw new InvalidOperationException("The node does not belong to the network");

			if (node.OutgoingEdges.Any() || node.IncomingEdges.Any())
				throw new InvalidOperationException("The node is still connected to the network");

			_nodesByData.Remove(node.Data);
			_nodes.Remove(node);

			CheckNetworkIntegrity();
		}

		/// <summary>
		/// Removes the node associated with the given object from the network.
		/// The node must be completely detached before it can be removed. If it is still
		/// connected to an edge, an exception will be thrown,
		/// </summary>
		public void RemoveNode(T associatedObject)
		{
			RemoveNode(GetNodeFor(associatedObject));
		}

		/// <summary>
		/// Removes the edge (or all edges) which are associated with the given object.
		/// </summary>
		public void RemoveEdges(U associatedObject)
		{
			List<Edge<V>> newEdges = new();
			foreach (var edge in _edges)
			{
				if (edge.AssociatedObject.Equals(associatedObject))
					DetachEdgeFromNetwork(edge);
				else
					newEdges.Add(edge);
			}
			_edges = newEdges;

			CheckNetworkIntegrity();
		}

		/// <summary>
		/// Removes the given edge from the network
		/// </summary>
		/// <param name="edge"></param>
		public void RemoveEdge(Edge<V> edge)
		{
			_edges.Remove(edge);
			DetachEdgeFromNetwork(edge);

			CheckNetworkIntegrity();
		}

		/// <summary>
		/// Returns the node associated with the given data, or throws an exception if no such node
		/// exist in the graph.
		/// </summary>
		public Node<V> GetNodeFor(T data)
		{
			bool success = _nodesByData.TryGetValue(data, out var n);
			if (!success)
				throw new InvalidOperationException("No nodes associated with the given data");
			return n;
		}

		/// <summary>
		/// Runs Bellman-Ford algorithm on the graph with the given start. Returns true if successful
		/// or false if the graph contains cycles.
		/// </summary>
		public bool BellmanFord(T start, V startCost, bool maximise)
		{
			if (!_nodesByData.TryGetValue(start, out var startNode))
				throw new InvalidOperationException("No nodes are associated with the given start");

			return BellmanFord(startNode, startCost, maximise);
		}

		/// <summary>
		/// Runs Bellman-Ford algorithm on the graph with the given starting points. Returns true if
		/// successful or false if the graph contains cycles.
		/// </summary>
		public bool BellmanFord(List<T> startPoints, List<V> startCosts, bool maximise)
		{
			List<Node<V>> startingNodes = new();
			foreach (T point in startPoints)
			{
				if (!_nodesByData.TryGetValue(point, out var startNode))
					throw new InvalidOperationException("No node is associated with one or more of the given starting points");

				startingNodes.Add(startNode);
			}

			return BellmanFord(startingNodes, startCosts, maximise);
		}

		/// <summary>
		/// Runs Bellman-Ford algorithm on the graph with the given start node. Returns true if successful
		/// or false if the graph contains cycles.
		/// </summary>
		public bool BellmanFord(Node<V> startNode, V startCost, bool maximise)
		{
			List<Node<V>> startNodes = [startNode];
			List<V> startCosts = [startCost];

			return BellmanFord(startNodes, startCosts, maximise);
		}

		/// <summary>
		/// Runs Bellman-Ford algorithm on the graph with the given start nodes and costs. Returns true if successful
		/// or false if the graph contains cycles.
		/// </summary>
		public bool BellmanFord(List<Node<V>> startNodes, List<V> startCosts, bool maximise)
		{
			_lastAlgorithmUsedGCost = false;

			// Validate input
			if (startNodes == null || startNodes.Count == 0)
				throw new InvalidOperationException("No starting nodes given");

			if (startCosts == null)
				throw new InvalidOperationException("No start costs given");

			if (startCosts.Count != startNodes.Count)
				throw new InvalidOperationException("Different number of starting nodes and costs given");

			// Set all costs to infinity
			foreach (var n in _nodesByData.Values)
			{
				n.Cost = maximise ? V.MinValue : V.MaxValue;

				n.PreviousNode = null;
			}

			// Initialize cost on start nodes
			int i;
			for (i = 0; i < startNodes.Count; ++i)
			{
				if (!_setOfNodes.ContainsKey(startNodes[i]))
					throw new InvalidOperationException("One or more of the given start nodes does not belong to the graph");
				startNodes[i].Cost = startCosts[i];
			}

			i = 0;
			while (i++ < _nodesByData.Count)
			{
				bool changed = false;
				// Relax edges
				foreach (var edge in Edges)
				{
					if ((!maximise && edge.Node1.Cost + edge.Weight < edge.Node2.Cost) ||
							(maximise && edge.Node1.Cost + edge.Weight > edge.Node2.Cost))
					{
						edge.Node2.Cost = edge.Node1.Cost + edge.Weight;
						edge.Node2.PreviousNode = edge.Node1;
						changed = true;
					}
					if (!edge.IsDirected)
					{
						if ((!maximise && edge.Node2.Cost + edge.Weight < edge.Node1.Cost) ||
								(maximise && edge.Node2.Cost + edge.Weight > edge.Node1.Cost))
						{
							edge.Node1.Cost = edge.Node2.Cost + edge.Weight;
							edge.Node1.PreviousNode = edge.Node2;
							changed = true;
						}
					}
				}

				if (changed && i == _nodesByData.Count - 1)
					return false;

				if (!changed)
					break;
			}

			return true;
		}

		/// <summary>
		/// Runs Dijkstra's algorithm on the graph from the given starting point. Dijkstra's algorithm
		/// assume edges has positive cost.
		/// </summary>
		public void Dijkstra(T start)
		{
			if (!_nodesByData.TryGetValue(start, out var startNode))
				throw new InvalidOperationException("No nodes are associated with the given start");

			List<Node<V>> startNodes = [startNode];
			List<V> startCosts = [V.Zero];

			Dijkstra(startNodes, startCosts);
		}

		/// <summary>
		/// Runs Dijkstra's algorithm on the graph from the given starting point. Dijkstra's algorithm
		/// assume edges has positive cost. This version will terminate as soon as the shortest path
		/// to the given destination is found. Use this for speed when you only want the shortest path
		/// to a single point.
		/// </summary>
		public void Dijkstra(T start, T destination)
		{
			if (!_nodesByData.TryGetValue(start, out var startNode))
				throw new InvalidOperationException("No nodes are associated with the given start");

			if (!_nodesByData.TryGetValue(destination, out var destinationNode))
				throw new InvalidOperationException("No nodes are associated with the given destination");

			List<Node<V>> startNodes = [startNode];
			List<V> startCosts = [V.Zero];

			Dijkstra(startNodes, startCosts, destinationNode);
		}

		/// <summary>
		/// Runs Dijkstra's algorithm on the graph from the given starting points with the give associated 
		/// starting costs. Dijkstra's algorithm assume edges has positive cost. If the destination node is
		/// not null, the search will terminate as soon as the shortest path to the destination is found.
		/// In this case the algorithm does not guarantee that the shortest path to all nodes are found.
		/// </summary>
		public void Dijkstra(List<Node<V>> startNodes, List<V> startCosts, Node<V> destination = null)
		{
			Dijkstra(startNodes, startCosts, destination == null ? null : [destination]);
		}

		/// <summary>
		/// Runs Dijkstra's algorithm on the graph from the given starting points with the give associated 
		/// starting costs. Dijkstra's algorithm assume edges has positive cost. If the destination node is
		/// not null, the search will terminate as soon as the shortest path to all the destinations are found.
		/// In this case the algorithm does not guarantee that the shortest path to all nodes are found,
		/// only to the destination nodes.
		/// </summary>
		public void Dijkstra(List<Node<V>> startNodes, List<V> startCosts, List<Node<V>> destinations)
		{
			_lastAlgorithmUsedGCost = false;

			// Validate input
			if (startNodes == null || startNodes.Count == 0)
				throw new InvalidOperationException("No starting nodes given");

			if (startCosts == null)
				throw new InvalidOperationException("No start costs given");

			if (startCosts.Count != startNodes.Count)
				throw new InvalidOperationException("Different number of starting nodes and costs given");

			// Heap storing the unvisited nodes
			IntervalHeap<Node<V>> unvisitedNodes = new(Nodes.Count);

			// Set all costs to infinity
			foreach (var n in _nodesByData.Values)
			{
				n.Cost = V.MaxValue;
				n.PreviousNode = null;
				n.ClosedSet = true;
			}

			// Initialize cost on start nodes
			int i;
			for (i = 0; i < startNodes.Count; ++i)
			{
				var startNode = startNodes[i];
				if (!_setOfNodes.ContainsKey(startNode))
					throw new InvalidOperationException("One or more of the given start nodes does not belong to the graph");
				startNode.Cost = startCosts[i];

				IPriorityQueueHandle<Node<V>> handle = null;
				unvisitedNodes.Add(ref handle, startNode);
				startNode.Handle = handle;
				startNode.ClosedSet = false;
			}

			while (!unvisitedNodes.IsEmpty)
			{
				// Fetch the unvisited node with the lowest cost
				var n = unvisitedNodes.DeleteMin();
				n.Handle = null;

				// Terminate early if shortest path to destination found
				if (destinations != null && destinations.Contains(n))
				{
					if (destinations.Count == 1)
						return;
					destinations.Remove(n);
				}

				// Update costs on all nodes reachable from this one
				foreach (var e in n.OutgoingEdges)
				{
					var opposite = e.OppositeNode(n);
					if (opposite.ClosedSet || n.Cost + e.Weight < opposite.Cost)
					{
						if (!opposite.ClosedSet && opposite.Handle == null)
							throw new InvalidOperationException("Error, cycle detected in Dijkstra's algorithm");
						opposite.Cost = n.Cost + e.Weight;
						opposite.PreviousNode = n;
						if (opposite.ClosedSet)
						{
							IPriorityQueueHandle<Node<V>> handle = null;
							unvisitedNodes.Add(ref handle, opposite);
							opposite.Handle = handle;
							opposite.ClosedSet = false;
						}
						else
							unvisitedNodes.Replace(opposite.Handle, opposite);
					}
				}
			}
		}

		/// <summary>
		/// Runs the A* algorithm from the given starting point to the given destination, using the given
		/// heuristic function. A* algorithm assumes positive edge costs.
		/// </summary>
		public void AStar(T start, T destination, Func<T, T, V> heuristic)
		{

			if (!_nodesByData.TryGetValue(start, out var startNode))
				throw new InvalidOperationException("No node in the graph is associated with the given start");

			if (!_nodesByData.TryGetValue(destination, out var destinationNode))
				throw new InvalidOperationException("No node in the graph is associated with the given destination");

			AStar(startNode, destinationNode, heuristic);
		}

		/// <summary>
		/// Runs the A* algorithm from the given starting node to the given destination node, using the 
		/// given heuristic function. A* algorithm assumes positive edge costs.
		/// </summary>
		public void AStar(Node<V> start, Node<V> destination, Func<T, T, V> heuristic)
		{
			_lastAlgorithmUsedGCost = true;

			// Heap storing the unvisited nodes
			IntervalHeap<Node<V>> openSet = new(Nodes.Count);

			// Set all costs to infinity
			foreach (var n in _nodesByData.Values)
			{
				n.Cost = V.MaxValue;
				n.PreviousNode = null;
				n.Handle = null;
				n.ClosedSet = false;
			}

			// Initialize cost on start nodes
			if (!_setOfNodes.ContainsKey(start))
				throw new InvalidOperationException("Start node does not belong to the graph");
			start.Cost = heuristic(start.Data, destination.Data);
			start.GCost = V.Zero;
			IPriorityQueueHandle<Node<V>> handle = null;
			openSet.Add(ref handle, start);
			start.Handle = handle;

			while (!openSet.IsEmpty)
			{
				// Fetch the unvisited node with the lowest cost
				var n = openSet.DeleteMin();
				n.Handle = null;
				if (n == destination)
					return;
				n.ClosedSet = true;

				// Update costs on all nodes reachable from this one
				foreach (var e in n.OutgoingEdges)
				{
					var opposite = e.OppositeNode(n);
					V tentativeGCost = n.GCost + e.Weight;
					if (opposite.ClosedSet)
						if (tentativeGCost >= opposite.GCost)
							continue;
					if (opposite.Handle == null || tentativeGCost < opposite.GCost)
					{
						opposite.GCost = tentativeGCost;
						opposite.Cost = opposite.GCost + heuristic(opposite.Data, destination.Data);
						opposite.PreviousNode = n;
						if (opposite.Handle != null)
							openSet.Replace(opposite.Handle, opposite);
						else
						{
							handle = null;
							openSet.Add(ref handle, opposite);
							opposite.Handle = handle;
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns the minimal cost to the given destination
		/// </summary>
		public V GetNodeCost(T destination)
		{
			bool success = _nodesByData.TryGetValue(destination, out var destinationNode);
			if (!success)
				throw new InvalidOperationException("Graph contains no node associated with the given destination");

			if (_lastAlgorithmUsedGCost)
				return destinationNode.GCost;
			else
				return destinationNode.Cost;
		}

		/// <summary>
		/// Returns the node path to the given destination based on the last run of the shortest
		/// path algorithm.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="nodePath"></param>
		public void GetNodePath(T destination, out List<T> nodePath)
		{
			nodePath = new List<T>();

			bool success = _nodesByData.TryGetValue(destination, out var current);
			if (!success)
				throw new InvalidOperationException("Graph contains no node associated with the given destination");

			do
			{
				nodePath.Add(current.Data);
				current = current.PreviousNode;
			} while (current != null);

			nodePath.Reverse();
		}

		/// <summary>
		/// Returns the node path and costs to the given destination based on the last run of the shortest
		/// path algorithm.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="nodePath"></param>
		/// <param name="nodeCosts"></param>
		public void GetNodePath(T destination, out List<T> nodePath, out List<V> nodeCosts)
		{
			nodePath = [];
			nodeCosts = [];

			bool success = _nodesByData.TryGetValue(destination, out var current);
			if (!success)
				throw new InvalidOperationException("Graph contains no node associated with the given destination");

			do
			{
				nodePath.Add(current.Data);
				nodeCosts.Add(_lastAlgorithmUsedGCost ? current.GCost : current.Cost);
				current = current.PreviousNode;
			} while (current != null);

			nodePath.Reverse();
			nodeCosts.Reverse();
		}

		/// <summary>
		/// Returns both the node and segment paths to the given destination based on
		/// the last run the shortest path algorithm.
		/// </summary>
		public void GetNodeAndSegmentPath(T destination, out List<T> nodePath, out List<U> segmentPath)
		{
			nodePath = [];
			segmentPath = [];

			bool success = _nodesByData.TryGetValue(destination, out var current);
			if (!success)
				throw new InvalidOperationException("Graph contains no node associated with the given destination");

			do
			{
				nodePath.Add(current.Data);
				if (current.PreviousNode != null)
				{
					segmentPath.Add(current.PreviousNode.GetEdgeLeadingToo(current).AssociatedObject);
				}
				current = current.PreviousNode;
			} while (current != null);

			nodePath.Reverse();
			segmentPath.Reverse();
		}

		/// <summary>
		/// Returns both the node and segment paths and the node costs to the given destination based on
		/// the last run of the shortest path algorithm.
		/// </summary>
		public void GetNodeAndSegmentPath(T destination, out List<T> nodePath, out List<V> nodeCosts, out List<U> segmentPath)
		{
			nodePath = [];
			nodeCosts = [];
			segmentPath = [];

			bool success = _nodesByData.TryGetValue(destination, out var current);
			if (!success)
				throw new InvalidOperationException("Graph contains no node associated with the given destination");

			do
			{
				nodePath.Add(current.Data);
				nodeCosts.Add(_lastAlgorithmUsedGCost ? current.GCost : current.Cost);
				if (current.PreviousNode != null)
				{
					segmentPath.Add(current.PreviousNode.GetEdgeLeadingToo(current).AssociatedObject);
				}
				current = current.PreviousNode;
			} while (current != null);

			nodePath.Reverse();
			segmentPath.Reverse();
			nodeCosts.Reverse();
		}

		#endregion
	}

	/// <summary>
	/// Shortcut for <see cref="ShortestPathCalculator{T, U, V}"/> using double for cost. This is provided for
	/// compatibility purposes.
	/// </summary>
	public class ShortestPathCalculator<T, U> : ShortestPathCalculator<T, U, double>
	{}

	/// <summary>
	/// Provides functionality to set up graphs and algorithms to calculate the shortest paths on these graphs.
	/// 
	/// In the graph, nodes are associated with an instance of <typeparamref name="T"/>. Only one node in the graph 
	/// can be associated with this instance.
	/// 
	/// The edges can be both directed or undirected, and can optionally be associated with instances
	/// of <typeparamref name="U"/>. In this implementation the distance of an edge is a double.
	/// 
	/// This is a version of the graph class where the cost of segments are also a generic <typeparamref name="V"/>.
	/// The user must specify Min / Max values of this type and functions for doing the operations on this type
	/// required by the graph algorithms. This allows more complex shortest path calculations where a more general
	/// state of travel can be calculated for each node.
	/// </summary>
	/// <typeparam name="T">A class or value type associated with the nodes in the graph</typeparam>
	/// <typeparam name="U">A class optionally associated with the edges in the graph</typeparam>
	/// <typeparam name="V">A class describing the cost of travel, typically a numeric like int / float
	/// / double but one can also use a class if desired</typeparam>
	public class GenericShortestPathCalculator<T, U, V>
	{

		#region Private members

		/// <summary>
		/// The nodes in the graph
		/// </summary>
		private readonly List<Node> _nodes = new();

		/// <summary>
		/// The edges in the graph
		/// </summary>
		private List<Edge> _edges = new();

		/// <summary>
		/// The nodes in the graph by their associated data
		/// </summary>
		private readonly Dictionary<T, Node> _nodesByData = new();

		/// <summary>
		/// The set of nodes, for quick lookup
		/// </summary>
		private readonly Dictionary<Node, int> _setOfNodes = new();

		private bool _lastAlgorithmUsedGCost;

		/// <summary>
		/// The last algorithm that was run.
		/// </summary>
		private Algorithm _lastAlgorithmRun = Algorithm.None;

		#endregion

		#region Nested classes / Enums

		/// <summary>
		/// Enumerates the algorithms
		/// </summary>
		private enum Algorithm
		{
			None,
			BellmanFord,
			Dijkstra,
			AStar
		};

		/// <summary>
		/// A node in the graph. A node contains information about which object it is
		/// associated with, which edges lead to or from this node, and results from
		/// the shortest path calculation in the form of cost and previous node / segment.
		/// </summary>
		public class Node : IComparable<Node>
		{
			/// <summary>
			/// The color of the node, used for extracting cycles.
			/// </summary>
			public enum Color
			{
				/// <summary>
				/// Black
				/// </summary>
				Black,
				/// <summary>
				/// Grey
				/// </summary>
				Grey,
				/// <summary>
				/// White
				/// </summary>
				White
			}

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="t">An object which will be associated with this Node</param>
			/// <param name="graph">The graph this node is part of</param>
			public Node(T t, GenericShortestPathCalculator<T, U, V> graph)
			{
				Graph = graph;
				Data = t;
				Cost = Graph.MaxCost;
				OutgoingEdges = new List<Edge>();
				IncomingEdges = new List<Edge>();
				Graph = graph;
				NodeColor = Color.White;
			}

			/// <summary>
			/// The node color, used when extracting cycles.
			/// </summary>
			public Color NodeColor { get; set; }

			/// <summary>
			/// The graph this node is a part of
			/// </summary>
			public GenericShortestPathCalculator<T, U, V> Graph { get; private set; }

			/// <summary>
			/// The data associated with this node
			/// </summary>
			public T Data
			{
				get;
				private set;
			}

			/// <summary>
			/// The edges leading out of this node
			/// </summary>
			public List<Edge> OutgoingEdges
			{
				get;
				private set;
			}

			/// <summary>
			/// The edges leading to this node
			/// </summary>
			public List<Edge> IncomingEdges
			{
				get;
				private set;
			}

			/// <summary>
			/// The cheapest cost for reaching this node
			/// 
			/// This property will not be set until you run an actual shortest path algorithm on the graph.
			/// 
			/// When using A* algorithm this is the heuristic cost for this node, GCost will hold the actual
			/// cheapest cost.
			/// </summary>
			public V Cost
			{
				get;
				set;
			}

			/// <summary>
			/// Used by A* algorithm only. When using A*, Cost is the heuristic cost of a node in the
			/// open set and GCost is the cheapest cost for reaching this node.
			/// </summary>
			public V GCost
			{
				get;
				set;
			}

			/// <summary>
			/// The previous node in the cheapest path to this node, or null if this is the start node.
			/// 
			/// This property will not be set until you run an actual shortest path algorithm on the graph,
			/// </summary>
			public Node PreviousNode
			{
				get;
				set;
			}

			/// <summary>
			/// Handle to this node in C5 while running algorithms using C5.
			/// 
			/// Only for use by the algorithms.
			/// </summary>
			public IPriorityQueueHandle<Node> Handle { get; set; }

			/// <summary>
			/// Set to true by A* if this node is part of the closed set, false otherwise.
			/// </summary>
			public bool ClosedSet
			{
				get;
				set;
			}

			/// <summary>
			/// Comparer so C5 can compare nodes with each other
			/// </summary>
			public int CompareTo(Node other)
			{
				return Graph.CostComparer(Cost, other.Cost);
			}

			/// <summary>
			/// Returns the edge leading from this node to the given node or throws an exception
			/// if no such edge exists.
			/// </summary>
			public Edge GetEdgeLeadingToo(Node n)
			{
				foreach (Edge edge in OutgoingEdges)
					if (edge.OppositeNode(this) == n)
						return edge;
				throw new InvalidOperationException("No edges lead to the given segment");
			}
		}

		/// <summary>
		/// An edge in the graph
		/// </summary>
		public class Edge
		{
			/// <summary>
			/// Constructor.
			/// </summary>
			public Edge(Node n1, Node n2, V weight, bool isDirected, U associatedObject)
			{
				IsDirected = isDirected;
				Node1 = n1;
				Node2 = n2;
				Weight = weight;
				AssociatedObject = associatedObject;
				if (Node1 == Node2)
					throw new InvalidOperationException("Edge has same start and end node");
			}

			/// <summary>
			/// If true this edge can only be traversed from Node1 to Node2
			/// </summary>
			public bool IsDirected
			{
				get;
				private set;
			}

			/// <summary>
			/// The first node this edge is connected to
			/// </summary>
			public Node Node1
			{
				get;
				private set;
			}

			/// <summary>
			/// The second node this edge is connected to
			/// </summary>
			public Node Node2
			{
				get;
				private set;
			}

			/// <summary>
			/// The edge of this weight. The graph itself has no requirements on
			/// the weight of an edge but do note that some algorithms may require
			/// a graph without negative cost cycles and maybe even positive weights.
			/// </summary>
			public V Weight
			{
				get;
				set;
			}

			/// <summary>
			/// Returns the other node associated with this edge
			/// </summary>
			public Node OppositeNode(Node n)
			{
				if (n == Node1)
					return Node2;
				else if (n == Node2)
					return Node1;
				else
					throw new InvalidOperationException("The given node is not connected to this edge");
			}

			/// <summary>
			/// The object of type U associated with this edge, or null if no objects
			/// are associated with this edge.
			/// </summary>
			public U AssociatedObject
			{
				get;
				private set;
			}
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Constructs an instance of this class.
		/// </summary>
		/// <param name="minCost">The minimum cost for the value type V.</param>
		/// <param name="maxCost">The maximum cost for the value type V.</param>
		/// <param name="costUpdater">This function takes the cost before traversing as first argument, the edge
		/// to traverse as second argument and returns the new cost after traversing the edge.</param>
		/// <param name="costComparer">This function takes two costs as arguments and returns a negative
		/// value if the first cost is smaller, 0 if they are equal and a positive number if the second
		/// cost is larger.</param>
		public GenericShortestPathCalculator(V minCost, V maxCost, Func<V, Edge, V> costUpdater, Func<V, V, int> costComparer)
		{
			CostAfterTraversalFunc = costUpdater;
			CostComparer = costComparer;
			MinCost = minCost;
			MaxCost = maxCost;
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Detaches the given edge from the network by removing it from the relevant nodes list
		/// of incoming and outgoing edges.
		/// </summary>
		/// <param name="edge"></param>
		private static void DetachEdgeFromNetwork(Edge edge)
		{
			if (!edge.IsDirected)
			{
				edge.Node1.IncomingEdges.Remove(edge);
				edge.Node2.OutgoingEdges.Remove(edge);
			}
			edge.Node1.OutgoingEdges.Remove(edge);
			edge.Node2.IncomingEdges.Remove(edge);
		}

		/// <summary>
		/// Checks that the integrity of the graph is intact after network has been altered, throws
		/// an exception otherwise.
		/// </summary>
		private void CheckNetworkIntegrity()
		{
			_lastAlgorithmRun = Algorithm.None;

			if (CheckGraphIntegrity)
			{
				foreach (var edge in _edges)
				{
					if (!edge.IsDirected)
					{
						_ = edge.Node2.OutgoingEdges.Single(x => x == edge);
						_ = edge.Node1.IncomingEdges.Single(x => x == edge);
					}
					_ = edge.Node1.OutgoingEdges.Single(x => x == edge);
					_ = edge.Node2.IncomingEdges.Single(x => x == edge);
					if (!_nodes.Contains(edge.Node1) || !_nodes.Contains(edge.Node2))
						throw new InvalidOperationException("Node has a incoming or outgoing edge which is not part of network");
				}
				if (_nodes.Count != _nodesByData.Count)
					throw new InvalidOperationException("_nodesByData does not have same amount of entries as _nodes");
				if (_nodes.Distinct().Count() != _nodes.Count)
					throw new InvalidOperationException("Same node occurs more than once");
				foreach (var node in _nodes)
					if (!_nodesByData.ContainsKey(node.Data))
						throw new InvalidOperationException("A node in _nodes does not exist in _nodesByData");
				foreach (var kvp in _nodesByData)
				{
					if (!kvp.Value.Data.Equals(kvp.Key))
						throw new InvalidOperationException("A node is not entered in the dictionary by it's correct key");
					if (!_nodes.Contains(kvp.Value))
						throw new InvalidOperationException("A node exists in the dictionary but not in the collection");
				}
				if (_edges.Distinct().Count() != _edges.Count)
					throw new InvalidOperationException("Some edge(s) occur more than once");
				foreach (var node in _nodes)
					foreach (var edge in node.IncomingEdges.Union(node.OutgoingEdges))
						if (!_edges.Contains(edge))
							throw new InvalidOperationException("Edge is registered as incoming or outgoing from a node but is not in the graph");
			}
		}

		#endregion

		#region Public properties

		/// <summary>
		/// The nodes in the graph
		/// </summary>
		public System.Collections.Generic.IList<Node> Nodes
		{
			get
			{
				return _nodes.AsReadOnly();
			}
		}

		/// <summary>
		/// The edges in the graph
		/// </summary>
		public System.Collections.Generic.IList<Edge> Edges
		{
			get
			{
				return _edges.AsReadOnly();
			}
		}

		/// <summary>
		/// Function used to calculate cost at a node after traversing an edge.
		/// 
		/// This function takes the cost before traversing as first argument, the edge to traverse as second
		/// argument and returns the new cost after traversing the edge.
		/// </summary>
		public Func<V, Edge, V> CostAfterTraversalFunc { get; set; }

		/// <summary>
		/// Function for comparing 2 costs. This function is expected to return negative int if first is smaller
		/// than second, 0 if they are equal and positive int if the first is larger
		/// </summary>
		public Func<V, V, int> CostComparer { get; private set; }

		/// <summary>
		/// The minimum cost for the value type V
		/// </summary>
		public V MinCost { get; private set; }

		/// <summary>
		/// The maximum cost for the value type V
		/// </summary>
		public V MaxCost { get; private set; }

		/// <summary>
		/// If set to true then graph integrity is checked every time a node or edge is added or removed.
		/// Set this to true only when debugging or testing this class, as there is a performance penalty.
		/// </summary>
		public bool CheckGraphIntegrity { get; set; } = false;

		#endregion

		#region Public methods

		/// <summary>
		/// Creates and adds a node to the graph. The node will be associated with the given object.
		/// 
		/// Only one node in the graph can be associated with a given object at a given time or else
		/// an exception will be thrown.
		/// 
		/// Returns the newly created node.
		/// </summary>
		public Node AddNode(T t)
		{
			if (_nodesByData.ContainsKey(t))
				throw new InvalidOperationException("There already exist a node associated with the given object");

			Node node = new(t, this);
			_nodesByData[t] = node;
			_nodes.Add(node);
			_setOfNodes.Add(node, 0);

			CheckNetworkIntegrity();

			return node;
		}

		/// <summary>
		/// Creates and adds an edge between the given nodes with the given weight. If given,
		/// the edge will also be associated with the given object.
		/// 
		/// The nodes must belong to the graph and have been created with AddNode beforehand or
		/// an exception will be thrown.
		/// 
		/// Returns the newly created edge.
		/// </summary>
		public Edge AddEdge(Node n1, Node n2, V weight, bool isDirected, U associatedObject = default)
		{
			if (n1 == n2)
				throw new InvalidOperationException("Edge has same start and end node");

			if (!_setOfNodes.ContainsKey(n1) || !_setOfNodes.ContainsKey(n2))
				throw new InvalidOperationException("Graph does not contain one (or both) of the given nodes");

			Edge edge = new(n1, n2, weight, isDirected, associatedObject);

			n1.OutgoingEdges.Add(edge);
			n2.IncomingEdges.Add(edge);

			if (!isDirected)
			{
				n1.IncomingEdges.Add(edge);
				n2.OutgoingEdges.Add(edge);
			}

			_edges.Add(edge);

			CheckNetworkIntegrity();

			return edge;
		}

		/// <summary>
		/// Adds and returns an edge with the given weight between the nodes. If given,
		/// the edge will also be associated with the given object.
		/// 
		/// There must exist nodes in the graph associated with the given data before
		/// calling this function or an exception will be thrown.
		/// 
		/// Returns the newly created edge.
		/// </summary>
		public Edge AddEdge(T data1, T data2, V weight, bool isDirected, U associatedObject = default)
		{
			if (!_nodesByData.TryGetValue(data1, out Node node1) || !_nodesByData.TryGetValue(data2, out Node node2))
				throw new InvalidOperationException("Graph does not contain nodes associated with one (or both) of the given data objects");

			var result = AddEdge(node1, node2, weight, isDirected, associatedObject);

			CheckNetworkIntegrity();

			return result;
		}

		/// <summary>
		/// Removes the given node from the network.
		/// The node must be completely detached before it can be removed. If it is still
		/// connected to an edge, an exception will be thrown,
		/// </summary>
		/// <param name="node"></param>
		public void RemoveNode(Node node)
		{
			if (!_nodesByData.ContainsKey(node.Data))
				throw new InvalidOperationException("The node does not belong to the network");

			if (node.OutgoingEdges.Any() || node.IncomingEdges.Any())
				throw new InvalidOperationException("The node is still connected to the network");

			_nodesByData.Remove(node.Data);
			_nodes.Remove(node);

			CheckNetworkIntegrity();
		}

		/// <summary>
		/// Removes the node associated with the given object from the network.
		/// The node must be completely detached before it can be removed. If it is still
		/// connected to an edge, an exception will be thrown,
		/// </summary>
		public void RemoveNode(T associatedObject)
		{
			RemoveNode(GetNodeFor(associatedObject));
		}

		/// <summary>
		/// Removes the edge (or all edges) which are associated with the given object.
		/// </summary>
		public void RemoveEdges(U associatedObject)
		{
			List<Edge> newEdges = new();
			foreach (var edge in _edges)
			{
				if (edge.AssociatedObject.Equals(associatedObject))
					DetachEdgeFromNetwork(edge);
				else
					newEdges.Add(edge);
			}
			_edges = newEdges;

			CheckNetworkIntegrity();
		}

		/// <summary>
		/// Removes the given edge from the network
		/// </summary>
		/// <param name="edge"></param>
		public void RemoveEdge(Edge edge)
		{
			_edges.Remove(edge);
			DetachEdgeFromNetwork(edge);

			CheckNetworkIntegrity();
		}

		/// <summary>
		/// Returns true if the graph has a node associated with the given data, false if not.
		/// </summary>
		public bool HasNodeFor(T data)
		{
			return _nodesByData.ContainsKey(data);
		}

		/// <summary>
		/// Returns the node associated with the given data, or throws an exception if no such node
		/// exist in the graph.
		/// </summary>
		public Node GetNodeFor(T data)
		{
			if (_nodesByData.TryGetValue(data, out Node n))
				return n;

			throw new InvalidOperationException("No nodes associated with the given data");
		}

		/// <summary>
		/// Runs Bellman-Ford algorithm on the graph with the given start. Returns true if successful
		/// or false if the graph contains cycles.
		/// </summary>
		public bool BellmanFord(T start, V startCost, bool maximise)
		{
			if (!_nodesByData.TryGetValue(start, out Node startNode))
				throw new InvalidOperationException("No nodes are associated with the given start");

			return BellmanFord(startNode, startCost, maximise);
		}

		/// <summary>
		/// Runs Bellman-Ford algorithm on the graph with the given starting points. Returns true if
		/// successful or false if the graph contains cycles.
		/// </summary>
		public bool BellmanFord(List<T> startPoints, List<V> startCosts, bool maximise)
		{
			List<Node> startingNodes = new();
			foreach (T point in startPoints)
			{
				if (!_nodesByData.TryGetValue(point, out Node startNode))
					throw new InvalidOperationException("No node is associated with one or more of the given starting points");

				startingNodes.Add(startNode);
			}

			return BellmanFord(startingNodes, startCosts, maximise);
		}

		/// <summary>
		/// Runs Bellman-Ford algorithm on the graph with the given start node. Returns true if successful
		/// or false if the graph contains cycles.
		/// </summary>
		public bool BellmanFord(Node startNode, V startCost, bool maximise)
		{
			List<Node> startNodes = new();
			List<V> startCosts = new();

			startNodes.Add(startNode);
			startCosts.Add(startCost);

			return BellmanFord(startNodes, startCosts, maximise);
		}

		/// <summary>
		/// Runs Bellman-Ford algorithm on the graph with the given start nodes and costs. Returns true if successful
		/// or false if the graph contains cycles.
		/// </summary>
		public bool BellmanFord(List<Node> startNodes, List<V> startCosts, bool maximise)
		{
			_lastAlgorithmUsedGCost = false;
			_lastAlgorithmRun = Algorithm.BellmanFord;

			// Validate input
			if (startNodes == null || startNodes.Count == 0)
				throw new InvalidOperationException("No starting nodes given");

			if (startCosts == null)
				throw new InvalidOperationException("No start costs given");

			if (startCosts.Count != startNodes.Count)
				throw new InvalidOperationException("Different number of starting nodes and costs given");

			// Set all costs to infinity
			foreach (Node n in _nodesByData.Values)
			{
				n.Cost = maximise ? MinCost : MaxCost;

				n.PreviousNode = null;
				n.NodeColor = Node.Color.White;
			}

			// Initialize cost on start nodes
			int i;
			for (i = 0; i < startNodes.Count; ++i)
			{
				if (!_setOfNodes.ContainsKey(startNodes[i]))
					throw new InvalidOperationException("One or more of the given start nodes does not belong to the graph");
				startNodes[i].Cost = startCosts[i];
			}

			i = 0;
			while (i++ < _nodesByData.Count)
			{
				bool changed = false;
				// Relax edges
				foreach (Edge edge in Edges)
				{
					V costAfter = CostAfterTraversalFunc(edge.Node1.Cost, edge);
					if ((!maximise && CostComparer(costAfter, edge.Node2.Cost) < 0) ||
							(maximise && CostComparer(costAfter, edge.Node2.Cost) > 0))
					{
						edge.Node2.Cost = costAfter;
						edge.Node2.PreviousNode = edge.Node1;
						changed = true;
					}
					if (!edge.IsDirected)
					{
						costAfter = CostAfterTraversalFunc(edge.Node2.Cost, edge);
						if ((!maximise && CostComparer(costAfter, edge.Node1.Cost) < 0) ||
								(maximise && CostComparer(costAfter, edge.Node1.Cost) > 0))
						{
							edge.Node1.Cost = costAfter;
							edge.Node1.PreviousNode = edge.Node2;
							changed = true;
						}
					}
				}

				if (changed && i == _nodesByData.Count - 1)
					break;

				if (!changed)
					return true;
			}

			// Detect cycles
			foreach (var edge in Edges)
			{
				V valueForward = CostAfterTraversalFunc(edge.Node1.Cost, edge);
				if (CostComparer(valueForward, edge.Node2.Cost) < 0)
				{
					return false;
				}
				V valueBackward = CostAfterTraversalFunc(edge.Node2.Cost, edge);
				if (CostComparer(valueBackward, edge.Node1.Cost) < 0)
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Returns a cycle in the graph (in the form of a List<typeparamref name="T"/>).
		/// If no cycles are found null is returned.
		/// This function can only be called directly after running BellmanFord. If the network
		/// has been changed or another algorithm has been run this function will cast an 
		/// exception.
		/// </summary>
		public List<T> ExtractCycle()
		{
			if (_lastAlgorithmRun != Algorithm.BellmanFord)
			{
				throw new InvalidOperationException("Cycle extraction can only be called directly after performing BellmanFord");
			}

			foreach (var node in Nodes)
			{
				if (node.NodeColor == Node.Color.White)
				{
					List<Node> currentNodes = new();
					var currentNode = node;
					while (currentNode.PreviousNode is { NodeColor: Node.Color.White })
					{
						currentNode.NodeColor = Node.Color.Grey;
						currentNodes.Add(currentNode);
						currentNode = currentNode.PreviousNode;
					}

					// Report the cycle if detected
					if (currentNode.PreviousNode is { NodeColor: Node.Color.Grey })
					{
						currentNodes.Add(currentNode);
						int cycleStartIndex = currentNodes.IndexOf(currentNode.PreviousNode);
						if (cycleStartIndex < 0)
						{
							throw new InvalidOperationException("Cycle detection failed");
						}

						List<T> cycle = new();
						for (int i = cycleStartIndex; i < currentNodes.Count; ++i)
						{
							cycle.Add(currentNodes[i].Data);
						}

						return cycle;
					}
					// Black out visited stuff
					foreach (var n in currentNodes)
					{
						n.NodeColor = Node.Color.Black;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Runs Dijkstra's algorithm on the graph from the given starting point. Dijkstra's algorithm
		/// assume edges has positive cost.
		/// </summary>
		public void Dijkstra(T start)
		{
			if (!_nodesByData.TryGetValue(start, out Node startNode))
				throw new InvalidOperationException("No nodes are associated with the given start");

			List<Node> startNodes = new();
			List<V> startCosts = new();

			startNodes.Add(startNode);
			startCosts.Add(default);

			Dijkstra(startNodes, startCosts);
		}

		/// <summary>
		/// Runs Dijkstra's algorithm on the graph from the given starting point. Dijkstra's algorithm
		/// assume edges has positive cost. This version will terminate as soon as the shortest path
		/// to the given destination is found. Use this for speed when you only want the shortest path
		/// to a single point.
		/// </summary>
		public void Dijkstra(T start, T destination)
		{
			if (!_nodesByData.TryGetValue(start, out Node startNode))
				throw new InvalidOperationException("No nodes are associated with the given start");

			if (!_nodesByData.TryGetValue(destination, out Node destinationNode))
				throw new InvalidOperationException("No nodes are associated with the given destination");

			List<Node> startNodes = new();
			List<V> startCosts = new();

			startNodes.Add(startNode);
			startCosts.Add(default);

			Dijkstra(startNodes, startCosts, destinationNode);
		}

		/// <summary>
		/// Runs Dijkstra's algorithm on the graph from the given starting points with the give associated 
		/// starting costs. Dijkstra's algorithm assume edges has positive cost. If the destination node is
		/// not null, the search will terminate as soon as the shortest path to the destination is found.
		/// In this case the algorithm does not guarantee that the shortest path to all nodes are found.
		/// </summary>
		public void Dijkstra(List<Node> startNodes, List<V> startCosts, Node destination = null)
		{
			List<Node> destinations = null;
			if (destination != null)
			{
				destinations = [destination];
			}
			Dijkstra(startNodes, startCosts, destinations);
		}

		/// <summary>
		/// Runs Dijkstra's algorithm on the graph from the given starting points with the give associated 
		/// starting costs. Dijkstra's algorithm assume edges has positive cost. If the destination node is
		/// not null, the search will terminate as soon as the shortest path to all the destinations are found.
		/// In this case the algorithm does not guarantee that the shortest path to all nodes are found,
		/// only to the destination nodes.
		/// </summary>
		public void Dijkstra(List<Node> startNodes, List<V> startCosts, List<Node> destinations)
		{
			_lastAlgorithmUsedGCost = false;
			_lastAlgorithmRun = Algorithm.Dijkstra;

			// Validate input
			if (startNodes == null || startNodes.Count == 0)
				throw new InvalidOperationException("No starting nodes given");

			if (startCosts == null)
				throw new InvalidOperationException("No start costs given");

			if (startCosts.Count != startNodes.Count)
				throw new InvalidOperationException("Different number of starting nodes and costs given");

			// Heap storing the unvisited nodes
			IntervalHeap<Node> unvisitedNodes = new(Nodes.Count);

			// Set all costs to infinity
			foreach (Node n in _nodesByData.Values)
			{
				n.Cost = MaxCost;
				n.PreviousNode = null;
				n.ClosedSet = true;
			}

			// Initialize cost on start nodes
			int i;
			for (i = 0; i < startNodes.Count; ++i)
			{
				Node startNode = startNodes[i];
				if (!_setOfNodes.ContainsKey(startNode))
					throw new InvalidOperationException("One or more of the given start nodes does not belong to the graph");
				startNode.Cost = startCosts[i];

				IPriorityQueueHandle<Node> handle = null;
				unvisitedNodes.Add(ref handle, startNode);
				startNode.Handle = handle;
				startNode.ClosedSet = false;
			}

			while (!unvisitedNodes.IsEmpty)
			{
				// Fetch the unvisited node with the lowest cost
				Node n = unvisitedNodes.DeleteMin();
				n.Handle = null;

				// Terminate early if shortest path to destination found
				if (destinations != null && destinations.Contains(n))
				{
					if (destinations.Count == 1)
						return;
					destinations.Remove(n);
				}

				// Update costs on all nodes reachable from this one
				foreach (Edge e in n.OutgoingEdges)
				{
					Node opposite = e.OppositeNode(n);
					V costAfter = CostAfterTraversalFunc(n.Cost, e);
					if (opposite.ClosedSet || CostComparer(costAfter, opposite.Cost) < 0)
					{
						if (!opposite.ClosedSet && opposite.Handle == null)
							throw new InvalidOperationException("Error, cycle detected in Dijkstra's algorithm");
						opposite.Cost = costAfter;
						opposite.PreviousNode = n;
						if (opposite.ClosedSet)
						{
							IPriorityQueueHandle<Node> handle = null;
							unvisitedNodes.Add(ref handle, opposite);
							opposite.Handle = handle;
							opposite.ClosedSet = false;
						}
						else
							unvisitedNodes.Replace(opposite.Handle, opposite);
					}
				}
			}
		}


		/// <summary>
		/// Runs the A* algorithm from the given starting point to the given destination, using the 
		/// given heuristic function which are expected to add the heuristic to its given cost of type
		/// V. A* algorithm assumes positive edge costs.
		/// </summary>
		public void AStar(T start, T destination, Func<T, T, V, V> addHeuristic)
		{

			if (!_nodesByData.TryGetValue(start, out Node startNode))
				throw new InvalidOperationException("No node in the graph is associated with the given start");

			if (!_nodesByData.TryGetValue(destination, out Node destinationNode))
				throw new InvalidOperationException("No node in the graph is associated with the given destination");

			AStar(startNode, destinationNode, addHeuristic);
		}

		/// <summary>
		/// Runs the A* algorithm from the given starting node to the given destination node, using the 
		/// given heuristic function which are expected to add the heuristic to its given cost of type
		/// V. A* algorithm assumes positive edge costs.
		/// </summary>
		public void AStar(Node start, Node destination, Func<T, T, V, V> addHeuristic)
		{
			_lastAlgorithmUsedGCost = true;
			_lastAlgorithmRun = Algorithm.AStar;

			// Heap storing the unvisited nodes
			IntervalHeap<Node> openSet = new(Nodes.Count);

			// Set all costs to infinity
			foreach (Node n in _nodesByData.Values)
			{
				n.Cost = MaxCost;
				n.PreviousNode = null;
				n.Handle = null;
				n.ClosedSet = false;
			}

			// Initialize cost on start nodes
			if (!_setOfNodes.ContainsKey(start))
				throw new InvalidOperationException("Start node does not belong to the graph");
			start.Cost = addHeuristic(start.Data, destination.Data, default);
			start.GCost = default;
			IPriorityQueueHandle<Node> handle = null;
			openSet.Add(ref handle, start);
			start.Handle = handle;

			while (!openSet.IsEmpty)
			{
				// Fetch the unvisited node with the lowest cost
				Node n = openSet.DeleteMin();
				n.Handle = null;
				if (n == destination)
					return;
				n.ClosedSet = true;

				// Update costs on all nodes reachable from this one
				foreach (Edge e in n.OutgoingEdges)
				{
					Node opposite = e.OppositeNode(n);
					V tentativeGCost = CostAfterTraversalFunc(n.GCost, e);
					int comparisonResult = CostComparer(tentativeGCost, opposite.GCost);
					if (opposite.ClosedSet)
						if (comparisonResult >= 0)
							continue;
					if (opposite.Handle == null || comparisonResult < 0)
					{
						opposite.GCost = tentativeGCost;
						opposite.Cost = addHeuristic(opposite.Data, destination.Data, opposite.GCost);
						opposite.PreviousNode = n;
						if (opposite.Handle != null)
							openSet.Replace(opposite.Handle, opposite);
						else
						{
							handle = null;
							openSet.Add(ref handle, opposite);
							opposite.Handle = handle;
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns the minimal cost to the given destination
		/// </summary>
		public V GetNodeCost(T destination)
		{
			bool success = _nodesByData.TryGetValue(destination, out Node destinationNode);
			if (!success)
				throw new InvalidOperationException("Graph contains no node associated with the given destination");

			if (_lastAlgorithmUsedGCost)
				return destinationNode.GCost;
			else
				return destinationNode.Cost;
		}

		/// <summary>
		/// Returns the node path to the given destination based on the last run of the shortest
		/// path algorithm.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="nodePath"></param>
		public void GetNodePath(T destination, out List<T> nodePath)
		{
			nodePath = new List<T>();

			bool success = _nodesByData.TryGetValue(destination, out Node current);
			if (!success)
				throw new InvalidOperationException("Graph contains no node associated with the given destination");

			do
			{
				nodePath.Add(current.Data);
				current = current.PreviousNode;
			} while (current != null);

			nodePath.Reverse();
		}

		/// <summary>
		/// Returns the node path and costs to the given destination based on the last run of the shortest
		/// path algorithm.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="nodePath"></param>
		/// <param name="nodeCosts"></param>
		public void GetNodePath(T destination, out List<T> nodePath, out List<V> nodeCosts)
		{
			nodePath = new List<T>();
			nodeCosts = new List<V>();

			bool success = _nodesByData.TryGetValue(destination, out Node current);
			if (!success)
				throw new InvalidOperationException("Graph contains no node associated with the given destination");

			do
			{
				nodePath.Add(current.Data);
				nodeCosts.Add(_lastAlgorithmUsedGCost ? current.GCost : current.Cost);
				current = current.PreviousNode;
			} while (current != null);

			nodePath.Reverse();
			nodeCosts.Reverse();
		}

		/// <summary>
		/// Returns both the node and segment paths to the given destination based on
		/// the last run of the shortest path algorithm.
		/// </summary>
		public void GetNodeAndSegmentPath(T destination, out List<T> nodePath, out List<U> segmentPath)
		{
			nodePath = new List<T>();
			segmentPath = new List<U>();

			bool success = _nodesByData.TryGetValue(destination, out Node current);
			if (!success)
				throw new InvalidOperationException("Graph contains no node associated with the given destination");

			do
			{
				nodePath.Add(current.Data);
				if (current.PreviousNode != null)
				{
					segmentPath.Add(current.PreviousNode.GetEdgeLeadingToo(current).AssociatedObject);
				}
				current = current.PreviousNode;
			} while (current != null);

			nodePath.Reverse();
			segmentPath.Reverse();
		}

		/// <summary>
		/// Returns both the node and segment paths and the node costs to the given destination based on
		/// the last run of the shortest path algorithm.
		/// </summary>
		public void GetNodeAndSegmentPath(T destination, out List<T> nodePath, out List<V> nodeCosts, out List<U> segmentPath)
		{
			nodePath = new List<T>();
			nodeCosts = new List<V>();
			segmentPath = new List<U>();

			bool success = _nodesByData.TryGetValue(destination, out Node current);
			if (!success)
				throw new InvalidOperationException("Graph contains no node associated with the given destination");

			do
			{
				nodePath.Add(current.Data);
				nodeCosts.Add(_lastAlgorithmUsedGCost ? current.GCost : current.Cost);
				if (current.PreviousNode != null)
				{
					segmentPath.Add(current.PreviousNode.GetEdgeLeadingToo(current).AssociatedObject);
				}
				current = current.PreviousNode;
			} while (current != null);

			nodePath.Reverse();
			segmentPath.Reverse();
			nodeCosts.Reverse();
		}

		#endregion
	}

}
