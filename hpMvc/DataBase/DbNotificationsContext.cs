using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using hpMvc.Infrastructure.Logging;
using hpMvc.Models;

namespace hpMvc.DataBase
{
    public static class DbNotificationsUtils
    {
        public static NLogger Nlogger;

        static DbNotificationsUtils()
        {
            Nlogger = new NLogger();
        }

        public static List<NotificationEvent> GetNotificationEvents()
        {
            var events = new List<NotificationEvent>();
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("Opps");
                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = CommandType.StoredProcedure,
                                      CommandText = "GetNotificationEvents"
                                  };

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var eVent = new NotificationEvent();
                        var pos = rdr.GetOrdinal("Id");
                        eVent.Id = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("Name");
                        eVent.Name = rdr.GetString(pos);
                        pos = rdr.GetOrdinal("Active");
                        eVent.Active = rdr.GetBoolean(pos);
                        events.Add(eVent);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                }
            }
            return events;
        }

        public static NotificationEvent GetNotificationEvent(string id)
        {
            var eVent = new NotificationEvent();
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("Opps");
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "GetNotificationEvent"
                    };
                    var parameter = new SqlParameter("@id", id);
                    cmd.Parameters.Add(parameter);

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("Id");
                        eVent.Id = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("Name");
                        eVent.Name = rdr.GetString(pos);
                        pos = rdr.GetOrdinal("Active");
                        eVent.Active = rdr.GetBoolean(pos);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                }
            }
            return eVent;
        }
        
        public static int AddNotificationEvent(NotificationEvent eEvent)
        {
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("Opps");
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "AddNotificationEvent"
                    };

                    var parameter = new SqlParameter("@name", eEvent.Name);
                    cmd.Parameters.Add(parameter);
                    
                    parameter = new SqlParameter("@Identity", SqlDbType.Int, 0, "ID")
                                    {
                                        Direction =
                                            ParameterDirection
                                                  .Output
                                    };
                    cmd.Parameters.Add(parameter);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    return (int)cmd.Parameters["@Identity"].Value;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return -1;
                }
            }
        }

        public static int UpdateNotificationEvent(NotificationEvent eEvent)
        {
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("Opps");
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = System.Data.CommandType.StoredProcedure,
                        CommandText = "UpdateNotificationEvent"
                    };

                    var parameter = new SqlParameter("@name", eEvent.Name);
                    cmd.Parameters.Add(parameter);

                    parameter = new SqlParameter("@active", eEvent.Active);
                    cmd.Parameters.Add(parameter);

                    parameter = new SqlParameter("@id", eEvent.Id);
                    cmd.Parameters.Add(parameter);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    return 1;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return -1;
                }
            }
        }
    }
}