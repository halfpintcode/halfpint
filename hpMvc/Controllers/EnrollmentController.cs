using System.Web;
using System.Web.Mvc;
using hpMvc.DataBase;
using hpMvc.Models;
using System.IO;

namespace hpMvc.Controllers
{
    public class EnrollmentController : Controller
    {
        //
        // GET: /Enrollment/

        [Authorize]
        public ActionResult Edit()
        {
            var ecm = DbInform.GetEnrollmentContent();
            return View(ecm);
        }

        public ActionResult SaveSuccess()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Save(HttpPostedFileBase file,EnrollmentContentModel ecm)
        {
            //check for file upload
            if (file != null && file.ContentLength > 0)
            {
                const string fileName = "enrollment.png";
                var path = Path.Combine(Server.MapPath("~/Content/Images"), fileName);
                file.SaveAs(path);
            }

            //check for not allowed input
            DTO dto = DbInform.ValidateInput(ecm);
            var iRetval = 0;

            if (dto.ReturnValue == 1)
                iRetval = DbInform.SaveStaffEnrollmentPage(ecm);

            if(iRetval != 1)
            {}

            return RedirectToAction("SaveSuccess");
        }
    }
}
