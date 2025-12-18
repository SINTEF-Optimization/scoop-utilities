//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using FluentAssertions;
using FluentAssertions.Equivalency;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Sintef.Scoop.Utilities.GeoCoding;
using Sintef.Scoop.Utilities.SpatialGraphDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using static Sintef.Scoop.Utilities.GeoCoding.GraphConstructor;
using CoordinateSystem = Sintef.Scoop.Utilities.GeoCoding.CoordinateSystem;
using SpatialGraph = Sintef.Scoop.Utilities.GeoCoding.SpatialGraph;

namespace Sintef.Scoop.Utilities.Tests.SpatialGraphTests
{
	[TestClass]
	public class GraphConstructorTests
	{
        private GeoCoding.SpatialGraph _graph;

		[TestInitialize]
		public void Setup()
		{
            _graph = new();
			var n1 = _graph.AddNode(new GeoCoordinate(50, 50), "1");
			var n2 = _graph.AddNode(new GeoCoordinate(40, 40), "2");
			_graph.AddEdge(n1, n2);
		}

		[TestMethod]
		public void TestRoundTrip()
		{
			try
			{
				var jsonString = SerializeGraphToJson(_graph);
				var roundTripGraph = ParseFromJsonString(CoordinateFormat.LongLat, jsonString);
				Assert.AreEqual("1 2", roundTripGraph.Nodes.Select(n => n.Id).OrderBy(x => x).Join(" "));
				Assert.AreEqual(
					"1 - 2",
					roundTripGraph.Edges.Select(e => $"{e.From.Id} - {e.To.Id}").OrderBy(x => x).Join(" "));
				Assert.AreEqual(
					"40 40 50 50",
					roundTripGraph.Nodes.Select(n => $"{n.Coordinate.X} {n.Coordinate.Y}").OrderBy(x => x).Join(" "));
			}
			catch (Exception e)
			{
				Assert.Fail(e.Message);
			}
		}

		[TestMethod("Creates coordinates without Z when given array of length 2")]
		public void GetCoordinateFromDTO_WithoutZCoordinate()
		{
			// Act + assert:
			AssertionExtensions.Should(
					GetCoordinateFromDTO(
						CoordinateFormat.Cartesian,
						new() { 123, 456 }))
				.BeEquivalentTo(new Coordinate(123, 456));

			AssertionExtensions.Should(
					GetCoordinateFromDTO(
						CoordinateFormat.LongLat,
						new() { -12.3, 45.6 }))
				.BeEquivalentTo(new GeoCoordinate(longitude: -12.3, latitude: 45.6));

			AssertionExtensions.Should(
					GetCoordinateFromDTO(
						CoordinateFormat.LatLong,
						new() { -12.3, 45.6 }))
				.BeEquivalentTo(new GeoCoordinate(latitude: -12.3, longitude: 45.6));
		}

		[TestMethod("Creates coordinates with Z when given array of length 3")]
		public void GetCoordinateFromDTO_WithZCoordinate()
		{
			// Act + assert:
			AssertionExtensions.Should(
					GetCoordinateFromDTO(
						CoordinateFormat.Cartesian,
						new() { 123, 456, 789 }))
				.BeEquivalentTo(new Coordinate(123, 456, 789));

			AssertionExtensions.Should(
					GetCoordinateFromDTO(
						CoordinateFormat.LongLat,
						new() { -12.3, 45.6, 789 }))
				.BeEquivalentTo(new GeoCoordinate(longitude: -12.3, latitude: 45.6, altitude: 789));

			AssertionExtensions.Should(
					GetCoordinateFromDTO(
						CoordinateFormat.LatLong,
						new() { -12.3, 45.6, 789 }))
				.BeEquivalentTo(new GeoCoordinate(latitude: -12.3, longitude: 45.6, altitude: 789));
		}

