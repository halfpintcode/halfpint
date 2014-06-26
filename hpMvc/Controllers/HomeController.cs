using System.Web.Mvc;
using hpMvc.Infrastructure.Logging;

namespace hpMvc.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        
        NLogger logger = new NLogger();
        public ActionResult Index()
        {
            //throw new Exception("Why ME!");
                      
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        
    }
}
