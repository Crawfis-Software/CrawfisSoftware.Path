using System;
using System.Collections.Generic;

namespace CrawfisSoftware.Path
{
    /// <summary>
    /// Utility helpers for creating and composing simple 1D index spans that represent
    /// horizontal and vertical movement on a 2D grid.
    /// </summary>
    /// <remarks>
    /// Grid cells are represented using a linear index computed as:
    /// <c>index = column + width * row</c>.
    ///
    /// The span methods in this class yield the intermediate indices encountered while moving
    /// from a start cell to a destination cell along a straight line. The starting cell index
    /// is not yielded; the first yielded value is the adjacent cell in the direction of travel.
    /// The destination cell index is yielded.
    /// </remarks>
    public static class SpanUtilities
    {
        /// <summary>
        /// Stitches a continuous path from a per-row column sequence.
        /// </summary>
        /// <remarks>
        /// This is a convenience API for callers that only need the underlying list of linear indices.
        /// The generated indices follow the same rules as <see cref="HorizontalSpan"/> and
        /// <see cref="VerticalSpan"/> (the starting cell index is included once at the beginning,
        /// then each connector/span yields intermediate indices and includes the destination).
        /// </remarks>
        /// <param name="width">The grid width (number of columns).</param>
        /// <param name="columns">One column index per row (0-based).</param>
        /// <returns>A list of linear indices representing the stitched path.</returns>
        internal static List<int> StitchPathFromColumns(int width, IReadOnlyList<int> columns)
        {
            return StitchPathFromColumns(width, 0, columns);
        }

        internal static List<int> StitchPathFromColumns(int width, int startingRow, IReadOnlyList<int> columns)
        {
            if (columns.Count == 0)
            {
                return new List<int>();
            }

            var indices = new List<int>(capacity: columns.Count * 2);
            indices.Add(columns[0] + width * startingRow);

            for (int i = 1; i < columns.Count; i++)
            {
                int previousRow = startingRow + i - 1;
                int currentRow = startingRow + i;
                int previousColumn = columns[i - 1];
                int currentColumn = columns[i];

                AddIndices(indices, VerticalSpan(width, previousColumn, previousRow, currentRow));
                AddIndices(indices, HorizontalSpan(width, currentRow, previousColumn, currentColumn));
            }

            return indices;
        }

        /// <summary>
        /// Enumerates the indices encountered when moving horizontally within a single row.
        /// </summary>
        /// <param name="width">The grid width (number of columns).</param>
        /// <param name="currentRow">The row of the start cell (0-based).</param>
        /// <param name="currentColumn">The column of the start cell (0-based).</param>
        /// <param name="newColumn">The column of the destination cell (0-based).</param>
        /// <returns>
        /// The sequence of linear indices for each intermediate cell, excluding the start cell
        /// and including the destination cell.
        /// </returns>
        public static IEnumerable<int> HorizontalSpan(int width, int currentRow, int currentColumn, int newColumn)
        {
            int index = currentColumn + width * currentRow;
            int numberOfCells = System.Math.Abs(currentColumn - newColumn);
            int step = (currentColumn < newColumn) ? 1 : -1;
            for (int i = 0; i < numberOfCells; i++)
            {
                index += step;
                yield return index;
            }
        }

        /// <summary>
        /// Enumerates the indices encountered when moving vertically within a single column.
        /// </summary>
        /// <param name="width">The grid width (number of columns).</param>
        /// <param name="currentColumn">The column of the start cell (0-based).</param>
        /// <param name="currentRow">The row of the start cell (0-based).</param>
        /// <param name="newRow">The row of the destination cell (0-based).</param>
        /// <returns>
        /// The sequence of linear indices for each intermediate cell, excluding the start cell
        /// and including the destination cell.
        /// </returns>
        public static IEnumerable<int> VerticalSpan(int width, int currentColumn, int currentRow, int newRow)
        {
            int index = currentColumn + width * currentRow;
            int numberOfCells = System.Math.Abs(currentRow - newRow);
            int step = (currentRow < newRow) ? width : -width;
            for (int i = 0; i < numberOfCells; i++)
            {
                index += step;
                yield return index;
            }
        }

        /// <summary>
        /// Appends all indices from <paramref name="span"/> to <paramref name="indices"/>.
        /// </summary>
        /// <param name="indices">The list to append to.</param>
        /// <param name="span">A span enumeration (typically from <see cref="HorizontalSpan"/> or <see cref="VerticalSpan"/>).</param>
        public static void AddIndices(List<int> indices, IEnumerable<int> span)
        {
            foreach (int index in span)
            {
                indices.Add(index);
            }
        }
    }
}