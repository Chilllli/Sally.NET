using Discord.WebSocket;
using log4net;
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
        private static ILog logger;
        private static IDBAccess dbAccess;
        public static void InitializeHandler(DiscordSocketClient client, ILog logger, IDBAccess dbAccess)
        {
            UserManagerService.client = client;
            UserManagerService.logger = logger;
            UserManagerService.dbAccess = dbAccess;
            client.UserJoined += Client_UserJoined;
            client.JoinedGuild += Client_JoinedGuild;
            GuildUser.OnLevelUp += GuildUser_OnLevelUp;
        }

        private static Task GuildUser_OnLevelUp(GuildUser guildUser)
        {
            logger.Info($"{guildUser.Id} has reached Level {guildUser.Level}");
            return Task.CompletedTask;
        }

        private static Task Client_JoinedGuild(SocketGuild arg)
        {
            List<SocketGuildUser> guildUsers = arg.Users.ToList();
            foreach (SocketGuildUser guildUser in guildUsers)
            {
                //check if the currrent user exists globally
                if (dbAccess.GetUser(guildUser.Id) == null)
                {
                    //current user dont exist in the global context
                    dbAccess.InsertUser(new User(guildUser.Id, true));
                }
                User myUser = dbAccess.GetUser(guildUser.Id);
                //check if the user has a guild entry of the current guild
                if (!myUser.GuildSpecificUser.ContainsKey(arg.Id))
                {
                    dbAccess.InsertGuildUser(new GuildUser(guildUser.Id, arg.Id, 500));
                }
            }
            return Task.CompletedTask;
        }

        private static Task Client_UserJoined(SocketGuildUser userNew)
        {
            //check if the user is complete new
            
            if (dbAccess.GetUser(userNew.Id) == null)
            {
                dbAccess.InsertUser(new User(userNew.Id, false));
            }
            User joinedUser = dbAccess.GetUser(userNew.Id);
            if (!joinedUser.GuildSpecificUser.ContainsKey(userNew.Guild.Id))
            {
                dbAccess.InsertGuildUser(new GuildUser(userNew.Id, userNew.Guild.Id, 500));
            }
            return Task.CompletedTask;
        }
    }
}
