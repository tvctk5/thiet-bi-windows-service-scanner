using System;
using System.Collections.Generic;
using System.Text;

namespace Scanner_Service.Models
{
    public class MySqlConfig
    {
        public string Server { get; set; }
        public string DBName { get; set; }
        public string User { get; set; }
        public string Pass { get; set; }
        public string SSL { get; set; }
    }
}
