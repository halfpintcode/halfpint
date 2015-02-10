using System.Web.Mvc;
using hpMvc.DataBase;

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

    }
}
