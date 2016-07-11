using hpMvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace hpMvc.ViewModels
{
    public class FamiliesViewModel
    {
        public int RandomizedCount { get; set; }
        public FamilyContactsModel FamilyContact { get; set; }
    }
}