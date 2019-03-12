using Discord.WebSocket;
using Discord_Chan.db;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Chan.services
{
    static class VoiceRewardService
    {
        private static int xp = 250;
        private static int xpTiming = 10 * 1000 * 60;

        public static async Task InitializeHandler(DiscordSocketClient client)
        {
            client.UserVoiceStateUpdated += voiceChannelJoined;
            client.UserVoiceStateUpdated += voiceChannelLeft;
        }

        private static async Task voiceChannelLeft(SocketUser disUser, SocketVoiceState voiceStateOld, SocketVoiceState voiceStateNew)
        {
            if (voiceStateNew.VoiceChannel != null || voiceStateOld.VoiceChannel?.Id == voiceStateNew.VoiceChannel?.Id)
            {
                return;
            }
            User currentUser = DataAccess.Instance.users.Find(u => u.Id == disUser.Id);
            if (!currentUser.HasMuted)
            {
                //send private message
                //await disUser.SendMessageAsync("testing");
            }
            stopTrackingVoiceChannel(DataAccess.Instance.users.Find(u => u.Id == disUser.Id));
        }

        private static async Task voiceChannelJoined(SocketUser disUser, SocketVoiceState voiceStateOld, SocketVoiceState voiceStateNew)
        {
            //if guild joined
            if (voiceStateOld.VoiceChannel?.Id == voiceStateNew.VoiceChannel?.Id || voiceStateNew.VoiceChannel == null)
            {
                return;
            }
            User currentUser = DataAccess.Instance.users.Find(u => u.Id == disUser.Id);
            if (!currentUser.HasMuted)
            {
                //send private message
                //await disUser.SendMessageAsync("testing");
            }
            startTrackingVoiceChannel(currentUser);
        }

        private static void startTrackingVoiceChannel(User user)
        {
            user.LastXpTime = DateTime.Now;
            user.XpTimer = new Timer(xpTiming);
            user.XpTimer.Elapsed += (s, e) => trackVoiceChannel(user);
        }

        private static void trackVoiceChannel(User user)
        {
            SocketGuildUser trackedUser = myGuild.Users.ToList().Find(u => u.Id == user.Id);
            if (trackedUser == null)
            {
                return;
            }
            if (trackedUser.VoiceChannel == null)
            {
                return;
            }
            user.Xp += xp;
            user.LastXpTime = DateTime.Now;
        }

        private static void stopTrackingVoiceChannel(User user)
        {
            user.XpTimer.Stop();
            user.Xp += (int)Math.Round(((DateTime.Now - user.LastXpTime).TotalMilliseconds / xpTiming) * xp);
        }
    }
}
