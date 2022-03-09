using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwesomepiaResultViewer.Utility
{
    internal static class SQLHelper
    {
        private const string ServerIP = "211.104.146.87";
        private const string Port = "53383";

        private const string DataBase = "MineHealth";
        private const string Uid = "minehealthsql";
        private const string Pwd = "minehealthsql";

        private const string connStr = "Server=" + ServerIP + ";Port=" + Port + ";Database=" + DataBase + ";Uid=" + Uid + ";Pwd=" + Pwd;



        public static DataTable GetPhone()
        {
            DataTable dt = new DataTable("datas");
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    string Query = $"SELECT Phone, TestID, TestDate From UserLogTbl WHERE TestDate > '2022-03-02 17:00:00' ORDER BY TestDate ASC";
                    conn.Open();
                    using (MySqlCommand cmd = new(Query, conn))
                    {
                        using (MySqlDataAdapter returnVal = new MySqlDataAdapter(cmd))
                        {
                            returnVal.Fill(dt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Debug.WriteLine(ex.Message);
                }
            }
            return dt;
        }

        public static DataTable GetPoint(string direction, string uuid)
        {
            DataTable dt = new DataTable("datas");
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    string Query = $"SELECT * From PoseATbl WHERE TestID = '{uuid}';";
                    conn.Open();
                    using (MySqlCommand cmd = new(Query, conn))
                    {
                        using (MySqlDataAdapter returnVal = new MySqlDataAdapter(cmd))
                        {
                            returnVal.Fill(dt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Debug.WriteLine(ex.Message);
                }
            }

            return dt;
        }
    }

}
