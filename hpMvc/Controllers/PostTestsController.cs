using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using hpMvc.DataBase;
using hpMvc.Models;
using hpMvc.Infrastructure.Logging;
using System.Text;

namespace hpMvc.Controllers
{
    [Authorize]
    public class PostTestsController : Controller
    {
        NLogger logger = new NLogger();

        public ActionResult Initialize(string id)
        {
            int site = DbUtils.GetSiteidIDForUser(User.Identity.Name);
            var users = DbPostTestsUtils.GetTestUsersForSite(site);

            users.Insert(0, new IDandName(0, "Select Your Name"));

            //check if employee id required
            var retDto = DbPostTestsUtils.CheckIfEmployeeIDRequired(User.Identity.Name);
            ViewBag.EmpIDRequired = retDto.Stuff.EmpIDRequired;
            ViewBag.EmpIDRegex = retDto.Stuff.EmpIDRegex;
            ViewBag.EmpIDMessage = retDto.Stuff.EmpIDMessage;
                        
            ViewBag.Users = new SelectList(users, "ID", "Name", id);
            if (id != "0")
            {                
                ViewBag.Email = DbPostTestsUtils.GetPostTestPersonEmail(id);
            }
            
            return View();
        }
                        
        public JsonResult CreateName()
        {
            var dto = new DTO();

            int siteID = DbUtils.GetSiteidIDForUser(HttpContext.User.Identity.Name);
            string lastName = Request.Params["LastName"];
            string firstName = Request.Params["FirstName"];            
            string empID = Request.Params["EmpID"];
            string email = Request.Params["Email"];

            dto.ReturnValue = DbPostTestsUtils.DoesStaffNameExist(lastName, firstName, siteID);
            if (dto.ReturnValue != 0)
            {
                if (dto.ReturnValue == -1)
                    dto.Message = "There was an error in determinig if this name was already in the database.";
                if (dto.ReturnValue == 0)
                    dto.Message = "This name already exists. Select your name from the drop down list." ;

                logger.LogInfo("PostTests.CreateName - message: " + dto.Message + ", name: " + lastName + "," + firstName + ", site: " + siteID.ToString());
                return Json(dto);               
            }

            dto.ReturnValue = DbPostTestsUtils.AddNurseStaff(lastName, firstName, empID, siteID, email);

            logger.LogInfo("PostTests.CreateName - message: " + dto.Message + ", name: " + lastName + "," + firstName + ", site: " + siteID.ToString());
            return Json(dto);
        }

        public JsonResult IsUserEmailDuplicate(string email)
        {
            bool retVal = true;

            if (AccountUtils.GetUserByEmail(email) == null)
                retVal = false;
            return Json(retVal);
        }

        public JsonResult IsUserEmployeeIDDuplicate(string employeeID)
        {
            bool retVal = true;
            int site = DbUtils.GetSiteidIDForUser(User.Identity.Name);
            int iRetVal = DbPostTestsUtils.DoesStaffEmployeeIDExist(employeeID, site);
            
            return Json(retVal);
        }
        
        public JsonResult GetTestsCompleted()
        {
            string id = Request.Params["ID"];
            int site = DbUtils.GetSiteidIDForUser(User.Identity.Name);
            var tests = DbPostTestsUtils.GetTestsCompleted(id);
            var email = DbPostTestsUtils.GetPostTestPersonEmail(id);

            var retVal = new { email = email, tests = tests };
            return Json(retVal);
        }

        public JsonResult Submit()
        {
            string id = Request.Params["ID"];
            string name = Request.Params["Name"];
            string[] email = new string[] {Request.Params["Email"]};

            var tests = DbPostTestsUtils.GetTestsCompleted(id);
            if(tests.Count == 0)
                return Json("no tests");
            
            int site = DbUtils.GetSiteidIDForUser(HttpContext.User.Identity.Name);            
            var coordinators = DbUtils.GetUserInRole("Coordinator", site);
                        
            var toEmails = new List<string>();
            foreach (var coord in coordinators)
            {
                toEmails.Add(coord.Email);
            }
            string siteName = DbUtils.GetSiteNameForUser(User.Identity.Name);

            var u = new UrlHelper(this.Request.RequestContext);
            string url = "http://" + this.Request.Url.Host + u.RouteUrl("Default", new { Controller = "Account", Action = "Logon" });            
            Utility.SendPostTestsSubmittedMail(toEmails.ToArray(), email, tests, name, siteName, Server, url);

            logger.LogInfo("Post-tests submitted: " + name);
            return Json("");
        }

        public ActionResult Checks(string id, string name)
        {
            ViewBag.Name = name;
            ViewBag.Test = "Checks";
            ViewBag.ID = id;

            if (Request.Params["completed"] != null)
                ViewBag.Completed = "true";
            else
                ViewBag.Completed = "false";
                        
            return View();
        }
        
        [HttpPost]        
        public JsonResult Checks()
        {
            string id = Request.Params["id"];
            string name = Request.Params["name"]; 

            var dto = DbPostTestsUtils.VerifyPostTest("Checks", Request.Params);
            if (dto.IsSuccessful)
            {
                //get the person id
                int siteID = DbUtils.GetSiteidIDForUser(HttpContext.User.Identity.Name);
                int nameID = int.Parse(id);
                //save test as completed
                DbPostTestsUtils.AddTestCompleted(nameID, "Checks");
            }

            string incorrect = "";
            if (dto.Messages.Count > 0)
            {
                foreach (var s in dto.Messages)
                {
                    incorrect += s + ",";
                }
                incorrect = incorrect.Substring(0, incorrect.Length - 1);
            }

            logger.LogInfo("Post-tests Checks: " + name + ", " + dto.Message + incorrect);
            return Json(dto);
        }
        
