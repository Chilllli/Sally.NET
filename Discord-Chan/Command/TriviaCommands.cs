using Discord;
using Discord.Commands;
using Sally_NET.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Sally_NET.Command
{
    public class TriviaCommands : ModuleBase
    {
        [Command("ask")]
        public async Task AskWikipedia(string searchTerm)
        {
            dynamic searchResult = JsonConvert.DeserializeObject<dynamic>(ApiRequestService.StartRequest("wikipedia", term: searchTerm));

            EmbedBuilder searchEmbed = new EmbedBuilder()
                .WithTitle($"What is \"{searchTerm}\"?")
                .WithDescription($"Results for {searchTerm}")
                .WithFooter(Program.GenericFooter, Program.GenericThumbnailUrl)
                .WithTimestamp(DateTime.Now)
                .WithColor(0xffffff);
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
