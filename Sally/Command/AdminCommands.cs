using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Sally.NET.Core;
using Sally.NET.Core.Configuration;
using Sally.NET.Module;
using Sally.NET.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sally.Command
{
    public class AdminCommands : ModuleBase
    {
        private readonly DiscordSocketClient client;

        public AdminCommands(DiscordSocketClient client)
        {
            this.client = client;
        }
        [Command("whois")]
        public async Task WhoIs(ulong userId)
        {
            SocketUser foundUser = client.GetUser(userId);
            if (foundUser == null)
            {
                await Context.Message.Channel.SendMessageAsync("User couldn't be found.");
                return;
            }
            await Context.Message.Channel.SendMessageAsync($"{userId} => {foundUser.Username}#{foundUser.Discriminator}");
        }


        /// <summary>
        /// command group for owner commands
        /// </summary>
        [Group("owner")]
        public class OwnerCommands : ModuleBase
        {
            private readonly BotCredentials credentials;
            private readonly DiscordSocketClient client;

            public OwnerCommands(BotCredentials credentials, DiscordSocketClient client)
            {
                this.credentials = credentials;
                this.client = client;
            }

            [Command("shutdown")]
            [RequireOwner]
            public async Task ShutdownBot()
            {
                if (Context.Message.Author.Id != credentials.MeId)
                {
                    await Context.Message.Channel.SendMessageAsync("permission denied");
                    return;
                }
                var me = client.GetUser(credentials.MeId);
                if (me is null)
                {
                    throw new ArgumentException("User not found");
                }
                await me.SendMessageAsync("I am shutting down now");
                Environment.Exit(0);
            }

            [Command("restart")]
            [RequireOwner]
            public async Task RestartBot()
            {
                if (Context.Message.Author.Id != credentials.MeId)
                {
                    await Context.Message.Channel.SendMessageAsync("permission denied");
                    return;
                }
                var me = client.GetUser(credentials.MeId);
                if (me is null)
                {
                    throw new ArgumentException("User not found");
                }
                await me.SendMessageAsync("I am restarting now");
                Environment.Exit(1);
            }

            [Command("update")]
            [RequireOwner]
            public async Task PerformUpdate()
            {
                if (Context.Message.Author.Id != credentials.MeId)
                {
                    await Context.Message.Channel.SendMessageAsync("permission denied");
                    return;
                }

                //perform update
                var me = client.GetUser(credentials.MeId);
                if (me is null)
                {
                    throw new ArgumentException("User not found");
                }
                await me.SendMessageAsync("I am updating now");
                Environment.Exit(2);
                
            }
        }
    }
}
