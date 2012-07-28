using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

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

    public class SubjectCompleted
    {       
        public int ID { get; set; }
        [Display(Name="Study ID")]
        public string StudyID { get; set; }
        [Display(Name = "Date Randomized")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/MM/yy}", ApplyFormatInEditMode = true)]
        public DateTime DateRandomized { get; set; }
        [Display(Name = "Date Randomized")]
        public string sDateRandomized { get; set; }
        [Display(Name = "Date Completed")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/MM/yy}", ApplyFormatInEditMode = true)]
        public DateTime? DateCompleted { get; set; }
        [Display(Name = "Date Completed")]
        public string sDateCompleted { get; set; }
        [Display(Name = "Monitor ID")]
        public string MonitorID { get; set; }
        [Display(Name = "CGM Upload")]
        public bool CgmUpload { get; set; }        
        public bool Older2 { get; set; }
        [Display(Name = "CGM Upload")]
        public bool CBCL { get; set; }        
        public bool PedsQL { get; set; }        
        public bool Demographics { get; set; }        
        public bool ContactInfo { get; set; }        
        public bool Cleared { get; set; }
        public string NotCompletedReason { get; set; }
        public string SiteName { get; set; }
    }
}