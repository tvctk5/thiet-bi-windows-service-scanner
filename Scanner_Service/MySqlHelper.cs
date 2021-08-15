using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Data;
using MySql.Data.MySqlClient;
using Scanner_Service.Models;

namespace Scanner_Service
{
    public class MySqlHelper
    {
        string ConnectionString;

        public MySqlHelper(string server, string user, string pass, string dbName, string ssl)
        {
            string myConnectionString;

            myConnectionString = "server={0};uid={1};pwd={2};database={3}";
            if (ssl != "1")
            {
                myConnectionString += ";SslMode=none";
            }

            ConnectionString = string.Format(myConnectionString, server, user, pass, dbName);

            ServiceLog.WriteErrorLog(ConnectionString);
        }

        #region Open - close connection

        /// <summary>
        /// Open conn
        /// </summary>
        /// <returns></returns>
        public MySqlConnection OpenConnection()
        {
            try
            {
                var connection = new MySql.Data.MySqlClient.MySqlConnection();
                connection.ConnectionString = ConnectionString;

                connection.Open();
                return connection;
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                ServiceLog.WriteErrorLog(ex);
                return null;
            }
        }

        /// <summary>
        /// Open conn
        /// </summary>
        /// <returns></returns>
        public bool CloseConnection(MySqlConnection connection)
        {
            try
            {
                if (connection != null && connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                ServiceLog.WriteErrorLog(ex);
                return false;
            }

            return true;
        }
        #endregion

        //Insert statement
        public void Insert(string query)
        {
            // string query = "INSERT INTO tableinfo (name, age) VALUES('John Smith', '33')";
            MySqlConnection connection;
            //open connection
            connection = this.OpenConnection();
            {
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //Execute command
                cmd.ExecuteNonQuery();

                //close connection
                this.CloseConnection(connection);
            }
        }

        //Update statement
        public void Update(string query)
        {
            //string query = "UPDATE tableinfo SET name='Joe', age='22' WHERE name='John Smith'";
            MySqlConnection connection;
            //Open connection
            connection = this.OpenConnection();
            {
                //create mysql command
                MySqlCommand cmd = new MySqlCommand();
                //Assign the query using CommandText
                cmd.CommandText = query;
                //Assign the connection using Connection
                cmd.Connection = connection;

                //Execute query
                cmd.ExecuteNonQuery();

                //close connection
                this.CloseConnection(connection);
            }
        }

        //Delete statement
        public void Delete(string query)
        {
            // string query = "DELETE FROM tableinfo WHERE name='John Smith'";
            MySqlConnection connection = null;
            connection = this.OpenConnection();
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection(connection);
            }
        }

        public List<Message> Select_SMSPending()
        {
            MySqlConnection connection = null;
            try
            {
                var list = new List<Message>();

                string query = @"
                    select sms.id, sms.hostId, sms.deviceId, sms.sent, sms.device_hostId, sms_type.message, h.name as hostname, sms.createddate
                    from sms join sms_type on sms.type = sms_type.id and sms_type.allowsendsms = 1
                    join host h on h.id = sms.hostId
                    where sms.sent = 0 
                    order by sms.createddate asc
                ";

                //Create a list to store the result
                //Open connection
                connection = this.OpenConnection();
                {
                    //Create Command
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    //Create a data reader and Execute the command
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    //Read the data and store them in the list
                    while (dataReader.Read())
                    {
                        list.Add(new Message()
                        {
                            id = dataReader["id"] + "",
                            hostId = dataReader["hostId"] + "",
                            deviceId = dataReader["deviceId"] + "",
                            sent = dataReader["sent"] + "",
                            device_hostId = dataReader["device_hostId"] + "",
                            message = dataReader["message"] + "",
                            hostname = dataReader["hostname"] + "",
                            createddate = dataReader["createddate"] + ""
                        });
                    }

                    //close Data Reader
                    dataReader.Close();

                    //close Connection
                    this.CloseConnection(connection);

                    //return list to be displayed
                    return list;
                }

            }
            catch (Exception ex)
            {
                ServiceLog.WriteErrorLog(ex);
                //close Connection
                this.CloseConnection(connection);
                return new List<Message>();
            }
        }

