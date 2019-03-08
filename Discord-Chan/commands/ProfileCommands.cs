using Discord.Commands;
using Discord_Chan.db;
using System;
using System.Collections.Generic;
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
    }
}
