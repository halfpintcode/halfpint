using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using hpMvc.Infrastructure.Logging;

namespace hpMvc.PostTestService
{
    

    public static class PostTestService
    {
        private static bool _bSendEmails = true;
        private static bool _bForceEmails;

        static readonly NLogger Nlogger = new NLogger();

        /// <summary>
        /// Runs the post tests service. Emails are sent to users once a week on Mondays
        /// </summary>
        /// <param name="sendEmails"></param>
        /// Default is true, set this to false to not send emails
        /// <param name="forceEmails"></param>
        /// Default is false - set to true to force emails
        /// 
        public static void Execute(bool sendEmails = true, bool forceEmails = false)
        {

            Nlogger.LogInfo("Starting PostTests Service - Internal");

            _bSendEmails = sendEmails;
            _bForceEmails = forceEmails;
            Nlogger.LogInfo("SendEmails:" + _bSendEmails + ", ForceEmails:" + _bForceEmails);
            
            //delete lists older than 7 days
            DeleteOldOperatorsLists();

            //get sites 
            var sites = GetSites();

            //iterate sites
            foreach (var si in sites.Where(si => si.EmpIdRequired))
            {
                //initialize email lists
                si.SiteEmailLists = new SiteEmailLists
                {
                    SiteId = si.Id,
                    NewStaffList = new List<PostTestNextDue>(),
                    ExpiredList = new List<PostTestNextDue>(),
                    DueList = new List<PostTestNextDue>(),
                    CompetencyMissingList = new List<PostTestNextDue>(),
                    EmailMissingList = new List<PostTestNextDue>(),
                    EmployeeIdMissingList = new List<PostTestNextDue>(),
                    StaffTestsNotCompletedList = new List<StaffTestsNotCompletedList>()
                };


            } //foreach (var si in sites.Where(si => si.EmpIdRequired))

        }

        private static void DeleteOldOperatorsLists()
        {
            var folderPath = ConfigurationManager.AppSettings["StatStripListPath"];
            var di = new DirectoryInfo(folderPath);

            var files = from f in di.GetFiles()
                        where f.LastWriteTime < DateTime.Now.AddDays(-7)
                        select f;
            files.ToList().ForEach(f => f.Delete());

        }