		[TestMethod("Creates coordinates without Z when given array of length 2")]
		public void GetCoordinateFromJson_WithoutZCoordinate()
		{
			// Act + assert:
			AssertionExtensions.Should(
					GetCoordinateFromJson(
						CoordinateFormat.Cartesian,
						new JArray { 123, 456 }))
				.BeEquivalentTo(new Coordinate(123, 456));

			AssertionExtensions.Should(
					GetCoordinateFromJson(
						CoordinateFormat.LongLat,
						new JArray { -12.3, 45.6 }))
				.BeEquivalentTo(new GeoCoordinate(longitude: -12.3, latitude: 45.6));

			AssertionExtensions.Should(
					GetCoordinateFromJson(
						CoordinateFormat.LatLong,
						new JArray { -12.3, 45.6 }))
				.BeEquivalentTo(new GeoCoordinate(latitude: -12.3, longitude: 45.6));
		}

		[TestMethod("Creates coordinates with Z when given array of length 3")]
		public void GetCoordinateFromJson_WithZCoordinate()
		{
			// Act + assert:
			AssertionExtensions.Should(
					GetCoordinateFromJson(
						CoordinateFormat.Cartesian,
						new JArray { 123, 456, 789 }))
				.BeEquivalentTo(new Coordinate(123, 456, 789));

			AssertionExtensions.Should(
					GetCoordinateFromJson(
						CoordinateFormat.LongLat,
						new JArray { -12.3, 45.6, 789 }))
				.BeEquivalentTo(new GeoCoordinate(longitude: -12.3, latitude: 45.6, altitude: 789));

			AssertionExtensions.Should(
					GetCoordinateFromJson(
						CoordinateFormat.LatLong,
						new JArray { -12.3, 45.6, 789 }))
				.BeEquivalentTo(new GeoCoordinate(latitude: -12.3, longitude: 45.6, altitude: 789));
		}

		[TestMethod("Creates coordinates without Z when given string with 2 components")]
		public void GetCoordinateFromJson_String_WithoutZCoordinate()
		{
			// Act + assert:
			AssertionExtensions.Should(GetCoordinateFromJson(CoordinateFormat.Cartesian, "123.0,456.0"))
				.BeEquivalentTo(new Coordinate(123, 456));

			AssertionExtensions.Should(GetCoordinateFromJson(CoordinateFormat.LongLat, "-12.3,45.6"))
				.BeEquivalentTo(new GeoCoordinate(longitude: -12.3, latitude: 45.6));

			AssertionExtensions.Should(GetCoordinateFromJson(CoordinateFormat.LatLong, "-12.3,45.6"))
				.BeEquivalentTo(new GeoCoordinate(latitude: -12.3, longitude: 45.6));
		}

		[TestMethod("Creates coordinates with Z when given string with 3 components")]
		public void GetCoordinateFromJson_String_WithZCoordinate()
		{
			// Act + assert:
			AssertionExtensions.Should(GetCoordinateFromJson(CoordinateFormat.Cartesian, "123.0,456,789.00"))
				.BeEquivalentTo(new Coordinate(123, 456, 789));

			AssertionExtensions.Should(GetCoordinateFromJson(CoordinateFormat.LongLat, "-12.3,45.60,789"))
				.BeEquivalentTo(new GeoCoordinate(longitude: -12.3, latitude: 45.6, altitude: 789));

			AssertionExtensions.Should(GetCoordinateFromJson(CoordinateFormat.LatLong, "-12.30,45.6,789"))
				.BeEquivalentTo(new GeoCoordinate(latitude: -12.3, longitude: 45.6, altitude: 789));
		}

