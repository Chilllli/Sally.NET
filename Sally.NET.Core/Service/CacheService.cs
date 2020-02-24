using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Sally.NET.Service
{
    public static class CacheService
    {
        public static async Task InitializeHandler()
        {
            Timer deleteCache = new Timer(60 * 60 * 1000);
            deleteCache.Start();
            deleteCache.Elapsed += DeleteCache_Elapsed;
        }

        private static void DeleteCache_Elapsed(object sender, ElapsedEventArgs e)
        {
            DirectoryInfo directory = new DirectoryInfo("cached");
            FileInfo[] Files = directory.GetFiles("*.json");
            foreach (FileInfo file in Files)
            {
                file.Delete();
            }

        }
    }
}
