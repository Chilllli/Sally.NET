using Discord;
using Discord.Commands;
using Sally.NET.Core.Enum;
using Sally.NET.Handler;
using Sally.NET.Module;
using Sally.NET.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sally.Command
{
    /// <summary>
    /// command group for all related to pictures
    /// </summary>
    public class PictureCommands : ModuleBase
    {
        private readonly KonachanApiHandler konachanApiHandler;
        public PictureCommands(KonachanApiHandler konachanApiHandler)
        {
            this.konachanApiHandler = konachanApiHandler;
        }
        [Command("konachan")]
        [Alias("k")]
        public async Task SendPicture()
        {
            await generateImageEmbed(await konachanApiHandler.GetKonachanPictureUrl());
        }

        [Command("konachan")]
        [Alias("k")]
        public async Task SendPicture(params String[] tags)
        {
            string[] lowerTags = tags.Select(s => s.ToLowerInvariant()).ToArray();
            string tagUrl = String.Empty;
            //generalize rating input, so misstyping isnt so bad
            string lowerRating = tags[0].ToLower();
            //try to parse as enum
            //check if first tag is equal to a rating
            if (Enum.TryParse(lowerRating.First().ToString().ToUpper() + lowerRating[1..], out Rating rating))
            {
                //get rest of the tags
                string[] tagCollection = lowerTags.Skip(1).ToArray();
                foreach (string tag in tagCollection)
                {
                    tagUrl += $"{tag} ";
                }
                await generateImageEmbed(konachanApiHandler.GetKonachanPictureUrl(tagCollection, rating), tagUrl);
            }
            else
            {
                //first arg isn't an enum
                foreach (string tag in lowerTags)
                {
                    tagUrl += $"{tag} ";
                }
                await generateImageEmbed(konachanApiHandler.GetKonachanPictureUrl(lowerTags), tagUrl);
            }
        }

        /// <summary>
        /// helper method for generating discord embeds
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private async Task generateImageEmbed(string response)
        {
            string tagResponse = String.Empty;
            List<string> tags = getTagsFromKonachanImageUrl(response).ToList();
            tags.RemoveRange(0, 3);
            foreach (string tag in tags)
            {
                tagResponse += $"[{tag}](https://konachan.com/post?tags={tag}) ";
            }
            EmbedBuilder embedBuilder = new EmbedBuilder()
                .WithDescription($"Tags: {tagResponse.Trim()}")
                .WithColor(new Color((uint)Convert.ToInt32(CommandHandlerService.MessageAuthor.EmbedColor, 16)))
                .WithImageUrl(response)
                .WithFooter(NET.DataAccess.File.FileAccess.GENERIC_FOOTER, NET.DataAccess.File.FileAccess.GENERIC_THUMBNAIL_URL);
            await Context.Message.Channel.SendMessageAsync(embed: embedBuilder.Build());
        }

        /// <summary>
        /// helper method for generating discord embeds
        /// </summary>
        /// <param name="response"></param>
        /// <param name="tagUrl"></param>
        /// <returns></returns>
        private async Task generateImageEmbed(string response, string tagUrl)
        {
            if (response.Length == 0)
            {
                await Context.Message.Channel.SendMessageAsync("nothing found!");
                return;
            }
            string tagResponse = String.Empty;
            if (!String.IsNullOrEmpty(tagUrl))
            {
                foreach (string tag in tagUrl.Split(" "))
                {
                    tagResponse += $"[{tag}](https://konachan.com/post?tags={tag}) ";
                }
            }
            else
            {
                foreach (string tag in getTagsFromKonachanImageUrl(response))
                {
                    tagResponse += $"[{tag}](https://konachan.com/post?tags={tag}) ";
                }
            }
            
            EmbedBuilder embedBuilder = new EmbedBuilder()
                .WithDescription($"Tags: {tagResponse.Trim()}")
                .WithColor(new Color((uint)Convert.ToInt32(CommandHandlerService.MessageAuthor.EmbedColor, 16)))
                .WithImageUrl(response)
                .WithFooter(NET.DataAccess.File.FileAccess.GENERIC_FOOTER, NET.DataAccess.File.FileAccess.GENERIC_THUMBNAIL_URL);
            await Context.Message.Channel.SendMessageAsync(embed: embedBuilder.Build());
        }

        private IEnumerable<string> getTagsFromKonachanImageUrl(string imageUrl)
        {
            return Path.GetFileNameWithoutExtension(imageUrl).Split("%20");
        }
    }
}
