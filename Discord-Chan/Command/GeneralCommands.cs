using Discord.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Chan.Command
{
    public class GeneralCommands : ModuleBase
    {
        public static readonly string PongMessage = "pong";
        private static ConcurrentDictionary<ulong, DateTime> pingTracker = new ConcurrentDictionary<ulong, DateTime>();

        [Command("ping")]
        public async Task Ping()
        {
            await Context.Message.Channel.SendMessageAsync($"Pong! `{Math.Round((DateTime.Now - Context.Message.CreatedAt).TotalMilliseconds)} ms`");
        }

        [Command("help")]
        public async Task GetHelpPage()
        {
            await Context.Message.Channel.SendMessageAsync("If you are looking for help open the following webpage: https://its-sally.net");
        }

        [Command("commands")]
        public async Task GetCommandPage()
        {
            await Context.Message.Channel.SendMessageAsync("Here you can find the list of all available commands: https://its-sally.net/commands");
        }
    }
}
