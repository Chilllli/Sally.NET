using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Chan.commands
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
    }
}
