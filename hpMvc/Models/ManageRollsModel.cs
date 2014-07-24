using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Security;
using System.Web.Mvc;
using hpMvc.Infrastructure.Logging;

namespace hpMvc.Models
{
    public static class UserRolesUtils
    {
        public static NLogger nlogger;
        static UserRolesUtils()
        {
            nlogger = new NLogger();
        }

        public static List<UserRole> GetAssignedRoles(string userName)
        {
            
            nlogger.LogInfo("GetAssignedRoles for user:" + userName);
            
            String[] allRoles = null;
            String[] assignedRoles = null;
            try
            {
                allRoles = Roles.GetAllRoles();
                assignedRoles = Roles.GetRolesForUser(userName);
            }
            catch(Exception ex)
            {
                nlogger.LogFatal(ex);
            }

            List<UserRole> roles = new List<UserRole>();
            foreach (var allRole in allRoles)
            {
                UserRole ur = new UserRole();
                ur.RoleName = allRole;
                foreach (var ar in assignedRoles)
                {
                    if (ar == allRole)
                        ur.IsAssigned = true;
                }

                roles.Add(ur);
            }

            return roles;
        }

        public static void ChangeUserRole(String newRole, string userName)
        {
            String[] assignedRolls = Roles.GetRolesForUser(userName);
            var oldRole = "";
            foreach (var role in assignedRolls)
            {
                oldRole = role;
                Roles.RemoveUserFromRole(userName, role);
            }
            Roles.AddUserToRole(userName, newRole);

            nlogger.LogInfo("Changed assigned role for user:" + userName + ", old role:" + oldRole + ". new role:" + newRole);
        }

        public static void SaveAsignedRoles(String[] assignedRolls, string userName)
        {

            bool isAssigned = false;
            String[] allRoles = Roles.GetAllRoles();
            foreach (var role in allRoles)
            {
                isAssigned = false;
                foreach (var ar in assignedRolls)
                {
                    if (ar == role)
                    {
                        if (!Roles.IsUserInRole(userName, role))
                            Roles.AddUserToRole(userName, role);
                        isAssigned = true;
                    }
                }
                if (!isAssigned)
                {
                    if (Roles.IsUserInRole(userName, role))
                        Roles.RemoveUserFromRole(userName, role);
                }

            }
            nlogger.LogInfo("SaveAsignedRoles for user:" + userName);
        }

        public static bool ResetPassword(string userName, string newPassword)
        {
            MembershipUser user = Membership.GetUser(userName);
            if (user.IsLockedOut)
                user.UnlockUser();
            if (user.Comment == "Reset")
                user.Comment = "";
            Membership.UpdateUser(user);

            string resetPassword = user.ResetPassword();
            return user.ChangePassword(resetPassword, newPassword);
        }

        public static bool ResetForgotPassword(MembershipUser user, string newPassword)
        {
            if (user.IsLockedOut)
                user.UnlockUser();
            user.Comment = "Reset";
            Membership.UpdateUser(user);

            string resetPassword = user.ResetPassword();
            return user.ChangePassword(resetPassword, newPassword);
        }

        public static bool UnlockUser(string userName)
        {
            MembershipUser user = Membership.GetUser(userName);
            if (user.IsLockedOut)
                user.UnlockUser();            
            Membership.UpdateUser(user);
            return true;           
        }
    }    

    public class ResetPasswordModel
    {        
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string UserName { get; set; }
        
    }

    public class UserRolesModel
    {
        public UserRolesModel()
        {
            UserRoles = new List<UserRole>();
        }
        public string UserName { get; set; }
        public List<UserRole> UserRoles;

    }

    public class UserRole
    {
        public UserRole()
        {
            IsAssigned = false;
        }

        public string RoleName { get; set; }
        public bool IsAssigned { get; set; }

    }

    
}