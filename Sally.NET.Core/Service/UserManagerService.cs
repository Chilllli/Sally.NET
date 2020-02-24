using Discord.WebSocket;
using Sally.NET.Core;
using Sally.NET.DataAccess.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sally.NET.Service
{
    public static class UserManagerService
    {
        public static void InitializeHandler(DiscordSocketClient client)
        {
            client.UserJoined += Client_UserJoined;
        }

        private static Task Client_UserJoined(SocketGuildUser userNew)
        {
            //check if the user is complete new
            User joinedUser = DatabaseAccess.Instance.users.Find(u => u.Id == userNew.Id);
            if (joinedUser == null)
            {
                User user = new User(userNew.Id, 10, false);
                DatabaseAccess.Instance.InsertUser(user);
            }
            return Task.CompletedTask;
        }
    }
}
