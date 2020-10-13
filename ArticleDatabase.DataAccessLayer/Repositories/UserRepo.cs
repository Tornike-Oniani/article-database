using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories.Base;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.DataAccessLayer.Repositories
{
    public class UserRepo : BaseRepo
    {
        // Login
        public bool Login(User user, string password)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("User")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    // Check if Username and password match
                    User result = conn.QuerySingleOrDefault<User>("SELECT Id, Username, Password FROM tblUser WHERE Username LIKE @Username AND Password = @Password;",
                        new { Username = user.Username, Password = password }, transaction);

                    if (result != null)
                    {
                        user.ID = result.ID;
                        transaction.Commit();
                        return true;
                    }

                    transaction.Commit();
                    return false;
                }
            }
        }
        // Login with the first user (Used in sections as we have only one user per each section application)
        public User LoginFirst()
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("User")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    // Check if Username and password match
                    User result = conn.QuerySingleOrDefault<User>("SELECT Id, Username, Password FROM tblUser WHERE ID = 1", transaction);

                    return result;
                }
            }
        }
        // Register
        public bool Register(User user, int admin = 0)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("User")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {

                    // Check if username is available
                    int result = conn.QuerySingleOrDefault<int>("SELECT COUNT(Username) FROM tblUser WHERE Username LIKE @Username;",
                        new { Username = user.Username }, transaction);

                    if (result > 0)
                    {
                        transaction.Commit();
                        return false;
                    }

                    // Register the user (Insert info into User.sqlite3)
                    conn.Execute("INSERT INTO tblUser (Username, Password, Admin) VALUES (@Username, @Password, @Admin);",
                        new { Username = user.Username, Password = user.Password, Admin = admin }, transaction);

                    transaction.Commit();
                    return true;
                }
            }
        }
        // Get admin status
        public int IsAdmin(User user)
        {
            int result;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("User")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    // Check if Username and password match
                    result = conn.QuerySingleOrDefault<int>("SELECT Admin FROM tblUser WHERE Username LIKE @Username;",
                        new { Username = user.Username }, transaction);
                }
            }

            return result;
        }
        // Get list of all registered users
        public List<User> GetUsers()
        {
            List<User> results;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("User")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    results = conn.Query<User>("SELECT Id, Username FROM tblUser;").ToList();
                }
            }

            return results;
        }
        // Get user by Username
        public User GetUserByName(string userName)
        {
            User result;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("User")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    // Check if Username and password match
                    result = conn.QuerySingleOrDefault<User>("SELECT ID, Username, Password, Admin FROM tblUser WHERE Username LIKE @Username;",
                        new { Username = userName }, transaction);
                }
            }

            return result;
        }
    }
}
