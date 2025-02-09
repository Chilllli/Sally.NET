using Discord;
using Discord.WebSocket;
using Sally.Command;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;
using log4net;
using log4net.Config;
using log4net.Repository;
using Sally.NET.Service;
using Sally.NET.DataAccess.Database;
using Sally.NET.Core.Configuration;
using Sally.NET.Core;
using Sally.NET.Handler;
using Microsoft.Extensions.DependencyInjection;
using Sally.NET.Core.Enum;
using Sally.NET.Module;
using Microsoft.Extensions.Hosting;

namespace Sally
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            await new Program().MainAsync(args);
        }

        public async Task MainAsync(string[] args)
        {
            ILoggerRepository logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
            var logger = LogManager.GetLogger("Logger");
            initializeDirectories();
            var botConfiguration = JsonConvert.DeserializeObject<BotCredentials>(File.ReadAllText("config/configuration.json"));
            if (botConfiguration is null)
            {
                throw new Exception("Configuration file not found");
            }
            if (!validateConfig(botConfiguration, out string message))
            {
                logger.Error(message);
                return;
            }
            var CredentialManager = new ConfigManager(botConfiguration);

            var client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                AlwaysDownloadUsers = true,
                GatewayIntents = GatewayIntents.DirectMessageReactions |
                GatewayIntents.DirectMessages |
                GatewayIntents.DirectMessageTyping |
                GatewayIntents.GuildBans |
                GatewayIntents.GuildEmojis |
                GatewayIntents.GuildIntegrations |
                GatewayIntents.GuildMembers |
                GatewayIntents.GuildMessageReactions |
                GatewayIntents.GuildMessages |
                GatewayIntents.GuildMessageTyping |
                GatewayIntents.Guilds |
                GatewayIntents.GuildVoiceStates |
                GatewayIntents.GuildWebhooks |
                GatewayIntents.MessageContent
            });


            IHost host = Host.CreateDefaultBuilder(args).ConfigureServices((_, services) =>
            {
                services.AddSingleton(client)
                .AddSingleton(botConfiguration)
                .AddSingleton<CleverbotApiHandler>()
                .AddSingleton<ColornamesApiHandler>()
                .AddSingleton<KonachanApiHandler>()
                .AddSingleton<WeatherApiHandler>()
                .AddSingleton<WikipediaApiHandler>()
                .AddSingleton<MusicModule>()
                .AddSingleton<GeneralModule>()
                .AddSingleton(CredentialManager)
                .AddSingleton<Helper>()
                .AddSingleton(logger)
                .AddSingleton<GameModule>()
                .AddSingleton<CommandHandlerService>()
                .AddSingleton<Bot>()
                .AddSingleton<VoiceRewardService>()
                .AddSingleton<OAuthHttpListener>();
                services.AddHttpClient<CleverbotApiHandler>(c => c.BaseAddress = new("https://www.cleverbot.com"));
                services.AddHttpClient<ColornamesApiHandler>(c => c.BaseAddress = new("https://colornames.org"));
                services.AddHttpClient<KonachanApiHandler>(c => c.BaseAddress = new("https://konachan.com"));
                services.AddHttpClient<WeatherApiHandler>(c => c.BaseAddress = new("https://api.openweathermap.org"));
                services.AddHttpClient<WikipediaApiHandler>(c => c.BaseAddress = new("https://en.wikipedia.org"));

                switch (botConfiguration.SQLType)
                {
                    case SQLType.Sqlite:
                        services.AddSingleton<IDBAccess>(new SQLiteAccess(botConfiguration.SqliteConnectionString));
                        break;
                    case SQLType.MySQL:
                        services.AddSingleton<IDBAccess>(new MySQLAccess(botConfiguration.DbUser, botConfiguration.DbPassword, botConfiguration.Db, botConfiguration.DbHost));
                        break;
                    default:

                        break;
                }
            }).Build();

            var bot = host.Services.GetRequiredService<Bot>();
            await bot.RunAsync();
            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private bool validateConfig(BotCredentials botConfiguration, out string message)
        {
            message = "";
            switch (botConfiguration.SQLType)
            {
                case SQLType.Sqlite:
                    if (String.IsNullOrWhiteSpace(botConfiguration.SqliteConnectionString))
                    {
                        message = "If using Sqlite: provide a valid sqlite connection string";
                        return false;
                    }
                    break;
                case SQLType.MySQL:
                    if (String.IsNullOrWhiteSpace(botConfiguration.Db))
                    {
                        message = "If using MySQL: provide a valid database name";
                        return false;
                    }
                    if (String.IsNullOrWhiteSpace(botConfiguration.DbUser))
                    {
                        message = "If using MySQL: provide a valid db user";
                        return false;
                    }
                    if (String.IsNullOrWhiteSpace(botConfiguration.DbPassword))
                    {
                        message = "If using MySQL: provide a valid db password";
                        return false;
                    }
                    if (String.IsNullOrWhiteSpace(botConfiguration.DbHost))
                    {
                        message = "If using MySQL: provide a valid db host";
                        return false;
                    }
                    break;
                default:
                    message = "can't determine sql type";
                    return false;
            }
            return true;
        }

        //private async Task checkNewUserEntriesAsync<T>(T dbAccess) where T : IDBAccess
        //{
        //    foreach (SocketGuild guild in Client.Guilds)
        //    {
        //        foreach (SocketGuildUser guildUser in guild.Users)
        //        {
        //            if (guildUser.Id == BotConfiguration.MeId)
        //            {
        //                Me = guildUser;
        //            }
        //            //check if user exist in global instance
        //            User? myUser = await dbAccess.GetUserAsync(guildUser.Id);
        //            if (myUser == null)
        //            {
        //                await dbAccess.InsertUserAsync(new User(guildUser.Id, true));
        //            }
        //            myUser = await dbAccess.GetUserAsync(guildUser.Id);
        //            //check if guilduser exist in guild instance
        //            if (!myUser!.GuildSpecificUser.ContainsKey(guild.Id))
        //            {
        //                if (await dbAccess.GetGuildUserAsync(guildUser.Id, guild.Id) == null)
        //                {
        //                    await dbAccess.InsertGuildUserAsync(new GuildUser(guildUser.Id, guild.Id, 500));
        //                }
        //            }
        //            //check if user is already in a voice channel
        //            if (guildUser.VoiceChannel != null)
        //            {
        //                //start tracking if user detected
        //                //VoiceRewardService.StartTrackingVoiceChannel(myUser.GuildSpecificUser[guild.Id]);
        //            }
        //        }
        //    }
        //}



        private static void initializeDirectories()
        {
            string[] directories = {
                "mood",
                "meta",
                "addons",
                "cached",
                "config"
            };

            foreach (string directory in directories)
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
        }
    }
}
