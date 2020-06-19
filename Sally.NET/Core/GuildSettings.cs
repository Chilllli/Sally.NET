using System;
using System.Collections.Generic;
using System.Text;

namespace Sally.NET.Core
{
    public class GuildSettings
    {
        public ulong GuildId { get; set; }
        public ulong Owner { get; set; }
        public byte[] LevelbackgroundImage { get; set; }
        public ulong MusicChannelId { get; set; }

        public GuildSettings(ulong id, ulong owner, byte[] image, ulong musicChannel)
        {
            this.GuildId = id;
            this.Owner = owner;
            this.LevelbackgroundImage = image;
            this.MusicChannelId = musicChannel;
        }
    }
}
