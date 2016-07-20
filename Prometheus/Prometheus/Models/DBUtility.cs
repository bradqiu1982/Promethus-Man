using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;


namespace Prometheus.Models
{
    public class DBUtility
    {
        public static string escapedbstr(string oldstr)
        {
            return oldstr.Replace("'", "\"");
        }

        private static SqlConnection GetLocalConnector()
        {
            var conn = new SqlConnection();
            try
            {
                conn.ConnectionString = "Data Source = (LocalDb)\\MSSQLLocalDB; AttachDbFilename = \"~/App_Data/Prometheus.mdf\"; Integrated Security = True";
                conn.Open();
                return conn;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
        }

        private static void CloseConnector(SqlConnection conn)
        {
            if (conn == null)
                return;

            try
            {
                conn.Close();
            }
            catch (Exception ex)
            {
            }
        }

        public static bool ExeLocalSqlNoRes(string sql)
        {
            var conn = GetLocalConnector();
            if (conn == null)
                return false;

            try
            {
                var command = conn.CreateCommand();
                command.CommandText = escapedbstr(sql);
                command.ExecuteNonQuery();
                CloseConnector(conn);
                return true;
            }
            catch (Exception ex)
            {
                CloseConnector(conn);
                System.Windows.MessageBox.Show(ex.ToString());
                return false;
            }
        }

        public static List<List<object>> ExeLocalSqlWithRes( string sql)
        {
            var ret = new List<List<object>>();
            var conn = GetLocalConnector();
            try
            {
                if (conn == null)
                    return ret;

                var command = conn.CreateCommand();
                command.CommandText = escapedbstr(sql);
                var sqlreader = command.ExecuteReader();
                if (sqlreader.HasRows)
                {

                    while (sqlreader.Read())
                    {
                        var newline = new List<object>();
                        for (var i = 0; i < sqlreader.FieldCount; i++)
                        {
                            newline.Add(sqlreader.GetValue(i));
                        }
                        ret.Add(newline);
                    }
                }

                sqlreader.Close();
                CloseConnector(conn);
                return ret;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
        }

        private static SqlConnection GetMESConnector()
        {
            var conn = new SqlConnection();
            try
            {
                conn.ConnectionString = "Server=CN-CSSQL;uid=SHG_Read;pwd=shgread;Database=InsiteDB;";
                conn.Open();
                return conn;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
        }

        public static bool ExeMESSqlNoRes(string sql)
        {
            var conn = GetMESConnector();
            if (conn == null)
                return false;

            try
            {
                var command = conn.CreateCommand();
                command.CommandText = escapedbstr(sql);
                command.ExecuteNonQuery();
                CloseConnector(conn);
                return true;
            }
            catch (Exception ex)
            {
                CloseConnector(conn);
                System.Windows.MessageBox.Show(ex.ToString());
                return false;
            }
        }

        public static List<List<object>> ExeMESSqlWithRes(string sql)
        {
            var ret = new List<List<object>>();
            var conn = GetMESConnector();
            try
            {
                if (conn == null)
                    return ret;

                var command = conn.CreateCommand();
                command.CommandText = sql;
                var sqlreader = command.ExecuteReader();
                if (sqlreader.HasRows)
                {

                    while (sqlreader.Read())
                    {
                        var newline = new List<object>();
                        for (var i = 0; i < sqlreader.FieldCount; i++)
                        {
                            newline.Add(sqlreader.GetValue(i));
                        }
                        ret.Add(newline);
                    }
                }

                sqlreader.Close();
                CloseConnector(conn);
                return ret;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
                CloseConnector(conn);
                ret.Clear();
                return ret;
            }
        }


        /*
        public static bool AddMovie(string title, string releasedate, string genre, float price, string rate)
        {
            string sqlstr = "insert into Movies(Title,ReleaseDate,Genre,Price,Rate) values('<title>','<releasedate>','<genre>',<price>,'<rate>')";
            sqlstr = sqlstr.Replace("<title>", escape(title))
                .Replace("<releasedate>", escape(releasedate))
                .Replace("<genre>", escape(genre))
                .Replace("<price>", price.ToString())
                .Replace("<rate>", escape(rate));
            return DBUtility.ExeSqlNoRes(sqlstr);
        }

        public static bool UpdateMovie(long id, string title, string releasedate, string genre, float price, string rate)
        {
            string sqlstr = "Update Movies Set Title = '<title>',ReleaseDate = '<releasedate>',Genre = '<genre>',price = <price>,rate = '<rate>' where Id = <id>";
            sqlstr = sqlstr.Replace("<id>", id.ToString())
                .Replace("<title>", escape(title))
                .Replace("<releasedate>", escape(releasedate))
                .Replace("<genre>", escape(genre))
                .Replace("<price>", price.ToString())
                .Replace("<rate>", escape(rate));
            return DBUtility.ExeSqlNoRes(sqlstr);
        }

        public static bool DeleteMovie(long id)
        {
            string sqlstr = "Delete from Movies where Id =" + id.ToString();
            return DBUtility.ExeSqlNoRes(sqlstr);
        }

        public static List<Movie> GetMovies(string sql)
        {
            var mvlist = new List<Movie>();
            var conn = GetConnector();
            if (conn == null)
                return new List<Movie>();
            var reader = ExeSqlWithRes(conn, sql);
            if (reader == null)
                return new List<Movie>();
            try
            {
                while (reader.Read())
                {
                    mvlist.Add(new Movie(reader.GetInt64(0), reader.GetString(1), reader.GetDateTime(2).ToString(), reader.GetString(3), (float)reader.GetDouble(4), reader.GetString(5)));
                }
                reader.Close();
                CloseConnector(conn);
                return mvlist;
            }
            catch (Exception ex)
            {
                mvlist.Clear();
                reader.Close();
                CloseConnector(conn);
                System.Windows.MessageBox.Show(ex.ToString());
                return new List<Movie>();
            }
        }
        */
    }
}