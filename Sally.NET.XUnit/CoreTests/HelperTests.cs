using Sally.NET.Core;
using Sally.NET.DataAccess.Database;
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
            Helper helper = new Helper(new SQLiteAccess(""));
            int expected = difference;

            int actual = helper.CalcLevenshteinDistance(a, b);

            Assert.Equal(expected, actual);
        }
    }
}
