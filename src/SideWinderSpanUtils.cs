using System;
using System.Collections.Generic;

namespace CrawfisSoftware.Path
{
    public static class SideWinderSpanUtils
    {
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

        public static void AddIndices(List<int> indices, IEnumerable<int> span)
        {
            foreach (int index in span)
            {
                indices.Add(index);
            }
        }

        public static List<int> StitchPathFromColumns(int width, IReadOnlyList<int> columns)
        {
            if (columns.Count == 0)
            {
                return new List<int>();
            }

            var indices = new List<int>(capacity: columns.Count * 2);
            indices.Add(columns[0] + width * 0);

            for (int row = 1; row < columns.Count; row++)
            {
                int previousColumn = columns[row - 1];
                int currentColumn = columns[row];

                AddIndices(indices, VerticalSpan(width, previousColumn, row - 1, row));
                AddIndices(indices, HorizontalSpan(width, row, previousColumn, currentColumn));
            }

            return indices;
        }
    }
}