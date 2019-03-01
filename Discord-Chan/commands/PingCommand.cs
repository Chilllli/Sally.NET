using Discord.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Chan.commands
{
    public class PingCommand : ModuleBase
    {
        public static readonly string PongMessage = "pong";
        private static ConcurrentDictionary<ulong, DateTime> pingTracker = new ConcurrentDictionary<ulong, DateTime>();

        [Command("ping")]
        public async Task Ping()
        {
            pingTracker.TryAdd((await Context.Channel.SendMessageAsync(PongMessage)).Id, DateTime.Now);
        }

        [Command("pong")]
        public async Task Pong()
        {
            if (Context.Client.CurrentUser.Id != Context.User.Id) return;
            await Context.Message.ModifyAsync((m) =>
            {
                DateTime date = new DateTime();
                pingTracker.TryRemove(Context.Message.Id, out date);
                m.Content = $"Pong! `{Math.Round((DateTime.Now - date).TotalMilliseconds)} ms`";
            });
        }
    }
}
