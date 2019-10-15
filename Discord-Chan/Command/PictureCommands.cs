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

        public enum Rating
        {
            None = 0,
            [RatingShortCut("s")] Safe,
            [RatingShortCut("q")] Questionable,
            [RatingShortCut("e")] Explicit
        }

        [Command("konachan")]

        public async Task SendPicture(params String[] tags)
        {
            string tagUrl = String.Empty;
            Rating rating;
            //try to parse as enum
            //check if first tag is equal to a rating
            if (Enum.TryParse<Rating>(tags[0], out rating))
            {
                //get rest of the tags
                string[] tagCollection = tags.Skip(1).ToArray();
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
                foreach (string tag in tags)
                {
                    tagUrl = tagUrl + $"{tag} ";
                }
                string response = ApiRequestService.StartRequest("konachan", tags: tags).Result;
                await generateImageEmbed(response, tagUrl);
            }
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
