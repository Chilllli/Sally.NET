using Discord.WebSocket;
using Discord_Chan.db;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Chan.services
{
    static class UserManagerService
    {
        public static async Task InitializeHandler(DiscordSocketClient client)
        {
            client.GuildMemberUpdated += userJoined;
        }
        private static async Task userJoined(SocketGuildUser userOld, SocketGuildUser userNew)
        {
            if (userOld != null || userNew == null)
            {
                return;
            }
            if (DataAccess.Instance.users.Find(u => u.Id == userNew.Id) == null)
            {
                User user = new User(userNew.Id, 10, false);
                DataAccess.Instance.InsertUser(user);
            }
        }
    }
}
