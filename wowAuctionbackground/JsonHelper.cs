using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
namespace wowAuctionbackground
{
    class JsonHelper
    {
        public string key;
        public object value;
        public JsonHelper(JObject jo)
        {
            this.key = "";
            this.value = "";
        }
    }
}
