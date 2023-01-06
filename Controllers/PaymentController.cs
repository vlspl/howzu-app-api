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
using System.Text;
using System.Web;
using System.Net;

namespace Howzu_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        DataAccessLayer DAL = new DataAccessLayer();
        InputValidation Ival = new InputValidation();

      
        [HttpPost]
        [Route("PaymentSignature")]
        public string PaymentSignature([FromBody] PaymentSignature model)
        {
            // var UserId = User.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase)).Value;
            string JSONString = string.Empty; // Create string object to return final output
            dynamic Result = new JObject();  //Create root JSON Object
            try
            {
                int data = 0;
                SqlParameter[] param = new SqlParameter[]
                {
                        new SqlParameter("@TestList",model.TestId),
                        new SqlParameter("@LabId",model.LabId),
                };
                DataTable dt = DAL.ExecuteStoredProcedureDataTable("WS_Sp_GetTestPriceByLabId", param);
                if (dt.Rows.Count > 0)
                {
                    double _trxAmt = Convert.ToDouble(model.TransactionAmount);
                    double _testSum = Convert.ToDouble(dt.Rows[0]["TestAmount"]);
                    if (_trxAmt == _testSum)
                    {
                        string strURL, strClientCode, strClientCodeEncoded;
                        byte[] b;
                        string strResponse = "";
                        string ru = "https://paynetzuat.atomtech.in/paynetzclient/ResponseParam.jsp";

                        b = Encoding.UTF8.GetBytes(model.ClientCode);
                        strClientCode = Convert.ToBase64String(b);
                        strClientCodeEncoded = HttpUtility.UrlEncode(strClientCode);

                        strURL = "https://paynetzuat.atomtech.in/paynetz/epi/fts?login=[MerchantLogin]pass=[MerchantPass]ttype=[TransactionType]prodid=[ProductID]amt=[TransactionAmount]txncurr=[TransactionCurrency]txnscamt=[TransactionServiceCharge]clientcode=[ClientCode]txnid=[TransactionID]date=[TransactionDateTime]custacc=[CustomerAccountNo]mdd=[MerchantDiscretionaryData]bankid=[BankID]ru=[ru]signature=[signature]";
                        strURL = strURL.Replace("[MerchantLogin]", model.MerchantLogin + "&");
                        strURL = strURL.Replace("[MerchantPass]", model.MerchantPass + "&");
                        strURL = strURL.Replace("[TransactionType]", model.TransactionType + "&");
                        strURL = strURL.Replace("[ProductID]", model.ProductID + "&");
                        strURL = strURL.Replace("[TransactionAmount]", model.TransactionAmount + "&");
                        strURL = strURL.Replace("[TransactionCurrency]", model.TransactionCurrency + "&");
                        strURL = strURL.Replace("[TransactionServiceCharge]", model.TransactionServiceCharge + "&");
                        strURL = strURL.Replace("[ClientCode]", strClientCodeEncoded + "&");
                        strURL = strURL.Replace("[TransactionID]", model.TransactionID + "&");
                        strURL = strURL.Replace("[TransactionDateTime]", model.TransactionDateTime + "&");
                        strURL = strURL.Replace("[CustomerAccountNo]", model.CustomerAccountNo + "&");
                        strURL = strURL.Replace("[MerchantDiscretionaryData]", model.MerchantDiscretionaryData + "&");
                        strURL = strURL.Replace("[BankID]", model.BankID + "&");
                        strURL = strURL.Replace("[ru]", ru + "&");// Remove on Production;
                        string reqHashKey = "KEY123657234";
                        string signature = "";
                        string strsignature = model.MerchantLogin + model.MerchantPass + model.TransactionType + model.ProductID +
                            model.TransactionID + model.TransactionAmount + model.TransactionCurrency;
                        byte[] bytes = Encoding.UTF8.GetBytes(reqHashKey);
                        byte[] bt = new System.Security.Cryptography.HMACSHA512(bytes).ComputeHash(Encoding.UTF8.GetBytes(strsignature));
                        signature = byteToHexString(bt).ToLower();
                        strURL = strURL.Replace("[signature]", signature);
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12; // comparable to modern browsers
                        Result.Status = true;
                        Result.Msg = "Success.";
                      //  Result.Request = strURL;
                         Result.Signature = signature;
                        JSONString = JsonConvert.SerializeObject(Result);
                    }
                    else
                    {
                        Result.Status = false;  //  Status Key 
                        Result.Msg = "Please enter valid Transaction Amount.";
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
            catch (Exception ex)
            {
                Result.Status = false;  //  Status Key
                Result.Msg = ex;
                JSONString = JsonConvert.SerializeObject(Result);
            }
            return JSONString;
        }

        public static string byteToHexString(byte[] byData)
        {
            StringBuilder sb = new StringBuilder((byData.Length * 2));
            for (int i = 0; (i < byData.Length); i++)
            {
                int v = (byData[i] & 255);
                if ((v < 16))
                {
                    sb.Append('0');
                }

                sb.Append(v.ToString("X"));

            }

            return sb.ToString();
        }

    }
}

