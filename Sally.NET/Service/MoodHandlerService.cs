using Discord;
using Discord.WebSocket;
using log4net;
using Newtonsoft.Json;
using Sally.NET.Core.ApiReference;
using Sally.NET.Core.Configuration;
using Sally.NET.Core.Enum;
using Sally.NET.DataAccess.Database;
using Sally.NET.Handler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Sally.NET.Service
{
    public static class MoodHandlerService
    {
        private static DiscordSocketClient client;
        private static BotCredentials credentials;
        private static double dailyPoints;
        private static float pointsSum;
        private static List<DateTime> messageList = new List<DateTime>();
        private static bool onStart;
        private static Mood oldMood;
        private static Mood newMood;
        private static WeatherApiHandler weatherApiHandler;
        private static ILog logger;

        /// <summary>
        /// initialize and create service
        /// </summary>
        /// <param name="client"></param>
        /// <param name="credentials"></param>
        /// <returns>
        /// return an async task, which can be awaited
        /// </returns>
        public static async Task InitializeHandler(DiscordSocketClient client, BotCredentials credentials, WeatherApiHandler weatherApiHandler, ILog logger)
        {
            MoodHandlerService.client = client;
            MoodHandlerService.credentials = credentials;
            MoodHandlerService.weatherApiHandler = weatherApiHandler;
            MoodHandlerService.logger = logger;
            //set start to true
            onStart = true;
            //Initialize Timer
            Timer dailyTimer = new Timer(24 * 60 * 60 * 1000);
            Timer weatherTimer = new Timer(8 * 60 * 60 * 1000);
            //question: does this timer even run? because its not started anywhere.
            Timer changeMoodTimer = new Timer(2 * 60 * 1000);
            //hook timer events
            dailyTimer.Elapsed += DailyTimer_Elapsed;
            weatherTimer.Elapsed += WeatherTimer_Elapsed;
            changeMoodTimer.Elapsed += ChangeMoodTimer_Elapsed;
            //start timer
            dailyTimer.Start();
            weatherTimer.Start();
            changeMoodTimer.Start();
            //get start values
            DailyTimer_Elapsed(null, null);
            await checkWeather();
            newMood = GetMood();
            await setMood(newMood).ConfigureAwait(false);
            onStart = false;
            client.MessageReceived += Client_MessageReceived;
        }

        private static async void ChangeMoodTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            oldMood = newMood;
            newMood = GetMood();
            await setMood(newMood);
        }

        private static Task Client_MessageReceived(SocketMessage message)
        {
            messageList.Add(message.CreatedAt.DateTime);
            messageList = messageList.Where(t => DateTime.Now.Subtract(new TimeSpan(0, 5, 0)) > t).ToList();
            return Task.CompletedTask;
        }

        private static async void WeatherTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await checkWeather();
        }

        private static void DailyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            dailyPoints = new Random().NextDouble();
        }

        private static async Task checkWeather()
        {
            WeatherApi apiResult = await weatherApiHandler.Request2WeatherApiAsync(credentials.WeatherApiKey, credentials.WeatherPlace);
            //main.temp 70%,
            pointsSum = calculateWeatherPoints(15f, 20f, 0.7f, apiResult.Weather.Temperature);
            //main.humidity 5%, 
            pointsSum += calculateWeatherPoints(10f, 50f, 0.05f, apiResult.Weather.Humidity);
            //wind.speed 5%,
            pointsSum += calculateWeatherPoints(10f, 4f, 0.05f, apiResult.Wind.Speed);
            //clouds.all 10%,
            pointsSum += calculateWeatherPoints(50f, 10f, 0.1f, apiResult.Clouds.Density);
            //rain.1h 0.2 10%,
            pointsSum += calculateWeatherPoints(2.5f, 0f, 0.1f, 0f);
        }
        private static float calculateWeatherPoints(float width, float optimum, float weigth, float weatherValue)
            => MathF.Max(-(1 / MathF.Pow(width, 2)) * MathF.Pow((weatherValue - optimum), 2) + 1, -1) * weigth;

        private static int messageCounter()
        {
            return messageList.Count(m => m > DateTime.Now.Subtract(new TimeSpan(0, 1, 0)));
        }

        /// <summary>
        /// Method will return a mood enum, which is corresponding to the calculated values
        /// </summary>
        /// <returns>
        /// calculated Mood enum from values
        /// </returns>
        public static Mood GetMood()
        {
            double currentMood = getMoodPoints();
            if (currentMood >= 0 && currentMood <= 0.25)
            {
                return Mood.Sad;
            }
            if (currentMood > 0.25 && currentMood <= 0.50)
            {
                return Mood.Meh;
            }
            if (currentMood > 0.50 && currentMood <= 0.75)
            {
                return Mood.Happy;
            }
            return Mood.Extatic;
        }


        private static async Task setMood(Mood mood)
        {
            if (client.Activity?.Name == mood.ToString())
            {
                return;
            }
            DatabaseAccess.Instance.saveMood(mood);
            //temporarily disbaled because its throwing http 400 for reasons which i dont understand
            //if (validateBotData(client, mood))
            //{
            //    await client.CurrentUser.ModifyAsync(c => c.Avatar = new Image($"./mood/{mood}.png"));
            //}
            await client.SetGameAsync($"{mood} | $help", type: ActivityType.Watching);
            logger.Info($"Mood Changed: {oldMood} -> {newMood}");
        }

        private static double getMoodPoints()
        {
            //calculate which mood sally will have
            //it depends on:
            //  a random value, whoch will generated daily
            //  the current weather
            //      each weather property is weighted differently
            //  how many users are currently logged in
            //  if there are users in voice channels
            // how many messages were send to sally
            return (dailyPoints + pointsSum + (getAllOnlineUser() / getAllUser()) + (voiceChannelWatcher() ? 1 : 0) + (1 - 1 / (1 + messageCounter()))) / (onStart ? 3.0f : 5.0f);
        }

        private static double getAllOnlineUser()
        {
            List<SocketGuild> guilds = client.Guilds.ToList();
            double totalOnlineUser = 0.0;
            foreach (SocketGuild guild in guilds)
            {
                totalOnlineUser += (double)guild.Users.ToList().Count(u => u.Status != UserStatus.Offline);
            }
            return totalOnlineUser;
        }

        private static double getAllUser()
        {
            List<SocketGuild> guilds = client.Guilds.ToList();
            double totalUser = 0.0;
            foreach (SocketGuild guild in guilds)
            {
                totalUser += (double)guild.Users.Count();
            }
            return totalUser;
        }

        private static bool voiceChannelWatcher()
        {
            List<SocketGuild> guilds = client.Guilds.ToList();
            foreach (SocketGuild guild in guilds)
            {
                if (guild.Users.ToList().Find(u => u.VoiceChannel != null) != null)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool validateBotData(DiscordSocketClient client, Mood? mood)
        {
            //validate image
            if (!File.Exists($"./mood/{mood}.png"))
                return false;
            if (!new Image($"./mood/{mood}.png").Stream.CanRead)
                return false;

            //validate bot connection
            if (client.ConnectionState != ConnectionState.Connected)
                return false;

            //validate mood
            //check if mood is null
            if (mood == null)
                return false;
            //check if enum contain mood
            if (!Enum.GetNames(typeof(Mood)).ToList().Contains(mood.ToString()))
                return false;

            return true;
        }
    }
}
