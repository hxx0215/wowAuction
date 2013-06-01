using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Timers;
namespace wowAuctionbackground
{
    public partial class Form1 : Form
    {
        private Thread thread = new Thread(new ThreadStart(threadfunc));
        static public bool stop;
        static public string Host = "http://www.battlenet.com.cn";
        static public string AuctionURL = "/api/wow/auction/data/";
        static public string realmlistURL = "/api/wow/realm/status";
        static public string realm = "激流之傲";
        static public System.Timers.Timer urltimer = new System.Timers.Timer(1);
        static public System.Timers.Timer auctiontimer = new System.Timers.Timer(1);
        static public void threadfunc()
        {
            while (true)
            {
                if (!stop)
                    Console.WriteLine("1");
                else Thread.Sleep(1000);
            }
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //if (thread.ThreadState==ThreadState.Unstarted)
                //thread.Start();
            stop = false;
            //Console.WriteLine(HttpGet(Host + AuctionURL + realm, ""));
            string json=HttpGet(Host + AuctionURL + realm, "");
            JObject jo = (JObject)JsonConvert.DeserializeObject(json);
            mySQLHelper.open();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            stop = true;
        }
        public string HttpGet(string Url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string json = HttpGet(Host + realmlistURL, "");
            JObject jo = (JObject)JsonConvert.DeserializeObject(json);
            mySQLHelper.open();
            int num=mySQLHelper.update((JArray)jo["realms"],  "t_realmstatus","name");
            Console.WriteLine("获取服务器列表成功一共获取{0}个服务器", num);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            urltimer.AutoReset = false;
            urltimer.Elapsed += new System.Timers.ElapsedEventHandler(getURL);
            urltimer.Enabled = true;
        }
        public void getURL(object source, System.Timers.ElapsedEventArgs e)
        {
            string tablename = "t_realmstatus";
            List<string> columnname = new List<string>();
            columnname.Add("ID"); columnname.Add("name");
            List<Dictionary<string, string>> realms = mySQLHelper.getAllData(tablename, columnname);
            foreach (Dictionary<string, string> dc in realms)
            {
                string URL = Host + AuctionURL + dc["name"];
                string json = HttpGet(URL, "");
                JObject jo = (JObject)JsonConvert.DeserializeObject(json);
                ((JObject)jo["files"][0]).Add("FK", dc["ID"]);
                ((JObject)jo["files"][0]).Add("name", dc["name"]);
                mySQLHelper.update((JArray)jo["files"], "t_auctionurl", "name");
            }
            Console.WriteLine("拉取数据成功一共获取{0}个服务器AH数据", realms.Count);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //auctiontimer.AutoReset = false;
            //auctiontimer.Elapsed+=new ElapsedEventHandler(getRealTimeAuctionData);
            //auctiontimer.Enabled = true;
            getRealTimeAuctionData();
        }
        public void getRealTimeAuctionData()//(object source, System.Timers.ElapsedEventArgs e)
        {
            string URL = Host + AuctionURL + realm;
            string json = HttpGet(URL, "");
            JObject jo = (JObject)JsonConvert.DeserializeObject(json);
            string AuctionDataURL = jo["files"][0]["url"].ToString();
            string LastModified = jo["files"][0]["lastModified"].ToString();
            List<Dictionary<string, string>> ldc = new List<Dictionary<string, string>>();
            List<string> column=new List<string>();
            column.Add("lastModified");
            Dictionary<string, string> clause = new Dictionary<string, string>();
            clause.Add("chnname", realm); clause.Add("lastModified", LastModified);
            ldc = mySQLHelper.getData("t_realtimeauctiondata", column,clause);
            if (ldc.Count > 0)
            {
                Console.WriteLine("当前{0}数据已为最新不需要更新", realm);
                return;//改为continue;
            }
            json = HttpGet(AuctionDataURL, "");
            jo = (JObject)JsonConvert.DeserializeObject(json);
            string addkey = mySQLHelper.getKeys((JObject)jo["realm"], "");
            addkey += "`side`,";
            string addValue = mySQLHelper.getValues((JObject)jo["realm"]);
            addValue += "'alliance',";
            string addwhere = "and `name`='" + jo["realm"]["name"] + "'";
            int num=mySQLHelper.update((JArray)jo["alliance"]["auctions"], "t_realtimeauctiondata", "auc",addkey,addValue,addwhere);
            Console.WriteLine("共取得{0}联盟拍卖行数据{1}条", realm, num);
        }
    }
}
