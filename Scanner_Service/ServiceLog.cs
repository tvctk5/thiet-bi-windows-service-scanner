using System;
using System.Collections.Generic;
using System.IO;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace Scanner_Service
{
    public class ServiceLog
    {
        public static void WriteErrorLog(string message)
        {
            StreamWriter sw = null;
            try
            {
                string fileName = CheckLogFile();
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\" + fileName + ".txt", true);
                sw.WriteLine(DateTime.Now.ToString() + ": " + message);
                sw.Flush();
                sw.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static void WriteErrorLog(Exception ex)
        {
            StreamWriter sw = null;
            try
            {
                string fileName = CheckLogFile();
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\" + fileName + ".txt", true);
                sw.WriteLine(DateTime.Now.ToString() + ": " + ex.Source.ToString().Trim() + "; " + ex.Message.ToString().Trim());

                sw.WriteLine("---- Detail: " + ex.ToString());
                sw.Flush();
                sw.Close();
            }
            catch (Exception exInMethod)
            {
                Console.WriteLine(exInMethod.ToString());
            }
        }

        private static string CheckLogFile(string FileName = "LogFile")
        {
            try
            {
                DirectoryInfo dic = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\");
                if (!dic.Exists)
                {
                    dic.Create();
                }

                FileInfo jobFile = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\" + FileName + ".txt");
                if (!jobFile.Exists)
                {
                    jobFile.Create().Dispose();
                    return FileName;
                }

                // >10 MB --> Create new log
                if (jobFile.Length > 10485760)
                {
                    return CheckLogFile("LogFile-" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss-fff"));
                }

                return FileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "";
            }
        }
    }
}