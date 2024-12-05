using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Data;
using System.Web;
using System.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using System;

namespace BL_WindowServiceReconciliation.AirAsia_API
{
    public class DotRezAirAsiaService
    {
        string Connec = "";
        public DotRezAirAsiaService(string Conn)
        {
            Connec = Conn;
        }
        //#region LCCFLOW_CHANGE
        public static string AirAsiaPostJson(string url, string MethodType, string AccessToken, string Request, string Userid, string LogsTrackID, string TransactionProcess)
        {
            string responseXML = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                byte[] data = Encoding.UTF8.GetBytes(Request);
                if (MethodType == "POST")
                {
                    request.Method = "POST";
                    request.ContentType = "application/json";
                }
                else if (MethodType == "PUT")
                {
                    request.Method = "PUT";
                    request.ContentType = "application/json";
                }
                else if (MethodType == "DELETE")
                {
                    request.Method = "DELETE";
                    request.ContentType = "application/json";
                }
                else
                    request.Method = "GET";

                if (!TransactionProcess.Contains("Token"))
                    request.Headers["Authorization"] = AccessToken;

                request.Headers.Add("Accept-Encoding", "gzip");
                request.ReadWriteTimeout = 200000;
                request.Timeout = 200000;
                if ((MethodType == "POST" || MethodType == "PUT") && Request.Length > 1)
                {
                    Stream dataStream = request.GetRequestStream();
                    dataStream.Write(data, 0, data.Length);
                    dataStream.Close();
                }
                HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();
                var rsp = webResponse.GetResponseStream();

                if (webResponse.ContentEncoding == null)
                {
                    StreamReader reader = new StreamReader(rsp, Encoding.Default);
                    responseXML = reader.ReadToEnd();
                }
                else if ((webResponse.ContentEncoding.ToLower().Contains("gzip")))
                {
                    using (StreamReader readStream = new StreamReader(new GZipStream(rsp, CompressionMode.Decompress)))
                    {
                        responseXML = readStream.ReadToEnd();
                    }
                }
                else
                {
                    StreamReader reader = new StreamReader(rsp, Encoding.Default);
                    responseXML = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                DAL.InsertExceptionLogs("", "", "DotRezAirAsiaService.cs", "XMLResponsePost_AirAsia", "Error", ex, "Exception occurred during calling AirAsiaPostJson method");
            }
            return responseXML;
        }

        public static string GetAccessToken(string TokenResponse)
        {
            string Tokenid = "";
            try
            {
                if (!string.IsNullOrEmpty(TokenResponse) && !TokenResponse.Contains("errors"))
                {
                    AccessToken tokens = JsonConvert.DeserializeObject<AccessToken>(TokenResponse);
                    JObject ObjResponse = JObject.Parse(TokenResponse);
                    if (ObjResponse != null)
                        Tokenid = ObjResponse["data"]["token"].ToString();
                }
            }
            catch (Exception ex)
            {
                DAL.InsertExceptionLogs("", "", "DotRezAirAsiaService.cs", "TokenResponse_AirAsia", "Error", ex, "Exception occurred during calling GetAccessToken method");
            }
            return Tokenid;
        }
        public class AccessToken
        {
            public Data data { get; set; }
            public Data metadata { get; set; }
        }
    }
}
