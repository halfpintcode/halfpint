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
            var ext = fileName.Substring(pos ).ToLower();
            switch (ext)
            {
                case ".docx":
                    contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
                case ".doc":
                    contentType = "application/msword";
                    break;
                case ".ppt":
                    contentType = "applica​tion/vnd.m​s-powerpoi​nt";
                    break;
                case ".pptx":
                    contentType = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
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
                case ".exe":
                    contentType = "application/exe";
                    break;
                case ".dll":
                    contentType = "application/dll";
                    break;
                case ".msi":
                    contentType = "application/msi";
                    break;
            }

            nlogger.LogInfo("Download.GetFile: " + fileName);
            return this.File(file, contentType, fileName);
        }

        private void GetSiteFileNameAndFolder(ref string fileName, ref string folder)
        {
            var si = DbUtils.GetSiteInfoForUser(User.Identity.Name);
            switch (si.SiteId)
            {
                case "01":
                    if (fileName.Contains("Permission to Approach"))
                    {
                        fileName = "CHB Permission to Approach.pdf";
                        folder = "Quick Links~Permission to Approach~CHB";
                    }
                    break;

                case "02":
                    if (fileName.Contains("Consent Form"))
                    {
                        fileName = "CHOP HalfPint Consent Form.pdf";
                        folder = "Enrollment~Sites~CHOP";
                    }
                    if (fileName.Contains("Assent Form"))
                    {
                        fileName = "CHOP HalfPint Assent Form.pdf";
                        folder = "Enrollment~Sites~CHOP";
                    }                    
                    if (fileName.Contains("Permission to Approach"))
                    {
                        fileName = "CHOP Permission to Approach.pdf";
                        folder = "Quick Links~Permission to Approach~CHOP";
                    }
                    break;


                case "04":
                    if (fileName.Contains("Consent Form"))
                    {
                        fileName = "Denver HalfPint Consent Form.pdf";
                        folder = "Enrollment~Sites~Denver";
                    }
                    if (fileName.Contains("Assent Form"))
                    {
                        fileName = "Denver HalfPint Assent Form.pdf";
                        folder = "Enrollment~Sites~Denver";
                    }
                    if (fileName.Contains("Permission to Approach"))
                    {
                        fileName = "Denver Permission to Approach.pdf";
                        folder = "Quick Links~Permission to Approach~Denver";
                    }
                    break;

                case "05":
                    //if (fileName.Contains("Consent Form"))
                    //{
                    //    fileName = "Nationwide HalfPint Consent Form.pdf";
                    //    folder = "Enrollment~Sites~Nationwide";
                    //}
                    //if (fileName.Contains("Assent Form"))
                    //{
                    //    fileName = "Nationwide HalfPint Assent Form.pdf";
                    //    folder = "Enrollment~Sites~Nationwide";
                    //}
                    //if (fileName.Contains("Permission to Approach"))
                    //{
                    //    fileName = "Nationwide Permission to Approach.pdf";
                    //    folder = "Quick Links~Permission to Approach~Nationwide";
                    //}
                    break;
                
                case "06":
                    if (fileName.Contains("Consent Form"))
                    {
                        fileName = "CCHMC HalfPint Consent Form.pdf";
                        folder = "Enrollment~Sites~Cincinnati";
                    }
                    if (fileName.Contains("Assent Form"))
                    {
                        fileName = "CCHMC HalfPint Assent Form.pdf";
                        folder = "Enrollment~Sites~Cincinnati";
                    }
                    if (fileName.Contains("Permission to Approach"))
                    {
                        fileName = "CCHMC Permission to Approach.pdf";
                        folder = "Quick Links~Permission to Approach~Cincinnati";
                    }
                    break;

                case "07":
                    if (fileName.Contains("Consent Form"))
                    {
                        fileName = "PennState HalfPint Consent Form.pdf";
                        folder = "Enrollment~Sites~Penn State";
                    }
                    if (fileName.Contains("Assent Form"))
                    {
                        fileName = "PennState HalfPint Assent Form.pdf";
                        folder = "Enrollment~Sites~Penn State";
                    }
                    if (fileName.Contains("Permission to Approach"))
                    {
                        fileName = "PennState Permission to Approach.pdf";
                        folder = "Quick Links~Permission to Approach~Penn State";
                    }
                    break;

                case "08":
                    if (fileName.Contains("Permission to Approach"))
                    {
                        fileName = "Westchester Permission to Approach.pdf";
                        folder = "Quick Links~Permission to Approach~Westchester";
                    }
                    break;

                case "09":
                    if (fileName.Contains("Permission to Approach"))
                    {
                        fileName = "CHLA Permission to Approach.pdf";
                        folder = "Quick Links~Permission to Approach~CHLA";
                    }
                    break;

                case "10":
                    if (fileName.Contains("Permission to Approach"))
                    {
                        fileName = "WCHOB Permission to Approach.pdf";
                        folder = "Quick Links~Permission to Approach~WCHOB";
                    }
                    break;
                case "14":
                    if (fileName.Contains("Permission to Approach"))
                    {
                        fileName = "PCMC Permission to Approach.pdf";
                        folder = "Quick Links~Permission to Approach~PCMC";
                    }
                    break;

                default:
                    if (fileName.Contains("Consent Form"))
                    {
                        fileName = "BCH HalfPint Consent Form.pdf";
                        folder = "Enrollment~Sites~CHB";
                    }
                    if (fileName.Contains("Assent Form"))
                    {
                        fileName = "BCH HalfPint Assent Form.pdf";
                        folder = "Enrollment~Sites~CHB";
                    }
                    break;
                    

            }
        }
    }
}
