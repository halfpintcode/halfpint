﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using hpMvc.Infrastructure.Logging;
using hpMvc.Models;
using System.Collections.Specialized;

namespace hpMvc.DataBase
{
    

    public static class DbPostTestsUtils
    {
        public static NLogger nlogger;
        static DbPostTestsUtils()
        {
            nlogger = new NLogger();
        }

        public static int SavePostTestsCompleted(PostTestsModel ptm)
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
                        cmd.CommandText = "DeleteStaffPostTestsCompleted";
                        SqlParameter param = new SqlParameter("@id", ptm.ID);
                        cmd.Parameters.Add(param);
                        
                        cmd.ExecuteNonQuery();
                        nlogger.LogInfo("DeletePostTestsCompleted - name: " + ptm.Name);

                        if (ptm.Checks)
                        {
                            cmd = new SqlCommand("", conn);
                            cmd.Transaction = trn;
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.CommandText = ("AddStaffPostTestCompleted");
                            param = new SqlParameter("@staffID", ptm.ID);
                            cmd.Parameters.Add(param);
                            param = new SqlParameter("@test", "Checks");
                            cmd.Parameters.Add(param);
                            param = new SqlParameter("@dateCompleted", ptm.ChecksCompleted);
                            cmd.Parameters.Add(param);

                            cmd.ExecuteNonQuery();
                            nlogger.LogInfo("AddTestCompleted - test: Checks, name: " + ptm.Name);
                        }

                        if (ptm.Overview)
                        {
                            cmd = new SqlCommand("", conn);
                            cmd.Transaction = trn;
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.CommandText = ("AddStaffPostTestCompleted");
                            param = new SqlParameter("@staffID", ptm.ID);
                            cmd.Parameters.Add(param);
                            param = new SqlParameter("@test", "Overview");
                            cmd.Parameters.Add(param);
                            param = new SqlParameter("@dateCompleted", ptm.OverviewCompleted);
                            cmd.Parameters.Add(param);

                            cmd.ExecuteNonQuery();
                            nlogger.LogInfo("AddTestCompleted - test: Overview, name: " + ptm.Name);
                        }

                        if (ptm.Medtronic)
                        {
                            cmd = new SqlCommand("", conn);
                            cmd.Transaction = trn;
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.CommandText = ("AddStaffPostTestCompleted");
                            param = new SqlParameter("@staffID", ptm.ID);
                            cmd.Parameters.Add(param);
                            param = new SqlParameter("@test", "Medtronic");
                            cmd.Parameters.Add(param);
                            param = new SqlParameter("@dateCompleted", ptm.MedtronicCompleted);
                            cmd.Parameters.Add(param);
                                                        
                            cmd.ExecuteNonQuery();
                            nlogger.LogInfo("AddTestCompleted - test: Medtronic, name: " + ptm.Name);
                        }
                                                
                        if (ptm.NovaStatStrip)
                        {
                            cmd = new SqlCommand("", conn);
                            cmd.Transaction = trn;
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.CommandText = ("AddStaffPostTestCompleted");
                            param = new SqlParameter("@staffID", ptm.ID);
                            cmd.Parameters.Add(param);
                            param = new SqlParameter("@test", "NovaStatStrip");
                            cmd.Parameters.Add(param);
                            param = new SqlParameter("@dateCompleted", ptm.NovaStatStripCompleted);
                            cmd.Parameters.Add(param);

                            cmd.ExecuteNonQuery();
                            nlogger.LogInfo("AddTestCompleted - test: NovaStatStrip, name: " + ptm.Name);
                        }

