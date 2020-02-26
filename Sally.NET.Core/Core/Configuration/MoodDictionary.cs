using Discord.WebSocket;
using Newtonsoft.Json;
using Sally.NET.Core.Enum;
using Sally.NET.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sally.NET.Core.Configuration
{
    public static class MoodDictionary
    {
        private static Dictionary<Mood, Dictionary<string, string>> moodDictionary = new Dictionary<Mood, Dictionary<string, string>>();
        private static DiscordSocketClient client;
        private static BotCredentials credentials;

        public static void InitializeMoodDictionary(DiscordSocketClient client, BotCredentials credentials)
        {
            MoodDictionary.client = client;
            MoodDictionary.credentials = credentials;
            foreach (Mood mood in System.Enum.GetValues(typeof(Mood)))
            {
                moodDictionary.Add(mood, JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText($"./mood/{mood}.json")));
            }
        }

        public static string getMoodMessage(string message)
        {
            return moodDictionary[client.Activity != null ? System.Enum.Parse<Mood>(client.Activity?.Name) : MoodHandlerService.getMood()][message];
        }
    }
}
