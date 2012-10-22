using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using hpMvc.Models;
using hpMvc.DataBase;
using hpMvc.Infrastructure.Logging;
using hpMvc.Infrastructure;
using System.IO;
using System.Configuration;
using Telerik.Web.Mvc;

namespace hpMvc.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class AdminBackDoorController : Controller
    {
        NLogger nlogger = new NLogger();

        [Telerik.Web.Mvc.PopulateSiteMap(SiteMapName = "adminbackdoor", ViewDataKey = "adminbackdoor")]
        public ActionResult Index()
        {
            List<Site> sites = new List<Site>();
                        
            sites = DbUtils.GetSites();
            if (sites.Count == 0)
                throw new Exception("There was an error retreiving the sites list from the database");
            sites.Insert(0, new Site { ID = 0, Name = "Select a site", SiteID = "" });
            ViewBag.Sites = new SelectList(sites, "ID", "Name");

            nlogger.LogInfo("Admin Backdoor Index - user: " + HttpContext.User.Identity.Name);

            List<MembershipUser> users = new List<MembershipUser>();
            users = AccountUtils.GetAllUsers();
            
            ViewBag.Users = new SelectList(users, "UserName", "UserName");
            if (!SiteMapManager.SiteMaps.ContainsKey("adminbackdoor"))
            {
                SiteMapManager.SiteMaps.Register<XmlSiteMap>("adminbackdoor", sitmap => sitmap.LoadFrom("~/adminbackdoor.sitemap"));
            }
            return View();
        }

    }
}
