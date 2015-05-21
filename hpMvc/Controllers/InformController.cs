using System.Web.Mvc;
using hpMvc.DataBase;
using hpMvc.Models;

namespace hpMvc.Controllers
{
    public class InformController : Controller
    {
        //
        // GET: /Inform/

        public ActionResult Index()
        {
            InformPageModel ifp = DbInform.GetInformPage();            
            return View(ifp);
        }

        [Authorize]
        public ActionResult Edit()
        {
            InformPageModel ifp = DbInform.GetInformPage();
            return View(ifp);            
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Save([Bind(Include = "HeaderContent," +
                                               "MainContent,FooterContent")]InformPageModel ifp)
        {
            if (ModelState.IsValid)
            {
                //check for not allowed input
                DTO dto = DbInform.ValidateInput(ifp);

                int iRetval = 0;

                if (dto.ReturnValue == 1)
                    iRetval = DbInform.SaveInformPage(ifp);
                if (iRetval != 1)
                {
                }

                return Json(dto);
            }
            else
            {
                return null;
            }
        }

        //public ActionResult InvalidInput()
        //{

        //    return View();
        //}
    }
}
