using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Security;
using hpMvc.Infrastructure.Logging;
using hpMvc.Models;

namespace hpMvc.DataBase
{
    public static class DbInform
    {
        public static NLogger nlogger;
        static DbInform()
        {
            nlogger = new NLogger();
        }

        public static int SaveInformPage(InformPageModel ifp)
        {
             String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

             using (SqlConnection conn = new SqlConnection(strConn))
             {
                 try
                 {
                     SqlCommand cmd = new SqlCommand("", conn);
                     cmd.CommandType = System.Data.CommandType.StoredProcedure;
                     cmd.CommandText = ("SaveInformPage");
                     SqlParameter param = new SqlParameter("@headerContent", ifp.HeaderContent);
                     cmd.Parameters.Add(param);
                     param = new SqlParameter("@mainContent", ifp.MainContent);
                     cmd.Parameters.Add(param);
                     param = new SqlParameter("@footerContent", ifp.FooterContent);
                     cmd.Parameters.Add(param);
                     conn.Open();
                     cmd.ExecuteNonQuery();
                 }
                 catch (Exception ex)
                 {
                     nlogger.LogError(ex);
                     return 0;
                 }
             }
            return 1;
        }

        public static int SaveMeetingsPage(InformPageModel ifp)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("SaveMeetingsPage");
                    SqlParameter param = new SqlParameter("@headerContent", ifp.HeaderContent);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@mainContent", ifp.MainContent);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@footerContent", ifp.FooterContent);
                    cmd.Parameters.Add(param);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return 0;
                }
            }
            return 1;
        }

        public static InformPageModel GetInformPage()
        {
            var ifp = new InformPageModel();
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("GetInformPage");
                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;

                    while (rdr.Read())
                    {                        
                        pos = rdr.GetOrdinal("HeaderContent");
                        ifp.HeaderContent = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("MainContent");
                        ifp.MainContent = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("FooterContent");
                        ifp.FooterContent = rdr.GetString(pos);
                    }
                    rdr.Close();

                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }
            return ifp;
        }

        public static InformPageModel GetMeetingsPage()
        {
            var ifp = new InformPageModel();
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("GetMeetingsPage");
                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;

                    while (rdr.Read())
                    {
                        pos = rdr.GetOrdinal("HeaderContent");
                        ifp.HeaderContent = "";
                        if(! rdr.IsDBNull(pos))
                            ifp.HeaderContent = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("MainContent");
                        ifp.MainContent = "";
                        if (!rdr.IsDBNull(pos))
                            ifp.MainContent = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("FooterContent");
                        ifp.FooterContent = "";
                        if (!rdr.IsDBNull(pos))
                            ifp.FooterContent = rdr.GetString(pos);
                    }
                    rdr.Close();

                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }
            return ifp;
        }

        public static DTO ValidateInput(InformPageModel ifp)
        {
            string message = "";
            DTO dto = new DTO();
            dto.ReturnValue = 1;
            string headerContent = ifp.HeaderContent.ToLower();
            if (!IsValidContent(headerContent, ref message))
            {
                dto.Message = "Header Content: " + message;
                dto.ReturnValue = 0;
                return dto;
            }

            string mainContent = ifp.MainContent.ToLower();
            if (! IsValidContent(mainContent, ref message))
            {
                dto.Message = "Main Content: " + message; ;
                dto.ReturnValue = 0;
                return dto;
            }

            string footerContent = ifp.FooterContent.ToLower();
            if (! IsValidContent(footerContent, ref message))
            {
                dto.Message = "Footer Content: " + message; ;
                dto.ReturnValue = 0;
                return dto;
            }
            return dto;
        }

        private static bool IsValidContent(string content, ref string message)
        {            
            string[] a1 = content.Split('<');

            int pos = 0;
            string sPart = "";
            foreach (string s in a1)
            {
                if (s.Length == 0)                
                    continue;
                
                if (s.StartsWith("/"))
                    continue;

                pos = s.IndexOf('>');
                sPart = s.Substring(0, pos).Trim().ToLower();
                if (sPart.Contains("script"))
                {
                    message = "No scritps allowed!";
                    return false;
                }

                if (s.StartsWith("a"))
                    continue;

                if (!IsValidTag(sPart, ref message))
                    return false;
                

            }
            return true;
        }

        private static bool IsValidAnchor(string sAnchor, ref string message)
        {
            //int pos = 0;
            if (sAnchor.Contains("mailto:"))
            {
                if (!sAnchor.Contains("dcc@halfpintstudy.org"))
                {
                    message = "Mail is allowed to be sent to 'dcc@halfpintstudy.org' only";
                    return false;
                }
            }
            else
            {
                message = "The anchor tag is not allowed";
                return false;
            }

            return true;
        }

        private static bool IsValidTag(string tag, ref string message)
        {
            string[] validTags = new[] {"p","ul","li","strong","h1","h2","h3" }; 
            foreach (string s in validTags)
            {
                if (tag == s)
                {                    
                    return true;
                }
            }
            message = "The tag '" + tag + "' is not allowed";
            return false;
        }
    }
}