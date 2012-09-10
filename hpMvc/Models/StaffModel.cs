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
        public string Role { get; set; }
        public bool Active { get; set; }
        public bool SendEmail { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string EmployeeID { get; set; }
        public string Phone { get; set; }
        public bool NovaStatStrip { get; set; }
        public DateTime? NovaStatStripDoc { get; set; }
        public bool Vamp { get; set; }
        public DateTime? VampDoc { get; set; }
        public bool Cgm { get; set; }
        public DateTime? CgmDoc { get; set; }
        public bool Inform { get; set; }
        public DateTime? InformDoc { get; set; }
        public bool OnCall { get; set; }
        public DateTime? OnCallDoc { get; set; }
        public bool HumanSubj { get; set; }
        public DateTime? HumanSubjStart { get; set; }
        public DateTime? HumanSubjExp { get; set; }
    }
}