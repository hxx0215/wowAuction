using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
namespace wowAuctionbackground
{
    class mySQLHelper
    {
        static private MySqlConnection con;
        static private string constr = "database=wowauction;password=922033;User ID=pma;server=192.168.1.102";
        static public void open()
        {
            con = new MySqlConnection(constr);
            //con.Open();
        }
        static public void update(JArray jo,List<string> columnname,string tablename,string wherecolumn)
        {
            for (int i = 0; i < jo.Count; i++)
            {
                con.Open();
                string cmdstr = "select * from " + tablename + " where " + wherecolumn + "='" + jo[i][wherecolumn] + "'";
                MySqlCommand cmd = new MySqlCommand(cmdstr, con);
                MySqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    cmdstr = "update "+tablename+" set ";
                    string key = getKeys((JObject)jo[i], "");
                    key = key.Remove(key.Length - 1, 1);
                    string[] keys=key.Split(new Char[] { ',' });
                    string value = getValues((JObject)jo[i]);
                    value = value.Remove(value.Length - 1, 1);
                    string[] values = value.Split(new Char[] { ',' });
                    if (keys.Length != values.Length)
                        Console.WriteLine("update {0} failed:keys.length!=values.length", tablename);
                    else
                    {
                        for (int j = 0; j < keys.Length; j++)
                            cmdstr += " " + keys[j] + " = " + values[j] + " , ";
                        cmdstr = cmdstr.Remove(cmdstr.Length - 2, 2);
                        cmdstr += " where " + wherecolumn + "='" + jo[i][wherecolumn] + "'";
                        MySqlConnection connect = new MySqlConnection(constr);
                        connect.Open();
                        MySqlCommand upcmd = new MySqlCommand(cmdstr, connect);
                        Console.Write("update {0}...", jo[i][wherecolumn]);
                        upcmd.ExecuteNonQuery();
                        Console.WriteLine("Success!");
                        connect.Close();
                    }
                }
                else
                {
                    string key= getKeys((JObject)jo[i],"");
                    key = key.Remove(key.Length - 1, 1);
                    string value = getValues((JObject)jo[i]);
                    value = value.Remove(value.Length - 1, 1);
                    cmdstr = "insert into " + tablename + " (" + key + ") values (" + value + ")";
                    MySqlConnection connect = new MySqlConnection(constr);
                    connect.Open();
                    MySqlCommand inscmd = new MySqlCommand(cmdstr, connect);
                    Console.Write("insert {0}...", jo[i][wherecolumn]);
                    inscmd.ExecuteNonQuery();
                    Console.WriteLine("Success!");
                    connect.Close();
                }
                con.Close();
            }
            Console.WriteLine("获取服务器列表成功一共获取{0}个服务器", jo.Count);
        }
        static public string getKeys(JObject jo,string prekey)
        {
            string ret="";
            Dictionary<string, object> dc = jo.ToObject<Dictionary<string, object>>();
            foreach (string key in dc.Keys)
            {
                if (Object.ReferenceEquals(dc[key].GetType(), jo.GetType()))
                {
                    if (prekey == "")
                        ret += getKeys((JObject)dc[key], key);
                    else ret += getKeys((JObject)dc[key], prekey + "_" + key);
                }
                else
                {
                    if (prekey == "")
                    {
                        ret += "`" + key + "`" + ",";
                    }
                    else
                    {
                        ret += "`" + prekey + "_" + key + "`" + ",";
                    }
                }
            }
            return ret;
        }
        static public string getValues(JObject jo)
        {
            string ret = "";
            Dictionary<string, object> dc = jo.ToObject<Dictionary<string, object>>();
            foreach (string key in dc.Keys)
            {
                if (Object.ReferenceEquals(dc[key].GetType(), jo.GetType()))
                {
                    ret += getValues((JObject)dc[key]);
                }
                else ret += "'"+dc[key].ToString()+"'"+",";
            }
            return ret;
        }
    }
}
