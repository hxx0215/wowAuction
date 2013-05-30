using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MySQLDriverCS;
namespace wowAuctionbackground
{
    class mySQLHelper
    {
        static public void open()
        {
            MySQLConnection conn = new MySQLConnection(new MySQLConnectionString("192.168.1.102", "testwordpress", "pma", "tjyrzhxx15").AsString);
            conn.Open();
            MySQLCommand commn = new MySQLCommand("set names gb2312", conn);
            commn.ExecuteNonQuery();
        }
    }
}
