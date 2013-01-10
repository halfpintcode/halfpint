using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using hpMvc.DataBase;
using hpMvc.Models;

namespace hpMvc.Business
{
    public static class Site
    {
        public static MessageListDTO Add(IEnumerable<HttpPostedFileBase> files, SiteInfo siteInfo)
        {
            var dto = new MessageListDTO();
            using(var scope = new TransactionScope() )
            {
                try
                {
                    dto = DbUtils.AddSiteInfo(siteInfo);
                    scope.Complete();
                }
                catch (Exception)
                {

                    scope.Dispose();
                }

                return dto;
            }
        }
    }
}