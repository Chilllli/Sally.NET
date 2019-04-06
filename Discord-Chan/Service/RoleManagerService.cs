using Discord;
using Discord.WebSocket;
using Discord_Chan.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Chan.Service
{
    static class RoleManagerService
    {
        public static async Task InitializeHandler()
        {
            User.OnLevelUp += User_OnLevelUp;
        }
        private static async void User_OnLevelUp(User user)
        {
            CreateOrAddRole($"Level {user.Level}", user.Id, new[] { $"Level " });
        }
        public static async void CreateOrAddRole(string role, ulong id, string[] removeCriteria = null, Color? color = null)
        {
            SocketRole newRole = Program.MyGuild.Roles.ToList().Find(r => r.Name == role);
            if (newRole == null)
            {
                await Program.MyGuild.CreateRoleAsync(role, color: color);
                while (newRole == null)
                {
                    newRole = Program.MyGuild.Roles.ToList().Find(r => r.Name == role);
                }
            }
            SocketGuildUser gUser = Program.MyGuild.Users.ToList().Find(u => u.Id == id);
            if (removeCriteria != null)
            {
                SocketRole oldRole = gUser.Roles.ToList().Find(r => removeCriteria.ToList().Find(c => r.Name.Contains(c)) != null);
                if (oldRole != null)
                {
                    await gUser.RemoveRoleAsync(oldRole);
                }
            }
            await gUser.AddRoleAsync(newRole);
        }
    }
}