        private static IEnumerable<SiteInfoPts> GetSites()
        {
            var sil = new List<SiteInfoPts>();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn) { CommandType = System.Data.CommandType.StoredProcedure, CommandText = "GetSitesActive" };

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var si = new SiteInfoPts();
                        var pos = rdr.GetOrdinal("ID");
                        si.Id = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("Name");
                        si.Name = rdr.GetString(pos);
                        pos = rdr.GetOrdinal("SiteID");
                        si.SiteId = rdr.GetString(pos);
                        pos = rdr.GetOrdinal("EmpIDRequired");
                        si.EmpIdRequired = rdr.GetBoolean(pos);
                        sil.Add(si);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                }
            }
            return sil;
        }

        public static List<String> GetActiveRequiredTests(bool isSecondYear)
        {
            var list = new List<string>();
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = System.Data.CommandType.Text,
                        CommandText = "SELECT Name FROM PostTests WHERE Active=1 AND Required=1"
                    };

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var name = rdr.GetString(0);

                        if (isSecondYear)
                        {
                            if (name == "Overview")
                                continue;

                        }
                        list.Add(name);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return list;
        }
        
        
    }

    #region classes
    public class PostTestPts
    {
        public int PostTestCompletedId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string PathName { get; set; }
        public DateTime? DateCompleted { get; set; }
        public string SDateCompleted { get; set; }
        public bool IsExpired { get; set; }
        public bool IsDue { get; set; }
        public bool IsRequired { get; set; }
    }
    
    public class SiteInfoPts
    {
        public int Id { get; set; }
        public string SiteId { get; set; }
        public string Name { get; set; }
        public bool EmpIdRequired { get; set; }
        public SiteEmailLists SiteEmailLists { get; set; }
        public List<PostTestNextDue> PostTestNextDues { get; set; }
        //public List<PostTestNextDue> PostTestNextDues2 { get; set; } 
    }

    public class PostTestNextDue
    {
        public PostTestNextDue()
        {
            TestsNotCompleted = PostTestService.GetActiveRequiredTests(false);
            TestsCompleted = new List<PostTestPts>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? NextDueDate { get; set; }
        public string SNextDueDate { get; set; }
        public string Email { get; set; }
        public string EmployeeId { get; set; }
        public bool IsNovaStatStripTested { get; set; }
        public bool IsVampTested { get; set; }
        public string Role { get; set; }
        public bool IsNew { get; set; }
        public bool IsExpired { get; set; }
        public bool IsDue { get; set; }
        public bool IsOkForList { get; set; }
        public List<String> TestsNotCompleted { get; set; }
        public List<PostTestPts> TestsCompleted { get; set; }
    }

    public class NovaNetColumns
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Col3 { get; set; }
        public string Col4 { get; set; }
        public string Col5 { get; set; }
        public string EmployeeId { get; set; }
        public string Col7 { get; set; }
        public string Col8 { get; set; }
        public string Col9 { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool Found { get; set; }
    }

    public class StaffTestsNotCompletedList
    {
        public StaffTestsNotCompletedList()
        {
            TestsNotCompleted = PostTestService.GetActiveRequiredTests(false);
        }

        public int StaffId { get; set; }
        public string StaffName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public List<string> TestsNotCompleted { get; set; }
        public List<PostTestPts> TestsCompleted { get; set; }
    }

    public class SitePostTestDueList
    {
        public int SiteId { get; set; }
        public List<PostTestNextDue> PostTestNextDueList { get; set; }
    }

    public class SiteEmailLists
    {
        public int SiteId { get; set; }
        public List<PostTestNextDue> NewStaffList { get; set; }
        public List<PostTestNextDue> ExpiredList { get; set; }
        public List<PostTestNextDue> DueList { get; set; }
        public List<PostTestNextDue> CompetencyMissingList { get; set; }
        public List<PostTestNextDue> EmailMissingList { get; set; }
        public List<PostTestNextDue> EmployeeIdMissingList { get; set; }
        public List<StaffTestsNotCompletedList> StaffTestsNotCompletedList { get; set; }
    }

    public static class EmailBodies
    {
        public static string PostTestsExpiredStaff(List<string> testsNotCompleted, List<PostTestPts> testsCompleted)
        {
            var sb = new StringBuilder();
            sb.Append("<p>Hello. You are receiving this email because either at least one of the online tests you completed for the HALF-PINT study has expired.  Please go to the study website and take the required post-tests when you have time. Though you can review the training videos if you would like, you are only required to complete the post-tests (containing 3-5 multiple-choice questions each).</p>");

            sb.Append("<p>You will receive automatic weekly email reminders until you have completed these post-tests. You are currently locked out of the Nova study glucometer. </p>");

            sb.Append("<p>If you have any questions concerning this request, please contact the HALF-PINT Nurse Champion in your ICU, or the national study nurse, Kerry Coughlin-Wells (Kerry.Coughlin-Wells@childrens.harvard.edu). </p>");

            sb.Append("<p>Thank you for your assistance!</p>");

            sb.Append("<p>The HALF-PINT Study Team</p>");

            if (testsNotCompleted.Count > 0)
            {
                sb.Append("<br/><p><strong>Required Modules Not Completed<strong></p>");
                sb.Append("<ul>");
                foreach (var test in testsNotCompleted)
                {
                    sb.Append("<li>" + test + " </li>");
                }
                sb.Append("</ul>");
            }

            if (testsCompleted.Count > 0)
            {
                sb.Append("<br/><p><strong>Required Modules - Next Due Dates<strong></p>");
                sb.Append(
                    "<table style='border-collapse:collapse;' cellpadding='5' border='1'><tr style='background-color:87CEEB'><th>Test</th><th>Next Due Date</th></tr>");
                foreach (var postTest in testsCompleted)
                {
                    Debug.Assert(postTest.DateCompleted != null, "postTest.DateCompleted != null");
                    var nextDueDate = postTest.DateCompleted.Value.AddYears(1);
                    var tsDayWindow = nextDueDate - DateTime.Now;
                    if (tsDayWindow.Days <= 30)
                        sb.Append("<tr><td>" + postTest.Name + "</td><td><strong>" + nextDueDate.ToShortDateString() +
                                  "</strong></td></tr>");
                    else
                        sb.Append("<tr><td>" + postTest.Name + "</td><td>" + nextDueDate.ToShortDateString() +
                                  "</td></tr>");

                }
                sb.Append("</table>");
            }
            return sb.ToString();
        }

        public static string PostTestsDueStaff(List<string> testsNotCompleted, List<PostTestPts> testsCompleted, DateTime dueDate)
        {
            var sb = new StringBuilder();
            sb.Append("<p>Hello. You are receiving this email because at least one of the online tests you completed for the HALF-PINT study will expire soon.  Please go to the study website and take the required post-tests when you have time. Though you can review the training videos if you would like, you are only required to complete the post-tests (containing 3-5 multiple-choice questions each).</p>");

            sb.Append("<p>You will receive automatic weekly email reminders until you have completed these post-tests. If you are not able to take these tests prior to the due date <strong>" + dueDate.ToShortDateString() + "</strong>, you will be locked out of the Nova study glucometer. </p>");

            sb.Append("<p>If you have any questions concerning this request, please contact the HALF-PINT Nurse Champion in your ICU, or the national study nurse, Kerry Coughlin-Wells (Kerry.Coughlin-Wells@childrens.harvard.edu). </p>");

            sb.Append("<p>Thank you for your assistance!</p>");

            sb.Append("<p>The HALF-PINT Study Team</p>");

            if (testsNotCompleted.Count > 0)
            {
                sb.Append("<br/><p><strong>Required Modules Not Completed<strong></p>");
                sb.Append("<ul>");
                foreach (var test in testsNotCompleted)
                {
                    sb.Append("<li>" + test + " </li>");
                }
                sb.Append("</ul>");
            }

            if (testsCompleted.Count > 0)
            {
                sb.Append("<br/><p><strong>Required Modules - Next Due Dates<strong></p>");
                sb.Append(
                    "<table style='border-collapse:collapse;' cellpadding='5' border='1'><tr style='background-color:87CEEB'><th>Test</th><th>Next Due Date</th></tr>");
                foreach (var postTest in testsCompleted)
                {
                    Debug.Assert(postTest.DateCompleted != null, "postTest.DateCompleted != null");
                    var nextDueDate = postTest.DateCompleted.Value.AddYears(1);
                    var tsDayWindow = nextDueDate - DateTime.Now;
                    if (tsDayWindow.Days <= 30)
                        sb.Append("<tr><td>" + postTest.Name + "</td><td><strong>" + nextDueDate.ToShortDateString() +
                                  "</strong></td></tr>");
                    else
                        sb.Append("<tr><td>" + postTest.Name + "</td><td>" + nextDueDate.ToShortDateString() +
                                  "</td></tr>");

                }
                sb.Append("</table>");
            }
            return sb.ToString();
        }

        public static string PostTestsDueNewStaff(List<string> testsNotCompleted, List<PostTestPts> testsCompleted)
        {
            var sb = new StringBuilder();
            sb.Append("<p>Hello. You are receiving this email because you have completed HALF-PINT hands-on competencies but have not yet taken the online post-tests required for you to be able to care for a patient on the HALF-PINT Study. Please go to the study website and take the required post-tests when you have time. Please review the training video for each module, then complete the post-test (containing 3-5 multiple-choice questions each).</p>");

            sb.Append("<p>You will receive automatic weekly email reminders until you have completed these post-tests. You will be given access to the Nova study glucometer, and be able to care for patients on the study, once all your post-tests are complete.</p>");

            sb.Append("<p>If you have any questions concerning this request, please contact the HALF-PINT Nurse Champion in your ICU, or the national study nurse, Kerry Coughlin-Wells (Kerry.Coughlin-Wells@childrens.harvard.edu). </p>");

            sb.Append("<p>Thank you for your assistance!</p>");

            sb.Append("<p>The HALF-PINT Study Team</p>");

            if (testsNotCompleted.Count > 0)
            {
                sb.Append("<br/><p><strong>Required Modules Not Completed<strong></p>");
                sb.Append("<ul>");
                foreach (var test in testsNotCompleted)
                {
                    sb.Append("<li>" + test + " </li>");
                }
                sb.Append("</ul>");
            }

            if (testsCompleted.Count > 0)
            {
                sb.Append("<br/><p><strong>Required Module - Next Due Dates<strong></p>");
                sb.Append(
                    "<table style='border-collapse:collapse;' cellpadding='5' border='1'><tr style='background-color:87CEEB'><th>Test</th><th>Next Due Date</th></tr>");
                foreach (var postTest in testsCompleted)
                {
                    Debug.Assert(postTest.DateCompleted != null, "postTest.DateCompleted != null");
                    var nextDueDate = postTest.DateCompleted.Value.AddYears(1);
                    var tsDayWindow = nextDueDate - DateTime.Now;
                    if (tsDayWindow.Days <= 30)
                        sb.Append("<tr><td>" + postTest.Name + "</td><td><strong>" + nextDueDate.ToShortDateString() +
                                  "</strong></td></tr>");
                    else
                        sb.Append("<tr><td>" + postTest.Name + "</td><td>" + nextDueDate.ToShortDateString() +
                                  "</td></tr>");

                }
                sb.Append("</table>");
            }
            return sb.ToString();
        }
    }
    #endregion classes


}