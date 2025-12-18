//SPDX-License-Identifier: LGPL-3.0-only 
/*
Copyright(C) 2025 SINTEF
This file is part of the Scoop Utilities project.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;
using DTO = Sintef.Scoop.Utilities.SpatialGraphDTO;

namespace Sintef.Scoop.Utilities.GeoCoding
{

	/// <summary>
	/// Utility class for reading/constructing graphs from file. 
	/// </summary>
	public static class GraphConstructor
	{
		/// <summary>
		/// Input format for coordinates. Either Cartesian (x,y,z), Latlong (Latitude, Longitude, Altitude) or LongLat (Longitude, Latitude, Altitude)
		/// </summary>
		public enum CoordinateFormat
		{
			/// <summary>
			/// x, y, z
			/// </summary>
			Cartesian,

			/// <summary>
			/// Latitude, Longitude, Altitude
			/// </summary>
			LatLong,

			/// <summary>
			/// Longitude, Latitude. Altitude
			/// </summary>
			LongLat
		}

		/// <summary>
		/// Constructs a graph from the given file name.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="distanceSlack">The uncertainty in positions of graph elements that we tolerate.</param>
		/// <param name="format"></param>
		/// <returns></returns>
		[Obsolete("XML/KML functionality for Spatialgraph is no longer maintained, please use ParseFromJsonFile instead.")]
		public static SpatialGraph ParseGraph(string fileName, double distanceSlack, CoordinateFormat format)
		{
			//If graph file is a kml from google maps:
			if (Path.GetExtension(fileName) == ".kml")
				return ParseKMLFile(fileName, distanceSlack);
			else if (Path.GetExtension(fileName) == ".xml")
				return ParseXMLFile(fileName);
			else if (Path.GetExtension(fileName) == ".json")
				return ParseFromJsonFile(format, fileName); //Default assumption is cartesian
			else
				throw new Exception("Unknown file type");
		}

		/// <summary>
		/// Constructs a graph from the given kml file. Assumes all coordinates on the file are geographical.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="distanceSlack">The uncertainty in positions of graph elements that we tolerate.</param>
		/// <returns></returns>
		[Obsolete("XML/KML functionality for Spatialgraph is no longer maintained, please use JSON instead.")]
		private static SpatialGraph ParseKMLFile(string fileName, double distanceSlack)
		{
			XNamespace ns = "http://www.opengis.net/kml/2.2";

			//Read the data
			List<(string name, List<ICoordinate> coords)> lines = new();
			using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				char[] splitters = new char[] { ',' };

				XDocument doc = XDocument.Load(stream);
				List<XElement> rootDocs = doc.Root.Elements(ns + "Document").ToList();
				List<XElement> foldersEls = rootDocs.Elements(ns + "Folder").ToList();

				XElement graphEl = foldersEls.Where(f => f.Element(ns + "name").Value == "Graph").Single();

				foreach (XElement lineEl in graphEl.Elements(ns + "Placemark"))
				{
					List<ICoordinate> coordinates = new();
					string name = lineEl.Element(ns + "name").Value;
					string coordinatesString = lineEl.Element(ns + "LineString").Element(ns + "coordinates").Value;
					string trim = coordinatesString.Replace(" ", "").Replace("\n", ",").TrimStart(',').TrimEnd(',');
					string[] splitCoords = trim.Split(splitters);
					while (splitCoords.Any())
					{
						double lon = splitCoords[0].ParseInvariantDouble();
						double lat = splitCoords[1].ParseInvariantDouble();
						double alt = splitCoords[2].ParseInvariantDouble();

						coordinates.Add(new GeoCoordinate(lat, lon, alt));

						splitCoords = splitCoords.Skip(3).ToArray();
					}
					lines.Add((name, coordinates));
				}
			}

			//Build the graph
			double minLat = lines.Min(l => l.coords.Min(c => c.Y));
			double minLong = lines.Min(l => l.coords.Min(c => c.X));

			SpatialGraph g = new(new CoordinateSystem(new GeoCoordinate(minLat, minLong)));

			List<SpatialEdge> edgeList = new();
			foreach (var (name, coords) in lines)
			{
				//Introduce this line, by identifying all intersections with existing edges.
				SpatialEdge newEdge = MakeEdge(g, name, coords, distanceSlack);
				List<SpatialEdge> newEdges = IntersectWithSelfAndCreateEdges(newEdge, distanceSlack);
				newEdges.Do(e => IntersectAndCreateEdges(g, e, distanceSlack));
			}

			return g;

		}

		/// <summary>
		/// Parses a graph from the given xml file (SINTEFs simple configuration file format).
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		[Obsolete("XML/KML functionality for Spatialgraph is no longer maintained, please use JSON instead.")]
		private static SpatialGraph ParseXMLFile(string fileName)
		{
			//		XNamespace ns = "http://www.opengis.net/kml/2.2";
			SpatialGraph spatialGraph = null;

			//Read the data
			using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				XDocument doc = XDocument.Load(stream);
				XElement configEl = doc.Element("Configuration");

				CoordinateSystem coordinateSystem = ParseCoordinateSystem(doc, out bool cartesian);
				XElement networkEl = doc.Element("Network");

				spatialGraph = new SpatialGraph(coordinateSystem, networkEl.Attribute("Id").Value);
				XElement pointsEl = networkEl.Element("Points");
				foreach (XElement pointEl in pointsEl.Elements("Point"))
				{
					spatialGraph.AddNode(GetGeoCoord(pointEl, "coordinates", cartesian, coordinateSystem), pointEl.Attribute("id").Value);
				}

				XElement edgesEl = networkEl.Element("Edges");
				foreach (XElement edgeEl in edgesEl.Elements("Edge"))
				{
					string p1Id = edgeEl.Element("Node1").Attribute("nodeId").Value;
					string p2Id = edgeEl.Element("Node2").Attribute("nodeId").Value;
					string id = edgeEl.Attribute("id").Value;
					List<GeoCoordinate> coordinates = GetGeoCoordinates(edgeEl, cartesian, coordinateSystem.UtmZone);
					spatialGraph.AddEdge(spatialGraph.GetNode(p1Id), spatialGraph.GetNode(p2Id), coordinates, id);
				}
			}
			return spatialGraph;
		}

		/// <summary>
		/// Parses and constructs the coordinate system that is defined in the given configuration, in XML format.
		/// </summary>
		/// <param name="doc"></param>
		/// <param name="cartesian">On return, this is true if the system was defined as Cartesian2D.</param>
		/// <returns></returns>
		[Obsolete("XML/KML functionality for Spatialgraph is no longer maintained, please use JSON instead.")]
		private static CoordinateSystem ParseCoordinateSystem(XDocument doc, out bool cartesian)
		{
			XElement coordSystem = doc.Element("Network").Element("coordinate_system");
			cartesian = false; //If true, we assume geometrical
			GeoCoordinate origo = new(0, 0);
			switch (coordSystem.Attribute("type").Value)
			{
				case "Cartesian2D":
					cartesian = true;
					break;
				case "Geometrical":
					break;
				default:
					throw new Exception("Unknown coordinate system");
			}

			if (cartesian)//UTM
			{
				(double lat, double lon, double alt) =
					GetCoordinatesFromString(coordSystem.Element("origoInLatLot").Value);
				origo = new GeoCoordinate(lat, lon, alt);
			}
			return new CoordinateSystem(origo);
		}

		/// <summary>
		/// Creates a new graph from the given Json file with the right coordinates.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="graphFilePath"></param>
		/// <returns></returns>
		static public SpatialGraph ParseFromJsonFile(CoordinateFormat format, string graphFilePath)
		{
			if (Path.GetExtension(graphFilePath) != ".json")
			{
				throw new Exception("Unknown file type. Input must be .json.");
			}
			using var stream = new FileStream(graphFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			string data = new StreamReader(stream, System.Text.Encoding.UTF8, true, 1024, true).ReadToEnd();
			return ParseFromJsonString(format, data);
		}

		/// <summary>
		/// Creates a new graph from the given Json string
		/// </summary>
		/// <param name="format"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static SpatialGraph ParseFromJsonString(CoordinateFormat format, string data)
		{
			try
			{
				return ParseFromJsonStringProperFormat(format, data);
			}
			catch (Exception)
			{
				try
				{
					// Due to some projects using data with integer ids, this legacy version is kept as a back-up in case parsing fails. 
					return ParseFromJsonStringIntegerIds(format, data);
				}
				catch (Exception) { }

				// If it still fails, throw original exception
				throw;
			}
		}

		/// <summary>
		/// Creates a new graph from the given Json string
		/// </summary>
		/// <param name="format"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		private static SpatialGraph ParseFromJsonStringProperFormat(CoordinateFormat format, string data)
		{
			DTO.SpatialGraphContainer deserialized = JsonConvert.DeserializeObject<DTO.SpatialGraphContainer>(data);
			return ParseFromDTO(format, deserialized);
		}

		/// <summary>
		/// Creates a SpatialGraph from the given SpatialGraphConatiner
		/// </summary>
		/// <param name="format"></param>
		/// <param name="container"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static SpatialGraph ParseFromDTO(CoordinateFormat format, DTO.SpatialGraphContainer container)
		{
			var network = container.Network;
			var origoInLongLat = network.coordinate_system.origoInLongitudeLatitude;

			GeoCoordinate origin = GetCoordinateFromDTO(CoordinateFormat.LongLat, origoInLongLat) as GeoCoordinate;
			CoordinateSystem coordinateSystem = new(origin);
			SpatialGraph graph = new(coordinateSystem, network.Id);

			foreach (var p in network.Nodes)
			{
				ICoordinate co = GetCoordinateFromDTO(format, p.coordinates);
				graph.AddNode(co, p.id);
			}

			foreach (var e in network.Edges)
			{
				SpatialNode from = graph.GetNode(e.Node1.nodeId);
				if (from == null)
					throw new ArgumentException($"Cannot parse edge ({e.id} because no node exists with id {e.Node1.nodeId}");

				SpatialNode to = graph.GetNode(e.Node2.nodeId);
				if (to == null)
					throw new ArgumentException($"Cannot parse edge ({e.id} because no node exists with id {e.Node2.nodeId}");

				// The DTO coordinates include the nodes, so we skip the first and the last when listing internal point.
				int numPoints = e.coordinates.Count;
				IEnumerable<ICoordinate> edgeCoordinates = numPoints == 2
					? Enumerable.Empty<ICoordinate>()
					: e.coordinates.Skip(1)
						.Take(numPoints - 2)
						.Select(c => GetCoordinateFromDTO(format,
							c));
				graph.AddEdge(from, to, edgeCoordinates, e.id);
			}

			return graph;
		}

		/// <summary>
		/// Creates a new graph from the given Json string, assuming ids are given as integers (instead of strings)
		/// </summary>
		/// <param name="format"></param>
		/// <param name="jsonString"></param>
		/// <returns></returns>
		private static SpatialGraph ParseFromJsonStringIntegerIds(CoordinateFormat format, string jsonString)
		{
			dynamic deserialized = JsonConvert.DeserializeObject<dynamic>(jsonString);
			dynamic network = deserialized.Network;
			dynamic origoInLongLat = network.coordinate_system.origoInLongitudeLatitude;

			GeoCoordinate origin = GetCoordinateFromJson(CoordinateFormat.LongLat, origoInLongLat) as GeoCoordinate;
			CoordinateSystem coordinateSystem = new(origin);
			SpatialGraph graph = new(coordinateSystem, network.Id);

			foreach (var p in network.Nodes)
			{
				ICoordinate co = GetCoordinateFromJson(format, p.coordinates);
				graph.AddNode(co, p.id.Value.ToString());
			}

			foreach (var e in network.Edges)
			{
				SpatialNode from = graph.GetNode(e.Node1.nodeId.Value.ToString());
				if (from == null)
					throw new ArgumentException($"Cannot parse edge ({e.id} because no node exists with id {e.Node1.nodeId.Value}");

				SpatialNode to = graph.GetNode(e.Node2.nodeId.Value.ToString());
				if (to == null)
					throw new ArgumentException($"Cannot parse edge ({e.id} because no node exists with id {e.Node2.nodeId.Value}");

				List<dynamic> coords = new();
				foreach (var cs in e.coordinates)
				{
					coords.Add(cs);
				}
				IEnumerable<ICoordinate> edgeCoordinates = GetCoordinatesFromJsonList(format, coords);
				graph.AddEdge(from, to, edgeCoordinates, e.id.Value.ToString());
			}

			return graph;
		}

		/// <summary>
		/// Serializes the given graph to a Json string
		/// </summary>
		/// <param name="graph"></param>
		/// <returns></returns>
		public static string SerializeGraphToJson(SpatialGraph graph)
		{
			DTO.SpatialGraphContainer graphContainer = CreateGraphContainer(graph);

			return JsonConvert.SerializeObject(graphContainer);
		}

		/// <summary>
		/// Creates a SpatialGraphContainer (DTO) for the given graph
		/// </summary>
		/// <param name="graph"></param>
		/// <returns></returns>
		public static DTO.SpatialGraphContainer CreateGraphContainer(SpatialGraph graph)
		{
			return new()
			{
				Network = new()
				{
					Id = graph.Id,
					coordinate_system = new()
					{
						origoInLongitudeLatitude = CreateCoordinate2dDto(graph.CoordinateSystem.Origin)
					},
					Nodes = graph.Nodes.Select(CreateGraphNodeDto).ToList(),
					Edges = graph.Edges.Select(CreateGraphEdgeDto).ToList(),
				}
			};
		}

		/// <summary>
		/// Enumerates the coordinates found in the list <paramref name="coordinatesStringArray"/>, assuming the given
		/// coordinate format.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="coordinatesStringArray"></param>
		/// <returns></returns>
		private static IEnumerable<ICoordinate> GetCoordinatesFromJsonList(CoordinateFormat format, List<dynamic> coordinatesStringArray)
		{
			return coordinatesStringArray.Select(s => GetCoordinateFromJson(format, s)).Cast<ICoordinate>();
		}

		/// <summary>
		/// Reads a coordinate from the given json object.
		/// </summary>
		/// <param name="coordinateStringOrArray"></param>
		/// <param name="format">The format that the coordinates are in.</param>
		/// <returns>If the format is Cartesian, the returned coordinate is of type Coordinate. If it is LatLon, 
		/// or LonLat, the returned coordinate is of type GeoCoordinate.</returns>
		public static ICoordinate GetCoordinateFromJson(CoordinateFormat format, dynamic coordinateStringOrArray)
		{
			(double x, double y, double z) c = GetCoordinateFromJson(coordinateStringOrArray);
			return format switch
			{
				CoordinateFormat.Cartesian => new Coordinate(c.x, c.y, c.z),
				CoordinateFormat.LongLat => new GeoCoordinate(c.y, c.x, c.z),
				CoordinateFormat.LatLong => new GeoCoordinate(c.x, c.y, c.z),
				_ => throw new NotImplementedException($"Un-supported coordinate format {format}")
			};
		}

		/// <summary>
		/// Reads a coordinate from the given json object.
		/// </summary>
		/// <param name="coordinate"></param>
		/// <param name="format">The format that the coordinates are in.</param>
		/// <returns>If the format is Cartesian, the returned coordinate is of type Coordinate. If it is LatLon, 
		/// or LonLat, the returned coordinate is of type GeoCoordinate.</returns>
		public static ICoordinate GetCoordinateFromDTO(CoordinateFormat format, List<double> coordinate)
		{
			(double x, double y) = (coordinate[0], coordinate[1]);
			var z = coordinate.Count > 2 ? coordinate[2] : double.NaN;

			return format switch
			{
				CoordinateFormat.Cartesian => new Coordinate(x, y, z),
				CoordinateFormat.LongLat => new GeoCoordinate(y, x, z),
				CoordinateFormat.LatLong => new GeoCoordinate(x, y, z),
				_ => throw new NotImplementedException($"Un-supported coordinate format {format}")
			};
		}

		/// <summary>
		/// Reads a string of coordinates for edges (xmlelements)
		/// </summary>
		/// <param name="edgeEl"></param>
		/// <param name="cartesian">If false, we assume the given XElement contains geographical coordinates. If true, we assume cartesian (UTM).</param>
		/// <param name="utmZone">The UTM zone. Only used if <paramref name="cartesian"/> == true.</param>
		/// <returns></returns>
		[Obsolete("XML/KML functionality for Spatialgraph is no longer maintained, please use JSON instead.")]
		private static List<GeoCoordinate> GetGeoCoordinates(XElement edgeEl, bool cartesian, int utmZone)
		{
			string coordinatesString = edgeEl.Element("coordinates").Value;
			return GetGeoCoordinates(coordinatesString, cartesian, utmZone);
		}


		/// <summary>
		/// Reads a string of coordinates for edges. If the input string contains no coordinates, the returned list will be empty
		/// (this may happen if, e.g., and edge has no additional geometry outside start and end points.
		/// </summary>
		/// <param name="coordinatesString"></param>
		/// <param name="cartesian">If false, we assume the given XElement contains geographical coordinates. If true, we assume cartesian (UTM).</param>
		/// <param name="utmZone">The utm zone that we use. Only used if <paramref name="cartesian"/> == true.</param>
		/// <returns></returns>
		[Obsolete("XML/KML functionality for Spatialgraph is no longer maintained, please use JSON instead.")]
		internal static List<GeoCoordinate> GetGeoCoordinates(string coordinatesString, bool cartesian, int utmZone)
		{
			if (coordinatesString.NullOrEmpty())
				return new List<GeoCoordinate>();

			char[] splitters = new char[] { ',' };
			List<GeoCoordinate> coordinates = new();
			string trim = coordinatesString.Replace(" ", "").Replace("\n", ",").Replace("\t", "").TrimStart(',').TrimEnd(',');
			string[] splitCoords = trim.Split(splitters);
			while (splitCoords.Any())
			{
				if (cartesian)
				{
					double x = splitCoords[0].ParseInvariantDouble();
					double y = splitCoords[1].ParseInvariantDouble();
					coordinates.Add(CoordinateSystem.ToGeoCoordinate(new UtmCoordinate(x, y, utmZone, true)));
					splitCoords = splitCoords.Skip(2).ToArray();
				}
				else
				{
					double lon = splitCoords[0].ParseInvariantDouble();
					double lat = splitCoords[1].ParseInvariantDouble();
					double alt = splitCoords[2].ParseInvariantDouble();
					coordinates.Add(new GeoCoordinate(lat, lon, alt));
					splitCoords = splitCoords.Skip(3).ToArray();
				}
			}

			return coordinates;
		}


		/// <summary>
		/// Reads cartesian coordinates, based on an array of coordinate arrays, where each element contains one
		/// set of coordinates (dynamic, assumed to be arrays of length two), and converts these to an enumeration of <see cref="GeoCoordinate"/>'s. 
		/// If the input string contains no coordinates, the returned list will be empty.
		/// </summary>
		/// <param name="coordinatesStringArray">The coordinate string array</param>
		/// <param name="coSys">The coordinates system that lets us convert the coordinates to geographical.</param>
		/// <returns></returns>
		internal static IEnumerable<GeoCoordinate> GetGeoCoordinatesFromCartesianArray(List<dynamic> coordinatesStringArray, CoordinateSystem coSys)
		{
			return coordinatesStringArray.Select(s => GetCoordinatesFromArray(s))
				.Select(tup => coSys.GetGeoCoordinate(new Coordinate(tup.x, tup.y, tup.z)));
		}


		/// <summary>
		/// Reads a string of cartesian coordinates, based on an array of coorinate strings where each string contains one
		/// set of coordinates, and converts these to an enumeration of <see cref="GeoCoordinate"/>'s. 
		/// If the input string contains no coordinates, the returned list will be empty.
		/// </summary>
		/// <param name="coordinatesStringArray">The coordinate string array</param>
		/// <param name="coSys">The coordinates system that lets us convert the coordinates to geographical.</param>
		/// <returns></returns>
		internal static IEnumerable<GeoCoordinate> GetGeoCoordinatesFromCartesianStrings(List<string> coordinatesStringArray, CoordinateSystem coSys)
		{
			return coordinatesStringArray.Select(s => GetCoordinatesFromString(s))
				.Select(tup => coSys.GetGeoCoordinate(new Coordinate(tup.x, tup.y, tup.z)));
		}

		/// <summary>
		/// Reads geographical coordinates, based on an array of coordinate arrays, where each element contains one
		/// set of coordinates (dynamic, assumed to be arrays of length two), and converts these to an enumeration of <see cref="GeoCoordinate"/>'s. 
		/// If the input string contains no coordinates, the returned list will be empty.
		/// </summary>
		/// <param name="coordinatesStringArray">The coordinate string array</param>
		/// <returns></returns>
		internal static IEnumerable<GeoCoordinate> GetGeoCoordinatesFromArrays(List<dynamic> coordinatesStringArray)
		{
			return coordinatesStringArray.Select(s => GetCoordinatesFromArray(s))
				.Select(tup => new GeoCoordinate(tup.x, tup.y, tup.z));
		}


		/// <summary>
		/// Reads a string of geographical coordinates for edges, based on an array of coorinate strings where each string contains one
		/// set of coordinates. 
		/// If the input string contains no coordinates, the returned list will be empty.
		/// </summary>
		/// <param name="coordinatesStringArray">The coordinate string array</param>
		/// <returns></returns>
		internal static IEnumerable<GeoCoordinate> GetGeoCoordinatesFromStrings(List<string> coordinatesStringArray)
		{
			return coordinatesStringArray.Select(s => GetCoordinatesFromString(s))
				.Select(
					tup => new GeoCoordinate(tup.y, tup.x, tup.z));
		}


		/// <summary>
		/// Returns the geographical coordinates of the point.
		/// </summary>
		/// <param name="pointEl"></param>
		/// <param name="coordinateElementString">Only</param>
		/// <param name="coSys">Only necessary if we are reading utm or local coordinates.</param>
		/// <param name="cartesian">If false, we assume the given XElement contains geographical coordinates. If true, we assume cartesian (UTM).</param>
		/// <returns></returns>
		internal static GeoCoordinate GetGeoCoord(XElement pointEl, string coordinateElementString, bool cartesian, CoordinateSystem coSys)
		{
			(double x, double y, double z) = GetCoordinatesFromString(pointEl.Element(coordinateElementString).Value);
			if (cartesian)
				return coSys.GetGeoCoordinate(new Coordinate(x, y, z));
			else
				return new GeoCoordinate(y, x, z);
		}

		/// <summary>
		/// Finds all intersection between the new edge and the other edges in the given <paramref name="graph"/>.
		/// Splits intersecting pairs of edges by introducing new edges and deleting the old ones.
		/// </summary>
		/// <param name="newEdge">The new edge (can already have been added to the <paramref name="graph"/>).</param>
		/// <param name="graph"></param>
		/// <param name="distanceSlack">Uncertainty in position in input data.</param>
		private static void IntersectAndCreateEdges(SpatialGraph graph, SpatialEdge newEdge, double distanceSlack)
		{
			List<SpatialEdge> newEdgeParts = new() { newEdge };
			Debug.Assert(graph == newEdge.Graph);
			List<SpatialEdge> newExistingParts = graph.Edges.Except(newEdge).ToList();

			while (true)
			{
				//Find a pair that intersect
				bool foundIntersection = false;
				(SpatialEdge newEdge, SpatialEdge oldEdge) interSectingPair = (null, null);
				foreach (SpatialEdge newPart in newEdgeParts)
				{
					foreach (var existing in newExistingParts)
					{
						if (newPart.Intersects(existing, distanceSlack))
						{
							interSectingPair = (newPart, existing);
							foundIntersection = true;
							break;
						}
					}
					if (foundIntersection)
						break;
				}
				if (!foundIntersection)
					break; //No more intersections

				//Split the members on each pair on their intersections
				List<SpatialEdge> newEdgeReplacements = interSectingPair.newEdge.SplitOnIntersectionsWith(interSectingPair.oldEdge, distanceSlack).ToList();
				if (newEdgeReplacements.Count > 0)
				{
					newEdgeParts.Remove(interSectingPair.newEdge);
					newEdgeParts.AddRange(newEdgeReplacements);
				}
				List<SpatialEdge> oldEdgeReplacements = interSectingPair.oldEdge.SplitOnIntersectionsWith(interSectingPair.newEdge, distanceSlack).ToList();
				if (oldEdgeReplacements.Count > 0)
				{
					newExistingParts.Remove(interSectingPair.oldEdge);
					newExistingParts.AddRange(oldEdgeReplacements);
				}
			}
		}


		/// <summary>
		/// Finds all intersection between the given edge and itself.
		/// For each found intersection, creates all resulting new edges and delete the old split ones from the graph.
		/// </summary>
		/// <param name="edge"></param>
		/// <param name="distanceSlack">Uncertainty in position in input data.</param>
		/// <returns></returns>
		private static List<SpatialEdge> IntersectWithSelfAndCreateEdges(SpatialEdge edge, double distanceSlack)
		{

			//Split the members on each pair on their intersections
			List<SpatialEdge> newEdgeParts = edge.SplitOnIntersectionsWith(edge, distanceSlack).ToList();
			if (!newEdgeParts.Any())
				newEdgeParts.Add(edge);
			return newEdgeParts;
		}

		/// <summary>
		/// Splits the given <paramref name="edgeToBeSplit"/> into two edges, by adding a new node part way along the edge.
		/// Adding the new node and the two new edges to the graph. 
		/// </summary>
		/// <param name="coord">The node that will be inserted to split the edge.</param>
		/// <param name="nodeName"></param>
		/// <param name="edgeToBeSplit">The edge to split</param>
		/// <param name="distanceSlack">The uncertainty in positions that we allow in defining the data. In meters.</param>
		/// <returns>The splitting partial node.</returns>
		public static SpatialNode SplitEdgeOnNode(GeoCoordinate coord, string nodeName, SpatialEdge edgeToBeSplit, double distanceSlack)
		{
			var proj = edgeToBeSplit.ClosestPoint(coord);
			ICoordinate closestCoord = proj.ClosestPoint;
			double distanceFromEdge = coord.DistanceTo(closestCoord);
			if (distanceFromEdge > distanceSlack)
				throw new Exception($"SplitEdgeOnNewNode: coordinate {nodeName} too far from any edge (distance > {distanceSlack}");

			SpatialNode splitterNode = edgeToBeSplit.SplitOnCoordinates(new List<ICoordinate> { closestCoord }, distanceSlack).nodes.Single();
			return splitterNode;
		}

		/// <summary>
		/// Constructs an edge, and the corresponding end nodes.
		/// </summary>
		/// <param name="g"></param>
		/// <param name="name"></param>
		/// <param name="coords">All the coordinates of the new edge, assumed to be sorted from "start" to "end".</param>
		/// <param name="distanceSlack">The uncertainty in spatial location that we accept between two things that should be "at the same place". Used
		/// to allow the user to provide un-precise data.</param>
		/// <returns></returns>
		private static SpatialEdge MakeEdge(SpatialGraph g, string name, List<ICoordinate> coords, double distanceSlack)
		{
			SpatialNode node1 = GetOrCreateNode(coords.First(), name + "From");
			SpatialNode node2 = GetOrCreateNode(coords.Last(), name + "To");
			int n = coords.Count;
			return g.AddEdge(node1, node2, coords.Skip(1).Take(n - 2), name);

			// <summary>
			// Gets the closest node within the distanceSlack, or creates a new one if no such exists.
			// </summary>
			// <param name="g"></param>
			// <param name="distanceSlack"></param>
			// <param name="c1"></param>
			SpatialNode GetOrCreateNode(ICoordinate c, string id)
			{
				SpatialNode node = g.ClosestNode(c);
				if (node == null || node.Coordinate.DistanceTo(c) > distanceSlack)
				{
					//Add new node
					node = g.AddNode(c, id);
				}
				return node;
			}
		}


		/// <summary>
		/// Creates a graph node DTO from a <see cref="SpatialNode"/>
		/// </summary>
		/// <param name="node">The node to convert.</param>
		public static DTO.Node CreateGraphNodeDto(SpatialNode node)
		{
			return new()
			{
				id = node.Id,
				coordinates = CreateCoordinateDto(node.Coordinate)
			};
		}

		/// <summary>
		/// Creates a graph edge DTO from a <see cref="SpatialEdge"/>
		/// </summary>
		/// <param name="edge">The edge to convert.</param>
		public static DTO.Edge CreateGraphEdgeDto(SpatialEdge edge)
		{
			return new()
			{
				id = edge.Id,
				coordinates = edge.Coordinates.Select(CreateCoordinateDto).ToList(),
				Node1 = new() { nodeId = edge.From.Id },
				Node2 = new() { nodeId = edge.To.Id },
			};
		}

		/// <summary>
		///	Creates a coordinate DTO (list of numbers) from a coordinate.
		/// </summary>
		/// <param name="from">The coordinate to create the DTO from</param>
		/// <returns>List of two numbers if from is a 2D coordinate, else three numbers.</returns>
		public static List<double> CreateCoordinateDto(ICoordinate from)
		{
			return double.IsNaN(from.Z) ?
				new() { from.X, from.Y } :
				new() { from.X, from.Y, from.Z };
		}

		/// <summary>
		///	Creates a 2D coordinate DTO (list of numbers) from a coordinate. Ignores any z-component if any.
		/// </summary>
		/// <param name="from">The coordinate to create the DTO from</param>
		/// <returns>List of two numbers.</returns>
		public static List<double> CreateCoordinate2dDto(ICoordinate from)
		{
			return new() { from.X, from.Y };
		}

		/// <summary>
		/// Returns a coordinate tuple (x,y,z) from the given dynamic array
		/// </summary>
		/// <param name="coordinatesArray">The text string to read from</param>
		/// <returns>The x, y, z coordinates, z is <see cref="double.NaN"/> if the array only has two items.</returns>
		private static (double x, double y, double z) GetCoordinatesFromArray(dynamic coordinatesArray)
		{
			double x = coordinatesArray[0];
			double y = coordinatesArray[1];
			double z = Enumerable.Count(coordinatesArray) > 2 ? coordinatesArray[2] : double.NaN;
			return (x, y, z);
		}

		/// <summary>
		/// Returns a coordinate tuple (x,y, z) from the given string (cartesian, utm, geographical).
		/// </summary>
		/// <param name="coordinatesString">The text string to read from</param>
		/// <returns></returns>
		private static (double x, double y, double z) GetCoordinatesFromString(string coordinatesString)
		{
			char[] splitters = new char[] { ',' };
			string trim = coordinatesString.Replace(" ", "").Replace("\n", ",").TrimStart(',').TrimEnd(',');
			string[] splitCoords = trim.Split(splitters);
			double x = splitCoords[0].ParseInvariantDouble();
			double y = splitCoords[1].ParseInvariantDouble();
			double z = splitCoords.Length > 2 ? splitCoords[2].ParseInvariantDouble() : double.NaN;

			return (x, y, z);
		}

		/// <summary>
		/// Reads the coordinates from the given json object.
		/// </summary>
		/// <param name="coordinateStringOrArray"></param>
		/// <returns></returns>
		private static (double x, double y, double z) GetCoordinateFromJson(dynamic coordinateStringOrArray)
		{
			//Check if this is a string
			bool isString = coordinateStringOrArray.GetType().Name != "JArray";
			return isString ? GetCoordinatesFromString((string)coordinateStringOrArray) : GetCoordinatesFromArray(coordinateStringOrArray);
		}

	}
}
