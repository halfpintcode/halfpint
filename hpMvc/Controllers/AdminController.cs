using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using hpMvc.Models;
using hpMvc.DataBase;
using hpMvc.Infrastructure.Logging;
using hpMvc.Infrastructure;
using System.IO;
using System.Configuration;
using hpMvc.Reports.RandomizedSubjectsChecksCompletedRows;
using hpMvc.Services.CgmImportService;
using Microsoft.Security.Application;
using Telerik.Web.Mvc;

namespace hpMvc.Controllers
{
    //[Authorize(Roles = "Admin")] 
    public class AdminController : Controller
    {
        readonly NLogger _nlogger = new NLogger();

        //protected override void OnAuthorization(AuthorizationContext filterContext)
        //{
        //    if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
        //    {
        //        // auth failed, redirect to login page 
        //        filterContext.Result = new HttpUnauthorizedResult();
        //        return;
        //    }

        //    if (!(filterContext.HttpContext.User.IsInRole("Admin"))) //|| (filterContext.HttpContext.User.IsInRole("Coordinator"))))
        //    {
        //        filterContext.Controller.TempData.Add("RedirectReason", "You are not authorized to access this page.");
        //        filterContext.Result = new RedirectResult("~/Error/Unauthorized");

        //        return;
        //    }

        //    base.OnAuthorization(filterContext);
        //}

        public ActionResult CgmImportExceptions()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CgmImportExceptions(string data)
        {
            var exceptions = CgmImportService.GetCgmImportExceptions();
            return Json(exceptions);
        }

        [HttpPost]
        public JsonResult AddDexcomSkips(string skips)
        {
            var retVal = string.Empty;
            if (! string.IsNullOrEmpty(skips))
            {
                var subjects = skips.Split(new string[] {","}, StringSplitOptions.None);
                foreach (var subject in subjects)
                {
                    //verify subjectId
                    if (DbUtils.IsStudyIdValid(subject) == 1)
                    {
                        DbUtils.AddDexcomSkipSubject(subject);
                        retVal += "Subject " + subject + " was added, ";
                    }
                    else
                    {
                        retVal += "Could not add subject " + subject + ", ";
                    }
                }
            }
            else
            {
                retVal = "There are no subjects to add!";
            }

            return Json(retVal);
        }

        public ActionResult RandomizedChecksRowsCompleted()
        {
            var sites = DbUtils.GetSitesActive();
            sites.Insert(0, new Site { ID = 0, Name = "All sites", SiteID = "" });
            
            ViewBag.Sites = new SelectList(sites, "ID", "Name", "1");
            
            //var list = ChecksRowInfo.GetRandomizedSubjectsChecksCompletedRows(1);
            
            return View();
        }


        public JsonResult GetRadomizedChecksCompletedRows(string siteId)
        {
            var site = int.Parse(siteId);
            var list = ChecksRowInfo.GetRandomizedSubjectsChecksCompletedRows(site);
            var grid = new WebGrid(list, defaultSort: "SubjectId", rowsPerPage: 50);
            var htmlString = grid.GetHtml(tableStyle: "webgrid", headerStyle: "header", alternatingRowStyle: "alt",
                columns: grid.Columns(
                    grid.Column("SubjectId", "Subject Id"),
                    grid.Column("DateRandomized", "Randomized Date"),
                    grid.Column("ScDateCompleted", "Completed Date"),
                    grid.Column("CksFirstDate", "Chks First Date"),
                    grid.Column("CksLastDate", "Chks Last Date"),
                    grid.Column("CgmFirstDate", "Cgm First Date"),
                    grid.Column("CgmLastDate", "Cgm Last Date"),
                    grid.Column("ScChecksLastRowImported", "Last Row Import"),
                    grid.Column("ScChecksRowsCompleted", "Rows Completed"),
                    grid.Column("CksRowsCompleted", "Chks Rows"),
                    grid.Column("DbRows", "Db Rows"),
                    grid.Column("ScChecksImportCompleted", "Import Completed")
                    ));

            return Json(new { Data = htmlString.ToHtmlString() }, JsonRequestBehavior.AllowGet);
        }

