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
        public bool EnableUpgradeJob { get; set; }
        public string LocalPathUpgradeJob { get; set; }

        public Config()
        {
            MySql = new MySqlConfig();
            SmsApi = new SmsService();
        }
    }

    public static class DBConfig
    {
        public const string SERVER_URL = "SERVER_URL";
        public const string FOLDER_UPLOAD = "FOLDER_UPLOAD";
    }
}
