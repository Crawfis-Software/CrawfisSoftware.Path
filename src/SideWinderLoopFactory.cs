using CrawfisSoftware.Collections.Graph;

using System;
using System.Collections.Generic;

namespace CrawfisSoftware.Path
{
    /// <summary>
    /// Helper methods for producing SideWinder-style loops.
    /// </summary>
    /// <remarks>
    /// This class provides two responsibilities:
    /// <list type="bullet">
    /// <item>
    /// <description>Generate the left/right per-row column sequences.</description>
    /// </item>
    /// <item>
    /// <description>Create a closed <see cref="GridPath{N,E}"/> by delegating stitching to <see cref="SideWinderLoop{N,E}"/>.</description>
    /// </item>
    /// </list>
    ///
    /// Callers can supply a custom column-selection function to control how both selected
    /// columns change row-to-row.
    /// </remarks>
    public static class SideWinderLoopFactory
    {
        /// <summary>
        /// Generates per-row left/right column sequences suitable for <see cref="SideWinderLoop{N,E}.CreateLoop"/>.
        /// </summary>
        /// <param name="width">The grid width (number of columns).</param>
        /// <param name="height">The number of rows to generate (typically the grid height).</param>
        /// <param name="initialLeftColumn">The starting left column for row 0.</param>
        /// <param name="initialRightColumn">The starting right column for row 0.</param>
        /// <param name="random">Optional random source; if omitted a new instance is created.</param>
        /// <param name="pickNextColumns">
        /// Optional function that selects the next row's left/right columns.
        /// If omitted, a default SideWinder-style function is used.
        /// </param>
        /// <param name="maxSpanWidth">Maximum horizontal delta used by the default picker.</param>
        /// <param name="minVerticalSpan">Minimum number of rows between turns in the default picker.</param>
        /// <param name="minLeftToRightSpacing">Minimum spacing between left and right columns in the default picker.</param>
        /// <returns>Two arrays of length <paramref name="height"/> containing one left/right column per row.</returns>
        public static (IReadOnlyList<int> leftColumns, IReadOnlyList<int> rightColumns) GenerateColumns(
            int width,
            int height,
            int initialLeftColumn,
            int initialRightColumn,
            System.Random random = null,
            Func<int, int, int, System.Random, (int left, int right)> pickNextColumns = null,
            int maxSpanWidth = 5,
            int minVerticalSpan = 1,
            int minLeftToRightSpacing = 1)
        {
            if (height <= 0)
            {
                return (Array.Empty<int>(), Array.Empty<int>());
            }

            random ??= new System.Random();

            int ClampColumn(int column)
            {
                if (column < 0) return 0;
                if (column >= width) return width - 1;
                return column;
            }

            int lastTurnRow = -99;

            (int left, int right) DefaultPickNextColumns(int row, int previousLeft, int previousRight, System.Random rng)
            {
                if (row < lastTurnRow + minVerticalSpan)
                {
                    return (previousLeft, previousRight);
                }

                lastTurnRow = row;

                int delta = rng.Next(maxSpanWidth + 1);
                int sign = rng.Next(2) == 1 ? 1 : -1;

                int newLeft = previousLeft + sign * delta;
                if (newLeft > width - 1 - minLeftToRightSpacing) newLeft = width - 1 - minLeftToRightSpacing - rng.Next(5);
                if (newLeft > previousRight - minLeftToRightSpacing) newLeft = previousRight - minLeftToRightSpacing;
                if (newLeft < 0) newLeft = previousLeft - sign * delta;
                if (newLeft > previousRight - minLeftToRightSpacing) newLeft = previousRight - minLeftToRightSpacing;

                delta = rng.Next(maxSpanWidth + 1);
                sign = rng.Next(2) == 1 ? 1 : -1;

                int newRight = previousRight + sign * delta;
                if (newRight < newLeft + minLeftToRightSpacing) newRight = newLeft + minLeftToRightSpacing;
                if (newRight < previousLeft + minLeftToRightSpacing) newRight = previousLeft + minLeftToRightSpacing;

                return (newRight >= width) ? (newLeft, width - 1) : (newLeft, newRight);
            }

            pickNextColumns ??= DefaultPickNextColumns;

            var leftColumns = new int[height];
            var rightColumns = new int[height];

            int left = initialLeftColumn;
            int right = initialRightColumn;
            leftColumns[0] = ClampColumn(left);
            rightColumns[0] = ClampColumn(right);

            for (int row = 1; row < height; row++)
            {
                (left, right) = pickNextColumns(row, left, right, random);
                leftColumns[row] = ClampColumn(left);
                rightColumns[row] = ClampColumn(right);
            }

            return (leftColumns, rightColumns);
        }

        /// <summary>
        /// Creates a loop by stitching the provided per-row left/right column sequences.
        /// </summary>
        public static GridPath<N, E> CreateLoop<N, E>(Grid<N, E> grid, IReadOnlyList<int> leftColumns, IReadOnlyList<int> rightColumns, int startingRow = 0)
        {
            var stitcher = new SideWinderLoop<N, E>(grid, startingRow);
            return stitcher.CreateLoop(leftColumns, rightColumns);
        }

        /// <summary>
        /// Creates a loop by generating per-row left/right column sequences and then stitching them.
        /// </summary>
        public static GridPath<N, E> CreateLoop<N, E>(
            Grid<N, E> grid,
            int initialLeftColumn,
            int initialRightColumn,
            int startingRow = 0,
            System.Random random = null,
            Func<int, int, int, System.Random, (int left, int right)> pickNextColumns = null,
            int maxSpanWidth = 5,
            int minVerticalSpan = 1,
            int minLeftToRightSpacing = 1)
        {
            int height = grid.Height - startingRow;
            (IReadOnlyList<int> leftColumns, IReadOnlyList<int> rightColumns) = GenerateColumns(
                grid.Width,
                height,
                initialLeftColumn,
                initialRightColumn,
                random,
                pickNextColumns,
                maxSpanWidth,
                minVerticalSpan,
                minLeftToRightSpacing);

            return CreateLoop(grid, leftColumns, rightColumns, startingRow);
        }
    }
}
