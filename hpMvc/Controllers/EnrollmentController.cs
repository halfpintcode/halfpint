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

        public ActionResult Error()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Save(HttpPostedFileBase file,[Bind(Include =
            "EnrollmentContent,AnnouncementContent")]EnrollmentContentModel ecm)
        {
            if (ModelState.IsValid)
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

                if (dto.ReturnValue == 1)
                {
                    var iRetval = DbInform.SaveStaffEnrollmentPage(ecm);

                    if (iRetval != 1)
                    {
                        
                    }
                }
                else
                {
                    TempData["message"] = dto.Message;
                    return RedirectToAction("Error");
                }


                return RedirectToAction("SaveSuccess");
            }
            else
            {
                return null;
            }
        }
    }
}
