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

namespace Sally.NET.Service
{
    public static class VoiceRewardService
    {
        private static DiscordSocketClient client;
        private static BotCredentials credentials;

        public static void InitializeHandler(DiscordSocketClient client, BotCredentials credentials)
        {
            VoiceRewardService.client = client;
            VoiceRewardService.credentials = credentials;
            client.UserVoiceStateUpdated += voiceChannelJoined;
            client.UserVoiceStateUpdated += voiceChannelLeft;
        }

        private static async Task voiceChannelLeft(SocketUser disUser, SocketVoiceState voiceStateOld, SocketVoiceState voiceStateNew)
        {
            if (voiceStateNew.VoiceChannel != null || voiceStateOld.VoiceChannel?.Id == voiceStateNew.VoiceChannel?.Id || disUser.IsBot)
            {
                return;
            }
            User currentUser = DatabaseAccess.Instance.users.Find(u => u.Id == disUser.Id);
            if (!currentUser.HasMuted && (DateTime.Now - currentUser.LastFarewell).TotalHours > 12)
            {
                //send private message
                await disUser.SendMessageAsync(MoodDictionary.getMoodMessage("Bye"));//Bye
                currentUser.LastFarewell = DateTime.Now;
            }
            stopTrackingVoiceChannel(DatabaseAccess.Instance.users.Find(u => u.Id == disUser.Id));
        }

        private static async Task voiceChannelJoined(SocketUser disUser, SocketVoiceState voiceStateOld, SocketVoiceState voiceStateNew)
        {
            //if guild joined
            if (voiceStateOld.VoiceChannel?.Id == voiceStateNew.VoiceChannel?.Id || voiceStateNew.VoiceChannel == null || disUser.IsBot)
            {
                return;
            }
            User currentUser = DatabaseAccess.Instance.users.Find(u => u.Id == disUser.Id);
            if (!currentUser.HasMuted && (DateTime.Now - currentUser.LastGreeting).TotalHours > 12)
            {
                //send private message
                await disUser.SendMessageAsync(String.Format(MoodDictionary.getMoodMessage("Hello"), disUser.Username));//Hello
                currentUser.LastGreeting = DateTime.Now;
            }
            StartTrackingVoiceChannel(currentUser);
        }

        public static void StartTrackingVoiceChannel(User user)
        {
            user.LastXpTime = DateTime.Now;
            user.XpTimer = new Timer(credentials.xpTimerInMin * 1000 * 60);
            user.XpTimer.Elapsed += (s, e) => trackVoiceChannel(user);
        }

        private static void trackVoiceChannel(User user)
        {
            SocketGuildUser trackedUser = (client.Guilds.Where(g => g.Id == credentials.guildId).First()).Users.ToList().Find(u => u.Id == user.Id);
            if (trackedUser == null)
            {
                return;
            }
            if (trackedUser.VoiceChannel == null)
            {
                return;
            }
            user.Xp += credentials.gainedXp;
            user.LastXpTime = DateTime.Now;
        }

        private static void stopTrackingVoiceChannel(User user)
        {
            user.XpTimer.Stop();
            user.Xp += (int)Math.Round(((DateTime.Now - user.LastXpTime).TotalMilliseconds / (credentials.xpTimerInMin * 1000 * 60)) * credentials.gainedXp);
        }
    }
}
