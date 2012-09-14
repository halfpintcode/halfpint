using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using hpMvc.Models;
using hpMvc.DataBase;
using System.Web.Helpers;
using System.Configuration;
using System.IO;
using hpMvc.Infrastructure.Logging;
using System.Web.Security;

namespace hpMvc.Controllers
{
    [Authorize]
    public class CoordinatorController : Controller
    {
        NLogger nlogger = new NLogger();

        public ActionResult Index()
        {
            string role = "";
            int siteID = DbUtils.GetSiteidIDForUser(User.Identity.Name);

            if (HttpContext.User.IsInRole("Admin"))
            {
                role = "Admin";

                List<Site> sites = new List<Site>();

                sites = DbUtils.GetSites();
                if (sites.Count == 0)
                    throw new Exception("There was an error retreiving the sites list from the database");
                sites.Insert(0, new Site { ID = 0, Name = "Select a site", SiteID = "" });

                ViewBag.Sites = new SelectList(sites, "ID", "Name", siteID);
            }
            ViewBag.Role = role;
            ViewBag.SiteID = siteID;            
            //ViewBag.CgmUpload = "false";
            //var list = DbUtils.GetSiteRandomizedStudiesActive(siteID);
            return View();            
        }

        public JsonResult GetActiveSubjects(string siteID, bool showCleared)
        {
            int site = int.Parse(siteID);
            List<SubjectCompleted> list;

            if(showCleared)
                list = DbUtils.GetSiteRandomizedStudiesCleared(site);    
            else
                list = DbUtils.GetSiteRandomizedStudiesActive(site);    
            return Json(list);
        }

        public ActionResult CompleteSubject(string id)
        {
            string role = "";
            if (HttpContext.User.IsInRole("Admin"))
            {
                role = "Admin";
            }

            ViewBag.Role = role;
            ViewBag.Confirmation = "false";
            var ra = DbUtils.GetRandomizedStudyActive(id);
            
            return View(ra);
        }

