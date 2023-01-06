using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using VLS_API.Model;
using VLS_API.Services;
using Validation;
using Howzu_API.Services;
using Howzu_API.Model;

namespace VLS_API.Controllers
{
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    [Route("[controller]")]
    [ApiController]
   // [Authorize]
    public class EnterpriseController : ControllerBase
    {
        DataAccessLayer DAL = new DataAccessLayer();
        InputValidation Ival = new InputValidation();
        /// <summary>
        /// Get Employee Details By Employee id
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("EmployeeMyProfile")]
        public string EmployeeMyProfile()
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                if (UserId != null)
                {
                    DataTable dt = DAL.GetDataTable("WS_Sp_GetEmployeePersonalDetail " + UserId);
                    if (dt.Rows.Count > 0)
                    {
                        Result.UserId = dt.Rows[0]["UserId"];
                        Result.FullName = dt.Rows[0]["FullName"];
                        Result.Mobile = dt.Rows[0]["Mobile"];
                        Result.Mobile = dt.Rows[0]["EmailId"];
                        Result.EmailId = dt.Rows[0]["EmailId"];
                        Result.AadharCard = dt.Rows[0]["AadharCard"];
                        Result.Gender = dt.Rows[0]["Gender"];
                        Result.DOB = dt.Rows[0]["DOB"];
                        Result.Address = dt.Rows[0]["Address"];
                        Result.Country = dt.Rows[0]["Country"];
                        Result.Pincode = dt.Rows[0]["Pincode"];
                        Result.City = dt.Rows[0]["City"];
                        Result.State = dt.Rows[0]["State"];
                        Result.ProfilePic = dt.Rows[0]["ProfilePic"];
                        Result.Msg = "Success";
                        Result.HealthId = dt.Rows[0]["HealthId"];
                        Result.EmployeeId = dt.Rows[0]["EmployeeId"];
                        Result.Org_Name = dt.Rows[0]["Name"];
                        Result.BranchName = dt.Rows[0]["BranchName"];
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

        [HttpGet]
        [Route("OrgnizationDetails")]
        public string OrgnizationDetails()
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                if (UserId != null)
                {
                    DataTable dt = DAL.GetDataTable("WS_Sp_GetOrgnizationDetail " + UserId);
                    if (dt.Rows.Count > 0)
                    {
                        Result.Org_id = dt.Rows[0]["Org_id"];
                        Result.OrgName = dt.Rows[0]["Name"];
                        Result.orgAddress = dt.Rows[0]["orgAddress"];
                        Result.orgContact = dt.Rows[0]["orgContact"];
                        Result.Org_Details = dt.Rows[0]["Org_Details"];
                        Result.Org_Logo = dt.Rows[0]["Org_Logo"];
                        Result.Branch_ID = dt.Rows[0]["Branch_ID"];
                        Result.BranchName = dt.Rows[0]["BranchName"];
                        Result.BranchAddress = dt.Rows[0]["BranchAddress"];
                        Result.BranchContact = dt.Rows[0]["BranchContact"];
                        Result.BranchEmail = dt.Rows[0]["BranchEmail"];
                        Result.Msg = "Success";
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

        /// <summary>
        /// Update Employee Details
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateEmployee")]
        public string UpdateEmployee([FromBody] Employee model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                SqlParameter[] param = new SqlParameter[]
            {
                    new SqlParameter("@UserId",UserId),
                    new SqlParameter("@FullName",model.FullName),
                    new SqlParameter("@Contact",model.Mobile),
                    new SqlParameter("@EmailId",model.EmailId),
                    new SqlParameter("@Address",model.Address),
                    new SqlParameter("@Gender",model.Gender),
                    new SqlParameter("@BirthDate",model.BirthDate),
                    new SqlParameter("@Country",model.Country),
                    new SqlParameter("@Pincode",model.Pincode),
                    new SqlParameter("@City",model.City),
                    new SqlParameter("@State",model.State),
                    new SqlParameter("@ImagePath",model.ProfileIamge),
                    new SqlParameter("@EmployeeId",model.EmployeeId),
                    new SqlParameter("@AadharCard",model.AadharCard),
                    new SqlParameter("@returnval",SqlDbType.Int),
            };
                int data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_UpdateEmployee", param);
                if (data == 1)
                {
                    Result.Status = true;  //  Status Key 
                    Result.Msg = "Employee details update successfully";
                    JSONString = JsonConvert.SerializeObject(Result);
                }
                else
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = "Something went wrong,Please try again.";
                    JSONString = JsonConvert.SerializeObject(Result);
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

        /// <summary>
        /// Get Lab list by orgnization Id
        /// </summary>
        /// <param name="OrgId">Mandatory</param>
        /// <param name="userId">Mandatory</param>
        /// <returns></returns>
        [HttpGet]
        [Route("EnterPriseLabList/{OrgId}")]
        public string EnterPriseLabList(int OrgId,int userId)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            userId =int.Parse(UserId.ToString());
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                SqlParameter[] param = new SqlParameter[]
                {
                    new SqlParameter("@orgId",OrgId),
                    new SqlParameter("@userId",userId),
                };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetOrglabList" ,param);
               // DataTable dt = DAL.GetDataTable("WS_Sp_GetOrglabList ", +OrgId);
                if (dt.Rows.Count > 0)
                {
                    Result.LabList = new JArray() as dynamic;   // Create Array for Lab Details
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjLabDetail = new JObject();
                        ObjLabDetail.LabId = dt.Rows[j]["sLabId"];
                        ObjLabDetail.LabCode = dt.Rows[j]["sLabCode"];
                        ObjLabDetail.LabName = dt.Rows[j]["sLabName"];
                        ObjLabDetail.LabManager = dt.Rows[j]["sLabManager"];
                        ObjLabDetail.LabEmailId = dt.Rows[j]["sLabEmailId"];
                        ObjLabDetail.LabContact = dt.Rows[j]["sLabContact"];
                        ObjLabDetail.LabAddress = dt.Rows[j]["sLabAddress"];
                        ObjLabDetail.LabLocation = dt.Rows[j]["sLabLocation"];
                        ObjLabDetail.LabDetails = dt.Rows[j]["sLabDetails"];
                        ObjLabDetail.LabLogo = dt.Rows[j]["sLabLogo"];
                        Result.LabList.Add(ObjLabDetail); //Add lab details to array
                    }
                    Result.Status = true;  //  Status Key
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
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        /// <summary>
        /// Get EnterPrise Test List
        /// </summary>
        /// <param name="OrgId">Mandatory</param>
        /// <returns></returns>
        [HttpGet]
        [Route("EnterPriseLabTestList/{OrgId}")]
        public string EnterPriseLabTestList(int OrgId)
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                DataTable dt = DAL.GetDataTable("WS_Sp_GetOrglabTestList " + OrgId);

                if (dt.Rows.Count > 0)
                {
                    Result.TestList = new JArray() as dynamic;   // Create Array for Lab Details
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjTestDetail = new JObject();
                        ObjTestDetail.TestId = dt.Rows[j]["sTestId"];
                        ObjTestDetail.TestCode = dt.Rows[j]["sTestCode"];
                        ObjTestDetail.TestName = dt.Rows[j]["sTestName"];
                        ObjTestDetail.TestGroup = dt.Rows[j]["sTestGroup"];
                        ObjTestDetail.TestUsefulFor = dt.Rows[j]["sTestUsefulFor"];
                        ObjTestDetail.TestInterpretation = dt.Rows[j]["sTestInterpretation"];
                        ObjTestDetail.TestLimitation = dt.Rows[j]["sTestLimitation"];
                        ObjTestDetail.TestClinicalReferance = dt.Rows[j]["sTestClinicalReferance"];
                        ObjTestDetail.TestProfileId = dt.Rows[j]["sTestProfileId"];
                        ObjTestDetail.ProfileName = dt.Rows[j]["sProfileName"];
                        Result.TestList.Add(ObjTestDetail); //Add lab details to array
                    }
                    Result.Status = true;  //  Status Key
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
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        /// <summary>
        /// Upload users report.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UploadReport")]
        public string UploadReport([FromBody] UploadReport model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                int data = 0;
                string[] splitReports = model.ReportPath.ToString().Split(',');
                foreach (string Reports in splitReports)
                {
                    SqlParameter[] param = new SqlParameter[]
                        {
                                 new SqlParameter("@EmpId",UserId),
                                 new SqlParameter("@ReportPath",Reports),
                                 new SqlParameter("@Returnval",SqlDbType.Int)
                        };
                    data = DAL.ExecuteStoredProcedureRetnInt("Sp_AddEmployeeReport", param);
                }
                if (data == 1)
                {
                    Result.Status =true;  //  Status Key 
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
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }
        /// <summary>
        /// Delete Test From List
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("DeleteTest")]
        public string DeleteTest(int ReportId)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                SqlParameter[] param = new SqlParameter[]
            {
                    new SqlParameter("@UserId",UserId),
                     new SqlParameter("@reportId",ReportId),

                    new SqlParameter("@returnval",SqlDbType.Int),
            };
                int data = DAL.ExecuteStoredProcedureRetnInt("SP_DeleteTest", param);
                if (data == 1)
                {
                    Result.Status = true;  //  Status Key 
                    Result.Msg = "Test Deleted successfully";
                    JSONString = JsonConvert.SerializeObject(Result);
                }
                else
                {
                    Result.Status = false;  //  Status Key 
                    Result.Msg = "Something went wrong,Please try again.";
                    JSONString = JsonConvert.SerializeObject(Result);
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
        [Route("EmployeeReportUploadhistory")]
        public string EmployeeReportUploadhistory()
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                DataTable dt = DAL.GetDataTable("WS_Sp_GetUploadReportListByEmp_Id " + UserId);
                if (dt.Rows.Count > 0)
                {
                    Result.ReportList = new JArray() as dynamic;   // Create Array for Report Details
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjReportDetail = new JObject();
                        ObjReportDetail.Id = dt.Rows[j]["ID"];
                        ObjReportDetail.ReportPath = dt.Rows[j]["ReportPath"];
                        ObjReportDetail.Status = dt.Rows[j]["Status"];
                        ObjReportDetail.UploadDate = dt.Rows[j]["UploadDate"];
                        Result.ReportList.Add(ObjReportDetail); //Add report details to array
                    }
                    Result.Status = true;  //  Status Key
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
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }




        [HttpPost]
        [Route("healthCampRegisterUserList")]
        public string healthCampRegisterUserList([FromBody] PagingParameterModel pagingparametermodel)
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            Result.MyDetails = new JArray() as dynamic;
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            try
            {
                if (UserId != null)
                {
                    SqlParameter[] param = new SqlParameter[]
                  {
                      new SqlParameter("@UserId",UserId),
                      new SqlParameter("@SearchingText",pagingparametermodel.Searching)
                  };

                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("sp_healthCampRegisterUserList " ,param);
                   
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

                        // Returns List of Reports after applying Paging   
                        var items = dt.Select().Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();


                        Result.ReportList = new JArray() as dynamic;   // Create Array for Report Details

                        foreach (DataRow row in dt.Rows)
                        {
                            dynamic ObjDoctorDetail = new JObject();

                            ObjDoctorDetail.UserId = row["UserId"].ToString();
                            ObjDoctorDetail.FullName = row["FullName"].ToString();
                            ObjDoctorDetail.Mobile = row["Mobile"].ToString();
                            ObjDoctorDetail.EmailId = row["EmailId"].ToString();
                            ObjDoctorDetail.AadharCard = row["AadharCard"].ToString();
                            ObjDoctorDetail.Gender = row["Gender"].ToString();
                            ObjDoctorDetail.DOB = row["DOB"].ToString();
                            ObjDoctorDetail.Address = row["Address"].ToString();
                            ObjDoctorDetail.Country = row["Country"].ToString();
                            ObjDoctorDetail.Pincode = row["Pincode"].ToString();
                            ObjDoctorDetail.City = row["City"].ToString();
                            ObjDoctorDetail.State = row["State"].ToString();
                            ObjDoctorDetail.ProfilePic = row["ProfilePic"].ToString();
                            ObjDoctorDetail.Msg = row["Msg"].ToString();
                            ObjDoctorDetail.HealthId = row["HealthId"].ToString();
                            ObjDoctorDetail.Role = row["Role"].ToString();
                            ObjDoctorDetail.EmployeeId = row["EmployeeId"].ToString();
                            ObjDoctorDetail.Org_Name = row["Name"].ToString();
                            ObjDoctorDetail.BranchName = row["BranchName"].ToString();
                            Result.MyDetails.Add(ObjDoctorDetail); //Add Doctor details to array
                          //  Result.Status = true;  //  Status Key 
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
        [Route("healthCampTestDoneList")]
        public string healthCampTestDoneList([FromBody] PagingParameterModel pagingparametermodel)
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            Result.MyDetails = new JArray() as dynamic;
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            try
            {
                if (UserId != null)
                {
                    SqlParameter[] param = new SqlParameter[]
                   {
                      new SqlParameter("@PatientId",UserId),
                      new SqlParameter("@SearchingText",pagingparametermodel.Searching)
                   };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_sp_healthCampTestDoneList", param);
                   
                       // DataTable dt = DAL.GetDataTable("WS_sp_healthCampTestDoneList " + UserId);
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

                        // Returns List of Reports after applying Paging   
                        var items = dt.Select().Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();


                        Result.ReportList = new JArray() as dynamic;   // Create Array for Report Details

                        foreach (DataRow row in dt.Rows)
                        {
                            dynamic ObjDoctorDetail = new JObject();

                            ObjDoctorDetail.TestCode = row["sTestCode"];
                            ObjDoctorDetail.TimeSlot = row["sTimeSlot"];
                            ObjDoctorDetail.TestDate = row["sTestDate"];
                            ObjDoctorDetail.TestName = row["sTestName"];
                            ObjDoctorDetail.ReportId = row["sBookLabTestId"];
                            ObjDoctorDetail.ProfileName = row["sProfileName"];
                            ObjDoctorDetail.patientName = row["patientName"];
                            //ObjDoctorDetail.LabLogo = row["sLabLogo"];
                            ObjDoctorDetail.ReportDate = row["ReportDate"];
                            Result.MyDetails.Add(ObjDoctorDetail); //Add Doctor details to array
                           // Result.Status = true;  //  Status Key 
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



        [HttpGet]
        [Route("GetHelthCampDashboardCount")]
        public string GetHelthCampDashboardCount()
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
                DataTable dt = DAL.GetDataTable("WS_Sp_HelthCampDashboardCount " + UserId);
                if (dt.Rows.Count > 0)
                {
                    Result.RegisterUserCount = dt.Rows[0]["count"].ToString();
                    Result.TestDoneCount = dt.Rows[1]["count"].ToString();
                    //Result.SuggestedTestCount = dt.Rows[0]["SuggestedTestCount"].ToString();
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
        [Route("HealthCampAdduser")]
        public string HealthCampAdduser([FromBody] healthCampUserDtls model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
                if (Ival.IsTextBoxEmpty(model.UserId.ToString()))
                {
                    Msg += "Please Enter Valid User id";
                }
                if (Ival.IsTextBoxEmpty(model.healthcampId.ToString()))
                {
                    Msg += "Please Enter Valid HealthCampID ";
                }
                if (Ival.IsTextBoxEmpty(model.org_id.ToString()))
                {
                    Msg += "Please Enter Valid Orgnization ID";
                }
                //if (Ival.IsTextBoxEmpty(model.Gender.ToString()))
                //{
                //    Msg += "Please Enter ";
                //}
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
                      

                        new SqlParameter("@UserId",model.UserId),
                        new SqlParameter("@healthcampid",model.healthcampId),
                        new SqlParameter("@orgid",model.org_id),
                        new SqlParameter("@technicianId",UserId), // added by Harshada to pass technician Id

                        new SqlParameter("@Returnval",SqlDbType.Int)
                     };
                    int data = DAL.ExecuteStoredProcedureRetnInt("Sp_WS_HelthCampAddUser", param);
                    if (data >= 1)
                    {
                        Result.Status = true;  //  Status Key 
                        
                        Result.Msg = "Health Camp User Added successfully.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                   else      //not  pubslished till 18-03-2022
                    {
                        Result.Status = false;  //  Status Key 

                        Result.Msg = "";
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
