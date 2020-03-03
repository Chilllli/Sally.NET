using Discord;
using Discord.WebSocket;
using Sally.Command;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Sally.NET.Core.Configuration;
using Sally.NET.DataAccess.Database;
using Sally.NET.Service;
using System.Collections.Generic;
using Discord.Commands;
using Sally.NET.Core;

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
        public static string GenericFooter 
        {
            get;
            private set;
        }
        public static string GenericThumbnailUrl
        {
            get;
            private set;
        }
        private static int startValue;
        private static Dictionary<ulong, char> prefixDictionary;

        public static void Main(string[] args)
        {
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
            StartTime = DateTime.Now;
            string[] moods = { "Sad", "Meh", "Happy", "Extatic" };
            if (!Directory.Exists("mood"))
            {
                Directory.CreateDirectory("mood");
            }
            //download content
            using (var client = new WebClient())
            {
                foreach (string item in moods)
                {
                    if (!File.Exists($"mood/{item}.png"))
                    {
                        client.DownloadFile($"https://cdn.its-sally.net/content/{item}.png", $"mood/{item}.png");
                    }
                }
            }

            BotConfiguration = JsonConvert.DeserializeObject<BotCredentials>(File.ReadAllText("configuration.json"));
            DatabaseAccess.Initialize(BotConfiguration.db_user, BotConfiguration.db_password, BotConfiguration.db_database);

            if (!Directory.Exists("meta"))
            {
                Directory.CreateDirectory("meta");
            }
            if (!File.Exists("meta/prefix.json"))
            {
                File.Create("meta/prefix.json").Dispose();
            }
            prefixDictionary = new Dictionary<ulong, char>();
            prefixDictionary = JsonConvert.DeserializeObject<Dictionary<ulong, char>>(File.ReadAllText("meta/prefix.json"));
            if(prefixDictionary == null)
            {
                prefixDictionary = new Dictionary<ulong, char>();
            }

            RequestCounter = Int32.Parse(File.ReadAllText("ApiRequests.txt"));

            Client = new DiscordSocketClient();

            List<Type> commandClasses = typeof(GeneralCommands)
                    .Assembly.GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(ModuleBase)) && !t.IsAbstract).ToList();

            LoggerService.Initialize();
            ApiRequestService.Initialize(BotConfiguration);
            VoiceRewardService.InitializeHandler(Client, BotConfiguration);
            UserManagerService.InitializeHandler(Client);
            MoodDictionary.InitializeMoodDictionary(Client, BotConfiguration);
            WeatherSubscriptionService.InitializeWeatherSub(Client, BotConfiguration);
            RoleManagerService.InitializeHandler(BotConfiguration);
            await CommandHandlerService.InitializeHandler(Client, BotConfiguration, commandClasses, prefixDictionary);
            await CacheService.InitializeHandler();
            Client.Ready += Client_Ready;

            Client.Log += Log;

            await Client.LoginAsync(TokenType.Bot, BotConfiguration.token);
            await Client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task Client_Ready()
        {
            GenericFooter = "Provided by Sally, your friendly and helpful Discordbot!";
            GenericThumbnailUrl = "https://static-cdn.jtvnw.net/emoticons/v1/279825/3.0";

            MyGuild = Client.Guilds.Where(g => g.Id == BotConfiguration.guildId).First();
            foreach (SocketGuildUser user in MyGuild.Users)
            {
                if (DatabaseAccess.Instance.users.Find(u => u.Id == user.Id) == null)
                {
                    DatabaseAccess.Instance.InsertUser(new User(user.Id, 10, false));
                }
                //check if user is already in a voice channel
                if (user.VoiceChannel != null)
                {
                    //start tracking if user detected
                    VoiceRewardService.StartTrackingVoiceChannel(DatabaseAccess.Instance.users.Find(u => u.Id == user.Id));
                }
                if (user.Id == BotConfiguration.meId)
                {
                    Me = user as SocketUser;
                }
            }
            StatusNotifierService.InitializeService(Me);
            MusicCommands.Initialize(Client);
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
            await MoodHandlerService.InitializeHandler(Client, BotConfiguration);
            
        }
    }
}
