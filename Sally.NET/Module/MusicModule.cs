using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sally.NET.Module
{
    public class MusicModule
    {
        public MessageComponent GetPlaylistEmbedButtons()
        {
            return new ComponentBuilder()
                .WithButton("Pause", "btnPause", emote: new Emoji("\u23EF"))
                .WithButton("Previous", "btnPrevious", emote: new Emoji("\u23EE"))
                .WithButton("Skip", "btnSkip", emote: new Emoji("\u23ED"))
                .WithButton("Repeat", "btnRepeat", emote: new Emoji("\U0001F502"))
                .Build();
        }
    }
}
