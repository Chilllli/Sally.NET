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
using System.Timers;

namespace Discord_Chan
{
    class Program
    {
        public static BotConfiguration botConfiguration;
        private CommandService commands;
        private DiscordSocketClient client;
        private IServiceProvider services;
        private SocketGuild myGuild;
        private int xp = 20;
        private int xpTiming = 5 * 1000 * 60;

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
            client.GuildMemberUpdated += userJoined;
            client.GuildMemberUpdated += voiceChannelJoined;
            client.GuildMemberUpdated += voiceChannelLeft;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }//Assembly.GetEntryAssembly()

        private async Task voiceChannelLeft(SocketGuildUser userOld, SocketGuildUser userNew)
        {
            if (userOld == null || userNew == null)
            {
                return;
            }
            if (userNew.VoiceChannel != null && userOld.VoiceChannel?.Id == userNew.VoiceChannel?.Id)
            {
                return;
            }
            stopTrackingVoiceChannel(DataAccess.Instance.users.Find(u => u.Id == userNew.Id));
        }

        private async Task voiceChannelJoined(SocketGuildUser userOld, SocketGuildUser userNew)
        {
            //if guild joined
            if(userOld == null || userNew == null)
            {
                return;
            } 
            if(userOld.VoiceChannel?.Id == userNew.VoiceChannel?.Id || userNew.VoiceChannel == null)
            {
                return;
            }
            startTrackingVoiceChannel(DataAccess.Instance.users.Find(u => u.Id == userNew.Id));
        }

        private void startTrackingVoiceChannel(User user)
        {
            user.LastXpTime = DateTime.Now;
            user.XpTimer = new Timer(xpTiming);
            user.XpTimer.Elapsed += (s,e) => trackVoiceChannel(user);
        }

        private void trackVoiceChannel(User user)
        {
            SocketGuildUser trackedUser = myGuild.Users.ToList().Find(u => u.Id == user.Id);
            if(trackedUser == null)
            {
                return;
            }
            if(trackedUser.VoiceChannel == null)
            {
                return;
            }
            user.Xp += xp;
            user.LastXpTime = DateTime.Now;


        }

        private void stopTrackingVoiceChannel(User user)
        {
            user.XpTimer.Stop();
            user.Xp += (int)Math.Round(((DateTime.Now - user.LastXpTime).TotalMilliseconds / xpTiming) * xp);
        }

        private async Task userJoined(SocketGuildUser userOld, SocketGuildUser userNew)
        {
            if(userOld != null || userNew == null)
            {
                return;
            }
            if (DataAccess.Instance.users.Find(u => u.Id == userNew.Id) == null)
            {
                User user = new User() { Id = userNew.Id, Xp = 10 };
                user.OnLevelUp += User_OnLevelUp;
                DataAccess.Instance.InsertUser(user);
            }
        }

        private async void User_OnLevelUp(User user)
        {
            SocketRole levelRole = myGuild.Roles.ToList().Find(r => r.Name == $"Level {user.Level}");
            if (levelRole == null)
            {
                await myGuild.CreateRoleAsync($"Level {user.Level}");
                levelRole = myGuild.Roles.ToList().Find(r => r.Name == $"Level {user.Level}");
            }
            SocketGuildUser gUser = myGuild.Users.ToList().Find(u => u.Id == user.Id);
            SocketRole oldLevelRole = myGuild.Roles.ToList().Find(r => r.Name == $"Level {user.Level - 1}");
            if(gUser.Roles.ToList().Find(r => r.Id == oldLevelRole.Id) == null)
            {
                await gUser.AddRoleAsync(levelRole);
            }
            await gUser.RemoveRoleAsync(oldLevelRole);
            await gUser.AddRoleAsync(levelRole);
        }

        private async Task Client_Ready()
        {
            myGuild = client.Guilds.Where(g => g.Id == 316621565305421825).First();
            MusicCommands.Initialize(client);
            // IAudioClient voiceChannel = await client.Guilds.Where(g => g.Name == "Its better together!").First().VoiceChannels.Where(c => c.Name == "Türschwelle").First().ConnectAsync();
            //  MusicCommands.audioClient = voiceChannel;
            foreach (SocketGuildUser user in myGuild.Users)
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
            if (message == null)
                return;
            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos) || message.Content == PingCommand.PongMessage))
                return;
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
