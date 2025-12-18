# Scoop Utilities

Scoop Utilities is a collection of classes that have no common theme apart from being useful in day to day programming (or at least from time to time). Most have not been fully designed from the start, but have grown according to the needs of other projects. Thus, you may find that functionality that naturally belongs here, is not present, because it was not needed before.

## Class overview

To get the most out of these classes, you really have to investigate and get to know them yourself. However, here is a basic overview:

* **Collections (folder).** Contains some collections and extensions to collections
    * ChronologicalDictionary - Dictionary that ensures that items are enumerated in the order of insertion.
    * CollectionExtensions - Extension methods for collections.
    * GuidRepository - A wrapper around a dictionary from GUID to object.
    * IntInterval - A class for managing 32 bit signed integer intervals.
    * NonDominatedSetOfLists - A set of lists where no list is dominated by another.
    * QuickList - A list with constant speed removal
    * RollingMemory - A collection containing up to a fixed number of elements.
    * WeakDictionary - A dictionary where the keys may get garbage collected.
* **Combinatorics (folder).** Contains classes to generate all permutations or combinations of a collection of items
    * CartesianProduct - Extension to produce the cartesian product of a set of sets
    * CombinationsFromTwoLists - Produces all combinations of the elemens in two lists.
    * PartialOrdering - Extension methods providing partial orderings of sequences.
    * Permutation - Provides permutations of numbers.
* **Comparers (folder).** Contains classes for comparing some sets or with numeric tolerance.
    * Comparers - Helpers for comparing lexically, or creating comparers similar to OrderBy()
    * DictionaryComparer - Compares dictionaries.
    * ListComparer - Compares lists.
    * SortedSetComparer - Compares two sorted sets.
    * TolerantComparer - Compares doubles with tolerance.
* **Concurrency (folder).** Contains some tools for managing parallel execution of tasks.
    * IBgTaskRunner - Interface for background task runners.
    * LimitedConcurrencyLevelTaskScheduler - A task scheduler which can limit the maximum degree of concurrency.
    * ParallelTasks - Provides mechanisms to limit the degree of parallelism from further up the call hierarchy.
    * SimpleBgTaskRunner - An implementation of IBgTaskRunner
* **Enumerables (folder).** Contains BufferEnumerator and CachedIEnumerable which Help minimize the invocations of heavy enumerations.
    * BufferEnumerator - An enumerator that lazily buffers elements of a given enumerable, for faster repeated enumerations.
    * CachedIEnumerable - An enumerable which (always) caches the elements of a given enumerable, for faster repeated enumerations.
* **FiniteStateMachine.** A set of classes for implementing finite state machines.
    * FiniteStateMachine - Main class for implementing state machines.
    * FiniteStateMachineAbstractStates - A FiniteStateMachine based on states derived from AbstractState.
    * IState - Interface for states in a FiniteStateMachine.
    * ITransition - Interface for a transition between 2 states in a FiniteStateMachine.
    * ITrigger - Interface for a trigger which cause a transition between 2 states in FiniteStateMachine.
* **GeoCoding (folder).** Classes for calculations with points/lines etc. on the Earth.
    * BoundingBox - A bounding box as an area on the surface of the earth.
    * BoundingBoxBase - Base class for bounding boxes.
    * BoundingBoxCartesian - A bounding box in a cartesian plane.
    * ClosedPolygon - A closed region bounded by edges that are straight lines in a cartesian coordinate system.
    * Coordinate - A local cartesian coordinate.
    * CoordinateSystem - A local coordinate system.
    * GeoCoordinate - A point on earth.
    * GeometricTools - Some utilities for intersecting lines with circle.
    * IBoundingBox - An interface for bounding boxes.
    * ICoordinate - A common interface for coordinate classes.
    * LocalCoordinateSystem - A local coordinate system.
    * PolyLine - A polyline, consisting of consecutive connected lines.
    * ProjectionResult - Describes the result of an attempted projection of a coordinate point onto a line segment or polyline.
    * Region2D - A 2D region in a cartesian coordinate system.
    * RegionComponent2D - A connected region in a cartesian 2D coordinate system with one positively oriented outer loop, and some possible negatively oriented inner loops representing holes.
    * UtmCoordinate - A coordinate in the UTM system.
    * WebMercatorSystem - Utilities for converting between GeoCoordinates and Web Mercator coordinates.
* **GeoGeometry (folder).** Some methods for doing calculations on a sphere or in the latitude/longitude plane.
    * LongitudeLatitudeStraightLinesGeometry - Utility functions for straight lines in the Longitude/Latitude coordinate system.
    * UnitSphereGeometry - Utility functions for points on the unit sphere
* **GeoRegions (folder).** Classes for representing regions on the globe, including multiple connected components and holes, and for doing union/intersection operations on them.
    * **Topology (folder)** 
        * Edge - An edge between two nodes in the topology graph of a region.
        * EdgeOnLine - The position of a topology Edge on a PolygonLine.
        * Node - A node in the region topology structure.
        * PolygonLine - A line from a region polygon or one of its gluing lines, with information on its edges and nodes in the topology.
    * ClosedGeoPolygon - A closed polygon on the Earth surface defining a region limited by a polygon.
    * GeoRegion - A region on the Earth, defined by a set of disjoint connected region components
    * GeoRegionComponent - A connected region on the Earth defined by one positively oriented outer loop, and some possible negatively oriented inner loops representing holes.
    * GluedGeoPolygons - An object holding a collection of closed polygons defining a region, together with some gluing edges between the polygons.
    * RegionOperations - Utility methods for doing set operations on regions.
