﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using hpMvc.Models;
using hpMvc.DataBase;
using hpMvc.Infrastructure.Logging;
using hpMvc.Infrastructure;
using System.Web.Helpers;
using System.IO;
using System.Configuration;

namespace hpMvc.Controllers
{
    //[Authorize(Roles = "Admin")] 
    public class AdminController : Controller
    {
        NLogger nlogger = new NLogger();

        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // auth failed, redirect to login page 
                filterContext.Result = new HttpUnauthorizedResult();
                return;
            }

            if (!(filterContext.HttpContext.User.IsInRole("Admin"))) //|| (filterContext.HttpContext.User.IsInRole("Coordinator"))))
            {
                filterContext.Controller.TempData.Add("RedirectReason", "You are not authorized to access this page.");
                filterContext.Result = new RedirectResult("~/Error/Unauthorized");

                return;
            }

            base.OnAuthorization(filterContext);
        }

        #region Index        
        public ActionResult Index()
        {
            string role = "Coordinator";
            if (HttpContext.User.IsInRole("Admin"))
                role = "Admin";
            ViewBag.Role = role;

            List<Site> sites = new List<Site>();

            if (role == "Admin")
            {
                sites = DbUtils.GetSites();
                if (sites.Count == 0)
                    throw new Exception("There was an error retreiving the sites list from the database");
                sites.Insert(0, new Site { ID = 0, Name = "Select a site", SiteID = "" });
                ViewBag.Sites = new SelectList(sites, "ID", "Name");

            }

            nlogger.LogInfo("Admin Index - user: " + HttpContext.User.Identity.Name + ", role: " + role);
           
            List<MembershipUser> users = new List<MembershipUser>();
            if (role == "Admin")
            {
                users = AccountUtils.GetAllUsers();
            }
            else
            {
                int siteID = DbUtils.GetSiteidIDForUser(HttpContext.User.Identity.Name);
                users = DbUtils.GetUsersForSite(siteID);
            }
                
            ViewBag.Users = new SelectList(users, "UserName", "UserName");
            return View();
        }

        [HttpPost]
        public JsonResult SiteSelected(string site)
        {
            nlogger.LogInfo("SiteSelected - site: " + site);
            List<MembershipUser> users = new List<MembershipUser>();
            users = DbUtils.GetUsersForSite(int.Parse(site));
            return Json(users);
        }


        [HttpPost]
        public JsonResult UserSelected(string userName)
        {
            nlogger.LogInfo("UserSelected - userName: " + userName);
            String[] roles = Roles.GetRolesForUser(userName);
            return Json(roles);
        }
        #endregion logon

        #region Roles
        public ActionResult ManageUserRoles(string userName)
        {
            if ((userName == "jrezuke") && User.Identity.Name != "jrezuke")
                return RedirectToAction("Index"); 
            List<UserRole> userRoles = new List<UserRole>();
            if (userName.Length > 0)
            {
                userRoles = UserRolesUtils.GetAssignedRoles(userName);
            }
            ViewBag.User = userName;
            return View(userRoles);
        }

        [HttpPost]
        public ActionResult ManageUserRoles(String[] selectedRoles, string userName)
        {

            UserRolesUtils.SaveAsignedRoles(selectedRoles, userName);
            MembershipUser user = Membership.GetUser(userName);

            nlogger.LogInfo("ManageUserRoles Post - user: " + userName + ", role: " + selectedRoles[0]);

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

            nlogger.LogInfo("ManageRoles Post - user: " + HttpContext.User.Identity.Name + ", role added: " + role);            
            return Json(role);
        }
        #endregion Roles

        #region ResetUserPassword
        public ActionResult ResetUserPassword(string userName)
        {
            if ((userName == "jrezuke") && User.Identity.Name != "jrezuke")
                return RedirectToAction("Index"); 
            ResetPasswordModel rpm = new ResetPasswordModel();
            ViewBag.User = userName;
            return View();
        }

        [HttpPost]
        public ActionResult ResetUserPassword(string userName, ResetPasswordModel rpm, bool reset)
        {
            bool result = false;
            var user = Membership.GetUser(userName);
            
            if (reset)                            
                result = UserRolesUtils.ResetForgotPassword(user, rpm.NewPassword);
            else
                result = UserRolesUtils.ResetPassword(userName, rpm.NewPassword);
            
            nlogger.LogInfo("ResetUserPassword Post - user: " + userName + ", new pasword: " + rpm.NewPassword);

            var u = new UrlHelper(this.Request.RequestContext);
            string url = "http://" + this.Request.Url.Host + u.RouteUrl("Default", new { Controller = "Account", Action = "Logon" });
            
            Utility.SendPasswordResetMail(new string[] { user.Email }, null, rpm.NewPassword, reset, Server, url);
                        
            return Json(result);
        }
        #endregion

        [HttpPost]
        public ActionResult RemoveUser(string userName)
        {
            nlogger.LogInfo("RemoveUser Post - user: " + userName);

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
            nlogger.LogInfo("RemoveUser - user: " + userName + ", message: " +dto.Message);
            return Json(dto);
        }

        #region AddUser        
        public ActionResult AddUser()
        {
            AddUserModel aum = new AddUserModel();

            List<Site> sites = DbUtils.GetSites();
            if (sites.Count == 0)
                throw new Exception("There was an error retreiving the sites list from the database");

            sites.Insert(0, new Site { ID = 0, Name = "Select your site", SiteID = "" });            
            aum.Site = sites.ToSelectList("ID", "Name", "0");

            return View(aum);
        }
        
        [HttpPost]
        public ActionResult AddUser(AddUserModel model, string SelectedSite)
        {
            nlogger.LogInfo("Admin.AddUser - post");
            string password = "";
            bool bSendEmail = false;

            if (Request.Params["SendEmail"] != null)
                bSendEmail = true;
            if (ModelState.IsValid)
            {
                // Attempt to add the user
                password = DbUtils.GetRandomPassword();
                
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

                    nlogger.LogInfo("AddUser - userName: " + model.UserName + ", site: " + SelectedSite + ", password: " + password);
                    return PartialView("AddUserSuccessPartial");
                }
                else
                {
                    ModelState.AddModelError("", ErrorCodeToString(createStatus));
                }
            }

            // If we got this far, something failed, redisplay form
            List<Site> sites = DbUtils.GetSites();
            if (sites.Count == 0)
                throw new Exception("There was an error retreiving the sites list from the database");

            sites.Insert(0, new Site { ID = 0, Name = "Select your site", SiteID = "" });
            model.Site = sites.ToSelectList("ID", "Name", "0");

            return PartialView("AddUserPartial", model);
        }
        #endregion //AddUser

        public ActionResult WebLogs()
        {            
            var list = DbUtils.GetWebLogs();
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
                sites = DbUtils.GetSites();
                if (sites.Count == 0)
                    throw new Exception("There was an error retreiving the sites list from the database");
                sites.Insert(0, new Site { ID = 0, Name = "Select a site", SiteID = "" });
                ViewBag.Sites = new SelectList(sites, "ID", "Name");

            }

            List<string> files = new List<string>();
            files.Add("Select file");
            ViewBag.Files = new SelectList(files);

            nlogger.LogInfo("Admin Index - user: " + HttpContext.User.Identity.Name + ", role: " + role);

            return View();
        }

        [HttpPost]
        public JsonResult GetSavedChecksSiteChange(string site)
        {
            nlogger.LogInfo("SiteSelected - site: " + site);
            var dto = DbUtils.GetSiteCodeForSiteID(int.Parse(site));
            var files = ssUtils.GetSavedStudyIDs(dto.Bag.ToString());
            
            return Json(files);
        }

        public FilePathResult GetSavedChecksDownload(string site, string fileName)
        {
            var dto = DbUtils.GetSiteCodeForSiteID(int.Parse(site));

            var folderPath = ConfigurationManager.AppSettings["ChecksUploadPath"].ToString();
            var path = Path.Combine(folderPath, dto.Bag.ToString());

            var fullpath = Path.Combine(path, fileName);


            nlogger.LogInfo("GetSavedChecksDownload: " + fileName);
            return this.File(fullpath, "application/vnd.ms-excel.sheet.macroEnabled.12", fileName.Replace("copy", ""));
        }

        public ActionResult TestEmail()
        {
            MembershipUser user = Membership.GetUser(User.Identity.Name);
            //var u = new UrlHelper(this.Request.RequestContext);
            //string host = this.Request.Url.Host;
            //string urlLeftPart = Request.Url.Scheme + Uri.SchemeDelimiter + host;
            ViewBag.Url = Utility.GetSiteLogonUrl(this.Request);
            ViewBag.AbsoluteUri = this.Request.Url.AbsoluteUri;
            ViewBag.AbsolutePath = this.Request.Url.AbsolutePath;
            ViewBag.LocalPath = this.Request.Url.LocalPath;
            ViewBag.Authority = this.Request.Url.Authority;
            ViewBag.DnsSafeHost = this.Request.Url.DnsSafeHost;

            ViewBag.Host = this.Request.Url.Host;

            ViewBag.Email = user.Email;
            return View();
        }

        [HttpPost]
        public ActionResult TestEmail(string email)
        {
            string[] to = new[] { email };
            
            var u = new UrlHelper(this.Request.RequestContext);
            string url = "http://" + this.Request.Url.Host + u.RouteUrl("Default", new { Controller = "Account", Action = "Logon" });
            
            Utility.SendTestMail(to, null, url, Server);

            return View();
        }

        public ActionResult BroadcastEmail()
        {
            List<Site> sites = new List<Site>();
            sites = DbUtils.GetSites();
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
            List<Site> sites = new List<Site>();
            sites = DbUtils.GetSites();
            if (sites.Count == 0)
                throw new Exception("There was an error retreiving the sites list from the database");
            ViewBag.sites = new SelectList(sites, "ID", "Name");

            String[] roles = Roles.GetAllRoles();
            ViewBag.roles = new SelectList(roles);

            var mUsers = DbUtils.GetAllUserInfo(); 
            
            return View(mUsers);
        }
        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
