using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DocumentFormat.OpenXml.Wordprocessing;

namespace hpMvc.Models
{
    public class SiteInfo : IValidatableObject
    {
        public int Id { get; set; }
        [Required]
        public string SiteId { get; set; }
        [Required]
        public string Name { get; set; }
        public string LongName { get; set; }
        public bool IsEmployeeIdRequired { get; set; }
        public string EmployeeIdRegEx { get; set; }
        public string EmployeeIdMessage { get; set; }
        public string AcctPassword { get; set; }
        public string AcctUserName { get; set; }
        public bool IsActive { get; set; }
        public int Sensor { get; set; }
        public bool HasRandomizations { get; set; }
        public bool HasStudyIds { get; set; }
        public bool UseVampjr { get; set; }
        public bool UseCalfpint { get; set; }
        public List<InsulinConcentration> InsulinConcentrations { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(IsEmployeeIdRequired)
            {
                var regExfield = new [] { "EmployeeIdRegEx" };
                var messagefield = new[] { "EmployeeIdMessage" };
                
                if((EmployeeIdRegEx == null) || (EmployeeIdRegEx.Trim().Length == 0))
                {
                    yield return new ValidationResult("Employee Id RegEx is required when Employee Id is required.", regExfield);
                }

                if ((EmployeeIdMessage == null) || (EmployeeIdMessage.Trim().Length == 0))
                {
                    yield return new ValidationResult("Employee Id Message is required when Employee Id is required.", messagefield);
                }
            }
        }
    }

    public class InsulinConcentration
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Concentration { get; set; }
        public bool IsUsed { get; set; }
    }
    
    public class SiteRandomizationsCount
    {
        public int Count { get; set; }
        public string Name { get; set; }
        public string LongName { get; set; }
        public string SiteId { get; set; }
    }

    public class SiteGerenicNurse
    {
        public string UserName { get; set; }
        public string UserPassword { get; set; }
    }

    public class InitializeSiteSpecific
    {
        public int Sensor { get; set; }
        public bool UseVampjr { get; set; }
        public bool UseCalfpint { get; set; }
    }

    public class AddAdditionalStudyIdsModel
    {
        public string SiteId { get; set; }
        //public List<Site> Sites { get; set; }  
        public IEnumerable<SelectListItem> Sites { get; set;} 
        public string Message { get; set; }
        public string MaxId { get; set; }
        public string SiteCode { get; set; }
    }
}