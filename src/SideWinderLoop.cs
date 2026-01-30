using CrawfisSoftware.Collections.Graph;

using System;
using System.Collections.Generic;

namespace CrawfisSoftware.Path
{
    /// <summary>
    /// Stitches a closed loop on a grid from two explicit per-row column sequences.
    /// </summary>
    /// <remarks>
    /// Callers provide two column sequences of equal length:
    /// <list type="bullet">
    /// <item><description><c>leftColumns[row]</c> is the column used by the left side at that row.</description></item>
    /// <item><description><c>rightColumns[row]</c> is the column used by the right side at that row.</description></item>
    /// </list>
    ///
    /// This type does not generate the column sequences; use <see cref="SideWinderLoopFactory"/>
    /// if you need helpers for producing them.
    /// </remarks>
    public class SideWinderLoop<N, E>
    {
        private readonly Grid<N, E> _grid;
        private readonly int _width;

        /// <summary>
        /// Creates a new loop stitcher for the given <paramref name="grid"/>.
        /// </summary>
        /// <param name="grid">The grid that the resulting <see cref="GridPath{N,E}"/> will reference.</param>
        public SideWinderLoop(Grid<N,E> grid)
        {
            _grid = grid;
            _width = grid.Width;
        }

        /// <summary>
        /// Creates a closed <see cref="GridPath{N,E}"/> by stitching the provided left/right column sequences.
        /// </summary>
        /// <param name="leftColumns">One column index per row (0-based) for the left side.</param>
        /// <param name="rightColumns">One column index per row (0-based) for the right side.</param>
        /// <returns>A closed <see cref="GridPath{N,E}"/> representing the stitched loop.</returns>
        public GridPath<N,E> CreateLoop(IReadOnlyList<int> leftColumns, IReadOnlyList<int> rightColumns)
        {
            List<int> gridPositions = StitchLoopFromTwoPaths(_width, leftColumns, rightColumns);
            return new GridPath<N, E>(_grid, gridPositions, -1, true);
        }

        private static List<int> StitchLoopFromTwoPaths(int width, IReadOnlyList<int> leftColumns, IReadOnlyList<int> rightColumns)
        {
            if (leftColumns.Count != rightColumns.Count)
            {
                throw new ArgumentException("Left/right column sequences must have the same row count.");
            }

            int rowCount = leftColumns.Count;
            if (rowCount == 0)
            {
                return new List<int>();
            }

            int lastRow = rowCount - 1;

            List<int> leftPath = SpanUtilities.StitchPathFromColumns(width, leftColumns);
            var indices = new List<int>(capacity: leftPath.Count + rowCount * 2);
            indices.AddRange(leftPath);

            SpanUtilities.AddIndices(indices, SpanUtilities.HorizontalSpan(width, lastRow, leftColumns[lastRow], rightColumns[lastRow]));

            List<int> rightPath = SpanUtilities.StitchPathFromColumns(width, rightColumns);
            rightPath.Reverse();

            if (rightPath.Count > 0)
            {
                rightPath.RemoveAt(0);
            }
            indices.AddRange(rightPath);

            SpanUtilities.AddIndices(indices, SpanUtilities.HorizontalSpan(width, 0, rightColumns[0], leftColumns[0]));
            return indices;
        }

    }
}