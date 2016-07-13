using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using hpMvc.Infrastructure.Logging;
using hpMvc.Models;

namespace hpMvc.DataBase
{
    public static class FamiliesBusiness
    {
        public static NLogger Nlogger;

        static FamiliesBusiness()
        {
            Nlogger = new NLogger();
        }

        public static bool AddFamiliesContact(FamilyContactsModel fcm)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "AddFamiliesContact"
                    };

                    var param = new SqlParameter("@firstName", fcm.FirstName);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@lastName", fcm.LastName);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@email", fcm.Email);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@comment", fcm.Comment);
                    cmd.Parameters.Add(param);



                    
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return false;
                }
            }
            return true;
        }
        public static bool ProcessEmails(FamilyContactsModel fcm, string url, HttpServerUtilityBase server)
        {
            //to halfpint
            var toEmail = new[] {"CCC@Halfpintstudy.org"};
            //var toEmail = new[] {"j.rezuke@verizon.net" };
            var ccEmail = new[] {"j.rezuke@verizon.net"};
            var sbBody = new StringBuilder("");
            sbBody.Append("Comments received from:");
            sbBody.Append("<br/>");
            sbBody.Append("Email: " +fcm.Email);
            sbBody.Append("<br/>");
            sbBody.Append("Name: " + fcm.FirstName + " " + fcm.LastName);
            sbBody.Append("<br/>");
            sbBody.Append("<br/>");
            sbBody.Append("Comments:");
            sbBody.Append("<br/>");
            sbBody.Append(fcm.Comment);
            string subject = "Families contact";
            
            Utility.SendFamilyContactMail(toEmail, ccEmail, null, server, sbBody.ToString(), subject);   
            
            //confirm to the user
            toEmail = new[] { fcm.Email };
            ccEmail = new[] { "j.rezuke@verizon.net" };
            sbBody = new StringBuilder("");
            sbBody.Append("Dear " + fcm.FirstName + ",");
            sbBody.Append("<br/><br/>");
            sbBody.Append("Thank you for sending us your comments! We greatly appreciate your interest in improving the HALF-PINT trial. We will respond to all of your questions as soon as possible." );
            sbBody.Append("<br/><br/>");
            sbBody.Append("Sincerely,");
            sbBody.Append("<br/>");
            sbBody.Append("<br/>");
            sbBody.Append("The HALF-PINT Team");
            sbBody.Append("<br/><br/>");
            sbBody.Append("Your comments:");
            sbBody.Append("<br/>");
            sbBody.Append(fcm.Comment);
            subject = "HALF-PINT email confirmation";
            Utility.SendFamilyContactMail(toEmail, ccEmail, url, server, sbBody.ToString(), subject);   
            
            return true;
        }
    }
}