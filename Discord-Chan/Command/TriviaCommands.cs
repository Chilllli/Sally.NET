using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Sally.Command
{
    public class TriviaCommands : ModuleBase
    {
        [Command("ask")]
        public async Task AskWikipedia(string searchTerm)
        {
            dynamic searchResult = JsonConvert.DeserializeObject<dynamic>(await Program.apiRequestService.request2wikiAsync(searchTerm));

            EmbedBuilder searchEmbed = new EmbedBuilder()
                .WithTitle($"What is \"{searchTerm}\"?")
                .WithDescription($"Results for {searchTerm}")
                .WithFooter(Program.GenericFooter, Program.GenericThumbnailUrl)
                .WithTimestamp(DateTime.Now)
                .WithColor(new Color((uint)Convert.ToInt32(Program.commandHandlerService.messageAuthor.EmbedColor, 16)));
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