        [HttpPost]
        public ActionResult CompleteSubject(SubjectCompleted model, HttpPostedFileBase file)
        {
            ViewBag.Confirmation = "true";
            //file upload
            if (model.NotCompletedReason == null)
                model.NotCompletedReason = "";
            if (file != null && file.ContentLength > 0)
            {
                //todo validate file type
                var fileName = Path.GetFileName(file.FileName);
                nlogger.LogInfo("CGM Upload - fileName: " + fileName);
                
                var folderPath = ConfigurationManager.AppSettings["CgmUploadPath"].ToString();
                var folderSitePath = Path.Combine(folderPath, model.SiteName);
                nlogger.LogInfo("CGM Upload - path: " + folderSitePath);
                if (!Directory.Exists(folderSitePath))
                    Directory.CreateDirectory(folderSitePath);

                var newName = model.StudyID.Trim() + "_CGM.csv";
                var fullPath = Path.Combine(folderSitePath, newName);
                file.SaveAs(fullPath);
                model.CgmUpload = true;                
            }

            bool isOkToClear = true;

            if (!model.CgmUpload)
            {
                isOkToClear = false;
                ModelState["CgmUpload"].Errors.Clear();
                //ModelState["CgmUpload"].Errors.Add("*The CGM file upload is required.  Enter a reason if you can not provide this data.");
                ModelState.AddModelError("" ,"*The CGM file upload is required.  Enter a reason if you can not provide this data.");
            }
            if (model.DateCompleted == null)
            {
                isOkToClear = false;
                ModelState["DateCompleted"].Errors.Clear();
                //ModelState["DateCompleted"].Errors.Add("*Date completed  is required.  Enter a reason if you can not provide this data.");
                ModelState.AddModelError("", "*Date completed  is required.  Enter a reason if you can not provide this data.");
            }
            if (model.Older2)
            {
                if (!model.CBCL)
                {
                    isOkToClear = false;
                    ModelState["CBCL"].Errors.Clear();
                    ModelState.AddModelError("", "You must certify with a check mark that CBCL has been collected and sent to the CCC.  Enter a reason if you can not provide this data.");
                }
                if (!model.Demographics)
                {
                    isOkToClear = false;
                    ModelState["Demographics"].Errors.Clear();
                    ModelState.AddModelError("", "You must certify with a check mark that subject demographics has been collected and sent to the CCC.  Enter a reason if you can not provide this data.");
                }
                if (!model.PedsQL)
                {
                    isOkToClear = false;
                    ModelState["PedsQL"].Errors.Clear();
                    ModelState.AddModelError("", "You must certify with a check mark that Peds-QL has been collected and sent to the CCC.  Enter a reason if you can not provide this data.");
                }
                if (!model.ContactInfo)
                {
                    isOkToClear = false;
                    ModelState["ContactInfo"].Errors.Clear();
                    ModelState.AddModelError("", "You must certify with a check mark that subject contact information has been collected.  Enter a reason if you can not provide this data.");
                }
            }

            if (!isOkToClear)
            {
                //the client shoud catch this - if we get this far then
                //the user did not provide all required fields and didn't 
                //give a reason
                bool isReason = false;
                if ( model.NotCompletedReason != null)
                {
                    if (model.NotCompletedReason.Trim().Length > 0)
                        isReason = true;
                }

                if (!isReason)
                {                
                    string role = "";
                    if (HttpContext.User.IsInRole("Admin"))
                    {
                        role = "Admin";
                    }
                    ViewBag.Role = role;
                    ViewBag.Confirmation = "false";

                    return View(model);
                }
            }
            
            if (isOkToClear)
                model.Cleared = true;
            
            DbUtils.SaveRandomizedSubjectActive(model, User.Identity.Name);

            //send email for non completion
            string[] users = ConfigurationManager.AppSettings["NewFormulaNotify"].ToString().Split(new[] { ',' }, StringSplitOptions.None);

            List<string> toEmails = new List<string>();
            foreach (var user in users)
            {
                var mUser = Membership.GetUser(user);
                if (mUser == null)
                    continue;
                toEmails.Add(mUser.Email);
            }
                                

            var u = new UrlHelper(this.Request.RequestContext);
            string url = "http://" + this.Request.Url.Host + u.RouteUrl("Default", new { Controller = "Home", Action = "Index" });

            Utility.SendCompleteSubjectMail(toEmails.ToArray(), null, url, Server, model, User.Identity.Name);
            
            return View(model);
        }

        public ActionResult StudyIdsNotRandomized(string siteID)
        {
            string role = "";

            if (HttpContext.User.IsInRole("Admin"))
            {
                role = "Admin";

                List<Site> sites = new List<Site>();

                sites = DbUtils.GetSites();
                if (sites.Count == 0)
                    throw new Exception("There was an error retreiving the sites list from the database");
                sites.Insert(0, new Site { ID = 0, Name = "Select a site", SiteID = "" });
                ViewBag.Sites = new SelectList(sites, "ID", "Name");
            }            
            ViewBag.Role = role;
            
            int site = DbUtils.GetSiteidIDForUser(User.Identity.Name);
            var list = DbUtils.GetStudyIDsNotRandomized(site);
            return View(list);
        }

        public ActionResult UpdateStaffInformation()
        {
            string role = "";

            if (HttpContext.User.IsInRole("Admin"))
            {
                role = "Admin";

                List<Site> sites = new List<Site>();

                sites = DbUtils.GetSites();
                if (sites.Count == 0)
                    throw new Exception("There was an error retreiving the sites list from the database");
                sites.Insert(0, new Site { ID = 0, Name = "Select a site", SiteID = "" });
                ViewBag.Sites = new SelectList(sites, "ID", "Name");
            }
            ViewBag.Role = role;

            int site = DbUtils.GetSiteidIDForUser(User.Identity.Name);
            ViewBag.Site = site;

            var list = DbUtils.GetStaffLookupForSite(site.ToString());
            list.Insert(0, new Site { ID = 0, Name = "Select a member", SiteID = "" });
            ViewBag.Users = new SelectList(list, "ID", "Name");
            return View();
        }

