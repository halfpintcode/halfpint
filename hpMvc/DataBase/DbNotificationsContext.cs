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
                        CommandType = CommandType.StoredProcedure,
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

        public static StaffSubscriptions GetStaffSubscriptions(string staffId)
        {
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            var staffSubs = new StaffSubscriptions();
            string firstName = "";
            string lastName = "";

            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "GetStaffInfo"
                    };

                    var param = new SqlParameter("@id", staffId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        int pos = rdr.GetOrdinal("ID");
                        staffSubs.StaffId = rdr.GetInt32(pos);
                        
                        pos = rdr.GetOrdinal("FirstName");
                        if (!rdr.IsDBNull(pos))
                            firstName = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("LastName");
                        if (!rdr.IsDBNull(pos))
                            lastName = rdr.GetString(pos);

                        staffSubs.StaffName = lastName + ", " + firstName;
                    }
                    rdr.Close();
                    conn.Close();

                    cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "GetStaffNotificationEvents"
                    };

                    param = new SqlParameter("@staffId", staffId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var eVent = new NotificationEvent();
                        var pos = rdr.GetOrdinal("NotificationEventId");
                        eVent.Id = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("NotificationEventName");
                        eVent.Name = rdr.GetString(pos);
                        eVent.IsSubscribed = true;
                        pos = rdr.GetOrdinal("AllSites");
                        eVent.IsAllSites = rdr.GetBoolean(pos);
                        staffSubs.NotificationEvents.Add(eVent);
                    }

                    rdr.Close();


                    return staffSubs;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return null;
                }
            }
        }

        public static StaffSubscriptions GetStaffSubscriptionsChange(string staffId)
        {
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            var staffSubs = new StaffSubscriptions();
            string firstName = "";
            string lastName = "";

            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "GetNotificationEventsActive"
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
                        eVent.IsSubscribed = false;
                        staffSubs.NotificationEvents.Add(eVent);
                    }
                    rdr.Close();
                    conn.Close();
                    
                    cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "GetStaffInfo"
                    };

                    var param = new SqlParameter("@id", staffId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        int pos = rdr.GetOrdinal("ID");
                        staffSubs.StaffId = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("FirstName");
                        if (!rdr.IsDBNull(pos))
                            firstName = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("LastName");
                        if (!rdr.IsDBNull(pos))
                            lastName = rdr.GetString(pos);
                        
                        staffSubs.StaffName = lastName + ", " + firstName;
                    }
                    rdr.Close();
                    conn.Close();

                    cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "GetStaffNotificationEvents"
                    };

                    param = new SqlParameter("@staffId", staffId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        int pos = rdr.GetOrdinal("NotificationEventId");
                        int Id = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("AllSites");
                        bool isAllSites = rdr.GetBoolean(pos);

                        var notEvnt = staffSubs.NotificationEvents.Find(x => x.Id == Id);
                        notEvnt.IsSubscribed = true;
                        notEvnt.IsAllSites = isAllSites;
                    }

                    rdr.Close();

                    return staffSubs;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return null;
                }
            }
        }

        public static MessageListDTO SaveStaffSubscriptions(StaffSubscriptions subs)
        {
            var dto = new MessageListDTO();

            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                conn.Open();
                using (var trn = conn.BeginTransaction())
                {
                    try
                    {
                        var cmd = new SqlCommand("", conn)
                                      {
                                          Transaction = trn,
                                          CommandType = System.Data.CommandType.StoredProcedure,
                                          CommandText = "DeleteStaffSubscriptions"
                                      };
                        var param = new SqlParameter("@staffId", subs.StaffId);
                        cmd.Parameters.Add(param);

                        cmd.ExecuteNonQuery();
                        
                        foreach (var evnt in subs.NotificationEvents)
                        {
                            if (evnt.IsSubscribed)
                            {
                                cmd = new SqlCommand("", conn);
                                cmd.Transaction = trn;
                                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                                cmd.CommandText = "AddStaffNotification";
                                param = new SqlParameter("@staffId", subs.StaffId);
                                cmd.Parameters.Add(param);
                                param = new SqlParameter("@eventId", evnt.Id);
                                cmd.Parameters.Add(param);
                                param = new SqlParameter("@allSites", evnt.IsAllSites);
                                cmd.Parameters.Add(param);
                                
                                cmd.ExecuteNonQuery();
                            }
                        }
                        trn.Commit();
                    }
                    catch (Exception ex)
                    {
                        trn.Rollback();
                        Nlogger.LogError(ex);
                        dto.IsSuccessful = false;
                        dto.Dictionary.Add("StaffSubscriptions", "There was an error in SaveStaffSubscriptions");
                        return dto;
                    }
                }
            }
            dto.Messages.Add("Staff subscriptions were successfully saved to the database!");
            dto.IsSuccessful = true;
            return dto;
        }

    }
}