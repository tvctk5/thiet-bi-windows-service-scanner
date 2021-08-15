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
using System.Collections;

namespace Scanner_Service
{
    public partial class Scanner_Service : ServiceBase
    {
        private Timer timeDelay = null;
        private Config config = null;
        int count;
        MySqlHelper mySql = null;
        Hashtable hasScanner;
        Hashtable hasUpgrade;

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
            // ServiceLog.WriteErrorLog("WorkProcess runing");
            // Get data and send message
            // Try to send a message
            // get all people to send sms
            var lstHost = mySql.Select_HostToScan();

            if (lstHost != null && lstHost.Count > 0)
            {
                foreach (var itemHost in lstHost)
                {
                    if (hasScanner.ContainsKey(itemHost.id) && (bool)hasScanner[itemHost.id] == false)
                        continue;

                    // waiting to finish
                    hasScanner[itemHost.id] = false;
                    System.Threading.Thread newThread = new System.Threading.Thread(HostScan);
                    newThread.Start(itemHost);
                }
            }
        }

        public void WorkProcess_Upgrade(object sender, System.Timers.ElapsedEventArgs e)
        {
            // ServiceLog.WriteErrorLog("WorkProcess_Upgrade runing
            // get active version
            var version = mySql.Select_Active_Version();
            if (version == null || version.id == "")
                return;

            // Get list host to upgrade
            var lstHostUpgrade = mySql.Select_HostToUpgrade(version.id);
            if (lstHostUpgrade == null || lstHostUpgrade.Count <= 0)
                return;

            var dbCOnfig = mySql.GetConfig();
            var Folder = "/admin/files/";
            var HostLink = "http://localhost";
            if (dbCOnfig != null && dbCOnfig.Count > 0)
            {
                if (dbCOnfig.ContainsKey(DBConfig.FOLDER_UPLOAD))
                {
                    Folder = dbCOnfig[DBConfig.FOLDER_UPLOAD] + "";
                }

                if (dbCOnfig.ContainsKey(DBConfig.SERVER_URL))
                {
                    HostLink = dbCOnfig[DBConfig.SERVER_URL] + "";
                }
            }

            var fullFileLink = HostLink + Folder + version.uri_file;
            version.uri_file = fullFileLink;

            foreach (var itemHost in lstHostUpgrade)
            {
                if (hasUpgrade.ContainsKey(itemHost.id) && (bool)hasUpgrade[itemHost.id] == false)
                    continue;

                hasUpgrade[itemHost.id] = false;
                itemHost.upgradeVersion = version;
                System.Threading.Thread newThread = new System.Threading.Thread(HostUpgrade);
                newThread.Start(itemHost);
            }

        }

        private void HostScan(object data)
        {
            // Try to connect to DB
            // var mySql = new MySqlHelper(config.MySql.Server, config.MySql.User, config.MySql.Pass, config.MySql.DBName, config.MySql.SSL);

            Host input = (Host)data;
            string url = input.url;
            try
            {
                if (string.IsNullOrEmpty(input.url))
                {
                    ServiceLog.WriteErrorLog("Url is null or empty (Host id: " + input.id + "; Host name: " + input.name + ")");
                    return;
                }

                url = (input.url.Trim('/')) + "/scanner.php?hostid=" + input.id + "&sms=" + (input.allow_send_sms ? "1" : "0");
                // ServiceLog.WriteErrorLog("hot Id: " + input.id + "; input.connection_status: " + input.connection_status +"; status: " + input.status);
                using (var wb = new System.Net.WebClient())
                {
                    wb.DownloadString(url);
                    // Update DB
                    if (!input.connection_status)
                    {
                        mySql.UpdateHostForConnectionIssue(input.id, 1);
                    }

                    if (input.allow_send_sms)
                    {
                        mySql.CreateSmsRecordToSent(input.id, SMSType.GROUP_CONNECTION_RESOLVED);
                    }
                }

                // Finished
                hasScanner[input.id] = true;
            }
            catch (Exception ex)
            {
                // Update DB
                if (input.connection_status)
                {
                    mySql.UpdateHostForConnectionIssue(input.id, 0);
                }
                // Add record
                // ServiceLog.WriteErrorLog("input.allow_send_sms: " + input.allow_send_sms);
                if (input.allow_send_sms)
                {
                    mySql.CreateSmsRecordToSent(input.id, SMSType.CONNECTION_ISSUE_FAILED);
                }

                // Log
                ServiceLog.WriteErrorLog("Error at: " + url);
                ServiceLog.WriteErrorLog(ex);

                // Finished
                hasScanner[input.id] = true;
            }
        }


        private void HostUpgrade(object data)
        {
            // Try to connect to DB
            // var mySql = new MySqlHelper(config.MySql.Server, config.MySql.User, config.MySql.Pass, config.MySql.DBName, config.MySql.SSL);

            Host input = (Host)data;
            string url = input.url;
            try
            {
                if (string.IsNullOrEmpty(input.url))
                {
                    ServiceLog.WriteErrorLog("Url is null or empty (Host id: " + input.id + "; Host name: " + input.name + ")");
                    return;
                }

                url = (input.url.Trim('/')) + "/upgrade.php?hostid=" + input.id + "&versionid=" + input.upgradeVersion.id + "&version=" + input.upgradeVersion.version + "&localpath=" + config.LocalPathUpgradeJob + "&fileupgrade=" + input.upgradeVersion.uri_file;
                // ServiceLog.WriteErrorLog("hot Id: " + input.id + "; input.connection_status: " + input.connection_status +"; status: " + input.status);
                // ServiceLog.WriteErrorLog("[UPGRADE] URL: " + url);
                using (var wb = new System.Net.WebClient())
                {
                    var dataResp = "<div>URL: " + url + " </div>" + wb.DownloadString(url);
                    // .WriteErrorLog("[UPGRADE] Response: " + dataResp);

                    // Update DB
                    mySql.UpdateUpgradeLog(input.id, dataResp);
                }

                // Finished
                hasUpgrade[input.id] = true;
            }
            catch (Exception ex)
            {
                // Log
                ServiceLog.WriteErrorLog("[UPGRADE] Error at: " + url);
                ServiceLog.WriteErrorLog(ex);

                // Finished
                hasUpgrade[input.id] = true;
            }
        }

        protected override void OnStart(string[] args)
        {
            hasScanner = new Hashtable();
            hasUpgrade = new Hashtable();
            // Load config
            config = FileHelper.ReadConfigFile();
            if (config.Exception != null)
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
            if (config.EnableUpgradeJob)
            {
                // Config WorkProcess_Upgrade
                ServiceLog.WriteErrorLog("Add WorkProcess_Upgrade");
                this.timeDelay.Elapsed += new ElapsedEventHandler(WorkProcess_Upgrade);
            }
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
