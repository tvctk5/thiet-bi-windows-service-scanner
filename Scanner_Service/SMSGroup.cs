using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scanner_Service
{
    public class SMSGroup
    {
        public static int GROUP_WARNING = 1;
        public static int GROUP_CONNECTION_ISSUE = 2;
    }

    public class SMSType
    {
        public static int CONNECTION_ISSUE_FAILED = 200;
        public static int GROUP_CONNECTION_RESOLVED = 201;
    }
}
