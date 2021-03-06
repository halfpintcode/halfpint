﻿using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using DocumentFormat.OpenXml.Wordprocessing;
using hpMvc.DataBase;
using hpMvc.Helpers;
using hpMvc.Infrastructure.Logging;
using hpMvc.Models;
using Microsoft.Security.Application;

namespace hpMvc.Controllers
{
    [Authorize]
    public class PostTestsController : Controller
    {
        readonly NLogger _logger = new NLogger();
        
        public ActionResult Initialize(string id)
        {
            var model = new PostTestsInitializeModel();
            var role = AccountUtils.GetRoleForUser(User.Identity.Name);
            model.Role = role;

            if (role != "Nurse")
            {
                if (id == "0")
                {
                    var dto = DbPostTestsUtils.GetStaffIdByUserName(User.Identity.Name);
                    id = dto.ReturnValue.ToString(CultureInfo.InvariantCulture);
                }
            }
            if (id =="-1")
                return RedirectToRoute(new {Controller = "Staff"});

            model.UserId = int.Parse(id);
            
            var si = DbUtils.GetSiteInfoForUser(User.Identity.Name);
            model.SiteId = si.Id;
            model.SiteCode = si.SiteId;
            model.Language = si.Language;

            var users = DbPostTestsUtils.GetStaffTestUsersForSite(model.SiteId);

            users.Insert(0, new IDandName(0, "Select Your Name"));

            //check if employee id required
            var retDto = DbPostTestsUtils.CheckIfEmployeeIdRequired(User.Identity.Name);
            model.EmpIdRequired = retDto.Stuff.EmpIDRequired;
            model.EmpIdRegex = retDto.Stuff.EmpIDRegex;
            model.EmpIdMessage = retDto.Stuff.EmpIDMessage;
            
            ViewBag.Users = new SelectList(users, "ID", "Name", id);
            if (id != "0")
            {
                model.Email = DbPostTestsUtils.GetPostTestStaffEmail(id);
            }
            
            return View(model);
        }
                        
        public JsonResult CreateName()
        {
            var dto = new DTO();

            var siteId = DbUtils.GetSiteidIdForUser(HttpContext.User.Identity.Name);
            var lastName = Request.Params["LastName"];
            var firstName = Request.Params["FirstName"];            
            var empId = Request.Params["EmpID"];
            var email = Request.Params["Email"];

            dto.ReturnValue = DbPostTestsUtils.DoesStaffNameExist(lastName, firstName, siteId);
            if (dto.ReturnValue != 0)
            {
                if (dto.ReturnValue == -1)
                    dto.Message = "There was an error in determinig if this name was already in the database.";
                if (dto.ReturnValue == 0)
                    dto.Message = "This name already exists. Select your name from the drop down list." ;

                _logger.LogInfo("PostTests.CreateName - message: " + dto.Message + ", name: " + lastName + "," + firstName + ", site: " + siteId.ToString(CultureInfo.InvariantCulture));
                return Json(dto);               
            }

            dto.ReturnValue = DbPostTestsUtils.AddNurseStaff(lastName, firstName, empId, siteId, email);
            
            var staff = NotificationUtils.GetStaffForEvent(3, siteId);
                
            string siteName = DbUtils.GetSiteNameForUser(User.Identity.Name);
            var u = new UrlHelper(Request.RequestContext);
            Debug.Assert(Request.Url != null, "Request.Url != null");
            var url = "http://" + Request.Url.Host + u.RouteUrl("Default", new { Controller = "Account", Action = "Logon" });

            Utility.SendNurseAccountCreatedMail(staff.ToArray(), new[] { Request.Params["Email"] }, firstName + " " + lastName, siteName, empId, Server, url);

            _logger.LogInfo("PostTests.CreateName - message: " + dto.Message + ", name: " + lastName + "," + firstName + ", site: " + siteId.ToString(CultureInfo.InvariantCulture));
            return Json(dto);
        }

        public JsonResult IsUserEmailDuplicate(string email)
        {
            var dto = DbPostTestsUtils.DoesStaffEmailExist(email);
            return Json(dto);
        }

        public JsonResult IsUserEmployeeIdDuplicate(string employeeId)
        {            
            var site = DbUtils.GetSiteidIdForUser(User.Identity.Name);
            var dto = DbPostTestsUtils.DoesStaffEmployeeIdExist(employeeId, site);

            //if site is dallas
            if (site == 13)
            {
                //if this is a duplicate then get the next non-dupelicate number
                if (dto.ReturnValue == 1)
                {
                    string nexNumber = DbPostTestsUtils.GetNextStaffEmployeeId(employeeId, site);
                    dto.Bag = nexNumber;
                }
            }
            return Json(dto);
        }
        
