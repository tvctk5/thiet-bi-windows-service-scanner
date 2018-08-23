using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using Scanner_Service.Models;
using System.Linq;

namespace Scanner_Service
{
    public partial class Scanner_Service : ServiceBase
    {
        private Timer timeDelay = null;
        private Config config = null;
        int count;
        MySqlHelper mySql = null;

        public Scanner_Service()
        {
            InitializeComponent();

            // timeDelay = new System.Timers.Timer();
            // timeDelay.Elapsed += new System.Timers.ElapsedEventHandler(WorkProcess);

            this.ServiceName = "Scanner Service";
            this.EventLog.Log = "Application";

            // These Flags set whether or not to handle that specific
            //  type of event. Set to true if you need it, false otherwise.
            this.CanHandlePowerEvent = true;
            this.CanHandleSessionChangeEvent = true;
            this.CanPauseAndContinue = true;
            this.CanShutdown = true;
            this.CanStop = true;
        }

        public void WorkProcess(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Get data and send message
            // Try to send a message
            // get all people to send sms
            var lstHost = mySql.Select_HostToScan();

            if (lstHost != null && lstHost.Count > 0)
            {
                foreach (var itemHost in lstHost)
                {
                    HostScan(itemHost);
                }
            }
        }

        private void HostScan(Host input)
        {
            try
            {
                if (string.IsNullOrEmpty(input.url))
                {
                    ServiceLog.WriteErrorLog("Url is null or empty (Host id: " + input.id + "; Host name: "+ input.name + ")");
                    return;
                }

                string url = (input.url.Trim('/')) + "/scanner.php?hostid=" + input.id;

                using (var wb = new System.Net.WebClient())
                {
                    wb.DownloadString(url);
                }
            }
            catch (Exception ex)
            {
                ServiceLog.WriteErrorLog(ex);
            }
        }
        protected override void OnStart(string[] args)
        {
            // Load config
            config = FileHelper.ReadConfigFile();
            if(config.Exception != null)
            {
                ServiceLog.WriteErrorLog(config.Exception);
                // Stop service
                OnStop();
            }

            // Try to connect to DB
            mySql = new MySqlHelper(config.MySql.Server, config.MySql.User, config.MySql.Pass, config.MySql.DBName, config.MySql.SSL);
            
            timeDelay = new Timer();
            this.timeDelay.Interval = config.Interval;
            this.timeDelay.Elapsed += new ElapsedEventHandler(WorkProcess);
            timeDelay.Enabled = true;
            ServiceLog.WriteErrorLog("Scanner service started");
        }
        protected override void OnStop()
        {
            LogService("Scanner service Stoped");
            timeDelay.Enabled = false;
        }
        private void LogService(string content)
        {
            ServiceLog.WriteErrorLog(content);
        }
        public string convertToUnSign(string s)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }
    }
}
