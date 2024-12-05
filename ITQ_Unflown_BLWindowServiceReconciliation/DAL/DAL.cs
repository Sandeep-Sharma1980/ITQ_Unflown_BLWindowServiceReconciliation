using System;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Net;
using System.Text;
using System.Net.Cache;
using System.Collections;

namespace BL_WindowServiceReconciliation
{
    public class DAL
    {
        SqlConnection conn;
        SqlCommand cmd;
        private string Constr;
        private static string stStrconn;
        private static string stStrErrorLogFolderPath;
        private static string stStrExceptionLogsPath;
        private static string stStrLogType;
        private static string stStrLogStatus;
        public DAL()
        {
            Constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();
            conn = new SqlConnection();
            cmd = new SqlCommand();
        }
        public DAL(string constr, string errorlogFolderPath, string exceptionlogsPath, string logType, string logStatus)
        {
            Constr = constr;
            stStrconn = constr;
            stStrErrorLogFolderPath = errorlogFolderPath;
            stStrExceptionLogsPath = exceptionlogsPath;
            stStrLogType = logType;
            stStrLogStatus = logStatus;
        }
        public DataSet GetBLCredential(string vName)
        {
            DataSet ds = new DataSet();
            conn = new SqlConnection(Constr);
            try
            {
                conn.Open();
                cmd = new SqlCommand("Usp_GetCredentialvendorName", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@vendorName", vName);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(ds);
            }
            catch (Exception ex)
            {
                InsertExceptionLogs("", "", "DALClass.cs", "GetBLCredential", "Error", ex, "");
            }
            finally
            {
                cmd.Dispose();
                conn.Close();
                conn.Dispose();
            }
            return ds;
        }

        public DataTable GetPNRDatafrmTab()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            conn = new SqlConnection(Constr);
            try
            {
                conn.Open();
                cmd = new SqlCommand("Usp_GetPNRList", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(ds);
            }
            catch (Exception ex)
            {
                InsertExceptionLogs("", "", "BAL-DALClass.cs", "GetBLCredential", "Error", ex, "");
            }
            finally
            {
                cmd.Dispose();
                conn.Close();
                conn.Dispose();
            }
            if (ds.Tables.Count > 0)
            {
                dt = ds.Tables[0];
            }
            return dt;

        }

        public static void InsertExceptionLogs(string UserRequestId, string ParameterList, string ClassName, string MethodName, string ErrorInfo, Exception exx, string Remark)
        {
            int temp = 0;
            SqlConnection conn = null;
            SqlCommand cmd = null;
            string Constr = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();
            conn = new SqlConnection(Constr);
            cmd = new SqlCommand();
            string stStrLogStatus = "true";
            string stStrLogType = "db";
            if (stStrLogStatus.ToLower().Trim() == "true")
            {
                if (stStrLogType.Trim().ToLower() == "db")
                {
                    try
                    {
                        conn.Open();
                        cmd = new SqlCommand("uspInsertExceptionLog", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UserRequestId", UserRequestId);
                        cmd.Parameters.AddWithValue("@ParameterList", ParameterList);
                        cmd.Parameters.AddWithValue("@ClassName", ClassName);
                        cmd.Parameters.AddWithValue("@MethodName", MethodName);
                        cmd.Parameters.AddWithValue("@ErrorInfo", ErrorInfo);
                        cmd.Parameters.AddWithValue("@ExMessage", exx.Message.ToString());
                        cmd.Parameters.AddWithValue("@ExStackTrace", exx.StackTrace.ToString());
                        cmd.Parameters.AddWithValue("@Remark ", Remark);
                        temp = cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        ErrLogs(ex);
                    }
                    finally
                    {
                        cmd.Dispose();
                        conn.Close();
                        conn.Dispose();
                    }
                }
                else
                {
                    try
                    {
                        ErrorLogs(UserRequestId, ParameterList, ClassName, MethodName, ErrorInfo, exx, Remark);
                    }
                    catch (Exception ex)
                    {
                        ErrLogs(ex);
                    }
                    finally
                    {

                    }
                }
            }
        }

        private static void ErrorLogs(string userRequestId, string parameterList, string className, string methodName, string errorInfo, Exception exx, string remark)
        {
            throw new NotImplementedException();
        }

        public static void ErrLogs(Exception Err_Msg, string Cus_err_msg = "")
        {
            string sErrPath = "";

            string logStatus = "true", logType = "txt", Err_Module = "APP";
            if (logStatus.ToLower().Trim() == "true")
            {
                if (logType.Trim().ToLower() == "txt")
                {
                    try
                    {
                        string Err_FileName = "ErrorLog";
                        //Error Details
                        System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(Err_Msg, true);
                        string ErrorLineNo = (trace.GetFrame((trace.FrameCount - 1)).GetFileLineNumber()).ToString();


                        string Err_Source = (trace.GetFrame((trace.FrameCount - 1)).GetFileName()).ToString();

                        string activeDir = string.Empty;
                        // Specify a "currently active folder" \Error_Folder_
                        string logfilePath = sErrPath;
                        if (logfilePath != null)
                            activeDir = logfilePath + DateTime.Now.Date.ToString("dd-MMM-yyyy");

                        //string activeDir = @"C:\ITQ_B2C\" + DateTime.Now.Date.ToString("dd-MMM-yyyy");
                        // Creating the folder
                        DirectoryInfo objDirectoryInfo = new DirectoryInfo(activeDir);
                        if (!Directory.Exists(objDirectoryInfo.FullName))
                        {
                            string newPath = "", newFileName = "";
                            try
                            {
                                // Create a new file name. This example generates 
                                Directory.CreateDirectory(activeDir);
                                newFileName = Path.GetFileName(Err_FileName.ToString() + ".txt");
                                // Combine the new file name with the path
                                newPath = Path.Combine(activeDir, newFileName);
                            }
                            catch (Exception)
                            {
                                string activeDir2 = logfilePath + "Error_Folder_" + DateTime.Now.Date.ToString("dd-MMM-yyyy");
                                DirectoryInfo objDirectoryInfo2 = new DirectoryInfo(activeDir2);
                                // Create a new file name. This example generates
                                Directory.CreateDirectory(activeDir2);
                                newFileName = Path.GetFileName(Err_FileName.ToString() + ".txt");
                                // Combine the new file name with the path
                                newPath = Path.Combine(activeDir2, newFileName);
                            }
                            //// Create a new file name. This example generates                          
                            //// Combine the new file name with the path                          
                            FileStream fs = new FileStream(newPath, FileMode.Append, FileAccess.Write);
                            StreamWriter sw = new StreamWriter(fs);
                            sw.WriteLine("-------------------------------------------" + DateTime.Now.ToString() + "-------------------------------------------------");
                            sw.Write(sw.NewLine);
                            sw.WriteLine("Time:" + DateTime.Now.ToString());
                            sw.Write(sw.NewLine);
                            sw.WriteLine("Error Message:" + Err_Msg.Message);
                            sw.Write(sw.NewLine);
                            sw.WriteLine("InnerException Message:" + Err_Msg.InnerException);
                            sw.Write(sw.NewLine);
                            sw.WriteLine("StackTrace Error Message:" + Err_Msg.StackTrace);
                            sw.Write(sw.NewLine);
                            sw.WriteLine("Errror Source:" + Err_Source);
                            sw.Write(sw.NewLine);
                            sw.WriteLine("Error Module:" + Err_Module);
                            sw.Write(sw.NewLine);
                            sw.WriteLine("Error LineNo:" + ErrorLineNo);
                            sw.Write(sw.NewLine);
                            sw.WriteLine("Custom Error Msg:" + Cus_err_msg);
                            sw.Write(sw.NewLine);
                            sw.Flush();
                            sw.Close();
                            fs.Close();
                        }
                        else
                        {
                            string newFileName = Path.GetFileName(Err_FileName.ToString() + ".txt");
                            // Combine the new file name with the path
                            string newPath = Path.Combine(activeDir, newFileName);
                            FileStream fs = new FileStream(newPath, FileMode.Append, FileAccess.Write);
                            StreamWriter sw = new StreamWriter(fs);
                            sw.Write(sw.NewLine);
                            sw.WriteLine("-------------------------------------------" + DateTime.Now.ToString() + "-------------------------------------------------");
                            sw.Write(sw.NewLine);
                            sw.WriteLine("Error Message:" + Err_Msg.Message);
                            sw.Write(sw.NewLine);
                            sw.WriteLine("InnerException Message:" + Err_Msg.InnerException);
                            sw.Write(sw.NewLine);
                            sw.WriteLine("StackTrace Error Message:" + Err_Msg.StackTrace);
                            sw.Write(sw.NewLine);
                            sw.WriteLine("Errror Source:" + Err_Source);
                            sw.Write(sw.NewLine);
                            sw.WriteLine("Error Module:" + Err_Module);
                            sw.Write(sw.NewLine);
                            sw.WriteLine("Error LineNo:" + ErrorLineNo);
                            sw.Write(sw.NewLine);
                            sw.WriteLine("Custom Error Msg:" + Cus_err_msg);
                            sw.Write(sw.NewLine);

                            sw.Flush();
                            sw.Close();
                            fs.Close();
                        }
                    }
                    catch (Exception)
                    {

                    }

                }
            }

        }
        public DataTable GetConfigurableInfo()
        {
            DataTable dt = new DataTable();
            try
            {
                conn.Open();
                SqlCommand cmnd = new SqlCommand("select * from Tbl_APIConfigBox with(nolock)", conn);
                SqlDataAdapter da = new SqlDataAdapter(cmnd);
                da.Fill(dt);
                conn.Close();
            }
            catch (Exception ex)
            {
                ErrLogs(ex);
            }
            return dt;
        }

        public static DataSet GetUAPIData(string requestType, string soapRequest, DataTable dtCredential)
        {
            DataSet ds = new DataSet();
            try
            {
                string URL = string.Empty;
                string userId = dtCredential.Rows[0]["TAUSERID"].ToString();
                string password = dtCredential.Rows[0]["TAPASSWORD"].ToString();
                string Branch = dtCredential.Rows[0]["TAID"].ToString();
                string environment = "P";


                if (requestType == "PNR")
                {
                    switch (environment)
                    {
                        case "C":
                            URL = "https://apac.universal-api.pp.travelport.com/B2BGateway/connect/uAPI/UniversalRecordService";
                            break;
                        case "P":
                            URL = "https://apac.universal-api.travelport.com/B2BGateway/connect/uAPI/UniversalRecordService";
                            break;
                        case "I":
                            URL = "https://apac-uapi.ut.travelport.com:443/B2BGateway/connect/FSPUAT/UniversalRecordService";
                            break;
                    }
                }
                else if (requestType == "TICKET")
                {
                    switch (environment)
                    {
                        case "C":
                            URL = "https://apac.universal-api.pp.travelport.com/B2BGateway/connect/uAPI/AirService";
                            break;
                        case "P":
                            URL = "https://apac.universal-api.travelport.com/B2BGateway/connect/uAPI/AirService";
                            break;
                        case "I":
                            URL = "https://apac-uapi.ut.travelport.com:443/B2BGateway/connect/FSPUAT/AirService";
                            break;
                    }
                }
                soapRequest = soapRequest.Replace("BXXXXXX", Branch);
                string response = string.Empty;
                response = DAL.DoWebRequest(URL, "POST", soapRequest, userId, password, Branch, dtCredential);
                if (response.Contains("errors") || response.Contains("UNABLE TO PROCESS ") || response.Contains("Type=\"Error\"") || response.Contains("<faultstring>") || response.Contains("DocumentFailureInfo"))
                {
                    IEnumerable<XElement> errResponse = null;
                    string error = string.Empty;
                    XDocument xd = XDocument.Parse(response);
                    xd = RemoveNamespace(xd);
                    DataTable dtError = DAL.CreateErrorTable();
                    dtError.TableName = "Error";

                    if (response.Contains("Fault"))
                    {
                        errResponse = xd.Element("Envelope").Element("Body").Element("Fault").Elements("faultstring");
                    }
                    else if (response.Contains("DocumentFailureInfo"))
                    {
                        error = xd.Descendants("Envelope").Descendants("Body").Descendants("AirRetrieveDocumentRsp").Descendants("DocumentFailureInfo").Attributes("Message").First().Value;
                    }

                    else if (response.Contains("AirRetrieveDocumentRsp"))
                    {
                        errResponse = xd.Element("Envelope").Element("Body").Element("AirRetrieveDocumentRsp").Elements("ResponseMessage");
                    }
                    if (errResponse != null)
                    {
                        var errorDescription = errResponse.FirstOrDefault().Value;
                        dtError.Rows.Add(errorDescription);
                        ds.Tables.Add(dtError);
                    }
                    else if (error.Length > 0)
                    {
                        dtError.Rows.Add(error);
                        ds.Tables.Add(dtError);
                    }
                    return ds;
                }

                else
                {
                    UAPIXmlParser ObjXmlParser = new UAPIXmlParser();
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(response);
                    ds = ObjXmlParser.BuildXmlElementToDataSet(doc.DocumentElement);
                    //Add New table with web response data
                    DataTable tbwebRes = new DataTable();
                    tbwebRes.Columns.Add(new DataColumn("strResponse", typeof(string)));
                    DataRow row = tbwebRes.NewRow();
                    row["strResponse"] = response;
                    tbwebRes.Rows.Add(row);
                    ds.Tables.Add(tbwebRes);
                }
                return ds;
            }
            catch (Exception ex)
            {
                InsertExceptionLogs("", "", "BAL-DALClass.cs", "GetUAPIData", "Error", ex, "");
                throw;
            }
        }

        private static DataTable CreateErrorTable()
        {
            DataTable dtError = new DataTable();
            try
            {
                dtError.Columns.Add("ErrorDescription", typeof(string));
            }
            catch (Exception ex)
            {
                //ErrLogs(ex);
                InsertExceptionLogs("", "", "BAL-DALClass.cs", "CreateErrorTable", "Error", ex, "");
            }
            return dtError;
        }

        public static XDocument RemoveNamespace(XDocument xdoc)
        {
            try
            {
                foreach (XElement e in xdoc.Root.DescendantsAndSelf())
                {
                    if (e.Name.Namespace != XNamespace.None)
                    {
                        e.Name = XNamespace.None.GetName(e.Name.LocalName);
                    }
                    if (e.Attributes().Where(a => a.IsNamespaceDeclaration || a.Name.Namespace != XNamespace.None).Any())
                    {
                        e.ReplaceAttributes(e.Attributes().Select(a => a.IsNamespaceDeclaration ? null : a.Name.Namespace != XNamespace.None ? new XAttribute(XNamespace.None.GetName(a.Name.LocalName), a.Value) : a));
                    }
                }
            }
            catch (Exception ex)
            {
                //ErrLogs(ex);
                InsertExceptionLogs("", "", "BAL-DALClass.cs", "RemoveNamespace", "Error", ex, "");
                throw;
            }
            return xdoc;
        }

        public void InsertAndWriteReqResUapiLogs(string CompanyName, string VC, string userId, string serviceUserId, string PNR, string soapRequest_URR, string soapResponse_URR, string soapRequest_ARD, string soapResponse_ARD, string soapRequest_ARDTNo, string soapResponse_ARDTNo, string err, string reqid, string FromIPAddress)
        {
            conn = new SqlConnection(Constr);
            stStrLogStatus = ConfigurationManager.AppSettings["LogStatus"];
            stStrLogType = ConfigurationManager.AppSettings["LogType"];
            DAL dalobj = new DAL(stStrconn, stStrErrorLogFolderPath, stStrExceptionLogsPath, stStrLogType, stStrLogStatus);
            if (stStrLogStatus.ToLower().Trim() == "true")
            {
                if (stStrLogType.Trim().ToLower() == "db")
                {
                    try
                    {
                        string bookingReq = string.Empty;
                        string bookingRes = string.Empty;
                        bookingReq = "soapRequest_URR " + " : " + soapRequest_URR + " " + "soapRequest_ARD" + " : " + soapRequest_ARD + " " + "soapRequest_ARDTNo" + " : " + soapRequest_ARDTNo;
                        bookingRes = "soapResponse_URR " + " : " + soapResponse_URR + " " + "soapResponse_ARD" + " : " + soapResponse_ARD + " " + "soapResponse_ARDTNo" + " : " + soapResponse_ARDTNo;
                        conn.Open();
                        cmd = new SqlCommand("uspInsertWebServiceLog", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@ServiceID", serviceUserId);
                        cmd.Parameters.AddWithValue("@VendorCode", VC);
                        cmd.Parameters.AddWithValue("@PNR", PNR);
                        cmd.Parameters.AddWithValue("@Request", bookingReq);
                        cmd.Parameters.AddWithValue("@Response", bookingRes);
                        cmd.Parameters.AddWithValue("@Error", err);
                        cmd.Parameters.AddWithValue("@RequestId", reqid);
                        cmd.Parameters.AddWithValue("@RequestFrom", FromIPAddress);
                        int temp = cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        //ErrLogs(ex);
                        InsertExceptionLogs("", "", "BAL-DALClass.cs", "InsertJSON", "Error", ex, "Exception occurred during inserting data to datatable (usp_insert_Req_Res_Log)");
                    }
                    finally
                    {
                        cmd.Dispose();
                        conn.Close();
                        conn.Dispose();
                    }
                }
                else
                {
                    try
                    {
                        WriteLogUapi(CompanyName, VC, serviceUserId, PNR, soapRequest_URR, soapResponse_URR, soapRequest_ARD, soapResponse_ARD, soapRequest_ARDTNo, soapResponse_ARDTNo, stStrErrorLogFolderPath);
                    }
                    catch (Exception ex)
                    {
                        //ErrLogs(ex);
                        InsertExceptionLogs("", "", "BAL-DALClass.cs", "InsertJSON", "Error", ex, "");
                    }
                    finally
                    { }
                }

            }
        }
        public void InsertAndWriteReqResLogs(string CompanyName, string VC, string userId, string serviceUserId, string PNR, string sreq, string sres, string bookingReq, string bookingRes, string logoutreq, string logoutres, string err, string reqid, string FromIPAddress)
        {
            conn = new SqlConnection(Constr);
            stStrLogStatus = ConfigurationManager.AppSettings["LogStatus"];
            stStrLogType = ConfigurationManager.AppSettings["LogType"];
            if (stStrLogStatus.ToLower().Trim() == "true")
            {
                if (stStrLogType.Trim().ToLower() == "db")
                {
                    try
                    {
                        //string CompanyName, string vc, string serviceUserId,  string PNR, string sreq, string sres, string bookingReq, string bookingRes, string logoutreq, string logoutRes, string errorFilePath)
                        conn.Open();
                        cmd = new SqlCommand("uspInsertWebServiceLog", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@ServiceID", serviceUserId);
                        cmd.Parameters.AddWithValue("@VendorCode", VC);
                        cmd.Parameters.AddWithValue("@PNR", PNR);
                        cmd.Parameters.AddWithValue("@Request", bookingReq);
                        cmd.Parameters.AddWithValue("@Response", bookingRes);
                        cmd.Parameters.AddWithValue("@Error", err);
                        cmd.Parameters.AddWithValue("@RequestId", reqid);
                        cmd.Parameters.AddWithValue("@RequestFrom", FromIPAddress);
                        int temp = cmd.ExecuteNonQuery();
                        conn.Close();

                    }
                    catch (Exception ex)
                    {
                        //ErrLogs(ex);
                        InsertExceptionLogs("", "", "BAL-DALClass.cs", "InsertJSON", "Error", ex, "Exception occurred during inserting data to datatable (usp_insert_Req_Res_Log)");
                    }
                    finally
                    {
                        cmd.Dispose();
                        conn.Close();
                        conn.Dispose();
                    }
                }
                else
                {
                    try
                    {
                        WriteLog(CompanyName, VC, serviceUserId, PNR, sreq, sres, bookingReq, bookingRes, logoutreq, logoutres, stStrErrorLogFolderPath);
                    }
                    catch (Exception ex)
                    {
                        //ErrLogs(ex);
                        InsertExceptionLogs("", "", "BAL-DALClass.cs", "InsertJSON", "Error", ex, "");
                    }
                    finally
                    { }
                }

            }
        }
        public void WriteLog(string CompanyName, string vc, string serviceUserId, string PNR, string sreq, string sres, string bookingReq, string bookingRes, string logoutreq, string logoutRes, string errorFilePath)
        {
            string dirPath = errorFilePath + "/" + CompanyName + "/" + DateTime.Now.ToString("ddMMMyyy") + "/" + vc + "/" + serviceUserId + "/" + PNR;
            string filePath = errorFilePath + "/" + CompanyName + "/" + DateTime.Now.ToString("ddMMMyyy") + "/" + vc + "/" + serviceUserId + "/" + PNR + "/log.txt";// Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["ErrorTxt"]);// @"C:\Error.txt";           
            try
            {
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                    Thread.Sleep(200);
                    if (!File.Exists(filePath))
                    {

                        string content = "SessionReq : " + sreq +
                           Environment.NewLine + "-----------------------------------------------------------------------------" +
                           Environment.NewLine + "SessionRes :" + sres +
                           Environment.NewLine + "-----------------------------------------------------------------------------" +
                           Environment.NewLine + "bookingReq :" + bookingReq +
                           Environment.NewLine + "-----------------------------------------------------------------------------" +
                           Environment.NewLine + " bookingRes: " + bookingRes +
                           Environment.NewLine + "-----------------------------------------------------------------------------" +
                           Environment.NewLine + "bookingRes :" + logoutreq +
                           Environment.NewLine + "-----------------------------------------------------------------------------" +
                           Environment.NewLine + "logout :" + logoutRes +
                           Environment.NewLine + "-----------------------------------------------------------------------------" +
                           Environment.NewLine;



                        File.WriteAllText(filePath, content);
                    }
                }
                //    using (StreamWriter writer = new StreamWriter(filePath, true))
                //{
                //    writer.WriteLine("SessionReq :" + sreq + "<br/>" + Environment.NewLine + "SessionRes :" + sres +
                //       "" + Environment.NewLine + "bookingReq :" + bookingReq  +Environment.NewLine + " bookingRes: "+ bookingRes + Environment.NewLine + "bookingRes :" + bookingRes + Environment.NewLine + "logout :" + logoutRes);
                //    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);

                //    writer.Dispose();
                //}

            }
            catch (Exception ex)
            {
                ErrLogs(ex);
                //InsertExceptionLogs("", "", "BAL-DALClass.cs", "WriteLog", "Error", ex, "");
            }
            finally
            {

            }
        }
        public void WriteLogUapi(string CompanyName, string vc, string serviceUserId, string PNR, string soapRequest_URR, string soapResponse_URR, string soapRequest_ARD, string soapResponse_ARD, string soapRequest_ARDTNo, string soapResponse_ARDTNo, string errorFilePath)
        {
            string dirPath = errorFilePath + "/" + CompanyName + "/" + DateTime.Now.ToString("ddMMMyyy") + "/" + vc + "/" + serviceUserId + "/" + PNR;
            string filePath = errorFilePath + "/" + CompanyName + "/" + DateTime.Now.ToString("ddMMMyyy") + "/" + vc + "/" + serviceUserId + "/" + PNR + "/log.txt";
            try
            {
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                    Thread.Sleep(200);
                    if (!File.Exists(filePath))
                    {

                        string content = "Universal Record Retrieve Request : " + soapRequest_URR +
                        Environment.NewLine + "----------------------------------------------------------------------------------------------------------------------------------------------------------" +
                        Environment.NewLine + "Universal Record Retrieve Response  : " + soapResponse_URR +
                        Environment.NewLine + "----------------------------------------------------------------------------------------------------------------------------------------------------------" +
                        Environment.NewLine + "Air Retrieve Document Request : " + soapRequest_ARD +
                        Environment.NewLine + "----------------------------------------------------------------------------------------------------------------------------------------------------------" +
                        Environment.NewLine + "Air Retrieve Document Response : " + soapResponse_ARD +
                        Environment.NewLine + "----------------------------------------------------------------------------------------------------------------------------------------------------------" +
                        Environment.NewLine + "Air Retrieve Document Request with TicketNumber :" + soapRequest_ARD +
                        Environment.NewLine + "----------------------------------------------------------------------------------------------------------------------------------------------------------" +
                        Environment.NewLine + "Air Retrieve Document Response with TicketNumber :" + soapResponse_ARD +
                        Environment.NewLine + "----------------------------------------------------------------------------------------------------------------------------------------------------------" +
                        Environment.NewLine;
                        File.WriteAllText(filePath, content);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrLogs(ex);
            }
        }
        public static string DoWebRequest(string address, string method, string body, string userId, string password, string Branch, DataTable dtCredential = null)
        {
            string SOAPrequest = string.Empty;
            string output = string.Empty;
            SecurityProtocolType activeProtocol = ServicePointManager.SecurityProtocol;
            try
            {
                string CCType = string.Empty;
                string CCnumber = string.Empty;
                string CCExpirty = string.Empty;
                ServicePointManager.DefaultConnectionLimit = 100;
                ServicePointManager.MaxServicePointIdleTime = 5000;
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                SOAPrequest = body;
                var request = (HttpWebRequest)WebRequest.Create(address);
                request.ServicePoint.Expect100Continue = false;
                request.Method = method;
                request.Credentials = new NetworkCredential(userId, password);
                request.PreAuthenticate = true;
                if (method == "POST")
                {
                    if (!string.IsNullOrEmpty(body))
                    {
                        var requestBody = Encoding.UTF8.GetBytes(body);
                        request.ContentLength = requestBody.Length;
                        request.ContentType = "application/xml";
                        using (var requestStream = request.GetRequestStream())
                        {
                            requestStream.Write(requestBody, 0, requestBody.Length);
                        }
                    }
                    else
                    {
                        request.ContentLength = 0;
                    }
                }
                request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                try
                {
                    using (var response = request.GetResponse())
                    {
                        using (var stream = new StreamReader(response.GetResponseStream()))
                        {
                            output = stream.ReadToEnd();
                            return output;
                        }
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var strContent = "";
                        var stream = ex.Response.GetResponseStream();
                        using (var reader = new StreamReader(stream))
                        {
                            strContent = reader.ReadToEnd();
                        }
                        if (!string.IsNullOrEmpty(strContent))
                        {
                            XDocument xd = XDocument.Parse(strContent);
                            xd = RemoveNamespace(xd);
                            var item = xd.Descendants("string")
                           .Where(node => (string)node.Attribute("name") == "detail")
                           .Select(node => node.Value.ToString()).ToList();
                            output = item[0].ToString();
                        }
                    }
                    else if (ex.Status == WebExceptionStatus.Timeout)
                    {
                        output = "Request timeout is expired.";
                    }
                    else
                    {
                        output = ex.Message;
                    }
                }
                throw new Exception(output);
            }
            catch (Exception ex)
            {
                //ErrLogs(ex);
                InsertExceptionLogs("", "", "BAL-DALClass.cs", "DoWebRequest", "Error", ex, "");
                throw;
            }
        }

        public void BulkInsertTab(DataTable dt)
        {
            using (SqlConnection con = new SqlConnection(Constr))
            {
                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
                {
                    //Set the database table name
                    sqlBulkCopy.DestinationTableName = "dbo.PNRResultTab";

                    //Map the DataTable columns with that of the database table
                    //sqlBulkCopy.ColumnMappings.Add("Index", "PnrResultID");
                    sqlBulkCopy.ColumnMappings.Add("PNR", "PNR");
                    sqlBulkCopy.ColumnMappings.Add("TotalPax", "TotalPax");
                    sqlBulkCopy.ColumnMappings.Add("FlightStatus", "FlightStatus");
                    sqlBulkCopy.ColumnMappings.Add("AirLine", "AirLine");
                    sqlBulkCopy.ColumnMappings.Add("Status", "Status");
                    sqlBulkCopy.ColumnMappings.Add("Description", "Description");
                    sqlBulkCopy.ColumnMappings.Add("LocationCode", "LocationCode");
                    sqlBulkCopy.ColumnMappings.Add("SupplierType", "SupplierType");
                    sqlBulkCopy.ColumnMappings.Add("VendorName", "VendorName");
                    con.Open();
                    sqlBulkCopy.WriteToServer(dt);
                    con.Close();
                }
            }

            ////Set the Status of BL_GDSPNR Table To 'C' Close
            string pnrLst = string.Empty;
            foreach (DataRow dtRow in dt.Rows)
            {
                string cellData = dtRow["PNR"].ToString(); ;
                string[] pnrOnly = cellData.Split('_');
                pnrLst = pnrLst + "'" + pnrOnly[0] + "'" + ",";
            }
            pnrLst = pnrLst.TrimEnd(',');

            //Remove the duplicate pnr from list
            string[] newPnr = pnrLst.Split(',');
            var sList = new ArrayList();
            for (int i = 0; i < newPnr.Length; i++)
            {
                if (sList.Contains(newPnr[i]) == false)
                {
                    sList.Add(newPnr[i]);
                }
            }

            var sNew = sList.ToArray();

            using (SqlConnection con = new SqlConnection(Constr))
            {
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = "Update dbo.BL_GDSPNR Set [CurrentStatus] = 'C' , UpdatedDate = GetDate() where GDSPNR in (" + pnrLst + ")";
                    con.Open();
                    command.ExecuteNonQuery();
                    con.Close();
                }
            }
        }

        public void holdPnrTab(string pnr)
        {
            pnr = "'" + pnr + "'";
            using (SqlConnection con = new SqlConnection(Constr))
            {
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = "Update dbo.BL_GDSPNR Set [CurrentStatus] = 'H' , UpdatedDate = GetDate() where GDSPNR in (" + pnr + ")";
                    con.Open();
                    command.ExecuteNonQuery();
                    con.Close();
                }
            }
        }

        public DataTable GetPnrOnBranchLocCode(BLCredential obj)
        {
            string Provider = string.Empty;
            string BranchLocCode = string.Empty;
            string VendorName = string.Empty;
            string AirCode = string.Empty;

            Provider = obj.SupplierType.ToString();
            BranchLocCode = obj.Location.ToString();
            VendorName = obj.VendorName.ToString();
            if(Provider=="LCC")
            {
                AirCode = obj.AirCode.ToString();
            }
            else
            {
                AirCode = string.Empty;
            }          

            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            conn = new SqlConnection(Constr);
            try
            {
                conn.Open();
                cmd = new SqlCommand("BL_GetPNRListByLocCode", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Provider", Provider);
                cmd.Parameters.AddWithValue("@BranchLocationCode", BranchLocCode);
                cmd.Parameters.AddWithValue("@VendorName", VendorName);
                cmd.Parameters.AddWithValue("@AirCode", AirCode);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(ds);
            }
            catch (Exception ex)
            {
                InsertExceptionLogs("", "", "DAL.cs", "GetPnrOnBranchLocCode", "Error", ex, "");
            }
            finally
            {
                cmd.Dispose();
                conn.Close();
                conn.Dispose();
            }
            if (ds.Tables.Count > 0)
            {
                dt = ds.Tables[0];
            }
            return dt;
        }

    }
}
