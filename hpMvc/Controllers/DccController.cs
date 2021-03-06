﻿using System;
using System.Collections.Generic;
using System.Web.Mvc;
using hpMvc.DataBase;
using System.Web.Helpers;
using hpMvc.Models;

namespace hpMvc.Controllers
{
    [Authorize(Roles = "Admin,DCC")]
    public class DccController : Controller
    {
        //
        // GET: /Dcc/

        public ActionResult Index()
        {
            return View();
        }
                
        public ActionResult RandomizedStudies(string siteID)
        {
            List<Site> sites = new List<Site>();

            sites = DbUtils.GetSitesActive();
            if (sites.Count == 0)
                throw new Exception("There was an error retreiving the sites list from the database");
            sites.Insert(0, new Site { ID = 0, Name = "Select a site", SiteID = "" });
            ViewBag.Sites = new SelectList(sites, "ID", "Name");

            List<Randomization> list = null;
            if (string.IsNullOrEmpty(siteID))
            {
                list = DbUtils.GetAllRandomizedStudies();
            }
            else
            {
                list = DbUtils.GetSiteRandomizedStudies(int.Parse(siteID));
            }
                        
            return View(list);
        }

        public JsonResult GetSiteRandomizedStudies(string siteID)
        {
            var list = DbUtils.GetSiteRandomizedStudies(int.Parse(siteID));
            var grid = new WebGrid(list, defaultSort: "Number", rowsPerPage: 50);
            var htmlString = grid.GetHtml(tableStyle:"webgrid",
                            columns:grid.Columns(
                            grid.Column("SiteName", header:"Site"),
                            grid.Column("Number"),
                            grid.Column("StudyID", header: "Study  ID"),
                            grid.Column("Arm"),
                            grid.Column("DateRandomized", header:"Date Randomized", format: x => x.DateRandomized.ToString("MM/dd/yyyy hh:mm tt"))));

            return Json(new { Data = htmlString.ToHtmlString(), Count = list.Count }, JsonRequestBehavior.AllowGet);
        }
    }
}
