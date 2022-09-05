using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using Sally.NET.Core;
using System;
using System.Collections.Generic;

namespace Sally.NET.DataAccess.Database
{
    public class MySQLAccess : IDBAccess
    {
        private readonly string connectionString = "";
        public MySQLAccess(string user, string password, string database, string host)
        {
            connectionString = String.Format("server={0};port={1};user id={2}; password={3}; database={4}; SslMode=none",
                          host, 3306, user, password, database);
            initializeTables();
        }

        public List<GuildSettings> GetGuildSettings()
        {
            List<GuildSettings> guildSettings = new();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT id,owner,levelbackground FROM Guildsettings";
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            guildSettings.Add(new GuildSettings((ulong)reader["id"], (ulong)reader["owner"], (byte[])reader["levelbackground"], (ulong)reader["musicchannelid"]));
                        }
                        return guildSettings;
                    }
                }
            }
        }

        public GuildSettings GetGuildSettings(ulong id)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT owner,levelbackground FROM Guildsettings where id=@id";
                    command.Parameters.AddWithValue("@id", id);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return new GuildSettings((ulong)reader["id"], (ulong)reader["owner"], (byte[])reader["levelbackground"], (ulong)reader["musicchannelid"]);
                        }
                        return null;
                    }
                }
            }
        }

        public GuildUser GetGuildUser(ulong id, ulong guildId)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT xp FROM GuildUser where id=@id and guildid=@guildId;";
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("guildId", guildId);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return new GuildUser(id, guildId, Convert.ToInt32(reader["xp"]));
                        }
                        return null;
                    }
                }
            }
        }

        public List<GuildUser> GetGuildUsers()
        {
            List<GuildUser> guildUsers = new();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT id, guildid, xp FROM GuildUser;";
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            guildUsers.Add(new GuildUser(Convert.ToUInt64(reader["id"]), Convert.ToUInt64(reader["guildid"]), Convert.ToInt32(reader["xp"])));
                        }
                        return guildUsers;
                    }
                }
            }
        }

        public List<GuildUser> GetGuildUsersFromUser(ulong id)
        {
            throw new NotImplementedException();
        }

        public User GetUser(ulong id)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "select isMuted,weatherLocation,notifierTime,embedColor FROM User where id=@id;";
                    command.Parameters.AddWithValue("@id", id);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return new User(id, Convert.ToBoolean(reader["isMuted"]), reader["weatherLocation"].ToString(), TimeSpan.Parse(reader["notifierTime"].ToString()), reader["embedColor"].ToString());
                        }
                        return null;
                    }
                }
            }
        }

        public List<User> GetUsers()
        {
            List<User> users = new();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "select id,isMuted,weatherLocation,notifierTime,embedColor FROM User;";
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User(Convert.ToUInt64(reader["id"]), Convert.ToBoolean(reader["isMuted"]), reader["weatherLocation"].ToString(), TimeSpan.Parse(reader["notifierTime"].ToString()), reader["embedColor"].ToString()));
                        }
                        return users;
                    }
                }
            }
        }

        public void InsertGuildSettings(GuildSettings guildSettings)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO Guildsettings(id,owner,levelbackground) VALUES (@id,@owner,@levelbackground)";
                    command.Parameters.AddWithValue("@id", guildSettings.GuildId);
                    command.Parameters.AddWithValue("@owner", guildSettings.Owner);
                    command.Parameters.AddWithValue("@levelbackground", guildSettings.LevelbackgroundImage);
                    command.Prepare();
                    command.ExecuteNonQuery();
                }
            }
        }

        public void InsertGuildUser(GuildUser guildUser)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO GuildUser(id,guildid,xp) VALUES (@id,@guildid,@xp)";
                    command.Parameters.AddWithValue("@id", guildUser.Id);
                    command.Parameters.AddWithValue("@guildid", guildUser.GuildId);
                    command.Parameters.AddWithValue("@xp", guildUser.Xp);
                    command.Prepare();
                    command.ExecuteNonQuery();
                }
            }
        }

        public void InsertUser(User user)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO User(id,isMuted) VALUES (@id,@mute)";
                    command.Parameters.AddWithValue("@id", user.Id);
                    command.Parameters.AddWithValue("@mute", user.HasMuted ? 1 : 0);
                    command.Prepare();
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateGuildSettings(GuildSettings guildSettings)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE Guildsettings SET owner=@owner,levelbackground=@levelbackground WHERE id = @id";
                    command.Parameters.AddWithValue("@id", guildSettings.GuildId);
                    command.Parameters.AddWithValue("@owner", guildSettings.Owner);
                    command.Parameters.AddWithValue("@levelbackground", guildSettings.LevelbackgroundImage);
                    command.Prepare();
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateGuildUser(GuildUser guildUser)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE GuildUser SET xp=@xp WHERE id = @id and guildid=@guildid";
                    command.Parameters.AddWithValue("@id", guildUser.Id);
                    command.Parameters.AddWithValue("@xp", guildUser.Xp);
                    command.Parameters.AddWithValue("@guildid", guildUser.GuildId);
                    command.Prepare();
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateUser(User user)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE User SET isMuted = @mute, weatherLocation = @weatherLocation, notifierTime = @notifierTime, embedColor = @embedColor WHERE id = @id";
                    command.Parameters.AddWithValue("@id", user.Id);
                    command.Parameters.AddWithValue("@mute", user.HasMuted ? 1 : 0);
                    command.Parameters.AddWithValue("@weatherLocation", user.WeatherLocation);
                    command.Parameters.AddWithValue("@notifierTime", user.NotifierTime);
                    command.Parameters.AddWithValue("@embedColor", user.EmbedColor);
                    command.Prepare();
                    command.ExecuteNonQuery();
                }
            }
        }

        private void initializeTables()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"CREATE TABLE IF NOT EXISTS `User` (
                                                            `id` unsigned big int(20) NOT NULL,
                                                            `isMuted` tinyint DEFAULT '1',
                                                            `embedColor` varchar(16) DEFAULT 'ffcc00',
                                                            `weatherLocation` varchar(64) DEFAULT NULL,
                                                            `notifierTime` time DEFAULT NULL,
                                                             PRIMARY KEY (`id`)
                                                            );";
                    command.ExecuteNonQuery();
                }
                using (MySqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"CREATE TABLE IF NOT EXISTS `Guildsettings` (
                                                                `id` unsigned big int(20) NOT NULL,
                                                                `owner` unsigned big int(20) DEFAULT NULL,
                                                                `levelbackground` blob,
                                                                `musicchannelid` unsigned big int(20) DEFAULT NULL,
                                                                PRIMARY KEY (`id`)
                                                                );";
                    command.ExecuteNonQuery();
                }
                using (MySqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"CREATE TABLE IF NOT EXISTS `GuildUser` (
                                                            `id` unsigned big int(20) NOT NULL,
                                                            `guildid` bigint(20) NOT NULL,
                                                            `xp` unsigned int(10) DEFAULT '0',
                                                            PRIMARY KEY (`id`,`guildid`),
                                                            CONSTRAINT `id` FOREIGN KEY (`id`) REFERENCES `User` (`id`) ON DELETE NO ACTION ON UPDATE NO                            ACTION
                                                            );";
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
