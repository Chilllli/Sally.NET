using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Sally.NET.DataAccess.Database;
using Sally.NET.Core;
using Sally.NET.Service;

namespace Sally.Command
{
    public class WeatherCommands : ModuleBase
    {
        [Command("sub2weather")]
        public async Task SubToService(string location, TimeSpan notiferTime)
        {
            if (notiferTime >= new TimeSpan(24, 0, 0))
            {
                await Context.Message.Channel.SendMessageAsync("TimeSpan need to be between 00:00 and 23:59");
                return;
            }

            dynamic temperature = JsonConvert.DeserializeObject<dynamic>(await ApiRequestService.Request2WeatherApiAsync(location));
            if(temperature.cod != 200)
            {
                await Context.Message.Channel.SendMessageAsync((string)temperature.message);
                return;
            }

            User currentUser = DatabaseAccess.Instance.Users.Find(u => u.Id == Context.Message.Author.Id);
            currentUser.WeatherLocation = location;
            currentUser.NotifierTime = notiferTime;
            await Context.Message.Channel.SendMessageAsync($"{Context.Message.Author}, you successfully subbed to weather notifications.");
        }
        [Command("unsub2weather")]
        public async Task UnSubToService()
        {
            User currentUser = DatabaseAccess.Instance.Users.Find(u => u.Id == Context.Message.Author.Id);
            currentUser.WeatherLocation = null;
            currentUser.NotifierTime = null;
            await Context.Message.Channel.SendMessageAsync($"{Context.Message.Author}, you successfully unsubbed to weather notifications.");
        }
        [Command("currentWeather")]
        public async Task CheckCurrentWeather(string location)
        {
            dynamic temperature = JsonConvert.DeserializeObject<dynamic>(await ApiRequestService.Request2WeatherApiAsync(location));
            if (temperature.cod != 200)
            {
                await Context.Message.Channel.SendMessageAsync((string)temperature.message);
                return;
            }
            EmbedBuilder weatherEmbed = new EmbedBuilder()
                    .WithTitle("Weather Info")
                    .WithDescription("Current Weather Informations")
                    .AddField(location, $"{temperature.main.temp} °C")
                    .AddField("Current Max. Temp", $"{temperature.main.temp_max} °C")
                    .AddField("Current Min. Temp", $"{temperature.main.temp_min} °C")
                    .AddField("Current Weather Condition", (string)temperature.weather[0].main)
                    .WithColor(new Color((uint)Convert.ToInt32(CommandHandlerService.MessageAuthor.EmbedColor, 16)))
                    .WithTimestamp(DateTime.Now)
                    .WithFooter(NET.DataAccess.File.FileAccess.GENERIC_FOOTER, NET.DataAccess.File.FileAccess.GENERIC_THUMBNAIL_URL);
            await Context.Message.Channel.SendMessageAsync(embed: weatherEmbed.Build()).ConfigureAwait(false);
        }
    }
}