using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Text;
using hpMvc.Infrastructure.Logging;
using hpMvc.Models;
using System.Web.Mvc;
using System.IO;


namespace hpMvc.DataBase
{
	public static class Utility
	{
		public static NLogger nlogger;
		
		static Utility()
		{
			nlogger = new NLogger();

		}

		public static string GetSiteUrl(HttpRequestBase request)
		{
			var u = new UrlHelper(request.RequestContext);
			return request.Url.Scheme + Uri.SchemeDelimiter + request.Url.Host;
		}

		public static string GetSiteLogonUrl(HttpRequestBase request)
		{
			var u = new UrlHelper(request.RequestContext);
			return request.Url.Scheme + Uri.SchemeDelimiter + request.Url.Host + u.RouteUrl("Default", new { Controller = "Account", Action = "Logon" });
		}

        public static void SendCompleteSubjectMail(string[] toAddress, string[] ccAddress, string url, HttpServerUtilityBase server, SubjectCompleted sc, string user)
        {
            string subject = "";
            string yesNo = "";
            if (sc.Cleared)
                subject = "Subject completed - " + sc.SiteName + " - SubjectID: " + sc.StudyID;
            else
                subject = "Subject completed (did not enter all required info) - " + sc.SiteName + " - SubjectID: " + sc.StudyID;
            
            StringBuilder sbBody = new StringBuilder("<div>Site: " + sc.SiteName + " - Subject ID: " + sc.StudyID + " - User name: " + user + "</div>");
            
            sbBody.Append("<br/>");
            sbBody.Append("Date Completed: " + (sc.DateCompleted !=null ? sc.DateCompleted.Value.ToString("MM/dd/yyyy") : ""));
            sbBody.Append("<br/>");

            if (sc.CgmUpload)
                yesNo = "yes";
            else
                yesNo = "no";
            sbBody.Append("CGM file upload: " + yesNo);
            sbBody.Append("<br/>");
                        
            if (sc.Age2to16)
                yesNo = "yes";
            else
                yesNo = "no";
            sbBody.Append("Older than 2 and less than 17: " + yesNo);
            sbBody.Append("<br/>");
            if (sc.Age2to16)
            {
                sbBody.Append("The following have been collected and sent to the CCC:");
                sbBody.Append("<br/>");
                
                if (sc.CBCL)
                    yesNo = "yes";
                else
                    yesNo = "no";
                sbBody.Append("CBCL: " + yesNo);
                sbBody.Append("<br/>");

                if (sc.PedsQL)
                    yesNo = "yes";
                else
                    yesNo = "no";
                sbBody.Append("PedsQL: " + yesNo);
                sbBody.Append("<br/>");

                if (sc.Demographics)
                    yesNo = "yes";
                else
                    yesNo = "no";
                sbBody.Append("Subject demographics: " + yesNo);
                sbBody.Append("<br/>");

                
                if (sc.ContactInfo)
                    yesNo = "yes";
                else
                    yesNo = "no";
                sbBody.Append("Subject contact information has been collected and stored at site: " + yesNo);
                sbBody.Append("<br/>");
            }
            if (sc.NotCompletedReason.Trim().Length > 0)
            {
                sbBody.Append("Reason for Not providing all required data:");
                sbBody.Append("<br/>");
                sbBody.Append(sc.NotCompletedReason);
            }
            string siteUrl = "Click here to logon to the website: <a href='" + url + "'>HalfpintStudy.org</a>";
            SendHtmlEmail(subject, toAddress, ccAddress, sbBody.ToString(), server, siteUrl, "");
        }

		public static void SendBroadcastMail(string[] toAddress, string[] ccAddress, string url, HttpServerUtilityBase server, string subject, string body)
		{
			string bodyHeader = "<h3 style='display:inline-block;background-color:Aqua;text-align:center;'>This is a Halfpint broadcast email</h3>";
			
			string siteUrl = "Click here to logon to the website: <a href='" + url + "'>HalfpintStudy.org</a>";

			SendHtmlEmail(subject, toAddress, ccAddress, body, server, siteUrl, bodyHeader);
		}

