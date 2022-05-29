using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using Sally.NET.Core;
using Sally.NET.Core.Enum;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sally.NET.DataAccess.Database
{
    public class DatabaseAccess : IDisposable
    {
        public List<User> Users { get; set; } = new List<User>();
        public List<GuildSettings> GuildSettings { get; set; } = new List<GuildSettings>();
        public Dictionary<ulong, GuildUser> GuildUserCollection { get; set; } = new Dictionary<ulong, GuildUser>();
        public static DatabaseAccess Instance { get; private set; }
        MySqlConnection connection;
        private static Task databaseWriter;
        private static ConcurrentQueue<MySqlCommand> databaseQueue = new ConcurrentQueue<MySqlCommand>();

        /// <summary>
        /// create and initialize service
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="database"></param>
        public static async Task InitializeAsync(string user, string password, string database, string host)
        {
            if (Instance == null)
            {
                Instance = new DatabaseAccess(user, password, database, host);
            }
            Task.Run(() => databaseQueueLoopAsync());
        }

        private static async Task databaseQueueLoopAsync()
        {
            while (true)
            {
                if (databaseQueue.TryDequeue(out MySqlCommand command))
                {
                    await command.ExecuteNonQueryAsync();
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }

        private DatabaseAccess(string user, string password, string database, string host)
        {
            string connectionString = String.Format("server={0};port={1};user id={2}; password={3}; database={4}; SslMode=none",
                          host, 3306, user, password, database);
            connection = new MySqlConnection(connectionString);

            try
            {
                connection.Open();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }

            //Initilaze database connection
            try
            {
                LoadUsers();
            }
            catch (MySqlException)
            {
                MySqlCommand createTable = new MySqlCommand(@"CREATE TABLE `User` (
                                                            `id` bigint(20) unsigned NOT NULL,
                                                            `isMuted` int(11) DEFAULT '1',
                                                            `embedColor` varchar(16) DEFAULT 'ffcc00',
                                                            `weatherLocation` varchar(64) DEFAULT NULL,
                                                            `notifierTime` time DEFAULT NULL,
                                                             PRIMARY KEY (`id`)
                                                            ) ENGINE=InnoDB DEFAULT CHARSET=utf8;", connection);
                createTable.ExecuteNonQuery();
            }

            try
            {
                LoadGuildSettings();
            }
            catch (MySqlException)
            {
                //todo: add new table column - music channel
                MySqlCommand createTable = new MySqlCommand(@"CREATE TABLE `Guildsettings` (
                                                                `id` bigint(20) unsigned NOT NULL,
                                                                `owner` bigint(20) unsigned DEFAULT NULL,
                                                                `levelbackground` blob,
                                                                `musicchannelid` bigint(20) unsigned DEFAULT NULL,
                                                                PRIMARY KEY (`id`)
                                                                ) ENGINE=InnoDB DEFAULT CHARSET=utf8;", connection);
                createTable.ExecuteNonQuery();
            }

            try
            {
                LoadGuildUser();
            }
            catch (MySqlException)
            {
                MySqlCommand createTable = new MySqlCommand(@"CREATE TABLE `GuildUser` (
                                                            `id` bigint(20) unsigned NOT NULL,
                                                            `guildid` bigint(20) NOT NULL,
                                                            `xp` int(10) unsigned DEFAULT '0',
                                                            PRIMARY KEY (`id`,`guildid`),
                                                            CONSTRAINT `id` FOREIGN KEY (`id`) REFERENCES `User` (`id`) ON DELETE NO ACTION ON UPDATE NO                            ACTION
                                                            ) ENGINE=InnoDB DEFAULT CHARSET=utf8;", connection);
                createTable.ExecuteNonQuery();
            }

        }

        /// <summary>
        /// insert user into database and user list
        /// </summary>
        /// <param name="user"></param>
        public void InsertUser(User user)
        {
            MySqlCommand command = new MySqlCommand("INSERT INTO User(id,isMuted) VALUES (@id,@mute)", connection);
            command.Parameters.AddWithValue("@id", user.Id);
            command.Parameters.AddWithValue("@mute", user.HasMuted ? 1 : 0);
            command.Prepare();
            databaseQueue.Enqueue(command);
            Users.Add(user);
        }

        /// <summary>
        /// update existing user from database
        /// </summary>
        /// <param name="user"></param>
        public void UpdateUser(User user)
        {
            MySqlCommand command = new MySqlCommand("UPDATE User SET isMuted = @mute, weatherLocation = @weatherLocation, notifierTime = @notifierTime, embedColor = @embedColor WHERE id = @id", connection);
            command.Parameters.AddWithValue("@id", user.Id);
            command.Parameters.AddWithValue("@mute", user.HasMuted ? 1 : 0);
            command.Parameters.AddWithValue("@weatherLocation", user.WeatherLocation);
            command.Parameters.AddWithValue("@notifierTime", user.NotifierTime);
            command.Parameters.AddWithValue("@embedColor", user.EmbedColor);
            command.Prepare();
            databaseQueue.Enqueue(command);
        }

        /// <summary>
        /// load all user from database into a list
        /// </summary>
        public void LoadUsers()
        {
            MySqlCommand command = new MySqlCommand("SELECT id,isMuted,weatherLocation,notifierTime,embedColor FROM User", connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Users.Add(new User((ulong)reader["id"], (int)reader["isMuted"] == 1, reader["weatherLocation"] == DBNull.Value ? null : (string)reader["weatherLocation"], reader["notifierTime"] == DBNull.Value ? null : (TimeSpan?)reader["notifierTime"], (string)reader["embedColor"]));
            }
            reader.Close();
            //Console.WriteLine($"{string.Format("{0:HH:mm:ss}", DateTime.Now)} DataAccess    All Users loaded");
        }


        public void saveMood(Mood mood)
        {
#if RELEASE
            MySqlCommand command = new MySqlCommand("INSERT INTO moodtable(mood) VALUES (@mood)", connection);
            command.Parameters.Add("@mood", MySqlDbType.VarString).Value = mood.ToString();
            command.Prepare();
            try
            {
                command.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                MySqlCommand createTable = new MySqlCommand(@"CREATE TABLE `moodtable` (
                                                                `time` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                                                                `mood` varchar(10) DEFAULT NULL
                                                                ) ENGINE=InnoDB DEFAULT CHARSET=latin1;", connection);
                createTable.ExecuteNonQuery();
            }
#endif
        }


        /// <summary>
        /// insert new guild settings into database and list
        /// </summary>
        /// <param name="settings"></param>
        public void InsertGuildSettings(GuildSettings settings)
        {
            MySqlCommand command = new MySqlCommand("INSERT INTO Guildsettings(id,owner,levelbackground) VALUES (@id,@owner,@levelbackground)", connection);
            command.Parameters.AddWithValue("@id", settings.GuildId);
            command.Parameters.AddWithValue("@owner", settings.Owner);
            command.Parameters.AddWithValue("@levelbackground", settings.LevelbackgroundImage);
            command.Prepare();
            command.ExecuteNonQuery();
            GuildSettings.Add(settings);
        }

        /// <summary>
        /// update existing guild settings in database
        /// </summary>
        /// <param name="settings"></param>
        public void UpdateGuildSettings(GuildSettings settings)
        {
            MySqlCommand command = new MySqlCommand("UPDATE Guildsettings SET owner=@owner,levelbackground=@levelbackground WHERE id = @id", connection);
            command.Parameters.AddWithValue("@id", settings.GuildId);
            command.Parameters.AddWithValue("@owner", settings.Owner);
            command.Parameters.AddWithValue("@levelbackground", settings.LevelbackgroundImage);
            command.Prepare();
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// load all guild settings from database into list
        /// </summary>
        public void LoadGuildSettings()
        {
            MySqlCommand command = new MySqlCommand("SELECT id,owner,levelbackground FROM Guildsettings", connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                GuildSettings.Add(new GuildSettings((ulong)reader["id"], (ulong)reader["owner"], (byte[])reader["levelbackground"], (ulong)reader["musicchannelid"]));
            }
            reader.Close();
        }

        /// <summary>
        /// insert new guild user into database with a corresponding guild id
        /// </summary>
        /// <param name="guildid"></param>
        /// <param name="guildUser"></param>
        public void InsertGuildUser(ulong guildid, GuildUser guildUser)
        {
            MySqlCommand command = new MySqlCommand("INSERT INTO GuildUser(id,guildid,xp) VALUES (@id,@guildid,@xp)", connection);
            command.Parameters.AddWithValue("@id", guildUser.Id);
            command.Parameters.AddWithValue("@guildid", guildUser.GuildId);
            command.Parameters.AddWithValue("@xp", guildUser.Xp);
            command.Prepare();
            databaseQueue.Enqueue(command);
            Users.Find(u => u.Id == guildUser.Id).GuildSpecificUser.Add(guildid, guildUser);
        }

        /// <summary>
        /// update existing guild user in database
        /// </summary>
        /// <param name="guildUser"></param>
        public void UpdateGuildUser(GuildUser guildUser)
        {
            MySqlCommand command = new MySqlCommand("UPDATE GuildUser SET xp=@xp WHERE id = @id and guildid=@guildid", connection);
            command.Parameters.AddWithValue("@id", guildUser.Id);
            command.Parameters.AddWithValue("@xp", guildUser.Xp);
            command.Parameters.AddWithValue("@guildid", guildUser.GuildId);
            command.Prepare();
            databaseQueue.Enqueue(command);
        }

        /// <summary>
        /// load all guild user into list
        /// </summary>
        public void LoadGuildUser()
        {
            MySqlCommand command = new MySqlCommand("SELECT id,guildid,xp FROM GuildUser", connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                ulong userId = (ulong)reader["id"];
                ulong guildId = Convert.ToUInt64(reader["guildid"]);
                int xp = Convert.ToInt32(reader["xp"]);
                Users.Find(u => u.Id == userId).GuildSpecificUser.Add(guildId, new GuildUser(userId, guildId, xp));
            }
            reader.Close();
        }

        /// <summary>
        /// dispose the current database connection
        /// </summary>
        public void Dispose()
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                Users.ToList().ForEach(u => UpdateUser(u));
                connection.Close();
            }
            databaseWriter.Dispose();
        }
    }
}
