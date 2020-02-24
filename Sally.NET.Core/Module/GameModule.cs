using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sally.NET.Module
{
    public static class GameModule
    {
        /// <summary>
        /// Returns a string array of enabled terraria mods
        /// 
        /// </summary>
        /// <param name="file">File, where enabled terraria mods are stored e.g.: enabled.json</param>
        /// <returns>string[]</returns>
        public static string[] GetTerrariaMods(string file)
        {
            return JsonConvert.DeserializeObject<string[]>(File.ReadAllText(file));
        }
    }
}
