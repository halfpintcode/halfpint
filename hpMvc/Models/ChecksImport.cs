using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace hpMvc.Models
{
    public class ChecksGg
    {
        public DateTime MeterTime { get; set; }
        public int MeterGlucose { get; set; }
        public string Critical { get; set; }
    }
}