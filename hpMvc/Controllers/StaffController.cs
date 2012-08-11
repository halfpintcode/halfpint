using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using hpMvc.DataBase;
using hpMvc.Models;
using Telerik.Web.Mvc;


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
        public ActionResult Index()
        {
            if (!SiteMapManager.SiteMaps.ContainsKey("staff"))
            {
                SiteMapManager.SiteMaps.Register<XmlSiteMap>("staff", sitmap => sitmap.LoadFrom("~/staff.sitemap"));
            }
            if (!SiteMapManager.SiteMaps.ContainsKey("quick"))
            {
                SiteMapManager.SiteMaps.Register<XmlSiteMap>("quick", sitmap => sitmap.LoadFrom("~/QuickLinks.sitemap"));
            }
            return View();
        }

        public ActionResult Regulatory()
        {
            DirectoryInfo di = new DirectoryInfo(@"C:\Dropbox\HalfPint Website Docs Library\Regulatory");
            var files = from f in di.EnumerateFiles()
                      select new
                      {
                          FileInf = f,
                      };
            foreach (var f in files)
            {
                Console.WriteLine("{0}", f.FileInf.Name );
            }


            var dirs = from d in  di.EnumerateDirectories()
                      select new
                      {
                          ProgDir = d,
                      };

            foreach (var d in dirs)
            {
                Console.WriteLine("{0}", d.ProgDir.Name);
            }

            return View();
        }

        public ActionResult GetInitializePassword()
        {
            int siteID = DbUtils.GetSiteidIDForUser(HttpContext.User.Identity.Name);
            var dict = DbUtils.GetInitializeStudyIDsWithPassword(siteID);
            
            ViewBag.StudyIDList = new SelectList(dict, "Value", "Key");
            return View();

        }

        public ActionResult RecoverChecksSS()
        {
            string siteCode = DbUtils.GetSiteIDForUser(HttpContext.User.Identity.Name);
            var list = ssUtils.GetRanomizedStudyIDs(this.Request.PhysicalApplicationPath, siteCode);
            list.Insert(0, "<Select spreadsheet>");
            ViewBag.SS = new SelectList(list);
            return View();
        }

        public ActionResult DocumentLookup(string name)
        {

            return RedirectToRoute(new { controller = "", action="" });
        }
    }
}
