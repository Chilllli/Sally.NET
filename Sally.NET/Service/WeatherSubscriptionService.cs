using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Sally.NET.Core;
using Sally.NET.Core.Configuration;
using Sally.NET.DataAccess.Database;
using Sally.NET.DataAccess.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Sally.NET.Service
{
    public static class WeatherSubscriptionService
    {
        public static DiscordSocketClient client { get; set; }
        //public static BotCredentials credentials { get; set; }

        public static void InitializeWeatherSub(DiscordSocketClient client, BotCredentials credentials)
        {
            WeatherSubscriptionService.client = client;
            //WeatherSubscriptionService.credentials = credentials;
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

                dynamic temperature = JsonConvert.DeserializeObject<dynamic>(await ApiRequestService.request2weatherAsync(user.WeatherLocation));

                EmbedBuilder weatherEmbed = new EmbedBuilder()
                    .WithTitle("Weather Info")
                    .WithDescription("Weather Notification for today")
                    .AddField(user.WeatherLocation, $"{temperature.main.temp} °C")
                    .AddField("Max. Temp today", $"{temperature.main.temp_max} °C")
                    .AddField("Min. Temp for today", $"{temperature.main.temp_min} °C")
                    .AddField("Weather Condition", (string)temperature.weather[0].main)
                    .WithColor(Color.Blue)
                    .WithTimestamp(DateTime.Now)
                    .WithFooter(FileAccess.GENERIC_FOOTER, FileAccess.GENERIC_THUMBNAIL_URL);
                await disUser.SendMessageAsync(embed: weatherEmbed.Build()).ConfigureAwait(false);
            }
        }
    }
}
