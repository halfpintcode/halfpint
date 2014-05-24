using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using hpMvc.Infrastructure.Logging;

namespace hpMvc.Helpers
{
    public class NotificationUtils
    {
        public static NLogger Nlogger;
        public static List<string> GetStaffForEvent(int eventId, int siteId)
        {
            var emails = new List<string>();
            SqlDataReader rdr = null;
            var connStr = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(connStr))
            {
                try
                {
                    var cmd = new SqlCommand
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "GetNotificationsStaffForEvent",
                                  Connection = conn
                              };
                    var param = new SqlParameter("@eventId", eventId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("AllSites");
                        var isAllSites = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("Email");
                        if (rdr.IsDBNull(pos))
                            continue;
                        var email = rdr.GetString(pos);

                        if (isAllSites)
                        {
                            emails.Add(email);
                            continue;
                        }

                        pos = rdr.GetOrdinal("SiteID");
                        var site = rdr.GetInt32(pos);

                        if (site == siteId)
                            emails.Add(email);

                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                }
                finally
                {
                    if (rdr != null)
                        rdr.Close();
                }

            }

            return emails;
        }
    }
}