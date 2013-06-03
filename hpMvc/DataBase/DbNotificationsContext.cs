using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using hpMvc.Infrastructure.Logging;
using System.Web.Security;
using hpMvc.Models;

namespace hpMvc.DataBase
{
    public static class DbNotificationsUtils
    {
        public static NLogger Nlogger;

        static DbNotificationsUtils()
        {
            Nlogger = new NLogger();
        }
    }
}