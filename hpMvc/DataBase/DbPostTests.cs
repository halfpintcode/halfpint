using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Configuration;
using System.Data.SqlClient;
using hpMvc.Infrastructure.Logging;
using hpMvc.Models;
using System.Collections.Specialized;

namespace hpMvc.DataBase
{


    public static class DbPostTestsUtils
    {
        readonly static NLogger Nlogger;
        static DbPostTestsUtils()
        {
            Nlogger = new NLogger();
        }

        public static int SaveNewPostTestsCompleted(List<PostTest> postTests, int staffId, string staffName)
        {
            Nlogger.LogInfo("SavePostTestsCompleted - for: " + staffName);
            if (postTests.Any(postTest => AddAndUpdateTestCompleted(staffId, postTest.Name, postTest.sDateCompleted) == -1))
            {
                return -1;
            }
            return 1;
        }

        public static int SavePostTestsCompleted(List<PostTest> ptl, int staffId, string staffName)
        {
            Nlogger.LogInfo("SavePostTestsCompleted - for: " + staffName);

            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {

                conn.Open();
                using (var trn = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (var postTest in ptl)
                        {
                            SqlCommand cmd;
                            if (!postTest.IsCompleted)
                            {
                                cmd = new SqlCommand("", conn)
                                {
                                    Transaction = trn,
                                    CommandType = System.Data.CommandType.StoredProcedure,
                                    CommandText = "DeleteStaffPostTestCompleted"
                                };
                                var param = new SqlParameter("@staffId", staffId);
                                cmd.Parameters.Add(param);
                                param = new SqlParameter("@testId", postTest.ID);
                                cmd.Parameters.Add(param);
                                Nlogger.LogInfo("DeleteStaffPostTestCompleted - test:" + postTest.Name);
                            }
                            else
                            {
                                cmd = new SqlCommand("", conn)
                                          {
                                              Transaction = trn,
                                              CommandType = System.Data.CommandType.StoredProcedure,
                                              CommandText = ("AddOrUpdatePostTestCompleted")
                                          };
                                var param = new SqlParameter("@staffID", staffId);
                                cmd.Parameters.Add(param);
                                param = new SqlParameter("@test", postTest.Name);
                                cmd.Parameters.Add(param);
                                param = new SqlParameter("@dateCompleted", postTest.sDateCompleted);
                                cmd.Parameters.Add(param);
                                Nlogger.LogInfo("AddOrUpdateTestCompleted - test:" + postTest.Name);
                            }
                            cmd.ExecuteNonQuery();

                        }

                        trn.Commit();
                    }
                    catch (Exception ex)
                    {
                        trn.Rollback();
                        Nlogger.LogError(ex);
                        return -1;
                    }
                }
            }
            return 1;
        }

        //public static int SavePostTestsCompleted(PostTestsModel ptm)
        //{
        //    var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

        //    using (var conn = new SqlConnection(strConn))
        //    {

        //        conn.Open();
        //        using (SqlTransaction trn = conn.BeginTransaction())
        //        {
        //            try
        //            {                       

        //                var cmd = new SqlCommand("", conn)
        //                              {
        //                                  Transaction = trn,
        //                                  CommandType = System.Data.CommandType.StoredProcedure,
        //                                  CommandText = "DeleteStaffPostTestsCompleted"
        //                              };
        //                var param = new SqlParameter("@id", ptm.ID);
        //                cmd.Parameters.Add(param);

        //                cmd.ExecuteNonQuery();
        //                Nlogger.LogInfo("DeletePostTestsCompleted - name: " + ptm.Name);

