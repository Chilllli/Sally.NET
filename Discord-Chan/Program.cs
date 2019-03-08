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
using System.IO.Pipes;
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
        private SocketUser me;
        private ulong meId = 249680382499225600;

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
            client.UserVoiceStateUpdated += voiceChannelJoined;
            client.UserVoiceStateUpdated += voiceChannelLeft;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        }


        private async Task voiceChannelLeft(SocketUser disUser, SocketVoiceState voiceStateOld, SocketVoiceState voiceStateNew)
        {
            if (voiceStateNew.VoiceChannel != null || voiceStateOld.VoiceChannel?.Id == voiceStateNew.VoiceChannel?.Id)
            {
                return;
            }
            User currentUser = DataAccess.Instance.users.Find(u => u.Id == disUser.Id);
            if (!currentUser.HasMuted)
            {
                //send private message
                //await disUser.SendMessageAsync("testing");
            }
            stopTrackingVoiceChannel(DataAccess.Instance.users.Find(u => u.Id == disUser.Id));
        }

        private async Task voiceChannelJoined(SocketUser disUser, SocketVoiceState voiceStateOld, SocketVoiceState voiceStateNew)
        {
            //if guild joined
            if (voiceStateOld.VoiceChannel?.Id == voiceStateNew.VoiceChannel?.Id || voiceStateNew.VoiceChannel == null)
            {
                return;
            }
            User currentUser = DataAccess.Instance.users.Find(u => u.Id == disUser.Id);
            if (!currentUser.HasMuted)
            {
                //send private message
                //await disUser.SendMessageAsync("testing");
            }
            startTrackingVoiceChannel(currentUser);
        }

        private void startTrackingVoiceChannel(User user)
        {
            user.LastXpTime = DateTime.Now;
            user.XpTimer = new Timer(xpTiming);
            user.XpTimer.Elapsed += (s, e) => trackVoiceChannel(user);
        }

        private void trackVoiceChannel(User user)
        {
            SocketGuildUser trackedUser = myGuild.Users.ToList().Find(u => u.Id == user.Id);
            if (trackedUser == null)
            {
                return;
            }
            if (trackedUser.VoiceChannel == null)
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
            if (userOld != null || userNew == null)
            {
                return;
            }
            if (DataAccess.Instance.users.Find(u => u.Id == userNew.Id) == null)
            {
                User user = new User(userNew.Id, 10, false);
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
            if (gUser.Roles.ToList().Find(r => r.Id == oldLevelRole.Id) != null)
            {
                await gUser.RemoveRoleAsync(oldLevelRole);
            }
            await gUser.AddRoleAsync(levelRole);
        }

        private async Task Client_Ready()
        {
            myGuild = client.Guilds.Where(g => g.Id == 316621565305421825).First();
            //MusicCommands.Initialize(client);
            // IAudioClient voiceChannel = await client.Guilds.Where(g => g.Name == "Its better together!").First().VoiceChannels.Where(c => c.Name == "Türschwelle").First().ConnectAsync();
            //  MusicCommands.audioClient = voiceChannel;
            foreach (SocketGuildUser user in myGuild.Users)
            {
                if (DataAccess.Instance.users.Find(u => u.Id == user.Id) == null)
                {
                    DataAccess.Instance.InsertUser(new User(user.Id, 10, false));
                }
            }
            //finding myself
            foreach(SocketUser user in myGuild.Users)
            {
                if(user.Id == meId)
                {
                    me = user;
                    break;
                }
            }
#pragma warning disable CS4014 // Da dieser Aufruf nicht abgewartet wird, wird die Ausführung der aktuellen Methode fortgesetzt, bevor der Aufruf abgeschlossen ist
            Task.Run(() =>
            {
                while (true)
                {
                    using (NamedPipeServerStream npss = new NamedPipeServerStream("StatusNotifier", PipeDirection.In))
                    {
                        npss.WaitForConnection();
                        using (StreamReader reader = new StreamReader(npss))
                        {
                            me.SendMessageAsync(reader.ReadLine());
                        }
                    }
                }
            });
#pragma warning restore CS4014 // Da dieser Aufruf nicht abgewartet wird, wird die Ausführung der aktuellen Methode fortgesetzt, bevor der Aufruf abgeschlossen ist
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
