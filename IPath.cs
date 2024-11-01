using System.Collections.Generic;

namespace CrawfisSoftware.Collections.Path
{
    /// <summary>
    /// Defines a path including positions, a path length attribute and a flag indicating whether it is a closed loop or not.
    /// </summary>
    /// <typeparam name="TPosition">The type of the position (e.g., Vector3, (i,j)-tuple, int, etc.)</typeparam>
    /// <typeparam name="TEdgeValue">The type of the "distance" (or time) of the path.</typeparam>
    public interface IPath<TPosition, TEdgeValue> : IReadOnlyList<TPosition>
    {
        /// <summary>
        /// If true, the path forms a loop connecting the last position to the first.
        /// </summary>
        public bool IsClosed { get; }
        /// <summary>
        /// The number of positions that are defined on this path. If it is a loop then the count is one more than the number of positions.
        /// </summary>
        public int PositionCount { get; }
        /// <summary>
        /// The path length computed by some measure.
        /// </summary>
        public TEdgeValue PathLength { get; }
    }
}