using Dapper;
using Lib.DataAccessLayer.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.DataAccessLayer.Repositories
{
    public class SectionRepo : BaseRepo
    {
        // Add section
        public void AddSection()
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("Sections")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    // 1. Get the last number of current sections
                    int number = conn.QuerySingleOrDefault<int>("SELECT Number FROM tblSection ORDER BY Number DESC LIMIT 1;", transaction);

                    number++;

                    // 2. Set the section name according to the number
                     string name = "Section " + number;

                    // 3. Insert the new record of name and number in database
                    conn.Execute("INSERT INTO tblSection (Name, Number) VALUES (@Name, @Number);", new { Name = name, Number = number }, transaction);
                    transaction.Commit();
                }
            }
        }

        // Delete section
        public void DeleteSection(string section)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("Sections")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    // Delete selected section from database
                    conn.Execute("DELETE FROM tblSection WHERE Name=@Name;", new { Name = section });
                    transaction.Commit();
                }
            }
        }

        // Get Sections
        public List<string> GetSections()
        {
            List<string> results;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString("Sections")))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    results = conn.Query<string>("SELECT Name FROM tblSection ORDER BY Number ASC;", transaction).ToList<string>();
                    transaction.Commit();
                }
            }

            return results;
        }
    }
}
