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
using Sally.NET.Core.Configuration;

namespace Sally.Command
{
    public class WeatherCommands : ModuleBase
    {
        private readonly WeatherApiHandler weatherApiHandler;
        private readonly ConfigManager configManager;
        private readonly IDBAccess dbAccess;
        private readonly BotCredentials credentials;

        public WeatherCommands(WeatherApiHandler weatherApiHandler, ConfigManager configManager, IDBAccess dbAccess, BotCredentials credentials)
        {
            this.weatherApiHandler = weatherApiHandler;
            this.configManager = configManager;
            this.dbAccess = dbAccess;
            this.credentials = credentials;
        }
        [Command("sub2weather")]
        public async Task SubToService(string location, TimeSpan notiferTime)
        {
            if (configManager.OptionalSettings.Contains("WeatherApiKey"))
            {
                return;
            }
            if (notiferTime >= new TimeSpan(24, 0, 0))
            {
                await Context.Message.Channel.SendMessageAsync("TimeSpan need to be between 00:00 and 23:59");
                return;
            }

            if (!weatherApiHandler.TryGetWeatherApi(credentials.WeatherApiKey, location, out WeatherApi weatherApi))
            {
                await Context.Message.Channel.SendMessageAsync("The request returned an error.");
                return;
            }

            User currentUser = dbAccess.GetUser(Context.Message.Author.Id);
            currentUser.WeatherLocation = location;
            currentUser.NotifierTime = notiferTime;
            dbAccess.UpdateUser(currentUser);
            await Context.Message.Channel.SendMessageAsync($"{Context.Message.Author}, you successfully subbed to weather notifications.");
        }
        [Command("unsub2weather")]
        public async Task UnSubToService()
        {
            if (configManager.OptionalSettings.Contains("WeatherApiKey"))
            {
                return;
            }
            User currentUser = dbAccess.GetUser(Context.Message.Author.Id);
            currentUser.WeatherLocation = null;
            currentUser.NotifierTime = null;
            dbAccess.UpdateUser(currentUser);
            await Context.Message.Channel.SendMessageAsync($"{Context.Message.Author}, you successfully unsubbed to weather notifications.");
        }
        [Command("currentWeather")]
        public async Task CheckCurrentWeather(string location)
        {
            var myUser = dbAccess.GetUser(Context.User.Id);
            if (configManager.OptionalSettings.Contains("WeatherApiKey"))
            {
                return;
            }
            WeatherApi apiResult = await weatherApiHandler.GetWeatherApiResultAsync(credentials.WeatherApiKey, location);
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
                    .WithColor(new Color((uint)Convert.ToInt32(myUser.EmbedColor, 16)))
                    .WithTimestamp(DateTime.Now)
                    .WithFooter(NET.DataAccess.File.FileAccess.GENERIC_FOOTER, NET.DataAccess.File.FileAccess.GENERIC_THUMBNAIL_URL);
            await Context.Message.Channel.SendMessageAsync(embed: weatherEmbed.Build()).ConfigureAwait(false);
        }
    }
}