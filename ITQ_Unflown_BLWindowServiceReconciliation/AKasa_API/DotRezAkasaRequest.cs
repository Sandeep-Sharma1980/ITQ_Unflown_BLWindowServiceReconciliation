using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL_WindowServiceReconciliation
{
    public class DotRezAkasaRequest
    {
        string Connection = "";
        public DotRezAkasaRequest(string Conecc)
        {
            Connection = Conecc;
        }
        internal string GenerateToken(string LoginID, string loginPass, string domain)
        {
            return "{ \"credentials\": { \"username\": \"" + LoginID + "\", \"password\": \"" + loginPass + "\", \"domain\": \"" + domain + "\"}}";
        }

    }
}
