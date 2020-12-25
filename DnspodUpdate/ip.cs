using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DnspodUpdate
{
    [DataContract]
    public class ips
    {
        [DataMember]
        public string ip { get; set; }
        [DataMember]
        public string country { get; set; }
        [DataMember]
        public string province { get; set; }
        [DataMember]
        public string city { get; set; }
        [DataMember]
        public string isp { get; set; }
        [DataMember]
        public string area { get; set; }
        [DataMember]
        public string loc { get; set; }

    }
}
