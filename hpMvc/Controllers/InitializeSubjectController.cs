using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using hpMvc.Models;
using hpMvc.DataBase;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using hpMvc.Infrastructure.Logging;
using System.Configuration;
using System.Web.Security;

namespace hpMvc.Controllers
{
    [Authorize(Roles="Admin, Coordinator, PI, Investigator")]
    public class InitializeSubjectController : Controller
    {

        NLogger logger = new NLogger();
 
        public ActionResult Sensor()
        {
            SensorModel sm = new SensorModel();
            return View(sm);
        }

        public ActionResult Initialize()
        {
            logger.LogInfo("InitializeSubject.Initialize: GET");
            int siteId = DbUtils.GetSiteidIDForUser(User.Identity.Name);
            var insCons = DbUtils.GetInsulinConcItems(siteId);
            insCons.Insert(0, new InsulinConcentration { Name = "Select ", Concentration = "" });

            ViewBag.Concentrations = new SelectList(insCons, "Concentration", "Name");

            List<IDandName> sl = DbUtils.GetLookupItems("SensorLocations");
            if (sl.Count == 0)
                throw new Exception("There was an error retrieving the senor locations list from the database");
            sl.Insert(0, new Site { ID = 0, Name = "Select"});
            
            int useSensor = DbUtils.DoesSiteUseSensor(siteId);
            
            ViewBag.SensorLocations = new SelectList(sl, "ID", "Name");
            ViewBag.UseSensor = useSensor;

            return View();
        }

        [HttpPost]
        public ActionResult Initialize(string studyID)
        {
            logger.LogInfo("InitializeSubject.Initialize: " + studyID);
            
            int siteId = DbUtils.GetSiteidIDForUser(User.Identity.Name);
            int useSensor = DbUtils.DoesSiteUseSensor(siteId);

            InitializeDTO dto = new InitializeDTO();
            dto.IsSuccessful = true;
            
            List<ValidationMessages> messages = null;
            SSInsertionData ssInsert = null;
            dto.ValidMessages = messages;
                        
            if (!DbInitializeContext.IsValidInitialize(this.Request.Params, useSensor, out messages, out ssInsert))
            {
                dto.IsSuccessful = false;
                dto.Message = "There are validation errors";
                logger.LogInfo("InitializeSubject.Initialize - validation errors: " + studyID);
            }

            if (dto.IsSuccessful)
            {
                if (useSensor == 1)
                {
                    //save the sensor data
                    if (!ssUtils.AddSenorData(studyID, this.Request.Params))
                    {
                        dto.IsSuccessful = false;
                        dto.Message = "Could not add sensor data";

                        logger.LogInfo("InitializeSubject.Initialize - could not save sensor data: " + studyID);
                    }
                    logger.LogInfo("InitializeSubject.Initialize - AddSenorData: " + studyID);
                }

                if (dto.IsSuccessful)
                {

                    TempData["InsertData"] = ssInsert;

                    string[] users = ConfigurationManager.AppSettings["InitializeSubject"].ToString().Split(new[] {','},
                                                                                                            StringSplitOptions
                                                                                                                .None);

                    List<string> toEmails = new List<string>();
                    foreach (var user in users)
                    {
                        var mUser = Membership.GetUser(user);
                        if (mUser == null)
                            continue;
                        toEmails.Add(mUser.Email);
                    }

                    string siteName = DbUtils.GetSiteNameForUser(User.Identity.Name);

                    var u = new UrlHelper(this.Request.RequestContext);
                    string url = "http://" + this.Request.Url.Host +
                                 u.RouteUrl("Default", new {Controller = "Home", Action = "Index"});

                    Utility.SendStudyInitializedMail(toEmails.ToArray(), null, studyID, User.Identity.Name, siteName,
                                                     Server,
                                                     url);
                }
            }
            if(dto.IsSuccessful)
            {
                logger.LogInfo("InitializeSubject.Initialize - is successful: " + dto.IsSuccessful.ToString());
            }
            else
            {
                logger.LogInfo("InitializeSubject.Initialize - failed: " + dto.Message);
            }
            return Json(dto);
        }

        //[HttpPost]
        public FilePathResult InitializeSS(string studyID)
        {
            int siteId = DbUtils.GetSiteidIDForUser(User.Identity.Name);
            int useSensor = DbUtils.DoesSiteUseSensor(siteId);

            SSInsertionData ssInsert = (SSInsertionData)TempData["InsertData"];
            int iret = ssUtils.SetRandomization(studyID, ref ssInsert, User.Identity.Name);
            if (iret == -1)
            {


            }

            ssUtils.InitializeSS(this.Request.PhysicalApplicationPath, studyID, ssInsert, useSensor);

            string path = this.Request.PhysicalApplicationPath + "xcel\\" + studyID.Substring(0, 2) + "\\";
            string file = path + studyID + ".xlsm";

            string fileDownloadName = "";
            if (!path.Contains("Prod"))            
                studyID = "T" + studyID;
            
            fileDownloadName = studyID + ".xlsm";            
            

            logger.LogInfo("InitializeSS: " + studyID);
            return this.File(file, "application/vnd.ms-excel.sheet.macroEnabled.12", fileDownloadName);
        }

