using Discord;
using Discord.Commands;
using Discord.WebSocket;
using log4net;
using log4net.Core;
using Microsoft.Extensions.DependencyInjection;
using MySqlX.XDevAPI;
using Sally.NET.Core;
using Sally.NET.Core.Configuration;
using Sally.NET.Core.Enum;
using Sally.NET.DataAccess.Database;
using Sally.NET.Handler;
using Sally.NET.Module;
using Sally.NET.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sally
{
    public class Bot
    {
        private readonly ILog logger;
        private readonly DiscordSocketClient client;
        private readonly IDBAccess dBAccess;
        private readonly CommandHandlerService commandService;
        private readonly IServiceProvider serviceProvider;
        private readonly VoiceRewardService voiceService;
        private readonly BotCredentials credentials;

        public Bot(ILog logger, DiscordSocketClient client, IDBAccess dBAccess, CommandHandlerService commandService, IServiceProvider serviceProvider, VoiceRewardService voiceService, BotCredentials credentials)
        {
            this.logger = logger;
            this.client = client;
            this.dBAccess = dBAccess;
            this.commandService = commandService;
            this.serviceProvider = serviceProvider;
            this.voiceService = voiceService;
            this.credentials = credentials;
            client.Ready += Client_Ready;
            client.Log += Log;
        }

        public async Task RunAsync()
        {
            await commandService.Start(serviceProvider);
            voiceService.Start();
            await client.LoginAsync(TokenType.Bot, credentials.Token);
            await client.StartAsync();
        }

        private async Task Client_Ready()
        {
            //AddonLoader.Load(Client);
            //checkNewUserEntries(dBAccess);
            //StatusNotifierService.InitializeService(Me);
            //MusicCommands.Initialize(Client);
            //RoleManagerService.InitializeHandler(Client, BotConfiguration);
            //UserManagerService.InitializeHandler(Client, fileLogger, dBAccess);
            //CacheService.InitializeHandler();
            //switch (startValue)
            //{
            //    case 0:
            //        //shutdown
            //        break;
            //    case 1:
            //        //restarting
            //        await Me.SendMessageAsync("I have restored and restarted successfully.");
            //        break;
            //    case 2:
            //        //updating
            //        //check if an update is nessasarry
            //        await Me.SendMessageAsync("I at all the new features and restarted successfully.");
            //        break;
            //    default:
            //        break;
            //}
            //if (!CredentialManager.OptionalSettings.Contains("WeatherApiKey"))
            //{
            //    WeatherSubscriptionService.InitializeWeatherSub(Client, BotConfiguration, services.GetRequiredService<WeatherApiHandler>(), dBAccess);
            //}
            logger.Info($"Addons loaded: {AddonLoader.LoadedAddonsCount}");
            logger.Info($"User loaded: {dBAccess.GetUsers().Count}");
            logger.Info($"Registered guilds: {client.Guilds.Count}");
            //logger.Info($"Bot start up time: {(DateTime.Now - startTime).TotalSeconds} s");
            await client.SetGameAsync("$help", type: ActivityType.Watching);
        }

        private Task Log(LogMessage msg)
        {
            logger.Info(msg.ToString());
            return Task.CompletedTask;
        }

    }
}
