using System.Collections.Generic;
using JetBrains.Annotations;

namespace QuikGraph.Algorithms
{
    /// <summary>
    /// Represents an object that stores information about distances between vertices.
    /// </summary>
    /// <typeparam name="TVertex">Vertex type.</typeparam>
    public interface IDistancesCollection<TVertex>
    {
        /// <summary>
        /// Tries to get the distance associated to the given <paramref name="vertex"/>.
        /// </summary>
        /// <param name="vertex">The vertex.</param>
        /// <param name="distance">Associated distance.</param>
        /// <returns>True if the distance was found, false otherwise.</returns>
        bool TryGetDistance([NotNull] TVertex vertex, out double distance);

        /// <summary>
        /// Vertices distances.
        /// </summary>
        IDictionary<TVertex, double> Distances { get; }
    }
}