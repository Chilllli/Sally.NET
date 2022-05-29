using MySql.Data.MySqlClient;
using Sally.NET.DataAccess.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Sally.NET.XUnit.DataAccessTests.DatabaseTests
{
    public class DatabaseAccessTests
    {
        [Theory]
        [InlineData("root", "root", "test", "localhost")]
        public async Task Initialize_ShouldThrowMySqlException(string user, string password, string database, string host)
        {
            await Assert.ThrowsAsync<MySqlException>(async () => await DatabaseAccess.InitializeAsync(user, password, database, host));
        }
    }
}
