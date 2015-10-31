using System;
using System.Collections.Generic;
using System.Web.Mvc;
using hpMvc.Helpers;
using hpMvc.Models;
using hpMvc.DataBase;
using hpMvc.Infrastructure.Logging;
using Microsoft.Security.Application;

namespace hpMvc.Controllers
{
    [Authorize(Roles="Admin, Coordinator, PI, Investigator")]
    public class InitializeSubjectController : Controller
    {
        readonly NLogger _logger = new NLogger();
 
        public ActionResult Sensor()
        {
            var sm = new SensorModel();
            return View(sm);
        }

        public ActionResult PrintPassword()
        {
            var ipm = new InitializePasswordModel
                          {Password = Request.Params["password"], StudyID = Request.Params["studyID"]};
            return View(ipm);
        }

        public ActionResult Initialize()
        {
            _logger.LogInfo("InitializeSubject.Initialize: GET");
            int siteId = DbUtils.GetSiteidIdForUser(User.Identity.Name);
            var insCons = DbUtils.GetInsulinConcItems(siteId);
            insCons.Insert(0, new InsulinConcentration { Name = "Select ", Concentration = "" });

            ViewBag.Concentrations = new SelectList(insCons, "Concentration", "Name");

            var sl = DbUtils.GetLookupItems("SensorLocations");
            if (sl.Count == 0)
                throw new Exception("There was an error retrieving the senor locations list from the database");
            sl.Insert(0, new Site { ID = 0, Name = "Select"});
            
            var inititalizeSiteSpecific = DbUtils.GetSiteSpecificForInitialize(siteId);
            
            ViewBag.SensorLocations = new SelectList(sl, "ID", "Name");
            ViewBag.SensorType = inititalizeSiteSpecific.Sensor;
            ViewBag.UseCafpint = inititalizeSiteSpecific.UseCalfpint;
            ViewBag.UseVampjr = inititalizeSiteSpecific.UseVampjr;

            _logger.LogInfo("InitializeSubject.Initialize: GET, Site: " + siteId);
            return View();
        }

