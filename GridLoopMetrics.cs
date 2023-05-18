using System.Collections.Generic;
using System.Text;

namespace CrawfisSoftware.Collections.Path
{
    /// <summary>
    /// Data structure to hold loop metrics on a grid.
    /// </summary>
    public class GridLoopMetrics
    {
        private int _currentStartingCell;
        /// <summary>
        /// A list of the grid cell indices that the path passes through.
        /// </summary>
        public List<int> PathGridCellIndices;
        /// <summary>
        /// The underlying grid width in terms of the number of columns.
        /// </summary>
        public int GridWidth;
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
        public string TurtlePath = "";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pathIndices">The path defined as a sequence of grid cells on a grid with the given width.</param>
        /// <param name="gridWidth">The width of the grid.</param>
        /// <param name="startingCellPathIndex">An index into the pathIndices that is the desired "starting" point for the loop. Useful for the string based representation.</param>
        public GridLoopMetrics(List<int> pathIndices, int gridWidth, int startingCellPathIndex = 0)
        {
            this.PathGridCellIndices = pathIndices;
            this.GridWidth = gridWidth;
            StartingCell = (pathIndices[0] % gridWidth, pathIndices[0] / gridWidth);
            EndingCell = (pathIndices[pathIndices.Count - 1] % gridWidth, pathIndices[pathIndices.Count - 1] / gridWidth);
            PathLength = pathIndices.Count + 1;
            SetStartingCell(startingCellPathIndex);
        }

        /// <summary>
        /// Rotate the loop (Turtle string) to start at the specified index in the original GridPath.
        /// </summary>
        /// <param name="index">The index into the GridPath list of grid cells.</param>
        public void SetLoopStartingPathIndex(int index)
        {
            SetStartingCell(index);
            _currentStartingCell = index;
        }

        private void SetStartingCell(int newStartingPathIndex)
        {
            MaximumConsecutiveTurns = 0;
            MaximumConsecutiveStraights = 0;
            StringBuilder path = new StringBuilder(PathGridCellIndices.Count);
            int numberOfTurns = 0;
            int numberOfStraights = 0;
            var loopCellIndices = new List<int>(PathGridCellIndices.Count + 2);
            loopCellIndices.Add(PathGridCellIndices[PathGridCellIndices.Count-1]);
            foreach (int index in PathGridCellIndices)
            {
                loopCellIndices.Add(index);
            }
            loopCellIndices.Add(PathGridCellIndices[0]);
            for (int i = 1; i < loopCellIndices.Count - 1; i++)
            {
                string token = GridPathMetrics<int,int>.DetermineCellDirectionAt(loopCellIndices, GridWidth, i, false);
                path.Append(token);
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
            TurtlePath = path.ToString();
        }
    }
}