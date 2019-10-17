using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sally_NET.Command
{
    public class AdminCommands : ModuleBase
    {
        //execution of commands need admin permission
        [Group("sudo")]
        public class SudoCommands : ModuleBase
        {
            [Command("whois")]
            public async Task WhoIs(ulong userId)
            {
                //check if the user, which has written the message, has admin rights or is server owner
                if ((Context.Message.Author as SocketGuildUser).Roles.ToList().FindAll(r => r.Permissions.Administrator) != null || Context.Message.Author.Id != Context.Guild.OwnerId)
                {
                    await Context.Message.Channel.SendMessageAsync($"{Context.Message.Author.Username}, you dont have the permissions to do this!");
                    return;
                }
                //user has admin rights
                await Context.Message.Channel.SendMessageAsync($"{userId} => {Program.MyGuild.Users.ToList().Find(u => u.Id == userId)}");
            }
            [Command("reverse")]
            public async Task ReverseUsernames()
            {
                //check if the user, which has written the message, has admin rights
                if ((Context.Message.Author as SocketGuildUser).Roles.ToList().FindAll(r => r.Permissions.Administrator) != null || Context.Message.Author.Id != Context.Guild.OwnerId)
                {
                    await Context.Message.Channel.SendMessageAsync($"{Context.Message.Author.Username}, you dont have the permissions to do this!");
                    return;
                }
                foreach (SocketGuildUser guildUser in Program.MyGuild.Users)
                {
                    //"remove" owner
                    if (guildUser.Id == Program.MyGuild.OwnerId)
                        continue;
                    await guildUser.ModifyAsync(u => u.Nickname = new String((guildUser.Nickname != null ? guildUser.Nickname : guildUser.Username).Reverse().ToArray())); 
                }
            } 
        }


        //execute commands only when the author is the bot owner
        [Group("owner")]
        public class OwnerCommands : ModuleBase
        {
            [Command("apiRequests")]
            public async Task ShowCurrentApiRequests()
            {
                if (Context.Message.Author.Id != Program.BotConfiguration.meId)
                {
                    await Context.Message.Channel.SendMessageAsync("permission denied");
                    return;
                }
                await Program.Me.SendMessageAsync($"There are currently {Program.RequestCounter} Requests.");
            }

            [Command("shutdown")]
            public async Task ShutdownBot()
            {
                if (Context.Message.Author.Id != Program.BotConfiguration.meId)
                {
                    await Context.Message.Channel.SendMessageAsync("permission denied");
                    return;
                }
                await Program.Me.SendMessageAsync("I am shutting down now");
                Environment.Exit(0);
            }

            [Command("restart")]
            public async Task RestartBot()
            {
                if (Context.Message.Author.Id != Program.BotConfiguration.meId)
                {
                    await Context.Message.Channel.SendMessageAsync("permission denied");
                    return;
                }
                await Program.Me.SendMessageAsync("I am restarting now");
                Environment.Exit(1);
            }

            [Command("update")]
            public async Task PerformUpdate()
            {
                if (Context.Message.Author.Id != Program.BotConfiguration.meId)
                {
                    await Context.Message.Channel.SendMessageAsync("permission denied");
                    return;
                }

                //perform update
                await Program.Me.SendMessageAsync("I am updating now");
                Environment.Exit(2);
            }
        }
    }
}
