using Discord_Chan.Config;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Discord_Chan.Service.MoodHandleService;

namespace Discord_Chan.Db
{
    class DataAccess : IDisposable
    {
        public static DataAccess Instance { get; private set; }
        MySqlConnection connection;

        public static void Initialize(BotConfiguration botConfiguration)
        {
            if (Instance == null)
            {
                Instance = new DataAccess(botConfiguration);
            }
        }
        private DataAccess(BotConfiguration botConfiguration)
        {
            string connectionString = String.Format("server={0};port={1};user id={2}; password={3}; database={4}; SslMode=none",
                          botConfiguration.db_host, 3306, botConfiguration.db_user, botConfiguration.db_password, botConfiguration.db_database);
            connection = new MySqlConnection(connectionString);

            try
            {
                connection.Open();
                Console.WriteLine($"{string.Format("{0:HH:mm:ss}", DateTime.Now)} DataAccess    Success");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }

            //Initilaze database connection
            loadUsers();
        }

        public List<User> users = new List<User>();

        public void InsertUser(User user)
        {
            MySqlCommand command = new MySqlCommand("INSERT INTO infos(id,xp,isMuted) VALUES (@id,@xp,@mute)", connection);
            command.Parameters.AddWithValue("@id", user.Id);
            command.Parameters.AddWithValue("@xp", user.Xp);
            command.Parameters.AddWithValue("@mute", user.HasMuted ? 1 : 0);
            command.Prepare();
            command.ExecuteNonQuery();
            users.Add(user);
        }

        public void UpdateUser(User user)
        {
            MySqlCommand command = new MySqlCommand("UPDATE infos SET xp = @xp, isMuted = @mute, weatherLocation = @weatherLocation, notifierTime = @notifierTime WHERE id = @id", connection);
            command.Parameters.AddWithValue("@id", user.Id);
            command.Parameters.AddWithValue("@xp", user.Xp);
            command.Parameters.AddWithValue("@mute", user.HasMuted ? 1 : 0);
            command.Parameters.AddWithValue("@weatherLocation", user.WeatherLocation);
            command.Parameters.AddWithValue("@notifierTime", user.NotifierTime);
            command.Prepare();
            command.ExecuteNonQuery();
        }
        
        void loadUsers()
        {
            MySqlCommand command = new MySqlCommand("SELECT id,xp,isMuted,weatherLocation,notifierTime FROM infos", connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                users.Add(new User((ulong)reader["id"], (int)reader["xp"], (int)reader["isMuted"] == 1, reader["weatherLocation"] == DBNull.Value ? null : (string)reader["weatherLocation"], reader["notifierTime"] == DBNull.Value ? null : (TimeSpan?)reader["notifierTime"]));
            }
            reader.Close();
            Console.WriteLine($"{string.Format("{0:HH:mm:ss}", DateTime.Now)} DataAccess    All Users loaded");
        }

        public void saveMood(Mood mood)
        {
            MySqlCommand command = new MySqlCommand("INSERT INTO moodtable(mood) VALUES (@mood)", connection);
            command.Parameters.AddWithValue("@mood", mood.ToString());
            command.Prepare();
            command.ExecuteNonQuery();
        }

        public void Dispose()
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                users.ToList().ForEach(u => UpdateUser(u));
                connection.Close();
            }
        }
    }
}
