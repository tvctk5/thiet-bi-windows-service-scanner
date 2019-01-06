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
        MySql.Data.MySqlClient.MySqlConnection connection;
        public MySqlHelper(string server, string user, string pass, string dbName, string ssl)
        {
            string myConnectionString;

            myConnectionString = "server={0};uid={1};pwd={2};database={3}";
            if (ssl != "1")
            {
                myConnectionString += ";SslMode=none";
            }

            myConnectionString = string.Format(myConnectionString, server, user, pass, dbName);

            connection = new MySql.Data.MySqlClient.MySqlConnection();
            connection.ConnectionString = myConnectionString;
            // ServiceLog.WriteErrorLog(myConnectionString);
        }

        #region Open - close connection

        /// <summary>
        /// Open conn
        /// </summary>
        /// <returns></returns>
        public bool OpenConnection()
        {
            try
            {
                if (connection.State == ConnectionState.Open)
                {
                    return true;
                }

                connection.Open();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                ServiceLog.WriteErrorLog(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Open conn
        /// </summary>
        /// <returns></returns>
        public bool CloseConnection()
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

            //open connection
            if (this.OpenConnection() == true)
            {
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //Execute command
                cmd.ExecuteNonQuery();

                //close connection
                this.CloseConnection();
            }
        }

        //Update statement
        public void Update(string query)
        {
            //string query = "UPDATE tableinfo SET name='Joe', age='22' WHERE name='John Smith'";

            //Open connection
            if (this.OpenConnection() == true)
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
                this.CloseConnection();
            }
        }

        //Delete statement
        public void Delete(string query)
        {
            // string query = "DELETE FROM tableinfo WHERE name='John Smith'";

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        public List<Message> Select_SMSPending()
        {
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
                if (this.OpenConnection() == true)
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
                    this.CloseConnection();

                    //return list to be displayed
                    return list;
                }
                else
                {
                    return list;
                }

            }
            catch (Exception ex)
            {
                ServiceLog.WriteErrorLog(ex);
                //close Connection
                this.CloseConnection();
                return new List<Message>();
            }
        }

        public List<Host> Select_HostToScan()
        {
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
                if (this.OpenConnection() == true)
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
                    this.CloseConnection();

                    //return list to be displayed
                    return list;
                }
                else
                {
                    return list;
                }
            }
            catch (Exception ex)
            {
                ServiceLog.WriteErrorLog(ex);
                //close Connection
                this.CloseConnection();
                return new List<Host>();
            }
        }

        public List<Sms> Select_CheckSMSToInsert(string hostId)
        {
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
                if (this.OpenConnection() == true)
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
                            type = int.Parse((dataReader["type"] +"") ==""?"0" : (dataReader["type"] + "")),
                            sent = bool.Parse(dataReader["sent"] + ""),
                            sms_groupId = int.Parse(dataReader["sms_groupId"] + "")
                        });
                    }

                    //close Data Reader
                    dataReader.Close();

                    //close Connection
                    this.CloseConnection();

                    //return list to be displayed
                    return list;
                }
                else
                {
                    return list;
                }
            }
            catch (Exception ex)
            {
                ServiceLog.WriteErrorLog(ex);
                //close Connection
                this.CloseConnection();
                return new List<Sms>();
            }
        }

        //Select statement
        public List<string>[] Select(string query)
        {
            // string query = "SELECT * FROM tableinfo";

            //Create a list to store the result
            List<string>[] list = new List<string>[3];
            list[0] = new List<string>();
            list[1] = new List<string>();
            list[2] = new List<string>();

            //Open connection
            if (this.OpenConnection() == true)
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
                this.CloseConnection();

                //return list to be displayed
                return list;
            }
            else
            {
                return list;
            }
        }

        //Count statement
        public int Count(string query)
        {
            // string query = "SELECT Count(*) FROM tableinfo";
            int Count = -1;

            //Open Connection
            if (this.OpenConnection() == true)
            {
                //Create Mysql Command
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //ExecuteScalar will return one value
                Count = int.Parse(cmd.ExecuteScalar() + "");

                //close Connection
                this.CloseConnection();

                return Count;
            }
            else
            {
                return Count;
            }
        }


        //Update statement
        public void UpdateSmsStatusToSent(string id)
        {
            string query = "UPDATE sms SET sent=1 WHERE id=" + id;
            try
            {
                //Open connection
                if (this.OpenConnection() == true)
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
                    this.CloseConnection();
                }
            }
            catch (Exception ex)
            {
                ServiceLog.WriteErrorLog(ex);
                //close Connection
                this.CloseConnection();
            }

        }

        public void UpdateHostForConnectionIssue(string hostId, int status = 0)
        {
            string query = "UPDATE host SET connection_status=" + status + " WHERE id=" + hostId;
            try
            {
                //Open connection
                if (this.OpenConnection() == true)
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
                    this.CloseConnection();
                }
            }
            catch (Exception ex)
            {
                ServiceLog.WriteErrorLog(ex);
                //close Connection
                this.CloseConnection();
            }

        }


        public void CreateSmsRecordToSent(string hostId, int type)
        {
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
                if (this.OpenConnection() == true)
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
                    this.CloseConnection();
                }
            }
            catch (Exception ex)
            {
                ServiceLog.WriteErrorLog(ex);
                //close Connection
                this.CloseConnection();
            }

        }
    }
}
