using Discord;
using Discord.WebSocket;
using Sally.NET.Core;
using Sally.NET.Core.Configuration;
using Sally.NET.DataAccess.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Sally.NET.Service
{
    public class VoiceRewardService
    {
        private DiscordSocketClient client;
        private BotCredentials credentials;
        private readonly ConfigManager configManager;
        private bool hasCleverbotApiKey;
        private IDBAccess dbAccess;
        public VoiceRewardService(DiscordSocketClient client, BotCredentials credentials, ConfigManager configManager, IDBAccess dbAccess)
        {
            this.client = client;
            this.credentials = credentials;
            this.configManager = configManager;
            this.dbAccess = dbAccess;
        }
        public void Start()
        {
            this.hasCleverbotApiKey = !configManager.OptionalSettings.Contains("CleverApi");
            client.UserVoiceStateUpdated += voiceChannelJoined;
            client.UserVoiceStateUpdated += voiceChannelLeft;
        }

        private async Task voiceChannelLeft(SocketUser disUser, SocketVoiceState voiceStateOld, SocketVoiceState voiceStateNew)
        {
            if (voiceStateNew.VoiceChannel != null || voiceStateOld.VoiceChannel?.Id == voiceStateNew.VoiceChannel?.Id || disUser.IsBot)
            {
                return;
            }
            User currentUser = dbAccess.GetUser(disUser.Id);
            GuildUser guildUser = currentUser.GuildSpecificUser[voiceStateOld.VoiceChannel.Guild.Id];
            if ((!currentUser.HasMuted && (DateTime.Now - currentUser.LastFarewell).TotalHours > 12) && hasCleverbotApiKey)
            {
                //send private message
                await disUser.SendMessageAsync("Goodbye!");//Bye
                currentUser.LastFarewell = DateTime.Now;
            }
            stopTrackingVoiceChannel(guildUser);
        }

        private async Task voiceChannelJoined(SocketUser disUser, SocketVoiceState voiceStateOld, SocketVoiceState voiceStateNew)
        {
            //if guild joined
            if (voiceStateOld.VoiceChannel?.Id == voiceStateNew.VoiceChannel?.Id || voiceStateNew.VoiceChannel == null || disUser.IsBot)
            {
                return;
            }
            User currentUser = dbAccess.GetUser(disUser.Id);
            GuildUser guildUser = currentUser.GuildSpecificUser[voiceStateNew.VoiceChannel.Guild.Id];
            if ((!currentUser.HasMuted && (DateTime.Now - currentUser.LastGreeting).TotalHours > 12) && hasCleverbotApiKey)
            {
                //send private message
                await disUser.SendMessageAsync(String.Format("Welcome!", disUser.Username));//Hello
                currentUser.LastGreeting = DateTime.Now;
            }
            StartTrackingVoiceChannel(guildUser);
        }

        public void StartTrackingVoiceChannel(GuildUser guildUser)
        {
            guildUser.LastXpTime = DateTime.Now;
            guildUser.XpTimer = new Timer(credentials.XpTimerInMin * 1000 * 60);
            guildUser.XpTimer.Start();
            guildUser.XpTimer.Elapsed += (s, e) => trackVoiceChannel(guildUser);
        }

        private void trackVoiceChannel(GuildUser guildUser)
        {
            SocketGuildUser trackedUser = client.GetGuild(guildUser.GuildId).Users.ToList().Find(u => u.Id == guildUser.Id);
            if (trackedUser == null)
            {
                return;
            }
            if (trackedUser.VoiceChannel == null)
            {
                return;
            }
            guildUser.Xp += credentials.GainedXp;
            guildUser.LastXpTime = DateTime.Now;
            dbAccess.UpdateGuildUser(guildUser);
        }

        private void stopTrackingVoiceChannel(GuildUser guildUser)
        {
            guildUser.XpTimer.Stop();
            guildUser.Xp += (int)Math.Round(((DateTime.Now - guildUser.LastXpTime).TotalMilliseconds / (credentials.XpTimerInMin * 1000 * 60)) * credentials.GainedXp);
            dbAccess.UpdateGuildUser(guildUser);
        }
    }
}
