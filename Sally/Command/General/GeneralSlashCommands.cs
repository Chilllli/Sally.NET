using Discord.Interactions;
using Sally.NET.Module;
using Sally;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sally_NET.Command.General
{
    public class GeneralSlashCommands : InteractionModuleBase
    {
        private readonly GeneralModule generalModule;
        public GeneralSlashCommands(GeneralModule generalModule)
        {
            this.generalModule = generalModule;
        }
        [SlashCommand("uptime", "check current uptime")]
        public async Task CalculateUptime()
        {
            TimeSpan uptime = DateTime.Now - Program.StartTime;
            await Context.Interaction.RespondAsync($"My current uptime is {generalModule.CurrentUptime(uptime)}. I'm online since {Program.StartTime} .");
        }

        [SlashCommand("commands", "get url for command overview")]
        public async Task GetCommandPage()
        {
            await Context.Interaction.RespondAsync("Here you can find the list of all available commands: <https://its-sally.net/commands>");
        }

        [SlashCommand("ping", "get command ping")]
        public async Task Ping()
        {
            await Context.Interaction.RespondAsync($"Pong! `{Math.Abs(Math.Round((DateTimeOffset.UtcNow - Context.Interaction.CreatedAt).TotalMilliseconds))} ms`");
        }

        [SlashCommand("help", "get link to homepage")]
        public async Task GetHelpPage()
        {
            await Context.Interaction.RespondAsync("If you are looking for help open the following webpage: <https://its-sally.net>");
        }
    }
}
