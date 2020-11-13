using Lib.DataAccessLayer.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Dapper;
using Lib.DataAccessLayer.Models;

namespace Lib.DataAccessLayer.Repositories
{
    public class GlobalRepo : BaseRepo
    {
        // Import section database records
        public List<CompFile> ImportSection(string importQuery, string pendingQuery, string filesQuery, string duplicatesQuery, out List<ExistCheck> duplicates)
        {
            List<CompFile> files;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    // Execute query to merge .sqlite3 files
                    conn.Execute(importQuery, transaction);
                    // Execute section pending query
                    conn.Execute(pendingQuery, transaction);
                    // Execute query to retrieve list of files to copy
                    files = conn.Query<CompFile>(filesQuery, transaction).ToList();
                    duplicates = conn.Query<ExistCheck>(duplicatesQuery, transaction).ToList();
                    transaction.Commit();
                }
            }

            return files;
        }
        // Get all file names from database
        public string[] GetFileNames()
        {
            int size;
            List<string> files_from_query;
            string[] fetched_files = null;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    size = conn.QuerySingleOrDefault<int>(@"SELECT COUNT(File) from tblArticle WHERE File IS NOT NULL;");
                    if (size != 0)
                    {
                        fetched_files = new string[size];
                    }
                    else
                    {
                        return null;
                    }

                    // Cycle through each file record and add it to fetched_files array
                    files_from_query = conn.Query<string>(@"SELECT File FROM tblArticle WHERE File IS NOT NULL;").ToList();
                    int i = 0;
                    foreach (string file in files_from_query)
                    {
                        fetched_files[i] = file + ".pdf";
                        i++;
                    }
                    transaction.Commit();
                }
            }

            return fetched_files;
        }
        // Remove articles from pending section (Might move to different repo)
        public void RemovePending(string section)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    // Execute query to merge .sqlite3 files
                    conn.Execute($"DELETE from tblPending WHERE Section = \"{section}\"", transaction);
                    transaction.Commit();
                }
            }
        }
    }
}
