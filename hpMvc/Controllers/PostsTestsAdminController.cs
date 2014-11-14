using System.Collections.Generic;
using System.Linq;
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

        public ActionResult AddPostTest(string id)
        {
            var site = DbUtils.GetSiteidIdForUser(User.Identity.Name);
            var siteCode = DbUtils.GetSiteCodeForUser(User.Identity.Name);
            var tests = DbPostTestsUtils.GetStaffPostTestsActive(id, siteCode);
            var staffInfo = DbUtils.GetStaffInfo(int.Parse(id));
            var postTestView = new PostTestView();
            postTestView.StaffId = staffInfo.ID;
            postTestView.StaffName = staffInfo.FirstName + " " + staffInfo.LastName;
            postTestView.PostTests = tests;
            return View(postTestView);
        }

        [HttpPost]
        public JsonResult AddPostTest(List<PostTest> postTests, string staffId, string staffName)
        {
            if (postTests.Any(pt => string.IsNullOrEmpty(pt.sDateCompleted)))
            {
                return Json(0);
            }
            
            int iRet = DbPostTestsUtils.SaveNewPostTestsCompleted(postTests, int.Parse(staffId), staffName);
            return Json(iRet);
        }

        public ActionResult EditPostTest(string id)
        {
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
        public JsonResult EditPostTest(List<PostTest> postTests, string staffId, string staffName)
        {
            int iRet = DbPostTestsUtils.SavePostTestsCompleted(postTests, int.Parse(staffId), staffName);  
            return Json(iRet);
        }

    }
}
