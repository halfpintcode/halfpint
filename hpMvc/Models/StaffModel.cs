using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace hpMvc.Models
{
    public class StaffModel
    {
        public int ID { get; set; }
        [Range(1,100,ErrorMessage="Site is required")]
        public int SiteID { get; set; }
        [Required]
        public string Role { get; set; }
        public bool Active { get; set; }
        public bool SendEmail { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string EmployeeID { get; set; }
        [Required]        
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

    public class StaffEditModel
    {
        public int ID { get; set; }
        public int SiteID { get; set; }
        public string Role { get; set; }
        public string OldRole { get; set; }
        public bool Active { get; set; }
        public bool OldActive { get; set; }
        public bool SendEmail { get; set; }
        public string UserName { get; set; }
        public string OldUserName { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string Email { get; set; }
        public string OldEmail { get; set; }
        public string EmployeeID { get; set; }
        public string OldEmployeeID { get; set; }  
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
        public PostTestPersonTestsCompleted PostTestsCompleted { get; set; }
        public PostTestPersonTestsCompleted PostTestsCompletedHistory { get; set; }
    }

}