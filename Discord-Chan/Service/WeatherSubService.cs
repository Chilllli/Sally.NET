﻿using Discord;
using Discord.WebSocket;
using Discord_Chan.Db;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Web;

namespace Discord_Chan.Service
{
    static class WeatherSubService
    {
        public static void InitializeWeatherSub()
        {
            Timer checkWeather = new Timer(60 * 1000);
            checkWeather.Start();
            checkWeather.Elapsed += CheckWeather_Elapsed;
        }

        private static async void CheckWeather_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (User user in DataAccess.Instance.users)
            {
                if (user.NotifierTime == null || user.WeatherLocation == null)
                    continue;
                if (!(DateTime.Now.Hour == user.NotifierTime.Value.Hours && DateTime.Now.Minute == user.NotifierTime.Value.Minutes))
                    continue;
                SocketUser disUser = Program.MyGuild.Users.ToList().Find(u => u.Id == user.Id) as SocketUser;
                if (disUser == null)
                    continue;

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://api.openweathermap.org");
                HttpResponseMessage response = await client.GetAsync($"/data/2.5/weather?q={HttpUtility.UrlEncode(user.WeatherLocation, Encoding.UTF8)}&appid={Program.BotConfiguration.WeatherApiKey}&units=metric");

                string stringResult = await response.Content.ReadAsStringAsync();

                dynamic temperature = JsonConvert.DeserializeObject<dynamic>(stringResult);

                EmbedBuilder weatherEmbed = new EmbedBuilder()
                    .WithTitle("Weather Info")
                    .WithDescription("Weather Notification for today")
                    .AddField(user.WeatherLocation, $"{temperature.main.temp} °C")
                    .AddField("Max. Temp today", $"{temperature.main.temp_max} °C")
                    .AddField("Min. Temp for today", $"{temperature.main.temp_min} °C")
                    .AddField("Weather Condition", (string)temperature.weather[0].main)
                    .WithColor(Color.Blue)
                    .WithTimestamp(DateTime.Now);
                await disUser.SendMessageAsync(embed: weatherEmbed.Build()).ConfigureAwait(false);
            }
        }
    }
}