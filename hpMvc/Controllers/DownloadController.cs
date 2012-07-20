using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using hpMvc.Infrastructure.Logging;
using hpMvc.DataBase;

namespace hpMvc.Controllers
{
    public class DownloadController : Controller
    {
        NLogger nlogger = new NLogger();

        
        public ActionResult Index()
        {
            return View();
        }
        
        public FilePathResult GetFile(string fileName, string folder)
        {
            if (fileName.StartsWith("Site~"))
            {
                GetSiteFileNameAndFolder(ref fileName, ref folder);
            }

            var docPath = @"C:\Dropbox\HalfPint Website Docs Library";
            if (folder != null)
            {
                if (folder.Length > 0)
                {
                    if (folder.IndexOf('~') > -1)
                    {
                        folder = folder.Replace('~', '\\');
                    }
                    docPath = Path.Combine(docPath, folder);
                }
            }
            
            var file = Path.Combine(docPath, fileName);
            var contentType = "";
            var pos = 0;
            pos = fileName.LastIndexOf(".");
            var ext = fileName.Substring(pos );
            switch (ext)
            {
                case ".docx":
                    contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
                case ".doc":
                    contentType = "application/msword";
                    break;

                    
                case ".xlsx":
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                case ".xls":
                    contentType = "application/msexcel";
                    break;
                
                case ".pdf":
                    contentType = "application/pdf";
                    break;
            }

            nlogger.LogInfo("Download.GetFile: " + fileName);
            return this.File(file, contentType, fileName);
        }

        private void GetSiteFileNameAndFolder(ref string fileName, ref string folder)
        {
            var si = DbUtils.GetSiteInfoForUser(User.Identity.Name);
            switch (si.SiteID)
            {
                case "02":
                    if (fileName.Contains("Consent Form"))
                    {
                        fileName = "CHOP HalfPint Consent Form.pdf";
                        folder = "Enrollment~CHOP";
                    }
                    if (fileName.Contains("Assent Form"))
                    {
                        fileName = "CHOP HalfPint Assent Form.pdf";
                        folder = "Enrollment~CHOP";
                    }
                    break;

                case "05":
                    if (fileName.Contains("Consent Form"))
                    {
                        fileName = "Nationwide HalfPint Consent Form.pdf";
                        folder = "Enrollment~Nationwide";
                    }
                    if (fileName.Contains("Assent Form"))
                    {
                        fileName = "Nationwide HalfPint Assent Form.pdf";
                        folder = "Enrollment~Nationwide";
                    }
                    break;
                
                case "06":
                    if (fileName.Contains("Consent Form"))
                    {
                        fileName = "CCHMC HalfPint Consent Form.pdf";
                        folder = "Enrollment~Cincinnati";
                    }
                    if (fileName.Contains("Assent Form"))
                    {
                        fileName = "CCHMC HalfPint Assent Form.pdf";
                        folder = "Enrollment~Cincinnati";
                    }
                    break;

                case "07":
                    if (fileName.Contains("Consent Form"))
                    {
                        fileName = "PennState HalfPint Consent Form.pdf";
                        folder = "Enrollment~Penn State";
                    }
                    if (fileName.Contains("Assent Form"))
                    {
                        fileName = "PennState HalfPint Assent Form.pdf";
                        folder = "Enrollment~Penn State";
                    }
                    break;

                default:
                    if (fileName.Contains("Consent Form"))
                    {
                        fileName = "HalfPint Consent Form for CHB.doc";
                        folder = "Enrollment";
                    }
                    if (fileName.Contains("Assent Form"))
                    {
                        fileName = "HalfPint Assent Form.docx";
                        folder = "Enrollment";
                    }
                    break;
                    

            }
        }
    }
}
