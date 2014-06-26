using System;
using System.Collections.Generic;
using System.Web.Mvc;
using hpMvc.DataBase;
using hpMvc.Helpers;
using hpMvc.Models;

namespace hpMvc.Controllers
{
    [Authorize]
    public class CalorieCalcController : Controller
    {
        //
        // GET: /CalorieCalc/

        public ActionResult EditSelect()
        {
            int siteId = DbUtils.GetSiteidIdForUser(User.Identity.Name);
            var idnl = CalorieCalc.GetCalStudySelectList(siteId);
            idnl.Insert(0, new IDandName { ID = 0, Name = "select" });
            
            ViewBag.CalStudyList = new SelectList(idnl, "ID", "Name");

            return View();
        }

        [HttpPost]
        public JsonResult GetCalcWeight(string studyId)
        {
            int id = int.Parse(studyId);
            double weight = CalorieCalc.GetCalcWeight(id);
            return Json(weight);
        }

        [HttpPost]
        public JsonResult GetStudyDay(string studyId, string calcDate)
        {
            int id = int.Parse(studyId);
            int day = CalorieCalc.GetStudyDay(id, DateTime.Parse(calcDate));
            return Json(day);
        }

        [HttpPost]
        public JsonResult GetCalcDates(string studyId)
        {
            List<IDandName> idnl = CalorieCalc.GetCalCalcDates(int.Parse(studyId));
            idnl.Insert(0, new IDandName { ID = 0, Name = "select" });
            return Json(idnl);
        }

        [HttpPost]
        public JsonResult GetStudyInfo(string calStudyId)
        {
            var csi = CalorieCalc.GetCalStudyInfo(calStudyId);
            return Json(csi);
        }

        public ActionResult Index()
        {
            int siteId = DbUtils.GetSiteidIdForUser(User.Identity.Name);
            var studyList = DbUtils.GetRandomizedStudiesForSite(siteId);
            studyList.Insert(0, new IDandStudyID { ID = 0, StudyID = "Select Study" });
            ViewBag.StudyList = new SelectList(studyList, "ID", "StudyID");
            ViewBag.StudyDay = "";

            var dc1 = CalorieCalc.GetDextroseConcentrations();
            var dc = new DextroseConcentration{ ID=0, Concentration=" Dextrose % ", Kcal_ml=0};
            dc1.Insert(0, dc);

            ViewBag.DexCons1 = new SelectList(dc1, "Kcal_ml", "Concentration");
            ViewBag.DexCons2 = new SelectList(dc1, "Kcal_ml", "Concentration");
            ViewBag.DexCons3 = new SelectList(dc1, "Kcal_ml", "Concentration");
            ViewBag.DexCons4 = new SelectList(dc1, "Kcal_ml", "Concentration");
            
            List<IDandName> sl = DbUtils.GetLookupItems("FormulaList");
            if (sl.Count == 0)
                throw new Exception("There was an error retrieving the FormulaList from the database");
            sl.Insert(0, new IDandName { ID = 0, Name = "Select" });
            ViewBag.FormulaList = new SelectList(sl, "ID", "Name");

            List<IDandName> sl2 = DbUtils.GetLookupItems("AdditiveList");
            if (sl2.Count == 0)
                throw new Exception("There was an error retrieving the Additives from the database");
            sl2.Insert(0, new IDandName { ID = 0, Name = "Select" });
            ViewBag.AdditiveList = new SelectList(sl2, "ID", "Name");

            List<IDandName> sl3 = DbUtils.GetLookupItems("Units");
            if (sl3.Count == 0)
                throw new Exception("There was an error retrieving the Units from the database");
            sl3.Insert(0, new IDandName { ID = 0, Name = "Select" });
            ViewBag.Units = new SelectList(sl3, "ID", "Name");
            
            return View();
        }
        
