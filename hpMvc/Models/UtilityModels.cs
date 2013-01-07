using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace hpMvc.Models
{
    public class IDandName
    {
        public IDandName() { }
        public IDandName(int id, string name)
        {
            ID = id;
            Name = name;
        }
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class Site : IDandName
    {
        public string SiteID { get; set; }
    }

    public class IDandStudyID
    {
        public int ID { get; set; }
        public string StudyID { get; set; }
    }

    public class Animal
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
}