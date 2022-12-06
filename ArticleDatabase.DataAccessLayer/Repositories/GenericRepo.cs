using Dapper;
using Lib.DataAccessLayer.Repositories.Base;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace Lib.DataAccessLayer.Repositories
{
    public class GenericRepo : BaseRepo
    {
        public void ExecuteSQL(string sql)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Execute(sql, transaction: transaction);

                    transaction.Commit();
                }
            }
        }

        public List<object> QuerySQL(string sql)
        {
            List<object> result = new List<object>();

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    result = conn.Query<object>(sql, transaction: transaction).ToList();

                    transaction.Commit();
                }
            }

            return result;
        }
    }
}
