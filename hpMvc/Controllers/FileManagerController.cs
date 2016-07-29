using System;
using System.Web;
using System.Web.Mvc;
using System.IO;
using hpMvc.Infrastructure.Logging;
using hpMvc.DataBase;
using System.Configuration;
using Microsoft.Security.Application;

namespace hpMvc.Controllers
{    
    public class FileManagerController : Controller
    {
        NLogger nlogger = new NLogger();

        public ActionResult UploadTest()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UploadTest(HttpPostedFileBase file, string other)
        {
            if (file != null && file.ContentLength > 0)
            {
                //string path;
                var fileName = Path.GetFileName(file.FileName);
                
            }

            return RedirectToAction("Upload");
        }

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
                if (fileName != null)
                {
                    var path = Path.Combine(Server.MapPath("~/Docs"), fileName);
                    file.SaveAs(path);
                }
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

        [Authorize(Roles = "Admin")]
        public ActionResult ImageUpload()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult ImageUpload(HttpPostedFileBase file)
        {
            try
            {
                //throw new Exception("This is a test error!");
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = "Checks_tmpl.xlsm";
                    var path = Path.Combine(Server.MapPath("~/content/images"), fileName);
                    file.SaveAs(path);
                }
            }
            catch (Exception ex)
            {
                nlogger.LogError(ex.Message);
                return RedirectToAction("ImageUploadFailed");
            }

            return RedirectToAction("ImageUploadSuccess");
        }

        public ActionResult ImageUploadSuccess()
        {
            return View();
        }

        public ActionResult ImageUploadFailed()
        {
            return View();
        }
        //[Authorize(Roles = "Admin")]
        //public ActionResult ChecksUpload(HttpPostedFileBase file)
        //{
        //    try
        //    {
        //        nlogger.LogInfo("ChecksUpload");
        //        if (file != null && file.ContentLength > 0)
        //        {
        //            //string key = Request.Form["key"];
        //            string institId = Request.Form["institID"];
        //            //key = Encoder.HtmlEncode(key);
        //            institId = Encoder.HtmlEncode(institId);

        //            //filename template : 01-0030-7copy.xlsm
        //            var fileName = Path.GetFileName(file.FileName);
        //            var studyId = fileName.Substring(0, 9);

        //            int iRetVal = DbUtils.IsStudyIdValid(studyId);
        //            if (iRetVal != 1)
        //            {
        //                nlogger.LogInfo("ChecksUpload - file name: " + fileName + ", IsStudyIdValid: " + iRetVal);
        //                return Content("IsStudyIdValid: " + iRetVal);
        //            }

        //            iRetVal = DbUtils.IsStudyIdCleared(studyId);
        //            if (iRetVal != 0)
        //            {
        //                nlogger.LogInfo("ChecksUpload - file name: " + fileName + ", IsStudyCleared: " + iRetVal);
        //                return Content("IsStudyCleared: " + iRetVal);
        //            }
        //            nlogger.LogInfo("ChecksUpload - file name: " + fileName ); //, key: " + key);

        //            //if (!SsUtils.VerifyKey(key, fileName, institId))
        //            //{
        //            //    nlogger.LogInfo("ChecksUpload - bad key - file name: " + fileName + ", key: " + key);
        //            //    return Content("Bad key");
        //            //}

        //            var folderPath = ConfigurationManager.AppSettings["ChecksUploadPath"].ToString();
        //            var path = Path.Combine(folderPath, institId);

        //            //nlogger.LogInfo("ChecksUpload - path: " + path);
        //            if (!Directory.Exists(path))
        //                Directory.CreateDirectory(path);

        //            path = Path.Combine(path, fileName);


        //            file.SaveAs(path);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        nlogger.LogError("ChecksUpload - " + ex.Message);
        //    }

        //    //nlogger.LogInfo("ChecksUpload - OK");
        //    return Content("OK");
        //}

        //public ActionResult ChecksWCUpload(HttpPostedFileBase file)
        //{
        //    try
        //    {
        //        //nlogger.LogInfo("ChecksUpload WebClient");
        //        if (file != null && file.ContentLength > 0)
        //        {
        //            string key = Request.QueryString["key"];
        //            string institID = Request.QueryString["institID"];

        //            //filename template : 01-0030-7copy.xlsm
        //            var fileName = Path.GetFileName(file.FileName);
        //            var studyID = fileName.Substring(0, 9);

        //            int iRetVal = DbUtils.IsStudyIdCleared(studyID);
        //            if (iRetVal != 0)
        //            {
        //                nlogger.LogInfo("ChecksUpload - file name: " + fileName + ", IsStudyCleared: " + iRetVal);
        //                return Content("IsStudyCleared: " + iRetVal);
        //            }
        //            nlogger.LogInfo("ChecksUpload - file name: " + fileName + ", key: " + key);

        //            if (!SsUtils.VerifyKey(key, fileName, institID))
        //            {
        //                nlogger.LogInfo("ChecksUpload - bad key - file name: " + fileName + ", key: " + key);
        //                return Content("Bad key");
        //            }

        //            var folderPath = ConfigurationManager.AppSettings["ChecksUploadPath"].ToString();
        //            var path = Path.Combine(folderPath, institID);

        //            //nlogger.LogInfo("ChecksUpload - path: " + path);
        //            if (!Directory.Exists(path))
        //                Directory.CreateDirectory(path);

        //            path = Path.Combine(path, fileName);
        //            file.SaveAs(path);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        nlogger.LogError("ChecksUpload - " + ex.Message);
        //    }

        //    //nlogger.LogInfo("ChecksUpload - OK");
        //    return Content("OK");
        //}

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
