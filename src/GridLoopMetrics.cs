using System.Collections.Generic;

namespace CrawfisSoftware.Collections.Path
{
    /// <summary>
    /// Data structure to hold loop metrics on a grid.
    /// </summary>
    public class GridLoopMetrics<N, E> : GridPathMetrics<N, E>
    {
        private readonly int gridWidth; // for convenience.
        private int _currentStartingCell;
        private GridPath<N, E> _originalPath;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gridPath">A GridPath.</param>
        /// <param name="startingCellPathIndex">An index into the pathIndices that is the desired "starting" point for the loop. Useful for the string based representation.</param>
        public GridLoopMetrics(GridPath<N, E> gridPath, int startingCellPathIndex = 0) : base(gridPath)
        {
            gridWidth = gridPath.Grid.Width;
            _originalPath = gridPath;
            SetStartingCell(startingCellPathIndex);
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
            Path = new GridPath<N, E>(_originalPath.Grid, loopCellIndices, _originalPath.PathLength, true);

            TurtlePath = PathQuery.DetermineTurtleString(Path);
        }
    }
}