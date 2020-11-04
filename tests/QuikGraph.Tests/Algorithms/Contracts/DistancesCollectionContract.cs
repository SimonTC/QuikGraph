
using System;
using JetBrains.Annotations;
using NUnit.Framework;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.ShortestPath;
using QuikGraph.Algorithms.TSP;
using QuikGraph.Tests.Algorithms.ShortestPath;
using QuikGraph.Tests.Algorithms.TSP;

namespace QuikGraph.Tests.Algorithms.Contracts
{
    /// <summary>
    /// Contract tests for <see cref="IDistancesCollection{TVertex}"/>.
    /// </summary>
    [TestFixtureSource(typeof(AlgorithmsProvider), nameof(AlgorithmsProvider.DistanceCollectors))]
    public class DistancesCollectionContract
    {
        [NotNull]
        private readonly Type _testedAlgorithm;

        /// <summary/>
        public DistancesCollectionContract([NotNull] Type algorithmToTest)
        {
            _testedAlgorithm = algorithmToTest;
        }

        [Test]
        public void NoDistanceFound_WhenVertexDoesNotExistInGraph()
        {
            var scenario = new ContractScenario
            {
                EdgesInGraph = new[] { new Edge<int>(1, 2) },
                AccessibleVerticesFromRoot = new[] { 2 },
                Root = 1,
                DoComputation = true
            };

            IDistancesCollection<int> algorithm = CreateAlgorithmAndMaybeDoComputation(scenario);

            var distanceFound = algorithm.TryGetDistance(3, out _);
            Assert.False(distanceFound, "No distance should have been found since the vertex does not exist.");
        }

        [Test]
        public void ExceptionThrown_WhenAlgorithmHasNotYetBeenComputed()
        {
            var scenario = new ContractScenario
            {
                EdgesInGraph = new[] { new Edge<int>(1, 2) },
                SingleVerticesInGraph = new int[0],
                AccessibleVerticesFromRoot = new[] { 2 },
                Root = 1,
                DoComputation = false
            };

            IDistancesCollection<int> algorithm = CreateAlgorithmAndMaybeDoComputation(scenario);

            Assert.Throws<InvalidOperationException>( () => algorithm.TryGetDistance(2, out _));
        }

        [Pure]
        [NotNull]
        private IDistancesCollection<int> CreateAlgorithmAndMaybeDoComputation(
            [NotNull] ContractScenario scenario)
        {
            var instantiateAlgorithm = GetAlgorithmFactory();
            return instantiateAlgorithm(scenario);
        }

        [Pure]
        [NotNull]
        private Func<ContractScenario, IDistancesCollection<int>> GetAlgorithmFactory()
        {
            return _testedAlgorithm switch
            {
                _ when _testedAlgorithm == typeof(AStarShortestPathAlgorithm<,>) =>
                    AStartShortestPathAlgorithmTests.CreateAlgorithmAndMaybeDoComputation,

                _ when _testedAlgorithm == typeof(BellmanFordShortestPathAlgorithm<,>) =>
                    BellmanFordShortestPathAlgorithmTests.CreateAlgorithmAndMaybeDoComputation,

                _ when _testedAlgorithm == typeof(DagShortestPathAlgorithm<,>) =>
                    DagShortestPathAlgorithmTests.CreateAlgorithmAndMaybeDoComputation,

                _ when _testedAlgorithm == typeof(DijkstraShortestPathAlgorithm<,>) =>
                    DijkstraShortestPathAlgorithmTests.CreateAlgorithmAndMaybeDoComputation,

                _ when _testedAlgorithm == typeof(TSP<,,>) =>
                    TSPTests.CreateAlgorithmAndMaybeDoComputation,

                _ when _testedAlgorithm == typeof(UndirectedDijkstraShortestPathAlgorithm<,>) =>
                    UndirectedDijkstraShortestPathAlgorithmTests.CreateAlgorithmAndMaybeDoComputation,

                _ => throw new AssertionException($"No test constructor known for {_testedAlgorithm}.")
            };
        }
    }
}