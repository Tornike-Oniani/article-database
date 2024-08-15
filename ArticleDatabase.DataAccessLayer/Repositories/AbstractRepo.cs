using Dapper;
using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories.Base;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace Lib.DataAccessLayer.Repositories
{
    public class AbstractRepo : BaseRepo
    {
        public void AddAbstract(int articleId, string body)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Execute("INSERT INTO tblAbstract (Article_ID, Body) VALUES (@Article_ID, @Body);", new { Article_ID = articleId, Body = body }, transaction: transaction);

                    transaction.Commit();
                }
            }
        }
        public void UpdateAbstract(int id, string body)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Execute("UPDATE tblAbstract SET Body=@Body WHERE Id=@Id", new { Id = id, Body = body }, transaction: transaction);

                    transaction.Commit();
                }
            }
        }
        public void UpdateAbstractByArticleId(int articleId, string body)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Execute("UPDATE tblAbstract SET Body=@Body WHERE Article_ID=@ArticleID", new { ArticleID = articleId, Body = body }, transaction: transaction);

                    transaction.Commit();
                }
            }
        }
        public Abstract GetAbstractByArticleId(int articleId)
        {
            Abstract result;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    result = conn.QuerySingleOrDefault<Abstract>("SELECT abst.Id, art.Title AS ArticleTitle, art.File AS ArticleFileName, abst.Body FROM tblAbstract AS abst INNER JOIN tblArticle AS art ON abst.Article_ID = art.ID WHERE Article_ID = @Article_ID;", new { Article_ID = articleId }, transaction: transaction);

                    transaction.Commit();
                }
            }

            return result;
        }
        public List<Abstract> GetAllAbstracts()
        {
            List<Abstract> result;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    result = conn.Query<Abstract>("SELECT abst.Id, art.Title AS ArticleTitle, art.File AS ArticleFileName, abst.Body FROM tblAbstract AS abst INNER JOIN tblArticle AS art ON abst.Article_ID = art.ID;", transaction: transaction).ToList();

                    transaction.Commit();
                }
            }

            return result;
        }
        public List<Abstract> GetArticleTitlesWithNoAbstracts(string sortDirection = "ASC")
        {
            List<Abstract> result;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    result = conn.Query<Abstract>($"SELECT abst.Id, art.Title AS ArticleTitle, art.File AS ArticleFileName, abst.Body FROM tblArticle AS art LEFT JOIN tblAbstract AS abst ON abst.Article_ID = art.ID WHERE Body IS NULL ORDER BY ArticleFileName {sortDirection} LIMIT 10;", transaction: transaction).ToList();

                    transaction.Commit();
                }
            }

            return result;
        }
        public void DeleteAbstract(int id)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Execute("DELETE FROM tblAbstract WHERE Id = @Id", new { Id = id }, transaction: transaction);

                    transaction.Commit();
                }
            }
        }
    }
}
