using Discord.Commands;
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
    }
}