        public List<Host> Select_HostToScan()
        {
            MySqlConnection connection = null;
            try
            {
                var list = new List<Host>();

                string query = @"
                    select *
                    from host
                    where status = 1
                    order by id
                ";

                //Create a list to store the result
                //Open connection
                connection = this.OpenConnection();
                {
                    //Create Command
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    //Create a data reader and Execute the command
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    //Read the data and store them in the list
                    while (dataReader.Read())
                    {
                        // ServiceLog.WriteErrorLog("connection_status=" + ((dataReader["connection_status"] + "")));
                        // ServiceLog.WriteErrorLog("allow_send_sms=" + ((dataReader["allow_send_sms"] + "")));
                        list.Add(new Host()
                        {
                            id = dataReader["id"] + "",
                            name = dataReader["name"] + "",
                            phone = dataReader["phone"] + "",
                            url = dataReader["url"] + "",
                            status = bool.Parse(dataReader["status"] + ""),
                            connection_status = bool.Parse(dataReader["connection_status"] + ""),
                            allow_send_sms = bool.Parse(dataReader["allow_send_sms"] + "")
                        });
                    }

                    //close Data Reader
                    dataReader.Close();

                    //close Connection
                    this.CloseConnection(connection);

                    //return list to be displayed
                    return list;
                }
            }
            catch (Exception ex)
            {
                ServiceLog.WriteErrorLog(ex);
                //close Connection
                this.CloseConnection(connection);
                return new List<Host>();
            }
        }

        public List<Sms> Select_CheckSMSToInsert(string hostId)
        {
            MySqlConnection connection = null;
            try
            {
                var list = new List<Sms>();

                string query = @"
                    select *
                    from sms
                    where hostId=" + hostId + " and sms_groupId=" + SMSGroup.GROUP_CONNECTION_ISSUE + @"
                    order by id desc
                    limit 1
                ";

                //Create a list to store the result
                //Open connection
                connection = this.OpenConnection();
                {
                    //Create Command
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    //Create a data reader and Execute the command
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    //Read the data and store them in the list
                    while (dataReader.Read())
                    {
                        list.Add(new Sms()
                        {
                            id = dataReader["id"] + "",
                            hostId = dataReader["hostId"] + "",
                            deviceId = dataReader["deviceId"] + "",
                            type = int.Parse((dataReader["type"] + "") == "" ? "0" : (dataReader["type"] + "")),
                            sent = bool.Parse(dataReader["sent"] + ""),
                            sms_groupId = int.Parse(dataReader["sms_groupId"] + "")
                        });
                    }

                    //close Data Reader
                    dataReader.Close();

                    //close Connection
                    this.CloseConnection(connection);

                    //return list to be displayed
                    return list;
                }
            }
            catch (Exception ex)
            {
                ServiceLog.WriteErrorLog(ex);
                //close Connection
                this.CloseConnection(connection);
                return new List<Sms>();
            }
        }

        //Select statement
        public List<string>[] Select(string query)
        {
            MySqlConnection connection = null;
            // string query = "SELECT * FROM tableinfo";

            //Create a list to store the result
            List<string>[] list = new List<string>[3];
            list[0] = new List<string>();
            list[1] = new List<string>();
            list[2] = new List<string>();

            //Open connection
            connection = this.OpenConnection();
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();

                //Read the data and store them in the list
                while (dataReader.Read())
                {
                    list[0].Add(dataReader["id"] + "");
                    list[1].Add(dataReader["name"] + "");
                    list[2].Add(dataReader["age"] + "");
                }

                //close Data Reader
                dataReader.Close();

                //close Connection
                this.CloseConnection(connection);

                //return list to be displayed
                return list;
            }
        }

        //Count statement
        public int Count(string query)
        {
            MySqlConnection connection = null;
            // string query = "SELECT Count(*) FROM tableinfo";
            int Count = -1;

            //Open Connection
            connection = this.OpenConnection();
            {
                //Create Mysql Command
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //ExecuteScalar will return one value
                Count = int.Parse(cmd.ExecuteScalar() + "");

                //close Connection
                this.CloseConnection(connection);

                return Count;
            }
        }


