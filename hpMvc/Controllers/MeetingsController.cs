using System.Web.Mvc;
using hpMvc.DataBase;
using hpMvc.Models;

namespace hpMvc.Controllers
{
    public class MeetingsController : Controller
    {
        //
        // GET: /Meetings/

        public ActionResult Index()
        {
            InformPageModel ifp = DbInform.GetMeetingsPage();
            return View(ifp);
        }

        [Authorize]
        public ActionResult Edit()
        {
            InformPageModel ifp = DbInform.GetMeetingsPage();
            return View(ifp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public JsonResult Save(InformPageModel ifp)
        {
            if(ifp.FooterContent == null)
                ifp.FooterContent = "<p></p>";
            else 
            {
                if (ifp.FooterContent.Length == 0)
                    ifp.FooterContent = "<p></p>";
            }
            if (ifp.HeaderContent == null)
                ifp.HeaderContent = "<p></p>";
            else
            {
                if (ifp.HeaderContent.Length == 0)
                    ifp.HeaderContent = "<p></p>";
            }
            if (ifp.MainContent == null)
                ifp.MainContent = "<p></p>";
            else
            {
                if (ifp.MainContent.Length == 0)
                    ifp.MainContent = "<p></p>";
            }

            //check for not allowed input
            DTO dto = DbInform.ValidateInput(ifp);
            
            int iRetval = 0;
            

            if (dto.ReturnValue == 1)
                iRetval = DbInform.SaveMeetingsPage(ifp);

            return Json(dto);
        }

    }
}
