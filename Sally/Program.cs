using Discord;
using Discord.WebSocket;
using Sally.Command;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Generic;
using Discord.Commands;
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
using System.Collections.Immutable;

namespace Sally
{
    class Program
    {
        public static BotCredentials BotConfiguration
        {
            get;
            private set;
        }

        public static DiscordSocketClient Client
        {
            get;
            private set;
        }

        public static SocketGuild MyGuild
        {
            get;
            private set;
        }
        public static SocketUser Me
        {
            get;
            private set;
        }

        public static DateTime StartTime
        {
            get;
            private set;
        }
        private static int requestCounter;
        public static int RequestCounter
        {
            get
            {
                return requestCounter;
            }
            set
            {
                requestCounter = value;
                File.WriteAllText("ApiRequests.txt", requestCounter.ToString());
            }
        }
        private static int startValue;
        private Dictionary<ulong, char> prefixDictionary;
        public static readonly ILog Logging = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public ConfigManager CredentialManager;
        private static ILog consoleLogger = LogManager.GetLogger("console");
        private static ILog fileLogger = LogManager.GetLogger("file");
        private static DateTime startTime { get; set; }


        public static void Main(string[] args)
        {
            startTime = DateTime.Now;
            if (args.Length > 0)
            {
                startValue = Int32.Parse(args[0]);
            }
            Console.CancelKeyPress += Console_CancelKeyPress;
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            DatabaseAccess.Instance.Dispose();
            Environment.Exit(0);
        }

        public async Task MainAsync()
        {
            ILoggerRepository logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
            InitializeDirectories();
            StartTime = DateTime.Now;
            string[] moods = { "Sad", "Meh", "Happy", "Extatic" };

            //download content
            using (var client = new WebClient())
            {
                foreach (string item in moods)
                {
                    if (!File.Exists($"mood/{item}.png"))
                    {
                        client.DownloadFile($"https://cdn.its-sally.net/content/{item}.png", $"mood/{item}.png");
                    }
                    if (!File.Exists($"mood/{item}.json"))
                    {
                        client.DownloadFile($"https://cdn.its-sally.net/content/{item}.json", $"mood/{item}.json");
                    }
                }
            }

            BotConfiguration = JsonConvert.DeserializeObject<BotCredentials>(File.ReadAllText("config/configuration.json"));
            await DatabaseAccess.InitializeAsync(BotConfiguration.DbUser, BotConfiguration.DbPassword, BotConfiguration.Db, BotConfiguration.DbHost);
            CredentialManager = new ConfigManager(BotConfiguration);


            if (!File.Exists("meta/prefix.json"))
            {
                File.Create("meta/prefix.json").Dispose();
            }
            //store in database
            prefixDictionary = new Dictionary<ulong, char>();
            prefixDictionary = JsonConvert.DeserializeObject<Dictionary<ulong, char>>(File.ReadAllText("meta/prefix.json"));
            if (prefixDictionary == null)
            {
                prefixDictionary = new Dictionary<ulong, char>();
            }

            if (!File.Exists("ApiRequests.txt"))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText("ApiRequests.txt"))
                {
                    sw.WriteLine("0");
                    sw.Close();
                }
            }
            RequestCounter = Int32.Parse(File.ReadAllText("ApiRequests.txt"));

            Client = new DiscordSocketClient();


            Client.Ready += Client_Ready;
            Client.Log += Log;

            await Client.LoginAsync(TokenType.Bot, BotConfiguration.Token);
            await Client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private void checkNewUserEntries()
        {
            List<SocketGuild> guilds = Client.Guilds.ToList();
            foreach (SocketGuild guild in guilds)
            {
                List<SocketGuildUser> guildUsers = guild.Users.ToList();
                foreach (SocketGuildUser guildUser in guildUsers)
                {
                    if (guildUser.Id == BotConfiguration.MeId)
                    {
                        Me = guildUser;
                    }
                    //check if user exist in global instance
                    User myUser = DatabaseAccess.Instance.Users.Find(u => u.Id == guildUser.Id);
                    if (myUser == null)
                    {
                        DatabaseAccess.Instance.InsertUser(new User(guildUser.Id, true));
                    }
                    myUser = DatabaseAccess.Instance.Users.Find(u => u.Id == guildUser.Id);
                    //check if guilduser exist in guild instance
                    if (!myUser.GuildSpecificUser.ContainsKey(guild.Id))
                    {
                        DatabaseAccess.Instance.InsertGuildUser(guild.Id, new GuildUser(guildUser.Id, guild.Id, 500));
                    }
                    //check if user is already in a voice channel
                    if (guildUser.VoiceChannel != null)
                    {
                        //start tracking if user detected
                        VoiceRewardService.StartTrackingVoiceChannel(myUser.GuildSpecificUser[guild.Id]);
                    }
                }
            }
        }
        private Task Log(LogMessage msg)
        {
            consoleLogger.Info(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task Client_Ready()
        {
            IServiceProvider services = new ServiceCollection()
              .AddSingleton(Client)
              .AddSingleton<CleverbotApiHandler>()
              .AddSingleton<ColornamesApiHandler>()
              .AddSingleton<KonachanApiHandler>()
              .AddSingleton<WeatherApiHandler>()
              .AddSingleton<WikipediaApiHandler>()
              .AddSingleton(CredentialManager)
              .BuildServiceProvider();
            AddonLoader.Load(Client);
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> commandClasses = new List<Type>();

            foreach (Assembly assembly in assemblies)
            {
                commandClasses.AddRange(assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(ModuleBase)) && !t.IsAbstract).ToList());
            }
            VoiceRewardService.InitializeHandler(Client, BotConfiguration, !CredentialManager.OptionalSettings.Contains("CleverApi"));
            checkNewUserEntries();
            StatusNotifierService.InitializeService(Me);
            MusicCommands.Initialize(Client);
            RoleManagerService.InitializeHandler(Client, BotConfiguration);
            UserManagerService.InitializeHandler(Client, fileLogger);
            await CommandHandlerService.InitializeHandler(Client, BotConfiguration, commandClasses, prefixDictionary, !CredentialManager.OptionalSettings.Contains("CleverApi"), fileLogger, services);
            CacheService.InitializeHandler();
            switch (startValue)
            {
                case 0:
                    //shutdown
                    break;
                case 1:
                    //restarting
                    await Me.SendMessageAsync("I have restored and restarted successfully.");
                    break;
                case 2:
                    //updating
                    //check if an update is nessasarry
                    await Me.SendMessageAsync("I at all the new features and restarted successfully.");
                    break;
                default:
                    break;
            }
            if (!CredentialManager.OptionalSettings.Contains("WeatherApiKey"))
            {
                if (!CredentialManager.OptionalSettings.Contains("WeatherPlace"))
                {
                    MoodDictionary.InitializeMoodDictionary(Client, BotConfiguration);
                    await MoodHandlerService.InitializeHandler(Client, BotConfiguration, services.GetRequiredService<WeatherApiHandler>(), fileLogger);
                }
                WeatherSubscriptionService.InitializeWeatherSub(Client, BotConfiguration, services.GetRequiredService<WeatherApiHandler>());
            }
            consoleLogger.Info($"Addons loaded: {AddonLoader.LoadedAddonsCount}");
            consoleLogger.Info($"User loaded: {DatabaseAccess.Instance.Users.Count}");
            consoleLogger.Info($"Registered guilds: {Client.Guilds.Count}");
            consoleLogger.Info($"Bot start up time: {(DateTime.Now - startTime).TotalSeconds} s");
        }

        private static void InitializeDirectories()
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
