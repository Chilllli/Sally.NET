using Discord.Commands;
using Discord.WebSocket;
using Discord_Chan.db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Chan.commands
{
    
    public class ProfileCommands : ModuleBase
    {
        [Group("profile")]
        public class ProfileManagement : ModuleBase
        {
            [Command("add")]
            public async Task AddProfile(string game, string active, string avatar, string password)
            {

                Console.WriteLine(game);
                Console.WriteLine(active);
                Console.WriteLine(avatar);
                Console.WriteLine(password);
            }
        }

        [Command("mute")]
        public async Task MuteBot()
        {
            User user = DataAccess.Instance.users.Find(u => u.Id == Context.Message.Author.Id);
            user.HasMuted = true;
        }

        [Command("unmute")]
        public async Task UnmuteBot()
        {
            User user = DataAccess.Instance.users.Find(u => u.Id == Context.Message.Author.Id);
            user.HasMuted = false;
        }
#if DEBUG
        [Command("getrich")]
        public async Task QuickExp()
        {
            //check if user has the specific role
            SocketGuildUser myGuildUser = Context.Message.Author as SocketGuildUser;
            //check if private message
            if(myGuildUser == null)
            {
                return;
            }
            if(myGuildUser.Roles.ToList().Find(r => r.Id == 483327985349558272) == null)
            {
                return;
            }
            User user = DataAccess.Instance.users.Find(u => u.Id == Context.Message.Author.Id);
            user.Xp += 10000;
        }
#endif
    }
}
