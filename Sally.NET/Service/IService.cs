using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sally.NET.Service
{
    public interface IService
    {
        void Initialize(DiscordSocketClient client, object config);
    }
}