		[TestMethod("Checks that internal geometry points are handled correctly, and that this still applies after serialization and deserialization.")]
		public void TestEdgeInteralGeometry()
		{
			SpatialGraph g = new SpatialGraph();
			var n1 = g.AddNode(new Coordinate(0, 0), "n1");
			var n2 = g.AddNode(new Coordinate(10, 0), "n2");

			// Test that an edge without internal geometry, has exactly two coordinates.
			var edge = g.AddEdge(n1, n2, id: "noInternalPoints");
			Assert.AreEqual(2, edge.Coordinates.Count());

			// Test that an edge with internal geometry, has more than two coordinates.
			var internalGeometry = new List<ICoordinate> { new Coordinate(5, 5) };
			var edgeWithGeometry = g.AddEdge(n1, n2, internalGeometry, id: "oneInternalPoint");
			Assert.AreEqual(3, edgeWithGeometry.Coordinates.Count());

			// Serialize and deserialize the graph
			var gJson = SerializeGraphToJson(g);
			var gDeserialized = ParseFromJsonString(CoordinateFormat.Cartesian, gJson);

			// Test that the deserialized edge without internal geometry, has exactly two coordinates.
			var deserializedEdge = gDeserialized.GetEdge(edge.Id);
			Assert.AreEqual(0, deserializedEdge.Geometry?.InternalPoints.Count() ?? 0);
			Assert.AreEqual(2, deserializedEdge.Coordinates.Count());

			// Test that the deserialized edge with internal geometry, has more than two coordinates.
			var deserializedEdgeWithGeometry = gDeserialized.GetEdge(edgeWithGeometry.Id);
			Assert.AreEqual(1, deserializedEdgeWithGeometry.Geometry!.InternalPoints.Count());
			Assert.AreEqual(3, deserializedEdgeWithGeometry.Coordinates.Count());
		}

		[TestMethod("Creates a 2D graph when the data has 2 coordinate numbers")]
		public void ParseFromDTO_WithoutZCoordinate()
		{
			// Arrange:
			var data = new SpatialGraphContainer
			{
				Network = new()
				{
					Id = "graphId",
					coordinate_system = new()
					{
						origoInLongitudeLatitude = new() { -12.3, 45.6 }
					},
					Nodes = new()
					{
						new()
						{
							id = "A1",
							coordinates = new() {-12.6, 45.8}
						},
						new()
						{
							id = "A2",
							coordinates = new() {-12.0, 45.8}
						},
						new()
						{
							id = "A3",
							coordinates = new() {-12.3, 46.1}
						}
					},
					Edges = new()
					{
						new()
						{
							id = "A1-A2",
							Node1 = new() {nodeId = "A1"},
							Node2 = new() {nodeId = "A2"},
							coordinates = new()
							{
								new() {-12.6, 45.8},
								new() {-12.3, 45.6},
								new() {-12.0, 45.8}
							}
						},
						new()
						{
							id = "A1-A3",
							Node1 = new() {nodeId = "A1"},
							Node2 = new() {nodeId = "A3"},
							coordinates = new()
							{
								new() {-12.6, 45.8},
								new() {-12.5, 45.0},
								new() {-12.3, 46.1}
							}
						}
					}
				}
			};

			// Act:
			var result = ParseFromDTO(CoordinateFormat.LatLong, data);

			// Assert:
			Assert.AreNotEqual(null, result.Id);
			var origin = new GeoCoordinate(longitude: -12.3, latitude: 45.6);
			var graph = new SpatialGraph(new CoordinateSystem(origin), result.Id);
			var n1 = graph.AddNode(new GeoCoordinate(-12.6, 45.8), "A1");
			var n2 = graph.AddNode(new GeoCoordinate(-12.0, 45.8), "A2");
			var n3 = graph.AddNode(new GeoCoordinate(-12.3, 46.1), "A3");

			graph.AddEdge(
				n1,
				n2,
				new List<ICoordinate>
				{
					new GeoCoordinate(-12.3, 45.6),
				},
				"A1-A2");

			graph.AddEdge(
				n1,
				n3,
				new List<ICoordinate>
				{
					new GeoCoordinate(-12.5, 45.0),
				},
				"A1-A3");

			result.Should()
				.BeEquivalentTo(graph, GraphOptions);
		}

