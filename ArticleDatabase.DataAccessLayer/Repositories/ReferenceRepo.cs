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
    public class ReferenceRepo : BaseRepo
    {
        // Add reference
        public bool AddReference(string name)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    // 1. Check for duplicates (Name to be unique)
                    int check = conn.QuerySingleOrDefault<int>("SELECT COUNT(ID) FROM tblReference WHERE lower(Name) = lower(@Name);",
                        new { Name = name }, transaction: transaction);

                    if (check > 0)
                        return false;

                    // 2. Add reference to database
                    conn.Execute("INSERT INTO tblReference (Name) VALUES (@Name);",
                        new { Name = name });

                    transaction.Commit();
                }
            }

            return true;
        }
        // Update reference
        public void UpdateReference(Reference reference, bool hasMainArticle = false)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    // Edit bookmark values in database
                    if (hasMainArticle)
                        conn.Execute("UPDATE tblReference SET Name=@Name, ArticleID=@ArticleID WHERE ID=@ID;",
                            new { Name = reference.Name, ArticleID = reference.ArticleID, ID = reference.ID }, transaction: transaction);
                    else
                        conn.Execute("UPDATE tblReference SET Name=@Name WHERE ID=@ID;",
                            new { Name = reference.Name, ID = reference.ID }, transaction: transaction);

                    transaction.Commit();
                }
            }
        }
        // Delete reference
        public void DeleteReference(Reference reference)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    // 1. Delete article relationships 
                    conn.Execute("DELETE FROM tblReferenceArticle WHERE ReferenceID=@ReferenceID;",
                        new { ReferenceID = reference.ID }, transaction: transaction);

                    // 2. Delete bookmark from database
                    conn.Execute("DELETE FROM tblReference WHERE ID=@ID;",
                        new { ID = reference.ID }, transaction: transaction);

                    transaction.Commit();
                }
            }
        }
        // Get reference by name
        public Reference GetReference(string name)
        {
            Reference result;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    result = conn.QuerySingleOrDefault<Reference>("SELECT ID, Name, ArticleID FROM tblReference WHERE Name=@Name;",
                        new { Name = name}, transaction: transaction);
                    transaction.Commit();
                }
            }

            return result;
        }
        // Get reference name (Used in update tracker)
        public string GetReferenceNameWithId(int id)
        {
            string result;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    result = conn.QuerySingleOrDefault<string>("SELECT Name FROM tblReference WHERE ID=@ID;",
                        new { ID = id }, transaction: transaction);
                    transaction.Commit();
                }
            }

            return result;
        }
        // Fetch references
        public List<Reference> LoadReferences()
        {
            List<Reference> result;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    result = conn.Query<Reference>("SELECT ID, Name, ArticleID FROM tblReference;", transaction: transaction).ToList();
                    transaction.Commit();
                }
            }

            return result;
        }
        public void AddArticleToReference(Reference reference, Article article)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Execute("INSERT INTO tblReferenceArticle(ReferenceID, ArticleID) VALUES (@ReferenceID, @ArticleID);",
                        new { ReferenceID = reference.ID, ArticleID = article.ID });

                    transaction.Commit();
                }
            }
        }
        // Remove article from reference
        public void RemoveArticleFromReference(Reference reference, Article article)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Execute("DELETE FROM tblReferenceArticle WHERE ReferenceID = @ReferenceID AND ArticleID = @ArticleID;",
                        new { ReferenceID = reference.ID, ArticleID = article.ID });

                    transaction.Commit();
                }
            }
        }
        // Fetch reference articles from database
        public void LoadReferenceArticles(Reference reference)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {

                    // !!! We have to load all articles as default (Joined with personal comments, SIC etc. and then we have to inner join it with bookmark table)

                    transaction.Commit();
                }
            }
        }
        // Check if article is in reference
        public bool CheckArticleInReference(Reference reference, Article article)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    int check = conn.QuerySingleOrDefault<int>("SELECT COUNT(ID) FROM tblReferenceArticle WHERE ReferenceID = @ReferenceID AND ArticleID = @ArticleID;",
                        new { ReferenceID = reference.ID, ArticleID = article.ID });

                    if (check > 0)
                    {
                        transaction.Commit();
                        return true;
                    }
                    else
                    {
                        transaction.Commit();
                        return false;
                    }
                }
            }
        }
        // Load articles for reference
        public List<Article> LoadArticlesForReference(Reference reference)
        {
            List<Article> results;

            string query = @"
SELECT cmp.ID, cmp.Article AS Title, cmp.Authors, cmp.Keywords, cmp.Year, cmp.FileName AS [FileName]
FROM
(SELECT art_ath.ID, art_ath.Article, art_ath.Authors, art_kwd.Keywords, art_ath.Year, art_ath.FileName
FROM
(SELECT art.ID as ID, art.Title AS Article, group_concat(ath.Name, "", "") AS Authors, art.Year AS Year, art.File AS FileName
FROM tblArticle AS art
LEFT JOIN jntArticleAuthor AS aa ON art.ID = aa.Article_ID
LEFT JOIN tblAuthor AS ath ON aa.Author_ID = ath.ID
GROUP BY art.Title) AS art_ath
JOIN
(SELECT art.Title AS Article, group_concat(kwd.Keyword, "", "") AS Keywords
FROM tblArticle AS art
LEFT JOIN jntArticleKeyword AS ak ON art.ID = ak.Article_ID
LEFT JOIN tblKeyword AS kwd ON ak.Keyword_ID = kwd.ID
GROUP BY art.Title) AS art_kwd
ON art_ath.Article = art_kwd.Article) AS cmp
JOIN tblReferenceArticle as ra ON cmp.ID = ra.ArticleID
JOIN tblReference as r ON r.ID = ra.ReferenceID
WHERE r.ID = @ReferenceID; 
";
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    results = conn.Query<Article>(query, new { ReferenceID = reference.ID }, transaction: transaction).ToList();

                    transaction.Commit();
                }
            }

            return results;
        }
        // Count articls in reference
        public int CountArticlesInReference(Reference reference)
        {
            int result;

            string query = $@"SELECT COUNT(ID) FROM tblReferenceArticle WHERE ReferenceID=@ReferenceID;";
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    result = conn.QuerySingleOrDefault<int>(query, new { ReferenceID = reference.ID }, transaction: transaction);

                    transaction.Commit();
                }
            }

            return result;
        }
    }
}
