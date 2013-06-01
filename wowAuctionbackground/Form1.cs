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
            
            List<string> columnname=new List<string>();
            mySQLHelper.open();
            mySQLHelper.update((JArray)jo["realms"], columnname, "t_realmstatus","name");
        }
    }
}
