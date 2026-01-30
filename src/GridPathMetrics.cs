using CrawfisSoftware.Collections.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CrawfisSoftware.Path
{
    /// <summary>
    /// Data structure to hold path metrics on a grid.
    /// </summary>
    public class GridPathMetrics<N,E>
    {
        private readonly int gridWidth; // for convenience.

        /// <summary>
        /// The number of valid cross sections, defined as the count of distinct grid indices that appear more than once in the path.
        /// Validity with respect to duplicate edges should be checked separately (see tests).
        /// </summary>
        public int CrossSectionCount;

        /// <summary>
        /// The Path on which these metrics are based.
        /// </summary>
        public GridPath<N,E> Path { get; protected set; }
        /// <summary>
        /// A (Column, Row) value tuple of the starting cell.
        /// </summary>
        public (int Column, int Row) StartingCell;
        /// <summary>
        /// A (Column, Row) value tuple of the ending cell.
        /// </summary>
        public (int Column, int Row) EndingCell;
        /// <summary>
        /// The maximum number of consecutive horizontal or vertical straights.
        /// </summary>
        public int MaximumConsecutiveTurns;
        /// <summary>
        /// The maximum number of consecutive turns (left or right).
        /// </summary>
        public int MaximumConsecutiveStraights;
        /// <summary>
        /// A string representing the path movements where S implies go straight, L implies go left, and R implies go right. This can be easily searched for patterns.
        /// </summary>
        /// <remarks>The string path is 2 characters shorter than the path length due to the start and end cells considered as dead-ends.</remarks>
        /// <seealso cref="System.Text.RegularExpressions"/>
        public string TurtlePath;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gridPath">The path to compute metrics on.</param>
        public GridPathMetrics(GridPath<N,E> gridPath)
        {
            gridWidth = gridPath.Grid.Width;
            this.Path = gridPath;

            TurtlePath = PathQuery.DetermineTurtleString<N,E>(Path);
            StartingCell = (Path[0] % gridWidth, Path[0] / gridWidth);
            EndingCell = (Path[Path.Count - 1] % gridWidth, Path[Path.Count - 1] / gridWidth);
            MaximumConsecutiveStraights = StringPathQuery.MaximumConsecutiveStraights(TurtlePath, gridPath.IsClosed);
            MaximumConsecutiveTurns = StringPathQuery.MaximumConsecutiveTurns(TurtlePath, gridPath.IsClosed);

            CrossSectionCount = CountCrossSections(gridPath);
        }

        public static int CountCrossSections(GridPath<N, E> gridPath)
        {
            // Cross section count is the number of distinct vertices that are repeated.
            // For closed loops, GridPath does NOT repeat the starting vertex at the end; it is only implied.
            var counts = new Dictionary<int, int>();
            foreach (int index in gridPath)
            {
                counts.TryGetValue(index, out int current);
                counts[index] = current + 1;
            }

            int duplicates = 0;
            foreach (var kvp in counts)
            {
                if (kvp.Value > 1) duplicates++;
            }
            return duplicates;
        }

        /// <summary>
        /// Given an index into the path list of cells (or string), return the grid indices as a (column, row) tuple.
        /// </summary>
        /// <param name="pathIndex">The index or number of cells along the path.</param>
        /// <returns>The underlying grid indices as a (column, row) tuple.</returns>
        public (int column, int row) GetGridColumnRowTuple(int pathIndex)
        {
            return (Path[pathIndex] % Path.Grid.Width, Path[pathIndex] / Path.Grid.Width);
        }

        /// <summary>
        /// Given an index into the path list of cells (or string), return the grid index for the cell along the path.
        /// </summary>
        /// <param name="pathIndex">The index or number of cells along the path.</param>
        /// <returns>The underlying grid index.</returns>
        public int GetGridIndex(int pathIndex)
        {
            return Path[pathIndex];
        }

        /// <summary>
        /// Given an index into the path list of cells (or string), return the grid index for the cell along the path.
        /// </summary>
        /// <param name="pathDistance">The percentage distance along the path.</param>
        /// <returns>The underlying grid index.</returns>
        public int GetGridIndex(float pathDistance)
        {
            int pathIndex = (int) Math.Min(0,Math.Max(Path.Count-1, Path.Count * pathDistance));
            return Path[pathIndex];
        }
    }
}