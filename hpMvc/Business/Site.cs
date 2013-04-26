﻿using System;
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
        public static MessageListDTO Add(IList<HttpPostedFileBase> files, SiteInfo siteInfo, Uri url )
        {
            var dto = new MessageListDTO { ReturnValue = 1 };

            bool isTestSite = url.LocalPath.Contains("hpTest");

            using (var scope = new TransactionScope())
            {
                try
                {
                    //check for duplicate name
                    var retVal = DbUtils.IsSiteNameDuplicate(siteInfo.Name);
                    if (retVal != 0)
                    {
                        if (retVal == 1)
                            dto.Dictionary.Add("Name", siteInfo.Name + " is already in the database");
                        else
                            dto.Dictionary.Add("Name", "There was a problem checking for a duplicate name");
                        dto.ReturnValue = 0;
                        scope.Dispose();
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
                        scope.Dispose();
                        return dto;
                    }

                    //check for import files
                    if (files[0] == null || files[1] == null)
                    {
                        dto.Dictionary.Add("importFiles", "One or both import files are missing");
                        dto.ReturnValue = 0;
                        scope.Dispose();
                        return dto;
                    }
                    
                    //study ids import file
                    string fileName = files[0].FileName.ToLower();                      
                    
                    if (! fileName.Contains("studyids"))
                    {
                        dto.Dictionary.Add("importFiles", "The study id's import file name is not correct, it must be named: studyids" + siteInfo.SiteId + ".cvs");
                        dto.ReturnValue = 0;
                        scope.Dispose();
                        return dto;
                    }
                    
                    //study ids import file
                    fileName = files[1].FileName.ToLower();

                    if (!fileName.Contains("randomizations"))
                    {
                        dto.Dictionary.Add("importFiles", "The radomization's import file name is not correct, it must be named: randomizations" + siteInfo.SiteId + ".cvs");
                        dto.ReturnValue = 0;
                        scope.Dispose();
                        return dto;
                    }

                    dto = DbUtils.AddSiteInfo(siteInfo);
                    if (dto.ReturnValue < 1)
                    {
                        scope.Dispose();
                        return dto;
                    }
                    
                    //check format and parse
                    var studyidLines = CheckFileFormatStudyId(files[0], siteInfo.SiteId, dto);
                    if (dto.ReturnValue == 0)
                    {
                        scope.Dispose();
                        return dto;
                    }
                    if (DbUtils.DoesStudyIdsExistForSite(siteInfo.Id, dto) != 0)
                    {
                        scope.Dispose();
                        return dto;
                    }
                    if (!AddStudyidsToDb(studyidLines, siteInfo.Id, dto))
                    {
                        scope.Dispose();
                        return dto;
                    }

                    //check format and parse
                    var randomizations = CheckFileFormatRandomization(files[1], siteInfo.SiteId, dto, isTestSite);
                    if (dto.ReturnValue == 0)
                    {
                        scope.Dispose();
                        return dto;
                    }
                    if (DbUtils.DoesRandomizationsExistForSite(siteInfo.Id, dto) != 0)
                    {
                        scope.Dispose();
                        return dto;
                    }
                    if (!AddRandomizationsToDb(randomizations, siteInfo.Id, dto))
                    {
                        scope.Dispose();
                        return dto;
                    }

                    dto.IsSuccessful = true;
                    scope.Complete();
                    
                }
                catch (Exception)
                {
                    scope.Dispose();
                }
            }
            return dto;
        }

        private static bool AddRandomizationsToDb(List<RandomizationLines> randomizations, int siteId, MessageListDTO dto)
        {
            foreach (var randomization in randomizations)
            {
                if (!DbUtils.AddRandomizationForNewSite(randomization.Number, randomization.Arm, siteId, dto))
                    return false;
            }
            return true;
        }

        private static List<RandomizationLines> CheckFileFormatRandomization(HttpPostedFileBase file, string siteId, MessageListDTO dto, bool isTestSite)
        {
            var randomizations = new List<RandomizationLines>();

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
                    if (line.Length == 0)
                        continue;

                    var sCols = line.Split(new string[] { "," }, StringSplitOptions.None);
                    var numberCol = sCols[1];

                    var sParts = numberCol.Split(new string[] { "-" }, StringSplitOptions.None);
                    string sPartSiteId = sParts[0];

                    if (sPartSiteId != siteId)
                    {
                        dto.Dictionary.Add("importFiles", "The randomizations do not begin with the correct site id. They begin with " + sPartSiteId + ", they should begin with " + siteId + ".");
                        dto.ReturnValue = 0;
                        return null;
                    }

                    var rndmz = new RandomizationLines();
                    rndmz.Number = numberCol;
                    string armCol = sCols[8];
                    if (!(armCol == "TGC-1" || armCol == "TGC-2"))
                    {
                        dto.Dictionary.Add("importFiles", "The randomizations arm column are not valid. Found " + armCol + ", should be either TGC-1 or TGC-2.");
                        dto.ReturnValue = 0;
                        return null;
                    }
                    if (isTestSite)
                        armCol = "TGC-1";

                    rndmz.Arm = armCol;

                    randomizations.Add(rndmz);
                }
                return randomizations;
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

                    lines.Add(line.Trim());

                }
                return lines;
            }
        }

        public static bool ImportRandomizatioFile()
        {
            return true;
        }
    }
    
    public class RandomizationLines
    {
        public string Number { get; set; }
        public string Arm { get; set; }
    }
}