        #region Index        
        [Telerik.Web.Mvc.PopulateSiteMap(SiteMapName = "admin", ViewDataKey = "admin")]
        public ActionResult Index()
        {
            string role = "Coordinator";
            if (HttpContext.User.IsInRole("Admin"))
                role = "Admin";
            ViewBag.Role = role;

            if (role == "Admin")
            {
                var sites = DbUtils.GetSitesActive();
                if (sites.Count == 0)
                    throw new Exception("There was an error retreiving the sites list from the database");
                sites.Insert(0, new Site { ID = 0, Name = "Select a site", SiteID = "" });
                ViewBag.Sites = new SelectList(sites, "ID", "Name");

            }

            _nlogger.LogInfo("Admin Index - user: " + HttpContext.User.Identity.Name + ", role: " + role);
           
            List<MembershipUser> users = new List<MembershipUser>();
            if (role == "Admin")
            {
                users = AccountUtils.GetAllUsers();
            }
            else
            {
                int siteID = DbUtils.GetSiteidIdForUser(HttpContext.User.Identity.Name);
                users = DbUtils.GetUsersForSite(siteID);
            }
                
            ViewBag.Users = new SelectList(users, "UserName", "UserName");
            if (!SiteMapManager.SiteMaps.ContainsKey("admin"))
            {
                SiteMapManager.SiteMaps.Register<XmlSiteMap>("admin", sitmap => sitmap.LoadFrom("~/admin.sitemap"));
            }
            return View();
        }

        [HttpPost]
        public JsonResult SiteSelected(string site)
        {
            _nlogger.LogInfo("SiteSelected - site: " + site);
            List<MembershipUser> users = new List<MembershipUser>();
            users = DbUtils.GetUsersForSite(int.Parse(site));
            return Json(users);
        }


        [HttpPost]
        public JsonResult UserSelected(string userName)
        {
            _nlogger.LogInfo("UserSelected - userName: " + userName);
            String[] roles = Roles.GetRolesForUser(userName);
            return Json(roles);
        }
        #endregion logon

        #region Roles
        public ActionResult ManageUserRoles(string userName)
        {
            var model = new UserRolesModel {UserName = userName};

            if ((userName == "jrezuke") && User.Identity.Name != "jrezuke")
                return RedirectToAction("Index"); 
            if (userName.Length > 0)
            {
                model.UserRoles = UserRolesUtils.GetAssignedRoles(userName);
            }
            
            return View(model);
        }

        [HttpPost]
        public ActionResult ManageUserRoles(String[] selectedRoles, string userName)
        {

            UserRolesUtils.SaveAsignedRoles(selectedRoles, userName);
            MembershipUser user = Membership.GetUser(userName);

            _nlogger.LogInfo("ManageUserRoles Post - user: " + userName + ", role: " + selectedRoles[0]);

            var u = new UrlHelper(this.Request.RequestContext);
            string url = "http://" + this.Request.Url.Host + u.RouteUrl("Default", new { Controller = "Account", Action = "Logon" });
            
            Utility.SendRoleAssignedMail(new string[] { user.Email }, null, selectedRoles[0], Server, url);
            return Json(true);
        }

        public ActionResult ManageRoles()
        {
            String[] roles = Roles.GetAllRoles();
            ViewBag.Roles = roles;
            return View();
        }

        [HttpPost]
        public JsonResult ManageRoles(string role)
        {
            if ((role.Trim()).Length > 0)
                if (!Roles.RoleExists(role))
                    Roles.CreateRole(role);

            _nlogger.LogInfo("ManageRoles Post - user: " + HttpContext.User.Identity.Name + ", role added: " + role);            
            return Json(role);
        }
        #endregion Roles

