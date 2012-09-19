using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using hpMvc.Infrastructure.Logging;
using System.Web.Security;
using hpMvc.Models;

namespace hpMvc.DataBase
{    

    public static class DbUtils
    {
        public static NLogger nlogger;
        static DbUtils()
        {
            nlogger = new NLogger();
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
                            int site = GetSiteidIDForUser(user);
                            if (!sites.Contains(site))
                                continue;
                        }
                        emails.Add((Membership.GetUser(user)).Email);
                    }
                }
            }
            else
            {
                if (sites != null)
                {
                    foreach (var site in sites)
                    {
                        var members = GetUsersForSite(site);
                        foreach (var member in members)
                            emails.Add(member.Email);
                    }
                }
            }

            return emails;
        }
            
        public static List<WebLog> GetWebLogs(int numRows=500)
        {

            var list = new List<WebLog>();
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("GetWebLogs");
                    SqlParameter param = new SqlParameter("@num", numRows);
                    cmd.Parameters.Add(param);
                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;

                    while (rdr.Read())
                    {
                        var log = new WebLog();
                        pos = rdr.GetOrdinal("logDate");
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
                    nlogger.LogError(ex);
                }
            }


            return list;
        }

        public static List<Randomization> GetAllRandomizedStudies()
        {
            var list = new List<Randomization>();
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("GetAllRandomizedStudies");
                    
                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;

                    while (rdr.Read())
                    {
                        var rndm = new Randomization();
                        pos = rdr.GetOrdinal("Name");
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
                    nlogger.LogError(ex);
                }
            }


            return list;
        }

        public static void SaveRandomizedSubjectActive(SubjectCompleted sc, string user)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("SaveRandomizedSubjectActive");
                    SqlParameter param = new SqlParameter("@id", sc.ID);
                    cmd.Parameters.Add(param);
                    if(sc.DateCompleted == null)
                        param = new SqlParameter("@dateCompleted", DBNull.Value);
                    else
                        param = new SqlParameter("@dateCompleted", sc.DateCompleted);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@cgmUpload", sc.CgmUpload);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@older2", sc.Older2);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@cbcl", sc.CBCL);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@pedsQL", sc.PedsQL);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@demographics", sc.Demographics);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@contactInfo", sc.ContactInfo);
                    cmd.Parameters.Add(param);
                    if(sc.NotCompletedReason == null)
                        param = new SqlParameter("@notCompletedReason", DBNull.Value);
                    else
                        param = new SqlParameter("@notCompletedReason", sc.NotCompletedReason);
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
                    nlogger.LogError(ex);
                }
            }


        }

        public static SubjectCompleted GetRandomizedStudyActive(string id)
        {
            var rndm = new SubjectCompleted();
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("GetRandomizedStudyActive");
                    SqlParameter param = new SqlParameter("@id", id);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;

                    while (rdr.Read())
                    {
                        
                        pos = rdr.GetOrdinal("ID");
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
                            rndm.sDateCompleted = rndm.DateCompleted !=null ? rndm.DateCompleted.Value.ToString("MM/dd/yyyy") : "";
                        }

                        pos = rdr.GetOrdinal("CgmUpload");
                        if (!rdr.IsDBNull(pos))
                            rndm.CgmUpload = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("Older2");
                        if (!rdr.IsDBNull(pos))
                            rndm.Older2 = rdr.GetBoolean(pos);

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

                        pos = rdr.GetOrdinal("MonitorID");
                        if (!rdr.IsDBNull(pos))
                            rndm.MonitorID = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("NotCompletedReason");
                        if (!rdr.IsDBNull(pos))
                            rndm.NotCompletedReason = rdr.GetString(pos);
                        else
                            rndm.NotCompletedReason = "";

                        pos = rdr.GetOrdinal("SiteName");
                        rndm.SiteName = rdr.GetString(pos);
                                                
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }


            return rndm;
        }

        public static List<SubjectCompleted> GetSiteRandomizedStudiesActive(int siteID)
        {
            var list = new List<SubjectCompleted>();
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("GetSiteRandomizedStudiesActive");
                    SqlParameter param = new SqlParameter("@siteID", siteID);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;

                    while (rdr.Read())
                    {
                        var rndm = new SubjectCompleted();
                        pos = rdr.GetOrdinal("ID");
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
                            rndm.sDateCompleted = rndm.DateCompleted != null ? rndm.DateCompleted.Value.ToString("MM/dd/yyyy") : ""; ;
                        }

                        pos = rdr.GetOrdinal("CgmUpload");
                        if (!rdr.IsDBNull(pos))
                            rndm.CgmUpload = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("Older2");
                        if (!rdr.IsDBNull(pos))
                            rndm.Older2 = rdr.GetBoolean(pos);

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

                        pos = rdr.GetOrdinal("MonitorID");
                        if (!rdr.IsDBNull(pos))    
                            rndm.MonitorID = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("NotCompletedReason");
                        if(!rdr.IsDBNull(pos))
                            rndm.NotCompletedReason = rdr.GetString(pos);

                        
                        list.Add(rndm);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }


            return list;
        }

        public static List<SubjectCompleted> GetSiteRandomizedStudiesCleared(int siteID)
        {
            var list = new List<SubjectCompleted>();
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("GetSiteRandomizedStudiesCleared");
                    SqlParameter param = new SqlParameter("@siteID", siteID);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;

                    while (rdr.Read())
                    {
                        var rndm = new SubjectCompleted();
                        pos = rdr.GetOrdinal("ID");
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
                            rndm.sDateCompleted = rndm.DateCompleted != null ? rndm.DateCompleted.Value.ToString("MM/dd/yyyy") : ""; ;
                        }

                        pos = rdr.GetOrdinal("CgmUpload");
                        if (!rdr.IsDBNull(pos))
                            rndm.CgmUpload = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("Older2");
                        if (!rdr.IsDBNull(pos))
                            rndm.Older2 = rdr.GetBoolean(pos);

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

                        pos = rdr.GetOrdinal("MonitorID");
                        if (!rdr.IsDBNull(pos))
                            rndm.MonitorID = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("NotCompletedReason");
                        if (!rdr.IsDBNull(pos))
                            rndm.NotCompletedReason = rdr.GetString(pos);


                        list.Add(rndm);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }


            return list;
        }

        public static List<Randomization> GetSiteRandomizedStudies(int siteID)
        {
            var list = new List<Randomization>();
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("GetSiteRandomizedStudies");
                    SqlParameter param = new SqlParameter("@siteID", siteID);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;

                    while (rdr.Read())
                    {
                        var rndm = new Randomization();
                        pos = rdr.GetOrdinal("Name");
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
                    nlogger.LogError(ex);
                }
            }


            return list;
        }

        public static List<StudyID> GetStudyIDsNotRandomized(int site)
        {
            List<StudyID> sls = new List<StudyID>();
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("GetStudyIDsNotRandomized");
                    SqlParameter param = new SqlParameter("@siteID", site);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;
                    
                    while (rdr.Read())
                    {
                        var stid = new StudyID();
                        pos = rdr.GetOrdinal("ID");
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
                    nlogger.LogError(ex);
                }
            }
            return sls;
        }

        public static MembershipUserCollection GetAllUsers()
        {
            //List<MembershipUser> users = new List<MembershipUser>();
            MembershipUserCollection mUsers = Membership.GetAllUsers();

            return mUsers;
        }

        public static List<UserInfo> GetAllUserInfo()
        {
            List<UserInfo> lUsers = new List<UserInfo>();
            MembershipUserCollection mUsers = Membership.GetAllUsers();

            foreach (MembershipUser user in mUsers)
            {
                var userInfo = new UserInfo();
                userInfo.UserName = user.UserName;
                userInfo.Email = user.Email;
                userInfo.IsLockedOut = user.IsLockedOut;
                userInfo.IsOnline = user.IsOnline;
                userInfo.Role = AccountUtils.GetRoleForUser(user.UserName);
                userInfo.Site = DbUtils.GetSiteNameForUser(user.UserName);
                lUsers.Add(userInfo);
            }
            return lUsers;
        }

        public static List<UserInfo> GetUserInfoForSite(int site)
        {
            List<UserInfo> lUsers = new List<UserInfo>();
            MembershipUser user = null;
            
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("GetSiteUsers");
                    SqlParameter param = new SqlParameter("@siteID", site);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;
                                        
                    while (rdr.Read())
                    {
                        var userInfo = new UserInfo();

                        pos = rdr.GetOrdinal("UserName");
                        userInfo.UserName = rdr.GetString(pos);
                        user = Membership.GetUser(userInfo.UserName);
                        userInfo.Email = user.Email;
                        userInfo.Role = AccountUtils.GetRoleForUser(userInfo.UserName);
                        userInfo.Site = GetSiteNameForUser(userInfo.UserName);
                        userInfo.IsLockedOut = user.IsLockedOut;
                        userInfo.IsOnline = user.IsOnline;
                        lUsers.Add(userInfo);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }
            return lUsers;
        }

        public static List<MembershipUser> GetUsersForSite(int site)
        {
            List<MembershipUser> users = new List<MembershipUser>();
            MembershipUser user = new MembershipUser("AspNetSqlMembershipProvider", "Select user", "", "", "", "",
                true, false, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now);
            users.Add(user);

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("GetSiteUsers");
                    SqlParameter param = new SqlParameter("@siteID", site);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;
                    string userName = "";

                    while (rdr.Read())
                    {
                        pos = rdr.GetOrdinal("UserName");
                        userName = rdr.GetString(pos);
                        user = Membership.GetUser(userName);
                        users.Add(user);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }
            return users;
        }

        public static List<MembershipUser> GetUserInRole(string role, int site)
        {
            var memUsers = new List<MembershipUser>();
            string[] users =  Roles.GetUsersInRole(role);

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("GetSiteUsers");
                    SqlParameter param = new SqlParameter("@siteID", site);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    
                    int pos = 0;
                    string userName = "";                    
                    while (rdr.Read())
                    {
                        pos = rdr.GetOrdinal("UserName");
                        userName = rdr.GetString(pos);
                        foreach (var u in users)
                        {
                            if (u == userName)
                            {
                                memUsers.Add(Membership.GetUser(u));                                
                            }                            
                        }                        
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }
            return memUsers;
        }

        public static int IsStudyIDRandomized(string studyID)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("Test error");
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "IsStudyIDRandomized";
                    SqlParameter param = new SqlParameter("@studyID", studyID);
                    cmd.Parameters.Add(param);
                    
                    conn.Open();
                    int count = (Int32)cmd.ExecuteScalar();
                    if (count == 1)
                        return 1;
                    return 0;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return -1;
                }
            }            
        }

        public static int IsStudyIDAssignedPasswordValid(string studyID, string animalName)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {                    
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "IsStudyIDAssignedPasswordValid";
                    SqlParameter param = new SqlParameter("@studyID", studyID);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@animalName", animalName);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    int count = (Int32)cmd.ExecuteScalar();
                    if (count == 1)
                        return 1;
                    return 0;
                }
                catch (Exception ex)
                { 
                    nlogger.LogError(ex);
                    return -1;
                }
            }                        
        }

        public static int IsStudyIDAssignedPassword(string studyID)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "IsStudyIDAssignedPassword";
                    SqlParameter param = new SqlParameter("@studyID", studyID);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    int count = (Int32)cmd.ExecuteScalar();
                    if (count == 1)
                        return 1;
                    return 0;        
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return -1;
                }
            }            
        }

        public static int IsStudyIDValid(string studyID)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("test error");
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "DoesStudyIDExist";
                    SqlParameter param = new SqlParameter("@studyID", studyID);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    int count = (Int32)cmd.ExecuteScalar();
                    if (count == 1)
                        return 1;
                    return 0;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex, "StudyID:" + studyID);
                    return -1;
                }
            }
        }

        public static int IsStudyIDCleared(string studyID)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("test error");
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "IsStudyIDCleared";
                    SqlParameter param = new SqlParameter("@studyID", studyID);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    int count = (Int32)cmd.ExecuteScalar();
                    if (count == 1)
                        return 1;
                    return 0;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex, "StudyID:" + studyID);
                    return -1;
                }
            }
        }

        public static string GetInitializePassword(string studyID)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "GetRandomizationPassword";
                    SqlParameter param = new SqlParameter("@studyID", studyID);
                    cmd.Parameters.Add(param);
                    
                    conn.Open();
                    return cmd.ExecuteScalar().ToString();                   

                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex, "StudyID:" + studyID);
                    return "error";                   
                }
            }            
        }
        
        public static int AddRandomizationPassword(string studyID, int animalID, string consentDate, string consentTime)
        {            
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "AddRandomizationPassword";
                    
                    SqlParameter param = new SqlParameter("@animalID", animalID);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@studyID", studyID);
                    cmd.Parameters.Add(param);

                    if (consentDate.Length == 0)
                        param = new SqlParameter("@consentDate", DBNull.Value);
                    else
                        param = new SqlParameter("@consentDate", DateTime.Parse(consentDate));
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@consentTime", consentTime);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return 1;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return -1;
                }
            } 
        }

        public static string GetRandomPassword()
        {
            string password = "";
            Random rnd = new Random();
            int rndID = rnd.Next(1, 18);

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "GetResetPasswordByID";
                    SqlParameter param = new SqlParameter("@id", rndID);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();

                    int pos = 0;
                    while (rdr.Read())
                    {
                        pos = rdr.GetOrdinal("Name");
                        password = rdr.GetString(pos);
                    }
                    rdr.Close();
                    return password;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return null;
                }
            }
            
        }

        public static Animal GetRandomAnimal()
        {
            Animal animal = null;
            Random rnd = new Random();
            int rndID = rnd.Next(1, 131);
            
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            { 
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "GetAnimalByID";
                    SqlParameter param = new SqlParameter("@id", rndID);
                    cmd.Parameters.Add(param);
                        
                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();

                    animal = new Animal();
                    animal.ID = rndID;
                    int pos = 0;
                    while (rdr.Read())
                    {
                        pos = rdr.GetOrdinal("Name");
                        animal.Name = rdr.GetString(pos);
                    }
                    rdr.Close();
                    return animal;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return null;
                }
            }
        }

        public static List<IDandStudyID> GetRandomizedStudiesForSite(int siteID)
        {
            var studs = new List<IDandStudyID>();
            IDandStudyID stud = null;

             String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
             using (SqlConnection conn = new SqlConnection(strConn))
             {
                 try
                 {
                     SqlCommand cmd = new SqlCommand("", conn);
                     cmd.CommandType = System.Data.CommandType.StoredProcedure;
                     cmd.CommandText = "GetRandomizedStudiesForSite";

                     SqlParameter param = new SqlParameter("@siteID", siteID);
                     cmd.Parameters.Add(param);

                     conn.Open();
                     SqlDataReader rdr = cmd.ExecuteReader();
                     int pos = 0;
                     while (rdr.Read())
                     {
                         stud = new IDandStudyID();

                         pos = rdr.GetOrdinal("ID");
                         stud.ID = rdr.GetInt32(pos);

                         pos = rdr.GetOrdinal("StudyID");
                         stud.StudyID = rdr.GetString(pos);

                         studs.Add(stud);
                     }
                     rdr.Close();
                 }
                 catch (Exception ex)
                 {
                     nlogger.LogError(ex);
                 }
                 return studs;
             }
        }
        
        public static List<Animal> GetAnimals()
        {            
            List<Animal> animals = new List<Animal>();
            Animal animal = null;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "GetAnimals";

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;
                    while (rdr.Read())
                    {
                        animal = new Animal();

                        pos = rdr.GetOrdinal("ID");
                        animal.ID = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("Name");
                        animal.Name = rdr.GetString(pos);
                        animals.Add(animal);
                    }
                    rdr.Close();
                    
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }

            return animals;
        }

        public static List<InsulinConcentration> GetInsulinConcItems(int siteID)
        {
            var items = new List<InsulinConcentration>();
            InsulinConcentration item = null;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("GetSiteInsulicConcentrations");
                    SqlParameter param = new SqlParameter("@siteID", siteID);
                    cmd.Parameters.Add(param);
                    conn.Open();

                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;
                    while (rdr.Read())
                    {
                        item = new InsulinConcentration();

                        pos = rdr.GetOrdinal("Concentration");
                        item.Concentration = rdr.GetDouble(pos).ToString("0.0#");
                        pos = rdr.GetOrdinal("Name");
                        item.Name = rdr.GetString(pos);
                        items.Add(item);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }
            return items;
        }
        
        public static List<IDandName> GetLookupItems(string itemsName)
        {            
            List<IDandName> items = new List<IDandName>();
            IDandName item = null;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("Get" + itemsName);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;
                    while (rdr.Read())
                    {
                        item = new IDandName();

                        pos = rdr.GetOrdinal("ID");
                        item.ID = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("Name");
                        item.Name = rdr.GetString(pos);
                        items.Add(item);
                    }
                    rdr.Close();                    
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }
            return items;
        }

        public static List<IDandName> GetStaffLookupForSite(string siteID)
        {
            List<IDandName> items = new List<IDandName>();
            IDandName item = null;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("GetStaffLookupForSite");
                    SqlParameter param = new SqlParameter("@siteID", siteID);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;
                    while (rdr.Read())
                    {
                        item = new IDandName();

                        pos = rdr.GetOrdinal("ID");
                        item.ID = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("UserName");
                        item.Name = rdr.GetString(pos);
                        items.Add(item);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }
            return items;
        }

        public static Dictionary<string,string> GetInitializeStudyIDsWithPassword(int siteID)
        {
            var dict = new Dictionary<string, string>();
            
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("Opps");
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "GetInitializeStudyIDsWithPassword";
                    SqlParameter param = new SqlParameter("@siteID",siteID);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;

                    string studyid = "";
                    string animal = "";
                    while (rdr.Read())
                    {
                        pos = rdr.GetOrdinal("StudyID");
                        studyid = rdr.GetString(pos);
                        pos = rdr.GetOrdinal("Name");
                        animal = rdr.GetString(pos);
                        dict.Add(studyid, animal);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }


            return dict;
        }

        public static List<Site> GetSites()
        {
            List<Site> sites = new List<Site>();
            Site site = null;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("Opps");
                    SqlCommand cmd = new SqlCommand("",conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText="GetSites";

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;
                    while (rdr.Read())
                    {
                        site = new Site();

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
                    nlogger.LogError(ex);
                }
            }

            return sites;
        }
        
        public static int RemoveUser (string userName)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("Opps");
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "DeleteUserSite";

                    SqlParameter param = new SqlParameter("@userName", userName);
                    cmd.Parameters.Add(param);
                    
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return -1;
                }
            }
            if(!Membership.DeleteUser(userName))
                return 0;
            return 1;
        }
        
        public static bool AddUserSite (string userName, int siteID)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("oops");
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "AddUserSite";
                    
                    SqlParameter param = new SqlParameter("@userName", userName);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@siteID", siteID);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return false;
                }
            }            
        }

        public static string GetSiteIDForUser(string userName)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            string retVal = "";
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "GetSiteInfoForUser";

                    SqlParameter param = new SqlParameter("@userName", userName);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    int pos = 0;
                    SqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        pos = rdr.GetOrdinal("SiteID");
                        retVal = rdr.GetString(pos);                       
                    }
                    rdr.Close();
                    return retVal;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return "error";
                }
            }

        }
        
        public static string GetSiteIDandNameForUser(string userName)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            string retVal = "";
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "GetSiteInfoForUser";

                    SqlParameter param = new SqlParameter("@userName", userName);
                    cmd.Parameters.Add(param);

                    conn.Open();                    
                    int pos = 0;
                    SqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        pos = rdr.GetOrdinal("SiteID");
                        retVal = rdr.GetString(pos) + ":";
                        pos = rdr.GetOrdinal("Name");
                        retVal = retVal + rdr.GetString(pos);                        
                    }
                    rdr.Close();
                    return retVal;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return "error";
                }
            }

        }

        public static string GetSiteNameForUser(string userName)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            string retVal = "";
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "GetSiteInfoForUser";

                    SqlParameter param = new SqlParameter("@userName", userName);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    int pos = 0;
                    SqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {                        
                        pos = rdr.GetOrdinal("Name");
                        retVal = rdr.GetString(pos);
                    }
                    rdr.Close();
                    return retVal;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return "error";
                }
            }
            
        }

        public static DTO GetSiteCodeForSiteID(int siteID)
        {
            DTO dto = new DTO();
            dto.ReturnValue = 1;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "GetSiteCodeBySiteID";

                    SqlParameter param = new SqlParameter("@siteID", siteID);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    int pos = 0;
                    SqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        pos = rdr.GetOrdinal("SiteID");
                        dto.Bag = rdr.GetString(pos);
                    }
                    rdr.Close();
                    return dto;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    dto.Message = "Error getting the site code.  This error has been reported to the administrator.";
                    dto.ReturnValue = -1;
                    return dto;
                }
            }

        }

        public static int GetSiteidIDForUser(string userName)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            int iretVal = 0;
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "GetSiteInfoForUser";

                    SqlParameter param = new SqlParameter("@userName", userName);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    int pos = 0;
                    SqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        pos = rdr.GetOrdinal("ID");
                        iretVal = rdr.GetInt32(pos);
                    }
                    rdr.Close();
                    return iretVal;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return -1;
                }
            }

        }

        public static SiteInfo GetSiteInfoForUser(string userName)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            var si = new SiteInfo();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "GetSiteInfoForUser";

                    SqlParameter param = new SqlParameter("@userName", userName);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    int pos = 0;
                    SqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        pos = rdr.GetOrdinal("ID");
                        si.ID = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("Name");
                        si.Name = rdr.GetString(pos);
                        pos = rdr.GetOrdinal("SiteID");
                        si.SiteID = rdr.GetString(pos);
                    }
                    rdr.Close();
                    return si;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return si;
                }
            }

        }

        public static MessageListDTO AddStaff(StaffModel model)
        {
            MessageListDTO dto = new MessageListDTO();
            
            // Attempt to add the user to the membership db
            string password = DbUtils.GetRandomPassword();            
            MembershipCreateStatus createStatus;
            MembershipUser user = Membership.CreateUser(model.UserName, password, model.Email, null, null, true, null, out createStatus);

            if (createStatus == MembershipCreateStatus.Success)
            {
                //FormsAuthentication.SetAuthCookie(model.UserName, false /* createPersistentCookie */);                    
                if (!DbUtils.AddUserSite(model.UserName, model.SiteID))
                {
                    dto.Messages.Add("There was an error adding the user and site to the database");
                    dto.IsSuccessful = false;
                    return dto;
                }

                //this will tell us that user needs to reset
                user.Comment = "Reset";
                Membership.UpdateUser(user);                                
                nlogger.LogInfo("AddUser - userName: " + model.UserName + ", site: " + model.SiteID.ToString() + ", password: " + password);                
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
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "AddStaff";
                                        
                    SqlParameter param = new SqlParameter("@siteID", model.SiteID);
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
                    
                    if(model.EmployeeID == null)
                        param = new SqlParameter("@employeeID", DBNull.Value);
                    else
                        param = new SqlParameter("@employeeID", model.EmployeeID);
                    cmd.Parameters.Add(param);
                    
                    param = new SqlParameter("@phone", model.Phone);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@novaStatStrip", model.NovaStatStrip);
                    cmd.Parameters.Add(param);
                    if(model.NovaStatStrip)
                        param = new SqlParameter("@novaStatStripDoc", model.NovaStatStripDoc);
                    else
                        param = new SqlParameter("@novaStatStripDoc", DBNull.Value);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@vamp", model.Vamp);
                    cmd.Parameters.Add(param);
                    if(model.Vamp)
                        param = new SqlParameter("@vampDoc", model.VampDoc);
                    else
                        param = new SqlParameter("@vampDoc", DBNull.Value);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@cgm", model.Cgm);
                    cmd.Parameters.Add(param);
                    if(model.Cgm)
                        param = new SqlParameter("@cgmDoc", model.CgmDoc);
                    else
                        param = new SqlParameter("@cgmDoc", DBNull.Value);
                    cmd.Parameters.Add(param);
                    
                    param = new SqlParameter("@inform", model.Inform);
                    cmd.Parameters.Add(param);
                    if(model.Inform)                                       
                        param = new SqlParameter("@informDoc", model.InformDoc);
                    else
                        param = new SqlParameter("@informDoc", DBNull.Value);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@onCall", model.OnCall);
                    cmd.Parameters.Add(param);
                    if(model.OnCall)                    
                        param = new SqlParameter("@onCallDoc", model.OnCallDoc);                                            
                    else
                        param = new SqlParameter("@onCallDoc", DBNull.Value);                        
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
                    nlogger.LogError(ex);
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
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

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
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion

        public static StaffEditModel GetStaffInfo(int userID)
        {
            StaffEditModel model = new StaffEditModel();
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "GetStaffInfo";

                    SqlParameter param = new SqlParameter("@id", userID);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    int pos = 0;
                    SqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        pos = rdr.GetOrdinal("ID");
                        model.ID = rdr.GetInt32(pos);
                        
                        pos = rdr.GetOrdinal("Active");
                        model.Active = rdr.GetBoolean(pos);
                        
                        pos = rdr.GetOrdinal("SiteID");
                        model.SiteID = rdr.GetInt32(pos);
                        
                        pos = rdr.GetOrdinal("Role");
                        if(!rdr.IsDBNull(pos))
                            model.Role = rdr.GetString(pos);
                        
                        pos = rdr.GetOrdinal("UserName");
                        if(!rdr.IsDBNull(pos))
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
                    return model;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return null;
                }
            }
            
        }

        public static MessageListDTO UpdateStaff(StaffEditModel model)
        {
            MessageListDTO dto = new MessageListDTO();
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "UpdateStaff";

                    SqlParameter param = new SqlParameter("@id", model.ID);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@active", model.Active);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@firstName", model.FirstName);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@lastName", model.LastName);
                    cmd.Parameters.Add(param);
                                        
                    if (model.EmployeeID == null)
                        param = new SqlParameter("@employeeID", DBNull.Value);
                    else
                        param = new SqlParameter("@employeeID", model.EmployeeID);
                    cmd.Parameters.Add(param);

                    if(model.Phone == null)
                        param = new SqlParameter("@phone", DBNull.Value);
                    else
                        param = new SqlParameter("@phone", model.Phone);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@novaStatStrip", model.NovaStatStrip);
                    cmd.Parameters.Add(param);
                    if (model.NovaStatStrip)
                        param = new SqlParameter("@novaStatStripDoc", model.NovaStatStripDoc);
                    else
                        param = new SqlParameter("@novaStatStripDoc", DBNull.Value);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@vamp", model.Vamp);
                    cmd.Parameters.Add(param);
                    if (model.Vamp)
                        param = new SqlParameter("@vampDoc", model.VampDoc);
                    else
                        param = new SqlParameter("@vampDoc", DBNull.Value);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@cgm", model.Cgm);
                    cmd.Parameters.Add(param);
                    if (model.Cgm)
                        param = new SqlParameter("@cgmDoc", model.CgmDoc);
                    else
                        param = new SqlParameter("@cgmDoc", DBNull.Value);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@inform", model.Inform);
                    cmd.Parameters.Add(param);
                    if (model.Inform)
                        param = new SqlParameter("@informDoc", model.InformDoc);
                    else
                        param = new SqlParameter("@informDoc", DBNull.Value);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@onCall", model.OnCall);
                    cmd.Parameters.Add(param);
                    if (model.OnCall)
                        param = new SqlParameter("@onCallDoc", model.OnCallDoc);
                    else
                        param = new SqlParameter("@onCallDoc", DBNull.Value);
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
                    nlogger.LogError(ex);
                    dto.IsSuccessful = false;
                    dto.Messages.Add("There was an error adding the new staff info into the staff database");
                    return dto;
                }
            }
            dto.Messages.Add("New staff information was successfully entered into the database!");
            dto.IsSuccessful = true;
            return dto;
        }
    }
    
}