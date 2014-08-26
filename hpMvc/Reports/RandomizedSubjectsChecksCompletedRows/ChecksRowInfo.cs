using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using hpMvc.DataBase;
using hpMvc.Infrastructure.Logging;
using hpMvc.Models;

namespace hpMvc.Reports.RandomizedSubjectsChecksCompletedRows
{
    public static class ChecksRowInfo
    {
        private const string TimeAccepted = "Time_accepted";
        private const string WorkSheet = "InsulinInfusionRecomendation";
        public static NLogger Nlogger;

        static ChecksRowInfo()
        {
            Nlogger = new NLogger();
        }
        public static List<RandomizedSubjecsChecksCompletedRowsModel> GetRandomizedSubjectsChecksCompletedRows(int site)
        {
            var list = new List<RandomizedSubjecsChecksCompletedRowsModel>();
            if (site > 0)
            {
                list = GetListFromDb(site);
                var siteCode = DbUtils.GetSiteCodeForSite(site);
                var fileList = GetChecksFileInfos(siteCode);
                foreach (var file in fileList)
                {
                    
                }
            }
            else
            {
                var sites = GetSitesForCompletedSubjects();
                foreach (var s in sites)
                {
                    var sList = GetListFromDb(s.Id);
                    list.AddRange(sList);
                }
            }
            return list;
        }

        private static IEnumerable<SiteInfoShort> GetSitesForCompletedSubjects()
        {
            var list = new List<SiteInfoShort>();
            SqlDataReader rdr = null;

            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = ("GetSitesForCompletedSubjects")
                    };
                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var ss = new SiteInfoShort {Id = rdr.GetInt32(1), SiteCode = rdr.GetString(2)};

