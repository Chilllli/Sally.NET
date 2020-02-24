using Discord.Commands;
using Sally.NET.Module;
using System;
using System.Threading.Tasks;

namespace Sally.Command
{
    public class GeneralCommands : ModuleBase
    {
        public static readonly string PongMessage = "pong";

        [Command("ping")]
        public async Task Ping()
        {
            await Context.Message.Channel.SendMessageAsync($"Pong! `{Math.Abs(Math.Round((DateTimeOffset.UtcNow - Context.Message.CreatedAt).TotalMilliseconds))} ms`");
        }

        [Command("help")]
        public async Task GetHelpPage()
        {
            await Context.Message.Channel.SendMessageAsync("If you are looking for help open the following webpage: <https://its-sally.net>");
        }

        [Command("commands")]
        public async Task GetCommandPage()
        {
            await Context.Message.Channel.SendMessageAsync("Here you can find the list of all available commands: <https://its-sally.net/commands>");
        }

        [Command("uptime")]
        public async Task CalculateUptime()
        {
            TimeSpan uptime = DateTime.Now - Program.StartTime;
            await Context.Message.Channel.SendMessageAsync($"My current uptime is{GeneralModule.CurrentUptime(uptime)}. I'm online since {Program.StartTime.ToString()} .");
        }
    }
}
