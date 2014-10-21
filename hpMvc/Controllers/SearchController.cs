using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace hpMvc.Controllers
{
    public class SearchController : Controller
    {
        private const string RootFolder = @"C:\Dropbox\HalfPint Website Docs Library";


        public JsonResult GetSearchResults(string criteria)
        {
            //var searchResults = Directory.GetFiles(RootFolder, criteria + "*.pdf", SearchOption.AllDirectories);
            var searchResults = Directory.EnumerateFiles(RootFolder, criteria + "*.pdf", SearchOption.AllDirectories)
                .Select(f => Path.GetFileName(f)).ToList();
            return Json(searchResults);
        }

    }
}
