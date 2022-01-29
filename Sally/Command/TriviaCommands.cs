using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Sally.NET.Handler;
using Sally.NET.Service;
using System;
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
            dynamic searchResult = JsonConvert.DeserializeObject<dynamic>(await wikipediaApiHandler.Request2WikipediaApiAsync(searchTerm));

            EmbedBuilder searchEmbed = new EmbedBuilder()
                .WithTitle($"What is \"{searchTerm}\"?")
                .WithDescription($"Results for {searchTerm}")
                .WithFooter(NET.DataAccess.File.FileAccess.GENERIC_FOOTER, NET.DataAccess.File.FileAccess.GENERIC_THUMBNAIL_URL)
                .WithTimestamp(DateTime.Now)
                .WithColor(new Color((uint)Convert.ToInt32(CommandHandlerService.MessageAuthor.EmbedColor, 16)));
                for (int i = 0; i < 5; i++)
                {
                    if (searchResult[1][i] == null || searchResult[2][i] == null)
                        break;
                    searchEmbed.AddField((searchResult[1][i]).ToString(), (searchResult[2][i]).ToString());
                }
            await Context.Message.Channel.SendMessageAsync(null, embed: searchEmbed.Build()).ConfigureAwait(false);
        }
    }
}
