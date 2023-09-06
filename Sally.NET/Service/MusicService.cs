using Discord.Audio;
using Sally.NET.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sally.NET.Service
{
    public class MusicService
    {
        private ConcurrentDictionary<ulong, MusicPlayer> activeMusicPlayers = new();

        public MusicPlayer? GetPlayerByGuildId(ulong guildId)
        {
            if (activeMusicPlayers.TryGetValue(guildId, out MusicPlayer musicPlayer))
            {
                return musicPlayer;
            }
            return null;
        }

        public void AddPlayer(ulong guildId, IAudioClient audioClient)
        {
            MusicPlayer player = new(audioClient);
            activeMusicPlayers.TryAdd(guildId, player);
        }
    }
}
