using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Telerik.Web.Mvc;

namespace hpMvc.Controllers
{
    [Authorize]
    public class CoordinatorController : Controller
    {
        readonly NLogger _nlogger = new NLogger();

        [PopulateSiteMap(SiteMapName = "coord", ViewDataKey = "coord")]
        public ActionResult Index()
        {
            string role = "";
            int siteId = DbUtils.GetSiteidIDForUser(User.Identity.Name);

            if (HttpContext.User.IsInRole("Admin"))
            {
                role = "Admin";

                var sites = DbUtils.GetSitesActive();
                if (sites.Count == 0)
                    throw new Exception("There was an error retreiving the sites list from the database");
                sites.Insert(0, new Site { ID = 0, Name = "Select a site", SiteID = "" });

                ViewBag.Sites = new SelectList(sites, "ID", "Name", siteId);
            }
            ViewBag.Role = role;
            ViewBag.SiteID = siteId;            
            //ViewBag.CgmUpload = "false";
            //var list = DbUtils.GetSiteRandomizedStudiesActive(siteID);
            if (!SiteMapManager.SiteMaps.ContainsKey("coord"))
            {
                SiteMapManager.SiteMaps.Register<XmlSiteMap>("coord", sitmap => sitmap.LoadFrom("~/coord.sitemap"));
            }
            return View();            
        }

        public JsonResult GetActiveSubjects(string siteId, bool showCleared)
        {
            int site = int.Parse(siteId);

            var list = showCleared ? DbUtils.GetSiteRandomizedStudiesCleared(site) : DbUtils.GetSiteRandomizedStudiesActive(site);    
            return Json(list);
        }

        public JsonResult GetGraphUrl(string id)
        {
            var chartPathcss = ConfigurationManager.AppSettings["ChartPathcss"];
            var chartPathfile = ConfigurationManager.AppSettings["ChartPathfile"];
            
            var sitePart = id.Substring(0, 2);
            var fileName1 = id + "insulinChart.gif";

            var fullName1 = Path.Combine(Request.PhysicalApplicationPath, chartPathfile, sitePart, fileName1);
            
            var cssfullName1 = chartPathcss + sitePart + "/" + fileName1;

            var fileName2 = id + "glucoseChart.gif";
            var fullName2 = Path.Combine(Request.PhysicalApplicationPath, chartPathfile, sitePart, fileName2);
            var cssfullName2 = chartPathcss + sitePart + "/" + fileName2;


            if (System.IO.File.Exists(fullName1) && System.IO.File.Exists(fullName2))
                return Json(new { path1 = cssfullName1, path2 = cssfullName2, studyID = id });
            return Json("Chart not available");

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
                _nlogger.LogInfo("CGM Upload - fileName: " + fileName);
                
                var folderPath = ConfigurationManager.AppSettings["CgmUploadPath"].ToString();
                var folderSitePath = Path.Combine(folderPath, model.SiteName);
                _nlogger.LogInfo("CGM Upload - path: " + folderSitePath);
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
            if (model.Age2to16)
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


            var u = new UrlHelper(this.Request.RequestContext);
            string url = "http://" + Request.Url.Host + u.RouteUrl("Default", new { Controller = "Home", Action = "Index" });

            Utility.SendCompleteSubjectMail((from user in users select Membership.GetUser(user) into mUser where mUser != null select mUser.Email).ToArray(), null, url, Server, model, User.Identity.Name);
            
            return View(model);
        }

