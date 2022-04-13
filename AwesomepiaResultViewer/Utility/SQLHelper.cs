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
                    string Query = $"SELECT Phone, TestID, TestDate From UserLogTbl WHERE TestDate > '2022-03-02 17:00:00' ORDER BY Id ASC";
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

        public static DataTable GetResultScoreTbl()
        {
            DataTable dt = new DataTable("datas");
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    string Query = $"SELECT Id, Phone, TestID, TestDate, ScoreFA, ScoreSA, ScoreSN From ResultScoreTbl ORDER BY Id ASC";
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

            string tablename = "PoseATbl";
            if (direction == "front")
                tablename = "PoseATbl";
            else if (direction == "side")
                tablename = "PoseBTbl";

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    string Query = $"SELECT * From {tablename} WHERE TestID = '{uuid}';";
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
    

        public enum ScoreCategory
        {
            FrontAngle = 0,
            SideAngle = 1,
            SideNeck = 2
        }
        public static bool SaveDatas(string TestId, ScoreCategory sc, string score)
        {
            bool result = false;
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    string valueqry = "";
                    switch (sc)
                    {
                        case ScoreCategory.FrontAngle:
                            valueqry = "ScoreFA";
                            break;
                        case ScoreCategory.SideAngle:
                            valueqry = "ScoreSA";
                            break;
                        case ScoreCategory.SideNeck:
                            valueqry = "ScoreSN";
                            break;
                        default:
                            return false;
                            break;
                    }

                    string Query = $"UPDATE ResultScoreTbl SET {valueqry} = '{score}' WHERE TestID = '{TestId}'";
                    conn.Open();
                    using (MySqlCommand cmd = new(Query, conn))
                    {
                        if (cmd.ExecuteNonQuery() > 0)
                            result = true;
                    }
                }
                catch (Exception ex)
                {
                    //Debug.WriteLine(ex.Message);
                }
            }
            return result;
        }
    
    }

}
