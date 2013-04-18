using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Security;
using hpMvc.Infrastructure.Logging;

namespace hpMvc.PostTestService
{
    

    public static class PostTestService
    {
        private static bool _bSendEmails = true;
        private static bool _bForceEmails;
        private static HttpServerUtilityBase _server;

        static readonly NLogger Nlogger = new NLogger();

        /// <summary>
        /// Runs the post tests service. Emails are sent to users once a week on Mondays
        /// </summary>
        /// <param name="server"></param>
        /// <param name="runParam"></param>
        /// <param name="site"></param>
        /// <param name="isReport"></param>
        /// runParam is one of three possible params: noEmails (sets _bSendEmails to false),  forceEmails (sets _bForceEmails to true), or coordinatorEmais (leaves everthing as default)
        public static SiteInfoPts Execute(HttpServerUtilityBase server, string runParam, int site = 0, bool isReport = false)
        {

            Nlogger.LogInfo("Starting PostTests Service - Internal");

            _server = server;
            switch (runParam)
            {
                case "noEmails":
                    _bSendEmails = false;
                    break;
                case "forceEmails":
                    _bForceEmails = true;
                    break;
            }
            
            Nlogger.LogInfo("SendEmails:" + _bSendEmails + ", ForceEmails:" + _bForceEmails);
            
            //get sites 
            var sites = GetSites();

            //iterate sites
            foreach (var si in sites.Where(si => si.EmpIdRequired))
            {
                if(site == 0)
                {}
                else
                {
                    if (si.Id != site)
                        continue;
                }

                //Console.WriteLine(si.Name);
                Nlogger.LogInfo("For Site:" + si.Name + " - " + si.SiteId);

                //delete lists older than 7 days
                DeleteOldOperatorsLists(si.SiteId);

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

                
                //Get staff info including next due date, tests not completed, is new staff - next due date will be 1 year from today for new staff
                //staff roles not included are Admin, DCC , Nurse generic (nurse accounts with a user name)
                si.PostTestNextDues = GetStaffPostTestsCompletedInfo(si.Id);

                //iterate people                
                foreach (var postTestNextDue in si.PostTestNextDues)
                {
                    //creat the StaffTestsNotCompletedList email list to coordinators
                    var stnc = new StaffTestsNotCompletedList
                    {
                        StaffId = postTestNextDue.Id,
                        StaffName = postTestNextDue.Name,
                        Role = postTestNextDue.Role,
                        TestsNotCompleted = postTestNextDue.TestsNotCompleted,
                        TestsCompleted = postTestNextDue.TestsCompleted
                    };
                    
                    si.SiteEmailLists.StaffTestsNotCompletedList.Add(stnc);

                    var bContinue = false;
                    //Console.WriteLine(postTestNextDue.Name + ", email: " + postTestNextDue.Email + ", Employee ID: " + postTestNextDue.EmployeeId + ", Role: " + postTestNextDue.Role);
                    //Nlogger.LogInfo("For staff member:" + postTestNextDue.Name + ", email: " + postTestNextDue.Email + ", Employee ID: " + postTestNextDue.EmployeeId + ", Role: " + postTestNextDue.Role);

                    if (postTestNextDue.Role != "Nurse")
                    {
                        //make sure they are nova net certified
                        if (!postTestNextDue.IsNovaStatStripTested)
                        {
                            //Nlogger.LogInfo("NovaStatStrip competency needed for " + postTestNextDue.Name);
                            si.SiteEmailLists.CompetencyMissingList.Add(postTestNextDue);
                            bContinue = true;
                        }
                    }
                    else
                    {
                        //make sure they are nova net and vamp certified
                        if ((!postTestNextDue.IsNovaStatStripTested) || (!postTestNextDue.IsVampTested))
                        {
                            //Nlogger.LogInfo("Competency needed for " + postTestNextDue.Name);
                            si.SiteEmailLists.CompetencyMissingList.Add(postTestNextDue);
                            bContinue = true;
                        }
                    }

                    if (string.IsNullOrEmpty(postTestNextDue.Email))
                    {
                        //Nlogger.LogInfo("Email missing for " + postTestNextDue.Name);
                        si.SiteEmailLists.EmailMissingList.Add(postTestNextDue);
                        bContinue = true;
                    }

                    stnc.Email = postTestNextDue.Email;

                    if (string.IsNullOrEmpty(postTestNextDue.EmployeeId))
                    {
                        //Nlogger.LogInfo("Employee ID missing for " + postTestNextDue.Name);
                        si.SiteEmailLists.EmployeeIdMissingList.Add(postTestNextDue);
                        bContinue = true;
                    }

                    if (bContinue)
                        continue;

                    string subject;
                    string body;
                    string[] to;
                    var bTempIncludOnList = false;

                    //see if all required post tests are completed
                    if (postTestNextDue.TestsNotCompleted.Count > 0)
                    {
                        //todo - this is temporary - you can get rid of this after 9/1/13
                        if (postTestNextDue.TestsNotCompleted.Count == 1)
                        {
                            if (DateTime.Today.CompareTo(new DateTime(2013, 9, 1)) < 0)
                            {
                                if (postTestNextDue.TestsNotCompleted[0] == "Dexcom G4 Receiver")
                                {
                                    bTempIncludOnList = true;
                                }
                            }
                        }

                        if (!bTempIncludOnList)
                        {
                            if (postTestNextDue.IsNew)
                            {
                                si.SiteEmailLists.NewStaffList.Add(postTestNextDue);
                                //send new user email
                                body = EmailBodies.PostTestsDueNewStaff(postTestNextDue.TestsNotCompleted,
                                                                        postTestNextDue.TestsCompleted);
                                to = new[] { postTestNextDue.Email };

                                subject =
                                    string.Format(
                                        "Please Read: Please Complete the Online HALF-PINT Post-Tests - site:{0}",
                                        si.Name);

                                if (_bForceEmails)
                                {
                                    SendHtmlEmail(subject, to, null, body, 
                                                  @"<a href='http://halfpintstudy.org/hpProd/PostTests/Initialize'>Halfpint Study Post Tests</a>");
                                }
                                else
                                {
                                    if (!isReport)
                                    {
                                        if (DateTime.Today.DayOfWeek == DayOfWeek.Monday)
                                        {
                                            if (_bSendEmails)
                                                SendHtmlEmail(subject, to, null, body,
                                                              @"<a href='http://halfpintstudy.org/hpProd/PostTests/Initialize'>Halfpint Study Post Tests</a>");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                si.SiteEmailLists.ExpiredList.Add(postTestNextDue);
                                //send new user email
                                body = EmailBodies.PostTestsExpiredStaff(postTestNextDue.TestsNotCompleted,
                                                                         postTestNextDue.TestsCompleted);
                                to = new[] { postTestNextDue.Email };

                                subject = "Please Read: Your HALF-PINT Training Has Expired - site:" + si.Name;

                                if (_bForceEmails)
                                {
                                    SendHtmlEmail(subject, to, null, body, 
                                        @"<a href='http://halfpintstudy.org/hpProd/PostTests/Initialize'>Halfpint Study Post Tests</a>");
                                }
                                else
                                {
                                    if (!isReport)
                                    {
                                        if (DateTime.Today.DayOfWeek == DayOfWeek.Monday)
                                        {
                                            if (_bSendEmails)
                                                SendHtmlEmail(subject, to, null, body,
                                                              @"<a href='http://halfpintstudy.org/hpProd/PostTests/Initialize'>Halfpint Study Post Tests</a>");
                                        }
                                    }
                                }
                            }
                        }
                        if (!bTempIncludOnList)
                            continue;
                    }//if (postTestNextDue.TestsNotCompleted.Count > 0)

                    //else all tests are completed
                    {
                        postTestNextDue.IsOkForList = true;

                        if (postTestNextDue.IsDue)
                        {
                            //add to list - to be sent to coordinator
                            si.SiteEmailLists.DueList.Add(postTestNextDue);
                            var minPtnd = postTestNextDue.TestsCompleted.Min(x => x.DateCompleted);

                            body = EmailBodies.PostTestsDueStaff(postTestNextDue.TestsNotCompleted, postTestNextDue.TestsCompleted, minPtnd.Value.AddYears(1));
                            to = new[] { postTestNextDue.Email };

                            subject = "Please Read: Your HALF-PINT Training is About to Expire - site:" + si.Name;

                            if (_bForceEmails)
                            {
                                SendHtmlEmail(subject, to, null, body, 
                                    @"<a href='http://halfpintstudy.org/hpProd/PostTests/Initialize'>Halfpint Study Post Tests</a>");
                            }
                            else
                            {
                                if (!isReport)
                                {
                                    if (DateTime.Today.DayOfWeek == DayOfWeek.Monday)
                                    {
                                        if (_bSendEmails)
                                            SendHtmlEmail(subject, to, null, body,
                                                          @"<a href='http://halfpintstudy.org/hpProd/PostTests/Initialize'>Halfpint Study Post Tests</a>");
                                    }
                                }
                            }
                        }

                        if (postTestNextDue.IsExpired)
                        {
                            si.SiteEmailLists.ExpiredList.Add(postTestNextDue);
                            //send new user email
                            body = EmailBodies.PostTestsExpiredStaff(postTestNextDue.TestsNotCompleted,
                                                                     postTestNextDue.TestsCompleted);
                            to = new[] { postTestNextDue.Email };

                            subject = "Please Read: Your HALF-PINT Training Has Expired - site:" + si.Name;

                            if (_bForceEmails)
                            {
                                SendHtmlEmail(subject, to, null, body, 
                                    @"<a href='http://halfpintstudy.org/hpProd/PostTests/Initialize'>Halfpint Study Post Tests</a>");
                            }
                            else
                            {
                                if (!isReport)
                                {
                                    if (DateTime.Today.DayOfWeek == DayOfWeek.Monday)
                                    {
                                        if (_bSendEmails)
                                            SendHtmlEmail(subject, to, null, body,
                                                          @"<a href='http://halfpintstudy.org/hpProd/PostTests/Initialize'>Halfpint Study Post Tests</a>");
                                    }
                                }
                            }
                        }
                    }//else all tests are completed
                    
                }//foreach (var postTestNextDue in si.PostTestNextDues)

            } //foreach (var si in sites.Where(si => si.EmpIdRequired))
            
            //only write to files if 
            if (site == 0)
            {
                //create the nova net files
                Nlogger.LogInfo("***Creating csv files***");
                foreach (var si in sites.Where(si => si.EmpIdRequired))
                {
                    Nlogger.LogInfo("For Site:" + si.Name + " - " + si.SiteId);

                    //create the new list
                    var lines = new List<NovaNetColumns>();

                    //iterate qualified staff
                    foreach (var ptnd in si.PostTestNextDues.Where(ptnd => ptnd.IsOkForList))
                    {
                        var nnc = new NovaNetColumns();
                        var sep = new[] {','};
                        var names = ptnd.Name.Split(sep);
                        nnc.LastName = names[0];
                        nnc.FirstName = names[1];
                        nnc.Col3 = "ALL";
                        nnc.Col4 = "ALL";
                        nnc.Col5 = "StatStrip";
                        nnc.EmployeeId = ptnd.EmployeeId;
                        nnc.Col7 = "T";
                        nnc.Col8 = "O";
                        nnc.Col9 = "Glucose";

                        var startDate = DateTime.Now.AddMonths(-12);

                        nnc.StartDate = startDate.ToString("M/d/yyyy");
                        nnc.EndDate = ptnd.NextDueDate.Value.ToString("M/d/yyyy");
                        lines.Add(nnc);

                    } //foreach (var ptnd in si.PostTestNextDues.Where(ptnd => !ptnd.IsOkForList))
                    //write lines to new file
                    WriteNovaNetFile(lines, si.Name, si.SiteId);
                    Nlogger.LogInfo("WriteNovaNetFile:" + si.Name);

                } //foreach (var si in siteInfoPtses.Where(si => !si.EmpIdRequired))
            }

            if (_bSendEmails)
            {
                Nlogger.LogInfo("***Sending coorinator emails***");
                foreach (var si in sites.Where(si => si.EmpIdRequired))
                {
                    if (site == 0)
                    { }
                    else
                    {
                        if (si.Id != site)
                            continue;
                    }

                    Nlogger.LogInfo("SendCoordinatorsEmail:" + si.Name);
                    if (isReport)
                    {
                        
                    }
                    else
                    {
                        SendCoordinatorsEmail(si.Id, si.Name, si.SiteEmailLists);    
                    }
                    
                }
            }
            return sites.Find(s => s.Id == site);
             
        }


        internal static void SendCoordinatorsEmail(int site, string siteName, SiteEmailLists siteEmailLists)
        {
            var coordinators = GetUserInRole("Coordinator", site);
            var sbBody = new StringBuilder("");
            const string newLine = "<br/>";

            sbBody.Append(newLine);

            if (siteEmailLists.CompetencyMissingList.Count == 0)
                sbBody.Append("<h3>All staff members have completed threir competency tests.</h3>");
            else
            {
                var competencyMissingSortedList = siteEmailLists.CompetencyMissingList.OrderBy(x => x.Name).ToList();
                sbBody.Append("<h3>The following staff members have not completed a competency test.</h3>");

                sbBody.Append("<table style='border-collapse:collapse;' cellpadding='5' border='1'><tr style='background-color:87CEEB'><th>Name</th><th>Role</th><th>Tests Not Completed</th><th>Email</th></tr>");
                foreach (var ptnd in competencyMissingSortedList)
                {
                    var email = "not entered";
                    if (ptnd.Email != null)
                        email = ptnd.Email;

                    var test = "";
                    if (!ptnd.IsNovaStatStripTested)
                        test = "NovaStatStrip ";
                    if (ptnd.Role == "Nurse")
                    {
                        if (!ptnd.IsVampTested)
                        {
                            if (test.Length > 0)
                                test += " and ";
                            test += "Vamp Jr";
                        }
                    }
                    sbBody.Append("<tr><td>" + ptnd.Name + "</td><td>" + ptnd.Role + "</td><td>" + test + "</td><td>" + email + "</td></tr>");
                }
                sbBody.Append("</table>");
            }

            if (siteEmailLists.EmailMissingList.Count > 0)
            {
                var emailMissingSortedList = siteEmailLists.EmailMissingList.OrderBy(x => x.Name).ToList();
                sbBody.Append("<h3>The following staff members need to have their email address entered into the staff table.</h3>");

                sbBody.Append("<div><table style='border-collapse:collapse;' cellpadding='5' border='1'><tr style='background-color:87CEEB'><th>Name</th><th>Role</th></tr>");
                foreach (var ptnd in emailMissingSortedList)
                {
                    sbBody.Append("<tr><td>" + ptnd.Name + "</td><td>" + ptnd.Role + "</td></tr>");
                }
                sbBody.Append("</table></div>");
            }

            if (siteEmailLists.EmployeeIdMissingList.Count > 0)
            {
                var employeeIdMissingSortedList = siteEmailLists.EmployeeIdMissingList.OrderBy(x => x.Name).ToList();
                sbBody.Append("<h3>The following staff members need to have their employee ID entered into the staff table.</h3>");

                sbBody.Append("<div><table style='border-collapse:collapse;' cellpadding='5' border='1'><tr style='background-color:87CEEB'><th>Name</th><th>Role</th><th>Email</th></tr>");
                foreach (var ptnd in employeeIdMissingSortedList)
                {
                    var email = "not entered";
                    if (ptnd.Email != null)
                        email = ptnd.Email;
                    sbBody.Append("<tr><td>" + ptnd.Name + "</td><td>" + ptnd.Role + "</td><td>" + email + "</td></tr>");
                }
                sbBody.Append("</table></div>");
            }

            if (siteEmailLists.NewStaffList.Count == 0)
            { }
            else
            {
                var newSortedList = siteEmailLists.NewStaffList.OrderBy(x => x.Name).ToList();
                sbBody.Append("<h3>The following new staff members have not completed their annual post tests.</h3>");

                sbBody.Append("<table style='border-collapse:collapse;' cellpadding='5' border='1'><tr style='background-color:87CEEB'><th>Name</th><th>Role</th><th>Email</th></tr>");
                foreach (var ptnd in newSortedList)
                {
                    sbBody.Append("<tr><td>" + ptnd.Name + "</td><td>" + ptnd.Role + "</td><td>" + ptnd.Email +
                                  "</td></tr>");
                }
                sbBody.Append("</table>");
            }

            if (siteEmailLists.ExpiredList.Count == 0)
            { }
            else
            {
                var expiredSortedList = siteEmailLists.ExpiredList.OrderBy(x => x.Name).ToList();
                sbBody.Append("<h3>The following expired staff members have not completed their annual post tests.</h3>");

                sbBody.Append("<table style='border-collapse:collapse;' cellpadding='5' border='1'><tr style='background-color:87CEEB'><th>Name</th><th>Role</th><th>Due Date</th><th>Email</th></tr>");
                foreach (var ptnd in expiredSortedList)
                {
                    Debug.Assert(ptnd.NextDueDate != null, "ptnd.NextDueDate != null");
                    sbBody.Append("<tr><td>" + ptnd.Name + "</td><td>" + ptnd.Role + "</td><td>" + ptnd.NextDueDate.Value.ToShortDateString() + "</td><td>" + ptnd.Email +
                                  "</td></tr>");
                }
                sbBody.Append("</table>");
            }

            if (siteEmailLists.DueList.Count == 0)
                sbBody.Append("<h3>There are no staff members due to take their annual post tests.</h3>");
            else
            {
                var dueSortedList = siteEmailLists.DueList.OrderBy(x => x.Name).ToList();
                sbBody.Append("<h3>The following staff members are due to take their annual post tests.</h3>");

                sbBody.Append("<table style='border-collapse:collapse;' cellpadding='5' border='1'><tr style='background-color:87CEEB'><th>Name</th><th>Role</th><th>Due Date</th><th>Email</th></tr>");
                foreach (var ptnd in dueSortedList)
                {
                    Debug.Assert(ptnd.NextDueDate != null, "ptnd.NextDueDate != null");
                    sbBody.Append("<tr><td>" + ptnd.Name + "</td><td>" + ptnd.Role + "</td><td>" + ptnd.NextDueDate.Value.ToShortDateString() + "</td><td>" + ptnd.Email +
                                  "</td></tr>");
                }
                sbBody.Append("</table>");
            }

            if (siteEmailLists.StaffTestsNotCompletedList.Count > 0)
            {
                var notCompletedSortedList = siteEmailLists.StaffTestsNotCompletedList.OrderBy(x => x.StaffName).ToList();
                sbBody.Append("<h3>The following staff members have not completed all post tests.</h3>");

                //sbBody.Append("<div><table style='border-collapse:collapse;' cellpadding='5' border='1';><tr style='background-color:87CEEB'><th>Name</th><th>Role</th><th>Email</th><th>Tests Not Completed</th><th>Tests Completed</th></tr>");
                sbBody.Append("<div><table style='border-collapse:collapse;' cellpadding='5' border='1';><tr style='background-color:87CEEB'><th>Name</th><th>Role</th><th>Email</th><th>Tests Not Completed</th></tr>");
                foreach (var tncl in notCompletedSortedList)
                {
                    if (tncl.TestsNotCompleted.Count == 0)
                        continue;

                    //sbBody.Append("<div>");
                    var email = "not entered";
                    if (tncl.Email != null)
                        email = tncl.Email;

                    sbBody.Append("<tr><td>" + tncl.StaffName + "</td><td>" + tncl.Role + "</td><td>" + email + "</td><td>");

                    foreach (var test in tncl.TestsNotCompleted)
                    {
                        sbBody.Append(test + newLine);
                    }
                    sbBody.Append("</td></tr>");


                }
                sbBody.Append("</table></div>");

            }

            SendHtmlEmail("Post Tests Notifications - " + siteName, coordinators.Select(coord => coord.Email).ToArray(), null, sbBody.ToString(), @"<a href='http://halfpintstudy.org/hpProd/'>Halfpint Study Website</a>");
        }

        internal static void SendHtmlEmail(string subject, string[] toAddress, string[] ccAddress, string bodyContent, string url, string bodyHeader = "")
        {
            Nlogger.LogInfo("SendHtmlEmail: " + subject + toAddress);

            if (toAddress.Length == 0)
                return;
            
            var mm = new MailMessage { Subject = subject, Body = bodyContent };
            
            string path = Path.Combine(_server.MapPath("~/Content/Images"), "mailLogo.jpg"); 
            var mailLogo = new LinkedResource(path);

            var sb = new StringBuilder("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">");
            sb.Append("<html>");
            sb.Append("<head>");

            //sb.Append("<style type='text/css'>");
            //sb.Append("td {padding:10px; }");
            //sb.Append("#Content {width:500px; margin:0px auto; text-align:left; padding:15px; border:1px dashed #333; background-color:#eee;}");
            //sb.Append("</style");

            sb.Append("</head>");
            sb.Append("<body style='text-align:left;'>");
            sb.Append("<img style='width:200px;' alt='' hspace=0 src='cid:mailLogoID' align=baseline />");
            if (bodyHeader.Length > 0)
            {
                sb.Append(bodyHeader);
            }

            sb.Append("<div style='text-align:left;margin-left:30px;width:100%'>");
            sb.Append("<table style='margin-left:0px;'>");
            sb.Append(bodyContent);
            sb.Append("</table>");
            sb.Append("<br/><br/>" + url);
            sb.Append("</div>");
            sb.Append("</body>");
            sb.Append("</html>");

            AlternateView av = AlternateView.CreateAlternateViewFromString(sb.ToString(), null, "text/html");

            mailLogo.ContentId = "mailLogoID";
            av.LinkedResources.Add(mailLogo);
            mm.AlternateViews.Add(av);

            foreach (string s in toAddress)
                mm.To.Add(s);
            if (ccAddress != null)
            {
                foreach (string s in ccAddress)
                    mm.CC.Add(s);
            }

            //Console.WriteLine("Send Email");
            //Console.WriteLine("Subject:" + subject);
            //Console.Write("To:" + toAddress[0]);
            //Console.Write("Email:" + sb);

            try
            {
                var smtp = new SmtpClient();
                smtp.Send(mm);
            }
            catch (Exception ex)
            {
                Nlogger.LogError(ex.Message);
            }

        }

        internal static List<MembershipUser> GetUserInRole(string role, int site)
        {
            var memUsers = new List<MembershipUser>();
            string[] users = Roles.GetUsersInRole(role);

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = System.Data.CommandType.StoredProcedure,
                        CommandText = "GetSiteUsers"
                    };
                    var param = new SqlParameter("@siteID", site);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var rdr = cmd.ExecuteReader();

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
            }
            return memUsers;
        }

        private static void DeleteOldOperatorsLists(string siteCode)
        {
            var folderPath = ConfigurationManager.AppSettings["StatStripListPath"];
            var path = Path.Combine(folderPath, siteCode);
            var di = new DirectoryInfo(path);

            if (di.Exists)
            {
                var files = from f in di.GetFiles()
                            where f.LastWriteTime < DateTime.Now.AddDays(-7)
                            select f;
                files.ToList().ForEach(f => f.Delete());
            }
        }

        private static List<SiteInfoPts> GetSites()
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

        private static List<PostTestNextDue> GetStaffPostTestsCompletedInfo(int siteId)
        {
            var ptndl = new List<PostTestNextDue>();

            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = System.Data.CommandType.StoredProcedure,
                        CommandText = "GetStaffActiveInfoForSite"
                    };
                    var param = new SqlParameter("@siteId", siteId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("Role");
                        var role = rdr.GetString(pos);

                        if (role == "Admin")
                            continue;

                        var userName = string.Empty;
                        pos = rdr.GetOrdinal("UserName");

                        if (!rdr.IsDBNull(pos))
                            userName = rdr.GetString(pos);

                        //skip generic roles
                        if (role == "Nurse" || role == "DCC")
                        {
                            if (userName != string.Empty)
                                continue;
                        }

                        var ptnd = new PostTestNextDue { Role = role };

                        pos = rdr.GetOrdinal("ID");
                        ptnd.Id = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("Name");
                        ptnd.Name = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("Email");
                        if (!rdr.IsDBNull(pos))
                            ptnd.Email = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("EmployeeID");
                        if (!rdr.IsDBNull(pos))
                            ptnd.EmployeeId = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("NovaStatStrip");
                        if (!rdr.IsDBNull(pos))
                        {
                            ptnd.IsNovaStatStripTested = rdr.GetBoolean(pos);
                        }

                        pos = rdr.GetOrdinal("Vamp");
                        if (!rdr.IsDBNull(pos))
                        {
                            ptnd.IsVampTested = rdr.GetBoolean(pos);
                        }

                        ptndl.Add(ptnd);
                    }
                    rdr.Close();
                    conn.Close();

                    foreach (var ptnd in ptndl)
                    {
                        cmd = new SqlCommand("", conn)
                        {
                            CommandType = System.Data.CommandType.StoredProcedure,
                            CommandText = "GetPostTestsCompletedForStaffMember"
                        };
                        param = new SqlParameter("@staffId", ptnd.Id);
                        cmd.Parameters.Add(param);

                        conn.Open();
                        rdr = cmd.ExecuteReader();

                        while (rdr.Read())
                        {
                            var postTest = new PostTestPts();

                            var pos = rdr.GetOrdinal("ID");
                            postTest.PostTestCompletedId = rdr.GetInt32(pos);

                            pos = rdr.GetOrdinal("TestID");
                            postTest.Id = rdr.GetInt32(pos);

                            pos = rdr.GetOrdinal("TestName");
                            postTest.Name = rdr.GetString(pos);

                            pos = rdr.GetOrdinal("DateCompleted");
                            postTest.DateCompleted = rdr.GetDateTime(pos);

                            //ignore 'Overview' for due and expired
                            if (postTest.Name != "Overview")
                            {

                                //this is temporary - take this out after May 1
                                #region tempDateCompleted

                                var dateCompleted = postTest.DateCompleted.GetValueOrDefault();
                                if (dateCompleted.CompareTo(DateTime.Parse("05/01/2012")) < 0)
                                {
                                    postTest.DateCompleted = DateTime.Parse("05/01/12");
                                    dateCompleted = DateTime.Parse("05/01/2012");
                                }
                                var nextDueDate = dateCompleted.AddYears(1);

                                #endregion tempDateCompleted
                                //assign the next due date to the staff member
                                //this works because the completed tests are in dateCompleted order 
                                if (ptnd.NextDueDate == null)
                                {
                                    ptnd.NextDueDate = postTest.DateCompleted.Value.AddYears(1);
                                    ptnd.SNextDueDate = ptnd.NextDueDate.Value.ToShortDateString();
                                }
                                var tsDayWindow = nextDueDate - DateTime.Now;
                                if (tsDayWindow.Days <= 30)
                                {
                                    //if within window
                                    //the staff member can both be due and expired
                                    //the test is one or the other
                                    if (tsDayWindow.Days < 0)
                                    {
                                        postTest.IsExpired = true;
                                        ptnd.IsExpired = true;
                                    }
                                    else
                                    {
                                        postTest.IsDue = true;
                                        ptnd.IsDue = true;
                                    }
                                }
                            }

                            //remove this from the tests not completed list
                            ptnd.TestsNotCompleted.Remove(postTest.Name);
                            //add to the tests completed
                            ptnd.TestsCompleted.Add(postTest);
                        }
                        rdr.Close();
                        conn.Close();

                        if (ptnd.TestsCompleted.Count == 0 || (ptnd.TestsNotCompleted.Contains("Overview")))
                        {
                            ptnd.IsNew = true;
                            if (ptnd.NextDueDate == null)
                                ptnd.NextDueDate = DateTime.Today.AddYears(1);

                        }
                        else
                        {
                            cmd = new SqlCommand("", conn)
                            {
                                CommandType = System.Data.CommandType.StoredProcedure,
                                CommandText = "IsStaffMemberPostTestsNew"
                            };
                            param = new SqlParameter("@staffId", ptnd.Id);
                            cmd.Parameters.Add(param);
                            conn.Open();
                            var count = (int)cmd.ExecuteScalar();
                            ptnd.IsNew = count > 0;

                            if (!ptnd.IsNew)
                            {
                                if (ptnd.TestsNotCompleted.Contains("Overview"))
                                    ptnd.TestsNotCompleted.Remove("Overview");
                            }
                            conn.Close();
                        }
                    }

                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return null;
                }
            }

