using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System.Threading;
namespace wowAuctionbackground
{
    class mySQLHelper
    {
        static private string constr = "database=wowauction;password=922033;User ID=pma;server=192.168.1.102";
        static public int beginn = 0;
        static public int succn = 0;
        static public void open()
        {
            //con = new MySqlConnection(constr);
            //con.Open();
        }


        static public int update(JArray jo,string tablename,string addkeys="",string addvalues="",string addwhere="",List<string>uniquekey=null)
        {
            MySqlConnection conn = new MySqlConnection(constr);
            string SQL = "";
            conn.Open();
            //MySqlDataReader dr;
            int updaten = 0;
            for (int i = 0; i < jo.Count; i++)
            {
                string key = addkeys;
                if (uniquekey != null)
                {
                    key += "`";
                    foreach (string str in uniquekey) key += str;
                    key += "`,";
                }
                key += getKeys((JObject)jo[i], "");
                key = key.Remove(key.Length - 1, 1);
                string[] keys=key.Split(new Char[] { ',' });
                string value = addvalues;
                if (uniquekey != null)
                {
                    value += "'";
                    foreach (string str in uniquekey) 
                        value += jo[i][str].ToString();
                    value += "',";
                }
                value += getValues((JObject)jo[i]);
                value = value.Remove(value.Length - 1, 1);
                string[] values = value.Split(new Char[] { ',' });
                if (keys.Length != values.Length)
                    Console.WriteLine("update {0} failed:keys.length!=values.length", tablename);
                else
                {
                    string cmdstr = "replace into " + tablename + " (" + key + ") values (" + value + ")";
                    SQL += cmdstr + "; ";
                    updaten++;
                }
                if (updaten >= 200)
                {
                    Console.WriteLine("{2}:begin update:{0} to {1}",beginn,tablename,DateTime.Now);
                    beginn++;
                    ParameterizedThreadStart Parstart = new ParameterizedThreadStart(updateSQL);
                    Thread myThread = new Thread(Parstart);
                    myThread.Start(SQL);
                    SQL = "";
                    updaten = 0;
                    }
                }
                if (SQL != "")
                {
                    Console.WriteLine("{2}:begin update:{0} to {1}", beginn, tablename, DateTime.Now);
                    beginn++;
                    ParameterizedThreadStart Parstart = new ParameterizedThreadStart(updateSQL);
                    Thread myThread = new Thread(Parstart);
                    myThread.Start(SQL);
                }
            conn.Close();
            return jo.Count;
            //Console.WriteLine("获取服务器列表成功一共获取{0}个服务器", jo.Count);
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
        static public List<Dictionary<string, string>> getAllData(string tablename, List<string> columnname)
        {
            //Dictionary<string, string> c = new Dictionary<string, string>();
            return getData(tablename, columnname, "");
        }
        static public List<Dictionary<string, string>> getData(string tablename, List<string> columnname, Dictionary<string, string> clause)
        {
            string c=constractWhereClause(clause);
            return getData(tablename, columnname, c);
        }
        static public List<Dictionary<string, string>> getData(string tablename, List<string> columnname,string clause)
        {
            List<Dictionary<string, string>> ret = new List<Dictionary<string, string>>();
            MySqlConnection conn = new MySqlConnection(constr);
            conn.Open();
            string columns = "";
            foreach (string str in columnname) columns += "`"+str+"`" + ",";
            columns = columns.Remove(columns.Length - 1, 1);
            string cmdstr = "select " + columns + " from " + tablename + " "+clause;
            MySqlCommand cmd = new MySqlCommand(cmdstr, conn);
            MySqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Dictionary<string, string> dc = new Dictionary<string, string>();
                foreach (string str in columnname)
                    dc.Add(str, dr[str].ToString());
                ret.Add(dc);
            }
            conn.Close();
            return ret;
        }
        static public string constractWhereClause(Dictionary<string, string> clause)
        {
            string ret = " where ";
            foreach (string key in clause.Keys)
            {
                ret += "`"+key +"`"+ " = '" + clause[key] + "' and ";
            }
            if (ret.Length > 7)
                ret = ret.Remove(ret.Length - 4, 4);
            else
                ret = "";
            return ret;
        }
        static public string constractWhereClausewithJObject(List<string> wherecolumn, JObject jo)
        {
            Dictionary<string, string> c = new Dictionary<string, string>();
            foreach (string str in wherecolumn)
                c.Add(str, jo[str].ToString());
            return constractWhereClause(c);
        }
        static public void testtime(string str)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            DateTime nowTime = DateTime.Now;
            long unixTime = (long)Math.Round((nowTime - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero);
            Console.WriteLine("{0}:{1}",str,unixTime);
        }
        static public void updateSQL(object str)
        {
            MySqlConnection con = new MySqlConnection(constr);
            con.Open();
            MySqlCommand c = new MySqlCommand(str.ToString(), con);
            //Console.Write("{2},begin update {0} to {1}...", updaten, tablename, DateTime.Now);
            c.ExecuteNonQuery();
            Console.WriteLine("{1}:{0} updateSuccess!",succn,DateTime.Now);
            succn++;
        }
    }
}