        #region ResetUserPassword
        public ActionResult ResetUserPassword(string userName)
        {
            userName = Encoder.HtmlEncode(userName);
            if ((userName == "jrezuke") && User.Identity.Name != "jrezuke")
                return RedirectToAction("Index"); 
            ResetPasswordModel rpm = new ResetPasswordModel();
            rpm.UserName = userName;
            ViewBag.User = userName;
            return View(rpm);
        }

        
        [HttpPost]
        public ActionResult ResetUserPassword(string userName, ResetPasswordModel rpm, bool reset)
        {
            userName = Encoder.HtmlEncode(userName);
            bool result = false;
            var user = Membership.GetUser(userName);
            
            if (reset)                            
                result = UserRolesUtils.ResetForgotPassword(user, rpm.NewPassword);
            else
                result = UserRolesUtils.ResetPassword(userName, rpm.NewPassword);
            
            _nlogger.LogInfo("ResetUserPassword Post - user: " + userName + ", new pasword: " + rpm.NewPassword);

            var u = new UrlHelper(this.Request.RequestContext);
            string url = "http://" + this.Request.Url.Host + u.RouteUrl("Default", new { Controller = "Account", Action = "Logon" });
            
            Utility.SendPasswordResetMail(new string[] { user.Email }, null, rpm.NewPassword, reset, Server, url);
                        
            return Json(result);
        }
        #endregion

        [HttpPost]
        public ActionResult UnlockUser(string userName)
        {
            _nlogger.LogInfo("UnlockUser Post - user: " + userName);

            bool retVal = UserRolesUtils.UnlockUser(userName);
            
            return Json(retVal);
        }

        [HttpPost]
        public ActionResult RemoveUser(string userName)
        {
            _nlogger.LogInfo("RemoveUser Post - user: " + userName);

            var dto = new DTO();
            dto.ReturnValue = DbUtils.RemoveUser(userName);
            if (dto.ReturnValue == 1)
                dto.Message = "User was removed successfully";
            else
            {
                dto.IsSuccessful = false;
                if (dto.ReturnValue == 0)
                    dto.Message = @"There was a problem with Membership removing the user";
                if (dto.ReturnValue == -1)
                    dto.Message = @"There was an error removing the user-site from the database";
            }
            _nlogger.LogInfo("RemoveUser - user: " + userName + ", message: " + dto.Message);
            return Json(dto);
        }
        
        #region AddUser        
        public ActionResult AddStaff()
        {     
            var sites = DbUtils.GetSitesActive();
            if (sites.Count == 0)
                throw new Exception("There was an error retreiving the sites list from the database");
            sites.Insert(0, new Site { ID = 0, Name = "Select a site", SiteID = "" });
            ViewBag.Sites = new SelectList(sites, "ID", "Name");
           
            var roles = Roles.GetAllRoles().ToList();
            roles.Insert(0, "Select a role");

            ViewBag.Roles = new SelectList(roles);
                        
            return View();
        }

        [HttpPost]
        public ActionResult AddStaff(StaffModel model)
        {
            if (model.Role == "Select a role")
            {
                ModelState["Role"].Errors.Add("Role is required");
            }            
            //validate model
            if (ModelState.IsValid)
            {
                MessageListDTO dto = DbUtils.AddStaff(model);
                if (dto.IsSuccessful)
                {
                    //send email notification
                    if(model.SendEmail)
                        Utility.SendAccountCreatedMail(new string[] { model.Email }, null, dto.Bag.ToString(), model.UserName, Utility.GetSiteLogonUrl(this.Request), this.Server);

                }
                return View("NewStaffConfirmation", dto);
            }


            List<Site> sites = new List<Site>();

            sites = DbUtils.GetSitesActive();
            if (sites.Count == 0)
                throw new Exception("There was an error retreiving the sites list from the database");
            sites.Insert(0, new Site { ID = 0, Name = "Select a site", SiteID = "" });
            ViewBag.Sites = new SelectList(sites, "ID", "Name", model.SiteID);

            var roles = Roles.GetAllRoles().ToList();
            roles.Insert(0, "Select a role");

            ViewBag.Roles = new SelectList(roles, model.Role);
            return View(model);
        }