        public static void SendTestMail(string[] toAddress, string[] ccAddress, string url, HttpServerUtilityBase server)
        {
            string subject = "Halfpint - Test Email";
            string bodyHeader = "<h3 style='display:inline-block;background-color:Aqua;text-align:center;'>This is a Test Header</h3>";

            StringBuilder sbBody = new StringBuilder("This is a test email");
            string siteUrl = "Click here to logon to the website: <a href='" + url + "'>HalfpintStudy.org</a>";

            SendHtmlEmail(subject, toAddress, ccAddress, sbBody.ToString(), server, siteUrl, bodyHeader);
        }

		public static void SendStudyInitializedMail(string[] toAddress, string[] ccAddress, string studyId, string userName, 
            string siteName, HttpServerUtilityBase server, string url, string arm, string cafpintId, DateTime dateRandomized)
		{
			const string subject = "Halfpint - New Study Initialized Added";
            if (!url.Contains("hpProd"))
                studyId = "T" + studyId;

            string bodyHeader = "<h3 style='display:inline-block;background-color:Aqua;text-align: center;'>" + userName + " from " + siteName + " has initialized a new subject, ID: <strong>" +
				studyId + " (" + arm + ")</strong></h3>";

		    var sbBody = new StringBuilder("");
            if (!string.IsNullOrEmpty(cafpintId))
            {
                sbBody.Append("This subject was enrolled in the CAF-PINT ancillary trial. CAF-PINT Id:" + cafpintId);
            }
            sbBody.Append("<br/>");

            sbBody.Append("Date time: " + dateRandomized.ToShortDateString() + " " + dateRandomized.ToShortTimeString());

		    

			string siteUrl = "Website: <a href='" + url + "'>HalfpintStudy.org</a>";
			SendHtmlEmail(subject, toAddress, ccAddress, sbBody.ToString(), server, siteUrl, bodyHeader);
		}

		public static void SendFormulaAddeddMail(string[] toAddress, string[] ccAddress, EnteralFormula formula, string name, string siteName, HttpServerUtilityBase server, string url)
		{
			string subject = "Halfpint - New Formula Added";
			StringBuilder sbBody = new StringBuilder("<p>" + name + " from " + siteName + " has added a new formula:</p>");

			sbBody.Append("<table><tr><th>Name</th><th>" + formula.Name +"</th></tr>");
			sbBody.Append("<tr><td>CHO %</td><td>" + formula.ChoKcal + "</td></tr>");
			sbBody.Append("<tr><td>Lipid %</td><td>" + formula.LipidKcal + "</td></tr>");
			sbBody.Append("<tr><td>Protein %</td><td>" + formula.ProteinKcal + "</td></tr>");
			sbBody.Append("<tr><td>kCal/mL</td><td>" + formula.Kcal_ml + "</td></tr>");
			sbBody.Append("</table>");

			string siteUrl = "Website: <a href='" + url + "'>HalfpintStudy.org</a>";
			SendHtmlEmail(subject, toAddress, ccAddress, sbBody.ToString(), server, siteUrl);
		}

		public static void SendOtherNutritionMail(string[] toAddress, string[] ccAddress, CalOtherNutrition con, CalStudyInfo csi, string name, string siteName, HttpServerUtilityBase server, string url)
		{
			string subject = "Halfpint - Other Nutrition Added";
			StringBuilder sbBody = new StringBuilder("<p>" + name + " from " + siteName + " has included other nutrition:</p>");
			
			sbBody.Append("Study ID: " + csi.SStudyId + ", Date: " + csi.CalcDate);
			sbBody.Append("<br/>");
			sbBody.Append("<table><tr><th>Type</th><th>Checked</th></tr>");
			sbBody.Append("<tr><td>Breast Feeding</td><td>" + con.BreastFeeding + "</td></tr>");
			sbBody.Append("<tr><td>Drinks</td><td>" + con.Drinks + "</td></tr>");
			sbBody.Append("<tr><td>Solid Foods</td><td>" + con.SolidFoods + "</td></tr>");
			sbBody.Append("<tr><td>Other</td><td>" + con.Other + "</td></tr>");            
			sbBody.Append("</table>");
			sbBody.Append("<br/>");
			sbBody.Append("Other Text:");
			sbBody.Append("<br/>");
			sbBody.Append(con.OtherText);

			string siteUrl = "Website: <a href='" + url + "'>HalfpintStudy.org</a>";
			SendHtmlEmail(subject, toAddress, ccAddress, sbBody.ToString(), server, siteUrl);
		}

