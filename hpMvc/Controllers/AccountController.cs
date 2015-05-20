using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using hpMvc.Models;
using hpMvc.DataBase;
using hpMvc.Infrastructure.Logging;
using Microsoft.Security.Application;

namespace hpMvc.Controllers
{
     [RequireHttps]
    public class AccountController : Controller
    {

        NLogger nlogger = new NLogger();

        #region logon
        public ActionResult LogOn()
        {            
            return View();
        }
                
        [HttpPost]
        public ActionResult LogOn([Bind(Include = "UserName,Password,RemberMe")]LogOnModel model, string returnUrl)
        {
            nlogger.LogInfo("Logon Post - user:" + model.UserName);
            if (ModelState.IsValid)
            {
                if (Membership.ValidateUser(model.UserName, model.Password))
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    MembershipUser user = Membership.GetUser(model.UserName);
                    if (user != null)
                    {
                        nlogger.LogInfo("Logon Post Success - user:" + model.UserName);
                        if (user.Comment == "Reset")
                        {
                            return RedirectToAction("ResetPassword", "Account");
                        }

                        if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                            && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
                else
                {
                    MembershipUser user = Membership.GetUser(model.UserName);
                    if (user == null)
                    {
                        ModelState.AddModelError("", "The user name or password provided is incorrect.");
                    }
                    else
                    {

                        if (user.IsLockedOut)
                        {
                            ModelState.AddModelError("", "This user name is locked out.");
                            
                            //send email to admin
                            var admins = DbUtils.GetUserInRole("Admin", 1);
                            Utility.SendUserLockedOutMail(admins.Select(membershipUser => membershipUser.Email).ToArray(), null, model.UserName, Server, Utility.GetSiteLogonUrl(this.Request));
                        }
                        else
                            ModelState.AddModelError("", "The user name or password provided is incorrect.");
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
                
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
        #endregion logon

        #region forgot password
        public ActionResult ForgotPassword()
        {
            //ResetPasswordModel rpm = new ResetPasswordModel();
            return View();
        }
                
        [HttpPost]
        public JsonResult ForgotPassword(string email)
        {
            
            string password = "";
            DTO dto = new DTO();
            if (email.Contains("password"))
            {
                dto.IsSuccessful = false;
                dto.Message = "Could not find a user account for email: " + email;
                nlogger.LogInfo("ForgotPassword - host:" + Request.UrlReferrer.Host + ", message: " + dto.Message + ", password: " + password);

                return Json(dto);
            }

            MembershipUser user = AccountUtils.GetUserByEmail(email);
            if (user == null)
            {
                dto.IsSuccessful = false;
                dto.Message = "Could not find a user account for email: " + email;
            }
            else
            {
                //reset the password to randam passwor (forcing the user to reset)
                password = DbUtils.GetRandomPassword();
                dto.IsSuccessful = UserRolesUtils.ResetForgotPassword(user, password);
                dto.Message = "A temporary password word has been sent to your email address.";

                //email the password to the user
                var u = new UrlHelper(this.Request.RequestContext);
                string url = "http://" + this.Request.Url.Host + u.RouteUrl("Default", new { Controller = "Account", Action = "Logon" });
            
                Utility.SendPasswordResetMail(new string[] { user.Email }, null, password, true, Server, url);
            }
            string userName = "";
            if (user != null)
                userName = user.UserName;
                        
            nlogger.LogInfo("ForgotPassword - user:" + userName + ", message: " + dto.Message + ", password: " + password);
                        
            return Json(dto);
        }
        #endregion forgot password

        #region reset password
        [Authorize]
        public ActionResult ResetPassword()
        {
            var rpm = new ResetPasswordModel {UserName = HttpContext.User.Identity.Name};
            //ViewBag.User = HttpContext.User.Identity.Name;
            return View(rpm);
        }

        //
        // POST: /Account/ChangePassword

        [Authorize]
        [HttpPost]
        public ActionResult ResetPassword(string userName, [Bind(Include = "NewPassword, UserName, ConfirmPassword")] ResetPasswordModel rpm)
        {
            if (ModelState.IsValid)
            {
                userName = Encoder.HtmlEncode(userName);

                bool result = UserRolesUtils.ResetPassword(userName, rpm.NewPassword);

                var user = Membership.GetUser(userName);

                var u = new UrlHelper(this.Request.RequestContext);
                string url = "http://" + this.Request.Url.Host +
                             u.RouteUrl("Default", new {Controller = "Account", Action = "Logon"});

                Utility.SendPasswordResetMail(new string[] {user.Email}, null, rpm.NewPassword, false, Server, url);

                nlogger.LogInfo("ResetPassword - user:" + userName);
                return Json(result);
            }
            else
            {
                return Json(null);
            }
        }
        #endregion reset password
        
        public ActionResult UserProfile()
        {
            return View();
        }
        
        #region user email
        public JsonResult GetUserEmail()
        {
            DTO dto = new DTO();
            MembershipUser user = Membership.GetUser(HttpContext.User.Identity.Name);
            dto.IsSuccessful = true;
            dto.Message = user.Email;

            nlogger.LogInfo("Profile.GetUserEmail - email: " + user.Email);
            return Json(dto);
        }

        public JsonResult UpdateUserEmail()
        {
            string newEmail = Request.Params["NewEmail"];
            
            DTO dto = AccountUtils.UpdateUserEmail(newEmail, User.Identity.Name);
            
            var u = new UrlHelper(this.Request.RequestContext);
            string url = "http://" + this.Request.Url.Host + u.RouteUrl("Default", new { Controller = "Account", Action = "Logon" });
            
            //send to old and new emails
            Utility.SendHtmlEmail("Halfpint - Email Change", new string[] { newEmail }, null, dto.Message, Server, url);
            Utility.SendHtmlEmail("Halfpint - Email Change", new string[] { dto.Bag.ToString() }, null, dto.Message, Server, url);

            return Json(dto);
        }
        #endregion user email


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

        //register is not needed at this time
        //public ActionResult Register()
        //{
        //    RegisterModel rm = new RegisterModel();

        //    List<Site> sites = DbUtils.GetSites();
        //    if (sites.Count == 0)
        //        throw new Exception("There was an error retreiving the sites list from the database");

        //    sites.Insert(0, new Site { ID = 0, Name = "Select your site", SiteID = "" });

        //    rm.Site = sites.ToSelectList("ID", "Name", "0");
        //    return View(rm);
        //}

        ////
        //// POST: /Account/Register

        //[HttpPost]
        //public ActionResult Register(RegisterModel model, string SelectedSite)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        // Attempt to register the user
        //        MembershipCreateStatus createStatus;
        //        MembershipUser user = Membership.CreateUser(model.UserName, model.Password, model.Email, null, null, true, null, out createStatus);

        //        if (createStatus == MembershipCreateStatus.Success)
        //        {
        //            nlogger.LogInfo("Register - user:" + model.UserName);

        //            if (!DbUtils.AddUserSite(model.UserName, int.Parse(SelectedSite)))
        //            {
        //                throw new Exception("There was an error adding the user and site to the database");
        //            }
        //            new MailController().AccountRegisteredEmail(user).DeliverAsync();
        //            new MailController().UserRegisteredEmail(user, int.Parse(SelectedSite)).DeliverAsync();
        //            return PartialView("RegisterSuccess");
        //        }
        //        else
        //        {
        //            ModelState.AddModelError("", ErrorCodeToString(createStatus));
        //        }
        //    }

        //    // If we got this far, something failed, redisplay form
        //    List<Site> sites = DbUtils.GetSites();
        //    if (sites.Count == 0)
        //        throw new Exception("There was an error retreiving the sites list from the database");

        //    sites.Insert(0, new Site { ID = 0, Name = "Select your site", SiteID = "" });
        //    model.Site = sites.ToSelectList("ID", "Name", "0");

        //    return PartialView("RegisterPartial", model);
        //}
    }
}
