using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.IO;

namespace Prometheus.Models
{
    public class DBUtility
    {

        private static SqlConnection GetLocalConnector()
        {
            var conn = new SqlConnection();
            try
            {
                //conn.ConnectionString = "Data Source = (LocalDb)\\MSSQLLocalDB; AttachDbFilename = ~\\App_Data\\Prometheus.mdf; Integrated Security = True";
                conn.ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=" + Path.Combine(HttpRuntime.AppDomainAppPath, "App_Data\\Prometheus.mdf") + ";Integrated Security=True;";
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
                return null;
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
            catch (SqlException ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
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
                command.CommandText = sql;
                command.ExecuteNonQuery();
                CloseConnector(conn);
                return true;
            }
            catch (SqlException ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
                return false;
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
            catch (SqlException ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
                CloseConnector(conn);
                ret.Clear();
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
            catch (SqlException ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
                return null;
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
                command.CommandText = sql;
                command.ExecuteNonQuery();
                CloseConnector(conn);
                return true;
            }
            catch (SqlException ex)
            {
                CloseConnector(conn);
                System.Windows.MessageBox.Show(ex.ToString());
                return false;
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
            catch (SqlException ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
                CloseConnector(conn);
                ret.Clear();
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


    }
}