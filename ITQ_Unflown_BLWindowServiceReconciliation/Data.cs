using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BL_WindowServiceReconciliation.FlightCommon;

namespace BL_WindowServiceReconciliation
{
    public class Data
    {
        static List<FlightCityList> _listofCity;
        static List<AirlineList> _listofAirline;
        static List<FltSrvChargeList> _ServiceTax;
        static List<CredentialList> _CredentialList;
        /// <summary>
        /// Service Tax,Tran.,IATAComm
        /// </summary>
        /// <param name="jrnyType">Domestic or Internation(D/I)</param>
        /// <param name="connectionString">connectionString</param>
        /// <returns>List of charges</returns>       
    }
}