        [HttpPost]
        public ActionResult Initialize(string studyId)
        {
            var messages = new List<ValidationMessages>();
            var dto = new InitializeDTO { IsSuccessful = true, ValidMessages = messages };

            studyId = Encoder.HtmlEncode(studyId);
            
            if (DbUtils.IsStudyIdValid(studyId) != 1)
            {
                dto.IsSuccessful = false;
                messages.Add(new ValidationMessages
                {
                    FieldName = "invalidStudyId",
                    DisplayName = "Study ID",
                    Message = "is invalid"
                });
                _logger.LogInfo("InitializeSubject.Initialize - invalid study id: " + studyId);
                return Json(dto);
            }
            
            _logger.LogInfo("InitializeSubject.Initialize - Post: " + studyId);
            
            var siteId = DbUtils.GetSiteidIdForUser(User.Identity.Name);
            var language = DbUtils.GetSiteLanguage(siteId);
            var sensorType = int.Parse( Request.Params["sensorType"]); 

            
            SSInsertionData ssInsert;

            _logger.LogInfo("InitializeSubject.Initialize - validation: " + studyId);
            if (!DbInitializeContext.IsValidInitialize(Request.Params, out messages, out ssInsert))
            {
                dto.ValidMessages = messages;
                dto.IsSuccessful = false;
                dto.Message = "There are validation errors";
                _logger.LogInfo("InitializeSubject.Initialize - validation errors: " + studyId);
            }

            if (dto.IsSuccessful)
            {
                _logger.LogInfo("InitializeSubject.Initialize - validated");
                _logger.LogInfo("InitializeSubject.Initialize - Add senor data to database: " + studyId + ", sensor: " + sensorType);
                if (sensorType > 0)
                {
                    //save the sensor data
                    if (!SsUtils.AddSenorData(studyId, ssInsert))
                    {
                        dto.IsSuccessful = false;
                        dto.Message = "Could not add sensor data";

                        _logger.LogInfo("InitializeSubject.Initialize - could not save sensor data: " + studyId);
                    }
                }

                bool useCafpint = bool.Parse(Request.Params["cafpint"]);
                bool? cafpintConsent = null;
                int? inrGreater3 = null;
                string cafpintId = null;

                var dateRandomized = DateTime.Parse(Request.Params["randDate"] + " " + Request.Params["randTime"]);
                    
                //set next randomiztion and update database
                if (dto.IsSuccessful)
                {
                    _logger.LogInfo("InitializeSubject.Initialize - sensor data added");
                    _logger.LogInfo("InitializeSubject.Initialize - SetRandomization: " + studyId);
                    
                    if (useCafpint)
                    {
                        if (Request.Params["cafpintYesNo"] == "yes")
                        {
                            cafpintConsent = true;
                           
                            if (Request.Params["cafpintYes"] == "yes")
                                inrGreater3 = 1;
                            else if(Request.Params["cafpintYes"] == "no")
                                inrGreater3 = 2;
                            else
                                inrGreater3 = 3;

                            if (inrGreater3 > 1)
                                cafpintId = Request.Params["cafPintId"];
                        }
                        else
                        {
                            cafpintConsent = false;
                        }
                    }

                    var onInsulinYesNo = Request.Params["onInsulinYesNo"];
                    bool onInsulinInfusion = onInsulinYesNo == "yes";

                    var iret = SsUtils.SetRandomization(studyId, cafpintConsent, inrGreater3, cafpintId, ref ssInsert, User.Identity.Name, dateRandomized, onInsulinInfusion);
                    if (iret == -1)
                    {
                        dto.IsSuccessful = false;
                        dto.Message = "Could not set randomization";

                        _logger.LogInfo("InitializeSubject.Initialize - SetRandomization encountered a problem: " + studyId);
                    }
                }

                //insert data into checks template
                if (dto.IsSuccessful)
                {
                    _logger.LogInfo("InitializeSubject.Initialize - Randomization set");
                    _logger.LogInfo("InitializeSubject.InitializeSs - CHECKS inititalization");
                    if (! SsUtils.InitializeSs(Request.PhysicalApplicationPath, studyId, ssInsert, sensorType, language))
                    {
                        dto.IsSuccessful = false;
                        dto.Message = "Could not initialized CHECKS";

                        _logger.LogInfo("InitializeSubject.InitializeSs - CHECKS inititalization encountered a problem");
                    }
                    _logger.LogInfo("InitializeSubject.InitializeSs - CHECKS initialized");
                }

                //send email notifications
                if (dto.IsSuccessful)
                {
                    _logger.LogInfo("InitializeSubject.Initialize - notifications: " + studyId);
                    TempData["InsertData"] = ssInsert;

                    //var users = ConfigurationManager.AppSettings["InitializeSubject"].Split(new[] { ',' }, StringSplitOptions.None);
                    var staff = NotificationUtils.GetStaffForEvent(1, siteId);
                    
                    string siteName = DbUtils.GetSiteNameForUser(User.Identity.Name);

                    var u = new UrlHelper(Request.RequestContext);
                    if (Request.Url != null)
                    {
                        string url = "http://" + Request.Url.Host +
                                     u.RouteUrl("Default", new { Controller = "Home", Action = "Index" });

                        // don't let notifications error stop initialization process
                        try
                        {
                            if (!url.Contains("hpProd"))
                            {
                                staff.Clear();
                                staff.Add("j.rezuke@verizon.net");
                                staff.Add("Jamin.Alexander@childrens.harvard.edu");
                            }
                            Utility.SendStudyInitializedMail(staff.ToArray(), null, studyId, User.Identity.Name,
                                    siteName, Server, url, ssInsert.Arm, cafpintId, dateRandomized);
                            _logger.LogInfo("InitializeSubject.Initialize - notifications sent: " + studyId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("InitializeSubject.Initialize - error sending notifications: " + ex.Message);
                        }
                    }
                }
            }
            if(dto.IsSuccessful)
            {
                _logger.LogInfo("InitializeSubject.Initialize - is successful: " + dto.IsSuccessful.ToString());
            }
            else
            {
                _logger.LogInfo("InitializeSubject.Initialize - failed: " + dto.Message);
            }
            return Json(dto);
        }

        //[HttpPost]
        public FilePathResult InitializeSs(string studyId)
        {
            studyId = Encoder.HtmlEncode(studyId);
            _logger.LogInfo("Initialize.InitializeSS: " + studyId);

            //int siteId = DbUtils.GetSiteidIDForUser(User.Identity.Name);
            //int sensorType = DbUtils.GetSiteSensor(siteId);

           // var ssInsert = (SSInsertionData)TempData["InsertData"];
            
            string path = Request.PhysicalApplicationPath + "xcel\\" + studyId.Substring(0, 2) + "\\";
            string file = path + studyId + ".xlsm";
            
            if (!path.Contains("Prod"))            
                studyId = "T" + studyId;

            string fileDownloadName = studyId + ".xlsm";

            _logger.LogInfo("Initialize.InitializeSS - file download: " + studyId);
            return this.File(file, "application/vnd.ms-excel.sheet.macroEnabled.12", fileDownloadName);
        }

        public FilePathResult AlternateSSDownload(string studyId)
        {
            studyId = Encoder.HtmlEncode(studyId);
            var subjectId = studyId.Replace(".xlsm","");

            if (DbUtils.IsStudyIdValid(subjectId) != 1)
                return null;

            _logger.LogInfo("Initialize.AlternateSSDownload: " + studyId);
            string path = this.Request.PhysicalApplicationPath + "xcel\\" + studyId.Substring(0, 2) + 
                "\\";
            string file = path + studyId;
            
            string fileDownloadName = "";
            if (!path.Contains("Prod"))
                studyId = "T" + studyId;

            fileDownloadName = studyId;

            _logger.LogInfo("Initialize.AlternateSSDownload - file download: " + studyId);
            return this.File(file, "application/vnd.ms-excel.sheet.macroEnabled.12", fileDownloadName);
        }   

        [HttpPost]
        public ActionResult ValidateLogin(string studyID, string password)
        {
            _logger.LogInfo("ValidateLogin: " + studyID + ", password: " + password);
            var dto = new DTO();
            
            //check if study id begins with the correct site
            string siteId = DbUtils.GetSiteCodeForUser(HttpContext.User.Identity.Name);
            if (siteId == "error")
            {
                dto.IsSuccessful = false;
                dto.Message = "There was an error retrieving the user's site id from the database";
                _logger.LogInfo("ValidateLogin: " + dto.Message);
                return Json(dto);
            }
            if (siteId == "")
            {
                dto.IsSuccessful = false;
                dto.Message = "There was a problem retrieving the user's site id from the database";
                _logger.LogInfo("ValidateLogin: " + dto.Message);
                return Json(dto);
            }
            if (siteId != studyID.Substring(0, 2))
            {
                dto.IsSuccessful = false;
                dto.Message = "The study id for your site must begin with " + siteId;
                _logger.LogInfo("ValidateLogin: " + dto.Message);
                return Json(dto);
            }

            //check if correct password
            dto.ReturnValue = DbUtils.IsStudyIdAssignedPasswordValid(studyID, password);
            if (dto.ReturnValue != 1)
            {
                dto.IsSuccessful = false;
                if (dto.ReturnValue == 0)
                    dto.Message = "This is not a valid study id and or password";
                if (dto.ReturnValue == -1)
                    dto.Message = "There was an error in determining if this is a valid login";

                _logger.LogInfo("ValidateLogin IsStudyIDAssignedPasswordValid: " + dto.Message);
                return Json(dto);
            }

            //check if already randomized
            dto.ReturnValue = DbUtils.IsStudyIdRandomized(studyID);
            if (dto.ReturnValue != 0)
            {
                dto.IsSuccessful = false;
                if (dto.ReturnValue == 1)
                    dto.Message = "This study id has been randomized";
                if (dto.ReturnValue == -1)
                    dto.Message = "There was an error in determining if this sudy subject has been previously radomized";
                _logger.LogInfo("ValidateLogin: " + dto.Message);
                return Json(dto);
            }

            _logger.LogInfo("ValidateLogin: password was valid" );
            dto.IsSuccessful = true;
            return Json(dto);
        }

        public ActionResult InitializePassword()
        {            
            var ipm = new InitializePasswordModel();
            _logger.LogInfo("InitializePassword: Get");
            return View(ipm);
        }
                
        [HttpPost]
        public ActionResult InitializePassword(string studyId)
        {
            _logger.LogInfo("InitializePassword: Post");
            studyId = Encoder.HtmlEncode(studyId);

            //Request.ApplicationPath;
            var consentDate = this.Request.Form["ConsentDate"].ToString();
            var consentTime = this.Request.Form["ConsentTime"].ToString();

            var dto = new RandomizePaswordDTO
                          {IsSuccessful = true, Message = "", ReturnValue = DbUtils.IsStudyIdValid(studyId)};

            if (dto.ReturnValue != 1)
            {
                dto.IsSuccessful = false;
                if (dto.ReturnValue == 0)
                    dto.Message = "This study id was not found in the list of valid study id's";
                if (dto.ReturnValue == -1)
                    dto.Message = "There was an error in determining if this is a valid study id";
                _logger.LogInfo("InitializePassword: IsStudyIDValid: " + dto.Message);
                return Json(dto);
            }
            _logger.LogInfo("InitializePassword Study ID is Valid");

            dto.ReturnValue = DbUtils.IsStudyIdAssignedPassword(studyId);
            if (dto.ReturnValue != 0)
            {
                dto.IsSuccessful = false;
                if (dto.ReturnValue == 1)
                    dto.Message = "This study id is already assigned a password";
                if (dto.ReturnValue == -1)
                    dto.Message = "There was an error in determining if this study id was previously assigned a password";                
                _logger.LogInfo("InitializePassword: IsStudyIDAssignedPassword: " + dto.Message);
                return Json(dto);
            }
            _logger.LogInfo("InitializePassword: Study ID is not Assigned Password");

            //get the random password
            Animal animal = DbUtils.GetRandomAnimal();
            if (animal == null)
            {
                dto.IsSuccessful = false;
                dto.ReturnValue = -1;
                dto.Message = "There was an error retreiving the password from the database";
                _logger.LogInfo("InitializePassword: GetRandomAnimal: " + dto.Message);
                return Json(dto);
            }
            dto.Password = animal.Name;
            _logger.LogInfo("InitializePassword: GetRandomAnimal: " + dto.Password);


            //add the randomization password / studyid to the db           
            dto.ReturnValue =DbUtils.AddRandomizationPassword(studyId, animal.ID, consentDate, consentTime);
            if (dto.ReturnValue != 1)
            {
                dto.IsSuccessful = false;
                dto.ReturnValue = -1;
                dto.Message = "This study id could not be saved to the randomization table";
                _logger.LogInfo("InitializePassword: AddRandomizationPassword: " + dto.Message);
            }           
            return Json(dto);
        }                 
                
    }
}
