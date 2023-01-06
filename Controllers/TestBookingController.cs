using Howzu_API.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using VLS_API.Model;
using Validation;
using CrossPlatformAESEncryption.Helper;
using System.IO;
using System.Net.Http.Headers;
using RestSharp;
using System.Globalization;
using Howzu_API.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace VLS_API.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    [ApiController]
    public class TestBookingController : ControllerBase
    {
        DataAccessLayer DAL = new DataAccessLayer();
        InputValidation Ival = new InputValidation();
        FCMPushNotification fcm = new FCMPushNotification();
        private IHostingEnvironment _hostingEnvironment;
        public TestBookingController(IHostingEnvironment environment)
        {
            _hostingEnvironment = environment;
        }
        /// <summary>
        /// Get users Appointments list
        /// </summary>
        /// <param name="pagingparametermodel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("MyAppointments")]
        public string MyAppointments([FromBody] PagingParameterModel pagingparametermodel)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Createroot JSON Object
            try
            {
                var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;

                SqlParameter[] param = new SqlParameter[]
                           {
                                new SqlParameter("@Patientid",UserId),
                                new SqlParameter("@SearchingText",pagingparametermodel.Searching)
                              };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetPatientMyTestwithSearching", param);
                if (dt.Rows.Count > 0)
                {
                    dt.Columns.Add("TestProfile", typeof(string));
                    dt.Columns.Add("TestName", typeof(string));
                    dt.Columns.Add("testCode", typeof(string));
                    dt.Columns.Add("testId", typeof(string));
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var BookingId = dt.Rows[i]["sBookLabId"].ToString();
                        if (BookingId != "")
                        {
                            DataTable dt1 = DAL.GetDataTable("WS_Sp__GetBookingDetails " + BookingId);
                            string testCode = "";
                            string testId = "";
                            string TestName = "";
                            string TestProfile = "";
                            foreach (DataRow test in dt1.Rows)
                            {
                                testId += test["sTestId"].ToString() + ",";
                                testCode += test["sTestCode"].ToString() + ",";
                                TestName += test["sTestName"].ToString() + ",";
                                TestProfile += test["sProfileName"].ToString() + ",";
                            }
                            DataRow row = dt.Rows[i];
                            row["TestName"] = TestName.TrimEnd(',');
                            row["testCode"] = testCode.TrimEnd(',');
                            row["testId"] = testId.TrimEnd(',');
                            row["TestProfile"] = TestName.TrimEnd(',');
                        }
                    }

                    // Get's No of Rows Count   
                    int count = dt.Rows.Count;

                    // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
                    int CurrentPage = pagingparametermodel.pageNumber;

                    // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
                    int PageSize = pagingparametermodel.pageSize;

                    // Display TotalCount to Records to User  
                    int TotalCount = count;

                    // Calculating Totalpage by Dividing (No of Records / Pagesize)  
                    int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

                    // Returns List of Doctor after applying Paging   
                    var items = dt.Select().Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

                    Result.AppointmentList = new JArray() as dynamic;

                    for (int i = 0; i < items.Count; i++)
                    {
                        dynamic ObjAppointmentDetail = new JObject();
                        ObjAppointmentDetail.BookLabId = items[i]["sBookLabId"];
                        ObjAppointmentDetail.TestProfileName = items[i]["TestProfile"];
                        ObjAppointmentDetail.LabId = items[i]["sLabId"];
                        ObjAppointmentDetail.LabName = items[i]["sLabName"];
                        ObjAppointmentDetail.BookDate = items[i]["sBookRequestedAt"];
                        ObjAppointmentDetail.TimeSlot = items[i]["sTimeSlot"];
                        ObjAppointmentDetail.TestDate = items[i]["sTestDate"];
                        ObjAppointmentDetail.BookStatus = items[i]["sBookStatus"];
                        ObjAppointmentDetail.TestName = items[i]["TestName"];
                        ObjAppointmentDetail.TestCode = items[i]["testCode"];
                        ObjAppointmentDetail.TestId = items[i]["testId"];
                        Result.AppointmentList.Add(ObjAppointmentDetail);
                    }
                    Result.Status = true;  //  Status Key
                    Result.Msg = "Success";
                    Result.TotalCount = TotalCount;
                    Result.PageSize = PageSize;
                    Result.CurrentPage = CurrentPage;
                    Result.TotalPages = TotalPages;
                    JSONString = JsonConvert.SerializeObject(Result);
                }
                else
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = "No Record found";
                    JSONString = JsonConvert.SerializeObject(Result);
                }
            }
            catch (Exception e)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        [HttpPost]
        [Route("CheckTestStatus/{BookingId}")]
        public string CheckTestStatus(int BookingId)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
                if (!Ival.IsInteger(BookingId.ToString()))
                {
                    Msg += "Please Enter Valid Booking Id";
                }
                if (Msg.Length > 0)
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = Msg;
                    JSONString = JsonConvert.SerializeObject(Result);
                    return JSONString;
                }
                else
                {
                    Result.CustomArray = new JArray() as dynamic;
                    dynamic ObjCustom = new JObject();
                    ObjCustom.Name = "Test Schedule";
                    ObjCustom.Date = null;
                    ObjCustom.Timeslot = null;
                    ObjCustom.IsActive = false;

                    Result.CustomArray.Add(ObjCustom);

                    dynamic ObjCustom1 = new JObject();
                    ObjCustom1.Name = "Sample Collection";
                    ObjCustom1.Date = null;
                    ObjCustom1.IsActive = false;

                    Result.CustomArray.Add(ObjCustom1);

                    dynamic ObjCustom2 = new JObject();
                    ObjCustom2.Name = "Payment Details";
                    ObjCustom2.Date = null;
                    ObjCustom2.Amount = null;
                    ObjCustom2.IsActive = false;

                    Result.CustomArray.Add(ObjCustom2);

                    dynamic ObjCustom3 = new JObject();
                    ObjCustom3.Name = "Report generated";
                    ObjCustom3.Date = null;
                    ObjCustom3.IsActive = false;

                    Result.CustomArray.Add(ObjCustom3);

                    DataTable dt = new DataTable();
                    SqlParameter[] param = new SqlParameter[]
                      {
                        new SqlParameter("@BookingId",BookingId),
                        new SqlParameter("@PatientId",UserId)
                      };
                    dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_CheckStatus", param);
                    if (dt.Rows.Count > 0)
                    {
                        dt.Columns.Add("TestName", typeof(string));
                        DataTable dt1 = DAL.GetDataTable("WS_Sp_GetTestDetailsByBookingId " + BookingId);
                        if (dt1.Rows.Count > 0)
                        {
                            string TestName = "";
                            foreach (DataRow test in dt1.Rows)
                            {
                                TestName += test["sTestName"].ToString() + ",";
                            }
                            DataRow row = dt.Rows[0];
                            row["TestName"] = TestName.TrimEnd(',');
                        }

                        Result.EventArray = new JArray() as dynamic;
                        DataTable dtTestDate = DAL.GetDataTable("WS_Sp_CheckBookingTestDateandTimeSlot " + BookingId);
                        if (dtTestDate.Rows.Count > 0)
                        {
                            string _FormatedDate = "";
                            string date = dtTestDate.Rows[0]["sTestDate"].ToString();
                            if (date.Contains("/"))
                            {
                                string[] _splitDate = date.Split('/');
                                _FormatedDate = _splitDate[2] + '-' + _splitDate[1] + '-' + _splitDate[0];
                            }
                            else
                            {
                                string[] _splitDate = date.Split('-');
                                _FormatedDate = _splitDate[2] + '-' + _splitDate[1] + '-' + _splitDate[0];
                            }
                            dynamic ObjEventDetail = new JObject();
                            ObjEventDetail.Name = "Test Schedule";
                            ObjEventDetail.Date = _FormatedDate;
                            ObjEventDetail.Timeslot = dtTestDate.Rows[0]["sTimeSlot"];
                            ObjEventDetail.IsActive = true;
                            Result.EventArray.Add(ObjEventDetail);
                        }

                        DataTable dtEvent = DAL.GetDataTable("WS_Sp_CheckBookingStatusUpdated " + BookingId);
                        if (dtEvent.Rows.Count > 0)
                        {
                            for (int j = 0; j < dtEvent.Rows.Count; j++)
                            {
                                dynamic ObjEventDetail = new JObject();

                                if (dtEvent.Rows[j]["Name"].ToString() == "Sample Collection")
                                {
                                    ObjEventDetail.Name = dtEvent.Rows[j]["Name"];
                                    ObjEventDetail.Date = dtEvent.Rows[j]["Date"];
                                    ObjEventDetail.IsActive = true;
                                }
                                else if (dtEvent.Rows[j]["Name"].ToString() == "Payment Details")
                                {
                                    ObjEventDetail.Name = dtEvent.Rows[j]["Name"];
                                    ObjEventDetail.Date = dtEvent.Rows[j]["Date"];
                                    ObjEventDetail.Amount = dtEvent.Rows[j]["Amount"];
                                    ObjEventDetail.IsActive = true;
                                }
                                Result.EventArray.Add(ObjEventDetail);
                            }
                            DataTable dtComplete = DAL.GetDataTable("WS_Sp_GetReportApproveStatus " + BookingId);
                            if (dtComplete.Rows.Count > 0)
                            {
                                dynamic ObjApprovesDetail = new JObject();
                                ObjApprovesDetail.Name = "Report generated";
                                ObjApprovesDetail.Date = dtComplete.Rows[0]["sReportApprovedOn"];
                                ObjApprovesDetail.IsActive = true;
                                Result.EventArray.Add(ObjApprovesDetail);
                            }
                        }
                        for (int i = 0; i < Result.EventArray.Count; i++)
                        {
                            for (int j = 0; j < Result.CustomArray.Count; j++)
                            {
                                if (Result.EventArray[i].Name == Result.CustomArray[j].Name)
                                {
                                    Result.CustomArray.Remove(Result.CustomArray[j]);
                                }
                            }
                        }

                        foreach (var item in Result.CustomArray)
                        {
                            Result.EventArray.Add(item);
                        }

                        Result.Remove("CustomArray");

                        Result.ReportArray = new JArray() as dynamic;
                        SqlParameter[] reportparam = new SqlParameter[]
                        {
                        new SqlParameter("@Patientid",UserId),
                        new SqlParameter("@BookingId",BookingId)
                        };
                        DataTable dtReport = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetReportListbyBookingId ", reportparam);
                        if (dtReport.Rows.Count > 0)
                        {
                            for (int j = 0; j < dtReport.Rows.Count; j++)
                            {
                                dynamic ObjreportDetail = new JObject();
                                ObjreportDetail.TestCode = dtReport.Rows[j]["sTestCode"];
                                ObjreportDetail.TimeSlot = dtReport.Rows[j]["sTimeSlot"];
                                ObjreportDetail.TestDate = dtReport.Rows[j]["sTestDate"];
                                ObjreportDetail.TestName = dtReport.Rows[j]["sTestName"];
                                ObjreportDetail.ReportId = dtReport.Rows[j]["sBookLabTestId"];
                                ObjreportDetail.ProfileName = dtReport.Rows[j]["sProfileName"];
                                ObjreportDetail.LabName = dtReport.Rows[j]["sLabName"];
                                ObjreportDetail.LabLogo = dtReport.Rows[j]["sLabLogo"];
                                ObjreportDetail.ApprovalStatus = dtReport.Rows[j]["sApprovalStatus"];
                                ObjreportDetail.Comment = dtReport.Rows[j]["sComment"] + " " + dtReport.Rows[j]["sNotes"];
                                ObjreportDetail.Flag = "";

                                Result.ReportArray.Add(ObjreportDetail);
                            }
                        }
                        Result.Status = true;  //  Status Key 
                        Result.TestName = dt.Rows[0]["TestName"];
                        Result.LabName = dt.Rows[0]["sLabName"];
                        Result.Labaddress = dt.Rows[0]["slabaddress"];
                        Result.Labcontact = dt.Rows[0]["slabcontact"];
                        Result.LabLogo = dt.Rows[0]["sLabLogo"];
                        Result.BookingDate = dt.Rows[0]["CreatedDate"];
                        Result.Bookstatus = dt.Rows[0]["sbookstatus"];
                        Result.Paymentstatus = dt.Rows[0]["sPaymentstatus"];
                        Result.TestDate = dt.Rows[0]["sTestDate"];
                        Result.TimeSlot = dt.Rows[0]["sTimeSlot"];
                        Result.AppoinmentComment = dt.Rows[0]["sComment"];                       
                        Result.Msg = "Success";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.Remove("CustomArray");
                        Result.Status = false;  //  Status Key
                        Result.Msg = "No records found";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again."; ;
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        /// <summary>
        /// Get User Test list history
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("UserMyTest")]
        public string UserMyTest()
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                DataTable dt = DAL.GetDataTable("WS_Sp__GetPatientMyTest " + UserId);
                if (dt.Rows.Count > 0)
                {
                    Result.TestList = new JArray() as dynamic;   // Create Array for Test Details
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjTestDetail = new JObject();

                        ObjTestDetail.TestCode = dt.Rows[j]["sTestCode"];
                        ObjTestDetail.TimeSlot = dt.Rows[j]["sTimeSlot"];
                        ObjTestDetail.TestDate = dt.Rows[j]["sTestDate"];
                        ObjTestDetail.TestName = dt.Rows[j]["sTestName"];
                        ObjTestDetail.LabName = dt.Rows[j]["sLabName"];
                        ObjTestDetail.LabId = dt.Rows[j]["sLabId"];
                        ObjTestDetail.BookLabId = dt.Rows[j]["sBookLabId"];
                        ObjTestDetail.TestId = dt.Rows[j]["sTestId"];
                        ObjTestDetail.BookStatus = dt.Rows[j]["sBookStatus"];
                        ObjTestDetail.BookRequestedAt = dt.Rows[j]["sBookRequestedAt"];
                        ObjTestDetail.PaymentStatus = dt.Rows[j]["sPaymentStatus"];
                        ObjTestDetail.Fees = dt.Rows[j]["sFees"];
                        ObjTestDetail.Comment = dt.Rows[j]["sComment"];
                        ObjTestDetail.TestProfileName = dt.Rows[j]["sTestProfileName"];

                        Result.TestList.Add(ObjTestDetail); //Add Test details to array
                    }
                    Result.Status = true;  //  Status Key
                    Result.Msg = "Success.";
                    JSONString = JsonConvert.SerializeObject(Result);
                }
                else
                {
                    Result.Status = false;  //  Status Key
                    Result.Msg = "No Records found.";
                    JSONString = JsonConvert.SerializeObject(Result);
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        /// <summary>
        /// Get Lab list
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("LabDataForPriscriptionBooking")]
        public string LabDataForPriscriptionBooking()
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                DataTable dt = DAL.GetDataTable("WS_Sp_GetLabDataForPriscriptionBooking");
                if (dt.Rows.Count > 0)
                {
                    Result.LabList = new JArray() as dynamic;   // Create Array for Test Details
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjLabDetail = new JObject();

                        ObjLabDetail.Labname = dt.Rows[j]["slabname"];
                        ObjLabDetail.LabAddress = dt.Rows[j]["sLabAddress"];
                        ObjLabDetail.LabContact = dt.Rows[j]["sLabContact"];
                        ObjLabDetail.Labid = dt.Rows[j]["slabid"];
                        ObjLabDetail.LabLocation = dt.Rows[j]["sLabLocation"];
                        ObjLabDetail.LabLogo = dt.Rows[j]["slabLogo"];

                        Result.LabList.Add(ObjLabDetail); //Add Test details to array
                    }
                    Result.Status = true;  //  Status Key
                    Result.Msg = "Success.";
                    JSONString = JsonConvert.SerializeObject(Result);
                }
                else
                {
                    Result.Status = false;  //  Status Key
                    Result.Msg = "No record found.";
                    JSONString = JsonConvert.SerializeObject(Result);
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        /// <summary>
        /// Get Lab slot Details
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("LabDataSlot")]
        public string LabDataSlot([FromBody] LabSlot model)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (Ival.IsTextBoxEmpty(model.Weekday))
                {
                    Msg += "Please Enter Valid Weekday";
                }
                if (Ival.IsTextBoxEmpty(model.AppointmentType))
                {
                    Msg += "Please Enter Valid Appointment Type";
                }
                if (!Ival.IsInteger(model.LabId.ToString()))
                {
                    Msg += "Please Enter Valid LabId";
                }
                if (Msg.Length > 0)
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = Msg;
                    JSONString = JsonConvert.SerializeObject(Result);
                    return JSONString;
                }
                else
                {
                    DataTable dt = new DataTable();
                    SqlParameter[] param = new SqlParameter[]
                      {
                        new SqlParameter("@Weekday",model.Weekday),
                        new SqlParameter("@labsiddata",model.LabId),
                        new SqlParameter("@AppointmentType",model.AppointmentType)
                      };
                    dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetLabDataSlotByAppoinmentType", param);
                    if (dt.Rows.Count > 0)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Success.";
                        Result.LabSlotDetails = new JArray() as dynamic;
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            dynamic ObjLabSlotDetail = new JObject();

                            ObjLabSlotDetail.SlotId = dt.Rows[j]["sSlotId"];
                            ObjLabSlotDetail.LabId = dt.Rows[j]["sLabId"];
                            ObjLabSlotDetail.Day = dt.Rows[j]["sDay"];
                            ObjLabSlotDetail.From = dt.Rows[j]["sFrom"];
                            ObjLabSlotDetail.AppointmentType = dt.Rows[j]["sAppointmentType"];
                            ObjLabSlotDetail.Slot = dt.Rows[j]["Slot"];
                            Result.LabSlotDetails.Add(ObjLabSlotDetail);
                        }
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.Status = false;  //  Status Key
                        Result.Msg = "No timeslots available";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    return JSONString;
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
                return JSONString;
            }
        }

        /// <summary>
        /// Get Profile List by Section id
        /// </summary>
        /// <param name="SectionId">mandatory</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetProfileBySectionId/{SectionId}")]
        public string GetProfileBySectionId(int SectionId)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Createroot JSON Object

            try
            {
                string Msg = "";
                if (!Ival.IsInteger(SectionId.ToString()))
                {
                    Msg += "Please Enter Valid Section Id";
                }
                if (Msg.Length > 0)
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = Msg;
                    JSONString = JsonConvert.SerializeObject(Result);
                    return JSONString;
                }
                else
                {
                    /*  Call GetDataTable method to return profile list by section Id. return DataTable     */
                    DataTable dt = DAL.GetDataTable("WS_Sp_GetProfileBySectionId " + SectionId);

                    if (dt.Rows.Count > 0)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Success.";
                        Result.SectionProfileList = new JArray() as dynamic;   // Create Array for Section Profile List

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            var TestProfileID = dt.Rows[i]["sTestProfileId"].ToString(); // Create variable to store profile Id

                            /*  Call GetDataTable method to return profile Test list by Profile Id.return DataTable */
                            DataTable dt1 = DAL.GetDataTable("WS_Sp_TestDetailsByProfileId " + TestProfileID);

                            dynamic ProfileObject = new JObject(); //Create Profiles JSON Object for 

                            if (i == 0)
                            {
                                ProfileObject.TestProfile = dt.Rows[i]["sProfileName"];
                                ProfileObject.TestProfileid = dt.Rows[i]["sTestProfileId"];

                                ProfileObject.ProfileTestList = new JArray() as dynamic; //Create Array to store Profile Test List Array
                                dynamic SubTestProfile = new JObject();             //  Create Object to Store each Test list details

                                for (int j = 0; j < dt1.Rows.Count; j++)
                                {
                                    SubTestProfile.TestCode = dt1.Rows[j]["sTestCode"];
                                    SubTestProfile.TestId = dt1.Rows[j]["sTestId"];
                                    SubTestProfile.TestUsefulFor = dt1.Rows[j]["sTestUsefulFor"];
                                    SubTestProfile.TestName = dt1.Rows[j]["sTestName"];

                                    ProfileObject.ProfileTestList.Add(SubTestProfile); //Add each test details to Test list array
                                }
                                Result.SectionProfileList.Add(ProfileObject); // Add Profile object to Section profile list array
                            }
                            else
                            {
                                ProfileObject.TestProfile = dt.Rows[i]["sProfileName"];
                                ProfileObject.TestProfileid = dt.Rows[i]["sTestProfileId"];

                                ProfileObject.ProfileTestList = new JArray() as dynamic; //Create Array to store Profile Test List Array
                                dynamic SubTestProfile = new JObject();//  Create Object to Store each Test list details

                                for (int j = 0; j < dt1.Rows.Count; j++)
                                {
                                    SubTestProfile.TestCode = dt1.Rows[j]["sTestCode"];
                                    SubTestProfile.TestId = dt1.Rows[j]["sTestId"];
                                    SubTestProfile.TestUsefulFor = dt1.Rows[j]["sTestUsefulFor"];
                                    SubTestProfile.TestName = dt1.Rows[j]["sTestName"];

                                    ProfileObject.ProfileTestList.Add(SubTestProfile); //Add each test details to Test list array
                                }
                                Result.SectionProfileList.Add(ProfileObject);// Add Profile object to Section profile list array
                            }
                        }
                        JSONString = JsonConvert.SerializeObject(Result); // store root Json Object to string variable
                    }
                    else
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "No record found.";
                        Result.SectionProfileList = new JArray() as dynamic;   // Create Array for Section Profile List
                        JSONString = JsonConvert.SerializeObject(Result); // store root Json Object to string variable
                    }
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        /// <summary>
        /// Reshedule Appoinments
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ResheduleTestBooking")]
        public string ResheduleTestBooking([FromBody] ResheduleAppointment model)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (!Ival.IsInteger(model.BookingId))
                {
                    Msg += "Please Enter Valid Booking Id";
                }
                if (Ival.IsTextBoxEmpty(model.TimeSlot))
                {
                    Msg += "Please Enter Valid Time Slot";
                }
                if (!Ival.IsValidDate(model.TestDate))
                {
                    Msg += "Please Enter Valid Test Date";
                }
                if (Ival.IsTextBoxEmpty(model.AppinmentType))
                {
                    Msg += "Please Enter Valid Appointment Type";
                }
                if (Msg.Length > 0)
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = Msg;
                    JSONString = JsonConvert.SerializeObject(Result);
                    return JSONString;
                }
                else
                {
                    SqlParameter[] param = new SqlParameter[]
                        {
                            new SqlParameter("@BookId",model.BookingId),
                            new SqlParameter("@TestDate",model.TestDate),
                            new SqlParameter("@TimeSlot",model.TimeSlot ),
                            new SqlParameter("@AppoinmentType",model.AppinmentType),
                            new SqlParameter("@BookStatus","Awaiting"),
                            new SqlParameter("@BookAddress",model.SampleCollectionAddress),
                            new SqlParameter("@returnval",SqlDbType.Int)
                        };
                    int data = DAL.ExecuteStoredProcedureRetnInt("Sp_RescheduleTestDate", param);
                    if (data == 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Appointment reschedule successfully";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "Something went wrong,Please try again.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    return JSONString;
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
                return JSONString;
            }
        }

        /// <summary>
        /// Get User Pending Appointment List
        /// </summary>
        /// <param name="pagingparametermodel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PendingAppointmentList")]
        public string PendingAppointmentList([FromBody] PagingParameterModel pagingparametermodel)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Createroot JSON Object
            try
            {
                var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;

                SqlParameter[] param = new SqlParameter[]
                           {
                                new SqlParameter("@Patientid",UserId),
                                new SqlParameter("@SearchingText",pagingparametermodel.Searching)
                           };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp__GetMyPendingBookingListwithSearch", param);
                if (dt.Rows.Count > 0)
                {
                    dt.Columns.Add("TestName", typeof(string));
                    dt.Columns.Add("testCode", typeof(string));
                    dt.Columns.Add("testId", typeof(string));
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var BookingId = dt.Rows[i]["sBookLabId"].ToString();
                        if (BookingId != "")
                        {
                            DataTable dt1 = DAL.GetDataTable("WS_Sp__GetBookingDetails " + BookingId);
                            string testCode = "";
                            string testId = "";
                            string TestName = "";
                            foreach (DataRow test in dt1.Rows)
                            {
                                testId += test["sTestId"].ToString() + ",";
                                testCode += test["sTestCode"].ToString() + ",";
                                TestName += test["sTestName"].ToString() + ",";
                            }
                            DataRow row = dt.Rows[i];
                            row["TestName"] = TestName.TrimEnd(',');
                            row["testCode"] = testCode.TrimEnd(',');
                            row["testId"] = testId.TrimEnd(',');
                        }
                    }

                    // Get's No of Rows Count   
                    int count = dt.Rows.Count;

                    // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
                    int CurrentPage = pagingparametermodel.pageNumber;

                    // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
                    int PageSize = pagingparametermodel.pageSize;

                    // Display TotalCount to Records to User  
                    int TotalCount = count;

                    // Calculating Totalpage by Dividing (No of Records / Pagesize)  
                    int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

                    // Returns List of Doctor after applying Paging   
                    var items = dt.Select().Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

                    Result.AppointmentList = new JArray() as dynamic;

                    for (int i = 0; i < items.Count; i++)
                    {
                        dynamic ObjAppointmentDetail = new JObject();
                        ObjAppointmentDetail.BookLabId = items[i]["sBookLabId"];
                        ObjAppointmentDetail.LabId = items[i]["sLabId"];
                        ObjAppointmentDetail.LabName = items[i]["sLabName"];
                        ObjAppointmentDetail.LabLogo = items[i]["sLabLogo"];
                        ObjAppointmentDetail.LabContact = items[i]["sLabContact"];
                        ObjAppointmentDetail.LabEmailId = items[i]["sLabEmailId"];
                        ObjAppointmentDetail.LabAddress = items[i]["sLabAddress"];
                        ObjAppointmentDetail.BookDate = items[i]["sBookRequestedAt"];
                        ObjAppointmentDetail.TimeSlot = items[i]["sTimeSlot"];
                        ObjAppointmentDetail.TestDate = items[i]["sTestDate"];
                        ObjAppointmentDetail.BookStatus = items[i]["sBookStatus"];
                        ObjAppointmentDetail.BookMode = items[i]["sBookMode"];
                        ObjAppointmentDetail.SampleCollectionAddress = items[i]["BookingAddress"];
                        ObjAppointmentDetail.AppointmentType = items[i]["sAppointmentType"];
                        ObjAppointmentDetail.TestName = items[i]["TestName"];
                        ObjAppointmentDetail.TestCode = items[i]["testCode"];
                        ObjAppointmentDetail.TestId = items[i]["testId"];
                        Result.AppointmentList.Add(ObjAppointmentDetail);
                    }
                    Result.Status = true;  //  Status Key
                    Result.Msg = "Success";
                    Result.TotalCount = TotalCount;
                    Result.PageSize = PageSize;
                    Result.CurrentPage = CurrentPage;
                    Result.TotalPages = TotalPages;
                    JSONString = JsonConvert.SerializeObject(Result);
                }
                else
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = "No Record found";
                    JSONString = JsonConvert.SerializeObject(Result);
                }
            }
            catch (Exception e)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        /// <summary>
        /// Get Lab List
        /// </summary>
        /// <param name="pagingparametermodel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("LabList")]
        public string LabList([FromBody] PagingParameterModel pagingparametermodel)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Createroot JSON Object

            try
            {
                SqlParameter[] param = new SqlParameter[]
                    {
                          new SqlParameter("@SearchingText",pagingparametermodel.Searching)
                    };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetLabDataForPriscriptionBooking", param);
                if (dt.Rows.Count > 0)
                {
                    // Get's No of Rows Count 
                    int count = dt.Rows.Count;

                    // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
                    int CurrentPage = pagingparametermodel.pageNumber;

                    // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
                    int PageSize = pagingparametermodel.pageSize;

                    // Display TotalCount to Records to User  
                    int TotalCount = count;

                    // Calculating Totalpage by Dividing (No of Records / Pagesize)  
                    int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

                    // Returns List of Doctor after applying Paging   
                    var items = dt.Select().Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

                    Result.LabList = new JArray() as dynamic;   // Create Array for Test Details

                    for (int i = 0; i < items.Count; i++)
                    {
                        dynamic ObjLabDetail = new JObject();

                        ObjLabDetail.Labname = items[i]["slabname"];
                        ObjLabDetail.LabAddress = items[i]["sLabAddress"];
                        ObjLabDetail.LabContact = items[i]["sLabContact"];
                        ObjLabDetail.Labid = items[i]["slabid"];
                        ObjLabDetail.LabLocation = items[i]["sLabLocation"];
                        ObjLabDetail.LabLogo = items[i]["slabLogo"];

                        Result.LabList.Add(ObjLabDetail); //Add Test details to array
                    }
                    Result.Status = true;  //  Status Key
                    Result.Msg = "Success";
                    Result.TotalCount = TotalCount;
                    Result.PageSize = PageSize;
                    Result.CurrentPage = CurrentPage;
                    Result.TotalPages = TotalPages;
                    JSONString = JsonConvert.SerializeObject(Result);
                }
                else
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = "No Record found";
                    JSONString = JsonConvert.SerializeObject(Result);
                }
            }
            catch (Exception e)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        /// <summary>
        /// Get Test List
        /// </summary>
        /// <param name="pagingparametermode"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("TestList")]
        public string TestList([FromBody] PagingParameterModel pagingparametermode)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Createroot JSON Object
            try
            {
                SqlParameter[] param = new SqlParameter[]
                    {
                         new SqlParameter("@SearchingText",pagingparametermode.Searching)
                    };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetTestList", param);
                if (dt.Rows.Count > 0)
                {
                    // Get's No of Rows Count   
                    int count = dt.Rows.Count;

                    // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
                    int CurrentPage = pagingparametermode.pageNumber;

                    // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
                    int PageSize = pagingparametermode.pageSize;

                    // Display TotalCount to Records to User  
                    int TotalCount = count;

                    // Calculating Totalpage by Dividing (No of Records / Pagesize)  
                    int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

                    // Returns List of Doctor after applying Paging   
                    var items = dt.Select().Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();


                    Result.TestList = new JArray() as dynamic;

                    for (int i = 0; i < items.Count; i++)
                    {
                        dynamic ObjTestDetail = new JObject();
                        ObjTestDetail.TestName = items[i]["sTestName"];
                        ObjTestDetail.TestCode = items[i]["sTestCode"];
                        ObjTestDetail.TestId = items[i]["sTestId"];
                        ObjTestDetail.TestUsefulFor = items[i]["sTestUsefulFor"];
                        ObjTestDetail.TestProfileId = items[i]["sTestProfileId"];
                        ObjTestDetail.ProfileName = items[i]["sProfileName"];
                        ObjTestDetail.SectionName = items[i]["sSectionName"];
                        Result.TestList.Add(ObjTestDetail);
                    }
                    Result.Status = true;  //  Status Key
                    Result.Msg = "Success";
                    Result.TotalCount = TotalCount;
                    Result.PageSize = PageSize;
                    Result.CurrentPage = CurrentPage;
                    Result.TotalPages = TotalPages;
                    JSONString = JsonConvert.SerializeObject(Result);
                }
                else
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = "No Record found";
                    JSONString = JsonConvert.SerializeObject(Result);
                }
            }
            catch (Exception e)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        /// <summary>
        /// Book Appointment with prescription
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("BookAppointmentusingPrescription")]
        public string BookAppointmentusingPrescription([FromBody] PrescriptionAppoinmentBooking model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (!Ival.IsInteger(model.LabId))
                {
                    Msg += "Please Enter Valid LabId";
                }
                if (Ival.IsTextBoxEmpty(model.TimeSlot))
                {
                    Msg += "Please Enter Valid Time Slot";
                }
                if (!Ival.IsValidDate(model.TestDate))
                {
                    Msg += "Please Enter Valid Test Date";
                }
                if (Ival.IsTextBoxEmpty(model.AppointmentType))
                {
                    Msg += "Please Enter Valid Appointment Type";
                }
                if (!Ival.IsTextBoxEmpty(model.SlotId))
                {
                    if (!Ival.IsInteger(model.SlotId))
                    {
                        Msg += "Please Enter Valid Slot Id ";
                    }
                    else
                    {
                        string[] _timeSlot = model.TimeSlot.Split('-');
                        string from = _timeSlot[0];
                        string to = _timeSlot[1];

                        SqlParameter[] paramtime = new SqlParameter[]
                        {
                            new SqlParameter("@SlotId",model.SlotId),
                            new SqlParameter("@From",from),
                            new SqlParameter("@To",to),
                            new SqlParameter("@ReturnVal",SqlDbType.Int),
                        };
                        int _tSlotResult = DAL.ExecuteStoredProcedureRetnInt("Sp_GetLabSlotDetails", paramtime);

                        if (_tSlotResult == 1)
                        {

                        }
                        else
                        {
                            Msg += "Please Enter Valid Time Slot ";
                        }
                    }
                }
                if (Msg.Length > 0)
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = Msg;
                    JSONString = JsonConvert.SerializeObject(Result);
                    return JSONString;
                }
                else
                {
                    int data = 0;
                    SqlParameter[] param = new SqlParameter[]
                    {
                        new SqlParameter("@Labid",model.LabId),
                        new SqlParameter("@Patientid",UserId),
                        new SqlParameter("@TimeSlot",model.TimeSlot),
                        new SqlParameter("@BookMode","Prescription"),
                        new SqlParameter("@TestDate",model.TestDate),
                        new SqlParameter("@Testprices",model.Testprices),
                        new SqlParameter("@AppointmentType",model.AppointmentType),
                        new SqlParameter("@UploadPrescriptionImg",model.PrescriptionImg),
                        new SqlParameter("@returnval",SqlDbType.Int)
                    };
                    data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_BookAppointmentforPrescriptionMode", param);

                    if (data >= 1)
                    {
                        SqlParameter[] param2 = new SqlParameter[]
                          {
                                   new SqlParameter("@LabId",model.LabId),
                                   new SqlParameter("@UserId",UserId)
                          };
                        DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetLabNameandDevicetoken", param2);
                        if (dt.Rows.Count > 0)
                        {
                            string _LabName = dt.Rows[0]["slabname"].ToString();
                            string _Devicetoken = dt.Rows[0]["sDeviceToken"].ToString(); ;
                            string _Msg = "Your Appointment request is submitted at " + _LabName + ". Once Lab confirms we will notify you.";

                            dynamic _Result = new JObject();
                            _Result.BookingId = data;
                            string _payload = JsonConvert.SerializeObject(_Result);

                            string _type = "Booking";
                            fcm.SendNotification("Test Booking Status", _Msg, _Devicetoken, _type, data.ToString());

                            Notification.AppNotification(UserId, model.LabId, "Test Booking Status", _Msg, _type, _payload, UserId);
                            Result.Msg = _Msg;
                        }
                        Result.BookingId = data;
                        Result.Status = true;  //  Status Key 
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "Something went wrong,Please try again.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        [HttpPost, DisableRequestSizeLimit]
        [Route("UploadPrescription")]
        public string UploadPrescription()
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("Resources");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (file.Length > 0)
                {
                    Guid obj = Guid.NewGuid();
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    String fileextension = System.IO.Path.GetExtension(fileName);
                    if (fileextension.ToLower() != ".jpg" && fileextension.ToLower() != ".png" && fileextension.ToLower() != ".jpeg" && fileextension.ToLower() != ".gif" && fileextension.ToLower() != ".bmp")
                    {
                        Msg = "Please Upload only jpg,png,jpeg,gif,bmp images only";
                    }
                    if (Msg.Length > 0)
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = Msg;
                        JSONString = JsonConvert.SerializeObject(Result);
                        return JSONString;
                    }
                    else
                    {
                        var _file = obj + fileName;
                        var fullPath = Path.Combine(pathToSave, _file);
                        var dbPath = Path.Combine(folderName, _file);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }

                        string api_url = "https://visionarylifescience.com/mobileapp/service/Uploadprescription.ashx";
                        var fullFileName = _file;
                        var filepath = dbPath;

                        RestClient client = new RestClient(api_url);
                        var request = new RestRequest("api/document", Method.POST);
                        request.AddFile(Path.GetFileNameWithoutExtension(fullFileName), filepath);
                        request.AddHeader("Content-Type", "multipart/form-data");
                        request.AddParameter("ReferenceType", 28, ParameterType.RequestBody);
                        IRestResponse response = client.Execute(request);
                        var x = response;
                        string fileNames = x.Content;

                        if (System.IO.File.Exists(Path.Combine(fullPath)))
                        {
                            System.IO.File.Delete(Path.Combine(fullPath));
                        }
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Prescription uploaded successfully.";
                        Result.Path = fileNames;
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    return JSONString;
                }
                else
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = "Something went wrong,Please try again.";
                    JSONString = JsonConvert.SerializeObject(Result);
                    return JSONString;
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key 
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);
                return JSONString;
            }
        }

        /// <summary>
        /// uplaod Profile Picture
        /// </summary>
        /// <param name="UploadPresciptionNew">Mandatory</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("UploadPresciptionNew")] //New API created by Harshada @06/09/2022 To upload prescription 
        public async Task<string> UploadPresciptionNew([FromForm] UserDataModel model)
        {
            // Action<RestResponse> callback;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            Guid obj = Guid.NewGuid();
            try
            {
                var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;

                Dictionary<string, string> resp = new Dictionary<string, string>();
                string fName = obj + model.ProfileImage.FileName;//file.FileName;
                                                                 // var file = Request.Form.Files[0];
                                                                 //var folderName = Path.Combine("images/profileimage");
                var folderName = Path.Combine("Images/Prescription");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                // var pathToSave = Path.Combine("https://www.visionarylifescience.com", folderName);

                //  string path = Path.Combine("https://www.visionarylifescience.com/images/profileimage/",fName);
                var path = Path.Combine(_hostingEnvironment.ContentRootPath, "Images/Prescription/" + fName);

                string filename = model.ProfileImage.FileName;
                if (filename.Length > 0)
                {
                    string[] getExtension = filename.Split('.');
                    String fileextension = getExtension[1];
                    if (fileextension.ToLower() != "jpg" && fileextension.ToLower() != "png" && fileextension.ToLower() != "jpeg" && fileextension.ToLower() != "gif" && fileextension.ToLower() != "bmp")
                    {
                        Msg = "Please Upload only jpg,png,jpeg,gif,bmp images only";
                    }
                    if (Msg.Length > 0)
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = Msg;
                        JSONString = JsonConvert.SerializeObject(Result);
                        return JSONString;
                    }
                    else
                    {
                        //getting file name and combine with path and save it
                        // ***** create folder with same name of image and add image in that, uncomment following line
                        // using (var fileStream = new FileStream(Path.Combine(path, filename), FileMode.Create))

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await model.ProfileImage.CopyToAsync(fileStream);

                        }
                        var _file = obj + filename;
                        var fullPath = Path.Combine(pathToSave, _file);
                        var dbPath = Path.Combine(folderName, _file);
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            model.ProfileImage.CopyTo(stream);
                        }

                      
                        // string api_url = "https://visionarylifescience.com/mobileapp/service/UploadHandlerforProfile.ashx";
                        // string api_url = "http://202.154.161.105:8010/UploadHandlerforProfile.ashx";

                        //   var fullFileName = obj+filename;
                        // var filepath = dbPath;

                        // RestClient client = new RestClient(api_url);
                        // var request = new RestRequest("api/document", Method.POST);
                        // request.AddFile(Path.GetFileNameWithoutExtension(fullFileName), filepath);
                        // request.AddHeader("Content-Type", "multipart/form-data");//Content-Type
                        // request.AddParameter("ReferenceType", 28, ParameterType.RequestBody);
                        // //  client.ExecuteAsync(request, callback);
                        // IRestResponse response = client.Execute(request);
                        //// IRestResponse response = client.Post(request);  //
                        // var x = response;
                        // string fileNames = x.Content;




                        // if (System.IO.File.Exists(Path.Combine(fullPath)))
                        // {
                        //     System.IO.File.Delete(Path.Combine(fullPath));
                        // }

                        // To move a file or folder to a new location:
                        //System.IO.File.Move(path, pathToSave);
                        // System.IO.File.Copy(path,pathToSave);

                        //using (var stream = new FileStream(pathToSave, FileMode.Create))
                        //{
                        //    //model.ProfileImage.CopyTo(stream);
                        //   // path.CopyTo(stream);
                        //}
                        //return api with response
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Prescription uploaded successfully.";
                        Result.Path = dbPath; //fileNames;
                                              // Result.Getdata= Path.GetFileNameWithoutExtension(fullFileName);
                        JSONString = JsonConvert.SerializeObject(Result);

                        // *** if we use return type as ActionResult then use that 2 lines
                        //resp.Add("Path ", filename); 
                        //return Ok(resp);

                        return JSONString;
                    }
                }
                return JSONString;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Book Appointment Manually
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("BookAppointment")]
        public string BookAppointment([FromBody] BookAppoinment model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (!Ival.IsInteger(model.LabId))
                {
                    Msg += "Please Enter Valid LabId";
                }
                if (!Ival.IsTextBoxEmpty(model.DoctorId))
                {
                    if (!Ival.IsInteger(model.DoctorId))
                    {
                        Msg += "Please Enter Valid Doctor Id ";
                    }
                }
                if (Ival.IsTextBoxEmpty(model.TimeSlot))
                {
                    Msg += "Please Enter Valid Time Slot";
                }
                if (!Ival.IsValidDate(model.TestDate))
                {
                    Msg += "Please Enter Valid Test Date";
                }
                if (!Ival.IsInteger(model.TotalAmount))
                {
                    Msg += "Please Enter Valid Test Amount";
                }
                if (Ival.IsTextBoxEmpty(model.AppointmentType))
                {
                    Msg += "Please Enter Valid Appointment Type";
                }
                if (!Ival.IsInteger(model.TestCount))
                {
                    Msg += "Please Enter Valid Test Count";
                }
                if (Ival.IsTextBoxEmpty(model.TestId))
                {
                    Msg += "Please Enter Valid Test Id";
                }
                if (Ival.IsTextBoxEmpty(model.TestPrice))
                {
                    Msg += "Please Enter Valid Test Price";
                }
                if (!Ival.IsTextBoxEmpty(model.SlotId))
                {
                    if (!Ival.IsInteger(model.SlotId))
                    {
                        Msg += "Please Enter Valid Slot Id ";
                    }
                    else
                    {
                        string[] _timeSlot = model.TimeSlot.Split('-');
                        string from = _timeSlot[0];
                        string to = _timeSlot[1];

                        SqlParameter[] paramtime = new SqlParameter[]
                        {
                            new SqlParameter("@SlotId",model.SlotId),
                            new SqlParameter("@From",from),
                            new SqlParameter("@To",to),
                            new SqlParameter("@ReturnVal",SqlDbType.Int),
                        };
                        int _tSlotResult = DAL.ExecuteStoredProcedureRetnInt("Sp_GetLabSlotDetails", paramtime);

                        if (_tSlotResult == 1)
                        {

                        }
                        else
                        {
                            Msg += "Please Enter Valid Time Slot ";
                        }
                    }
                }
                if (!Ival.IsTextBoxEmpty(model.PaymentMethod))
                {
                    if (model.PaymentMethod.ToLower() != "online")
                    {
                        Msg += "Payment method should be online or empty";
                    }
                }
                if (Msg.Length > 0)
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = Msg;
                    JSONString = JsonConvert.SerializeObject(Result);
                    return JSONString;
                }
                else
                {
                    int data = 0;
                    SqlParameter[] param = new SqlParameter[]
                    {
                        new SqlParameter("@Labid",model.LabId),
                        new SqlParameter("@Patientid",UserId),
                        new SqlParameter("@Doctorid",model.DoctorId),
                        new SqlParameter("@TimeSlot",model.TimeSlot),
                        new SqlParameter("@TestDate",model.TestDate),
                        new SqlParameter("@Testprices",model.TotalAmount),
                        new SqlParameter("@AppointmentType",model.AppointmentType),
                        new SqlParameter("@SampleCollectionAddress",model.SampleCollectionAddress),
                         new SqlParameter("@OnlinePayment",model.PaymentMethod),
                        new SqlParameter("@Returnval",SqlDbType.Int)
                    };
                    data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_BookAppoinmentUpdatedOne", param);

                    if (data >= 1)
                    {
                        int _testCount = Convert.ToInt32(model.TestCount);
                        var _testIdArray = model.TestId.Split(',');
                        var _testPrices = model.TestPrice.Split(',');

                        for (int x = 0; x < _testCount; x++)
                        {
                            var _testId = _testIdArray[x];
                            var _testprice = _testPrices[x];

                            SqlParameter[] param1 = new SqlParameter[]
                                {
                                new SqlParameter("@bookingId",data),
                                new SqlParameter("@TestId",_testId),
                                new SqlParameter("@TestPrice",_testprice),
                                new SqlParameter("@Returnval",SqlDbType.Int)
                            };
                            int result = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_AddBookAppoinmentTestDetails", param1);
                        }
                        if (model.PaymentMethod.ToLower() != "online")
                        {
                            SqlParameter[] param2 = new SqlParameter[]
                           {
                                   new SqlParameter("@LabId",model.LabId),
                                   new SqlParameter("@UserId",UserId)
                           };
                            DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetLabNameandDevicetoken", param2);
                            if (dt.Rows.Count > 0)
                            {
                                string _LabName = dt.Rows[0]["slabname"].ToString();
                                string _Devicetoken = dt.Rows[0]["sDeviceToken"].ToString(); ;
                                string _Msg = "Your Appointment request is submitted  at " + _LabName + ". Once Lab confirms we will notify you.";

                                dynamic _Result = new JObject();
                                _Result.BookingId = data;
                                string _payload = JsonConvert.SerializeObject(_Result);

                                string _type = "Booking";
                                fcm.SendNotification("Test Booking Status", _Msg, _Devicetoken, _type, data.ToString());

                                Notification.AppNotification(UserId, model.LabId, "Test Booking Status", _Msg, _type, _payload, UserId);
                                Result.Msg = _Msg;
                            }
                        }
                        else
                        {
                            Result.Msg = "Your booking is submited.";
                        }
                        Result.BookingId = data;
                        Result.Status = true;  //  Status Key                        
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "Something went wrong,Please try again.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    return JSONString;
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);
                return JSONString;
            }
        }

        /// <summary>
        /// Get Test price with lab details
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetTestPriceandLabDetails")]
        public string GetTestPriceandLabDetails([FromBody] LabList model)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (!Ival.IsInteger(model.TestCount.ToString()))
                {
                    Msg += "Please Enter Valid Test Count";
                }
                if (Ival.IsTextBoxEmpty(model.TestList))
                {
                    Msg += "Please Enter Valid Test list";
                }
                if (Msg.Length > 0)
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = Msg;
                    JSONString = JsonConvert.SerializeObject(Result);
                    return JSONString;
                }
                else
                {
                    DataTable dt = new DataTable();
                    SqlParameter[] param = new SqlParameter[]
                      {
                        new SqlParameter("@TestList",model.TestList),
                        new SqlParameter("@Count",model.TestCount),
                        new SqlParameter("@SearchingText",model.Searching)
                      };
                    dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_PatientSelectLabwithSearch", param);
                    if (dt.Rows.Count > 0)
                    {
                        // Get's No of Rows Count   
                        int count = dt.Rows.Count; 


                        // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
                        int CurrentPage = model.pageNumber;

                        // Parameter is passed model Query string if it is null then it default Value will be pageSize:20  
                        int PageSize = model.pageSize;

                        // Display TotalCount to Records to User  
                        int TotalCount = count;

                        // Calculating Totalpage by Dividing (No of Records / Pagesize)  
                        int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

                        // Returns List of Doctor after applying Paging   
                        var items = dt.Select().Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

                        Result.LabList = new JArray() as dynamic;   // Create Array for Test Details
                        for (int i = 0; i < items.Count; i++)
                        {
                            string _LabId = items[i]["sLabId"].ToString();
                            if (_LabId != "1")
                            {
                                SqlParameter[] param1 = new SqlParameter[]
                                {
                                new SqlParameter("@LabId",_LabId),
                                new SqlParameter("@TestList",model.TestList)
                                };
                                DataTable dt1 = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetTestPricebyLabIdAndTestlist", param1);

                                dynamic LabObject = new JObject(); //Create Profiles JSON Object for 

                                if (i == 0)
                                {
                                    LabObject.LabId = items[i]["slabid"];
                                    LabObject.LabName = items[i]["sLabName"];
                                    LabObject.LabAddress = items[i]["sLabAddress"];
                                    LabObject.LabContact = items[i]["sLabContact"];
                                    LabObject.LabLocation = items[i]["sLabLocation"];
                                    LabObject.LabLogo = items[i]["sLabLogo"];
                                    LabObject.LabEmailId = items[i]["sLabEmailId"];
                                    LabObject.LabOnlinePayment = items[i]["OnlinePayment"];

                                    LabObject.TestDetailList = new JArray() as dynamic; //Create Array to store Profile Test List Array

                                    for (int j = 0; j < dt1.Rows.Count; j++)
                                    {
                                        dynamic SubTestPrice = new JObject();
                                        SubTestPrice.Price = dt1.Rows[j]["sPrice"];
                                        SubTestPrice.TestId = dt1.Rows[j]["sTestId"];
                                        SubTestPrice.TestName = dt1.Rows[j]["sTestName"];
                                        SubTestPrice.TestCode = dt1.Rows[j]["sTestCode"];

                                        LabObject.TestDetailList.Add(SubTestPrice); //Add Test details to array
                                    }
                                    Result.LabList.Add(LabObject); // 
                                }
                                else
                                {
                                    LabObject.LabId = items[i]["slabid"];
                                    LabObject.LabName = items[i]["sLabName"];
                                    LabObject.LabAddress = items[i]["sLabAddress"];
                                    LabObject.LabContact = items[i]["sLabContact"];
                                    LabObject.LabLocation = items[i]["sLabLocation"];
                                    LabObject.LabLogo = items[i]["sLabLogo"];
                                    LabObject.LabEmailId = items[i]["sLabEmailId"];
                                    LabObject.LabOnlinePayment = items[i]["OnlinePayment"];

                                    LabObject.TestDetailList = new JArray() as dynamic; //Create Array to store Profile Test List Array

                                    for (int j = 0; j < dt1.Rows.Count; j++)
                                    {
                                        dynamic SubTestPrice = new JObject();
                                        SubTestPrice.Price = dt1.Rows[j]["sPrice"];
                                        SubTestPrice.TestId = dt1.Rows[j]["sTestId"];
                                        SubTestPrice.TestName = dt1.Rows[j]["sTestName"];
                                        SubTestPrice.TestCode = dt1.Rows[j]["sTestCode"];

                                        LabObject.TestDetailList.Add(SubTestPrice); //Add Test details to array
                                    }
                                    Result.LabList.Add(LabObject); // 
                                }
                            }
                        }
                        Result.Status = true;  //  Status Key
                        Result.Msg = "Success";
                        Result.TotalCount = TotalCount;
                        Result.PageSize = PageSize;
                        Result.CurrentPage = CurrentPage;
                        Result.TotalPages = TotalPages;
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.Status = false;  //  Status Key
                        Result.Msg = "No record found.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    return JSONString;
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
                return JSONString;
            }
        }

        /// <summary>
        /// Get non howzu lab list
        /// </summary>
        /// <param name="pagingparametermodel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("NonHowzuLabList")]
        public string NonHowzuLabList([FromBody] PagingParameterModel pagingparametermodel)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Createroot JSON Object

            try
            {
                SqlParameter[] param = new SqlParameter[]
                    {
                          new SqlParameter("@SearchingText",pagingparametermodel.Searching)
                    };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetnonHowzUlabsfor", param);
                if (dt.Rows.Count > 0)
                {
                    // Get's No of Rows Count 
                    int count = dt.Rows.Count;

                    // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
                    int CurrentPage = pagingparametermodel.pageNumber;

                    // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
                    int PageSize = pagingparametermodel.pageSize;

                    // Display TotalCount to Records to User  
                    int TotalCount = count;

                    // Calculating Totalpage by Dividing (No of Records / Pagesize)  
                    int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

                    // Returns List of Doctor after applying Paging   
                    var items = dt.Select().Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

                    Result.LabList = new JArray() as dynamic;   // Create Array for Test Details

                    for (int i = 0; i < items.Count; i++)
                    {
                        dynamic ObjLabDetail = new JObject();

                        ObjLabDetail.Labname = items[i]["sLabName"];
                        ObjLabDetail.LabAddress = items[i]["sLabAddress"];
                        ObjLabDetail.LabContact = items[i]["sLabContact"];
                        ObjLabDetail.Labid = items[i]["sLabId"];
                        ObjLabDetail.LabEmailId = items[i]["sLabEmailId"];
                        ObjLabDetail.LabLogo = items[i]["slabLogo"];

                        Result.LabList.Add(ObjLabDetail); //Add Test details to array
                    }
                    Result.Status = true;  //  Status Key
                    Result.Msg = "Success";
                    Result.TotalCount = TotalCount;
                    Result.PageSize = PageSize;
                    Result.CurrentPage = CurrentPage;
                    Result.TotalPages = TotalPages;
                    JSONString = JsonConvert.SerializeObject(Result);
                }
                else
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = "No Record found";
                    JSONString = JsonConvert.SerializeObject(Result);
                }
            }
            catch (Exception e)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        /// <summary>
        /// Add Non Howzu Lab
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddNonHowzuLab")]
        public string AddNonHowzuLab([FromBody] AddLab model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (Ival.IsTextBoxEmpty(model.LabName))
                {
                    Msg += "Please Enter Valid Lab Name";
                }
                if (!Ival.IsCharOnly(model.LabManager))
                {
                    Msg += "Please Enter Valid Lab Manager Name";
                }
                if (!Ival.IsValidEmailAddress(model.LabEmaild))
                {
                    Msg += "Please Enter Valid Email Id";
                }
                if (Ival.IsInteger(model.LabContact))
                {
                    if (!Ival.MobileValidation(model.LabContact))
                    {
                        Msg += "Please Enter Valid Mobile Number";
                    }
                }
                else
                {
                    Msg += "Please Enter Valid Mobile Number";
                }
                if (Msg.Length > 0)
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = Msg;
                    JSONString = JsonConvert.SerializeObject(Result);
                    return JSONString;
                }
                else
                {
                    string _EmailId = CryptoHelper.Encrypt(model.LabEmaild.ToLower());
                    string _Mobile = CryptoHelper.Encrypt(model.LabContact);
                    string timestamp = DateTime.UtcNow.ToString("ddMMyyyyHHmmssms");
                    int data = 0;
                    SqlParameter[] param = new SqlParameter[]
                    {
                        new SqlParameter("@LabCode","LAB" + timestamp),
                        new SqlParameter("@LabName",model.LabName),
                        new SqlParameter("@LabManager",model.LabManager),
                        new SqlParameter("@LabEmailId",_EmailId),
                        new SqlParameter("@LabContact",_Mobile),
                        new SqlParameter("@LabAddress",model.LabAddress),
                        new SqlParameter("@CreatedBy",UserId),
                        new SqlParameter("@returnval",SqlDbType.Int)
                    };
                    data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_AddNonHowzuLab", param);

                    if (data >= 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Lab added successfully.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "Something went wrong,Please try again.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    return JSONString;
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);
                return JSONString;
            }
        }

        /// <summary>
        /// Add previous reports
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ManualReportPunching")]
        public string ManualReportPunching([FromBody] ManualTestBooking model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            string LabId = "";
            try
            {
                if (!Ival.IsTextBoxEmpty(model.LabName))
                {
                    SqlParameter[] param = new SqlParameter[]
                    {
                          new SqlParameter("@SearchingText",model.LabName)
                    };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetLabId", param);
                    if (dt.Rows.Count > 0)
                    {
                        LabId = dt.Rows[0]["sLabId"].ToString();
                    }
                    else
                    {
                        string timestamp = DateTime.UtcNow.ToString("ddMMyyyyHHmmssms");
                        int data = 0;
                        SqlParameter[] param1 = new SqlParameter[]
                        {
                             new SqlParameter("@LabCode","LAB" + timestamp),
                             new SqlParameter("@LabName",model.LabName),
                             new SqlParameter("@CreatedBy",UserId),
                             new SqlParameter("@returnval",SqlDbType.Int)
                        };
                        data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_AddNonHowzuLabfromUser", param1);
                        if (data >= 1)
                        {
                            LabId = data.ToString();
                        }
                    }
                }
                if (!Ival.IsValidDate(model.Testdate))
                {
                    Msg += "Please Enter Valid Test Date";
                }
                if (!Ival.IsInteger(model.TestId.ToString()))
                {
                    Msg += "Please Enter Valid Test Id ";
                }
                if (Msg.Length > 0)
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = Msg;
                    JSONString = JsonConvert.SerializeObject(Result);
                    return JSONString;
                }
                else
                {
                    int data = 0;
                    SqlParameter[] param = new SqlParameter[]
                   {
                        new SqlParameter("@sLabId",LabId),
                        new SqlParameter("@sPatientId",UserId),
                        new SqlParameter("@sTestDate",model.Testdate),
                        new SqlParameter("@CreatedBy",UserId),
                        new SqlParameter("@returnval",SqlDbType.Int)
                  };
                    data = DAL.ExecuteStoredProcedureRetnInt("Sp_AddManualBookTest", param);

                    if (data >= 1)
                    {
                        SqlParameter[] param1 = new SqlParameter[]
                            {
                                new SqlParameter("@sBookLabId",data),
                                new SqlParameter("@sTestId",model.TestId),
                                new SqlParameter("@returnval",SqlDbType.Int)
                            };
                        int result = DAL.ExecuteStoredProcedureRetnInt("Sp_AddbookLabTestformanualPunching", param1);

                        //DataTable dt = DAL.GetDataTable("WS_Sp_GetUserdevicetoken " + UserId);
                        //if (dt.Rows.Count > 0)
                        //{
                        //    string _title = "";
                        //    string _Devicetoken = dt.Rows[0]["sDeviceToken"].ToString();
                        //    string _Msg = "";
                        //    string _payload = "";
                        //    fcm.SendNotification(_title, _Msg, _Devicetoken, _payload);
                        //    Notification.AppNotification(UserId, "", _title, _Msg, UserId);
                        //}


                        Result.Status = true;  //  Status Key 
                        Result.BookingId = data;
                        Result.ReportId = result;
                        Result.Msg = "Appointment booked successfully.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "Something went wrong,Please try again.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    return JSONString;
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);
                return JSONString;
            }
        }

        [HttpPost]
        [Route("GetReportValues")]
        public string GetReportValues(int BookingId)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (!Ival.IsInteger(BookingId.ToString()))
                {
                    Msg += "Please Enter Valid Booking Id";
                }
                if (Msg.Length > 0)
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = Msg;
                    JSONString = JsonConvert.SerializeObject(Result);
                    return JSONString;
                }
                else
                {
                    SqlParameter[] param = new SqlParameter[]
                    {
                          new SqlParameter("@UserId",UserId),
                          new SqlParameter("@BookingId",BookingId)
                    };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetBookingdetailsformanualpunching", param);

                    if (dt.Rows.Count > 0)

                    {
                        CalculateAge _age = new CalculateAge();
                        Result.AnalyteList = new JArray() as dynamic;
                        string DateOfBirth = dt.Rows[0]["sBirthDate"].ToString();
                        string Currentdate = dt.Rows[0]["sTestDate"].ToString();
                        DateTime Dob;
                        DateTime dtDob;
                        if (DateTime.TryParseExact(DateOfBirth, "dd/MM/yyyy", null, DateTimeStyles.None, out Dob))
                        {
                            dtDob = Dob;
                        }
                        else
                        {
                            dtDob = Convert.ToDateTime(DateOfBirth);
                        }
                        DateTime _CuuretDateOut;
                        DateTime _currentdate;
                        if (DateTime.TryParseExact(Currentdate, "dd/MM/yyyy", null, DateTimeStyles.None, out _CuuretDateOut))
                        {
                            _currentdate = _CuuretDateOut;
                        }
                        else
                        {
                            _currentdate = Convert.ToDateTime(Currentdate);
                        }
                        string patientAge = "";
                        if (_age.CalculateYourAge(dtDob, _currentdate)["Years"] != "0")
                        {
                            patientAge = _age.CalculateYourAge(Dob, _currentdate)["Years"] + " year";
                        }
                        else if (_age.CalculateYourAge(Dob, _currentdate)["Months"] != "0")
                        {
                            patientAge = _age.CalculateYourAge(Dob, _currentdate)["Months"] + " month";
                        }
                        else if (_age.CalculateYourAge(Dob, _currentdate)["Days"] != "0")
                        {
                            patientAge = _age.CalculateYourAge(Dob, _currentdate)["Days"] + " day";
                        }
                        string[] spiltAge = patientAge.Split(' ').ToArray();
                        int PatientAge = Convert.ToInt32(spiltAge[0]);
                        string patientageunit = spiltAge[1];

                        SqlParameter[] param1 = new SqlParameter[]
                             {
                                new SqlParameter("@testId",dt.Rows[0]["sTestId"].ToString()),
                                new SqlParameter("@Gender",dt.Rows[0]["sGender"].ToString())
                            };
                        DataTable dsTestAnalyte = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetTestReferencerange", param1);
                        SqlParameter[] param2 = new SqlParameter[]
                           {
                                new SqlParameter("@testId",dt.Rows[0]["sTestId"].ToString()),
                                new SqlParameter("@Gender",dt.Rows[0]["sGender"].ToString())
                          };
                        DataTable dsTestSubAnalyte = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetTestSubAnalyteReferenceRange", param2);

                       
                        int FromAge;
                        int ToAge;
                        string AgeUnit = "";

                        if (dsTestAnalyte.Rows.Count > 0)
                        {
                            List<string> lstASM = new List<string>();
                            foreach (DataRow row in dsTestAnalyte.Rows)
                            {
                                dynamic AnalyteObject = new JObject();
                                string asm = row["TASMId"].ToString();
                                if (dt.Rows[0]["sGender"].ToString().ToLower() == "male")
                                {
                                    FromAge = Convert.ToInt32(row["MaleFromAge"].ToString());
                                    ToAge = Convert.ToInt32(row["MaleToAge"].ToString());
                                    AgeUnit = row["MaleAgeUnit"].ToString();
                                    string Age = FromAge + "-" + ToAge + " " + AgeUnit;

                                    if ((FromAge == 0 || FromAge == 1) && ToAge == 100 && AgeUnit == "year")
                                    {
                                        if (lstASM.Contains(asm) == false)
                                        {
                                            lstASM.Add(asm);
                                            AnalyteObject.AnalyteName = row["sAnalyteName"].ToString();
                                            AnalyteObject.SubAnalyteName = "--";
                                            AnalyteObject.Specimen = row["sSampleType"].ToString();
                                            AnalyteObject.MethodName = row["sMethodName"].ToString();
                                            AnalyteObject.ResultType = row["sResultType"].ToString();
                                            AnalyteObject.ReferenceType = row["ReferenceType"].ToString();
                                            AnalyteObject.AgeGroup = Age;
                                            AnalyteObject.MaleRange = row["MaleMinValue"].ToString() + "-" + row["MaleMaxValue"].ToString();
                                            AnalyteObject.FemaleRange = "";
                                            AnalyteObject.Grade = row["Grade"].ToString();
                                            AnalyteObject.Unit = row["Unit"].ToString();
                                            AnalyteObject.Interpretation = row["Interpretation"].ToString();
                                            AnalyteObject.UpperLimit = row["UpperLimit"].ToString();
                                            AnalyteObject.LowerLimit = row["LowerLimit"].ToString();

                                            AnalyteObject.InterpretationList = new JArray() as dynamic;

                                          //  DataTable dtinterpretation = DAL.GetDataTable("WS_Sp_TestInterpretation " + row["sTestCode"].ToString().Trim());

                                            SqlParameter[] paramTest = new SqlParameter[]
                                             {
                                                  new SqlParameter("@TestCode",row["sTestCode"].ToString().Trim())
                                             };
                                            DataTable dtinterpretation = DAL.ExecuteStoredProcedureDataTable("WS_Sp_TestInterpretation ", paramTest);

                                            if (dtinterpretation.Rows.Count > 0)
                                            {
                                                foreach (DataRow _rowint in dtinterpretation.Rows)
                                                {
                                                    dynamic ObjInterPretation = new JObject();
                                                    ObjInterPretation.Result = _rowint["Interpretation"].ToString();
                                                    AnalyteObject.InterpretationList.Add(ObjInterPretation);
                                                }
                                            }
                                            Result.AnalyteList.Add(AnalyteObject);
                                        }

                                    }
                                    else
                                    {
                                        if (FromAge <= PatientAge && ToAge >= PatientAge && AgeUnit == patientageunit)
                                        {
                                            if (lstASM.Contains(asm) == false)
                                            {
                                                lstASM.Add(asm);
                                                AnalyteObject.AnalyteName = row["sAnalyteName"].ToString();
                                                AnalyteObject.SubAnalyteName = "--";
                                                AnalyteObject.Specimen = row["sSampleType"].ToString();
                                                AnalyteObject.MethodName = row["sMethodName"].ToString();
                                                AnalyteObject.ResultType = row["sResultType"].ToString();
                                                AnalyteObject.ReferenceType = row["ReferenceType"].ToString();
                                                AnalyteObject.AgeGroup = Age;
                                                AnalyteObject.MaleRange = row["MaleMinValue"].ToString() + "-" + row["MaleMaxValue"].ToString();
                                                AnalyteObject.FemaleRange = "";
                                                AnalyteObject.Grade = row["Grade"].ToString();
                                                AnalyteObject.Unit = row["Unit"].ToString();
                                                AnalyteObject.Interpretation = row["Interpretation"].ToString();
                                                AnalyteObject.UpperLimit = row["UpperLimit"].ToString();
                                                AnalyteObject.LowerLimit = row["LowerLimit"].ToString();

                                                AnalyteObject.InterpretationList = new JArray() as dynamic;

                                                //  DataTable dtinterpretation = DAL.GetDataTable("WS_Sp_TestInterpretation " + row["sTestCode"].ToString().Trim());
                                                SqlParameter[] paramTest = new SqlParameter[]
                                               {
                                                  new SqlParameter("@TestCode",row["sTestCode"].ToString().Trim())
                                               };
                                                DataTable dtinterpretation = DAL.ExecuteStoredProcedureDataTable("WS_Sp_TestInterpretation ", paramTest);
                                                if (dtinterpretation.Rows.Count > 0)
                                                {
                                                    foreach (DataRow _rowint in dtinterpretation.Rows)
                                                    {
                                                        dynamic ObjInterPretation = new JObject();
                                                        ObjInterPretation.Result = _rowint["Interpretation"].ToString();
                                                        AnalyteObject.InterpretationList.Add(ObjInterPretation);
                                                    }
                                                }
                                                Result.AnalyteList.Add(AnalyteObject);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    FromAge = Convert.ToInt32(row["FemaleFromAge"].ToString());
                                    ToAge = Convert.ToInt32(row["FemaleToAge"].ToString());
                                    AgeUnit = row["FemaleAgeUnit"].ToString();
                                    string Age = FromAge + "-" + ToAge + " " + AgeUnit;
                                    if ((FromAge == 0 || FromAge == 1) && ToAge == 100 && AgeUnit == "year")
                                    {
                                        if (lstASM.Contains(asm) == false)
                                        {
                                            lstASM.Add(asm);
                                            AnalyteObject.AnalyteName = row["sAnalyteName"].ToString();
                                            AnalyteObject.SubAnalyteName = "--";
                                            AnalyteObject.Specimen = row["sSampleType"].ToString();
                                            AnalyteObject.MethodName = row["sMethodName"].ToString();
                                            AnalyteObject.ResultType = row["sResultType"].ToString();
                                            AnalyteObject.ReferenceType = row["ReferenceType"].ToString();
                                            AnalyteObject.AgeGroup = Age;
                                            AnalyteObject.MaleRange = "";
                                            AnalyteObject.FemaleRange = row["FemaleMinValue"].ToString() + "-" + row["FemaleMaxValue"].ToString();
                                            AnalyteObject.Grade = row["Grade"].ToString();
                                            AnalyteObject.Unit = row["Unit"].ToString();
                                            AnalyteObject.Interpretation = row["Interpretation"].ToString();
                                            AnalyteObject.UpperLimit = row["UpperLimit"].ToString();
                                            AnalyteObject.LowerLimit = row["LowerLimit"].ToString();

                                            AnalyteObject.InterpretationList = new JArray() as dynamic;

                                            // DataTable dtinterpretation = DAL.GetDataTable("WS_Sp_TestInterpretation " + row["sTestCode"].ToString().Trim());
                                            SqlParameter[] paramTest = new SqlParameter[]
                                              {
                                                  new SqlParameter("@TestCode",row["sTestCode"].ToString().Trim())
                                              };
                                            DataTable dtinterpretation = DAL.ExecuteStoredProcedureDataTable("WS_Sp_TestInterpretation ", paramTest);
                                            if (dtinterpretation.Rows.Count > 0)
                                            {
                                                foreach (DataRow _rowint in dtinterpretation.Rows)
                                                {
                                                    dynamic ObjInterPretation = new JObject();
                                                    ObjInterPretation.Result = _rowint["Interpretation"].ToString();
                                                    AnalyteObject.InterpretationList.Add(ObjInterPretation);
                                                }
                                            }
                                            Result.AnalyteList.Add(AnalyteObject);
                                        }
                                    }
                                    else
                                    {
                                        if (FromAge <= PatientAge && ToAge >= PatientAge && AgeUnit == patientageunit)
                                        {
                                            if (lstASM.Contains(asm) == false)
                                            {
                                                lstASM.Add(asm);
                                                AnalyteObject.AnalyteName = row["sAnalyteName"].ToString();
                                                AnalyteObject.SubAnalyteName = "--";
                                                AnalyteObject.Specimen = row["sSampleType"].ToString();
                                                AnalyteObject.MethodName = row["sMethodName"].ToString();
                                                AnalyteObject.ResultType = row["sResultType"].ToString();
                                                AnalyteObject.ReferenceType = row["ReferenceType"].ToString();
                                                AnalyteObject.AgeGroup = Age;
                                                AnalyteObject.MaleRange = "";
                                                AnalyteObject.FemaleRange = row["FemaleMinValue"].ToString() + "-" + row["FemaleMaxValue"].ToString();
                                                AnalyteObject.Grade = row["Grade"].ToString();
                                                AnalyteObject.Unit = row["Unit"].ToString();
                                                AnalyteObject.Interpretation = row["Interpretation"].ToString();
                                                AnalyteObject.UpperLimit = row["UpperLimit"].ToString();
                                                AnalyteObject.LowerLimit = row["LowerLimit"].ToString();

                                                AnalyteObject.InterpretationList = new JArray() as dynamic;

                                                //  DataTable dtinterpretation = DAL.GetDataTable("WS_Sp_TestInterpretation " + row["sTestCode"].ToString().Trim());
                                                SqlParameter[] paramTest = new SqlParameter[]
                                              {
                                                  new SqlParameter("@TestCode",row["sTestCode"].ToString().Trim())
                                              };
                                                DataTable dtinterpretation = DAL.ExecuteStoredProcedureDataTable("WS_Sp_TestInterpretation ", paramTest);
                                                if (dtinterpretation.Rows.Count > 0)
                                                {
                                                    foreach (DataRow _rowint in dtinterpretation.Rows)
                                                    {
                                                        dynamic ObjInterPretation = new JObject();
                                                        ObjInterPretation.Result = _rowint["Interpretation"].ToString();
                                                        AnalyteObject.InterpretationList.Add(ObjInterPretation);
                                                    }
                                                }
                                                Result.AnalyteList.Add(AnalyteObject);
                                            }
                                        }
                                    }

                                }
                            }
                        }

                       
                        if (dsTestSubAnalyte.Rows.Count > 0)
                        {
                            List<string> lstSASM = new List<string>();

                            foreach (DataRow row in dsTestSubAnalyte.Rows)
                            {
                                dynamic SubAnalyteObject = new JObject();
                                string sasm = row["TSASMId"].ToString();
                                if (dt.Rows[0]["sGender"].ToString().ToLower() == "male")
                                {
                                    FromAge = Convert.ToInt32(row["MaleFromAge"].ToString());
                                    ToAge = Convert.ToInt32(row["MaleToAge"].ToString());
                                    AgeUnit = row["MaleAgeUnit"].ToString();
                                    string Age = FromAge + "-" + ToAge + " " + AgeUnit;
                                    if ((FromAge == 0 || FromAge == 1) && ToAge == 100 && AgeUnit == "year")
                                    {
                                        if (lstSASM.Contains(sasm) == false)
                                        {
                                            lstSASM.Add(sasm);
                                            SubAnalyteObject.AnalyteName = row["sAnalyteName"].ToString();
                                            SubAnalyteObject.SubAnalyteName = row["sSubAnalyteName"].ToString();
                                            SubAnalyteObject.Specimen = row["sSampleType"].ToString();
                                            SubAnalyteObject.MethodName = row["sMethodName"].ToString();
                                            SubAnalyteObject.ResultType = row["sResultType"].ToString();
                                            SubAnalyteObject.ReferenceType = row["ReferenceType"].ToString();
                                            SubAnalyteObject.AgeGroup = Age;
                                            SubAnalyteObject.MaleRange = row["MaleMinValue"].ToString() + "-" + row["MaleMaxValue"].ToString();
                                            SubAnalyteObject.FemaleRange = "";
                                            SubAnalyteObject.Grade = row["Grade"].ToString();
                                            SubAnalyteObject.Unit = row["Unit"].ToString();
                                            SubAnalyteObject.Interpretation = row["Interpretation"].ToString();
                                            SubAnalyteObject.UpperLimit = row["UpperLimit"].ToString();
                                            SubAnalyteObject.LowerLimit = row["LowerLimit"].ToString();

                                            SubAnalyteObject.InterpretationList = new JArray() as dynamic;

                                            // DataTable dtinterpretation = DAL.GetDataTable("WS_Sp_TestInterpretation " + row["sTestCode"].ToString().Trim());
                                            SqlParameter[] paramTest = new SqlParameter[]
                                             {
                                                  new SqlParameter("@TestCode",row["sTestCode"].ToString().Trim())
                                             };
                                            DataTable dtinterpretation = DAL.ExecuteStoredProcedureDataTable("WS_Sp_TestInterpretation ", paramTest);
                                            if (dtinterpretation.Rows.Count > 0)
                                            {
                                                foreach (DataRow _rowint in dtinterpretation.Rows)
                                                {
                                                    dynamic ObjInterPretation = new JObject();
                                                    ObjInterPretation.Result = _rowint["Interpretation"].ToString();
                                                    SubAnalyteObject.InterpretationList.Add(ObjInterPretation);
                                                }
                                            }
                                            Result.AnalyteList.Add(SubAnalyteObject);
                                        }
                                    }
                                    else
                                    {
                                        if (FromAge <= PatientAge && ToAge >= PatientAge && AgeUnit == patientageunit)
                                        {
                                            if (lstSASM.Contains(sasm) == false)
                                            {
                                                lstSASM.Add(sasm);
                                                SubAnalyteObject.AnalyteName = row["sAnalyteName"].ToString();
                                                SubAnalyteObject.SubAnalyteName = row["sSubAnalyteName"].ToString();
                                                SubAnalyteObject.Specimen = row["sSampleType"].ToString();
                                                SubAnalyteObject.MethodName = row["sMethodName"].ToString();
                                                SubAnalyteObject.ResultType = row["sResultType"].ToString();
                                                SubAnalyteObject.ReferenceType = row["ReferenceType"].ToString();
                                                SubAnalyteObject.AgeGroup = Age;
                                                SubAnalyteObject.MaleRange = row["MaleMinValue"].ToString() + "-" + row["MaleMaxValue"].ToString();
                                                SubAnalyteObject.FemaleRange = "";
                                                SubAnalyteObject.Grade = row["Grade"].ToString();
                                                SubAnalyteObject.Unit = row["Unit"].ToString();
                                                SubAnalyteObject.Interpretation = row["Interpretation"].ToString();
                                                SubAnalyteObject.UpperLimit = row["UpperLimit"].ToString();
                                                SubAnalyteObject.LowerLimit = row["LowerLimit"].ToString();

                                                SubAnalyteObject.InterpretationList = new JArray() as dynamic;

                                                // DataTable dtinterpretation = DAL.GetDataTable("WS_Sp_TestInterpretation " + row["sTestCode"].ToString().Trim());
                                                SqlParameter[] paramTest = new SqlParameter[]
                                             {
                                                  new SqlParameter("@TestCode",row["sTestCode"].ToString().Trim())
                                             };
                                                DataTable dtinterpretation = DAL.ExecuteStoredProcedureDataTable("WS_Sp_TestInterpretation ", paramTest);
                                                if (dtinterpretation.Rows.Count > 0)
                                                {
                                                    foreach (DataRow _rowint in dtinterpretation.Rows)
                                                    {
                                                        dynamic ObjInterPretation = new JObject();
                                                        ObjInterPretation.Result = _rowint["Interpretation"].ToString();
                                                        SubAnalyteObject.InterpretationList.Add(ObjInterPretation);
                                                    }
                                                }
                                                Result.AnalyteList.Add(SubAnalyteObject);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    FromAge = Convert.ToInt32(row["FemaleFromAge"].ToString());
                                    ToAge = Convert.ToInt32(row["FemaleToAge"].ToString());
                                    AgeUnit = row["FemaleAgeUnit"].ToString();
                                    string Age = FromAge + "-" + ToAge + " " + AgeUnit;
                                    if ((FromAge == 0 || FromAge == 1) && ToAge == 100 && AgeUnit == "year")
                                    {
                                        if (lstSASM.Contains(sasm) == false)
                                        {
                                            lstSASM.Add(sasm);
                                            SubAnalyteObject.AnalyteName = row["sAnalyteName"].ToString();
                                            SubAnalyteObject.SubAnalyteName = row["sSubAnalyteName"].ToString();
                                            SubAnalyteObject.Specimen = row["sSampleType"].ToString();
                                            SubAnalyteObject.MethodName = row["sMethodName"].ToString();
                                            SubAnalyteObject.ResultType = row["sResultType"].ToString();
                                            SubAnalyteObject.ReferenceType = row["ReferenceType"].ToString();
                                            SubAnalyteObject.AgeGroup = Age;
                                            SubAnalyteObject.MaleRange = "";
                                            SubAnalyteObject.FemaleRange = row["FemaleMinValue"].ToString() + "-" + row["FemaleMaxValue"].ToString();
                                            SubAnalyteObject.Grade = row["Grade"].ToString();
                                            SubAnalyteObject.Unit = row["Unit"].ToString();
                                            SubAnalyteObject.Interpretation = row["Interpretation"].ToString();
                                            SubAnalyteObject.UpperLimit = row["UpperLimit"].ToString();
                                            SubAnalyteObject.LowerLimit = row["LowerLimit"].ToString();

                                            SubAnalyteObject.InterpretationList = new JArray() as dynamic;

                                            //  DataTable dtinterpretation = DAL.GetDataTable("WS_Sp_TestInterpretation " + row["sTestCode"].ToString().Trim());
                                            SqlParameter[] paramTest = new SqlParameter[]
                                              {
                                                  new SqlParameter("@TestCode",row["sTestCode"].ToString().Trim())
                                              };
                                            DataTable dtinterpretation = DAL.ExecuteStoredProcedureDataTable("WS_Sp_TestInterpretation ", paramTest);
                                            if (dtinterpretation.Rows.Count > 0)
                                            {
                                                foreach (DataRow _rowint in dtinterpretation.Rows)
                                                {
                                                    dynamic ObjInterPretation = new JObject();
                                                    ObjInterPretation.Result = _rowint["Interpretation"].ToString();
                                                    SubAnalyteObject.InterpretationList.Add(ObjInterPretation);
                                                }
                                            }
                                            Result.AnalyteList.Add(SubAnalyteObject);
                                        }
                                    }
                                    else
                                    {
                                        if (FromAge <= PatientAge && ToAge >= PatientAge && AgeUnit == patientageunit)
                                        {
                                            if (lstSASM.Contains(sasm) == false)
                                            {
                                                lstSASM.Add(sasm);
                                                SubAnalyteObject.AnalyteName = row["sAnalyteName"].ToString();
                                                SubAnalyteObject.SubAnalyteName = row["sSubAnalyteName"].ToString();
                                                SubAnalyteObject.Specimen = row["sSampleType"].ToString();
                                                SubAnalyteObject.MethodName = row["sMethodName"].ToString();
                                                SubAnalyteObject.ResultType = row["sResultType"].ToString();
                                                SubAnalyteObject.ReferenceType = row["ReferenceType"].ToString();
                                                SubAnalyteObject.AgeGroup = Age;
                                                SubAnalyteObject.MaleRange = "";
                                                SubAnalyteObject.FemaleRange = row["FemaleMinValue"].ToString() + "-" + row["FemaleMaxValue"].ToString();
                                                SubAnalyteObject.Grade = row["Grade"].ToString();
                                                SubAnalyteObject.Unit = row["Unit"].ToString();
                                                SubAnalyteObject.Interpretation = row["Interpretation"].ToString();
                                                SubAnalyteObject.UpperLimit = row["UpperLimit"].ToString();
                                                SubAnalyteObject.LowerLimit = row["LowerLimit"].ToString();

                                                SubAnalyteObject.InterpretationList = new JArray() as dynamic;

                                                //  DataTable dtinterpretation = DAL.GetDataTable("WS_Sp_TestInterpretation " + row["sTestCode"].ToString().Trim());
                                                SqlParameter[] paramTest = new SqlParameter[]
                                              {
                                                  new SqlParameter("@TestCode",row["sTestCode"].ToString().Trim())
                                              };
                                                DataTable dtinterpretation = DAL.ExecuteStoredProcedureDataTable("WS_Sp_TestInterpretation ", paramTest);
                                                if (dtinterpretation.Rows.Count > 0)
                                                {
                                                    foreach (DataRow _rowint in dtinterpretation.Rows)
                                                    {
                                                        dynamic ObjInterPretation = new JObject();
                                                        ObjInterPretation.Result = _rowint["Interpretation"].ToString();
                                                        SubAnalyteObject.InterpretationList.Add(ObjInterPretation);
                                                    }
                                                }
                                                Result.AnalyteList.Add(SubAnalyteObject);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Success";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "No Record found";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    return JSONString;
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);
                return JSONString;
            }
        }

        [HttpPost]
        [Route("AddReport")]
        public string AddReport([FromBody] AddManualReport model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                var _analyteList = model.AnalyteDetails;
                CreateReport _createReport = new CreateReport();
                int _count = _analyteList.Count;
                for (int i = 0; i < _count; i++)
                {
                    _createReport = _analyteList[i];
                    if (!Ival.IsInteger(_createReport.ReportId.ToString()))
                    {
                        Msg += "Please Enter Valid Report Id";
                    }
                    if (!Ival.IsInteger(_createReport.TestId.ToString()))
                    {
                        Msg += "Please Enter Valid Test Id";
                    }
                    if (Msg.Length > 0)
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = Msg;
                        JSONString = JsonConvert.SerializeObject(Result);
                        return JSONString;
                    }
                    else
                    {
                        string _value = _createReport.Value != "" ? CryptoHelper.Encrypt(_createReport.Value) : "";
                        string _result = _createReport.Result != "" ? CryptoHelper.Encrypt(_createReport.Result) : "";
                        int data = 0;
                        SqlParameter[] param = new SqlParameter[]
                        {
                            new SqlParameter("@sBookLabTestId",_createReport.ReportId),
                            new SqlParameter("@sTestId",_createReport.TestId),
                            new SqlParameter("@sAnalyte",_createReport.AnalyteName),
                            new SqlParameter("@sSubAnalyte",_createReport.SubAnalyteName),
                            new SqlParameter("@sSpecimen",_createReport.Specimen),
                            new SqlParameter("@sMethod",_createReport.MethodName),
                            new SqlParameter("@sResultType",_createReport.ResultType),
                            new SqlParameter("@sReferenceType",_createReport.ReferenceType),
                            new SqlParameter("@sAge",_createReport.AgeGroup),
                            new SqlParameter("@sMale",_createReport.MaleRange),
                            new SqlParameter("@sFemale",_createReport.FemaleRange),
                            new SqlParameter("@sGrade",_createReport.Grade),
                            new SqlParameter("@sUnits",_createReport.Unit),
                            new SqlParameter("@sInterpretation",_createReport.Interpretation),
                            new SqlParameter("@sLowerLimit",_createReport.LowerLimit),
                            new SqlParameter("@sUpperLimit",_createReport.UpperLimit),

                            


                            new SqlParameter("@sValue",_value),
                            new SqlParameter("@sResult",_result),
                            new SqlParameter("@returnval",SqlDbType.Int)
                        };
                        data = DAL.ExecuteStoredProcedureRetnInt("Sp_AddTestReport", param);

                        if (data == 1)
                        {
                            SqlParameter[] param1 = new SqlParameter[]
                                {
                                     new SqlParameter("@ReportCreatedBy",UserId),
                                     new SqlParameter("@BookLabTestId",_createReport.ReportId),
                                     new SqlParameter("@Notes",model.Notes),
                                     new SqlParameter("@returnval",SqlDbType.Int)
                                };
                            int retrnval = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_CreateReportUpdated", param1);
                            Result.Status = true;  //  Status Key 
                            Result.Msg = "Report added successfully.";
                            JSONString = JsonConvert.SerializeObject(Result);
                        }
                        else
                        {
                            Result.Status = false;  //  Status Key 
                            Result.Msg = "Something went wrong,Please try again.";
                            JSONString = JsonConvert.SerializeObject(Result);
                        }
                    }
                }
                SqlParameter[] param2 = new SqlParameter[]
                              {
                                     new SqlParameter("@EmpId",UserId),
                                     new SqlParameter("@ReportPath",_createReport.ReportPath),
                                     new SqlParameter("@Status","In Progress"),
                                     new SqlParameter("@BookingId",_createReport.BookingId),
                                     new SqlParameter("@Returnval",SqlDbType.Int)
                              };
                int retrnva1 = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_AddEmployeeReport", param2);
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);

            }
            return JSONString;
        }

        [HttpPost]
        [Route("GetUploadedReportlist")]
        public string GetUploadedReportlist([FromBody] PagingParameterModel pagingparametermodel)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Createroot JSON Object

            try
            {
                SqlParameter[] param = new SqlParameter[]
                    {
                         new SqlParameter("@UserId",UserId),
                         new SqlParameter("@SearchingText",pagingparametermodel.Searching)
                    };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetUploadedReportlistUpdated", param);
                if (dt.Rows.Count > 0)
                {
                    // Get's No of Rows Count 
                    int count = dt.Rows.Count;

                    // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
                    int CurrentPage = pagingparametermodel.pageNumber;

                    // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
                    int PageSize = pagingparametermodel.pageSize;

                    // Display TotalCount to Records to User  
                    int TotalCount = count;

                    // Calculating Totalpage by Dividing (No of Records / Pagesize)  
                    int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

                    // Returns List of Doctor after applying Paging   
                    var items = dt.Select().Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

                    Result.LabList = new JArray() as dynamic;   // Create Array for Test Details

                    for (int i = 0; i < items.Count; i++)
                    {
                        dynamic ObjLabDetail = new JObject();

                        ObjLabDetail.UploadedId = items[i]["ID"];
                        ObjLabDetail.ReportPath = items[i]["ReportPath"];
                        ObjLabDetail.Status = items[i]["Status"];
                        ObjLabDetail.IsVerified = items[i]["IsVerified"];
                        ObjLabDetail.Date = items[i]["CreatedDate"];
                        // ObjLabDetail.TestName = items[i]["sTestName"];
                        // ObjLabDetail.TestCode = items[i]["sTestCode"];

                        Result.LabList.Add(ObjLabDetail); //Add Test details to array
                    }
                    Result.Status = true;  //  Status Key
                    Result.Msg = "Success";
                    Result.TotalCount = TotalCount;
                    Result.PageSize = PageSize;
                    Result.CurrentPage = CurrentPage;
                    Result.TotalPages = TotalPages;
                    JSONString = JsonConvert.SerializeObject(Result);
                }
                else
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = "No Record found";
                    JSONString = JsonConvert.SerializeObject(Result);
                }
            }
            catch (Exception e)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        [HttpPost]
        [Route("GetManualPunchedReportlist")]
        public string GetManualPunchedReportlist([FromBody] PagingParameterModel pagingparametermodel)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Createroot JSON Object

            try
            {
                SqlParameter[] param = new SqlParameter[]
                    {
                         new SqlParameter("@UserId",UserId),
                         new SqlParameter("@SearchingText",pagingparametermodel.Searching)
                    };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetUploadedReportlist", param);
                if (dt.Rows.Count > 0)
                {
                    // Get's No of Rows Count 
                    int count = dt.Rows.Count;

                    // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
                    int CurrentPage = pagingparametermodel.pageNumber;

                    // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
                    int PageSize = pagingparametermodel.pageSize;

                    // Display TotalCount to Records to User  
                    int TotalCount = count;

                    // Calculating Totalpage by Dividing (No of Records / Pagesize)  
                    int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

                    // Returns List of Doctor after applying Paging   
                    var items = dt.Select().Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

                    Result.LabList = new JArray() as dynamic;   // Create Array for Test Details

                    for (int i = 0; i < items.Count; i++)
                    {
                        dynamic ObjLabDetail = new JObject();
                        ObjLabDetail.ReportId = items[i]["ReportId"];
                        ObjLabDetail.UploadedId = items[i]["ID"];
                        ObjLabDetail.ReportPath = items[i]["ReportPath"];
                        ObjLabDetail.Status = items[i]["Status"];
                        ObjLabDetail.IsVerified = items[i]["IsVerified"];
                        ObjLabDetail.Date = items[i]["CreatedDate"];
                        ObjLabDetail.TestName = items[i]["sTestName"];
                        ObjLabDetail.TestCode = items[i]["sTestCode"];
                        ObjLabDetail.LabName = items[i]["sLabName"];
                        ObjLabDetail.LabLogo = items[i]["sLabLogo"];

                        Result.LabList.Add(ObjLabDetail); //Add Test details to array
                    }
                    Result.Status = true;  //  Status Key
                    Result.Msg = "Success";
                    Result.TotalCount = TotalCount;
                    Result.PageSize = PageSize;
                    Result.CurrentPage = CurrentPage;
                    Result.TotalPages = TotalPages;
                    JSONString = JsonConvert.SerializeObject(Result);
                }
                else
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = "No Record found";
                    JSONString = JsonConvert.SerializeObject(Result);
                }
            }
            catch (Exception e)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        [HttpPost, DisableRequestSizeLimit]
        [Route("UploadReportImage")]
        public string UploadReportImage()
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("Resources");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (file.Length > 0)
                {
                    string timestamp = DateTime.UtcNow.ToString("ddMMyyyyHHmmssms");
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    String fileextension = System.IO.Path.GetExtension(fileName);
                    if (fileextension.ToLower() != ".jpg" && fileextension.ToLower() != ".png" && fileextension.ToLower() != ".jpeg" && fileextension.ToLower() != ".pdf" && fileextension.ToLower() != ".bmp")
                    {
                        Msg = "Please Upload only jpg,png,jpeg,pdf,bmp images only";

                    }
                    if (Msg.Length > 0)
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = Msg;
                        JSONString = JsonConvert.SerializeObject(Result);
                        return JSONString;
                    }
                    else
                    {
                        var _file = timestamp + "_" + fileName;
                        var fullPath = Path.Combine(pathToSave, _file);
                        var dbPath = Path.Combine(folderName, _file);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }

                        string api_url = "https://visionarylifescience.com/mobileapp/service/UploadReports.ashx";
                        var fullFileName = _file;
                        var filepath = dbPath;

                        RestClient client = new RestClient(api_url);
                        var request = new RestRequest("api/document", Method.POST);
                        request.AddFile(Path.GetFileNameWithoutExtension(fullFileName), filepath);
                        request.AddHeader("Content-Type", "multipart/form-data");
                        request.AddParameter("ReferenceType", 28, ParameterType.RequestBody);
                        IRestResponse response = client.Execute(request);
                        var x = response;
                        string fileNames = x.Content;

                        if (System.IO.File.Exists(Path.Combine(fullPath)))
                        {
                            System.IO.File.Delete(Path.Combine(fullPath));
                        }
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Report added successfully.";
                        Result.Path = fileNames;
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    return JSONString;
                }
                else
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = "Something went wrong,Please try again.";
                    JSONString = JsonConvert.SerializeObject(Result);
                    return JSONString;
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key 
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);
                return JSONString;
            }
        }

        [HttpGet]
        [Route("UploadReportforPunchung")]
        public string UploadReportforPunchung(string ReportPath)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (Ival.IsTextBoxEmpty(ReportPath))
                {
                    Msg += "Please Enter Valid report path";
                }
                if (Msg.Length > 0)
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = Msg;
                    JSONString = JsonConvert.SerializeObject(Result);
                    return JSONString;
                }
                else
                {
                    int data = 0;
                    SqlParameter[] param2 = new SqlParameter[]
                         {
                                     new SqlParameter("@EmpId",UserId),
                                     new SqlParameter("@ReportPath",ReportPath),
                                     new SqlParameter("@Returnval",SqlDbType.Int)
                         };
                    data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_AddEmployeeReportupdated", param2);

                    if (data == 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Report added successfully.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "Something went wrong,Please try again.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);

            }
            return JSONString;
        }

        [HttpPost]
        [Route("SuggestTestBookAppoinment")]
        public string SuggestTestBookAppoinment([FromBody] SuggestTestBookAppoinment model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (!Ival.IsInteger(model.RcomId.ToString()))
                {
                    Msg += "Please Enter Valid Recommendation Id";
                }
                if (!Ival.IsInteger(model.LabId.ToString()))
                {
                    Msg += "Please Enter Valid LabId";
                }
                if (!Ival.IsInteger(model.LabId.ToString()))
                {
                    Msg += "Please Enter Valid LabId";
                }
                if (!Ival.IsTextBoxEmpty(model.DoctorId.ToString()))
                {
                    if (!Ival.IsInteger(model.DoctorId.ToString()))
                    {
                        Msg += "Please Enter Valid Doctor Id ";
                    }
                }
                if (Ival.IsTextBoxEmpty(model.TimeSlot))
                {
                    Msg += "Please Enter Valid Time Slot";
                }
                if (!Ival.IsValidDate(model.TestDate))
                {
                    Msg += "Please Enter Valid Test Date";
                }
                if (!Ival.IsInteger(model.TotalAmount))
                {
                    Msg += "Please Enter Valid Test Amount";
                }
                if (Ival.IsTextBoxEmpty(model.AppointmentType))
                {
                    Msg += "Please Enter Valid Appointment Type";
                }
                if (!Ival.IsInteger(model.TestCount))
                {
                    Msg += "Please Enter Valid Test Count";
                }
                if (Ival.IsTextBoxEmpty(model.TestId))
                {
                    Msg += "Please Enter Valid Test Id";
                }
                if (Ival.IsTextBoxEmpty(model.TestPrice))
                {
                    Msg += "Please Enter Valid Test Price";
                }
                if (!Ival.IsTextBoxEmpty(model.PaymentMethod))
                {
                    if (model.PaymentMethod.ToLower() != "online")
                    {
                        Msg += "Payment method should be online or empty";
                    }
                }
                if (Msg.Length > 0)
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = Msg;
                    JSONString = JsonConvert.SerializeObject(Result);
                    return JSONString;
                }
                else
                {
                    int data = 0;
                    SqlParameter[] param = new SqlParameter[]
                    {
                        new SqlParameter("@Rcomid",model.RcomId),
                        new SqlParameter("@Labid",model.LabId),
                        new SqlParameter("@Patientid",UserId),
                        new SqlParameter("@Doctorid",model.DoctorId),
                        new SqlParameter("@TimeSlot",model.TimeSlot),
                        new SqlParameter("@TestDate",model.TestDate),
                        new SqlParameter("@Testprices",model.TotalAmount),
                        new SqlParameter("@AppointmentType",model.AppointmentType),
                        new SqlParameter("@SampleCollectionAddress",model.SampleCollectionAddress),
                        new SqlParameter("@OnlinePayment",model.PaymentMethod),
                        new SqlParameter("@Returnval",SqlDbType.Int)
                    };
                    data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_SuggestTestBookAppoinmentUpdated", param);

                    if (data >= 1)
                    {
                        int _testCount = Convert.ToInt32(model.TestCount);
                        var _testIdArray = model.TestId.Split(',');
                        var _testPrices = model.TestPrice.Split(',');

                        for (int x = 0; x < _testCount; x++)
                        {
                            var _testId = _testIdArray[x];
                            var _testprice = _testPrices[x];

                            SqlParameter[] param1 = new SqlParameter[]
                                {
                                new SqlParameter("@bookingId",data),
                                new SqlParameter("@TestId",_testId),
                                new SqlParameter("@TestPrice",_testprice),
                                new SqlParameter("@Returnval",SqlDbType.Int)
                            };
                            int result = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_AddBookAppoinmentTestDetails", param1);
                        }
                        if (model.PaymentMethod.ToLower() != "online")
                        {
                            SqlParameter[] param2 = new SqlParameter[]
                           {
                                   new SqlParameter("@LabId",model.LabId),
                                   new SqlParameter("@UserId",UserId)
                           };
                            DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetLabNameandDevicetoken", param2);
                            if (dt.Rows.Count > 0)
                            {
                                string _LabName = dt.Rows[0]["slabname"].ToString();
                                string _Devicetoken = dt.Rows[0]["sDeviceToken"].ToString(); ;
                                string _Msg = "Your Appointment request is submited at " + _LabName + ". Once Lab confirms we will notify you.";

                                dynamic _Result = new JObject();
                                _Result.BookingId = data;
                                string _payload = JsonConvert.SerializeObject(_Result);

                                string _type = "Booking";
                                fcm.SendNotification("Test Booking Status", _Msg, _Devicetoken, _type, data.ToString());

                                Notification.AppNotification(UserId, model.LabId.ToString(), "Test Booking Status", _Msg, _type, _payload, UserId);
                                Result.Msg = _Msg;
                            }
                        }
                        else
                        {
                            Result.Msg = "Your booking is submited.";
                        }
                        Result.BookingId = data;
                        Result.Status = true;  //  Status Key                        
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "Something went wrong,Please try again.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    return JSONString;
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);
                return JSONString;
            }
        }

        [HttpPost]
        [Route("PrescriptionAppointmentList")]
        public string PrescriptionAppointmentList([FromBody] PagingParameterModel pagingparametermodel)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Createroot JSON Object
            try
            {
                var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;

                SqlParameter[] param = new SqlParameter[]
                           {
                                 new SqlParameter("@Patientid",UserId),
                                new SqlParameter("@SearchingText",pagingparametermodel.Searching)
                              };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp__GetMyPrescriptionPendingBookingListwithSearch", param);
                if (dt.Rows.Count > 0)
                {
                    // Get's No of Rows Count   
                    int count = dt.Rows.Count;

                    // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
                    int CurrentPage = pagingparametermodel.pageNumber;

                    // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
                    int PageSize = pagingparametermodel.pageSize;

                    // Display TotalCount to Records to User  
                    int TotalCount = count;

                    // Calculating Totalpage by Dividing (No of Records / Pagesize)  
                    int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

                    // Returns List of Doctor after applying Paging   
                    var items = dt.Select().Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

                    Result.AppointmentList = new JArray() as dynamic;

                    for (int i = 0; i < items.Count; i++)
                    {
                        dynamic ObjAppointmentDetail = new JObject();
                        ObjAppointmentDetail.BookLabId = items[i]["sBookLabId"];
                        ObjAppointmentDetail.LabId = items[i]["sLabId"];
                        ObjAppointmentDetail.LabName = items[i]["sLabName"];
                        ObjAppointmentDetail.LabLogo = items[i]["sLabLogo"];
                        ObjAppointmentDetail.BookDate = items[i]["sBookRequestedAt"];
                        ObjAppointmentDetail.TimeSlot = items[i]["sTimeSlot"];
                        ObjAppointmentDetail.TestDate = items[i]["sTestDate"];
                        ObjAppointmentDetail.BookStatus = items[i]["sBookStatus"];
                        ObjAppointmentDetail.BookMode = items[i]["sBookMode"];
                        ObjAppointmentDetail.AppointmentType = items[i]["sAppointmentType"];
                        ObjAppointmentDetail.PrescriptionImage = items[i]["sUploadPrescriptionImg"];
                        Result.AppointmentList.Add(ObjAppointmentDetail);
                    }
                    Result.Status = true;  //  Status Key
                    Result.Msg = "Success";
                    Result.TotalCount = TotalCount;
                    Result.PageSize = PageSize;
                    Result.CurrentPage = CurrentPage;
                    Result.TotalPages = TotalPages;
                    JSONString = JsonConvert.SerializeObject(Result);
                }
                else
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = "No Record found";
                    JSONString = JsonConvert.SerializeObject(Result);
                }
            }
            catch (Exception e)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        [HttpGet]
        [Route("ReportDetilsForGraph")]
        public string ReportDetilsForGraph(int TestId)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = ""; 
                if (!Ival.IsInteger(TestId.ToString()))
                {
                    Msg += "Please Enter Valid Test Id";
                }
                if (Msg.Length > 0)
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = Msg;
                    JSONString = JsonConvert.SerializeObject(Result);
                    return JSONString;
                }
                else
                {
                    SqlParameter[] param = new SqlParameter[]
                    {
                             new SqlParameter("@UserID",UserId),
                             new SqlParameter("@TestID",TestId)
                    };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetReportDetailsForGraph", param);
                    //DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetReportDetailsForGraph_CompireGroupDev", param);


                     
                    if (dt.Rows.Count > 1)
                    {
                      
                            Result.Status = true;  //  Status Key
                            Result.Male = dt.Rows[0]["sMale"];
                            Result.Female = dt.Rows[0]["sFemale"];
                            Result.Age = dt.Rows[0]["sAge"];
                            Result.TestName = dt.Rows[0]["sTestName"];
                            Result.TestCode = dt.Rows[0]["sTestCode"];
                            Result.Units = dt.Rows[0]["sUnits"];
                            Result.ReportData = new JArray() as dynamic;   // Create Array for Baner Details
                            for (int j = 0; j < dt.Rows.Count; j++)
                            {
                                dynamic ObjReportDetail = new JObject();
                                ObjReportDetail.Unit = dt.Rows[j]["sUnits"];
                                ObjReportDetail.Analyte = dt.Rows[j]["sAnalyte"];
                                ObjReportDetail.SubAnalyte = dt.Rows[j]["sSubAnalyte"];
                                ObjReportDetail.Male = dt.Rows[j]["sMale"];
                                ObjReportDetail.Female = dt.Rows[j]["sFemale"];
                                ObjReportDetail.TestDate = dt.Rows[j]["sTestDate"];

                                ObjReportDetail.Value = dt.Rows[j]["sValue"];
                                ObjReportDetail.Result = dt.Rows[j]["sResult"];
                                Result.ReportData.Add(ObjReportDetail); //Add baner details to array
                            }

                            JSONString = JsonConvert.SerializeObject(Result);
                        
                    }
                    else
                    {
                        Result.Status = false;  //  Status Key
                        Result.Msg = "No Records found.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);

            }
            return JSONString;
        }



        

          
        [HttpGet]
        [Route("ReportDetilsForGraph_compare")]
        public string ReportDetilsForGraph_compare(int TestId)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
                if (!Ival.IsInteger(TestId.ToString()))
                {
                    Msg += "Please Enter Valid Test Id";
                }
                if (Msg.Length > 0)
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = Msg;
                    JSONString = JsonConvert.SerializeObject(Result);
                    return JSONString;
                }
                else
                {
                    SqlParameter[] param = new SqlParameter[]
                    {
                             new SqlParameter("@UserID",UserId),
                             new SqlParameter("@TestID",TestId)
                    };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetReportDetailsForGraph_CompireGroup", param);
                    //DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetReportDetailsForGraph_CompireGroupDev", param);
                  


                    if (dt.Rows.Count > 1)
                    {
                        Result.Status = true;  //  Status Key
                        Result.Male = dt.Rows[0]["sMale"];
                        Result.Female = dt.Rows[0]["sFemale"];
                        Result.Age = dt.Rows[0]["sAge"];
                        Result.TestName = dt.Rows[0]["sTestName"];
                        Result.TestCode = dt.Rows[0]["sTestCode"];
                        Result.Units = dt.Rows[0]["sUnits"];
                        Result.ReportData = new JArray() as dynamic;   // Create Array for Baner Details
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            dynamic ObjReportDetail = new JObject();
                            ObjReportDetail.Unit = dt.Rows[j]["sUnits"];
                            ObjReportDetail.Analyte = dt.Rows[j]["sAnalyte"];
                            ObjReportDetail.SubAnalyte = dt.Rows[j]["sSubAnalyte"];
                            ObjReportDetail.TestDate = dt.Rows[j]["sTestDate"];
                            ObjReportDetail.Value = dt.Rows[j]["sValue"];
                            ObjReportDetail.Result = dt.Rows[j]["sResult"];
                            Result.ReportData.Add(ObjReportDetail); //Add baner details to array
                        }
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.Status = false;  //  Status Key
                        Result.Msg = "No Records found.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);

            }
            return JSONString;
        }

        //[HttpGet]
        //[Route("ReportDetilsForGraph_compare")]
        //public string ReportDetilsForGraph_compare(int TestId)
        //{
        //    var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;

        //    //  int UserId = 635;
        //    string JSONString = string.Empty;
        //    dynamic Result = new JObject();  //Create root JSON Object
        //    try
        //    {
        //        string Msg = "";
        //        if (!Ival.IsInteger(TestId.ToString()))
        //        {
        //            Msg += "Please Enter Valid Test Id";
        //        }
        //        if (Msg.Length > 0)
        //        {
        //            Result.Status = false;  //  Status Key 
        //            Result.Msg = Msg;
        //            JSONString = JsonConvert.SerializeObject(Result);
        //            return JSONString;
        //        }
        //        else
        //        {
        //            SqlParameter[] param = new SqlParameter[]
        //            {
        //                     new SqlParameter("@UserID",UserId),
        //                     new SqlParameter("@TestID",TestId)
        //            };
        //            DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetReportDetailsForGraph_CompireGroup", param);

        //            if (dt.Rows.Count > 1)
        //            {
        //                Result.Status = true;  //  Status Key
        //                Result.Male = dt.Rows[0]["sMale"];
        //                Result.Female = dt.Rows[0]["sFemale"];
        //                Result.Age = dt.Rows[0]["sAge"];
        //                Result.TestName = dt.Rows[0]["sTestName"];
        //                Result.TestCode = dt.Rows[0]["sTestCode"];
        //                Result.Units = dt.Rows[0]["sUnits"];
        //                Result.ReportData = new JArray() as dynamic;   // Create Array for Baner Details
        //                for (int j = 0; j < dt.Rows.Count; j++)
        //                {
        //                    dynamic ObjReportDetail = new JObject();
        //                    ObjReportDetail.Unit = dt.Rows[j]["sUnits"];
        //                    ObjReportDetail.Analyte = dt.Rows[j]["sAnalyte"];
        //                    ObjReportDetail.SubAnalyte = dt.Rows[j]["sSubAnalyte"];
        //                    ObjReportDetail.TestDate = dt.Rows[j]["sTestDate"];
        //                    ObjReportDetail.Value = dt.Rows[j]["sValue"];
        //                    ObjReportDetail.Result = dt.Rows[j]["sResult"];
        //                    Result.ReportData.Add(ObjReportDetail); //Add baner details to array
        //                }
        //                JSONString = JsonConvert.SerializeObject(Result);
        //            }
        //            else
        //            {
        //                Result.Status = false;  //  Status Key
        //                Result.Msg = "No Records found.";
        //                JSONString = JsonConvert.SerializeObject(Result);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Result.Status = false;  //  Status Key
        //        Result.Msg = ex;
        //        JSONString = JsonConvert.SerializeObject(Result);

        //    }
        //    return JSONString;
        //}




        //[HttpGet]
        //[Route("GetReportDetailsForGraphCompireGroup")]
        //public string GetReportDetailsForGraphCompireGroup(int TestId)
        //{
        //    var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
        //    string JSONString = string.Empty;
        //    dynamic Result = new JObject();  //Create root JSON Object
        //    try
        //    {
        //        string Msg = "";
        //        if (!Ival.IsInteger(TestId.ToString()))
        //        {
        //            Msg += "Please Enter Valid Test Id";
        //        }
        //        if (Msg.Length > 0)
        //        {
        //            Result.Status = false;  //  Status Key 
        //            Result.Msg = Msg;
        //            JSONString = JsonConvert.SerializeObject(Result);
        //            return JSONString;
        //        }
        //        else
        //        {
        //            SqlParameter[] param = new SqlParameter[]
        //            {
        //                     new SqlParameter("@UserID",UserId),
        //                     new SqlParameter("@TestID",TestId)
        //            };
        //            DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetReportDetailsForGraph_CompireGroup", param);

        //            if (dt.Rows.Count > 1)
        //            {
        //                Result.Status = true;  //  Status Key
        //                Result.Male = dt.Rows[0]["sMale"];
        //                Result.Female = dt.Rows[0]["sFemale"];
        //                Result.Age = dt.Rows[0]["sAge"];
        //                Result.TestName = dt.Rows[0]["sTestName"];
        //                Result.TestCode = dt.Rows[0]["sTestCode"];
        //                Result.Units = dt.Rows[0]["sUnits"];
        //                Result.ReportData = new JArray() as dynamic;   // Create Array for Baner Details
        //                for (int j = 0; j < dt.Rows.Count; j++)
        //                {
        //                    dynamic ObjReportDetail = new JObject();
        //                    ObjReportDetail.Unit = dt.Rows[j]["sUnits"];
        //                    ObjReportDetail.Analyte = dt.Rows[j]["sAnalyte"];
        //                    ObjReportDetail.SubAnalyte = dt.Rows[j]["sSubAnalyte"];
        //                    ObjReportDetail.TestDate = dt.Rows[j]["sTestDate"];
        //                    ObjReportDetail.Value = dt.Rows[j]["sValue"];
        //                    ObjReportDetail.Result = dt.Rows[j]["sResult"];
        //                    Result.ReportData.Add(ObjReportDetail); //Add baner details to array
        //                }

        //                JSONString = JsonConvert.SerializeObject(Result);
        //            }
        //            else
        //            {
        //                Result.Status = false;  //  Status Key
        //                Result.Msg = "No Records found.";
        //                JSONString = JsonConvert.SerializeObject(Result);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Result.Status = false;  //  Status Key
        //        Result.Msg = ex;
        //        JSONString = JsonConvert.SerializeObject(Result);

        //    }
        //    return JSONString;
        //}


        //[Route("ReportDetilsForGraph")]
        //public string ReportDetilsForGraph(int TestId)
        //{
        //    var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
        //    string JSONString = string.Empty;
        //    dynamic Result = new JObject();  //Create root JSON Object
        //    try
        //    {
        //        string Msg = "";
        //        if (!Ival.IsInteger(TestId.ToString()))
        //        {
        //            Msg += "Please Enter Valid Test Id";
        //        }
        //        if (Msg.Length > 0)
        //        {
        //            Result.Status = false;  //  Status Key 
        //            Result.Msg = Msg;
        //            JSONString = JsonConvert.SerializeObject(Result);
        //            return JSONString;
        //        }
        //        else
        //        {
        //            SqlParameter[] param = new SqlParameter[]
        //            {
        //                     new SqlParameter("@UserID",UserId),
        //                     new SqlParameter("@TestID",TestId)
        //            };
        //            DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetReportDetailsForGraph", param);

        //            SqlParameter[] param2 = new SqlParameter[]
        //            {
        //                     new SqlParameter("@UserID",UserId),
        //                     new SqlParameter("@TestID",TestId)
        //            };
        //            DataTable dt2= DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetReportDetailsForGraph_CompireGroup", param2);

        //            if (true)
        //            {

        //            }

        //            if (dt.Rows.Count > 0)
        //            {
        //                Result.Status = true;  //  Status Key
        //                Result.Male = dt.Rows[0]["sMale"];
        //                Result.Female = dt.Rows[0]["sFemale"];
        //                Result.Age = dt.Rows[0]["sAge"];
        //                Result.TestName = dt.Rows[0]["sTestName"];
        //                Result.TestCode = dt.Rows[0]["sTestCode"];
        //                Result.Units = dt.Rows[0]["sUnits"];


        //      //          Result.SubAnalyte = dt2.Rows[0]["sSubAnalyte"];
        //                Result.ReportData = new JArray() as dynamic;   // Create Array for Baner Details

        //                dynamic ObjReportDetail = new JObject();
        //                //added by santosh on 0382021 for required return data in anyalatics group by
        //                for (int i = 0; i < dt2.Rows.Count; i++)
        //                {

        //                    ObjReportDetail.SubAnalyte_header = dt2.Rows[i]["sSubAnalyte"];
        //                    for (int j = 0; j < dt.Rows.Count; j++)
        //                    {
        //                        if (dt2.Rows[i]["sSubAnalyte"].ToString() == dt.Rows[j]["sSubAnalyte"].ToString())
        //                        {
        //                            ObjReportDetail.Unit = dt.Rows[j]["sUnits"];
        //                            ObjReportDetail.Analyte = dt.Rows[j]["sAnalyte"];
        //                            ObjReportDetail.SubAnalyte = dt.Rows[j]["sSubAnalyte"];
        //                            ObjReportDetail.TestDate = dt.Rows[j]["sTestDate"];
        //                            ObjReportDetail.Value = dt.Rows[j]["sValue"];
        //                            ObjReportDetail.Result = dt.Rows[j]["sResult"];
        //                            Result.ReportData.Add(ObjReportDetail); //Add baner details to array
        //                        }




        //                        Result.SubAnalyte_header.Add(ObjReportDetail);

        //                    }


        //                }
        //                JSONString = JsonConvert.SerializeObject(Result);
        //            }
        //            else
        //            {
        //                Result.Status = false;  //  Status Key
        //                Result.Msg = "No Records found.";
        //                JSONString = JsonConvert.SerializeObject(Result);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Result.Status = false;  //  Status Key
        //        Result.Msg = ex;
        //        JSONString = JsonConvert.SerializeObject(Result);

        //    }
        //    return JSONString;
        //}
        [HttpPost]
        [Route("FillReportTestList")]
        public string FillReportTestList([FromBody] PagingParameterModel pagingparametermode)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Createroot JSON Object
            try
            {
                SqlParameter[] param = new SqlParameter[]
                    {
                         new SqlParameter("@SearchingText",pagingparametermode.Searching)
                    };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetFillReportTestList", param);
                if (dt.Rows.Count > 0)
                {
                    // Get's No of Rows Count   
                    int count = dt.Rows.Count;

                    // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
                    int CurrentPage = pagingparametermode.pageNumber;

                    // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
                    int PageSize = pagingparametermode.pageSize;

                    // Display TotalCount to Records to User  
                    int TotalCount = count;

                    // Calculating Totalpage by Dividing (No of Records / Pagesize)  
                    int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

                    // Returns List of Doctor after applying Paging   
                    var items = dt.Select().Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

                    Result.TestList = new JArray() as dynamic;

                    for (int i = 0; i < items.Count; i++)
                    {
                        dynamic ObjTestDetail = new JObject();
                        ObjTestDetail.TestName = items[i]["sTestName"];
                        ObjTestDetail.TestCode = items[i]["sTestCode"];
                        ObjTestDetail.TestId = items[i]["sTestId"];
                        ObjTestDetail.TestUsefulFor = items[i]["sTestUsefulFor"];
                        ObjTestDetail.TestProfileId = items[i]["sTestProfileId"];
                        ObjTestDetail.ProfileName = items[i]["sProfileName"];
                        ObjTestDetail.SectionName = items[i]["sSectionName"];
                        Result.TestList.Add(ObjTestDetail);
                    }
                    Result.Status = true;  //  Status Key
                    Result.Msg = "Success";
                    Result.TotalCount = TotalCount;
                    Result.PageSize = PageSize;
                    Result.CurrentPage = CurrentPage;
                    Result.TotalPages = TotalPages;
                    JSONString = JsonConvert.SerializeObject(Result);
                }
                else
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = "No Record found";
                    JSONString = JsonConvert.SerializeObject(Result);
                }
            }
            catch (Exception e)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        [HttpPost]
        [Route("GetLabListForAppointment")]
        public string GetLabListForAppointment([FromBody] PagingParameterModel pagingparametermodel)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Createroot JSON Object

            try
            {
                SqlParameter[] param = new SqlParameter[]
                    {
                          new SqlParameter("@SearchingText",pagingparametermodel.Searching)
                    };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetAllLabwithSearch", param);
                if (dt.Rows.Count > 0)
                {
                    // Get's No of Rows Count 
                    int count = dt.Rows.Count;

                    // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
                    int CurrentPage = pagingparametermodel.pageNumber;

                    // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
                    int PageSize = pagingparametermodel.pageSize;

                    // Display TotalCount to Records to User  
                    int TotalCount = count;

                    // Calculating Totalpage by Dividing (No of Records / Pagesize)  
                    int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

                    // Returns List of Doctor after applying Paging   
                    var items = dt.Select().Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

                    Result.LabList = new JArray() as dynamic;   // Create Array for Test Details

                    for (int i = 0; i < items.Count; i++)
                    {
                        dynamic ObjLabDetail = new JObject();

                        ObjLabDetail.Labname = items[i]["sLabName"];
                        ObjLabDetail.LabAddress = items[i]["sLabAddress"];
                        ObjLabDetail.LabContact = items[i]["sLabContact"];
                        ObjLabDetail.Labid = items[i]["slabid"];
                        ObjLabDetail.LabLocation = items[i]["sLabLocation"];
                        ObjLabDetail.LabLogo = items[i]["sLabLogo"];
                        ObjLabDetail.LabEmailId = items[i]["sLabEmailId"];
                        ObjLabDetail.LabOnlinePayment = items[i]["OnlinePayment"];
                        ObjLabDetail.LabStatus = items[i]["sLabStatus"];

                        Result.LabList.Add(ObjLabDetail); //Add Test details to array
                    }
                    Result.Status = true;  //  Status Key
                    Result.Msg = "Success";
                    Result.TotalCount = TotalCount;
                    Result.PageSize = PageSize;
                    Result.CurrentPage = CurrentPage;
                    Result.TotalPages = TotalPages;
                    JSONString = JsonConvert.SerializeObject(Result);
                }
                else
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = "No Record found";
                    JSONString = JsonConvert.SerializeObject(Result);
                }
            }
            catch (Exception e)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        [HttpPost]
        [Route("TestListByLabId")]
        public string TestListByLabId([FromBody] LabTestList model)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Createroot JSON Object
            try
            {
                string Msg = "";
                if (!Ival.IsInteger(model.LabId.ToString()))
                {
                    Msg += "Please Enter Valid LabId";
                }
                if (Msg.Length > 0)
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = Msg;
                    JSONString = JsonConvert.SerializeObject(Result);
                    return JSONString;
                }
                else
                {
                    SqlParameter[] param = new SqlParameter[]
                    {
                         new SqlParameter("@LabId",model.LabId),
                         new SqlParameter("@SearchingText",model.Searching)
                    };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetTestListWithLabId", param);
                    if (dt.Rows.Count > 0)
                    {
                        // Get's No of Rows Count   
                        int count = dt.Rows.Count;

                        // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
                        int CurrentPage = model.pageNumber;

                        // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
                        int PageSize = model.pageSize;

                        // Display TotalCount to Records to User  
                        int TotalCount = count;

                        // Calculating Totalpage by Dividing (No of Records / Pagesize)  
                        int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

                        // Returns List of Doctor after applying Paging   
                        var items = dt.Select().Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

                        Result.TestList = new JArray() as dynamic;

                        for (int i = 0; i < items.Count; i++)
                        {
                            dynamic ObjTestDetail = new JObject();
                            ObjTestDetail.TestName = items[i]["sTestName"];
                            ObjTestDetail.TestCode = items[i]["sTestCode"];
                            ObjTestDetail.TestId = items[i]["sTestId"];
                            ObjTestDetail.TestUsefulFor = items[i]["sTestUsefulFor"];
                            ObjTestDetail.TestProfileId = items[i]["sTestProfileId"];
                            ObjTestDetail.ProfileName = items[i]["sProfileName"];
                            ObjTestDetail.SectionName = items[i]["sSectionName"];
                            ObjTestDetail.Price = items[i]["sPrice"];
                            Result.TestList.Add(ObjTestDetail);
                        }
                        Result.Status = true;  //  Status Key
                        Result.Msg = "Success";
                        Result.TotalCount = TotalCount;
                        Result.PageSize = PageSize;
                        Result.CurrentPage = CurrentPage;
                        Result.TotalPages = TotalPages;
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "No Record found";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                }
            }
            catch (Exception e)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        [HttpPost]
        [Route("getReportValuesRefRange")]
        public string getReportValuesRefRange(int BookingId)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (!Ival.IsInteger(BookingId.ToString()))
                {
                    Msg += "Please Enter Valid Booking Id";
                }
                if (Msg.Length > 0)
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = Msg;
                    JSONString = JsonConvert.SerializeObject(Result);
                    return JSONString;
                }
                else
                {
                    SqlParameter[] param = new SqlParameter[]
                    {
                          new SqlParameter("@UserId",UserId),
                          new SqlParameter("@BookingId",BookingId)
                    };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetBookingdetailsformanualpunching", param);

                    if (dt.Rows.Count > 0)

                    {
                        CalculateAge _age = new CalculateAge();
                        Result.AnalyteList = new JArray() as dynamic;
                        string DateOfBirth = dt.Rows[0]["sBirthDate"].ToString();
                        string Currentdate = dt.Rows[0]["sTestDate"].ToString();
                        DateTime Dob;
                        DateTime dtDob;
                        if (DateTime.TryParseExact(DateOfBirth, "dd/MM/yyyy", null, DateTimeStyles.None, out Dob))
                        {
                            dtDob = Dob;
                        }
                        else
                        {
                            dtDob = Convert.ToDateTime(DateOfBirth);
                        }
                        DateTime _CuuretDateOut;
                        DateTime _currentdate;
                        if (DateTime.TryParseExact(Currentdate, "dd/MM/yyyy", null, DateTimeStyles.None, out _CuuretDateOut))
                        {
                            _currentdate = _CuuretDateOut;
                        }
                        else
                        {
                            _currentdate = Convert.ToDateTime(Currentdate);
                        }
                        string patientAge = "";
                        if (_age.CalculateYourAge(dtDob, _currentdate)["Years"] != "0")
                        {
                            patientAge = _age.CalculateYourAge(Dob, _currentdate)["Years"] + " year";
                        }
                        else if (_age.CalculateYourAge(Dob, _currentdate)["Months"] != "0")
                        {
                            patientAge = _age.CalculateYourAge(Dob, _currentdate)["Months"] + " month";
                        }
                        else if (_age.CalculateYourAge(Dob, _currentdate)["Days"] != "0")
                        {
                            patientAge = _age.CalculateYourAge(Dob, _currentdate)["Days"] + " day";
                        }
                        string[] spiltAge = patientAge.Split(' ').ToArray();
                        int PatientAge = Convert.ToInt32(spiltAge[0]);
                        string patientageunit = spiltAge[1];

                        SqlParameter[] param1 = new SqlParameter[]
                             {
                                new SqlParameter("@testId",dt.Rows[0]["sTestId"].ToString()),
                                new SqlParameter("@Gender",dt.Rows[0]["sGender"].ToString())
                            };
                        DataTable dsTestAnalyte = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetTestReferencerange", param1);
                        SqlParameter[] param2 = new SqlParameter[]
                           {
                                new SqlParameter("@testId",dt.Rows[0]["sTestId"].ToString()),
                                new SqlParameter("@Gender",dt.Rows[0]["sGender"].ToString())
                          };
                        DataTable dsTestSubAnalyte = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetTestSubAnalyteReferenceRange", param2);


                        int FromAge;
                        int ToAge;
                        string AgeUnit = "";

                    


                        if (dsTestSubAnalyte.Rows.Count > 0)
                        {
                            List<string> lstSASM = new List<string>();

                            foreach (DataRow row in dsTestSubAnalyte.Rows)
                            {
                                dynamic SubAnalyteObject = new JObject();
                                string sasm = row["TSASMId"].ToString();
                                if (dt.Rows[0]["sGender"].ToString().ToLower() == "male")
                                {
                                    FromAge = Convert.ToInt32(row["MaleFromAge"].ToString());
                                    ToAge = Convert.ToInt32(row["MaleToAge"].ToString());
                                    AgeUnit = row["MaleAgeUnit"].ToString();
                                    string Age = FromAge + "-" + ToAge + " " + AgeUnit;
                                    if ((FromAge == 0 || FromAge == 1) && ToAge == 100 && AgeUnit == "year")
                                    {
                                        if (lstSASM.Contains(sasm) == false)
                                        {
                                            lstSASM.Add(sasm);
                                            SubAnalyteObject.AnalyteName = row["sAnalyteName"].ToString();
                                            SubAnalyteObject.SubAnalyteName = row["sSubAnalyteName"].ToString();
                                            SubAnalyteObject.Specimen = row["sSampleType"].ToString();
                                            SubAnalyteObject.MethodName = row["sMethodName"].ToString();
                                            SubAnalyteObject.ResultType = row["sResultType"].ToString();
                                            SubAnalyteObject.ReferenceType = row["ReferenceType"].ToString();
                                            SubAnalyteObject.AgeGroup = Age;
                                            SubAnalyteObject.MaleRange = row["MaleMinValue"].ToString() + "-" + row["MaleMaxValue"].ToString();
                                            SubAnalyteObject.FemaleRange = "";
                                            SubAnalyteObject.Grade = row["Grade"].ToString();
                                            SubAnalyteObject.Unit = row["Unit"].ToString();
                                            SubAnalyteObject.Interpretation = row["Interpretation"].ToString();
                                            SubAnalyteObject.UpperLimit = row["UpperLimit"].ToString();
                                            SubAnalyteObject.LowerLimit = row["LowerLimit"].ToString();

                                            SubAnalyteObject.InterpretationList = new JArray() as dynamic;

                                            // DataTable dtinterpretation = DAL.GetDataTable("WS_Sp_TestInterpretation " + row["sTestCode"].ToString().Trim());
                                            SqlParameter[] paramTest = new SqlParameter[]
                                             {
                                                  new SqlParameter("@TSASMId",sasm)
                                             };
                                            DataTable dtinterpretation = DAL.ExecuteStoredProcedureDataTable("Sp_GetsTestSubAnalyteInterpretation ", paramTest);
                                            if (dtinterpretation.Rows.Count > 0)
                                            {
                                                foreach (DataRow _rowint in dtinterpretation.Rows)
                                                {
                                                    dynamic ObjInterPretation = new JObject();
                                                    ObjInterPretation.Result = _rowint["Interpretation"].ToString();
                                                    ObjInterPretation.MaleRange = _rowint["maleRange"].ToString();
                                                    ObjInterPretation.FemaleRange = "";
                                                    SubAnalyteObject.InterpretationList.Add(ObjInterPretation);
                                                }
                                            }
                                            Result.AnalyteList.Add(SubAnalyteObject);
                                        }
                                    }
                                    else
                                    {
                                        if (FromAge <= PatientAge && ToAge >= PatientAge && AgeUnit == patientageunit)
                                        {
                                            if (lstSASM.Contains(sasm) == false)
                                            {
                                                lstSASM.Add(sasm);
                                                SubAnalyteObject.AnalyteName = row["sAnalyteName"].ToString();
                                                SubAnalyteObject.SubAnalyteName = row["sSubAnalyteName"].ToString();
                                                SubAnalyteObject.Specimen = row["sSampleType"].ToString();
                                                SubAnalyteObject.MethodName = row["sMethodName"].ToString();
                                                SubAnalyteObject.ResultType = row["sResultType"].ToString();
                                                SubAnalyteObject.ReferenceType = row["ReferenceType"].ToString();
                                                SubAnalyteObject.AgeGroup = Age;
                                                SubAnalyteObject.MaleRange = row["MaleMinValue"].ToString() + "-" + row["MaleMaxValue"].ToString();
                                                SubAnalyteObject.FemaleRange = "";
                                                SubAnalyteObject.Grade = row["Grade"].ToString();
                                                SubAnalyteObject.Unit = row["Unit"].ToString();
                                                SubAnalyteObject.Interpretation = row["Interpretation"].ToString();
                                                SubAnalyteObject.UpperLimit = row["UpperLimit"].ToString();
                                                SubAnalyteObject.LowerLimit = row["LowerLimit"].ToString();

                                                SubAnalyteObject.InterpretationList = new JArray() as dynamic;

                                                SqlParameter[] paramTest = new SqlParameter[]
                                           {
                                                  new SqlParameter("@TSASMId",sasm)
                                           };
                                                DataTable dtinterpretation = DAL.ExecuteStoredProcedureDataTable("Sp_GetsTestSubAnalyteInterpretation ", paramTest);
                                                if (dtinterpretation.Rows.Count > 0)
                                                {
                                                    foreach (DataRow _rowint in dtinterpretation.Rows)
                                                    {
                                                        dynamic ObjInterPretation = new JObject();
                                                        ObjInterPretation.Result = _rowint["Interpretation"].ToString();
                                                        ObjInterPretation.MaleRange = _rowint["maleRange"].ToString();
                                                        ObjInterPretation.FemaleRange = "";
                                                        SubAnalyteObject.InterpretationList.Add(ObjInterPretation);
                                                    }
                                                }
                                                Result.AnalyteList.Add(SubAnalyteObject);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    FromAge = Convert.ToInt32(row["FemaleFromAge"].ToString());
                                    ToAge = Convert.ToInt32(row["FemaleToAge"].ToString());
                                    AgeUnit = row["FemaleAgeUnit"].ToString();
                                    string Age = FromAge + "-" + ToAge + " " + AgeUnit;
                                    if ((FromAge == 0 || FromAge == 1) && ToAge == 100 && AgeUnit == "year")
                                    {
                                        if (lstSASM.Contains(sasm) == false)
                                        {
                                            lstSASM.Add(sasm);
                                            SubAnalyteObject.AnalyteName = row["sAnalyteName"].ToString();
                                            SubAnalyteObject.SubAnalyteName = row["sSubAnalyteName"].ToString();
                                            SubAnalyteObject.Specimen = row["sSampleType"].ToString();
                                            SubAnalyteObject.MethodName = row["sMethodName"].ToString();
                                            SubAnalyteObject.ResultType = row["sResultType"].ToString();
                                            SubAnalyteObject.ReferenceType = row["ReferenceType"].ToString();
                                            SubAnalyteObject.AgeGroup = Age;
                                            SubAnalyteObject.MaleRange = "";
                                            SubAnalyteObject.FemaleRange = row["FemaleMinValue"].ToString() + "-" + row["FemaleMaxValue"].ToString();
                                            SubAnalyteObject.Grade = row["Grade"].ToString();
                                            SubAnalyteObject.Unit = row["Unit"].ToString();
                                            SubAnalyteObject.Interpretation = row["Interpretation"].ToString();
                                            SubAnalyteObject.UpperLimit = row["UpperLimit"].ToString();
                                            SubAnalyteObject.LowerLimit = row["LowerLimit"].ToString();

                                            SubAnalyteObject.InterpretationList = new JArray() as dynamic;

                                            SqlParameter[] paramTest = new SqlParameter[]
                                                 {
                                                  new SqlParameter("@TSASMId",sasm)
                                                 };
                                            DataTable dtinterpretation = DAL.ExecuteStoredProcedureDataTable("Sp_GetsTestSubAnalyteInterpretation ", paramTest);
                                            if (dtinterpretation.Rows.Count > 0)
                                            {
                                                foreach (DataRow _rowint in dtinterpretation.Rows)
                                                {
                                                    dynamic ObjInterPretation = new JObject();
                                                    ObjInterPretation.Result = _rowint["Interpretation"].ToString();
                                                    ObjInterPretation.FemaleRange = _rowint["femaleRange"].ToString();
                                                    ObjInterPretation.MaleRange = "";
                                                    SubAnalyteObject.InterpretationList.Add(ObjInterPretation);
                                                }
                                            }
                                            Result.AnalyteList.Add(SubAnalyteObject);
                                        }
                                    }
                                    else
                                    {
                                        if (FromAge <= PatientAge && ToAge >= PatientAge && AgeUnit == patientageunit)
                                        {
                                            if (lstSASM.Contains(sasm) == false)
                                            {
                                                lstSASM.Add(sasm);
                                                SubAnalyteObject.AnalyteName = row["sAnalyteName"].ToString();
                                                SubAnalyteObject.SubAnalyteName = row["sSubAnalyteName"].ToString();
                                                SubAnalyteObject.Specimen = row["sSampleType"].ToString();
                                                SubAnalyteObject.MethodName = row["sMethodName"].ToString();
                                                SubAnalyteObject.ResultType = row["sResultType"].ToString();
                                                SubAnalyteObject.ReferenceType = row["ReferenceType"].ToString();
                                                SubAnalyteObject.AgeGroup = Age;
                                                SubAnalyteObject.MaleRange = "";
                                                SubAnalyteObject.FemaleRange = row["FemaleMinValue"].ToString() + "-" + row["FemaleMaxValue"].ToString();
                                                SubAnalyteObject.Grade = row["Grade"].ToString();
                                                SubAnalyteObject.Unit = row["Unit"].ToString();
                                                SubAnalyteObject.Interpretation = row["Interpretation"].ToString();
                                                SubAnalyteObject.UpperLimit = row["UpperLimit"].ToString();
                                                SubAnalyteObject.LowerLimit = row["LowerLimit"].ToString();

                                                SubAnalyteObject.InterpretationList = new JArray() as dynamic;

                                                SqlParameter[] paramTest = new SqlParameter[]
                                              {
                                                  new SqlParameter("@TSASMId",sasm)
                                              };
                                                DataTable dtinterpretation = DAL.ExecuteStoredProcedureDataTable("Sp_GetsTestSubAnalyteInterpretation ", paramTest);
                                                if (dtinterpretation.Rows.Count > 0)
                                                {
                                                    foreach (DataRow _rowint in dtinterpretation.Rows)
                                                    {
                                                        dynamic ObjInterPretation = new JObject();
                                                        ObjInterPretation.Result = _rowint["Interpretation"].ToString();
                                                        ObjInterPretation.FemaleRange = _rowint["femaleRange"].ToString();
                                                        ObjInterPretation.MaleRange = "";
                                                        SubAnalyteObject.InterpretationList.Add(ObjInterPretation);
                                                    }
                                                }
                                                Result.AnalyteList.Add(SubAnalyteObject);

                                            }
                                        }
                                    }
                                }
                            }
                        }
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Success";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "No Record found";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    return JSONString;
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);
                return JSONString;
            }
        }

        [HttpPost]
        [Route("AddOldReport")]
        public string AddOldReport([FromBody] AddOldReport model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final 

            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                var _analyteList = model.ParameterDetails;

                
                OldReportParameterList _createReport = new OldReportParameterList();
                int _count = _analyteList.Count;

                if (_count==0)
                {
                    Msg += "Please Enter Valid  Parameter";
                }


                if (Ival.IsTextBoxEmpty(model.TestName))
                {
                    Msg += "Please Enter Valid Test Name";
                }
                if (Ival.IsValidDateForDateFilteration(model.TestDate))
                {
                    Msg += "Please Enter Valid Test Date";
                }

                if (Ival.IsValidDate(model.ReportPath))
                {
                    Msg += "Please Enter Valid Report Path";
                }
                if (Ival.IsTextBoxEmpty(model.LabName))
                {
                    Msg += "Please Enter Valid Lab Name";
                }

                if (Msg.Length > 0)
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = Msg;
                    JSONString = JsonConvert.SerializeObject(Result);
                    return JSONString;
                }
                else
                {
                    SqlParameter[] param = new SqlParameter[]
                            {
                            new SqlParameter("@UserId",UserId),
                            new SqlParameter("@TestName",model.TestName),
                            new SqlParameter("@TestDate",model.TestDate),
                            new SqlParameter("@Notes",model.Notes),
                            new SqlParameter("@RefDocName",model.RefDoctor),
                            new SqlParameter("@ReportPath",model.ReportPath),
                            new SqlParameter("@LabName",model.LabName),
                            new SqlParameter("@Returnval",SqlDbType.Int)
                            };
                    int data = DAL.ExecuteStoredProcedureRetnInt("Sp_AddOldTsetReport", param);

                    if (data >= 1)
                    {
                        for (int i = 0; i < _count; i++)
                        {
                            _createReport = _analyteList[i];
                            if (Ival.IsTextBoxEmpty(_createReport.ParameterName))
                            {
                                Msg += "Please Enter Valid Parameter Name";
                            }
                            if (Ival.IsTextBoxEmpty(_createReport.Result))
                            {
                                Msg += "Please Enter Valid Result";  
                            }

                            if (Msg.Length > 0)
                            {
                                Result.Status = false;  //  Status Key 
                                Result.Msg = Msg;
                                JSONString = JsonConvert.SerializeObject(Result);
                                return JSONString;
                            }
                            else
                            {


                                string _value = _createReport.Value != "" ? CryptoHelper.Encrypt(_createReport.Value) : "";
                                string _result = _createReport.Value != "" ? CryptoHelper.Encrypt(_createReport.Result) : "";

                                SqlParameter[] param1 = new SqlParameter[]
                                {
                                    new SqlParameter("@UserId",UserId),
                                    new SqlParameter("@ReportId",data),
                                    new SqlParameter("@ParameterName",_createReport.ParameterName),
                                    new SqlParameter("@Value",_value),
                                    new SqlParameter("@Result",_result),
                                    new SqlParameter("@MinRange",_createReport.MinRange),
                                    new SqlParameter("@MaxRange",_createReport.MaxRange),
                                    new SqlParameter("@Unit",_createReport.Unit),
                                    new SqlParameter("@Returnval",SqlDbType.Int)
                                };
                                int result = DAL.ExecuteStoredProcedureRetnInt("Sp_AddOldTestReportValues", param1);
                                if (result == 1)
                                {
                                    Result.Status = true;  //  Status Key 
                                    Result.Msg = "Report added successfully.";
                                    JSONString = JsonConvert.SerializeObject(Result);
                                }
                                else
                                {
                                    Result.Status = false;  //  Status Key 
                                    Result.Msg = "Something went wrong,Please try again.";
                                    JSONString = JsonConvert.SerializeObject(Result);
                                }
                            }
                        }
                    }
                    else
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "Something went wrong,Please try again.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        [HttpPost]
        [Route("GetOldReportlist")]
        public string GetOldReportlist([FromBody] PagingParameterModel pagingparametermodel)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Createroot JSON Object

            try
            {
                SqlParameter[] param = new SqlParameter[]
                    {
                         new SqlParameter("@UserId",UserId),
                         new SqlParameter("@SearchingText",pagingparametermodel.Searching)
                    };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetOldReportlist", param);
                if (dt.Rows.Count > 0)
                {
                    // Get's No of Rows Count 
                    int count = dt.Rows.Count;

                    // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
                    int CurrentPage = pagingparametermodel.pageNumber;

                    // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
                    int PageSize = pagingparametermodel.pageSize;

                    // Display TotalCount to Records to User  
                    int TotalCount = count;

                    // Calculating Totalpage by Dividing (No of Records / Pagesize)  
                    int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

                    // Returns List of Doctor after applying Paging   
                    var items = dt.Select().Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

                    Result.LabList = new JArray() as dynamic;   // Create Array for Test Details

                    for (int i = 0; i < items.Count; i++)
                    {
                        dynamic ObjLabDetail = new JObject();
                        ObjLabDetail.Flag = "OldReport";
                        ObjLabDetail.ReportId = items[i]["ID"];
                        ObjLabDetail.TestName = items[i]["TestName"];
                        ObjLabDetail.TestDate = items[i]["TestDate"];
                        ObjLabDetail.RefDoctorName = items[i]["RefDoctorName"];
                        ObjLabDetail.Notes = items[i]["Notes"];
                        ObjLabDetail.CreatedDate = items[i]["CreatedDate"];
                        ObjLabDetail.FilePath = items[i]["FilePath"];
                        ObjLabDetail.LabName = items[i]["LabName"];
                        Result.LabList.Add(ObjLabDetail); //Add Test details to array
                    }
                    Result.Status = true;  //  Status Key
                    Result.Msg = "Success";
                    Result.TotalCount = TotalCount;
                    Result.PageSize = PageSize;
                    Result.CurrentPage = CurrentPage;
                    Result.TotalPages = TotalPages;
                    JSONString = JsonConvert.SerializeObject(Result);
                }
                else
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = "No Record found";
                    JSONString = JsonConvert.SerializeObject(Result);
                }
            }
            catch (Exception e)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        [HttpGet]
        [Route("MyOldReportDetails/{ReportId}")]
        public string MyOldReportDetails(int ReportId)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Createroot JSON Object
            try
            {
                string Msg = "";
                if (!Ival.IsInteger(ReportId.ToString()))
                {
                    Msg += "Please Enter Valid Report Id";
                }
                if (Msg.Length > 0)
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = Msg;
                    JSONString = JsonConvert.SerializeObject(Result);
                    return JSONString;
                }
                else
                {
                    var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
                    SqlParameter[] param = new SqlParameter[]
                           {
                                    new SqlParameter("@UserId",UserId),
                                    new SqlParameter("@ReportId",ReportId)
                           };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetOldReportDetails", param);

                    if (dt.Rows.Count > 0)
                    {

                        Result.ReportData = new JArray() as dynamic;   // Create Array for Baner Details
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            dynamic ObjReportDetail = new JObject();
                            ObjReportDetail.ID = dt.Rows[j]["ID"];
                            ObjReportDetail.ReportId = dt.Rows[j]["ReportId"];
                            ObjReportDetail.ParameterName = dt.Rows[j]["ParameterName"];
                            ObjReportDetail.Value = dt.Rows[j]["Value"];
                            ObjReportDetail.Result = dt.Rows[j]["Result"];
                            ObjReportDetail.MinRange = dt.Rows[j]["MinRange"];
                            ObjReportDetail.MaxRange = dt.Rows[j]["MaxRange"];
                            ObjReportDetail.Unit = dt.Rows[j]["Unit"];
                            ObjReportDetail.CreatedDate = dt.Rows[j]["CreatedDate"];
                            Result.ReportData.Add(ObjReportDetail); //Add baner details to array
                        }

                        Result.DocNote = new JArray() as dynamic;
                        SqlParameter[] paramdoc = new SqlParameter[]
                       {
                             new SqlParameter("@ReportID",ReportId),
                             new SqlParameter("@UserId",UserId)
                       };
                        DataTable dtDoc = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetNoteListsBySharedOldReport ", paramdoc);
                        if (dtDoc.Rows.Count > 0)
                        {
                            for (int j = 0; j < dtDoc.Rows.Count; j++)
                            {
                                dynamic ObjTestDetail = new JObject();
                                ObjTestDetail.Createddate = dtDoc.Rows[j]["ModifiedDate"];
                                ObjTestDetail.DoctorComment = dtDoc.Rows[j]["DoctorComment"];
                                Result.DocNote.Add(ObjTestDetail); //Add Doctor details to array
                            }
                        }

                        Result.Status = true;  //  Status Key
                        Result.TestName = dt.Rows[0]["TestName"];
                        Result.LabName = dt.Rows[0]["LabName"];
                        Result.TestDate = dt.Rows[0]["TestDate"];
                        Result.RefDoctorName = dt.Rows[0]["RefDoctorName"];
                        Result.Notes = dt.Rows[0]["Notes"];
                        Result.FilePath = dt.Rows[0]["FilePath"];
                        Result.Msg = "Success";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "No Record found";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                }
            }
            catch (Exception e)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        [HttpPost]
        [Route("GetReportValuesUpdated")]
        public string GetReportValuesUpdated(int BookingId)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (!Ival.IsInteger(BookingId.ToString()))
                {
                    Msg += "Please Enter Valid Booking Id";
                }
                if (Msg.Length > 0)
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = Msg;
                    JSONString = JsonConvert.SerializeObject(Result);
                    return JSONString;
                }
                else
                {
                    SqlParameter[] param = new SqlParameter[]
                    {
                          new SqlParameter("@UserId",UserId),
                          new SqlParameter("@BookingId",BookingId)
                    };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetBookingdetailsformanualpunching", param);

                    if (dt.Rows.Count > 0)

                    {
                        CalculateAge _age = new CalculateAge();
                        Result.AnalyteList = new JArray() as dynamic;
                        string DateOfBirth = dt.Rows[0]["sBirthDate"].ToString();
                        string Currentdate = dt.Rows[0]["sTestDate"].ToString();
                        DateTime Dob;
                        DateTime dtDob;
                        if (DateTime.TryParseExact(DateOfBirth, "dd/MM/yyyy", null, DateTimeStyles.None, out Dob))
                        {
                            dtDob = Dob;
                        }
                        else
                        {
                            dtDob = Convert.ToDateTime(DateOfBirth);
                        }
                        DateTime _CuuretDateOut;
                        DateTime _currentdate;
                        if (DateTime.TryParseExact(Currentdate, "dd/MM/yyyy", null, DateTimeStyles.None, out _CuuretDateOut))
                        {
                            _currentdate = _CuuretDateOut;
                        }
                        else
                        {
                            _currentdate = Convert.ToDateTime(Currentdate);
                        }
                        string patientAge = "";
                        if (_age.CalculateYourAge(dtDob, _currentdate)["Years"] != "0")
                        {
                            patientAge = _age.CalculateYourAge(Dob, _currentdate)["Years"] + " year";
                        }
                        else if (_age.CalculateYourAge(Dob, _currentdate)["Months"] != "0")
                        {
                            patientAge = _age.CalculateYourAge(Dob, _currentdate)["Months"] + " month";
                        }
                        else if (_age.CalculateYourAge(Dob, _currentdate)["Days"] != "0")
                        {
                            patientAge = _age.CalculateYourAge(Dob, _currentdate)["Days"] + " day";
                        }
                        string[] spiltAge = patientAge.Split(' ').ToArray();
                        int PatientAge = Convert.ToInt32(spiltAge[0]);
                        string patientageunit = spiltAge[1];

                        SqlParameter[] param1 = new SqlParameter[]
                             {
                                new SqlParameter("@testId",dt.Rows[0]["sTestId"].ToString()),
                                new SqlParameter("@Gender",dt.Rows[0]["sGender"].ToString())
                            };
                        DataTable dsTestAnalyte = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetTestReferencerange", param1);
                        SqlParameter[] param2 = new SqlParameter[]
                           {
                                new SqlParameter("@testId",dt.Rows[0]["sTestId"].ToString()),
                                new SqlParameter("@Gender",dt.Rows[0]["sGender"].ToString())
                          };
                        DataTable dsTestSubAnalyte = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetTestSubAnalyteReferenceRange", param2);


                        int FromAge;
                        int ToAge;
                        string AgeUnit = "";

                        if (dsTestAnalyte.Rows.Count > 0)
                        {
                            List<string> lstASM = new List<string>();
                            foreach (DataRow row in dsTestAnalyte.Rows)
                            {
                                dynamic AnalyteObject = new JObject();
                                string asm = row["TASMId"].ToString();
                                if (dt.Rows[0]["sGender"].ToString().ToLower() == "male")
                                {
                                    FromAge = Convert.ToInt32(row["MaleFromAge"].ToString());
                                    ToAge = Convert.ToInt32(row["MaleToAge"].ToString());
                                    AgeUnit = row["MaleAgeUnit"].ToString();
                                    string Age = FromAge + "-" + ToAge + " " + AgeUnit;

                                    if ((FromAge == 0 || FromAge == 1) && ToAge == 100 && AgeUnit == "year")
                                    {
                                        if (lstASM.Contains(asm) == false)
                                        {
                                            lstASM.Add(asm);
                                            AnalyteObject.AnalyteName = row["sAnalyteName"].ToString();
                                            AnalyteObject.SubAnalyteName = "--";
                                            AnalyteObject.Specimen = row["sSampleType"].ToString();
                                            AnalyteObject.MethodName = row["sMethodName"].ToString();
                                            AnalyteObject.ResultType = row["sResultType"].ToString();
                                            AnalyteObject.ReferenceType = row["ReferenceType"].ToString();
                                            AnalyteObject.AgeGroup = Age;
                                            AnalyteObject.MaleRange = row["MaleMinValue"].ToString() + "-" + row["MaleMaxValue"].ToString();
                                            AnalyteObject.FemaleRange = "";
                                            AnalyteObject.Grade = row["Grade"].ToString();
                                            AnalyteObject.Unit = row["Unit"].ToString();
                                            AnalyteObject.Interpretation = row["Interpretation"].ToString();
                                            AnalyteObject.UpperLimit = row["UpperLimit"].ToString();
                                            AnalyteObject.LowerLimit = row["LowerLimit"].ToString();

                                            AnalyteObject.InterpretationList = new JArray() as dynamic;

                                            //  DataTable dtinterpretation = DAL.GetDataTable("WS_Sp_TestInterpretation " + row["sTestCode"].ToString().Trim());

                                            SqlParameter[] paramTest = new SqlParameter[]
                                             {
                                                  new SqlParameter("@TASMId",asm)
                                             };
                                            DataTable dtinterpretation = DAL.ExecuteStoredProcedureDataTable("Sp_GetsTestAnalyteInterpretation ", paramTest);

                                            if (dtinterpretation.Rows.Count > 0)
                                            {
                                                foreach (DataRow _rowint in dtinterpretation.Rows)
                                                {
                                                    dynamic ObjInterPretation = new JObject();
                                                    ObjInterPretation.Result = _rowint["Interpretation"].ToString();
                                                    AnalyteObject.InterpretationList.Add(ObjInterPretation);
                                                }
                                            }
                                            Result.AnalyteList.Add(AnalyteObject);
                                        }

                                    }
                                    else
                                    {
                                        if (FromAge <= PatientAge && ToAge >= PatientAge && AgeUnit == patientageunit)
                                        {
                                            if (lstASM.Contains(asm) == false)
                                            {
                                                lstASM.Add(asm);
                                                AnalyteObject.AnalyteName = row["sAnalyteName"].ToString();
                                                AnalyteObject.SubAnalyteName = "--";
                                                AnalyteObject.Specimen = row["sSampleType"].ToString();
                                                AnalyteObject.MethodName = row["sMethodName"].ToString();
                                                AnalyteObject.ResultType = row["sResultType"].ToString();
                                                AnalyteObject.ReferenceType = row["ReferenceType"].ToString();
                                                AnalyteObject.AgeGroup = Age;
                                                AnalyteObject.MaleRange = row["MaleMinValue"].ToString() + "-" + row["MaleMaxValue"].ToString();
                                                AnalyteObject.FemaleRange = "";
                                                AnalyteObject.Grade = row["Grade"].ToString();
                                                AnalyteObject.Unit = row["Unit"].ToString();
                                                AnalyteObject.Interpretation = row["Interpretation"].ToString();
                                                AnalyteObject.UpperLimit = row["UpperLimit"].ToString();
                                                AnalyteObject.LowerLimit = row["LowerLimit"].ToString();

                                                AnalyteObject.InterpretationList = new JArray() as dynamic;

                                                //  DataTable dtinterpretation = DAL.GetDataTable("WS_Sp_TestInterpretation " + row["sTestCode"].ToString().Trim());
                                                SqlParameter[] paramTest = new SqlParameter[]
                                             {
                                                  new SqlParameter("@TASMId",asm)
                                             };
                                                DataTable dtinterpretation = DAL.ExecuteStoredProcedureDataTable("Sp_GetsTestAnalyteInterpretation ", paramTest);
                                                if (dtinterpretation.Rows.Count > 0)
                                                {
                                                    foreach (DataRow _rowint in dtinterpretation.Rows)
                                                    {
                                                        dynamic ObjInterPretation = new JObject();
                                                        ObjInterPretation.Result = _rowint["Interpretation"].ToString();
                                                        AnalyteObject.InterpretationList.Add(ObjInterPretation);
                                                    }
                                                }
                                                Result.AnalyteList.Add(AnalyteObject);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    FromAge = Convert.ToInt32(row["FemaleFromAge"].ToString());
                                    ToAge = Convert.ToInt32(row["FemaleToAge"].ToString());
                                    AgeUnit = row["FemaleAgeUnit"].ToString();
                                    string Age = FromAge + "-" + ToAge + " " + AgeUnit;
                                    if ((FromAge == 0 || FromAge == 1) && ToAge == 100 && AgeUnit == "year")
                                    {
                                        if (lstASM.Contains(asm) == false)
                                        {
                                            lstASM.Add(asm);
                                            AnalyteObject.AnalyteName = row["sAnalyteName"].ToString();
                                            AnalyteObject.SubAnalyteName = "--";
                                            AnalyteObject.Specimen = row["sSampleType"].ToString();
                                            AnalyteObject.MethodName = row["sMethodName"].ToString();
                                            AnalyteObject.ResultType = row["sResultType"].ToString();
                                            AnalyteObject.ReferenceType = row["ReferenceType"].ToString();
                                            AnalyteObject.AgeGroup = Age;
                                            AnalyteObject.MaleRange = "";
                                            AnalyteObject.FemaleRange = row["FemaleMinValue"].ToString() + "-" + row["FemaleMaxValue"].ToString();
                                            AnalyteObject.Grade = row["Grade"].ToString();
                                            AnalyteObject.Unit = row["Unit"].ToString();
                                            AnalyteObject.Interpretation = row["Interpretation"].ToString();
                                            AnalyteObject.UpperLimit = row["UpperLimit"].ToString();
                                            AnalyteObject.LowerLimit = row["LowerLimit"].ToString();

                                            AnalyteObject.InterpretationList = new JArray() as dynamic;

                                            SqlParameter[] paramTest = new SqlParameter[]
                                            {
                                                  new SqlParameter("@TASMId",asm)
                                            };
                                            DataTable dtinterpretation = DAL.ExecuteStoredProcedureDataTable("Sp_GetsTestAnalyteInterpretation ", paramTest);

                                            if (dtinterpretation.Rows.Count > 0)
                                            {
                                                foreach (DataRow _rowint in dtinterpretation.Rows)
                                                {
                                                    dynamic ObjInterPretation = new JObject();
                                                    ObjInterPretation.Result = _rowint["Interpretation"].ToString();
                                                    AnalyteObject.InterpretationList.Add(ObjInterPretation);
                                                }
                                            }
                                            Result.AnalyteList.Add(AnalyteObject);
                                        }
                                    }
                                    else
                                    {
                                        if (FromAge <= PatientAge && ToAge >= PatientAge && AgeUnit == patientageunit)
                                        {
                                            if (lstASM.Contains(asm) == false)
                                            {
                                                lstASM.Add(asm);
                                                AnalyteObject.AnalyteName = row["sAnalyteName"].ToString();
                                                AnalyteObject.SubAnalyteName = "--";
                                                AnalyteObject.Specimen = row["sSampleType"].ToString();
                                                AnalyteObject.MethodName = row["sMethodName"].ToString();
                                                AnalyteObject.ResultType = row["sResultType"].ToString();
                                                AnalyteObject.ReferenceType = row["ReferenceType"].ToString();
                                                AnalyteObject.AgeGroup = Age;
                                                AnalyteObject.MaleRange = "";
                                                AnalyteObject.FemaleRange = row["FemaleMinValue"].ToString() + "-" + row["FemaleMaxValue"].ToString();
                                                AnalyteObject.Grade = row["Grade"].ToString();
                                                AnalyteObject.Unit = row["Unit"].ToString();
                                                AnalyteObject.Interpretation = row["Interpretation"].ToString();
                                                AnalyteObject.UpperLimit = row["UpperLimit"].ToString();
                                                AnalyteObject.LowerLimit = row["LowerLimit"].ToString();

                                                AnalyteObject.InterpretationList = new JArray() as dynamic;
                                                SqlParameter[] paramTest = new SqlParameter[]
                                                                                            {
                                                  new SqlParameter("@TASMId",asm)
                                                                                            };
                                                DataTable dtinterpretation = DAL.ExecuteStoredProcedureDataTable("Sp_GetsTestAnalyteInterpretation ", paramTest);

                                                if (dtinterpretation.Rows.Count > 0)
                                                {
                                                    foreach (DataRow _rowint in dtinterpretation.Rows)
                                                    {
                                                        dynamic ObjInterPretation = new JObject();
                                                        ObjInterPretation.Result = _rowint["Interpretation"].ToString();
                                                        AnalyteObject.InterpretationList.Add(ObjInterPretation);
                                                    }
                                                }
                                                Result.AnalyteList.Add(AnalyteObject);
                                            }
                                        }
                                    }

                                }
                            }
                        }


                        if (dsTestSubAnalyte.Rows.Count > 0)
                        {
                            List<string> lstSASM = new List<string>();

                            foreach (DataRow row in dsTestSubAnalyte.Rows)
                            {
                                dynamic SubAnalyteObject = new JObject();
                                string sasm = row["TSASMId"].ToString();
                                if (dt.Rows[0]["sGender"].ToString().ToLower() == "male")
                                {
                                    FromAge = Convert.ToInt32(row["MaleFromAge"].ToString());
                                    ToAge = Convert.ToInt32(row["MaleToAge"].ToString());
                                    AgeUnit = row["MaleAgeUnit"].ToString();
                                    string Age = FromAge + "-" + ToAge + " " + AgeUnit;
                                    if ((FromAge == 0 || FromAge == 1) && ToAge == 100 && AgeUnit == "year")
                                    {
                                        if (lstSASM.Contains(sasm) == false)
                                        {
                                            lstSASM.Add(sasm);
                                            SubAnalyteObject.AnalyteName = row["sAnalyteName"].ToString();
                                            SubAnalyteObject.SubAnalyteName = row["sSubAnalyteName"].ToString();
                                            SubAnalyteObject.Specimen = row["sSampleType"].ToString();
                                            SubAnalyteObject.MethodName = row["sMethodName"].ToString();
                                            SubAnalyteObject.ResultType = row["sResultType"].ToString();
                                            SubAnalyteObject.ReferenceType = row["ReferenceType"].ToString();
                                            SubAnalyteObject.AgeGroup = Age;
                                            SubAnalyteObject.MaleRange = row["MaleMinValue"].ToString() + "-" + row["MaleMaxValue"].ToString();
                                            SubAnalyteObject.FemaleRange = "";
                                            SubAnalyteObject.Grade = row["Grade"].ToString();
                                            SubAnalyteObject.Unit = row["Unit"].ToString();
                                            SubAnalyteObject.Interpretation = row["Interpretation"].ToString();
                                            SubAnalyteObject.UpperLimit = row["UpperLimit"].ToString();
                                            SubAnalyteObject.LowerLimit = row["LowerLimit"].ToString();

                                            SubAnalyteObject.InterpretationList = new JArray() as dynamic;

                                            // DataTable dtinterpretation = DAL.GetDataTable("WS_Sp_TestInterpretation " + row["sTestCode"].ToString().Trim());
                                            SqlParameter[] paramTest = new SqlParameter[]
                                             {
                                                  new SqlParameter("@TSASMId",sasm)
                                             };
                                            DataTable dtinterpretation = DAL.ExecuteStoredProcedureDataTable("Sp_GetsTestSubAnalyteInterpretation ", paramTest);
                                            if (dtinterpretation.Rows.Count > 0)
                                            {
                                                foreach (DataRow _rowint in dtinterpretation.Rows)
                                                {
                                                    dynamic ObjInterPretation = new JObject();
                                                    ObjInterPretation.Result = _rowint["Interpretation"].ToString();
                                                    SubAnalyteObject.InterpretationList.Add(ObjInterPretation);
                                                }
                                            }
                                            Result.AnalyteList.Add(SubAnalyteObject);
                                        }
                                    }
                                    else
                                    {
                                        if (FromAge <= PatientAge && ToAge >= PatientAge && AgeUnit == patientageunit)
                                        {
                                            if (lstSASM.Contains(sasm) == false)
                                            {
                                                lstSASM.Add(sasm);
                                                SubAnalyteObject.AnalyteName = row["sAnalyteName"].ToString();
                                                SubAnalyteObject.SubAnalyteName = row["sSubAnalyteName"].ToString();
                                                SubAnalyteObject.Specimen = row["sSampleType"].ToString();
                                                SubAnalyteObject.MethodName = row["sMethodName"].ToString();
                                                SubAnalyteObject.ResultType = row["sResultType"].ToString();
                                                SubAnalyteObject.ReferenceType = row["ReferenceType"].ToString();
                                                SubAnalyteObject.AgeGroup = Age;
                                                SubAnalyteObject.MaleRange = row["MaleMinValue"].ToString() + "-" + row["MaleMaxValue"].ToString();
                                                SubAnalyteObject.FemaleRange = "";
                                                SubAnalyteObject.Grade = row["Grade"].ToString();
                                                SubAnalyteObject.Unit = row["Unit"].ToString();
                                                SubAnalyteObject.Interpretation = row["Interpretation"].ToString();
                                                SubAnalyteObject.UpperLimit = row["UpperLimit"].ToString();
                                                SubAnalyteObject.LowerLimit = row["LowerLimit"].ToString();

                                                SubAnalyteObject.InterpretationList = new JArray() as dynamic;

                                                SqlParameter[] paramTest = new SqlParameter[]
                                           {
                                                  new SqlParameter("@TSASMId",sasm)
                                           };
                                                DataTable dtinterpretation = DAL.ExecuteStoredProcedureDataTable("Sp_GetsTestSubAnalyteInterpretation ", paramTest);
                                                if (dtinterpretation.Rows.Count > 0)
                                                {
                                                    foreach (DataRow _rowint in dtinterpretation.Rows)
                                                    {
                                                        dynamic ObjInterPretation = new JObject();
                                                        ObjInterPretation.Result = _rowint["Interpretation"].ToString();
                                                        SubAnalyteObject.InterpretationList.Add(ObjInterPretation);
                                                    }
                                                }
                                                Result.AnalyteList.Add(SubAnalyteObject);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    FromAge = Convert.ToInt32(row["FemaleFromAge"].ToString());
                                    ToAge = Convert.ToInt32(row["FemaleToAge"].ToString());
                                    AgeUnit = row["FemaleAgeUnit"].ToString();
                                    string Age = FromAge + "-" + ToAge + " " + AgeUnit;
                                    if ((FromAge == 0 || FromAge == 1) && ToAge == 100 && AgeUnit == "year")
                                    {
                                        if (lstSASM.Contains(sasm) == false)
                                        {
                                            lstSASM.Add(sasm);
                                            SubAnalyteObject.AnalyteName = row["sAnalyteName"].ToString();
                                            SubAnalyteObject.SubAnalyteName = row["sSubAnalyteName"].ToString();
                                            SubAnalyteObject.Specimen = row["sSampleType"].ToString();
                                            SubAnalyteObject.MethodName = row["sMethodName"].ToString();
                                            SubAnalyteObject.ResultType = row["sResultType"].ToString();
                                            SubAnalyteObject.ReferenceType = row["ReferenceType"].ToString();
                                            SubAnalyteObject.AgeGroup = Age;
                                            SubAnalyteObject.MaleRange = "";
                                            SubAnalyteObject.FemaleRange = row["FemaleMinValue"].ToString() + "-" + row["FemaleMaxValue"].ToString();
                                            SubAnalyteObject.Grade = row["Grade"].ToString();
                                            SubAnalyteObject.Unit = row["Unit"].ToString();
                                            SubAnalyteObject.Interpretation = row["Interpretation"].ToString();
                                            SubAnalyteObject.UpperLimit = row["UpperLimit"].ToString();
                                            SubAnalyteObject.LowerLimit = row["LowerLimit"].ToString();

                                            SubAnalyteObject.InterpretationList = new JArray() as dynamic;

                                            SqlParameter[] paramTest = new SqlParameter[]
                                                 {
                                                  new SqlParameter("@TSASMId",sasm)
                                                 };
                                            DataTable dtinterpretation = DAL.ExecuteStoredProcedureDataTable("Sp_GetsTestSubAnalyteInterpretation ", paramTest);
                                            if (dtinterpretation.Rows.Count > 0)
                                            {
                                                foreach (DataRow _rowint in dtinterpretation.Rows)
                                                {
                                                    dynamic ObjInterPretation = new JObject();
                                                    ObjInterPretation.Result = _rowint["Interpretation"].ToString();
                                                    SubAnalyteObject.InterpretationList.Add(ObjInterPretation);
                                                }
                                            }
                                            Result.AnalyteList.Add(SubAnalyteObject);
                                        }
                                    }
                                    else
                                    {
                                        if (FromAge <= PatientAge && ToAge >= PatientAge && AgeUnit == patientageunit)
                                        {
                                            if (lstSASM.Contains(sasm) == false)
                                            {
                                                lstSASM.Add(sasm);
                                                SubAnalyteObject.AnalyteName = row["sAnalyteName"].ToString();
                                                SubAnalyteObject.SubAnalyteName = row["sSubAnalyteName"].ToString();
                                                SubAnalyteObject.Specimen = row["sSampleType"].ToString();
                                                SubAnalyteObject.MethodName = row["sMethodName"].ToString();
                                                SubAnalyteObject.ResultType = row["sResultType"].ToString();
                                                SubAnalyteObject.ReferenceType = row["ReferenceType"].ToString();
                                                SubAnalyteObject.AgeGroup = Age;
                                                SubAnalyteObject.MaleRange = "";
                                                SubAnalyteObject.FemaleRange = row["FemaleMinValue"].ToString() + "-" + row["FemaleMaxValue"].ToString();
                                                SubAnalyteObject.Grade = row["Grade"].ToString();
                                                SubAnalyteObject.Unit = row["Unit"].ToString();
                                                SubAnalyteObject.Interpretation = row["Interpretation"].ToString();
                                                SubAnalyteObject.UpperLimit = row["UpperLimit"].ToString();
                                                SubAnalyteObject.LowerLimit = row["LowerLimit"].ToString();

                                                SubAnalyteObject.InterpretationList = new JArray() as dynamic;

                                                SqlParameter[] paramTest = new SqlParameter[]
                                              {
                                                  new SqlParameter("@TSASMId",sasm)
                                              };
                                                DataTable dtinterpretation = DAL.ExecuteStoredProcedureDataTable("Sp_GetsTestSubAnalyteInterpretation ", paramTest);
                                                if (dtinterpretation.Rows.Count > 0)
                                                {
                                                    foreach (DataRow _rowint in dtinterpretation.Rows)
                                                    {
                                                        dynamic ObjInterPretation = new JObject();
                                                        ObjInterPretation.Result = _rowint["Interpretation"].ToString();
                                                        SubAnalyteObject.InterpretationList.Add(ObjInterPretation);
                                                    }
                                                }
                                                Result.AnalyteList.Add(SubAnalyteObject);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Success";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "No Record found";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    return JSONString;
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);
                return JSONString;
            }
        }

        [HttpPost]
        [Route("FamilyMemberOldReportDetails")]
        public string FamilyMemberOldReportDetails([FromBody] familymemberoldreoortsdetails model) 
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Createroot JSON Object
            try
            {
                string Msg = "";
                if (!Ival.IsInteger(model.reportid.ToString()))
                {
                    Msg += "Please Enter Valid Report Id";
                }
                if (!Ival.IsInteger(model.Familymemberid.ToString()))
                {
                    Msg += "Please Enter Valid Family Member Id";
                }
                if (Msg.Length > 0)
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = Msg;
                    JSONString = JsonConvert.SerializeObject(Result);
                    return JSONString;
                }
                else
                {
                   // var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
                    SqlParameter[] param = new SqlParameter[]
                           {
                                    new SqlParameter("@UserId",model.Familymemberid),
                                    new SqlParameter("@ReportId",model.reportid)
                           };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetOldReportDetails", param);

                    if (dt.Rows.Count > 0)
                    {

                        Result.ReportData = new JArray() as dynamic;   // Create Array for Baner Details
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            dynamic ObjReportDetail = new JObject();
                            ObjReportDetail.ID = dt.Rows[j]["ID"];
                            ObjReportDetail.ReportId = dt.Rows[j]["ReportId"];
                            ObjReportDetail.ParameterName = dt.Rows[j]["ParameterName"];
                            ObjReportDetail.Value = dt.Rows[j]["Value"];
                            ObjReportDetail.Result = dt.Rows[j]["Result"];
                            ObjReportDetail.MinRange = dt.Rows[j]["MinRange"];
                            ObjReportDetail.MaxRange = dt.Rows[j]["MaxRange"];
                            ObjReportDetail.Unit = dt.Rows[j]["Unit"];
                            ObjReportDetail.CreatedDate = dt.Rows[j]["CreatedDate"];
                            Result.ReportData.Add(ObjReportDetail); //Add baner details to array
                        }

                        Result.DocNote = new JArray() as dynamic;
                        SqlParameter[] paramdoc = new SqlParameter[]
                       {
                             new SqlParameter("@ReportID",model.reportid),
                             new SqlParameter("@UserId",model.Familymemberid)
                       };
                        DataTable dtDoc = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetNoteListsBySharedOldReport ", paramdoc);
                        if (dtDoc.Rows.Count > 0)
                        {
                            for (int j = 0; j < dtDoc.Rows.Count; j++)
                            {
                                dynamic ObjTestDetail = new JObject();
                                ObjTestDetail.Createddate = dtDoc.Rows[j]["ModifiedDate"];
                                ObjTestDetail.DoctorComment = dtDoc.Rows[j]["DoctorComment"];
                                Result.DocNote.Add(ObjTestDetail); //Add Doctor details to array
                            }
                        }

                        Result.Status = true;  //  Status Key
                        Result.TestName = dt.Rows[0]["TestName"];
                        Result.LabName = dt.Rows[0]["LabName"];
                        Result.TestDate = dt.Rows[0]["TestDate"];
                        Result.RefDoctorName = dt.Rows[0]["RefDoctorName"];
                        Result.Notes = dt.Rows[0]["Notes"];
                        Result.FilePath = dt.Rows[0]["FilePath"];
                        Result.Msg = "Success";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "No Record found";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                }
            }
            catch (Exception e)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        [HttpPost]
        [Route("GetOldReportlistCompair")]
        public string GetOldReportlistCompair([FromBody] PagingParameterModel pagingparametermodel)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Createroot JSON Object

            try
            {
                string Msg = "";
                //if (pagingparametermodel.Searching == "")
                //{
                //    Msg += "Please Enter Valid TestName";
                //}

                SqlParameter[] param = new SqlParameter[]
                    {
                         new SqlParameter("@UserId",UserId)
                        // new SqlParameter("@SearchingText",pagingparametermodel.Searching)
                    };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetOldReportlistcompair", param);
                if (dt.Rows.Count > 0)
                {
                    // Get's No of Rows Count 
                    int count = dt.Rows.Count;

                    // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
                    int CurrentPage = pagingparametermodel.pageNumber;

                    // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
                    int PageSize = pagingparametermodel.pageSize;

                    // Display TotalCount to Records to User  
                    int TotalCount = count;

                    // Calculating Totalpage by Dividing (No of Records / Pagesize)  
                    int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

                    // Returns List of Doctor after applying Paging   
                    var items = dt.Select().Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

                    Result.ReportData = new JArray() as dynamic;   // Create Array for Test Details


                    foreach (DataRow row in dt.Rows)
                    {
                        dynamic ObjLabDetail = new JObject();
                        ObjLabDetail.Flag = "OldReport";

                        
                        ObjLabDetail.TestName = row["TestName"].ToString();
                        Result.ReportData.Add(ObjLabDetail);
                    }


                    //for (int i = 0; i < dt.Rows.Count; i++)
                    //{
                    //    dynamic ObjLabDetail = new JObject();
                    //    ObjLabDetail.Flag = "OldReport";
                    //    //ObjLabDetail.ID = items[i]["ID"];
                    //    //ObjLabDetail.TestName = items[i]["TestName"];
                    //    //ObjLabDetail.TestDate = items[i]["TestDate"];
                    //    //ObjLabDetail.RefDoctorName = items[i]["RefDoctorName"];
                    //    //ObjLabDetail.Notes = items[i]["Notes"];
                    //    //ObjLabDetail.CreatedDate = items[i]["CreatedDate"];
                    //    //ObjLabDetail.FilePath = items[i]["FilePath"];
                    //    //ObjLabDetail.LabName = items[i]["LabName"];
                    // //   ObjLabDetail.ID = dt.Rows[0]["ID"];
                    //    ObjLabDetail.TestName = dt.Rows[0]["TestName"];
                    //    //Result.TestDate = dt.Rows[0]["TestDate"];

                    //    Result.ReportData.Add(ObjLabDetail);
                    //    //Add Test details to array

                      
                    //}
                    Result.Status = true;  //  Status Key
                    Result.Msg = "Success";
                    Result.TotalCount = TotalCount;
                    Result.PageSize = PageSize;
                    Result.CurrentPage = CurrentPage;
                    Result.TotalPages = TotalPages;
                    JSONString = JsonConvert.SerializeObject(Result);
                }
                else
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = "No Record found";
                    JSONString = JsonConvert.SerializeObject(Result);
                }
            }
            catch (Exception e)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }
        [HttpPost] 
        [Route("ReportoldDetilsForGraph")]
        public string ReportoldDetilsForGraph([FromBody] WS_Sp_GetoldReportDetailsForGraph Graphmodel)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                WS_Sp_GetoldReportDetailsForGraph obj = new WS_Sp_GetoldReportDetailsForGraph();
                string Msg = "";
                if (Graphmodel.ParameterName == "")
                {
                    Msg += "Please Enter Valid TestName";
                }
                if (Msg.Length > 0)
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = Msg;
                    JSONString = JsonConvert.SerializeObject(Result);
                    return JSONString;
                }
                else
                {
                    SqlParameter[] param = new SqlParameter[]
                      {
                             new SqlParameter("@UserID",UserId),
                             new SqlParameter("@ParameterName",Graphmodel.ParameterName)
                      };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetoldReportDetailsForGraph", param);
                    

                    if (dt.Rows.Count > 0)
                    {
                        Result.Status = true;
                        Result.Flag = "OldReport";//  Status Key
                        Result.TestName = dt.Rows[0]["TestName"];
                        //Result.ReportId = dt.Rows[0]["ReportId"];
                        Result.MaxRange = dt.Rows[0]["MaxRange"];
                        Result.MinRange = dt.Rows[0]["MinRange"];
                       // Result.Result = dt.Rows[0]["Result"];
                        Result.Unit = dt.Rows[0]["Unit"];
                       // Result.Value = dt.Rows[0]["Value"];
                       // Result.TestDate = dt.Rows[0]["TestDate"];
                        Result.ReportData = new JArray() as dynamic;   // Create Array for Baner Details
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            dynamic ObjReportDetail = new JObject();

                           // ObjReportDetail.Status = true;
                            ObjReportDetail.ParameterName = dt.Rows[j]["ParameterName"];
                            //ObjReportDetail.ReportId = dt.Rows[j]["ReportId"];
                            ObjReportDetail.MaxRange = dt.Rows[j]["MaxRange"];
                            ObjReportDetail.MinRange = dt.Rows[j]["MinRange"];
                            ObjReportDetail.Result = dt.Rows[j]["Result"];
                           ObjReportDetail.Unit = dt.Rows[0]["Unit"];
                            ObjReportDetail.Value = dt.Rows[0]["Value"];
                            ObjReportDetail.TestDate = dt.Rows[0]["TestDate"];
                            Result.ReportData.Add(ObjReportDetail); //Add baner details to array

                            //  ObjReportDetail.ReportData.Add(ObjReportDetail); //Add baner details to array
                        }
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.Status = false;  //  Status Key
                        Result.Msg = "No Records found.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);

            }
            return JSONString;
        }
    }
}
