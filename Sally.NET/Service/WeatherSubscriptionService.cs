using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Sally.NET.Core;
using Sally.NET.Core.ApiReference;
using Sally.NET.Core.Configuration;
using Sally.NET.DataAccess.Database;
using Sally.NET.DataAccess.File;
using Sally.NET.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using FileAccess = Sally.NET.DataAccess.File.FileAccess;
using Timer = System.Timers.Timer;

namespace Sally.NET.Service
{
    public static class WeatherSubscriptionService
    {
        private static DiscordSocketClient client { get; set; }
        public static BotCredentials credentials { get; set; }
        private static WeatherApiHandler weatherApiHandler;

        public static void InitializeWeatherSub(DiscordSocketClient client, BotCredentials credentials, WeatherApiHandler weatherApiHandler)
        {
            WeatherSubscriptionService.client = client;
            WeatherSubscriptionService.weatherApiHandler = weatherApiHandler;
            WeatherSubscriptionService.credentials = credentials;
            Timer checkWeather = new Timer(60 * 1000);
            checkWeather.Start();
            checkWeather.Elapsed += CheckWeather_Elapsed;
        }

        private static async void CheckWeather_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (User user in DatabaseAccess.Instance.Users)
            {
                if (user.NotifierTime == null || user.WeatherLocation == null)
                    continue;
                if (!(DateTime.Now.Hour == user.NotifierTime.Value.Hours && DateTime.Now.Minute == user.NotifierTime.Value.Minutes))
                    continue;
                SocketUser disUser = client.GetUser(user.Id);
                if (disUser == null)
                    continue;

                WeatherApi apiResult = await weatherApiHandler.Request2WeatherApiAsync(credentials.WeatherApiKey, user.WeatherLocation);

                EmbedBuilder weatherEmbed = new EmbedBuilder()
                    .WithTitle("Weather Info")
                    .WithDescription("Weather Notification for today")
                    .AddField(user.WeatherLocation, $"{apiResult.Weather.Temperature} °C")
                    .AddField("Max. Temp today", $"{apiResult.Weather.MaxTemperature} °C")
                    .AddField("Min. Temp for today", $"{apiResult.Weather.MinTemperature} °C")
                    .AddField("Weather Condition", apiResult.WeatherCondition.First().ShortDescription)
                    .WithColor(Color.Blue)
                    .WithTimestamp(DateTime.Now)
                    .WithFooter(FileAccess.GENERIC_FOOTER, FileAccess.GENERIC_THUMBNAIL_URL);
                await disUser.SendMessageAsync(embed: weatherEmbed.Build()).ConfigureAwait(false);
            }
        }
    }
}
