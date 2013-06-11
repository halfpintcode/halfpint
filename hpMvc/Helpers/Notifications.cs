using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace hpMvc.Helpers
{
    public class NotificationUtils
    {
        public static List<string> GetStaffForEvent(int eventId, int siteId)
        {
            var emails = new List<string>();

            var connStr = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(connStr))
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
                SqlDataReader rdr = cmd.ExecuteReader();
                int pos = 0;

                while (rdr.Read())
                {
                    pos = rdr.GetOrdinal("AllSites");
                    var isAllSites = rdr.GetBoolean(pos);

                    pos = rdr.GetOrdinal("Email");
                    if(rdr.IsDBNull(pos))
                        continue;
                    var email = rdr.GetString(pos);

                    if (isAllSites)
                    {
                        emails.Add(email);
                        continue;
                    }

                    pos = rdr.GetOrdinal("SiteID");
                    var site = rdr.GetInt32(pos);

                    if(site == siteId)
                        emails.Add(email);

                }
                rdr.Close();
            }

            return emails;
        }
    }
}