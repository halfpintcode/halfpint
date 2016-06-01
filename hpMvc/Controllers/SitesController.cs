using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using hpMvc.DataBase;
using hpMvc.Models;

namespace hpMvc.Controllers
{
    [Authorize(Roles = "Admin,DCC") ]
    public class SitesController : Controller
    {
        //
        // GET: /Sites/

        public ActionResult Index()
        {
            var sites = DbUtils.GetSitesAll();
            var sitesOrdered = sites.OrderBy(x => x.SiteId);

            return View(sitesOrdered.ToList());
        }

        public ActionResult SiteDetails(string id)
        {
            var site = DbUtils.GetSiteInfoForSite(id);
            
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "English", Value = "0" });
            items.Add(new SelectListItem { Text = "French", Value = "1" });
            items.Add(new SelectListItem { Text = "Metric", Value = "2" });
            ViewBag.Language = new SelectList(items, "Value", "Text", site.Language);

            return View(site);
        }

        [HttpPost]
        public ActionResult SiteDetails([Bind(Exclude = "HasRandomizations,HasStudyIds")]SiteInfo siteInfo)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "English", Value = "0" });
            items.Add(new SelectListItem { Text = "French", Value = "1" });
            items.Add(new SelectListItem { Text = "Metric", Value = "2" });
            ViewBag.Language = new SelectList(items, "Value", "Text", siteInfo.Language);

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

        public ActionResult AddAdditionalStudyIds(string siteId)
        {
            if (siteId == null)
                siteId = "";
            
            var sites = DbUtils.GetSitesActive();
            sites.Insert(0, new Site { ID = 0, Name = "Select a site", SiteID = "" });
            var selectList = DbUtils.GetSitesActiveListItems(sites);

            var model = new AddAdditionalStudyIdsModel {Sites = selectList, SiteId = siteId};

            return View(model);
        }

        [HttpPost]
        public ActionResult AddAdditionalStudyIds(AddAdditionalStudyIdsModel model, IList<HttpPostedFileBase> files)
        {
            var isError = false;

            if (model.SiteId == "0" || string.IsNullOrEmpty(model.SiteId))
            {
                ModelState.AddModelError("site", "*You must select a site");
                isError = true;
            }

            var siteCode = DbUtils.GetSiteCodeForSite(int.Parse(model.SiteId));

            if (!isError)
            {
                var dto = Business.Site.AddAdditionalStudyIds(files, model, siteCode);

                if (dto.ReturnValue == 0)
                {
                    foreach (var d in dto.Dictionary)
                    {
                        ModelState.AddModelError(d.Key, d.Value);
                    }
                    isError = true;
                }
            }

            if (!isError) return RedirectToAction("AddAdditionalStudyIdsConfirmSuccess");

            var sites = DbUtils.GetSitesActive();
            sites.Insert(0, new Site { ID = 0, Name = "Select a site", SiteID = "" });
            var selectList = DbUtils.GetSitesActiveListItems(sites);
            model.Sites = selectList;

            return View(model);
        }

        public ActionResult AddAdditionalStudyIdsConfirmSuccess()
        {
            return View();
        }

        public ActionResult Add()
        {
            var siteInfo = DbUtils.GetSiteInfoForNewSite();
            siteInfo.IsActive = true;
            siteInfo.UseCalfpint = true;
            siteInfo.UseVampjr = true;
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "English", Value = "0" });
            items.Add(new SelectListItem { Text = "French", Value = "1" });
            items.Add(new SelectListItem { Text = "Metric", Value = "2" });
            ViewBag.Language = items;
            return View(siteInfo);
        }

        [HttpPost]
        public ActionResult Add(IList<HttpPostedFileBase> files, [Bind(Exclude = "Id")]SiteInfo siteInfo)
        {
            if (ModelState.IsValid)
            {
                //everyone is using dexcom at the moment
                //we might want to let dcc select this in the future
                siteInfo.Sensor = 2;
                siteInfo.PhoneFormat = "999-999-9999";
                List<SelectListItem> items = new List<SelectListItem>();
                items.Add(new SelectListItem { Text = "English", Value = "0" });
                items.Add(new SelectListItem { Text = "French", Value = "1" });
                items.Add(new SelectListItem { Text = "Metric", Value = "2" });
                ViewBag.Language = items;

                var retVal = Business.Site.Add(files, siteInfo, Request.Url);
                if (retVal.ReturnValue == 0)
                {
                    if (retVal.Dictionary.Count > 0)
                    {
                        foreach (var s in retVal.Dictionary)
                        {
                            ModelState.AddModelError(s.Key, s.Value);
                        }
                    }
                    return View(siteInfo);
                }

                if (retVal.IsSuccessful)
                {
                    TempData.Add("siteInfo", siteInfo);
                    return RedirectToAction("AddConfirmation");
                }
                else
                    return View(siteInfo);
            }
            else
            {
                return View(siteInfo);
            }
        }

        public ActionResult AddConfirmation()
        {
            var siteInfo = TempData["siteInfo"] as SiteInfo;
            return View(siteInfo);
        }

        public ActionResult InsulinConcentrations()
        {
            var icl = DbUtils.GetInsulinConcentrations();
            var iclOrdered = icl.OrderBy(x => x.Concentration);
            return View(iclOrdered.ToList());
        }


        public ActionResult AddInsulinConcentration()
        {
            var ic = new InsulinConcentration(); 

            return View(ic);
        }

        [HttpPost]
        public ActionResult AddInsulinConcentration([Bind(Exclude = "Id, IsUsed")]InsulinConcentration ic)
        {
            if(ModelState.IsValid)
            {
                int id = DbUtils.AddInsulinConcentrations(ic);
                if(id >0)
                    return RedirectToAction("InsulinConcentrations");

            }

            return View(ic);
        }
        
        public ActionResult EditInsulinConcentration(int id)
        {
            var ic = DbUtils.GetInsulinConcentration(id);

            return View(ic);
        }

        [HttpPost]
        public ActionResult EditInsulinConcentration([Bind(Exclude = "IsUsed")]InsulinConcentration ic)
        {
            if (ModelState.IsValid)
            {
                int id = DbUtils.UpdateInsulinConcentrations(ic);
                if (id > 0)
                    return RedirectToAction("InsulinConcentrations");

            }

            return View(ic);
        }
    }
}
