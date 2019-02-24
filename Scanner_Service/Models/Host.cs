using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scanner_Service.Models
{
    public class Host
    {
        public string id { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string url { get; set; }
        public bool status { get; set; }
        public bool connection_status { get; set; }
        public bool allow_send_sms { get; set; }
        public string versionId { get; set; }
        public string version { get; set; }
        public string last_upgrade { get; set; }
        public bool auto_upgrade { get; set; }

        public UpgradeVersion upgradeVersion { get; set; }
    }
}
