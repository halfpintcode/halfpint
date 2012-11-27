using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;
using hpMvc.Infrastructure.Logging;
using hpMvc.Models;
using System.Collections.Specialized;

namespace hpMvc.DataBase
{
    public static class ssUtils
    {
        public static NLogger nlogger;
        static ssUtils()
        {
            nlogger = new NLogger();
        }

        public static bool VerifyKey(string key, string fileName, string institID)
        {
            int ikey = 0;

            foreach (var c in fileName)
            {
                if (char.IsNumber(c))
                    ikey += int.Parse(c.ToString());
            }
            ikey *= ikey;

            int iInstitID = int.Parse(institID);
            ikey = ikey * iInstitID;

            int iFromClientKey = int.Parse(key.Substring(6));

            if (iFromClientKey == ikey)
                return true;

            return false;
        }

        public static bool AddSenorData(string studyID,NameValueCollection formParams )
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("AddSensorData");

                    SqlParameter param = new SqlParameter("@studyID", studyID);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@monitorDate", formParams["MonitorDate"]);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@monitorTime", formParams["MonitorTime"]);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@monitorID", formParams["MonitorID"]);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@transmitterID", formParams["TransmitterID"]);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@sensorLot", formParams["SensorLot"]);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@inserterFirstName", formParams["InserterFirstName"]);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@inserterLastName", formParams["InserterLastName"]);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@sensorLocation", formParams["SensorLocations"]);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@sensorReason", 1);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@dateEntered", DateTime.Now);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    
                }
                catch (Exception ex)
                {
                    //nlogger.LogError(ex);
                    return false;
                }
            }
            return true;
        }

        public static int SetRandomization(string studyID, ref SSInsertionData ssInsert, string user)
        {
            int site = DbUtils.GetSiteidIDForUser(user);
            int arm = 0;
            int randomizationID = 0;
            string sArm = "";

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            { 
                try
                {                    
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("GetNextRandomization");
                    SqlParameter param = new SqlParameter("@site", site);
                    cmd.Parameters.Add(param);
                        
                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                                        
                    int pos = 0;
                    while (rdr.Read())
                    {
                        pos = rdr.GetOrdinal("ID");
                        randomizationID = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("Arm");
                        sArm = rdr.GetString(pos);
                    }
                    rdr.Close();
                    arm = int.Parse(sArm.Substring(4,1));

                    cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("AddStudyIDToRandomization");
                    param = new SqlParameter("@id", randomizationID);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@studyID", studyID);
                    cmd.Parameters.Add(param);
                    cmd.ExecuteNonQuery();
                
                }
                    catch (Exception ex)
                    {
                        nlogger.LogError(ex);
                        throw new Exception("There was a problem with the randomization process");                        
                    }
            }

            string appSettingKey = "Arm" + arm.ToString();
            string armVal = System.Configuration.ConfigurationManager.AppSettings[appSettingKey];
            string[] avParts = armVal.Split(new string[] { ":" }, StringSplitOptions.None);
            ssInsert.TargetLow = avParts[0];
            ssInsert.TargetHigh = avParts[1];
            return arm;
        }

        public static List<string> GetSavedStudyIDs(string siteCode)
        {
            var list = new List<string>();
            
            var folderPath = ConfigurationManager.AppSettings["ChecksUploadPath"].ToString();
            var path = Path.Combine(folderPath, siteCode);

            if (Directory.Exists(path))
            {
                var di = new DirectoryInfo(path);

                FileInfo[] fis = di.GetFiles();

                foreach (var fi in fis.OrderBy(f => f.Name.Replace("copy", "")))
                    list.Add(fi.Name);
            }

            return list;
        }
        
        public static List<string> GetRanomizedStudyIDs(string physicalAppPath, string siteCode)
        {
            var list = new List<string>();
            string sitePath = physicalAppPath + "xcel\\" + siteCode;
            if (!Directory.Exists(sitePath))
                Directory.CreateDirectory(sitePath);
            var di = new DirectoryInfo(sitePath);
            FileInfo[] fis = di.GetFiles();

            foreach (var fi in fis)
                list.Add(fi.Name);
                       

            return list;
        }

        public static bool InitializeSS(string physicalAppPath, string studyID, SSInsertionData ssInsert, int useSensor)
        {
            string path = physicalAppPath + "sstemplate\\";
            string file = path + "Checks_tmpl.xlsm";
            string file2 = path + studyID + ".xlsm";
            int iGGonlyMode = 0;

            if (useSensor != 1)
                iGGonlyMode = 1;

            if (!File.Exists(file2))
                File.Copy(file, file2);
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(file2, true))
            {
                WorkbookPart wbPart = document.WorkbookPart;
                Sheet theSheet = wbPart.Workbook.Descendants<Sheet>().
                    Where(s => s.Name == "ParameterDefaults").FirstOrDefault();

                //WorksheetPart wsPart = (WorksheetPart)wbPart.GetPartById(theSheet.Id);

                UpdateValue(wbPart, "D2", studyID, 0, true, "ParameterDefaults");
                UpdateValue(wbPart, "E2", studyID, 0, true, "ParameterDefaults");
                UpdateValue(wbPart, "D3", ssInsert.BodyWeight, 0, false, "ParameterDefaults");
                UpdateValue(wbPart, "E3", ssInsert.BodyWeight, 0, false, "ParameterDefaults");
                UpdateValue(wbPart, "D4", ssInsert.InsulinConcentration, 0, false, "ParameterDefaults");
                UpdateValue(wbPart, "E4", ssInsert.InsulinConcentration, 0, false, "ParameterDefaults");
                UpdateValue(wbPart, "D5", ssInsert.TargetLow, 0, false, "ParameterDefaults");
                UpdateValue(wbPart, "E5", ssInsert.TargetLow, 0, false, "ParameterDefaults");
                UpdateValue(wbPart, "D6", ssInsert.TargetHigh, 0, false, "ParameterDefaults");
                UpdateValue(wbPart, "E6", ssInsert.TargetHigh, 0, false, "ParameterDefaults");
                UpdateValue(wbPart, "D47", iGGonlyMode.ToString(), 0, false, "ParameterDefaults");
                
                if (useSensor == 1)
                {
                    UpdateValue(wbPart, "A2", "1", 0, true, "SensorData");
                    UpdateValue(wbPart, "B2", ssInsert.MonitorDate, 0, true, "SensorData");
                    UpdateValue(wbPart, "C2", ssInsert.MonitorTime, 0, true, "SensorData");
                    UpdateValue(wbPart, "D2", ssInsert.MonitorID, 0, true, "SensorData");
                    UpdateValue(wbPart, "E2", ssInsert.TransmitterID, 0, true, "SensorData");
                    UpdateValue(wbPart, "F2", ssInsert.SensorLot, 0, true, "SensorData");
                    UpdateValue(wbPart, "G2", ssInsert.InserterFirstName, 0, true, "SensorData");
                    UpdateValue(wbPart, "H2", ssInsert.InserterLastName, 0, true, "SensorData");
                    UpdateValue(wbPart, "I2", GetSensorLocationString(ssInsert.SensorLocation), 0, true, "SensorData");
                    UpdateValue(wbPart, "J2", "Initial Insertion", 0, true, "SensorData");
                    UpdateValue(wbPart, "K2", DateTime.Now.ToString(), 0, true, "SensorData");
                }
                document.Close();
            }

            //get the path to the site folder
            string siteCode = studyID.Substring(0, 2);
            string sitePath = physicalAppPath + "xcel\\" + siteCode;
            //if it doesn't exist then create it
            if (!Directory.Exists(sitePath))
                Directory.CreateDirectory(sitePath);
                        
            string file3 = sitePath + "\\" + studyID + ".xlsm";
            if (File.Exists(file3))
                File.Delete(file3);

            File.Move(file2, file3);
            return true;
        }

        public static bool UpdateValue(WorkbookPart wbPart, string addressName, string value,
                                UInt32Value styleIndex, bool isString, string sheetName)
        {
            // Assume failure.
            bool updated = false;

            Sheet sheet = wbPart.Workbook.Descendants<Sheet>().Where(
                (s) => s.Name == sheetName).FirstOrDefault();

            if (sheet != null)
            {
                Worksheet ws = ((WorksheetPart)(wbPart.GetPartById(sheet.Id))).Worksheet;
                Cell cell = InsertCellInWorksheet(ws, addressName);

                if (isString)
                {
                    // Either retrieve the index of an existing string,
                    // or insert the string into the shared string table
                    // and get the index of the new item.
                    int stringIndex = InsertSharedStringItem(wbPart, value);

                    cell.CellValue = new CellValue(stringIndex.ToString());
                    cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
                }
                else
                {
                    cell.CellValue = new CellValue(value);
                    cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                }

                if (styleIndex > 0)
                    cell.StyleIndex = styleIndex;

                // Save the worksheet.
                ws.Save();
                updated = true;
            }

            return updated;
        }

        // Given the main workbook part, and a text value, insert the text into 
        // the shared string table. Create the table if necessary. If the value 
        // already exists, return its index. If it doesn't exist, insert it and 
        // return its new index.
        private static int InsertSharedStringItem(WorkbookPart wbPart, string value)
        {
            int index = 0;
            bool found = false;
            var stringTablePart = wbPart
                .GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

            // If the shared string table is missing, something's wrong.
            // Just return the index that you found in the cell.
            // Otherwise, look up the correct text in the table.
            if (stringTablePart == null)
            {
                // Create it.
                stringTablePart = wbPart.AddNewPart<SharedStringTablePart>();
            }

            var stringTable = stringTablePart.SharedStringTable;
            if (stringTable == null)
            {
                stringTable = new SharedStringTable();
            }

            // Iterate through all the items in the SharedStringTable. 
            // If the text already exists, return its index.
            foreach (SharedStringItem item in stringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == value)
                {
                    found = true;
                    break;
                }
                index += 1;
            }

            if (!found)
            {
                stringTable.AppendChild(new SharedStringItem(new Text(value)));
                stringTable.Save();
            }

            return index;
        }

        // Given a Worksheet and an address (like "AZ254"), either return a 
        // cell reference, or create the cell reference and return it.
        private static Cell InsertCellInWorksheet(Worksheet ws, string addressName)
        {
            SheetData sheetData = ws.GetFirstChild<SheetData>();
            Cell cell = null;

            UInt32 rowNumber = GetRowIndex(addressName);
            Row row = GetRow(sheetData, rowNumber);

            // If the cell you need already exists, return it.
            // If there is not a cell with the specified column name, insert one.  
            Cell refCell = row.Elements<Cell>().
                Where(c => c.CellReference.Value == addressName).FirstOrDefault();
            if (refCell != null)
            {
                cell = refCell;
            }
            else
            {
                cell = CreateCell(row, addressName);
            }
            return cell;
        }

        // Add a cell with the specified address to a row.
        private static Cell CreateCell(Row row, String address)
        {
            Cell cellResult;
            Cell refCell = null;

            // Cells must be in sequential order according to CellReference. 
            // Determine where to insert the new cell.
            foreach (Cell cell in row.Elements<Cell>())
            {
                if (string.Compare(cell.CellReference.Value, address, true) > 0)
                {
                    refCell = cell;
                    break;
                }
            }

            cellResult = new Cell();
            cellResult.CellReference = address;

            row.InsertBefore(cellResult, refCell);
            return cellResult;
        }

        private static Row GetRow(SheetData wsData, UInt32 rowIndex)
        {
            var row = wsData.Elements<Row>().
            Where(r => r.RowIndex.Value == rowIndex).FirstOrDefault();
            if (row == null)
            {
                row = new Row();
                row.RowIndex = rowIndex;
                wsData.Append(row);
            }
            return row;
        }

        // Given an Excel address such as E5 or AB128, GetRowIndex
        // parses the address and returns the row index.
        private static UInt32 GetRowIndex(string address)
        {
            string rowPart;
            UInt32 l;
            UInt32 result = 0;

            for (int i = 0; i < address.Length; i++)
            {
                if (UInt32.TryParse(address.Substring(i, 1), out l))
                {
                    rowPart = address.Substring(i, address.Length - i);
                    if (UInt32.TryParse(rowPart, out l))
                    {
                        result = l;
                        break;
                    }
                }
            }
            return result;
        }


        private static string GetSensorLocationString (string sLocationInt)
        {
            switch (sLocationInt)
            {
                case ("1"):
                    return "Thigh";
                case ("2"):
                    return "Abdomen";
                case ("3"):
                    return "Upper Extremity";
            }
            return "";
        }
    }

    public class SSInsertionData
    {
        public string BodyWeight { get; set; }
        public string InsulinConcentration { get; set; }
        public string TargetLow { get; set; }
        public string TargetHigh { get; set; }
        public string MonitorDate { get; set; }
        public string MonitorTime { get; set; }
        public string MonitorID { get; set; }
        public string TransmitterID { get; set; }
        public string SensorLot { get; set; }
        public string InserterFirstName { get; set; }
        public string InserterLastName { get; set; }
        public string SensorLocation { get; set; }
        public string SensorReason { get; set; }
        public string DateCreated { get; set; }
        
    }
}