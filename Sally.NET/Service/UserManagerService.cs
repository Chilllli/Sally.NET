using Discord.WebSocket;
using Sally.NET.Core;
using Sally.NET.DataAccess.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sally.NET.Service
{
    public static class UserManagerService
    {
        private static DiscordSocketClient client;
        public static void InitializeHandler(DiscordSocketClient client)
        {
            UserManagerService.client = client;
            client.UserJoined += Client_UserJoined;
            client.JoinedGuild += Client_JoinedGuild;
        }

        private static Task Client_JoinedGuild(SocketGuild arg)
        {
            List<SocketGuildUser> guildUsers = arg.Users.ToList();
            foreach (SocketGuildUser guildUser in guildUsers)
            {
                //check if the currrent user exists globally
                if (DatabaseAccess.Instance.Users.Find(u => u.Id == guildUser.Id) == null)
                {
                    //current user dont exist in the global context
                    DatabaseAccess.Instance.InsertUser(new User(guildUser.Id, true));
                }
                User myUser = DatabaseAccess.Instance.Users.Find(u => u.Id == guildUser.Id);
                //check if the user has a guild entry of the current guild
                if (!myUser.GuildSpecificUser.ContainsKey(arg.Id))
                {
                    DatabaseAccess.Instance.InsertGuildUser(arg.Id, new GuildUser(guildUser.Id, arg.Id, 500));
                }
            }
            return Task.CompletedTask;
        }

        private static Task Client_UserJoined(SocketGuildUser userNew)
        {
            //check if the user is complete new
            
            if (DatabaseAccess.Instance.Users.Find(u => u.Id == userNew.Id) == null)
            {
                DatabaseAccess.Instance.InsertUser(new User(userNew.Id, false));
            }
            User joinedUser = DatabaseAccess.Instance.Users.Find(u => u.Id == userNew.Id);
            if (!joinedUser.GuildSpecificUser.ContainsKey(userNew.Guild.Id))
            {
                DatabaseAccess.Instance.InsertGuildUser(userNew.Guild.Id, new GuildUser(userNew.Id, userNew.Guild.Id, 500));
            }
            return Task.CompletedTask;
        }
    }
}
