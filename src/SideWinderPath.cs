using CrawfisSoftware.Collections.Graph;

namespace CrawfisSoftware.Path
{
    /// <summary>
    /// Generate a path from the start column on the bottom row to the end column
    /// on the top row.
    /// </summary>
    public class SideWinderPath<N, E>
    {
        private Grid<N, E> _grid;
        private readonly System.Random _random;
        private readonly int _width;
        private readonly int _height;
        private bool first = true;

        /// <summary>
        /// Get or set the maximum horizontal passage length used in the default
        /// PickNextColumn function.
        /// </summary>
        public int MaxSpanWidth { get; set; } = 5;

        /// <summary>
        /// Get or set the a function to determine on a per row basis the exact column
        /// the curve should shift over to. Defaults to a random column to the left or
        /// right of the previous column at most MaxSpanWidth away.
        /// </summary>
        public Func<int, int, System.Random, int> PickNextColumn { get; set; }

        /// <summary>
        /// Get or set the a function to determine on a per row basis the exact row
        /// the curve should move to after the span for that row is completed. Defaults
        /// to the next row (returns row+1).
        /// </summary>
        public Func<int, int, System.Random, int> PickNextRow { get; set; }

        private int DefaultPickNextColumnFunc(int row, int previousColumn, System.Random randomGenerator = null)
        {
            int delta = randomGenerator.Next(_grid.Width) - previousColumn;
            int sign = 1;
            if (delta < 0) sign = -1;
            delta = ((sign * delta) > MaxSpanWidth) ? sign * MaxSpanWidth : delta;
            return previousColumn + delta;
        }

        private int DefaultPickNextRowFunc(int row, int previousColumn, System.Random randomGenerator = null)
        {
            // An example of a "reset". Once it hits 12 it loops back to row 2, creating a long vertical and possibly loops
            //if(first && row == 12)
            //{
            //    first = false;
            //    return 2;
            //}
            return row + 3;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public SideWinderPath(Grid<N, E> grid, System.Random random = null)
        {
            _grid = grid;
            _width = _grid.Width;
            _height = _grid.Height;
            _random = (random ?? new System.Random());
            this.PickNextColumn = DefaultPickNextColumnFunc;
            this.PickNextRow = DefaultPickNextRowFunc;
        }

        /// <summary>
        /// Generate a path from an explicit per-row column sequence.
        /// </summary>
        public GridPath<N, E> CarvePath(IReadOnlyList<int> columns)
        {
            List<int> gridPositions = SideWinderSpanUtils.StitchPathFromColumns(_width, columns);
            return new GridPath<N, E>(_grid, gridPositions, -1, false);
        }

        /// <summary>
        /// Generate a path by randomly generating one column per row.
        /// </summary>
        public GridPath<N, E> CarvePath(int startingColumn, int endingColumn)
        {
            IReadOnlyList<int> columns = GenerateColumns(startingColumn, endingColumn);
            return CarvePath(columns);
        }

        public static GridPath<N, E> CreatePath(Grid<N, E> grid, int startingColumn, int endingColumn, System.Random random = null, Action<SideWinderPath<N, E>> configure = null)
        {
            var generator = new SideWinderPath<N, E>(grid, random);
            configure?.Invoke(generator);
            return generator.CarvePath(startingColumn, endingColumn);
        }

        internal IReadOnlyList<int> GenerateColumns(int startingColumn, int endingColumn)
        {
            var columns = new int[_height];
            columns[0] = ClampColumn(startingColumn);

            for (int row = 1; row < _height; row++)
            {
                int column = ClampColumn(PickNextColumn(row, columns[row - 1], _random));
                columns[row] = column;
            }

            columns[_height - 1] = ClampColumn(endingColumn);
            return columns;
        }

        private int ClampColumn(int column)
        {
            if (column < 0) return 0;
            if (column >= _width) return _width - 1;
            return column;
        }

        private int ClampRow(int row)
        {
            if (row < 0) return 0;
            if (row >= _height) return _height - 1;
            return row;
        }
    }
}