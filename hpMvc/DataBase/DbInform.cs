﻿using System;
using System.Configuration;
using System.Data.SqlClient;
using HtmlAgilityPack;
using System.Linq;
using hpMvc.Infrastructure.Logging;
using hpMvc.Models;
using hpMvc.Services.HtmlSanitizer;

namespace hpMvc.DataBase
{
    public static class DbInform
    {
        public static NLogger Nlogger;
        static DbInform()
        {
            Nlogger = new NLogger();
        }

        public static int SaveStaffEnrollmentPage(EnrollmentContentModel ecm)
        {
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = System.Data.CommandType.StoredProcedure,
                        CommandText = ("SaveStaffEnrollmentPage")
                    };
                    var param = new SqlParameter("@enrollmentContent", ecm.EnrollmentContent);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@announcementContent", ecm.AnnouncementContent);
                    cmd.Parameters.Add(param);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return 0;
                }
            }
            return 1;
        }

        public static int SaveInformPage(InformPageModel ifp)
        {
             var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

             using (var conn = new SqlConnection(strConn))
             {
                 try
                 {
                     var cmd = new SqlCommand("", conn)
                               {
                                   CommandType = System.Data.CommandType.StoredProcedure,
                                   CommandText = ("SaveInformPage")
                               };
                     var param = new SqlParameter("@headerContent", ifp.HeaderContent);
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
                     Nlogger.LogError(ex);
                     return 0;
                 }
             }
            return 1;
        }

        public static int SaveMeetingsPage(InformPageModel ifp)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = ("SaveMeetingsPage")
                              };
                    var param = new SqlParameter("@headerContent", ifp.HeaderContent);
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
                    Nlogger.LogError(ex);
                    return 0;
                }
            }
            return 1;
        }

        public static EnrollmentContentModel GetEnrollmentContent()
        {
            var ecm = new EnrollmentContentModel();
            
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            SqlDataReader rdr = null;
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = ("GetStaffEnrollmentPage")
                              };
                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {                        
                        int pos = rdr.GetOrdinal("Enrollment");
                        ecm.EnrollmentContent = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("Announcement");
                        ecm.AnnouncementContent = rdr.GetString(pos);
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
            return ecm;
        }

        public static InformPageModel GetInformPage()
        {
            var ifp = new InformPageModel();
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            SqlDataReader rdr = null;
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = ("GetInformPage")
                              };
                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {                        
                        int pos = rdr.GetOrdinal("HeaderContent");
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
                    Nlogger.LogError(ex);
                }
                finally
                {
                    if (rdr != null)
                        rdr.Close();
                }
            }   
            return ifp;
        }

        public static InformPageModel GetMeetingsPage()
        {
            var ifp = new InformPageModel();
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            SqlDataReader rdr = null;
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = ("GetMeetingsPage")
                              };
                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        int pos = rdr.GetOrdinal("HeaderContent");
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
                    Nlogger.LogError(ex);
                }
                finally
                {
                    if (rdr != null)
                        rdr.Close();
                }
            }
            return ifp;
        }

        public static DTO ValidateInput(EnrollmentContentModel ecm)
        {
            string message = "";
            var dto = new DTO {ReturnValue = 1};
            string enrollmentContent = ecm.EnrollmentContent.ToLower();

            HtmlSanitizer sanitizer = new HtmlSanitizer();
            if (!sanitizer.Sanitize(enrollmentContent, out message))
            {
                dto.Message = "Enrollment Content: " + message;
                dto.ReturnValue = 0;
                return dto;
            }

            //if (!IsValidContent(enrollmentContent, ref message))
            //{
            //    dto.Message = "Enrollment Content: " + message;
            //    dto.ReturnValue = 0;
            //    return dto;
            //}

            string announcementContent = ecm.AnnouncementContent.ToLower();
            if (!sanitizer.Sanitize(announcementContent, out message))
            {
                dto.Message = "Announcement Content: " + message;
                dto.ReturnValue = 0;
                return dto;
            }
            //if (!IsValidContent(announcementContent, ref message))
            //{
            //    dto.Message = "Announcement Content: " + message; 
            //    dto.ReturnValue = 0;
            //    return dto;
            //}
            
            return dto;
        }

        public static DTO ValidateInput(InformPageModel ifp)
        {
            string message = "";
            var dto = new DTO { ReturnValue = 1,IsSuccessful = true};
            string headerContent = ifp.HeaderContent.ToLower();
            if (!IsValidContent(headerContent, ref message))
            {
                dto.Message = "Header Content: " + message;
                dto.IsSuccessful = false;
                dto.ReturnValue = 0;
                return dto;
            }

            string mainContent = ifp.MainContent.ToLower();
            if (!IsValidContent(mainContent, ref message))
            {
                dto.Message = "Main Content: " + message;
                dto.IsSuccessful = false;
                dto.ReturnValue = 0;
                return dto;
            }

            string footerContent = ifp.FooterContent.ToLower();
            if (!IsValidContent(footerContent, ref message))
            {
                dto.Message = "Footer Content: " + message;
                dto.IsSuccessful = false;
                dto.ReturnValue = 0;
                return dto;
            }
            return dto;
        }

        private static bool IsValidContent(string content, ref string message)
        {            
            string[] a1 = content.Split('<');

            foreach (string s in a1)
            {
                if (s.Length == 0)                
                    continue;
                
                if (s.StartsWith("/"))
                    continue;

                if (s.ToLower().StartsWith("img"))
                {
                    if (!IsValidImageSource(s))
                    {
                        message = "invalid source value for img tag";
                    }
                }

                int pos = s.IndexOf('>');
                string sPart = s.Substring(0, pos).Trim().ToLower().Trim();
                
                if (sPart.Contains("script"))
                {
                    message = "No scritps allowed!";
                    return false;
                }

                if (s.StartsWith("a"))
                    continue;
                
                if (!IsValidTagAttribute(sPart, ref message))
                    return false;
                

            }
            return true;
        }

        private static bool IsValidImageSource(string img)
        {
            string[] a1 = img.Split(' ');
            foreach (string s in a1)
            {
                if (s.ToLower().StartsWith("src"))
                {
                    var pos = s.IndexOf("=", StringComparison.InvariantCulture);
                    string src = s.Substring(pos+1);
                    src = src.Replace("'", "");
                    if (src.ToLower().StartsWith("content"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        //private static bool IsValidAnchor(string sAnchor, ref string message)
        //{
        //    //int pos = 0;
        //    if (sAnchor.Contains("mailto:"))
        //    {
        //        if (!sAnchor.Contains("dcc@halfpintstudy.org"))
        //        {
        //            message = "Mail is allowed to be sent to 'dcc@halfpintstudy.org' only";
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        message = "The anchor tag is not allowed";
        //        return false;
        //    }

        //    return true;
        //}

        private static bool IsValidTagAttribute(string sPart, ref string message)
        {

            var pos = sPart.IndexOf(" ", StringComparison.InvariantCulture);
            var tag = "";
            if (pos == -1)
                tag = sPart;
            else
            {
                tag = sPart.Substring(0, pos);
            }
                

            if (!IsValidTag(tag, ref message))
            {
                return false;
            }
            
            var attrsPart = sPart.Substring(pos+1);
            //var attrs = attrsPart.Split('=');
            var attrCount = GetAttrCount(attrsPart);

            for (var i = 0; i < attrCount-1; i++)
            {

                pos = attrsPart.IndexOf("=", StringComparison.InvariantCulture);
                var attr = attrsPart.Substring(0, pos);

                if (!IsValidAttribute(attr))
                {
                    message = "The attribute, " + attr + " is not valid";
                    return false;
                }

                attrsPart = GetNextAttrVal(attrsPart, pos);
            }
            return true;
        }

        private static string GetNextAttrVal(string attrsPart, int pos)
        {
            var pos2 = attrsPart.IndexOf("=", pos + 1, StringComparison.InvariantCulture);
            return GetAttrStartPos(attrsPart, pos2);
        }

        private static string GetAttrStartPos(string attrsPart, int pos2)
        {
            for (var i = pos2; i < attrsPart.Length; i--)
            {
                if (attrsPart[i] == ' ')
                {
                    return attrsPart.Substring(i+1);
                }
            }
            return "";
        }

        private static int GetAttrCount(string attrsPart)
        {
            return attrsPart.Count(c => c == '=');
        }

        private static bool IsValidAttribute(string attrPart)
        {
            string[] validTags = { "title", "style", "src", "width", "height" };
            if (validTags.Any(s => attrPart == s))
            {
                return true;
            }
            
            return false;
        }

        private static bool IsValidTag(string tag, ref string message)
        {
            string[] validTags = {"img","div","p","ul","li","strong","h1","h2","h3","h4" };

            if (validTags.Any(s => tag == s))
            {
                return true;
            }
            message = "The tag '" + tag + "' is not allowed";
            return false;
        }
    }
}