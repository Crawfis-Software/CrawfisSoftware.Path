using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CrawfisSoftware.Collections.Path
{
    /// <summary>
    /// Static utility for querying a path or loop as an encoded turtle-based string.
    /// </summary>
    public static class StringPathQuery
    {
        /// <summary>
        /// The character that indicates: Go Straight (defaults to 'S').
        /// </summary>
        public static char StraightChar = 'S';
        /// <summary>
        /// The character that indicates: Go Left (defaults to 'L').
        /// </summary>
        public static char LeftChar = 'L';
        /// <summary>
        /// The character that indicates: Go Right (defaults to 'R').
        /// </summary>
        public static char RightChar = 'R';
        /// <summary>
        /// A character that indicates the path is disconnect here or invalid (defaults to 'X').
        /// </summary>
        public static char InvalidChar = 'X';

        /// <summary>
        /// Searches the path (expressed as an input string) for the regular expression and returns the starting string index for each instance it encounters.
        /// </summary>
        /// <param name="pathString">The turtle string of straight, left and right movements.</param>
        /// <param name="regex">A Regular Expression in the System.Text.RegularExpression.Regex format.</param>
        /// <param name="isClosed">True if the string represents a loop. Default is false.</param>
        /// <returns>The starting index for the pattern for each occurance.</returns>
        /// <remarks>Note that the pattern usually starts at the cell before. For instance a left turn that starts at i-1, goes through i to i+width, will return i-1, not i.</remarks>
        public static IEnumerable<int> SearchPathString(string pathString, Regex regex, bool isClosed = false)
        {
            var matches = regex.Matches(pathString);
            foreach (Match match in matches)
            {
                yield return match.Index;
            }
        }

        /// <summary>
        /// Enumerates all of the U-turns in TurtlePath string (aka, all "RR" and "LL" substrings).
        /// </summary>
        /// <param name="pathString">The turtle string of straight, left and right movements.</param>
        /// <returns>The starting index for the pattern for each occurance.</returns>
        /// <remarks>Note that the pattern usually starts at the cell before. For instance a left turn that starts at i-1, goes through i to i+width, will return i-1, not i.</remarks>
        /// <seealso cref="Search(Regex)"/>
        public static IEnumerable<int> UTurns(string pathString)
        {
            string pattern = "(" + RightChar + RightChar + "|" + LeftChar + LeftChar + ")";
            Regex regex = new Regex(pattern, RegexOptions.Compiled);
            return SearchPathString(pathString, regex);
        }

        /// <summary>
        /// Enumerates all of the consecutive straights (StringPathQuery.Straight) in TurtlePath string with a length greater than or equal to specified length.
        /// </summary>
        /// <param name="pathString">The turtle string of straight, left and right movements.</param>
        /// <param name="straightLength">The desired straight-away length to match.</param>
        /// <returns>The starting index for the pattern for each occurance.</returns>
        /// <remarks>Note that the pattern usually starts at the cell before. For instance a left turn that starts at i-1, goes through i to i+width, will return i-1, not i.</remarks>
        /// <seealso cref="Search(Regex)"/>
        public static IEnumerable<int> StraightAways(string pathString, int straightLength)
        {
            //string pattern = "S{" + straightLength + "}";
            //Regex regex = new Regex(pattern, RegexOptions.Compiled);
            //return Search(regex);
            StringBuilder minStraightsSequence = new StringBuilder();
            for (int i = 0; i < straightLength; i++)
                minStraightsSequence.Append(StringPathQuery.StraightChar);
            string subString = pathString;
            int stringIndex = subString.IndexOf(minStraightsSequence.ToString());
            int turtleIndex = 0;
            while (stringIndex >= 0)
            {
                yield return stringIndex + turtleIndex;
                while (stringIndex < subString.Length && subString[stringIndex] == StringPathQuery.StraightChar) stringIndex++;
                subString = subString.Substring(stringIndex);
                turtleIndex += stringIndex;
                stringIndex = subString.IndexOf(minStraightsSequence.ToString());
            }
        }

        /// <summary>
        /// Calculate the ratio of straights to turns within a specified window centered on the specified path index.
        /// </summary>
        /// <param name="pathString">The turtle string of straight, left and right movements.</param>
        /// <param name="pathIndex">The index into the List of grid cells that define the path, not a grid index itself.</param>
        /// <param name="halfWindowSize">The half window size to use in the analysis.</param>
        /// <returns>A float from 0 to 1 representing the ratio of straights to turns with the window centered at the path location. 
        /// Note, if the window exceeds the path it is cropped to the valid region. If the resulting window size is less than one, a -1 is returned.</returns>
        public static float SpeedAgilityRatio(string pathString, int pathIndex, int halfWindowSize = 2)
        {
            int numberOfStraights = 0;
            int numberOfTurns = 0;
            int startIndex = Math.Max(0, pathIndex - halfWindowSize);
            int endIndex = Math.Min(pathIndex + halfWindowSize, pathString.Length - 2);
            int windowSize = endIndex - startIndex + 1;
            if (windowSize == 1) return pathString[startIndex] == StringPathQuery.StraightChar ? 1 : 0;
            if (windowSize <= 0) return -1;
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (pathString[i] == StringPathQuery.StraightChar) numberOfStraights++;
                else numberOfTurns++;
            }
            return (float)numberOfStraights / (float)windowSize;
        }

        /// <summary>
        /// Calculate the number of consecutive straights.
        /// </summary>
        /// <param name="pathString">The turtle string of straight, left and right movements.</param>
        public static int MaximumConsecutiveStraights(string pathString)
        {
            int maxConsecutiveStraights = 0;
            int numberOfStraights = 0;
            foreach (char token in pathString)
            {
                if (token == StringPathQuery.StraightChar)
                {
                    numberOfStraights++;
                    maxConsecutiveStraights = (maxConsecutiveStraights >= numberOfStraights) ? maxConsecutiveStraights : numberOfStraights;
                }
            }
            return maxConsecutiveStraights;
        }

        /// <summary>
        /// Calculate the number of Consecutive turns (left or right).
        /// </summary>
        /// <param name="pathString">The turtle string of straight, left and right movements.</param>
        public static int MaximumConsecutiveTurns(string pathString)
        {
            int maxConsecutiveTurns = 0;
            int numberOfTurns = 0;
            foreach (char token in pathString)
            {
                if (token == StringPathQuery.LeftChar || token == StringPathQuery.RightChar)
                {
                    numberOfTurns++;
                    maxConsecutiveTurns = (maxConsecutiveTurns >= numberOfTurns) ? maxConsecutiveTurns : numberOfTurns;
                }
            }
            return maxConsecutiveTurns;
        }
    }
}