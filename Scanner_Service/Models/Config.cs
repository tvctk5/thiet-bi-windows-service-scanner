using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace Scanner_Service.Models
{
    public class Config
    {
        public int Interval { get; set; }
        public MySqlConfig MySql { get; set; }
        public SmsService SmsApi { get; set; }
        public Exception Exception { get; set; }

        public Config()
        {
            MySql = new MySqlConfig();
            SmsApi = new SmsService();
        }
    }
}
