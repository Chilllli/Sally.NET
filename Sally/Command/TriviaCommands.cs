using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Sally.NET.Core.ApiReference;
using Sally.NET.Handler;
using Sally.NET.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sally.Command
{
    /// <summary>
    /// command group for everything related to trivia
    /// </summary>
    public class TriviaCommands : ModuleBase
    {
        private readonly WikipediaApiHandler wikipediaApiHandler;
        public TriviaCommands(WikipediaApiHandler wikipediaApiHandler)
        {
            this.wikipediaApiHandler = wikipediaApiHandler;
        }
        [Command("ask")]
        public async Task AskWikipedia(string searchTerm)
        {
            WikipediaApi searchResult = JsonConvert.DeserializeObject<WikipediaApi>(await wikipediaApiHandler.Request2WikipediaApiAsync(searchTerm), new WikipediaJsonConverter());

            EmbedBuilder searchEmbed = new EmbedBuilder()
                .WithTitle($"What is \"{searchTerm}\"?")
                .WithDescription($"Results for {searchTerm}")
                .WithFooter(NET.DataAccess.File.FileAccess.GENERIC_FOOTER, NET.DataAccess.File.FileAccess.GENERIC_THUMBNAIL_URL)
                .WithTimestamp(DateTime.Now)
                .WithColor(new Color((uint)Convert.ToInt32(CommandHandlerService.MessageAuthor.EmbedColor, 16)));
                for (int i = 0; i < 5; i++)
                {
                    if (searchResult.Records[0].PossibleResults[i] == null || searchResult.Records[0].PossibleURLs[i] == null)
                        break;
                    searchEmbed.AddField((searchResult.Records[0].PossibleResults[i]).ToString(), (searchResult.Records[0].PossibleURLs[i]).ToString());
                }
            await Context.Message.Channel.SendMessageAsync(null, embed: searchEmbed.Build()).ConfigureAwait(false);
        }
    }
}
