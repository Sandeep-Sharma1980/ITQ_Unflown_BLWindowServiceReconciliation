using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL_WindowServiceReconciliation.AirAsia_API
{
    public class DotRezAirAsiaRequest
    {
        string Connection = "";
        public DotRezAirAsiaRequest(string Conecc)
        {
            Connection = Conecc;
        }
        internal string GenerateToken(string LoginID, string loginPass, string domain)
        {
            return "{ \"credentials\": { \"username\": \"" + LoginID + "\", \"password\": \"" + loginPass + "\", \"domain\": \"" + domain + "\"}}";
        }
    }
}
