using System.Collections.Generic;
using System.Web.Mvc;
using hpMvc.Infrastructure.Logging;
using hpMvc.DataBase;
using hpMvc.Models;

namespace hpMvc.Controllers
{
    [Authorize]
    public class PostTestsAdminController : Controller
    {
        readonly NLogger _logger = new NLogger();

        public ActionResult Index()
        {
            int site = DbUtils.GetSiteidIDForUser(HttpContext.User.Identity.Name);
            
            var users = DbPostTestsUtils.GetStaffTestUsersForSite(site);            
            users.Insert(0, new IDandName(0, "Select Your Name"));

            //check if employee id required
            var retDto = DbPostTestsUtils.CheckIfEmployeeIdRequired(User.Identity.Name);
            ViewBag.EmpIDRequired = retDto.Stuff.EmpIDRequired;
            ViewBag.EmpIDRegex = retDto.Stuff.EmpIDRegex;
            ViewBag.EmpIDMessage = retDto.Stuff.EmpIDMessage;
            ViewBag.SiteId = site;
                        
            ViewBag.Users = new SelectList(users, "ID", "Name");
            return View();
        }

        public ActionResult EditPostTest(string id)
        {
            //PostTestsModel ptm = new PostTestsModel();            
            //ptm.ID = int.Parse(id);

            int site = DbUtils.GetSiteidIDForUser(User.Identity.Name);
            var tests = DbPostTestsUtils.GetStaffPostTestsCompletedCurrentAndActive(id);

            var staffInfo = DbUtils.GetStaffInfo(int.Parse(id));
            ViewBag.StaffId = staffInfo.ID;
            ViewBag.StaffName = staffInfo.FirstName + " " + staffInfo.LastName;

            return View(tests);
        }

        [HttpPost]
        public JsonResult EditPostTest(List<PostTest> ptList, string staffId, string staffName)
        {
            int iRet = DbPostTestsUtils.SavePostTestsCompleted(ptList, int.Parse(staffId), staffName);  
            return Json(1);
        }

    }
}
