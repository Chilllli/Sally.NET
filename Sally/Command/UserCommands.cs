using Discord;
using Discord.Commands;
using Sally.NET.Core;
using Sally.NET.DataAccess.Database;
using Sally.NET.Handler;
using Sally.NET.Module;
using Sally.NET.Service;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sally_NET.Command
{
    public class UserCommands : ModuleBase
    {
        /// <summary>
        /// command group for setting or updating something user specific
        /// </summary>
        [Group("set")]
        public class SetUserCommands : ModuleBase
        {
            private readonly ColornamesApiHandler colornamesApiHandler;
            public SetUserCommands(ColornamesApiHandler colornamesApiHandler)
            {
                this.colornamesApiHandler = colornamesApiHandler;
            }
            [Command("color")]
            public async Task SetEmbedColor(string color)
            {
                if (color.StartsWith('#'))
                {
                    color = color.Substring(1, color.Length - 1);
                }
                int hexColor;
                if (Int32.TryParse(color, System.Globalization.NumberStyles.HexNumber, null, out hexColor))
                {
                    string result = "0x" + color.PadRight(6, '0');
                    if (hexColor < 16777216 && hexColor >= 0)
                    {
                        string previousColorCode = CommandHandlerService.MessageAuthor.EmbedColor;
                        string previousColor;
                        if (previousColorCode.Length != 6)
                        {
                            previousColor = previousColorCode.Substring(2, previousColorCode.Length - 2);
                        }
                        else
                        {
                            previousColor = previousColorCode;
                        }
                        string oldColorName = colornamesApiHandler.GetColorName(previousColor);
                        if (oldColorName == null)
                        {
                            oldColorName = "Color has no name yet.";
                        }
                        string newColorName = colornamesApiHandler.GetColorName(color);
                        if (newColorName == null)
                        {
                            newColorName = "Color has no name yet.";
                        }
                        //hex value is in range
                        User user = DatabaseAccess.Instance.Users.Find(u => u.Id == Context.Message.Author.Id);
                        user.EmbedColor = result;

                        EmbedBuilder embed = new EmbedBuilder()
                            .WithTitle("Custom embed color changed successfully")
                            .WithFooter(Sally.NET.DataAccess.File.FileAccess.GENERIC_FOOTER, Sally.NET.DataAccess.File.FileAccess.GENERIC_THUMBNAIL_URL)
                            .AddField("Previous color name", oldColorName)
                            .AddField("Previous color hexcode", previousColor)
                            .AddField("New color name", newColorName)
                            .AddField("New color hexcode", color)
                            .AddField("All color names are provided by colornames.org", "https://colornames.org")
                            .WithDescription("New color preview on the left side")
                            .WithColor(new Discord.Color((uint)Convert.ToInt32(color, 16)));

                        await Context.Message.Channel.SendMessageAsync(embed: embed.Build());
                    }
                    else
                    {
                        await Context.Message.Channel.SendMessageAsync("wrong hex value");
                    }
                }
                else
                {
                    //error
                    await Context.Message.Channel.SendMessageAsync("Something went wrong");
                }
            }
        }
    }
}