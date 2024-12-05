using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace BL_WindowServiceReconciliation
{
    public class AkasaAir_API
    {
        BAL objBal = new BAL();        
        string Constr;
        string ErrorTextFile;
        private string LogType;
        private string Logstatus;
        private string ErrorlogFolderPath;
        private string CompanyName= "Balmer Lawrie";
        string ExceptionLogsPath = string.Empty;
        string sErrMsg = string.Empty;
        public async Task<DataSet> XMLResponsePost_Akasa(string Airline, DataTable ds1, DataTable dt2)
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
            DAL objDal = new DAL(Constr, ErrorlogFolderPath, ExceptionLogsPath, LogType, Logstatus);
            try
            {
                string errorMsg = string.Empty;

                string UID = dt2.Rows[0]["TAUSERID"].ToString();
                string PWD = dt2.Rows[0]["TAPASSWORD"].ToString();
                string Dom = dt2.Rows[0]["LOGINID"].ToString();
                string exp1 = dt2.Rows[0]["Expr1"].ToString();
                string exp2 = dt2.Rows[0]["Expr2"].ToString();

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
                            if(!string.IsNullOrEmpty(description))
                            {
                                description = description.Remove(description.Length - 1);
                            }
                            else
                            {
                                description = "";
                            }
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
