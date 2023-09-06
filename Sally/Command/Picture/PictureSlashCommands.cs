using Discord;
using Discord.Interactions;
using Sally.NET.Core.Enum;
using Sally.NET.DataAccess.Database;
using Sally.NET.Handler;
using Sally.NET.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sally_NET.Command.Picture
{
    public class PictureSlashCommands : InteractionModuleBase
    {
        private readonly KonachanApiHandler konachanApiHandler;
        private readonly IDBAccess dbAccess;
        public PictureSlashCommands(KonachanApiHandler konachanApiHandler, IDBAccess dBAccess)
        {
            this.konachanApiHandler = konachanApiHandler;
            this.dbAccess = dBAccess;
        }
        [SlashCommand("konachan", "get image from konachan.com")]
        public async Task SendKonachanPicture([Choice("safe", nameof(Rating.Safe)), Choice("questionable", nameof(Rating.Questionable)), Choice("explicit", nameof(Rating.Explicit))] Rating rating)
        {
            if (rating == Rating.None)
            {
                await generateImageEmbed(await konachanApiHandler.GetKonachanPictureUrlAsync());
                return;
            }
            await generateImageEmbed(await konachanApiHandler.GetKonachanPictureUrlAsync(Array.Empty<string>(), rating));
        }

        /// <summary>
        /// helper method for generating discord embeds
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private async Task generateImageEmbed(string response)
        {
            var myUser = dbAccess.GetUser(Context.User.Id);
            StringBuilder tagResponse = new StringBuilder();
            List<string> tags = getTagsFromKonachanImageUrl(response).ToList();
            tags.RemoveRange(0, 3);
            foreach (string tag in tags)
            {
                tagResponse.Append($"[{tag}](https://konachan.com/post?tags={tag}) ");
            }
            EmbedBuilder embedBuilder = new EmbedBuilder()
                .WithDescription($"Tags: {tagResponse.ToString().Trim()}")
                .WithColor(new Color((uint)Convert.ToInt32(myUser.EmbedColor, 16)))
                .WithImageUrl(response)
                .WithFooter(Sally.NET.DataAccess.File.FileAccess.GENERIC_FOOTER, Sally.NET.DataAccess.File.FileAccess.GENERIC_THUMBNAIL_URL);
            await Context.Interaction.RespondAsync(embed: embedBuilder.Build());
        }

        private IEnumerable<string> getTagsFromKonachanImageUrl(string imageUrl)
        {
            return Path.GetFileNameWithoutExtension(imageUrl).Split("%20");
        }
    }
}
