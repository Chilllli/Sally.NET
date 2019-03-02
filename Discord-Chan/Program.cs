using Discord;
using Discord.Addons.Interactive;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Chan.commands;
using Discord_Chan.config;
using Discord_Chan.db;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Discord_Chan
{
    class Program
    {
        public static BotConfiguration botConfiguration;
        private CommandService commands;
        private DiscordSocketClient client;
        private IServiceProvider services;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {

            botConfiguration = JsonConvert.DeserializeObject<BotConfiguration>(File.ReadAllText("configuration.json"));
            DataAccess.Initialize(botConfiguration);

            client = new DiscordSocketClient();
            commands = new CommandService();
            services = new ServiceCollection()
              .AddSingleton(client)
              .AddSingleton<InteractiveService>()
              .BuildServiceProvider();

            await InitializeCommands();

            client.Log += Log;

            await client.LoginAsync(TokenType.Bot, botConfiguration.token);
            await client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);

        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public async Task InitializeCommands()
        {
            client.MessageReceived += CommandHandler;
            client.Ready += Client_Ready;
            client.GuildMemberUpdated += Client_GuildMemberUpdated;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }//Assembly.GetEntryAssembly()

        private async Task Client_GuildMemberUpdated(SocketGuildUser userOld, SocketGuildUser userNew)
        {
            if(userOld == null && userNew != null)
            {
                if (DataAccess.Instance.users.Find(u => u.Id == userNew.Id) == null)
                {
                    DataAccess.Instance.InsertUser(new User() { Id = userNew.Id, Xp = 10 });
                }
            }            
        }

        private async Task Client_Ready()
        {
            MusicCommands.Initialize(client);
            // IAudioClient voiceChannel = await client.Guilds.Where(g => g.Name == "Its better together!").First().VoiceChannels.Where(c => c.Name == "Türschwelle").First().ConnectAsync();
            //  MusicCommands.audioClient = voiceChannel;
            foreach (SocketGuildUser user in client.Guilds.Where(g => g.Id == 316621565305421825).First().Users)
            {
                if(DataAccess.Instance.users.Find(u => u.Id == user.Id) == null)
                {
                    DataAccess.Instance.InsertUser(new User() { Id=user.Id, Xp=10});
                }
            }
        }

        private async Task CommandHandler(SocketMessage arg)
        {
            // Don't process the command if it was a System Message
            SocketUserMessage message = arg as SocketUserMessage;
            if (message == null) return;
            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos) || message.Content == PingCommand.PongMessage)) return;
            // Create a Command Context
            SocketCommandContext context = new SocketCommandContext(client, message);
            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)
            IResult result = await commands.ExecuteAsync(context, argPos, services);
            //Error Handler
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}
