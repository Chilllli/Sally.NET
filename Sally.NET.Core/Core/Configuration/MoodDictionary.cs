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
    public class MoodDictionary
    {
        private static Dictionary<Mood, Dictionary<string, string>> moodDictionary = new Dictionary<Mood, Dictionary<string, string>>();
        private DiscordSocketClient client;
        private BotCredentials credentials;
        private MoodHandlerService moodHandlerService;

        public MoodDictionary(DiscordSocketClient client, BotCredentials credentials)
        {
            this.client = client;
            this.credentials = credentials;
            moodHandlerService = new MoodHandlerService(client, credentials);
        }

        public void InitializeMoodDictionary()
        {
            foreach (Mood mood in System.Enum.GetValues(typeof(Mood)))
            {
                moodDictionary.Add(mood, JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText($"./mood/{mood}.json")));
            }
        }

        public string getMoodMessage(string message)
        {
            return moodDictionary[client.Activity != null ? System.Enum.Parse<Mood>(client.Activity?.Name) : moodHandlerService.getMood()][message];
        }
    }
}
