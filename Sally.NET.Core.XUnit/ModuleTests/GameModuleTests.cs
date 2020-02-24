using Newtonsoft.Json;
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
            string mods = "[\"FastStart\",\"HelpfulNPCs\",\"LootBags\",\"MagicStorage\"]";

            string[] expect = JsonConvert.DeserializeObject<string[]>(mods);

            string[] actual = GameModule.GetTerrariaMods("debug.json");

            Assert.Equal(expect, actual);
        }
    }
}