        public ActionResult Edit(CalStudyInfo csi)
        {
            
            //DateTime date = DateTime.Parse( csi.CalcDate);

            ViewBag.Mode = "Edit";
            ViewBag.Weight = csi.Weight.ToString();
            ViewBag.CalcDate = csi.CalcDate;
            ViewBag.CalStudyID = csi.Id;
            ViewBag.Hours = csi.Hours;
            int studyDay = CalorieCalc.GetStudyDay(csi.StudyId, DateTime.Parse(csi.CalcDate));
            ViewBag.StudyDay = studyDay.ToString();

            int siteId = DbUtils.GetSiteidIdForUser(User.Identity.Name);
            //todo remove for production
            //if (siteId == 0)
            //    siteId = 1;

            var studyList = DbUtils.GetRandomizedStudiesForSite(siteId);
            studyList.Insert(0, new IDandStudyID { ID = 0, StudyID = "Select Study" });
            ViewBag.StudyList = new SelectList(studyList, "ID", "StudyID",csi.StudyId);

            var dc1 = CalorieCalc.GetDextroseConcentrations();
            var dc = new DextroseConcentration { ID = 0, Concentration = " Dextrose % ", Kcal_ml = 0 };
            dc1.Insert(0, dc);

            ViewBag.DexCons1 = new SelectList(dc1, "Kcal_ml", "Concentration");
            ViewBag.DexCons2 = new SelectList(dc1, "Kcal_ml", "Concentration");
            ViewBag.DexCons3 = new SelectList(dc1, "Kcal_ml", "Concentration");
            ViewBag.DexCons4 = new SelectList(dc1, "Kcal_ml", "Concentration");

            List<IDandName> sl = DbUtils.GetLookupItems("FormulaList");
            if (sl.Count == 0)
                throw new Exception("There was an error retrieving the FormulaList from the database");
            sl.Insert(0, new IDandName { ID = 0, Name = "Select" });
            ViewBag.FormulaList = new SelectList(sl, "ID", "Name");

            List<IDandName> sl2 = DbUtils.GetLookupItems("AdditiveList");
            if (sl2.Count == 0)
                throw new Exception("There was an error retrieving the Additives from the database");
            sl2.Insert(0, new IDandName { ID = 0, Name = "Select" });
            ViewBag.AdditiveList = new SelectList(sl2, "ID", "Name");

            List<IDandName> sl3 = DbUtils.GetLookupItems("Units");
            if (sl3.Count == 0)
                throw new Exception("There was an error retrieving the Units from the database");
            sl3.Insert(0, new IDandName { ID = 0, Name = "Select" });
            ViewBag.Units = new SelectList(sl3, "ID", "Name");

            return View("Index");
        }

        public JsonResult GetAllData(string studyID, string calcDate)
        {
            var cad = new CalAllData();
            var cicl = new List<CalInfuseColumn>();

            int calStudyID = CalorieCalc.GetCalStudyId(studyID, calcDate);
            
            var cidl = CalorieCalc.GetCalInfusionsDexData(calStudyID);

            foreach(var cid in cidl)
            {
                List<CalInfusionVol> civl = CalorieCalc.GetCalInfusionsVolData(cid.ID);
                int[] volumes = new int[civl.Count];
                int index = 0;
                var cic = new CalInfuseColumn();

                foreach (var civ in civl)
                {
                    
                    cic.DexValue = cid.DexVal;
                    volumes[index] = civ.Volume;
                    index++;
                }
                cic.Volumes = volumes;
                cicl.Add(cic);
            }

            var cpl = CalorieCalc.GetCalParenteralsData(calStudyID);
            var cel = CalorieCalc.GetCalEnteralsData(calStudyID);
            var cal = CalorieCalc.GetCalAdditivesData(calStudyID);
            var con = CalorieCalc.GetCalOtherNutrition(calStudyID);


            cad.calInfusionCol = cicl;
            cad.calParenterals = cpl;
            cad.calEnterals = cel;
            cad.calAdditives = cal;
            cad.calOtherNutrition = con;

            return Json(cad);
        }

        public JsonResult GetFormulaData()
        {
            var efl = CalorieCalc.GetFormulaList();
            return Json(efl);
        }

        public JsonResult GetAdditiveData()
        {
            var addl = CalorieCalc.GetAdditiveList();
            return Json(addl);
        }

        public JsonResult AddNewFormula(EnteralFormula ef)
        {
            DTO dto = new DTO();

            if (CalorieCalc.IsFormulaNameDuplicate(ef.Name) == 1)
            {
                dto.ReturnValue = 0;
                dto.Message = ef.Name + " is already in the list!";
            }
            else
            {
                dto = CalorieCalc.AddFormula(ef);

                var siteId = DbUtils.GetSiteidIdForUser(User.Identity.Name);
                var staff = NotificationUtils.GetStaffForEvent(4, siteId);
                
                string siteName = DbUtils.GetSiteNameForUser(User.Identity.Name);

                var u = new UrlHelper(this.Request.RequestContext);
                string url = "http://" + this.Request.Url.Host + u.RouteUrl("Default", new { Controller = "Account", Action = "Logon" });
            
                Utility.SendFormulaAddeddMail(staff.ToArray(), null, ef, User.Identity.Name, siteName, Server, url);
            }
           
            dto.Bag = ef;
            return Json(dto);
        }