        public FilePathResult AlternateSSDownload(string studyID)
        {
            string path = this.Request.PhysicalApplicationPath + "xcel\\" + studyID.Substring(0, 2) + "\\";
            string file = "";
            if(studyID.IndexOf(".xlsm")> -1)
                file = path + studyID;
            else
                file = path + studyID + ".xlsm";

            string fileDownloadName = "";
            if (!path.Contains("Prod"))
                studyID = "T" + studyID;

            fileDownloadName = studyID;  
            
            logger.LogInfo("AlternateSSDownload: " + studyID);
            return this.File(file, "application/vnd.ms-excel.sheet.macroEnabled.12", fileDownloadName);
        }   

        [HttpPost]
        public ActionResult ValidateLogin(string studyID, string password)
        {
            logger.LogInfo("ValidateLogin: " + studyID + ", password: " + password);
            DTO dto = new DTO();
            //check if study id begins with the correct site
            string siteID = DbUtils.GetSiteIDForUser(HttpContext.User.Identity.Name);
            if (siteID == "error")
            {
                dto.IsSuccessful = false;
                dto.Message = "There was an error retrieving the user's site id from the database";
                logger.LogInfo("ValidateLogin: " + dto.Message);
                return Json(dto);
            }
            if (siteID == "")
            {
                dto.IsSuccessful = false;
                dto.Message = "There was a problem retrieving the user's site id from the database";
                logger.LogInfo("ValidateLogin: " + dto.Message);
                return Json(dto);
            } 
            if (siteID != studyID.Substring(0, 2))
            {
                dto.IsSuccessful = false;
                dto.Message = "The study id for your site must begin with " + siteID;
                logger.LogInfo("ValidateLogin: " + dto.Message);
                return Json(dto);
            }

            //check if correct password
            dto.ReturnValue = DbUtils.IsStudyIDAssignedPasswordValid(studyID, password);
            if (dto.ReturnValue != 1)
            {
                dto.IsSuccessful = false;
                if(dto.ReturnValue == 0)
                    dto.Message = "This is not a valid study id and or password";
                if (dto.ReturnValue == -1)
                    dto.Message = "There was an error in determining if this is a valid login";

                logger.LogInfo("ValidateLogin IsStudyIDAssignedPasswordValid: " + dto.Message);
                return Json(dto);
            }

            //check if already randomized
            dto.ReturnValue = DbUtils.IsStudyIDRandomized(studyID);
            if (dto.ReturnValue != 0) 
            {
                dto.IsSuccessful = false;
                if(dto.ReturnValue == 1)
                    dto.Message = "This study id has been randomized";
                if (dto.ReturnValue == -1)
                    dto.Message = "There was an error in determining if this sudy subject has been previously radomized";
                logger.LogInfo("ValidateLogin: " + dto.Message);
                return Json(dto);
            }

            logger.LogInfo("ValidateLogin: password was valid" );
            dto.IsSuccessful = true;
            return Json(dto);
        }

        public ActionResult InitializePassword()
        {            
            var ipm = new InitializePasswordModel();
            logger.LogInfo("InitializePassword: Get");
            return View(ipm);
        }
                
        [HttpPost]
        public ActionResult InitializePassword(string studyId)
        {
            logger.LogInfo("InitializePassword: Post");

            //Request.ApplicationPath;
            var consentDate = this.Request.Form["ConsentDate"].ToString();
            var consentTime = this.Request.Form["ConsentTime"].ToString();

            var dto = new RandomizePaswordDTO
                          {IsSuccessful = true, Message = "", ReturnValue = DbUtils.IsStudyIDValid(studyId)};

            if (dto.ReturnValue != 1)
            {
                dto.IsSuccessful = false;
                if (dto.ReturnValue == 0)
                    dto.Message = "This study id was not found in the list of valid study id's";
                if (dto.ReturnValue == -1)
                    dto.Message = "There was an error in determining if this is a valid study id";
                logger.LogInfo("InitializePassword: IsStudyIDValid: " + dto.Message);
                return Json(dto);
            }
            logger.LogInfo("InitializePassword Study ID is Valid");

            dto.ReturnValue = DbUtils.IsStudyIDAssignedPassword(studyId);
            if (dto.ReturnValue != 0)
            {
                dto.IsSuccessful = false;
                if (dto.ReturnValue == 1)
                    dto.Message = "This study id is already assigned a password";
                if (dto.ReturnValue == -1)
                    dto.Message = "There was an error in determining if this study id was previously assigned a password";                
                logger.LogInfo("InitializePassword: IsStudyIDAssignedPassword: " + dto.Message);
                return Json(dto);
            }
            logger.LogInfo("InitializePassword: Study ID is not Assigned Password");

            //get the random password
            Animal animal = DbUtils.GetRandomAnimal();
            if (animal == null)
            {
                dto.IsSuccessful = false;
                dto.ReturnValue = -1;
                dto.Message = "There was an error retreiving the password from the database";
                logger.LogInfo("InitializePassword: GetRandomAnimal: " + dto.Message);
                return Json(dto);
            }
            dto.Password = animal.Name;
            logger.LogInfo("InitializePassword: GetRandomAnimal: " + dto.Password);


            //add the randomization password / studyid to the db           
            dto.ReturnValue =DbUtils.AddRandomizationPassword(studyId, animal.ID, consentDate, consentTime);
            if (dto.ReturnValue != 1)
            {
                dto.IsSuccessful = false;
                dto.ReturnValue = -1;
                dto.Message = "This study id could not be saved to the randomization table";
                logger.LogInfo("InitializePassword: AddRandomizationPassword: " + dto.Message);
            }           
            return Json(dto);
        }                 
                
    }
}
