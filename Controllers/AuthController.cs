using CrossPlatformAESEncryption.Helper;
using Howzu_API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using VLS_API.Model;
using VLS_API.Services;
using Validation;
using Howzu_API.Services;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace VLS_API.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        DataAccessLayer DAL = new DataAccessLayer();
        InputValidation Ival = new InputValidation();
      
        private IAuthenticateService _authenticateService;
        public AuthController(IAuthenticateService authenticateService)
        {
            _authenticateService = authenticateService;
        }

        /// <summary>
        /// Sign in API for Howzu App User
        /// </summary>
        /// <param name="UserName">Mandatory</param>
        /// <param name="Password">Mandatory</param>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("Signin")]
        public string Signin([FromBody] Userlogin model)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (Ival.IsTextBoxEmpty(model.UserName))
                {
                    Msg += "Please Enter Valid Username";
                }
                if (Ival.IsTextBoxEmpty(model.Password))
                {
                    Msg += "Please Enter Valid password";
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
                    var _userName = CryptoHelper.Encrypt(model.UserName);
                    var _password = CryptoHelper.Encrypt(model.Password);
                    var user = _authenticateService.Authenticate(_userName, _password);
                    if (user == null)
                    {
                        Result.Status = false;  //  Status Key
                        Result.Msg = "Please check username & password";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Login Successful.";
                        Result.Name = user.Name;
                        Result.Token = user.Token;
                        Result.Role = user.Role;
                        Result.MobileVerified = user.MobileVerified;
                        Result.RegistrationStatus = user.RegistrationStatus;
                        Result.EmailId = user.EmailId;
                        Result.UserId = user.UserId;
                        Result.OrgId = user.OrgId;

                        if (user.MobileVerified == false)
                        {
                            string url = "https://visionarylifescience.com/mobileapp/WebAPI/WebAPI.asmx/GenrateOTP?Mobile=" + model.UserName + ""; // sample url
                            using (HttpClient client = new HttpClient())
                            {
                                HttpResponseMessage response = client.GetAsync(url).Result;
                                int StatusCode = Convert.ToInt32(response.StatusCode);
                                if (StatusCode == 200)
                                {
                                    Result.OTPSend = true;

                                }
                                else
                                {
                                    Result.OTPSend = false;
                                }
                            }
                        }
                        JSONString = JsonConvert.SerializeObject(Result);//Add user details to array
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
        /// Signout API used to remove device tokan
        /// </summary>
        /// <param name="UserId">Mandatory</param>
        /// <returns></returns>
        [HttpPost("Signout")]
        public string Signout()
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;

            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                SqlParameter[] param = new SqlParameter[]
                {
                        new SqlParameter("@DeviceTokan",""),
                        new SqlParameter("@UserId",UserId),
                        new SqlParameter("@returnval",SqlDbType.Int),
                };
                int Val = DAL.ExecuteStoredProcedureRetnInt("Sp_UpdateDeviceTokan", param);

                if (Val == 1)
                {
                    Result.Status = true;  //  Status Key 
                    Result.Msg = "Logout successfully.";
                    JSONString = JsonConvert.SerializeObject(Result);
                }
                else
                {
                    Result.Status = false;  //  Status Key
                    Result.Msg = "Something went wrong, Please try again.";
                    JSONString = JsonConvert.SerializeObject(Result);
                }
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong, Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        /// <summary>
        /// Send OTP on entered Mobile number
        /// </summary>
        /// <param name="Mobile">Mandatory</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("SendOTP/{Mobile}")]
        public string SendOTP(Int64 Mobile)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                SendOTP _otp = new SendOTP();
                int StatusCode = _otp.sendOTP(Mobile.ToString());
                if (StatusCode == 200)
                {
                    Result.OTPSend = true;
                    Result.Msg = "OTP sent on registered mobile number.";
                }
                else
                {
                    Result.OTPSend = false;
                    Result.Msg = "Something went wrong,Please try again.";
                }
                JSONString = JsonConvert.SerializeObject(Result);
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
        /// Update Password
        /// </summary>
        /// <param name="model">Mandatory</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("SetNewPassword")]
        public string SetNewPassword([FromBody] SetNewPassword model)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (!Ival.ValidatePassword(model.Password.ToString()))
                {
                    Msg += "Please enter Minimum 6 characters at least 1 Uppercase Alphabet, 1 Lowercase Alphabet, 1 Number and 1 Special Character";
                }
                if (Ival.IsInteger(model.Mobile))
                {
                    if (!Ival.MobileValidation(model.Mobile))
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
                    var _Mobile = CryptoHelper.Encrypt(model.Mobile);
                    var _password = CryptoHelper.Encrypt(model.Password);
                    SqlParameter[] param1 = new SqlParameter[]
                         {
                            new SqlParameter("@Mobile",_Mobile),
                            new SqlParameter("@Password",_password),
                            new SqlParameter("@returnval",SqlDbType.Int),
                        };
                    int Val = DAL.ExecuteStoredProcedureRetnInt("WS_Sp__SetNewPassword", param1);
                    if (Val == 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Password updated Successfully.";
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
        /// Reset Password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ResetPassword")]
        public string ResetPassword([FromBody] ResetPassword model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;

            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (!Ival.ValidatePassword(model.NewPassword.ToString()))
                {
                    Msg += "Please enter Minimum 6 characters at least 1 Uppercase Alphabet, 1 Lowercase Alphabet, 1 Number and 1 Special Character";
                }
                if (Ival.IsTextBoxEmpty(model.OldPassword))
                {
                    Msg += "Please Enter Valid Old Password";
                }
                if (Ival.IsInteger(model.Mobile))
                {
                    if (!Ival.MobileValidation(model.Mobile))
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
                    var _Mobile = CryptoHelper.Encrypt(model.Mobile);
                    var _Newpassword = CryptoHelper.Encrypt(model.NewPassword);
                    var _Oldpassword = CryptoHelper.Encrypt(model.OldPassword);

                    SqlParameter[] param = new SqlParameter[]
                        {
                            new SqlParameter("@Mobile",_Mobile),
                            new SqlParameter("@UserId",UserId),
                            new SqlParameter("@OldPassword",_Oldpassword),
                            new SqlParameter("@NewPassword",_Newpassword),
                            new SqlParameter("@returnval",SqlDbType.Int)
                        };
                    int Val = DAL.ExecuteStoredProcedureRetnInt("WS_Sp__ResetPassword", param);
                    if (Val == 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Password changed successfully.";
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
                Result.Msg = "Something went wrong,Please try again."; ;
                JSONString = JsonConvert.SerializeObject(Result);
                return JSONString;
            }
        }

        /// <summary>
        /// Signup for HowzU App.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("SignUp")]
        public string SignUp([FromBody] UserRegister model)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (!Ival.IsCharOnly(model.FullName))
                {
                    Msg += "Please Enter Valid Full Name";
                }
                if (!Ival.IsCharOnly(model.Gender))
                {
                    Msg += "Please Enter Valid Gender";
                }
                if (!Ival.IsValidDate(model.BirthDate))
                {
                    Msg += "Please Enter Valid Birth Date";
                }
                if (!Ival.IsTextBoxEmpty(model.EmailId))
                {
                    if (!Ival.IsValidEmailAddress(model.EmailId))
                    {
                        Msg += " Please Enter Valid Email Id";
                    }
                }
                if (Ival.IsInteger(model.Mobile))
                {
                    if (!Ival.MobileValidation(model.Mobile))
                    {
                        Msg += "Please Enter Valid Mobile Number";
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
                //******* Hide by harshada to remove password validation 01-08-2022 ***
                //if (!Ival.ValidatePassword(model.Password.ToString()))
                //{
                //    Msg += "Please enter Minimum 6 characters at least 1 Uppercase Alphabet, 1 Lowercase Alphabet, 1 Number and 1 Special Character";
                //}
                if (!Ival.ValidatePasswordNew(model.Password.ToString()))
                {
                    Msg += "Please enter Minimum 6 characters";
                }
                if (!Ival.IsInteger(model.Pincode.ToString()))
                {
                    Msg += "Please Enter Valid Pincode";
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
                    var _password = CryptoHelper.Encrypt(model.Password);
                    var _Aadhar = (model.Aadharnumber != "") ? CryptoHelper.Encrypt(model.Aadharnumber) : "";
                    var _HealthId = (model.HealthId != "") ? CryptoHelper.Encrypt(model.HealthId) : "";
                    SqlParameter[] param = new SqlParameter[]
                    {
                    new SqlParameter("@UserName",model.FullName),
                    new SqlParameter("@Gender",model.Gender),
                    new SqlParameter("@DOB",model.BirthDate),
                 
                    new SqlParameter("@Contact",_mobile),
                    new SqlParameter("@Email",_emailId),
                    new SqlParameter("@HealthId",_HealthId),
                    new SqlParameter("@Aadharnumber",_Aadhar),
                    new SqlParameter("@RegisterFrom","Mobile"),
                    new SqlParameter("@PinCode",model.Pincode),

                   new SqlParameter("@ChannelPartnerCode",model.ChannelPartnerCode),

                    new SqlParameter("@returnval",SqlDbType.Int)
                    };
                    int data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_UserRegisterupdatedOne", param);
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
                    else if (data == -5)
                    {
                        Result.Status = false;  //  Status Key
                        Result.Msg = "Please Enter Valid Channel Partner Code";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        SqlParameter[] param2 = new SqlParameter[]
                            {
                            new SqlParameter("@UserId",data),
                            new SqlParameter("@Mobile",_mobile),
                            new SqlParameter("@EmailId",_emailId),
                            new SqlParameter("@Role","Patient"),
                            new SqlParameter("@Password",_password),
                            new SqlParameter("@UserName",""),
                          //new SqlParameter("@loginStatus","A"),
                            new SqlParameter("@Returnval",SqlDbType.Int)
                            };
                        int ResultVal1 = DAL.ExecuteStoredProcedureRetnInt("Sp_AddUserLoginCredentials", param2);

                        if (ResultVal1 == 1)
                        {
                            var user = _authenticateService.Authenticate(_mobile, _password);
                            Result.Status = true;  //  Status Key 
                            Result.Msg = "Thank you for information. We have sent OTP on your registered mobile number for verification";
                            Result.Name = user.Name;
                            Result.Token = user.Token;
                            Result.Role = user.Role;
                            Result.MobileVerified = user.MobileVerified;
                            Result.UserId = user.UserId;

                            if (user.MobileVerified == false)
                            {
                                SendOTP _otp = new SendOTP();

                                if (Ival.IsInteger(model.Mobile))
                                {
                                    // if (!Ival.MobileValidation(model.Mobile)) //Change by Harshada Removed ! symbol.Becoz OTP not getting  29/7/22
                                    if (Ival.MobileValidation(model.Mobile))
                                    {
                                        int StatusCode = _otp.sendOTP(model.Mobile);
                                        if (StatusCode == 200)
                                        {
                                            Result.OTPSend = true;
                                        }
                                        else
                                        {
                                            Result.OTPSend = false;
                                        }
                                    }
                                }
                            }
                            JSONString = JsonConvert.SerializeObject(Result);
                        }
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



        /// Added for QuikRegistration by pass
        [AllowAnonymous]
        [HttpPost]
        [Route("UpdatePatientToDoctor")]
        public string UpdatePatientToDoctor([FromBody] UpdatePatientToDoctor model)
        {


            //   var _mobile = (modelM.Mobile != "") ? CryptoHelper.Encrypt(modelM.Mobile) : "";

            var _Mobile = CryptoHelper.Encrypt(model.Mobile.ToString());
            //    var _mobile = CryptoHelper.Encrypt(Mobile.ToString());

            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                //SqlParameter[] param3 = new SqlParameter[]
                //       {
                //            new SqlParameter("@sMobile",_Mobile.ToString())

                //          //  new SqlParameter("@returnval",SqlDbType.Int)
                //       };

                //DataTable UserIdd = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetUserIdPatientToDoctorQuikRegistartion", param3);

                //string UserId = UserIdd.Rows[0]["sAppUserId"].ToString();




                /// get mobile number.
                /// retrive mobilenumber from model and get userid based on mobile number
                /// pass to existing store proc as userid
                /// 


                SqlParameter[] param3 = new SqlParameter[]
                       {
                            new SqlParameter("@sMobile",_Mobile.ToString()),

                            new SqlParameter("@returnval",SqlDbType.Int)
                       };

                int UserId = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_GetUserIdPatientToDoctorQuikRegistartion1", param3);

                // string UserId = UserIdd.Rows[0]["sAppUserId"].ToString();



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
        /// Add Device Token for FCM Notification
        /// </summary>
        /// <param name="DeviceToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateDeviceToken/{DeviceToken}")]
        public string UpdateDeviceToken(string DeviceToken)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                //Update Device Token
                if (UserId != null)
                {
                    SqlParameter[] param1 = new SqlParameter[]
                    {
                        new SqlParameter("@DeviceTokan",DeviceToken),
                        new SqlParameter("@UserId",UserId),
                        new SqlParameter("@returnval",SqlDbType.Int),
                    };
                    int Val = DAL.ExecuteStoredProcedureRetnInt("Sp_UpdateDeviceTokan", param1);

                    Result.Status = true;  //  Status Key 
                    Result.Msg = "Device Token Updated Successfully.";
                    JSONString = JsonConvert.SerializeObject(Result);//Add user details to array
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
                Result.Msg = "Something went wrong,Please try again."; ;
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        /// <summary>
        /// Verify OTP
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("VerifiedOTP")]
        public string VerifiedOTP([FromBody] VerifyOTP model)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (Ival.IsInteger(model.Mobile))
                {
                    if (!Ival.MobileValidation(model.Mobile))
                    {
                        Msg += "Please Enter Valid Mobile Number";
                    }
                }
                else
                {
                    Msg += "Please Enter Valid Mobile Number";
                }
                if (Ival.IsTextBoxEmpty(model.OTP))
                {
                    Msg += "Please Enter Valid OTP Number";
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
                    var _Mobile = CryptoHelper.Encrypt(model.Mobile);

                    SqlParameter[] param1 = new SqlParameter[]
                        {
                        new SqlParameter("@Mobile",_Mobile),
                        new SqlParameter("@OTP",model.OTP),
                        new SqlParameter("@returnval",SqlDbType.Int),
                        };
                    int Val = DAL.ExecuteStoredProcedureRetnInt("Ws_Sp_VerifiedOTP", param1);
                    if (Val == 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "OTP Verified Successfully.";
                        JSONString = JsonConvert.SerializeObject(Result);//Add user details to array
                    }
                    else
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "Wrong OTP";
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
        /// Update Registraion Process
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("UpdateRegistrationStatus")]
        public string UpdateRegistrationStatus()
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;

            try
            {
                SqlParameter[] param1 = new SqlParameter[]
                    {
                        new SqlParameter("@UserId",UserId),
                        new SqlParameter("@returnval",SqlDbType.Int),
                    };
                int Val = DAL.ExecuteStoredProcedureRetnInt("Ws_Sp_UpdateRegistrationStatus", param1);
                if (Val == 1)
                {
                    DataTable dt = DAL.GetDataTable("Sp_GetMobileVerifiedStatsu " + UserId);
                    if (dt.Rows.Count > 0)
                    {
                        Result.MobileVerified = dt.Rows[0]["MobileVerified"];
                    }
                    Result.Status = true;  //  Status Key 
                    Result.Msg = "Registration completed.";
                    JSONString = JsonConvert.SerializeObject(Result);//Add user details to array
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
                Result.Msg = "Something went wrong,Please try again."; ;
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        [AllowAnonymous]
        [HttpGet("ForgotPasswordSendOTP/{Mobile}")]
        public string ForgotPasswordSendOTP(Int64 Mobile)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (Ival.IsInteger(Mobile.ToString()))
                {
                    if (!Ival.MobileValidation(Mobile.ToString()))
                    {
                        Msg += " Please Enter Valid Mobile Number";
                    }
                }
                else
                {
                    Msg += " Please Enter Valid Mobile Number";
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
                    var _mobile = CryptoHelper.Encrypt(Mobile.ToString());
                    Random generator = new Random();
                    string OTP = generator.Next(1, 10000).ToString("D4");
                    SqlParameter[] param = new SqlParameter[]
                   {
                            new SqlParameter("@Mobile",_mobile),
                            new SqlParameter("@OTP",OTP),
                            new SqlParameter("@returnval",SqlDbType.Int)
                   };
                    int data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_VerifyMobileNumber", param);

                    if (data == 1)
                    {
                        SendOTP _otp = new SendOTP();
                        int StatusCode = _otp.FamilyMemberOTP(Mobile.ToString(), OTP);
                        if (StatusCode == 200)
                        {
                            Result.OTPSend = true;
                            Result.Msg = "OTP sent on registered mobile number.";
                        }
                        else
                        {
                            Result.OTPSend = false;
                            Result.Msg = "Something went wrong,Please try again.";
                        }
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.OTPSend = false;
                        Result.Msg = "Mobile number not exists.";
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

        [AllowAnonymous]
        [HttpPost]
        [Route("GetSliderPath")]
        public string GetSliderPath()
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Createroot JSON Object
            Result.MyDetails = new JArray() as dynamic;

            try
            {
                SqlParameter[] param = new SqlParameter[]
                    {
                          new SqlParameter("@SearchingText","")
                    };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetSliderPath", param);
                foreach(DataRow row in dt.Rows)
                {
                    dynamic ObjImageDetail = new JObject();
                    ObjImageDetail.ImageTitle = row["ImageTitle"];
                    ObjImageDetail.ImagePath = row["ImagePath"];
                    // Get's No of Rows Count 
                    Result.MyDetails.Add(ObjImageDetail); //Add Doctor details to array
                    Result.Status = true;  //  Status Key 
                }
                //***** Hide code by harshada @09/08/2022 to resolve silder issue
                //if (dt.Rows.Count > 0)
                //{
                //    dynamic ObjImageDetail = new JObject();


                //    //for (int j = 1; j <= dt.Rows.Count-1; j++)
                //    for (int j = 0; j < dt.Rows.Count; j++) //new line by harshada
                //    {

                //        ObjImageDetail.ImageTitle = dt.Rows[j]["ImageTitle"];
                //        ObjImageDetail.ImagePath = dt.Rows[j]["ImagePath"];
                //        // Get's No of Rows Count 
                //        Result.MyDetails.Add(ObjImageDetail); //Add Doctor details to array
                //        Result.Status = true;  //  Status Key 
                //    }
                //}
                //else
                //{
                //    Result.Status = false;  //  Status Key
                //    Result.Msg = "No Record Found";
                //}
                JSONString = JsonConvert.SerializeObject(Result);

            }
            catch (Exception e)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("GetImagePath")]
        public string GetImagePath()
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Createroot JSON Object
            Result.MyDetails = new JArray() as dynamic;
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;

            try
            {
                SqlParameter[] param = new SqlParameter[]
                    {
                          new SqlParameter("@userId",UserId)
                    };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetImagePath", param);
                foreach (DataRow row in dt.Rows)
                {
                    dynamic ObjImageDetail = new JObject();
                   // ObjImageDetail.ImageTitle = row["ImageTitle"];
                    ObjImageDetail.ImagePath = row["sImagePath"];
                    // Get's No of Rows Count 
                    Result.MyDetails.Add(ObjImageDetail); //Add Doctor details to array
                    Result.Status = true;  //  Status Key 
                }
             
                JSONString = JsonConvert.SerializeObject(Result);

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


        [HttpGet]
        [Route("EnterPriseLabList/{OrgId}")]
        public string EnterPriseLabList(int OrgId)
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                DataTable dt = DAL.GetDataTable("WS_Sp_GetOrglabList " + OrgId);

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
                    Result.Status = false;  //  Status Key
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
    }

}
