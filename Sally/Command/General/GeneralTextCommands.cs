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

namespace Sally_NET.Command.General
{
    /// <summary>
    /// general purpose commands
    /// </summary>
    public class GeneralTextCommands : ModuleBase
    {

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
            await Context.Message.Channel.SendMessageAsync($"My current uptime is {GeneralModule.CurrentUptime(uptime)}. I'm online since {Program.StartTime} .");
        }

        [Command("support")]
        public async Task ShowSupportLinks()
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color((uint)Convert.ToInt32(CommandHandlerService.MessageAuthor.EmbedColor, 16)))
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
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color((uint)Convert.ToInt32(CommandHandlerService.MessageAuthor.EmbedColor, 16)))
                .WithCurrentTimestamp()
                .WithFooter(Sally.NET.DataAccess.File.FileAccess.GENERIC_FOOTER, Sally.NET.DataAccess.File.FileAccess.GENERIC_THUMBNAIL_URL)
                .WithTitle("If you want to have Sally on your server, you came to the right place! (▰˘◡˘▰)")
                .AddField("Invite Link", "https://invite.its.sally.net");
            await Context.Message.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}
