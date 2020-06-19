using Sally.NET.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Sally.NET.XUnit.CoreTests
{
    public class UserTests
    {
        [Fact]
        public void User_ShouldAddValidXpAmount()
        {
            GuildUser testUser = new GuildUser(00000001, 000000001, 0);
            
            int exp = 1000;

            int expect = exp;

            testUser.Xp += exp;

            int actual = testUser.Xp;

            Assert.Equal(expect, actual);
        }

        [Fact]
        public void User_ShouldAddInvalidXpAmount()
        {
            GuildUser testUser = new GuildUser(00000001, 000000001, 0);

            int exp = -1000;

            int expect = 0;

            testUser.Xp += exp;

            int actual = testUser.Xp;

            Assert.Equal(expect, actual);
        }

        [Theory]
        [InlineData(18872, 14)]
        [InlineData(168244, 42)]
        [InlineData(859944, 95)]
        public void User_ShouldHaveCorrectLevel(int exp, int lvl)
        {
            GuildUser testUser = new GuildUser(00000001, 000000001, 0);

            int expect = lvl;

            testUser.Xp += exp;

            int actual = testUser.Level;

            Assert.Equal(expect, actual);
        }
    }
}
