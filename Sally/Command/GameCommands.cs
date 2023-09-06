using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net;
using Sally.NET.Core.Enum;
using Sally.NET.Module;
using Sally.NET.Service;
using Sally.NET.Core.Attr;
using Sally.NET.Core;

namespace Sally.Command
{
    public class GameCommands : ModuleBase
    {
        [Group("rs")]
        public class RuneScapeCommands : ModuleBase
        {
            private readonly GameModule gameModule;
            public RuneScapeCommands(GameModule gameModule)
            {
                this.gameModule = gameModule;
            }
            [Command("value")]
            public async Task CheckPrice(string name)
            {
                IMessage searchMessage = Context.Message.Channel.SendMessageAsync("Searching for item....").Result;
                Embed embed = gameModule.TryGetRsItemPrice(name, Context.Message.Author.Id, out string suggestion);
                await searchMessage.DeleteAsync();
                if (embed != null)
                {
                    await Context.Message.Channel.SendMessageAsync(embed: embed);
                }
                else
                {
                    await Context.Message.Channel.SendMessageAsync($"Item not found... But do you mean {suggestion}?");
                }
            }
        }
    }
}
