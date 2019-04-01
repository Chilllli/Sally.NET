using Discord;
using Discord.WebSocket;
using Discord_Chan.commands;
using Discord_Chan.config;
using Discord_Chan.db;
using Discord_Chan.services;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Chan
{
    class Program
    {
        public static BotConfiguration BotConfiguration
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
       

        public static void Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            DataAccess.Instance.Dispose();
            Environment.Exit(0);
        }

        public async Task MainAsync()
        {

            BotConfiguration = JsonConvert.DeserializeObject<BotConfiguration>(File.ReadAllText("configuration.json"));
            DataAccess.Initialize(BotConfiguration);

            Client = new DiscordSocketClient();

            VoiceRewardService.InitializeHandler(Client);
            UserManagerService.InitializeHandler(Client);
            MoodDictionary.InitializeMoodDictionary();
            WeatherSubService.InitializeWeatherSub();
            await RoleManagerService.InitializeHandler();
            await CommandHandlerService.InitializeHandler(Client);
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
            MyGuild = Client.Guilds.Where(g => g.Id == BotConfiguration.guildId).First();
            foreach (SocketGuildUser user in MyGuild.Users)
            {
                if (DataAccess.Instance.users.Find(u => u.Id == user.Id) == null)
                {
                    DataAccess.Instance.InsertUser(new User(user.Id, 10, false));
                }
            }
            StatusNotifierService.InitializeService(BotConfiguration);
            MusicCommands.Initialize(Client);
            await MoodHandleService.InitializeHandler(Client);
        }
    }
}
