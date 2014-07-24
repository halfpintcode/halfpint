using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using hpMvc.DataBase;
using hpMvc.Models;
using Telerik.Web.Mvc;
using System.Configuration;


namespace hpMvc.Controllers
{
    
    public class StaffController : Controller
    {
        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // auth failed, redirect to login page 
                filterContext.Result = new HttpUnauthorizedResult();
                return;
            }

            //if (!filterContext.HttpContext.User.IsInRole("Admin"))
            //{
            //    filterContext.Controller.TempData.Add("RedirectReason", "You are not authorized to access this page.");
            //    filterContext.Result = new RedirectResult("~/Error/Unauthorized");

            //    return;
            //}

            base.OnAuthorization(filterContext);
        }

        [Telerik.Web.Mvc.PopulateSiteMap(SiteMapName = "staff", ViewDataKey = "staff")]
        [Telerik.Web.Mvc.PopulateSiteMap(SiteMapName = "quick", ViewDataKey = "quick")]
        public ActionResult Index(string folder)
        {
            if (!SiteMapManager.SiteMaps.ContainsKey("staff"))
            {
                SiteMapManager.SiteMaps.Register<XmlSiteMap>("staff", sitmap => sitmap.LoadFrom("~/staff.sitemap"));
            }
            if (!SiteMapManager.SiteMaps.ContainsKey("quick"))
            {
                SiteMapManager.SiteMaps.Register<XmlSiteMap>("quick", sitmap => sitmap.LoadFrom("~/QuickLinks2.sitemap"));
            }

            ViewBag.ShowSurvey = "false";
            if(folder != null)
            {
                if (folder == "NurseSurvey")
                {
                    ViewBag.ShowSurvey = "true";
                }
                else
                {
                    string path = ConfigurationManager.AppSettings["FileRepositoryPath"].ToString();
                    path = Path.Combine(path, folder);

                    string user = User.Identity.Name;
                    var list = DynamicFolderFile.GetFileFolderModel(path, folder, user);

                    ViewBag.ShowFolder = "true";
                    ViewBag.FolderName = folder;
                    
                    return View(list);
                }
            }
            
            ViewBag.ShowFolder = "false";
            
            return View();
        }        

        public JsonResult GetSiteEmployeeInfo(string site)
        {
            var retDto = DbPostTestsUtils.GetSiteEmployeeInfoForSite(site);
            return Json(retDto);
        }

        public ActionResult NurseWorkloadSurvey()
        {
            return View();
        }

        //public ActionResult FolderDisplay(string folder)
        //{
        //    var folderList = new List<StaffFolder>();

        //    string path = ConfigurationManager.AppSettings["FileRepositoryPath"].ToString();
        //    path = Path.Combine(path, folder);
        //    GetFolderFiles(path, folder, folderList);
        //    ViewBag.FolderName = folder;
            
        //    return View(folderList);
        //}

        //public ActionResult FolderDisplayTree(string folder)
        //{            
        //    string path = ConfigurationManager.AppSettings["FileRepositoryPath"].ToString();
        //    path = Path.Combine(path, folder);
            
        //    ViewBag.FolderName = folder;

        //    string user = User.Identity.Name;
        //    var list = DynamicFolderFile.GetFileFolderModel(path, folder, user);
        //    return View(list);
        //}
        
        private void GetFolderFiles(string path, string topFolder, List<StaffFolder> folderList)
        {  
            //DirectoryInfo di = new DirectoryInfo(path);
            string staffFolder = topFolder;
                        
            string[] parts = path.Split(new string[] { "\\" }, StringSplitOptions.None);
            int iStart =100;
            for (int i = 3; i < parts.Length; i++)
            {
                if (parts[i] == topFolder)
                {
                    iStart = i;
                }
                if (i > iStart)
                    staffFolder += "/" + parts[i];

            }
            
            var sf = new StaffFolder(staffFolder);
            List<StaffFile> files = new List<StaffFile>();
            sf.Files = files;
            folderList.Add(sf);

            string fileFolder = staffFolder.Replace("/", "~");

            foreach (string f in Directory.GetFiles(path))
            {
                StaffFile file = new StaffFile(Path.GetFileName(f));
                file.FolderName = fileFolder;
                files.Add(file);
            } 

            foreach (string d in Directory.GetDirectories(path))
                GetFolderFiles(d, topFolder, folderList);                       

        }
                
        public ActionResult GetInitializePassword()
        {
            int siteID = DbUtils.GetSiteidIdForUser(HttpContext.User.Identity.Name);
            var dict = DbUtils.GetInitializeStudyIDsWithPassword(siteID);
            
            ViewBag.StudyIDList = new SelectList(dict, "Value", "Key");
            return View();

        }

        public ActionResult RecoverChecksSS()
        {
            string siteCode = DbUtils.GetSiteCodeForUser(HttpContext.User.Identity.Name);
            var list = SsUtils.GetRanomizedStudyIDs(this.Request.PhysicalApplicationPath, siteCode);
            list.Insert(0, "<Select spreadsheet>");
            ViewBag.SS = new SelectList(list);
            return View();
        }
        
    }
}
