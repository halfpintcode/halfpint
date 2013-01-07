﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using hpMvc.DataBase;
using hpMvc.Models;

namespace hpMvc.Controllers
{
    //[Authorize]
    public class SitesController : Controller
    {
        //
        // GET: /Sites/

        public ActionResult Index()
        {
            var sites = DbUtils.GetSitesAll();

            return View(sites);
        }

        public ActionResult SiteDetails(string id)
        {
            var site = DbUtils.GetSiteInfoForSite(id);
            return View(site);
        }

        [HttpPost]
        public ActionResult SiteDetails(SiteInfo siteInfo)
        {
            if (ModelState.IsValid)
            {
                var retVal = DbUtils.SaveSiteInfo(siteInfo);
                if (retVal.IsSuccessful)
                    return RedirectToAction("Index");
                else
                    return View(siteInfo);
            }
            else
            {
                return View(siteInfo);
            }
        }

        public ActionResult Add()
        {
            var siteInfo = DbUtils.GetSiteInfoForNewSite();
            return View(siteInfo);
        }

        [HttpPost]
        public ActionResult Add(IEnumerable<HttpPostedFileBase> files, SiteInfo siteInfo)
        {
            if (ModelState.IsValid)
            {
                var retVal = DbUtils.AddSiteInfo(siteInfo);
                if (retVal.IsSuccessful)
                    return RedirectToAction("Index");
                else
                    return View(siteInfo);
            }
            else
            {
                return View(siteInfo);
            }
        }
    }
}
