using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using hpMvc.Infrastructure.Logging;
using hpMvc.Models;


namespace hpMvc.DataBase
{
    public static class CalorieCalc
    {
        public static NLogger Nlogger;
        static CalorieCalc()
        {
            Nlogger = new NLogger();
        }
        
        public static int GetStudyDay(int studyId, DateTime calcDate)
        {
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = System.Data.CommandType.StoredProcedure,
                                      CommandText = "GetRadomizationInfoForStudyId"
                                  };
                    var param = new SqlParameter("@id", studyId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    var dateRandomized = new DateTime();

                    while (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("DateRandomized");
                        dateRandomized = rdr.GetDateTime(pos);
                    }
                    rdr.Close();

                    TimeSpan ts = calcDate.Date - dateRandomized.Date;
                    return ts.Days;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return -1;
                }
            }
        }

        public static double GetCalcWeight(int studyId)
        {
            double weight = 0;
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = System.Data.CommandType.StoredProcedure,
                                      CommandText = "GetCalcWeight"
                                  };
                    var param = new SqlParameter("@studyID", studyId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    
                    while (rdr.Read())
                    {                        
                        weight = rdr.GetDouble(0);                                               
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                }
            }
            return weight;
        }

        public static int DeleteCurrentEntries(int calStudyId)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            SqlDataReader rdr = null;
            using (var conn = new SqlConnection(strConn))
            {
                
                conn.Open();
                using (SqlTransaction trn = conn.BeginTransaction())
                {
                    try
                    {

                        var cmd = new SqlCommand("", conn)
                                  {
                                      Transaction = trn,
                                      CommandType = System.Data.CommandType.StoredProcedure,
                                      CommandText = "DeleteCalAdditive"
                                  };
                        var param = new SqlParameter("@calStudyID", calStudyId);
                        cmd.Parameters.Add(param);

                        cmd.ExecuteNonQuery();

                        cmd = new SqlCommand("", conn)
                              {
                                  Transaction = trn,
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = "DeleteCalOther"
                              };
                        param = new SqlParameter("@calStudyID", calStudyId);
                        cmd.Parameters.Add(param);

                        cmd.ExecuteNonQuery();

                        cmd = new SqlCommand("", conn)
                              {
                                  Transaction = trn,
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = "DeleteCalParenterals"
                              };
                        param = new SqlParameter("@calStudyID", calStudyId);
                        cmd.Parameters.Add(param);

                        cmd.ExecuteNonQuery();

                        cmd = new SqlCommand("", conn)
                              {
                                  Transaction = trn,
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = "DeleteCalEnterals"
                              };
                        param = new SqlParameter("@calStudyID", calStudyId);
                        cmd.Parameters.Add(param);

                        cmd.ExecuteNonQuery();

                        cmd = new SqlCommand("SELECT ID FROM CalInfusionsDex WHERE CalStudyID=" + calStudyId, conn)
                              {
                                  Transaction
                                      =
                                      trn
                              };
                        param = new SqlParameter("@calStudyID", calStudyId);
                        cmd.Parameters.Add(param);

                        rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            int dexId = rdr.GetInt32(0);

                            var cmd2 = new SqlCommand("", conn)
                                       {
                                           Transaction = trn,
                                           CommandType = System.Data.CommandType.StoredProcedure,
                                           CommandText = "DeleteCalInfusionsVol"
                                       };
                            param = new SqlParameter("@dexID", dexId);
                            cmd2.Parameters.Add(param);

                            cmd2.ExecuteNonQuery();

                        }
                        rdr.Close();

                        cmd = new SqlCommand("", conn)
                              {
                                  Transaction = trn,
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = "DeleteCalInfusionsDex"
                              };
                        param = new SqlParameter("@calStudyID", calStudyId);
                        cmd.Parameters.Add(param);

                        cmd.ExecuteNonQuery();

                        trn.Commit();
                    }
                    catch (Exception ex)
                    {
                        trn.Rollback();
                        Nlogger.LogError(ex);
                    }
                    finally
                    {
                        if(rdr != null)
                            rdr.Close();
                    }
                }
            }
            return 1;
        }

        public static List<CalStudyInfo> GetCalStudyInfoAll()
        {
            var csil = new List<CalStudyInfo>();
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = System.Data.CommandType.StoredProcedure,
                        CommandText = "GetCallStudyInfoAll"
                    };
                   
                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var csi = new CalStudyInfo();
                        var pos = rdr.GetOrdinal("ID");
                        csi.Id = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("Hours");
                        csi.Hours = rdr.IsDBNull(pos) ? 0 : rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("StudyID");
                        csi.StudyId = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("CalcWeight");
                        csi.Weight = rdr.GetDouble(pos);

                        pos = rdr.GetOrdinal("CalcDate");
                        csi.CalcDate = rdr.GetDateTime(pos).ToString("MM/dd/yyyy");

                        csil.Add(csi);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return null;
                }
            }
            return csil;

        }
        public static CalStudyInfo GetCalStudyInfo(string id)
        {
            var csi = new CalStudyInfo();
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = System.Data.CommandType.StoredProcedure,
                                      CommandText = "GetCalStudyInfo"
                                  };
                    var param = new SqlParameter("@id", id);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {                        
                        var pos = rdr.GetOrdinal("ID");
                        csi.Id = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("Hours");
                        csi.Hours = rdr.IsDBNull(pos) ? 0 : rdr.GetInt32(pos);
                        
                        pos = rdr.GetOrdinal("StudyID");
                        csi.StudyId = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("CalcWeight");
                        csi.Weight = rdr.GetDouble(pos);

                        pos = rdr.GetOrdinal("CalcDate");
                        csi.CalcDate = rdr.GetDateTime(pos).ToString("MM/dd/yyyy");

                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                }
            }
            return csi;
                
        }

        public static CalOtherNutrition GetCalOtherNutrition(int calStudyId)
        {
            SqlDataReader rdr = null;
            var con = new CalOtherNutrition();
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = "GetCalOther"
                              };
                    var param = new SqlParameter("@calStudyID", calStudyId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        con.CalStudyId = calStudyId;

                        int pos = rdr.GetOrdinal("ID");
                        con.Id = rdr.GetInt32(pos);

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
                    Nlogger.LogError(ex);
                }
                finally
                {
                    if (rdr != null)
                        rdr.Close();
                }
            }
            return con;
        }

        public static List<CalAdditive> GetCalAdditivesData(int calStudyId)
        {
            var cal = new List<CalAdditive>();
            SqlDataReader rdr = null;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = "GetCalAdditives"
                              };
                    var param = new SqlParameter("@calStudyID", calStudyId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var ca = new CalAdditive {CalStudyID = calStudyId};

                        int pos = rdr.GetOrdinal("AdditiveID");
                        ca.AdditiveID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("Volume");
                        ca.Volume = rdr.GetDouble(pos);

                        cal.Add(ca);
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
            return cal;
        }

        public static List<CalEnteral> GetCalEnteralsData(int calStudyId)
        {
            var cel = new List<CalEnteral>();
            SqlDataReader rdr = null;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = "GetCalEnterals"
                              };
                    var param = new SqlParameter("@calStudyID", calStudyId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var ce = new CalEnteral {CalStudyID = calStudyId};

                        int pos = rdr.GetOrdinal("FormulaID");
                        ce.FormulaID = rdr.GetInt32(pos);
                                                
                        pos = rdr.GetOrdinal("Volume");
                        ce.Volume = rdr.GetDouble(pos);

                        cel.Add(ce);
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
            return cel;
        }
        
        public static List<CalParenteral> GetCalParenteralsData(int calStudyId)
        {
            var cpl = new List<CalParenteral>();
            SqlDataReader rdr = null;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = "GetCalParenterals"
                              };
                    var param = new SqlParameter("@calStudyID", calStudyId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var cp = new CalParenteral {CalStudyID = calStudyId};

                        int pos = rdr.GetOrdinal("ID");
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
                    Nlogger.LogError(ex);
                }
                finally
                {
                    if (rdr != null)
                        rdr.Close();
                }
            }
            return cpl;
        }

        public static List<CalInfusionDex> GetCalInfusionsDexData(int calStudyId)
        {
            var cil = new List<CalInfusionDex>();
            SqlDataReader rdr = null;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = "GetCalInfusionsDex"
                              };
                    var param = new SqlParameter("@calStudyID", calStudyId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var ci = new CalInfusionDex {CalStudyID = calStudyId};

                        int pos = rdr.GetOrdinal("ID");
                        ci.ID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("Dexval");
                        ci.DexVal = rdr.GetDouble(pos);
                                           
                        cil.Add(ci);
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
            return cil;
        }

        public static List<CalInfusionVol> GetCalInfusionsVolData(int dexId)
        {
            var cil = new List<CalInfusionVol>();
            SqlDataReader rdr = null;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = "GetCalInfusionsVol"
                              };
                    var param = new SqlParameter("@dexID", dexId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var ci = new CalInfusionVol {DexID = dexId};

                        int pos = rdr.GetOrdinal("ID");
                        ci.ID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("Volume");
                        ci.Volume = rdr.GetInt32(pos);

                        

                        cil.Add(ci);
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
            return cil;
        }

        public static int GetCalStudyId(string studyId, string calcDate)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("Test error");
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = ("GetCalStudyID")
                              };
                    var param = new SqlParameter("@studyID", studyId);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@calcDate", calcDate);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var id = (Int32)cmd.ExecuteScalar();
                    return id;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return -1;
                }
            }
        }

        public static List<IDandName> GetCalStudySelectList(int siteId)
        {
            var idnl = new List<IDandName>();
            SqlDataReader rdr = null;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = "GetCalStudySelectList"
                              };
                    var param = new SqlParameter("@siteID", siteId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var idn = new IDandName();
                        int pos = rdr.GetOrdinal("ID");
                        idn.ID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("StudyID");
                        idn.Name = rdr.GetString(pos);
                         
                        idnl.Add(idn);
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
            return idnl;
            
        }

        public static List<IDandName> GetCalCalcDates(int studyId)
        {
            var idnl = new List<IDandName>();
            SqlDataReader rdr = null;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = "GetCalCalcDates"
                              };
                    var param = new SqlParameter("@studyID", studyId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var idn = new IDandName();

                        int pos = rdr.GetOrdinal("ID");
                        idn.ID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("CalcDate");
                        idn.Name = rdr.GetDateTime(pos).ToString("MM/dd/yyyy");

                        idnl.Add(idn);
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
            return idnl;

        }

        public static DTO AddCalAdditive(CalAdditive ca)
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
                                  CommandText = ("[AddCalAdditive]")
                              };

                    var param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID")
                                {
                                    Direction =
                                        System.Data
                                        .ParameterDirection
                                        .Output
                                };
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
                    Nlogger.LogError(ex);
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
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = ("[AddCalEnteral]")
                              };

                    var param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID")
                                {
                                    Direction =
                                        System.Data
                                        .ParameterDirection
                                        .Output
                                };
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
                    Nlogger.LogError(ex);
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
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = ("[AddCalParenteral]")
                              };

                    var param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID")
                                {
                                    Direction =
                                        System.Data
                                        .ParameterDirection
                                        .Output
                                };
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
                    Nlogger.LogError(ex);
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
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = ("[AddCalInfusionDex]")
                              };

                    var param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID")
                                {
                                    Direction =
                                        System.Data
                                        .ParameterDirection
                                        .Output
                                };
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
                    Nlogger.LogError(ex);
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
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = ("[AddCalInfusionVol]")
                              };

                    var param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID")
                                {
                                    Direction =
                                        System.Data
                                        .ParameterDirection
                                        .Output
                                };
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
                    Nlogger.LogError(ex);
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
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = ("[AddCalOther]")
                              };

                    var param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID")
                                {
                                    Direction =
                                        System.Data
                                        .ParameterDirection
                                        .Output
                                };
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@calStudyID", con.CalStudyId);
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

                    con.Id = (int)cmd.Parameters["@Identity"].Value;
                    dto.ReturnValue = 1;
                    dto.Bag = con;

                    return dto;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
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
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = ("AddCalStudyInfo")
                              };

                    var param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID")
                                {
                                    Direction =
                                        System.Data
                                        .ParameterDirection
                                        .Output
                                };
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@studyID", csi.StudyId);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@weight", csi.Weight);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@gir", csi.Gir);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@calcDate", csi.CalcDate);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@totalCals", csi.TotalCals);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@hours", csi.Hours);
                    cmd.Parameters.Add(param);

                    conn.Open();

                    cmd.ExecuteNonQuery();

                    csi.Id = (int)cmd.Parameters["@Identity"].Value;
                    dto.ReturnValue = 1;
                    dto.Bag = csi;

                    return dto;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
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
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = System.Data.CommandType.StoredProcedure,
                                      CommandText = ("UpdateCalStudyInfo")
                                  };

                    var param = new SqlParameter("@id", csi.Id);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@weight", csi.Weight);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@gir", csi.Gir);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@calcDate", csi.CalcDate);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@totalCals", csi.TotalCals);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@hours", csi.Hours);
                    cmd.Parameters.Add(param);


                    conn.Open();

                    cmd.ExecuteNonQuery();

                    dto.ReturnValue = 1;
                    dto.Bag = csi;

                    return dto;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    dto.Message = ex.Message;
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }
        
        public static int IsCalStudyInfoDuplicate(int studyId, string calcDate)
        {
             String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("Test error");
                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = System.Data.CommandType.StoredProcedure,
                                      CommandText = ("IsCalStudyInfoDuplicate")
                                  };
                    var param = new SqlParameter("@studyID", studyId);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@calcDate", calcDate);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var count = (Int32)cmd.ExecuteScalar();
                    if (count == 1)
                        return 1;
                    return 0;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return -1;
                }
            }
        }

        public static int IsFormulaNameDuplicate(string name)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("Test error");
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = ("IsFormulaNameDuplicate")
                              };
                    var param = new SqlParameter("@name", name);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var count = (Int32)cmd.ExecuteScalar();
                    if (count == 1)
                        return 1;
                    return 0;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return -1;
                }
            }
        }

        public static DTO AddFormula(EnteralFormula ef)
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
                                  CommandText = ("AddFormula")
                              };

                    var param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID")
                                {
                                    Direction =
                                        System.Data
                                        .ParameterDirection
                                        .Output
                                };
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
                    Nlogger.LogError(ex);
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
                    Nlogger.LogError(ex);
                    dto.Message = ex.Message;
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }
        
        public static List<DextroseConcentration> GetDextroseConcentrations()
        {
            var dcs = new List<DextroseConcentration>();
            SqlDataReader rdr = null;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = ("GetDextroseConcentrations")
                              };

                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var dc = new DextroseConcentration();
                        int pos = rdr.GetOrdinal("ID");
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
                    Nlogger.LogError(ex);
                }
                finally
                {
                    if (rdr != null)
                        rdr.Close();
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
                    var formula = new EnteralFormula();

                    while (rdr.Read())
                    {

                        int pos = rdr.GetOrdinal("ID");
                        formula.ID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("Name");
                        formula.Name = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("Kcal/mL");
                        formula.Kcal_ml = !rdr.IsDBNull(pos) ? rdr.GetDouble(pos) : 0;

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
                    Nlogger.LogError(ex);
                    return null;
                }
            }

        }

        public static List<EnteralFormula> GetFormulaList()
        {
            var efl = new List<EnteralFormula>();
            SqlDataReader rdr = null;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = ("GetFormulaList")
                              };

                    conn.Open();
                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var ef = new EnteralFormula();
                        int pos = rdr.GetOrdinal("ID");
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
                    Nlogger.LogError(ex);
                }
                finally
                {
                    if (rdr != null)
                        rdr.Close();
                }
            }
            return efl;
        }

        public static Additive GetAdditive(string id)
        {
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = System.Data.CommandType.StoredProcedure,
                        CommandText = "GetAdditive"
                    };
                    var param = new SqlParameter("@id", id);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    var additive = new Additive();

                    while (rdr.Read())
                    {

                        var pos = rdr.GetOrdinal("ID");
                        additive.ID = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("Name");
                        additive.Name = rdr.GetString(pos);

                        pos = rdr.GetOrdinal("Kcal/unit");
                        additive.Kcal_unit = !rdr.IsDBNull(pos) ? rdr.GetDouble(pos) : 0;

                        pos = rdr.GetOrdinal("CHO % of kcal");
                        additive.ChoKcal = rdr.GetDouble(pos);

                        pos = rdr.GetOrdinal("Protein % of kcal");
                        additive.ProteinKcal = rdr.GetDouble(pos);

                        pos = rdr.GetOrdinal("Lipid % of kcal");
                        additive.LipidKcal = rdr.GetDouble(pos);

                        pos = rdr.GetOrdinal("Unit");
                        additive.Unit = rdr.GetInt32(pos);

                        pos = rdr.GetOrdinal("UnitName");
                        additive.UnitName = rdr.GetString(pos);

                    }
                    rdr.Close();

                    return additive;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return null;
                }
            }

        }

        public static List<Additive> GetAdditiveList()
        {
            var addl = new List<Additive>();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = System.Data.CommandType.StoredProcedure,
                                      CommandText = ("GetAdditiveList")
                                  };

                    conn.Open();
                    var rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var add = new Additive();
                        var pos = rdr.GetOrdinal("ID");
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
                    Nlogger.LogError(ex);
                }
            }
            return addl;
        }

        public static DTO UpdateAdditive(Additive additive)
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
                        CommandText = ("UpdateAdditive")
                    };

                    var param = new SqlParameter("@name", additive.Name);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@kcalunit", additive.Kcal_unit);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@portein", additive.ProteinKcal);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@lipid", additive.LipidKcal);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@cho", additive.ChoKcal);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@id", additive.ID);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@unit", additive.Unit);
                    cmd.Parameters.Add(param);

                    conn.Open();

                    cmd.ExecuteNonQuery();

                    dto.ReturnValue = 1;
                    return dto;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    dto.Message = ex.Message;
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }

        public static DTO AddAdditive(Additive add)
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
                                  CommandText = ("AddAdditive")
                              };

                    var param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID")
                                {
                                    Direction =
                                        System.Data
                                        .ParameterDirection
                                        .Output
                                };
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
                    Nlogger.LogError(ex);
                    dto.Message = ex.Message;
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }

        public static int IsAdditiveNameDuplicate(string name)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("Test error");
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = ("IsAdditiveNameDuplicate")
                              };
                    var param = new SqlParameter("@name", name);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var count = (Int32)cmd.ExecuteScalar();
                    if (count == 1)
                        return 1;
                    return 0;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return -1;
                }
            }
        }

        public static void GetFormularData(CalEnteral ent)
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
                    var param = new SqlParameter("@id", ent.FormulaID.ToString());
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    
                    while (rdr.Read())
                    {
                        int pos = rdr.GetOrdinal("Kcal/mL");
                        ent.KcalMl = !rdr.IsDBNull(pos) ? rdr.GetDouble(pos) : 0;

                        pos = rdr.GetOrdinal("CHO % of kcal");
                        ent.ChoPercent = rdr.GetDouble(pos);

                        pos = rdr.GetOrdinal("Protein % of kcal");
                        ent.ProteinPercent = rdr.GetDouble(pos);

                        pos = rdr.GetOrdinal("Lipid % of kcal");
                        ent.LipidPercent = rdr.GetDouble(pos);

                    }
                    rdr.Close();

                    return;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return;
                }
            }
        }
    }
}