        //new procedure
        public JsonResult GetTestsCompletedActive()
        {
            var staffId = Request.Params["ID"];
            var siteCode = Request.Params["SiteCode"];
            //var siteLanguage = Request.Params["SiteLanguage"];
            var tests = DbPostTestsUtils.GetStaffPostTestsCompletedCurrentAndActive(staffId, siteCode);
            var email = DbPostTestsUtils.GetPostTestStaffEmail(staffId);

            var retVal = new {email, tests };
            return Json(retVal);
        }

        public JsonResult Submit()
        {
            var id = Request.Params["ID"];
            var name = Request.Params["Name"];
            var email = new[] {Request.Params["Email"]};
            //var empId = Request.Params["EmployeeId"]; 
            
            var tests = DbPostTestsUtils.GetTestsCompleted(id);
            if(tests.Count == 0)
                return Json("no tests");
            
            var site = DbUtils.GetSiteidIdForUser(HttpContext.User.Identity.Name);
            var staff = NotificationUtils.GetStaffForEvent(7, site);
            
            string siteName = DbUtils.GetSiteNameForUser(User.Identity.Name);

            var u = new UrlHelper(Request.RequestContext);
            Debug.Assert(Request.Url != null, "Request.Url != null");
            var url = "http://" + Request.Url.Host + u.RouteUrl("Default", new { Controller = "Account", Action = "Logon" });            
            
            Utility.SendPostTestsSubmittedMail(staff.ToArray(), email, tests, name, siteName, Server, url);

            _logger.LogInfo("Post-tests submitted: " + name);
            return Json("");
        }

        public ActionResult Checks(string id, string name, string language)
        {
            id = Encoder.HtmlEncode(id);
            name = Encoder.HtmlEncode(name);
            ViewBag.Name = name;
            ViewBag.Test = "Checks";
            ViewBag.ID = id;

            ViewBag.Completed = Request.Params["completed"] != null ? "true" : "false";
            if(language == "1")
                return View("ChecksF");
            return View();
        }
        
        [HttpPost]        
        public JsonResult Checks()
        {
            var id = Request.Params["id"];
            var name = Request.Params["name"];
            id = Encoder.HtmlEncode(id);
            name = Encoder.HtmlEncode(name);

            var dto = DbPostTestsUtils.VerifyPostTest("Checks", Request.Params);
            if (dto.IsSuccessful)
            {
                //get the person id
                var nameId = int.Parse(id);
                //save test as completed
                DbPostTestsUtils.AddAndUpdateTestCompleted(nameId, "Checks");
            }


            var incorrect = "";
            if (dto.Messages.Count > 0)
            {
                incorrect = dto.Messages.Aggregate(incorrect, (current, s) => current + (s + ","));
                incorrect = incorrect.Substring(0, incorrect.Length - 1);
            }

            _logger.LogInfo("Post-tests Checks: " + name + ", " + dto.Message + incorrect);
            return Json(dto);
        }
        
        public ActionResult Medtronic(string id, string name)
        {
            id = Encoder.HtmlEncode(id);
            name = Encoder.HtmlEncode(name);

            ViewBag.Name = name;
            ViewBag.Test = "Medtronic";
            ViewBag.ID = id;

            ViewBag.Completed = Request.Params["completed"] != null ? "true" : "false";

            return View();
        }

        [HttpPost]        
        public JsonResult Medtronic()
        {
            var id = Request.Params["id"];
            var name = Request.Params["name"];
            id = Encoder.HtmlEncode(id);
            name = Encoder.HtmlEncode(name);

            var dto = DbPostTestsUtils.VerifyPostTest("Medtronic", Request.Params);
            if (dto.IsSuccessful)
            {
                //get the person id
                int nameId = int.Parse(id);
                //save test as completed
                DbPostTestsUtils.AddAndUpdateTestCompleted(nameId, "Medtronic");
            }

            string incorrect = "";
            if (dto.Messages.Count > 0)
            {
                incorrect = dto.Messages.Aggregate(incorrect, (current, s) => current + (s + ","));
                incorrect = incorrect.Substring(0, incorrect.Length - 1);
            }

            _logger.LogInfo("Post-tests Medtronic: " + name + ", " + dto.Message + incorrect);
            return Json(dto);
        }

        public ActionResult Overview(string id, string name, string language)
        {
            id = Encoder.HtmlEncode(id);
            name = Encoder.HtmlEncode(name);

            ViewBag.Name = name;
            ViewBag.Test = "Overview";
            ViewBag.ID = id;

            ViewBag.Completed = Request.Params["completed"] != null ? "true" : "false";
            if (language == "1")
                return View("OverviewF");
                     
            return View();
        }

