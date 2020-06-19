using MySql.Data.MySqlClient;
using Sally.NET.DataAccess.Database;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Sally.NET.XUnit.DataAccessTests.DatabaseTests
{
    public class DatabaseAccessTests
    {
        [Theory]
        [InlineData("root", "root", "test")]
        public void Initialize_ShouldThrowMySqlException(string user, string password, string database)
        {
            Assert.Throws<MySqlException>(() => DatabaseAccess.Initialize(user, password, database));
        }
    }
}
