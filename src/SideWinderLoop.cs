using CrawfisSoftware.Collections.Graph;

using System;
using System.Collections.Generic;

namespace CrawfisSoftware.Path
{
    /// <summary>
    /// Generate a random loop on a grid that consists of two paths that merge at
    /// the bottom row and top row.
    /// </summary>
    public class SideWinderLoop<N, E>
    {
        /// <summary>
        /// Get or set the maximum horizontal passage length used in the default
        /// PickNextColumn function.
        /// </summary>
        public int MaxSpanWidth { get; set; } = 5;

        /// <summary>
        /// Get or set the number of rows that should be vertical spans before any turns. This is used in the default
        /// PickNextColumn function.
        /// </summary>
        public int MinVerticalSpan { get; private set; } = 1;
        /// <summary>
        /// Get or set the number of rows that should be vertical spans before any turns. This is used in the default
        /// PickNextColumn function.
        /// </summary>
        public int MinLeftToRightSpacing { get; private set; } = 1;

        /// <summary>
        /// Get or set the a function to determine on a per row basis the exact column
        /// the curve should shift over to. Defaults to a random column to the left or
        /// right of the previous column at most MaxSpanWidth away.
        /// </summary>
        public Func<int, int, int, System.Random, (int, int)> PickNextColumns { get; set; }


        private Grid<N, E> _grid;
        private readonly System.Random _random;
        private int _lastRow = -99;
        private readonly int _width;
        private readonly int _height;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="random">System.Random to use.</param>
        public SideWinderLoop(Grid<N,E> grid, System.Random random = null)
        {
            _grid = grid;
            _width = grid.Width;
            _height = grid.Height;
            _random = (random ?? new System.Random());
            this.PickNextColumns = DefaultPickNextColumnsFunc;
        }

        /// <summary>
        /// Create a maze using the Sidewinder algorithm
        /// </summary>
        public GridPath<N,E> CarveLoop(int initialLeftColumn, int initialRightColumn)
        {
            _lastRow = -99;
            (IReadOnlyList<int> leftColumns, IReadOnlyList<int> rightColumns) = GenerateColumns(initialLeftColumn, initialRightColumn);
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

            List<int> leftPath = SideWinderSpanUtils.StitchPathFromColumns(width, leftColumns);
            var indices = new List<int>(capacity: leftPath.Count + rowCount * 2);
            indices.AddRange(leftPath);

            SideWinderSpanUtils.AddIndices(indices, SideWinderSpanUtils.HorizontalSpan(width, lastRow, leftColumns[lastRow], rightColumns[lastRow]));

            List<int> rightPath = SideWinderSpanUtils.StitchPathFromColumns(width, rightColumns);
            rightPath.Reverse();

            if (rightPath.Count > 0)
            {
                rightPath.RemoveAt(0);
            }
            indices.AddRange(rightPath);

            SideWinderSpanUtils.AddIndices(indices, SideWinderSpanUtils.HorizontalSpan(width, 0, rightColumns[0], leftColumns[0]));
            return indices;
        }

        public static GridPath<N, E> CreateLoop(Grid<N, E> grid, int initialLeftColumn, int initialRightColumn, System.Random random = null, Action<SideWinderLoop<N, E>> configure = null)
        {
            var generator = new SideWinderLoop<N, E>(grid, random);
            configure?.Invoke(generator);
            return generator.CarveLoop(initialLeftColumn, initialRightColumn);
        }

        public (IReadOnlyList<int> leftColumns, IReadOnlyList<int> rightColumns) GenerateColumns(int initialLeftColumn, int initialRightColumn)
        {
            var leftColumns = new int[_height];
            var rightColumns = new int[_height];

            int leftColumn = initialLeftColumn;
            int rightColumn = initialRightColumn;
            leftColumns[0] = leftColumn;
            rightColumns[0] = rightColumn;

            for (int row = 1; row < _height; row++)
            {
                (leftColumn, rightColumn) = PickNextColumns(row, leftColumn, rightColumn, _random);
                leftColumns[row] = leftColumn;
                rightColumns[row] = rightColumn;
            }

            return (leftColumns, rightColumns);
        }
        private (int, int) DefaultPickNextColumnsFunc(int row, int previousLeftColumn, int previousRightColumn, System.Random randomGenerator = null)
        {
            if (row < _lastRow + MinVerticalSpan) return (previousLeftColumn, previousRightColumn);
            _lastRow = row;
            //return (0, Width - 1);
            int delta = randomGenerator.Next(MaxSpanWidth + 1);
            int sign = randomGenerator.Next(2) == 1 ? 1 : -1;
            int newLeftColumn = previousLeftColumn + sign * delta;
            if (newLeftColumn > _grid.Width - 1 - MinLeftToRightSpacing) newLeftColumn = _grid.Width - 1 - MinLeftToRightSpacing - randomGenerator.Next(5);
            if (newLeftColumn > previousRightColumn - MinLeftToRightSpacing) newLeftColumn = previousRightColumn - MinLeftToRightSpacing;
            if (newLeftColumn < 0) newLeftColumn = previousLeftColumn - sign * delta;
            if (newLeftColumn > previousRightColumn - MinLeftToRightSpacing) newLeftColumn = previousRightColumn - MinLeftToRightSpacing;
            delta = randomGenerator.Next(MaxSpanWidth + 1);
            sign = randomGenerator.Next(2) == 1 ? 1 : -1;
            int newRightColumn = previousRightColumn + sign * delta;
            if (newRightColumn < newLeftColumn + MinLeftToRightSpacing) newRightColumn = newLeftColumn + MinLeftToRightSpacing;
            if (newRightColumn < previousLeftColumn + MinLeftToRightSpacing) newRightColumn = previousLeftColumn + MinLeftToRightSpacing;
            if (newRightColumn >= _grid.Width) newRightColumn = _grid.Width - 1;
            //Console.WriteLine($"{newLeftColumn} - {newRightColumn}");
            return (newLeftColumn, newRightColumn);
        }
    }
}