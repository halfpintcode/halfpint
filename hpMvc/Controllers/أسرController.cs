using System;
using System.Web.Mvc;
using hpMvc.DataBase;
using hpMvc.Models;
using hpMvc.ViewModels;
using hpMvc.DataBase;

namespace hpMvc.Controllers
{
    public class أسرController : Controller
    {
        //
        // GET: /Families/

        public ActionResult Index()
        {
            int count = DbUtils.GetRandomizedSubjectsCount();

            var contact = new FamilyContactsModel();

            var vm = new FamiliesViewModel();
            vm.RandomizedCount = count;
            vm.FamilyContact = contact;

            return View(vm);
        }

        [HttpPost]
        public ActionResult Index(FamiliesViewModel vm)
        {
            if (ModelState.IsValid)
            {
                FamiliesBusiness.AddFamiliesContact(vm.FamilyContact);
                var u = new UrlHelper(this.Request.RequestContext);
                string url = "http://" + Request.Url.Host + u.RouteUrl("Default", new { Controller = "Families", Action = "Index" });
                
                FamiliesBusiness.ProcessEmails(vm.FamilyContact,url,Server);
                
                return RedirectToAction("ContactConfirmation", "Families", new {fvm = vm});
            }
            else
            {
                return View(vm);    
            }
            
        }

        public ActionResult ContactConfirmation(FamiliesViewModel fvm)
        {
            return View();
        }

    }
}
