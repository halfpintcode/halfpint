using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;

namespace hpMvc.Models
{
    public class TestEmailModel
    {
        public string Url  { get; set; }
        public string Host { get; set; }
        public string Authority { get; set; }
        public string DnsSafeHost { get; set; }
        public string AbsoluteUri { get; set; }
        public string AbsolutePath { get; set; }
        public string LocalPath { get; set; }
        public string Email { get; set; }
    }
}