        [HttpPost]        
        public JsonResult Overview()
        {
            var id = Request.Params["id"]; 
            var name = Request.Params["name"];
            id = Encoder.HtmlEncode(id);
            name = Encoder.HtmlEncode(name);

            var dto = DbPostTestsUtils.VerifyPostTest("Overview", Request.Params);
            if (dto.IsSuccessful)
            {
                //get the person id
                int nameId = int.Parse(id);
                //save test as completed
                DbPostTestsUtils.AddAndUpdateTestCompleted(nameId, "Overview");
            }

            var incorrect = "";
            if (dto.Messages.Count > 0)
            {
                incorrect = dto.Messages.Aggregate(incorrect, (current, s) => current + (s + ","));
                incorrect = incorrect.Substring(0, incorrect.Length - 1);
            }

            _logger.LogInfo("Post-tests Overview: " + name + ", " + dto.Message + incorrect);
            return Json(dto);
        }

        public ActionResult NovaStatStrip(string id, string name, string language)
        {
            id = Encoder.HtmlEncode(id);
            name = Encoder.HtmlEncode(name);
            ViewBag.Name = name;
            ViewBag.Test = "NovaStatStrip";
            ViewBag.ID = id;

            ViewBag.Completed = Request.Params["completed"] != null ? "true" : "false";
            if (language == "1")
                return View("NovaStatStripF");
                     
            return View();
        }

        [HttpPost]        
        public JsonResult NovaStatStrip()
        {
            string id = Request.Params["id"];
            string name = Request.Params["name"];
            id = Encoder.HtmlEncode(id);
            name = Encoder.HtmlEncode(name);

            var dto = DbPostTestsUtils.VerifyPostTest("NovaStatStrip", Request.Params);
            if (dto.IsSuccessful)
            {
                //get the person id
                int nameId = int.Parse(id);
                //save test as completed
                DbPostTestsUtils.AddAndUpdateTestCompleted(nameId, "NovaStatStrip");
            }

            var incorrect = "";
            if (dto.Messages.Count > 0)
            {
                incorrect = dto.Messages.Aggregate(incorrect, (current, s) => current + (s + ","));
                incorrect = incorrect.Substring(0, incorrect.Length - 1);
            }

            _logger.LogInfo("Post-tests NovaStatStrip: " + name + ", " + dto.Message + incorrect);
            return Json(dto);
        }

        public ActionResult VampJr(string id, string name, string language)
        {
            id = Encoder.HtmlEncode(id);
            name = Encoder.HtmlEncode(name);

            ViewBag.Name = name;
            ViewBag.Test = "VampJr";
            ViewBag.ID = id;
            
            ViewBag.Completed = Request.Params["completed"] != null ? "true" : "false";
            if (language == "1")
                return View("VampJrF");

            return View();
        }

        [HttpPost]        
        public JsonResult VampJr()
        {
            var id = Request.Params["id"];
            var name = Request.Params["name"];
            id = Encoder.HtmlEncode(id);
            name = Encoder.HtmlEncode(name);
            
            var dto = DbPostTestsUtils.VerifyPostTest("VampJr", Request.Params);
            if (dto.IsSuccessful)
            {
                //get the person id
                var nameId = int.Parse(id);
                //save test as completed
                DbPostTestsUtils.AddAndUpdateTestCompleted(nameId, "VampJr");
            }

            string incorrect = "";
            if (dto.Messages.Count > 0)
            {
                incorrect = dto.Messages.Aggregate(incorrect, (current, s) => current + (s + ","));
                incorrect = incorrect.Substring(0, incorrect.Length - 1);
            }

            _logger.LogInfo("Post-tests VampJr: " + name + ", " + dto.Message + incorrect);
            return Json(dto);
        }

        public ActionResult DexcomG4SensorVideo()
        {
            return View();
        }

        public ActionResult DexcomG4Sensor(string id, string name)
        {
            id = Encoder.HtmlEncode(id);
            name = Encoder.HtmlEncode(name);

            ViewBag.Name = name;
            ViewBag.Test = "DexcomG4Sensor";
            ViewBag.ID = id;

            ViewBag.Completed = Request.Params["completed"] != null ? "true" : "false";

            return View();
        }

        [HttpPost]
        public JsonResult DexcomG4Sensor()
        {
            var id = Request.Params["id"];
            var name = Request.Params["name"];
            id = Encoder.HtmlEncode(id);
            name = Encoder.HtmlEncode(name);

            var dto = DbPostTestsUtils.VerifyPostTest("DexcomG4Sensor", Request.Params);
            if (dto.IsSuccessful)
            {
                //get the person id
               var nameId = int.Parse(id);
                //save test as completed
                DbPostTestsUtils.AddAndUpdateTestCompleted(nameId, "Dexcom G4 Sensor");
            }

            var incorrect = "";
            if (dto.Messages.Count > 0)
            {
                incorrect = dto.Messages.Aggregate(incorrect, (current, s) => current + (s + ","));
                incorrect = incorrect.Substring(0, incorrect.Length - 1);
            }

            _logger.LogInfo("Post-tests DexcomG4Sensor: " + name + ", " + dto.Message + incorrect);
            return Json(dto);
        }

