using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace BL_WindowServiceReconciliation
{
    public class BAL
    {
        DAL newObj = new DAL();
        public DataSet GetBLCredential(string vName)
        {
            DataSet usrCreden = newObj.GetBLCredential(vName);
            return usrCreden;
        }
        public DataTable GetConfigurableInfo()
        {
            DataTable dtConfigurableInfo = newObj.GetConfigurableInfo();
            return dtConfigurableInfo;
        }
        public DataTable GetPNRDatafrmTab()
        {
            DataTable pnrdt = newObj.GetPNRDatafrmTab();
            return pnrdt;
        }
        public DataSet GetUAPIDataAccess(string requestType, string soapRequest, DataTable dtCredential)
        {
            DataSet ds = new DataSet();
            try
            {
                ds = DAL.GetUAPIData(requestType, soapRequest, dtCredential);
            }
            catch (Exception ex)
            {

                DAL.InsertExceptionLogs("", "", "BAL-DALClass.cs", "GetUAPIDataAccess", "Error", ex, "");
            }
            return ds;
        }
        public void InsertAndWriteReqResUapiLogs(string CompanyName, string VC, string userId, string serviceUserId, string PNR, string soapRequest_URR, string soapResponse_URR, string soapRequest_ARD, string soapResponse_ARD, string soapRequest_ARDTNo, string soapResponse_ARDTNo, string err, string reqid, string FromIPAddress)
        {
            newObj.InsertAndWriteReqResUapiLogs(CompanyName, VC, userId, serviceUserId, PNR, soapRequest_URR, soapRequest_ARD, soapResponse_ARD, soapResponse_ARD, soapRequest_ARDTNo, soapResponse_ARDTNo, err, reqid, FromIPAddress);
        }
        public void InsertAndWriteReqResLogs(string CompanyName, string VC, string userId, string serviceUserId, string PNR, string soapRequest_URR, string soapResponse_URR, string soapRequest_ARD, string soapResponse_ARD, string soapRequest_ARDTNo, string soapResponse_ARDTNo, string err, string reqid, string FromIPAddress)
        {
            newObj.InsertAndWriteReqResLogs(CompanyName, VC, userId, serviceUserId, PNR, soapRequest_URR, soapRequest_ARD, soapResponse_ARD, soapResponse_ARD, soapRequest_ARDTNo, soapResponse_ARDTNo, err, reqid, FromIPAddress);
        }

        public static void InsertExceptionLogs(string UserRequestId, string ParameterList, string ClassName, string MethodName, string ErrorInfo, Exception exx, string Remark)
        {
            DAL.InsertExceptionLogs(UserRequestId, ParameterList, ClassName, MethodName, ErrorInfo, exx, Remark);
        }

        public void BulkInsertTab(DataTable dt)
        {
            newObj.BulkInsertTab(dt);
        }

        public void holdPnrTab(string pnr)
        {
            newObj.holdPnrTab(pnr);
        }

        public DataTable GetPnrOnBranchLocCode(BLCredential obj)
        {
            DataTable dt = newObj.GetPnrOnBranchLocCode(obj);
            return dt;
        }

    }
}
