using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace Discord_Chan.db
{
    class User
    {
        public ulong Id;
        private int xp;
        public int Xp
        {
            get
            {
                return xp;
            }
            set
            {
                if(Level < getLevelFromXp(value))
                {
                    xp = value;
                    OnLevelUp?.Invoke(this);
                }
                xp = value;
            }
        }
        public Timer XpTimer;
        public DateTime LastXpTime;

        public int Level
        {
            get
            {
                return getLevelFromXp(Xp);
            }
        }
        public delegate void LevelUp(User user);
        public event LevelUp OnLevelUp;
        private static int getLevelFromXp(int xp)
        {
            return (int)Math.Floor(Math.Sqrt((xp - 200) / 300) + Math.Sqrt((xp - 200) / 500));
        }
        public int HasMuted;
    }
}
