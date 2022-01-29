using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Sally.NET.DataAccess.Database;
using Sally.NET.Core;
using Sally.NET.Service;
using Sally.NET.Handler;
using Sally.NET.Module;
using Sally.NET.Core.ApiReference;
using System.Linq;

namespace Sally.Command
{
    public class WeatherCommands : ModuleBase
    {
        private readonly WeatherApiHandler weatherApiHandler;
        public WeatherCommands(WeatherApiHandler weatherApiHandler)
        {
            this.weatherApiHandler = weatherApiHandler;
        }
        [Command("sub2weather")]
        public async Task SubToService(string location, TimeSpan notiferTime)
        {
            if (Program.CredentialManager.OptionalSettings.Contains("WeatherApiKey"))
            {
                return;
            }
            if (notiferTime >= new TimeSpan(24, 0, 0))
            {
                await Context.Message.Channel.SendMessageAsync("TimeSpan need to be between 00:00 and 23:59");
                return;
            }

            if (!weatherApiHandler.TryGetWeatherApi(Program.BotConfiguration.WeatherApiKey, location, out WeatherApi weatherApi))
            {
                await Context.Message.Channel.SendMessageAsync("The request returned an error.");
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
            if (Program.CredentialManager.OptionalSettings.Contains("WeatherApiKey"))
            {
                return;
            }
            User currentUser = DatabaseAccess.Instance.Users.Find(u => u.Id == Context.Message.Author.Id);
            currentUser.WeatherLocation = null;
            currentUser.NotifierTime = null;
            await Context.Message.Channel.SendMessageAsync($"{Context.Message.Author}, you successfully unsubbed to weather notifications.");
        }
        [Command("currentWeather")]
        public async Task CheckCurrentWeather(string location)
        {
            if (Program.CredentialManager.OptionalSettings.Contains("WeatherApiKey"))
            {
                return;
            }
            WeatherApi apiResult = weatherApiHandler.GetWeatherApiResult(Program.BotConfiguration.WeatherApiKey, location);
            if (apiResult.StatusCode != 200)
            {
                await Context.Message.Channel.SendMessageAsync("Warn: can't process request");
                return;
            }
            EmbedBuilder weatherEmbed = new EmbedBuilder()
                    .WithTitle("Weather Info")
                    .WithDescription("Current Weather Informations")
                    .AddField(location, $"{apiResult.Weather.Temperature} °C")
                    .AddField("Max. Temp today", $"{apiResult.Weather.MaxTemperature} °C")
                    .AddField("Min. Temp for today", $"{apiResult.Weather.MinTemperature} °C")
                    .AddField("Weather Condition", apiResult.WeatherCondition.First().ShortDescription)
                    .WithColor(new Color((uint)Convert.ToInt32(CommandHandlerService.MessageAuthor.EmbedColor, 16)))
                    .WithTimestamp(DateTime.Now)
                    .WithFooter(NET.DataAccess.File.FileAccess.GENERIC_FOOTER, NET.DataAccess.File.FileAccess.GENERIC_THUMBNAIL_URL);
            await Context.Message.Channel.SendMessageAsync(embed: weatherEmbed.Build()).ConfigureAwait(false);
        }
    }
}