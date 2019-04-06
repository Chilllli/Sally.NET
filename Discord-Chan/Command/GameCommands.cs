using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord_Chan.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Chan.Command
{
    public class GameCommands : ModuleBase
    {
        [Group("terraria")]
        public class TerrariaServer : ModuleBase
        {
            [Command("mods")]
            public async Task GetEnabledMods()
            {
                SocketGuildUser guildUser = Context.Message.Author as SocketGuildUser;
                if (guildUser == null || guildUser.Roles.ToList().Find(r => r.Id == Program.BotConfiguration.TerrariaId) == null)
                {
                    return;
                }
#if DEBUG
                string[] mods = JsonConvert.DeserializeObject<string[]>(File.ReadAllText("debug.json"));
#else
                string[] mods = JsonConvert.DeserializeObject<string[]>(File.ReadAllText("/srv/terraria/.local/share/Terraria/ModLoader/Mods/enabled.json"));
#endif
                await Context.Message.Channel.SendMessageAsync(null, embed: modInfoEmbed(mods)).ConfigureAwait(false);
            }
        }
        private static Embed modInfoEmbed(string[] mods)
        {
            EmbedBuilder dynamicEmbed = new EmbedBuilder()
                .WithTitle("Terraria Server")
                .WithDescription("Mods, which are currently running on the server")
                .WithColor(Color.Green)
                .WithTimestamp(DateTime.Now)
                .WithFooter(footer =>
                {
                    footer
                    .WithText($"There are currently {mods.Length} mods.");
                });
            foreach (string mod in mods)
            {
                dynamicEmbed.AddField(mod, "\u2705", true);
            }
            return dynamicEmbed.Build();
        }
        [Group("rl")]
        public class RocketLeagueCommands : ModuleBase
        {
            [Command("setRank")]
            public async Task setPlayerRank(Rank rank, int level = 0)
            {
                if (level != 0 && rank == Rank.GrandChampion)
                {
                    await Context.Message.Channel.SendMessageAsync("invalid operation");
                    return;
                }
                if (level > 3 || level < 1)
                {
                    await Context.Message.Channel.SendMessageAsync("invalid rank level");
                    return;
                }
                Type enumType = typeof(Rank);
                MemberInfo[] memInfo = enumType.GetMember(rank.ToString());
                Object[] attributes = memInfo[0].GetCustomAttributes(typeof(RankAttribute), false);
                Color color = ((RankAttribute)attributes[0]).color;

                RoleManagerService.CreateOrAddRole(rank != Rank.GrandChampion ? $"{rank} {level}" : rank.ToString(), Context.Message.Author.Id, Enum.GetNames(typeof(Rank)), color);
            }
            public enum Rank
            {
                [RankAttribute(0x77391a)] Wood,
                [RankAttribute(0xc95d1a)] Bronze,
                [RankAttribute(0x5eedea)] Silver,
                [RankAttribute(0xd2d60a)] Gold,
                [RankAttribute(0x62c4ba)] Platinum,
                [RankAttribute(0x1653e2)] Diamond,
                [RankAttribute(0x801aed)] Champion,
                [RankAttribute(0x801aed)] GrandChampion
            }
            public class RankAttribute : Attribute
            {
                public uint HexColor;
                public Color color
                {
                    get
                    {
                        return new Color(HexColor);
                    }
                }
                public RankAttribute(uint hexColor)
                {
                    HexColor = hexColor;
                    Color color = new Color(hexColor);
                }
            }
        }
    }
}
