using CrossPlatformAESEncryption.Helper;
using Howzu_API.Model;
using Howzu_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Validation;
using VLS_API.Model;
namespace VLS_API.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        DataAccessLayer DAL = new DataAccessLayer();
        InputValidation Ival = new InputValidation();
        FCMPushNotification fcm = new FCMPushNotification();

        /// <summary>
        /// Get Doctor Details By Doctor id
        /// </summary>
        /// <param name="id">mandatory</param>
        /// <returns></returns>
        [HttpGet]
        [Route("DoctorMyProfile")]
        public string DoctorMyProfile()
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            Result.MyDetails = new JArray() as dynamic;
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            try
            {
                if (UserId != null)
                {
                    DataTable dt = DAL.GetDataTable("WS_Sp_GetDoctorPersonalDetail " + UserId);
                    if (dt.Rows.Count > 0)
                    {
                        dynamic ObjDoctorDetail = new JObject();

                        ObjDoctorDetail.UserId = dt.Rows[0]["UserId"];
                        ObjDoctorDetail.FullName = dt.Rows[0]["FullName"];
                        ObjDoctorDetail.Mobile = dt.Rows[0]["Mobile"];
                        // ObjDoctorDetail.Mobile = dt.Rows[0]["EmailId"];
                        ObjDoctorDetail.EmailId = dt.Rows[0]["EmailId"];
                        ObjDoctorDetail.AadharCard = dt.Rows[0]["AadharCard"];
                        ObjDoctorDetail.Gender = dt.Rows[0]["Gender"];
                        ObjDoctorDetail.DOB = dt.Rows[0]["DOB"];
                        ObjDoctorDetail.Address = dt.Rows[0]["Address"];
                        ObjDoctorDetail.Country = dt.Rows[0]["Country"];
                        ObjDoctorDetail.Pincode = dt.Rows[0]["Pincode"];
                        ObjDoctorDetail.City = dt.Rows[0]["City"];
                        ObjDoctorDetail.State = dt.Rows[0]["State"];
                        ObjDoctorDetail.ProfilePic = dt.Rows[0]["ProfilePic"];
                        ObjDoctorDetail.Msg = dt.Rows[0]["Msg"];
                        ObjDoctorDetail.Role = dt.Rows[0]["Role"];
                        ObjDoctorDetail.Degree = dt.Rows[0]["Degree"];
                        ObjDoctorDetail.Clinic = dt.Rows[0]["Clinic"];
                        ObjDoctorDetail.Specialization = dt.Rows[0]["Specialization"];
                        Result.MyDetails.Add(ObjDoctorDetail); //Add Doctor details to array
                        Result.Status = true;  //  Status Key 
                    }
                    else
                    {
                        Result.Status = false;  //  Status Key
                        Result.Msg = "No Record Found";
                    }
                    JSONString = JsonConvert.SerializeObject(Result);
                }
                else
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = "Something went wrong,Please try again.";
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
        [Route("MyPatientList")]
        public string MyPatientList([FromBody] PagingParameterModel pagingparametermodel)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                SqlParameter[] param = new SqlParameter[]
                    {
                                new SqlParameter("@DoctorId",UserId),
                                new SqlParameter("@SearchingText",pagingparametermodel.Searching)
                    };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp__GetPatientsByDoctorIdwithSearch", param);
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
                    Result.PatientList = new JArray() as dynamic;
                    for (int j = 0; j < items.Count; j++)
                    {
                        dynamic ObjPatientDetail = new JObject();
                        ObjPatientDetail.MyPatientId = items[j]["sMyPatientId"];
                        ObjPatientDetail.UserId = items[j]["sAppUserId"];
                        ObjPatientDetail.PatientName = items[j]["sFullName"];
                        ObjPatientDetail.Mobile = items[j]["sMobile"];
                        ObjPatientDetail.EmailId = items[j]["sEmailId"];
                        ObjPatientDetail.Address = items[j]["sAddress"];
                        ObjPatientDetail.BirthDate = items[j]["sBirthDate"];
                        ObjPatientDetail.ProfilePic = items[j]["sImagePath"];
                        Result.PatientList.Add(ObjPatientDetail); //Add Patient details to array
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
                    Result.Msg = "No Record Found";
                    JSONString = JsonConvert.SerializeObject(Result);
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        /// <summary>
        /// Get All Patients List 
        /// </summary>
        /// <param name="id">Mandatory</param>
        /// <returns></returns>
        [HttpPost]
        [Route("AllPatientList")]
        public string AllPatientList([FromBody] PagingParameterModel pagingparametermodel)
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
                SqlParameter[] param = new SqlParameter[]
                    {
                                new SqlParameter("@DoctorId",UserId),
                                new SqlParameter("@SearchingText",pagingparametermodel.Searching)
                    };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp__GetPatientListswithsearch", param);

                // DataTable dt = DAL.GetDataTable("WS_Sp__GetPatientLists " + UserId);
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
                    Result.PatientList = new JArray() as dynamic;   // Create Array for Patient Details
                    for (int j = 0; j < items.Count; j++)
                    {
                        dynamic ObjPatientDetail = new JObject();
                        ObjPatientDetail.MyPatientId = dt.Rows[j]["sMyDoctorId"];
                        ObjPatientDetail.UserId = dt.Rows[j]["sAppUserId"];
                        ObjPatientDetail.PatientName = dt.Rows[j]["sFullName"];
                        ObjPatientDetail.Mobile = dt.Rows[j]["sMobile"];
                        ObjPatientDetail.EmailId = dt.Rows[j]["sEmailId"];
                        ObjPatientDetail.Address = dt.Rows[j]["sAddress"];
                        ObjPatientDetail.BirthDate = dt.Rows[j]["sBirthDate"];
                        ObjPatientDetail.ProfilePic = dt.Rows[j]["sImagePath"];
                        Result.PatientList.Add(ObjPatientDetail); //Add Patient details to array
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
                    Result.Msg = "No Record Found";
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
        /// Add patients in my Patient List, Patient should be "," seprated if we need to add more than 1 Patient
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddPatientToMyList")]
        public string AddPatientToMyList([FromBody] MyPatientList model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            string _patientId = "";
            try
            {
                if (Ival.IsTextBoxEmpty(model.PatientId))
                {
                    Msg += "Please Enter Valid Patient Id";
                }
                else
                {
                    string[] splitPatient = model.PatientId.ToString().Split(',');
                    foreach (string PatientId in splitPatient)
                    {
                        if (!Ival.IsInteger(PatientId))
                        {
                            Msg += "Please Enter Valid Patient Id";
                        }
                        else
                        {
                            _patientId += PatientId + ",";
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
                    _patientId = _patientId.TrimEnd(',');
                    string[] splitPatient = _patientId.Split(',');
                    foreach (string PatientId in splitPatient)
                    {
                        SqlParameter[] param = new SqlParameter[]
                            {
                                    new SqlParameter("@PatientId",PatientId),
                                    new SqlParameter("@DoctorId",UserId),
                                    new SqlParameter("@returnval",SqlDbType.Int)
                            };
                        data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp__AddPatientInDoctorList", param);

                        //DataTable dt = DAL.GetDataTable("WS_Sp_GetUserdevicetoken " + PatientId);
                        //if (dt.Rows.Count > 0)
                        //{
                        //    string _title = "";
                        //    string _Devicetoken = dt.Rows[0]["sDeviceToken"].ToString();
                        //    string _Msg = "";
                        //    string _PayLoad = "";
                        //    fcm.SendNotification(_title, _Msg, _Devicetoken, _PayLoad);
                        //    Notification.AppNotification(PatientId, "", _title, _Msg, UserId);
                        //}
                    }
                    if (data == 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Patient's Added successfully.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else if (data == -2)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Patient already exists in your list.";
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
        /// Update Doctor Details by doctor id
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateDoctor")]
        public string UpdateDoctor([FromBody] UpdateDoctor model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;

            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (Ival.IsTextBoxEmpty(model.Degree))
                {
                    Msg += "Please Enter Valid Degree.";
                }
                if (Ival.IsTextBoxEmpty(model.SpecialistIn))
                {
                    Msg += "Please Enter Valid specialization.";
                }
                if (!Ival.IsInteger(model.Pincode.ToString()))
                {
                    Msg += "Please Enter Valid Pincode";
                }
                if (!Ival.IsTextBoxEmpty(model.Aadharcard))
                {
                    if (Ival.IsInteger(model.Aadharcard))
                    {
                        if (!Ival.AadharValidation(model.Aadharcard))
                        {
                            Msg += " Please Enter Valid Aadhar Number";
                        }
                    }
                    else
                    {
                        Msg += " Please Enter Valid Aadhar Number";
                    }
                }
                if (!Ival.IsTextBoxEmpty(model.EmailId))
                {
                    if (!Ival.IsValidEmailAddress(model.EmailId))
                    {
                        Msg += " Please Enter Valid Email Id";
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
                    var _emailId = (model.EmailId != "") ? CryptoHelper.Encrypt(model.EmailId.ToLower()) : "";
                    var _Aadhar = (model.Aadharcard != "") ? CryptoHelper.Encrypt(model.Aadharcard) : "";
                    SqlParameter[] param = new SqlParameter[]
                        {
                            new SqlParameter("@UserId",UserId),
                            new SqlParameter("@Address",model.Address),
                            new SqlParameter("@Degree",model.Degree),
                            new SqlParameter("@SpecialistIn",model.SpecialistIn),
                            new SqlParameter("@Clinic",model.Clinic),
                            new SqlParameter("@sImagePath",model.ProfileIamge),
                            new SqlParameter("@Pincode",model.Pincode),
                            new SqlParameter("@City",model.City),
                            new SqlParameter("@Aadharcard",_Aadhar),
                              new SqlParameter("@EmailId",_emailId),
                            new SqlParameter("@returnval",SqlDbType.Int)
                      };
                    int data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_UpdateDoctorDetailsUpdatedOne", param);
                    if (data == 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Profile updated successfully";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else if (data == -1)
                    {
                        Result.Status = false;  //  Status Key
                        Result.Msg = "Email Id already exists";
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
        /// Add suggested test to patient
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("TestSuggestToPatient")]
        public string TestSuggestToPatient([FromBody] TestSuggest model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string _testId = "";

            try
            {
                string Msg = "";
                if (!Ival.IsInteger(model.LabId.ToString()))
                {
                    Msg += "Please Enter Valid LabId";
                }
                if (!Ival.IsInteger(model.PatientId.ToString()))
                {
                    Msg += "Please Enter Valid Patient Id";
                }
                if (Ival.IsTextBoxEmpty(model.TestId))
                {
                    Msg += "Please Enter Valid Test Id";
                }
                else
                {
                    string[] splitTestId = model.TestId.ToString().Split(',');
                    foreach (string testId in splitTestId)
                    {
                        if (!Ival.IsInteger(testId))
                        {
                            Msg += "Please Enter Valid Test Id";
                        }
                        else
                        {
                            _testId += testId + ",";
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
                            new SqlParameter("@Patientid",model.PatientId),
                            new SqlParameter("@DoctorId",UserId),
                            new SqlParameter("@RecommendedAt",""),
                            new SqlParameter("@LabId",model.LabId),
                            new SqlParameter("@Returnval",SqlDbType.Int)
                            };
                    data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_AddRecommendation", param);
                    if (data >= 1)
                    {
                        string _testIds = "";
                        string _testName = "";
                        string _testPrice = "";
                        int _totalAmount = 0;
                        int _testCount = 0;
                        SqlParameter[] paramTest = new SqlParameter[]
                        {
                          new SqlParameter("@TestList",model.TestId),
                          new SqlParameter("@LabId",model.LabId)
                        };
                        DataTable dtTest = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetTestPricebyLabIdAndTestlist", paramTest);
                        if (dtTest.Rows.Count > 0)
                        {
                            foreach (DataRow row in dtTest.Rows)
                            {
                                _testName += row["sTestName"] + ",";
                                _testIds += row["sTestId"] + ",";
                                _testPrice += row["sPrice"] + ",";
                                _totalAmount += Convert.ToInt32(row["sPrice"] != "" ? row["sPrice"] : "0");
                                _testCount = _testCount + 1;
                            }
                        }

                        SqlParameter[] paramName = new SqlParameter[]
                            {
                            new SqlParameter("@Patientid",model.PatientId),
                            new SqlParameter("@DoctorId",UserId),
                            new SqlParameter("@LabId",model.LabId)
                             };
                        DataTable dt = DAL.ExecuteStoredProcedureDataTable("Sp_Name", paramName);
                        if (dt.Rows.Count > 0)
                        {
                            string _Msg = "Dr. " + dt.Rows[0]["DoctorName"].ToString() + " has suggested you a test at " + dt.Rows[0]["LabName"].ToString() + ". Kindly book the same at the earliest.";
                            FCMPushNotification fcm = new FCMPushNotification();

                            dynamic _Result = new JObject();
                            _Result.TotalAmount = _totalAmount;
                            _Result.TestCount = _testCount;
                            _Result.TestPrice = _testPrice.TrimEnd(',');
                            _Result.testId = _testIds.TrimEnd(',');
                            _Result.TestName = _testName.TrimEnd(',');
                            _Result.LabId = model.LabId;
                            _Result.DoctorId = UserId;
                            _Result.RecomndationId = data;
                            _Result.LabLogo = dt.Rows[0]["LabLogo"].ToString();
                            _Result.LabContact = dt.Rows[0]["LabContact"].ToString();
                            _Result.LabAddress = dt.Rows[0]["LabAddress"].ToString();
                            _Result.LabName = dt.Rows[0]["LabName"].ToString();
                            _Result.LabOnlinePayment = dt.Rows[0]["LabOnlinePayment"].ToString();
                            string _payload = JsonConvert.SerializeObject(_Result);

                            string _type = "Test";
                            fcm.SendNotificationSuggestTest("Suggest Test", _Msg, dt.Rows[0]["DeviceTokan"].ToString(), _type, _testIds.TrimEnd(','), model.LabId.ToString(), UserId,
                                data.ToString(), _testPrice.TrimEnd(','), _totalAmount, _testName.TrimEnd(','), _testCount, dt.Rows[0]["LabLogo"].ToString(), dt.Rows[0]["LabContact"].ToString(),
                                dt.Rows[0]["LabAddress"].ToString(), dt.Rows[0]["LabName"].ToString(), Convert.ToBoolean(dt.Rows[0]["LabOnlinePayment"].ToString()));
                            Notification.AppNotification(model.PatientId.ToString(), model.LabId.ToString(), "Suggest Test", _Msg, _type, _payload, UserId);
                        }

                        _testId = _testId.TrimEnd(',');
                        string[] splitTest = _testId.Split(',');

                        foreach (string TestId in splitTest)
                        {
                            SqlParameter[] param1 = new SqlParameter[]
                                     {
                                new SqlParameter("@RecommendationId",data),
                                new SqlParameter("@TestId",TestId),
                                new SqlParameter("@LabId",model.LabId),
                                new SqlParameter("@Returnval",SqlDbType.Int)
                            };
                            int resVal = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_AddTestRecommendation", param1);
                        }
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Test suggested successfully.";
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
        /// Add note on shared report
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddNote")]
        public string AddNote([FromBody] ReportNote model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object

            try
            {
                string Msg = "";
                if (!Ival.IsInteger(model.ReportId.ToString()))
                {
                    Msg += "Please Enter Valid ReportId";
                }
                if (Ival.IsTextBoxEmpty(model.Note))
                {
                    Msg += "Please Enter Valid Note";
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
                        new SqlParameter("@UserId",UserId),
                        new SqlParameter("@Notes",model.Note),
                        new SqlParameter("@ReportsId",model.ReportId),
                        new SqlParameter("@returnval",SqlDbType.Int)
                    };
                    data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_AddNoteUpdated", param);
                    if (data == 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Note added successfully.";
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
        /// Update Patient to Doctor 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdatePatientToDoctor")]
        public string UpdatePatientToDoctor([FromBody] UpdatePatientToDoctor model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
                if (Ival.IsTextBoxEmpty(model.Degree))
                {
                    Msg += "Please Enter Valid Degree.";
                }
                if (Ival.IsTextBoxEmpty(model.SpecialistIn))
                {
                    Msg += "Please Enter Valid specialization.";
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
                            new SqlParameter("@Degree",model.Degree),
                            new SqlParameter("@SpecialistIn",model.SpecialistIn),
                            new SqlParameter("@Clinic",model.Clinic),
                            new SqlParameter("@returnval",SqlDbType.Int)
                        };
                    int data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_UpdatePatientToDoctor", param);
                    if (data == 1)
                    {
                        SqlParameter[] param1 = new SqlParameter[]
                       {
                        new SqlParameter("@UserId",UserId),
                        new SqlParameter("@returnval",SqlDbType.Int),
                       };
                        int Val = DAL.ExecuteStoredProcedureRetnInt("Ws_Sp_UpdateRegistrationStatus", param1);
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Doctor details updated successfully";
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
                Result.Msg = "Something went wrong,Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        /// <summary>
        /// Shared Report List for Doctor Dashboard
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("SharedReportListforDashboard")]
        public string SharedReportListforDashboard()
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
                DataTable dt = DAL.GetDataTable("WS_Sp_GetSharedreportlistwithDoctor " + UserId);
                if (dt.Rows.Count > 0)
                {
                    Result.ReportList = new JArray() as dynamic;   // Create Array for List Details
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjReportDetail = new JObject();
                        ObjReportDetail.Flag = "";
                        ObjReportDetail.SharedReportId = dt.Rows[j]["sSharedReportId"].ToString();
                        ObjReportDetail.ReportId = dt.Rows[j]["sBookLabTestId"].ToString();
                        ObjReportDetail.PatientName = dt.Rows[j]["sFullName"].ToString();
                        ObjReportDetail.Mobile = dt.Rows[j]["sMobile"].ToString();
                        ObjReportDetail.EmailId = dt.Rows[j]["sEmailId"].ToString();
                        ObjReportDetail.PatientId = dt.Rows[j]["sPatientId"].ToString();
                        ObjReportDetail.TestCode = dt.Rows[j]["sTestCode"].ToString();
                        ObjReportDetail.TestName = dt.Rows[j]["sTestName"].ToString();
                        ObjReportDetail.ReportCreatedDate = dt.Rows[j]["sReportCreatedOn"].ToString();
                        ObjReportDetail.RecommendedDate = dt.Rows[j]["sReportCreatedOn"].ToString();
                        ObjReportDetail.SharedDate = dt.Rows[j]["sReportCreatedOn"];
                        ObjReportDetail.Flag = "";
                        Result.ReportList.Add(ObjReportDetail); //Add Object details to array
                    }
                    Result.Status = true;  //  Status Key
                    Result.Msg = "Success";
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
                Result.Msg = "Something went wromg,Please try again";
                JSONString = JsonConvert.SerializeObject(Result);

            }
            return JSONString;
        }

        /// <summary>
        /// Get Suggest Test list For Doctor Dahsboard
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetSuggestTestlistForDahsboard")]
        public string GetSuggestTestlistForDahsboard()
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Createroot JSON Object
            try
            {
                var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
                DataTable dt = DAL.GetDataTable("Sp_GetDoctorRecommendedTestListtoPatient " + UserId);

                if (dt.Rows.Count > 0)
                {
                    dt.Columns.Add("TestName", typeof(string));
                    dt.Columns.Add("testId", typeof(string));
                    dt.Columns.Add("Patient", typeof(string));
                    dt.Columns.Add("LabName", typeof(string));
                    dt.Columns.Add("LabId", typeof(string));
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var BookingId = dt.Rows[i]["sRecommendationId"].ToString();
                        if (BookingId != "")
                        {
                            SqlParameter[] param = new SqlParameter[]
                               {
                                        new SqlParameter("@RcomId",BookingId)
                               };
                            DataTable dt1 = DAL.ExecuteStoredProcedureDataTable("Sp_GetTestDetailsByRecommendedByDoctor", param);

                            string testId = "";
                            string TestName = "";
                            string Doctor = dt1.Rows[0]["sDoctor"].ToString();
                            string LabName = dt1.Rows[0]["sLabName"].ToString();
                            string LabId = dt1.Rows[0]["sLabId"].ToString();

                            foreach (DataRow test in dt1.Rows)
                            {
                                testId += test["sTestId"].ToString() + ",";
                                TestName += test["sTestName"].ToString() + ",";
                            }
                            DataRow row = dt.Rows[i];
                            row["TestName"] = TestName.TrimEnd(',');
                            row["testId"] = testId.TrimEnd(',');
                            row["Patient"] = Doctor;
                            row["LabName"] = LabName;
                            row["LabId"] = LabId;
                        }
                    }
                    Result.SuggestTestList = new JArray() as dynamic;
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjASuggestTestDetail = new JObject();
                        ObjASuggestTestDetail.RecommendationId = dt.Rows[j]["sRecommendationId"];
                        ObjASuggestTestDetail.BookStatus = dt.Rows[j]["sViewStatus"];
                        ObjASuggestTestDetail.RecommendedDate = dt.Rows[j]["sRecommendedAt"];
                        ObjASuggestTestDetail.PatientName = dt.Rows[j]["Patient"];
                        ObjASuggestTestDetail.PatientId = dt.Rows[j]["sPatientId"];
                        ObjASuggestTestDetail.LabId = dt.Rows[j]["LabId"];
                        ObjASuggestTestDetail.LabName = dt.Rows[j]["LabName"];
                        ObjASuggestTestDetail.TestName = dt.Rows[j]["TestName"];
                        ObjASuggestTestDetail.testId = dt.Rows[j]["testId"];
                        Result.SuggestTestList.Add(ObjASuggestTestDetail);
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
        /// Get Patient,Report and Suggest Test Count for Doctor Dashboard
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetDashboardKPICount")]
        public string GetDashboardKPICount()
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
                DataTable dt = DAL.GetDataTable("WS_Sp_GetDoctorDashboardCount " + UserId);
                if (dt.Rows.Count > 0)
                {
                    Result.PatientCount = dt.Rows[0]["PatientCount"].ToString();
                    Result.SharedReportCount = dt.Rows[0]["SharedReportCount"].ToString();
                    Result.SuggestedTestCount = dt.Rows[0]["SuggestedTestCount"].ToString();
                    Result.Status = true;  //  Status Key
                    Result.Msg = "Success";
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
                Result.Msg = "Something went wromg,Please try again";
                JSONString = JsonConvert.SerializeObject(Result);

            }
            return JSONString;
        }

        [HttpGet]
        [Route("GetSharedReportCountStatusvise")]
        public string GetSharedReportCountStatusvise()
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
                DataTable dt = DAL.GetDataTable("WS_Sp_GetDoctorSharedReportCount " + UserId);
                if (dt.Rows.Count > 0)
                {
                    Result.PendingReport = dt.Rows[0]["PendingReport"].ToString();
                    Result.OnHoldReport = dt.Rows[0]["OnHoldReport"].ToString();
                    Result.CompleteReport = dt.Rows[0]["CompleteReport"].ToString();
                    Result.Status = true;  //  Status Key
                    Result.Msg = "Success";
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
                Result.Msg = "Something went wromg,Please try again";
                JSONString = JsonConvert.SerializeObject(Result);

            }
            return JSONString;
        }

        [HttpPost]
        [Route("SharedReportListwithStatus")]
        public string SharedReportListwithStatus([FromBody] SharedReportListforDoctor model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
                if (Ival.IsTextBoxEmpty(model.Status))
                {
                    Msg += "Please Enter Valid Status";
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
                            new SqlParameter("@DoctorId",UserId),
                            new SqlParameter("@Status",model.Status),
                            new SqlParameter("@SearchingText",model.Searching)
                        };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetReportListwithStatus", param);
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

                        // Returns List of Reports after applying Paging   
                        var items = dt.Select().Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

                        Result.ReportList = new JArray() as dynamic;   // Create Array for Report Details

                        for (int j = 0; j < items.Count; j++)
                        {
                            dynamic ObjReportDetail = new JObject();
                            ObjReportDetail.Flag = "";
                            ObjReportDetail.SharedReportId = items[j]["sSharedReportId"];
                            ObjReportDetail.TestCode = items[j]["sTestCode"];
                            ObjReportDetail.TimeSlot = items[j]["sTimeSlot"];
                            ObjReportDetail.TestDate = items[j]["sTestDate"];
                            ObjReportDetail.TestName = items[j]["sTestName"];
                            ObjReportDetail.ReportId = items[j]["sBookLabTestId"];
                            ObjReportDetail.Status = items[j]["Status"];
                            ObjReportDetail.PatientName = items[j]["sFullName"];
                            ObjReportDetail.PatientId = items[j]["sPatientId"];
                            ObjReportDetail.SharedDate = items[j]["CreatedDate"];
                            ObjReportDetail.ProfileName = items[j]["sProfileName"];
                            ObjReportDetail.ProfilePic = items[j]["sImagePath"];
                            Result.ReportList.Add(ObjReportDetail); //Add Report details to array
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
                        Result.Msg = "No Record Found";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        [HttpPost]
        [Route("UpdateSharedReportStatus")]
        public string UpdateSharedReportStatus([FromBody] DoctorSharedReport model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;

            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object

            try
            {
                string Msg = "";
                if (Ival.IsTextBoxEmpty(model.ReportId))
                {
                    Msg += "Please Enter Valid ReportId";
                }
                if (Ival.IsTextBoxEmpty(model.Status))
                {
                    Msg += "Please Enter Valid Status";
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
                    string _repotId = model.ReportId.TrimEnd(',');
                    string[] _reportIdArray = _repotId.Split(',');
                    foreach (string reportId in _reportIdArray)
                    {
                        var _testId = _reportIdArray;
                        SqlParameter[] param = new SqlParameter[]
                        {
                        new SqlParameter("@UserId",UserId),
                        new SqlParameter("@ReportId",reportId),
                        new SqlParameter("@Status",model.Status),
                        new SqlParameter("@returnval",SqlDbType.Int)
                        };
                        int data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_UpdateSharedReportStatus", param);
                        if (data == 1)
                        {
                            Result.Status = true;  //  Status Key 
                            Result.Msg = "Report status updated successfully";
                            JSONString = JsonConvert.SerializeObject(Result);
                        }
                        else
                        {
                            Result.Status = false;  //  Status Key 
                            Result.Msg = "Something went wrong,Please try again.";
                            JSONString = JsonConvert.SerializeObject(Result);
                        }
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

        [HttpPost]
        [Route("MyAllSuggestedTest")]
        public string MyAllSuggestedTest([FromBody] PagingParameterModel pagingparametermode)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Createroot JSON Object
            try
            {
                var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;

                SqlParameter[] param1 = new SqlParameter[]
                              {
                                        new SqlParameter("@DoctorId",UserId),
                                        new SqlParameter("@SearchingText",pagingparametermode.Searching)
                              };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("Sp_GetDoctorRecommendationsListUpdated", param1);

                if (dt.Rows.Count > 0)
                {
                    dt.Columns.Add("TestName", typeof(string));
                    dt.Columns.Add("testId", typeof(string));
                    dt.Columns.Add("Patient", typeof(string));
                    dt.Columns.Add("LabName", typeof(string));
                    dt.Columns.Add("LabId", typeof(string));
                    dt.Columns.Add("TestPrice", typeof(string));
                    dt.Columns.Add("LabAddress", typeof(string));
                    dt.Columns.Add("LabContact", typeof(string));
                    dt.Columns.Add("LabLogo", typeof(string));
                    dt.Columns.Add("PatientPic", typeof(string));
                    dt.Columns.Add("PatientMobile", typeof(string));
                    dt.Columns.Add("PatientEmail", typeof(string));
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var BookingId = dt.Rows[i]["sRecommendationId"].ToString();
                        var _LabId = dt.Rows[i]["sLabId"].ToString();
                        if (BookingId != "")
                        {
                            // DataTable dt1 = DAL.GetDataTable("Sp_GetTestDetailsByRecommendationsId " + BookingId);
                            SqlParameter[] param = new SqlParameter[]
                               {
                                        new SqlParameter("@RcomId",BookingId),
                                        new SqlParameter("@SearchingText",pagingparametermode.Searching),
                                         new SqlParameter("@LabId",_LabId)
                               };
                            DataTable dt1 = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetTestDetailsByRecommendationsIdandDoctorwithSearch", param);

                            if (dt1.Rows.Count > 0)
                            {
                                string testId = "";
                                string TestName = "";
                                string TestPrice = "";

                                string Patient = dt1.Rows[0]["PatientName"].ToString();
                                string PatientPic = dt1.Rows[0]["sImagePath"].ToString();
                                string PatientMobile = dt1.Rows[0]["sMobile"].ToString();
                                string PatientEmail = dt1.Rows[0]["sEmailId"].ToString();

                                string LabName = dt1.Rows[0]["sLabName"].ToString();
                                string LabId = dt1.Rows[0]["sLabId"].ToString();

                                string LabAddress = dt1.Rows[0]["sLabAddress"].ToString();
                                string LabLogo = dt1.Rows[0]["sLabLogo"].ToString();
                                string LabContact = dt1.Rows[0]["sLabContact"].ToString();

                                foreach (DataRow test in dt1.Rows)
                                {
                                    testId += test["sTestId"].ToString() + ",";
                                    TestName += test["sTestName"].ToString() + ",";
                                    TestPrice += test["sPrice"].ToString() + ",";
                                }
                                DataRow row = dt.Rows[i];
                                row["TestName"] = TestName.TrimEnd(',');
                                row["testId"] = testId.TrimEnd(',');
                                row["TestPrice"] = TestPrice.TrimEnd(',');
                                row["Patient"] = Patient;
                                row["LabName"] = LabName;
                                row["LabId"] = LabId;
                                row["LabAddress"] = LabAddress;
                                row["LabLogo"] = LabLogo;
                                row["LabContact"] = LabContact;
                                row["PatientPic"] = PatientPic;
                                row["PatientMobile"] = PatientMobile;
                                row["PatientEmail"] = PatientEmail;

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

                                Result.SuggestTestList = new JArray() as dynamic;

                                for (int x = 0; x < items.Count; x++)
                                {
                                    dynamic ObjASuggestTestDetail = new JObject();
                                    ObjASuggestTestDetail.RecommendationId = items[x]["sRecommendationId"];
                                    ObjASuggestTestDetail.BookStatus = items[x]["sViewStatus"];
                                    ObjASuggestTestDetail.RecommendedDate = items[x]["sRecommendedAt"];
                                    ObjASuggestTestDetail.PatientName = items[x]["Patient"];
                                    ObjASuggestTestDetail.PatientId = items[x]["sPatientId"];
                                    ObjASuggestTestDetail.PatientPic = items[x]["PatientPic"];
                                    ObjASuggestTestDetail.PatientMobile = items[x]["PatientMobile"];
                                    ObjASuggestTestDetail.PatientEmail = items[x]["PatientEmail"];
                                    ObjASuggestTestDetail.LabId = items[x]["LabId"];
                                    ObjASuggestTestDetail.LabName = items[x]["LabName"];
                                    ObjASuggestTestDetail.TestName = items[x]["TestName"];
                                    ObjASuggestTestDetail.testId = items[x]["testId"];
                                    ObjASuggestTestDetail.TestPrice = items[x]["TestPrice"];
                                    ObjASuggestTestDetail.LabAddress = items[x]["LabAddress"];
                                    ObjASuggestTestDetail.LabLogo = items[x]["LabLogo"];
                                    ObjASuggestTestDetail.LabContact = items[x]["LabContact"];
                                    Result.SuggestTestList.Add(ObjASuggestTestDetail);
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
        [Route("AllSharedReportList")]
        public string AllSharedReportList(PagingParameterModel pagingparametermode)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                SqlParameter[] param = new SqlParameter[]
                {
                    new SqlParameter("@DoctorId",UserId),
                    new SqlParameter("@SearchingText",pagingparametermode.Searching)
                };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetAllSharedReportList", param);
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

                    // Returns List of Reports after applying Paging   
                    var items = dt.Select().Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();


                    Result.ReportList = new JArray() as dynamic;   // Create Array for Report Details

                    for (int j = 0; j < items.Count; j++)
                    {
                        dynamic ObjReportDetail = new JObject();
                        ObjReportDetail.Flag = "";
                        ObjReportDetail.TestCode = items[j]["sTestCode"];
                        ObjReportDetail.TimeSlot = items[j]["sTimeSlot"];
                        ObjReportDetail.TestDate = items[j]["sTestDate"];
                        ObjReportDetail.TestName = items[j]["sTestName"];
                        ObjReportDetail.ReportId = items[j]["sBookLabTestId"];
                        ObjReportDetail.Status = items[j]["Status"];
                        ObjReportDetail.PatientName = items[j]["sFullName"];
                        ObjReportDetail.PatientId = items[j]["sPatientId"];
                        ObjReportDetail.PatientPic = items[j]["sImagePath"];
                        ObjReportDetail.PatientMobile = items[j]["sMobile"];
                        ObjReportDetail.PatientEmail = items[j]["sEmailId"];
                        ObjReportDetail.SharedDate = items[j]["CreatedDate"];
                        ObjReportDetail.ProfileName = items[j]["sProfileName"];
                        Result.ReportList.Add(ObjReportDetail); //Add Report details to array
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
                    Result.Msg = "No Record Found";
                    JSONString = JsonConvert.SerializeObject(Result);
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        [HttpGet]
        [Route("GetReportListSharedwithDoctor/{PatientId}")]
        public string GetReportListSharedwithDoctor(int PatientId)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
                if (!Ival.IsInteger(PatientId.ToString()))
                {
                    Msg += "Please Enter Valid Patient Id";
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
                        new SqlParameter("@PatientId",PatientId),
                        new SqlParameter("@DoctorId",UserId)
                 };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp__GetReportListSharedwithDoctor", param);
                    if (dt.Rows.Count > 0)
                    {
                        Result.Status = true;  //  Status Key
                        Result.TestList = new JArray() as dynamic;   // Create Array for Doctor Details

                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            dynamic ObjTestDetail = new JObject();
                            ObjTestDetail.TestCode = dt.Rows[j]["sTestCode"];
                            ObjTestDetail.TestName = dt.Rows[j]["sTestName"];
                            ObjTestDetail.BookingId = dt.Rows[j]["sBookLabId"];
                            ObjTestDetail.ReportId = dt.Rows[j]["sBookLabTestId"];
                            ObjTestDetail.SharedReportId = dt.Rows[j]["sSharedReportId"];
                            ObjTestDetail.TestDate = dt.Rows[j]["sTestDate"];
                            ObjTestDetail.TimeSlot = dt.Rows[j]["sTimeSlot"];
                            ObjTestDetail.ProfileName = dt.Rows[j]["sProfileName"];
                            ObjTestDetail.LabName = dt.Rows[j]["sLabName"];
                            ObjTestDetail.LabLogo = dt.Rows[j]["sLabLogo"];
                            ObjTestDetail.LabContact = dt.Rows[j]["sLabContact"];
                            Result.TestList.Add(ObjTestDetail); //Add Doctor details to array
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

        [HttpPost]
        [Route("PatientReportDetails")]
        public string PatientReportDetails([FromBody] DoctorSharedReportDetails model)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Createroot JSON Object
            try
            {
                string Msg = "";
                if (!Ival.IsInteger(model.ReportId.ToString()))
                {
                    Msg += "Please Enter Valid Report Id";
                }
                if (!Ival.IsInteger(model.UserId.ToString()))
                {
                    Msg += "Please Enter Valid Patient Id";
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
                                    new SqlParameter("@booklabtestid",model.ReportId),
                                    new SqlParameter("@UserId",model.UserId)
                                               };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("Sp_GetSharedReportDetails", param);

                    if (dt.Rows.Count > 0)
                    {
                        SqlParameter[] param1 = new SqlParameter[]
                                              {
                                    new SqlParameter("@booklabtestid",model.ReportId),
                                    new SqlParameter("@Gender",dt.Rows[0]["sGender"])
                                              };
                        DataTable dt1 = DAL.ExecuteStoredProcedureDataTable("Sp_GetReportValues", param1);

                        Result.ReportList = new JArray() as dynamic;

                        dynamic ReportDetailsObject = new JObject();
                        ReportDetailsObject.BookingId = dt.Rows[0]["sBookLabId"];
                        ReportDetailsObject.BookingDate = dt.Rows[0]["sBookRequestedAt"];
                        ReportDetailsObject.PatientName = dt.Rows[0]["sPatient"];
                        ReportDetailsObject.DoctorName = dt.Rows[0]["sDoctor"];
                        ReportDetailsObject.LabName = dt.Rows[0]["sLabName"];
                        ReportDetailsObject.LabAddress = dt.Rows[0]["sLabAddress"];
                        ReportDetailsObject.LabContact = dt.Rows[0]["sLabContact"];
                        ReportDetailsObject.TestId = dt.Rows[0]["sTestId"];
                        ReportDetailsObject.TestCode = dt.Rows[0]["sTestCode"];
                        ReportDetailsObject.TestName = dt.Rows[0]["sTestName"];
                        ReportDetailsObject.TestDate = dt.Rows[0]["sTestDate"];
                        ReportDetailsObject.TestAmount = dt.Rows[0]["sFees"];
                        ReportDetailsObject.ReportCreatedAt = dt.Rows[0]["sReportCreatedOn"];
                        ReportDetailsObject.ReportCreatedBy = dt.Rows[0]["sReportCreatedBy"];
                        ReportDetailsObject.ReportApprovedAt = dt.Rows[0]["sReportApprovedOn"];
                        ReportDetailsObject.ReportApprovedBy = dt.Rows[0]["sReportApprovedBy"];
                        ReportDetailsObject.Note_Comment = dt.Rows[0]["sNotes"] + "/" + dt.Rows[0]["sComment"];
                        ReportDetailsObject.SharedReportStatus = dt.Rows[0]["sharedReport"];

                        ReportDetailsObject.SubReportList = new JArray() as dynamic;
                        for (int j = 0; j < dt1.Rows.Count; j++)
                        {
                            dynamic SubPaymentObj = new JObject();

                            SubPaymentObj.Analyte = dt1.Rows[j]["sAnalyte"];
                            SubPaymentObj.Subanalyte = dt1.Rows[j]["sSubanalyte"];
                            SubPaymentObj.Specimen = dt1.Rows[j]["sSpecimen"];
                            SubPaymentObj.Method = dt1.Rows[j]["sMethod"];
                            SubPaymentObj.ResultType = dt1.Rows[j]["sResultType"];
                            SubPaymentObj.ReferenceType = dt1.Rows[j]["sReferenceType"];
                            SubPaymentObj.AgeGroup = dt1.Rows[j]["sAge"];
                            SubPaymentObj.ReferenceRange = dt1.Rows[j]["Range"];
                            SubPaymentObj.Grade = dt1.Rows[j]["sGrade"];
                            SubPaymentObj.Units = dt1.Rows[j]["sUnits"];
                            SubPaymentObj.Interpretation = dt1.Rows[j]["sInterpretation"];
                            SubPaymentObj.LowerLimit = dt1.Rows[j]["sLowerLimit"];
                            SubPaymentObj.UpperLimit = dt1.Rows[j]["sUpperLimit"];
                            SubPaymentObj.Value = dt1.Rows[j]["sValue"];
                            SubPaymentObj.Result = dt1.Rows[j]["sResult"];
                            ReportDetailsObject.SubReportList.Add(SubPaymentObj);
                        }
                        Result.ReportList.Add(ReportDetailsObject);
                        SqlParameter[] paramdoc = new SqlParameter[]
                       {
                             new SqlParameter("@SharedReportID",model.ReportId),
                            new SqlParameter("@UserId",UserId)
                       };
                        DataTable dtDoc = DAL.ExecuteStoredProcedureDataTable("WS_Sp__GetNoteListsBySharedReportIDforDoctor ", paramdoc);
                        Result.DocNote = new JArray() as dynamic;
                        if (dtDoc.Rows.Count > 0)
                        {

                            for (int j = 0; j < dtDoc.Rows.Count; j++)
                            {
                                dynamic ObjTestDetail = new JObject();
                                ObjTestDetail.Createddate = dtDoc.Rows[j]["Createddate"];
                                ObjTestDetail.Note = dtDoc.Rows[j]["Note"];
                                Result.DocNote.Add(ObjTestDetail); //Add Doctor details to array
                            }
                        }
                        Result.Status = true;
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
        [Route("AddOldReportNote")]
        public string AddOldReportNote([FromBody] ReportNote model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object

            try
            {
                string Msg = "";
                if (!Ival.IsInteger(model.ReportId.ToString()))
                {
                    Msg += "Please Enter Valid ReportId";
                }
                if (Ival.IsTextBoxEmpty(model.Note))
                {
                    Msg += "Please Enter Valid Note";
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
                        new SqlParameter("@UserId",UserId),
                        new SqlParameter("@Notes",model.Note),
                        new SqlParameter("@ReportsId",model.ReportId),
                        new SqlParameter("@returnval",SqlDbType.Int)
                    };
                    data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_AddOldReportComment", param);
                    if (data == 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Note added successfully.";
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

        [HttpPost]
        [Route("AllOldSharedReportList")]
        public string AllOldSharedReportList(PagingParameterModel pagingparametermode)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                SqlParameter[] param = new SqlParameter[]
                {
                    new SqlParameter("@DoctorId",UserId),
                    new SqlParameter("@SearchingText",pagingparametermode.Searching)
                };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetAllOldSharedReportList", param);
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

                    // Returns List of Reports after applying Paging   
                    var items = dt.Select().Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();


                    Result.ReportList = new JArray() as dynamic;   // Create Array for Report Details

                    for (int j = 0; j < items.Count; j++)
                    {
                        dynamic ObjReportDetail = new JObject();
                        ObjReportDetail.Flag = "OldReport";
                        ObjReportDetail.SharedID = items[j]["SharedID"];
                        ObjReportDetail.TestName = items[j]["TestName"];
                        ObjReportDetail.TestDate = items[j]["TestDate"];
                        ObjReportDetail.RefDoctorName = items[j]["RefDoctorName"];
                        ObjReportDetail.ReportId = items[j]["ReportId"];
                        ObjReportDetail.Notes = items[j]["Notes"];
                        ObjReportDetail.PatientName = items[j]["sFullName"];
                        ObjReportDetail.PatientId = items[j]["PatientId"];
                        ObjReportDetail.PatientPic = items[j]["sImagePath"];
                        ObjReportDetail.PatientMobile = items[j]["sMobile"];
                        ObjReportDetail.PatientEmail = items[j]["sEmailId"];
                        ObjReportDetail.SharedDate = items[j]["CreatedDate"];
                        ObjReportDetail.ReportPath = items[j]["FilePath"];
                        Result.ReportList.Add(ObjReportDetail); //Add Report details to array
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
                    Result.Msg = "No Record Found";
                    JSONString = JsonConvert.SerializeObject(Result);
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        [HttpPost]
        [Route("OldSharedReportListwithStatus")]
        public string OldSharedReportListwithStatus([FromBody] SharedReportListforDoctor model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
                if (Ival.IsTextBoxEmpty(model.Status))
                {
                    Msg += "Please Enter Valid Status";
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
                            new SqlParameter("@DoctorId",UserId),
                            new SqlParameter("@Status",model.Status),
                            new SqlParameter("@SearchingText",model.Searching)
                        };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetOldReportListwithStatus", param);
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

                        // Returns List of Reports after applying Paging   
                        var items = dt.Select().Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

                        Result.ReportList = new JArray() as dynamic;   // Create Array for Report Details

                        for (int j = 0; j < items.Count; j++)
                        {
                            dynamic ObjReportDetail = new JObject();
                            ObjReportDetail.Flag = "OldReport";
                            ObjReportDetail.SharedReportId = items[j]["ID"];
                            ObjReportDetail.PatientId = items[j]["UserId"];
                            ObjReportDetail.TestName = items[j]["TestName"];
                            ObjReportDetail.TestDate = items[j]["TestDate"];
                            ObjReportDetail.RefDoctorName = items[j]["RefDoctorName"];
                            ObjReportDetail.Notes = items[j]["Notes"];
                            ObjReportDetail.LabName = items[j]["LabName"];
                            ObjReportDetail.ReportId = items[j]["ReportId"];
                            ObjReportDetail.PatientName = items[j]["sFullName"];
                            ObjReportDetail.SharedDate = items[j]["CreatedDate"];
                            ObjReportDetail.Status = items[j]["Status"];
                            ObjReportDetail.ProfilePic = items[j]["sImagePath"];
                            Result.ReportList.Add(ObjReportDetail); //Add Report details to array
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
                        Result.Msg = "No Record Found";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        [HttpPost]
        [Route("UpdateOldSharedReportStatus")]
        public string UpdateOldSharedReportStatus([FromBody] DoctorSharedReport model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
                if (Ival.IsTextBoxEmpty(model.ReportId))
                {
                    Msg += "Please Enter Valid ReportId";
                }
                if (Ival.IsTextBoxEmpty(model.Status))
                {
                    Msg += "Please Enter Valid Status";
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
                    string _repotId = model.ReportId.TrimEnd(',');
                    string[] _reportIdArray = _repotId.Split(',');
                    foreach (string reportId in _reportIdArray)
                    {
                        var _testId = _reportIdArray;
                        SqlParameter[] param = new SqlParameter[]
                        {
                        new SqlParameter("@UserId",UserId),
                        new SqlParameter("@ReportId",reportId),
                        new SqlParameter("@Status",model.Status),
                        new SqlParameter("@returnval",SqlDbType.Int)
                        };
                        int data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_UpdateOldSharedReportStatus", param);
                        if (data == 1)
                        {
                            Result.Status = true;  //  Status Key 
                            Result.Msg = "Report status updated successfully";
                            JSONString = JsonConvert.SerializeObject(Result);
                        }
                        else
                        {
                            Result.Status = false;  //  Status Key 
                            Result.Msg = "Something went wrong,Please try again.";
                            JSONString = JsonConvert.SerializeObject(Result);
                        }
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

        [HttpPost]
        [Route("PatientOldReportDetails")]
        public string PatientOldReportDetails([FromBody] DoctorSharedReportDetails model)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Createroot JSON Object
            try
            {
                string Msg = "";
                if (!Ival.IsInteger(model.ReportId.ToString()))
                {
                    Msg += "Please Enter Valid Report Id";
                }
                if (!Ival.IsInteger(model.UserId.ToString()))
                {
                    Msg += "Please Enter Valid Patient Id";
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
                                    new SqlParameter("@UserId",model.UserId),
                                    new SqlParameter("@ReportId",model.ReportId)
                           };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetOldSharedReportDetails", param);

                    if (dt.Rows.Count > 0)
                    {

                        Result.ReportData = new JArray() as dynamic;   // Create Array for Baner Details
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            dynamic ObjReportDetail = new JObject();
                            ObjReportDetail.SharedReportId = dt.Rows[j]["ID"];
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
                             new SqlParameter("@ReportID",model.ReportId),
                             new SqlParameter("@UserId",UserId)
                       };
                        DataTable dtDoc = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetDoctorNoteListsBySharedOldReport ", paramdoc);
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

        [HttpGet]
        [Route("OldSharedReportListforDashboard")]
        public string OldSharedReportListforDashboard()
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
                DataTable dt = DAL.GetDataTable("WS_Sp_GetAllOldSharedReportListForDashboard " + UserId);
                if (dt.Rows.Count > 0)
                {
                    Result.ReportList = new JArray() as dynamic;   // Create Array for List Details
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjReportDetail = new JObject();
                        ObjReportDetail.Flag = "OldReport";
                        ObjReportDetail.SharedID = dt.Rows[j]["SharedID"];
                        ObjReportDetail.TestName = dt.Rows[j]["TestName"];
                        ObjReportDetail.TestDate = dt.Rows[j]["TestDate"];
                        ObjReportDetail.RefDoctorName = dt.Rows[j]["RefDoctorName"];
                        ObjReportDetail.ReportId = dt.Rows[j]["ReportId"];
                        ObjReportDetail.Notes = dt.Rows[j]["Notes"];
                        ObjReportDetail.PatientName = dt.Rows[j]["sFullName"];
                        ObjReportDetail.PatientId = dt.Rows[j]["PatientId"];
                        ObjReportDetail.PatientPic = dt.Rows[j]["sImagePath"];
                        ObjReportDetail.PatientMobile = dt.Rows[j]["sMobile"];
                        ObjReportDetail.PatientEmail = dt.Rows[j]["sEmailId"];
                        ObjReportDetail.SharedDate = dt.Rows[j]["CreatedDate"];
                        ObjReportDetail.ReportPath = dt.Rows[j]["FilePath"];
                        Result.ReportList.Add(ObjReportDetail); //Add Report details to array
                    }
                    Result.Status = true;  //  Status Key
                    Result.Msg = "Success";
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
                Result.Msg = "Something went wromg,Please try again";
                JSONString = JsonConvert.SerializeObject(Result);

            }
            return JSONString;
        }

        [HttpGet]
        [Route("GetOldReportListSharedwithDoctor/{PatientId}")]
        public string GetOldReportListSharedwithDoctor(int PatientId)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {

                string Msg = "";
                if (!Ival.IsInteger(PatientId.ToString()))
                {
                    Msg += "Please Enter Valid Patient Id";
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
                        new SqlParameter("@PatientId",PatientId),
                        new SqlParameter("@DoctorId",UserId)
                 };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetOldReportListSharedwithDoctor", param);
                    if (dt.Rows.Count > 0)
                    {
                        Result.Status = true;  //  Status Key
                        Result.TestList = new JArray() as dynamic;   // Create Array for Doctor Details



                        //for (int j = 0; j < dt.Rows.Count; j++)
                        //{
                        //    dynamic ObjTestDetail = new JObject();
                        //    ObjTestDetail.TestCode = dt.Rows[j]["sTestCode"];
                        //    ObjTestDetail.TestName = dt.Rows[j]["sTestName"];
                        //    ObjTestDetail.BookingId = dt.Rows[j]["sBookLabId"];
                        //    ObjTestDetail.ReportId = dt.Rows[j]["sBookLabTestId"];
                        //    ObjTestDetail.SharedReportId = dt.Rows[j]["sSharedReportId"];
                        //    ObjTestDetail.TestDate = dt.Rows[j]["sTestDate"];
                        //    ObjTestDetail.TimeSlot = dt.Rows[j]["sTimeSlot"];
                        //    ObjTestDetail.ProfileName = dt.Rows[j]["sProfileName"];
                        //    ObjTestDetail.LabName = dt.Rows[j]["sLabName"];
                        //    ObjTestDetail.LabLogo = dt.Rows[j]["sLabLogo"];
                        //    ObjTestDetail.LabContact = dt.Rows[j]["sLabContact"];
                        //    Result.TestList.Add(ObjTestDetail); //Add Doctor details to array
                        //}



                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            dynamic ObjTestDetail = new JObject();

                            ObjTestDetail.Flag = "OldReport";
                            ObjTestDetail.SharedReportId = dt.Rows[j]["SharedReportID"];
                            ObjTestDetail.TestName = dt.Rows[j]["TestName"];
                            ObjTestDetail.TestDate = dt.Rows[j]["TestDate"];
                            //ObjTestDetail.RefDoctorName = dt.Rows[j]["RefDoctorName"];                           
                            //ObjTestDetail.Notes = dt.Rows[j]["Notes"];
                            //ObjTestDetail.LabCoReportPathntact = dt.Rows[j]["FilePath"];
                            ObjTestDetail.LabName = dt.Rows[j]["LabName"];

                            //ObjTestDetail.PatientName = dt.Rows[j]["sFullName"];
                            //ObjTestDetail.PatientId = dt.Rows[j]["PatientId"];
                            //ObjTestDetail.PatientPic = dt.Rows[j]["sImagePath"];
                            //ObjTestDetail.PatientMobile = dt.Rows[j]["sMobile"];
                            //ObjTestDetail.PatientEmail = dt.Rows[j]["sEmailId"];
                            ObjTestDetail.ReportId = dt.Rows[j]["ReportId"];




                            Result.TestList.Add(ObjTestDetail); //Add Doctor details to array
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


        //[HttpPost]
        //[Route("AddNewPatient")]
        //public string AddNewPatient([FromBody] AddNewPatient model)
        //{
        //    string JSONString = string.Empty; // Create string object to return final output
        //    dynamic Result = new JObject();  //Create root JSON Object
        //    string Msg = "";
        //    var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;

        //    try
        //    {
        //        if (!Ival.IsCharOnly(model.FullName))
        //        {
        //            Msg += "Please Enter Valid Full Name";
        //        }
        //        if (!Ival.IsCharOnly(model.Gender) && model.Gender != "Male" || model.Gender != "Female")
        //        {

        //                 Msg += "Please Enter Valid Gender";
        //        }
        //        if (!Ival.IsValidDate(model.BirthDate))
        //        {
        //            Msg += "Please Enter Valid Birth Date";
        //        }

        //        //if (Ival.IsNumeric(model.Age))
        //        //{
        //        //    if (!Ival.AgeValidation(model.Age))
        //        //    {
        //        //        Msg += "Please Enter Valid Age";
        //        //    }
        //        //}
        //        //else
        //        //{
        //        //    Msg += "Please Enter Valid Age";
        //        //}


        //        if (!Ival.IsTextBoxEmpty(model.EmailId))
        //        {
        //            if (!Ival.IsValidEmailAddress(model.EmailId))
        //            {
        //                Msg += " Please Enter Valid Email Id";
        //            }
        //        }
        //        if (!Ival.IsInteger(model.HealthId))
        //        {
        //            Msg += "Please Enter Valid Health Id";
        //        }


        //            if (!Ival.IsTextBoxEmpty(model.Mobile))
        //        {
        //            if (Ival.IsInteger(model.Mobile))
        //            {
        //                if (!Ival.MobileValidation(model.Mobile))
        //                {
        //                    Msg += "Please Enter Valid Mobile Number";
        //                }
        //            }
        //        }
        //        else
        //        {
        //            Msg += "Please Enter Valid Mobile Number";
        //        }
        //        if (!Ival.IsTextBoxEmpty(model.Aadharnumber))
        //        {
        //            if (Ival.IsInteger(model.Aadharnumber))
        //            {
        //                if (!Ival.AadharValidation(model.Aadharnumber))
        //                {
        //                    Msg += "Please Enter Valid Aadhar Number";
        //                }
        //            }
        //            else
        //            {
        //                Msg += "Please Enter Valid Aadhar Number";
        //            }
        //        }
        //        //if (!Ival.ValidatePassword(model.Password.ToString()))
        //        //{
        //        //    Msg += "Please enter Minimum 6 characters at least 1 Uppercase Alphabet, 1 Lowercase Alphabet, 1 Number and 1 Special Character";
        //        //}
        //        if (!Ival.IsInteger(model.Pincode.ToString()))
        //        {
        //            if (!Ival.PincodeValidation(model.Pincode))

        //                Msg += "Please Enter Valid Pincode";
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
        //            var _mobile = (model.Mobile != "") ? CryptoHelper.Encrypt(model.Mobile) : "";
        //            var _emailId = (model.EmailId != "") ? CryptoHelper.Encrypt(model.EmailId.ToLower()) : "";
        //            //var _password = CryptoHelper.Encrypt(model.Password);
        //            var _Aadhar = (model.Aadharnumber != "") ? CryptoHelper.Encrypt(model.Aadharnumber) : "";
        //            var _HealthId = (model.HealthId != "") ? CryptoHelper.Encrypt(model.HealthId) : "";
        //            SqlParameter[] param = new SqlParameter[]
        //            {
        //            new SqlParameter("@UserName",model.FullName),
        //            new SqlParameter("@Gender",model.Gender),
        //            new SqlParameter("@DOB",model.BirthDate),
        //              //  new SqlParameter("@Age",model.Age),
        //            new SqlParameter("@Contact",_mobile),
        //            new SqlParameter("@Email",_emailId),
        //            new SqlParameter("@HealthId",_HealthId),
        //            new SqlParameter("@Aadharnumber",_Aadhar),
        //            new SqlParameter("@RegisterFrom","Mobile"),
        //            new SqlParameter("@PinCode",model.Pincode),
        //            //new SqlParameter("@ChannelPartnerCode",model.ChannelPartnerCode),
        //            new SqlParameter("@returnval",SqlDbType.Int)
        //            };
        //            int data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_DocterPatientRegistration", param);
        //            if (data == 0)
        //            {
        //                Result.Status = false;  //  Status Key 
        //                Result.Msg = "Something went wrong,Please try again.";
        //                JSONString = JsonConvert.SerializeObject(Result);
        //            }
        //            else if (data == -2)
        //            {
        //                Result.Status = false;  //  Status Key
        //                Result.Msg = "Mobile number already exists";
        //                JSONString = JsonConvert.SerializeObject(Result);
        //            }
        //            else if (data == -1)
        //            {
        //                Result.Status = false;  //  Status Key
        //                Result.Msg = "Email Id already exists";
        //                JSONString = JsonConvert.SerializeObject(Result);
        //            }
        //            else if (data == -4)
        //            {
        //                Result.Status = false;  //  Status Key
        //                Result.Msg = "Email Id & Mobile number already exists";
        //                JSONString = JsonConvert.SerializeObject(Result);
        //            }
        //            //else if (data == -5)
        //            //{
        //            //    Result.Status = false;  //  Status Key
        //            //    Result.Msg = "Please Enter Valid Channel Partner Code";
        //            //    JSONString = JsonConvert.SerializeObject(Result);
        //            //}
        //            else
        //            {
        //                SqlParameter[] param2 = new SqlParameter[]
        //                    {
        //                    new SqlParameter("@UserId",data),
        //                    new SqlParameter("@Mobile",_mobile),
        //                    new SqlParameter("@EmailId",_emailId),
        //                    new SqlParameter("@Role","Patient"),
        //                    //new SqlParameter("@Password",_password),
        //                    new SqlParameter("@UserName",""),
        //                  //new SqlParameter("@loginStatus","A"),
        //                    new SqlParameter("@Returnval",SqlDbType.Int)
        //                    };
        //                int ResultVal1 = DAL.ExecuteStoredProcedureRetnInt("Sp_AddNewPatient", param2);
        //                // here add data in addptientDoctor Table
        //                SqlParameter[] paramPatient = new SqlParameter[]
        //              {
        //                        new SqlParameter("@DoctorId",UserId),
        //                        new SqlParameter("@PatientId",data),
        //                    new SqlParameter("@Returnval",SqlDbType.Int)
        //              };
        //                int data1 = DAL.ExecuteStoredProcedureRetnInt("WS_Sp__AddPatientInDoctorList", paramPatient);


        //                //if (ResultVal1 == 1)
        //                //{
        //                //    var user = _authenticateService.Authenticate(_mobile, _password);
        //                //    Result.Status = true;  //  Status Key 
        //                //    Result.Msg = "Thank you for information. We have sent OTP on your registered mobile number for verification";
        //                //    Result.Name = user.Name;
        //                //    Result.Token = user.Token;
        //                //    Result.Role = user.Role;
        //                //    Result.MobileVerified = user.MobileVerified;
        //                //    Result.UserId = user.UserId;

        //                //    if (user.MobileVerified == false)
        //                //    {
        //                //        SendOTP _otp = new SendOTP();

        //                //        if (Ival.IsInteger(model.Mobile))
        //                //        {
        //                //            if (!Ival.MobileValidation(model.Mobile))
        //                //            {
        //                //                int StatusCode = _otp.sendOTP(model.Mobile);
        //                //                if (StatusCode == 200)
        //                //                {
        //                //                    Result.OTPSend = true;
        //                //                }
        //                //                else
        //                //                {
        //                //                    Result.OTPSend = false;
        //                //                }
        //                //            }
        //                //        }
        //                //    }
        //                //    JSONString = JsonConvert.SerializeObject(Result);
        //                //}

        //                Result.Status = true;
        //                Result.Msg = "Patient Added Successfully..";
        //                JSONString = JsonConvert.SerializeObject(Result);
        //            }
        //            return JSONString;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Result.Status = false;  //  Status Key
        //        Result.Msg = "Something went wrong,Please try again."; ;
        //        JSONString = JsonConvert.SerializeObject(Result);
        //        return JSONString;
        //    }
        //}


        [HttpPost]
        [Route("AddNewPatient")]
        public string AddNewPatient([FromBody] AddNewPatient model)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "",doctorNm;
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;

            try
            {
                if (!Ival.IsTextBoxEmpty(model.FullName))
                {
                    if (!Ival.IsCharOnly(model.FullName))
                    {
                        Msg += "Please Enter Valid Full Name";
                    }
                }
                else
                {
                    Msg += "Please Enter Valid Name";
                }


                if (!Ival.IsTextBoxEmpty(model.Gender))
                {
                    if (!Ival.IsCharOnly(model.Gender))
                    {
                        if (Ival.GenderValidation(model.Gender))
                        {
                            Msg += "Please Enter Valid Gender";
                        }
                    }
                    
                }
                else
                {
                    Msg += "Please Enter Valid Gender";
                }
                if (!Ival.IsTextBoxEmpty(model.BirthDate))
                {
                    if (!Ival.IsValidDate(model.BirthDate))
                    {
                        Msg += "Please Enter Valid Birth Date";
                    }
                }

                //if (!Ival.IsValidDate(model.BirthDate))
                //{
                //    Msg += "Please Enter Valid Birth Date";
                //}

                if (Ival.IsNumeric(model.Age))
                {
                    if (!Ival.AgeValidation(model.Age))
                    {
                        Msg += "Please Enter Valid Age";
                    }
                }
                else
                {
                    Msg += "Please Enter Valid Age";
                }


                if (!Ival.IsTextBoxEmpty(model.EmailId))
                {
                    if (!Ival.IsValidEmailAddress(model.EmailId))
                    {
                        Msg += " Please Enter Valid Email Id";
                    }
                }
                if (!Ival.IsTextBoxEmpty(model.HealthId))
                {
                    if (!Ival.IsInteger(model.HealthId))
                    {
                        Msg += "Please Enter Valid Health Id";
                    }
                }


                if (!Ival.IsTextBoxEmpty(model.Mobile))
                {
                    if (Ival.IsInteger(model.Mobile))
                    {
                        if (!Ival.MobileValidation(model.Mobile))
                        {
                            Msg += "Please Enter Valid Mobile Number";
                        }
                    }
                }
                else
                {
                    Msg += "Please Enter Valid Mobile Number";
                }
                if (!Ival.IsTextBoxEmpty(model.Aadharnumber))
                {
                    if (Ival.IsInteger(model.Aadharnumber))
                    {
                        if (!Ival.AadharValidation(model.Aadharnumber))
                        {
                            Msg += "Please Enter Valid Aadhar Number";
                        }
                    }
                    else
                    {
                        Msg += "Please Enter Valid Aadhar Number";
                    }
                }
                //if (!Ival.ValidatePassword(model.Password.ToString()))
                //{
                //    Msg += "Please enter Minimum 6 characters at least 1 Uppercase Alphabet, 1 Lowercase Alphabet, 1 Number and 1 Special Character";
                //}
                if (!Ival.IsTextBoxEmpty(model.Pincode))
                {
                    if (Ival.IsInteger(model.Pincode))
                    {
                        if (!Ival.PincodeValidation(model.Pincode))
                        {
                            Msg += "Please Enter Valid Pincode";
                        }
                    }
                    else
                    {
                        Msg += "Please Enter Valid Pincode";
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
                    var _mobile = (model.Mobile != "") ? CryptoHelper.Encrypt(model.Mobile) : "";
                    var _emailId = (model.EmailId != "") ? CryptoHelper.Encrypt(model.EmailId.ToLower()) : "";
                    //var _password = CryptoHelper.Encrypt(model.Password);
                    var _Aadhar = (model.Aadharnumber != "") ? CryptoHelper.Encrypt(model.Aadharnumber) : "";
                    var _HealthId = (model.HealthId != "") ? CryptoHelper.Encrypt(model.HealthId) : "";
                    SqlParameter[] param = new SqlParameter[]
                    {
                    new SqlParameter("@UserName",model.FullName),
                    new SqlParameter("@Gender",model.Gender),
                    new SqlParameter("@DOB",model.BirthDate),
                    new SqlParameter("@Age",model.Age),
                    new SqlParameter("@Address",model.Address),

                    new SqlParameter("@Contact",_mobile),
                    new SqlParameter("@Email",_emailId),
                    new SqlParameter("@HealthId",_HealthId),
                    new SqlParameter("@Aadharnumber",_Aadhar),
                    new SqlParameter("@RegisterFrom","Mobile"),
                    new SqlParameter("@PinCode",model.Pincode),
                    //new SqlParameter("@ChannelPartnerCode",model.ChannelPartnerCode),
                    new SqlParameter("@returnval",SqlDbType.Int)
                    };
                    int data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_DocterPatientRegistration", param);
                    if (data == 0)
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "Something went wrong,Please try again.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else if (data == -2)
                    {
                        Result.Status = false;  //  Status Key
                        Result.Msg = "Mobile number already exists";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else if (data == -1)
                    {
                        Result.Status = false;  //  Status Key
                        Result.Msg = "Email Id already exists";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else if (data == -4)
                    {
                        Result.Status = false;  //  Status Key
                        Result.Msg = "Email Id & Mobile number already exists";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else if (data == -3)
                    {
                        Result.Status = false;  //  Status Key
                        Result.Msg = "User already exists";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    //else if (data == -5)
                    //{
                    //    Result.Status = false;  //  Status Key
                    //    Result.Msg = "Please Enter Valid Channel Partner Code";
                    //    JSONString = JsonConvert.SerializeObject(Result);
                    //}
                    else
                    {
                        SqlParameter[] param2 = new SqlParameter[]
                            {
                            new SqlParameter("@UserId",data),
                            new SqlParameter("@Mobile",_mobile),
                            new SqlParameter("@EmailId",_emailId),
                            new SqlParameter("@Role","Patient"),
                            //new SqlParameter("@Password",_password),
                            new SqlParameter("@UserName",""),
                          //new SqlParameter("@loginStatus","A"),
                            new SqlParameter("@Returnval",SqlDbType.Int)
                            };
                        int ResultVal1 = DAL.ExecuteStoredProcedureRetnInt("Sp_AddNewPatient", param2);
                        // here add data in addptientDoctor Table
                        SqlParameter[] paramPatient = new SqlParameter[]
                      {
                                new SqlParameter("@DoctorId",UserId),
                                new SqlParameter("@PatientId",data),
                            new SqlParameter("@Returnval",SqlDbType.Int)
                      };
                        int data1 = DAL.ExecuteStoredProcedureRetnInt("WS_Sp__AddPatientInDoctorList", paramPatient);

                       // here add data in addptientDoctor Table
                        SqlParameter[] paramDoctor = new SqlParameter[]
                      {
                                new SqlParameter("@DoctorId",UserId),
                                new SqlParameter("@PatientId",data),
                            new SqlParameter("@Returnval",SqlDbType.Int)
                      };
                        int data2 = DAL.ExecuteStoredProcedureRetnInt("WS_Sp__AddDoctorInMylist", paramDoctor);
                        DataTable dt = DAL.GetDataTable("SP_getDoctorName " + UserId);
                        if (data2==1)
                        {
                           doctorNm= dt.Rows[0]["sFullName"].ToString();
                            SendOTP _otp = new SendOTP();
                            _otp.InvationSMSToPatient(model.Mobile,doctorNm,model.FullName);
                        }

                        //if (ResultVal1 == 1)
                        //{
                        //    var user = _authenticateService.Authenticate(_mobile, _password);
                        //    Result.Status = true;  //  Status Key 
                        //    Result.Msg = "Thank you for information. We have sent OTP on your registered mobile number for verification";
                        //    Result.Name = user.Name;
                        //    Result.Token = user.Token;
                        //    Result.Role = user.Role;
                        //    Result.MobileVerified = user.MobileVerified;
                        //    Result.UserId = user.UserId;

                        //    if (user.MobileVerified == false)
                        //    {
                        //        SendOTP _otp = new SendOTP();

                        //        if (Ival.IsInteger(model.Mobile))
                        //        {
                        //            if (!Ival.MobileValidation(model.Mobile))
                        //            {
                        //                int StatusCode = _otp.sendOTP(model.Mobile);
                        //                if (StatusCode == 200)
                        //                {
                        //                    Result.OTPSend = true;
                        //                }
                        //                else
                        //                {
                        //                    Result.OTPSend = false;
                        //                }
                        //            }
                        //        }
                        //    }
                        //    JSONString = JsonConvert.SerializeObject(Result);
                        //}

                        Result.Status = true;
                        Result.Msg = "Patient Added Successfully..";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    return JSONString;
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again."; ;
                JSONString = JsonConvert.SerializeObject(Result);
                return JSONString;
            }
        }

    }
}