        public JsonResult AddNewAdditive(Additive add)
        {
            DTO dto = new DTO();

            if (CalorieCalc.IsAdditiveNameDuplicate(add.Name) == 1)
            {
                dto.ReturnValue = 0;
                dto.Message = add.Name + " is already in the list!";
            }
            else
            {
                dto = CalorieCalc.AddAdditive(add);
                
                var siteId = DbUtils.GetSiteidIdForUser(User.Identity.Name);
                var staff = NotificationUtils.GetStaffForEvent(5, siteId);
                
                string siteName = DbUtils.GetSiteNameForUser(User.Identity.Name);

                var u = new UrlHelper(this.Request.RequestContext);

                string url = "http://" + this.Request.Url.Host + u.RouteUrl("Default", new { Controller = "Account", Action = "Logon" });            
                Utility.SendAdditiveAddeddMail(staff.ToArray(), null, add, User.Identity.Name, siteName, Server, url);
            }

            if (dto.ReturnValue == -1)
                dto.Message = "There was an error adding this additive.  It has been reported to the web master!";
            
            dto.Bag = add;
            return Json(dto);
        }

        public JsonResult IsCaclDateDuplicate(int studyId, string calcDate)
        {
            var retVal = CalorieCalc.IsCalStudyInfoDuplicate(studyId, calcDate); 
            
            return Json(new {val= retVal});
        }

        public JsonResult Save(int calStudyID, CalParenteral[] pis, CalInfuseColumn[] cic, CalStudyInfo csi, CalEnteral[] ces, CalAdditive[] cas, CalOtherNutrition con)
        {
            bool isEdit = false || calStudyID > 0;
            var dto = new DTO();

            if (isEdit)
            {
                //get the original calcdate and see if it has been changed
                //if changed then check for duplicate


                CalorieCalc.DeleteCurrentEntries(calStudyID);
                csi.Id = calStudyID;

                dto = CalorieCalc.UpdateCalStudyInfo(csi);
                if (dto.ReturnValue == -1)
                {
                    dto.Message = "There was an error updating this caloric entry.  It has been reported to the web master!";
                    return Json(dto);
                }
            }
            else
            {                
                if (CalorieCalc.IsCalStudyInfoDuplicate(csi.StudyId, csi.CalcDate) == 1)
                {
                    dto.ReturnValue = 0;
                    dto.Message = "This study id for the date, " + csi.CalcDate + ", has already been entered!";

                    return Json(dto);
                }

                dto = CalorieCalc.AddCalStudyInfo(csi);
                if (dto.ReturnValue == -1)
                {
                    dto.Message = "There was an error adding this caloric entry.  It has been reported to the web master!";
                    return Json(dto);
                }
            }

            con.CalStudyId = csi.Id;
            if (con.OtherText == null)
                con.OtherText = "";
            dto = CalorieCalc.AddCalOtherNutrition(con);
            //check for any data and if there is data - send email
            if ((con.BreastFeeding) || (con.Drinks) || (con.SolidFoods) || (con.Other) || (con.OtherText.Trim().Length > 0))
            {
                //string[] users = ConfigurationManager.AppSettings["NewFormulaNotify"].ToString().Split(new[] { ',' }, StringSplitOptions.None);

                var siteId = DbUtils.GetSiteidIdForUser(User.Identity.Name);
                var staff = NotificationUtils.GetStaffForEvent(6, siteId);
                
                string siteName = DbUtils.GetSiteNameForUser(User.Identity.Name);

                var u = new UrlHelper(this.Request.RequestContext);
                string url = "http://" + this.Request.Url.Host + u.RouteUrl("Default", new { Controller = "Account", Action = "Logon" });
            
                Utility.SendOtherNutritionMail(staff.ToArray(), null, con, csi, User.Identity.Name, siteName,Server, url);
            }

            if (dto.ReturnValue == -1)
            {
                dto.Message = "There was an error adding other nutrition.  It has been reported to the web master!";
                return Json(dto);
            }

            if (cic != null)
            {
                foreach (var ci in cic)
                {
                    if (ci.Volumes == null)
                        continue;
                    var cid = new CalInfusionDex();
                    cid.DexVal = ci.DexValue;
                    cid.CalStudyID = csi.Id;
                                        
                    dto = CalorieCalc.AddCalInfusionDex(cid);
                    if (dto.ReturnValue == -1)
                    {
                        dto.Message = "There was an error adding a infusion dextrose entry.  It has been reported to the web master!";
                        return Json(dto);
                    }
                                        
                    foreach (var vol in ci.Volumes)
                    {
                        var civ = new CalInfusionVol();
                        civ.DexID = cid.ID;
                        civ.Volume = vol;
                        CalorieCalc.AddCalInfusionVol(civ);
                        if (dto.ReturnValue == -1)
                        {
                            dto.Message = "There was an error adding a infusion volume entry.  It has been reported to the web master!";
                            return Json(dto);
                        }
                    }
                }
            }

            if (pis != null)
            {
                foreach (var pi in pis)
                {
                    pi.CalStudyID = csi.Id;
                    dto = CalorieCalc.AddCalParenteral(pi);
                    if (dto.ReturnValue == -1)
                    {
                        dto.Message = "There was an error adding a parenteral entry.  It has been reported to the web master!";
                        return Json(dto);
                    }
                }
            }

            if (ces != null)
            {
                foreach (var ce in ces)
                {
                    ce.CalStudyID = csi.Id;
                    dto = CalorieCalc.AddCalEnteral(ce);
                    if (dto.ReturnValue == -1)
                    {
                        dto.Message = "There was an error adding an enteral entry.  It has been reported to the web master!";
                        return Json(dto);
                    }
                }
            }

            if (cas != null)
            {
                foreach (var ca in cas)
                {
                    ca.CalStudyID = csi.Id;
                    dto = CalorieCalc.AddCalAdditive(ca);
                    if (dto.ReturnValue == -1)
                    {
                        dto.Message = "There was an error adding an additive entry.  It has been reported to the web master!";
                        return Json(dto);
                    }
                }
            }

            dto.ReturnValue = 1;
            dto.Message = "Caloric Entries were added successfully!";
            return Json(dto);
        }
    
