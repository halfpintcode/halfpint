using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace hpMvc.Models
{
    public class ChecksGg
    {
        [DisplayName("Date")]
        public string MeterTime { get; set; }
        [DisplayName ("Nova Blood Glucose  (mg/dL)")]
        public int MeterGlucose { get; set; }
        public string Critical { get; set; }
    }
}