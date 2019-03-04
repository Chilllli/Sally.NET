using Discord_Chan.config;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Discord_Chan.db
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
                Console.WriteLine($"{string.Format("{0:HH:mm:ss}", DateTime.Now)} Success");
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
            MySqlCommand command = new MySqlCommand("INSERT INTO users.infos(id,xp,isMuted) VALUES (@id,@xp,@mute)", connection);
            command.Parameters.AddWithValue("@id", user.Id);
            command.Parameters.AddWithValue("@xp", user.Xp);
            command.Parameters.AddWithValue("@mute", user.HasMuted);
            command.Prepare();
            command.ExecuteNonQuery();
        }

        public void UpdateUser(User user)
        {
            MySqlCommand command = new MySqlCommand("UPDATE users.infos SET xp = @xp, isMuted = @mute WHERE id = @id", connection);
            command.Parameters.AddWithValue("@id", user.Id);
            command.Parameters.AddWithValue("@xp", user.Xp);
            command.Parameters.AddWithValue("@mute", user.HasMuted);
            command.Prepare();
            command.ExecuteNonQuery();
        }
        
        void loadUsers()
        {
            MySqlCommand command = new MySqlCommand("SELECT id,xp,isMuted FROM users.infos", connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                users.Add(new User() { Id = (ulong)reader["id"], Xp = (int)reader["xp"], HasMuted = (int)reader["isMuted"] });
            }
            reader.Close();
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