                        if (ptm.VampJr)
                        {
                            cmd = new SqlCommand("", conn);
                            cmd.Transaction = trn;
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.CommandText = ("AddStaffPostTestCompleted");
                            param = new SqlParameter("@staffID", ptm.ID);
                            cmd.Parameters.Add(param);
                            param = new SqlParameter("@test", "VampJr");
                            cmd.Parameters.Add(param);
                            param = new SqlParameter("@dateCompleted", ptm.VampJrCompleted);
                            cmd.Parameters.Add(param);

                            cmd.ExecuteNonQuery();
                            nlogger.LogInfo("AddTestCompleted - test: NovaStatStrip, name: " + ptm.Name);
                        }
                        trn.Commit();
                    }
                    catch (Exception ex)
                    {
                        trn.Rollback();
                        nlogger.LogError(ex);
                        return -1;
                    }
                }
            }
            return 1;
        }

        public static int DoesStaffNameExist(string lastName, string firstName, int site)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("DoesStaffNameExist");
                    SqlParameter param = new SqlParameter("@siteID", site);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@lastName", lastName);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@firstName", firstName);
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

        public static DTO DoesStaffEmployeeIDExist(string employeeID, int site)
        {
            var dto = new DTO();
            dto.IsSuccessful = false;
            dto.ReturnValue = 0;
            
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("DoesStaffEmployeeIDExist");
                    SqlParameter param = new SqlParameter("@siteID", site);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@employeeID", employeeID);
                    cmd.Parameters.Add(param);
                    //SqlParameter param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID");
                    param = new SqlParameter("@name", System.Data.SqlDbType.NVarChar, 50);
                    param.Direction = System.Data.ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    conn.Open();
                    int count = (Int32)cmd.ExecuteScalar();
                    dto.IsSuccessful = true;
                    if (count == 1)
                    {
                        string s = cmd.Parameters["@name"].Value.ToString();
                        dto.Message = s;
                        dto.ReturnValue = 1;
                    }
                    
                    return dto;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    dto.ReturnValue = -1;
                    return dto;
                }                
            }
        }

        public static DTO DoesStaffEmployeeIDExistOtherThan(int id, string employeeID, int site)
        {
            var dto = new DTO();
            dto.IsSuccessful = false;
            dto.ReturnValue = 0;

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {

                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("DoesStaffEmployeeIDExistOtherThan");
                    SqlParameter param = new SqlParameter("@id", id);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@siteID", site);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@employeeID", employeeID);
                    cmd.Parameters.Add(param);
                    //SqlParameter param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID");
                    param = new SqlParameter("@name", System.Data.SqlDbType.NVarChar, 50);
                    param.Direction = System.Data.ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    conn.Open();
                    int count = (Int32)cmd.ExecuteScalar();
                    dto.IsSuccessful = true;
                    if (count == 1)
                    {
                        string s = cmd.Parameters["@name"].Value.ToString();
                        dto.Message = s;
                        dto.ReturnValue = 1;
                    }

                    return dto;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }

        public static DTO DoesStaffEmailExist(string email)
        {
            var dto = new DTO();
            dto.IsSuccessful = false;
            dto.ReturnValue = 0;

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {                    
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("DoesStaffEmailExist");
                    SqlParameter param = new SqlParameter("@email", email);
                    cmd.Parameters.Add(param);                    
                    param = new SqlParameter("@name", System.Data.SqlDbType.NVarChar, 50);
                    param.Direction = System.Data.ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    conn.Open();
                    int count = (Int32)cmd.ExecuteScalar();
                    dto.IsSuccessful = true;
                    if (count == 1)
                    {
                        string s = cmd.Parameters["@name"].Value.ToString();
                        dto.Message = s;
                        dto.ReturnValue = 1;
                    }
                    
                    return dto;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }

        public static DTO DoesStaffEmailExistOtherThan(int id, string email)
        {
            var dto = new DTO();
            dto.IsSuccessful = false;
            dto.ReturnValue = 0;

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("DoesStaffEmailExistOtherThan");
                    SqlParameter param = new SqlParameter("@email", email);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@id", id);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@name", System.Data.SqlDbType.NVarChar, 50);
                    param.Direction = System.Data.ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    conn.Open();
                    int count = (Int32)cmd.ExecuteScalar();
                    dto.IsSuccessful = true;
                    if (count == 1)
                    {
                        string s = cmd.Parameters["@name"].Value.ToString();
                        dto.Message = s;
                        dto.ReturnValue = 1;
                    }

                    return dto;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }

        public static DTO DoesStaffUserNameExist(string userName)
        {
            var dto = new DTO();
            dto.IsSuccessful = false;
            dto.ReturnValue = 0;

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("DoesStaffUserNameExist");
                    SqlParameter param = new SqlParameter("@userName", userName);
                    cmd.Parameters.Add(param);
                    //SqlParameter param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID");
                    param = new SqlParameter("@name", System.Data.SqlDbType.NVarChar, 50);
                    param.Direction = System.Data.ParameterDirection.Output;
                    cmd.Parameters.Add(param);

                    conn.Open();
                    int count = (Int32)cmd.ExecuteScalar();
                    dto.IsSuccessful = true;
                    if (count == 1)
                    {
                        string s = cmd.Parameters["@name"].Value.ToString();
                        dto.Message = s;
                        dto.ReturnValue = 1;
                    }

                    return dto;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }

        public static DTO GetStaffIdByUserName(string userName)
        {
            var dto = new DTO {IsSuccessful = false, ReturnValue = 0};

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = System.Data.CommandType.StoredProcedure,
                                      CommandText = ("GetStaffIDByUserName")
                                  };
                    var param = new SqlParameter("@userName", userName);
                    cmd.Parameters.Add(param);
                    
                    conn.Open();
                    dto.ReturnValue = (Int32)cmd.ExecuteScalar();
                    dto.IsSuccessful = true;
                    
                    return dto;
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }

        public static int AddNurseStaff(string lastName, string firstName, string empID, int siteID, string email)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("AddNurseStaff");

                    SqlParameter param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID");
                    param.Direction = System.Data.ParameterDirection.Output;
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@lastName", lastName);
                    cmd.Parameters.Add(param); 
                    param = new SqlParameter("@firstName", firstName);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@siteID", siteID);
                    cmd.Parameters.Add(param);
                    if(empID == null)
                        empID = "";
                    param = new SqlParameter("@employeeID", empID);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@email", email);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    
                    return (int)cmd.Parameters["@Identity"].Value;
                    
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return -1;
                }
            }
        }

        public static string GetPostTestStaffEmail(string id)
        {
            var email = "";
            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = System.Data.CommandType.StoredProcedure,
                                      CommandText = ("GetPostTestStaffEmail")
                                  };

                    var param = new SqlParameter("@id", id);
                    cmd.Parameters.Add(param);
                    
                    conn.Open();
                    var rdr = cmd.ExecuteReader();

                    if (rdr.Read())
                    {
                        if (!rdr.IsDBNull(0))
                            email = rdr.GetString(0);
                    }
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);                    
                }
                return email;
            }
        }              

        public static int AddTestCompleted(int staffID, string test)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    //throw new Exception("Test error");
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("AddPostTestCompleted");
                    SqlParameter param = new SqlParameter("@staffID", staffID);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@test", test);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@dateCompleted", DateTime.Now);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    nlogger.LogInfo("AddTestCompleted - test: " + test + ", staffID: " + staffID);
                    return 1;
                }
                catch (Exception ex)
                {
                    nlogger.LogError("AddTestCompleted staffID: " + staffID + ", " + ex.Message);                    
                    return -1;
                }
            }            

        }

        public static DynamicDTO GetSiteEmployeeInfoForSite(string site)
        {
            var dto = new DynamicDTO {IsSuccessful = true};

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = System.Data.CommandType.StoredProcedure,
                                      CommandText = "GetSiteInfoForSite"
                                  };
                    var param = new SqlParameter("@id", site);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    var pos = 0;
                    rdr.Read();

                    dto.Stuff.EmpIDRequired = "false";
                    dto.Stuff.EmpIDRegex = "";
                    dto.Stuff.EmpIDMessage = "";
                    pos = rdr.GetOrdinal("EmpIDRequired");
                    bool bEmpIdRequired = rdr.GetBoolean(pos);
                    if (bEmpIdRequired)
                    {
                        dto.Stuff.EmpIDRequired = "true";

                        pos = rdr.GetOrdinal("EmpIDRegex");
                        string regEx = rdr.GetString(pos);
                        dto.Stuff.EmpIDRegex = regEx;

                        pos = rdr.GetOrdinal("EmpIDMessage");
                        string message = rdr.GetString(pos);
                        dto.Stuff.EmpIDMessage = message;
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return null;
                }
            }
            return dto;
        }

        public static DynamicDTO CheckIfEmployeeIdRequired(string user)
        {
            var dto = new DynamicDTO();
            dto.IsSuccessful = true;
            
             String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
             using (SqlConnection conn = new SqlConnection(strConn))
             {
                 try
                 {
                     SqlCommand cmd = new SqlCommand("", conn);
                     cmd.CommandType = System.Data.CommandType.StoredProcedure;
                     cmd.CommandText = "GetSiteInfoForUser";
                     SqlParameter param = new SqlParameter("@userName", user);
                     cmd.Parameters.Add(param);

                     conn.Open();
                     SqlDataReader rdr = cmd.ExecuteReader();
                     int pos = 0;
                     rdr.Read();
                                          
                     dto.Stuff.EmpIDRequired = "false";
                     dto.Stuff.EmpIDRegex = "";
                     dto.Stuff.EmpIDMessage = "";
                     pos = rdr.GetOrdinal("EmpIDRequired");
                     bool bEmpIDRequired =rdr.GetBoolean(pos);
                     if (bEmpIDRequired)
                     {
                         dto.Stuff.EmpIDRequired = "true";

                         pos = rdr.GetOrdinal("EmpIDRegex");
                         string regEx = rdr.GetString(pos);
                         dto.Stuff.EmpIDRegex = regEx;
                         
                         pos = rdr.GetOrdinal("EmpIDMessage");
                         string message = rdr.GetString(pos);
                         dto.Stuff.EmpIDMessage = message;
                     }
                     rdr.Close();
                 }
                 catch (Exception ex)
                 {
                     nlogger.LogError(ex);
                     return null;
                 }
             }
            return dto;
        }

        public static MessageListDTO VerifyPostTest(string testName, NameValueCollection formParams) 
        {
            var dto = new MessageListDTO();
            dto.IsSuccessful = true;
            List<bool> bResults = new List<bool>();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("GetPostTestAnswers");
                    SqlParameter param = new SqlParameter("@testName", testName);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;
                    int iquestion = 0;
                    int qType = 0;
                    string correctAnswer = "";
                    string answer;
                    string pararmString = "";
                    while (rdr.Read())
                    {
                        pos = rdr.GetOrdinal("Question");
                        iquestion = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("QType");
                        qType = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("Answer");
                        correctAnswer = rdr.GetString(pos);

                        bResults.Add(true);
                        switch (qType)
                        {
                            case 1: //multiple choice
                                pararmString = "question" + iquestion.ToString();
                                answer = formParams[pararmString];
                                if (answer != correctAnswer)
                                {
                                    dto.IsSuccessful = false;
                                    bResults[iquestion - 1] = false;
                                }  
                                break;
                            case 2: // yes no
                                pararmString = "question" + iquestion.ToString();
                                answer = formParams[pararmString];
                                if (answer != correctAnswer)                                
                                {
                                    dto.IsSuccessful = false;
                                    bResults[iquestion - 1] = false;
                                }
                                break;
                            case 3: // check all
                                string[] aAnswers = correctAnswer.Split(':');
                                int numChecks = int.Parse(aAnswers[0]);
                                bool bIsCorrect = false;

                                for (int i = 0; i < numChecks; i++)
                                {
                                    pararmString = "question" + iquestion.ToString() + "." + i.ToString();
                                    bIsCorrect = false;
                                    if (formParams[pararmString] != null) //will not be null if checked
                                    {
                                        for (int j = 1; j < aAnswers.Length; j++)
                                        {
                                            if (aAnswers[j] == i.ToString())
                                            {
                                                bIsCorrect = true;
                                                break;
                                            }
                                        }                                        
                                    }
                                    else
                                    {
                                        //see if this should have been checked
                                        bIsCorrect = true;
                                        for (int j = 1; j < aAnswers.Length; j++)
                                        {
                                            if (aAnswers[j] == i.ToString())
                                            {
                                                bIsCorrect = false;
                                                break;
                                            }
                                        }
                                    }
                                    if (!bIsCorrect)
                                    {
                                        dto.IsSuccessful = false;
                                        bResults[iquestion-1] = false;
                                        break;
                                    }
                                }
                                break;
                        }
                    }
                    rdr.Close();
                    if (dto.IsSuccessful)
                        dto.Message = "You have answered all the questions correctly.";
                    else
                    {
                        dto.Message = "You have answered the following questions incorrectly:";
                        for (int i = 0; i < bResults.Count; i++)
                        {
                            if (!bResults[i])
                                dto.Messages.Add("Question " + (i + 1).ToString());
                        }
                                             
                    }

                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return null;
                }
            }

            return dto;
        }

        public static List<IDandName> GetStaffTestUsersForSite(int site)
        {
            var users = new List<IDandName>();
            
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("GetStaffTestUsersForSite");
                    SqlParameter param = new SqlParameter("@site", site);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        users.Add(new IDandName(rdr.GetInt32(0), rdr.GetString(1)));
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return null;
                }
            }

            return users;
        }

        public static List<PostTest> GetTestsCompleted(string id)
        {
            var tests = new List<PostTest>();
            var test = new PostTest();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("GetStaffTestsCompleted");
                    SqlParameter param = new SqlParameter("@id", id);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;
                    while (rdr.Read())
                    {
                        test = new PostTest();
                        pos = rdr.GetOrdinal("Name");
                        test.Name = rdr.GetString(pos);
 
                        pos = rdr.GetOrdinal("DateCompleted");
                        test.DateCompleted = rdr.GetDateTime(pos);
                        test.sDateCompleted = (test.DateCompleted != null ? test.DateCompleted.Value.ToString("MM/dd/yyyy") : "");

                        tests.Add(test);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return null;
                }
            }
            return tests;
        }

        public static List<PostTestPersonTestsCompleted> GetPostTestStaffsTestsCompleted(int siteID)
        {
            var ptpcl = new List<PostTestPersonTestsCompleted>();
            var ptpc = new PostTestPersonTestsCompleted();
            var pt = new PostTest();

            string name = "";
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("GetPostTestStaffsTestsCompleted");
                    SqlParameter param = new SqlParameter("@siteID", siteID);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;
            
                    while (rdr.Read())
                    {
                        pt = new PostTest();

                        pos = rdr.GetOrdinal("Name");
                        name = rdr.GetString(pos);
                        if (ptpc.Name != name)
                        {                     
                            ptpc = new PostTestPersonTestsCompleted();
                            ptpc.Name = name;
                            ptpcl.Add(ptpc);
                        }

                        pos = rdr.GetOrdinal("DateCompleted");
                        pt.DateCompleted = rdr.GetDateTime(pos);
                        pt.sDateCompleted = (pt.DateCompleted != null ? pt.DateCompleted.Value.ToString("MM/dd/yyyy") : "");
                        pos = rdr.GetOrdinal("TestName");
                        pt.Name = rdr.GetString(pos);
                        
                        ptpc.PostTestsCompleted.Add(pt);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return null;
                }
            }
            return ptpcl;
        }

        public static List<PostTestNextDue> GetStaffPostTestsFirstDateCompletedBySite(int siteID)
        {
            var ptndl = new List<PostTestNextDue>();
            var ptnd = new PostTestNextDue();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("GetStaffPostTestsFirstDateCompletedBySite");
                    SqlParameter param = new SqlParameter("@siteID", siteID);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;

                    while (rdr.Read())
                    {
                        ptnd = new PostTestNextDue();

                        pos = rdr.GetOrdinal("Name");
                        ptnd.Name = rdr.GetString(pos);
                        
                        pos = rdr.GetOrdinal("MinDate");
                        if (!rdr.IsDBNull(pos))
                        {
                            ptnd.NextDueDate = rdr.GetDateTime(pos).AddYears(1);
                            ptnd.sNextDueDate = ptnd.NextDueDate.Value.ToString("MM/dd/yyyy");
                        }
                        ptndl.Add(ptnd);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return null;
                }
            }
            return ptndl;
        }

        public static List<PostTestExtended> GetPostTestStaffsTestsCompletedExtended(int siteID)
        {
            var ptel = new List<PostTestExtended>();
            var pte = new PostTestExtended();
            
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = ("GetPostTestStaffsTestsCompleted");
                    SqlParameter param = new SqlParameter("@siteID", siteID);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    int pos = 0;

                    while (rdr.Read())
                    {
                        pte = new PostTestExtended();

                        pos = rdr.GetOrdinal("Name");
                        pte.PersonName = rdr.GetString(pos);
                        pos = rdr.GetOrdinal("DateCompleted");
                        pte.DateCompleted = rdr.GetDateTime(pos);
                        pte.sDateCompleted = (pte.DateCompleted != null ? pte.DateCompleted.Value.ToString("MM/dd/yyyy") : "");
                        pos = rdr.GetOrdinal("TestName");
                        pte.Name = rdr.GetString(pos);

                        ptel.Add(pte);
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    nlogger.LogError(ex);
                    return null;
                }
            }
            return ptel;
        }
    }
}