        public ActionResult Medtronic(string id, string name)
        {
            ViewBag.Name = name;
            ViewBag.Test = "Medtronic";
            ViewBag.ID = id;

            if (Request.Params["completed"] != null)
                ViewBag.Completed = "true";
            else
                ViewBag.Completed = "false";

            return View();
        }

        [HttpPost]        
        public JsonResult Medtronic()
        {
            string id = Request.Params["id"];
            string name = Request.Params["name"];

            var dto = DbPostTestsUtils.VerifyPostTest("Medtronic", Request.Params);
            if (dto.IsSuccessful)
            {
                //get the person id
                int siteID = DbUtils.GetSiteidIDForUser(HttpContext.User.Identity.Name);
                int nameID = int.Parse(id);
                //save test as completed
                DbPostTestsUtils.AddTestCompleted(nameID, "Medtronic");
            }

            string incorrect = "";
            if (dto.Messages.Count > 0)
            {
                foreach (var s in dto.Messages)
                {
                    incorrect += s + ",";
                }
                incorrect = incorrect.Substring(0, incorrect.Length - 1);
            }

            logger.LogInfo("Post-tests Medtronic: " + name + ", " + dto.Message + incorrect);
            return Json(dto);
        }

        public ActionResult Overview(string id, string name)
        {
            ViewBag.Name = name;
            ViewBag.Test = "Overview";
            ViewBag.ID = id;

            if (Request.Params["completed"] != null)
                ViewBag.Completed = "true";
            else
                ViewBag.Completed = "false";

                     
            return View();
        }

        [HttpPost]        
        public JsonResult Overview()
        {

            string id = Request.Params["id"]; 
            string name = Request.Params["name"];

            var dto = DbPostTestsUtils.VerifyPostTest("Overview", Request.Params);
            if (dto.IsSuccessful)
            {
                //get the person id
                int siteID = DbUtils.GetSiteidIDForUser(HttpContext.User.Identity.Name);
                int nameID = int.Parse(id);
                //save test as completed
                DbPostTestsUtils.AddTestCompleted(nameID, "Overview");
            }

            string incorrect = "";
            if (dto.Messages.Count > 0)
            {
                foreach (var s in dto.Messages)
                {
                    incorrect += s + ",";
                }
                incorrect = incorrect.Substring(0, incorrect.Length - 1);
            }

            logger.LogInfo("Post-tests Overview: " + name + ", " + dto.Message + incorrect);
            return Json(dto);
        }

        public ActionResult NovaStatStrip(string id, string name)
        {
            ViewBag.Name = name;
            ViewBag.Test = "NovaStatStrip";
            ViewBag.ID = id;

            if (Request.Params["completed"] != null)
                ViewBag.Completed = "true";
            else
                ViewBag.Completed = "false";

                     
            return View();
        }

        [HttpPost]        
        public JsonResult NovaStatStrip()
        {
            string id = Request.Params["id"];
            string name = Request.Params["name"];

            var dto = DbPostTestsUtils.VerifyPostTest("NovaStatStrip", Request.Params);
            if (dto.IsSuccessful)
            {
                //get the person id
                int siteID = DbUtils.GetSiteidIDForUser(HttpContext.User.Identity.Name);
                int nameID = int.Parse(id);
                //save test as completed
                DbPostTestsUtils.AddTestCompleted(nameID, "NovaStatStrip");
            }

            string incorrect = "";
            if (dto.Messages.Count > 0)
            {
                foreach (var s in dto.Messages)
                {
                    incorrect += s + ",";
                }
                incorrect = incorrect.Substring(0, incorrect.Length - 1);
            }

            logger.LogInfo("Post-tests NovaStatStrip: " + name + ", " + dto.Message + incorrect);
            return Json(dto);
        }

        public ActionResult VampJr(string id, string name)
        {
            ViewBag.Name = name;
            ViewBag.Test = "VampJr";
            ViewBag.ID = id;
            
            if (Request.Params["completed"] != null)
                ViewBag.Completed = "true";
            else
                ViewBag.Completed = "false";

            return View();
        }

        [HttpPost]        
        public JsonResult VampJr()
        {
            string id = Request.Params["id"];
            string name = Request.Params["name"];
                        
            var dto = DbPostTestsUtils.VerifyPostTest("VampJr", Request.Params);
            if (dto.IsSuccessful)
            {
                //get the person id
                int siteID = DbUtils.GetSiteidIDForUser(HttpContext.User.Identity.Name);
                int nameID = int.Parse(id);
                //save test as completed
                DbPostTestsUtils.AddTestCompleted(nameID, "VampJr");
            }

            string incorrect = "";
            if (dto.Messages.Count > 0)
            {
                foreach (var s in dto.Messages)
                {
                    incorrect += s + "," ;
                }
                incorrect = incorrect.Substring(0, incorrect.Length - 1);
            }

            logger.LogInfo("Post-tests VampJr: " + name + ", " + dto.Message + incorrect);
            return Json(dto);
        }
    }
}
