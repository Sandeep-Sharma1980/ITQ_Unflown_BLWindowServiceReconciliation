using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BL_WindowServiceReconciliation
{
    public class APIs
    {
        private string CompanyName= "Balmer Lawrie";
        BAL objBal = new BAL();
        public async Task<DataSet> XMLResponsePost_UAPI(string Airline, DataTable dt1, DataTable dt2)
        {            
            //Implementation of UAPI on 30Nov2023 by Sandeep Sharma
            string soapRequest_URR = string.Empty;
            string soapResponse_URR = string.Empty;
            string errDesc = string.Empty;
            string soapRequest_ARD = string.Empty;
            string soapResponse_ARD = string.Empty;
            string soapRequest_ARDTNo = string.Empty; 
            string soapResponse_ARDTNo = string.Empty;
            string serusrid = string.Empty;
            int taskCount = 0;
            int Count = 0;
            DataSet FnlPNR = new DataSet();
            DataTable dtPNR = PnrDetails();
            ConcurrentBag<Task<Task<DataTable>>> bag = new ConcurrentBag<Task<Task<DataTable>>>();
            FnlPNR.Tables.Add(dtPNR);           
            int countt = dt1.Rows.Count;
            int newCount = 0;
            int index = 0;
            string GPNR = string.Empty;            
            int flgrAgn_URRR = 0;
            int flgrAgn_ARDR = 0;
            int flgrAgn_ARDRT = 0;           
            try
            {
                string errorMsg = string.Empty;
                try
                {
                    for (int k = 0; k < countt; k++)
                    {
                        soapRequest_URR = string.Empty;
                        GPNR = dt1.Rows[k]["GDSPNR"].ToString();

                        //Hold the PNR from the table
                        objBal.holdPnrTab(GPNR);

                        string BranchCode = dt2.Rows[0]["TAID"].ToString();
                        Regex r = new Regex("^[a-zA-Z0-9]*$");
                        if (r.IsMatch(GPNR))
                        {
                            //Universal Record Retrieve Request with GPNR and BranchCode
                            ineligible:
                            soapRequest_URR = "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:univ=\"http://www.travelport.com/schema/universal_v51_0\" xmlns:com=\"http://www.travelport.com/schema/common_v51_0\"><soapenv:Header/><soapenv:Body><univ:UniversalRecordRetrieveReq TargetBranch=\"" + BranchCode + "\" AuthorizedBy=\"user\"><com:BillingPointOfSaleInfo OriginApplication=\"UAPI\" /> <univ:ProviderReservationInfo ProviderCode=\"1G\" ProviderLocatorCode=\"" + GPNR + "\"/></univ:UniversalRecordRetrieveReq></soapenv:Body></soapenv:Envelope>";
                            DataSet dsPNRDetail = objBal.GetUAPIDataAccess("PNR", soapRequest_URR, dt2);
                            if (!dsPNRDetail.Tables.Contains("Error") && dsPNRDetail.Tables.Count != 0)
                            {
                                if (dsPNRDetail.Tables.Contains("Table1"))
                                {
                                    soapResponse_URR = dsPNRDetail.Tables["Table1"].Rows[0][0].ToString();
                                }
                            }
                            if (dsPNRDetail.Tables.Contains("Error"))
                            {
                                index = newCount + 1;
                                DataRow Air_row = dtPNR.NewRow();
                                Air_row["PNR"] = GPNR;                              
                                Air_row["TotalPax"] = "";
                                Air_row["FlightStatus"] = "";                               
                                Air_row["Status"] = "Failure";
                                Air_row["Description"] = dsPNRDetail.Tables["Error"].Rows[0]["ErrorDescription"].ToString();
                                errDesc = dsPNRDetail.Tables["Error"].Rows[0]["ErrorDescription"].ToString();
                                Air_row["Airline"] = "";
                                Air_row["Index"] = index;
                                Air_row["LocationCode"] = dt2.Rows[0]["Location"].ToString();
                                Air_row["SupplierType"] = dt2.Rows[0]["SUPPLIERTYPE"].ToString();
                                Air_row["VendorName"] = dt2.Rows[0]["VendorName"].ToString();                              
                                dtPNR.Rows.Add(Air_row);
                                newCount = index;
                                objBal.InsertAndWriteReqResUapiLogs(CompanyName, Airline, "", serusrid, GPNR, soapRequest_URR, errDesc, "", "", "", "", "", Airline, "");
                            }
                            else if (dsPNRDetail.Tables.Count == 0)
                            {
                                //Need To Run Again
                                if (flgrAgn_URRR == 0)
                                {
                                    flgrAgn_URRR = flgrAgn_URRR++;
                                    goto ineligible;
                                }
                                index = newCount + 1;
                                DataRow Air_row = dtPNR.NewRow();
                                Air_row["PNR"] = GPNR;
                                Air_row["TotalPax"] = "";
                                Air_row["FlightStatus"] = "";                                
                                Air_row["Status"] = "Failure";
                                Air_row["Description"] = "Request Time Out" + " Rerun No of Times : " + flgrAgn_URRR;
                                errDesc = "Need to run again " + GPNR;
                                Air_row["Airline"] = "";
                                Air_row["Index"] = index;
                                Air_row["LocationCode"] = dt2.Rows[0]["Location"].ToString();
                                Air_row["SupplierType"] = dt2.Rows[0]["SUPPLIERTYPE"].ToString();
                                Air_row["VendorName"] = dt2.Rows[0]["VendorName"].ToString();
                                dtPNR.Rows.Add(Air_row);
                                newCount = index;
                                objBal.InsertAndWriteReqResUapiLogs(CompanyName, Airline, "", serusrid, GPNR, soapRequest_URR, errDesc, "", "", "", "", "", Airline, "");
                            }
                            else
                            {
                                try
                                {
                                    if (dsPNRDetail.Tables.Contains("UniversalRecord"))
                                    {
                                        string UniversalRecordLocatorCode = string.Empty;
                                        UniversalRecordLocatorCode = dsPNRDetail.Tables["UniversalRecord"].Rows[0]["LocatorCode"].ToString();
                                        string AirReservationLocatorCode = string.Empty;
                                        AirReservationLocatorCode = dsPNRDetail.Tables["AirReservation"].Rows[0]["LocatorCode"].ToString();
                                        //Air Retrieve Document Request with UniversalRecordLocatorCode,AirReservationLocatorCode,BranchCode and GPNR
                                        ineligible1:
                                        soapRequest_ARD = "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:air=\"http://www.travelport.com/schema/air_v51_0\" xmlns:com=\"http://www.travelport.com/schema/common_v51_0\"><soapenv:Header/><soapenv:Body><air:AirRetrieveDocumentReq TraceId = '" + Guid.NewGuid().ToString() + "' AuthorizedBy = \"user\" TargetBranch = \"" + BranchCode + "\" ReturnPricing = \"true\" UniversalRecordLocatorCode = \"" + UniversalRecordLocatorCode + "\" ProviderCode = \"1G\" ProviderLocatorCode =\"" + GPNR + "\"><com:BillingPointOfSaleInfo OriginApplication = \"UAPI\" /><air:AirReservationLocatorCode>" + AirReservationLocatorCode + "</air:AirReservationLocatorCode></air:AirRetrieveDocumentReq></soapenv:Body></soapenv:Envelope>";
                                        DataSet dsAirRetDoc = objBal.GetUAPIDataAccess("TICKET", soapRequest_ARD, dt2);
                                        if (!dsAirRetDoc.Tables.Contains("Error") && dsAirRetDoc.Tables.Count != 0)
                                        {
                                            if (dsAirRetDoc.Tables.Contains("Table1"))
                                            {
                                                soapResponse_ARD = dsAirRetDoc.Tables["Table1"].Rows[0][0].ToString();
                                            }
                                        }
                                        if (dsAirRetDoc.Tables.Contains("Error"))
                                        {
                                            index = newCount + 1;
                                            DataRow Air_row = dtPNR.NewRow();
                                            Air_row["PNR"] = GPNR;
                                            Air_row["TotalPax"] = "";
                                            Air_row["FlightStatus"] = "";                                           
                                            Air_row["Status"] = "Failure";
                                            Air_row["Description"] = dsAirRetDoc.Tables["Error"].Rows[0]["ErrorDescription"].ToString();
                                            errDesc = dsAirRetDoc.Tables["Error"].Rows[0]["ErrorDescription"].ToString();
                                            Air_row["Airline"] = "";
                                            Air_row["Index"] = index;
                                            dtPNR.Rows.Add(Air_row);
                                            newCount = index;
                                            Air_row["LocationCode"] = dt2.Rows[0]["Location"].ToString();
                                            Air_row["SupplierType"] = dt2.Rows[0]["SUPPLIERTYPE"].ToString();
                                            Air_row["VendorName"] = dt2.Rows[0]["VendorName"].ToString();
                                            objBal.InsertAndWriteReqResUapiLogs(CompanyName, Airline, "", serusrid, GPNR, soapRequest_URR, soapResponse_URR, soapRequest_ARD, errDesc, "", "", "", Airline, "");
                                        }
                                        else if (dsAirRetDoc.Tables.Count == 0)
                                        {
                                            if (flgrAgn_ARDR == 0)
                                            {
                                                flgrAgn_ARDR = flgrAgn_ARDR++;
                                                goto ineligible1;
                                            }
                                            index = newCount + 1;
                                            DataRow Air_row = dtPNR.NewRow();
                                            Air_row["PNR"] = GPNR;
                                            Air_row["TotalPax"] = "";
                                            Air_row["FlightStatus"] = "";                                           
                                            Air_row["Status"] = "Failure";
                                            Air_row["Description"] = "Request Time Out" + " Rerun No of Times : " + flgrAgn_ARDR;
                                            errDesc = "Need to run again " + GPNR;
                                            Air_row["Airline"] = "";
                                            Air_row["Index"] = index;
                                            Air_row["LocationCode"] = dt2.Rows[0]["Location"].ToString();
                                            Air_row["SupplierType"] = dt2.Rows[0]["SUPPLIERTYPE"].ToString();
                                            Air_row["VendorName"] = dt2.Rows[0]["VendorName"].ToString();
                                            dtPNR.Rows.Add(Air_row);
                                            newCount = index;
                                            objBal.InsertAndWriteReqResUapiLogs(CompanyName, Airline, "", serusrid, GPNR, soapRequest_URR, soapResponse_URR, soapRequest_ARD, errDesc, "", "", "", Airline, "");
                                        }
                                        else
                                        {
                                            int tktcount = dsAirRetDoc.Tables["Ticket"].Rows.Count;
                                            for (int m = 0; m < tktcount; m++)
                                            {
                                                string specificdes = string.Empty;
                                                string tktnumber = dsAirRetDoc.Tables["Ticket"].Rows[m]["TicketNumber"].ToString();
                                                //Air Retrieve Document Request with TicketNumber,UniversalRecordLocatorCode and BranchCode
                                                ineligible2:
                                                soapRequest_ARDTNo = "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\"><soapenv:Header/><soapenv:Body><air:AirRetrieveDocumentReq TargetBranch=\"" + BranchCode + "\" AuthorizedBy = \"user\" UniversalRecordLocatorCode = \"" + UniversalRecordLocatorCode + "\" ReturnRestrictions = \"true\" ProviderCode = \"1G\" ReturnPricing = \"true\" xmlns:air=\"http://www.travelport.com/schema/air_v51_0\" xmlns:common=\"http://www.travelport.com/schema/common_v51_0\"><common:BillingPointOfSaleInfo OriginApplication=\"UAPI\"/><common:TicketNumber>" + tktnumber + "</common:TicketNumber></air:AirRetrieveDocumentReq></soapenv:Body></soapenv:Envelope>";
                                                DataSet dsAirRetDocTkt = objBal.GetUAPIDataAccess("TICKET", soapRequest_ARDTNo, dt2);
                                                if (!dsAirRetDocTkt.Tables.Contains("Error") && dsAirRetDocTkt.Tables.Count != 0)
                                                {
                                                    if (dsAirRetDocTkt.Tables.Contains("Table1"))
                                                    {
                                                        soapResponse_ARDTNo = dsAirRetDocTkt.Tables["Table1"].Rows[0][0].ToString();
                                                    }
                                                }
                                                if (dsAirRetDocTkt.Tables.Contains("Error"))
                                                {
                                                    index = newCount + 1;
                                                    DataRow Air_row = dtPNR.NewRow();
                                                    Air_row["PNR"] = GPNR;
                                                    Air_row["TotalPax"] = "";
                                                    Air_row["FlightStatus"] = "";                                                 
                                                    Air_row["Status"] = "Failure";
                                                    Air_row["Description"] = dsAirRetDocTkt.Tables["Error"].Rows[0]["ErrorDescription"].ToString();
                                                    errDesc = dsAirRetDocTkt.Tables["Error"].Rows[0]["ErrorDescription"].ToString();
                                                    Air_row["Airline"] = "";
                                                    Air_row["Index"] = index;
                                                    Air_row["LocationCode"] = dt2.Rows[0]["Location"].ToString();
                                                    Air_row["SupplierType"] = dt2.Rows[0]["SUPPLIERTYPE"].ToString();
                                                    Air_row["VendorName"] = dt2.Rows[0]["VendorName"].ToString();
                                                    dtPNR.Rows.Add(Air_row);
                                                    newCount = index;
                                                    objBal.InsertAndWriteReqResUapiLogs(CompanyName, Airline, "", serusrid, GPNR, soapRequest_URR, soapResponse_URR, soapRequest_ARD, soapResponse_ARD, soapRequest_ARDTNo, errDesc, "", Airline, "");
                                                }
                                                else if (dsAirRetDocTkt.Tables.Count == 0)
                                                {
                                                    if (flgrAgn_ARDRT == 0)
                                                    {
                                                        flgrAgn_ARDRT = flgrAgn_ARDRT++;
                                                        goto ineligible2;
                                                    }
                                                    index = newCount + 1;
                                                    DataRow Air_row = dtPNR.NewRow();
                                                    Air_row["PNR"] = GPNR;
                                                    Air_row["TotalPax"] = "";
                                                    Air_row["FlightStatus"] = "";                                                    
                                                    Air_row["Status"] = "Failure";
                                                    Air_row["Description"] = "Request Time Out" + " Rerun No of Times : " + flgrAgn_ARDRT;
                                                    errDesc = "Need to run again " + GPNR;
                                                    Air_row["Airline"] = "";
                                                    Air_row["Index"] = index;
                                                    Air_row["LocationCode"] = dt2.Rows[0]["Location"].ToString();
                                                    Air_row["SupplierType"] = dt2.Rows[0]["SUPPLIERTYPE"].ToString();
                                                    Air_row["VendorName"] = dt2.Rows[0]["VendorName"].ToString();
                                                    dtPNR.Rows.Add(Air_row);
                                                    newCount = index;
                                                    objBal.InsertAndWriteReqResUapiLogs(CompanyName, Airline, "", serusrid, GPNR, soapRequest_URR, soapResponse_URR, soapRequest_ARD, soapResponse_ARD, soapRequest_ARDTNo, errDesc, "", Airline, "");
                                                }
                                                else
                                                {
                                                    taskCount = 20;
                                                    Count = 1;
                                                    int totTicket = dsAirRetDocTkt.Tables["Ticket"].Rows.Count;
                                                    if (totTicket > 1)
                                                    {
                                                        specificdes = "Congestion Ticket";
                                                    }
                                                    string airline = string.Empty;
                                                    string status = string.Empty;
                                                    for (int i = 0; i < totTicket; i++)
                                                    {
                                                        int totCoupon = dsAirRetDocTkt.Tables["Coupon"].Rows.Count;
                                                        GPNR = GPNR + "_" + dsAirRetDocTkt.Tables["Ticket"].Rows[i]["TicketNumber"].ToString();
                                                        airline = dsAirRetDocTkt.Tables["Coupon"].Rows[i]["MarketingCarrier"].ToString();
                                                        DataRow Air_row = dtPNR.NewRow();
                                                        String TotalPax = totTicket.ToString();
                                                        String StatusCode = dsAirRetDocTkt.Tables["Coupon"].Rows[i]["Status"].ToString();
                                                        DateTime target = DateTime.Parse(dsAirRetDoc.Tables["Coupon"].Rows[i]["DepartureTime"].ToString());
                                                        DateTime today = DateTime.Today;
                                                        string description = string.Empty;
                                                        for (int j = 0; j < totCoupon; j++)
                                                        {
                                                            if (StatusCode == "O")
                                                            {
                                                                if (today > target)
                                                                {
                                                                    status = "Sucess";
                                                                    description = "Open/No Show,";
                                                                    break;
                                                                }
                                                                else
                                                                {
                                                                    status = "Failure";
                                                                    description = description + "Open/Future Date" + ",";
                                                                }
                                                            }
                                                            else
                                                            {
                                                                status = "Failure";
                                                                if (StatusCode == "F")
                                                                {
                                                                    description = description + "Flown/Used" + ",";
                                                                }
                                                                else if (StatusCode == "C")
                                                                {
                                                                    description = description + "Checked In" + ",";
                                                                }
                                                                else if (StatusCode == "A")
                                                                {
                                                                    description = description + "Airport Controlled" + ",";
                                                                }
                                                                else if (StatusCode == "L")
                                                                {
                                                                    description = description + "Boarded/Lifted" + ",";
                                                                }
                                                                else if (StatusCode == "P")
                                                                {
                                                                    description = description + "Printed" + ",";
                                                                }
                                                                else if (StatusCode == "R")
                                                                {
                                                                    description = description + "Refunded" + ",";
                                                                }
                                                                else if (StatusCode == "E")
                                                                {
                                                                    description = description + "Exchanged" + ",";
                                                                }
                                                                else if (StatusCode == "V")
                                                                {
                                                                    description = description + "Void" + ",";
                                                                }
                                                                else if (StatusCode == "Z")
                                                                {
                                                                    description = description + "Archived/Carrier" + ",";
                                                                }
                                                                else if (StatusCode == "U")
                                                                {
                                                                    description = description + "Unavailable" + ",";
                                                                }
                                                                else if (StatusCode == "S")
                                                                {
                                                                    description = description + "Suspended" + ",";
                                                                }
                                                                else if (StatusCode == "I")
                                                                {
                                                                    description = description + "Irregular Ops" + ",";
                                                                }
                                                                else if (StatusCode == "D")
                                                                {
                                                                    description = description + "Deleted/Removed" + ",";
                                                                }
                                                                else if (StatusCode == "X")
                                                                {
                                                                    description = description + "Unknown" + ",";
                                                                }
                                                                else
                                                                {
                                                                    description = description + "Unidentified Status" + ",";
                                                                }
                                                            }
                                                        }
                                                        description = description.Remove(description.Length - 1);
                                                        index = newCount + 1;
                                                        Air_row["PNR"] = GPNR;
                                                        Air_row["TotalPax"] = TotalPax;
                                                        Air_row["FlightStatus"] = StatusCode;                                                        
                                                        Air_row["Status"] = status;
                                                        Air_row["Description"] = description + " " + specificdes;
                                                        Air_row["Airline"] = airline;
                                                        Air_row["Index"] = index;
                                                        Air_row["LocationCode"] = dt2.Rows[0]["Location"].ToString();
                                                        Air_row["SupplierType"] = dt2.Rows[0]["SUPPLIERTYPE"].ToString();
                                                        Air_row["VendorName"] = dt2.Rows[0]["VendorName"].ToString();
                                                        dtPNR.Rows.Add(Air_row);
                                                        string[] subs = GPNR.Split('_');
                                                        GPNR = subs[0];
                                                        newCount = index;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        index = newCount + 1;
                                        DataRow Air_row = dtPNR.NewRow();
                                        Air_row["PNR"] = GPNR;
                                        Air_row["TotalPax"] = "";
                                        Air_row["FlightStatus"] = "";                                     
                                        Air_row["Status"] = "Failure";
                                        Air_row["Description"] = "Request Time Out";
                                        errDesc = "Need to run again " + GPNR;
                                        Air_row["Airline"] = "";
                                        Air_row["Index"] = index;
                                        Air_row["LocationCode"] = dt2.Rows[0]["Location"].ToString();
                                        Air_row["SupplierType"] = dt2.Rows[0]["SUPPLIERTYPE"].ToString();
                                        Air_row["VendorName"] = dt2.Rows[0]["VendorName"].ToString();
                                        dtPNR.Rows.Add(Air_row);
                                        newCount = index;
                                        objBal.InsertAndWriteReqResUapiLogs(CompanyName, Airline, "", serusrid, GPNR, soapRequest_URR, errDesc, "", "", "", "", "", Airline, "");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    index = newCount + 1;
                                    DataRow Air_row = dtPNR.NewRow();
                                    Air_row["PNR"] = GPNR;
                                    Air_row["TotalPax"] = "";
                                    Air_row["FlightStatus"] = "";                                 
                                    Air_row["Status"] = "Failure";
                                    Air_row["Description"] = "Bad Request";
                                    Air_row["Airline"] = "";
                                    Air_row["Index"] = index;
                                    Air_row["LocationCode"] = dt2.Rows[0]["Location"].ToString();
                                    Air_row["SupplierType"] = dt2.Rows[0]["SUPPLIERTYPE"].ToString();
                                    Air_row["VendorName"] = dt2.Rows[0]["VendorName"].ToString();
                                    dtPNR.Rows.Add(Air_row);
                                    newCount = index;
                                    BAL.InsertExceptionLogs("", "", "BAL-LCC_API_Req_Res.cs", "JSONResponsePost_UAPI", "Error", ex, "Exception occurred during calling XMLResponsePost_UAPI method");
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(errDesc))
                        {
                            objBal.InsertAndWriteReqResUapiLogs(CompanyName, Airline, "", serusrid, GPNR, soapRequest_URR, soapResponse_URR, soapRequest_ARD, soapResponse_ARD, soapRequest_ARDTNo, soapResponse_ARDTNo, "", Airline, "");
                        }
                        else
                        {
                            errDesc = string.Empty;
                        }
                    }
                }
                catch (Exception ex)
                {
                    DAL.InsertExceptionLogs("", "", "BAL-LCC_API_Req_Res.cs", "XMLResponsePost_UAPI", "Error", ex, "Exception occurred during calling XMLResponsePost_UAPI method");
                }
                if (taskCount == Count)
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
                                // var hh=  result.Result;
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
                else
                {
                    Count++;
                }
                if (FnlPNR.Tables[0].Rows.Count > 0)
                {
                    //Bulk copy data from DataTable (DataSet) to SQL Server database Table using SqlBulkCopy
                    objBal.BulkInsertTab(FnlPNR.Tables[0]);
                }
                return FnlPNR;
            }
            catch (Exception ex)
            {
                DAL.InsertExceptionLogs("", "", "APIs.cs", "XMLResponsePost_UAPI", "Error", ex, "Exception occurred during calling XMLResponsePost_UAPI method");
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
