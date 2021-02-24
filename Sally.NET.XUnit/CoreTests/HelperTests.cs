using Sally.NET.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Sally.NET.XUnit.CoreTests
{
    public class HelperTests
    {
        [Theory]
        [InlineData("test", "test", 0)]
        [InlineData("admin", "afmin", 1)]
        [InlineData("aaaa", "bbbb", 4)]
        public void CalcLevenshteinDistance_ShouldReturnValidInt(string a, string b, int difference)
        {
            int expected = difference;

            int actual = Helper.CalcLevenshteinDistance(a, b);

            Assert.Equal(expected, actual);
        }
    }
}
