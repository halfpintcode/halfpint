using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace hpMvc.Models
{
    public class ChecksNovaBolldGlucoseReportModel
    {
        public ChecksNovaBolldGlucoseReportModel()
        {
            ChecksGgs = new List<ChecksGg>();
        }
        public string SubjectId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public List<ChecksGg> ChecksGgs { get; set; }
    }

    public class ChecksGg
    {
        [DisplayName("Date")]
        public string MeterTime { get; set; }
        [DisplayName("Nova Blood Glucose  (mg/dL)")]
        public int MeterGlucose { get; set; }
        public string Critical { get; set; }
    }
}