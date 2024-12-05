using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL_WindowServiceReconciliation
{
    public class FlightCommon
    {
        public class FlightCityList
        {
            public string City { get; set; }
            public string AirportCode { get; set; }
            public string AirportName { get; set; }
            public string CountryCode { get; set; }
            public string Trip { get; set; }

        }

        public class AirlineList
        {
            public string AlilineName { get; set; }
            public string AirlineCode { get; set; }
            public string Trip { get; set; }
            public string PCatagoryCode { get; set; }

        }

        public class FltSrvChargeList
        {
            public string AirlineCode { get; set; }
            public decimal SrviceTax { get; set; }
            public decimal TransactionFee { get; set; }
            public int IATACommissiom { get; set; }
        }
        public class CredentialList
        {
            public string TAID { get; set; }
            public string TAUSERID { get; set; }
            public string TAPASSWORD { get; set; }
            public string LOGINID { get; set; }
            public string PASSWORD { get; set; }
            public string URL { get; set; }
            public string PCatagoryCode { get; set; }
            public string SUPPLIERCode { get; set; }
            public string AirCode { get; set; }
            public string SupplierType { get; set; }
            public string Exprs1 { get; set; }
            public string Exprs2 { get; set; }
            public string Exprs3 { get; set; }
            public string Exprs4 { get; set; }
            public string Exprs5 { get; set; }
            public string Exprs6 { get; set; }
            public bool IsOwnPcc { get; set; }
            public string IdType { get; set; }
        }
    }
}
