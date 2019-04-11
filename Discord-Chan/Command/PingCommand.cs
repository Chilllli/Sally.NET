using Discord.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Chan.Command
{
    public class PingCommand : ModuleBase
    {
        public static readonly string PongMessage = "pong";
        private static ConcurrentDictionary<ulong, DateTime> pingTracker = new ConcurrentDictionary<ulong, DateTime>();

        [Command("ping")]
        public async Task Ping()
        {
            await Context.Message.Channel.SendMessageAsync($"Pong! `{Math.Round((DateTime.Now - Context.Message.CreatedAt).TotalMilliseconds)} ms`");
        }
    }
}
