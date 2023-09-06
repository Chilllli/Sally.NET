using Sally.NET.DataAccess.Database;
using Sally.NET.Service;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Sally.NET.Core
{
    public class GuildUser
    {
        public ulong Id { get; set; }
        public ulong GuildId { get; set; }
        private int level = 1;
        public int Level
        {
            get
            {
                return level;
            }
            set
            {
                level = GetLevelFromXp(this.Xp);
            }
        }
        private int xp;
        public int Xp
        {
            get
            {
                return xp;
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                if (Level < GetLevelFromXp(value))
                {
                    OnLevelUp?.Invoke(this);
                }
                xp = value;
            }
        }
        
        public Timer XpTimer { get; set; }
        public DateTime LastXpTime { get; set; }
        public static int GetLevelFromXp(int xp)
        {
            return (int)Math.Floor(Math.Sqrt((xp - 200) / (double)300) + Math.Sqrt((xp - 200) / (double)500));
        }

        public delegate Task LevelUp(GuildUser guildUser);

        //why is this event static?
        public static event LevelUp OnLevelUp;
        public GuildUser(ulong id, ulong guildId, int xp)
        {
            this.Id = id;
            this.GuildId = guildId;
            this.Xp = xp;
        }
    }
}
