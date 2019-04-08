using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Chan.Command
{
    public class AdminCommands : ModuleBase
    {
        [Group("sudo")]
        public class SudoCommands : ModuleBase
        {
            [Command("whois")]
            public async Task WhoIs(ulong userId)
            {
                //check if the user, which has written the message, has admin rights
                if((Context.Message.Author as SocketGuildUser).Roles.ToList().Find(r => r.Id == Program.BotConfiguration.AdminRole) != null)
                {
                    //user has not admin rights
                    await Context.Message.Channel.SendMessageAsync($"{Context.Message.Author.Username}, you have no rights to do that.");
                    return;
                }
                //user has admin rights
                await Context.Message.Channel.SendMessageAsync($"{userId} => {Program.MyGuild.Users.ToList().Find(u => u.Id == userId)}");
            }
            [Command("reverse")]
            public async Task ReverseUsernames()
            {
                //check if the user, which has written the message, has admin rights
                if ((Context.Message.Author as SocketGuildUser).Roles.ToList().Find(r => r.Id == Program.BotConfiguration.AdminRole) == null)
                {
                   //user has not admin rights
                    await Context.Message.Channel.SendMessageAsync($"{Context.Message.Author.Username}, you have no rights to do that.");
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
            [Command("restart")]
            public async Task RestartBot()
            {
                if(Context.Message.Author.Id != Program.BotConfiguration.meId)
                {
                    await Context.Message.Channel.SendMessageAsync("permission denied");
                    return;
                }
                await Program.Me.SendMessageAsync("Bot is restarting now");
                Environment.Exit(1);
            }

            [Command("shutdown")]
            public async Task ShutdownBot()
            {
                if (Context.Message.Author.Id != Program.BotConfiguration.meId)
                {
                    await Context.Message.Channel.SendMessageAsync("permission denied");
                    return;
                }
                await Program.Me.SendMessageAsync("Bot is shutting down now");
                Environment.Exit(0);
            }
        }
    }
}
