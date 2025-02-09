using Discord.Commands;
using Discord;
using Sally.NET.Module;
using Sally.NET.Service;
using Sally;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sally.NET.DataAccess.Database;
using System.Diagnostics;

namespace Sally_NET.Command.General
{
    /// <summary>
    /// general purpose commands
    /// </summary>
    public class GeneralTextCommands : ModuleBase
    {
        private readonly GeneralModule generalModule;
        private readonly IDBAccess dBAccess;

        public GeneralTextCommands(GeneralModule generalModule, IDBAccess dBAccess)
        {
            this.generalModule = generalModule;
            this.dBAccess = dBAccess;
        }

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
            var startTime = Process.GetCurrentProcess().StartTime;
            TimeSpan uptime = DateTime.Now - startTime;
            await Context.Message.Channel.SendMessageAsync($"My current uptime is {generalModule.CurrentUptime(uptime)}. I'm online since {startTime} .");
        }

        [Command("support")]
        public async Task ShowSupportLinks()
        {
            string? colorCode = await dBAccess.GetColorByUserIdAsync(Context.Message.Author.Id);
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color((uint)Convert.ToInt32(colorCode ?? "ff6600", 16)))
                .WithCurrentTimestamp()
                .WithFooter(Sally.NET.DataAccess.File.FileAccess.GENERIC_FOOTER, Sally.NET.DataAccess.File.FileAccess.GENERIC_THUMBNAIL_URL)
                .WithTitle("Thanks for considering to support us! (ﾉ◕ヮ◕)ﾉ*:･ﾟ✧")
                .AddField("Patreon", "<https://patreon.com/sallydev>")
                .AddField("PayPal", "Coming soon");
            await Context.Message.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Alias("inv")]
        [Command("invite")]
        public async Task ShowInviteLink()
        {
            string? colorCode = await dBAccess.GetColorByUserIdAsync(Context.Message.Author.Id);
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color((uint)Convert.ToInt32(colorCode ?? "ff6600", 16)))
                .WithCurrentTimestamp()
                .WithFooter(Sally.NET.DataAccess.File.FileAccess.GENERIC_FOOTER, Sally.NET.DataAccess.File.FileAccess.GENERIC_THUMBNAIL_URL)
                .WithTitle("If you want to have Sally on your server, you came to the right place! (▰˘◡˘▰)")
                .AddField("Invite Link", "https://invite.its.sally.net");
            await Context.Message.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}
