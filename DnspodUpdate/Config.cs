using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnspodUpdate
{
    [Serializable]
    public class Config
    {
        public string TokenId { get; set; } = "";
        public string Token { get; set; } = "";
        public List<string> DOMAIN { get; set; } = new List<string>();
        public string Ip { get; set; } = "127.0.0.1";
        public string Addr { get; set; }
        public string Speed = "300";
    }
}
