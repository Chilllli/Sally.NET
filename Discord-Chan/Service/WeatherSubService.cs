using Discord;
using Discord.WebSocket;
using Sally_NET.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Web;

namespace Sally_NET.Service
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

                dynamic temperature = JsonConvert.DeserializeObject<dynamic>(ApiRequestService.StartRequest("weatherapi", location: user.WeatherLocation).Result);

                EmbedBuilder weatherEmbed = new EmbedBuilder()
                    .WithTitle("Weather Info")
                    .WithDescription("Weather Notification for today")
                    .AddField(user.WeatherLocation, $"{temperature.main.temp} °C")
                    .AddField("Max. Temp today", $"{temperature.main.temp_max} °C")
                    .AddField("Min. Temp for today", $"{temperature.main.temp_min} °C")
                    .AddField("Weather Condition", (string)temperature.weather[0].main)
                    .WithColor(Color.Blue)
                    .WithTimestamp(DateTime.Now)
                    .WithFooter(Program.GenericFooter, Program.GenericThumbnailUrl);
                await disUser.SendMessageAsync(embed: weatherEmbed.Build()).ConfigureAwait(false);
            }
        }
    }
}
