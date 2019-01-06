using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scanner_Service.Models
{
    public class Sms
    {
        public string id { get; set; }
        public string hostId { get; set; }
        public string deviceId { get; set; }
        public int type { get; set; }
        public bool sent { get; set; }
        public int sms_groupId { get; set; }
    }
}
