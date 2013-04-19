using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace hpMvc.Models
{
    public class SiteInfo : IValidatableObject
    {
        public int Id { get; set; }
        [Required]
        public string SiteId { get; set; }
        [Required]
        public string Name { get; set; }
        public bool IsEmployeeIdRequired { get; set; }
        public string EmployeeIdRegEx { get; set; }
        public string EmployeeIdMessage { get; set; }
        public bool IsActive { get; set; }
        public int Sensor { get; set; }
        public bool HasRandomizations { get; set; }
        public bool HasStudyIds { get; set; }
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
    
}