using Discord_Chan.config;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discord_Chan.db
{
    class DataAccess 
    {

        public DataAccess(BotConfiguration botConfiguration)
        {
            //Initilaze database connection
            loadUsers();
        }

        public User[] users;

        public void InsertUser(User user)
        {
            
        }

        public void UpdateUser(User user)
        {

        }

        void loadUsers()
        {

        }
    }
}
