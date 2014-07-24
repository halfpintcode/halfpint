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
            var model = new PostTestsInitializeModel();

            model.SiteId = DbUtils.GetSiteidIdForUser(User.Identity.Name);
            model.SiteCode = DbUtils.GetSiteCodeForUser(User.Identity.Name);

            var users = DbPostTestsUtils.GetStaffTestUsersForSite(model.SiteId);            
            users.Insert(0, new IDandName(0, "Select Your Name"));

            //check if employee id required
            var retDto = DbPostTestsUtils.CheckIfEmployeeIdRequired(User.Identity.Name);
            model.EmpIdRequired = retDto.Stuff.EmpIDRequired;
            model.EmpIdRegex = retDto.Stuff.EmpIDRegex;
            model.EmpIdMessage = retDto.Stuff.EmpIDMessage;
            
            ViewBag.Users = new SelectList(users, "ID", "Name");
            return View(model);
        }

        public ActionResult EditPostTest(string id)
        {
            //PostTestsModel ptm = new PostTestsModel();            
            //ptm.ID = int.Parse(id);

            var site = DbUtils.GetSiteidIdForUser(User.Identity.Name);
            var siteCode = DbUtils.GetSiteCodeForUser(User.Identity.Name);
            var tests = DbPostTestsUtils.GetStaffPostTestsCompletedCurrentAndActive(id, siteCode);
            var staffInfo = DbUtils.GetStaffInfo(int.Parse(id));
            var postTestView = new PostTestView();
            postTestView.StaffId = staffInfo.ID;
            postTestView.StaffName = staffInfo.FirstName + " " + staffInfo.LastName;
            postTestView.PostTests = tests;
            return View(postTestView);
        }

        [HttpPost]
        public JsonResult EditPostTest(List<PostTest> ptList, string staffId, string staffName)
        {
            int iRet = DbPostTestsUtils.SavePostTestsCompleted(ptList, int.Parse(staffId), staffName);  
            return Json(1);
        }

    }
}
