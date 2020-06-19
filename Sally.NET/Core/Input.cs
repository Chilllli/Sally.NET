using Discord.WebSocket;
using Sally.NET.Core.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sally.NET.Core
{
    class Input
    {
        public InputType Type { get; set; }
        public SocketUserMessage Message { get; set; }
        public int? ArgumentPosition { get; set; }
    }
}