        public JsonResult IsMembershipUserNameDuplicate(string userName)
        {
            if (AccountUtils.GetUserByUserName(userName) != null)
                return Json(true);
            else
                return Json(false);
        }

        public JsonResult IsUserNameDuplicate(string userName)
        {
            var dto = DbPostTestsUtils.DoesStaffUserNameExist(userName);

            return Json(dto);
        }

        public JsonResult IsUserEmailDuplicate(string email)
        {
            var dto = DbPostTestsUtils.DoesStaffEmailExist(email);
            return Json(dto);
        }

        public JsonResult IsUserEmailDuplicateOtherThan(int id, string email)
        {
            var dto = DbPostTestsUtils.DoesStaffEmailExistOtherThan(id, email);
            return Json(dto);
        }

        

        public JsonResult IsUserEmployeeIdDuplicate(string employeeID, int site)
        {            
            var dto = DbPostTestsUtils.DoesStaffEmployeeIdExist(employeeID, site);
            return Json(dto);
        }

        public JsonResult IsUserEmployeeIdDuplicateOtherThan(int id, string employeeID, int site)
        {           
            var dto = DbPostTestsUtils.DoesStaffEmployeeIdExistOtherThan(id, employeeID, site);
            return Json(dto);
        }

        public ActionResult AddUser()
        {
            AddUserModel aum = new AddUserModel();

            List<Site> sites = DbUtils.GetSitesActive();
            if (sites.Count == 0)
                throw new Exception("There was an error retreiving the sites list from the database");

            sites.Insert(0, new Site { ID = 0, Name = "Select your site", SiteID = "" });            
            aum.Site = sites.ToSelectList("ID", "Name", "0");

            return View(aum);
        }
        
        [HttpPost]
        public ActionResult AddUser(AddUserModel model, string SelectedSite)
        {
            _nlogger.LogInfo("Admin.AddUser - post");
            bool bSendEmail = Request.Params["SendEmail"] != null;

            if (ModelState.IsValid)
            {
                // Attempt to add the user
                string password = DbUtils.GetRandomPassword();
                
                MembershipCreateStatus createStatus;
                MembershipUser user = Membership.CreateUser(model.UserName, password, model.Email, null, null, true, null, out createStatus);
                
                if (createStatus == MembershipCreateStatus.Success)
                {   
                    //FormsAuthentication.SetAuthCookie(model.UserName, false /* createPersistentCookie */);                    
                    if (!DbUtils.AddUserSite(model.UserName, int.Parse(SelectedSite)))
                    {
                        throw new Exception("There was an error adding the user and site to the database");
                    }
                 
                    //this will tell us that user needs to reset
                    user.Comment = "Reset";
                    Membership.UpdateUser(user);
                    var u = new UrlHelper(this.ControllerContext.RequestContext);
                    string url = u.Action("Index", "Home");

                    //send email to user
                    if(bSendEmail)
                        Utility.SendAccountCreatedMail(new string[] { user.Email }, null, password, model.UserName, Utility.GetSiteLogonUrl(this.Request), this.Server);

                    _nlogger.LogInfo("AddUser - userName: " + model.UserName + ", site: " + SelectedSite + ", password: " + password);
                    return PartialView("AddUserSuccessPartial");
                }
                else
                {
                    ModelState.AddModelError("", DbUtils.ErrorCodeToString(createStatus));
                }
            }

            // If we got this far, something failed, redisplay form
            List<Site> sites = DbUtils.GetSitesActive();
            if (sites.Count == 0)
                throw new Exception("There was an error retreiving the sites list from the database");

            sites.Insert(0, new Site { ID = 0, Name = "Select your site", SiteID = "" });
            model.Site = sites.ToSelectList("ID", "Name", "0");

            return PartialView("AddUserPartial", model);
        }
        #endregion //AddUser

