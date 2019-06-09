using System;
using System.Collections.Generic;
using System.Text;

namespace Sally_NET.Config
{
    class BotConfiguration
    {
        public string token = String.Empty;
        public string db_user = String.Empty;
        public string db_database = String.Empty;
        public string db_password = String.Empty;
        public string db_host = String.Empty;
        public ulong radioControlChannel = 0;
        public ulong guildId = 0;
        public ulong meId = 0;
        public int gainedXp = 0;
        public int xpTimerInMin = 0;
        public ulong TerrariaId = 0;
        public ulong AdminRole = 0;
        public string WeatherPlace = String.Empty;
        public string WeatherApiKey = String.Empty;
        public string CleverApi = String.Empty;
        //public ulong StarterChannel = 0;
        //public ulong ClientId = 0;
    }
}
