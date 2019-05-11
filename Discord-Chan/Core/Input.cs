﻿using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discord_Chan.Core
{
    class Input
    {
        public InputType Type { get; set; }
        public SocketUserMessage Message { get; set; }
        public int? ArgumentPosition { get; set; }
    }
}