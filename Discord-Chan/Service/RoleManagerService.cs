using Discord;
using Discord.WebSocket;
using Sally_NET.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sally_NET.Service
{
    static class RoleManagerService
    {
        public static Dictionary<int, ulong> roleDictionary = new Dictionary<int, ulong>
        {
            { 0, 583712540069199893},
            { 5, 583712661347237888},
            { 10, 583712739407691817},
            { 15, 583712794101284894},
            { 20, 583712865878409226},
            { 25, 583712966327926785},
            { 30, 583713031410679826},
            { 35, 583713106623201281},
            { 40, 583713192258306058},
            { 45, 583713277792485389},
            { 50, 583713406247239843},
            { 55, 583713511176404992},
            { 60, 583713745595793410},
            { 65, 583714001687412746},
            { 70, 583714264070619158},
            { 75, 583714339018637314}

        };
        public static async Task InitializeHandler()
        {
            User.OnLevelUp += User_OnLevelUp;
        }
        private static async void User_OnLevelUp(User user)
        {
            //check if user level is a key
            if (roleDictionary.ContainsKey(user.Level))
            {
                //get value of key-specific value
                ulong roleId = roleDictionary.GetValueOrDefault(user.Level);
                //get user from guild
                SocketGuildUser guildUser = Program.MyGuild.Users.ToList().Find(u => u.Id == user.Id);
                //remove any other level-specific roles
                foreach (KeyValuePair<int, ulong> entry in roleDictionary)
                {
                    if(guildUser.Roles.ToList().Find(r => r.Id == entry.Value) != null)
                        await guildUser.RemoveRoleAsync(Program.MyGuild.Roles.ToList().Find(r => r.Id == entry.Value));
                }
                //add new role to user
                await guildUser.AddRoleAsync(Program.MyGuild.Roles.ToList().Find(r => r.Id == roleId));
            }
            //CreateOrAddRole($"Level {user.Level}", user.Id, new[] { $"Level " });
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
