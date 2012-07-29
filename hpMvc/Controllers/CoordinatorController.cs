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
            ViewBag.CgmUpload = "false";
            //var list = DbUtils.GetSiteRandomizedStudiesActive(siteID);
            return View();            
        }
                
        public ActionResult CompleteSubject(string id)
        {
            var ra = DbUtils.GetRandomizedStudyActive(id);
            return View(ra);
        }

        [HttpPost]
        public ActionResult CompleteSubject(SubjectCompleted model, HttpPostedFileBase file)
        {
            //file upload
            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                //todo validate file type
                var folderPath = ConfigurationManager.AppSettings["CgmUploadPath"].ToString();
                
                var fullPath = Path.Combine(folderPath, fileName);
                //file.SaveAs(fullPath);
                //model.CgmUpload = true;
                ViewBag.CgmUpload = "true";
            }
            else
            {                
                ModelState["CgmUpload"].Errors.Add("File not uploaded");
            }

            
            //if(!ModelState.IsValid)
            
            return View("CompleteSubject", model);
            
            //return View(model);

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