        public ActionResult ChangeUserRole()
        {
            var sites = DbUtils.GetSitesActive();
            if (sites.Count == 0)
                throw new Exception("There was an error retreiving the sites list from the database");
            sites.Insert(0, new Site { ID = 0, Name = "Select a site", SiteID = "" });
            ViewBag.Sites = new SelectList(sites, "ID", "Name");

            int site = DbUtils.GetSiteidIdForUser(User.Identity.Name);
            ViewBag.Site = site;

            var list = DbUtils.GetStaffLookupForSite(site.ToString());
            list.Insert(0, new Site { ID = 0, Name = "Select a member", SiteID = "" });
            ViewBag.Users = new SelectList(list, "ID", "Name");

            var roles = Roles.GetAllRoles();
            
            ViewBag.Roles = new SelectList(roles);
            return View();
        }

        [HttpPost]
        public JsonResult ChangeUserRole(string newUserName, string curUserName, string role, string staffId, string siteId, string email, string empId)
        {
            var userName = "";
            if (newUserName.Length > 0)
            {
                userName = newUserName;
                //create new membership user
                MembershipCreateStatus createStatus;
                MembershipUser user = Membership.CreateUser(userName, "halfpint", email, null, null, true, null, out createStatus);
                if (createStatus == MembershipCreateStatus.Success)
                {
                    if (!DbUtils.AddUserSite(userName, int.Parse(siteId)))
                    {
                        throw new Exception("There was an error adding the user and site to the database");
                    }

                    //this will tell us that user needs to reset
                    user.Comment = "Reset";
                    Membership.UpdateUser(user);
                }
            }
            else
            {
                userName = curUserName;
            }

            UserRolesUtils.ChangeUserRole(role, userName);
            
            //update staff info
            var dto = DbUtils.UpdateStaffInfoForRoleChange(staffId, email, empId, userName, role);
            
            return Json(dto);
        }

        [HttpPost]
        public JsonResult GetStaffInfoForRoleChange(string userId)
        {
            var staffInfo = DbUtils.GetStaffInfoForRoleChange(int.Parse(userId));

            return Json(staffInfo);
        }

        public ActionResult UpdateStaffInformation()
        {
            string role = "";

            if (HttpContext.User.IsInRole("Admin"))
            {
                role = "Admin";

                var sites = DbUtils.GetSitesActive();
                if (sites.Count == 0)
                    throw new Exception("There was an error retreiving the sites list from the database");
                sites.Insert(0, new Site { ID = 0, Name = "Select a site", SiteID = "" });
                ViewBag.Sites = new SelectList(sites, "ID", "Name");
            }
            ViewBag.Role = role;

            int site = DbUtils.GetSiteidIdForUser(User.Identity.Name);
            ViewBag.Site = site;
            
            var retDto = DbPostTestsUtils.GetSiteEmployeeInfoForSite(site.ToString());
            ViewBag.EmpRequired = retDto.Stuff.EmpIDRequired;
            if (retDto.Stuff.EmpIDRequired == "true")
            {
                ViewBag.EmpRegex = retDto.Stuff.EmpIDRegex;
                ViewBag.EmpMessage = retDto.Stuff.EmpIDMessage;
            }
            else
            {
                ViewBag.EmpRegex = "";
                ViewBag.EmpMessage = "";
            }

            var list = DbUtils.GetStaffLookupForSite(site.ToString());
            list.Insert(0, new Site { ID = 0, Name = "Select a member", SiteID = "" });            
            ViewBag.Users = new SelectList(list, "ID", "Name");
            
            ViewBag.IsValid = "true";              
            
            return View(new StaffEditModel());
        }

