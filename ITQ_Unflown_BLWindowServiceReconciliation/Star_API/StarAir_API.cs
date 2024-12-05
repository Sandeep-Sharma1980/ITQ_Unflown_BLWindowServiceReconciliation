using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;


namespace BL_WindowServiceReconciliation.Star_API
{
    public class StarAir_API
    {
        BAL objBal = new BAL();
        string Constr;
        string ErrorTextFile;
        private string LogType;
        private string Logstatus;
        private string ErrorlogFolderPath;
        private string CompanyName = "Balmer Lawrie";
        string ExceptionLogsPath = string.Empty;
        string sErrMsg = string.Empty;
        public async Task<DataSet> JsonResponsePost_Star(string Airline, DataTable ds1, DataTable dt2)
        {
            int taskCount = 0;
            int Count = 0;
            DAL objDal = new DAL(Constr, ErrorlogFolderPath, ExceptionLogsPath, LogType, Logstatus);
            DataSet FnlPNR = new DataSet();
            DataTable dtPNR = PnrDetails();
            ConcurrentBag<Task<Task<DataTable>>> bag = new ConcurrentBag<Task<Task<DataTable>>>();
            FnlPNR.Tables.Add(dtPNR);
            string GPNR = string.Empty, ServiceUid = string.Empty, airlineCode = string.Empty, airlineName = string.Empty;

            int index = 0;
            string airline = string.Empty;
            try
            {
                string errorMsg = string.Empty;
                ServiceUid = dt2.Rows[0]["TAUSERID"].ToString();
                string PWD = dt2.Rows[0]["TAPASSWORD"].ToString();
                string Dom = dt2.Rows[0]["LOGINID"].ToString();
                string exp1 = dt2.Rows[0]["Expr1"].ToString();
                string exp2 = dt2.Rows[0]["Expr2"].ToString();
                airlineCode = dt2.Rows[0]["AIRCODE"].ToString();                

                DotRezStarService objreq = new DotRezStarService(Constr);

                int countt = ds1.Rows.Count;
                try
                {
                    for (int i = 0; i < countt; i++)
                    {
                        string status = "Failure", seat_no = string.Empty, departuredate = string.Empty;
                        string description = string.Empty, serr_msg = string.Empty;
                        int unflown = 0, totalPax = 0, paxCount = 0;
                        IList<string> paxdetailItemList = null;
                        IList<string> routedetailItemList = null;
                        bool departuredateflag = false, seatnoflag = false;
                        GPNR = ds1.Rows[i][0].ToString();
                        DataRow Air_row = dtPNR.NewRow();
                        string requestID = exp1.ToString();
                        string retrieveUrl = exp1.Replace("{0}", exp2).Replace("{1}", airlineCode).Replace("{2}", GPNR);
                        string retrieveResponse = DotRezStarService.StarPostJson(retrieveUrl, "POST", "");
                        if (!string.IsNullOrEmpty(retrieveResponse))
                        {
                            JObject ObjDivdResponse = JObject.Parse(retrieveResponse);
                            string errorCode = (string)ObjDivdResponse["err_code"];
                            try
                            {
                                string sbook_code = (string)ObjDivdResponse["book_code"];

                                if (errorCode == "0")
                                {
                                    if (ObjDivdResponse["pax_list"] != null)
                                    {
                                        dynamic paxList = ObjDivdResponse["pax_list"];
                                        totalPax = paxList.Count;
                                        if (totalPax > 0)
                                        {
                                            foreach (var item in paxList)
                                            {
                                                JArray listt = (JArray)item;
                                                paxdetailItemList = listt.Select(c => (string)c).ToList();
                                                if (paxdetailItemList != null && paxdetailItemList.Count > 0)
                                                {
                                                    if (paxdetailItemList.Count > 10)
                                                    {
                                                        seat_no = paxdetailItemList[7];
                                                        if (seat_no == "0" || seat_no == null || seat_no == "")
                                                        {
                                                            seatnoflag = true;
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            paxCount++;
                                                        }
                                                    }
                                                    if (paxCount < totalPax)
                                                    {
                                                        unflown = 1;
                                                        seatnoflag = true;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (ObjDivdResponse["route_info"] != null)
                                    {
                                        dynamic routeinfoList = ObjDivdResponse["route_info"];
                                        int counter = routeinfoList.Count;
                                        if (counter > 0)
                                        {
                                            foreach (var item in routeinfoList)
                                            {
                                                JArray listt = (JArray)item;
                                                routedetailItemList = listt.Select(c => (string)c).ToList();
                                                if (routedetailItemList != null && routedetailItemList.Count > 0)
                                                {
                                                    if (routedetailItemList.Count > 4)
                                                    {
                                                        departuredate = routedetailItemList[2];
                                                        if (DateTime.Parse(departuredate.ToString()) < DateTime.Now)
                                                        {
                                                            departuredateflag = true;
                                                            unflown = 1;
                                                            status = "success";
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            //PaxSeats
                                            if (departuredateflag == true && seatnoflag == true)
                                            {
                                                Air_row = dtPNR.NewRow();
                                                index = index + 1;
                                                Air_row["PNR"] = GPNR;
                                                Air_row["TotalPax"] = totalPax;
                                                Air_row["FlightStatus"] = unflown;
                                                Air_row["Status"] = status;
                                                Air_row["Description"] = "";
                                                Air_row["Airline"] = Airline;
                                                Air_row["Index"] = index;
                                                Air_row["LocationCode"] = dt2.Rows[0]["Location"].ToString();
                                                Air_row["SupplierType"] = dt2.Rows[0]["SUPPLIERTYPE"].ToString();
                                                Air_row["VendorName"] = dt2.Rows[0]["VendorName"].ToString();
                                                dtPNR.Rows.Add(Air_row);
                                            }
                                            else if (seatnoflag == true)
                                            {
                                                Air_row = dtPNR.NewRow();
                                                index = index + 1;
                                                Air_row["PNR"] = GPNR;
                                                Air_row["TotalPax"] = totalPax;
                                                Air_row["FlightStatus"] = unflown;
                                                Air_row["Status"] = status;
                                                Air_row["Description"] = "";
                                                Air_row["Airline"] = Airline;
                                                Air_row["Index"] = index;
                                                Air_row["LocationCode"] = dt2.Rows[0]["Location"].ToString();
                                                Air_row["SupplierType"] = dt2.Rows[0]["SUPPLIERTYPE"].ToString();
                                                Air_row["VendorName"] = dt2.Rows[0]["VendorName"].ToString();
                                                dtPNR.Rows.Add(Air_row);
                                            }
                                            else if (departuredateflag == true)
                                            {
                                                Air_row = dtPNR.NewRow();
                                                index = index + 1;
                                                Air_row["PNR"] = GPNR;
                                                Air_row["TotalPax"] = totalPax;
                                                Air_row["FlightStatus"] = unflown;
                                                Air_row["Status"] = status;
                                                Air_row["Description"] = "";
                                                Air_row["Airline"] = Airline;
                                                Air_row["Index"] = index;
                                                Air_row["LocationCode"] = dt2.Rows[0]["Location"].ToString();
                                                Air_row["SupplierType"] = dt2.Rows[0]["SUPPLIERTYPE"].ToString();
                                                Air_row["VendorName"] = dt2.Rows[0]["VendorName"].ToString();
                                                dtPNR.Rows.Add(Air_row);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    serr_msg = (string)ObjDivdResponse["err_msg"];
                                    description = serr_msg;
                                    Air_row = dtPNR.NewRow();
                                    index = index + 1;
                                    Air_row["PNR"] = GPNR;
                                    Air_row["TotalPax"] = totalPax;
                                    Air_row["FlightStatus"] = unflown;
                                    Air_row["Status"] = status;
                                    Air_row["Description"] += description;
                                    Air_row["Airline"] = Airline;
                                    Air_row["Index"] = index;
                                    Air_row["LocationCode"] = dt2.Rows[0]["Location"].ToString();
                                    Air_row["SupplierType"] = dt2.Rows[0]["SUPPLIERTYPE"].ToString();
                                    Air_row["VendorName"] = dt2.Rows[0]["VendorName"].ToString();
                                    dtPNR.Rows.Add(Air_row);
                                }
                            }
                            catch (Exception ex)
                            {
                                Air_row = dtPNR.NewRow();
                                index = index + 1;
                                Air_row["PNR"] = GPNR;
                                Air_row["TotalPax"] = "";
                                Air_row["FlightStatus"] = "";
                                Air_row["Status"] = "Fail";
                                Air_row["Description"] = ex.Message + "------" + ex.StackTrace;
                                Air_row["Airline"] = "";
                                Air_row["Index"] = index;
                                Air_row["LocationCode"] = dt2.Rows[0]["Location"].ToString();
                                Air_row["SupplierType"] = dt2.Rows[0]["SUPPLIERTYPE"].ToString();
                                Air_row["VendorName"] = dt2.Rows[0]["VendorName"].ToString();
                                dtPNR.Rows.Add(Air_row);
                                DAL.InsertExceptionLogs("", "", "BAL-LCC_API_Req_Res.cs", "JSONResponsePost_Star", "Error", ex, "Exception occurred during calling JSONResponsePost_Star method");
                            }

                        }
                        else
                        {
                            Air_row = dtPNR.NewRow();
                            index = index + 1;
                            Air_row["PNR"] = GPNR;
                            Air_row["TotalPax"] = totalPax;
                            Air_row["FlightStatus"] = unflown;
                            Air_row["Status"] = status;
                            Air_row["Description"] += "Not a valid response";
                            Air_row["Airline"] = "";
                            Air_row["Index"] = index;
                            Air_row["LocationCode"] = dt2.Rows[0]["Location"].ToString();
                            Air_row["SupplierType"] = dt2.Rows[0]["SUPPLIERTYPE"].ToString();
                            Air_row["VendorName"] = dt2.Rows[0]["VendorName"].ToString();
                            dtPNR.Rows.Add(Air_row);
                        }
                        objDal.InsertAndWriteReqResLogs(CompanyName, airlineCode, "", ServiceUid, GPNR, "", "", retrieveUrl, retrieveResponse, "", "", "", Airline, "");
                    }
                }
                catch (Exception ex)
                {
                    DataRow Air_row = dtPNR.NewRow();
                    index = index + 1;
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
                    DAL.InsertExceptionLogs("", "", "BAL-LCC_API_Req_Res.cs", "JSONResponsePost_Star", "Error", ex, "Exception occurred during calling JSONResponsePost_Star method");
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
                DAL.InsertExceptionLogs("", "", "BAL-LCC_API_Req_Res.cs", "Logon", "Error", ex, "Exception occurred during calling Get_Booking_Details method");
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
