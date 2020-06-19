using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sally.NET.Module
{
    public static class GeneralModule
    {
        /// <summary>
        /// Builds a string cotaining the current uptime
        /// </summary>
        /// <param name="uptime"></param>
        /// <returns>string</returns>
        public static string CurrentUptime(TimeSpan uptime)
        {
            //build result, through checking some properties
            string result = String.Empty;
            if (uptime.Days > 0)
            {
                result = uptime.Days == 1 ? result + $" {uptime.Days} Day" : result + $" {uptime.Days} Days";
            }
            if (uptime.Hours > 0)
            {
                result = uptime.Hours == 1 ? result + $" {uptime.Hours} Hour" : result + $" {uptime.Hours} Hours";
            }
            if (uptime.Minutes > 0)
            {
                result = uptime.Minutes == 1 ? result + $" {uptime.Minutes} Minute" : result + $" {uptime.Minutes} Minutes";
            }
            if (uptime.Seconds > 0)
            {
                result = uptime.Seconds == 1 ? result + $" {uptime.Seconds} Second" : result + $" {uptime.Seconds} Seconds";
            }
            return result;
        }

        /// <summary>
        /// get specific user from guild as a guild user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="guild"></param>
        /// <returns></returns>
        public static SocketGuildUser GetGuildUserFromGuild(SocketUser user, SocketGuild guild)
        {
            return guild.Users.ToList().Find(u => u.Id == user.Id);
        }
    }
}
