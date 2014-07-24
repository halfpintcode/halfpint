using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;
using System.Data.SqlClient;

namespace hpMvc.Models
{
    public class AddUserModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        [EmailValidation(ErrorMessage = "Not a valid email address")]
        public string Email { get; set; }

        public IEnumerable<SelectListItem> Site { get; set; }

        [Range(1, 99, ErrorMessage = "You must select a site")]
        public int SelectedSite { get; set; }

    }

    public class WebLog
    {
        public DateTime LogDate { get; set; }
        public string LogLevel { get; set; }
        public string LogMessage { get; set; }

    }

    public class UserInfo
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Site { get; set; }
        public bool LockedOut { get; set; }
        public bool Online { get; set; }
        public bool Active { get; set; }
    }
}

    