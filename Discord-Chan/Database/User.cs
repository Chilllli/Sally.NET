using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace Sally_NET.Db
{
    class User
    {
        public User(ulong id, int xp, bool mute, string weatherLocation = null, TimeSpan? notifierTime = null){
            Id = id;
            this.xp = xp;
            hasMuted = mute;
            this.weatherLocation = weatherLocation;
            this.notifierTime = notifierTime;
        }

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
                DataAccess.Instance.UpdateUser(this);
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
        public static event LevelUp OnLevelUp;
        public static int getLevelFromXp(int xp)
        {
            return (int)Math.Floor(Math.Sqrt((xp - 200) / 300) + Math.Sqrt((xp - 200) / 500));
        }
        private bool hasMuted;
        public bool HasMuted
        {
            set
            {
                hasMuted = value;
                DataAccess.Instance.UpdateUser(this);
            }
            get
            {
                return hasMuted;
            }
        }
        public DateTime LastGreeting = new DateTime();
        public DateTime LastFarewell = new DateTime();
        private string weatherLocation;
        private TimeSpan? notifierTime;

        public string WeatherLocation
        {
            get
            {
                return weatherLocation;
            }
            set
            {
                weatherLocation = value;
                DataAccess.Instance.UpdateUser(this);
            }
        }

        public TimeSpan? NotifierTime
        {
            get
            {
                return notifierTime;
            }
            set
            {
                notifierTime = value;
                DataAccess.Instance.UpdateUser(this);
            }
        }
    }
}
