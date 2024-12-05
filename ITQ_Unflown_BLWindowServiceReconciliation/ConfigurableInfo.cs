using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;

namespace BL_WindowServiceReconciliation
{
    public class ConfigurableInfo
    {
        public static string apiKey = "";
        public static string datasetKey = "";
        public static string employeeApiUrl = "";
        public static string username = "";
        public static string password = "";
        public static string logPath = "";
        public static string MailServer = "";
        public static string MailFrom = "";
        public static string ToEmail = "";
        public static string CcEmail = "";
        public static string BccEmail = "";
        public static string logPathnew = "";

        static ConfigurableInfo()
        {
            BAL obj = new BAL();
            DataTable dt = obj.GetConfigurableInfo();
            apiKey = dt.Rows[0]["api_key"].ToString();// ConfigurationManager.AppSettings["api_key"].ToString();
            datasetKey = dt.Rows[0]["datasetKey"].ToString();//ConfigurationManager.AppSettings["datasetKey"].ToString();
            employeeApiUrl = dt.Rows[0]["employeeApiUrl"].ToString();// ConfigurationManager.AppSettings["employeeApiUrl"].ToString();
            username = dt.Rows[0]["userId"].ToString();// ConfigurationManager.AppSettings["userId"].ToString();
            password = dt.Rows[0]["_password"].ToString();// ConfigurationManager.AppSettings["password"].ToString();
            logPath = dt.Rows[0]["logPath"].ToString();// ConfigurationManager.AppSettings["logPath"].ToString();
            MailServer = dt.Rows[0]["MailServer"].ToString();
            MailFrom = dt.Rows[0]["MailFrom"].ToString();
            ToEmail = dt.Rows[0]["ToEmail"].ToString();
            CcEmail = dt.Rows[0]["CcEmail"].ToString();
            BccEmail = dt.Rows[0]["BccEmail"].ToString();
            logPathnew = ConfigurationManager.AppSettings["logPathNew"].ToString();
        }
    }
}