		public static void SendAdditiveAddeddMail(string[] toAddress, string[] ccAddress, Additive additive, string name, string siteName, HttpServerUtilityBase server, string url)
		{
			string subject = "Halfpint - New Additive Added";
			var sbBody = new StringBuilder("<p>" + name + " from " + siteName + " has added a new additive:</p>");

			sbBody.Append("<table><tr><th>Name</th><th>" + additive.Name + "</th></tr>");
			sbBody.Append("<tr><td>CHO %</td><td>" + additive.ChoKcal + "</td></tr>");
			sbBody.Append("<tr><td>Lipid %</td><td>" + additive.LipidKcal + "</td></tr>");
			sbBody.Append("<tr><td>Protein %</td><td>" + additive.ProteinKcal + "</td></tr>");
			sbBody.Append("<tr><td>Unit</td><td>" + additive.UnitName + "</td></tr>");
			sbBody.Append("<tr><td>kCal/unit </td><td>" + additive.Kcal_unit + "</td></tr>");
			
			sbBody.Append("</table>");

			string siteUrl = "Website: <a href='" + url + "'>HalfpintStudy.org</a>";
			SendHtmlEmail(subject, toAddress, ccAddress, sbBody.ToString(), server, siteUrl);
		}

		public static void SendPostTestsSubmittedMail(string[] toAddress, string[] ccAddress, List<PostTest> tests, string name, string siteName, HttpServerUtilityBase server, string url)
		{
			string subject = "Halfpint post-tests submitted by: " + name;
			var sbBody = new StringBuilder("<p>" + name + " from " + siteName + " has completed the Halfpint training modules and passed post-tests for the following:</p>");

            sbBody.Append("<table><tr><th>Test</th><th>Date Completed</th></tr>");
			foreach (PostTest test in tests)
				sbBody.Append("<tr><td>" + test.Name + "</td><td>" + (test.DateCompleted != null ? test.DateCompleted.Value.ToString("MM/dd/yyyy") : "") + "</td></tr>");
			sbBody.Append("</table>");

			string siteUrl = "Website: <a href='" + url + "'>HalfpintStudy.org</a>";
			SendHtmlEmail(subject, toAddress, ccAddress, sbBody.ToString(), server, siteUrl);            
		}

        public static void SendNurseAccountCreatedMail(string[] toAddress, string[] ccAddress, string name, string siteName, string employeeId, HttpServerUtilityBase server, string url)
        {
            var subject = "Halfpint - New Nurse Account Created: " + name;
            var sbBody = new StringBuilder("<p>" + name + " from " + siteName + " has created a new staff account.</p>");

            if (employeeId.Length > 0)
                sbBody.Append("<p>" + name + "'s operater id for the Nova StatStrip Glucose Meter is <strong>" + employeeId + "</strong></p>");
            
            var siteUrl = "Website: <a href='" + url + "'>HalfpintStudy.org</a>";
            SendHtmlEmail(subject, toAddress, ccAddress, sbBody.ToString(), server, siteUrl);
        }

        public static void SendUserLockedOutMail(string[] toAddress, string[] ccAddress, string name, HttpServerUtilityBase server, string url)
        {
            var subject = "Halfpint - User Locked Out - User name: " + name;
            var sbBody = new StringBuilder("<p> User name <string>" + name + "</string> has been locked out.</p>");
            
            var siteUrl = "Website: <a href='" + url + "'>HalfpintStudy.org</a>";
            SendHtmlEmail(subject, toAddress, ccAddress, sbBody.ToString(), server, siteUrl);
        }

