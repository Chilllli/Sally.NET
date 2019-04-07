using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Chan.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Chan.Command
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
        //Math.Floor(-50 * (15 * Math.Sprt(15) * Math.Pow(y, 2) - 60 * Math.Pow(y, 2) - 4))
        [Command("myxp")]
        public async Task LevelOverview()
        {
            User myUser = DataAccess.Instance.users.Find(u => u.Id == Context.Message.Author.Id);
            EmbedBuilder lvlEmbed = new EmbedBuilder()
                .WithAuthor($"To {Context.Message.Author}")
                .WithTimestamp(DateTime.Now)
                .WithTitle("Personal Level/Exp Overview")
                .WithDescription("Check how much xp you miss for the next level up.")
                .AddField("Current Level", myUser.Level)
                .AddField("Xp needed until level up", (Math.Floor(-50 * (15 * Math.Sqrt(15) * Math.Pow(myUser.Level + 1, 2) - 60 * Math.Pow(myUser.Level + 1, 2) - 4))) - myUser.Xp)
                .WithColor(0x5e099b)
                .WithFooter("Provided by your friendly bot Sally");
            await Context.Message.Channel.SendMessageAsync(embed: lvlEmbed.Build());
        }
    }
}
