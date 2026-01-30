using CrawfisSoftware.Collections.Graph;

using System;
using System.Collections.Generic;

namespace CrawfisSoftware.Path
{
    /// <summary>
    /// Helper methods for producing SideWinder-style paths.
    /// </summary>
    /// <remarks>
    /// This class provides two responsibilities:
    /// <list type="bullet">
    /// <item>
    /// <description>Generate a per-row column sequence (one column index per row).</description>
    /// </item>
    /// <item>
    /// <description>Create a <see cref="GridPath{N,E}"/> by delegating stitching to <see cref="SideWinderPath{N,E}"/>.</description>
    /// </item>
    /// </list>
    ///
    /// Callers can supply a custom-selection function to control how the
    /// selected column changes row-to-row.
    /// </remarks>
    public static class SideWinderPathFactory
    {
        /// <summary>
        /// Generates a per-row column sequence suitable for <see cref="SideWinderPath{N,E}.CreatePath"/>.
        /// </summary>
        /// <param name="width">The grid width (number of columns).</param>
        /// <param name="height">The number of rows to generate (typically the grid height).</param>
        /// <param name="startingColumn">The column to use for the first row (row 0).</param>
        /// <param name="endingColumn">The column to force for the last row (row <c>height - 1</c>).</param>
        /// <param name="random">Optional random source; if omitted a new instance is created.</param>
        /// <param name="pickNextColumn">
        /// Optional function that selects the next row's column from the current row, previous column, and random source.
        /// If omitted, a default SideWinder-style function is used.
        /// </param>
        /// <param name="maxSpanWidth">Maximum horizontal delta used by the default picker.</param>
        /// <returns>An array of length <paramref name="height"/> containing one column index per row.</returns>
        public static IReadOnlyList<int> GenerateColumns(int width, int height, int startingColumn, int endingColumn, System.Random random = null, Func<int, int, System.Random, int> pickNextColumn = null, int maxSpanWidth = 5)
        {
            if (height <= 0)
            {
                return Array.Empty<int>();
            }

            random ??= new System.Random();

            int ClampColumn(int column)
            {
                if (column < 0) return 0;
                if (column >= width) return width - 1;
                return column;
            }

            int DefaultPickNextColumn(int row, int previousColumn, System.Random rng)
            {
                int delta = rng.Next(width) - previousColumn;
                int sign = 1;
                if (delta < 0) sign = -1;
                delta = ((sign * delta) > maxSpanWidth) ? sign * maxSpanWidth : delta;
                return previousColumn + delta;
            }

            pickNextColumn ??= DefaultPickNextColumn;

            var columns = new int[height];
            columns[0] = ClampColumn(startingColumn);

            for (int row = 1; row < height; row++)
            {
                columns[row] = ClampColumn(pickNextColumn(row, columns[row - 1], random));
            }

            columns[height - 1] = ClampColumn(endingColumn);
            return columns;
        }

        /// <summary>
        /// Creates a path by stitching the provided per-row column sequence.
        /// </summary>
        /// <typeparam name="N">Node payload type.</typeparam>
        /// <typeparam name="E">Edge payload type.</typeparam>
        /// <param name="grid">Grid the resulting path will reference.</param>
        /// <param name="startingRow">The starting row for the first column entry (0-based).</param>
        /// <param name="columns">One column index per row (0-based).</param>
        /// <returns>A stitched <see cref="GridPath{N,E}"/>.</returns>
        public static GridPath<N, E> CreatePath<N, E>(Grid<N, E> grid, IReadOnlyList<int> columns, int startingRow = 0)
        {
            var builder = new SideWinderPath<N, E>(grid);
            return builder.CreatePath(columns, startingRow);
        }

        /// <summary>
        /// Creates a path by first generating a per-row column sequence and then stitching it.
        /// </summary>
        /// <typeparam name="N">Node payload type.</typeparam>
        /// <typeparam name="E">Edge payload type.</typeparam>
        /// <param name="grid">Grid the resulting path will reference.</param>
        /// <param name="startingRow">The starting row for the first column entry (0-based).</param>
        /// <param name="startingColumn">The column to use for the first row (row 0).</param>
        /// <param name="endingColumn">The column to force for the last row.</param>
        /// <param name="random">Optional random source; if omitted a new instance is created.</param>
        /// <param name="pickNextColumn">Optional per-row column selection function.</param>
        /// <param name="maxSpanWidth">Maximum horizontal delta used by the default picker.</param>
        /// <returns>A stitched <see cref="GridPath{N,E}"/>.</returns>
        public static GridPath<N, E> CreatePath<N, E>(Grid<N, E> grid, int startingColumn, int endingColumn, int startingRow = 0, System.Random random = null, Func<int, int, System.Random, int> pickNextColumn = null, int maxSpanWidth = 5)
        {
            int height = grid.Height - startingRow;
            IReadOnlyList<int> columns = GenerateColumns(grid.Width, height, startingColumn, endingColumn, random, pickNextColumn, maxSpanWidth);
            return CreatePath(grid, columns, startingRow);
        }
    }
}
