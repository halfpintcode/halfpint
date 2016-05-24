using System.Web.Mvc;
using hpMvc.DataBase;

namespace hpMvc.Controllers
{
    public class FamiliesController : Controller
    {
        //
        // GET: /Families/

        public ActionResult Index()
        {
            int count = DbUtils.GetRandomizedSubjectsCount();

            return View(count);
        }

    }
}
