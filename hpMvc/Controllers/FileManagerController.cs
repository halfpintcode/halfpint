using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using hpMvc.Infrastructure.Logging;
using hpMvc.DataBase;
using System.Configuration;

namespace hpMvc.Controllers
{    
    public class FileManagerController : Controller
    {
        NLogger nlogger = new NLogger();        
        
        [Authorize]
        public ActionResult Upload()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var path = Path.Combine(Server.MapPath("~/Docs"),fileName);
                file.SaveAs(path);
            }

            return RedirectToAction("Upload");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult TemplateUpload()
        {
            return View();
        }

        public ActionResult TemplateUploadSuccess()
        {
            return View();
        }

        public ActionResult TemplateUploadFailed()
        {
            return View();
        }

        [Authorize(Roles="Admin")]
        [HttpPost]
        public ActionResult TemplateUpload(HttpPostedFileBase file)
        {
            try
            {
                //throw new Exception("This is a test error!");
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = "Checks_tmpl.xlsm";
                    var path = Path.Combine(Server.MapPath("~/ssTemplate"), fileName);
                    file.SaveAs(path);
                }
            }
            catch (Exception ex)
            {
                nlogger.LogError(ex.Message);
                return RedirectToAction("TemplateUploadFailed");
            }

            return RedirectToAction("TemplateUploadSuccess");
        }

        //[Authorize(Roles = "Admin")]
        public ActionResult ChecksUpload(HttpPostedFileBase file)
        {
            try
            {
                //nlogger.LogInfo("ChecksUpload");
                if (file != null && file.ContentLength > 0)
                {
                    string key = Request.Form["key"];
                    string institID = Request.Form["institID"];
                    var fileName = Path.GetFileName(file.FileName);

                    nlogger.LogInfo("ChecksUpload - file name: " + fileName + ", key: " + key);

                    if (!ssUtils.VerifyKey(key, fileName, institID))
                    {
                        nlogger.LogInfo("ChecksUpload - bad key - file name: " + fileName + ", key: " + key);
                        return Content("Bad key");
                    }

                    var folderPath = ConfigurationManager.AppSettings["ChecksUploadPath"].ToString();
                    var path = Path.Combine(folderPath, institID);

                    //nlogger.LogInfo("ChecksUpload - path: " + path);
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    path = Path.Combine(path, fileName);
                    file.SaveAs(path);
                }
            }
            catch (Exception ex)
            {
                nlogger.LogError("ChecksUpload - " + ex.Message);
            }

            //nlogger.LogInfo("ChecksUpload - OK");
            return Content("OK");
        }

        public ActionResult ChecksWCUpload(HttpPostedFileBase file)
        {
            try
            {
                //nlogger.LogInfo("ChecksUpload");
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    nlogger.LogInfo("ChecksUpload - " + fileName);                    
                    var folderPath = ConfigurationManager.AppSettings["ChecksUploadPath"].ToString();

                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    var path = Path.Combine(folderPath, fileName);
                    file.SaveAs(path);
                }
            }
            catch (Exception ex)
            {
                nlogger.LogError("ChecksUpload - " + ex.Message);
            }

            //nlogger.LogInfo("ChecksUpload - OK");
            return Content("OK");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult UploadAjax()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult UploadAjax(HttpPostedFileBase file)
        {
            return View();
        }
    }
}
