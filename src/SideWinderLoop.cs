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
        /// <param name="startingRow">The starting row for the first column entry (0-based).</param>
        public SideWinderLoop(Grid<N,E> grid, int startingRow = 0)
        {
            _grid = grid;
            _width = grid.Width;
        }

        /// <summary>
        /// Creates a closed <see cref="GridPath{N,E}"/> by stitching the provided left/right column sequences.
        /// </summary>
        /// <param name="leftColumns">One column index per row (0-based) for the left side.</param>
        /// <param name="rightColumns">One column index per row (0-based) for the right side.</param>
        /// <param name="startingRow">The starting row for the first column entry (0-based).</param>
        /// <returns>A closed <see cref="GridPath{N,E}"/> representing the stitched loop.</returns>
        public GridPath<N,E> CreateLoop(IReadOnlyList<int> leftColumns, IReadOnlyList<int> rightColumns, int startingRow = 0)
        {
            List<int> gridPositions = StitchLoopFromTwoPaths(_width, startingRow, leftColumns, rightColumns);
            return new GridPath<N, E>(_grid, gridPositions, -1, true);
        }

        private static List<int> StitchLoopFromTwoPaths(int width, int startingRow, IReadOnlyList<int> leftColumns, IReadOnlyList<int> rightColumns)
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
            int topRow = startingRow;
            int bottomRow = startingRow + lastRow;

            List<int> indices = SpanUtilities.StitchPathFromColumns(width, startingRow, leftColumns);

            if (leftColumns[lastRow] != rightColumns[lastRow])
            {
                SpanUtilities.AddIndices(indices, SpanUtilities.HorizontalSpan(width, bottomRow, leftColumns[lastRow], rightColumns[lastRow]));
            }

            List<int> rightPath = SpanUtilities.StitchPathFromColumns(width, startingRow, rightColumns);
            for (int i = rightPath.Count - 2; i >= 0; i--)
            {
                indices.Add(rightPath[i]);
            }

            if (rightColumns[0] != leftColumns[0])
            {
                // Add the top connector back to the left side, but avoid adding the left endpoint.
                // The GridPath is marked closed, so the final vertex will connect back to the start.
                foreach (int idx in SpanUtilities.HorizontalSpan(width, topRow, rightColumns[0], leftColumns[0]))
                {
                    if (idx != indices[0])
                    {
                        indices.Add(idx);
                    }
                }
            }
            return indices;
        }

    }
}