        public ActionResult StudyIdsNotRandomized(string siteId)
        {
            string role = "";

            if (HttpContext.User.IsInRole("Admin"))
            {
                role = "Admin";

                var sites = DbUtils.GetSitesActive();
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
            int site = DbUtils.GetSiteidIDForUser(User.Identity.Name);
            ViewBag.Site = site;

            var retDto = DbPostTestsUtils.GetSiteEmployeeInfoForSite(site.ToString());
            ViewBag.EmpRequired = retDto.Stuff.EmpIDRequired;
            if (retDto.Stuff.EmpIDRequired == "true")
            {
                ViewBag.EmpRegex = retDto.Stuff.EmpIDRegex;
                ViewBag.EmpMessage = retDto.Stuff.EmpIDMessage;
            }
            else
            {
                ViewBag.EmpRegex = "";
                ViewBag.EmpMessage = "";
            }

            var list = DbUtils.GetStaffLookupForSite(site.ToString());
            list.Insert(0, new Site { ID = 0, Name = "Select a member", SiteID = "" });
            ViewBag.Users = new SelectList(list, "ID", "Name");
            
            //show partial if false
            ViewBag.IsValid = "true";
            
            //ViewBag.Error = "";
            return View(new StaffEditModel());
        }

        [HttpPost]
        public ActionResult UpdateStaffInformation(StaffEditModel model)
        {   
            //validate model
            if (ModelState.IsValid)
            {
                if (model.Role != "Nurse")
                {
                    model.Email = model.OldEmail;
                    model.Active = model.OldActive;
                }
                MessageListDTO dto = DbUtils.UpdateStaff(model);
                if (dto.IsSuccessful)
                {

                }
                if (model.OldActive != model.Active)
                {
                    if (model.UserName != null)
                    {
                        var mUser = Membership.GetUser(model.UserName);
                        if (mUser != null)
                        {
                            mUser.IsApproved = model.Active;
                            Membership.UpdateUser(mUser);
                        }
                    }
                }

                return View("UpdateStaffConfirmationPartial", dto);
            }

            //ModelState.AddModelError("FirstName", "Test Error");
            //string key1 = "";
            //string error = "";
            //foreach (var m in ModelState)
            //{
            //    if (m.Value.Errors.Count > 0)
            //    {
            //        key1 = m.Key;
            //        error = key1 + ":" + m.Value.Errors[0].ErrorMessage;
            //    }
            //}
            
           //ViewBag.Error = error;
            string role = "";

            if (HttpContext.User.IsInRole("Admin"))
            {
                role = "Admin";

                var sites = DbUtils.GetSitesActive();
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
            ViewBag.Users = new SelectList(list, "ID", "Name", model.ID.ToString());
            ViewBag.IsValid = "false";

            //need to get tests completed for model - this was not returned from the client
            var postTestsCompleted = DbPostTestsUtils.GetTestsCompleted(model.ID.ToString());
            var ptpc = new PostTestPersonTestsCompleted();
            ptpc.PostTestsCompleted = postTestsCompleted;
            model.PostTestsCompleted = ptpc;
            return View(model);
        }

        public JsonResult IsUserEmailDuplicateOtherThan(int id, string email)
        {
            var dto = DbPostTestsUtils.DoesStaffEmailExistOtherThan(id, email);
            return Json(dto);
        }

        public JsonResult IsUserEmployeeIdDuplicateOtherThan(int id, string employeeId, int site)
        {
            var dto = DbPostTestsUtils.DoesStaffEmployeeIDExistOtherThan(id, employeeId, site);
            return Json(dto);
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
            StaffEditModel model = DbUtils.GetStaffInfo(int.Parse(user));
            model.OldActive = model.Active;
            model.OldEmail = model.Email;
            model.OldEmployeeID = model.EmployeeID;
            return PartialView("UpdateStaffPartial", model);
        }

        public JsonResult GetNonradomizedStudies(string siteId)
        {
            var list = DbUtils.GetStudyIDsNotRandomized(int.Parse(siteId));
            var grid = new WebGrid(list, defaultSort: "StudyID", rowsPerPage: 50);
            var htmlString = grid.GetHtml();

            return Json(new { Data = htmlString.ToHtmlString() }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowPostTestsCompleted()
        {
            int siteId = DbUtils.GetSiteidIDForUser(User.Identity.Name);
            var ptpcl = DbPostTestsUtils.GetPostTestStaffsTestsCompleted(siteId);
            return View(ptpcl);
        }

        public ActionResult ShowPostTestsCompleted2()
        {
            int siteId = DbUtils.GetSiteidIDForUser(User.Identity.Name);
            var ptel = DbPostTestsUtils.GetPostTestStaffsTestsCompletedExtended(siteId);
            return View(ptel);
        }

        public ActionResult ShowPostTestsDue()
        {
            int siteId = DbUtils.GetSiteidIDForUser(User.Identity.Name);
            var ptndl = DbPostTestsUtils.GetStaffPostTestsFirstDateCompletedBySite(siteId);
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
            int siteId = DbUtils.GetSiteidIDForUser(User.Identity.Name);
            var list = DbUtils.GetSiteRandomizedStudies(siteId);
            return View(list);
        }

        public JsonResult GetSiteRandomizedStudies(string siteId)
        {
            var list = DbUtils.GetSiteRandomizedStudies(int.Parse(siteId));
            var grid = new WebGrid(list, defaultSort: "Number", rowsPerPage: 50);
            var htmlString = grid.GetHtml(tableStyle: "webgrid",
                            columns: grid.Columns(
                            grid.Column("SiteName", header: "Site"),
                            grid.Column("Number"),
                            grid.Column("StudyID", header: "Study  ID"),
                            grid.Column("Arm"),
                            grid.Column("DateRandomized", "Date Randomized", x => x.DateRandomized.ToString("MM/dd/yyyy hh:mm tt"))));

            return Json(new { Data = htmlString.ToHtmlString() }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetNovaLists()
        {
            //string role = "Coordinator";
            //if (HttpContext.User.IsInRole("Admin"))
            //    role = "Admin";
            //ViewBag.Role = role;

            //var sites = new List<Site>();

            //if (role == "Admin")
            //{
            //    sites = DbUtils.GetSitesActive();
            //    if (sites.Count == 0)
            //        throw new Exception("There was an error retreiving the sites list from the database");
            //    sites.Insert(0, new Site {ID = 0, Name = "Select a site", SiteID = ""});
            //    ViewBag.Sites = new SelectList(sites, "ID", "Name");

            //}

            string siteCode = DbUtils.GetSiteCodeForUser(HttpContext.User.Identity.Name);
            ViewBag.SiteCode = siteCode;

            List<string> files = SsUtils.GetNovaOperatorLists(siteCode);
            files.Insert(0,"Select file");
            ViewBag.Files = new SelectList(files);

            _nlogger.LogInfo("GetNovaLists - user: " + HttpContext.User.Identity.Name);

            return View();
        }

        public FilePathResult GetNovaListDownload(string siteCode, string fileName)
        {


            var folderPath = ConfigurationManager.AppSettings["StatStripListPath"].ToString();
            var path = Path.Combine(folderPath, siteCode);

            var fullpath = Path.Combine(path, fileName);


            _nlogger.LogInfo("GetNovaListDownload: " + fileName);
            return this.File(fullpath, "text/plain", fileName);
        }

        public FilePathResult DownloadStatStripList()
        {
            string siteName = DbUtils.GetSiteNameForUser(User.Identity.Name);
            var folderPath = ConfigurationManager.AppSettings["StatStripListPath"];
            string fileName = siteName + " " + ConfigurationManager.AppSettings["StatStripListName"];
                        
            var fullpath = Path.Combine(folderPath, fileName);

            _nlogger.LogInfo("DownloadStatStripList: " + fileName);
            return File(fullpath, "application/csv", fileName);
        }
    }
}
