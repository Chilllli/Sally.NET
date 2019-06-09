using Discord.WebSocket;
using Sally_NET.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sally_NET.Service
{
    static class UserManagerService
    {
        public static void InitializeHandler(DiscordSocketClient client)
        {
            client.UserJoined += Client_UserJoined;
        }

        private static Task Client_UserJoined(SocketGuildUser userNew)
        {
            //check if the user is complete new
            User joinedUser = DataAccess.Instance.users.Find(u => u.Id == userNew.Id);
            if (joinedUser == null)
            {
                User user = new User(userNew.Id, 10, false);
                DataAccess.Instance.InsertUser(user);
            }
            return Task.CompletedTask;
        }
    }
}