        //Update statement
        public void UpdateSmsStatusToSent(string id)
        {
            MySqlConnection connection = null;
            string query = "UPDATE sms SET sent=1 WHERE id=" + id;
            try
            {
                //Open connection
                connection = this.OpenConnection();
                {
                    //create mysql command
                    MySqlCommand cmd = new MySqlCommand();
                    //Assign the query using CommandText
                    cmd.CommandText = query;
                    //Assign the connection using Connection
                    cmd.Connection = connection;

                    //Execute query
                    cmd.ExecuteNonQuery();

                    //close connection
                    this.CloseConnection(connection);
                }
            }
            catch (Exception ex)
            {
                ServiceLog.WriteErrorLog(ex);
                //close Connection
                this.CloseConnection(connection);
            }

        }

        public void UpdateHostForConnectionIssue(string hostId, int status = 0)
        {
            MySqlConnection connection = null;
            string query = "UPDATE host SET connection_status=" + status + " WHERE id=" + hostId;
            try
            {
                //Open connection
                connection = this.OpenConnection();
                {
                    //create mysql command
                    MySqlCommand cmd = new MySqlCommand();
                    //Assign the query using CommandText
                    cmd.CommandText = query;
                    //Assign the connection using Connection
                    cmd.Connection = connection;

                    //Execute query
                    cmd.ExecuteNonQuery();

                    //close connection
                    this.CloseConnection(connection);
                }
            }
            catch (Exception ex)
            {
                ServiceLog.WriteErrorLog(ex);
                //close Connection
                this.CloseConnection(connection);
            }

        }


        public void CreateSmsRecordToSent(string hostId, int type)
        {
            MySqlConnection connection = null;
            var lstSms = Select_CheckSMSToInsert(hostId);
            if (lstSms != null && lstSms.Count > 0 && lstSms[0].type == type)
            {
                // ServiceLog.WriteErrorLog("Đã tồn tại HostId=" + hostId + "; Type=" + type);
                return;
            }

            // ServiceLog.WriteErrorLog("INSERT INTO HostId=" + hostId + "; Type=" + type);
            string query = "INSERT INTO sms (hostId, sent, type, sms_groupId) VALUES(" + hostId + ",0, " + type + "," + SMSGroup.GROUP_CONNECTION_ISSUE + ");";
            try
            {
                //Open connection
                connection = this.OpenConnection();
                {
                    //create mysql command
                    MySqlCommand cmd = new MySqlCommand();
                    //Assign the query using CommandText
                    cmd.CommandText = query;
                    //Assign the connection using Connection
                    cmd.Connection = connection;

                    //Execute query
                    cmd.ExecuteNonQuery();

                    //close connection
                    this.CloseConnection(connection);
                }
            }
            catch (Exception ex)
            {
                ServiceLog.WriteErrorLog(ex);
                //close Connection
                this.CloseConnection(connection);
            }

        }

        public UpgradeVersion Select_Active_Version()
        {
            MySqlConnection connection = null;
            try
            {
                var version = new UpgradeVersion();

                string query = @"
                    select *
                    from upgrade_version
                    where active = 1
                    order by id desc
                ";

                //Create a list to store the result
                //Open connection
                connection = this.OpenConnection();
                {
                    //Create Command
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    //Create a data reader and Execute the command
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    //Read the data and store them in the list
                    while (dataReader.Read())
                    {
                        // ServiceLog.WriteErrorLog("connection_status=" + ((dataReader["connection_status"] + "")));
                        // ServiceLog.WriteErrorLog("allow_send_sms=" + ((dataReader["allow_send_sms"] + "")));
                        version = new UpgradeVersion()
                        {
                            id = dataReader["id"] + "",
                            version = dataReader["version"] + "",
                            uri_file = dataReader["uri_file"] + "",
                            active = bool.Parse(dataReader["active"] + ""),
                            createdate = dataReader["createdate"] + ""
                        };

                        break;
                    }

                    //close Data Reader
                    dataReader.Close();

                    //close Connection
                    this.CloseConnection(connection);

                    //return list to be displayed
                    return version;
                }

                return null;
            }
            catch (Exception ex)
            {
                ServiceLog.WriteErrorLog(ex);
                //close Connection
                this.CloseConnection(connection);
                return null;
            }
        }


