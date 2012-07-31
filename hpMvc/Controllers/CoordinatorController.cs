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
                
        public ActionResult CompleteSubject(string id)
        {
            string role = "";
            if (HttpContext.User.IsInRole("Admin"))
            {
                role = "Admin";
            }

            ViewBag.Role = role;
            var ra = DbUtils.GetRandomizedStudyActive(id);
            
            return View(ra);
        }

        [HttpPost]
        public ActionResult CompleteSubject(SubjectCompleted model, HttpPostedFileBase file)
        {            
            //file upload
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
                ModelState["CgmUpload"].Errors.Add("The CGM file upload is required.  Enter a reason if you can not provide this data.");
            }
            if(model.DateCompleted == null)
                isOkToClear = false;

            if (model.Older2)
            {
                if (!model.CBCL)
                {
                    isOkToClear = false;
                    ModelState["CBCL"].Errors.Clear();
                    ModelState["CBCL"].Errors.Add("You must certify with a check mark that CBCL has been collected and sent to the CCC.  Enter a reason if you can not provide this data.");
                }
                if (!model.Demographics)
                {
                    isOkToClear = false;
                    ModelState["Demographics"].Errors.Clear();
                    ModelState["Demographics"].Errors.Add("You must certify with a check mark that subject demographics has been collected and sent to the CCC.  Enter a reason if you can not provide this data.");
                }
                if (!model.PedsQL)
                {
                    isOkToClear = false;
                    ModelState["PedsQL"].Errors.Clear();
                    ModelState["PedsQL"].Errors.Add("You must certify with a check mark that Peds-QL has been collected and sent to the CCC.  Enter a reason if you can not provide this data.");
                }
                if (!model.ContactInfo)
                {
                    isOkToClear = false;
                    ModelState["ContactInfo"].Errors.Clear();
                    ModelState["ContactInfo"].Errors.Add("You must certify with a check mark that subject contact information has been collected.  Enter a reason if you can not provide this data.");
                }
            }

            if (!isOkToClear)
            {
                //the client shoud catch this - if we get this far then
                //the user did not provide all required fields and didn't 
                //give a reason
                if (model.NotCompletedReason.Trim().Length == 0)
                {
                    ModelState["NotCompletedReason"].Errors.Clear();
                    ModelState["NotCompletedReason"].Errors.Add("Enter a reason if you can not provide all required data.");
                }

                //send email for non completion

                string role = "";
                if (HttpContext.User.IsInRole("Admin"))
                {
                    role = "Admin";
                }
                ViewBag.Role = role;


                return View(model);
            }
            
            if (isOkToClear)
                model.Cleared = true;

            //DbUtils.SaveRandomizedSubjectActive(model);

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

        public JsonResult GetActiveSubjects(string siteID)
        {
            int site = int.Parse(siteID);
            var list = DbUtils.GetSiteRandomizedStudiesActive(site);
            return Json(list);
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
