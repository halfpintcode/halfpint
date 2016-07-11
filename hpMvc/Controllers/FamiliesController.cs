using System;
using System.Web.Mvc;
using hpMvc.DataBase;
using hpMvc.Models;
using hpMvc.ViewModels;

namespace hpMvc.Controllers
{
    public class FamiliesController : Controller
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
            
            return View(vm);
        }

    }
}
