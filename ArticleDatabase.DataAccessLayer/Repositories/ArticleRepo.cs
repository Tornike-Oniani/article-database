using Lib.DataAccessLayer.Models;
using Lib.DataAccessLayer.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Lib.DataAccessLayer.Repositories
{
    public class ArticleRepo : BaseRepo
    {
        // Save article to database
        public void SaveArticle(Article article, User user)
        {
            string count = "";
            string model = "000000";

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Query(AttachUser(), transaction);

                    count = conn.QuerySingleOrDefault<int>("SELECT seq + 1 FROM sqlite_sequence WHERE name = 'tblArticle';").ToString();

                    string concat = model.Substring(0, model.Length - count.Length) + count;
                    string final = concat.Substring(0, concat.Length / 2) + "-" + concat.Substring(concat.Length / 2, concat.Length / 2);
                    article.FileName = final;

                    // Insert Article into tblArticle
                    conn.Execute(@"INSERT INTO tblArticle (Title, Year, File) VALUES (@Title, @Year, @FileName);",
                        new { Title = article.Title, Year = article.Year, FileName = article.FileName });
                    article.ID = conn.LastInsertRowId;

                    // Insert PersonalComment and SIC into tblUser
                    conn.Execute("INSERT INTO user.tblUserPersonal (UserID, ArticleID, PersonalComment, SIC) VALUES (@UserID, @ArticleID, @PersonalComment, @SIC);",
                        new { UserID = user.ID, ArticleID = article.ID, PersonalComment = article.PersonalComment, SIC = article.SIC }, transaction);

                    // Insert ArticleID in tblUserArticles so we know which user added this article
                    conn.Execute("INSERT INTO user.tblUserArticles (UserID, ArticleID) VALUES (@UserID, @ArticleID);",
                        new { UserId = user.ID, ArticleID = article.ID }, transaction);

                    AddAuthors(conn, transaction, article);
                    AddKeywords(conn, transaction, article);

                    transaction.Commit();
                }
            }
        }
        // Update article fields in database
        public void UpdateArticle(Article article, User user)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Query(AttachUser(), transaction);

                    List<long> listAuthorID = new List<long>();
                    List<long> listKeywordID = new List<long>();
                    long author_id;
                    long keyword_id;

                    // 1. Update article's direct attributes (title, year)
                    conn.Execute(@"UPDATE tblArticle SET Title=@Title, Year=@Year WHERE ID=@ID;",
                        new { Title = article.Title, Year = article.Year, ID = article.ID }, transaction);

                    // 2. Update or insert comment and SIC
                    int check = conn.QuerySingleOrDefault<int>(@"SELECT ID FROM user.tblUserPersonal WHERE UserID=@UserID AND ArticleID=@ArticleID;",
                        new { UserID = user.ID, ArticleID = article.ID }, transaction);
                    if (check > 0)
                    {
                        conn.Execute(@"UPDATE user.tblUserPersonal SET PersonalComment=@PersonalComment, SIC=@SIC WHERE UserID=@UserID AND ArticleID=@ArticleID;",
                            new { PersonalComment = article.PersonalComment, SIC = article.SIC, UserID = user.ID, ArticleID = article.ID }, transaction);
                    }
                    else
                    {
                        conn.Execute(@"INSERT INTO  user.tblUserPersonal (UserID, ArticleID, PersonalComment, SIC) VALUES(@UserID, @ArticleID, @PersonalComment, @SIC);",
                            new { UserID = user.ID, ArticleID = article.ID, PersonalComment = article.PersonalComment, SIC = article.SIC }, transaction);
                    }

                    // Update authors and keywords

                    // 3. First delete all relationships for current article
                    conn.Execute(@"DELETE FROM jntArticleAuthor WHERE Article_ID=@ArticleID;",
                        new { ArticleID = article.ID }, transaction);

                    // 4. Add new relationships for each author
                    foreach (string author in article.AuthorsCollection)
                    {
                        // If author already exists add its id to listAuthorID then to add realtionship with foreach
                        author_id = conn.QuerySingleOrDefault<long>(@"SELECT ID FROM tblAuthor WHERE Name=@Name;",
                            new { Name = author }, transaction);
                        if (author_id != 0)
                        {
                            listAuthorID.Add(author_id);
                        }
                        else
                        {
                            // If author doesn't exist add it to the database and retrieve last added id
                            conn.Execute(@"INSERT INTO tblAuthor (Name) VALUES (@Name);",
                                new { Name = author }, transaction);
                            listAuthorID.Add(conn.LastInsertRowId);
                        }
                    }

                    // 5. Foreach authorID add it and ArticleID in jntArticleAuthor
                    foreach (long authorID in listAuthorID)
                    {
                        conn.Execute(@"INSERT INTO jntArticleAuthor (Article_ID, Author_ID) VALUES (@ArticleID, @AuthorID);",
                            new { ArticleID = article.ID, AuthorID = authorID }, transaction);
                    }

                    // 6. Do exactly the same as authors just with keywords
                    conn.Execute(@"DELETE FROM jntArticleKeyword WHERE Article_ID=@ArticleID;",
                        new { ArticleID = article.ID });

                    foreach (string keyword in article.KeywordsCollection)
                    {
                        keyword_id = conn.QuerySingleOrDefault<long>(@"SELECT ID FROM tblKeyword WHERE Keyword=@Keyword;",
                            new { Keyword = keyword }, transaction);
                        if (keyword_id != 0)
                        {
                            listKeywordID.Add(keyword_id);
                        }
                        else
                        {
                            conn.Execute(@"INSERT INTO tblKeyword (Keyword) VALUES (@Keyword);",
                                new { Keyword = keyword }, transaction);
                            listKeywordID.Add(conn.LastInsertRowId);
                        }
                    }

                    foreach (long keywordID in listKeywordID)
                    {
                        conn.Execute(@"INSERT INTO jntArticleKeyword (Article_ID, Keyword_ID) VALUES (@ArticleID, @KeywordID)",
                            new { ArticleID = article.ID, KeywordID = keywordID }, transaction);
                    }

                    transaction.Commit();
                }
            }

            // 7. Remove any leftover author or keyword that are not bound to any article
            ConsolidateReferences();
        }
        // Update/Insert only comment and sic
        public void UpdatePersonal(Article article, User user)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Query(AttachUser(), transaction);

                    // Insert PersonalComment and SIC into User database file -> 'tblUsername'
                    int check = conn.QuerySingleOrDefault<int>(@"SELECT ID FROM user.tblUserPersonal WHERE UserID=@UserID AND ArticleID=@ArticleID;",
                        new { UserID = user.ID, ArticleID = article.ID }, transaction);
                    if (check > 0)
                    {
                        conn.Execute(@"UPDATE user.tblUserPersonal SET PersonalComment=@PersonalComment, SIC=@SIC WHERE UserID=@UserID AND ArticleID=@ArticleID;",
                            new { PersonalComment = article.PersonalComment, SIC = article.SIC, UserID = user.ID, ArticleID = article.ID }, transaction);
                    }
                    else
                    {
                        conn.Execute(@"INSERT INTO user.tblUserPersonal (UserID, ArticleID, PersonalComment, SIC) VALUES(@UserID, @ArticleID, @PersonalComment, @SIC);",
                            new { UserID = user.ID, ArticleID = article.ID, PersonalComment = article.PersonalComment, SIC = article.SIC }, transaction);
                    }

                    transaction.Commit();
                }
            }
        }
        // Delete article from database
        public void DeleteArticle(Article article)
        {
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Query(AttachUser(), transaction);
                    // Clear author relationships
                    conn.Execute(@"DELETE FROM jntArticleAuthor WHERE Article_ID=@ArticleID;",
                        new { ArticleID = article.ID }, transaction);
                    // Clear keyword relationships
                    conn.Execute(@"DELETE FROM jntArticleKeyword WHERE Article_ID=@ArticleID;",
                        new { ArticleID = article.ID }, transaction);
                    // Remove actual article
                    conn.Execute(@"DELETE FROM tblArticle WHERE ID=@ID;",
                        new { ID = article.ID }, transaction);
                    // Remove from user Article table
                    conn.Execute(@"DELETE FROM user.tblUserArticles WHERE ArticleID=@ArticleID;",
                        new { ArticleID = article.ID }, transaction);
                    // Remove from user Personal table (where PersonalComment and SIC are stored)
                    conn.Execute(@"DELETE FROM user.tblUserPersonal WHERE ArticleID=@ArticleID",
                        new { ArticleID = article.ID }, transaction);
                    transaction.Commit();
                }
            }

            // Remove any leftofer authors and keywords which have no relationships
            ConsolidateReferences();
        }
        // Check if article exists and return the file
        public bool Exists(string title, out string file)
        {
            bool check;
            string result;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    result = conn.QueryFirstOrDefault<string>(@"SELECT File FROM tblArticle WHERE Title LIKE @Title;", new { Title = title });

                    if (result != null)
                    {
                        check = true;
                        file = result;
                    }
                    else
                    {
                        check = false;
                        file = null;
                    }
                    transaction.Commit();
                }
            }

            return check;
        }
        // Fetch list of articles from database without paging or advanced filter
        public List<Article> GetAllArticles(User user, string title)
        {
            // Results
            List<Article> results;

            // Template query
            StringBuilder queryBuilder = new StringBuilder(@"
SELECT final.ID, final.Title, final.Authors, final.Keywords, final.Year, final.FileName, final.PersonalComment, final.SIC
FROM
(SELECT cmp.ID, cmp.Article AS Title, cmp.Authors, cmp.Keywords, cmp.Year, cmp.FileName AS [FileName], per.PersonalComment, IFNULL(per.SIC, 0) AS SIC
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
LEFT JOIN
(SELECT ArticleID, PersonalComment, SIC
FROM tblUserPersonal WHERE UserID = #UserID) AS per ON cmp.ID = per.ArticleID) AS final
");
            // 1. Add filters to template query
            string query = AddFilter(queryBuilder, user, title, new List<string>(), new List<string>(), null, null);

            // 3.Fetch articles
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Query(AttachUser(), transaction);

                    results = conn.Query<Article>(query, transaction: transaction).ToList();

                    transaction.Commit();
                }
            }

            return results;
        }
        // Fetch list of articles from database
        public List<Article> LoadArticles(User user, string title, List<string> authors, List<string> keywords, string year, string personalComment, int offset, int itemsPerPage)
        {
            // Results
            List<Article> results;

            // Template query
            StringBuilder queryBuilder = new StringBuilder(@"
SELECT final.ID, final.Title, final.Authors, final.Keywords, final.Year, final.FileName, final.PersonalComment, final.SIC
FROM
(SELECT cmp.ID, cmp.Article AS Title, cmp.Authors, cmp.Keywords, cmp.Year, cmp.FileName AS [FileName], per.PersonalComment, IFNULL(per.SIC, 0) AS SIC
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
LEFT JOIN
(SELECT ArticleID, PersonalComment, SIC
FROM tblUserPersonal WHERE UserID = #UserID) AS per ON cmp.ID = per.ArticleID) AS final
");
            // 1. Add filters to template query
            string query = AddFilter(queryBuilder, user, title, authors, keywords, year, personalComment);

            // 2. Add Pagination
            query += " LIMIT " + itemsPerPage.ToString() + " OFFSET " + offset.ToString() + ";";

            // 3.Fetch articles
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Query(AttachUser(), transaction);

                    results = conn.Query<Article>(query, transaction: transaction).ToList();

                    transaction.Commit();
                }
            }

            return results;

        }
        // Get count of fetched articles
        public int GetRecordCount(User user, string title, List<string> authors, List<string> keywords, string year, string personalComment)
        {
            int result = 1;

            // Template query
            StringBuilder queryBuilder = new StringBuilder(@"
SELECT COUNT(final.ID)
FROM
(SELECT cmp.ID, cmp.Article AS Title, cmp.Authors, cmp.Keywords, cmp.Year, cmp.FileName AS [FileName], per.PersonalComment, IFNULL(per.SIC, 0) AS SIC
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
LEFT JOIN
(SELECT ArticleID, PersonalComment, SIC
FROM tblUserPersonal WHERE UserID = #UserID) AS per ON cmp.ID = per.ArticleID) AS final
");

            // 1. Add filters to template query
            string query = AddFilter(queryBuilder, user, title, authors, keywords, year, personalComment);


            // 2. Fetch count
            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Query(AttachUser(), transaction);

                    result = conn.QuerySingleOrDefault<int>(query, transaction: transaction);

                    transaction.Commit();
                }
            }

            return result;
        }
        public Article GetArticleWithId(int? id)
        {
            // Null escape
            if (id == null)
                return new Article();

            Article result;

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
WHERE cmp.ID = @ID;
";

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    result = conn.QuerySingleOrDefault<Article>(query, new { ID = id }, transaction: transaction);
                    transaction.Commit();
                }
            }

            return result;
        }
        public int CheckArticleWithTitle(string title)
        {
            int result;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    result = conn.QuerySingleOrDefault<int>(@"SELECT ID FROM tblArticle WHERE Title=@Title;", new { Title = title }, transaction: transaction);
                    transaction.Commit();
                }
            }

            return result;
        }
        public Article GetArticleWithTitle(string title)
        {
            Article result;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Query(AttachUser(), transaction);

                    result = conn.QuerySingleOrDefault<Article>("SELECT ID FROM tblArticle WHERE Title=@Title",
                        new { Title = title }, transaction: transaction);

                    transaction.Commit();
                }
            }

            return result;
        }
        public Article GetFullArticleWithTitle(User user, string title)
        {
            Article result;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    conn.Query(AttachUser(), transaction);

                    string query = @"
SELECT final.ID, final.Title, final.Authors, final.Keywords, final.Year, final.FileName, final.PersonalComment, final.SIC
FROM
(SELECT cmp.ID, cmp.Article AS Title, cmp.Authors, cmp.Keywords, cmp.Year, cmp.FileName AS [FileName], per.PersonalComment, IFNULL(per.SIC, 0) AS SIC
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
LEFT JOIN
(SELECT ArticleID, PersonalComment, SIC
FROM tblUserPersonal WHERE UserID = @UserID) AS per ON cmp.ID = per.ArticleID) AS final
WHERE Title = @Title
";

                    result = conn.QuerySingleOrDefault<Article>(query,
                        new { UserID = user.ID, Title = title }, transaction: transaction);

                    transaction.Commit();
                }
            }

            return result;
        }

        private void AddAuthors(SQLiteConnection conn, SQLiteTransaction transaction, Article article)
        {
            long author_id;
            List<long> lstAuthorID = new List<long>();
            // Insert Authors
            if (article.AuthorsCollection.Count > 0)
            {
                // Iterate through Authors collection
                foreach (string author in article.AuthorsCollection)
                {
                    // Check if the author is already in database
                    author_id = conn.QueryFirstOrDefault<long>(@"SELECT ID FROM tblAuthor WHERE Name LIKE @Name;",
                        new { Name = author }, transaction);
                    if (author_id != 0)
                    {
                        // If the author is in database retrieve its id and add it to lstAuthorID
                        lstAuthorID.Add(author_id);
                    }
                    else
                    {
                        conn.Execute(@"INSERT INTO tblAuthor (Name) VALUES (@Name);",
                            new { Name = author }, transaction);
                        lstAuthorID.Add(conn.LastInsertRowId);
                    }
                }

                //Foreach author Ids insert ArticleID and AuthorIds from list in joint table for many-to-many relationship
                foreach (long authorID in lstAuthorID)
                {
                    conn.Execute(@"INSERT into jntArticleAuthor (Article_ID, Author_ID) VALUES (@ArticleID, @AuthorID);",
                        new { ArticleID = article.ID, AuthorID = authorID }, transaction);
                }
            }
        }
        private void AddKeywords(SQLiteConnection conn, SQLiteTransaction transaction, Article article)
        {
            long keyword_id;
            List<long> lstKeywordID = new List<long>();

            // Insert Keywords --> See reference to "Insert Authors"
            if (article.KeywordsCollection.Count > 0)
            {
                foreach (var keyword in article.KeywordsCollection)
                {
                    keyword_id = conn.QueryFirstOrDefault<long>(@"SELECT ID FROM tblKeyword WHERE Keyword LIKE @Keyword;",
                        new { Keyword = keyword }, transaction);
                    if (keyword_id != 0)
                    {
                        lstKeywordID.Add(keyword_id);
                    }
                    else
                    {
                        conn.Execute(@"INSERT INTO tblKeyword (Keyword) VALUES (@Keyword);",
                            new { Keyword = keyword }, transaction);
                        lstKeywordID.Add(conn.LastInsertRowId);
                    }
                }

                foreach (var keywordID in lstKeywordID)
                {
                    conn.Execute(@"INSERT into jntArticleKeyword (Article_ID, Keyword_ID) VALUES (@ArticleID, @KeywordID);",
                        new { ArticleID = article.ID, KeywordID = keywordID }, transaction);
                }
            }
        }
        private string AddFilter(
            StringBuilder queryBuilder,
            User user,
            string title,
            List<string> authors,
            List<string> keywords,
            string year,
            string personalCommnet)
        {
            StringBuilder result = queryBuilder;

            // 1. Add user id for comments and sic
            result.Replace("#UserID", user.ID.ToString());

            // 2. Append title filter
            if (!string.IsNullOrWhiteSpace(title))
                result.Append(" WHERE final.Title LIKE " + ToWildCard(title));


            // 3. Add WHERE clause if title was null but authors or keywords aren't
            if ((authors.Count > 0 || keywords.Count > 0 || !string.IsNullOrEmpty(year) || !string.IsNullOrEmpty(personalCommnet)) && string.IsNullOrEmpty(title))
                result.Append(" WHERE ");

            // 4. Add authors filter
            if (authors.Count > 0)
            {
                // 1. Determine if we need to add "AND"
                if (title != null)
                    result.Append(" AND ");

                // 2. Add filter clause for each author
                foreach (string author in authors)
                {
                    result.Append("final.Authors LIKE " + ToWildCard(author));

                    // If its not the last iteration add "AND"
                    if (author != authors.Last())
                        result.Append(" AND ");
                }
            }

            // 5. Add keywords filter
            if (keywords.Count > 0)
            {
                // 1. Determine if we need to add "AND" or "WHERE"
                if (authors.Count > 0 || !string.IsNullOrEmpty(title))
                    result.Append(" AND ");

                // 2. Add filter clause for each author
                foreach (string keyword in keywords)
                {
                    result.Append("final.Keywords LIKE " + ToWildCard(keyword));

                    // If its not the last iteration add "AND"
                    if (keyword != keywords.Last())
                        result.Append(" AND ");
                }
            }

            // 6. Year filter
            if (!string.IsNullOrWhiteSpace(year))
            {
                // Determine if we need to add "AND"
                if (authors.Count > 0 || keywords.Count > 0 || !string.IsNullOrEmpty(title))
                    queryBuilder.Append(" AND ");

                if (year.Contains("-"))
                {
                    string[] dates = year.Split('-');
                    if (dates.Length == 2 && !string.IsNullOrWhiteSpace(dates[1]))
                        queryBuilder.Append($" final.year >= {dates[0]} AND year <= {dates[1]}");
                    else
                        queryBuilder.Append($" final.year = {dates[0]}");
                }
                else
                {
                    queryBuilder.Append($" final.year = {year}");
                }
            }

            // 7. Personal comment filter
            if (!string.IsNullOrWhiteSpace(personalCommnet))
            {
                // Determine if we need to add "AND"
                if (authors.Count > 0 || keywords.Count > 0 || !string.IsNullOrEmpty(year) || !string.IsNullOrEmpty(title) )
                    queryBuilder.Append(" AND ");

                queryBuilder.Append($" final.PersonalComment LIKE {ToWildCard(personalCommnet)}");
            }


            Console.WriteLine(result.ToString());

            // 8. Return the result
            return result.ToString();
        }
        private string ToWildCard(string input)
        {
            return "'%" + input + "%'";
        }
        private void ConsolidateReferences()
        {
            string sql;

            using (SQLiteConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Open();
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    // 1. Authors check
                    sql = @"
DELETE FROM tblAuthor WHERE ID IN
(SELECT ath.ID
FROM tblAuthor AS ath
LEFT JOIN jntArticleAuthor AS aa ON ath.ID = aa.Author_ID
WHERE aa.Author_ID IS NULL);
";
                    conn.Execute(sql, transaction);

                    // 2. Keywords check
                    sql = @"
DELETE FROM tblKeyword WHERE ID IN
(SELECT kwd.ID
FROM tblKeyword AS kwd
LEFT JOIN jntArticleKeyword AS ak ON kwd.ID = ak.Keyword_ID
WHERE ak.Keyword_ID IS NULL);
";
                    conn.Execute(sql, transaction);
                    transaction.Commit();
                }
            }
        }
    }
}