        //                if (ptm.Checks)
        //                {
        //                    cmd = new SqlCommand("", conn)
        //                              {
        //                                  Transaction = trn,
        //                                  CommandType = System.Data.CommandType.StoredProcedure,
        //                                  CommandText = ("AddStaffPostTestCompleted")
        //                              };
        //                    param = new SqlParameter("@staffID", ptm.ID);
        //                    cmd.Parameters.Add(param);
        //                    param = new SqlParameter("@test", "Checks");
        //                    cmd.Parameters.Add(param);
        //                    param = new SqlParameter("@dateCompleted", ptm.ChecksCompleted);
        //                    cmd.Parameters.Add(param);

        //                    cmd.ExecuteNonQuery();
        //                    Nlogger.LogInfo("AddOrUpdateTestCompleted - test: Checks, name: " + ptm.Name);
        //                }

        //                if (ptm.Overview)
        //                {
        //                    cmd = new SqlCommand("", conn);
        //                    cmd.Transaction = trn;
        //                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
        //                    cmd.CommandText = ("AddStaffPostTestCompleted");
        //                    param = new SqlParameter("@staffID", ptm.ID);
        //                    cmd.Parameters.Add(param);
        //                    param = new SqlParameter("@test", "Overview");
        //                    cmd.Parameters.Add(param);
        //                    param = new SqlParameter("@dateCompleted", ptm.OverviewCompleted);
        //                    cmd.Parameters.Add(param);

        //                    cmd.ExecuteNonQuery();
        //                    Nlogger.LogInfo("AddOrUpdateTestCompleted - test: Overview, name: " + ptm.Name);
        //                }

        //                if (ptm.Medtronic)
        //                {
        //                    cmd = new SqlCommand("", conn);
        //                    cmd.Transaction = trn;
        //                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
        //                    cmd.CommandText = ("AddStaffPostTestCompleted");
        //                    param = new SqlParameter("@staffID", ptm.ID);
        //                    cmd.Parameters.Add(param);
        //                    param = new SqlParameter("@test", "Medtronic");
        //                    cmd.Parameters.Add(param);
        //                    param = new SqlParameter("@dateCompleted", ptm.MedtronicCompleted);
        //                    cmd.Parameters.Add(param);

        //                    cmd.ExecuteNonQuery();
        //                    Nlogger.LogInfo("AddOrUpdateTestCompleted - test: Medtronic, name: " + ptm.Name);
        //                }

        //                if (ptm.NovaStatStrip)
        //                {
        //                    cmd = new SqlCommand("", conn);
        //                    cmd.Transaction = trn;
        //                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
        //                    cmd.CommandText = ("AddStaffPostTestCompleted");
        //                    param = new SqlParameter("@staffID", ptm.ID);
        //                    cmd.Parameters.Add(param);
        //                    param = new SqlParameter("@test", "NovaStatStrip");
        //                    cmd.Parameters.Add(param);
        //                    param = new SqlParameter("@dateCompleted", ptm.NovaStatStripCompleted);
        //                    cmd.Parameters.Add(param);

        //                    cmd.ExecuteNonQuery();
        //                    Nlogger.LogInfo("AddOrUpdateTestCompleted - test: NovaStatStrip, name: " + ptm.Name);
        //                }

        //                if (ptm.VampJr)
        //                {
        //                    cmd = new SqlCommand("", conn);
        //                    cmd.Transaction = trn;
        //                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
        //                    cmd.CommandText = ("AddStaffPostTestCompleted");
        //                    param = new SqlParameter("@staffID", ptm.ID);
        //                    cmd.Parameters.Add(param);
        //                    param = new SqlParameter("@test", "VampJr");
        //                    cmd.Parameters.Add(param);
        //                    param = new SqlParameter("@dateCompleted", ptm.VampJrCompleted);
        //                    cmd.Parameters.Add(param);

        //                    cmd.ExecuteNonQuery();
        //                    Nlogger.LogInfo("AddOrUpdateTestCompleted - test: NovaStatStrip, name: " + ptm.Name);
        //                }
        //                trn.Commit();
        //            }
        //            catch (Exception ex)
        //            {
        //                trn.Rollback();
        //                Nlogger.LogError(ex);
        //                return -1;
        //            }
        //        }
        //    }
        //    return 1;
        //}

