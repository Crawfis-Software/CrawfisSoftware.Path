using System.Collections.Generic;
using System.Text;

namespace CrawfisSoftware.Collections.Path
{
    /// <summary>
    /// Data structure to hold loop metrics on a grid.
    /// </summary>
    public class GridLoopMetrics<N,E>
    {
        private readonly int gridWidth; // for convience.
        private int _currentStartingCell;
        private GridPath<N, E> _originalPath;
        /// <summary>
        /// The Path on which these metrics are based.
        /// </summary>
        public GridPath<N, E> Path { get; private set; }
        /// <summary>
        /// A (Column, Row) value tuple of the starting cell.
        /// </summary>
        public (int Column, int Row) StartingCell;
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
        /// <param name="gridPath">A GridPath.</param>
        /// <param name="startingCellPathIndex">An index into the pathIndices that is the desired "starting" point for the loop. Useful for the string based representation.</param>
        public GridLoopMetrics(GridPath<N, E> gridPath, int startingCellPathIndex = 0)
        {
            gridWidth = gridPath.Grid.Width;
            _originalPath = gridPath;
            Path = _originalPath;
            SetStartingCell(startingCellPathIndex);
            MaximumConsecutiveTurns = StringPathQuery.MaximumConsecutiveStraights(TurtlePath);
            MaximumConsecutiveStraights = StringPathQuery.MaximumConsecutiveTurns(TurtlePath);
        }

        /// <summary>
        /// Rotate the loop (Turtle string) to start at the specified index in the original GridPath.
        /// </summary>
        /// <param name="index">The index into the GridPath list of grid cells.</param>
        public void SetLoopStartingPathIndex(int index)
        {
            SetStartingCell(index);
            StartingCell = (Path[0] % gridWidth, Path[0] / gridWidth);
            _currentStartingCell = index;
        }

        private void SetStartingCell(int newStartingPathIndex)
        {
            var loopCellIndices = new List<int>(_originalPath.Count + 2);
            loopCellIndices.Add(_originalPath[newStartingPathIndex]);
            int index = newStartingPathIndex + 1;
            for (int i = 0; i < _originalPath.Count - 1; i++)
            {
                if (index >= _originalPath.Count) index = 0;
                loopCellIndices.Add(_originalPath[index]);
                index++;
            }
            //if (index >= _originalPath.Count) index = 0;
            //loopCellIndices.Add(_originalPath[index]);
            Path = new GridPath<N, E>(_originalPath.Grid, loopCellIndices, _originalPath.PathLength, true);

            TurtlePath = PathQuery.DetermineTurtleString(Path);
        }
    }
}