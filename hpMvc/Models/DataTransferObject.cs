﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Dynamic;

namespace hpMvc.Models
{
    public class DTO
    {
        public DTO()
        {
            Dictionary = new Dictionary<string, string>();
        }

        public bool IsSuccessful { get; set; }
        public int ReturnValue { get; set; }
        public string Message { get; set; }
        public object Bag { get; set; }
        public Dictionary<string, string> Dictionary { get; set; } 
    }

    public class DynamicDTO: DTO
    {
        public DynamicDTO():base()
        {
            Stuff = new ExpandoObject();
        }
        
        public dynamic Stuff { get; set; }

    }

    public class MessageListDTO:DTO
    {
        public MessageListDTO():base()
        {
            Messages = new List<string>();
        }
        public List<string> Messages { get; set; }
    }

    public class RandomizePaswordDTO : DTO
    {
        public string Password { get; set; }
    }

    public class InitializeDTO:DTO
    {
        public List<ValidationMessages> ValidMessages { get; set; }  
    }

    public class ValidationMessages
    {
        public string FieldName { get; set; }
        public string DisplayName { get; set; }
        public string Message { get; set; }
    }
}