        public static int DoesStaffNameExist(string lastName, string firstName, int site)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = ("DoesStaffNameExist")
                              };
                    var param = new SqlParameter("@siteID", site);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@lastName", lastName);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@firstName", firstName);
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

        public static string GetNextStaffEmployeeId(string employeeId, int site)
        {
            string nextNumber;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = System.Data.CommandType.StoredProcedure,
                        CommandText = ("GetEmployeeIdsLike")
                    };
                    var param = new SqlParameter("@siteID", site);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@number", employeeId);
                    cmd.Parameters.Add(param);

                    var numList = new List<Int32>();

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        numList.Add(int.Parse(rdr.GetString(0)));
                    }
                    rdr.Close();
                    if (numList.Count == 1)
                        nextNumber = employeeId + "1";
                    else
                    {
                        var numMax = numList.Max();
                        ++numMax;
                        nextNumber = numMax.ToString(CultureInfo.InvariantCulture);
                    }

                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return "";
                }
            }

            return nextNumber;
        }

        public static DTO DoesStaffEmployeeIdExist(string employeeId, int site)
        {
            var dto = new DTO { IsSuccessful = false, ReturnValue = 0 };

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();

            using (var conn = new SqlConnection(strConn))
            {
                try
                {

                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = System.Data.CommandType.StoredProcedure,
                                      CommandText = ("DoesStaffEmployeeIDExist")
                                  };
                    var param = new SqlParameter("@siteID", site);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@employeeID", employeeId);
                    cmd.Parameters.Add(param);
                    //SqlParameter param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID");
                    param = new SqlParameter("@name", System.Data.SqlDbType.NVarChar, 50)
                                {
                                    Direction =
                                        System.Data
                                              .ParameterDirection
                                              .Output
                                };
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var count = (Int32)cmd.ExecuteScalar();
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
                    Nlogger.LogError(ex);
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }

        public static DTO DoesStaffEmployeeIdExistOtherThan(int id, string employeeId, int site)
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
                                  CommandText = ("DoesStaffEmployeeIDExistOtherThan")
                              };
                    var param = new SqlParameter("@id", id);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@siteID", site);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@employeeID", employeeId);
                    cmd.Parameters.Add(param);
                    //SqlParameter param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID");
                    param = new SqlParameter("@name", System.Data.SqlDbType.NVarChar, 50)
                            {
                                Direction =
                                    System.Data
                                    .ParameterDirection
                                    .Output
                            };
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var count = (Int32)cmd.ExecuteScalar();
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
                    Nlogger.LogError(ex);
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }

        public static DTO DoesStaffEmailExist(string email)
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
                                  CommandText = ("DoesStaffEmailExist")
                              };
                    var param = new SqlParameter("@email", email);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@name", System.Data.SqlDbType.NVarChar, 50)
                            {
                                Direction =
                                    System.Data
                                    .ParameterDirection
                                    .Output
                            };
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var count = (Int32)cmd.ExecuteScalar();
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
                    Nlogger.LogError(ex);
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }

        public static DTO DoesStaffEmailExistOtherThan(int id, string email)
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
                                  CommandText = ("DoesStaffEmailExistOtherThan")
                              };
                    var param = new SqlParameter("@email", email);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@id", id);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@name", System.Data.SqlDbType.NVarChar, 50)
                            {
                                Direction =
                                    System.Data
                                    .ParameterDirection
                                    .Output
                            };
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var count = (Int32)cmd.ExecuteScalar();
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
                    Nlogger.LogError(ex);
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }

        public static DTO DoesStaffUserNameExist(string userName)
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
                                  CommandText = ("DoesStaffUserNameExist")
                              };
                    var param = new SqlParameter("@userName", userName);
                    cmd.Parameters.Add(param);
                    //SqlParameter param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID");
                    param = new SqlParameter("@name", System.Data.SqlDbType.NVarChar, 50)
                            {
                                Direction =
                                    System.Data
                                    .ParameterDirection
                                    .Output
                            };
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var count = (Int32)cmd.ExecuteScalar();
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
                    Nlogger.LogError(ex);
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }

        public static DTO GetStaffIdByUserName(string userName)
        {
            var dto = new DTO { IsSuccessful = false, ReturnValue = 0 };

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
                    if (string.IsNullOrEmpty(userName))
                    {
                        Nlogger.LogError(ex, "user name is null or empty");
                    }
                    else
                    {
                        Nlogger.LogInfo("Error-GetStaffIdByUserName: user name: " + userName);    
                    }
                    
                    dto.ReturnValue = -1;
                    return dto;
                }
            }
        }

        public static int AddNurseStaff(string lastName, string firstName, string empId, int siteId, string email)
        {
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = ("AddNurseStaff")
                              };

                    var param = new SqlParameter("@Identity", System.Data.SqlDbType.Int, 0, "ID")
                                {
                                    Direction =
                                        System.Data
                                        .ParameterDirection
                                        .Output
                                };
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@lastName", lastName);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@firstName", firstName);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@siteID", siteId);
                    cmd.Parameters.Add(param);
                    if (empId == null)
                        empId = "";
                    param = new SqlParameter("@employeeID", empId);
                    cmd.Parameters.Add(param);

                    param = new SqlParameter("@email", email);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    return (int)cmd.Parameters["@Identity"].Value;

                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
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
                    Nlogger.LogError(ex);
                }
                return email;
            }
        }

        public static int AddAndUpdateTestCompleted(int staffId, string test, string dateCompleted)
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
                        CommandText = ("AddAndUpdatePostTestCompleted")
                    };
                    var param = new SqlParameter("@staffID", staffId);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@test", test);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@dateCompleted", dateCompleted);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    Nlogger.LogInfo("AddOrUpdateTestCompleted - test: " + test + ", staffID: " + staffId);
                    return 1;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError("AddOrUpdateTestCompleted staffID: " + staffId + ", " + ex.Message);
                    return -1;
                }
            }

        }

        public static int AddAndUpdateTestCompleted(int staffId, string test)
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
                                      CommandText = ("AddAndUpdatePostTestCompleted")
                                  };
                    var param = new SqlParameter("@staffID", staffId);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@test", test);
                    cmd.Parameters.Add(param);
                    param = new SqlParameter("@dateCompleted", DateTime.Now);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    Nlogger.LogInfo("AddOrUpdateTestCompleted - test: " + test + ", staffID: " + staffId);
                    return 1;
                }
                catch (Exception ex)
                {
                    Nlogger.LogError("AddOrUpdateTestCompleted - test: " + test + ", staffID: " + staffId + ", " + ex.Message);
                    return -1;
                }
            }

        }

        public static DynamicDTO GetSiteEmployeeInfoForSite(string site)
        {
            var dto = new DynamicDTO { IsSuccessful = true };

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
                    rdr.Read();

                    dto.Stuff.EmpIDRequired = "false";
                    dto.Stuff.EmpIDRegex = "";
                    dto.Stuff.EmpIDMessage = "";
                    int pos = rdr.GetOrdinal("EmpIDRequired");
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
                    Nlogger.LogError(ex);
                    return null;
                }
            }
            return dto;
        }

        public static DynamicDTO CheckIfEmployeeIdRequired(string user)
        {
            var dto = new DynamicDTO {IsSuccessful = true};
            SqlDataReader rdr = null;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = "GetSiteInfoForUser"
                              };
                    var param = new SqlParameter("@userName", user);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();
                    rdr.Read();

                    dto.Stuff.EmpIDRequired = "false";
                    dto.Stuff.EmpIDRegex = "";
                    dto.Stuff.EmpIDMessage = "";
                    int pos = rdr.GetOrdinal("EmpIDRequired");
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
                    Nlogger.LogError(ex);
                    return null;
                }
                finally
                {
                    if (rdr != null)
                        rdr.Close();
                }
            }
            return dto;
        }

        public static MessageListDTO VerifyPostTest(string testName, NameValueCollection formParams)
        {
            var dto = new MessageListDTO {IsSuccessful = true};
            var bResults = new List<bool>();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = System.Data.CommandType.StoredProcedure,
                                      CommandText = ("GetPostTestAnswers")
                                  };
                    var param = new SqlParameter("@testName", testName);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("Question");
                        var iquestion = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("QType");
                        var qType = rdr.GetInt32(pos);
                        pos = rdr.GetOrdinal("Answer");
                        var correctAnswer = rdr.GetString(pos);

                        bResults.Add(true);
                        string answer;
                        string pararmString;
                        switch (qType)
                        {
                            case 1: //multiple choice
                                pararmString = "question" + iquestion;
                                answer = formParams[pararmString];
                                if (answer != correctAnswer)
                                {
                                    dto.IsSuccessful = false;
                                    bResults[iquestion - 1] = false;
                                }
                                break;
                            case 2: // yes no
                                pararmString = "question" + iquestion;
                                answer = formParams[pararmString];
                                if (answer != correctAnswer)
                                {
                                    dto.IsSuccessful = false;
                                    bResults[iquestion - 1] = false;
                                }
                                break;
                            case 3: // check all
                                var aAnswers = correctAnswer.Split(':');
                                var numChecks = int.Parse(aAnswers[0]);

                                for (int i = 0; i < numChecks; i++)
                                {
                                    pararmString = "question" + iquestion + "." + i;
                                    var bIsCorrect = false;
                                    if (formParams[pararmString] != null) //will not be null if checked
                                    {
                                        for (int j = 1; j < aAnswers.Length; j++)
                                        {
                                            if (aAnswers[j] == i.ToString(CultureInfo.InvariantCulture))
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
                                        for (var j = 1; j < aAnswers.Length; j++)
                                        {
                                            if (aAnswers[j] == i.ToString(CultureInfo.InvariantCulture))
                                            {
                                                bIsCorrect = false;
                                                break;
                                            }
                                        }
                                    }
                                    if (!bIsCorrect)
                                    {
                                        dto.IsSuccessful = false;
                                        bResults[iquestion - 1] = false;
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
                                dto.Messages.Add("Question " + (i + 1));
                        }

                    }

                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return null;
                }
            }

            return dto;
        }

        public static List<IDandName> GetStaffTestUsersForSite(int site)
        {
            var users = new List<IDandName>();

            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = System.Data.CommandType.StoredProcedure,
                                      CommandText = ("GetStaffTestUsersForSite")
                                  };
                    var param = new SqlParameter("@site", site);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        users.Add(new IDandName(rdr.GetInt32(0), rdr.GetString(1)));
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return null;
                }
            }

            return users;
        }

        public static List<PostTest> GetStaffPostTestsActive(string staffId, string siteCode)
        {
            var tests = new List<PostTest>();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = System.Data.CommandType.StoredProcedure,
                        CommandText = ("GetPostTestsActive")
                    };

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("Required");
                        if (!(rdr.GetBoolean(pos)))
                            continue;

                        pos = rdr.GetOrdinal("Name");
                        string testName = rdr.GetString(pos);

                        if (siteCode == "14" || siteCode == "20" || siteCode == "27")
                        {
                            if (testName == "NovaStatStrip" || testName == "VampJr")
                                continue;
                        }

                        if (siteCode == "01" || siteCode == "02" || siteCode == "13" || siteCode == "09" || siteCode == "31")
                        {
                            if (testName == "NovaStatStrip")
                                continue;
                        }

                        if (siteCode == "15" || siteCode == "18" || siteCode == "21" || siteCode == "30" || siteCode == "33")
                        {
                            if (testName == "VampJr")
                                continue;
                        }

                        var test = new PostTest();

                        pos = rdr.GetOrdinal("ID");
                        test.ID = rdr.GetInt32(pos);

                        test.Name = testName;

                        pos = rdr.GetOrdinal("PathName");
                        test.PathName = rdr.GetString(pos);
                        test.sDateCompleted = "";
                        tests.Add(test);
                    }
                    rdr.Close();
                    conn.Close();

                    cmd = new SqlCommand("", conn)
                    {
                        CommandType = System.Data.CommandType.StoredProcedure,
                        CommandText = ("GetStaffPostTestsCompletedCurrentAndActive")
                    };
                    var param = new SqlParameter("@staffId", staffId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("TestID");
                        var testId = rdr.GetInt32(pos);
                        var test = tests.Find(x => x.ID == testId);
                        if (test == null)
                            continue;
                        pos = rdr.GetOrdinal("DateCompleted");
                        //test.DateCompleted = rdr.GetDateTime(pos);
                        var dateCompleted = rdr.GetDateTime(pos);
                        //test.sDateCompleted = dateCompleted.ToString("MM/dd/yyyy");
                        test.IsCompleted = true;

                        //don't mark 'Overview' as expired
                        if (test.Name == "Overview")
                        {
                            tests.Remove(test);
                        }
                        var nextDueDate = dateCompleted.AddYears(1);
                        var tsDayWindow = nextDueDate - DateTime.Now;

                        if (tsDayWindow.Days <= 30)
                        {
                            if (tsDayWindow.Days < 0)
                            {
                                test.IsExpired = true;
                            }
                            else
                            {
                                test.IsExpiring = true;
                            }
                        }
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return null;
                }
            }
            return tests;
        }

        public static List<PostTest> GetStaffPostTestsCompletedCurrentAndActive(string staffId, string siteCode)
        {
            var tests = new List<PostTest>();

            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                    {
                        CommandType = System.Data.CommandType.StoredProcedure,
                        CommandText = ("GetPostTestsActive")
                    };

                    conn.Open();
                    var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("Required");
                        if (!(rdr.GetBoolean(pos)))
                            continue;

                        pos = rdr.GetOrdinal("Name");
                        string testName = rdr.GetString(pos);

                        if (siteCode == "14" || siteCode == "20" || siteCode == "27")
                        {
                            if (testName == "NovaStatStrip" || testName == "VampJr")
                                continue;
                        }

                        if (siteCode == "01" || siteCode == "02" || siteCode == "13" || siteCode == "09" || siteCode == "31")
                        {
                            if (testName == "NovaStatStrip")
                                continue;
                        }

                        if (siteCode == "15" || siteCode == "18" || siteCode == "21" || siteCode == "30" || siteCode == "33" || siteCode == "36")
                        {
                            if (testName == "VampJr")
                                continue;
                        }

                        var test = new PostTest();

                        pos = rdr.GetOrdinal("ID");
                        test.ID = rdr.GetInt32(pos);

                        test.Name = testName;

                        pos = rdr.GetOrdinal("PathName");
                        test.PathName = rdr.GetString(pos);



                        test.sDateCompleted = "";

                        tests.Add(test);
                    }
                    rdr.Close();
                    conn.Close();

                    cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = System.Data.CommandType.StoredProcedure,
                                      CommandText = ("GetStaffPostTestsCompletedCurrentAndActive")
                                  };
                    var param = new SqlParameter("@staffId", staffId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("TestID");
                        var testId = rdr.GetInt32(pos);
                        var test = tests.Find(x => x.ID == testId);
                        if (test == null)
                            continue;
                        pos = rdr.GetOrdinal("DateCompleted");
                        test.DateCompleted = rdr.GetDateTime(pos);
                        test.sDateCompleted = (test.DateCompleted != null ? test.DateCompleted.Value.ToString("MM/dd/yyyy") : "");
                        test.IsCompleted = true;

                        //don't mark 'Overview' as expired
                        if (test.Name == "Overview") continue;
                        var nextDueDate = test.DateCompleted.Value.AddYears(1);
                        var tsDayWindow = nextDueDate - DateTime.Now;

                        if (tsDayWindow.Days <= 30)
                        {
                            if (tsDayWindow.Days < 0)
                            {
                                test.IsExpired = true;
                            }
                            else
                            {
                                test.IsExpiring = true;
                            }
                        }
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    Nlogger.LogError(ex);
                    return null;
                }
            }
            return tests;
        }

        public static List<PostTest> GetTestsCompleted(string id)
        {
            var tests = new List<PostTest>();
            SqlDataReader rdr = null;
            String strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                              {
                                  CommandType = System.Data.CommandType.StoredProcedure,
                                  CommandText = ("GetStaffTestsCompleted")
                              };
                    var param = new SqlParameter("@id", id);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var test = new PostTest();
                        int pos = rdr.GetOrdinal("Name");
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
                    Nlogger.LogError(ex);
                    return null;
                }
                finally
                {
                    if (rdr != null)
                        rdr.Close();
                }
            }
            return tests;
        }

        public static List<PostTestPersonTestsCompleted> GetPostTestStaffsTestsCompleted(int siteId)
        {
            var ptpcl = new List<PostTestPersonTestsCompleted>();
            var ptpc = new PostTestPersonTestsCompleted();

            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = System.Data.CommandType.StoredProcedure,
                                      CommandText = ("GetPostTestStaffsTestsCompleted")
                                  };
                    var param = new SqlParameter("@siteID", siteId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var pt = new PostTest();

                        var pos = rdr.GetOrdinal("Name");
                        var name = rdr.GetString(pos);
                        if (ptpc.Name != name)
                        {
                            ptpc = new PostTestPersonTestsCompleted { Name = name };
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
                    Nlogger.LogError(ex);
                    return null;
                }
            }
            return ptpcl;
        }

        public static List<PostTestNextDue> GetStaffPostTestsFirstDateCompletedBySite(int siteId)
        {
            var ptndl = new List<PostTestNextDue>();
            //var ptnd = new PostTestNextDue();

            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = System.Data.CommandType.StoredProcedure,
                                      CommandText = ("GetStaffPostTestsFirstDateCompletedBySite")
                                  };
                    var param = new SqlParameter("@siteID", siteId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("Role");
                        if (rdr.IsDBNull(pos))
                            continue;
                        if (rdr.GetString(pos) != "Nurse")
                            continue;

                        var ptnd = new PostTestNextDue();

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
                    Nlogger.LogError(ex);
                    return null;
                }
            }
            return ptndl;
        }

        public static List<PostTestExtended> GetPostTestStaffsTestsCompletedExtended(int siteId)
        {
            var ptel = new List<PostTestExtended>();
            //var pte = new PostTestExtended();

            var strConn = ConfigurationManager.ConnectionStrings["Halfpint"].ToString();
            using (var conn = new SqlConnection(strConn))
            {
                try
                {
                    var cmd = new SqlCommand("", conn)
                                  {
                                      CommandType = System.Data.CommandType.StoredProcedure,
                                      CommandText = ("GetPostTestStaffsTestsCompleted")
                                  };
                    var param = new SqlParameter("@siteID", siteId);
                    cmd.Parameters.Add(param);

                    conn.Open();
                    var rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        var pos = rdr.GetOrdinal("Role");
                        if (rdr.IsDBNull(pos))
                            continue;
                        if (rdr.GetString(pos) != "Nurse")
                            continue;

                        var pte = new PostTestExtended();

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
                    Nlogger.LogError(ex);
                    return null;
                }
            }
            return ptel;
        }
    }
}