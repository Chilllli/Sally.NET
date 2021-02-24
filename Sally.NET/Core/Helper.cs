﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Sally.NET.Core
{
    public static class Helper
    {
        /// <summary>
        /// The Levenstein algorithm is used to calculate the similarities between two strings. 
        /// </summary>
        /// <remarks>
        /// The more two strings have in common, the smaller will be the returned value.
        /// </remarks>
        /// <param name="a">first compared string</param>
        /// <param name="b">second compared string</param>
        /// <returns>
        /// The returned value is an int with the calculated differences of the two strings.
        /// </returns>
        public static int CalcLevenshteinDistance(string a, string b)
        {
            if (String.IsNullOrEmpty(a) && String.IsNullOrEmpty(b))
            {
                return 0;
            }
            if (String.IsNullOrEmpty(a))
            {
                return b.Length;
            }
            if (String.IsNullOrEmpty(b))
            {
                return a.Length;
            }
            int lengthA = a.Length;
            int lengthB = b.Length;
            var distances = new int[lengthA + 1, lengthB + 1];
            for (int i = 0; i <= lengthA; distances[i, 0] = i++)
                ;
            for (int j = 0; j <= lengthB; distances[0, j] = j++)
                ;

            for (int i = 1; i <= lengthA; i++)
                for (int j = 1; j <= lengthB; j++)
                {
                    int cost = b[j - 1] == a[i - 1] ? 0 : 1;
                    distances[i, j] = Math.Min
                        (
                        Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                        distances[i - 1, j - 1] + cost
                        );
                }
            return distances[lengthA, lengthB];
        }
    }
}