            return ptndl;
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
                    Nlogger.LogError(ex);
                }
            }

            return list;
        }

        static void WriteNovaNetFile(IEnumerable<NovaNetColumns> lines, string siteName, string siteCode)
        {
            //write lines to new file
            var folderPath = ConfigurationManager.AppSettings["StatStripListPath"];
            var path = Path.Combine(folderPath, siteCode);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var fileName = siteName + " " + DateTime.Now.ToString("MM-dd-yyyy") + " " + ConfigurationManager.AppSettings["StatStripListName"];


            var fullpath = Path.Combine(path, fileName);


            var sw = new StreamWriter(fullpath, false);


            sw.WriteLine("Novanet Operator Import Data,version 2.0,,,,,,,,,");
            foreach (var line in lines)
            {
                sw.Write(line.LastName + ",");
                sw.Write(line.FirstName + ",");
                sw.Write(line.Col3 + ",");
                sw.Write(line.Col4 + ",");
                sw.Write(line.Col5 + ",");
                sw.Write(line.EmployeeId + ",");
                sw.Write(line.Col7 + ",");
                sw.Write(line.Col8 + ",");
                sw.Write(line.Col9 + ",");
                sw.Write(line.StartDate + ",");
                sw.Write(line.EndDate);
                sw.Write(sw.NewLine);
            }
            sw.Close();
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