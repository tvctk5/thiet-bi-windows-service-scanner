using System;
using System.Collections.Generic;
using System.IO;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Xml;
using Scanner_Service.Models;

namespace Scanner_Service
{
    public class FileHelper
    {
        public static Config ReadConfigFile()
        {
            //Tạo đối tượng lưu trữ thông tin COnfig
            Config objConfig = new Config();
            try
            {
                //Tạo đối tượng kết nối tới XML
                string sPathXML = AppDomain.CurrentDomain.BaseDirectory + "\\" + "config.xml";
                if (!CheckFileStatic(sPathXML))
                {
                    objConfig.Exception = new Exception("Không tồn tại tệp tin XML lưu cấu hình kết nối DataBase");
                    return objConfig;
                }

                XmlDocument xmlData = new XmlDocument();
                xmlData.Load(sPathXML);
                //Lấy thông tin kết nối tới SQL Server
                XmlNodeList nodeList_CONFIG = xmlData.GetElementsByTagName("CONFIG");
                string strInfo = null;

                objConfig = new Config();
                foreach (XmlNode NodeItem_INFO in nodeList_CONFIG.Item(0))
                {
                    strInfo = NodeItem_INFO.InnerText;
                    switch (NodeItem_INFO.Name.ToUpper())
                    {
                        case "INTERVAL": objConfig.Interval = int.Parse(strInfo); break;
                        case "MYSQL_SERVER": objConfig.MySql.Server = strInfo; break;
                        case "MYSQL_DBNAME": objConfig.MySql.DBName = strInfo; break;
                        case "MYSQL_USER": objConfig.MySql.User = strInfo; break;
                        case "MYSQL_PASS": objConfig.MySql.Pass = strInfo; break;
                        case "MYSQL_SSL": objConfig.MySql.SSL = strInfo; break;
                        case "SMS_USER": objConfig.SmsApi.User = strInfo; break;
                        case "SMS_PASS": objConfig.SmsApi.Pass = strInfo; break;
                        case "UPGRADE_JOB_ENABLE":
                            objConfig.EnableUpgradeJob = false;
                            if (strInfo == "1")
                            {
                                objConfig.EnableUpgradeJob = true;
                            }

                            break;
                        case "UPGRADE_JOB_LOCAL_PATH":
                            objConfig.LocalPathUpgradeJob = strInfo;
                            break;
                    }
                }

                return objConfig;
            }
            catch (Exception ex)
            {
                objConfig.Exception = ex;
                return objConfig;
            }
        }

        private static bool CheckFileStatic(string PathFile)
        {
            try
            {
                if (File.Exists(PathFile))
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
