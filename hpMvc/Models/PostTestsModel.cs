using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace hpMvc.Models
{
    public class PostTestsModel
    {
        public PostTestsModel()
        {
            OverviewCompleted = null;
            ChecksCompleted = null;
            MedtronicCompleted = null;
            NovaStatStripCompleted = null;
            VampJrCompleted = null;
        }

        public string Name { get; set; }
        
        public bool Overview { get; set; }
        [Display(Name = "Enter date completed")]
        public DateTime? OverviewCompleted { get  ; set; }
        
        public bool Checks { get; set; }
        [Display(Name = "Enter date completed")]
        public DateTime? ChecksCompleted { get; set; }
        
        public bool Medtronic { get; set; }
        [Display(Name = "Enter date completed")]
        public DateTime? MedtronicCompleted { get; set; }
        
        [Display(Name = "Nova Stat Strip")]
        public bool NovaStatStrip { get; set; }
        [Display(Name = "Enter date completed")]
        public DateTime? NovaStatStripCompleted { get; set; }
        
        [Display(Name = "Vamp Jr")]
        public bool VampJr { get; set; }
        [Display(Name = "Enter date completed")]
        public DateTime? VampJrCompleted { get; set; }

    }

    public class PostTest
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTime? DateCompleted { get; set; }
        public string sDateCompleted { get; set; }        
    }

    public class PostTestNextDue
    {
        public string Name { get; set; }
        public DateTime? NextDueDate { get; set; }
        public string sNextDueDate { get; set; }
        public string email { get; set; }
    }

    public class PostTestExtended
    {
        public string PersonName { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTime? DateCompleted { get; set; }
        public string sDateCompleted { get; set; }
        public string email { get; set; }
    }

    public class PostTestPersonTestsCompleted
    {
        public string Name { get; set; }
        public List<PostTest> _postTestsCompleted;

        public List<PostTest> PostTestsCompleted
        {
            get 
            {
                if(_postTestsCompleted == null)
                    _postTestsCompleted = new List<PostTest>();
                return _postTestsCompleted; 
            }
            set { _postTestsCompleted = value; }
        }

    }
}