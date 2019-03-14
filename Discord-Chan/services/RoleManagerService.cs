using Discord.WebSocket;
using Discord_Chan.db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Chan.services
{
    static class RoleManagerService
    {
        public static async Task InitializeHandler()
        {
            User.OnLevelUp += User_OnLevelUp;
        }
        private static async void User_OnLevelUp(User user)
        {
            SocketRole levelRole = Program.MyGuild.Roles.ToList().Find(r => r.Name == $"Level {user.Level}");
            if (levelRole == null)
            {
                await Program.MyGuild.CreateRoleAsync($"Level {user.Level}");
                while (levelRole == null)
                {
                    levelRole = Program.MyGuild.Roles.ToList().Find(r => r.Name == $"Level {user.Level}");
                }
            }
            SocketGuildUser gUser = Program.MyGuild.Users.ToList().Find(u => u.Id == user.Id);
            SocketRole oldLevelRole = gUser.Roles.ToList().Find(r => r.Name.Contains("Level "));
            if (oldLevelRole != null)
            {
                await gUser.RemoveRoleAsync(oldLevelRole);
            }
            await gUser.AddRoleAsync(levelRole);
        }
    }
}
