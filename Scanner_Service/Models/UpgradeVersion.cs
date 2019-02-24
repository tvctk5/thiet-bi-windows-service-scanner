using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scanner_Service.Models
{
    public class UpgradeVersion
    {
        public string id { get; set; }
        public string version { get; set; }
        public string uri_file { get; set; }
        public bool active { get; set; }
        public string createdate { get; set; }
    }
}
