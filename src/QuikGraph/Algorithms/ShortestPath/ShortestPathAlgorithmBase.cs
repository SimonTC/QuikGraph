using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using QuikGraph.Algorithms.Services;

namespace QuikGraph.Algorithms.ShortestPath
{
    /// <summary>
    /// Base class for all shortest path finder algorithms.
    /// </summary>
    /// <typeparam name="TVertex">Vertex type.</typeparam>
    /// <typeparam name="TEdge">Edge type.</typeparam>
    /// <typeparam name="TGraph">Graph type.</typeparam>
    public abstract class ShortestPathAlgorithmBase<TVertex, TEdge, TGraph>
        : RootedAlgorithmBase<TVertex, TGraph>
        , IVertexColorizerAlgorithm<TVertex>
        , ITreeBuilderAlgorithm<TVertex, TEdge>
        , IDistancesCollection<TVertex>
        where TEdge : IEdge<TVertex>
        where TGraph : IVertexSet<TVertex>
    {
        /// <summary>
        /// Backing field
        /// </summary>
        protected IDictionary<TVertex, double> _distances;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortestPathAlgorithmBase{TVertex,TEdge,TGraph}"/> class.
        /// </summary>
        /// <param name="host">Host to use if set, otherwise use this reference.</param>
        /// <param name="visitedGraph">Graph to visit.</param>
        /// <param name="edgeWeights">Function that computes the weight for a given edge.</param>
        protected ShortestPathAlgorithmBase(
            [CanBeNull] IAlgorithmComponent host,
            [NotNull] TGraph visitedGraph,
            [NotNull] Func<TEdge, double> edgeWeights)
            : this(host, visitedGraph, edgeWeights, DistanceRelaxers.ShortestDistance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortestPathAlgorithmBase{TVertex,TEdge,TGraph}"/> class.
        /// </summary>
        /// <param name="host">Host to use if set, otherwise use this reference.</param>
        /// <param name="visitedGraph">Graph to visit.</param>
        /// <param name="edgeWeights">Function that computes the weight for a given edge.</param>
        /// <param name="distanceRelaxer">Distance relaxer.</param>
        protected ShortestPathAlgorithmBase(
            [CanBeNull] IAlgorithmComponent host,
            [NotNull] TGraph visitedGraph,
            [NotNull] Func<TEdge, double> edgeWeights,
            [NotNull] IDistanceRelaxer distanceRelaxer)
            : base(host, visitedGraph)
        {
            Weights = edgeWeights ?? throw new ArgumentNullException(nameof(edgeWeights));
            DistanceRelaxer = distanceRelaxer ?? throw new ArgumentNullException(nameof(distanceRelaxer));
        }

        /// <inheritdoc />
        public bool TryGetDistance(TVertex vertex, out double distance)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (_distances is null)
                throw new InvalidOperationException("Run the algorithm before.");

            return _distances.TryGetValue(vertex, out distance);
        }

        /// <inheritdoc />
        public double GetDistance(TVertex vertex)
        {
            bool keyFound = TryGetDistance(vertex, out double distance);
            if (! keyFound)
                throw new KeyNotFoundException($"No distance for vertex {vertex} recorded");
            return distance;
        }

        /// <inheritdoc />
        public void SetDistance(TVertex vertex, double distance)
        {
            _distances[vertex] = distance;
        }

        /// <summary>
        /// Vertices distances.
        /// </summary>
        public IDictionary<TVertex, double> GetDistances()
        {
            return _distances;
        }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<TVertex, double>> GetKnownDistances() => _distances.Select(pair => pair);

        /// <summary>
        /// Gets the function that gives access to distances from a vertex.
        /// </summary>
        [Pure]
        [NotNull]
        protected Func<TVertex, double> DistancesIndexGetter()
        {
            return AlgorithmExtensions.GetIndexer(_distances);
        }

        /// <summary>
        /// Function that given an edge return the weight of this edge.
        /// </summary>
        [NotNull]
        public Func<TEdge, double> Weights { get; }

        /// <summary>
        /// Distance relaxer.
        /// </summary>
        [NotNull]
        public IDistanceRelaxer DistanceRelaxer { get; }

        #region AlgorithmBase<TGraph>

        /// <inheritdoc />
        protected override void Initialize()
        {
            base.Initialize();

            VerticesColors = new Dictionary<TVertex, GraphColor>(VisitedGraph.VertexCount);
            _distances = new Dictionary<TVertex, double>(VisitedGraph.VertexCount);
        }

        #endregion

        /// <summary>
        /// Stores vertices associated to their colors (treatment state).
        /// </summary>
        public IDictionary<TVertex, GraphColor> VerticesColors { get; private set; }

        #region IVertexColorizerAlgorithm<TVertex>

        /// <inheritdoc />
        public GraphColor GetVertexColor(TVertex vertex)
        {
            if (VerticesColors.TryGetValue(vertex, out GraphColor color))
                return color;
            throw new VertexNotFoundException();
        }

        #endregion

        /// <summary>
        /// Fired when the distance label for the target vertex is decreased.
        /// The edge that participated in the last relaxation for vertex v is
        /// an edge in the shortest paths tree.
        /// </summary>
        public event EdgeAction<TVertex, TEdge> TreeEdge;

        /// <summary>
        /// Called on each <see cref="TreeEdge"/> event.
        /// </summary>
        /// <param name="edge">Concerned edge.</param>
        protected virtual void OnTreeEdge([NotNull] TEdge edge)
        {
            Debug.Assert(edge != null);

            TreeEdge?.Invoke(edge);
        }

        /// <summary>
        /// Runs the relaxation algorithm on the given <paramref name="edge"/>.
        /// </summary>
        /// <param name="edge">Edge to relax.</param>
        /// <returns>True if relaxation decreased the target vertex distance, false otherwise.</returns>
        protected bool Relax([NotNull] TEdge edge)
        {
            Debug.Assert(edge != null);

            TVertex source = edge.Source;
            TVertex target = edge.Target;
            double du = GetDistance(source);
            double dv = GetDistance(target);
            double we = Weights(edge);

            double duwe = DistanceRelaxer.Combine(du, we);
            if (DistanceRelaxer.Compare(duwe, dv) < 0)
            {
                _distances[target] = duwe;
                return true;
            }

            return false;
        }
    }
}