                        list.Add(ss);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                }
                finally
                {
                    if (rdr != null)
                        rdr.Close();
                }
            }
            return list;
            
        }

        private static List<RandomizedSubjecsChecksCompletedRowsModel> GetListFromDb(int siteId)
        {
            var list = new List<RandomizedSubjecsChecksCompletedRowsModel>();
            SqlDataReader rdr = null;

            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandText = ("GetRandomizedSubjectsChecksCompletedRows")
                    };
                    var param = new SqlParameter("@siteId", siteId);
                    cmd.Parameters.Add(param);
                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var model = new RandomizedSubjecsChecksCompletedRowsModel();
                        
                        var pos = rdr.GetOrdinal("ID");
                        model.StudyId = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("StudyID");
                        model.SubjectId = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("DateRandomized");
                        model.DateRandomized = rdr.GetDateTime(pos);

                        pos = rdr.GetOrdinal("DateCompleted");
                        model.ScDateCompleted = rdr.GetDateTime(pos);

                        pos = rdr.GetOrdinal("ChecksImportCompleted");
                        if(!rdr.IsDBNull(pos))
                            model.ScChecksImportCompleted = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("ChecksLastRowImported");
                        if (!rdr.IsDBNull(pos))
                            model.ScChecksLastRowImported = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("ChecksRowsCompleted");
                        if (!rdr.IsDBNull(pos))
                            model.ScChecksRowsCompleted = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("FirstCgmDate");
                        if (!rdr.IsDBNull(pos))
                            model.CgmFirstDate = rdr.GetDateTime(pos);

                        pos = rdr.GetOrdinal("LastCgmDate");
                        if (!rdr.IsDBNull(pos))
                            model.CgmLastDate = rdr.GetDateTime(pos);

                        list.Add(model);
                    }
                    rdr.Close();

                    
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                }
                finally
                {
                    if (rdr != null)
                        rdr.Close();
                }
            }
            return list;
        }

        private static IEnumerable<ChecksFileInfo> GetChecksFileInfos(string siteCode)
        {
            var list = new List<ChecksFileInfo>();

            var folderPath = ConfigurationManager.AppSettings["ChecksUploadPath"];
            var path = Path.Combine(folderPath, siteCode);

            if (Directory.Exists(path))
            {
                var di = new DirectoryInfo(path);

                FileInfo[] fis = di.GetFiles();

                list.AddRange(fis.OrderBy(f => f.Name).Select(fi => new ChecksFileInfo
                {
                    FileName = fi.Name,
                    FullName = fi.FullName,
                    SubjectId = fi.Name.Replace("copy.xlsm", "")
                }));
            }
            return list;
        }

        private static void GetChecksInfo(ChecksFileInfo fileInfo, RandomizedSubjecsChecksCompletedRowsModel model)
        {
            //copy file into memory stream
            var ms = new MemoryStream();

            using (var fs = File.OpenRead(fileInfo.FullName))
            {
                fs.CopyTo(ms);
            }

            var rangeNames = GetDefinedNames(ms);
            var firstDate = "";
            var lastDate = "";
            var lastVal = "";
            var actualRows = 0;
            var row = 2;
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(ms, false))
            {
                while (true)
                {
                    if (rangeNames.ContainsKey(TimeAccepted))
                    {
                        var rangeVal = rangeNames[TimeAccepted];
                        var colName = GetRangeNameCol(rangeVal);
                        var colRow = colName + row;
                        var cellVal = GetCellValue(document.WorkbookPart, WorkSheet, colRow);
                        if (string.IsNullOrEmpty(cellVal))
                        {
                            lastDate = lastVal;
                            actualRows = row - 2;
                            break;
                        }
                        else
                        {
                            var dbl = Double.Parse(cellVal);
                            //if (dbl > 59)
                            //    dbl = dbl - 1;
                            var dt = DateTime.FromOADate(dbl);
                            cellVal = dt.ToString(CultureInfo.InvariantCulture);
                            if (row == 2)
                            {
                                firstDate = cellVal;
                            }
                            lastVal = cellVal;
                            row++;
                        }
                    }
                    else
                    {

                    }
                }
            }

        }
        
        private static string GetRangeNameCol(string rangeValue)
        {
            var aParts = rangeValue.Split('!');
            var bParts = aParts[1].Split(':');
            var colRow = bParts[0].Split('$');
            return colRow[1];
        }
        private static Dictionary<String, String> GetDefinedNames(MemoryStream fileStream)
        {
            // Given a workbook name, return a dictionary of defined names.
            // The pairs include the range name and a string 
            // representing the range.

            var retVal = new Dictionary<String, String>();
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(fileStream, false))
            {
                var wbPart = document.WorkbookPart;
                DefinedNames definedNames = wbPart.Workbook.DefinedNames;
                if (definedNames != null)
                {
                    foreach (var openXmlElement in definedNames)
                    {
                        var dn = (DefinedName)openXmlElement;
                        retVal.Add(dn.Name.Value, dn.Text);

                    }
                }
            }
            return retVal;
        }

        private static string GetCellValue(WorkbookPart wbPart, string sheetName, string addressName)
        {
            string value = null;


            // Find the sheet with the supplied name, and then use that Sheet object
            // to retrieve a reference to the appropriate worksheet.
            Sheet theSheet = wbPart.Workbook.Descendants<Sheet>().FirstOrDefault(s => s.Name == sheetName);

            if (theSheet == null)
            {
                throw new ArgumentException("sheetName");
            }

            // Retrieve a reference to the worksheet part, and then use its Worksheet property to get 
            // a reference to the cell whose address matches the address you've supplied:
            var wsPart = (WorksheetPart)(wbPart.GetPartById(theSheet.Id));
            //var styles = wbPart.WorkbookStylesPart;
            //var cellFormats = styles.Stylesheet.CellFormats;


            var theCell = wsPart.Worksheet.Descendants<Cell>().FirstOrDefault(c => c.CellReference == addressName);

            // If the cell doesn't exist, return an empty string:
            if (theCell != null)
            {
                value = theCell.InnerText;
                if (theCell.CellFormula != null)
                    value = theCell.CellValue.InnerText;

                //int sIndex = 0;
                //if (theCell.StyleIndex != null)
                //    sIndex = Convert.ToInt32(theCell.StyleIndex.Value);

                //var cellFormat = cellFormats.Descendants<CellFormat>().ElementAt<CellFormat>(sIndex);
                //determine the data type from the cellFormat





                // If the cell represents an integer number, you're done. 
                // For dates, this code returns the serialized value that 
                // represents the date. The code handles strings and booleans
                // individually. For shared strings, the code looks up the corresponding
                // value in the shared string table. For booleans, the code converts 
                // the value into t he words TRUE or FALSE.
                if (theCell.DataType != null)
                {
                    switch (theCell.DataType.Value)
                    {
                        case CellValues.SharedString:
                            // For shared strings, look up the value in the shared strings table.
                            var stringTable = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                            // If the shared string table is missing, something's wrong.
                            // Just return the index that you found in the cell.
                            // Otherwise, look up the correct text in the table.
                            if (stringTable != null)
                            {
                                value = stringTable.SharedStringTable.ElementAt(int.Parse(value)).InnerText;
                            }
                            break;

                        case CellValues.Boolean:
                            switch (value)
                            {
                                case "0":
                                    value = "FALSE";
                                    break;
                                default:
                                    value = "TRUE";
                                    break;
                            }
                            break;
                    }
                }
            }

            return value;
        }

       
    }

    public class ChecksFileInfo
    {
        public string FileName { get; set; }
        public string FullName { get; set; }
        public string SubjectId { get; set; }
    }

    public class SiteInfoShort
    {
        public int Id { get; set; }
        public string  SiteCode { get; set; }
    }
}