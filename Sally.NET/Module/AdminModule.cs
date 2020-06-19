using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sally.NET.Module
{
    public static class AdminModule
    {
        public static bool IsAuthorized(SocketGuildUser user)
        {
            if(user?.Roles.ToList().FindAll(r => r.Permissions.Administrator) == null)
            {
                //user has no admin rights on guild
                return false;
            }
            //user has admin rights
            return true;
        }
    }
}
