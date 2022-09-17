using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.VisualBasic;
using Sally.NET.Core;
using Sally.NET.DataAccess.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sally_NET.Command.Profile
{
    public class ProfileSlashCommands : InteractionModuleBase
    {
        private readonly IDBAccess dbAccess;
        public ProfileSlashCommands(IDBAccess dbAccess)
        {
            this.dbAccess = dbAccess;
        }

        [SlashCommand("myxp", "get current experience progession")]
        public async Task GetCurrentExp()
        {
            //User myUser = CommandHandlerService.MessageAuthor;
            User myUser = dbAccess.GetUser(Context.User.Id);

            if (!Context.Interaction.IsDMInteraction)
            {
                ulong guildId = Context.Interaction.GuildId ?? 0;
                string guildName = (await Context.Client.GetGuildAsync(guildId)).Name;
                //message from guild
                EmbedBuilder lvlEmbed = new EmbedBuilder()
                    .WithAuthor($"To {Context.Interaction.User}")
                    .WithTimestamp(DateTime.Now)
                    .WithTitle("Personal Level/Exp Overview")
                    .WithDescription("Check how much xp you miss for the next level up.")
                    .WithThumbnailUrl(Context.Interaction.User.GetAvatarUrl())
                    .AddField("Current Global Level", myUser.GuildSpecificUser.Sum(x => x.Value.Level) / myUser.GuildSpecificUser.Count)
                    .AddField($"Current \"{guildName}\" Level", myUser.GuildSpecificUser[guildId].Level)
                .AddField("Xp needed until level up", (Math.Floor(-50 * (15 * Math.Sqrt(15) * Math.Pow(myUser.GuildSpecificUser[guildId].Level + 1, 2) - 60 * Math.Pow(myUser.GuildSpecificUser[guildId].Level + 1, 2) - 4))) - myUser.GuildSpecificUser[guildId].Xp)
                    .WithColor(new Discord.Color((uint)Convert.ToInt32(myUser.EmbedColor, 16)))
                .WithFooter(Sally.NET.DataAccess.File.FileAccess.GENERIC_FOOTER, Sally.NET.DataAccess.File.FileAccess.GENERIC_THUMBNAIL_URL);
                await Context.Interaction.RespondAsync(embed: lvlEmbed.Build());
                return;
            }
            else
            {
                //message from dm channel
                EmbedBuilder lvlEmbed = new EmbedBuilder()
                    .WithAuthor($"To {Context.Interaction.User}")
                    .WithTimestamp(DateTime.Now)
                    .WithTitle("Personal Level/Exp Overview")
                    .WithDescription("Check your current level.")
                    .WithThumbnailUrl(Context.Interaction.User.GetAvatarUrl())
                    .AddField("Current Global Level", myUser.GuildSpecificUser.Sum(x => x.Value.Level) / myUser.GuildSpecificUser.Count)
                    .WithColor(new Discord.Color((uint)Convert.ToInt32(myUser.EmbedColor, 16)))
                .WithFooter(Sally.NET.DataAccess.File.FileAccess.GENERIC_FOOTER, Sally.NET.DataAccess.File.FileAccess.GENERIC_THUMBNAIL_URL);
                await Context.Interaction.RespondAsync(embed: lvlEmbed.Build());
                return;
            }
        }
    }
}
