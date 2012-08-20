using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace hpMvc.Models
{
    public class StaffModel
    {
        public int ID { get; set; }
        public int SiteID { get; set; }
        public bool Active { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string EmployeeID { get; set; }
        public string Phone { get; set; }
    }
}