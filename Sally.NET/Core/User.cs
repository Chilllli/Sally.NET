﻿using Sally.NET.DataAccess.Database;
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

        public Dictionary<ulong, GuildUser> GuildSpecificUser = new Dictionary<ulong, GuildUser>();

        public User(ulong id, bool mute, string weatherLocation = null, TimeSpan? notifierTime = null, string embedColor = null)
        {
            Id = id;
            hasMuted = mute;
            this.weatherLocation = weatherLocation;
            this.notifierTime = notifierTime;
            this.embedColor = embedColor;
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
