using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using hpMvc.Models;
using hpMvc.DataBase;

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
        
        public ActionResult Index()
        {
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
