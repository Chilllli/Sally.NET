using Newtonsoft.Json;
using Sally.NET.Core;
using Sally.NET.DataAccess.Database;
using Sally.NET.Module;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Sally.NET.XUnit.ModuleTests
{
    public class GameModuleTests
    {
        [Fact]
        public void GetTerrariaMods_ShouldReturnStringArray()
        {
            Helper helper = new Helper(new SQLiteAccess(""));
            GameModule gameModule = new GameModule(helper, new SQLiteAccess(""));
            string mods = "[\"FastStart\",\"HelpfulNPCs\",\"LootBags\",\"MagicStorage\"]";

            string[] expect = JsonConvert.DeserializeObject<string[]>(mods);

            string[] actual = gameModule.GetTerrariaMods("debug.json");

            Assert.Equal(expect, actual);
        }
    }
}
