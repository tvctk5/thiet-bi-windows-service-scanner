using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scanner_Service.Models
{
    public class Message
    {
        public string id { get; set; }
        public string hostId { get; set; }
        public string deviceId { get; set; }
        public string sent { get; set; }
        public string device_hostId { get; set; }
        public string message { get; set; }
        public string hostname { get; set; }
        public string createddate { get; set; }
    }
}
