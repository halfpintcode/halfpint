using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using hpMvc.Models;
using System.Web.Security;
using hpMvc.Infrastructure.Logging;

namespace hpMvc.DataBase
{
    public static class AccountUtils
    {
        public static NLogger nlogger;
        static AccountUtils()
        {
            nlogger = new NLogger();
        }

        public static DTO UpdateUserEmail(string newEmail, string userName)
        {
            DTO dto = new DTO();

            MembershipUser user = Membership.GetUser(userName);
            string oldEmail = user.Email;

            string userName2 = Membership.GetUserNameByEmail(newEmail);

            //check for another user with the same email as the new one
            if (userName2 != null)
            {
                if (userName2 != userName)
                {
                    dto.IsSuccessful = false;
                    dto.Message = "The email address, " + newEmail + " could not be saved. It is being used by another user";
                    nlogger.LogInfo("UpdateUserEmail - user:" + user.UserName + ", message: " + dto.Message);
                    return dto;
                }
            }

            user.Email = newEmail;
            Membership.UpdateUser(user);

            dto.IsSuccessful = true;
            dto.Message = "You email has been changed to " + newEmail + ". Contact your coordinator if you did not request this change";
            nlogger.LogInfo("UpdateUserEmail - user:" + user.UserName + ", message: " + dto.Message);
            dto.Bag = oldEmail;
            return dto;
        }

        public static MembershipUser GetUserByUserName(string userName)
        {
            
                
            MembershipUser user = null;
            if (userName != null)
                user = Membership.GetUser(userName);

            return user;
        }

        public static MembershipUser GetUserByEmail(string email)
        {
            MembershipUser user = null;
            if (email != null)
            {
                string userName = Membership.GetUserNameByEmail(email);

                if (userName != null)
                {
                    user = Membership.GetUser(userName);
                }
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
}