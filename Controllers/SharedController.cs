using CrossPlatformAESEncryption.Helper;
using Howzu_API.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using Validation;
using VLS_API.Model;

namespace VLS_API.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    [ApiController]
    public class SharedController : ControllerBase
    {
        DataAccessLayer DAL = new DataAccessLayer();
        InputValidation Ival = new InputValidation();

        /// <summary>
        /// Get baner list
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("BanerList")]
        public string BanerList()
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                DataTable dt = DAL.GetDataTable("WS_Sp_GetBanerList ");
                if (dt.Rows.Count > 0)
                {
                    Result.BanerList = new JArray() as dynamic;   // Create Array for Baner Details
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjBanerDetail = new JObject();
                        ObjBanerDetail.path = dt.Rows[j]["path"];
                        Result.BanerList.Add(ObjBanerDetail); //Add baner details to array
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
        /// Ger Surway question list
        /// </summary>
        /// <param name="SurwayId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("AllCovidQuestionsList/{SurwayId}")]
        public string AllCovidQuestionsList(int SurwayId)
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
                if (!Ival.IsInteger(SurwayId.ToString()))
                {
                    Msg += "Please Enter Valid Surway Id";
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
                    DataTable dt = DAL.GetDataTable("WS_Sp_GetCovidQuestions " + SurwayId);
                    if (dt.Rows.Count > 0)
                    {
                        Result.QuestionList = new JArray() as dynamic;
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            var prnt = dt.Rows[j]["Id"].ToString();
                            DataTable dtQueDetails = DAL.GetDataTable("WS_Sp_SurwayQuestionOptionsByQueId " + prnt);
                            dynamic ObjQuestionDetails = new JObject();
                            if (j == 0)
                            {
                                if (dtQueDetails.Rows.Count != 0)
                                {
                                    ObjQuestionDetails.QuesDetail = new JArray() as dynamic;

                                    ObjQuestionDetails.QueId = dt.Rows[j]["Id"].ToString();
                                    ObjQuestionDetails.Questions = dt.Rows[j]["Questions"].ToString();
                                    ObjQuestionDetails.MultipleOptionSelection = dt.Rows[j]["MultipleOptionSelection"].ToString();
                                    for (int i = 0; i < dtQueDetails.Rows.Count; i++)
                                    {
                                        dynamic Quesoptiondetails = new JObject();
                                        if (i == 0)
                                        {

                                            Quesoptiondetails.OptionId = dtQueDetails.Rows[i]["Id"].ToString();
                                            Quesoptiondetails.Option = dtQueDetails.Rows[i]["Options"].ToString();
                                            Quesoptiondetails.Weightage = dtQueDetails.Rows[i]["Weightage(%)"].ToString();
                                            ObjQuestionDetails.QuesDetail.Add(Quesoptiondetails);
                                        }
                                        else
                                        {

                                            Quesoptiondetails.OptionId = dtQueDetails.Rows[i]["Id"].ToString();
                                            Quesoptiondetails.Option = dtQueDetails.Rows[i]["Options"].ToString();
                                            Quesoptiondetails.Weightage = dtQueDetails.Rows[i]["Weightage(%)"].ToString();
                                            ObjQuestionDetails.QuesDetail.Add(Quesoptiondetails);
                                        }
                                    }
                                    Result.QuestionList.Add(ObjQuestionDetails);
                                }

                            }
                            else
                            {
                                ObjQuestionDetails.QuesDetail = new JArray() as dynamic;
                                ObjQuestionDetails.QueId = dt.Rows[j]["Id"].ToString();
                                ObjQuestionDetails.Questions = dt.Rows[j]["Questions"].ToString();
                                ObjQuestionDetails.MultipleOptionSelection = dt.Rows[j]["MultipleOptionSelection"].ToString();
                                for (int i = 0; i < dtQueDetails.Rows.Count; i++)
                                {
                                    dynamic Quesoptiondetails = new JObject();
                                    if (i == 0)
                                    {

                                        Quesoptiondetails.OptionId = dtQueDetails.Rows[i]["Id"].ToString();
                                        Quesoptiondetails.Option = dtQueDetails.Rows[i]["Options"].ToString();
                                        Quesoptiondetails.Weightage = dtQueDetails.Rows[i]["Weightage(%)"].ToString();
                                        ObjQuestionDetails.QuesDetail.Add(Quesoptiondetails);
                                    }
                                    else
                                    {

                                        Quesoptiondetails.OptionId = dtQueDetails.Rows[i]["Id"].ToString();
                                        Quesoptiondetails.Option = dtQueDetails.Rows[i]["Options"].ToString();
                                        Quesoptiondetails.Weightage = dtQueDetails.Rows[i]["Weightage(%)"].ToString();
                                        ObjQuestionDetails.QuesDetail.Add(Quesoptiondetails);
                                    }
                                }
                                Result.QuestionList.Add(ObjQuestionDetails);
                            }
                        }
                        Result.Status = true;  //  Status Key
                        Result.Msg = "Success";  //  Status Key 
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.Status = false;  //  Status Key
                        Result.Msg = "No Records found.";  //  Status Key 
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
        /// Add surway details
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddSurwayReport")]
        public string AddSurwayReport([FromBody] Surway model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string _optionId = "";
            try
            {
                string Msg = "";

                if (Ival.IsTextBoxEmpty(model.Result))
                {
                    Msg += "Please Enter Valid Result";
                }
                if (Ival.IsTextBoxEmpty(model.OptionsId))
                {
                    Msg += "Please Enter Valid Options Id";
                }
                else
                {
                    string[] splitOptions = model.OptionsId.ToString().Split(',');
                    foreach (string optId in splitOptions)
                    {
                        if (!Ival.IsInteger(optId))
                        {
                            Msg += "Please Enter Valid Options Id";
                        }
                        else
                        {
                            _optionId += optId + ",";
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
                    SqlParameter[] param = new SqlParameter[]
                        {
                             new SqlParameter("@UserId",UserId),
                             new SqlParameter("@Result",model.Result),
                             new SqlParameter("@Remark",model.Remark),
                             new SqlParameter("@returnval",SqlDbType.Int)
                        };
                    int data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp__AddSurway", param);

                    if (data >= 1)
                    {
                        int relval = 0;
                        _optionId = _optionId.TrimEnd(',');
                        string[] splitOptions = _optionId.Split(',');
                        foreach (string Option in splitOptions)
                        {
                            SqlParameter[] param1 = new SqlParameter[]
                                {
                                    new SqlParameter("@UserId",UserId),
                                    new SqlParameter("@SurwayId",data),
                                    new SqlParameter("@OptionId",Option),
                                    new SqlParameter("@returnval",SqlDbType.Int)
                                };
                            relval = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_AddSurwayData", param1);
                        }
                        if (relval == 1)
                        {
                            Result.Status = true;  //  Status Key 
                            Result.Msg = "Surway Added successfully.";
                            JSONString = JsonConvert.SerializeObject(Result);
                        }
                        else
                        {
                            Result.Status = false;  //  Status Key 
                            Result.Msg = "Something went wrong,Please try again.";
                            JSONString = JsonConvert.SerializeObject(Result);
                        }
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
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key 
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);
                return JSONString;
            }
        }

        /// <summary>
        /// Add BMI Report
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddBMIReport")]
        public string AddBMIReport([FromBody] BMIReport model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (Ival.IsTextBoxEmpty(model.Result))
                {
                    Msg += "Please Enter Valid Result";
                }
                if (!Ival.IsDecimal(model.BMIValue))
                {
                    Msg += "Please Enter Valid BMI Value";
                }
                if (!Ival.IsDecimal(model.Weight))
                {
                    Msg += "Please Enter Valid Weight";
                }
                if (!Ival.IsDecimal(model.Height.ToString()))
                {
                    Msg += "Please Enter Valid Height";
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
                               new SqlParameter("@Height",model.Height),
                               new SqlParameter("@Weight",model.Weight),
                               new SqlParameter("@Result",model.Result),
                               new SqlParameter("@BMIValue",model.BMIValue),
                               new SqlParameter("@Returnval",SqlDbType.Int)
                        };
                    int data = DAL.ExecuteStoredProcedureRetnInt("Sp_WS_AddBMIReportUpdated", param);
                    if (data == 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "BMI Report Added successfully.";
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
        /// Get BMI Details by UserId
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetBMIDetails")]
        public string GetBMIDetails()
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                DataTable dt = DAL.GetDataTable("Sp_WS_GetBMIHistoryByUserID " + UserId);

                if (dt.Rows.Count > 0)
                {
                    Result.Status = true;  //  Status Key 
                    Result.BMIList = new JArray() as dynamic;

                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        SqlParameter[] param1 = new SqlParameter[]
                     {
                        new SqlParameter("@UserId",UserId),
                        new SqlParameter("@Date",dt.Rows[j]["Date"].ToString())
                     };
                        DataTable dtBMIDetails = DAL.ExecuteStoredProcedureDataTable("Sp_WS_GetBMIAllDeatilsByUserID", param1);
                        dynamic ObjBMIDetails = new JObject();
                        if (j == 0)
                        {
                            if (dtBMIDetails.Rows.Count != 0)
                            {
                                ObjBMIDetails.BMIDetail = new JArray() as dynamic;

                                for (int i = 0; i < dtBMIDetails.Rows.Count; i++)
                                {
                                    dynamic Bmidetails = new JObject();
                                    if (i == 0)
                                    {
                                        Bmidetails.BMIValue = dtBMIDetails.Rows[i]["BMIValue"].ToString();
                                        Bmidetails.Date = dt.Rows[j]["Date"].ToString();
                                        Bmidetails.Result = dtBMIDetails.Rows[i]["Result"].ToString();
                                        ObjBMIDetails.BMIDetail.Add(Bmidetails);
                                    }
                                    else
                                    {
                                        Bmidetails.BMIValue = dtBMIDetails.Rows[i]["BMIValue"].ToString();
                                        Bmidetails.Date = dt.Rows[j]["Date"].ToString();
                                        Bmidetails.Result = dtBMIDetails.Rows[i]["Result"].ToString();
                                        ObjBMIDetails.BMIDetail.Add(Bmidetails);
                                    }
                                }
                                Result.BMIList.Add(ObjBMIDetails);
                            }
                        }
                        else
                        {
                            if (dtBMIDetails.Rows.Count != 0)
                            {
                                ObjBMIDetails.BMIDetail = new JArray() as dynamic;

                                for (int i = 0; i < dtBMIDetails.Rows.Count; i++)
                                {
                                    dynamic Bmidetails = new JObject();
                                    if (i == 0)
                                    {
                                        Bmidetails.BMIValue = dtBMIDetails.Rows[i]["BMIValue"].ToString();
                                        Bmidetails.Date = dt.Rows[j]["Date"].ToString();
                                        Bmidetails.Result = dtBMIDetails.Rows[i]["Result"].ToString();
                                        ObjBMIDetails.BMIDetail.Add(Bmidetails);
                                    }
                                    else
                                    {
                                        Bmidetails.BMIValue = dtBMIDetails.Rows[i]["BMIValue"].ToString();
                                        Bmidetails.Date = dt.Rows[j]["Date"].ToString();
                                        Bmidetails.Result = dtBMIDetails.Rows[i]["Result"].ToString();
                                        ObjBMIDetails.BMIDetail.Add(Bmidetails);
                                    }
                                }
                                Result.BMIList.Add(ObjBMIDetails);
                            }
                        }
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
                return JSONString;
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
        /// get User notification list
        /// </summary>
        /// <param name="pagingparametermodel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PushNotifications")]
        public string PushNotifications([FromBody] PagingParameterModel pagingparametermodel)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                SqlParameter[] param = new SqlParameter[]
                    {
                          new SqlParameter("@UserID",UserId),
                          new SqlParameter("@SearchingText",pagingparametermodel.Searching)
                    };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetPatientNotification", param);
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

                    Result.NotificationList = new JArray() as dynamic;   // Create Array for Baner Details

                    for (int i = 0; i < items.Count; i++)
                    {
                        dynamic ObjNotificationDetail = new JObject();

                        ObjNotificationDetail.NotificationID = items[i]["sNotificationID"];
                        ObjNotificationDetail.Title = items[i]["sTitle"];
                        ObjNotificationDetail.Message = items[i]["sMessage"];
                        ObjNotificationDetail.Status = items[i]["sStatus"];
                        ObjNotificationDetail.Date = items[i]["sDate"];
                        Result.NotificationList.Add(ObjNotificationDetail); //Add baner details to array
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
        /// Get unread Notification  count
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("UnreadNotificationCountData")]
        public string UnreadNotificationCountData()
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                DataTable dt = DAL.GetDataTable("WS_Sp_GetNotificationCountData " + UserId);
                if (dt.Rows.Count > 0)
                {
                    Result.Status = true;  //  Status Key
                    Result.Unreadnotifications = dt.Rows[0]["newnotifications"];
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
        /// Update notification seen status
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("ReadNotification")]
        public string ReadNotification()
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                SqlParameter[] param = new SqlParameter[]
                    {
                        new SqlParameter("@UserID",UserId),
                        new SqlParameter("@Returnval",SqlDbType.Int)
                     };
                int data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_ReadNotificationSeenData", param);
                if (data == 1)
                {
                    Result.Status = true;  //  Status Key 
                    Result.Msg = "Update successfully.";
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

        [HttpPost]
        [Route("AddHydrationDetails")]
        public string AddHydrationDetails([FromBody] Hydration model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
                if (!Ival.IsInteger(model.Height_cm.ToString()))
                {
                    Msg += "Please Enter Valid Height";
                }
                if (Ival.IsTextBoxEmpty(model.Weight_Kg.ToString()))
                {
                    Msg += "Please Enter Valid Weight";
                }
                if (Ival.IsTextBoxEmpty(model.Wakeuptime.ToString()))
                {
                    Msg += "Please Enter Valid Wakeup time";
                }
                if (Ival.IsTextBoxEmpty(model.Bedtime.ToString()))
                {
                    Msg += "Please Enter Valid Bed time";
                }
                if (!Ival.IsInteger(model.Intake_ml.ToString()))
                {
                    Msg += "Please Enter Valid intek";
                }
                if (Ival.IsTextBoxEmpty(model.Cupsize_ml.ToString()))
                {
                    Msg += "Please Enter Valid cup size";
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
                        new SqlParameter("@Height",model.Height_cm),
                        new SqlParameter("@Weight",model.Weight_Kg),
                        new SqlParameter("@WakeupTime",model.Wakeuptime),
                        new SqlParameter("@BedTime",model.Bedtime),
                        new SqlParameter("@Intek",model.Intake_ml),
                        new SqlParameter("@Cupsize",model.Cupsize_ml),
                        new SqlParameter("@Returnval",SqlDbType.Int)
                     };
                    int data = DAL.ExecuteStoredProcedureRetnInt("Sp_WS_AddHydrationDetails", param);
                    if (data >= 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.HydrationId = data;  //  Status Key 
                        Result.Msg = "Hydration Details Added successfully.";
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
        [Route("AddHydrationDailyReport")]
        public string AddHydrationDailyReport([FromBody] HydrationDailyReport model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
                if (!Ival.IsInteger(model.HydrationId.ToString()))
                {
                    Msg += "Please Enter Valid Hydration Id";
                }
                if (Ival.IsTextBoxEmpty(model.Consumetime.ToString()))
                {
                    Msg += "Please Enter Valid Consume time";
                }
                if (!Ival.IsInteger(model.Water_ml.ToString()))
                {
                    Msg += "Please Enter Valid intek";
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
                        new SqlParameter("@HydrationId",model.HydrationId),
                        new SqlParameter("@Water",model.Water_ml),
                        new SqlParameter("@ConsumeTime",model.Consumetime),
                        new SqlParameter("@CreatedBy",UserId),
                        new SqlParameter("@Returnval",SqlDbType.Int)
                     };
                    int data = DAL.ExecuteStoredProcedureRetnInt("Sp_WS_AddHydrationDailyReport", param);
                    if (data == 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.HydrationId = data;  //  Status Key 
                        Result.Msg = "Hydration details added successfully.";
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
        [Route("AddMedication")]
        public string AddMedication([FromBody] Medication model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
                if (Ival.IsTextBoxEmpty(model.TabletName.ToString()))
                {
                    Msg += "Please Enter Valid Tablet Name";
                }
                if (Ival.IsTextBoxEmpty(model.DailyDose.ToString()))
                {
                    Msg += "Please Enter Valid Daily Dose";
                }
                if (!Ival.IsValidDate(model.StartDate))
                {
                    Msg += " Please Enter Valid Start Date";
                }
                if (!Ival.IsValidDate(model.EndDate))
                {
                    Msg += " Please Enter Valid End Date";
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
                        new SqlParameter("@TabletName",model.TabletName),
                        new SqlParameter("@Reason",model.Reason),
                        new SqlParameter("@DailyDoses",model.DailyDose),
                        new SqlParameter("@StartDate",model.StartDate),
                        new SqlParameter("@EndDate",model.EndDate),
                        new SqlParameter("@Returnval",SqlDbType.Int)
                     };
                    int data = DAL.ExecuteStoredProcedureRetnInt("Sp_WS_AddMedication", param);
                    if (data >= 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.MedicationId = data;  //  Status Key 
                        Result.Msg = "Medication Details Added successfully.";
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
        [Route("AddDoseDetails")]
        public string AddDoseDetails([FromBody] DoseDetails model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
                if (!Ival.IsInteger(model.MedicationId.ToString()))
                {
                    Msg += "Please Enter Valid Medication Id";
                }
                if (Ival.IsTextBoxEmpty(model.DaySlot.ToString()))
                {
                    Msg += "Please Enter Valid day slot";
                }
                if (Ival.IsTextBoxEmpty(model.Dosetime.ToString()))
                {
                    Msg += "Please Enter Valid Daily Dose time";
                }
                if (Ival.IsTextBoxEmpty(model.Mealtime.ToString()))
                {
                    Msg += "Please Enter Valid Meal time";
                }
                if (Ival.IsTextBoxEmpty(model.Quantity.ToString()))
                {
                    Msg += "Please Enter Valid Quantity";
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
                        new SqlParameter("@MedicationId",model.MedicationId),
                        new SqlParameter("@DosesSlot",model.DaySlot),
                        new SqlParameter("@DosesTime",model.Dosetime),
                        new SqlParameter("@Whentotake",model.Mealtime),
                        new SqlParameter("@Quantity",model.Quantity),
                        new SqlParameter("@Returnval",SqlDbType.Int)
                     };
                    int data = DAL.ExecuteStoredProcedureRetnInt("Sp_WS_AddMedicationDosesDetails", param);
                    if (data >= 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.DoseId = data;
                        Result.MedicationId = model.MedicationId;
                        Result.Msg = "Dose Details Added successfully.";
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
        [Route("AddDoseRecords")]
        public string AddDoseRecords([FromBody] DoseHistory model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
                if (!Ival.IsInteger(model.MedicationId.ToString()))
                {
                    Msg += "Please Enter Valid Medication Id";
                }
                if (!Ival.IsInteger(model.DoseId.ToString()))
                {
                    Msg += "Please Enter Valid Medication Id";
                }
                if (Ival.IsTextBoxEmpty(model.Dosetaken.ToString()))
                {
                    Msg += "Please Enter Valid dose taken or not";
                }
                if (Ival.IsTextBoxEmpty(model.Takentime.ToString()))
                {
                    Msg += "Please Enter Valid Daily Dose taken time ";
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
                        new SqlParameter("@MedicationId",model.MedicationId),
                        new SqlParameter("@DoseId",model.DoseId),
                        new SqlParameter("@Istaken",model.Dosetaken),
                        new SqlParameter("@Skipreason",model.Skipreason),
                        new SqlParameter("@Takentime",model.Takentime),
                        new SqlParameter("@Returnval",SqlDbType.Int)
                     };
                    int data = DAL.ExecuteStoredProcedureRetnInt("Sp_WS_AddMedicationDosesHistory", param);
                    if (data == 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Dose record Added successfully.";
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
    }
}
