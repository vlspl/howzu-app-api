using CrossPlatformAESEncryption.Helper;
using Howzu_API.Model;
using Howzu_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using Validation;
using VLS_API.Model;  
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace VLS_API.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        DataAccessLayer DAL = new DataAccessLayer();
        InputValidation Ival = new InputValidation();
        FCMPushNotification fcm = new FCMPushNotification();

        private IHostingEnvironment _hostingEnvironment;
        public PatientController(IHostingEnvironment environment)
        {
            _hostingEnvironment = environment;
        }


        /// <summary>
        /// uplaod Profile Picture
        /// </summary>
        /// <param name="ProfileImage">Mandatory</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("UploadProfileImage")] //New API created by Harshada @17/06/2022 To upload profile picture
        public async Task<string> UploadProfileImage([FromForm] UserDataModel model)
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
                string fName = obj+model.ProfileImage.FileName;//file.FileName;
               // var file = Request.Form.Files[0];
                //var folderName = Path.Combine("images/profileimage");
                var folderName = Path.Combine("Images/ProfileImage");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
               // var pathToSave = Path.Combine("https://www.visionarylifescience.com", folderName);

                //  string path = Path.Combine("https://www.visionarylifescience.com/images/profileimage/",fName);
                var path = Path.Combine(_hostingEnvironment.ContentRootPath, "Images/ProfileImage/" + fName);

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

                        SqlParameter[] param = new SqlParameter[]
                        {
                            new SqlParameter("@userId",UserId),
                             new SqlParameter("@imagePath",_file)
                        };
                        DataTable dt = DAL.ExecuteStoredProcedureDataTable("SP_ImageUpload", param);
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
                        Result.Msg = "Profile pic uploaded successfully.";
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
            catch(Exception ex)
            {
                throw ex;
            }
        }

        // This api is for directly showing image instead of path
        //[HttpPost]
        //[Route("GetImage")]
        //public async Task<IActionResult> GetImage(string imageName)
        //{

        //    Byte[] b;
        //    b = await System.IO.File.ReadAllBytesAsync(Path.Combine(_hostingEnvironment.ContentRootPath, "Images/ProfileImage", $"{imageName}"));
        //    return File(b, "image/jpeg");
        //}


        /// <summary>
        /// Get Patient Details By UserID
        /// </summary>
        /// <param name="id">Mandatory</param>
        /// <returns></returns>
        [HttpGet]
        [Route("PatientMyProfile")]
        public string PatientMyProfile()
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            Result.MyDetails = new JArray() as dynamic;
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            try
            {
                if (UserId != null)
                {
                    DataTable dt = DAL.GetDataTable("WS_Sp__GetPatientPersonalDetail " + UserId);
                    if (dt.Rows.Count > 0)
                    {
                        dynamic ObjDoctorDetail = new JObject();
                        // new added by Urmila to pass OrgId 08-03-22
                        ObjDoctorDetail.OrgId = dt.Rows[0]["Org_Id"];

                        ObjDoctorDetail.UserId = dt.Rows[0]["UserId"];
                        ObjDoctorDetail.FullName = dt.Rows[0]["FullName"];
                        ObjDoctorDetail.Mobile = dt.Rows[0]["Mobile"];
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
                        ObjDoctorDetail.HealthId = dt.Rows[0]["HealthId"];
                        ObjDoctorDetail.Role = dt.Rows[0]["Role"];
                        ObjDoctorDetail.EmployeeId = dt.Rows[0]["EmployeeId"];
                        ObjDoctorDetail.Org_Name = dt.Rows[0]["Name"];
                        ObjDoctorDetail.BranchName = dt.Rows[0]["BranchName"];
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

        /// <summary>
        /// Update Patient Details
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdatePatient")]
        public string UpdatePatient([FromBody] UpdatePatient model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;

            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (!Ival.IsTextBoxEmpty(model.EmailId))
                {
                    if (!Ival.IsValidEmailAddress(model.EmailId))
                    {
                        Msg += " Please Enter Valid Email Id";
                    }
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
                    var _HealthId = (model.HealthId != "") ? CryptoHelper.Encrypt(model.HealthId) : "";

                    SqlParameter[] param = new SqlParameter[]
                         {
                            new SqlParameter("@UserId",UserId),
                            new SqlParameter("@Address",model.Address),
                            new SqlParameter("@Pincode",model.Pincode),
                            new SqlParameter("@City",model.City),
                            new SqlParameter("@ImagePath",model.ProfileIamge),
                            new SqlParameter("@HealthId",_HealthId),
                            new SqlParameter("@AadharCard",_Aadhar),
                            new SqlParameter("@EmailId",_emailId),
                            new SqlParameter("@returnval",SqlDbType.Int),
                         };
                    int data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_UpdatePatientDetailsupdateOne", param);
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
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);
                return JSONString;
            }
        }

        [HttpPost, DisableRequestSizeLimit]
        [Route("UploadPProfilePic")]
        public string UploadPProfilePic()
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                var file =  Request.Form.Files[0];//"calendar.png"; //
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

                        string api_url = "https://visionarylifescience.com/mobileapp/service/UploadHandlerforProfile.ashx";
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
                        Result.Msg = "Profile pic uploaded successfully.";
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
        /// Get My  doctor Details
        /// </summary>
        /// <param name="id">Mandatory</param>
        /// <returns></returns>
        [HttpPost]
        [Route("MyDoctorList")]
        public string MyDoctorList([FromBody] PagingParameterModel pagingparametermodel)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                //  DataTable dt = DAL.GetDataTable("WS_Sp__GetMyDocList " + UserId);

                SqlParameter[] param = new SqlParameter[]
              {
                    new SqlParameter("@UserId",UserId),
                    new SqlParameter("@SearchingText",pagingparametermodel.Searching)
              };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetMyDoctorListwtthSearching", param);
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

                    Result.DoctorList = new JArray() as dynamic;   // Create Array for Doctor Details

                    for (int j = 0; j < items.Count; j++)
                    {
                        dynamic ObjDoctorDetail = new JObject();
                        ObjDoctorDetail.MyDoctorId = items[j]["sMyDoctorId"];
                        ObjDoctorDetail.DoctorId = items[j]["DoctorId"];
                        ObjDoctorDetail.DoctorName = items[j]["DoctorName"];
                        ObjDoctorDetail.Mobile = items[j]["Mobile"];
                        ObjDoctorDetail.EmailId = items[j]["EmailId"];
                        ObjDoctorDetail.Specialization = items[j]["Specialization"];
                        ObjDoctorDetail.Clinic = items[j]["Clinic"];
                        ObjDoctorDetail.Degree = items[j]["Degree"];
                        ObjDoctorDetail.ProfilePic = items[j]["ProfilePic"];
                        Result.DoctorList.Add(ObjDoctorDetail); //Add Doctor details to array
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
        /// Fetch All Doctor List
        /// </summary>
        /// <param name="pagingparametermodel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AllDoctorList")]
        public string AllDoctorList([FromBody] PagingParameterModel pagingparametermodel)
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
                var _text = pagingparametermodel.Searching != "" ? CryptoHelper.Encrypt(pagingparametermodel.Searching) : "";

                SqlParameter[] param = new SqlParameter[]
               {
                    new SqlParameter("@UserId",UserId),
                    new SqlParameter("@SearchingText",pagingparametermodel.Searching),
                     new SqlParameter("@Searchingname",_text)
               };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetAllDoctorListwithsearchingfilter1", param);
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

                    Result.DoctorList = new JArray() as dynamic;   // Create Array for Doctor Details

                    for (int j = 0; j < items.Count; j++)
                    {
                        dynamic ObjDoctorDetail = new JObject();
                        ObjDoctorDetail.DoctorId = items[j]["sAppUserId"];
                        ObjDoctorDetail.DoctorName = items[j]["sFullName"];
                        ObjDoctorDetail.Mobile = items[j]["sMobile"];
                        ObjDoctorDetail.EmailId = items[j]["sEmailId"];
                        ObjDoctorDetail.Address = items[j]["sAddress"];
                        ObjDoctorDetail.Specialization = items[j]["sSpecialization"];
                        ObjDoctorDetail.Clinic = items[j]["sClinic"];
                        ObjDoctorDetail.Degree = items[j]["sDegree"];
                        ObjDoctorDetail.ProfilePic = items[j]["sImagePath"];
                        Result.DoctorList.Add(ObjDoctorDetail); //Add Doctor details to array
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
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        /// <summary>
        /// Add doctor in my doctor list, DoctorId should be "," seprated if we need to add more than 1 doctor
        /// </summary>
        /// <param name="model"> All field Mandatory</param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddDoctortoMyDoctorList")]
        public string AddDoctortoMyDoctorList([FromBody] MyDoctorList model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            string _doctorId = "";
            try
            {
                if (Ival.IsTextBoxEmpty(model.DoctorId))
                {
                    Msg += "Please Enter Valid Doctor Id";
                }
                else
                {
                    string[] splitdoc = model.DoctorId.ToString().Split(',');
                    foreach (string docId in splitdoc)
                    {
                        if (!Ival.IsInteger(docId))
                        {
                            Msg += "Please Enter Valid Doctor Id";
                        }
                        else
                        {
                            _doctorId += docId + ",";
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
                    _doctorId = _doctorId.TrimEnd(',');
                    string[] splitDoctor = _doctorId.Split(',');
                    foreach (string DocID in splitDoctor)
                    {
                        SqlParameter[] param = new SqlParameter[]
                        {
                            new SqlParameter("@PatientId",UserId),
                            new SqlParameter("@DoctorId",DocID),
                            new SqlParameter("@returnval",SqlDbType.Int)
                        };
                        data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp__AddDoctorInMylist", param);
                    }
                    if (data == 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Doctor's Added successfully.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else if (data == -2)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Doctor already exists in your list.";
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
        /// Remove Doctor from My Doctor list. Patient Id and Doctor Id are Mandatory.
        /// </summary>
        /// <param name="model.PatientId">Mandatory</param>
        /// <param name="model.DoctorId">Mandatory</param>
        /// <returns></returns>
        [HttpPost]
        [Route("RemoveDoctorFormMyDoctorList")]
        public string RemoveDoctorFormMyDoctorList([FromBody] MyDoctorList model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (!Ival.IsInteger(model.DoctorId))
                {
                    Msg += "Please Enter Valid Doctor Id";
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
                        new SqlParameter("@PatientId",UserId),
                        new SqlParameter("@DoctorId",model.DoctorId),
                        new SqlParameter("@returnval",SqlDbType.Int)
                    };
                    data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp__RemoveDoctorFromPatientList", param);
                    if (data == 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Remove Added successfully.";
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
        /// Get shared report doctor list
        /// </summary>
        /// <param name="pagingparametermodel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("MySharedReportDoctorList")]
        public string MySharedReportDoctorList([FromBody] PagingParameterModel pagingparametermodel)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                // DataTable dt = DAL.GetDataTable("WS_Sp__GetMySharedReportDoctorList " + UserId);
                SqlParameter[] param = new SqlParameter[]
                 {
                         new SqlParameter("@UserId",UserId),
                         new SqlParameter("@SearchingText",pagingparametermodel.Searching)
                 };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp__GetMySharedReportDoctorListwithsearch", param);
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


                    Result.DoctorList = new JArray() as dynamic;   // Create Array for Doctor Details

                    for (int j = 0; j < items.Count; j++)
                    {
                        dynamic ObjDoctorDetail = new JObject();
                        ObjDoctorDetail.DoctorId = items[j]["DoctorId"];
                        ObjDoctorDetail.DoctorName = items[j]["DoctorName"];
                        ObjDoctorDetail.Mobile = items[j]["Mobile"];
                        ObjDoctorDetail.EmailId = items[j]["EmailId"];
                        ObjDoctorDetail.Specialization = items[j]["Specialization"];
                        ObjDoctorDetail.Clinic = items[j]["Clinic"];
                        ObjDoctorDetail.Degree = items[j]["Degree"];
                        ObjDoctorDetail.ProfilePic = items[j]["ProfilePic"];
                        Result.DoctorList.Add(ObjDoctorDetail); //Add Doctor details to array
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
        /// Get Shared report details by patient id and doctor Id
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PatientSharedReportList")]
        public string PatientSharedReportList([FromBody] PatientSharedReport model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (!Ival.IsInteger(model.DoctorId))
                {
                    Msg += "Please Enter Valid Doctor Id";
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
                        new SqlParameter("@PatientId",UserId),
                        new SqlParameter("@DoctorId",model.DoctorId)
                     };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_PatientSharedReortList", param);
                    Result.ReportList = new JArray() as dynamic;   // Create Array for Report List
                    if (dt.Rows.Count > 0)
                    {
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            dynamic ObjReportDetail = new JObject();
                            ObjReportDetail.Flag = "";
                            ObjReportDetail.BookLabTestId = dt.Rows[j]["sBookLabTestId"];
                            ObjReportDetail.SharedReportId = dt.Rows[j]["sSharedReportId"];
                            ObjReportDetail.TestCode = dt.Rows[j]["sTestCode"];
                            ObjReportDetail.TestName = dt.Rows[j]["sTestName"];
                            ObjReportDetail.Date = dt.Rows[j]["Createddate"];
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
        /// Unshared report with doctor by shared report Id
        /// </summary>
        /// <param name="ReportId">Mandatory</param>
        /// <returns></returns>
        [HttpPost]
        [Route("UnshreadMyReport")]
        public string UnshreadMyReport([FromBody] UnsharedReport model)
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
                string _ReportId = "";
                if (Ival.IsTextBoxEmpty(model.ReportId))
                {
                    Msg += "Please Enter Valid ReportId";
                }
                else
                {
                    string[] splitReport = model.ReportId.ToString().Split(',');
                    foreach (string Report in splitReport)
                    {
                        if (!Ival.IsInteger(Report))
                        {
                            Msg += "Please Enter Valid Report Id";
                        }
                        else
                        {
                            _ReportId += Report + ",";
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
                    _ReportId = _ReportId.TrimEnd(',');
                    string[] splitReports = _ReportId.Split(',');
                    foreach (string Reports in splitReports)
                    {
                        SqlParameter[] param = new SqlParameter[]
                        {
                            new SqlParameter("@ReportId",Reports),
                            new SqlParameter("@returnval",SqlDbType.Int)
                        };
                        data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_UnshredReport", param);
                    }
                    if (data == 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Report unshared successfully.";
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
        /// Get recommendation test details
        /// </summary>
        /// <param name="RcomId">Mandatory</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetRecommendationTestDetails/{RcomId}")]
        public string GetRecommendationTestDetails(int RcomId)
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
                if (!Ival.IsInteger(RcomId.ToString()))
                {
                    Msg += "Please Enter Valid Recommendation Id";
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
                    DataTable dt = DAL.GetDataTable("Sp_GetTestRecommendedDetailsByRcomId " + RcomId);

                    if (dt.Rows.Count > 0)
                    {
                        Result.Status = true;  //  Status Key
                        Result.TestList = new JArray() as dynamic;   // Create Array for Doctor Details
                        dynamic ObjTestDetail = new JObject();
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            ObjTestDetail.RecommendationId = dt.Rows[j]["sRecommendationId"];
                            ObjTestDetail.RecommendedDate = dt.Rows[j]["sRecommendedAt"];
                            ObjTestDetail.DoctorName = dt.Rows[j]["sDoctor"];
                            ObjTestDetail.sComment = dt.Rows[j]["sComment"];
                            ObjTestDetail.DoctorId = dt.Rows[j]["sDoctorId"];
                            ObjTestDetail.TestId = dt.Rows[j]["sTestId"];
                            ObjTestDetail.TestCode = dt.Rows[j]["sTestCode"];
                            ObjTestDetail.TestName = dt.Rows[j]["sTestName"];
                            ObjTestDetail.TestUsefulFor = dt.Rows[j]["sTestUsefulFor"];
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

        /// <summary>
        /// Get Doctor notes on shared report
        /// </summary>
        /// <param name="ReportId">Mandatory</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetDoctorNote/{ReportId}")]
        public string GetDoctorNote(int ReportId)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
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
                    SqlParameter[] param = new SqlParameter[]
                    {
                        new SqlParameter("@SharedReportID",ReportId),
                        new SqlParameter("@UserId",UserId)
                    };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp__GetNoteListsBySharedReportID ", param);
                    if (dt.Rows.Count > 0)
                    {
                        Result.Status = true;  //  Status Key
                        Result.TestList = new JArray() as dynamic;   // Create Array for Doctor Details
                        dynamic ObjTestDetail = new JObject();
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            ObjTestDetail.ReportId = dt.Rows[j]["ReportId"];
                            ObjTestDetail.Note = dt.Rows[j]["Note"];
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

        /// <summary>
        /// Shared reports with doctor
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddSharedReport")]
        public string AddSharedReport([FromBody] SharedReport model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            var UserName = User.Claims.FirstOrDefault(x => x.Type.Equals("UserName", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string _ReportId = "";
            try
            {
                string Msg = "";
                if (Ival.IsTextBoxEmpty(model.ReportId))
                {
                    Msg += "Please Enter Valid ReportId";
                }
                else
                {
                    string[] splitReport = model.ReportId.ToString().Split(',');
                    foreach (string Report in splitReport)
                    {
                        if (!Ival.IsInteger(Report))
                        {
                            Msg += "Please Enter Valid Report Id";
                        }
                        else
                        {
                            _ReportId += Report + ",";
                        }
                    }
                }
                if (!Ival.IsInteger(model.DoctorId.ToString()))
                {
                    Msg += "Please Enter Valid Doctor Id ";
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
                    _ReportId = _ReportId.TrimEnd(',');
                    string[] splitReports = _ReportId.Split(',');
                    foreach (string Reports in splitReports)
                    {
                        SqlParameter[] param = new SqlParameter[]
                            {
                                new SqlParameter("@ReportId",Reports),
                                new SqlParameter("@DoctorsId",model.DoctorId),
                                new SqlParameter("@PatientsId",UserId),
                                new SqlParameter("@returnval",SqlDbType.Int)
                            };
                        data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp__AddSharedReport", param);
                    }
                    if (data == 1)
                    {
                        DataTable dt = DAL.GetDataTable("WS_Sp_GetUserdevicetoken " + model.DoctorId);
                        if (dt.Rows.Count > 0)
                        {
                            string _title = "Shared Report";
                            string _Devicetoken = dt.Rows[0]["sDeviceToken"].ToString();
                            string _Msg = "Your patient " + UserName + " has shared a report with you. Please take a look.";
                            dynamic _Result = new JObject();
                            _Result.PatientId = UserId;
                            string _payload = JsonConvert.SerializeObject(_Result);
                            string _type = "Shared Report";
                            fcm.SendNotification(_title, _Msg, _Devicetoken, _type, UserId);
                            Notification.AppNotification(model.DoctorId.ToString(), "", _title, _Msg, _type, _payload, UserId);
                        }

                        Result.Status = "Success";  //  Status Key 
                        Result.Msg = "Shared reports added successfully.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else if (data == -2)
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "Report already shared with doctor.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.Status = "Server Error";  //  Status Key 
                        Result.Msg = "Something went wrong,Please try again.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    return JSONString;
                }
            }
            catch (Exception ex)
            {
                Result.Status = "Failed";  //  Status Key
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);
                return JSONString;
            }
        }

        /// <summary>
        /// Get Patient Report List
        /// </summary>
        /// <param name="pagingparametermodel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("MyReportList")]
        public string MyReportList([FromBody] PagingParameterModel pagingparametermodel)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                SqlParameter[] param = new SqlParameter[]
                    {
                      new SqlParameter("@PatientId",UserId),
                      new SqlParameter("@SearchingText",pagingparametermodel.Searching)
                    };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetReportListwithSearching", param);
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

                    for (int j = 0; j < items.Count; j++)
                    {
                        dynamic ObjReportDetail = new JObject();
                        ObjReportDetail.TestCode = items[j]["sTestCode"];
                        ObjReportDetail.TimeSlot = items[j]["sTimeSlot"];
                        ObjReportDetail.TestDate = items[j]["sTestDate"];
                        ObjReportDetail.TestName = items[j]["sTestName"];
                        ObjReportDetail.ReportId = items[j]["sBookLabTestId"];
                        ObjReportDetail.ProfileName = items[j]["sProfileName"];
                        ObjReportDetail.LabName = items[j]["sLabName"];
                        ObjReportDetail.LabLogo = items[j]["sLabLogo"];
                        ObjReportDetail.ReportDate = items[j]["ReportDate"];
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

        /// <summary>
        /// Fetch Report Details by Report Id
        /// </summary>
        /// <param name="ReportId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("MyReportDetails/{ReportId}")]
        public string MyReportDetails(int ReportId)
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
                                    new SqlParameter("@booklabtestid",ReportId),
                                    new SqlParameter("@UserId",UserId)
                                               };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("Sp_GetReportDetails", param);

                    if (dt.Rows.Count > 0)
                    {
                        SqlParameter[] param1 = new SqlParameter[]
                                              {
                                    new SqlParameter("@booklabtestid",ReportId),
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
                        ReportDetailsObject.Flag = "";

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
                        Result.DocNote = new JArray() as dynamic;
                        SqlParameter[] paramdoc = new SqlParameter[]
                       {
                             new SqlParameter("@SharedReportID",ReportId),
                            new SqlParameter("@UserId",UserId)
                       };
                        DataTable dtDoc = DAL.ExecuteStoredProcedureDataTable("WS_Sp__GetNoteListsBySharedReportID ", paramdoc);
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

        [HttpGet]
        [Route("GetReportDetilsForGraph/{TestId}")]
        public string GetReportDetilsForGraph(int TestId)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Createroot JSON Object
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
                    var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
                    SqlParameter[] param = new SqlParameter[]
                               {
                                new SqlParameter("@TestID",TestId),
                               new SqlParameter("@UserID",UserId)
                              };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetReportDetailsByUserIdandTestId", param);

                    if (dt.Rows.Count > 0)
                    {
                        Result.ReportList = new JArray() as dynamic;

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            dynamic ReportDetailsObject = new JObject();
                            ReportDetailsObject.TestName = dt.Rows[i]["sTestName"];
                            ReportDetailsObject.TestCode = dt.Rows[i]["sTestCode"];
                            ReportDetailsObject.TestId = dt.Rows[i]["sTestId"];
                            ReportDetailsObject.TestDate = dt.Rows[i]["sTestDate"];
                            DataTable dt1 = DAL.GetDataTable("Sp_GetReportValuesforgraph " + dt.Rows[i]["sBookLabTestId"]);

                            ReportDetailsObject.SubReportList = new JArray() as dynamic;
                            for (int j = 0; j < dt1.Rows.Count; j++)
                            {
                                dynamic SubPaymentObj = new JObject();

                                SubPaymentObj.Analyte = dt1.Rows[j]["sAnalyte"];
                                SubPaymentObj.Subanalyte = dt1.Rows[j]["sSubanalyte"];
                                SubPaymentObj.ResultType = dt1.Rows[j]["sResultType"];
                                SubPaymentObj.AgeGroup = dt1.Rows[j]["sAge"];
                                SubPaymentObj.MaleReferenceRange = dt1.Rows[j]["sMale"];
                                SubPaymentObj.FemaleReferenceRange = dt1.Rows[j]["sFemale"];
                                SubPaymentObj.Grade = dt1.Rows[j]["sGrade"];
                                SubPaymentObj.Units = dt1.Rows[j]["sUnits"];
                                SubPaymentObj.Interpretation = dt1.Rows[j]["sInterpretation"];
                                SubPaymentObj.Value = dt1.Rows[j]["sValue"];
                                SubPaymentObj.Result = dt1.Rows[j]["sResult"];
                                ReportDetailsObject.SubReportList.Add(SubPaymentObj);
                            }
                            Result.ReportList.Add(ReportDetailsObject);
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

        /// <summary>
        /// Get Patient Suggested test list
        /// </summary>
        /// <param name="pagingparametermode"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("MySuggestedTest")]
        public string MySuggestedTest([FromBody] PagingParameterModel pagingparametermode)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Createroot JSON Object
            try
            {
                var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
                DataTable dt = DAL.GetDataTable("Sp_GetPatientRecommendationsList " + UserId);
                if (dt.Rows.Count > 0)
                {
                    dt.Columns.Add("TestName", typeof(string));
                    dt.Columns.Add("testId", typeof(string));
                    dt.Columns.Add("Doctor", typeof(string));
                    dt.Columns.Add("LabName", typeof(string));
                    dt.Columns.Add("LabId", typeof(string));
                    dt.Columns.Add("TestPrice", typeof(string));
                    dt.Columns.Add("LabAddress", typeof(string));
                    dt.Columns.Add("LabContact", typeof(string));
                    dt.Columns.Add("LabLogo", typeof(string));
                    dt.Columns.Add("OnlinePayment", typeof(bool));
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
                            DataTable dt1 = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetTestDetailsByRecommendationsIdwithSearch", param);

                            if (dt1.Rows.Count > 0)
                            {
                                string testId = "";
                                string TestName = "";
                                string TestPrice = "";

                                string Doctor = dt1.Rows[0]["sDoctor"].ToString();
                                string LabName = dt1.Rows[0]["sLabName"].ToString();
                                string LabId = dt1.Rows[0]["sLabId"].ToString();

                                string LabAddress = dt1.Rows[0]["sLabAddress"].ToString();
                                string LabLogo = dt1.Rows[0]["sLabLogo"].ToString();
                                string LabContact = dt1.Rows[0]["sLabContact"].ToString();
                                bool OnlinePayment = Convert.ToBoolean(dt1.Rows[0]["OnlinePayment"].ToString());

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
                                row["Doctor"] = Doctor;
                                row["LabName"] = LabName;
                                row["LabId"] = LabId;
                                row["LabAddress"] = LabAddress;
                                row["LabLogo"] = LabLogo;
                                row["LabContact"] = LabContact;
                                row["OnlinePayment"] = OnlinePayment;
                            }
                        }
                    }
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

                    for (int i = 0; i < items.Count; i++)
                    {
                        dynamic ObjASuggestTestDetail = new JObject();
                        ObjASuggestTestDetail.RecommendationId = items[i]["sRecommendationId"];
                        ObjASuggestTestDetail.BookStatus = items[i]["sViewStatus"];
                        ObjASuggestTestDetail.RecommendedDate = items[i]["sRecommendedAt"];
                        ObjASuggestTestDetail.DoctorName = items[i]["Doctor"];
                        ObjASuggestTestDetail.DoctorId = items[i]["sDoctorId"];
                        ObjASuggestTestDetail.LabId = items[i]["LabId"];
                        ObjASuggestTestDetail.LabName = items[i]["LabName"];
                        ObjASuggestTestDetail.TestName = items[i]["TestName"];
                        ObjASuggestTestDetail.testId = items[i]["testId"];
                        ObjASuggestTestDetail.TestPrice = items[i]["TestPrice"];
                        ObjASuggestTestDetail.LabAddress = items[i]["LabAddress"];
                        ObjASuggestTestDetail.LabLogo = items[i]["LabLogo"];
                        ObjASuggestTestDetail.LabContact = items[i]["LabContact"];
                        ObjASuggestTestDetail.LabOnlinePayment = items[i]["OnlinePayment"];
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
            catch (Exception e)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        /// <summary>
        /// Get Patient Payment History
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("PaymentHistory")]
        public string PaymentHistory()
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Createroot JSON Object
            try
            {
                var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
                DataTable dt = DAL.GetDataTable("Sp_WS_GetPaymentInvoiceByuserId " + UserId);

                if (dt.Rows.Count > 0)
                {
                    Result.PaymentList = new JArray() as dynamic;   // Create Array for Section Profile List
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        SqlParameter[] param = new SqlParameter[]
                            {
                                    new SqlParameter("@BookingId",dt.Rows[i]["sBookLabId"].ToString()),
                                    new SqlParameter("@LabId",dt.Rows[i]["sLabId"].ToString())
                            };
                        DataTable dt1 = DAL.ExecuteStoredProcedureDataTable("Sp_WS_GetTestDetailsByBookingId", param);

                        dynamic PaymentDetailsObject = new JObject(); //Create Profiles JSON Object for 
                        PaymentDetailsObject.BookLabId = dt.Rows[i]["sBookLabId"];
                        PaymentDetailsObject.LabName = dt.Rows[i]["sLabName"];
                        PaymentDetailsObject.LabLogo = dt.Rows[i]["sLabLogo"];
                        PaymentDetailsObject.TestDate = dt.Rows[i]["sTestDate"];
                        PaymentDetailsObject.TotalAmount = dt.Rows[i]["sFees"];
                        PaymentDetailsObject.PaymentStatus = dt.Rows[i]["sPaymentStatus"];

                        PaymentDetailsObject.SubPaymentList = new JArray() as dynamic;
                        for (int j = 0; j < dt1.Rows.Count; j++)
                        {
                            dynamic SubPaymentObj = new JObject();

                            SubPaymentObj.TestCode = dt1.Rows[j]["sTestCode"];
                            SubPaymentObj.TestId = dt1.Rows[j]["sTestId"];
                            SubPaymentObj.TestAmount = dt1.Rows[j]["sPrice"];
                            SubPaymentObj.TestName = dt1.Rows[j]["sTestName"];
                            PaymentDetailsObject.SubPaymentList.Add(SubPaymentObj); //Add each test details to Test list array
                        }
                        Result.PaymentList.Add(PaymentDetailsObject);
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
            catch (Exception e)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again.";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        /// <summary>
        /// Get Life Style Disorder List
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("LifestyleDisorder")]
        public string LifestyleDisorder()
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                DataTable dt = DAL.GetDataTable("WS_Sp_GetLifestyleDisorderlist");
                if (dt.Rows.Count > 0)
                {
                    Result.LifeStyleDisorderList = new JArray() as dynamic;   // Create Array for Baner Details
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjLifeStyleDisorderDetail = new JObject();
                        ObjLifeStyleDisorderDetail.Id = dt.Rows[j]["Id"];
                        ObjLifeStyleDisorderDetail.Name = dt.Rows[j]["Name"];
                        ObjLifeStyleDisorderDetail.Description = dt.Rows[j]["Description"];
                        ObjLifeStyleDisorderDetail.Image = dt.Rows[j]["Image"];
                        Result.LifeStyleDisorderList.Add(ObjLifeStyleDisorderDetail); //Add baner details to array
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
        /// Get Life style disorder test list
        /// </summary>
        /// <param name="LifeStyleId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("LifestyleDisorder/{LifeStyleId}")]
        public string LifestyleDisorderTestList(int LifeStyleId)
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
                if (!Ival.IsInteger(LifeStyleId.ToString()))
                {
                    Msg += "Please Enter Valid Life Style Id";
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
                    DataTable dt = DAL.GetDataTable("WS_Sp_GetLifestyleDisorderTestlist " + LifeStyleId);
                    if (dt.Rows.Count > 0)
                    {
                        Result.LifeStyleDisorderList = new JArray() as dynamic;
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            dynamic ObjLifeStyleDisorderDetail = new JObject();
                            ObjLifeStyleDisorderDetail.TestId = dt.Rows[j]["TestId"];
                            ObjLifeStyleDisorderDetail.TestCode = dt.Rows[j]["sTestCode"];
                            ObjLifeStyleDisorderDetail.TestName = dt.Rows[j]["sTestName"];
                            ObjLifeStyleDisorderDetail.TestUsefulFor = dt.Rows[j]["sTestUsefulFor"];
                            Result.LifeStyleDisorderList.Add(ObjLifeStyleDisorderDetail);
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
        /// Get Patient Recent top 5 test 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("MyRecentTest")]
        public string MyRecentTest()
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
                DataTable dt = DAL.GetDataTable("WS_Sp_GetPatientRecentTest " + UserId);
                if (dt.Rows.Count > 0)
                {
                    Result.AppointmentList = new JArray() as dynamic;   // Create Array for List Details
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjAppointmentDetail = new JObject();
                        ObjAppointmentDetail.BookingId = dt.Rows[j]["sBookLabId"].ToString();
                        ObjAppointmentDetail.TestProfileName = dt.Rows[j]["sTestProfileName"].ToString();
                        ObjAppointmentDetail.TestCode = dt.Rows[j]["sTestCode"].ToString();
                        ObjAppointmentDetail.TestName = dt.Rows[j]["sTestName"].ToString();
                        ObjAppointmentDetail.LabName = dt.Rows[j]["sLabName"].ToString();
                        ObjAppointmentDetail.BookingDate = dt.Rows[j]["sBookRequestedAt"].ToString();
                        ObjAppointmentDetail.BookStatus = dt.Rows[j]["sBookStatus"].ToString();
                        ObjAppointmentDetail.ReportApprovalStatus = dt.Rows[j]["sApprovalStatus"].ToString();
                        ObjAppointmentDetail.ReportId = dt.Rows[j]["sBookLabTestId"].ToString();
                        ObjAppointmentDetail.Flag = dt.Rows[j]["flag"].ToString();
                        Result.AppointmentList.Add(ObjAppointmentDetail); //Add Object details to array
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
        /// Get Patient Health Details
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("MyHealth")]
        public string MyHealth()
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
                DataTable dt = DAL.GetDataTable("WS_Sp_GetMyHealthDetails " + UserId);
                if (dt.Rows.Count > 0)
                {
                    Result.HealthList = new JArray() as dynamic;   // Create Array for List Details
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjHealthDetail = new JObject();
                        ObjHealthDetail.Name = dt.Rows[j]["Name"].ToString();
                        ObjHealthDetail.Value = dt.Rows[j]["Value"].ToString();
                        ObjHealthDetail.Result = dt.Rows[j]["Result"].ToString();
                        ObjHealthDetail.Unit = dt.Rows[j]["Unit"].ToString();
                        ObjHealthDetail.Date = dt.Rows[j]["Date"].ToString();
                        Result.HealthList.Add(ObjHealthDetail); //Add Object details to array
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
        [Route("MyHealthAnaylteDetails")]
        public string MyHealthAnaylteDetails(string Anayltename)
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
                if (Ival.IsTextBoxEmpty(Anayltename))
                {
                    Msg += "Please Enter Valid Anaylte Name";
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
                              new SqlParameter("@PatientId",UserId),
                              new SqlParameter("@Paramter",Anayltename)
                         };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetMyHealthDetailsByAnalyte", param);
                    if (dt.Rows.Count > 0)
                    {
                        Result.HealthList = new JArray() as dynamic;   // Create Array for List Details
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            dynamic ObjHealthDetail = new JObject();
                            ObjHealthDetail.Name = dt.Rows[j]["Name"].ToString();
                            ObjHealthDetail.Value = dt.Rows[j]["Value"].ToString();
                            ObjHealthDetail.Result = dt.Rows[j]["Result"].ToString();
                            ObjHealthDetail.Unit = dt.Rows[j]["Unit"].ToString();
                            ObjHealthDetail.Date = dt.Rows[j]["Date"].ToString();
                            ObjHealthDetail.MaleRange = dt.Rows[j]["MaleRange"].ToString();
                            ObjHealthDetail.FemaleRange = dt.Rows[j]["FemaleRange"].ToString();
                            Result.HealthList.Add(ObjHealthDetail); //Add Object details to array
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
        /// Get Patient Analyte details by dates {Date format : 'yyyy-MM-dd'}
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("MyHealthAnaylteDetailsByDates")]
        public string MyHealthAnaylteDetailsByDates([FromBody] MyHealthAnaylteDetails model)
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
                if (Ival.IsTextBoxEmpty(model.AnalyteName))
                {
                    Msg += "Please Enter Valid Anaylte Name";
                }
                if (!Ival.IsTextBoxEmpty(model.StartDate))
                {
                    if (!Ival.IsValidDateForDateFilteration(model.StartDate))
                    {
                        Msg += "Please Enter Valid Start date";
                    }
                }
                if (!Ival.IsTextBoxEmpty(model.EndDate))
                {
                    if (!Ival.IsValidDateForDateFilteration(model.EndDate))
                    {
                        Msg += "Please Enter Valid End Date";
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
                    var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
                    SqlParameter[] param = new SqlParameter[]
                         {
                              new SqlParameter("@PatientId",UserId),
                              new SqlParameter("@Paramter",model.AnalyteName),
                              new SqlParameter("@StartDate",model.StartDate),
                              new SqlParameter("@EndDate",model.EndDate)
                         };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetMyHealthDetailsByAnalyteandDates", param);
                    if (dt.Rows.Count > 0)
                    {
                        Result.HealthList = new JArray() as dynamic;   // Create Array for List Details
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            dynamic ObjHealthDetail = new JObject();
                            ObjHealthDetail.Name = dt.Rows[j]["Name"].ToString();
                            ObjHealthDetail.Value = dt.Rows[j]["Value"].ToString();
                            ObjHealthDetail.Result = dt.Rows[j]["Result"].ToString();
                            ObjHealthDetail.Unit = dt.Rows[j]["Unit"].ToString();
                            ObjHealthDetail.Date = dt.Rows[j]["Date"].ToString();
                            ObjHealthDetail.MaleRange = dt.Rows[j]["MaleRange"].ToString();
                            ObjHealthDetail.FemaleRange = dt.Rows[j]["FemaleRange"].ToString();
                            Result.HealthList.Add(ObjHealthDetail); //Add Object details to array
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
        /// Get My upcoming test list
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("MyUpcomingTest")]
        public string MyUpcomingTest()
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
                DataTable dt = DAL.GetDataTable("Sp_GetMyUpcomingTestListUpdated " + UserId);
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

                    Result.AppointmentList = new JArray() as dynamic;   // Create Array for List Details
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjAppointmentDetail = new JObject();
                        ObjAppointmentDetail.BookingId = dt.Rows[j]["sBookLabId"].ToString();
                        ObjAppointmentDetail.TestProfileName = dt.Rows[j]["TestProfile"].ToString();
                        ObjAppointmentDetail.TestCode = dt.Rows[j]["testCode"].ToString();
                        ObjAppointmentDetail.TestName = dt.Rows[j]["TestName"].ToString();
                        ObjAppointmentDetail.TestId = dt.Rows[j]["testId"].ToString();
                        ObjAppointmentDetail.LabName = dt.Rows[j]["sLabName"].ToString();
                        ObjAppointmentDetail.LabLogo = dt.Rows[j]["sLabLogo"].ToString();
                        ObjAppointmentDetail.LabContact = dt.Rows[j]["sLabContact"].ToString();
                        ObjAppointmentDetail.LabAddress = dt.Rows[j]["sLabAddress"].ToString();
                        ObjAppointmentDetail.LabId = dt.Rows[j]["sLabId"].ToString();
                        ObjAppointmentDetail.BookingDate = dt.Rows[j]["sBookRequestedAt"].ToString();
                        ObjAppointmentDetail.TestDate = dt.Rows[j]["sTestDate"].ToString();
                        ObjAppointmentDetail.BookStatus = dt.Rows[j]["sBookStatus"].ToString();
                        Result.AppointmentList.Add(ObjAppointmentDetail); //Add Object details to array
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
        /// Search Howzu User
        /// </summary>
        /// <param name="pagingparametermodel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SearchFamilyMember")]
        public string SearchFamilyMember([FromBody] PagingParameterModel pagingparametermodel)
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
                if (Ival.IsTextBoxEmpty(pagingparametermodel.Searching))
                {
                    Msg += "No record found";
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
                    var _text = pagingparametermodel.Searching != "" ? CryptoHelper.Encrypt(pagingparametermodel.Searching) : "";
                    SqlParameter[] param = new SqlParameter[]
                                {
                              new SqlParameter("@UserId",UserId),
                              new SqlParameter("@SearchName",pagingparametermodel.Searching),
                              new SqlParameter("@SearchingText",_text)
                               };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetUserlist1", param);

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
        /// Send request to your family member
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddFamilyMember")]
        public string AddFamilyMember([FromBody] FamilyMember model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            var UserName = User.Claims.FirstOrDefault(x => x.Type.Equals("UserName", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
                if (!Ival.IsInteger(model.FamilyMemberId.ToString()))
                {
                    Msg += "Please Enter Valid Family Member Id";
                }
                if (!Ival.IsInteger(model.Relation))
                {
                    Msg += "Please Enter Valid Relation";
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
                    Random generator = new Random();
                    string OTP = generator.Next(1, 10000).ToString("D4");
                    int data = 0;
                    SqlParameter[] param = new SqlParameter[]
                        {
                              new SqlParameter("@UserId",UserId),
                              new SqlParameter("@FamilymemberId",model.FamilyMemberId),
                              new SqlParameter("@RequestStatus","Pending"),
                              new SqlParameter("@Relation",model.Relation),
                              new SqlParameter("@OTP",OTP),
                              new SqlParameter("@returnval",SqlDbType.Int)
                        };
                    data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_AddFamilyMember", param);

                    if (data >= 1)
                    {
                        DataTable dt = DAL.GetDataTable("WS_Sp_GetUserBirthdate " + model.FamilyMemberId);
                        if (dt.Rows.Count > 0)
                        {
                            string _title = "Family Member Request";
                            string _Devicetoken = dt.Rows[0]["sDeviceToken"].ToString();
                            string _Msg = "Mr/Mrs." + UserName + " has sent you the request to view your report";

                            dynamic _Result = new JObject();
                            _Result.RequestId = data;
                            string _payload = JsonConvert.SerializeObject(_Result);

                            string _type = "Family Member";
                            fcm.SendNotification(_title, _Msg, _Devicetoken, _type, data.ToString());
                            Notification.AppNotification(model.FamilyMemberId.ToString(), "", _title, _Msg, _type, _payload, UserId);

                            string _birthdate = dt.Rows[0]["sBirthDate"].ToString();
                            string _Mobile = dt.Rows[0]["sMobile"].ToString() != "" ? CryptoHelper.Decrypt(dt.Rows[0]["sMobile"].ToString()) : "";
                            string _EmailId = dt.Rows[0]["sEmailId"].ToString() != "" ? CryptoHelper.Decrypt(dt.Rows[0]["sEmailId"].ToString()) : "";
                            string _FullName = dt.Rows[0]["sFullName"].ToString();

                            CalculateAge _age = new CalculateAge();
                            string DateOfBirth = _birthdate;
                            string Currentdate = DateTime.Now.ToShortDateString();
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

                            if (PatientAge < 18 && patientageunit == "year")
                            {
                                Result.Below18 = true;
                                SendOTP _otp = new SendOTP();
                                if (!Ival.IsTextBoxEmpty(_Mobile))
                                {
                                    if (Ival.MobileValidation(_Mobile))
                                    {
                                        int result = _otp.FamilyMemberOTP(_Mobile, OTP);

                                        if (result == 200)
                                        {
                                            Result.Msg += "& OTP sent on registered mobile number.";
                                        }
                                    }
                                }
                                else
                                {
                                    if (Ival.IsValidEmailAddress(_EmailId))
                                    {
                                        string _emailOP = _otp.sendmail(_EmailId, OTP, _FullName);
                                        if (_emailOP == "1")
                                        {
                                            Result.Msg += "& OTP sent on registered email address.";
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Result.Below18 = false;
                            }
                        }
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Request sent successfully.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else if (data == -2)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Already exists.";
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
        /// Get list of relation
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("FamilyRelation")]
        public string FamilyRelation()
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();
            try
            {
                DataTable dt = DAL.GetDataTable("WS_Sp_GetFamilyRelationList ");
                if (dt.Rows.Count > 0)
                {
                    Result.List = new JArray() as dynamic;
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjDetail = new JObject();
                        ObjDetail.Id = dt.Rows[j]["Id"];
                        ObjDetail.Name = dt.Rows[j]["Name"];
                        Result.List.Add(ObjDetail);
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
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);

            }
            return JSONString;
        }

        /// <summary>
        /// Get User Pending family request's list
        /// </summary>
        /// <param name="pagingparametermodel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UserPendingRequest")]
        public string UserPendingRequest([FromBody] PagingParameterModel pagingparametermodel)
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
                SqlParameter[] param = new SqlParameter[]
                            {
                              new SqlParameter("@SearchingText",pagingparametermodel.Searching),
                              new SqlParameter("@UserId",UserId)
                           };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetMyPendinrequest", param);

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
                        ObjPatientDetail.RequestId = dt.Rows[j]["Id"];
                        ObjPatientDetail.UserId = dt.Rows[j]["sAppUserId"];
                        ObjPatientDetail.Name = dt.Rows[j]["sFullName"];
                        ObjPatientDetail.Mobile = dt.Rows[j]["sMobile"];
                        ObjPatientDetail.EmailId = dt.Rows[j]["sEmailId"];
                        ObjPatientDetail.BirthDate = dt.Rows[j]["sBirthDate"];
                        ObjPatientDetail.ProfilePic = dt.Rows[j]["sImagePath"];
                        ObjPatientDetail.Relation = dt.Rows[j]["Name"];
                        ObjPatientDetail.RequestStatus = dt.Rows[j]["RequestStatus"];
                        ObjPatientDetail.Date = dt.Rows[j]["CreatedDate"];
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
        /// Get User Family Member list
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("MyFamilyMemberList")]
        public string MyFamilyMemberList()
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
                SqlParameter[] param = new SqlParameter[]
                            {
                              new SqlParameter("@UserId",UserId)
                           };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetMyFamilyList", param);
                if (dt.Rows.Count > 0)
                {
                    Result.PatientList = new JArray() as dynamic;
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjPatientDetail = new JObject();
                        ObjPatientDetail.RequestId = dt.Rows[j]["Id"];
                        ObjPatientDetail.UserId = dt.Rows[j]["sAppUserId"];
                        ObjPatientDetail.Name = dt.Rows[j]["sFullName"];
                        ObjPatientDetail.Mobile = dt.Rows[j]["sMobile"];
                        ObjPatientDetail.EmailId = dt.Rows[j]["sEmailId"];
                        ObjPatientDetail.BirthDate = dt.Rows[j]["sBirthDate"];
                        ObjPatientDetail.ProfilePic = dt.Rows[j]["sImagePath"];
                        ObjPatientDetail.Relation = dt.Rows[j]["Name"];
                        ObjPatientDetail.RequestStatus = dt.Rows[j]["RequestStatus"];
                        ObjPatientDetail.Date = dt.Rows[j]["CreatedDate"];
                        Result.PatientList.Add(ObjPatientDetail);
                    }
                    Result.Status = true;  //  Status Key
                    Result.Msg = "Success";
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
        /// Request status should be {Pending/Rejected/Accepted }
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateFamilyRequestStatus")]
        public string UpdateFamilyRequestStatus([FromBody] FamilyRequestStatus model)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            var UserName = User.Claims.FirstOrDefault(x => x.Type.Equals("UserName", StringComparison.InvariantCultureIgnoreCase)).Value;
            try
            {
                string Msg = "";
                string _status = model.Status.ToLower();
                if (_status != "pending" && _status != "rejected" && _status != "accepted")
                {
                    Msg += "Please Enter Valid Status {ex.pending/rejected/accepted}";
                }
                if (!Ival.IsInteger(model.RequestId.ToString()))
                {
                    Msg += "Please Enter Valid Request Id";
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
                    SqlParameter[] param1 = new SqlParameter[]
                    {
                        new SqlParameter("@RequestId",model.RequestId),
                        new SqlParameter("@UserId",UserId),
                        new SqlParameter("@Status",model.Status),
                        new SqlParameter("@returnval",SqlDbType.Int),
                    };
                    int Val = DAL.ExecuteStoredProcedureRetnInt("Sp_UpdateFamilyMemberRequest", param1);
                    if (Val == 1)
                    {
                        DataTable dt = DAL.GetDataTable("Sp_GuardianDetails " + model.RequestId);
                        if (dt.Rows.Count > 0)
                        {
                            string _title = "Request Status";
                            string _Devicetoken = dt.Rows[0]["DeviceTokan"].ToString();
                            string _PatientId = dt.Rows[0]["PatientId"].ToString();
                            string _Msg = "Mr/Mrs." + UserName + " " + model.Status + " your request to view reports";
                            string _payload = "";
                            string _type = "Family Member";
                            fcm.SendNotification(_title, _Msg, _Devicetoken, _type, _payload);
                            Notification.AppNotification(_PatientId, "", _title, _Msg, _type, _payload, UserId);
                        }
                        if (_status == "rejected")
                        {
                            Result.Msg = "Decline request.";
                        }
                        else
                        {
                            Result.Msg = "Family member added successfully.";
                        }
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
                Result.Msg = "Something went wrong,Please try again."; ;
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        /// <summary>
        /// Add new family member
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("RegisterFamilyMember")]
        public string RegisterFamilyMember([FromBody] RegisterFamilyMember model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (!Ival.IsCharOnly(model.FullName))
                {
                    Msg += "Please Enter Valid User Name";
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
                        Msg += "Please Enter Valid Email Id";
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
                    else
                    {
                        Msg += "Please Enter Valid Mobile Number";
                    }
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
                if (!Ival.IsInteger(model.Relation))
                {
                    Msg += "Please Enter Valid Relation";
                }
                if (!Ival.ValidatePassword(model.Password.ToString()))
                {
                    Msg += "Please enter Minimum 6 characters at least 1 Uppercase Alphabet, 1 Lowercase Alphabet, 1 Number and 1 Special Character";
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
                        new SqlParameter("@UserId",UserId),
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
                    int data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_RegisterFamilyMenberUpdated", param);
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
                            new SqlParameter("@Returnval",SqlDbType.Int)
                            };
                        int ResultVal1 = DAL.ExecuteStoredProcedureRetnInt("Sp_AddUserLoginCredentials", param2);

                        if (ResultVal1 == 1)
                        {
                            Random generator = new Random();
                            string OTP = generator.Next(1, 10000).ToString("D4");

                            SqlParameter[] param4 = new SqlParameter[]
                            {
                              new SqlParameter("@UserId",UserId),
                              new SqlParameter("@FamilymemberId",data),
                              new SqlParameter("@RequestStatus","Verification Pending"),
                              new SqlParameter("@Relation",model.Relation),
                              new SqlParameter("@OTP",OTP),
                              new SqlParameter("@returnval",SqlDbType.Int)
                            };
                            int _requestId = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_AddFamilyMember", param4);

                            SendOTP _otp = new SendOTP();
                            if (!Ival.IsTextBoxEmpty(model.Mobile))
                            {
                                if (Ival.MobileValidation(model.Mobile))
                                {
                                    int result = _otp.FamilyMemberOTP(model.Mobile, OTP);

                                    if (result == 200)
                                    {
                                        Result.Msg += "OTP sent on registered mobile number.";
                                    }
                                }
                            }
                            else
                            {
                                if (Ival.IsValidEmailAddress(model.EmailId))
                                {
                                    string _emailOP = _otp.sendmail(model.EmailId, OTP, model.FullName);
                                    if (_emailOP == "1")
                                    {
                                        Result.Msg += "OTP sent on registered email address.";
                                    }
                                }
                            }
                        }
                        Result.Status = true;  //  Status Key 
                        Result.FamilyMemberId = data;  //  Status Key 
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
        /// Verify family member request using family member id & OTP
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("VerifiedFamilyMember")]
        public string VerifiedFamilyMember([FromBody] VerifyFamilyMemberRequest model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";

                if (!Ival.IsInteger(model.FamilyMemberId))
                {
                    Msg += "Please Enter Valid family member Id";
                }
                if (!Ival.IsInteger(model.OTP.ToString()))
                {
                    Msg += "Please Enter Valid otp";
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
                    SqlParameter[] param1 = new SqlParameter[]
                   {
                        new SqlParameter("@FamilymemberId",model.FamilyMemberId),
                        new SqlParameter("@OTP",model.OTP),
                        new SqlParameter("@UserId",UserId),
                        new SqlParameter("@returnval",SqlDbType.Int),
                   };
                    int Val = DAL.ExecuteStoredProcedureRetnInt("Sp_verifyFamilyMemberRequest", param1);
                    if (Val == 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Family member added successfully.";
                        JSONString = JsonConvert.SerializeObject(Result);//Add user details to array
                    }
                    else if (Val == -1)
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "Wrong OTP";
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
                Result.Msg = "Something went wrong,Please try again."; ;
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        /// <summary>
        /// Delete family member from user list
        /// </summary>
        /// <param name="RequestId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DeleteFamilyMember")]
        public string DeleteFamilyMember(int RequestId)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            var UserName = User.Claims.FirstOrDefault(x => x.Type.Equals("UserName", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";

                if (!Ival.IsInteger(RequestId.ToString()))
                {
                    Msg += "Please Enter Valid request Id";
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
                    DataTable dt = DAL.GetDataTable("Sp_FamilyMemberDetails " + RequestId);
                    SqlParameter[] param1 = new SqlParameter[]
                   {
                        new SqlParameter("@RequestId",RequestId),
                        new SqlParameter("@UserId",UserId),
                        new SqlParameter("@returnval",SqlDbType.Int),
                   };
                    int Val = DAL.ExecuteStoredProcedureRetnInt("Sp_DeleteFamilyMember", param1);
                    if (Val == 1)
                    {
                        if (dt.Rows.Count > 0)
                        {
                            string _title = "Family Member Removed";
                            string _Devicetoken = dt.Rows[0]["DeviceTokan"].ToString();
                            string _PatientId = dt.Rows[0]["PatientId"].ToString();
                            string _Msg = "Mr/Mrs." + UserName + " removed you from his family member";
                            string _payload = "";
                            string _type = "Family Member";
                            fcm.SendNotification(_title, _Msg, _Devicetoken, _type, _payload);
                            Notification.AppNotification(_PatientId, "", _title, _Msg, _type, _payload, UserId);
                        }
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Family member deleted successfully.";
                        JSONString = JsonConvert.SerializeObject(Result);//Add user details to array
                    }
                    else if (Val == -1)
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "Please enter valid request Id";
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
                Result.Msg = "Something went wrong,Please try again."; ;
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        [HttpGet]
        [Route("ResendFamilymemberOTP")]
        public string ResendFamilymemberOTP(int RequestId)
        {
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;

            try
            {
                string Msg = "";

                if (!Ival.IsInteger(RequestId.ToString()))
                {
                    Msg += "Please Enter Valid Request Id.";
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
                    Random generator = new Random();
                    string OTP = generator.Next(1, 10000).ToString("D4");
                    SqlParameter[] param1 = new SqlParameter[]
                    {
                        new SqlParameter("@OTP",OTP),
                        new SqlParameter("@UserId",UserId),
                        new SqlParameter("@RequestId",RequestId),
                        new SqlParameter("@returnval",SqlDbType.Int),
                    };
                    int Val = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_UpdateFamilymemberOTP", param1);
                    if (Val >= 1)
                    {
                        DataTable dt = DAL.GetDataTable("WS_Sp_GetUserBirthdate " + Val);
                        if (dt.Rows.Count > 0)
                        {
                            string _Mobile = dt.Rows[0]["sMobile"].ToString() != "" ? CryptoHelper.Decrypt(dt.Rows[0]["sMobile"].ToString()) : "";
                            string _EmailId = dt.Rows[0]["sEmailId"].ToString() != "" ? CryptoHelper.Decrypt(dt.Rows[0]["sEmailId"].ToString()) : "";
                            string _FullName = dt.Rows[0]["sFullName"].ToString();

                            SendOTP _otp = new SendOTP();
                            if (!Ival.IsTextBoxEmpty(_Mobile))
                            {
                                if (Ival.MobileValidation(_Mobile))
                                {
                                    int result = _otp.FamilyMemberOTP(_Mobile, OTP);

                                    if (result == 200)
                                    {
                                        Result.Msg += "OTP sent on registered mobile number.";
                                    }
                                }
                            }
                            else
                            {
                                if (Ival.IsValidEmailAddress(_EmailId))
                                {
                                    string _emailOP = _otp.sendmail(_EmailId, OTP, _FullName);
                                    if (_emailOP == "1")
                                    {
                                        Result.Msg += "OTP sent on registered email address.";
                                    }
                                }
                            }
                        }
                        Result.Status = true;
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else if (Val == -2)
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "Please enter valid request Id";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.Status = false;  //  Status Key
                        Result.Msg = "Something went wrong,Please try again."; ;
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
        /// Get Family member report list
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("MyFamilyMemberReportList")]
        public string MyFamilyMemberReportList([FromBody] FamilyMemberreportlist model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                SqlParameter[] param = new SqlParameter[]
                    {
                      new SqlParameter("@Userid",UserId),
                      new SqlParameter("@PatientId",model.Familymemberid),
                      new SqlParameter("@SearchingText",model.Searching)
                    };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetMyFamilymemberReportListwithSearching", param);
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
                                                                   //  dynamic ObjReportDetail = new JObject();

                    for (int j = 0; j < items.Count; j++)
                    {




                        dynamic ObjReportDetail = new JObject();


                        ObjReportDetail.TestCode = items[j]["sTestCode"];
                        ObjReportDetail.TimeSlot = items[j]["sTimeSlot"];
                        ObjReportDetail.TestDate = items[j]["sTestDate"];
                        ObjReportDetail.TestName = items[j]["sTestName"];
                        ObjReportDetail.ReportId = items[j]["sBookLabTestId"];
                        ObjReportDetail.ProfileName = items[j]["sProfileName"];
                        Result.ReportList.Add(ObjReportDetail); //Add Report details to array
                 
                        //Code hide by harshada 10/05/2022 to show all record of 2nd page

                        //ObjReportDetail.TestCode = dt.Rows[j]["sTestCode"];
                        //ObjReportDetail.TimeSlot = dt.Rows[j]["sTimeSlot"];

                        //ObjReportDetail.TestDate = dt.Rows[j]["sTestDate"];
                        //ObjReportDetail.TestName = items[j]["sTestName"];
                        //ObjReportDetail.ReportId = dt.Rows[j]["sBookLabTestId"];
                        //ObjReportDetail.ProfileName = dt.Rows[j]["sProfileName"];

                        //Result.ReportList.Add(ObjReportDetail);
                    }



                    //Result.ReportList = new JArray() as dynamic;   // Create Array for Report Details
                    //dynamic ObjReportDetail = new JObject();
                    //for (int j = 0; j < items.Count; j++)
                    //{
                    //    ObjReportDetail.TestCode = items[j]["sTestCode"];
                    //    ObjReportDetail.TimeSlot = items[j]["sTimeSlot"];
                    //    ObjReportDetail.TestDate = items[j]["sTestDate"];
                    //    ObjReportDetail.TestName = items[j]["sTestName"];
                    //    ObjReportDetail.ReportId = items[j]["sBookLabTestId"];
                    //    ObjReportDetail.ProfileName = items[j]["sProfileName"];
                    //    Result.ReportList.Add(ObjReportDetail); //Add Report details to array
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
        [Route("MyReportAccessFamilyMemberList")]
        public string MyReportAccessFamilyMemberList()
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
                SqlParameter[] param = new SqlParameter[]
                            {
                              new SqlParameter("@UserId",UserId)
                           };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetMyReportAccessFamilyList", param);
                if (dt.Rows.Count > 0)
                {
                    Result.PatientList = new JArray() as dynamic;
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjPatientDetail = new JObject();
                        ObjPatientDetail.RequestId = dt.Rows[j]["Id"];
                        ObjPatientDetail.UserId = dt.Rows[j]["sAppUserId"];
                        ObjPatientDetail.Name = dt.Rows[j]["sFullName"];
                        ObjPatientDetail.Mobile = dt.Rows[j]["sMobile"];
                        ObjPatientDetail.EmailId = dt.Rows[j]["sEmailId"];
                        ObjPatientDetail.BirthDate = dt.Rows[j]["sBirthDate"];
                        ObjPatientDetail.ProfilePic = dt.Rows[j]["sImagePath"];
                        ObjPatientDetail.Relation = dt.Rows[j]["Name"];
                        ObjPatientDetail.RequestStatus = dt.Rows[j]["RequestStatus"];
                        ObjPatientDetail.Date = dt.Rows[j]["CreatedDate"];
                        Result.PatientList.Add(ObjPatientDetail);
                    }
                    Result.Status = true;  //  Status Key
                    Result.Msg = "Success";
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

        [HttpGet]
        [Route("RevokeReportAccess")]
        public string RevokeReportAccess(int RequestId)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            var UserName = User.Claims.FirstOrDefault(x => x.Type.Equals("UserName", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";

                if (!Ival.IsInteger(RequestId.ToString()))
                {
                    Msg += "Please Enter Valid request Id";
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
                    DataTable dt = DAL.GetDataTable("Sp_GuardianDetails " + RequestId);
                    SqlParameter[] param1 = new SqlParameter[]
                   {
                        new SqlParameter("@RequestId",RequestId),
                        new SqlParameter("@UserId",UserId),
                        new SqlParameter("@returnval",SqlDbType.Int),
                   };
                    int Val = DAL.ExecuteStoredProcedureRetnInt("Sp_RevokeReportAccess", param1);
                    if (Val == 1)
                    {

                        if (dt.Rows.Count > 0)
                        {
                            string _title = "Family Member Removed";
                            string _Devicetoken = dt.Rows[0]["DeviceTokan"].ToString();
                            string _PatientId = dt.Rows[0]["PatientId"].ToString();
                            string _Msg = "Mr/Mrs." + UserName + " revoke access to view reports";
                            string _payload = "";
                            string _type = "Family Member";
                            fcm.SendNotification(_title, _Msg, _Devicetoken, _type, _payload);
                            Notification.AppNotification(_PatientId, "", _title, _Msg, _type, _payload, UserId);
                        }
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Family member deleted successfully.";
                        JSONString = JsonConvert.SerializeObject(Result);//Add user details to array
                    }
                    else if (Val == -1)
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "Please enter valid request Id";
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
                Result.Msg = "Something went wrong,Please try again."; ;
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        [HttpGet]
        [Route("DeleteDoctorFromPatientList")]
        public string DeleteDoctorFromPatientList(int MyDoctorId)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            var UserName = User.Claims.FirstOrDefault(x => x.Type.Equals("UserName", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";

                if (!Ival.IsInteger(MyDoctorId.ToString()))
                {
                    Msg += "Please Enter Valid My Doctor Id ";
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
                    SqlParameter[] param1 = new SqlParameter[]
                   {
                        new SqlParameter("@MyDoctorId",MyDoctorId),
                        new SqlParameter("@PatientId",UserId),
                        new SqlParameter("@returnval",SqlDbType.Int),
                   };
                    int Val = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_DeleteDoctorfromMyList", param1);
                    if (Val == 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Doctor deleted successfully.";
                        JSONString = JsonConvert.SerializeObject(Result);//Add user details to array
                    }
                    else if (Val == -1)
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "Record not found";
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
                Result.Msg = "Something went wrong,Please try again."; ;
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        [HttpPost]
        [Route("MyUnsharedReportReportList")]
        public string MyUnsharedReportReportList([FromBody] Unsharedreportlist pagingparametermodel)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
                if (!Ival.IsInteger(pagingparametermodel.DoctorId.ToString()))
                {
                    Msg += "Please Enter Valid Doctor Id";
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
                      new SqlParameter("@PatientId",UserId),
                      new SqlParameter("@DoctorId",pagingparametermodel.DoctorId),
                      new SqlParameter("@SearchingText",pagingparametermodel.Searching)
                    };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetUnsharedReportListwithSearching", param);
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

                        for (int j = 0; j < items.Count; j++)
                        {
                            dynamic ObjReportDetail = new JObject();
                            ObjReportDetail.Flag = "";
                            ObjReportDetail.TestCode = items[j]["sTestCode"];
                            ObjReportDetail.TimeSlot = items[j]["sTimeSlot"];
                            ObjReportDetail.TestDate = items[j]["sTestDate"];
                            ObjReportDetail.TestName = items[j]["sTestName"];
                            ObjReportDetail.ReportId = items[j]["sBookLabTestId"];
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
            }
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = "Something went wrong,Please try again";
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        //[HttpPost]
        //[Route("AddHealthParameter")]
        //public string AddHealthParameter([FromBody] HealthParameter model)
        //{
        //    var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
        //    string JSONString = string.Empty; // Create string object to return final output
        //    dynamic Result = new JObject();  //Create root JSON Object
        //    string Msg = "";
        //    try
        //    {
        //        if (!Ival.IsInteger(model.Oxygen.ToString()))
        //        {
        //            Msg += "Please Enter Valid Oxygen Value";
        //        }
        //        if (!Ival.IsDecimal(model.Temprature.ToString()))
        //        {
        //            Msg += "Please Enter Valid Temprature Value";
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
        //                {
        //                       new SqlParameter("@UserId",UserId),
        //                       new SqlParameter("@Oxygen",model.Oxygen),
        //                       new SqlParameter("@Temprature",model.Temprature),
        //                       new SqlParameter("@BloodPressure",model.BloodPressure),
        //                       new SqlParameter("@Returnval",SqlDbType.Int)
        //                };
        //            int data = DAL.ExecuteStoredProcedureRetnInt("Sp_WS_AddHealthParameters", param);
        //            if (data == 1)
        //            {
        //                Result.Status = true;  //  Status Key 
        //                Result.Msg = "Health Parameter Added successfully.";
        //                JSONString = JsonConvert.SerializeObject(Result);
        //            }
        //            else
        //            {
        //                Result.Status = false;  //  Status Key 
        //                Result.Msg = "Something went wrong,Please try again.";
        //                JSONString = JsonConvert.SerializeObject(Result);
        //            }
        //            return JSONString;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Result.Status = false;  //  Status Key
        //        Result.Msg = ex;
        //        JSONString = JsonConvert.SerializeObject(Result);
        //        return JSONString;
        //    }
        //}

        //[HttpGet]
        //[Route("GetHealthParameter")]
        //public string GetHealthParameter()
        //{
        //    var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
        //    string JSONString = string.Empty;
        //    dynamic Result = new JObject();
        //    try
        //    {
        //        DataTable dt = DAL.GetDataTable("WS_Sp_GetHealthParameter " + UserId);
        //        if (dt.Rows.Count > 0)
        //        {
        //            Result.List = new JArray() as dynamic;
        //            for (int j = 0; j < dt.Rows.Count; j++)
        //            {
        //                dynamic ObjDetail = new JObject();
        //                ObjDetail.Id = dt.Rows[j]["ID"];
        //                ObjDetail.Oxygen = dt.Rows[j]["Oxygen"];
        //                ObjDetail.Temperature = dt.Rows[j]["Temperature"];
        //                ObjDetail.Bloodpressure = dt.Rows[j]["Bloodpressure"];
        //                ObjDetail.CreatedDate = dt.Rows[j]["CreatedDate"];
        //                Result.List.Add(ObjDetail);
        //            }
        //            Result.Status = true;  //  Status Key
        //            Result.Msg = "Success";
        //            JSONString = JsonConvert.SerializeObject(Result);
        //        }
        //        else
        //        {
        //            Result.Status = false;  //  Status Key
        //            Result.Msg = "No Records found.";
        //            JSONString = JsonConvert.SerializeObject(Result);
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
        [Route("AddOxygen")]
        public string AddOxygen([FromBody] Addoxygen model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (!Ival.IsInteger(model.Oxygen.ToString()))
                {
                    Msg += "Please Enter Valid Oxygen Value";
                }
                if (!Ival.IsInteger(model.PulseRate.ToString()))
                {
                    Msg += "Please Enter Valid Pulse Rate ";
                }
                if (!Ival.IsValidDate(model.Date))
                {
                    Msg += "Please Enter Valid Date";
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
                               new SqlParameter("@Oxygen",model.Oxygen),
                               new SqlParameter("@PulseRate",model.PulseRate),
                               new SqlParameter("@Notes",model.Notes),
                               new SqlParameter("@Result",model.Result),
                               new SqlParameter("@Date",model.Date),
                               new SqlParameter("@Returnval",SqlDbType.Int)
                        };
                    int data = DAL.ExecuteStoredProcedureRetnInt("Sp_WS_AddHealthParametersOxygenUpdated", param);
                    if (data == 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Health Parameter Oxygen Added successfully.";
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

        [HttpGet]
        [Route("GetHealthParameterOxygen")]
        public string GetHealthParameterOxygen()
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();
            try
            {
                DataTable dt = DAL.GetDataTable("WS_Sp_GetHealthParameterOxygen " + UserId);
                if (dt.Rows.Count > 0)
                {
                    Result.List = new JArray() as dynamic;
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjDetail = new JObject();
                        ObjDetail.Id = dt.Rows[j]["ID"]; 
                        ObjDetail.Name = dt.Rows[j]["Oxygen"];
                        ObjDetail.PulseRate = dt.Rows[j]["PulseRate"];
                        ObjDetail.Notes = dt.Rows[j]["Notes"];
                        ObjDetail.Result = dt.Rows[j]["Result"];
                        ObjDetail.RecordDate = dt.Rows[j]["Date"];
                        ObjDetail.CreatedDate = dt.Rows[j]["CreatedDate"];
                        Result.List.Add(ObjDetail);
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
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);

            }
            return JSONString;
        }

        [HttpPost]
        [Route("GetHealthParameterOxygenByDate")]
        public string GetHealthParameterOxygenByDate([FromBody] BloodPressureByDate model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();
            try
            {
                SqlParameter[] param = new SqlParameter[]
                    {
                    new SqlParameter("@UserId",UserId),
                    new SqlParameter("@FromDate",model.FromDate),
                     new SqlParameter("@ToDate",model.ToDate)
                    };

                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetHealthParameterOxygenbyDate ", param);
                if (dt.Rows.Count > 0)
                {
                    Result.List = new JArray() as dynamic;
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjDetail = new JObject();
                        ObjDetail.Id = dt.Rows[j]["ID"];
                        ObjDetail.Name = dt.Rows[j]["Oxygen"];
                        ObjDetail.PulseRate = dt.Rows[j]["PulseRate"];
                        ObjDetail.Notes = dt.Rows[j]["Notes"];
                        ObjDetail.Result = dt.Rows[j]["Result"];
                        ObjDetail.RecordDate = dt.Rows[j]["RecordDate"];
                        ObjDetail.CreatedDate = dt.Rows[j]["CreatedDate"];
                        Result.List.Add(ObjDetail);
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
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);

            }
            return JSONString;
        }

        [HttpPost]
        [Route("AddTemprature")]
        public string AddTemprature([FromBody] AddTemprature model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (!Ival.IsDecimal(model.Temprature.ToString()))
                {
                    Msg += "Please Enter Valid Temprature Value";
                }
                if (!Ival.IsInteger(model.PulseRate.ToString()))
                {
                    Msg += "Please Enter Valid Pulse Rate ";
                }
                if (!Ival.IsValidDate(model.Date))
                {
                    Msg += "Please Enter Valid Date";
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
                               new SqlParameter("@Temprature",model.Temprature),
                               new SqlParameter("@PulseRate",model.PulseRate),
                               new SqlParameter("@Notes",model.Notes),
                                new SqlParameter("@Result",model.Result),
                               new SqlParameter("@Date",model.Date),
                               new SqlParameter("@Returnval",SqlDbType.Int)
                        };
                    int data = DAL.ExecuteStoredProcedureRetnInt("Sp_WS_AddHealthParametersTemperatureUpdated", param);
                    if (data == 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Health Parameter Temprature Added successfully.";
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

        [HttpGet]
        [Route("GetHealthParameterTemprature")]
        public string GetHealthParameterTemprature()
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();
            try
            {
                DataTable dt = DAL.GetDataTable("WS_Sp_GetHealthParameterTemperature " + UserId);
                if (dt.Rows.Count > 0)
                {
                    Result.List = new JArray() as dynamic;
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjDetail = new JObject();
                        ObjDetail.Id = dt.Rows[j]["ID"];
                        ObjDetail.Name = dt.Rows[j]["Temperature"];
                        ObjDetail.PulseRate = dt.Rows[j]["PulseRate"];
                        ObjDetail.Notes = dt.Rows[j]["Notes"];
                        ObjDetail.Result = dt.Rows[j]["Result"];
                        ObjDetail.RecordDate = dt.Rows[j]["Date"];
                        ObjDetail.CreatedDate = dt.Rows[j]["CreatedDate"];
                        Result.List.Add(ObjDetail);
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
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);

            }
            return JSONString;
        }

        [HttpPost]
        [Route("GetHealthParameterTempratureByDate")]
        public string GetHealthParameterTempratureByDate([FromBody] BloodPressureByDate model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();
            try
            {
                SqlParameter[] param = new SqlParameter[]
                    {
                    new SqlParameter("@UserId",UserId),
                    new SqlParameter("@FromDate",model.FromDate),
                     new SqlParameter("@ToDate",model.ToDate)
                    };

                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetHealthParameterTempraturebyDate ", param);
                if (dt.Rows.Count > 0)
                {
                    Result.List = new JArray() as dynamic;
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjDetail = new JObject();
                        ObjDetail.Id = dt.Rows[j]["ID"];
                        ObjDetail.Name = dt.Rows[j]["Temperature"];
                        ObjDetail.PulseRate = dt.Rows[j]["PulseRate"];
                        ObjDetail.Notes = dt.Rows[j]["Notes"];
                        ObjDetail.Result = dt.Rows[j]["Result"];
                        ObjDetail.RecordDate = dt.Rows[j]["RecordDate"];
                        ObjDetail.CreatedDate = dt.Rows[j]["CreatedDate"];
                        Result.List.Add(ObjDetail);
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
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);

            }
            return JSONString;
        }

        /// <summary>
        /// Add BloodPressure Exmp.{120/80}
        /// </summary>
        /// <param name="Bloodpressure"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddBloodpressure")]
        public string AddBloodpressure([FromBody] AddBloodPressure model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (Ival.IsTextBoxEmpty(model.Bloodpressure))
                {
                    Msg += "Please Enter Valid Blood Pressure";
                }
                if (Ival.IsTextBoxEmpty(model.Result))
                {
                    Msg += "Please Enter Valid Result";
                }
                if (!Ival.IsValidDate(model.Date))
                {
                    Msg += "Please Enter Valid Date";
                }
                if (!Ival.IsInteger(model.PulseRate.ToString()))
                {
                    Msg += "Please Enter Valid Pulse Rate ";
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
                               new SqlParameter("@BloodPressure",model.Bloodpressure),
                               new SqlParameter("@PulseRate",model.PulseRate),
                               new SqlParameter("@Notes",model.Notes),
                               new SqlParameter("@Result",model.Result),
                               new SqlParameter("@Date",model.Date),
                               new SqlParameter("@Returnval",SqlDbType.Int)
                        };
                    int data = DAL.ExecuteStoredProcedureRetnInt("Sp_WS_AddHealthParametersBloodpressure", param);
                    if (data == 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Health Parameter Blood Pressure Added successfully.";
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

        [HttpGet]
        [Route("GetHealthParameterBloodpressure")]
        public string GetHealthParameterBloodpressure()
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();
            try
            {
                DataTable dt = DAL.GetDataTable("WS_Sp_GetHealthParameterBloodpressure " + UserId);
                if (dt.Rows.Count > 0)
                {
                    Result.List = new JArray() as dynamic;
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjDetail = new JObject();
                        ObjDetail.Id = dt.Rows[j]["ID"];
                        ObjDetail.Name = dt.Rows[j]["Bloodpressure"];
                        ObjDetail.PulseRate = dt.Rows[j]["PulseRate"];
                        ObjDetail.Notes = dt.Rows[j]["Notes"];
                        ObjDetail.Result = dt.Rows[j]["Result"];
                        ObjDetail.RecordDate = dt.Rows[j]["Date"];
                        ObjDetail.CreatedDate = dt.Rows[j]["CreatedDate"];
                        Result.List.Add(ObjDetail);
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
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);

            }
            return JSONString;
        }


        /// <summary>
        /// Date Format should be {dd/MM/yyyy (15/04/2021)}
        /// </summary>
        /// <param name="Date">Date Format should be {dd/MM/yyyy (15/04/2021)}</param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetHealthParameterBloodpressureByDate")]
        public string GetHealthParameterBloodpressureByDate([FromBody] BloodPressureByDate model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();
            try
            {
                SqlParameter[] param = new SqlParameter[]
                    {
                    new SqlParameter("@UserId",UserId),
                    new SqlParameter("@FromDate",model.FromDate),
                     new SqlParameter("@ToDate",model.ToDate)
                    };
                    
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetHealthParameterBloodpressurebyDate ", param);
                if (dt.Rows.Count > 0)
                {
                    Result.List = new JArray() as dynamic;
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjDetail = new JObject();
                        ObjDetail.Id = dt.Rows[j]["ID"];
                        ObjDetail.Name = dt.Rows[j]["Bloodpressure"];
                        ObjDetail.PulseRate = dt.Rows[j]["PulseRate"];
                        ObjDetail.Notes = dt.Rows[j]["Notes"];
                        ObjDetail.Result = dt.Rows[j]["Result"];
                        ObjDetail.RecordDate = dt.Rows[j]["RecordDate"];
                        ObjDetail.CreatedDate = dt.Rows[j]["CreatedDate"];
                        Result.List.Add(ObjDetail);
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
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);

            }
            return JSONString;
        }

        [HttpPost]
        [Route("AddOldSharedReport")]
        public string AddOldSharedReport([FromBody] SharedReport model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            var UserName = User.Claims.FirstOrDefault(x => x.Type.Equals("UserName", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            string _ReportId = "";
            try
            {
                string Msg = "";
                if (Ival.IsTextBoxEmpty(model.ReportId))
                {
                    Msg += "Please Enter Valid ReportId";
                }
                else
                {
                    string[] splitReport = model.ReportId.ToString().Split(',');
                    foreach (string Report in splitReport)
                    {
                        if (!Ival.IsInteger(Report))
                        {
                            Msg += "Please Enter Valid Report Id";
                        }
                        else
                        {
                            _ReportId += Report + ",";
                        }
                    }
                }
                if (!Ival.IsInteger(model.DoctorId.ToString()))
                {
                    Msg += "Please Enter Valid Doctor Id ";
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
                    _ReportId = _ReportId.TrimEnd(',');
                    string[] splitReports = _ReportId.Split(',');
                    foreach (string Reports in splitReports)
                    {
                        SqlParameter[] param = new SqlParameter[]
                            {
                                new SqlParameter("@ReportId",Reports),
                                new SqlParameter("@DoctorsId",model.DoctorId),
                                new SqlParameter("@PatientsId",UserId),
                                new SqlParameter("@returnval",SqlDbType.Int)
                            };
                        data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_AddOldSharedReport", param);
                    }
                    if (data == 1)
                    {
                        DataTable dt = DAL.GetDataTable("WS_Sp_GetUserdevicetoken " + model.DoctorId);
                        if (dt.Rows.Count > 0)
                        {
                            string _title = "Shared Report";
                            string _Devicetoken = dt.Rows[0]["sDeviceToken"].ToString();
                            string _Msg = "Your patient " + UserName + " has shared a report with you. Please take a look.";
                            dynamic _Result = new JObject();
                            _Result.PatientId = UserId;
                            string _payload = JsonConvert.SerializeObject(_Result);
                            string _type = "Shared Report";
                            fcm.SendNotification(_title, _Msg, _Devicetoken, _type, UserId);
                            Notification.AppNotification(model.DoctorId.ToString(), "", _title, _Msg, _type, _payload, UserId);
                        }

                        Result.Status = "Success";  //  Status Key 
                        Result.Msg = "Shared reports added successfully.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else if (data == -2)
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "Report already shared with doctor.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.Status = "Server Error";  //  Status Key 
                        Result.Msg = "Something went wrong,Please try again.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    return JSONString;
                }
            }
            catch (Exception ex)
            {
                Result.Status = "Failed";  //  Status Key
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);
                return JSONString;
            }
        }

        [HttpPost]
        [Route("UnshreadMyOldReport")]
        public string UnshreadMyOldReport([FromBody] UnsharedReport model)
        {
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
                string _ReportId = "";
                if (Ival.IsTextBoxEmpty(model.ReportId))
                {
                    Msg += "Please Enter Valid ReportId";
                }
                else
                {
                    string[] splitReport = model.ReportId.ToString().Split(',');
                    foreach (string Report in splitReport)
                    {
                        if (!Ival.IsInteger(Report))
                        {
                            Msg += "Please Enter Valid Report Id";
                        }
                        else
                        {
                            _ReportId += Report + ",";
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
                    _ReportId = _ReportId.TrimEnd(',');
                    string[] splitReports = _ReportId.Split(',');
                    foreach (string Reports in splitReports)
                    {
                        SqlParameter[] param = new SqlParameter[]
                        {
                            new SqlParameter("@ReportId",Reports),
                            new SqlParameter("@returnval",SqlDbType.Int)
                        };
                        data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_UnshredOldReport", param);
                    }
                    if (data == 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Report unshared successfully.";
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
        [Route("PatientOldSharedReportList")]
        public string PatientOldSharedReportList([FromBody] PatientSharedReport model)
        {
              var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            string Msg = "";
            try
            {
                if (!Ival.IsInteger(model.DoctorId))
                {
                    Msg += "Please Enter Valid Doctor Id";
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
                        new SqlParameter("@PatientId",UserId),
                        new SqlParameter("@DoctorId",model.DoctorId)
                     };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_PatientOldSharedReortList", param);
                    Result.ReportList = new JArray() as dynamic;   // Create Array for Report List
                    if (dt.Rows.Count > 0)
                    {
                        for (int j = 0; j < dt.Rows.Count; j++)
                        {
                            dynamic ObjReportDetail = new JObject();
                            ObjReportDetail.Flag = "OldReport";
                            ObjReportDetail.PatientID = dt.Rows[j]["UserId"];
                            ObjReportDetail.SharedReportId = dt.Rows[j]["SharedId"];
                            ObjReportDetail.TestName = dt.Rows[j]["TestName"];
                            ObjReportDetail.TestDate = dt.Rows[j]["TestDate"];
                            ObjReportDetail.RefDoctorName = dt.Rows[j]["RefDoctorName"];
                            ObjReportDetail.Notes = dt.Rows[j]["Notes"];
                            ObjReportDetail.LabName = dt.Rows[j]["LabName"];
                            ObjReportDetail.CreatedDate = dt.Rows[j]["CreatedDate"];
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
        [Route("MyOldUnsharedReportReportList")]
        public string MyOldUnsharedReportReportList([FromBody] Unsharedreportlist pagingparametermodel)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
                if (!Ival.IsInteger(pagingparametermodel.DoctorId.ToString()))
                {
                    Msg += "Please Enter Valid Doctor Id";
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
                      new SqlParameter("@PatientId",UserId),
                      new SqlParameter("@DoctorId",pagingparametermodel.DoctorId),
                      new SqlParameter("@SearchingText",pagingparametermodel.Searching)
                    };
                    DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetOldUnsharedReportListwithSearching", param);
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

                        for (int j = 0; j < items.Count; j++)
                        {
                            dynamic ObjReportDetail = new JObject();
                            ObjReportDetail.Flag = "OldReport";
                            ObjReportDetail.PatientID = dt.Rows[j]["UserId"];
                            ObjReportDetail.ReportId = dt.Rows[j]["ReportId"];
                            ObjReportDetail.TestName = dt.Rows[j]["TestName"];
                            ObjReportDetail.TestDate = dt.Rows[j]["TestDate"];
                            ObjReportDetail.RefDoctorName = dt.Rows[j]["RefDoctorName"];
                            ObjReportDetail.Notes = dt.Rows[j]["Notes"];
                            ObjReportDetail.LabName = dt.Rows[j]["LabName"];
                            ObjReportDetail.CreatedDate = dt.Rows[j]["CreatedDate"];
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
        [Route("MyFamilyMemberOldReportList")]
        public string MyFamilyMemberOldReportList([FromBody] FamilyMemberreportlist model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                SqlParameter[] param = new SqlParameter[]
                    {
                      new SqlParameter("@Userid",UserId),
                      new SqlParameter("@PatientId",model.Familymemberid),
                      new SqlParameter("@SearchingText",model.Searching)
                    };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetMyFamilymemberOldReportListwithSearching", param);
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
                        Result.ReportList.Add(ObjLabDetail); //Add Test details to array
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
        [Route("MyReportListwithCompair")]
        public string MyReportListwithCompair([FromBody] PagingParameterModel pagingparametermodel)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                SqlParameter[] param = new SqlParameter[]
                    {
                      new SqlParameter("@PatientId",UserId),
                      new SqlParameter("@SearchingText",pagingparametermodel.Searching)
                    };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetReportListwithSearchingCompair", param);
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

                    for (int j = 0; j < items.Count; j++)
                    {
                        dynamic ObjReportDetail = new JObject();
                        //ObjReportDetail.TestCode = items[j]["sTestCode"];
                        ObjReportDetail.Flag = "";
                        ObjReportDetail.sTestId = items[j]["sTestId"];

                        //ObjReportDetail.TimeSlot = items[j]["sTimeSlot"];
                        //ObjReportDetail.TestDate = items[j]["sTestDate"];
                        ObjReportDetail.TestName = items[j]["sTestName"];
                        //ObjReportDetail.ReportId = items[j]["sBookLabTestId"];
                        //ObjReportDetail.ProfileName = items[j]["sProfileName"];
                        //ObjReportDetail.LabName = items[j]["sLabName"];
                        //ObjReportDetail.LabLogo = items[j]["sLabLogo"];
                        //ObjReportDetail.ReportDate = items[j]["ReportDate"];
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
        [Route("AddPatientToDoctor")]
        public string AddPatientToDoctor([FromBody] AddPatientToDoctor model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object

            var _Mobile = CryptoHelper.Encrypt(model.Mobile.ToString());
            try
            {
                       

                string Msg = "";
                if (!Ival.IsInteger(model.Mobile.ToString()))
                {
                    Msg += "Please Enter Valid Mobile Number";
                }

                if (Ival.IsInteger(model.Mobile))
                {
                    if (!Ival.MobileValidation(model.Mobile))
                    {
                        Msg += "Please Enter Valid Mobile Number";
                    }
                }


                if ((model.FullName == "string"))
                {
                    Msg += "Please Enter Full Name";
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
                            new SqlParameter("@FullName",model.FullName),
                            new SqlParameter("@Mobile",_Mobile),
                             new SqlParameter("@User_ID",UserId),
                            new SqlParameter("@Speciality",model.Speciality),
                             new SqlParameter("@Education",model.Education),
                            new SqlParameter("@Clinic_name",model.Clinic_name),
                             new SqlParameter("@RequestDate",""),
                            new SqlParameter("@RequestStatus",""),
                            new SqlParameter("@returnval",SqlDbType.Int)
                    };
                    data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp__AddPatientToDoctor", param);

                    //wait user www.howzu.co.in/quickregistration.aspx   url in here 

                    //    string url = "http://localhost:1034/Regisration.aspx?tempRegID=2";

                    string url = "www.howzu.co.in/Registration.aspx?tempRegID=" + data + "";

                 //   string url = "http://localhost:1034/Regisration.aspx?tempRegID=" + data + "";

                    string tinyUrl;

                    string api = "http://tinyurl.com/api-create.php?url=";



                    var request = WebRequest.Create(api + url);
                    var res = request.GetResponse();
                    using (var reader = new StreamReader(res.GetResponseStream()))
                    {
                        tinyUrl = reader.ReadToEnd();
                    }

                    string MSG = "Hi Mr " + model.FullName + ", Please find the url " + tinyUrl + " to get yourself registered as a referral doctor. Please sign up at your earliest. Howzu";



                    //  string MSG = "Hi  " + model.FullName + ", Please find the url " + tinyUrl + " to get yourself registered as a referral doctor Please sign up at your earliest.";


                    if (data > 1)
                    {

                        SendOTP _otp = new SendOTP();


                        //  string StatusCode = _otp.SendMsgToDoctor(model.Mobile.ToString(), tinyUrl,model.FullName);
                        int StatusCode = _otp.InvationSMSToDoctor(model.Mobile.ToString(), MSG);

                        if (StatusCode == 200)
                        {
                            Result.OTPSend = true;
                            Result.Msg = "Message Successfully Sent.";
                        }

                    }
                    if (data >= 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Doctor's Added successfully.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else if (data == -2)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Doctor already exists in your list.";
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
        [Route("AddReminderDetetails")] 
        public string AddReminderDetetails([FromBody] AddReminder model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
                                             //      var _mobile = (model.Mobile != "") ? CryptoHelper.Encrypt(model.Mobile) : "";


            try
            {

                string Msg = "";





                //if (!Ival.IsInteger(model.MedName.ToString()))
                //{
                //    Msg += "Please Enter Tablet Name";
                //}

                //if (!Ival.IsInteger(model.Medfor.ToString()))
                //{
                //    Msg += "Please Enter You taking it for";
                //}

                //if (!Ival.IsInteger(model.MedSdate.ToString()))
                //{
                //    Msg += "Please Select Start Date ";
                //}
                //if (!Ival.IsInteger(model.MedDoseDuration.ToString()))
                //{
                //    Msg += "Please Enter Duration";
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
                    int data = 0;



                    SqlParameter[] param = new SqlParameter[]
                  {
                            new SqlParameter("@UserId",UserId),
                            new SqlParameter("@MedName",model.MedName),
                             new SqlParameter("@Medfor",model.Medfor),
                         new SqlParameter("@MedSdate",model.MedSdate),
                              new SqlParameter("@MedDoseDuration",model.MedDoseDuration),
                            new SqlParameter("@DoseInterval",model.DoseInterval),
                            new SqlParameter("@ForWhome",model.ForWhome),
                              new SqlParameter("@ColorCode",model.ColorCode),
                            new SqlParameter("@returnval",SqlDbType.Int)
                  };
                    data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_AddReminder", param);


                    string[] splitDoseTime = model.DoseTime.ToString().Split(',');


                    dynamic ObjDetail = new JObject();
                    DateTime startDAte = Convert.ToDateTime(model.MedSdate);
                    //ObjDetail.MedSdate = model.MedSdate;

                    DateTime date2;
                    for (int i = 0; i < model.MedDoseDuration; i++)
                    {


                        date2 = startDAte.AddDays(i);

                        //ObjDetail.MedSdate = model.MedSdate;

                        for (int j = 0; j < model.DoseInterval; j++)
                        {


                            SqlParameter[] param3 = new SqlParameter[]
                        {
                             new SqlParameter("@RemidermId",data),
                            new SqlParameter("@medicineName",model.MedName),
                             new SqlParameter("@DoseDate",date2.ToString()),

                            new SqlParameter("@DoseTime",splitDoseTime[j]),
                             //  new SqlParameter("@DoseTime","mmmm"),

                             new SqlParameter("@MedicationStatus",""),
                              new SqlParameter("@NotificationStatus","N"),

                             new SqlParameter("@returnval",SqlDbType.Int),

                        };

                            int DetailData = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_AddReminderDetails", param3);


                        }
                    }







                    if (data >= 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Reminder Added successfully.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else if (data == -2)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Add Reminder already exists in your list.";
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
        [Route("DeleteReminderFromPatient")]
        public string DeleteReminderFromPatient([FromBody] DeleteReminder model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            //  var UserName = User.Claims.FirstOrDefault(x => x.Type.Equals("UserName", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string UpdatedStaus = "";
                string Msg = "";
                if (model.MedicationStatus == "U")
                {
                    model.MedStatus.ToString();

                }
                else
                {
                    model.MedStatus = "";
                }




                if (!Ival.IsInteger(model.RemidermId.ToString()))
                {
                    Msg += "Please Enter Valid Reminder Id ";
                }
                if (!(model.MedicationStatus == "U" || model.MedicationStatus == "D" || model.MedicationStatus == "DM"))
                {
                    Msg += "Please Enter Valid  Medication Status ";
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

                    SqlParameter[] param1 = new SqlParameter[]
                   {
                        new SqlParameter("@RemidermId",model.RemidermId),
                        new SqlParameter("@UserId",UserId),
                        new SqlParameter("@MedicationStatus",model.MedicationStatus),
                         new SqlParameter("@MedStatus",model.MedStatus),
                         new SqlParameter("@returnval",SqlDbType.Int),
                   };
                    int Val = DAL.ExecuteStoredProcedureRetnInt("Sp_DeleteReminder", param1);
                    if (Val == 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Reminder deleted successfully.";
                        JSONString = JsonConvert.SerializeObject(Result);//Add user details to array
                    }
                    else if (Val == -1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Reminder Updated successfully";
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
                Result.Msg = "Something went wrong,Please try again."; ;
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }


        /// <summary>
        /// Get Medicine Reminder Details
        /// </summary>
        /// <param name="id">Mandatory</param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetMedicineReminderDetails")]
        public string GetMedicineReminderDetails([FromBody] GetMedicineReminderDetails modeld)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                //  DataTable dt = DAL.GetDataTable("WS_Sp__GetMyDocList " + UserId);

                SqlParameter[] param = new SqlParameter[]
              {
                    new SqlParameter("@UserID",UserId),
                    new SqlParameter("@DoseDate",modeld.DoseDate)

              };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetMEdicationDetailsForGraph", param);
                if (dt.Rows.Count > 0)
                {
                    // Get's No of Rows Count   


                    // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  


                    Result.medicineDetails = new JArray() as dynamic;   // Create Array for Doctor Details

                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjMedicatioDetail = new JObject();
                        ObjMedicatioDetail.RemindermId = dt.Rows[j]["ReminderdId"];

                        ObjMedicatioDetail.Medicinename = dt.Rows[j]["Medname"];
                        ObjMedicatioDetail.MedicineFor = dt.Rows[j]["Medfor"];
                        ObjMedicatioDetail.medStartDate = dt.Rows[j]["MedSdate"];
                        ObjMedicatioDetail.doseDate = dt.Rows[j]["DoseDate"];
                        ObjMedicatioDetail.doseTime = dt.Rows[j]["DoseTime"];
                        ObjMedicatioDetail.MedicationStatus = dt.Rows[j]["MedicationStatus"];
                        ObjMedicatioDetail.notificationStatus = dt.Rows[j]["NotificationStatus"];

                        ObjMedicatioDetail.doseDuration = dt.Rows[j]["MedDoseDuration"];
                        ObjMedicatioDetail.DoseInterval = dt.Rows[j]["DoseInterval"];
                        ObjMedicatioDetail.ForWhome = dt.Rows[j]["ForWhome"];
                        ObjMedicatioDetail.ColorCode = dt.Rows[j]["ColorCode"];
                        ObjMedicatioDetail.medicineMasterId = dt.Rows[j]["RemidermId"];

                        Result.medicineDetails.Add(ObjMedicatioDetail); //Add Doctor details to array
                    }
                    Result.Status = true;  //  Status Key
                    Result.Msg = "Success";

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
        [Route("AddHydration")]
        public string AddHydration([FromBody] UserHydration model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
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
                    Msg += "Please Enter Valid Bedtime";
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
                //    double intakeGoal = 0;

                //    intakeGoal = model.Weight_Kg * 0.033 * 1000;
                    SqlParameter[] param = new SqlParameter[]
                    {
                        //new SqlParameter("@UserId",UserId),
                        //new SqlParameter("@Weight",model.Weight_Kg),
                        //new SqlParameter("@Wekup_time",model.Wakeuptime),
                        //new SqlParameter("@Bed_time",model.Bedtime),

                        //new SqlParameter("@Returnval",SqlDbType.Int)




                         new SqlParameter("@UserId",UserId),
                        new SqlParameter("@Weight",model.Weight_Kg),
                        new SqlParameter("@Wekup_time",model.Wakeuptime),
                        new SqlParameter("@Bed_time",model.Bedtime),
                        new SqlParameter("@Intekgoal",model.Intakegoal),
                         new SqlParameter("@Gender",model.Gender),
                        new SqlParameter("@ActionStatus",model.ActionStatus),
                        new SqlParameter("@CreatedAt",""),

                        new SqlParameter("@ModifiedAt",""),




                        new SqlParameter("@Returnval",SqlDbType.Int)
                     };
                    int data = DAL.ExecuteStoredProcedureRetnInt("Sp_WS_AddHydrationn", param);
                    if (data >= 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.MedicationId = data;  //  Status Key 
                        Result.Msg = "Hydration Details Added successfully.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Hydration Details Updated successfully.";
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
        [Route("AddWaterInTake")]
        public string AddWaterInTake([FromBody] WaterInTake model)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";
                if (Ival.IsTextBoxEmpty(model.Water_cosumtion.ToString()))
                {
                    Msg += "Please Enter Valid Water cosumtion";
                }
                if (Ival.IsTextBoxEmpty(model.date.ToString()))
                {
                    Msg += "Please Enter Valid date";
                }
                if (Ival.IsTextBoxEmpty(model.time.ToString()))
                {
                    Msg += "Please Enter Valid time";
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
                        new SqlParameter("@Water_cosumtion",model.Water_cosumtion),
                        new SqlParameter("@time",model.time),
                        new SqlParameter("@date",model.date),
                         //new SqlParameter("@Intake_Goal",model.Intake_Goal),
                         // new SqlParameter("@ActionStatus",""),
                        new SqlParameter("@Returnval",SqlDbType.Int)
                     };
                    int data = DAL.ExecuteStoredProcedureRetnInt("Sp_WS_AddWatrConsumtion", param);
                    if (data >= 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.MedicationId = data;  //  Status Key 
                        Result.Msg = "Water Intek Details Added successfully.";
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
        [Route("AlluserWaterconsumtiondata")]
        public string AlluserWaterconsumtiondata([FromBody] WaterConsumption WCmodel)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();
            try
            {

                SqlParameter[] param = new SqlParameter[]
            {
                    new SqlParameter("@UserId",UserId),
                    new SqlParameter("@StartDate",WCmodel.StartDate),
                       new SqlParameter("@EndDate",WCmodel.EndDate)

            };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetWaterCosumtion " ,param);
                if (dt.Rows.Count > 0)
                {
                    Result.List = new JArray() as dynamic;
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjDetail = new JObject();
                        ObjDetail.WaterConsuId = dt.Rows[j]["WaterConsuId"];
                        ObjDetail.Water_cosumtion = dt.Rows[j]["Water_cosumtion"];
                        ObjDetail.time = dt.Rows[j]["time"];
                        ObjDetail.date = dt.Rows[j]["date"];

                        Result.List.Add(ObjDetail);
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
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);

            }
            return JSONString;
        }

        [HttpPost]
        [Route("userhydrationhistoryforGraph")]
        public string userhydrationhistoryforGraph([FromBody] WaterConsumption WCmodel)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();
            try
            {
                SqlParameter[] param = new SqlParameter[]
            {
                    new SqlParameter("@UserId",UserId),
                    new SqlParameter("@StartDate",WCmodel.StartDate),
                       new SqlParameter("@EndDate",WCmodel.EndDate)

            };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_Get_user_HydrationHistory ", param);
                if (dt.Rows.Count > 0)
                {
                    Result.List = new JArray() as dynamic;
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjDetail = new JObject();

                        ObjDetail.time = dt.Rows[j]["total_ml"];
                        ObjDetail.date = dt.Rows[j]["date"];

                        Result.List.Add(ObjDetail);
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
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);

            }
            return JSONString;
        }


        [HttpGet]
        [Route("GetUserHydrationDetails")]
        public string GetUserHydrationDetails()
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();
            try
            {
                SqlParameter[] param = new SqlParameter[]
            {
                    new SqlParameter("@UserId",UserId),
                    //new SqlParameter("@StartDate",hydrationDtl.StartDate),
                    //   new SqlParameter("@EndDate",hydrationDtl.EndDate)

            };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetUserHydrationDetails ", param);
                if (dt.Rows.Count > 0)
                {
                    Result.List = new JArray() as dynamic;
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjDetail = new JObject();
                        ObjDetail.Weight = dt.Rows[j]["Weight"];
                        ObjDetail.Wekup_time = dt.Rows[j]["Wekup_time"];
                        ObjDetail.Bed_time = dt.Rows[j]["Bed_time"];
                        ObjDetail.Intekgoal = dt.Rows[j]["Intekgoal"];
                        ObjDetail.sGender = dt.Rows[j]["Gender"];

                        Result.List.Add(ObjDetail);
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
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);

            }
            return JSONString;
        }

        [HttpPost]
        [Route("GetMedicationReminderDetailsbyUderId")]

        public string GetMedicationReminderDetailsbyUderId()
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                //  DataTable dt = DAL.GetDataTable("WS_Sp__GetMyDocList " + UserId);

                SqlParameter[] param = new SqlParameter[]
              {
                    new SqlParameter("@UserID",UserId)
                 //   new SqlParameter("@DoseDate",modeld.DoseDate)

              };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetMEdicationDetailsByUserId", param);
                if (dt.Rows.Count > 0)
                {
                    // Get's No of Rows Count   


                    // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  


                    Result.medicineDetails = new JArray() as dynamic;   // Create Array for Doctor Details

                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjMedicatioDetail = new JObject();
                        ObjMedicatioDetail.RemindermId = dt.Rows[j]["ReminderdId"];

                        ObjMedicatioDetail.Medicinename = dt.Rows[j]["Medname"];
                        ObjMedicatioDetail.MedicineFor = dt.Rows[j]["Medfor"];
                        ObjMedicatioDetail.medStartDate = dt.Rows[j]["MedSdate"];
                        ObjMedicatioDetail.doseDate = dt.Rows[j]["DoseDate"];
                        ObjMedicatioDetail.doseTime = dt.Rows[j]["DoseTime"];
                        ObjMedicatioDetail.MedicationStatus = dt.Rows[j]["MedicationStatus"];
                        ObjMedicatioDetail.notificationStatus = dt.Rows[j]["NotificationStatus"];

                        ObjMedicatioDetail.doseDuration = dt.Rows[j]["MedDoseDuration"];
                        ObjMedicatioDetail.DoseInterval = dt.Rows[j]["DoseInterval"];



                        Result.medicineDetails.Add(ObjMedicatioDetail); //Add Doctor details to array
                    }
                    Result.Status = true;  //  Status Key
                    Result.Msg = "Success";

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
        [Route("DeleteWaterConsumptionByConsumptionId")]
        public string DeleteWaterConsumptionByConsumptionId(int waterConsumptionId)
        {
            // waterConsumptionId = User.Claims.FirstOrDefault(x => x.Type.Equals("WaterConsuId", StringComparison.InvariantCultureIgnoreCase)).Value;
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                string Msg = "";

                if (!Ival.IsInteger(waterConsumptionId.ToString()))
                {
                    Msg += "Please Enter Valid Consumption Id";
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
                    //  DataTable dt = DAL.GetDataTable("Sp_FamilyMemberDetails " + waterConsumptionId);
                    SqlParameter[] param1 = new SqlParameter[]
                   {
                        new SqlParameter("@WaterConsuId",waterConsumptionId),
                      //  new SqlParameter("@UserId",UserId),
                        new SqlParameter("@returnval",SqlDbType.Int),
                   };
                    int Val = DAL.ExecuteStoredProcedureRetnInt("Sp_DeleteWaterConsumptionData", param1);
                    if (Val == 1)
                    {

                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Data deleted successfully.";
                        JSONString = JsonConvert.SerializeObject(Result);//Add user details to array
                    }
                    else if (Val == -1)
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "Please enter valid Consumption Id";
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
                Result.Msg = "Something went wrong,Please try again."; ;
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }



        [HttpPost]
        [Route("GetUserProfileFDMList")]
        public string GetUserProfileFDMList([FromBody] UserProfileFDM FDMModel)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
                                             //      var _mobile = (model.Mobile != "") ? CryptoHelper.Encrypt(model.Mobile) : "";


            try
            {
                //  DataTable dt = DAL.GetDataTable("WS_Sp__GetMyDocList " + UserId);

                SqlParameter[] param = new SqlParameter[]
              {
                    new SqlParameter("@type",FDMModel.type),
                  new SqlParameter("@userId",UserId)

              };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetUserProfileFDMDetails", param);
                if (dt.Rows.Count > 0)
                {
                    // Get's No of Rows Count   


                    // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  


                    Result.ObjProfileDetail = new JArray() as dynamic;   // Create Array for Profile Details

                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjProfileDetail = new JObject();
                        ObjProfileDetail.name = dt.Rows[j]["name"];

                        ObjProfileDetail.type = dt.Rows[j]["type"];
                        ObjProfileDetail.cratedDate = dt.Rows[j]["createdDate"];
                        ObjProfileDetail.UserDetailsID = dt.Rows[j]["userProfileDtlsId"];


                        Result.ObjProfileDetail.Add(ObjProfileDetail);
                    }
                    Result.Status = true;  //  Status Key
                    Result.Msg = "Success";

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
        [Route("AddUserProfileDetails")]
        public string AddUserProfileDetails([FromBody] UserProfileDetails userProfileModel)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
                                             //      var _mobile = (model.Mobile != "") ? CryptoHelper.Encrypt(model.Mobile) : "";


            try
            {

                string Msg = "";

                //if (!Ival.IsCharOnly(userProfileModel.Name.ToString()))
                //{
                //    Msg += "Please Enter Valid Name";
                //}

                if (!Ival.IsCharOnly(userProfileModel.type.ToString()))
                {
                    Msg += "Please Enter Type";
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
                       new SqlParameter("@userprofileDtlsId",userProfileModel.userprofileDtlsId),
                       new SqlParameter("@UserId",UserId),
                       new SqlParameter("@Name",userProfileModel.Name),
                       new SqlParameter("@type",userProfileModel.type),
                       new SqlParameter("@createdDate",DateTime.Now.ToString("MM/dd/yyyy")),
                       new SqlParameter("@relation",userProfileModel.relation),
                       new SqlParameter("@familyMemberName",userProfileModel.familyMemberName),
                       new SqlParameter("@diseaseName",""),
                       new SqlParameter("@col8",userProfileModel.col8),
                       new SqlParameter("@col9",userProfileModel.col9),
                       new SqlParameter("@col10",userProfileModel.col10),
                      new SqlParameter("@actionStatus",userProfileModel.actionStatus),
                       new SqlParameter("@returnval",SqlDbType.Int)

                  };
                    data = DAL.ExecuteStoredProcedureRetnInt("WS_Sp_AddUserProfileDetails", param);



                    if (data >= 1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Details Added successfully.";
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    //else if (data == -2)
                    //{
                    //    Result.Status = true;  //  Status Key 
                    //    Result.Msg = "Name already exists in your list.";
                    //    JSONString = JsonConvert.SerializeObject(Result);
                    //}
                    else if (data == -1)
                    {
                        Result.Status = true;  //  Status Key 
                        Result.Msg = "Deleted successfully.";
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

            return JSONString;

        }



        [HttpPost]
        [Route("GetUserProfileDetails")]
        public string GetUserProfileDetails([FromBody] GetUserProfileDetails detailsModel)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                //  DataTable dt = DAL.GetDataTable("WS_Sp__GetMyDocList " + UserId);

                SqlParameter[] param = new SqlParameter[]
              {
                    new SqlParameter("@userId",UserId),
                  new SqlParameter("@type",detailsModel.type)

              };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetUserProfileDetails", param);
                if (dt.Rows.Count > 0)
                {
                    // Get's No of Rows Count   


                    // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  


                    Result.ObjProfileDetail = new JArray() as dynamic;   // Create Array for Profile Details

                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjProfileDetail = new JObject();
                        ObjProfileDetail.name = dt.Rows[j]["name"];

                        ObjProfileDetail.type = dt.Rows[j]["type"];
                        ObjProfileDetail.relation = dt.Rows[j]["relation"];
                        ObjProfileDetail.familyMemberNm = dt.Rows[j]["familyMemberName"];
                        ObjProfileDetail.col8 = dt.Rows[j]["col8"];
                        ObjProfileDetail.col9 = dt.Rows[j]["col9"];
                        ObjProfileDetail.col10 = dt.Rows[j]["col10"];
                        ObjProfileDetail.createdDate = dt.Rows[j]["createdDate"];
                        ObjProfileDetail.userprofile_dtls_id = dt.Rows[j]["userprofile_dtls_id"];


                        Result.ObjProfileDetail.Add(ObjProfileDetail);
                    }
                    Result.Status = true;  //  Status Key
                    Result.Msg = "Success";

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
        [Route("MedicationList")]

        public string MedicationList([FromBody] MedicationList modeld)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                //  DataTable dt = DAL.GetDataTable("WS_Sp__GetMyDocList " + UserId);


                // DateTime from_date = new DateTime(modeld.FromDate);
                // DateTime to_date = new DateTime(modeld.EndDate);
                //string str_fromDate = Convert.ToDateTime(modeld.FromDate).ToString("MM/dd/yyyy");
                //string str_toDate = Convert.ToDateTime(modeld.EndDate).ToString("MM/dd/yyyy");


                SqlParameter[] param = new SqlParameter[]
              {
                  // new SqlParameter("@MedicineName",modeld.MedicineName),
                    //new SqlParameter("@FromDate",modeld.FromDate),
                    //new SqlParameter("@ToDate",modeld.EndDate),
                     new SqlParameter("@ForWhome",modeld.ForWhome),
                     new SqlParameter("@SearchingText",modeld.Searching) ,
                        new SqlParameter("@userID",UserId)
              };


                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetMedicationList", param);
                if (dt.Rows.Count > 0)
                {
                    // Get's No of Rows Count   


                    // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  


                    Result.medicineDetails = new JArray() as dynamic;   // Create Array for Doctor Details

                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        dynamic ObjMedicatioDetail = new JObject();
                        //ObjMedicatioDetail.RemindermId = dt.Rows[j]["ReminderdId"];

                        ObjMedicatioDetail.Medicinename = dt.Rows[j]["MedName"];
                        ObjMedicatioDetail.MedSdate = dt.Rows[j]["MedSdate"];
                        ObjMedicatioDetail.MedDoseDuration = dt.Rows[j]["MedDoseDuration"];
                        ObjMedicatioDetail.DoseInterval = dt.Rows[j]["DoseInterval"];
                        ObjMedicatioDetail.RemidermId = dt.Rows[j]["RemidermId"];
                        ObjMedicatioDetail.persantange = dt.Rows[j]["per"];

                        ObjMedicatioDetail.TotalDose = dt.Rows[j]["TotalCount"];
                        ObjMedicatioDetail.TakenDose = dt.Rows[j]["TakenDose"];
                        ObjMedicatioDetail.SkipDose = dt.Rows[j]["SkipDose"];


                        Result.medicineDetails.Add(ObjMedicatioDetail); //Add Doctor details to array
                    }
                    //}


                    Result.Status = true;  //  Status Key
                    Result.Msg = "Success";

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
        [Route("MedicationDetails")]

        public string MedicationDetails([FromBody] MedicationDetails modeld)
        {
            var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty;
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                //  DataTable dt = DAL.GetDataTable("WS_Sp__GetMyDocList " + UserId);

                SqlParameter[] param = new SqlParameter[]
              {
                    new SqlParameter("@MasterId",modeld.MasterId),
                    //new SqlParameter("@DoseDate",modeld.DoseDate)

              };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp__GetMedicationDetails", param);

                if (dt.Rows.Count > 0)
                {
                    Result.ReportList = new JArray() as dynamic;  // temp hide 1804



                    //  JArray ReportList = new JArray() as dynamic;

                    dynamic ReportDetailsObject = new JObject();

                    // ReportDetailsObject.SubReportList = new JArray() as dynamic;

                    //for (int i = 0; i < dt.Rows.Count; i++)
                    //for (int i = 0; i < 2; i++)
                    int count = 0;
                    foreach (DataRow row in dt.Rows)

                    {


                        ReportDetailsObject.SubReportList = new JArray() as dynamic;

                        ReportDetailsObject.MedicineName = row["medicineName"]; //dt.Rows[i]["medicineName"];
                        ReportDetailsObject.DoseDate = row["DoseDate"]; //dt.Rows[i]["DoseDate"];
                        ReportDetailsObject.counter = count;

                        //string[] arr_Dosetime = dt.Rows[i]["DoseTime"].ToString().Split(',');
                        //string[] arr_MedicationStatus = dt.Rows[i]["MedicationStatus"].ToString().Split(',');
                        //string[] arr_m_status = dt.Rows[i]["mStatus"].ToString().Split(',');

                        string[] arr_Dosetime = row["DoseTime"].ToString().Split(',');
                        string[] arr_MedicationStatus = row["MedicationStatus"].ToString().Split(',');
                        string[] arr_m_status = row["mStatus"].ToString().Split(',');

                        for (int j = 0; j < arr_Dosetime.Length; j++)
                        {
                            dynamic SubPaymentObj = new JObject();

                            SubPaymentObj.DoseTime = arr_Dosetime[j];
                            SubPaymentObj.MedicationStatus = arr_MedicationStatus[j];
                            SubPaymentObj.mStatus = arr_m_status[j];

                            ReportDetailsObject.SubReportList.Add(SubPaymentObj);

                        }


                        // ReportList.Add(ReportDetailsObject);
                        //  ReportList.Add(ReportDetailsObject);

                        // Result.ReportList = new JArray() as dynamic;
                        Result.ReportList.Add(ReportDetailsObject);
                        count++;
                    }



                    //JArray list = new JArray() as dynamic;

                    // dynamic jsonArray = new JObject();

                    // jsonArray = ReportDetailsObject;

                    // if (jsonArray != null)
                    // {
                    //     int len = jsonArray.length();
                    //     for (int i = 0; i < len; i++)
                    //     {
                    //         list.Add(jsonArray.get(i).toString());
                    //     }
                    // }
                    // //Remove the element from arraylist
                    // list.Remove(0);
                    // //Recreate JSON Array
                    // JArray jsArray = new JArray(list);



                    string s = "tt";
                    //   ReportList.Remove(0);
                    //  Result = new JArray(ReportList);
                    //  Result.ReportList = new JArray() as dynamic;
                    // Result.ReportList.Add(ReportDetailsObject);
                    //Result.Add(ReportList);




                    Result.ReportList.Remove(1);

                    Result.Status = true;
                    Result.Msg = "Success";
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


      
    }
}
