using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
        public JsonResult Save(InformPageModel ifp)
        {
            //check for not allowed input
            DTO dto = DbInform.ValidateInput(ifp);
            
            int iRetval = 0;

            if(dto.ReturnValue == 1)
                 iRetval = DbInform.SaveInformPage(ifp); 
            
            return Json(dto);
        }

        public ActionResult InvalidInput()
        {

            return View();
        }
    }
}