        [HttpPost]
        public ActionResult UpdateStaffInformation(StaffEditModel model)
        {            
            //validate model
            if (ModelState.IsValid)
            {
                MessageListDTO dto = DbUtils.UpdateStaffAdmin(model);
                if (dto.IsSuccessful)
                {

                }

                if (model.Email != model.OldEmail)
                {
                    DTO dtoEmail = null; 
                    if(model.UserName != null)
                        dtoEmail = AccountUtils.UpdateUserEmail(model.Email, model.UserName);
                }
                if (model.Role != model.OldRole)
                {
                    if (model.UserName != null)
                    {
                        string[] newroles = { model.Role };
                        UserRolesUtils.SaveAsignedRoles(newroles, model.UserName);
                    }
                }
                if (model.OldActive != model.Active)
                {
                    if (model.UserName != null)
                    {                        
                        var mUser = Membership.GetUser(model.UserName);
                        if (mUser != null)
                        {
                            mUser.IsApproved = model.Active;
                            Membership.UpdateUser(mUser);
                        }
                    }
                }
                return View("UpdateStaffConfirmationPartial", dto);
            }

            //ViewBag.Error = error;
            string role = "";

            if (HttpContext.User.IsInRole("Admin"))
            {
                role = "Admin";

                var sites = DbUtils.GetSitesActive();
                if (sites.Count == 0)
                    throw new Exception("There was an error retreiving the sites list from the database");
                sites.Insert(0, new Site { ID = 0, Name = "Select a site", SiteID = "" });
                ViewBag.Sites = new SelectList(sites, "ID", "Name");
            }
            ViewBag.Role = role;

            int site = DbUtils.GetSiteidIdForUser(User.Identity.Name);
            ViewBag.Site = site;
            var retDto = DbPostTestsUtils.GetSiteEmployeeInfoForSite(site.ToString());
            ViewBag.EmpRequired = retDto.Stuff.EmpIDRequired;
            if (retDto.Stuff.EmpIDRequired == "true")
            {
                ViewBag.EmpRegex = retDto.Stuff.EmpIDRegex;
                ViewBag.EmpMessage = retDto.Stuff.EmpIDMessage;
            }
            else
            {
                ViewBag.EmpRegex = "";
                ViewBag.EmpMessage = "";
            }

            var list = DbUtils.GetStaffLookupForSite(site.ToString());
            list.Insert(0, new Site { ID = 0, Name = "Select a member", SiteID = "" });
            ViewBag.Users = new SelectList(list, "ID", "Name", model.ID.ToString());
            ViewBag.IsValid = "false";

            //need to get tests completed for model - this was not returned from the client
            var postTestsCompleted = DbPostTestsUtils.GetTestsCompleted(model.ID.ToString());
            PostTestPersonTestsCompleted ptpc = new PostTestPersonTestsCompleted();
            ptpc.PostTestsCompleted = postTestsCompleted;
            model.PostTestsCompleted = ptpc;

            var roles = Roles.GetAllRoles().ToList();
            ViewBag.Roles = new SelectList(roles, model.Role);

            return View(model);
        }

        [HttpPost]
        public ActionResult GetStaffInfo(string user)
        {
            StaffEditModel model = DbUtils.GetStaffInfo(int.Parse(user));
            
            var roles = Roles.GetAllRoles().ToList();            
            ViewBag.Roles = new SelectList(roles,model.Role);
            model.OldRole = model.Role;
            model.OldEmail = model.Email;
            model.OldActive = model.Active;
            model.OldEmployeeID = model.EmployeeID;
            return PartialView("UpdateStaffPartial", model);
        }

        public ActionResult WebLogs()
        {            
            var list = DbUtils.GetWebLogs();
            return View(list);
        }

        public ActionResult ChecksImportLog()
        {
            var list = DbUtils.GetChecksImportLog();
            return View(list);
        } 

