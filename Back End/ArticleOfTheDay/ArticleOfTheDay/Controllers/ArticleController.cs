using ArticleOfTheDay.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MySql.Data.MySqlClient;

namespace ArticleOfTheDay.Controllers
{
    public class ArticleController : ApiController
    {
        const string CONNECTION_STRING = "SERVER=localhost;DATABASE=articles;UID=articles;PASSWORD=password;";

        [HttpGet]
        [Route("api/Article/GetAllArticles")]
        public IEnumerable<Article> GetAllArticles()
        {
            using (var connection = new MySqlConnection(CONNECTION_STRING))
            {
                connection.Open();

                try
                {
                    using (var command = new MySqlCommand("SELECT * FROM articles", connection))
                    {
                        var articles = new List<Article>();

                        var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            articles.Add(new Article
                            {
                                Id = reader.GetInt32(0),
                                Poster = reader.GetString(1),
                                Title = reader.GetString(2),
                                Summary = reader.GetString(3),
                                PostDate = reader.GetDateTime(4),
                            });
                        }

                        return articles;
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        [HttpGet]
        [Route("api/Article/ArticlesByPoster/{name}")]
        public IEnumerable<Article> ArticlesByPoster(string name)
        {
            using (var connection = new MySqlConnection(CONNECTION_STRING))
            {
                connection.Open();

                try
                {
                    using (var command = new MySqlCommand("SELECT * FROM articles WHERE POSTER = @name", connection))
                    {
                        command.Parameters.AddWithValue("@name", name);

                        var articles = new List<Article>();

                        var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            articles.Add(new Article
                            {
                                Id = reader.GetInt32(0),
                                Poster = reader.GetString(1),
                                Title = reader.GetString(2),
                                Summary = reader.GetString(3),
                                PostDate = reader.GetDateTime(4),
                            });
                        }

                        return articles;
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        [HttpGet]
        [Route("api/Article/ArticlesForMonth/{year}/{month}")]
        public IEnumerable<Article> ArticlesForMonth(int year, int month)
        {
            using (var connection = new MySqlConnection(CONNECTION_STRING))
            {
                connection.Open();

                try
                {
                    using (var command = new MySqlCommand("SELECT * FROM articles WHERE postDate BETWEEN @earlierDate AND @laterDate", connection))
                    {
                        command.Parameters.AddWithValue("@earlierDate", year + "-" + month + "-" + 1);
                        command.Parameters.AddWithValue("@laterDate", year + "-" + month + "-" + DateTime.DaysInMonth(year, month));

                        var articles = new List<Article>();

                        var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            articles.Add(new Article
                            {
                                Id = reader.GetInt32(0),
                                Poster = reader.GetString(1),
                                Title = reader.GetString(2),
                                Summary = reader.GetString(3),
                                PostDate = reader.GetDateTime(4),
                            });
                        }

                        return articles;
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        [HttpPost]
        [Route("api/Article/AddArticle")]
        public HttpResponseMessage AddArticle(Article articleToAdd)
        {
            using (var connection = new MySqlConnection(CONNECTION_STRING))
            {
                connection.Open();

                try
                {
                    bool dateTaken = false;

                    using (var command = new MySqlCommand("SELECT EXISTS(SELECT postDate FROM articles WHERE postDate = @postDate)", connection))
                    {
                        command.Parameters.AddWithValue("@postDate", articleToAdd.PostDate);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                               dateTaken = reader.GetBoolean(0);
                            }
                        }
                    }

                    if (dateTaken)
                    {
                        return new HttpResponseMessage(HttpStatusCode.Conflict);
                    }

                    using (var command = new MySqlCommand("INSERT INTO articles (Poster, Title, Summary, PostDate) VALUES (@poster, @title, @summary, @postDate)", connection))
                    {
                        command.Parameters.AddWithValue("@poster", articleToAdd.Poster);
                        command.Parameters.AddWithValue("@title", articleToAdd.Title);
                        command.Parameters.AddWithValue("@summary", articleToAdd.Summary);
                        command.Parameters.AddWithValue("@postDate", articleToAdd.PostDate);

                        command.ExecuteNonQuery();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }

            return new HttpResponseMessage(HttpStatusCode.Created);
        }
    }
}
