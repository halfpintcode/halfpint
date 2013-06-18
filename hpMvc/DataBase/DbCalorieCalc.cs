using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using hpMvc.Infrastructure.Logging;
using System.Web.Security;
using hpMvc.Models;


namespace hpMvc.DataBase
{
    public static class CalorieCalc
    {
        public static NLogger nlogger;
        static CalorieCalc()
        {
            nlogger = new NLogger();
        }
        
        public static double GetCalcWeight(int studyID)
        {
            double weight = 0;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "GetCalcWeight";
                    SqlParameter param = new SqlParameter("@studyID", studyID);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;
                    while (rdr.Read())
                    {                        
                        weight = rdr.GetDouble(pos);                                               
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }
            return weight;
        }

        public static int DeleteCurrentEntries(int calStudyID)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                
                conn.Open();
                using (SqlTransaction trn = conn.BeginTransaction())
                {
                    try
                    {                       
                        
                        SqlCommand cmd = new SqlCommand("", conn);
                        cmd.Transaction = trn;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandText = "DeleteCalAdditive";
                        SqlParameter param = new SqlParameter("@calStudyID", calStudyID);
                        cmd.Parameters.Add(param);
                        
                        cmd.ExecuteNonQuery();

                        cmd = new SqlCommand("", conn);
                        cmd.Transaction = trn;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandText = "DeleteCalOther";
                        param = new SqlParameter("@calStudyID", calStudyID);
                        cmd.Parameters.Add(param);

                        cmd.ExecuteNonQuery();

                        cmd = new SqlCommand("", conn);
                        cmd.Transaction = trn;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandText = "DeleteCalParenterals";
                        param = new SqlParameter("@calStudyID", calStudyID);
                        cmd.Parameters.Add(param);

                        cmd.ExecuteNonQuery();

                        cmd = new SqlCommand("", conn);
                        cmd.Transaction = trn;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandText = "DeleteCalEnterals";
                        param = new SqlParameter("@calStudyID", calStudyID);
                        cmd.Parameters.Add(param);

                        cmd.ExecuteNonQuery();
                                                
                        cmd = new SqlCommand("SELECT ID FROM CalInfusionsDex WHERE CalStudyID=" + calStudyID, conn);
                        cmd.Transaction = trn;
                        param = new SqlParameter("@calStudyID", calStudyID);
                        cmd.Parameters.Add(param);

                        SqlDataReader rdr = cmd.ExecuteReader();
                        SqlCommand cmd2 = null;
                        while (rdr.Read()) 
                        {
                            int dexID = rdr.GetInt32(0);

                            cmd2 = new SqlCommand("", conn);
                            cmd2.Transaction = trn;                        
                            cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd2.CommandText = "DeleteCalInfusionsVol";
                            param = new SqlParameter("@dexID", dexID);
                            cmd2.Parameters.Add(param);

                            cmd2.ExecuteNonQuery();

                        }
                        rdr.Close();

                        cmd = new SqlCommand("", conn);
                        cmd.Transaction = trn;                        
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandText = "DeleteCalInfusionsDex";
                        param = new SqlParameter("@calStudyID", calStudyID);
                        cmd.Parameters.Add(param);

                        cmd.ExecuteNonQuery();

                        trn.Commit();
                    }
                    catch (Exception ex)
                    {
                        trn.Rollback();
                        nlogger.LogError(ex);
                    }
                }
            }
            return 1;
        }
        
        public static CalStudyInfo GetCalStudyInfo(string id)
        {
            CalStudyInfo csi = new CalStudyInfo();
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "GetCalStudyInfo";
                    SqlParameter param = new SqlParameter("@id", id);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;
                    while (rdr.Read())
                    {                        
                        pos = rdr.GetOrdinal("ID");
                        csi.ID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("StudyID");
                        csi.StudyID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("CalcWeight");
                        csi.Weight = rdr.GetDouble(pos);

                        pos = rdr.GetOrdinal("CalcDate");
                        csi.CalcDate = rdr.GetDateTime(pos).ToShortDateString();

                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }
            return csi;
                
        }

        public static CalOtherNutrition GetCalOtherNutrition(int calStudyID)
        {
            CalOtherNutrition con = new CalOtherNutrition();
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "GetCalOther";
                    SqlParameter param = new SqlParameter("@calStudyID", calStudyID);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;
                    while (rdr.Read())
                    {
                        con.CalStudyID = calStudyID;

                        pos = rdr.GetOrdinal("ID");
                        con.ID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("BreastFeeding");
                        con.BreastFeeding = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("SolidFoods");
                        con.SolidFoods = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("Drinks");
                        con.Drinks = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("Other");
                        con.Other = rdr.GetBoolean(pos);

                        pos = rdr.GetOrdinal("OtherText");
                        if(! rdr.IsDBNull(pos))
                            con.OtherText = rdr.GetString(pos); 

                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }
            return con;

        }

        public static List<CalAdditive> GetCalAdditivesData(int calStudyID)
        {
            List<CalAdditive> cal = new List<CalAdditive>();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "GetCalAdditives";
                    SqlParameter param = new SqlParameter("@calStudyID", calStudyID);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;

                    while (rdr.Read())
                    {
                        var ca = new CalAdditive();
                        ca.CalStudyID = calStudyID;

                        pos = rdr.GetOrdinal("AdditiveID");
                        ca.AdditiveID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("Volume");
                        ca.Volume = rdr.GetInt32(pos);

                        cal.Add(ca);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }
            return cal;
        }

        public static List<CalEnteral> GetCalEnteralsData(int calStudyID)
        {
            List<CalEnteral> cel = new List<CalEnteral>();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "GetCalEnterals";
                    SqlParameter param = new SqlParameter("@calStudyID", calStudyID);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;

                    while (rdr.Read())
                    {
                        var ce = new CalEnteral();
                        ce.CalStudyID = calStudyID;

                        pos = rdr.GetOrdinal("FormulaID");
                        ce.FormulaID = rdr.GetInt32(pos);
                                                
                        pos = rdr.GetOrdinal("Volume");
                        ce.Volume = rdr.GetInt32(pos);

                        cel.Add(ce);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }
            return cel;
        }
        
        public static List<CalParenteral> GetCalParenteralsData(int calStudyID)
        {
            List<CalParenteral> cpl = new List<CalParenteral>();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "GetCalParenterals";
                    SqlParameter param = new SqlParameter("@calStudyID", calStudyID);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;

                    while (rdr.Read())
                    {
                        var cp = new CalParenteral();
                        cp.CalStudyID = calStudyID;

                        pos = rdr.GetOrdinal("ID");
                        cp.ID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("DexPercent");
                        cp.DexPercent = rdr.GetDouble(pos);

                        pos = rdr.GetOrdinal("AminoPercent");
                        cp.AminoPercent = rdr.GetDouble(pos);

                        pos = rdr.GetOrdinal("LipidPercent");
                        cp.LipidPercent = rdr.GetDouble(pos);

                        pos = rdr.GetOrdinal("Volume");
                        cp.Volume = rdr.GetDouble(pos);

                        cpl.Add(cp);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }
            return cpl;
        }

        public static List<CalInfusionDex> GetCalInfusionsDexData(int calStudyID)
        {
            List<CalInfusionDex> cil = new List<CalInfusionDex>();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "GetCalInfusionsDex";
                    SqlParameter param = new SqlParameter("@calStudyID", calStudyID);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;

                    while (rdr.Read())
                    {
                        var ci = new CalInfusionDex();
                        ci.CalStudyID = calStudyID;

                        pos = rdr.GetOrdinal("ID");
                        ci.ID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("Dexval");
                        ci.DexVal = rdr.GetDouble(pos);
                                           
                        cil.Add(ci);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }
            return cil;
        }

        public static List<CalInfusionVol> GetCalInfusionsVolData(int dexID)
        {
            List<CalInfusionVol> cil = new List<CalInfusionVol>();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "GetCalInfusionsVol";
                    SqlParameter param = new SqlParameter("@dexID", dexID);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;

                    while (rdr.Read())
                    {
                        var ci = new CalInfusionVol();
                        ci.DexID = dexID;

                        pos = rdr.GetOrdinal("ID");
                        ci.ID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("Volume");
                        ci.Volume = rdr.GetInt32(pos);

                        

                        cil.Add(ci);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }
            return cil;
        }

        public static int GetCalStudyID(string studyID, string calcDate)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("Test error");
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("GetCalStudyID");
                    SqlParameter param = new SqlParameter("@studyID", studyID);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@calcDate", calcDate);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    int id = (Int32)cmd.ExecuteScalar();
                    return id;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return -1;
                }
            }
        }

        public static List<IDandName> GetCalStudySelectList(int siteID)
        {
            List<IDandName> idnl = new List<IDandName>();
            
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "GetCalStudySelectList";
                    SqlParameter param = new SqlParameter("@siteID", siteID);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;

                    while (rdr.Read())
                    {
                        var idn = new IDandName();
                        pos = rdr.GetOrdinal("ID");
                        idn.ID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("StudyID");
                        idn.Name = rdr.GetString(pos);
                         
                        idnl.Add(idn);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }
            return idnl;
            
        }

        public static List<IDandName> GetCalCalcDates(int studyID)
        {
            List<IDandName> idnl = new List<IDandName>();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "GetCalCalcDates";
                    SqlParameter param = new SqlParameter("@studyID", studyID);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;

                    while (rdr.Read())
                    {
                        var idn = new IDandName();

                        pos = rdr.GetOrdinal("ID");
                        idn.ID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("CalcDate");
                        idn.Name = rdr.GetDateTime(pos).ToShortDateString();

                        idnl.Add(idn);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }
            return idnl;

        }

        public static DTO AddCalAdditive(CalAdditive ca)
        {
            var dto = new DTO();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("[AddCalAdditive]");

                    SqlParameter param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID");
                    param.Direction = System.Data.ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@calStudyID", ca.CalStudyID);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@additiveID", ca.AdditiveID);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@volume", ca.Volume);
                    cmd.Parameters.Add(param);
                    conn.Open();

                    cmd.ExecuteNonQuery();

                    ca.ID = (int)cmd.Parameters["@Identity"].Value;
                    dto.ReturnValue = 1;
                    dto.Bag = ca;

                    return dto;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    dto.Message = ex.Message;
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }

        public static DTO AddCalEnteral(CalEnteral ce)
        {
            var dto = new DTO();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("[AddCalEnteral]");

                    SqlParameter param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID");
                    param.Direction = System.Data.ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@calStudyID", ce.CalStudyID);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@formulaID", ce.FormulaID);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@volume", ce.Volume);
                    cmd.Parameters.Add(param);
                    conn.Open();

                    cmd.ExecuteNonQuery();

                    ce.ID = (int)cmd.Parameters["@Identity"].Value;
                    dto.ReturnValue = 1;
                    dto.Bag = ce;

                    return dto;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    dto.Message = ex.Message;
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }

        public static DTO AddCalParenteral(CalParenteral pi)
        {
            var dto = new DTO();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("[AddCalParenteral]");

                    SqlParameter param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID");
                    param.Direction = System.Data.ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@calStudyID", pi.CalStudyID);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@dexPercent", pi.DexPercent);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@aminoPercent", pi.AminoPercent);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@lipidPercent", pi.LipidPercent);
                    cmd.Parameters.Add(param);                   
                    param = new SqlParameter("@volume", pi.Volume);
                    cmd.Parameters.Add(param);
                    conn.Open();

                    cmd.ExecuteNonQuery();

                    pi.ID = (int)cmd.Parameters["@Identity"].Value;
                    dto.ReturnValue = 1;
                    dto.Bag = pi;

                    return dto;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    dto.Message = ex.Message;
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }

        public static DTO AddCalInfusionDex(CalInfusionDex cid)
        {
            var dto = new DTO();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("[AddCalInfusionDex]");

                    SqlParameter param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID");
                    param.Direction = System.Data.ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@calStudyID", cid.CalStudyID);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@dexVal", cid.DexVal);
                    cmd.Parameters.Add(param);
                    
                    conn.Open();

                    cmd.ExecuteNonQuery();

                    cid.ID = (int)cmd.Parameters["@Identity"].Value;
                    dto.ReturnValue = 1;
                    dto.Bag = cid;

                    return dto;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    dto.Message = ex.Message;
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }

        public static DTO AddCalInfusionVol(CalInfusionVol civ)
        {
            var dto = new DTO();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("[AddCalInfusionVol]");

                    SqlParameter param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID");
                    param.Direction = System.Data.ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@dexID", civ.DexID);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@volume", civ.Volume);
                    cmd.Parameters.Add(param);

                    conn.Open();

                    cmd.ExecuteNonQuery();

                    civ.ID = (int)cmd.Parameters["@Identity"].Value;
                    dto.ReturnValue = 1;
                    dto.Bag = civ;

                    return dto;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    dto.Message = ex.Message;
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }

        public static DTO AddCalOtherNutrition(CalOtherNutrition con)
        {
            var dto = new DTO();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("[AddCalOther]");

                    SqlParameter param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID");
                    param.Direction = System.Data.ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@calStudyID", con.CalStudyID);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@breastFeeding", con.BreastFeeding);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@solidFoods", con.SolidFoods);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@drinks", con.Drinks);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@other", con.Other);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@otherText", con.OtherText);
                    cmd.Parameters.Add(param);                    
                    conn.Open();

                    cmd.ExecuteNonQuery();

                    con.ID = (int)cmd.Parameters["@Identity"].Value;
                    dto.ReturnValue = 1;
                    dto.Bag = con;

                    return dto;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    dto.Message = ex.Message;
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }

        public static DTO AddCalStudyInfo(CalStudyInfo csi)
        {
            var dto = new DTO();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("AddCalStudyInfo");

                    SqlParameter param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID");
                    param.Direction = System.Data.ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@studyID", csi.StudyID);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@weight", csi.Weight);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@calcDate", csi.CalcDate);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@totalCals", csi.TotalCals);
                    cmd.Parameters.Add(param);
                    
                    conn.Open();

                    cmd.ExecuteNonQuery();

                    csi.ID = (int)cmd.Parameters["@Identity"].Value;
                    dto.ReturnValue = 1;
                    dto.Bag = csi;

                    return dto;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    dto.Message = ex.Message;
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }

        public static DTO UpdateCalStudyInfo(CalStudyInfo csi)
        {
            var dto = new DTO();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("UpdateCalStudyInfo");

                    SqlParameter param = new SqlParameter("@studyID", csi.StudyID);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@calcDate", csi.CalcDate);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@totalCals", csi.TotalCals);
                    cmd.Parameters.Add(param);

                    conn.Open();

                    cmd.ExecuteNonQuery();

                    dto.ReturnValue = 1;
                    dto.Bag = csi;

                    return dto;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    dto.Message = ex.Message;
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }
        
        public static int IsCalStudyInfoDuplicate(int studyID, string calcDate)
        {
             String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("Test error");
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("IsCalStudyInfoDuplicate");
                    SqlParameter param = new SqlParameter("@studyID", studyID);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@calcDate", calcDate);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    int count = (Int32)cmd.ExecuteScalar();
                    if (count == 1)
                        return 1;
                    return 0;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return -1;
                }
            }
        }

        public static int IsFormulaNameDuplicate(string name)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("Test error");
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("IsFormulaNameDuplicate");
                    SqlParameter param = new SqlParameter("@name", name);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    int count = (Int32)cmd.ExecuteScalar();
                    if (count == 1)
                        return 1;
                    return 0;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return -1;
                }
            }
        }

        public static DTO AddFormula(EnteralFormula ef)
        {
            var dto = new DTO();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("AddFormula");
                    
                    SqlParameter param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID");
                    param.Direction = System.Data.ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@name", ef.Name);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@kcalml", ef.Kcal_ml);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@portein", ef.ProteinKcal);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@lipid", ef.LipidKcal);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@cho", ef.ChoKcal);
                    cmd.Parameters.Add(param);
                    
                    conn.Open();

                    cmd.ExecuteNonQuery();

                    ef.ID = (int)cmd.Parameters["@Identity"].Value;
                    dto.ReturnValue = 1;
                    dto.Bag = ef;

                    return dto;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    dto.Message = ex.Message;
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }

        public static DTO UpdateFormula(EnteralFormula ef)
        {
            var dto = new DTO();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = System.Data.CommandType.StoredProcedure,
                                      CommandText = ("UpdateFormula")
                                  };

                    var param = new SqlParameter("@name", ef.Name);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@kcalml", ef.Kcal_ml);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@portein", ef.ProteinKcal);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@lipid", ef.LipidKcal);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@cho", ef.ChoKcal);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@id", ef.ID);
                    cmd.Parameters.Add(param);

                    conn.Open();

                    cmd.ExecuteNonQuery();
                    
                    dto.ReturnValue = 1;
                    dto.Bag = ef;

                    return dto;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    dto.Message = ex.Message;
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }

        public static List<DextroseConcentration> GetDextroseConcentrations()
        {
            List<DextroseConcentration> dcs = new List<DextroseConcentration>();
            
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("GetDextroseConcentrations");
                    
                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;
                    
                    while (rdr.Read())
                    {
                        var dc = new DextroseConcentration();
                        pos = rdr.GetOrdinal("ID");
                        dc.ID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("Concentration");                        
                        dc.Concentration = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("Kcal_ml");
                        dc.Kcal_ml = rdr.GetDouble(pos);

                        dcs.Add(dc);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }
            return dcs;
        }

        public static EnteralFormula GetFormula(string id)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = System.Data.CommandType.StoredProcedure,
                        CommandText = "GetFormula"
                    };
                    var param = new SqlParameter("@id", id);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    int pos = 0;
                    var formula = new EnteralFormula();

                    while (rdr.Read())
                    {

                        pos = rdr.GetOrdinal("ID");
                        formula.ID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("Name");
                        formula.Name = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("Kcal/mL");
                        if (!rdr.IsDBNull(pos))
                            formula.Kcal_ml = rdr.GetDouble(pos);
                        else
                            formula.Kcal_ml = 0;

                        pos = rdr.GetOrdinal("CHO % of kcal");
                        formula.ChoKcal = rdr.GetDouble(pos);

                        pos = rdr.GetOrdinal("Protein % of kcal");
                        formula.ProteinKcal = rdr.GetDouble(pos);

                        pos = rdr.GetOrdinal("Lipid % of kcal");
                        formula.LipidKcal = rdr.GetDouble(pos);

                    }
                    rdr.Close();

                    return formula;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return null;
                }
            }

        }

        public static List<EnteralFormula> GetFormulaList()
        {
            List<EnteralFormula> efl = new List<EnteralFormula>();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("GetFormulaList");

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;

                    while (rdr.Read())
                    {
                        var ef = new EnteralFormula();
                        pos = rdr.GetOrdinal("ID");
                        ef.ID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("Name");
                        ef.Name = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("Kcal/mL");
                        if(!rdr.IsDBNull(pos))
                            ef.Kcal_ml = rdr.GetDouble(pos);

                        pos = rdr.GetOrdinal("CHO % of kcal");
                        ef.ChoKcal = rdr.GetDouble(pos);

                        pos = rdr.GetOrdinal("Protein % of kcal");
                        ef.ProteinKcal = rdr.GetDouble(pos);

                        pos = rdr.GetOrdinal("Lipid % of kcal");
                        ef.LipidKcal = rdr.GetDouble(pos);
                        efl.Add(ef);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }
            return efl;
        }

        public static List<Additive> GetAdditiveList()
        {
            List<Additive> addl = new List<Additive>();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("GetAdditiveList");

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;

                    while (rdr.Read())
                    {
                        var add = new Additive();
                        pos = rdr.GetOrdinal("ID");
                        add.ID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("Name");
                        add.Name = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("UnitName");
                        add.UnitName = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("Kcal/unit");
                        if (!rdr.IsDBNull(pos))
                            add.Kcal_unit = rdr.GetDouble(pos);

                        pos = rdr.GetOrdinal("CHO % of kcal");
                        add.ChoKcal = rdr.GetDouble(pos);

                        pos = rdr.GetOrdinal("Protein % of kcal");
                        add.ProteinKcal = rdr.GetDouble(pos);

                        pos = rdr.GetOrdinal("Lipid % of kcal");
                        add.LipidKcal = rdr.GetDouble(pos);

                        pos = rdr.GetOrdinal("Unit");
                        add.Unit = rdr.GetInt32(pos);
                        addl.Add(add);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                }
            }
            return addl;
        }

        public static DTO AddAdditive(Additive add)
        {
            var dto = new DTO();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("AddAdditive");

                    SqlParameter param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID");
                    param.Direction = System.Data.ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@name", add.Name);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@kcalunit", add.Kcal_unit);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@portein", add.ProteinKcal);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@lipid", add.LipidKcal);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@cho", add.ChoKcal);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@unit", add.Unit);
                    cmd.Parameters.Add(param);

                    conn.Open();

                    cmd.ExecuteNonQuery();

                    add.ID = (int)cmd.Parameters["@Identity"].Value;
                    dto.ReturnValue = 1;
                    dto.Bag = add;

                    return dto;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    dto.Message = ex.Message;
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }

        public static int IsAdditiveNameDuplicate(string name)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("Test error");
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("IsAdditiveNameDuplicate");
                    SqlParameter param = new SqlParameter("@name", name);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    int count = (Int32)cmd.ExecuteScalar();
                    if (count == 1)
                        return 1;
                    return 0;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return -1;
                }
            }
        }
    }
}