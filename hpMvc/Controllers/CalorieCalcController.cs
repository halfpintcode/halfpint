using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using hpMvc.DataBase;
using hpMvc.Helpers;
using hpMvc.Models;
using System.Configuration;
using System.Web.Security;

namespace hpMvc.Controllers
{
    [Authorize]
    public class CalorieCalcController : Controller
    {
        //
        // GET: /CalorieCalc/

        public ActionResult EditSelect()
        {
            int siteID = DbUtils.GetSiteidIDForUser(User.Identity.Name);
            List<IDandName> idnl = CalorieCalc.GetCalStudySelectList(siteID);
            idnl.Insert(0, new IDandName { ID = 0, Name = "select" });
            
            ViewBag.CalStudyList = new SelectList(idnl, "ID", "Name");

            return View();
        }

        [HttpPost]
        public JsonResult GetCalctWeight(string studyID)
        {
            double weight = CalorieCalc.GetCalcWeight(int.Parse(studyID)); 
            return Json(weight);
        }

        [HttpPost]
        public JsonResult GetCalcDates(string studyID)
        {
            List<IDandName> idnl = CalorieCalc.GetCalCalcDates(int.Parse(studyID));
            idnl.Insert(0, new IDandName { ID = 0, Name = "select" });
            return Json(idnl);
        }

        [HttpPost]
        public JsonResult GetStudyInfo(string calStudyID)
        {
            CalStudyInfo csi = CalorieCalc.GetCalStudyInfo(calStudyID);
            return Json(csi);
        }

        public ActionResult Index()
        {
            int siteID = DbUtils.GetSiteidIDForUser(User.Identity.Name);
            var studyList = DbUtils.GetRandomizedStudiesForSite(siteID);
            studyList.Insert(0, new IDandStudyID { ID = 0, StudyID = "Select Study" });
            ViewBag.StudyList = new SelectList(studyList, "ID", "StudyID");

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
            
            ViewBag.Mode = "Edit";
            ViewBag.Weight = csi.Weight.ToString();
            ViewBag.CalcDate = csi.CalcDate;
            ViewBag.CalStudyID = csi.ID;

            int siteID = DbUtils.GetSiteidIDForUser(User.Identity.Name);
            //todo remove for production
            if (siteID == 0)
                siteID = 1;

            var studyList = DbUtils.GetRandomizedStudiesForSite(siteID);
            studyList.Insert(0, new IDandStudyID { ID = 0, StudyID = "Select Study" });
            ViewBag.StudyList = new SelectList(studyList, "ID", "StudyID",csi.StudyID);

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

            int calStudyID = CalorieCalc.GetCalStudyID(studyID, calcDate);
            
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

                var siteId = DbUtils.GetSiteidIDForUser(User.Identity.Name);
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
                
                var siteId = DbUtils.GetSiteidIDForUser(User.Identity.Name);
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

        public JsonResult Save(int calStudyID, CalParenteral[] pis, CalInfuseColumn[] cic, CalStudyInfo csi, CalEnteral[] ces, CalAdditive[] cas, CalOtherNutrition con)
        {
            bool isEdit = false;
            if (calStudyID > 0)
                isEdit = true;
            DTO dto = new DTO();

            if (isEdit)
            {
                CalorieCalc.DeleteCurrentEntries(calStudyID);
                csi.ID = calStudyID;

                dto = CalorieCalc.UpdateCalStudyInfo(csi);
                if (dto.ReturnValue == -1)
                {
                    dto.Message = "There was an error updating this caloric entry.  It has been reported to the web master!";
                    return Json(dto);
                }
            }
            else
            {                
                if (CalorieCalc.IsCalStudyInfoDuplicate(csi.StudyID, csi.CalcDate) == 1)
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

            con.CalStudyID = csi.ID;
            if (con.OtherText == null)
                con.OtherText = "";
            dto = CalorieCalc.AddCalOtherNutrition(con);
            //check for any data and if there is data - send email
            if ((con.BreastFeeding) || (con.Drinks) || (con.SolidFoods) || (con.Other) || (con.OtherText.Trim().Length > 0))
            {
                string[] users = ConfigurationManager.AppSettings["NewFormulaNotify"].ToString().Split(new[] { ',' }, StringSplitOptions.None);

                var siteId = DbUtils.GetSiteidIDForUser(User.Identity.Name);
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
                    cid.CalStudyID = csi.ID;
                                        
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
                    pi.CalStudyID = csi.ID;
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
                    ce.CalStudyID = csi.ID;
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
                    ca.CalStudyID = csi.ID;
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
    }
}
