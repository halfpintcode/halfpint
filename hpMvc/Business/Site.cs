using System;
using System.Collections.Generic;
using System.IO;
using System.Transactions;
using System.Web;
using hpMvc.DataBase;
using hpMvc.Models;

namespace hpMvc.Business
{
    public static class Site
    {
        public static MessageListDTO Add(IList<HttpPostedFileBase> files, SiteInfo siteInfo)
        {
            var dto = new MessageListDTO {ReturnValue = 1};

            using (var scope = new TransactionScope())
            {
                try
                {
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

                    //check for import files
                    if (files[0] == null || files[1] == null)
                    {
                        dto.Dictionary.Add("importFiles", "One or both import files are missing");
                        dto.ReturnValue = 0;
                        return dto;
                    }
                    
                    //study ids import file
                    string fileName = files[0].FileName.ToLower();                      
                    
                    if (! fileName.Contains("studyids"))
                    {
                        dto.Dictionary.Add("importFiles", "The stud id's import file name is not correct, it must be named: studyids" + siteInfo.SiteId + ".cvs");
                        dto.ReturnValue = 0;
                        return dto;
                    }
                    else
                    {
                        //check format and parse
                        var studyidLines = CheckFileFormatStudyId(files[0], siteInfo.SiteId, dto);
                        if (dto.ReturnValue == 0)
                            return dto;
                        if (DbUtils.DoesStudyIdsExistForSite(siteInfo.Id, dto) != 0)
                        {
                            
                        }
                        if (! AddStudyidsToDb(studyidLines, siteInfo.Id, dto))
                        {
                            return dto;
                        }
                    }
                    
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

        private static bool AddStudyidsToDb(List<string> studyidLines, int siteId, MessageListDTO dto)
        {
            foreach (var line in studyidLines)
            {
                if (!DbUtils.AddStudyId(line, siteId, dto))
                    return false;
            }
            return true;
        }

        

        private static List<string> CheckFileFormatStudyId(HttpPostedFileBase file, string siteId, MessageListDTO dto)
        {
            using (var srdr = new StreamReader(file.InputStream))
            {
                //var count = 0;
                var lines = new List<string>();
                while (true)
                {
                    //count++;
                    var line = srdr.ReadLine();
                    
                    if (line == null)
                        break;
                    if(line.Length == 0)
                        continue;

                    var sParts = line.Split(new string[] { "-" }, StringSplitOptions.None);
                    string sPartSiteId = sParts[0];
                    if (sPartSiteId != siteId)
                    {
                        dto.Dictionary.Add("importFiles", "The study id's do not begin with the correct site id. They begin with " + sPartSiteId + ", they should begin with " + siteId + ".");
                        dto.ReturnValue = 0;
                        return null;
                    }

                    lines.Add(line);

                }
                return lines;
            }
        }

        public static bool ImportRandomizatioFile()
        {
            return true;
        }
    }
}