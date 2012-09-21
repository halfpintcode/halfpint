using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace hpMvc
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
               "StudySubject", // Route name
               "InitializeSubject/{action}/{studyID}", // URL with parameters
               new { controller = "InitializeSubject", action = "Initialize", studyID = UrlParameter.Optional } // Parameter defaults
            );

            routes.MapRoute(
               "PostTestsAdmin", // Route name
               "PostTestsAdmin/{action}/{id}", // URL with parameters
               new { controller = "PostTestsAdmin", action = "Index", id = "0" } // Parameter defaults
            );

            routes.MapRoute(
               "PostTests", // Route name
               "PostTests/{action}/{id}", // URL with parameters
               new { controller = "PostTests", action = "Index", id = "0" } // Parameter defaults
            );
           
            routes.MapRoute(
                "Admin", // Route name
                "Admin/{action}/{userName}", // URL with parameters
                new { controller = "Admin", action = "Index", userName = UrlParameter.Optional } // Parameter defaults
            );

            routes.MapRoute(
                "Download", // Route name
                "Download/{action}/{fileName}/{folder}", // URL with parameters
                new { controller = "Download", action = "GetFile", fileName = UrlParameter.Optional, folder=UrlParameter.Optional } // Parameter defaults
            );

            routes.MapRoute(
                "Staff", // Route name
                "Staff/{action}/{folder}", // URL with parameters
                new { controller = "Staff", action = "Index", folderName = UrlParameter.Optional } // Parameter defaults
            );

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}