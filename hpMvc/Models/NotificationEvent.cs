using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace hpMvc.Models
{
    public class NotificationEvent
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public bool Active { get; set; }
    }

    public class StaffSubscriptions
    {
        public StaffSubscriptions()
        {
            NotificationEvents = new List<NotificationEvent>();
        }

        public int StaffId { get; set; }
        public string StaffName { get; set; }
        public List<NotificationEvent> NotificationEvents { get; set; } 
    }
}