* **GUI (folder).** 
    * ColorPalette - a mapping from objects to colours and automatically assigns a contrasting colour to new objects
* **IO (folder).** IO related tools
    * ConcatenatedStream - Presents two streams as one.
    * DataContractUtils - Deserialize data contracts.
    * DirectoryFinder - Find directories above the current in the file hieararchy.
    * FileUtilities - Some file handling and IO utilities.
    * NpyParser - Can parse single or multidimensional arrays saved by the NumPy Python package into C# arrays.
    * StreamUtilities - Stream handling utilities.
    * XmlParseHelper - Extension methods for XML parsing. Using the Tag... and Require... methods allow you to verify that all XML was parsed.
* **Logging (folder).** A framework for application logging.
    * GeneralLogger - A class providing methods for logging events or messages during application execution.
    * GeneralLoggerLogView - Provides methods for viewing logs produced by a GeneralLogger.
    * GeneralLoggerParser - Parses the result of a GeneralLogger.
    * StackableLogger - ???
    * ThreadSafeLogger - Wraps a general logger into a thread safe class.
* **Misc (folder).** A collection of classes which does not fit into the other categories.
    * BinarySearch - Extension methods implementing various binary searches.
    * ConversionExtensions - Extension methods for converting various types to or from string.
    * DateTimeExtensions - DateTime related extension methods.
    * GenericObjectConverter - ???
    * HungarianAlgorithm - A generic implementation of the hungarian algorithm.
    * ImitatorFactory - Provides methods for imitating other classes.
    * LinqExtensions - Provides some LINQ style extension methods to IEnumerable.
    * RandomShuffle - Shuffles the order of elements in IEnumerable or List randomly.
    * RandomUtils - Contains a couple random number generation classes.
    * RunComparer - A class which can compare 2 runs of a program.
    * ShortestPathCalculator - A generic class for setting up graphs and calculating shortest paths on the graph using multiple algorithms.
    * StringExtensions - Various extensions of string.
    * StringPairManipulation - Provides string manipulation, which can be used to parse data or code where elements or scoped are defined within brackets.
    * TableFormatter - Formats tables of strings
    * TimeModel - Models time in discrete steps.
    * UtcDateTime - A variation on DateTime that is explicitly UTC and minimizes the opportunities for time zone errors.
    * VariationTester - Helper for testing numerous input variations in service API calls.
* **Numerics (folder).** Numeric related classes.
    * Distribution - Generates random numbers in various statistical distributions.
    * DoubleWithError - A double which tracks the numeric error.
    * Interval - Generic implementation of an interval and methods to manipulate them.
    * LeastSquare - Functions for least squares regression for linear and exponentially decaying functions
    * NumericExtensions - Various numeric extension methods.
    * NumericZero - Container class for a function that numerically finds the zero of functions
    * RouletteWheel - A simple roulette wheel class, that you can spin to get a random element based on the probability of selecting each element.
    * TimeInterval - Implements time intervals.
    * TimeSpanDistribution - Produces random TimeSpan based on various distributions.
* **PieceWiseFunctions (folder).** Some implementations of piece wise functions.
    * ContinousPiecewiseLinearFunction - An implementation of a continuous piecewise linear function which aims to be efficient at calculating function values or inverse function values.
    * PiecewiseConstFunction - A piecewise constant function that takes integer values.
    * PiecewiseConstFunctionDouble - A piecewise constant function that takes double Y-values and integer X-values.
    * PiecewiseLinearFunction - A class representing simple piecewise linear function of one variable, Y(X).
    * StepwiseProfile - A class that represents a piecewise constant function, where data points mark the transition from one function value to the next.
* **SpatialGraph (folder).** Classes for a graph where nodes and edges have a geographic location on the globe.
    * GraphConstructor - Utility class for reading/constructing spatial graphs from file.
    * SpatialEdge - An non-directional edge in a spatial graph.
    * SpatialGraph - A topological graph with spatial (geographical and geometrical) properties
    * SpatialGraphDTO - A data structure used for serializing SpatialGraphs
    * SpatialIndex - A spatial index which organizes the collection of SpatialGraphNode's in a Graph spatially.
    * SpatialNode - A node in a spatial graph.
    * SpatialPath - A path in a spatial graph, given as an alternating sequence of nodes and edges.

## Contributing

If you want to contribute to this project check out the guidelines in [CONTRIBUTING.md](CONTRIBUTING.md).

## License

This software is distributed under the [LGPL 3.0 only](Licenses/lgpl-3.0.txt) license.

## Breaking changes from version 1.x

* The Graph classes have been renamed ShortestPathCalculator and GenericShortestPathCalculator. There is no longer a specialized version for double cost but rather a generic version which can use any numeric type.
* Licensing tools have been moved into a separate internal repo.
* Removed Generic2DArray, GooglePolyLineConverter, ArgMin And ArgMax extensions (use .NET MinBy, MaxBy instead), FlagEnumEditor, AbortableThreadPool and HiPerfTimer.
