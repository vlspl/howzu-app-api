using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using VLS_API.Model;

namespace Howzu_API.Services
{
    public class SendOTP
    {

        public string GenrateOTP(string Mobile)
        {

            DataAccessLayer DAL = new DataAccessLayer();
            string resultJSON = "";
            try
            {

                Random generator = new Random();
                string number = generator.Next(1, 10000).ToString("D4");

                string MSG = "" + number + " is your HowzU verification code(OTP).";

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                       | SecurityProtocolType.Tls11
                       | SecurityProtocolType.Tls12
                       | SecurityProtocolType.Ssl3;
                var client = new WebClient();

                HttpWebRequest myReq = (System.Net.HttpWebRequest)WebRequest.Create("http://myvaluefirst.com/smpp/sendsms?username=Visionhtptrns&password=trujd@k34&to=" + Mobile + "&from=HOWZUX&text=" + MSG + "");
                myReq.Credentials = new System.Net.NetworkCredential("Visionhtptrns", "trujd@k34");
                HttpWebResponse myResp = (HttpWebResponse)myReq.GetResponse();
                System.IO.StreamReader respStreamReader = new System.IO.StreamReader(myResp.GetResponseStream());
                string responseString = respStreamReader.ReadToEnd();

                // string clientIPAddress = HttpContext.Current.Request.UserHostAddress;

                resultJSON = "{\"Status\":\"" + respStreamReader + "\",\"OTP\" : \"" + number + "\"}";

            }
            catch (Exception ex)
            {
                resultJSON = ex.Message.ToString();
            }

            return resultJSON;
        }
        public int InvationSMSToDoctor(string Mobile, string msg)
        {
            int StatusCode;
            try
            {

                string url = "https://visionarylifescience.com/mobileapp/WebAPI/WebAPI.asmx/invitation_sms_to_doctor?Mobile=" + Mobile + "&msg=" + msg + ""; // sample url
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(url).Result;
                     StatusCode = Convert.ToInt32(response.StatusCode);
                    return StatusCode;
                }
            }
            catch (Exception ex)
            {

                StatusCode=Convert.ToInt32(ex.Message);
            }

            return StatusCode;
        }
        public int sendOTP(string Mobile)
        {
            string url = "https://visionarylifescience.com/mobileapp/WebAPI/WebAPI.asmx/GenrateOTP?Mobile=" + Mobile + ""; // sample url
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(url).Result;
                int StatusCode = Convert.ToInt32(response.StatusCode);
                return StatusCode;
            }

        }
        public int FamilyMemberOTP(string Mobile, string OTP)
        {
            string url = "https://visionarylifescience.com/mobileapp/WebAPI/WebAPI.asmx/RegisterFamilyMemberOTP?Mobile=" + Mobile + "&OTP=" + OTP + ""; // sample url
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(url).Result;
                int StatusCode = Convert.ToInt32(response.StatusCode);
                return StatusCode;
            }

        }
        public string sendmail(string emailId, string OTP, string Name)
        {
            try
            {
                MailMessage MailMsg = new MailMessage();
                MailMsg.From = new MailAddress("visionarylifesciences7@gmail.com");
                MailMsg.To.Add(emailId);
                MailMsg.Subject = "Howzu verification code(OTP)";

                MailMsg.Body = "<div style='padding:18px; font-family:verdana; font-size:small;  background-color:#eaf7ec;text-align:center '><img src='https://visionarylifescience.com/images/Howzulogo1092020101600.png' height='57px' width: 254px; class='img-thumbnail' /><h4>Dear " + Name + "</h4><br /><h3> Greetings!!!</h3> <br /> From <span style=' font-weight:bold'>VISIONARY LIFE SCIENCES PVT.LTD   .<br />" +
                " <br />" + " </span>Your Howzu verification code is:<h3 style='font-weight:bold'>" + OTP + "</h3></span>" +
                 " <br />" +
                    " Regards Team," +
                     "<br />" +
                     "Visionary Life Science Pvt.Ltd" +
                    " </div>";
                MailMsg.IsBodyHtml = true;
                SmtpClient smtpclient = new SmtpClient("smtp.gmail.com", 587);
                smtpclient.UseDefaultCredentials = true;
                smtpclient.Credentials = new System.Net.NetworkCredential("visionarylifesciences7@gmail.com", "vls1234$");
                smtpclient.EnableSsl = true;

                smtpclient.Send(MailMsg);
                return "1";
            }
            catch (Exception ex)
            {
                return "0";
            }
        }
        public int InvationSMSToPatient(string Mobile,string docName,string patientName)
        {
            int StatusCode;
            string msg = docName +","+patientName;
            try
            {

                string url = "https://visionarylifescience.com/mobileapp/WebAPI/WebAPI.asmx/invitation_sms_to_patient?Mobile=" + Mobile + "&msg="+ msg + ""; // sample url
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(url).Result;
                    StatusCode = Convert.ToInt32(response.StatusCode);
                    return StatusCode;
                }
            }
            catch (Exception ex)
            {

                StatusCode = Convert.ToInt32(ex.Message);
            }

            return StatusCode;
        }

    }
}
