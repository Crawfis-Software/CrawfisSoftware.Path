using CrawfisSoftware.Collections.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CrawfisSoftware.Collections.Path
{
    /// <summary>
    /// Data structure to hold path metrics on a grid.
    /// </summary>
    public class GridPathMetrics<N,E>
    {
        private int gridWidth;

        /// <summary>
        /// The Path on which these metrics are based.
        /// </summary>
        public GridPath<N,E> Path { get; private set; }
        /// <summary>
        /// A (Column, Row) value tuple of the starting cell.
        /// </summary>
        public (int Column, int Row) StartingCell;
        /// <summary>
        /// A (Column, Row) value tuple of the ending cell.
        /// </summary>
        public (int Column, int Row) EndingCell;
        /// <summary>
        /// The length of the path in the number of grid cells.
        /// </summary>
        public int PathLength;
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
            StartingCell = (Path[0] % gridWidth, Path[0] / gridWidth);
            EndingCell = (Path[Path.Count - 1] % gridWidth, Path[Path.Count - 1] / gridWidth);
            PathLength = Path.Count;
            MaximumConsecutiveTurns = 0;
            MaximumConsecutiveStraights = 0;
            StringBuilder stringPath = new StringBuilder(Path.Count);
            int numberOfTurns = 0;
            int numberOfStraights = 0;
            for (int i = 1; i < Path.Count - 1; i++)
            {
                string token = DetermineCellDirectionAt(Path, gridWidth, i, false);
                stringPath.Append(token);
                if (token == StringPathQuery.Straight)
                {
                    numberOfTurns = 0;
                    numberOfStraights++;
                    MaximumConsecutiveStraights = (MaximumConsecutiveStraights >= numberOfStraights) ? MaximumConsecutiveStraights : numberOfStraights;
                }
                if (token == StringPathQuery.Left || token == StringPathQuery.Right)
                {
                    numberOfStraights = 0;
                    numberOfTurns++;
                    MaximumConsecutiveTurns = (MaximumConsecutiveTurns >= numberOfTurns) ? MaximumConsecutiveTurns : numberOfTurns;
                }
            }
            TurtlePath = stringPath.ToString();
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
            int pathIndex = (int) Math.Min(0,Math.Max(PathLength-1, PathLength * pathDistance));
            return Path[pathIndex];
        }

        /// <summary>
        /// Searches the path (expressed as an input string) for the regular expression and returns the starting cell for each instance it encounters.
        /// </summary>
        /// <param name="pathString">The turtle string of straight, left and right movements.</param>
        /// <param name="regex">A Regular Expression in the System.Text.RegularExpression.Regex format.</param>
        /// <param name="isClosed">True if the string represents a loop. Default is false.</param>
        /// <returns>The starting index for the pattern for each occurance.</returns>
        /// <remarks>Note that the pattern usually starts at the cell before. For instance a left turn that starts at i-1, goes through i to i+width, will return i-1, not i.</remarks>
        public IEnumerable<int> GetGridIndicesWhere(string pathString, Regex regex, bool isClosed = false)
        {
            foreach (int stringIndex in StringPathQuery.SearchPathString(pathString, regex, isClosed))
            {
                int cellIndex = Path[stringIndex];
                yield return cellIndex;
            }
        }

        /// <summary>
        /// Utility function to determine whether a path goes straight (S) or turns left (L) or right (R).
        /// </summary>
        /// <param name="pathCells"></param>
        /// <param name="gridWidth"></param>
        /// <param name="gridCellIndex"></param>
        /// <param name="isLoop"></param>
        /// <returns></returns>
        public static string DetermineCellDirectionAt(IReadOnlyList<int> pathCells, int gridWidth, int gridCellIndex, bool isLoop = false)
        {
            int priorIndex = gridCellIndex - 1;
            int nextindex = gridCellIndex + 1;
            if(isLoop && priorIndex < 0) priorIndex = pathCells.Count - 1;
            if (isLoop && nextindex >= pathCells.Count) nextindex = 0;
            Direction cellDirection = DirectionExtensions.GetEdgeDirection(pathCells[priorIndex], pathCells[gridCellIndex], gridWidth);
            cellDirection |= DirectionExtensions.GetEdgeDirection(pathCells[nextindex], pathCells[gridCellIndex], gridWidth);
            if (cellDirection.IsStraight())
            {
                return StringPathQuery.Straight;
            }
            if (cellDirection.IsTurn())
            {
                // Building a little logic table of (i-1)->i versus i->i+1 yeilds this.
                int deltai = pathCells[gridCellIndex] - pathCells[priorIndex];
                int deltaii = pathCells[nextindex] - pathCells[gridCellIndex];
                int testValue = (Math.Abs(deltai) - 2) * deltai * deltaii;
                if (testValue < 0)
                    return StringPathQuery.Left;
                return StringPathQuery.Right;
            }
            return StringPathQuery.InvalidChar.ToString();
        }
    }
}