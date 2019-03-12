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
                levelRole = Program.MyGuild.Roles.ToList().Find(r => r.Name == $"Level {user.Level}");
            }
            SocketGuildUser gUser = Program.MyGuild.Users.ToList().Find(u => u.Id == user.Id);
            SocketRole oldLevelRole = Program.MyGuild.Roles.ToList().Find(r => r.Name == $"Level {user.Level - 1}");
            if (oldLevelRole != null && gUser.Roles.ToList().Find(r => r.Id == oldLevelRole.Id) != null)
            {
                await gUser.RemoveRoleAsync(oldLevelRole);
            }
            await gUser.AddRoleAsync(levelRole);
        }
    }
}
