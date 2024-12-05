using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace BL_WindowServiceReconciliation
{
    public class UAPIXmlParser
    {
        public DataSet BuildXmlElementToDataSetE(XmlElement objXmlRequest1)
        {
            DataTable dtable = new DataTable();
            DataSet ds = new DataSet();
            try
            {
                if (objXmlRequest1.ChildNodes.Count > 0)
                {
                    string xmlData = string.Empty;
                    for (int i = 0; i < objXmlRequest1.ChildNodes.Count; i++)
                    {
                        if (objXmlRequest1.ChildNodes[i].HasChildNodes)
                        {
                            for (int j = 0; j < objXmlRequest1.ChildNodes[i].ChildNodes.Count; j++)
                            {
                                xmlData = objXmlRequest1.ChildNodes[i].ChildNodes[j].OuterXml;
                                BuildXMLElementDTE(xmlData, ref ds);
                            }
                        }
                        else
                        {
                            xmlData = objXmlRequest1.ChildNodes[i].InnerXml;
                            BuildXMLElementDTE(xmlData, ref ds);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                //DALClass.ErrLogs(ex); 
                DAL.InsertExceptionLogs("", "", "BAL-UAPIXmlParse.cs", "BuildXmlElementToDataSetE", "Error", ex, "");
            }
            return ds;
        }
        public DataSet BuildXMLElementDTE(string xmlData, ref DataSet ds)
        {
            try
            {
                XElement processXelement = new XElement("Process");
                Boolean isInsert = false;
                XElement x = XElement.Load(new StringReader(xmlData));//get your file
                if (!x.HasElements)
                {
                    ConvertToXtoDT(x, ref ds);
                    return ds;
                }
                Boolean islastNode1 = true;
                foreach (XElement x1 in x.Descendants())
                {
                    if (x1.HasElements)
                    {
                        foreach (XElement setups in x1.Descendants())
                        {
                            if (!setups.HasElements)
                            {
                                processXelement = setups;
                                ConvertToXtoDT(processXelement, ref ds);
                                isInsert = true;
                                continue;
                            }
                            Boolean islastNode = true;//bool isLastNRead = false;
                            foreach (XElement xg in setups.Descendants())
                            {
                                isInsert = false;
                                if (xg.HasElements)
                                {
                                    islastNode = false;
                                    #region //2nd level
                                    foreach (XElement xe in xg.Descendants())
                                    {
                                        islastNode = true;
                                        if (!xe.HasElements)
                                        {
                                            processXelement = xe;
                                            ConvertToXtoDT(processXelement, ref ds);
                                            isInsert = true;
                                            continue;
                                        }
                                        foreach (XElement xg2 in xe.Descendants())
                                        {
                                            if (xg2.HasElements)
                                            {
                                                islastNode = false;
                                            }
                                            else { processXelement = xg2; }
                                        }
                                        if (islastNode)
                                        {
                                            ConvertToXtoDT(processXelement, ref ds); continue;
                                        }
                                        foreach (XElement xe2 in xe.Descendants())
                                        {
                                            islastNode = true;
                                            if (!xe2.HasElements)
                                            {
                                                processXelement = xe2;
                                            }
                                            foreach (XElement xe3 in xe2.Descendants())
                                            {
                                                if (xe3.HasElements)
                                                {
                                                    islastNode = false;
                                                }
                                                else { processXelement = xe3; }
                                            }
                                            if (islastNode)
                                            {
                                                ConvertToXtoDT(processXelement, ref ds); continue;
                                            }
                                            islastNode = true;
                                            foreach (XElement xe3 in xe2.Nodes())
                                            {
                                                if (!xe3.HasElements)
                                                {
                                                    processXelement = xe3;
                                                }
                                                foreach (XElement xe4 in xe3.Descendants())
                                                {
                                                    if (!xe4.HasElements)
                                                    {
                                                        processXelement = xe4;
                                                    }
                                                    if (xe4.HasElements)
                                                    {
                                                        islastNode = false;
                                                    }
                                                    else { processXelement = xe4; }
                                                }
                                                if (islastNode)
                                                {
                                                    ConvertToXtoDT(processXelement, ref ds); continue;
                                                }
                                            }
                                        }
                                    }
                                    #endregion                                    
                                }
                                else
                                {
                                    processXelement = xg;
                                    ConvertToXtoDT(processXelement, ref ds);
                                    isInsert = true;
                                    continue;
                                }
                            }
                            if (islastNode && !isInsert)
                            {
                                ConvertToXtoDT(processXelement, ref ds);
                                continue;
                            }

                        }
                    }
                    if (islastNode1)
                    {
                        ConvertToXtoDT(x, ref ds);
                    }

                }
            }
            catch (Exception ex)
            {
                //DALClass.ErrLogs(ex); 
                DAL.InsertExceptionLogs("", "", "BAL-UAPIXmlParse.cs", "BuildXMLElementDTE", "Error", ex, "");
            }
            return ds;
        }
        public DataSet BuildXmlElementToDataSet(XmlElement objXmlRequest1)
        {
            DataTable dtable = new DataTable();
            DataSet ds = new DataSet();
            try
            {
                if (objXmlRequest1.ChildNodes.Count > 0)
                {
                    string xmlData = string.Empty;
                    for (int i = 0; i < objXmlRequest1.ChildNodes.Count; i++)
                    {
                        if (objXmlRequest1.ChildNodes[i].HasChildNodes)
                        {
                            for (int j = 0; j < objXmlRequest1.ChildNodes[i].ChildNodes.Count; j++)
                            {
                                xmlData = objXmlRequest1.ChildNodes[i].ChildNodes[j].OuterXml;
                                BuildXMLElementDT(xmlData, ref ds);
                            }
                        }
                        else
                        {
                            xmlData = objXmlRequest1.ChildNodes[i].InnerXml;
                            BuildXMLElementDT(xmlData, ref ds);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                //DALClass.ErrLogs(ex);
                DAL.InsertExceptionLogs("", "", "BAL-UAPIXmlParse.cs", "BuildXmlElementToDataSet", "Error", ex, "");
            }
            return ds;
        }
        public DataSet BuildXMLElementDT(string xmlData, ref DataSet ds)
        {
            try
            {
                XElement processXelement = new XElement("Process");
                XElement x = XElement.Load(new StringReader(xmlData));//get your file
                if (!x.HasElements)
                {
                    ConvertToXtoDT(x, ref ds);
                    return ds;
                }
                Boolean islastNode1 = true;//bool isLastNRead = false;
                bool isattributeProcess = false;
                foreach (XElement x1 in x.Descendants())
                {
                    isattributeProcess = false;
                    if (x1.HasElements)
                    {
                        if (!isattributeProcess)
                        {
                            ConvertToXtoDT(x1, ref ds);
                        }
                        isattributeProcess = true;
                        islastNode1 = false;
                    }
                    if (islastNode1)
                    {
                        if (!xmlData.Contains("<common_v39_0:ResponseMessage "))
                        {
                            ConvertToXtoDT(x, ref ds); return ds;
                        }
                    }
                    if (x1.HasElements)
                    {
                        foreach (XElement setups in x1.Nodes())
                        {
                            if (!setups.HasElements)
                            {
                                processXelement = setups;
                            }
                            Boolean islastNode = true;//bool isLastNRead = false;
                            foreach (XElement xg in setups.Descendants())
                            {
                                if (xg.HasElements)
                                {
                                    islastNode = false;
                                }
                                else
                                {
                                    processXelement = xg;
                                }
                            }
                            if (islastNode)
                            {
                                ConvertToXtoDT(processXelement, ref ds);

                                continue;
                            }
                            foreach (XElement xe in setups.Nodes())
                            {
                                islastNode = true;
                                if (!xe.HasElements)
                                {
                                    processXelement = xe;
                                }
                                foreach (XElement xg in xe.Descendants())
                                {
                                    if (xg.HasElements)
                                    {
                                        islastNode = false;
                                    }
                                    else { processXelement = xg; }
                                }
                                if (islastNode)
                                {
                                    ConvertToXtoDT(processXelement, ref ds); continue;
                                }
                                foreach (XElement xe2 in xe.Descendants())
                                {
                                    islastNode = true;
                                    if (!xe2.HasElements)
                                    {
                                        processXelement = xe2;
                                    }
                                    foreach (XElement xe3 in xe2.Descendants())
                                    {
                                        if (xe3.HasElements)
                                        {
                                            islastNode = false;
                                            // break;
                                        }
                                        else { processXelement = xe3; }
                                    }
                                    if (islastNode)
                                    {
                                        ConvertToXtoDT(processXelement, ref ds); continue;
                                    }
                                    islastNode = true;
                                    foreach (XElement xe3 in xe2.Nodes())
                                    {
                                        if (!xe3.HasElements)
                                        {
                                            processXelement = xe3;
                                        }
                                        foreach (XElement xe4 in xe3.Descendants())
                                        {
                                            if (!xe4.HasElements)
                                            {
                                                processXelement = xe4;
                                            }
                                            if (xe4.HasElements)
                                            {
                                                islastNode = false;
                                                //break;
                                            }
                                            else { processXelement = xe4; }
                                        }
                                        if (islastNode)
                                        {
                                            ConvertToXtoDT(processXelement, ref ds); continue;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //DALClass.ErrLogs(ex);
                DAL.InsertExceptionLogs("", "", "BAL-UAPIXmlParse.cs", "BuildXMLElementDT", "Error", ex, "");
            }
            return ds;
        }
        private DataSet ConvertToXtoDT(XElement xe3, ref DataSet ds)
        {

            int cCount = 0;
            Boolean isHasAttribute = false;
            DataTable dtl = null;
            int strF = 0; int strL = 0;
            Dictionary<string, string> dicConditions = new Dictionary<string, string>();
            try
            {
                if (xe3.Name.ToString().Contains('}'))
                {
                    strF = xe3.Name.ToString().IndexOf('}');
                    strL = xe3.Name.ToString().Length;
                    dtl = new DataTable(xe3.Name.ToString().Substring(strF + 1, strL - strF - 1));
                    string strNames = xe3.Name.ToString().Substring(strF + 1, strL - strF - 1);
                }
                else
                {
                    dtl = new DataTable(xe3.Name.ToString());

                }
                foreach (XAttribute xe4 in xe3.Attributes())
                {
                    DataColumnCollection columns = dtl.Columns;
                    isHasAttribute = true;
                    if (!columns.Contains(xe4.Name.ToString()))
                    {
                        dtl.Columns.Add(new DataColumn(xe4.Name.ToString(), typeof(string))); // add columns to your dt
                    }
                }
                if (!xe3.HasAttributes && !isHasAttribute)
                {
                    DataColumnCollection columns = dtl.Columns;
                    if (xe3.Name.ToString().Contains('}'))
                    {
                        strF = xe3.Name.ToString().IndexOf('}');
                        strL = xe3.Name.ToString().Length;
                        string strName = xe3.Name.ToString().Substring(strF + 1, strL - strF - 1);
                        if (!columns.Contains(strName))
                        {
                            dtl.Columns.Add(new DataColumn(strName, typeof(string))); // add columns to your dt
                        }
                    }
                    else if (!columns.Contains(xe3.Name.ToString()))
                    {
                        dtl.Columns.Add(new DataColumn(xe3.Name.ToString(), typeof(string))); // add columns to your dt
                    }
                }
                DataRow dr1 = null;
                foreach (XAttribute xe4 in xe3.Attributes())
                {

                    if (cCount == 0)
                    {
                        dr1 = dtl.NewRow();
                    }
                    cCount++;
                    DataColumnCollection columns = dtl.Columns;
                    if (columns.Contains(xe4.Name.ToString()))
                    {
                        dr1[xe4.Name.ToString()] = xe4.Value;
                        dicConditions.Add(xe4.Name.ToString(), xe4.Value);
                    }
                    if (cCount == dtl.Columns.Count)
                    {
                        if (dr1 != null)
                            dtl.Rows.Add(dr1);
                        cCount = 0;
                    }
                }
                if (!xe3.HasAttributes && !isHasAttribute)
                {
                    if (cCount == 0)
                    {
                        dr1 = dtl.NewRow();
                    }
                    cCount++;
                    DataColumnCollection columns = dtl.Columns;
                    if (xe3.Name.ToString().Contains('}'))
                    {
                        strF = xe3.Name.ToString().IndexOf('}');
                        strL = xe3.Name.ToString().Length;
                        string strName = xe3.Name.ToString().Substring(strF + 1, strL - strF - 1);
                        if (columns.Contains(strName))
                        {
                            dr1[strName] = xe3.Value; // add columns to your dt
                            dicConditions.Add(strName, xe3.Value);
                        }
                    }
                    else if (columns.Contains(xe3.Name.ToString()))
                    {
                        dr1[xe3.Name.ToString()] = xe3.Value;
                        dicConditions.Add(xe3.Name.ToString(), xe3.Value);
                    }
                    if (cCount == dtl.Columns.Count)
                    {
                        if (dr1 != null)
                            dtl.Rows.Add(dr1);
                        cCount = 0;
                    }
                }
                if (dtl != null)
                {
                    if (dtl.Rows.Count > 0)
                    {
                        if (ds.Tables.Contains(dtl.TableName))
                        {
                            if (dicConditions.ContainsKey(ds.Tables[dtl.TableName].Columns[0].ToString()))
                            {
                                var result = ds.Tables[dtl.TableName].Rows.OfType<DataRow>().Where(r => dicConditions.All(d => r[d.Key].ToString() == d.Value));
                                try
                                {
                                    if (result != null && result.Count() == 0)
                                    {
                                        ds.Tables[dtl.TableName].ImportRow(dtl.Rows[0]);
                                    }
                                }
                                catch
                                {

                                }
                            }
                        }
                        else
                        {
                            ds.Tables.Add(dtl);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //DALClass.ErrLogs(ex);
                DAL.InsertExceptionLogs("", "", "BAL-UAPIXmlParse.cs", "ConvertToXtoDT", "Error", ex, "");
                string strmsg = ex.Message;
            }
            return ds;
        }
        public DataSet ConvertXMLToDataSet(string xmlData)
        {
            StringReader stream = null;
            XmlTextReader reader = null;
            try
            {
                DataSet xmlDS = new DataSet();
                stream = new StringReader(xmlData);
                // Load the XmlTextReader from the stream
                reader = new XmlTextReader(stream);
                xmlDS.ReadXml(reader);
                return xmlDS;
            }
            catch (Exception ex)
            {
                //DALClass.ErrLogs(ex);
                DAL.InsertExceptionLogs("", "", "BAL-UAPIXmlParse.cs", "ConvertXMLToDataSet", "Error", ex, "");
                return null;
            }
            finally
            {
                if (reader != null) reader.Close();
            }
        }
    }
}
