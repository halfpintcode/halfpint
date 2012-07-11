using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using hpMvc.Infrastructure.Logging;
using hpMvc.DataBase;
using hpMvc.Models;

namespace hpMvc.Controllers
{
    [Authorize]
    public class PostTestsAdminController : Controller
    {
        NLogger logger = new NLogger();

        public ActionResult Index()
        {
            int site = DbUtils.GetSiteidIDForUser(HttpContext.User.Identity.Name);
            
            //for testing
            //if (site == 0)
            //    site = 1;
            
            var users = DbPostTestsUtils.GetTestUsersForSite(site);

            users.Insert(0, "Select Your Name");

            //check if employee id required
            var retDto = DbPostTestsUtils.CheckIfEmployeeIDRequired(User.Identity.Name);
            ViewBag.EmpIDRequired = retDto.Stuff.EmpIDRequired;
            ViewBag.EmpIDRegex = retDto.Stuff.EmpIDRegex;
            ViewBag.EmpIDMessage = retDto.Stuff.EmpIDMessage;
            
                        
            ViewBag.Users = new SelectList(users);
            return View();
        }

        public ActionResult EditPostTest(string name)
        {
            PostTestsModel ptm = new PostTestsModel();

            int site = DbUtils.GetSiteidIDForUser(User.Identity.Name);
            var tests = DbPostTestsUtils.GetTestsCompleted(name);

            ptm.Name = name;
            foreach (var test in tests)
            {
                switch (test.Name)
                {
                    case "Overview":
                        ptm.Overview = true;
                        ptm.OverviewCompleted = test.DateCompleted;
                        break;
                    case "Checks":
                        ptm.Checks = true;
                        ptm.ChecksCompleted = test.DateCompleted;
                        break;
                    case "Medtronic":
                        ptm.Medtronic = true;
                        ptm.MedtronicCompleted = test.DateCompleted;
                        break;
                    case "NovaStatStrip":
                        ptm.NovaStatStrip = true;
                        ptm.NovaStatStripCompleted = test.DateCompleted;
                        break;
                    case "VampJr":
                        ptm.VampJr = true;
                        ptm.VampJrCompleted = test.DateCompleted;
                        break;
                }
            }

            return View(ptm);
        }

        [HttpPost]
        public JsonResult EditPostTest(PostTestsModel ptm)
        {
            int iRet = DbPostTestsUtils.SavePostTestsCompleted(ptm);  
            return Json(iRet);
        }

    }
}
