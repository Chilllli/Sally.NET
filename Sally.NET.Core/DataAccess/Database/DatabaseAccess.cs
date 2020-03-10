using MySql.Data.MySqlClient;
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
        public List<User> Users = new List<User>();
        public List<GuildSettings> guildSettings = new List<GuildSettings>();
        public Dictionary<ulong, GuildUser> GuildUserCollection = new Dictionary<ulong, GuildUser>();
        //public List<GuildUser> GuildUser = new List<GuildUser>();
        public static DatabaseAccess Instance { get; private set; }
        MySqlConnection connection;
        private static Task databaseWriter;
        private static ConcurrentQueue<MySqlCommand> databaseQueue = new ConcurrentQueue<MySqlCommand>();

        public static void Initialize(string user, string password, string database)
        {
            if (Instance == null)
            {
                Instance = new DatabaseAccess(user, password, database);
            }
            databaseWriter = new Task(databaseQueueLoop);
            databaseWriter.Start();
        }

        private static void databaseQueueLoop()
        {
            while (true)
            {
                if (databaseQueue.TryDequeue(out MySqlCommand command))
                {
                    command.ExecuteNonQuery();
                }
                else
                {
                    Task.Delay(100);
                }
            }
        }

        private DatabaseAccess(string user, string password, string database)
        {
            string connectionString = String.Format("server={0};port={1};user id={2}; password={3}; database={4}; SslMode=none",
                          "localhost", 3306, user, password, database);
            connection = new MySqlConnection(connectionString);

            try
            {
                connection.Open();
                //Console.WriteLine($"{string.Format("{0:HH:mm:ss}", DateTime.Now)} DataAccess    Success");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }

            //Initilaze database connection
            LoadUsers();
            LoadGuildSettings();
            LoadGuildUser();
        }


        public void InsertUser(User user)
        {
            MySqlCommand command = new MySqlCommand("INSERT INTO User(id,isMuted) VALUES (@id,@mute)", connection);
            command.Parameters.AddWithValue("@id", user.Id);
            command.Parameters.AddWithValue("@mute", user.HasMuted ? 1 : 0);
            command.Prepare();
            databaseQueue.Enqueue(command);
            Users.Add(user);
        }

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

#if RELEASE
        public void saveMood(Mood mood)
        {
            MySqlCommand command = new MySqlCommand("INSERT INTO moodtable(mood) VALUES (@mood)", connection);
            command.Parameters.AddWithValue("@mood", mood.ToString());
            command.Prepare();
            command.ExecuteNonQuery();
        }
#endif

        public void InsertGuildSettings(GuildSettings settings)
        {
            MySqlCommand command = new MySqlCommand("INSERT INTO Guildsettings(id,owner,levelbackground) VALUES (@id,@owner,@levelbackground)", connection);
            command.Parameters.AddWithValue("@id", settings.GuildId);
            command.Parameters.AddWithValue("@owner", settings.Owner);
            command.Parameters.AddWithValue("@levelbackground", settings.LevelbackgroundImage);
            command.Prepare();
            command.ExecuteNonQuery();
            guildSettings.Add(settings);
        }

        public void UpdateGuildSettings(GuildSettings settings)
        {
            MySqlCommand command = new MySqlCommand("UPDATE Guildsettings SET owner=@owner,levelbackground=@levelbackground WHERE id = @id", connection);
            command.Parameters.AddWithValue("@id", settings.GuildId);
            command.Parameters.AddWithValue("@owner", settings.Owner);
            command.Parameters.AddWithValue("@levelbackground", settings.LevelbackgroundImage);
            command.Prepare();
            command.ExecuteNonQuery();
        }
        public void LoadGuildSettings()
        {
            MySqlCommand command = new MySqlCommand("SELECT id,owner,levelbackground FROM Guildsettings", connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                guildSettings.Add(new GuildSettings((ulong)reader["id"], (ulong)reader["owner"], (byte[])reader["levelbackground"]));
            }
            reader.Close();
        }
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
        public void UpdateGuildUser(GuildUser guildUser)
        {
            MySqlCommand command = new MySqlCommand("UPDATE GuildUser SET xp=@xp WHERE id = @id and guildid=@guildid", connection);
            command.Parameters.AddWithValue("@id", guildUser.Id);
            command.Parameters.AddWithValue("@xp", guildUser.Xp);
            command.Parameters.AddWithValue("@guildid", guildUser.GuildId);
            command.Prepare();
            databaseQueue.Enqueue(command);
        }
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
