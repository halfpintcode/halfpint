using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Mvc;
using hpMvc.Infrastructure.Logging;
using System.Web.Security;
using hpMvc.Models;

namespace hpMvc.DataBase
{

    public static class DbUtils
    {
        public static NLogger Nlogger;

        static DbUtils()
        {
            Nlogger = new NLogger();
        }

        public static IEnumerable<string> GetUserEmails(int[] sites, string[] roles)
        {
            var emails = new List<string>();

            if (roles != null)
            {
                foreach (var role in roles)
                {
                    string[] userNames = Roles.GetUsersInRole(role);
                    foreach (var user in userNames)
                    {
                        if (sites != null)
                        {
                            int site = GetSiteidIdForUser(user);
                            if (!sites.Contains(site))
                                continue;
                        }
                        var membershipUser = Membership.GetUser(user);
                        if (membershipUser != null) emails.Add(membershipUser.Email);
                    }
                }
            }
            else
            {
                if (sites != null)
                {
                    emails.AddRange(from site in sites from member in GetUsersForSite(site) select member.Email);
                }
            }

            return emails;
        }

        public static List<WebLog> GetWebLogs(int numRows = 1000)
        {
            SqlDataReader rdr = null;
            var list = new List<WebLog>();
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = CommandType.StoredProcedure,
                                      CommandText = ("GetWebLogs")
                                  };
                    var param = new SqlParameter("@num", numRows);
                    cmd.Parameters.Add(param);
                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var log = new WebLog();
                        var pos = rdr.GetOrdinal("logDate");
                        log.LogDate = rdr.GetDateTime(pos);

                        pos = rdr.GetOrdinal("logLevel");
                        log.LogLevel = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("logMessage");
                        log.LogMessage = rdr.GetString(pos);

                        list.Add(log);
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


            return list;
        }

        public static List<WebLog> GetChecksImportLog(int numRows = 1000)
        {
            SqlDataReader rdr = null;
            var list = new List<WebLog>();
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = ("GetChecksImportLog")
                    };
                    var param = new SqlParameter("@num", numRows);
                    cmd.Parameters.Add(param);
                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var log = new WebLog();
                        var pos = rdr.GetOrdinal("logDate");
                        log.LogDate = rdr.GetDateTime(pos);

                        pos = rdr.GetOrdinal("logLevel");
                        log.LogLevel = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("logMessage");
                        log.LogMessage = rdr.GetString(pos);

                        list.Add(log);
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