        public System.Collections.Hashtable GetConfig()
        {
            MySqlConnection connection = null;
            try
            {
                var data = new System.Collections.Hashtable();

                string query = @"
                    select *
                    from config
                ";

                //Create a list to store the result
                //Open connection
                connection = this.OpenConnection();
                {
                    //Create Command
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    //Create a data reader and Execute the command
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    data = new System.Collections.Hashtable();

                    //Read the data and store them in the list
                    while (dataReader.Read())
                    {
                        // ServiceLog.WriteErrorLog("connection_status=" + ((dataReader["connection_status"] + "")));
                        // ServiceLog.WriteErrorLog("allow_send_sms=" + ((dataReader["allow_send_sms"] + "")));
                        data[dataReader["code"] + ""] = dataReader["value"] + "";
                    }

                    //close Data Reader
                    dataReader.Close();

                    //close Connection
                    this.CloseConnection(connection);

                    //return list to be displayed
                    return data;
                }

                return null;
            }
            catch (Exception ex)
            {
                ServiceLog.WriteErrorLog(ex);
                //close Connection
                this.CloseConnection(connection);
                return null;
            }
        }


        public List<Host> Select_HostToUpgrade(string version)
        {
            MySqlConnection connection = null;
            if (string.IsNullOrEmpty(version))
                version = "''";

            string query = @"
                    select *
                    from host
                    where auto_upgrade=1 and (versionId is null or versionId <> " + version + @")  
                    order by id
                ";

            try
            {
                var list = new List<Host>();

                //Create a list to store the result
                //Open connection
                connection = this.OpenConnection();
                {
                    //Create Command
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    //Create a data reader and Execute the command
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    //Read the data and store them in the list
                    while (dataReader.Read())
                    {
                        // ServiceLog.WriteErrorLog("connection_status=" + ((dataReader["connection_status"] + "")));
                        // ServiceLog.WriteErrorLog("allow_send_sms=" + ((dataReader["allow_send_sms"] + "")));
                        list.Add(new Host()
                        {
                            id = dataReader["id"] + "",
                            name = dataReader["name"] + "",
                            phone = dataReader["phone"] + "",
                            url = dataReader["url"] + "",
                            status = bool.Parse(dataReader["status"] + ""),
                            connection_status = bool.Parse(dataReader["connection_status"] + ""),
                            allow_send_sms = bool.Parse(dataReader["allow_send_sms"] + ""),
                            auto_upgrade = true,
                            versionId = dataReader["versionId"] + "",
                            version = dataReader["version"] + "",
                            last_upgrade = dataReader["last_upgrade"] + ""
                        });
                    }

                    //close Data Reader
                    dataReader.Close();

                    //close Connection
                    this.CloseConnection(connection);

                    //return list to be displayed
                    return list;
                }
            }
            catch (Exception ex)
            {
                ServiceLog.WriteErrorLog(ex);
                ServiceLog.WriteErrorLog("Query: " + query);
                //close Connection
                this.CloseConnection(connection);
                return new List<Host>();
            }
        }


        public void UpdateUpgradeLog(string hostId, string log)
        {
            MySqlConnection connection = null;
            log = log.Replace("'", "\"");
            string query = "UPDATE host SET log_upgrade='" + log + "' WHERE id=" + hostId;
            try
            {
                //Open connection
                connection = this.OpenConnection();
                {
                    //create mysql command
                    MySqlCommand cmd = new MySqlCommand();
                    //Assign the query using CommandText
                    cmd.CommandText = query;
                    //Assign the connection using Connection
                    cmd.Connection = connection;

                    //Execute query
                    cmd.ExecuteNonQuery();

                    //close connection
                    this.CloseConnection(connection);
                }
            }
            catch (Exception ex)
            {
                ServiceLog.WriteErrorLog(ex);
                //close Connection
                this.CloseConnection(connection);
            }

        }



    }
}
