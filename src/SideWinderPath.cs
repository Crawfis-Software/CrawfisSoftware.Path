using CrawfisSoftware.Collections.Graph;

namespace CrawfisSoftware.Path
{
    /// <summary>
    /// Stitches a continuous, grid-aligned path from an explicit per-row column sequence.
    /// </summary>
    /// <remarks>
    /// This type is intentionally minimal: it does not generate or randomize the per-row column
    /// choices. Callers provide one column index per row and this class converts that sequence
    /// into a list of grid indices by adding vertical connectors between rows and horizontal spans
    /// within each row.
    ///
    /// If you need helpers for generating the column sequence (random or otherwise), use
    /// <see cref="SideWinderPathFactory"/>.
    /// </remarks>
    public class SideWinderPath<N, E>
    {
        private readonly Grid<N, E> _grid;
        private readonly int _width;

        /// <summary>
        /// Creates a new path stitcher for the given <paramref name="grid"/>.
        /// </summary>
        /// <param name="grid">The grid that the resulting <see cref="GridPath{N,E}"/> will reference.</param>
        public SideWinderPath(Grid<N, E> grid)
        {
            _grid = grid;
            _width = _grid.Width;
        }

        /// <summary>
        /// Generates a <see cref="GridPath{N,E}"/> from an explicit per-row column sequence.
        /// </summary>
        /// <param name="columns">
        /// One column index per row (0-based). The list length controls the number of rows
        /// included in the resulting path; typically this matches <c>grid.Height</c>.
        /// </param>
        /// <returns>A <see cref="GridPath{N,E}"/> representing the stitched path.</returns>
        public GridPath<N, E> CreatePath(IReadOnlyList<int> columns)
        {
            List<int> gridPositions = SpanUtilities.StitchPathFromColumns(_width, columns);
            return new GridPath<N, E>(_grid, gridPositions, -1, false);
        }
    }
}