		public static void SendAccountCreatedMail(string[] toAddress, string[] ccAddress, string password, string userName, string url, HttpServerUtilityBase server)
		{
			string subject = "Halfpint - Account Created for: " + userName;
			StringBuilder sbBody = new StringBuilder("<div>A Half-pint website account has been created for you.</div>");
			sbBody.Append("<br/><div>Your user name is " + userName + "</strong></div>");
			sbBody.Append("<br/><div>Your temporary password is <strong>" + password + "</strong>.</div>");
			sbBody.Append("<br/><br/><div>You will be required to reset this password on your first logon.</div>");
			sbBody.Append("<br/><br/><div>Click here for logon: <a href='" + url + "'>HalfpintStudy.org</a></div>");

			string siteUrl = "Website: <a href='" + url + "'>HalfpintStudy.org</a>";
			SendHtmlEmail(subject, toAddress, ccAddress, sbBody.ToString(), server, siteUrl);
		}

		public static void SendPasswordResetMail(string[] toAddress, string[] ccAddress, string password, bool reset, HttpServerUtilityBase server, string url)
		{
			string subject = "Halfpint - Password Change";
			StringBuilder sbBody = new StringBuilder("<div>Your password has been changed to <strong>" + password + "</strong>.</div>");
			if (reset)
				sbBody.Append("<br/><div>You will be required to reset your password on the your next logon.</div>");

			string siteUrl = "Website: <a href='" + url + "'>HalfpintStudy.org</a>";
			SendHtmlEmail(subject, toAddress, ccAddress, sbBody.ToString(), server, siteUrl);
		}

		public static void SendRoleAssignedMail(string[] toAddress, string[] ccAddress, string role, HttpServerUtilityBase server, string url)
		{
			string subject = "Halfpint - Role Assigned";
			StringBuilder sbBody = new StringBuilder("<div>The role of <strong>" + role + "</strong> has been assigned to you.</div>");
			sbBody.Append("<br/><div>You will see the name of your role in top right of the screen after you log on.</div>");

			string siteUrl = "Website: <a href='" + url + "'>HalfpintStudy.org</a>";
			SendHtmlEmail(subject, toAddress, ccAddress, sbBody.ToString(), server, siteUrl);
		}

		public static void SendEmail(string subject, string[] toAddress, string[] ccAddress, string body)
		{
			MailMessage mm = new MailMessage();
			foreach(string s in toAddress)
				mm.To.Add(s);
			foreach (string s in ccAddress)
				mm.CC.Add(s); 
			mm.Subject = subject;
			//mm.Body = body;
			
			SmtpClient smtp = new SmtpClient();
			smtp.Send(mm);
		}

		public static void SendHtmlEmail(string subject, string[] toAddress, string[] ccAddress, string body, HttpServerUtilityBase server, string url, string bodyHeader="")
		{
			MailMessage mm = new MailMessage();
			mm.Subject = subject;
			//mm.IsBodyHtml = true;
			mm.Body = body;
			string path = Path.Combine(server.MapPath("~/Content/Images"), "mailLogo.jpg");            
			LinkedResource mailLogo = new LinkedResource(path);

			StringBuilder sb = new StringBuilder("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">");
			sb.Append("<html>");
			sb.Append("<head>");
			//sb.Append("<style type='text/css'>");
			//sb.Append("body {margin:50px 0px; padding:0px; text-align:center; }");
			//sb.Append("#Content {width:500px; margin:0px auto; text-align:left; padding:15px; border:1px dashed #333; background-color:#eee;}");
				 
							
			//sb.Append("</style");
			sb.Append("</head>");
			sb.Append("<body style='text-align:center;'>");
			sb.Append("<img style='width:200px;' alt='' hspace=0 src='cid:mailLogoID' align=baseline />");
			if (bodyHeader.Length > 0)
			{
				sb.Append(bodyHeader);
			}
			
			sb.Append("<div style='text-align:left;margin-left:30px;'>");			
			sb.Append("<table style='margin-left:0px;'>");
			sb.Append(body);
			sb.Append("</table>");
			sb.Append("<br/><br/>" + url);
			sb.Append("</div>");
			sb.Append("</body>");            
			sb.Append("</html>");

			//string sAv = "<img alt='' hspace=0 src='cid:mailLogoID' align=baseline /><br/>";
			//sAv += body;

			
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
			
			var smtp = new SmtpClient();
		    //smtp.EnableSsl = false;
            smtp.Send(mm);
		}
	}
}