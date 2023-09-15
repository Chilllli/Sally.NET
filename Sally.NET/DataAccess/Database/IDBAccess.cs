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
        Task<List<User>> GetUsersAsync();
        void UpdateUser(User user);
        Task UpdateUserAsync(User user);
        void InsertUser(User user);
        Task InsertUserAsync(User user);
        User? GetUser(ulong id);
        Task<User?> GetUserAsync(ulong id);

        List<GuildUser> GetGuildUsers();
        Task<List<GuildUser>> GetGuildUsersAsync();
        void UpdateGuildUser(GuildUser guildUser);
        Task UpdateGuildUserAsync(GuildUser guildUser);
        void InsertGuildUser(GuildUser guildUser);
        Task InsertGuildUserAsync(GuildUser guildUser);
        GuildUser? GetGuildUser(ulong id, ulong guildId);
        Task<GuildUser?> GetGuildUserAsync(ulong id, ulong guildId);
        List<GuildUser> GetGuildUsersFromUser(ulong id);
        Task<List<GuildUser>> GetGuildUsersFromUserAsync(ulong id);

        void InsertGuildSettings(GuildSettings guildSettings);
        Task InsertGuildSettingsAsync(GuildSettings guildSettings);
        void UpdateGuildSettings(GuildSettings guildSettings);
        Task UpdateGuildSettingsAsync(GuildSettings guildSettings);
        List<GuildSettings> GetGuildSettings();
        Task<List<GuildSettings>> GetGuildSettingsAsync();
        GuildSettings? GetGuildSettingsById(ulong id);
        Task<GuildSettings?> GetGuildSettingsByIdAsync(ulong id);

        Task<string?> GetColorByUserIdAsync(ulong userId);
    }
}
