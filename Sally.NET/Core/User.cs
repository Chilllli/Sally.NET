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
        public DateTime LastGreeting { get; set; } = new DateTime();
        public DateTime LastFarewell { get; set; } = new DateTime();
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
            }
        }

        public Dictionary<ulong, GuildUser> GuildSpecificUser { get; set; } = new Dictionary<ulong, GuildUser>();

        public User(ulong id, bool mute, string weatherLocation = null, TimeSpan? notifierTime = null, string embedColor = null)
        {
            Id = id;
            hasMuted = mute;
            this.weatherLocation = weatherLocation;
            this.notifierTime = notifierTime;
            this.embedColor = embedColor;
        }
    }
}
