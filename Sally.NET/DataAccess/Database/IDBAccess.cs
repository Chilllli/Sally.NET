using Sally.NET.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sally.NET.DataAccess.Database
{
    public interface IDBAccess
    {
        List<User> GetUsers();
        void UpdateUser(User user);
        void InsertUser(User user);
        User GetUser(ulong id);

        List<GuildUser> GetGuildUsers();
        void UpdateGuildUser(GuildUser guildUser);
        void InsertGuildUser(GuildUser guildUser);
        GuildUser GetGuildUser(ulong id, ulong guildId);
        List<GuildUser> GetGuildUsersFromUser(ulong id);

        void InsertGuildSettings(GuildSettings guildSettings);
        void UpdateGuildSettings(GuildSettings guildSettings);
        List<GuildSettings> GetGuildSettings();
        GuildSettings GetGuildSettings(ulong id);

        Task<string?> GetColorByUserIdAsync(ulong userId);
    }
}
