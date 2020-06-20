using System;
using System.Collections.Generic;
using System.Text;

namespace Sally.NET.Core.Configuration
{
    public class BotCredentials
    {
        public string token { get; set; } = String.Empty;
        public string db_user { get; set; } = String.Empty;
        public string db_database { get; set; } = String.Empty;
        public string db_password { get; set; } = String.Empty;
        public string db_host { get; set; } = String.Empty;
        public ulong radioControlChannel { get; set; } = 0;
        public ulong guildId { get; set; } = 0;
        public ulong meId { get; set; } = 0;
        public int gainedXp { get; set; } = 0;
        public int xpTimerInMin { get; set; } = 0;
        public ulong TerrariaId { get; set; } = 0;
        public ulong AdminRole { get; set; } = 0;
        public string WeatherPlace { get; set; } = String.Empty;
        public string WeatherApiKey { get; set; } = String.Empty;
        public string CleverApi { get; set; } = String.Empty;
        //public ulong StarterChannel = 0;
        //public ulong ClientId = 0;
    }
}
