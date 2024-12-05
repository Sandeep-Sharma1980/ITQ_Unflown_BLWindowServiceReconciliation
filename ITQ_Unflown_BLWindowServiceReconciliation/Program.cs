using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Data;


namespace BL_WindowServiceReconciliation
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            //Get Credential from service Table
            BAL objBAL = new BAL();
            string vendorName = "Balmer Lawrie";
            DataSet usrCredenDT = new DataSet();
            usrCredenDT = objBAL.GetBLCredential(vendorName);

            //Get the credential for 1G for all the Branch
            DataTable BLTab_1G = new DataTable();
            if (usrCredenDT.Tables[0].Rows.Count > 0)
            {
                BLTab_1G = usrCredenDT.Tables[0];
                foreach (DataRow dr in BLTab_1G.Rows)
                {
                    BLCredential objCre = new BLCredential();
                    objCre.TAID = dr.ItemArray[1].ToString();
                    objCre.UserID = dr.ItemArray[2].ToString();
                    objCre.Password = dr.ItemArray[3].ToString();
                    objCre.URL = dr.ItemArray[6].ToString();
                    objCre.SupplierType = dr.ItemArray[8].ToString();
                    objCre.AirCode = dr.ItemArray[9].ToString();
                    objCre.Expr1 = dr.ItemArray[10].ToString();
                    objCre.Expr2 = dr.ItemArray[11].ToString();
                    objCre.Expr3 = dr.ItemArray[12].ToString();
                    objCre.IsActive = dr.ItemArray[13].ToString();
                    objCre.Location = dr.ItemArray[17].ToString();
                    objCre.VendorName = dr.ItemArray[18].ToString();


                    // Get The PNR On the Basis of Branch Location Code
                    DataTable PnrOnLocTab = new DataTable();
                    PnrOnLocTab = objBAL.GetPnrOnBranchLocCode(objCre);

                    // Check PNR On Location Table has data or not
                    if (PnrOnLocTab.Rows.Count > 0)
                    {
                        //Call UAPI for all BranchLocation Code
                        DataRow[] rslt = BLTab_1G.Select("Location = " + objCre.Location);
                        DataTable newDataTab = new DataTable();
                        newDataTab = BLTab_1G.Clone();
                        foreach (DataRow row in rslt)
                        {
                            newDataTab.Rows.Add(row.ItemArray);
                        }
                        APIs obj1 = new APIs();
                        DataSet dsFRst = new DataSet();
                        dsFRst = obj1.XMLResponsePost_UAPI("UAPI", PnrOnLocTab, newDataTab).Result;
                    }
                }
            }
            //Get the Credential for LCC for all the Airline
            DataTable BLTab_LCC = new DataTable();
            if (usrCredenDT.Tables[1].Rows.Count > 0)
            {
                BLTab_LCC = usrCredenDT.Tables[1];
                foreach (DataRow dr in BLTab_LCC.Rows)
                {
                    BLCredential objCre = new BLCredential();
                    objCre.TAID = dr.ItemArray[1].ToString();
                    objCre.UserID = dr.ItemArray[2].ToString();
                    objCre.Password = dr.ItemArray[3].ToString();
                    objCre.URL = dr.ItemArray[6].ToString();
                    objCre.SupplierType = dr.ItemArray[8].ToString();
                    objCre.AirCode = dr.ItemArray[9].ToString();
                    objCre.Expr1 = dr.ItemArray[10].ToString();
                    objCre.Expr2 = dr.ItemArray[11].ToString();
                    objCre.Expr3 = dr.ItemArray[12].ToString();
                    objCre.IsActive = dr.ItemArray[13].ToString();
                    objCre.Location = dr.ItemArray[17].ToString();
                    objCre.VendorName = dr.ItemArray[18].ToString();

                    // Get The PNR On the Basis of Branch Location Code
                    DataTable PnrOnLocTab = new DataTable();
                    PnrOnLocTab = objBAL.GetPnrOnBranchLocCode(objCre);

                    // Check PNR On Location Table has data or not
                    if (PnrOnLocTab.Rows.Count > 0)
                    {
                        //Call LCC for all BranchLocation Code
                        DataRow[] rslt = BLTab_LCC.Select("Location = '" + objCre.Location + "' AND AIRCODE = '" + objCre.AirCode + "'");
                        //DataRow[] row = datatablename.Select("KEY = '" + SKEY + "' AND COLUMN_NAME = '" + item[0].tostring() + "'");
                        DataTable newDataTab = new DataTable();
                        newDataTab = BLTab_LCC.Clone();
                        foreach (DataRow row in rslt)
                        {
                            newDataTab.Rows.Add(row.ItemArray);
                        }
                        string Airline = String.Empty;
                        Airline = Get_AirlineName(objCre.AirCode);                       
                       
                        DataSet dsFRst = new DataSet();
                        if(Airline== "Indigo(6E)" || Airline == "SpiceJet(SG)" || Airline == "GoAir(G8)")
                        {
                            //IndigoSpiceGoAir_API.IndigoSpiceGoAir_API obj1 = new IndigoSpiceGoAir_API.IndigoSpiceGoAir_API();
                            //dsFRst = obj1.XMLResponsePost_IndigoSpiceGoAir(Airline, PnrOnLocTab, newDataTab).Result;
                        }
                        else if(Airline == "AirAsia(AK)")
                        {
                            AirAsia_API.AirAsia_API obj1 = new AirAsia_API.AirAsia_API();
                            dsFRst = obj1.XMLResponsePost_AirAsia(Airline, PnrOnLocTab, newDataTab).Result;
                        }
                        else if (Airline == "AkasaAir(QP)")
                        {
                            AkasaAir_API obj1 = new AkasaAir_API();
                            dsFRst = obj1.XMLResponsePost_Akasa(Airline, PnrOnLocTab, newDataTab).Result;
                        }                       
                        else if (Airline == "StarAirways(S5)")
                        {
                           Star_API.StarAir_API obj1 = new Star_API.StarAir_API();
                           dsFRst = obj1.JsonResponsePost_Star(Airline, PnrOnLocTab, newDataTab).Result;
                        }
                        else
                        {

                        }
                    }
                }
            }
        }

        public static string Get_AirlineName(string aircode)
        {
            string airlineName = string.Empty;
            switch (aircode)
            {
                case "2T":
                    airlineName="TrueJet(2T)";
                    break;
                case "6E":
                    airlineName = "Indigo(6E)";
                    break;
                case "9I":
                    airlineName = "AllianceAir(9I)";
                    break;
                case "AK":
                    airlineName = "AirAsia(AK)";
                    break;
                case "G8":
                    airlineName = "GoFirst(G8)";
                    break;
                case "QP":
                    airlineName = "AkasaAir(QP)";
                    break;
                case "S5":
                    airlineName = "StarAirways(S5)";
                    break;
                case "SG":
                    airlineName = "SpiceJet(SG)";
                    break;
            }
            return airlineName;
        }
    }
}
