using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using hpMvc.Models;
using hpMvc.DataBase;

namespace hpMvc.Controllers
{
    public class TestController : Controller
    {
        //
        // GET: /Test/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetDate(string date, string time)
        {
            var dtDate = DateTime.Parse(date + " " + time);
            return Json(1);
        }


        public ActionResult Temp()
        {
            return View();
        }

        
        public ActionResult Create()
        {
            var testModel = new TestModel();
            return View(testModel);
        }

        [HttpPost]
        public JsonResult Create(TestModel testModel)
        {
            return Json(testModel);
        }

        public void SendEmail(string subject, string to, string body)
        {
            //Utility.SendEmail(subject, to, body);
        }

    }
}
