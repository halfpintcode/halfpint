using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using hpMvc.Infrastructure.Logging;
using hpMvc.Models;

namespace hpMvc.DataBase
{
    public class DbInitializeContext
    {
        public static NLogger Nlogger;
        static DbInitializeContext()
        {
            Nlogger = new NLogger();
        }

        public static bool IsValidInitialize(NameValueCollection formParams, int useSensor, out List<ValidationMessages> messages, out SSInsertionData insertData)
        {
            messages = new List<ValidationMessages>();
            insertData = new SSInsertionData();

            if (useSensor > 0)
            {
                insertData.SensorType = useSensor;

                string monitorDate = formParams["MonitorDate"];
                monitorDate = monitorDate.Trim();

                if (monitorDate.Length == 0)
                {
                    messages.Add(new ValidationMessages
                                     {FieldName = "MonitorDate", DisplayName = "Monitor Date", Message = "is required"});
                }
                else
                {
                    try
                    {
                        var dtMonitorDate = DateTime.Parse(monitorDate);
                    }
                    catch (Exception ex)
                    {
                        messages.Add(new ValidationMessages {FieldName = "MonitorDate", Message = "is not a valid date"});
                    }
                }
                insertData.MonitorDate = monitorDate;

                string monitorTime = formParams["MonitorTime"];
                monitorTime = monitorTime.Trim();
                if (monitorTime.Length == 0)
                {
                    messages.Add(new ValidationMessages
                                     {FieldName = "MonitorTime", DisplayName = "Monitor Time", Message = "is required"});
                }
                else
                {
                    if (!IsValidTime(monitorTime))
                    {
                        messages.Add(new ValidationMessages
                                         {
                                             FieldName = "MonitorTime",
                                             DisplayName = "Monitor Time",
                                             Message = "is not a valid time"
                                         });
                    }
                }
                insertData.MonitorTime = monitorTime;

                string monitorID = formParams["MonitorID"];
                monitorID = monitorID.Trim();
                if (monitorID.Length == 0)
                {
                    messages.Add(new ValidationMessages
                                     {FieldName = "MonitorID", DisplayName = "Monitor ID", Message = "is required"});
                }
                insertData.MonitorId = monitorID;

                string transmitterID = formParams["TransmitterID"];
                transmitterID = transmitterID.Trim();
                if (transmitterID.Length == 0)
                {
                    messages.Add(new ValidationMessages
                                     {
                                         FieldName = "TransmitterID",
                                         DisplayName = "Transmitter ID",
                                         Message = "is required"
                                     });
                }
                insertData.TransmitterId = transmitterID;

                string sensorLot = formParams["SensorLot"];
                sensorLot = sensorLot.Trim();
                if (sensorLot.Length == 0)
                {
                    messages.Add(new ValidationMessages
                                     {FieldName = "SensorLot", DisplayName = "Sensor Lot", Message = "is required"});
                }
                insertData.SensorLot = sensorLot;

                string expirationDate = formParams["ExpirationDate"];
                sensorLot = sensorLot.Trim();
                if (sensorLot.Length == 0)
                {
                    messages.Add(new ValidationMessages { FieldName = "ExpirationDate", DisplayName = "Expiration date", Message = "is required" });
                }
                insertData.ExpirationDate = expirationDate;

                string inserterFirstName = formParams["InserterFirstName"];
                inserterFirstName = inserterFirstName.Trim();
                if (inserterFirstName.Length == 0)
                {
                    messages.Add(new ValidationMessages
                                     {
                                         FieldName = "InserterFirstName",
                                         DisplayName = "Inserter First Name",
                                         Message = "is required"
                                     });
                }
                insertData.InserterFirstName = inserterFirstName;

                string inserterLastName = formParams["InserterLastName"];
                inserterLastName = inserterLastName.Trim();
                if (inserterLastName.Length == 0)
                {
                    messages.Add(new ValidationMessages
                                     {
                                         FieldName = "InserterLastName",
                                         DisplayName = "Inserter Last Name",
                                         Message = "is required"
                                     });
                }
                insertData.InserterLastName = inserterLastName;

                string sensorLocations = formParams["SensorLocations"];
                if (sensorLocations == "0")
                {
                    messages.Add(new ValidationMessages
                                     {
                                         FieldName = "SensorLocations",
                                         DisplayName = "Sensor Location",
                                         Message = "is required"
                                     });
                }
                insertData.SensorLocation = sensorLocations;
            }//if userSensor == 1

            string bodyWeight = formParams["BodyWeight"];
            bodyWeight = bodyWeight.Trim();
            if (bodyWeight.Length == 0)
            {
                messages.Add(new ValidationMessages { FieldName = "BodyWeight", DisplayName = "Body Weight", Message = "is required" });
            }
            else
            {
                insertData.BodyWeight = bodyWeight;
            }

            string insulinConcentration = formParams["Concentrations"];
            if (insulinConcentration == "")
            {
                messages.Add(new ValidationMessages { FieldName = "SensorLocations", DisplayName = "Sensor Location", Message = "is required" });
            }
            else
            {
                insertData.InsulinConcentration = insulinConcentration;
            }

            if (messages.Count > 0)
                return false;
            
            return true;
        }
    
        private static bool IsValidTime(string monitorTime)
        {
            string[] parts = monitorTime.Split(new string[] {":"}, StringSplitOptions.None  );

            try
            {
                int hours = int.Parse(parts[0]);
                if (hours < 0 || hours > 23)
                    return false;
                int mins = int.Parse(parts[1]);
                if (mins < 0 || mins > 59)
                    return false;
            }
            catch (Exception ex)
            {                
                return false; 
            }

            return true;
        }
    }
     

    
}