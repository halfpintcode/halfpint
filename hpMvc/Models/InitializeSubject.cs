using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace hpMvc.Models
{
    public class InitializePasswordModel
    {        

        [Required]
        [Display(Name = "Study ID")]
        [RegularExpression("^\\d{2}-\\d{4}-\\d$", ErrorMessage="Must match format: nn-nnnn-n")]
        public string StudyID { get; set; }                
        public string Password { get; set; }
    }

    public class StudyID
    {
        public int ID { get; set; }
        public int SiteID { get; set; }
        public string SstudyID { get; set; }                
        
    }

    public class Randomization
    {
        public int ID { get; set; }
        public int SiteID { get; set; }
        public string SiteName { get; set; }
        public string Number { get; set; }
        public string StudyID { get; set; }
        public string Arm { get; set; }
        public DateTime DateRandomized { get; set; }
        public string sDateRandomized { get; set; }
        public DateTime DateCompleted { get; set; }
    }

    public class StudyCompleted
    {
        public string StudyID { get; set; }
        public DateTime DateRandomized { get; set; }
        public DateTime DateCompleted { get; set; }
        public string MonitorID { get; set; }
        public bool CgmUpload { get; set; }
        public bool Older2 { get; set; }
        public bool Cbcl { get; set; }
        public bool PedsQL { get; set; }
        public bool Demographics { get; set; }
        public bool ContactInfo { get; set; }
        public bool Cleared { get; set; }
        public string NotCompletedReason { get; set; }
    }
}