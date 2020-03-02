using Sally.NET.DataAccess.Database;
using Sally.NET.Service;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace Sally.NET.Core
{
    public class User
    {
        public ulong Id { get; private set; }
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
                if (Level < getLevelFromXp(value))
                {
                    xp = value;
                    OnLevelUp?.Invoke(this);
                    LoggerService.levelUpLogger.Log($"{this.Id} has reached Level {this.Level}");
                }
                xp = value;
                Update(this);
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

        //why is this event static?
        public static event LevelUp OnLevelUp;
        private bool hasMuted;
        public bool HasMuted
        {
            set
            {
                hasMuted = value;
                DatabaseAccess.Instance.UpdateUser(this);
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
                Update(this);
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
                Update(this);
            }
        }

        private string embedColor;
        public string EmbedColor
        {
            get
            {
                return embedColor;
            }
            set
            {
                embedColor = value;
                Update(this);
            }
        }

        public User(ulong id, int xp, bool mute, string weatherLocation = null, TimeSpan? notifierTime = null, string embedColor = null)
        {
            Id = id;
            this.xp = xp;
            hasMuted = mute;
            this.weatherLocation = weatherLocation;
            this.notifierTime = notifierTime;
            this.embedColor = embedColor;
        }
        public static int getLevelFromXp(int xp)
        {
            return (int)Math.Floor(Math.Sqrt((xp - 200) / (double)300) + Math.Sqrt((xp - 200) / (double)500));
        }

        private void Update(User user)
        {
            //this if is only needed for unit testing
            //check if there is a instance for the database
            if (DatabaseAccess.Instance != null)
            {
                DatabaseAccess.Instance.UpdateUser(this);
            }

        }
    }
}