        public ActionResult DexcomG4Receiver(string id, string name, string language  )
        {
            id = Encoder.HtmlEncode(id);
            name = Encoder.HtmlEncode(name);

            ViewBag.Name = name;
            ViewBag.Test = "DexcomG4Receiver";
            ViewBag.ID = id;

            ViewBag.Completed = Request.Params["completed"] != null ? "true" : "false";
            if(language=="1")
                return View("DexcomG4ReceiverF");
            return View();
        }

        [HttpPost]
        public JsonResult DexcomG4Receiver()
        {
            var id = Request.Params["id"];
            var name = Request.Params["name"];
            id = Encoder.HtmlEncode(id);
            name = Encoder.HtmlEncode(name);

            var dto = DbPostTestsUtils.VerifyPostTest("DexcomG4Receiver", Request.Params);
            if (dto.IsSuccessful)
            {
                //get the person id
                var nameId = int.Parse(id);
                //save test as completed
                DbPostTestsUtils.AddAndUpdateTestCompleted(nameId, "Dexcom G4 Receiver");
            }

            var incorrect = "";
            if (dto.Messages.Count > 0)
            {
                incorrect = dto.Messages.Aggregate(incorrect, (current, s) => current + (s + ","));
                incorrect = incorrect.Substring(0, incorrect.Length - 1);
            }

            _logger.LogInfo("Post-tests DexcomG4Receiver: " + name + ", " + dto.Message + incorrect);
            return Json(dto);
        }

        public ActionResult MedtronicSofSensor(string id, string name)
        {
            id = Encoder.HtmlEncode(id);
            name = Encoder.HtmlEncode(name);

            ViewBag.Name = name;
            ViewBag.Test = "MedtronicSofSensor";
            ViewBag.ID = id;

            ViewBag.Completed = Request.Params["completed"] != null ? "true" : "false";

            return View();
        }

        [HttpPost]
        public JsonResult MedtronicSofSensor()
        {
            var id = Request.Params["id"];
            var name = Request.Params["name"];
            id = Encoder.HtmlEncode(id);
            name = Encoder.HtmlEncode(name);

            var dto = DbPostTestsUtils.VerifyPostTest("MedtronicSofSensor", Request.Params);
            if (dto.IsSuccessful)
            {
                //get the person id
                var nameId = int.Parse(id);
                //save test as completed
                DbPostTestsUtils.AddAndUpdateTestCompleted(nameId, "Medtronic Sof Sensor");
            }

            var incorrect = "";
            if (dto.Messages.Count > 0)
            {
                incorrect = dto.Messages.Aggregate(incorrect, (current, s) => current + (s + ","));
                incorrect = incorrect.Substring(0, incorrect.Length - 1);
            }

            _logger.LogInfo("Post-tests MedtronicSofSensor: " + name + ", " + dto.Message + incorrect);
            return Json(dto);
        }

        public ActionResult GuardianREALTimeMonitor(string id, string name)
        {
            id = Encoder.HtmlEncode(id);
            name = Encoder.HtmlEncode(name);

            ViewBag.Name = name;
            ViewBag.Test = "GuardianREALTimeMonitor";
            ViewBag.ID = id;

            ViewBag.Completed = Request.Params["completed"] != null ? "true" : "false";

            return View();
        }

        [HttpPost]
        public JsonResult GuardianREALTimeMonitor()
        {
            var id = Request.Params["id"];
            var name = Request.Params["name"];
            id = Encoder.HtmlEncode(id);
            name = Encoder.HtmlEncode(name);

            var dto = DbPostTestsUtils.VerifyPostTest("GuardianREALTimeMonitor", Request.Params);
            if (dto.IsSuccessful)
            {
                //get the person id
                var nameId = int.Parse(id);
                //save test as completed
                DbPostTestsUtils.AddAndUpdateTestCompleted(nameId, "Guardian REAL-Time Monitor");
            }

            var incorrect = "";
            if (dto.Messages.Count > 0)
            {
                incorrect = dto.Messages.Aggregate(incorrect, (current, s) => current + (s + ","));
                incorrect = incorrect.Substring(0, incorrect.Length - 1);
            }

            _logger.LogInfo("Post-tests GuardianREALTimeMonitor: " + name + ", " + dto.Message + incorrect);
            return Json(dto);
        }
    }
}
