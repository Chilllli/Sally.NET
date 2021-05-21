using System;
using System.Collections.Generic;
using System.Text;

namespace Sally.NET.Core.Configuration
{
    public class BotCredentials
    {
        public string Token { get; set; } = String.Empty;
        public string DbUser { get; set; } = String.Empty;
        public string Db { get; set; } = String.Empty;
        public string DbPassword { get; set; } = String.Empty;
        public string DbHost { get; set; } = String.Empty;
        public ulong RadioControlChannel { get; set; } = 0;
        public ulong MeId { get; set; } = 0;
        public int GainedXp { get; set; } = 0;
        public int XpTimerInMin { get; set; } = 0;
        public string WeatherPlace { get; set; } = String.Empty;
        public string WeatherApiKey { get; set; } = String.Empty;
        public string CleverApi { get; set; } = String.Empty;
    }
}
