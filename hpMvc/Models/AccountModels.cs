using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;
using System.Data.SqlClient;

namespace hpMvc.Models
{
    public class AccountUtils
    {
        public static MembershipUser GetUserByUserName(string userName)
        {
            MembershipUser user = null;
            user = Membership.GetUser(userName);

            return user;
        }

        public static MembershipUser GetUserByEmail(string email)
        {
            MembershipUser user = null;
            string userName = Membership.GetUserNameByEmail(email);
            if (userName != null)
            {
                user = Membership.GetUser(userName);
            }
            return user;
        }

        public static List<MembershipUser> GetAllUsers()
        {
            List<MembershipUser> users = new List<MembershipUser>();
            MembershipUser user1 = new MembershipUser("AspNetSqlMembershipProvider", "Select user", "", "", "", "",
                true, false, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now);
            users.Add(user1);
            var allusers = Membership.GetAllUsers();
            foreach (MembershipUser user in allusers)
            {
                //user.UserName;
                //user.Email;
                users.Add(user);
            }
            return users;
        }
        
        public static string GetRolesForUser(string userName)
        {
            string[] roles = Roles.GetRolesForUser(userName);
            string sMyRoles = "";
            if (roles.Length > 0)
            {
                if (roles.Length == 1)
                    sMyRoles = "My role: " + roles[0];
                else
                {
                    sMyRoles = "My roles: ";
                    for (int i = 0; i < roles.Length; i++)
                    {
                        sMyRoles = sMyRoles + roles[i];
                        if (i < (roles.Length - 1))
                            sMyRoles = sMyRoles + ",";
                    }
                }
            }
            return sMyRoles;
        }

        public static string GetRoleForUser(string userName)
        {
            string[] roles = Roles.GetRolesForUser(userName);
            string sRole = "";
            if (roles.Length > 0)
            {
                if (roles.Length == 1)
                    sRole = roles[0];                
            }
            return sRole;
        }
    }


    public class ChangePasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LogOnModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        [EmailValidation(ErrorMessage="Not a valid email address")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public IEnumerable<SelectListItem> Site { get; set; }

        [Range(1, 99, ErrorMessage = "You must select a site")]
        public int SelectedSite { get; set; } 

    }

    
    
}
