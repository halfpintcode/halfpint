using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace hpMvc.Helpers
{
    public static class AuthHelper
    {
        public static bool IsUserAdmin(this HtmlHelper helper)
        {
            return helper.ViewContext.HttpContext.User.IsInRole("Admin");
        }
    }


}