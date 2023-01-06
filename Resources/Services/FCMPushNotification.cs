using Nancy.Json;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;
using VLS_API.Model;

namespace Howzu_API.Services
{

    public class FCMPushNotification
    {
        public bool Successful
        {
            get;
            set;
        }
        public string Response
        {
            get;
            set;
        }
        public Exception Error
        {
            get;
            set;
        }

        public FCMPushNotification SendNotification(string _title, string _message, string _topic)
        {
            FCMPushNotification result = new FCMPushNotification();
            try
            {
                result.Successful = true;
                result.Error = null;
                // var value = message;
                var requestUri = "https://fcm.googleapis.com/fcm/send";

                WebRequest webRequest = WebRequest.Create(requestUri);
                webRequest.Method = "POST";
                webRequest.Headers.Add(string.Format("Authorization: key={0}", "AAAAdsxbSYs:APA91bFDLpDKhZEKA8fCzB1d9hnlVXWv0lc7mr6xuH4qfV1jklMuRAt9z86TdhOJN1iahJe23bhzsGP1EErdmXQlpkKIO_e5FHajdBWF7_fNcmC6Z4q880uatquRZEg-nOhRQiOfiaDW"));
                webRequest.Headers.Add(string.Format("Sender: id={0}", "510234675595"));
                webRequest.ContentType = "application/json";

                var data = new
                {
                    to = _topic, // Uncoment this if you want to test for single device
                    //  to = "/topics/" + _topic, // this is for topic 
                    notification = new
                    {
                        title = _title,
                        body = _message,
                        icon = "R.drawable.logoz",
                        click_action = "com.med.visionarylsci.lifescienes.hzu.vlsmarchup.activity.NotificationActivity"
                    },
                    payload = new
                    {
                        targetScreen = "",
                        id = 0,
                    },
                };
                var serializer = new JavaScriptSerializer();
                var json = serializer.Serialize(data);

                Byte[] byteArray = Encoding.UTF8.GetBytes(json);

                webRequest.ContentLength = byteArray.Length;
                using (Stream dataStream = webRequest.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);

                    using (WebResponse webResponse = webRequest.GetResponse())
                    {
                        using (Stream dataStreamResponse = webResponse.GetResponseStream())
                        {
                            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                            {
                                String sResponseFromServer = tReader.ReadToEnd();
                                result.Response = sResponseFromServer;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Successful = false;
                result.Response = null;
                result.Error = ex;
            }
            return result;
        }

      
    }
}
