using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace hpMvc.Models
{
    public class SiteInfo
    {
        public int Id { get; set; }
        public string SiteId { get; set; }
        public string Name { get; set; }
        public bool IsEmployeeIdRequired { get; set; }
        public string EmployeeIdRegEx { get; set; }
        public string EmployeeIdMessage { get; set; }
        public bool IsActive { get; set; }
        public bool UseSensor { get; set; }
    }
}