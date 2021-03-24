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

        /// <summary>
        /// create and initialize service
        /// </summary>
        /// <param name="client"></param>
        /// <param name="credentials"></param>
        public static void InitializeMoodDictionary(DiscordSocketClient client, BotCredentials credentials)
        {
            MoodDictionary.client = client;
            MoodDictionary.credentials = credentials;
            foreach (Mood mood in System.Enum.GetValues(typeof(Mood)))
            {
                moodDictionary.TryAdd(mood, JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText($"mood/{mood}.json")));
            }
        }


        /// <summary>
        /// get a message to the corresponding mood
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string getMoodMessage(string message)
        {
            return moodDictionary[MoodHandlerService.GetMood()][message];
        }
    }
}
