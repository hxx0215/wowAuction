using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
namespace wowAuctionbackground
{
    public partial class Form1 : Form
    {
        private Thread thread = new Thread(new ThreadStart(start));
        static public bool stop;
        static public void start()
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
            //thread.Start();
            //Console.WriteLine("{0}", thread.ThreadState);
            if (thread.ThreadState==ThreadState.Unstarted)
                thread.Start();
            stop = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            stop = true;
        }
    }
}
