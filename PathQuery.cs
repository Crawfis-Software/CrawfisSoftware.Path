using CrawfisSoftware.Collections.Graph;
using CrawfisSoftware.Collections.Path;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrawfisSoftware.Collections.Path
{
    /// <summary>
    /// Static class with methods to query an IPath or GridPath.
    /// </summary>
    public static class PathQuery
    {
        // Todo: Add support for Toroidal grids - AKA make sure to use the grid for any direction choices.
        /// <summary>
        /// Utility function to determine whether a path goes straight (S) or turns left (L) or right (R).
        /// </summary>
        /// <param name="path">A GridPath</param>
        /// <param name="positionIndex">The position index of the path which we are querying the cell direction.</param>
        /// <param name="isLoop">True is the path forms a closed loop.</param>
        /// <returns>A character respresenting the turtle action.</returns>
        /// <remarks>Does not support toriodal grids.</remarks>
        public static char DetermineCellDirectionAt<N,E>(GridPath<N,E> path, int positionIndex, bool isLoop = false)
        {
            int priorIndex = positionIndex - 1;
            int nextindex = positionIndex + 1;
            if (isLoop && priorIndex < 0) priorIndex = path.Count - 1;
            if (isLoop && nextindex >= path.Count) nextindex = 0;
            Direction cellDirection = DirectionExtensions.GetEdgeDirection(path[priorIndex], path[positionIndex], path.Grid.Width);
            cellDirection |= DirectionExtensions.GetEdgeDirection(path[nextindex], path[positionIndex], path.Grid.Width);
            if (cellDirection.IsStraight())
            {
                return StringPathQuery.StraightChar;
            }
            if (cellDirection.IsTurn())
            {
                // Building a little logic table of (i-1)->i versus i->i+1 yeilds this.
                int deltai = path[positionIndex] - path[priorIndex];
                int deltaii = path[nextindex] - path[positionIndex];
                int testValue = (Math.Abs(deltai) - 2) * deltai * deltaii;
                if (testValue < 0)
                    return StringPathQuery.LeftChar;
                return StringPathQuery.RightChar;
            }
            return StringPathQuery.InvalidChar;
        }

        /// <summary>
        /// Give a GridPath, determine the turtle string defining the path.
        /// </summary>
        /// <typeparam name="N">The underlying node label type of the grid. Not used.</typeparam>
        /// <typeparam name="E">The underlying edge label type of the grid. Not used.</typeparam>
        /// <param name="path">A GridPath</param>
        /// <returns>A string representing the straights and turns as you move along the path.</returns>
        public static string DetermineTurtleString<N, E>(GridPath<N, E> path)
        {
            StringBuilder stringPath = new StringBuilder(path.Count);
            int startIndex = 1;
            int endIndex = path.Count - 1;
            if(path.IsClosed)
            {
                startIndex = 0;
                endIndex = path.Count;
            }
            for (int i = startIndex; i < endIndex; i++)
            {
                char token = PathQuery.DetermineCellDirectionAt<N, E>(path, i, path.IsClosed);
                stringPath.Append(token);
            }
            return stringPath.ToString();
        }
    }
}