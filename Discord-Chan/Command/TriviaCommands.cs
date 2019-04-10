using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Discord_Chan.Command
{
    public class TriviaCommands : ModuleBase
    {
        [Command("ask")]
        public async Task AskWikipedia(string searchTerm)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://en.wikipedia.org");
            HttpResponseMessage response = await client.GetAsync($"/w/api.php?action=opensearch&format=json&search={searchTerm}&namespace=0&limit=5&utf8=1");

            string stringResult = await response.Content.ReadAsStringAsync();

            dynamic searchResult = JsonConvert.DeserializeObject<dynamic>(stringResult);

            EmbedBuilder searchEmbed = new EmbedBuilder()
                .WithTitle($"What is \"{searchTerm}\"?")
                .WithDescription($"Results for {searchTerm}")
                .WithFooter("Provided by your friendly bot Sally")
                .WithTimestamp(DateTime.Now)
                .WithColor(0xffffff);
                for (int i = 0; i < 5; i++)
                {
                    searchEmbed.AddField((searchResult[1][i]).ToString(), (searchResult[2][i]).ToString());
                }
            await Context.Message.Channel.SendMessageAsync(null, embed: searchEmbed.Build()).ConfigureAwait(false);
        }
    }
}