        public ActionResult GetSavedChecks()
        {
            string role = "Coordinator";
            if (HttpContext.User.IsInRole("Admin"))
                role = "Admin";
            ViewBag.Role = role;

            List<Site> sites = new List<Site>();

            if (role == "Admin")
            {
                sites = DbUtils.GetSitesActive();
                if (sites.Count == 0)
                    throw new Exception("There was an error retreiving the sites list from the database");
                sites.Insert(0, new Site { ID = 0, Name = "Select a site", SiteID = "" });
                ViewBag.Sites = new SelectList(sites, "ID", "Name");

            }

            List<string> files = new List<string>();
            files.Add("Select file");
            ViewBag.Files = new SelectList(files);

            _nlogger.LogInfo("Admin GetSavedChecks - user: " + HttpContext.User.Identity.Name + ", role: " + role);

            return View();
        }

        public ActionResult GetSavedSensorInfo()
        {
            string role = "Coordinator";
            if (HttpContext.User.IsInRole("Admin"))
                role = "Admin";
            ViewBag.Role = role;

            var sites = new List<Site>();

            if (role == "Admin")
            {
                sites = DbUtils.GetSitesActive();
                if (sites.Count == 0)
                    throw new Exception("There was an error retreiving the sites list from the database");
                sites.Insert(0, new Site { ID = 0, Name = "Select a site", SiteID = "" });
                ViewBag.Sites = new SelectList(sites, "ID", "Name");

            }

            List<string> files = new List<string>();
            files.Add("Select file");
            ViewBag.Files = new SelectList(files);

            _nlogger.LogInfo("Admin GetSavedSensorInfo - user: " + HttpContext.User.Identity.Name + ", role: " + role);

            return View();
        }
        
        [HttpPost]
        public JsonResult GetSavedChecksSiteChange(string site)
        {
            _nlogger.LogInfo("SiteSelected - site: " + site);
            var dto = DbUtils.GetSiteCodeForSiteId(int.Parse(site));
            var files = SsUtils.GetSavedStudyIDs(dto.Bag.ToString());
            
            return Json(files);
        }

        [HttpPost]
        public JsonResult GetSavedSensorInfoSiteChange(string site)
        {
            _nlogger.LogInfo("SiteSelected - site: " + site);
            //var dto = DbUtils.GetSiteCodeForSiteID(int.Parse(site));
            var files = SsUtils.GetSavedSensorFiles(site);

            return Json(files);
        }

        public FilePathResult GetSavedChecksDownload(string site, string fileName)
        {
            site = Encoder.HtmlEncode(site);
            fileName = Encoder.HtmlEncode(fileName);
            var dto = DbUtils.GetSiteCodeForSiteId(int.Parse(site));

            var folderPath = ConfigurationManager.AppSettings["ChecksUploadPath"].ToString();
            var path = Path.Combine(folderPath, dto.Bag.ToString());

            var fullpath = Path.Combine(path, fileName);


            _nlogger.LogInfo("GetSavedChecksDownload: " + fileName);
            return this.File(fullpath, "application/vnd.ms-excel.sheet.macroEnabled.12", fileName.Replace("copy", ""));
        }

        public FilePathResult GetSavedSensorInfoDownload(string site, string fileName)
        {
            site = Encoder.HtmlEncode(site);
            fileName = Encoder.HtmlEncode(fileName);
            var folderPath = ConfigurationManager.AppSettings["CgmUploadPath"].ToString();
            var path = Path.Combine(folderPath, site);

            var fullpath = Path.Combine(path, fileName);


            _nlogger.LogInfo("GetSavedSensorInfoDownload: " + fileName);
            return this.File(fullpath, "application/CSV", fileName);
        }

        public FilePathResult DownloadChecksTemplate()
        {
            var fileName = "Checks_tmpl.xlsm";
            var fullpath = Path.Combine(Server.MapPath("~/ssTemplate"), fileName);
            _nlogger.LogInfo("GetChecksTemplate");
            return this.File(fullpath, "application/vnd.ms-excel.sheet.macroEnabled.12", fileName.Replace("copy", ""));
        }

