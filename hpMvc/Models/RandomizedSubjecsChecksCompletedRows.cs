using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace hpMvc.Models
{
    public class RandomizedSubjecsChecksCompletedRowsModel
    {
        public int StudyId { get; set; }
        public string SubjectId { get; set; }
        public DateTime? DateRandomized { get; set; }
        public DateTime? ScDateCompleted { get; set; }
        public bool ScChecksImportCompleted { get; set; }
        public int ScChecksLastRowImported { get; set; }
        public int ScChecksRowsCompleted { get; set; }
        public int CksRowsCompleted { get; set; }
        public int DbRows { get; set; }
        public DateTime? CksFirstDate { get; set; }
        public DateTime? CksLastDate { get; set; }
        public DateTime? CgmFirstDate { get; set; }
        public DateTime? CgmLastDate { get; set; }

    }
}