            return list;
        }

        public static List<SiteRandomizationsCount> GetSiteRandomizationsCount()
        {
            var srl = new List<SiteRandomizationsCount>();
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = CommandType.StoredProcedure,
                                      CommandText = ("SiteRandomizationsCount")
                                  };

                    conn.Open();
                    var rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var siteRandomizations = new SiteRandomizationsCount();
                        
                        var pos = rdr.GetOrdinal("Randomizations");
                        siteRandomizations.Count = rdr.GetInt32(pos);
                        
                        pos = rdr.GetOrdinal("LongName");
                        siteRandomizations.LongName = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("Name");
                        siteRandomizations.Name = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("SiteID");
                        siteRandomizations.SiteId = rdr.GetString(pos);
                        
                        srl.Add(siteRandomizations);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                }
            }
            return srl;
        }
        public static List<Randomization> GetAllRandomizedStudies()
        {
            SqlDataReader rdr = null;
            var list = new List<Randomization>();
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = CommandType.StoredProcedure,
                                      CommandText = ("GetAllRandomizedStudies")
                                  };

                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var rndm = new Randomization();
                        var pos = rdr.GetOrdinal("Name");
                        rndm.SiteName = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("Number");
                        rndm.Number = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("StudyID");
                        rndm.StudyID = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("Arm");
                        rndm.Arm = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("DateRandomized");
                        rndm.DateRandomized = rdr.GetDateTime(pos);

                        list.Add(rndm);
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


            return list;
        }

        public static void SaveRandomizedSubjectActive(SubjectCompleted sc, string user)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = ("SaveRandomizedSubjectActive")
                              };
                    var param = new SqlParameter("@id", sc.ID);
                    cmd.Parameters.Add(param);
                    param = sc.DateCompleted == null ? new SqlParameter("@dateCompleted", DBNull.Value) : new SqlParameter("@dateCompleted", sc.DateCompleted);
                    cmd.Parameters.Add(param);
                    param = sc.RowsCompleted == null ? new SqlParameter("@checksRowsCompleted", DBNull.Value): new SqlParameter("@checksRowsCompleted", sc.RowsCompleted);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@cgmUpload", sc.CgmUpload);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@age2to16", sc.Age2to16);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@ageNot2to16", sc.AgeNot2to16);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@cbcl", sc.CBCL);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@pedsQL", sc.PedsQL);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@demographics", sc.Demographics);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@contactInfo", sc.ContactInfo);
                    cmd.Parameters.Add(param);
                    param = sc.NotCompletedReason == null ? new SqlParameter("@notCompletedReason", DBNull.Value) : new SqlParameter("@notCompletedReason", sc.NotCompletedReason);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@cleared", sc.Cleared);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@user", user);
                    cmd.Parameters.Add(param);
                    conn.Open();
                    cmd.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                }
            }
        }

        public static SubjectCompleted GetRandomizedStudyActive(string id)
        {
            SqlDataReader rdr = null;
            var rndm = new SubjectCompleted();
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = ("GetRandomizedStudyActive")
                              };
                    var param = new SqlParameter("@id", id);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {

                        var pos = rdr.GetOrdinal("ID");
                        rndm.ID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("StudyID");
                        rndm.StudyID = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("DateRandomized");
                        rndm.DateRandomized = rdr.GetDateTime(pos);
                        rndm.sDateRandomized = rndm.DateRandomized.ToShortDateString();

                        pos = rdr.GetOrdinal("DateCompleted");
                        if (!rdr.IsDBNull(pos))
                        {
                            rndm.DateCompleted = rdr.GetDateTime(pos);
                            rndm.sDateCompleted = rndm.DateCompleted != null
                                                      ? rndm.DateCompleted.Value.ToString("MM/dd/yyyy")
                                                      : "";
                        }

                        pos = rdr.GetOrdinal("CgmUpload");
                        if (!rdr.IsDBNull(pos))
                            rndm.CgmUpload = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("Age2to16");
                        if (!rdr.IsDBNull(pos))
                            rndm.Age2to16 = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("AgeNot2to16");
                        if (!rdr.IsDBNull(pos))
                            rndm.AgeNot2to16 = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("CBCL");
                        if (!rdr.IsDBNull(pos))
                            rndm.CBCL = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("PedsQL");
                        if (!rdr.IsDBNull(pos))
                            rndm.PedsQL = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("Demographics");
                        if (!rdr.IsDBNull(pos))
                            rndm.Demographics = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("ContactInfo");
                        if (!rdr.IsDBNull(pos))
                            rndm.ContactInfo = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("Cleared");
                        if (!rdr.IsDBNull(pos))
                            rndm.Cleared = rdr.GetBoolean(pos);

                        //pos = rdr.GetOrdinal("MonitorID");
                        //if (!rdr.IsDBNull(pos))
                        //    rndm.MonitorID = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("NotCompletedReason");
                        rndm.NotCompletedReason = !rdr.IsDBNull(pos) ? rdr.GetString(pos) : "";

                        pos = rdr.GetOrdinal("SiteName");
                        rndm.SiteName = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("ChecksRowsCompleted");
                        if (!rdr.IsDBNull(pos))
                            rndm.RowsCompleted = rdr.GetInt32(pos);

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


            return rndm;
        }

        public static List<SubjectCompleted> GetSiteRandomizedStudiesActive(int siteId)
        {
            SqlDataReader rdr = null;
            var list = new List<SubjectCompleted>();
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = ("GetSiteRandomizedStudiesActive")
                              };
                    var param = new SqlParameter("@siteID", siteId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var rndm = new SubjectCompleted();
                        var pos = rdr.GetOrdinal("ID");
                        rndm.ID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("StudyID");
                        rndm.StudyID = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("DateRandomized");
                        rndm.DateRandomized = rdr.GetDateTime(pos);
                        rndm.sDateRandomized = rndm.DateRandomized.ToString("MM/dd/yyyy");
                        rndm.STimeRandomized = rndm.DateRandomized.ToString("HH:mm");
                        
                        pos = rdr.GetOrdinal("DateCompleted");
                        if (!rdr.IsDBNull(pos))
                        {
                            rndm.DateCompleted = rdr.GetDateTime(pos);
                            rndm.sDateCompleted = rndm.DateCompleted != null
                                                      ? rndm.DateCompleted.Value.ToString("MM/dd/yyyy")
                                                      : "";
                        }

                        pos = rdr.GetOrdinal("CgmUpload");
                        if (!rdr.IsDBNull(pos))
                            rndm.CgmUpload = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("Age2to16");
                        if (!rdr.IsDBNull(pos))
                            rndm.Age2to16 = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("CBCL");
                        if (!rdr.IsDBNull(pos))
                            rndm.CBCL = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("PedsQL");
                        if (!rdr.IsDBNull(pos))
                            rndm.PedsQL = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("Demographics");
                        if (!rdr.IsDBNull(pos))
                            rndm.Demographics = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("ContactInfo");
                        if (!rdr.IsDBNull(pos))
                            rndm.ContactInfo = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("Cleared");
                        if (!rdr.IsDBNull(pos))
                            rndm.Cleared = rdr.GetBoolean(pos);

                        //pos = rdr.GetOrdinal("MonitorID");
                        //if (!rdr.IsDBNull(pos))
                        //    rndm.MonitorID = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("NotCompletedReason");
                        if (!rdr.IsDBNull(pos))
                            rndm.NotCompletedReason = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("ConsentTime");
                        rndm.SConsentTime = !rdr.IsDBNull(pos) ? rdr.GetString(pos) : string.Empty;

                        pos = rdr.GetOrdinal("ConsentDate");
                        rndm.SConsentDate = !rdr.IsDBNull(pos) ? rdr.GetDateTime(pos).ToString("MM/dd/yyyy") : String.Empty;

                        list.Add(rndm);
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


            return list;
        }

        public static List<SubjectCompleted> GetSiteRandomizedStudiesCleared(int siteId)
        {
            SqlDataReader rdr = null;
            var list = new List<SubjectCompleted>();
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = ("GetSiteRandomizedStudiesCleared")
                              };
                    var param = new SqlParameter("@siteID", siteId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var rndm = new SubjectCompleted();
                        var pos = rdr.GetOrdinal("ID");
                        rndm.ID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("StudyID");
                        rndm.StudyID = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("DateRandomized");
                        rndm.DateRandomized = rdr.GetDateTime(pos);
                        rndm.sDateRandomized = rndm.DateRandomized.ToString("MM/dd/yyyy");
                        rndm.STimeRandomized = rndm.DateRandomized.ToString("HH:mm");

                        pos = rdr.GetOrdinal("DateCompleted");
                        if (!rdr.IsDBNull(pos))
                        {
                            rndm.DateCompleted = rdr.GetDateTime(pos);
                            rndm.sDateCompleted = rndm.DateCompleted != null
                                                      ? rndm.DateCompleted.Value.ToString("MM/dd/yyyy")
                                                      : "";
                        }

                        pos = rdr.GetOrdinal("CgmUpload");
                        if (!rdr.IsDBNull(pos))
                            rndm.CgmUpload = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("Age2to16");
                        if (!rdr.IsDBNull(pos))
                            rndm.Age2to16 = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("AgeNot2to16");
                        if (!rdr.IsDBNull(pos))
                            rndm.AgeNot2to16 = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("CBCL");
                        if (!rdr.IsDBNull(pos))
                            rndm.CBCL = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("PedsQL");
                        if (!rdr.IsDBNull(pos))
                            rndm.PedsQL = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("Demographics");
                        if (!rdr.IsDBNull(pos))
                            rndm.Demographics = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("ContactInfo");
                        if (!rdr.IsDBNull(pos))
                            rndm.ContactInfo = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("Cleared");
                        if (!rdr.IsDBNull(pos))
                            rndm.Cleared = rdr.GetBoolean(pos);

                        //pos = rdr.GetOrdinal("MonitorID");
                        //if (!rdr.IsDBNull(pos))
                        //    rndm.MonitorID = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("NotCompletedReason");
                        if (!rdr.IsDBNull(pos))
                            rndm.NotCompletedReason = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("ConsentTime");
                        rndm.SConsentTime = !rdr.IsDBNull(pos) ? rdr.GetString(pos) : string.Empty;

                        pos = rdr.GetOrdinal("ConsentDate");
                        rndm.SConsentDate = !rdr.IsDBNull(pos) ? rdr.GetDateTime(pos).ToString("MM/dd/yyyy") : String.Empty;

                        list.Add(rndm);
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
            return list;
        }

        public static List<Randomization> GetSiteRandomizedStudies(int siteId)
        {
            SqlDataReader rdr = null;
            var list = new List<Randomization>();
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = ("GetSiteRandomizedStudies")
                              };
                    var param = new SqlParameter("@siteID", siteId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var rndm = new Randomization();
                        var pos = rdr.GetOrdinal("Name");
                        rndm.SiteName = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("Number");
                        rndm.Number = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("StudyID");
                        rndm.StudyID = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("Arm");
                        rndm.Arm = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("DateRandomized");
                        rndm.DateRandomized = rdr.GetDateTime(pos);

                        list.Add(rndm);
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
            return list;
        }

        public static List<StudyID> GetStudyIDsNotRandomized(int site)
        {
            SqlDataReader rdr = null;
            var sls = new List<StudyID>();
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = ("GetStudyIDsNotRandomized")
                              };
                    var param = new SqlParameter("@siteID", site);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var stid = new StudyID();
                        var pos = rdr.GetOrdinal("ID");
                        stid.ID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("SiteID");
                        stid.SiteID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("StudyID");
                        stid.SstudyID = rdr.GetString(pos);

                        sls.Add(stid);
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
            return sls;
        }

        public static MembershipUserCollection GetAllUsers()
        {
            //List<MembershipUser> users = new List<MembershipUser>();
            var mUsers = Membership.GetAllUsers();

            return mUsers;
        }

        public static List<UserInfo> GetAllUserInfo()
        {
            var mUsers = Membership.GetAllUsers();

            return (from MembershipUser user in mUsers
                select new UserInfo
                       {
                           UserName = user.UserName, Email = user.Email, Active = user.IsApproved, LockedOut = user.IsLockedOut, Online = user.IsOnline, Role = AccountUtils.GetRoleForUser(user.UserName), Site = GetSiteNameForUser(user.UserName)
                       }).ToList();
        }

        public static List<UserInfo> GetUserInfoForSite(int site)
        {
            var lUsers = new List<UserInfo>();
            SqlDataReader rdr = null;
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = ("GetSiteUsers")
                              };
                    var param = new SqlParameter("@siteID", site);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var userInfo = new UserInfo();

                        var pos = rdr.GetOrdinal("UserName");
                        userInfo.UserName = rdr.GetString(pos);
                        var user = Membership.GetUser(userInfo.UserName);
                        if (user != null)
                        {
                            userInfo.Email = user.Email;
                            userInfo.Role = AccountUtils.GetRoleForUser(userInfo.UserName);
                            userInfo.Site = GetSiteNameForUser(userInfo.UserName);
                            userInfo.LockedOut = user.IsLockedOut;
                            userInfo.Online = user.IsOnline;
                        }
                        lUsers.Add(userInfo);
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
            return lUsers;
        }

        public static List<MembershipUser> GetUsersForSite(int site)
        {
            var users = new List<MembershipUser>();
            var user = new MembershipUser("AspNetSqlMembershipProvider", "Select user", "", "", "", "",
                                                     true, false, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now,
                                                     DateTime.Now);
            users.Add(user);
            SqlDataReader rdr = null;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "GetSiteUsers"
                              };
                    var param = new SqlParameter("@siteID", site);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("UserName");
                        var userName = rdr.GetString(pos);
                        //Nlogger.LogInfo("userName:" + userName);
                        user = Membership.GetUser(userName);
                        if (user != null)
                        {
                            users.Add(user);
                        }
                        else
                        {
                            Nlogger.LogError("user is null:" + userName);
                        }
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
            return users;
        }

        public static StaffEditModel GetStaffInfoForRoleChange(int userId)
        {
            var model = new StaffEditModel();
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "GetStaffInfo"
                    };

                    var param = new SqlParameter("@id", userId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("ID");
                        model.ID = rdr.GetInt32(pos);
                        
                        pos = rdr.GetOrdinal("Role");
                        if (!rdr.IsDBNull(pos))
                            model.Role = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("UserName");
                        if (!rdr.IsDBNull(pos))
                            model.UserName = rdr.GetString(pos);
                        
                        pos = rdr.GetOrdinal("Email");
                        if (!rdr.IsDBNull(pos))
                            model.Email = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("EmployeeID");
                        if (!rdr.IsDBNull(pos))
                            model.EmployeeID = rdr.GetString(pos);
                        
                    }
                    rdr.Close();

                    return model;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return null;
                }
            }

        }

        public static object GetUserRoleAndUserName(int staffId)
        {
            string role = "";
            string userName = "";
            SqlDataReader rdr = null;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = ("GetUserRoleAndUserName")
                    };

                    var param = new SqlParameter("@staffId", staffId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("UserName");
                        if(! rdr.IsDBNull(pos))
                            userName = rdr.GetString(pos);
                        pos = rdr.GetOrdinal("Role");
                        role = rdr.GetString(pos);
                    }
                    rdr.Close();
                    return new {Role = role, UserName = userName};
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return null;
                }
                finally
                {
                    if (rdr != null)
                        rdr.Close();
                }
            }
        }
        
        public static string GetUserRole(int staffId)
        {
            string role = "";
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = ("GetUserRole")
                              };

                    var param = new SqlParameter("@staffId", staffId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    role = cmd.ExecuteScalar().ToString();
                    return role;        
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return role;
                }
            }
        }

        public static List<MembershipUser> GetUserInRole(string role, int site)
        {
            var memUsers = new List<MembershipUser>();
            string[] users = Roles.GetUsersInRole(role);
            SqlDataReader rdr = null;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = ("GetSiteUsers")
                              };
                    var param = new SqlParameter("@siteID", site);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("UserName");
                        var userName = rdr.GetString(pos);
                        memUsers.AddRange(from u in users where u == userName select Membership.GetUser(u));
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
            return memUsers;
        }

        public static int IsStudyIdRandomized(string studyId)
        {
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("Test error");
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "IsStudyIDRandomized"
                              };
                    var param = new SqlParameter("@studyID", studyId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var count = (Int32) cmd.ExecuteScalar();
                    if (count == 1)
                        return 1;
                    return 0;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return -1;
                }
            }
        }

        public static int IsStudyIdAssignedPasswordValid(string studyId, string animalName)
        {
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "IsStudyIDAssignedPasswordValid"
                              };
                    var param = new SqlParameter("@studyID", studyId);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@animalName", animalName);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var count = (Int32) cmd.ExecuteScalar();
                    if (count == 1)
                        return 1;
                    return 0;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return -1;
                }
            }
        }

        public static bool IsStudyIdDuplicate(string studyId)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = ("IsStudyIdDuplicate")
                    };
                    var param = new SqlParameter("@studyId", studyId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var count = (Int32)cmd.ExecuteScalar();
                    if (count == 1)
                        return true;
                    return false;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                }
            }

            return false;
        }

        public static int IsSiteNameDuplicate(string name)
        {
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = CommandType.StoredProcedure,
                                      CommandText = "IsSiteNameDuplicate"
                                  };
                    var param = new SqlParameter("@name", name);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var count = (Int32)cmd.ExecuteScalar();
                    if (count == 1)
                        return 1;
                    return 0;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return -1;
                }
            }
        }

        public static int IsSiteIdDuplicate(string siteId)
        {
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "IsSiteIdDuplicate"
                    };
                    var param = new SqlParameter("@siteId", siteId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var count = (Int32)cmd.ExecuteScalar();
                    if (count == 1)
                        return 1;
                    return 0;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return -1;
                }
            }
        }
        
        public static int IsStudyIdAssignedPassword(string studyId)
        {
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "IsStudyIDAssignedPassword"
                              };
                    var param = new SqlParameter("@studyID", studyId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var count = (Int32) cmd.ExecuteScalar();
                    if (count > 0)
                        return 1;
                    return 0;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return -1;
                }
            }
        }

        public static int DoesRandomizationsExistForSite(int siteId, MessageListDTO dto)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("test error");
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "DoesRandomizationsExistForSite"
                              };
                    var param = new SqlParameter("@siteId", siteId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var count = (Int32)cmd.ExecuteScalar();
                    if (count > 0)
                    {
                        dto.Dictionary.Add("importFiles", "Study id's already exist for this site.");
                        dto.ReturnValue = 0;
                        return 1;
                    }
                    return 0;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex, "SiteID:" + siteId);
                    dto.Dictionary.Add("importFiles", "There was an error checking if study id's exist for this site.");
                    dto.ReturnValue = 0;
                    return -1;
                }
            }
        }

        public static int DoesStudyIdsExistForSite(int siteId, MessageListDTO dto)
        {
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("test error");
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "DoesStudyIdsExistForSite"
                              };
                    var param = new SqlParameter("@siteId", siteId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var count = (Int32)cmd.ExecuteScalar();
                    if (count > 0)
                    {
                        dto.Dictionary.Add("importFiles", "Study id's already exist for this site.");
                        dto.ReturnValue = 0;
                        return 1;
                    }
                    return 0;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex, "SiteID:" + siteId);
                    dto.Dictionary.Add("importFiles", "There was an error checking if study id's exist for this site.");
                    dto.ReturnValue = 0;
                    return -1;
                }
            }
        }

        public static int IsStudyIdValid(string studyId)
        {
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("test error");
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "DoesStudyIDExist"
                              };
                    var param = new SqlParameter("@studyID", studyId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var count = (Int32) cmd.ExecuteScalar();
                    if (count == 1)
                        return 1;
                    return 0;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex, "StudyID:" + studyId);
                    return -1;
                }
            }
        }

        public static int IsStudyIdCleared(string studyId)
        {
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("test error");
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "IsStudyIDCleared"
                              };
                    var param = new SqlParameter("@studyID", studyId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var count = (Int32) cmd.ExecuteScalar();
                    if (count == 1)
                        return 1;
                    return 0;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex, "StudyID:" + studyId);
                    return -1;
                }
            }
        }

        public static InitializeSiteSpecific GetSiteSpecificForInitialize(int siteId)
        {
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "GetSiteSpecificForInitialize"
                    };
                    var param = new SqlParameter("@siteId", siteId);
                    cmd.Parameters.Add(param);

                    conn.Open();

                    var iss = new InitializeSiteSpecific();
                    var rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        int pos = rdr.GetOrdinal("Sensor");
                        iss.Sensor = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("UseCalfpint");
                        iss.UseCalfpint = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("UseVampjr");
                        iss.UseVampjr = rdr.GetBoolean(pos);
                    }
                    rdr.Close();

                    return iss;

                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex, "SiteID:" + siteId);
                    return null;
                }
            }
        }

        public static int GetSiteSensor(int siteId)
        {
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "GetSiteSensor"
                    };
                    var param = new SqlParameter("@siteId", siteId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var sensor = (Int32)cmd.ExecuteScalar();
                   
                    return sensor;

                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex, "SiteID:" + siteId);
                    return -1;
                }
            }
        }
        
        public static int AddRandomizationPassword(string studyId, int animalId, string consentDate,
                                                     string consentTime)
        {
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "AddRandomizationPassword"
                              };

                    var param = new SqlParameter("@animalID", animalId);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@studyID", studyId);
                    cmd.Parameters.Add(param);

                    param = consentDate.Length == 0 ? new SqlParameter("@consentDate", DBNull.Value) : new SqlParameter("@consentDate", DateTime.Parse(consentDate));
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@consentTime", consentTime);
                    cmd.Parameters.Add(param);

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

        public static string GetRandomPassword()
        {
            var password = "";
            var rnd = new Random();
            var rndId = rnd.Next(1, 18);
            SqlDataReader rdr = null;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "GetResetPasswordByID"
                              };
                    var param = new SqlParameter("@id", rndId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("Name");
                        password = rdr.GetString(pos);
                    }
                    rdr.Close();
                    return password;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return null;
                }
                finally
                {
                    if (rdr != null)
                        rdr.Close();
                }
            }

        }

        public static Animal GetRandomAnimal()
        {
            var rnd = new Random();
            var rndId = rnd.Next(1, 131);
            SqlDataReader rdr = null;
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "GetAnimalByID"
                              };
                    var param = new SqlParameter("@id", rndId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    var animal = new Animal {ID = rndId};
                    while (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("Name");
                        animal.Name = rdr.GetString(pos);
                    }
                    rdr.Close();
                    return animal;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return null;
                }
                finally
                {
                    if (rdr != null)
                        rdr.Close();
                }
            }
        }

        public static List<IDandStudyID> GetRandomizedStudiesForSite(int siteId)
        {
            var studs = new List<IDandStudyID>();

            SqlDataReader rdr = null;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "GetRandomizedStudiesForSite"
                              };

                    var param = new SqlParameter("@siteID", siteId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var stud = new IDandStudyID();

                        var pos = rdr.GetOrdinal("ID");
                        stud.ID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("StudyID");
                        stud.StudyID = rdr.GetString(pos);

                        studs.Add(stud);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                }
                finally
                {   
                    if(rdr != null)
                        rdr.Close();
                }
                return studs;
            }
        }

        public static List<Animal> GetAnimals()
        {
            var animals = new List<Animal>();
            SqlDataReader rdr = null;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "GetAnimals"
                              };

                    conn.Open();
                    rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var animal = new Animal();

                        var pos = rdr.GetOrdinal("ID");
                        animal.ID = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("Name");
                        animal.Name = rdr.GetString(pos);
                        animals.Add(animal);
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

            return animals;
        }

        public static List<InsulinConcentration> GetInsulinConcItems(int siteId)
        {
            var items = new List<InsulinConcentration>();
            SqlDataReader rdr = null;
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = ("GetSiteInsulicConcentrations")
                              };
                    var param = new SqlParameter("@siteID", siteId);
                    cmd.Parameters.Add(param);
                    conn.Open();

                    rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var item = new InsulinConcentration();

                        var pos = rdr.GetOrdinal("Concentration");
                        item.Concentration = rdr.GetDouble(pos).ToString("0.0#");
                        pos = rdr.GetOrdinal("Name");
                        item.Name = rdr.GetString(pos);
                        items.Add(item);
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
            return items;
        }

        public static List<IDandName> GetLookupItems(string itemsName)
        {
            var items = new List<IDandName>();
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = CommandType.StoredProcedure,
                                      CommandText = ("Get" + itemsName)
                                  };

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var item = new IDandName();

                        var pos = rdr.GetOrdinal("ID");
                        item.ID = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("Name");
                        item.Name = rdr.GetString(pos);
                        items.Add(item);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                }
            }
            return items;
        }

        public static List<IDandName> GetStaffLookupForSite(string siteId)
        {
            var items = new List<IDandName>();
            SqlDataReader rdr = null;
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = ("GetStaffLookupForSite")
                              };
                    var param = new SqlParameter("@siteID", siteId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var item = new IDandName();

                        var pos = rdr.GetOrdinal("ID");
                        item.ID = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("Name");
                        item.Name = rdr.GetString(pos);
                        items.Add(item);
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
            return items;
        }

        public static Dictionary<string, string> GetInitializeStudyIDsWithPassword(int siteId)
        {
            var dict = new Dictionary<string, string>();
            SqlDataReader rdr = null;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("Opps");
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "GetInitializeStudyIDsWithPassword"
                              };
                    var param = new SqlParameter("@siteID", siteId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("StudyID");
                        var studyid = rdr.GetString(pos);
                        pos = rdr.GetOrdinal("Name");
                        var animal = rdr.GetString(pos);
                        dict.Add(studyid, animal);
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


            return dict;
        }

        public static InsulinConcentration GetInsulinConcentration(int id)
        {
            var insCon = new InsulinConcentration();
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "GetInsulinConcentration"
                    };

                    var param = new SqlParameter("@id", id);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        
                        var pos = rdr.GetOrdinal("ID");
                        insCon.Id = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("Name");
                        insCon.Name = rdr.GetString(pos);
                        pos = rdr.GetOrdinal("Concentration");
                        insCon.Concentration = rdr.GetDouble(pos).ToString("0.0#");
                        insCon.IsUsed = false;

                    }
                    rdr.Close();

                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                }
            }
            return insCon;
        }

        public static List<InsulinConcentration> GetInsulinConcentrations()
        {
            var list = new List<InsulinConcentration>();
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "GetInsulinConcentrations"
                    };

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var insCon = new InsulinConcentration();
                        var pos = rdr.GetOrdinal("ID");
                        insCon.Id = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("Name");
                        insCon.Name = rdr.GetString(pos);
                        pos = rdr.GetOrdinal("Concentration");
                        insCon.Concentration = rdr.GetDouble(pos).ToString("0.0#");
                        insCon.IsUsed = false;
                        list.Add(insCon);

                    }
                    rdr.Close();

                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                }
            }
            return list;
        }

        public static int UpdateInsulinConcentrations(InsulinConcentration ic)
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
                                      CommandText = "UpdateInsulinConcentrations"
                                  };

                    var parameter = new SqlParameter("@name", ic.Name);
                    cmd.Parameters.Add(parameter);
                    
                    parameter = new SqlParameter("@concentration", ic.Concentration);
                    cmd.Parameters.Add(parameter);

                    parameter = new SqlParameter("@id", ic.Id);
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

        public static int AddInsulinConcentrations(InsulinConcentration ic)
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
                        CommandText = "AddInsulinConcentrations"
                    };

                    var parameter = new SqlParameter("@name", ic.Name);
                    cmd.Parameters.Add(parameter);

                    parameter = new SqlParameter("@concentration", ic.Concentration);
                    cmd.Parameters.Add(parameter);

                    parameter = new SqlParameter("@Identity", SqlDbType.Int, 0, "ID")
                                {
                                    Direction =
                                        ParameterDirection.Output
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

        public static SiteInfo GetSiteInfoForSite(string id)
        {
            var site = new SiteInfo();
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {

                    //throw new Exception("Opps");
                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = CommandType.StoredProcedure,
                                      CommandText = "GetSiteInfoForSite"
                                  };

                    var parameter = new SqlParameter("@id", id);
                    cmd.Parameters.Add(parameter);

                    conn.Open();

                    var rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("ID");
                        site.Id = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("SiteID");
                        site.SiteId = rdr.GetString(pos);
                        pos = rdr.GetOrdinal("Name");
                        site.Name = rdr.GetString(pos);
                        
                        pos = rdr.GetOrdinal("LongName");
                        site.LongName = rdr.IsDBNull(pos) ? string.Empty : rdr.GetString(pos);
                        
                        pos = rdr.GetOrdinal("EmpIDRequired");
                        site.IsEmployeeIdRequired = rdr.GetBoolean(pos);
                        pos = rdr.GetOrdinal("EmpIDRegex");
                        site.EmployeeIdRegEx = rdr.IsDBNull(pos) ? "" : rdr.GetString(pos);
                        pos = rdr.GetOrdinal("EmpIDMessage");
                        site.EmployeeIdMessage = rdr.IsDBNull(pos) ? "" : rdr.GetString(pos);
                        pos = rdr.GetOrdinal("AcctPassword");
                        site.AcctPassword = rdr.IsDBNull(pos) ? "" : rdr.GetString(pos);
                        pos = rdr.GetOrdinal("AcctUserName");
                        site.AcctUserName = rdr.IsDBNull(pos) ? "" : rdr.GetString(pos);

                        pos = rdr.GetOrdinal("Active");
                        site.IsActive = rdr.GetBoolean(pos);
                        pos = rdr.GetOrdinal("Sensor");
                        site.Sensor = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("UseVampjr");
                        site.UseVampjr = rdr.GetBoolean(pos);
                        pos = rdr.GetOrdinal("UseCalfpint");
                        site.UseCalfpint = rdr.GetBoolean(pos);
                        pos = rdr.GetOrdinal("Language");
                        site.Language = rdr.GetInt32(pos);
                    }
                    rdr.Close();
                    conn.Close();

                    cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "GetInsulinConcentrations"
                    };

                    conn.Open();
                    rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        if (site.InsulinConcentrations == null)
                            site.InsulinConcentrations = new List<InsulinConcentration>();

                        var insCon = new InsulinConcentration();
                        var pos = rdr.GetOrdinal("ID");
                        insCon.Id = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("Name");
                        insCon.Name = rdr.GetString(pos);
                        pos = rdr.GetOrdinal("Concentration");
                        insCon.Concentration = rdr.GetDouble(pos).ToString("0.0#");
                        insCon.IsUsed = false;
                        site.InsulinConcentrations.Add(insCon);
                        
                    }
                    rdr.Close();
                    conn.Close();

                    cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "GetSiteInsulinConcentrations"
                              };

                    parameter = new SqlParameter("@siteId", id);
                    cmd.Parameters.Add(parameter);

                    
                    conn.Open();
                    rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("ID");
                        var id2 = rdr.GetInt32(pos);
                        var insCon = site.InsulinConcentrations.Find(x => x.Id == id2);
                        insCon.IsUsed = true;
                    }
                    rdr.Close();
                    conn.Close();

                    cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "DoesSiteHaveRandomizations"
                    };

                    parameter = new SqlParameter("@siteID", id);
                    cmd.Parameters.Add(parameter);
                    
                    conn.Open();
                    rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("Randomizations");
                        int randoms = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("StudyIDs");
                        int studyIds = rdr.GetInt32(pos);
                        
                        site.HasRandomizations = randoms != 0;
                        site.HasStudyIds = studyIds != 0;
                    }
                    else
                    {
                        site.HasRandomizations = false;
                        site.HasStudyIds = false;
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                }
            }

            return site;
        }

        public static SiteInfo GetSiteInfoForNewSite()
        {
            var site = new SiteInfo();
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "GetInsulinConcentrations"
                    };

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        if (site.InsulinConcentrations == null)
                            site.InsulinConcentrations = new List<InsulinConcentration>();

                        var insCon = new InsulinConcentration();
                        var pos = rdr.GetOrdinal("ID");
                        insCon.Id = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("Name");
                        insCon.Name = rdr.GetString(pos);
                        pos = rdr.GetOrdinal("Concentration");
                        insCon.Concentration = rdr.GetDouble(pos).ToString("0.0#");
                        insCon.IsUsed = false;
                        site.InsulinConcentrations.Add(insCon);

                    }
                    rdr.Close();
                    
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                }
            }

            return site;
        }

        public static List<SiteInfo> GetSitesAll()
        {
            var sites = new List<SiteInfo>();
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("Opps");
                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = CommandType.StoredProcedure,
                                      CommandText = "GetSitesActive"
                                  };

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var site = new SiteInfo();

                        var pos = rdr.GetOrdinal("ID");
                        site.Id = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("SiteID");
                        site.SiteId = rdr.GetString(pos);
                        pos = rdr.GetOrdinal("Name");
                        site.Name = rdr.GetString(pos);
                        pos = rdr.GetOrdinal("LongName");
                        site.LongName = !rdr.IsDBNull(pos) ? rdr.GetString(pos) : "";
                        pos = rdr.GetOrdinal("EmpIDRequired");
                        site.IsEmployeeIdRequired = rdr.GetBoolean(pos);
                        pos = rdr.GetOrdinal("EmpIDRegex");
                        site.EmployeeIdRegEx = rdr.IsDBNull(pos) ? "" : rdr.GetString(pos);
                        pos = rdr.GetOrdinal("EmpIDMessage");
                        site.EmployeeIdMessage = rdr.IsDBNull(pos) ? "" : rdr.GetString(pos);
                        pos = rdr.GetOrdinal("AcctPassword");
                        site.AcctPassword = rdr.IsDBNull(pos) ? "" : rdr.GetString(pos);
                        pos = rdr.GetOrdinal("AcctUserName");
                        site.AcctUserName = rdr.IsDBNull(pos) ? "" : rdr.GetString(pos);
                        pos = rdr.GetOrdinal("Active");
                        site.IsActive = rdr.GetBoolean(pos);
                        pos = rdr.GetOrdinal("Sensor");
                        site.Sensor = rdr.GetInt32(pos);
                        sites.Add(site);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                }
            }

            return sites;
        }

        public static List<Site> GetSitesActiveForNovanetList()
        {
            var sites = new List<Site>();
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("Opps");
                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = CommandType.StoredProcedure,
                                      CommandText = "GetSitesActive"
                                  };

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var site = new Site();
                        var pos = rdr.GetOrdinal("EmpIDRequired");
                        if (!rdr.GetBoolean(pos))
                            continue;

                        pos = rdr.GetOrdinal("ID");
                        site.ID = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("SiteID");
                        site.SiteID = rdr.GetString(pos);
                        pos = rdr.GetOrdinal("Name");
                        site.Name = rdr.GetString(pos);
                        sites.Add(site);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                }
            }

            return sites;
        }

        public static SiteGerenicNurse GetGenericUserInfo(int site)
        {
            var sgn = new SiteGerenicNurse();
            SqlDataReader rdr = null;
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "GetSiteGenericAccountInfo"
                    };
                    var param = new SqlParameter("@id", site);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("AcctUserName");
                        if (!rdr.IsDBNull(pos))
                            sgn.UserName = rdr.GetString(pos);
                        pos = rdr.GetOrdinal("AcctPassword");
                        if (!rdr.IsDBNull(pos))
                            sgn.UserPassword = rdr.GetString(pos);

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
                return sgn;
            }
        }

        public static List<Site> GetSitesActive()
        {
            var sites = new List<Site>();
            SqlDataReader rdr = null;
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("Opps");
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "GetSitesActive"
                    };

                    conn.Open();
                    rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var site = new Site();

                        var pos = rdr.GetOrdinal("ID");
                        site.ID = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("SiteID");
                        site.SiteID = rdr.GetString(pos);
                        pos = rdr.GetOrdinal("Name");
                        site.Name = rdr.GetString(pos);
                        sites.Add(site);
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

            return sites;
        }
        public static List<SelectListItem> GetSitesActiveListItems(List<Site> sites)
        {
            var list = new List<SelectListItem>();
            foreach (var site in sites)
            {
                var item = new SelectListItem();
                item.Value = site.ID.ToString();
                item.Text = site.Name;
                list.Add(item);
            }

            return list;
        }
        
        public static int RemoveUser(string userName)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("Opps");
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "DeleteUserSite"
                              };

                    var param = new SqlParameter("@userName", userName);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return -1;
                }
            }
            if (!Membership.DeleteUser(userName))
                return 0;
            return 1;
        }

        public static bool AddUserSite(string userName, int siteId)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("oops");
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "AddUserSite"
                              };

                    var param = new SqlParameter("@userName", userName);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@siteID", siteId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns the site code
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static string GetSiteCodeForUser(string userName)
        {
            // Gets the siteCode
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            string retVal = "";
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "GetSiteInfoForUser"
                              };

                    var param = new SqlParameter("@userName", userName);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("SiteID");
                        retVal = rdr.GetString(pos);
                    }
                    rdr.Close();
                    return retVal;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return "error";
                }
            }

        }

        public static string GetSiteIDandNameForUser(string userName)
        {
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            var retVal = "";
            SqlDataReader rdr = null;
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "GetSiteInfoForUser"
                              };

                    var param = new SqlParameter("@userName", userName);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("SiteID");
                        retVal = rdr.GetString(pos) + ":";
                        pos = rdr.GetOrdinal("Name");
                        retVal = retVal + rdr.GetString(pos);
                    }
                    rdr.Close();
                    return retVal;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return "error";
                }
                finally
                {
                    if (rdr != null)
                        rdr.Close();
                }
            }

        }

        public static string GetSiteNameForUser(string userName)
        {
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            var retVal = "";
            SqlDataReader rdr = null;
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "GetSiteInfoForUser"
                              };

                    var param = new SqlParameter("@userName", userName);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("Name");
                        retVal = rdr.GetString(pos);
                    }
                    rdr.Close();
                    return retVal;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return "error";
                }
                finally
                {
                    if (rdr != null)
                        rdr.Close();
                }
            }

        }

        public static string GetSiteCodeForSite(int siteId)
        {
            string retVal = string.Empty;
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            SqlDataReader rdr = null;
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "GetSiteCodeBySiteID"
                    };

                    var param = new SqlParameter("@siteID", siteId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("SiteID");
                        retVal = rdr.GetString(pos);
                    }
                    rdr.Close();
                    return retVal;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return retVal;
                }
                finally
                {
                    if (rdr != null)
                        rdr.Close();
                }
            }

        }

        public static int GetSiteLanguage(int siteId)
        {
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            SqlDataReader rdr = null;
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "GetSiteLanguage"
                    };

                    var param = new SqlParameter("@siteID", siteId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("Language");
                        return rdr.GetInt32(pos);
                    }
                    
                    rdr.Close();
                    return 0;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return -1;
                }
                finally
                {
                    if (rdr != null)
                        rdr.Close();
                }
            }
        }

        public static DTO GetSiteCodeForSiteId(int siteId)
        {
            var dto = new DTO
            {
                ReturnValue = 1,
                IsSuccessful = true
            };
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            SqlDataReader rdr = null;
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "GetSiteCodeBySiteID"
                              };

                    var param = new SqlParameter("@siteID", siteId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("SiteID");
                        dto.Bag = rdr.GetString(pos);
                    }
                    else
                    {
                        dto.IsSuccessful = false;
                    }
                    rdr.Close();
                    return dto;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    dto.IsSuccessful = false;
                    dto.Message = "Error getting the site code.  This error has been reported to the administrator.";
                    dto.ReturnValue = -1;
                    return dto;
                }
                finally
                {
                    if (rdr != null)
                        rdr.Close();
                }
            }

        }

        public static int GetSiteidIdForUser(string userName)
        {
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            var iretVal = 0;
            SqlDataReader rdr = null;
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "GetSiteInfoForUser"
                              };

                    var param = new SqlParameter("@userName", userName);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("ID");
                        iretVal = rdr.GetInt32(pos);
                    }
                    rdr.Close();
                    return iretVal;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return -1;
                }
                finally
                {
                    if (rdr != null)
                        rdr.Close();
                }
            }

        }

        public static SiteInfo GetSiteInfoForUser(string userName)
        {
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            var si = new SiteInfo();
            SqlDataReader rdr = null;
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "GetSiteInfoForUser"
                              };

                    var param = new SqlParameter("@userName", userName);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("ID");
                        si.Id = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("Name");
                        si.Name = rdr.GetString(pos);
                        pos = rdr.GetOrdinal("SiteID");
                        si.SiteId = rdr.GetString(pos);
                        pos = rdr.GetOrdinal("Language");
                        si.Language = rdr.GetInt32(pos);
                    }
                    rdr.Close();
                    return si;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return si;
                }
                finally
                {
                    if (rdr != null)
                        rdr.Close();
                }
            }

        }

        public static MessageListDTO AddStaff(StaffModel model)
        {
            var dto = new MessageListDTO();

            // Attempt to add the user to the membership db
            var password = GetRandomPassword();
            MembershipCreateStatus createStatus;
            var user = Membership.CreateUser(model.UserName, password, model.Email, null, null, true, null,
                                                        out createStatus);

            if (createStatus == MembershipCreateStatus.Success)
            {
                //FormsAuthentication.SetAuthCookie(model.UserName, false /* createPersistentCookie */);                    
                if (!AddUserSite(model.UserName, model.SiteID))
                {
                    dto.Messages.Add("There was an error adding the user and site to the database");
                    dto.IsSuccessful = false;
                    return dto;
                }

                //this will tell us that user needs to reset
                if (user != null)
                {
                    user.Comment = "Reset";
                    Membership.UpdateUser(user);
                }
                Nlogger.LogInfo("AddUser - userName: " + model.UserName + ", site: " + model.SiteID +
                                ", password: " + password);
            }
            else
            {
                dto.Messages.Add("Membership user not created for reason: " + ErrorCodeToString(createStatus));
                dto.IsSuccessful = false;
                return dto;
            }
            dto.Messages.Add("New user, " + model.UserName + ", was created successfully!");
            dto.Bag = password;

            //add the user role
            Roles.AddUserToRole(model.UserName, model.Role);
            dto.Messages.Add("The role of " + model.Role + " was assigned successfully!");

            //add the user to the staff table
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "AddStaff"
                              };

                    var param = new SqlParameter("@siteID", model.SiteID);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@role", model.Role);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@userName", model.UserName);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@firstName", model.FirstName);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@lastName", model.LastName);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@email", model.Email);
                    cmd.Parameters.Add(param);

                    param = model.EmployeeID == null ? new SqlParameter("@employeeID", DBNull.Value) : new SqlParameter("@employeeID", model.EmployeeID);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@phone", model.Phone);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@novaStatStrip", model.NovaStatStrip);
                    cmd.Parameters.Add(param);
                    param = model.NovaStatStrip ? new SqlParameter("@novaStatStripDoc", model.NovaStatStripDoc) : new SqlParameter("@novaStatStripDoc", DBNull.Value);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@vamp", model.Vamp);
                    cmd.Parameters.Add(param);
                    param = model.Vamp ? new SqlParameter("@vampDoc", model.VampDoc) : new SqlParameter("@vampDoc", DBNull.Value);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@cgm", model.Cgm);
                    cmd.Parameters.Add(param);
                    param = model.Cgm ? new SqlParameter("@cgmDoc", model.CgmDoc) : new SqlParameter("@cgmDoc", DBNull.Value);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@inform", model.Inform);
                    cmd.Parameters.Add(param);
                    param = model.Inform ? new SqlParameter("@informDoc", model.InformDoc) : new SqlParameter("@informDoc", DBNull.Value);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@onCall", model.OnCall);
                    cmd.Parameters.Add(param);
                    param = model.OnCall ? new SqlParameter("@onCallDoc", model.OnCallDoc) : new SqlParameter("@onCallDoc", DBNull.Value);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@humanSubj", model.HumanSubj);
                    cmd.Parameters.Add(param);
                    if (model.HumanSubj)
                    {
                        param = new SqlParameter("@humanSubjStart", model.HumanSubjStart);
                        cmd.Parameters.Add(param);
                        param = new SqlParameter("@humanSubjExp", model.HumanSubjExp);
                        cmd.Parameters.Add(param);
                    }
                    else
                    {
                        param = new SqlParameter("@humanSubjStart", DBNull.Value);
                        cmd.Parameters.Add(param);
                        param = new SqlParameter("@humanSubjExp", DBNull.Value);
                        cmd.Parameters.Add(param);
                    }

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    dto.IsSuccessful = false;
                    dto.Messages.Add("There was an error adding the new staff info into the staff database");
                    return dto;
                }
            }
            dto.Messages.Add("New staff information was successfully entered into the database!");
            dto.IsSuccessful = true;
            return dto;
        }

        #region Status Codes

        public static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return
                        "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return
                        "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return
                        "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return
                        "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        #endregion

        public static StaffEditModel GetStaffInfo(int userId)
        {
            var model = new StaffEditModel();
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = CommandType.StoredProcedure,
                                      CommandText = "GetStaffInfo"
                                  };

                    var param = new SqlParameter("@id", userId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    int pos;
                    if (rdr.Read())
                    {
                        pos = rdr.GetOrdinal("ID");
                        model.ID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("Active");
                        model.Active = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("SiteID");
                        model.SiteID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("Role");
                        if (!rdr.IsDBNull(pos))
                            model.Role = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("UserName");
                        if (!rdr.IsDBNull(pos))
                            model.UserName = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("FirstName");
                        if (!rdr.IsDBNull(pos))
                            model.FirstName = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("LastName");
                        if (!rdr.IsDBNull(pos))
                            model.LastName = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("Email");
                        if (!rdr.IsDBNull(pos))
                            model.Email = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("EmployeeID");
                        if (!rdr.IsDBNull(pos))
                            model.EmployeeID = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("Phone");
                        if (!rdr.IsDBNull(pos))
                            model.Phone = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("NovaStatStrip");
                        model.NovaStatStrip = rdr.GetBoolean(pos);
                        pos = rdr.GetOrdinal("NovaStatStripDoc");
                        if (!rdr.IsDBNull(pos))
                            model.NovaStatStripDoc = rdr.GetDateTime(pos);

                        pos = rdr.GetOrdinal("Vamp");
                        model.Vamp = rdr.GetBoolean(pos);
                        pos = rdr.GetOrdinal("VampDoc");
                        if (!rdr.IsDBNull(pos))
                            model.VampDoc = rdr.GetDateTime(pos);

                        pos = rdr.GetOrdinal("Cgm");
                        model.Cgm = rdr.GetBoolean(pos);
                        pos = rdr.GetOrdinal("CgmDoc");
                        if (!rdr.IsDBNull(pos))
                            model.CgmDoc = rdr.GetDateTime(pos);

                        pos = rdr.GetOrdinal("Inform");
                        model.Inform = rdr.GetBoolean(pos);
                        pos = rdr.GetOrdinal("InformDoc");
                        if (!rdr.IsDBNull(pos))
                            model.InformDoc = rdr.GetDateTime(pos);

                        pos = rdr.GetOrdinal("OnCall");
                        model.OnCall = rdr.GetBoolean(pos);
                        pos = rdr.GetOrdinal("OnCallDoc");
                        if (!rdr.IsDBNull(pos))
                            model.OnCallDoc = rdr.GetDateTime(pos);

                        pos = rdr.GetOrdinal("HumanSubj");
                        model.HumanSubj = rdr.GetBoolean(pos);
                        pos = rdr.GetOrdinal("HumanSubjStart");
                        if (!rdr.IsDBNull(pos))
                            model.HumanSubjStart = rdr.GetDateTime(pos);
                        pos = rdr.GetOrdinal("HumanSubjExp");
                        if (!rdr.IsDBNull(pos))
                            model.HumanSubjExp = rdr.GetDateTime(pos);

                    }
                    rdr.Close();

                    //get tests completed
                    cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "GetStaffTestsCompleted"
                              };
                    param = new SqlParameter("@id", userId);
                    cmd.Parameters.Add(param);

                    rdr = cmd.ExecuteReader();

                    var ptpc = new PostTestPersonTestsCompleted();
                    while (rdr.Read())
                    {
                        var pt = new PostTest();
                        pos = rdr.GetOrdinal("Name");
                        pt.Name = rdr.GetString(pos);
                        pos = rdr.GetOrdinal("DateCompleted");
                        if (!rdr.IsDBNull(pos))
                        {
                            pt.DateCompleted = rdr.GetDateTime(pos);
                            pt.sDateCompleted = pt.DateCompleted != null
                                                    ? pt.DateCompleted.Value.ToString("MM/dd/yyyy")
                                                    : "";
                        }
                        ptpc.PostTestsCompleted.Add(pt);
                    }
                    rdr.Close();
                    model.PostTestsCompleted = ptpc;


                    //get tests completed
                    cmd = new SqlCommand("", conn)
                          {
                              CommandType = CommandType.StoredProcedure,
                              CommandText = "GetStaffTestsCompletedHistory"
                          };
                    param = new SqlParameter("@id", userId);
                    cmd.Parameters.Add(param);

                    rdr = cmd.ExecuteReader();

                    ptpc = new PostTestPersonTestsCompleted();
                    while (rdr.Read())
                    {
                        var pt = new PostTest();
                        pos = rdr.GetOrdinal("Name");
                        pt.Name = rdr.GetString(pos);
                        pos = rdr.GetOrdinal("DateCompleted");
                        if (!rdr.IsDBNull(pos))
                        {
                            pt.DateCompleted = rdr.GetDateTime(pos);
                            pt.sDateCompleted = pt.DateCompleted != null
                                                    ? pt.DateCompleted.Value.ToString("MM/dd/yyyy")
                                                    : "";
                        }
                        ptpc.PostTestsCompleted.Add(pt);
                    }
                    rdr.Close();
                    model.PostTestsCompletedHistory = ptpc;
                    return model;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return null;
                }
            }
        }

        public static MessageListDTO UpdateStaff(StaffEditModel model)
        {
            var dto = new MessageListDTO();
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "UpdateStaff"
                              };

                    var param = new SqlParameter("@id", model.ID);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@active", model.Active);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@firstName", model.FirstName);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@lastName", model.LastName);
                    cmd.Parameters.Add(param);

                    param = model.EmployeeID == null ? new SqlParameter("@employeeID", DBNull.Value) : new SqlParameter("@employeeID", model.EmployeeID);
                    cmd.Parameters.Add(param);

                    param = model.Email == null ? new SqlParameter("@email", DBNull.Value) : new SqlParameter("@email", model.Email);
                    cmd.Parameters.Add(param);

                    param = model.Phone == null ? new SqlParameter("@phone", DBNull.Value) : new SqlParameter("@phone", model.Phone);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@novaStatStrip", model.NovaStatStrip);
                    cmd.Parameters.Add(param);
                    param = model.NovaStatStrip ? new SqlParameter("@novaStatStripDoc", model.NovaStatStripDoc) : new SqlParameter("@novaStatStripDoc", DBNull.Value);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@vamp", model.Vamp);
                    cmd.Parameters.Add(param);
                    param = model.Vamp ? new SqlParameter("@vampDoc", model.VampDoc) : new SqlParameter("@vampDoc", DBNull.Value);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@cgm", model.Cgm);
                    cmd.Parameters.Add(param);
                    param = model.Cgm ? new SqlParameter("@cgmDoc", model.CgmDoc) : new SqlParameter("@cgmDoc", DBNull.Value);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@inform", model.Inform);
                    cmd.Parameters.Add(param);
                    param = model.Inform ? new SqlParameter("@informDoc", model.InformDoc) : new SqlParameter("@informDoc", DBNull.Value);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@onCall", model.OnCall);
                    cmd.Parameters.Add(param);
                    param = model.OnCall ? new SqlParameter("@onCallDoc", model.OnCallDoc) : new SqlParameter("@onCallDoc", DBNull.Value);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@humanSubj", model.HumanSubj);
                    cmd.Parameters.Add(param);
                    if (model.HumanSubj)
                    {
                        param = new SqlParameter("@humanSubjStart", model.HumanSubjStart);
                        cmd.Parameters.Add(param);
                        param = new SqlParameter("@humanSubjExp", model.HumanSubjExp);
                        cmd.Parameters.Add(param);
                    }
                    else
                    {
                        param = new SqlParameter("@humanSubjStart", DBNull.Value);
                        cmd.Parameters.Add(param);
                        param = new SqlParameter("@humanSubjExp", DBNull.Value);
                        cmd.Parameters.Add(param);
                    }

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    dto.IsSuccessful = false;
                    dto.Messages.Add("There was an error adding the new staff info into the staff database");
                    return dto;
                }
            }
            dto.Messages.Add("New staff information was successfully entered into the database!");
            dto.IsSuccessful = true;
            return dto;
        }

        public static MessageListDTO UpdateStaffInfoForRoleChange(string staffId, string email, string employeeId, string userName, string role)
        {
            var dto = new MessageListDTO();
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "UpdateStaffInfoForRoleChange"
                    };

                    var param = new SqlParameter("@staffId", staffId);
                    cmd.Parameters.Add(param);
                    
                    param = new SqlParameter("@role", role);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@email", email);
                    cmd.Parameters.Add(param);

                    param = string.IsNullOrEmpty(employeeId) ? new SqlParameter("@employeeID", DBNull.Value) : new SqlParameter("@employeeID", employeeId);
                    cmd.Parameters.Add(param);

                    param = string.IsNullOrEmpty(userName) ? new SqlParameter("@userName", DBNull.Value) : new SqlParameter("@userName", userName);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    dto.IsSuccessful = false;
                    dto.Messages.Add("There was an error changing the staff info into the staff database");
                    return dto;
                }
            }
            dto.Messages.Add("Staff information was successfully changed into the database!");
            dto.IsSuccessful = true;
            return dto;
        }

        public static MessageListDTO UpdateStaffAdmin(StaffEditModel model)
        {
            var dto = new MessageListDTO();
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "UpdateStaffAdmin"
                              };

                    var param = new SqlParameter("@id", model.ID);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@active", model.Active);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@firstName", model.FirstName);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@lastName", model.LastName);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@role", model.Role);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@email", model.Email);
                    cmd.Parameters.Add(param);

                    param = model.EmployeeID == null ? new SqlParameter("@employeeID", DBNull.Value) : new SqlParameter("@employeeID", model.EmployeeID);
                    cmd.Parameters.Add(param);

                    param = model.Phone == null ? new SqlParameter("@phone", DBNull.Value) : new SqlParameter("@phone", model.Phone);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@novaStatStrip", model.NovaStatStrip);
                    cmd.Parameters.Add(param);
                    param = model.NovaStatStrip ? new SqlParameter("@novaStatStripDoc", model.NovaStatStripDoc) : new SqlParameter("@novaStatStripDoc", DBNull.Value);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@vamp", model.Vamp);
                    cmd.Parameters.Add(param);
                    param = model.Vamp ? new SqlParameter("@vampDoc", model.VampDoc) : new SqlParameter("@vampDoc", DBNull.Value);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@cgm", model.Cgm);
                    cmd.Parameters.Add(param);
                    param = model.Cgm ? new SqlParameter("@cgmDoc", model.CgmDoc) : new SqlParameter("@cgmDoc", DBNull.Value);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@inform", model.Inform);
                    cmd.Parameters.Add(param);
                    param = model.Inform ? new SqlParameter("@informDoc", model.InformDoc) : new SqlParameter("@informDoc", DBNull.Value);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@onCall", model.OnCall);
                    cmd.Parameters.Add(param);
                    param = model.OnCall ? new SqlParameter("@onCallDoc", model.OnCallDoc) : new SqlParameter("@onCallDoc", DBNull.Value);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@humanSubj", model.HumanSubj);
                    cmd.Parameters.Add(param);
                    if (model.HumanSubj)
                    {
                        param = new SqlParameter("@humanSubjStart", model.HumanSubjStart);
                        cmd.Parameters.Add(param);
                        param = new SqlParameter("@humanSubjExp", model.HumanSubjExp);
                        cmd.Parameters.Add(param);
                    }
                    else
                    {
                        param = new SqlParameter("@humanSubjStart", DBNull.Value);
                        cmd.Parameters.Add(param);
                        param = new SqlParameter("@humanSubjExp", DBNull.Value);
                        cmd.Parameters.Add(param);
                    }

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    dto.IsSuccessful = false;
                    dto.Messages.Add("There was an error adding the new staff info into the staff database");
                    return dto;
                }
            }
            dto.Messages.Add("New staff information was successfully entered into the database!");
            dto.IsSuccessful = true;
            return dto;
        }

        public static MessageListDTO SaveSiteInfo(SiteInfo siteInfo)
        {
            var dto = new MessageListDTO();
            
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                conn.Open();
                using (SqlTransaction trn = conn.BeginTransaction())
                {
                    try
                    {
                        var cmd = new SqlCommand("", conn)
                                  {
                                      Transaction = trn,
                                      CommandType = CommandType.StoredProcedure,
                                      CommandText = "DeleteSiteInsulinConcentrations"
                                  };
                        var param = new SqlParameter("@siteID", siteInfo.Id);
                        cmd.Parameters.Add(param);

                        cmd.ExecuteNonQuery();
                        
                        cmd = new SqlCommand("", conn)
                              {
                                  Transaction = trn,
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "UpdateSiteInfo"
                              };

                        param = new SqlParameter("@id", siteInfo.Id);
                        cmd.Parameters.Add(param);

                        string longName = "";
                        if (siteInfo.LongName != null)
                            longName = siteInfo.LongName;

                        param = new SqlParameter("@longName", longName);
                        cmd.Parameters.Add(param);

                        param = new SqlParameter("@empIDRequired", siteInfo.IsEmployeeIdRequired);
                        cmd.Parameters.Add(param);

                        param = siteInfo.EmployeeIdRegEx == null ? new SqlParameter("@empIDRexEx", DBNull.Value) : new SqlParameter("@empIDRexEx", siteInfo.EmployeeIdRegEx);
                        cmd.Parameters.Add(param);

                        param = siteInfo.EmployeeIdMessage == null ? new SqlParameter("@empIDMessage", DBNull.Value) : new SqlParameter("@empIDMessage", siteInfo.EmployeeIdMessage);
                        cmd.Parameters.Add(param);

                        param = new SqlParameter("@active", siteInfo.IsActive);
                        cmd.Parameters.Add(param);

                        param = new SqlParameter("@useCalfpint", siteInfo.UseCalfpint);
                        cmd.Parameters.Add(param);

                        param = new SqlParameter("@useVampjr", siteInfo.UseVampjr);
                        cmd.Parameters.Add(param);

                        param = new SqlParameter("@sensor", siteInfo.Sensor);
                        cmd.Parameters.Add(param);

                        param = new SqlParameter("@language", siteInfo.Language);
                        cmd.Parameters.Add(param);

                        param = string.IsNullOrEmpty(siteInfo.AcctPassword) ? new SqlParameter("@acctPassword", DBNull.Value) : new SqlParameter("@acctPassword", siteInfo.AcctPassword);
                        cmd.Parameters.Add(param);

                        param = string.IsNullOrEmpty(siteInfo.AcctUserName) ? new SqlParameter("@acctUserName", DBNull.Value) : new SqlParameter("@acctUserName", siteInfo.AcctUserName);
                        cmd.Parameters.Add(param);

                        cmd.ExecuteNonQuery();
                        
                        foreach (var incon in siteInfo.InsulinConcentrations)
                        {
                            if(incon.IsUsed)
                            {
                                cmd = new SqlCommand("", conn)
                                      {
                                          Transaction = trn,
                                          CommandType = CommandType.StoredProcedure,
                                          CommandText = "AddSiteInsulinConcentration"
                                      };
                                param = new SqlParameter("@siteId", siteInfo.Id);
                                cmd.Parameters.Add(param);
                                param = new SqlParameter("@insulinConId", incon.Id);
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
                        dto.Dictionary.Add("siteInfo", "There was an error in SaveSiteInfo");
                        return dto;
                    }
                }
            }
            dto.Messages.Add("Site information was successfully saved to the database!");
            dto.IsSuccessful = true;
            return dto;
        }

        public static bool AddRandomizationForNewSite(string number, string arm, int siteId, MessageListDTO dto)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "AddRandomizationForNewSite"
                              };

                    var param = new SqlParameter("@number", number);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@arm", arm);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@site", siteId);
                    cmd.Parameters.Add(param);


                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    dto.ReturnValue = 0;
                    dto.Dictionary.Add("importFiles", "There was an error adding the randomization:" + number + " to the database");
                    return false;
                }

            }

            return true;
        }

        public static bool AddDexcomSkipSubject(string subjectId)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = "AddDexcomSkip"
                    };

                    var param = new SqlParameter("@subjectId", subjectId);
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

        public static bool AddStudyId(string studyId, int siteId, MessageListDTO dto)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = "AddStudyId"
                              };

                    var param = new SqlParameter("@studyId", studyId);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@siteId", siteId);
                    cmd.Parameters.Add(param);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    dto.ReturnValue = 0;
                    dto.Dictionary.Add("importFiles", "There was an error adding the study id:" + studyId + " to the database");
                    return false;
                }
                
            }

            return true;
        }

        public static MessageListDTO AddSiteInfo(SiteInfo siteInfo)
        {
            var dto = new MessageListDTO {ReturnValue = 0};

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                conn.Open();
                using (SqlTransaction trn = conn.BeginTransaction())
                {
                    try
                    {
                        var cmd = new SqlCommand("", conn)
                                  {
                                      Transaction = trn,
                                      CommandType = CommandType.StoredProcedure,
                                      CommandText = "AddSite"
                                  };

                        var param = new SqlParameter("@siteId", siteInfo.SiteId);
                        cmd.Parameters.Add(param);

                        param = new SqlParameter("@name", siteInfo.Name);
                        cmd.Parameters.Add(param);

                        string longName = "";
                        if (siteInfo.LongName != null)
                            longName = siteInfo.LongName;

                        param = new SqlParameter("@longName", longName);
                        cmd.Parameters.Add(param);

                        param = new SqlParameter("@empIDRequired", siteInfo.IsEmployeeIdRequired);
                        cmd.Parameters.Add(param);

                        param = string.IsNullOrEmpty(siteInfo.EmployeeIdRegEx) ? new SqlParameter("@empIDRegex", DBNull.Value) : new SqlParameter("@empIDRegex", siteInfo.EmployeeIdRegEx);
                        cmd.Parameters.Add(param);

                        param = string.IsNullOrEmpty(siteInfo.EmployeeIdMessage) ? new SqlParameter("@empIDMessage", DBNull.Value) : new SqlParameter("@empIDMessage", siteInfo.EmployeeIdMessage);
                        cmd.Parameters.Add(param);

                        param = string.IsNullOrEmpty(siteInfo.AcctPassword) ? new SqlParameter("@acctPassword", DBNull.Value) : new SqlParameter("@acctPassword", siteInfo.AcctPassword);
                        cmd.Parameters.Add(param);

                        param = string.IsNullOrEmpty(siteInfo.AcctUserName) ? new SqlParameter("@acctUserName", DBNull.Value) : new SqlParameter("@acctUserName", siteInfo.AcctUserName);
                        cmd.Parameters.Add(param);

                        param = new SqlParameter("@active", siteInfo.IsActive);
                        cmd.Parameters.Add(param);

                        param = new SqlParameter("@useCalfpint", siteInfo.UseCalfpint);
                        cmd.Parameters.Add(param);

                        param = new SqlParameter("@useVampjr", siteInfo.UseVampjr);
                        cmd.Parameters.Add(param);

                        param = new SqlParameter("@sensor", siteInfo.Sensor);
                        cmd.Parameters.Add(param);

                        param = new SqlParameter("@language", siteInfo.Language);
                        cmd.Parameters.Add(param);

                        param = new SqlParameter("@Identity", SqlDbType.Int, 0, "ID")
                                {
                                    Direction =
                                        ParameterDirection.Output
                                };
                        cmd.Parameters.Add(param);

                        cmd.ExecuteNonQuery();
                        siteInfo.Id = (int)cmd.Parameters["@Identity"].Value;

                        foreach (var incon in siteInfo.InsulinConcentrations)
                        {
                            if (incon.IsUsed)
                            {
                                cmd = new SqlCommand("", conn)
                                      {
                                          Transaction = trn,
                                          CommandType = CommandType.StoredProcedure,
                                          CommandText = "AddSiteInsulinConcentration"
                                      };
                                param = new SqlParameter("@siteId", siteInfo.Id);
                                cmd.Parameters.Add(param);
                                param = new SqlParameter("@insulinConId", incon.Id);
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
                        dto.Dictionary.Add("siteInfo", "There was an error in AddSiteInfo");
                        dto.ReturnValue = -1;
                        return dto;
                    }
                }
            }
            dto.Messages.Add("New site information was successfully entered into the database!");
            dto.ReturnValue = 1;
            return dto;
        }

        public static List<ChecksGg> GetChecksGgReport(int studyId, string startDate, string endDate )
        {
            var list = new List<ChecksGg>();
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = CommandType.StoredProcedure,
                                  CommandText = ("GetChecksGgReport")
                              };
                    var param = new SqlParameter("@studyId", studyId);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@startDate", startDate);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@endDate", endDate);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var ggItem = new ChecksGg();
                        var pos = rdr.GetOrdinal("Meter_Time");
                        ggItem.MeterTime = rdr.GetDateTime(pos).ToString("yyyy-MM-dd HH:mm");
                        pos = rdr.GetOrdinal("Meter_Glucose");
                        ggItem.MeterGlucose = rdr.GetInt32(pos);
                        
                        ggItem.Critical = "";
                        if (ggItem.MeterGlucose <= 39)
                            ggItem.Critical = "C";
                        if (ggItem.MeterGlucose >= 40 && ggItem.MeterGlucose <= 59)
                            ggItem.Critical = "L";
                        if (ggItem.MeterGlucose >= 250)
                            ggItem.Critical = "H";
                        list.Add(ggItem);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                }
                return list;
            }
        }

        internal static HashSet<string> GetCgmSkips()
        {
            var hash = new HashSet<string>();
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = ("GetDexcomSkips")
                    };
                    conn.Open();
                    var rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        hash.Add(rdr.GetString(0));
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                }
            }
            return hash;
        }

        
    }
}