        public ActionResult TestEmail()
        {
            MembershipUser user = Membership.GetUser(User.Identity.Name);
            //var u = new UrlHelper(this.Request.RequestContext);
            //string host = this.Request.Url.Host;
            //string urlLeftPart = Request.Url.Scheme + Uri.SchemeDelimiter + host;
            var model = new TestEmailModel();
            model.Url = Utility.GetSiteLogonUrl(this.Request);
            model.AbsoluteUri = this.Request.Url.AbsoluteUri;
            model.AbsolutePath = this.Request.Url.AbsolutePath;
            model.LocalPath = this.Request.Url.LocalPath;
            model.Authority = this.Request.Url.Authority;
            model.DnsSafeHost = this.Request.Url.DnsSafeHost;

            model.Host = this.Request.Url.Host;

            model.Email = user.Email;
            return View(model);
        }

        [HttpPost]
        public ActionResult TestEmail(string email)
        {
            var to = new[] { email };
            
            var u = new UrlHelper(this.Request.RequestContext);
            string url = "http://" + this.Request.Url.Host + u.RouteUrl("Default", new { Controller = "Account", Action = "Logon" });
            try
            {
                Utility.SendTestMail(to, null, url, Server);
                return Json("Email has been sent!");
            }
            catch (Exception ex)
            {
                return Json(ex.InnerException.Message);
            }

        }

        public ActionResult BroadcastEmail()
        {
            List<Site> sites = new List<Site>();
            sites = DbUtils.GetSitesActive();
            //sites.Insert(0, new Site { ID = 0, Name = "All Sites" });
            if (sites.Count == 0)
                throw new Exception("There was an error retreiving the sites list from the database");
            ViewBag.sites = new SelectList(sites, "ID", "Name");

            var roles = Roles.GetAllRoles().ToList();
            //roles.Insert(0, "All Roles");
            
            ViewBag.roles = new SelectList(roles);

            MembershipUserCollection mUsers = DbUtils.GetAllUsers();
            ViewBag.users = new SelectList(mUsers, "Email","UserName");
            return View();
        }

        public JsonResult BroadcastSend(int[] sites, String[] roles, String[] users, string subject, string body)
        {
            var to = new List<string>();
            if (users != null)
                to = users.ToList();

            to.AddRange(DbUtils.GetUserEmails(sites, roles));

            var u = new UrlHelper(this.Request.RequestContext);
            string url = "http://" + this.Request.Url.Host + u.RouteUrl("Default", new { Controller = "Account", Action = "Logon" });

            Utility.SendBroadcastMail(to.ToArray(), null, url, Server, subject, body);

            return Json("OK");
        }
        
        public ActionResult ShowUsers()
        {
            List<Site> sites;
            sites = DbUtils.GetSitesActive();
            if (sites.Count == 0)
                throw new Exception("There was an error retreiving the sites list from the database");
            ViewBag.sites = new SelectList(sites, "ID", "Name");

            String[] roles = Roles.GetAllRoles();
            ViewBag.roles = new SelectList(roles);

            var mUsers = DbUtils.GetAllUserInfo(); 
            
            return View(mUsers);
        }

        public ActionResult PostTestsService()
        {
            var sites = DbUtils.GetSitesActiveForNovanetList();
            
            if (sites.Count == 0)
                throw new Exception("There was an error retreiving the sites list from the database");
            sites.Insert(0, new Site { ID = 0, Name = "Select a site", SiteID = "" });
            ViewBag.Sites = new SelectList(sites, "ID", "Name");

            return View();
        }

        [HttpPost]
        public JsonResult PostTestsService(string emailParam, string runType, string siteId)
        {
            if (runType == "0")
            {
                PostTestService.PostTestService.Execute(Server, emailParam);
                return Json("Successful");
            }
            else
            {
                var sites = PostTestService.PostTestService.Execute(Server,emailParam, int.Parse(siteId), true);
                return Json(sites);
            }
            
        }
    }
}