        [HttpPost]
        public ActionResult UpdateStaffInformation(StaffModel model)
        {
            //validate model
            if (ModelState.IsValid)
            {
                MessageListDTO dto = DbUtils.UpdateStaff(model);
                if (dto.IsSuccessful)
                {
                    //send email notification
                    if (model.SendEmail)
                        Utility.SendAccountCreatedMail(new string[] { model.Email }, null, dto.Bag.ToString(), model.UserName, Utility.GetSiteLogonUrl(this.Request), this.Server);

                }
                return View("UpdateStaffConfirmation", dto);
            }
            return View();
        }

        public JsonResult GetStaffForSite(string site)
        {
            var list = DbUtils.GetStaffLookupForSite(site);
            list.Insert(0, new Site { ID = 0, Name = "Select a member", SiteID = "" });
            return Json(list);
        }

        [HttpPost]
        public ActionResult GetStaffInfo(string user)
        {
            StaffModel model = DbUtils.GetStaffInfo(int.Parse(user));
            return PartialView("UpdateStaffPartial", model);
        }

        public JsonResult GetNonradomizedStudies(string siteID)
        {
            var list = DbUtils.GetStudyIDsNotRandomized(int.Parse(siteID));
            var grid = new WebGrid(list, defaultSort: "StudyID", rowsPerPage: 50);
            var htmlString = grid.GetHtml();

            return Json(new { Data = htmlString.ToHtmlString() }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowPostTestsCompleted()
        {
            int siteID = DbUtils.GetSiteidIDForUser(User.Identity.Name);
            var ptpcl = DbPostTestsUtils.GetPostTestPeoplesTestsCompleted(siteID);
            return View(ptpcl);
        }

        public ActionResult ShowPostTestsCompleted2()
        {
            int siteID = DbUtils.GetSiteidIDForUser(User.Identity.Name);
            var ptel = DbPostTestsUtils.GetPostTestPeoplesTestsExtended(siteID);
            return View(ptel);
        }

        public ActionResult ShowPostTestsDue()
        {
            int siteID = DbUtils.GetSiteidIDForUser(User.Identity.Name);
            var ptndl = DbPostTestsUtils.GetPostTestPeopleFirstDateCompleted(siteID);
            return View(ptndl);
        }

        public ActionResult RandomizedStudies()
        {
            //List<Site> sites = new List<Site>();

            //sites = DbUtils.GetSites();
            //if (sites.Count == 0)
            //    throw new Exception("There was an error retreiving the sites list from the database");
            //sites.Insert(0, new Site { ID = 0, Name = "Select a site", SiteID = "" });
            //ViewBag.Sites = new SelectList(sites, "ID", "Name");

            //var list = DbUtils.GetAllRandomizedStudies();
            int siteID = DbUtils.GetSiteidIDForUser(User.Identity.Name);
            var list = DbUtils.GetSiteRandomizedStudies(siteID);
            return View(list);
        }

        public JsonResult GetSiteRandomizedStudies(string siteID)
        {
            var list = DbUtils.GetSiteRandomizedStudies(int.Parse(siteID));
            var grid = new WebGrid(list, defaultSort: "Number", rowsPerPage: 50);
            var htmlString = grid.GetHtml(tableStyle: "webgrid",
                            columns: grid.Columns(
                            grid.Column("SiteName", header: "Site"),
                            grid.Column("Number"),
                            grid.Column("StudyID", header: "Study  ID"),
                            grid.Column("Arm"),
                            grid.Column("DateRandomized", header: "Date Randomized", format: x => x.DateRandomized.ToString("MM/dd/yyyy hh:mm tt"))));

            return Json(new { Data = htmlString.ToHtmlString() }, JsonRequestBehavior.AllowGet);
        }

        public FilePathResult DownloadStatStripList()
        {
            string siteName = DbUtils.GetSiteNameForUser(User.Identity.Name);
            var folderPath = ConfigurationManager.AppSettings["StatStripListPath"].ToString();
            string fileName = siteName + " " + ConfigurationManager.AppSettings["StatStripListName"].ToString();
                        
            var fullpath = Path.Combine(folderPath, fileName);

            nlogger.LogInfo("DownloadStatStripList: " + fileName);
            return this.File(fullpath, "application/csv", fileName);
        }
    }
}
