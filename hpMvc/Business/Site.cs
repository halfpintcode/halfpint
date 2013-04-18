using System;
using System.Collections.Generic;
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
            using (var scope = new TransactionScope())
            {
                try
                {
                    //check for import files
                    //if (files[0] == null || files[1] == null)
                    //{
                    //    dto.Dictionary.Add("importFiles", "One or both import files are missing");
                    //    return dto;
                    //}

                    foreach (var file in files)
                    {
                        

                    }
                    //check for duplicate name
                    int retVal = DbUtils.IsSiteNameDuplicate(siteInfo.Name);
                    if (retVal != 0)
                    {
                        if (retVal == 1)
                            dto.Dictionary.Add("Name", siteInfo.Name + " is already in the database");
                        else
                            dto.Dictionary.Add("Name", "There was a problem checking for a duplicate name");
                        dto.ReturnValue = 0;
                        return dto;
                    }
                    //check for duplicat site id
                    retVal = DbUtils.IsSiteIdDuplicate(siteInfo.SiteId);
                    if (retVal != 0)
                    {
                        if (retVal == 1)
                            dto.Dictionary.Add("SiteId", siteInfo.SiteId + " is already being used for site ID");
                        else
                            dto.Dictionary.Add("SiteId", "There was a problem checking for a duplicate site ID");
                        dto.ReturnValue = 0;
                        return dto;
                    }

                    //dto = DbUtils.AddSiteInfo(siteInfo);
                    scope.Complete();
                }
                catch (Exception)
                {

                    scope.Dispose();
                }

                return dto;
            }
        }

        public static bool ImportRandomizatioFile()
        {
            return true;
        }
    }
}