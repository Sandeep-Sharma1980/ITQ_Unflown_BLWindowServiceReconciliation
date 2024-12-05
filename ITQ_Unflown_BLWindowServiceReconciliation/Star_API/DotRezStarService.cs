using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace BL_WindowServiceReconciliation.Star_API
{
    public class DotRezStarService
    {
        string Connec = "";
        public DotRezStarService(string Conn)
        {
            Connec = Conn;
        }
        public static string StarPostJson(string url, string MethodType, string TransactionProcess = "")
        {
            string responseXML = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                string _compress = string.Empty;

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

                request.ReadWriteTimeout = 200000;
                request.Timeout = 200000;
                if (MethodType == "POST" || MethodType == "PUT" || MethodType == "DELETE")
                {
                    Stream dataStream = request.GetRequestStream();
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
                DAL.InsertExceptionLogs("", "", "DotRezStarService.cs", "XMLResponsePost_Star", "Error", ex, "Exception occurred during calling StarPostJson method");
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
                DAL.InsertExceptionLogs("", "", "DotRezStarService.cs", "GetAccessToken_Star", "Error", ex, "Exception occurred during calling GetAccessToken method");
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