		[TestMethod("Creates a 3D graph when the data has 3 coordinate numbers")]
		public void ParseFromDTO_WithZCoordinate()
		{
			// Arrange:
			var data = new SpatialGraphContainer
			{
				Network = new()
				{
					Id = "graphId",
					coordinate_system = new()
					{
						origoInLongitudeLatitude = new() { -12.3, 45.6, 768 }
					},
					Nodes = new()
					{
						new()
						{
							id = "A1",
							coordinates = new() {-12.6, 45.8, 768}
						},
						new()
						{
							id = "A2",
							coordinates = new() {-12.0, 45.8, 769}
						},
						new()
						{
							id = "A3",
							coordinates = new() {-12.3, 46.1, 770}
						}
					},
					Edges = new()
					{
						new()
						{
							id = "A1-A2",
							Node1 = new() {nodeId = "A1"},
							Node2 = new() {nodeId = "A2"},
							coordinates = new()
							{
								new() {-12.6, 45.8, 768},
								new() {-12.3, 45.6, 768.6},
								new() {-12.0, 45.8, 769}
							}
						},
						new()
						{
							id = "A1-A3",
							Node1 = new() {nodeId = "A1"},
							Node2 = new() {nodeId = "A3"},
							coordinates = new()
							{
								new() {-12.6, 45.8, 768},
								new() {-12.5, 45.0, 766},
								new() {-12.3, 46.1, 770}
							}
						}
					}
				}
			};

			// Act:
			var result = ParseFromDTO(CoordinateFormat.LatLong, data);

			// Assert:
			var origin = new GeoCoordinate(longitude: -12.3, latitude: 45.6, altitude: 768);
			var graph = new SpatialGraph(new CoordinateSystem(origin), result.Id);
			var n1 = graph.AddNode(new GeoCoordinate(-12.6, 45.8, 768), "A1");
			var n2 = graph.AddNode(new GeoCoordinate(-12.0, 45.8, 769), "A2");
			var n3 = graph.AddNode(new GeoCoordinate(-12.3, 46.1, 770), "A3");

			graph.AddEdge(
				n1,
				n2,
				new List<ICoordinate>
				{
					new GeoCoordinate(-12.3, 45.6, 768.6),
				},
				"A1-A2");

			graph.AddEdge(
				n1,
				n3,
				new List<ICoordinate>
				{
					new GeoCoordinate(-12.5, 45.0, 766),
				},
				"A1-A3");

			result.Should()
				.BeEquivalentTo(graph, GraphOptions);
		}

		/// <summary>
		/// Builds object comparison options so that we can compare the equality of two graphs without running into
		/// issues around cyclic definitions.
		/// </summary>
		private static EquivalencyAssertionOptions<SpatialGraph> GraphOptions(
			EquivalencyAssertionOptions<SpatialGraph> options)
		{
			// This is a bit of a mess, but due to the cyclic nature of the definition, (everything is connected to
			// everything else), we have to break this up somehow.

			options.For(g => g.Nodes).Exclude(node => node.Graph);
			options.For(g => g.Nodes).Exclude(node => node.AllEdges);

			options.For(g => g.Nodes).For(n => n.InEdges).Exclude(e => e.From);
			options.For(g => g.Nodes).For(n => n.InEdges).Exclude(e => e.To);
			options.For(g => g.Nodes).For(n => n.InEdges).Exclude(e => e.Geometry);
			options.For(g => g.Nodes).For(n => n.InEdges).Exclude(e => e.Graph);

			options.For(g => g.Nodes).For(n => n.OutEdges).Exclude(e => e.From);
			options.For(g => g.Nodes).For(n => n.OutEdges).Exclude(e => e.To);
			options.For(g => g.Nodes).For(n => n.OutEdges).Exclude(e => e.Geometry);
			options.For(g => g.Nodes).For(n => n.OutEdges).Exclude(e => e.Graph);

			options.For(g => g.Edges).Exclude(edge => edge.Graph);
			options.For(g => g.Edges).Exclude(edge => edge.From.Graph);
			options.For(g => g.Edges).Exclude(edge => edge.From.AllEdges);
			options.For(g => g.Edges).Exclude(edge => edge.From.InEdges);
			options.For(g => g.Edges).Exclude(edge => edge.From.OutEdges);
			options.For(g => g.Edges).Exclude(edge => edge.To.Graph);
			options.For(g => g.Edges).Exclude(edge => edge.To.AllEdges);
			options.For(g => g.Edges).Exclude(edge => edge.To.InEdges);
			options.For(g => g.Edges).Exclude(edge => edge.To.OutEdges);
            options.For(g => g.Edges).Exclude(edge => edge.To.OutEdges);

			return options;
		}
	}
}