using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BL_WindowServiceReconciliation.IndigoSpiceGoAir_API
{
    public class IndigoSpiceGoAir_API
    {
        private string Constr;
        private string UploadedFolderPath;
        private string DownloadFolderPath;
        private string ErrorTextFilePath;
        private string SrvRequest;
        private string LogType;
        private string Logstatus;
        private string ErrorLogFolderPath;
        private string ExceptionLogsPath;
        string sErrMsg = string.Empty;
        private string CompanyName = "Balmer Lawrie";
        BAL objBal = new BAL();
        public async Task<DataSet> XMLResponsePost_IndigoSpiceGoAir(string Airline, DataTable ds1, DataTable dt2)
        {
            int taskCount = 0;
            int Count = 0;
            DataSet FnlPNR = new DataSet();
            DataTable dtPNR = PnrDetails();
            ConcurrentBag<Task<Task<DataTable>>> bag = new ConcurrentBag<Task<Task<DataTable>>>();
            FnlPNR.Tables.Add(dtPNR);
            string GPNR = string.Empty;
            string TCCode = string.Empty;
            string LogsTrackID = string.Empty;
            int index = 0;
            int newCount = 0;
            string liftstatus = string.Empty;
            string airline = string.Empty;
            DAL objDal = new DAL(Constr, ErrorLogFolderPath, ExceptionLogsPath, LogType, Logstatus);
            try
            {
                string errorMsg = string.Empty;

                string UID = dt2.Rows[0]["TAUSERID"].ToString();
                string PWD = dt2.Rows[0]["TAPASSWORD"].ToString();
                string Dom = dt2.Rows[0]["LOGINID"].ToString();
                string exp1 = dt2.Rows[0]["Expr1"].ToString();
                string exp2 = dt2.Rows[0]["Expr2"].ToString();
                string exp3 = dt2.Rows[0]["Expr3"].ToString();

                DotRezAkasaRequest objreq = new DotRezAkasaRequest(Constr);
                string tockenReq = objreq.GenerateToken(UID, PWD, Dom);
                string tockenResp = DotRezAkasaService.AkasaPostJson(exp1, "POST", "", tockenReq, TCCode, LogsTrackID, "CancelToken");

                string errmsg = string.Empty;
                if (!string.IsNullOrEmpty(tockenResp) && !tockenResp.Contains("errors"))
                {
                    string tocken = DotRezAkasaService.GetAccessToken(tockenResp);
                    int countt = ds1.Rows.Count;
                    try
                    {
                        for (int k = 0; k < countt; k++)
                        {
                            string status = string.Empty;
                            string description = string.Empty;
                            int totpax = 0;
                            GPNR = ds1.Rows[k][0].ToString();
                            DataRow Air_row = dtPNR.NewRow();
                            string retrieveUrl = exp2.Replace("/v3/", "/v1/") + "/retrieve/byRecordLocator/" + GPNR;
                            string retrieveResponse = DotRezAkasaService.AkasaPostJson(retrieveUrl, "GET", tocken, "", TCCode, "", "retrieve");
                            if (!string.IsNullOrEmpty(retrieveResponse))
                            {
                                JObject ObjDivdResponse = JObject.Parse(retrieveResponse);
                                if (ObjDivdResponse["data"]["passengers"] != null)
                                {
                                    foreach (var passenger in ObjDivdResponse["data"]["passengers"])
                                    {
                                        totpax = totpax + passenger.Count();
                                    }
                                }
                                if (ObjDivdResponse["data"]["journeys"] != null)
                                {
                                    foreach (var journey in ObjDivdResponse["data"]["journeys"])
                                    {
                                        foreach (var segment in journey["segments"])
                                        {
                                            string sector = "";
                                            JObject o = JObject.Parse(segment["passengerSegment"].ToString());
                                            foreach (JProperty property in o.Properties())
                                            {
                                                sector = property.Name;
                                                liftstatus = segment["passengerSegment"][sector]["liftStatus"].ToString();
                                                if (liftstatus == "3")
                                                {
                                                    status = "Sucess";
                                                    description = "NoShow,";
                                                    break;
                                                }
                                                else
                                                {
                                                    status = "Failure";
                                                    if (liftstatus == "0")
                                                    {
                                                        description = description + "Default" + ",";
                                                    }
                                                    else if (liftstatus == "1")
                                                    {
                                                        description = description + "CheckedIn" + ",";
                                                    }
                                                    else if (liftstatus == "2")
                                                    {
                                                        description = description + "Boarded" + ",";
                                                    }
                                                    else
                                                    {
                                                        description = description + "Unidentified Status" + ",";
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            description = description.Remove(description.Length - 1);
                            airline = "QP";
                            index = newCount + 1;
                            Air_row["PNR"] = GPNR;
                            Air_row["TotalPax"] = totpax;
                            Air_row["FlightStatus"] = liftstatus;
                            Air_row["Status"] = status;
                            Air_row["Description"] = description;
                            Air_row["Airline"] = airline;
                            Air_row["Index"] = index;
                            Air_row["LocationCode"] = dt2.Rows[0]["Location"].ToString();
                            Air_row["SupplierType"] = dt2.Rows[0]["SUPPLIERTYPE"].ToString();
                            Air_row["VendorName"] = dt2.Rows[0]["VendorName"].ToString();
                            dtPNR.Rows.Add(Air_row);
                            newCount = index;
                            objBal.InsertAndWriteReqResLogs(CompanyName, "QP", "", UID, GPNR, tockenReq, tockenResp, retrieveUrl, retrieveResponse, "", "", "", Airline, "");
                        }
                    }
                    catch (Exception ex)
                    {
                        DataRow Air_row = dtPNR.NewRow();
                        airline = "QP";
                        index = newCount + 1;
                        Air_row["PNR"] = GPNR;
                        Air_row["TotalPax"] = 0;
                        Air_row["FlightStatus"] = "";
                        Air_row["Status"] = "Failure";
                        Air_row["Description"] = "Unidentified Status";
                        Air_row["Airline"] = airline;
                        Air_row["Index"] = index;
                        Air_row["LocationCode"] = dt2.Rows[0]["Location"].ToString();
                        Air_row["SupplierType"] = dt2.Rows[0]["SUPPLIERTYPE"].ToString();
                        Air_row["VendorName"] = dt2.Rows[0]["VendorName"].ToString();
                        dtPNR.Rows.Add(Air_row);
                        BAL.InsertExceptionLogs("", "", "AkasaAir_API.cs", "XMLResponsePost_Akasa", "Error", ex, "Exception occurred during calling XMLResponsePost_Akasa method");
                    }
                }
                if (taskCount == Count)
                {
                    try
                    {
                        var continuation = Task.WhenAll(bag.ToArray());
                        try
                        {
                            await continuation.ConfigureAwait(true);
                            continuation.Wait();
                        }
                        catch (AggregateException)
                        {
                        }
                        if (continuation.Status == TaskStatus.RanToCompletion)
                        {
                            if (continuation.Status == TaskStatus.RanToCompletion)
                            {
                                foreach (var result in continuation.Result)
                                {
                                    if (result.Status == TaskStatus.RanToCompletion)
                                    {
                                        DataTable lst = (DataTable)result.Result;

                                        if (lst.Rows.Count > 0)
                                        {
                                            FnlPNR.Tables[0].Rows.Add(lst.Rows[0].ItemArray);
                                        }
                                    }
                                }
                            }
                        }
                        Thread.Sleep(200);
                        Count = 1;

                        //taskList = new List<Task>();
                        bag = new ConcurrentBag<Task<Task<DataTable>>>();
                        Thread.Sleep(200);
                    }
                    catch
                    {

                    }
                }
                else
                {
                    Count++;
                }
                if (FnlPNR.Tables[0].Rows.Count > 0)
                {
                    objBal.BulkInsertTab(FnlPNR.Tables[0]);
                }
                return FnlPNR;
            }
            catch (Exception ex)
            {
                BAL.InsertExceptionLogs("", "", "AkasaAir_API.cs", "XMLResponsePost_Akasa", "Error", ex, "Exception occurred during calling XMLResponsePost_Akasa method");
                throw;
            }
        }
        public string GetFlightCrdAndStatus(string flight, string crdId, string fileName, string filePath, string fileDirectory, string folderName, string folderCreateDate, string companyName, string UserName, string exceptionlogsPath)
        {

            string msg = "";
            string UID = "";
            string PWD = "";
            string Dom = "";
            string BrCode = "";

            int CV = 0;

            string Surl = "";
            string Burl = "";
            string exp1 = "";
            string exp2 = "";
            string paramList = "CompanyName : " + companyName + " ,Vendor Code : " + flight + ", ServiceId : " + crdId + ", User Name : " + UserName;
            DAL objDal = new DAL(Constr, ErrorLogFolderPath, ExceptionLogsPath, LogType, Logstatus);
            string ServiceRequest = SrvRequest;// Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SrvRequest"]); // when  ServiceRequest=="true" ,call service and  ServiceRequest=="false" ,not call service 

            try
            {
                DataTable dt2 = new DataTable();

                if (!string.IsNullOrEmpty(flight))
                {
                    #region Flight credential 6E,SG,G8
                    if (flight.Trim().ToUpper() == "6E" || flight.Trim().ToUpper() == "SG" || flight.Trim().ToUpper() == "G8")
                    {
                        dt2 = objDal.GetFltCrdAndAirLineName("CRD", flight.Trim(), crdId.Trim(), companyName);
                        if (dt2.Rows.Count > 0)
                        {
                            UID = dt2.Rows[0]["UserId"].ToString();
                            PWD = dt2.Rows[0]["Password"].ToString();
                            Dom = dt2.Rows[0]["ServerUrlOrIP"].ToString();
                            exp1 = dt2.Rows[0]["Exprs1"].ToString();
                            exp2 = dt2.Rows[0]["Exprs2"].ToString();
                        }
                        if ((!string.IsNullOrEmpty(exp1)) && (!string.IsNullOrEmpty(exp1)))
                        {
                            if (flight.Trim().ToUpper() == "6E" || flight.Trim().ToUpper() == "SG")
                            {
                                CV = int.Parse(MethodSvcUrl.Indigo_CV);
                                Surl = exp1;// MethodSvcUrl.Indigo_Surl;
                                Burl = exp2;// MethodSvcUrl.Indigo_Burl;
                            }
                        }

                        else if (flight.Trim().ToUpper() == "G8")
                        {
                            CV = int.Parse(MethodSvcUrl.Goair_CV); //int.Parse(System.Configuration.ConfigurationManager.AppSettings["SpiceCV"].ToString());
                                                                   //S_Binding = System.Configuration.ConfigurationManager.AppSettings["SG_S_Binding"].ToString();
                                                                   //B_Binding = System.Configuration.ConfigurationManager.AppSettings["SG_B_Binding"].ToString();
                            Surl = MethodSvcUrl.Goair_Surl;// System.Configuration.ConfigurationManager.AppSettings["SG_Surl"].ToString();
                            Burl = MethodSvcUrl.Goair_Burl;// System.Configuration.ConfigurationManager.AppSettings["SG_Burl"].ToString();
                        }

                        DataSet ds = new DataSet();
                        if (flight.Trim().ToUpper() == "6E" || flight.Trim().ToUpper() == "SG" || flight.Trim().ToUpper() == "G8")
                        {
                            #region 6E,SG,G8,ST and FB Request and Response                   
                            LCC_API_Req_Res lCC_API_Req = new LCC_API_Req_Res(UID, PWD, Dom, CV, Surl, Burl, flight.Trim().ToUpper(), Constr, ErrorTextFilePath, LogType, Logstatus, ErrorLogFolderPath, companyName, ExceptionLogsPath);

                            if (!string.IsNullOrEmpty(filePath))
                            {
                                try
                                {
                                    DataTable dt1 = ReadFromExcel(filePath);
                                    for (int i = dt1.Rows.Count - 1; i >= 0; i--)
                                    {
                                        if (!String.IsNullOrEmpty(dt1.Rows[i][0].ToString()))
                                        {
                                            continue;
                                        }
                                        dt1.Rows[i].Delete();
                                    }
                                    dt1.AcceptChanges();
                                    DataSet ds1 = new DataSet();
                                    ds1.Tables.Add(dt1);
                                    try
                                    {
                                        string Fromfile = filePath;
                                        String[] strFileName = fileName.Split('.');
                                        string NewFileName = strFileName[0] + "_inprocess." + strFileName[1];
                                        string Tofile = fileDirectory + "\\" + NewFileName;
                                        //STATUS CHANGE 
                                        bool status = File.Exists(Tofile);
                                        if (status)
                                        {
                                            File.Delete(Tofile);
                                        }
                                        File.Move(Fromfile, Tofile);
                                        fileName = NewFileName;
                                        filePath = Tofile;
                                    }
                                    catch (Exception ex)
                                    {
                                        DALClass.InsertExceptionLogs("", "", "BAL-FileRead.cs", "GetFlightCrdAndStatus-6E", "Error", ex, "");
                                    }
                                    if (ServiceRequest.ToLower() == "false")
                                    {
                                        CreateDirectoryForDownload(dt1, folderName, fileName, filePath, fileDirectory, companyName);// use for service testing file download and excel
                                    }
                                    if (dt1 != null && ServiceRequest.ToLower() == "true")
                                    {
                                        if (flight.Trim().ToUpper() == "6E")
                                        {
                                            ds = lCC_API_Req.XMLResponsePost("6E", ds1).Result; //URL, indigoID, "6E", 
                                        }
                                        else if (flight.Trim().ToUpper() == "SG")
                                        {
                                            ds = lCC_API_Req.XMLResponsePost("SG", ds1).Result;//URL, spiceID, "SG", 
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    DALClass.InsertExceptionLogs("", "", "BAL-FileRead.cs", "GetFlightCrdAndStatus", "Error", ex, "Exception occurred during calling 6E & SG Request and Response method(GetFlightCrdAndStatus)");
                                }
                            }
                            #endregion  6E & SG Request and Response
                        }

                        if (flight.Trim().ToUpper() == "6E" || flight.Trim().ToUpper() == "SG" || flight.Trim().ToUpper() == "G8")
                        {

                            if (ds.Tables.Count > 0)
                            {
                                //Added by Hitesh just addon a parameter
                                bool result = UnflownPNRDetails(ds, fileName, crdId, UserName, flight);

                                if (ds.Tables[0].Rows.Count > 0)
                                {
                                    //ExportData(ds);
                                    string DownloadPath = DownloadFolderPath; // System.Configuration.ConfigurationManager.AppSettings["Download"].ToString();
                                    DataTable dtRes = new DataTable();
                                    dtRes = ds.Tables[0];
                                    CreateDirectoryForDownload(dtRes, folderName, fileName, filePath, fileDirectory, companyName);
                                }
                                else
                                {
                                    //DALClassDal.InsertExceptionLog("BookingStatusDLL", "FileRead", "GetFlightCrdAndStatus", "FR-06", "", "", folderName, filePath, "Go to else part - Not getting any rows in service response dataset table.");
                                    if (ServiceRequest.ToLower() != "false")
                                    {
                                        bool statusEx = RenameExcelFileName(folderName, fileName, filePath, fileDirectory);
                                        bool statusFolder = CheckRenameFolderStatus(folderName, fileName, filePath, fileDirectory);
                                    }
                                }
                            }
                            else
                            {
                                //DALClassDal.InsertExceptionLog("BookingStatusDLL", "FileRead", "GetFlightCrdAndStatus", "FR-07", "", "", folderName, filePath, "Go to else part - Service response dataset is null or empty.");
                                if (ServiceRequest.ToLower() != "false")
                                {
                                    bool statusEx = RenameExcelFileName(folderName, fileName, filePath, fileDirectory);
                                    bool statusFolder = CheckRenameFolderStatus(folderName, fileName, filePath, fileDirectory);
                                }
                            }
                        }
                    }
                    #endregion Flight credential 6E,SG,G8

                    #region Flight credential UAPI and AK,QP
                    else if (flight.Trim().ToUpper() == "UAPI")
                    {

                        DataSet dsFRst = new DataSet();
                        try
                        {
                            //Fill the Datatable with the Value from Excel & Remove the empty row                    
                            DataTable dtReFrmExl = ReadFromExcel(filePath);

                            for (int i = dtReFrmExl.Rows.Count - 1; i >= 0; i--)
                            {
                                if (!String.IsNullOrEmpty(dtReFrmExl.Rows[i][0].ToString()))
                                {
                                    continue;
                                }
                                dtReFrmExl.Rows[i].Delete();
                            }
                            dtReFrmExl.AcceptChanges();
                            DataSet ds1 = new DataSet();
                            ds1.Tables.Add(dtReFrmExl);

                            //Get the credential provided and authenticate
                            String[] Crd = filePath.Split("\\");
                            string str1 = Crd[Crd.Length - 3];
                            string str2 = Crd[Crd.Length - 2];
                            String[] gap = str1.Split("_");
                            string gap1 = gap[gap.Length - 1];
                            string str3 = gap1 + "/" + str2;

                            //String str3 = UID;
                            dt2 = objDal.GetFltCrdAndAirLineName("CRD", flight.Trim(), str3.Trim(), companyName);

                            if (dt2.Rows.Count > 0)
                            {
                                UID = dt2.Rows[0]["UserId"].ToString();
                                PWD = dt2.Rows[0]["Password"].ToString();
                                Dom = dt2.Rows[0]["ServerUrlOrIP"].ToString();
                                BrCode = dt2.Rows[0]["BranchCode"].ToString();
                            }

                            // Intiate Process flow of UAPI web services
                            LCC_API_Req_Res lCC_API_Req = new LCC_API_Req_Res(UID, PWD, Dom, CV, Surl, Burl, flight.Trim().ToUpper(), Constr, ErrorTextFilePath, LogType, Logstatus, ErrorLogFolderPath, companyName, ExceptionLogsPath);
                            if (dtReFrmExl != null && ServiceRequest.ToLower() == "true")
                            {
                                dsFRst = lCC_API_Req.XMLResponsePost_UAPI("UAPI", ds1, dt2).Result;
                            }

                            if (dsFRst.Tables.Count > 0)
                            {
                                //Added by Hitesh just addon a parameter
                                bool result = UnflownPNRDetails(dsFRst, fileName, crdId, UserName, flight);
                                if (dsFRst.Tables[0].Rows.Count > 0)
                                {

                                    //ExportData(ds);
                                    string DownloadPath = DownloadFolderPath; // System.Configuration.ConfigurationManager.AppSettings["Download"].ToString();
                                    DataTable dtRes = new DataTable();
                                    dtRes = dsFRst.Tables[0];
                                    CreateDirectoryForDownload(dtRes, folderName, fileName, filePath, fileDirectory, companyName);
                                }
                                else
                                {
                                    if (ServiceRequest.ToLower() != "false")
                                    {
                                        bool statusEx = RenameExcelFileName(folderName, fileName, filePath, fileDirectory);
                                        bool statusFolder = CheckRenameFolderStatus(folderName, fileName, filePath, fileDirectory);
                                    }
                                }
                            }
                            else
                            {
                                if (ServiceRequest.ToLower() != "false")
                                {
                                    bool statusEx = RenameExcelFileName(folderName, fileName, filePath, fileDirectory);
                                    bool statusFolder = CheckRenameFolderStatus(folderName, fileName, filePath, fileDirectory);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            DALClass.InsertExceptionLogs("", paramList, "BAL-FileRead.cs", "GetFlightCrdAndStatus-UAPI", "Error", ex, "Exception occurred during calling UAPI Request and Response method(GetFlightCrdAndStatus)");
                            //sErrMsg = ex.Message;
                            //DALClass.ErrLogs(ex);
                        }

                    }
                    else if (flight.Trim().ToUpper() == "AK")
                    {
                        string TCCode = string.Empty;
                        string LogsTrackID = string.Empty;
                        string TransactionProcess = string.Empty;
                        string GPNR = string.Empty;
                        DataSet dsFRst = new DataSet();
                        try
                        {
                            //Fill the Datatable with the Value from Excel & Remove the empty row                    
                            DataTable dtReFrmExl = ReadFromExcel(filePath);

                            for (int i = dtReFrmExl.Rows.Count - 1; i >= 0; i--)
                            {
                                if (!String.IsNullOrEmpty(dtReFrmExl.Rows[i][0].ToString()))
                                {
                                    continue;
                                }
                                dtReFrmExl.Rows[i].Delete();
                            }
                            dtReFrmExl.AcceptChanges();
                            DataSet ds1 = new DataSet();
                            ds1.Tables.Add(dtReFrmExl);

                            dt2 = objDal.GetFltCrdAndAirLineName("CRD", flight.Trim(), crdId.Trim(), companyName);
                            if (dt2.Rows.Count > 0)
                            {
                                UID = dt2.Rows[0]["UserId"].ToString();
                                PWD = dt2.Rows[0]["Password"].ToString();
                                Dom = dt2.Rows[0]["LoginID"].ToString();
                                exp1 = dt2.Rows[0]["Exprs1"].ToString();
                                exp2 = dt2.Rows[0]["Exprs2"].ToString();
                            }

                            // Intiate Process flow of AirAsia web services
                            LCC_API_Req_Res lCC_API_Req = new LCC_API_Req_Res(UID, PWD, Dom, CV, Surl, Burl, flight.Trim().ToUpper(), Constr, ErrorTextFilePath, LogType, Logstatus, ErrorLogFolderPath, companyName, ExceptionLogsPath);
                            if (dtReFrmExl != null && ServiceRequest.ToLower() == "true")
                            {
                                dsFRst = lCC_API_Req.XMLResponsePost_AirAsia("AirAsia", ds1, dt2).Result;
                            }

                            if (dsFRst.Tables.Count > 0)
                            {

                                if (dsFRst.Tables[0].Rows.Count > 0)
                                {

                                    //ExportData(ds);
                                    string DownloadPath = DownloadFolderPath; // System.Configuration.ConfigurationManager.AppSettings["Download"].ToString();
                                    DataTable dtRes = new DataTable();
                                    dtRes = dsFRst.Tables[0];
                                    CreateDirectoryForDownload(dtRes, folderName, fileName, filePath, fileDirectory, companyName);
                                }
                                else
                                {
                                    DALClass.WriteExceLogs("", "", "BAL-FileRead.cs", "GetFlightCrdAndStatus", "Custom-Error", "", "", "Go to else part - Not getting any rows in service response dataset table.");
                                    if (ServiceRequest.ToLower() != "false")
                                    {
                                        bool statusEx = RenameExcelFileName(folderName, fileName, filePath, fileDirectory);
                                        bool statusFolder = CheckRenameFolderStatus(folderName, fileName, filePath, fileDirectory);
                                    }
                                }
                            }
                            else
                            {
                                DALClass.WriteExceLogs("", "", "BAL-FileRead.cs", "GetFlightCrdAndStatus", "Custom-Error", "", "", "Go to else part - Service response dataset is null or empty.");
                                if (ServiceRequest.ToLower() != "false")
                                {
                                    bool statusEx = RenameExcelFileName(folderName, fileName, filePath, fileDirectory);
                                    bool statusFolder = CheckRenameFolderStatus(folderName, fileName, filePath, fileDirectory);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            sErrMsg = ex.Message;
                            //DALClass.ErrLogs(ex);
                            DALClass.InsertExceptionLogs("", "", "BAL-FileRead.cs", "GetFlightCrdAndStatus-AK", "Error", ex, "Exception occurred during calling AK Request and Response method(GetFlightCrdAndStatus)");
                        }
                    }
                    else if (flight.Trim().ToUpper() == "QP")
                    {
                        string TCCode = string.Empty;
                        string LogsTrackID = string.Empty;
                        string TransactionProcess = string.Empty;
                        string GPNR = string.Empty;
                        DataSet dsFRst = new DataSet();
                        try
                        {
                            //Fill the Datatable with the Value from Excel & Remove the empty row                    
                            DataTable dtReFrmExl = ReadFromExcel(filePath);

                            for (int i = dtReFrmExl.Rows.Count - 1; i >= 0; i--)
                            {
                                if (!String.IsNullOrEmpty(dtReFrmExl.Rows[i][0].ToString()))
                                {
                                    continue;
                                }
                                dtReFrmExl.Rows[i].Delete();
                            }
                            dtReFrmExl.AcceptChanges();
                            DataSet ds1 = new DataSet();
                            ds1.Tables.Add(dtReFrmExl);

                            dt2 = objDal.GetFltCrdAndAirLineName("CRD", flight.Trim(), crdId.Trim(), companyName);
                            if (dt2.Rows.Count > 0)
                            {
                                UID = dt2.Rows[0]["UserId"].ToString();
                                PWD = dt2.Rows[0]["Password"].ToString();
                                Dom = dt2.Rows[0]["LoginID"].ToString();
                                exp1 = dt2.Rows[0]["Exprs1"].ToString();
                                exp2 = dt2.Rows[0]["Exprs2"].ToString();
                            }

                            // Intiate Process flow of Akasa web services                       
                            LCC_API_Req_Res lCC_API_Req = new LCC_API_Req_Res(UID, PWD, Dom, CV, Surl, Burl, flight.Trim().ToUpper(), Constr, ErrorTextFilePath, LogType, Logstatus, ErrorLogFolderPath, companyName, ExceptionLogsPath);
                            if (dtReFrmExl != null && ServiceRequest.ToLower() == "true")
                            {
                                dsFRst = lCC_API_Req.XMLResponsePost_Akasa("Akasa", ds1, dt2).Result;
                            }

                            if (dsFRst.Tables.Count > 0)
                            {
                                if (dsFRst.Tables[0].Rows.Count > 0)
                                {
                                    //ExportData(ds);
                                    string DownloadPath = DownloadFolderPath; // System.Configuration.ConfigurationManager.AppSettings["Download"].ToString();
                                    DataTable dtRes = new DataTable();
                                    dtRes = dsFRst.Tables[0];
                                    CreateDirectoryForDownload(dtRes, folderName, fileName, filePath, fileDirectory, companyName);
                                }
                                else
                                {
                                    DALClass.WriteExceLogs("", "", "BAL-FileRead.cs", "GetFlightCrdAndStatus", "Custom-Error", "", "", "Go to else part - Not getting any rows in service response dataset table.");
                                    if (ServiceRequest.ToLower() != "false")
                                    {
                                        bool statusEx = RenameExcelFileName(folderName, fileName, filePath, fileDirectory);
                                        bool statusFolder = CheckRenameFolderStatus(folderName, fileName, filePath, fileDirectory);
                                    }
                                }
                            }
                            else
                            {
                                DALClass.WriteExceLogs("", "", "BAL-FileRead.cs", "GetFlightCrdAndStatus", "Custom-Error", "", "", "Go to else part - Service response dataset is null or empty.");
                                if (ServiceRequest.ToLower() != "false")
                                {
                                    bool statusEx = RenameExcelFileName(folderName, fileName, filePath, fileDirectory);
                                    bool statusFolder = CheckRenameFolderStatus(folderName, fileName, filePath, fileDirectory);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            sErrMsg = ex.Message;
                            //DALClass.ErrLogs(ex);
                            DALClass.InsertExceptionLogs("", "", "BAL-FileRead.cs", "GetFlightCrdAndStatus-QP", "Error", ex, "Exception occurred during calling QP Request and Response method(GetFlightCrdAndStatus)");

                        }

                    }
                    #endregion Flight credential UAPI,Akasa and Airasia

                    #region Star and Flybig Airlines
                    else if (flight.Trim().ToUpper() == "TROG")
                    {
                        string TCCode = string.Empty;
                        string LogsTrackID = string.Empty;
                        string TransactionProcess = string.Empty;
                        string GPNR = string.Empty;
                        DataSet dsFRst = new DataSet();


                        try
                        {
                            //First Step
                            dt2 = objDal.GetFltCrdAndAirLineName("CRD", flight.Trim(), crdId.Trim(), companyName);
                            if (dt2.Rows.Count > 0)
                            {
                                UID = dt2.Rows[0]["UserId"].ToString();
                                PWD = dt2.Rows[0]["Password"].ToString();
                                Dom = dt2.Rows[0]["LoginID"].ToString();
                                exp1 = dt2.Rows[0]["Exprs1"].ToString();
                                exp2 = dt2.Rows[0]["Exprs2"].ToString();
                            }

                            //Second Step

                            // Intiate Process flow of Star web services
                            LCC_API_Req_Res lCC_API_Req = new LCC_API_Req_Res(UID, PWD, Dom, CV, Surl, Burl, flight.Trim().ToUpper(), Constr, ErrorTextFilePath, LogType, Logstatus, ErrorLogFolderPath, companyName, ExceptionLogsPath);
                            if (!string.IsNullOrEmpty(filePath))
                            {
                                DataTable dtReFrmExl = null;

                                //Fill the Datatable with the Value from Excel & Remove the empty row                    
                                dtReFrmExl = ReadFromExcel(filePath);
                                DataSet ds1 = new DataSet();

                                #region Extra Jobs
                                for (int i = dtReFrmExl.Rows.Count - 1; i >= 0; i--)
                                {
                                    if (!String.IsNullOrEmpty(dtReFrmExl.Rows[i][0].ToString()))
                                    {
                                        continue;
                                    }
                                    dtReFrmExl.Rows[i].Delete();
                                }
                                dtReFrmExl.AcceptChanges();
                                ds1.Tables.Add(dtReFrmExl);
                                #endregion

                                try
                                {
                                    string Fromfile = filePath;
                                    String[] strFileName = fileName.Split('.');
                                    string NewFileName = strFileName[0] + "_inprocess." + strFileName[1];
                                    string Tofile = fileDirectory + "\\" + NewFileName;
                                    //STATUS CHANGE 
                                    bool status = File.Exists(Tofile);
                                    if (status)
                                    {
                                        File.Delete(Tofile);
                                    }
                                    File.Move(Fromfile, Tofile);
                                    fileName = NewFileName;
                                    filePath = Tofile;
                                }
                                catch (Exception ex)
                                {
                                    //DALClass.ErrLogs(ex);
                                    DALClass.InsertExceptionLogs("", "", "BAL-FileRead.cs", "GetFlightCrdAndStatus-TROG", "Error", ex, "");
                                }

                                if (ServiceRequest.ToLower() == "false")
                                {
                                    CreateDirectoryForDownload(dtReFrmExl, folderName, fileName, filePath, fileDirectory, companyName);// use for service testing file download and excel
                                }

                                if (dtReFrmExl != null && ServiceRequest.ToLower() == "true")
                                {
                                    dsFRst = lCC_API_Req.JsonResponsePost_Star("Star", ds1, dt2).Result;
                                }
                                if (dsFRst.Tables.Count > 0)
                                {
                                    //Added by Hitesh just addon a parameter
                                    bool result = UnflownPNRDetails(dsFRst, fileName, crdId, UserName, flight);
                                    if (result)
                                    {
                                        //DALClass.ErrLogs("flight : " + flight + " , PNR data have been successfully inserted");
                                        DALClass.WriteExceLogs("", "", "BAL-FileRead.cs", "GetFlightCrdAndStatus", "Custom-Error", "", "", "PNR data have been successfully inserted");
                                    }
                                    if (dsFRst.Tables[0].Rows.Count > 0)
                                    {
                                        //ExportData(dsFRst);
                                        string DownloadPath = DownloadFolderPath; // System.Configuration.ConfigurationManager.AppSettings["Download"].ToString();
                                        DataTable dtRes = new DataTable();
                                        dtRes = dsFRst.Tables[0];
                                        CreateDirectoryForDownload(dtRes, folderName, fileName, filePath, fileDirectory, companyName);
                                    }
                                    else
                                    {
                                        DALClass.WriteExceLogs("", "", "BAL-FileRead.cs", "GetFlightCrdAndStatus", "Custom-Error", "", "", "Go to else part - Not getting any rows in service response dataset table.");
                                        if (ServiceRequest.ToLower() != "false")
                                        {
                                            bool statusEx = RenameExcelFileName(folderName, fileName, filePath, fileDirectory);
                                            bool statusFolder = CheckRenameFolderStatus(folderName, fileName, filePath, fileDirectory);
                                        }
                                    }
                                }
                                else
                                {
                                    DALClass.WriteExceLogs("", "", "BAL-FileRead.cs", "GetFlightCrdAndStatus", "Custom-Error", "", "", "Go to else part - Service response dataset is null or empty.");
                                    if (ServiceRequest.ToLower() != "false")
                                    {
                                        bool statusEx = RenameExcelFileName(folderName, fileName, filePath, fileDirectory);
                                        bool statusFolder = CheckRenameFolderStatus(folderName, fileName, filePath, fileDirectory);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            sErrMsg = ex.Message;
                            //DALClass.ErrLogs(ex);
                            DALClass.WriteExceLogs("", "", "BAL-FileRead.cs", "GetFlightCrdAndStatus", "Excetion-Handling", ex.Message, ex.StackTrace, "Exception occurred during calling Star Request and Response method(GetFlightCrdAndStatus)");
                        }

                    }
                    else if (flight.Trim().ToUpper() == "S9")
                    {
                        string TCCode = string.Empty;
                        string LogsTrackID = string.Empty;
                        string TransactionProcess = string.Empty;
                        string GPNR = string.Empty;
                        DataSet dsFRst = new DataSet();
                        try
                        {
                            //First Step
                            dt2 = objDal.GetFltCrdAndAirLineName("CRD", flight.Trim(), crdId.Trim(), companyName);
                            if (dt2.Rows.Count > 0)
                            {
                                UID = dt2.Rows[0]["UserId"].ToString();
                                PWD = dt2.Rows[0]["Password"].ToString();
                                Dom = dt2.Rows[0]["LoginID"].ToString();
                                exp1 = dt2.Rows[0]["Exprs1"].ToString();
                                exp2 = dt2.Rows[0]["Exprs2"].ToString();
                            }

                            //Second Step

                            // Intiate Process flow of Star web services
                            LCC_API_Req_Res lCC_API_Req = new LCC_API_Req_Res(UID, PWD, Dom, CV, Surl, Burl, flight.Trim().ToUpper(), Constr, ErrorTextFilePath, LogType, Logstatus, ErrorLogFolderPath, companyName, ExceptionLogsPath);
                            if (!string.IsNullOrEmpty(filePath))
                            {
                                DataTable dtReFrmExl = null;

                                //Fill the Datatable with the Value from Excel & Remove the empty row                    
                                dtReFrmExl = ReadFromExcel(filePath);
                                DataSet ds1 = new DataSet();

                                #region Extra Jobs
                                for (int i = dtReFrmExl.Rows.Count - 1; i >= 0; i--)
                                {
                                    if (!String.IsNullOrEmpty(dtReFrmExl.Rows[i][0].ToString()))
                                    {
                                        continue;
                                    }
                                    dtReFrmExl.Rows[i].Delete();
                                }
                                dtReFrmExl.AcceptChanges();
                                ds1.Tables.Add(dtReFrmExl);
                                #endregion

                                try
                                {
                                    string Fromfile = filePath;
                                    String[] strFileName = fileName.Split('.');
                                    string NewFileName = strFileName[0] + "_inprocess." + strFileName[1];
                                    string Tofile = fileDirectory + "\\" + NewFileName;
                                    //STATUS CHANGE 
                                    bool status = File.Exists(Tofile);
                                    if (status)
                                    {
                                        File.Delete(Tofile);
                                    }
                                    File.Move(Fromfile, Tofile);
                                    fileName = NewFileName;
                                    filePath = Tofile;
                                }
                                catch (Exception ex)
                                {
                                    //DALClass.ErrLogs(ex);
                                    DALClass.InsertExceptionLogs("", "", "BAL-FileRead.cs", "GetFlightCrdAndStatus-S9", "Error", ex, "");
                                }

                                if (ServiceRequest.ToLower() == "false")
                                {
                                    CreateDirectoryForDownload(dtReFrmExl, folderName, fileName, filePath, fileDirectory, companyName);// use for service testing file download and excel
                                }

                                if (dtReFrmExl != null && ServiceRequest.ToLower() == "true")
                                {
                                    dsFRst = lCC_API_Req.JsonResponsePost_Flybig("Flybig", ds1, dt2).Result;
                                }
                                if (dsFRst.Tables.Count > 0)
                                {
                                    //Added by Hitesh just addon a parameter
                                    bool result = UnflownPNRDetails(dsFRst, fileName, crdId, UserName, flight);
                                    if (result)
                                    {
                                        //DALClass.ErrLogs("flight : " + flight + " , PNR data have been successfully inserted");
                                        DALClass.WriteExceLogs("", "", "BAL-FileRead.cs", "GetFlightCrdAndStatus", "Custom-Error", "", "", "PNR data have been successfully inserted");
                                    }
                                    if (dsFRst.Tables[0].Rows.Count > 0)
                                    {
                                        //ExportData(dsFRst);
                                        string DownloadPath = DownloadFolderPath; // System.Configuration.ConfigurationManager.AppSettings["Download"].ToString();
                                        DataTable dtRes = new DataTable();
                                        dtRes = dsFRst.Tables[0];
                                        CreateDirectoryForDownload(dtRes, folderName, fileName, filePath, fileDirectory, companyName);
                                    }
                                    else
                                    {
                                        DALClass.WriteExceLogs("", "", "BAL-FileRead.cs", "GetFlightCrdAndStatus", "Custom-Error", "", "", "Go to else part - Not getting any rows in service response dataset table.");
                                        if (ServiceRequest.ToLower() != "false")
                                        {
                                            bool statusEx = RenameExcelFileName(folderName, fileName, filePath, fileDirectory);
                                            bool statusFolder = CheckRenameFolderStatus(folderName, fileName, filePath, fileDirectory);
                                        }
                                    }
                                }
                                else
                                {
                                    DALClass.WriteExceLogs("", "", "BAL-FileRead.cs", "GetFlightCrdAndStatus", "Custom-Error", "", "", "Go to else part - Service response dataset is null or empty.");
                                    if (ServiceRequest.ToLower() != "false")
                                    {
                                        bool statusEx = RenameExcelFileName(folderName, fileName, filePath, fileDirectory);
                                        bool statusFolder = CheckRenameFolderStatus(folderName, fileName, filePath, fileDirectory);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            sErrMsg = ex.Message;
                            DAL.InsertExceptionLogs("", "", "BAL-FileRead.cs", "GetFlightCrdAndStatus", "Error", ex, "");
                        }

                    }
                    #endregion

                }
            }
            catch (Exception ex)
            {
                DAL.InsertExceptionLogs(UserName, paramList, "IndigoSpiceGoAir.cs", "GetFlightCrdAndStatus", "Error", ex, "Exception occurred during get the all files from directory(ReadFileName)");
            }
            return msg;
        }
        public DataTable PnrDetails()
        {
            DataTable PNRDataTable = new DataTable();
            DataColumn PNRDataColumn = null;
            PNRDataColumn = new DataColumn();
            PNRDataColumn.DataType = Type.GetType("System.Int32");
            PNRDataColumn.ColumnName = "Index";
            PNRDataTable.Columns.Add(PNRDataColumn);

            PNRDataColumn = new DataColumn();
            PNRDataColumn.DataType = Type.GetType("System.String");
            PNRDataColumn.ColumnName = "PNR";
            PNRDataTable.Columns.Add(PNRDataColumn);

            PNRDataColumn = new DataColumn();
            PNRDataColumn.DataType = Type.GetType("System.String");
            PNRDataColumn.ColumnName = "TotalPax";
            PNRDataTable.Columns.Add(PNRDataColumn);

            PNRDataColumn = new DataColumn();
            PNRDataColumn.DataType = Type.GetType("System.String");
            PNRDataColumn.ColumnName = "FlightStatus";
            PNRDataTable.Columns.Add(PNRDataColumn);

            PNRDataColumn = new DataColumn();
            PNRDataColumn.DataType = Type.GetType("System.String");
            PNRDataColumn.ColumnName = "Airline";
            PNRDataTable.Columns.Add(PNRDataColumn);

            PNRDataColumn = new DataColumn();
            PNRDataColumn.DataType = Type.GetType("System.String");
            PNRDataColumn.ColumnName = "Status";
            PNRDataTable.Columns.Add(PNRDataColumn);

            PNRDataColumn = new DataColumn();
            PNRDataColumn.DataType = Type.GetType("System.String");
            PNRDataColumn.ColumnName = "Description";
            PNRDataTable.Columns.Add(PNRDataColumn);

            PNRDataColumn = new DataColumn();
            PNRDataColumn.DataType = Type.GetType("System.String");
            PNRDataColumn.ColumnName = "LocationCode";
            PNRDataTable.Columns.Add(PNRDataColumn);

            PNRDataColumn = new DataColumn();
            PNRDataColumn.DataType = Type.GetType("System.String");
            PNRDataColumn.ColumnName = "SupplierType";
            PNRDataTable.Columns.Add(PNRDataColumn);

            PNRDataColumn = new DataColumn();
            PNRDataColumn.DataType = Type.GetType("System.String");
            PNRDataColumn.ColumnName = "VendorName";
            PNRDataTable.Columns.Add(PNRDataColumn);

            return PNRDataTable;
        }
    }
}
