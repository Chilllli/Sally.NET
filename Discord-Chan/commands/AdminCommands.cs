using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Chan.commands
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
                    //user has admin rights
                    await Context.Message.Channel.SendMessageAsync($"{userId} => {Program.MyGuild.Users.ToList().Find(u => u.Id == userId)}");
                }
                else
                {
                    //user has not admin rights
                    await Context.Message.Channel.SendMessageAsync($"{Context.Message.Author.Username}, you have no rights to do that.");
                }
            }
        }
    }
}