        public ActionResult FormulaList()
        {
            string role = "";
            
            if (HttpContext.User.IsInRole("Admin"))
                role = "Admin";

            ViewBag.Role = role;
            var list = CalorieCalc.GetFormulaList();
            return View(list);
        }

        public ActionResult AdditiveList()
        {
            string role = "";

            if (HttpContext.User.IsInRole("Admin"))
                role = "Admin";

            ViewBag.Role = role;
            var list = CalorieCalc.GetAdditiveList();
            return View(list);
        }

        public ActionResult FormulaDetails(string id)
        {
            var formula = CalorieCalc.GetFormula(id);
            return View(formula);
        }

        public ActionResult AdditiveDetails(string id)
        {
            IEnumerable<IDandName> units = DbUtils.GetLookupItems("Units");
            var additive = CalorieCalc.GetAdditive(id);
            additive.Units = units;
            
            return View(additive);
        }

        [HttpPost]
        public ActionResult AdditiveDetails(Additive additive)
        {
            if (ModelState.IsValid)
            {
                var dto = CalorieCalc.UpdateAdditive(additive);
                if (dto.ReturnValue == 1)
                {
                    TempData.Add("additive", additive);
                    return RedirectToAction("AdditiveConfirmation");
                }
            }
            IEnumerable<IDandName> units = DbUtils.GetLookupItems("Units");
            additive.Units = units;
            return View(additive);
        }

        [HttpPost]
        public ActionResult FormulaDetails(EnteralFormula enteralFormula)
        {
            if (ModelState.IsValid)
            {
                var dto = CalorieCalc.UpdateFormula(enteralFormula);
                if (dto.ReturnValue == 1)
                {
                    TempData.Add("enteralFormula", enteralFormula);
                    return RedirectToAction("FormulaConfirmation");
                }
            }
            return View(enteralFormula);
        }

        public ActionResult AdditiveConfirmation()
        {
            var additive = TempData["additive"] as Additive;
            return View(additive);
        }

        public ActionResult FormulaConfirmation()
        {
            var enteralFormula = TempData["enteralFormula"] as EnteralFormula;
            return View(enteralFormula);
        }
    }
}
