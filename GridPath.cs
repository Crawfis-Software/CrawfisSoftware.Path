using CrawfisSoftware.Collections.Graph;
using System.Collections;
using System.Collections.Generic;

namespace CrawfisSoftware.Collections.Path
{
    /// <summary>
    /// Data structure to hold a path or loop through a Grid
    /// </summary>
    /// <typeparam name="TNodeValue">The underlying node label type of the grid. Not used.</typeparam>
    /// <typeparam name="TEdgeValue">The underlying edge label type of the grid. Not used.</typeparam>
    /// <seealso cref="Grid{N, E}"/>
    public class GridPath<TNodeValue, TEdgeValue> : IPath<int, float>
    {
        private readonly Grid<TNodeValue, TEdgeValue> _grid;
        private readonly List<int> _positions;
        private readonly float _edgeLength;

        /// <inheritdoc/>
        public bool IsClosed { get; private set; } = false;
        /// <inheritdoc/>
        public int PositionCount { get { return Count + (IsClosed ? 1 : 0); } }
        /// <inheritdoc/>
        public float PathLength { get { return _edgeLength; } }

        /// <summary>
        /// Get the underlying grid that this path is defined on.
        /// </summary>
        public Grid<TNodeValue,TEdgeValue> Grid { get { return _grid; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="grid">The underlying grid.</param>
        /// <param name="positions">A sequence of grid indices that the path goes through (in order).</param>
        /// <param name="pathLength">A value representing the length of a path perhaps with edge costs.</param>
        /// <param name="isClosed">If true, the path forms a loop and the last position is connected to the first position automatically.</param>
        public GridPath(Grid<TNodeValue,TEdgeValue> grid, IEnumerable<int> positions, float pathLength = -1, bool isClosed = false)
        {
            _grid = grid;
            _positions = new List<int>(positions);
            IsClosed = isClosed;
            _edgeLength = (pathLength < 0) ? this.Count : pathLength;
        }

        #region IReadOnlyList
        /// <inheritdoc/>
        public int Count { get { return _positions.Count; } }

        /// <inheritdoc/>
        public int this[int index]
        {
            get
            {
                return _positions[index];
            }
        }

        /// <inheritdoc/>
        public IEnumerator<int> GetEnumerator()
        {
            return _positions.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}