using Discord;
using Discord.Commands;
using Sally_NET.Service;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sally_NET.Command
{
    public class PictureCommands : ModuleBase
    {
        public class RatingShortCutAttribute : Attribute
        {
            public string ShortCut;
            public RatingShortCutAttribute(string shortCut)
            {
                ShortCut = shortCut;
            }
        }

        //Enum for Image Rating Classification
        public enum Rating
        {
            None = 0,
            [RatingShortCut("s")] Safe,
            [RatingShortCut("q")] Questionable,
            [RatingShortCut("e")] Explicit
        }

        [Command("konachan")]
        public async Task SendPicture()
        {
            //search for image without tags and rating
            string response = ApiRequestService.StartRequest("konachan").Result;
            await generateImageEmbed(response);
        }
        [Command("konachan")]
        public async Task SendPicture(params String[] tags)
        {
            string[] lowerTags = tags.Select(s => s.ToLowerInvariant()).ToArray();
            string tagUrl = String.Empty;
            Rating rating;
            //generalize rating input, so misstyping isnt so bad
            string lowerRating = tags[0].ToLower();
            string ratingElement = lowerRating.First().ToString().ToUpper() + lowerRating.Substring(1);
            //try to parse as enum
            //check if first tag is equal to a rating
            if (Enum.TryParse<Rating>(ratingElement, out rating))
            {
                //get rest of the tags
                string[] tagCollection = lowerTags.Skip(1).ToArray();
                foreach (string tag in tagCollection)
                {
                    tagUrl = tagUrl + $"{tag} ";
                }
                string response = ApiRequestService.StartRequest("konachanWithRating", rating: rating, tags: tagCollection).Result;
                await generateImageEmbed(response, tagUrl);
            }
            else
            {
                //first arg isn't an enum
                foreach (string tag in lowerTags)
                {
                    tagUrl = tagUrl + $"{tag} ";
                }
                string response = ApiRequestService.StartRequest("konachanWithTag", tags: lowerTags).Result;
                await generateImageEmbed(response, tagUrl);
            }
        }

        //helper method for generate discord embed
        private async Task generateImageEmbed(string response)
        {
            EmbedBuilder embedBuilder = new EmbedBuilder()
                .WithTitle("Result")
                .WithImageUrl(response);
            await Context.Message.Channel.SendMessageAsync(embed: embedBuilder.Build());
        }

        private async Task generateImageEmbed(string response, string tagUrl)
        {
            if (response.Length == 0)
            {
                await Context.Message.Channel.SendMessageAsync("nothing found!");
                return;
            }
            EmbedBuilder embedBuilder = new EmbedBuilder()
                .WithTitle($"Tags: {tagUrl}")
                .WithImageUrl(response);
            await Context.Message.Channel.SendMessageAsync(embed: embedBuilder.Build());
        }
    }
}
