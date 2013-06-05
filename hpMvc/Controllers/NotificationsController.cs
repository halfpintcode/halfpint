﻿using System;
using System.Web.Mvc;
using hpMvc.DataBase;
using hpMvc.Models;

namespace hpMvc.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        
        public ActionResult Events()
        {
            var events = DbNotificationsUtils.GetNotificationEvents();
            return View(events);
        }

        public ActionResult Event(string id)
        {
            var evnt = DbNotificationsUtils.GetNotificationEvent(id);
            return View(evnt);

        }

        [HttpPost]
        public ActionResult Event(NotificationEvent evnt)
        {
            if (ModelState.IsValid)
            {
                var retVal = DbNotificationsUtils.UpdateNotificationEvent(evnt);
                if (retVal == 1)
                    return RedirectToAction("Events");
                
            }
            return View(evnt);
            
        }

        public ActionResult Add()
        {
            var evnt = new NotificationEvent {Active = true};
            return View(evnt);
        }

        [HttpPost]
        public ActionResult Add(NotificationEvent evnt)
        {
            if (ModelState.IsValid)
            {
                int retVal = DbNotificationsUtils.AddNotificationEvent(evnt);
                if (retVal > 0)
                {
                    
                    TempData.Add("evnt", evnt);
                    return RedirectToAction("AddConfirmation");
                }
                
            }
            return View(evnt);    
            
        }

        public ActionResult AddConfirmation()
        {
            var evnt = TempData["evnt"] as NotificationEvent;
            return View(evnt);
        }

        public ActionResult StaffSubscribe()
        {
            var sites = DbUtils.GetSitesActive();
            if (sites.Count == 0)
                throw new Exception("There was an error retreiving the sites list from the database");
            sites.Insert(0, new Site { ID = 0, Name = "Select a site", SiteID = "" });

            int site = DbUtils.GetSiteidIDForUser(User.Identity.Name);
            ViewBag.Site = site;

            ViewBag.Sites = new SelectList(sites, "ID", "Name", site);

            

// ReSharper disable SpecifyACultureInStringConversionExplicitly
            var list = DbUtils.GetStaffLookupForSite(site.ToString());
// ReSharper restore SpecifyACultureInStringConversionExplicitly
            list.Insert(0, new Site { ID = 0, Name = "Select a member", SiteID = "" });
            ViewBag.Users = new SelectList(list, "ID", "Name");

            ViewBag.IsValid = "true";
            return View();
        }

        public ActionResult GetSubscriptionInfo(string user)
        {
            return PartialView("StaffSubscriptionsPartial");
        }
    }
}
