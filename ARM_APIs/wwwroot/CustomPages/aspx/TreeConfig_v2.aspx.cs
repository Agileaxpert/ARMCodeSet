using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Web.Services;
using System.Net.Http;
using System.Security.Cryptography;
using ASBCustom;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using DocumentFormat.OpenXml.Drawing.Charts;

public partial class TreeConfig_v2 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string companyCode = Request.QueryString["cc"];
        string hierarchyCode = Request.QueryString["hc"];
        string treeCaption = Request.QueryString["caption"];
        string targetDimension = Request.QueryString["target"];
        if (string.IsNullOrEmpty(targetDimension))
        {
            targetDimension = "0";
        }
        ScriptManager.RegisterStartupScript(this, this.GetType(), "jsparams", string.Format("var companyCode='{0}';var hierarchyCode='{1}';var treeCaption='{2}';;var targetDimension='{3}';", companyCode, hierarchyCode, treeCaption, targetDimension), true);
    }

    public static string MD5Hash(string text)
    {
        MD5 md5 = new MD5CryptoServiceProvider();
        //compute hash from the bytes of text  
        md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));
        //get hash result after compute it  
        byte[] result = md5.Hash;
        StringBuilder strBuilder = new StringBuilder();
        for (int i = 0; i < result.Length; i++)
        {
            //change it into 2 hexadecimal digits  
            //for each byte  
            strBuilder.Append(result[i].ToString("x2"));
        }
        return strBuilder.ToString();
    }

    [WebMethod(EnableSession = true)]
    public static string GetTreeData(string companyCode, string hierarchyCode)
    {
        if (HttpContext.Current.Session["project"] == null)
        {
            return "Error: Session is expired. Please re-login.";
        }

        string password = MD5Hash(HttpContext.Current.Session["pwd"].ToString());

        string getChoiceStr = @"{
        ""_parameters"": [
	        {
		        ""getchoices"": {
			        ""axpapp"": """ + HttpContext.Current.Session["project"].ToString() + @""",
			        ""username"":""" + HttpContext.Current.Session["username"].ToString() + @""",
			        ""password"":""" + password + @""",
			        ""s"": """",
			        ""sql"": ""$SQL$"",
			        ""direct"": ""false"",
			        ""params"": """"
			        }
		        }
	        ]
        }";

        string sql = string.Format("select * from vw_tree_definition_data_v2  where company_code = '{0}' and hierarchy_code='{1}'", companyCode, hierarchyCode);

        API api = new API();
        var apiUrl = ConfigurationManager.AppSettings["GetChoiceAPIUrl"].ToString();
        var apiBody = getChoiceStr.Replace("$SQL$", sql);
        var apiResult = Task.Run(async () => api.POSTData(apiUrl, apiBody)).Result;
        return apiResult.Result.result.ToString();
    }

    [WebMethod(EnableSession = true)]
    public static string GetDimensionsData(string formName, string companyCode, string hierarchyCode)
    {
        if (HttpContext.Current.Session["project"] == null)
        {
            return "Error: Session is expired. Please re-login.";
        }

        string password = MD5Hash(HttpContext.Current.Session["pwd"].ToString());

        string getChoiceStr = @"{
        ""_parameters"": [
	        {
		        ""getchoices"": {
			        ""axpapp"": """ + HttpContext.Current.Session["project"].ToString() + @""",
			        ""username"":""" + HttpContext.Current.Session["username"].ToString() + @""",
			        ""password"":""" + password + @""",
			        ""s"": """",
			        ""sql"": ""$SQL$"",
			        ""direct"": ""false"",
			        ""params"": """"
			        }
		        }
	        ]
        }";

        string sql = string.Format("select * from VW_Dimension_Data_v2 where form_name = '{0}' and company_code = '{1}' and hierarchy_code = '{2}'", formName, companyCode, hierarchyCode);

        API api = new API();
        var apiUrl = ConfigurationManager.AppSettings["GetChoiceAPIUrl"].ToString();
        var apiBody = getChoiceStr.Replace("$SQL$", sql);
        var apiResult = Task.Run(async () => api.POSTData(apiUrl, apiBody)).Result;
        return apiResult.Result.result.ToString();
    }

    [WebMethod(EnableSession = true)]
    public static string GetDimensionsProperties(string companyCode, string hierarchyCode)
    {
        if (HttpContext.Current.Session["project"] == null)
        {
            return "Error: Session is expired. Please re-login.";
        }

        string password = MD5Hash(HttpContext.Current.Session["pwd"].ToString());

        string getChoiceStr = @"{
        ""_parameters"": [
	        {
		        ""getchoices"": {
			        ""axpapp"": """ + HttpContext.Current.Session["project"].ToString() + @""",
			        ""username"":""" + HttpContext.Current.Session["username"].ToString() + @""",
			        ""password"":""" + password + @""",
			        ""s"": """",
			        ""sql"": ""$SQL$"",
			        ""direct"": ""false"",
			        ""params"": """"
			        }
		        }
	        ]
        }";

        string sql = string.Format("select * from VW_Dimension_Properties_v2 where company_code = '{0}' and hierarchy_code = '{1}'", companyCode, hierarchyCode);

        API api = new API();
        var apiUrl = ConfigurationManager.AppSettings["GetChoiceAPIUrl"].ToString();
        var apiBody = getChoiceStr.Replace("$SQL$", sql);
        var apiResult = Task.Run(async () => api.POSTData(apiUrl, apiBody)).Result;
        return apiResult.Result.result.ToString();
    }

    [WebMethod(EnableSession = true)]
    public static string PublishTreeChanges(string companyCode, string hierarchyCode)
    {
        if (HttpContext.Current.Session["project"] == null)
        {
            return "Error: Session is expired. Please re-login.";
        }

        string password = MD5Hash(HttpContext.Current.Session["pwd"].ToString());

        string getChoiceStr = @"{
        ""_parameters"": [
	        {
		        ""getchoices"": {
			        ""axpapp"": """ + HttpContext.Current.Session["project"].ToString() + @""",
			        ""username"":""" + HttpContext.Current.Session["username"].ToString() + @""",
			        ""password"":""" + password + @""",
			        ""s"": """",
			        ""sql"": ""$SQL$"",
			        ""direct"": ""false"",
			        ""params"": """"
			        }
		        }
	        ]
        }";

        string sql = string.Format("call pr_publish_treecore_v2('{0}','{1}')", companyCode, hierarchyCode);

        API api = new API();
        var apiUrl = ConfigurationManager.AppSettings["GetChoiceAPIUrl"].ToString();
        var apiBody = getChoiceStr.Replace("$SQL$", sql);
        var apiResult = Task.Run(async () => api.POSTData(apiUrl, apiBody)).Result;
        string result = "success";
        var tempResult = apiResult.Result.result.ToString().ToLower();
        if (tempResult.IndexOf("access violation") > -1 || tempResult.IndexOf("error") > -1)
        {
            result = apiResult.Result.result.ToString();
        }
        return result;
    }

    [WebMethod(EnableSession = true)]
    public static string DiscardTreeChanges(string companyCode, string hierarchyCode)
    {
        if (HttpContext.Current.Session["project"] == null)
        {
            return "Error: Session is expired. Please re-login.";
        }

        string password = MD5Hash(HttpContext.Current.Session["pwd"].ToString());

        string getChoiceStr = @"{
        ""_parameters"": [
	        {
		        ""getchoices"": {
			        ""axpapp"": """ + HttpContext.Current.Session["project"].ToString() + @""",
			        ""username"":""" + HttpContext.Current.Session["username"].ToString() + @""",
			        ""password"":""" + password + @""",
			        ""s"": """",
			        ""sql"": ""$SQL$"",
			        ""direct"": ""false"",
			        ""params"": """"
			        }
		        }
	        ]
        }";

        string sql = string.Format("call pr_discard_treecore_v2('{0}','{1}')", companyCode, hierarchyCode);

        API api = new API();
        var apiUrl = ConfigurationManager.AppSettings["GetChoiceAPIUrl"].ToString();
        var apiBody = getChoiceStr.Replace("$SQL$", sql);
        var apiResult = Task.Run(async () => api.POSTData(apiUrl, apiBody)).Result;
        string result = "success";
        var tempResult = apiResult.Result.result.ToString().ToLower();
        if (tempResult.IndexOf("access violation") > -1 || tempResult.IndexOf("error") > -1)
        {
            result = apiResult.Result.result.ToString();
        }
        return result;
    }

    private static string ServiceInputSpecialChars(string value)
    {
        value = value.Replace("&", "&amp;");
        return value;
    }

    [WebMethod(EnableSession = true)]
    public static string SaveTree(string companyCode, string hierarchyCode, ArrayList saveJson)
    {
        if (HttpContext.Current.Session["project"] == null)
        {
            return "Error: Session is expired. Please re-login.";
        }

        string password = MD5Hash(HttpContext.Current.Session["pwd"].ToString());

        string getChoiceStr = @"{
        ""_parameters"": [
	        {
		        ""getchoices"": {
			        ""axpapp"": """ + HttpContext.Current.Session["project"].ToString() + @""",
			        ""username"":""" + HttpContext.Current.Session["username"].ToString() + @""",
			        ""password"":""" + password + @""",
			        ""s"": """",
			        ""sql"": ""$SQL$"",
			        ""direct"": ""false"",
			        ""params"": """"
			        }
		        }
	        ]
        }";

        string strSaveJson = Newtonsoft.Json.JsonConvert.SerializeObject(saveJson);
        strSaveJson = ServiceInputSpecialChars(strSaveJson);

        string sql = string.Format("call pr_intermediate_treecore_save_v2 ('{0}','{1}','{2}', '{3}')", companyCode, hierarchyCode, strSaveJson, "" + HttpContext.Current.Session["username"].ToString() + @"");

        //sql = System.Web.HttpUtility.JavaScriptStringEncode(sql);

        string result = "";
        ASBCustom.CustomWebservice objWeb = new ASBCustom.CustomWebservice();
        try
        {
            result = objWeb.GetChoices("Tree", sql);
        }
        catch (Exception ex)
        {
            throw ex;
        }

        if (result.Contains(Constants.ERROR) == true)
        {
            result = result.Replace(Constants.ERROR, "");
            result = result.Replace("</error>", "");
            result = result.Replace("\n", "");
            //TODO: Exception logging to be done
        }
        return result;
        //string sql = string.Format("call pr_intermediate_treecore_save_v2 ('{0}','{1}','{2}', '{3}')", companyCode, hierarchyCode, Newtonsoft.Json.JsonConvert.SerializeObject(saveJson).Replace("\"", "\\\""), "" + HttpContext.Current.Session["username"].ToString() + @"");

        //API api = new API();
        //var apiUrl = ConfigurationManager.AppSettings["GetChoiceAPIUrl"].ToString();
        //var apiBody = getChoiceStr.Replace("$SQL$", sql);
        //var apiResult = Task.Run(async () => api.POSTData(apiUrl, apiBody)).Result;
        //string result = "success";
        //var tempResult = apiResult.Result.result.ToString().ToLower();
        //if (tempResult.IndexOf("access violation") > -1 || tempResult.IndexOf("error") > -1)
        //{
        //    result = apiResult.Result.result.ToString();
        //}
        //return result;
    }

    public static string ExecuteSQL(string sql) {

        if (HttpContext.Current.Session["project"] == null)
        {
            return "Error: Session is expired. Please re-login.";
        }

        string password = MD5Hash(HttpContext.Current.Session["pwd"].ToString());

        string getChoiceStr = @"{
        ""_parameters"": [
	        {
		        ""getchoices"": {
			        ""axpapp"": """ + HttpContext.Current.Session["project"].ToString() + @""",
			        ""username"":""" + HttpContext.Current.Session["username"].ToString() + @""",
			        ""password"":""" + password + @""",
			        ""s"": """",
			        ""sql"": ""$SQL$"",
			        ""direct"": ""false"",
			        ""params"": """"
			        }
		        }
	        ]
        }";        

        API api = new API();
        var apiUrl = ConfigurationManager.AppSettings["GetChoiceAPIUrl"].ToString();
        var apiBody = getChoiceStr.Replace("$SQL$", sql);
        var apiResult = Task.Run(async () => api.POSTData(apiUrl, apiBody)).Result;
        return apiResult.Result.result.ToString();
    }

    [WebMethod(EnableSession = true)]
    public static string GetAttributes()
    {
        string sql = @"select a.form_name,fieldcaption,field_name, a.form_id
        from global_config_empgrouping a";

        return ExecuteSQL(sql);
    }

    [WebMethod(EnableSession = true)]
    public static string SetAttributes(string groupCode, string selectedAttributes, ArrayList selectedValues)
    {
        string tempData = JsonConvert.SerializeObject(selectedValues);
        tempData = ServiceInputSpecialChars(tempData);

        if (HttpContext.Current.Session["project"] == null)
        {
            return "Error: Session is expired. Please re-login.";
        }

        string password = MD5Hash(HttpContext.Current.Session["pwd"].ToString());

        string sql = string.Format("update global_config_employeegrouping set selected_attributes= '{1}', selected_attributes_value = '{2}' where group_code='{0}'", groupCode, selectedAttributes, tempData);

        string result = "";
        ASBCustom.CustomWebservice objWeb = new ASBCustom.CustomWebservice();
        try
        {
            result = objWeb.GetChoices("EmployeeGroupig", sql);
        }
        catch (Exception ex)
        {
            throw ex;
        }

        if (result.Contains(Constants.ERROR) == true)
        {
            result = result.Replace(Constants.ERROR, "");
            result = result.Replace("</error>", "");
            result = result.Replace("\n", "");
            //TODO: Exception logging to be done
        }
        return result;
    }

    [WebMethod(EnableSession = true)]
    public static string GetSelectedAttributes(string groupCode)
    {
        string sql = string.Format("select group_code,group_description,field_name,form_id,fieldcaption,form_name, order_seq from  vw_employee_group_attributes_properties where group_code='{0}' order by order_seq", groupCode);

        return ExecuteSQL(sql);
    }

    [WebMethod(EnableSession = true)]
    public static string GetAttributeFieldValues(string groupCode)
    {
        string sql = string.Format("select a.group_code,a.group_description, a.form_name,a.form_id, a.field_name,a.fieldcaption,a.order_seq, a.field_value,a.data_type from vw_employee_group_attributes_data a where a.group_code='{0}' and a.field_value is not null and a.field_value <> '' order by a.order_seq,a.field_name,a.form_id", groupCode);

        return ExecuteSQL(sql);
    }

    //[WebMethod(EnableSession = true)]
    //public static string SetSelectedAttributeFieldValues(string groupCode, ArrayList data)
    //{
    //    string tempData = JsonConvert.SerializeObject(data);
    //    tempData = tempData.Replace("\"", "\\\"");
    //    string sql = string.Format("update global_config_employeegrouping set selected_attributes_value= '{1}' where group_code= '{0}'", groupCode, tempData);

    //    return ExecuteSQL(sql);
    //}

    [WebMethod(EnableSession = true)]
    public static string SetSelectedAttributeFieldValues(string groupCode, ArrayList data)
    {
        string tempData = JsonConvert.SerializeObject(data);
        //tempData = tempData.Replace("\"", "\\\"");
        tempData = ServiceInputSpecialChars(tempData);

        if (HttpContext.Current.Session["project"] == null)
        {
            return "Error: Session is expired. Please re-login.";
        }

        string password = MD5Hash(HttpContext.Current.Session["pwd"].ToString());       

        string sql = string.Format("update global_config_employeegrouping set selected_attributes_value= '{1}' where group_code= '{0}'", groupCode, tempData);

        string result = "";
        ASBCustom.CustomWebservice objWeb = new ASBCustom.CustomWebservice();
        try
        {
            result = objWeb.GetChoices("EmployeeGroupig", sql);
        }
        catch (Exception ex)
        {
            throw ex;
        }

        if (result.Contains(Constants.ERROR) == true)
        {
            result = result.Replace(Constants.ERROR, "");
            result = result.Replace("</error>", "");
            result = result.Replace("\n", "");
            //TODO: Exception logging to be done
        }
        return result;
    }


    [WebMethod(EnableSession = true)]
    public static string GetSelectedAttributeFieldValues(string groupCode)
    {
        string sql = string.Format("Select global_config_employeegroupingid, selected_attributes_value from global_config_employeegrouping where group_code= '{0}'", groupCode);

        return ExecuteSQL(sql);
    }

    [WebMethod(EnableSession = true)]
    public static string UpdateOrgAttributeFlag(string attributeId)
    {
        string sql = string.Format("update global_config_addattribute set active=case when active='T' then 'F' else 'T' end where global_config_addattributeid = '{0}'", attributeId);

        return ExecuteSQL(sql);
    }

    [WebMethod(EnableSession = true)]
    public static string GetTreeCoreData(string companyCode, string hierarchyCode)
    {
        if (HttpContext.Current.Session["project"] == null)
        {
            return "Error: Session is expired. Please re-login.";
        }

        string password = MD5Hash(HttpContext.Current.Session["pwd"].ToString());

        string getChoiceStr = @"{
        ""_parameters"": [
	        {
		        ""getchoices"": {
			        ""axpapp"": """ + HttpContext.Current.Session["project"].ToString() + @""",
			        ""username"":""" + HttpContext.Current.Session["username"].ToString() + @""",
			        ""password"":""" + password + @""",
			        ""s"": """",
			        ""sql"": ""$SQL$"",
			        ""direct"": ""false"",
			        ""params"": """"
			        }
		        }
	        ]
        }";

        string sql = string.Format("select * from vw_treecore_definition_data_v2  where company_code = '{0}' and hierarchy_code='{1}'", companyCode, hierarchyCode);

        API api = new API();
        var apiUrl = ConfigurationManager.AppSettings["GetChoiceAPIUrl"].ToString();
        var apiBody = getChoiceStr.Replace("$SQL$", sql);
        var apiResult = Task.Run(async () => api.POSTData(apiUrl, apiBody)).Result;
        return apiResult.Result.result.ToString();
    }

    [WebMethod(EnableSession = true)]
    public static string GetFile(string filePath, string recordId)
    {
        if (HttpContext.Current.Session["project"] == null)
        {
            return "Error: Session is expired. Please re-login.";
        }

        string serverPath = HttpContext.Current.Session["axpimagepath"].ToString();
        string fullFilePath = serverPath + "\\" + filePath.Replace("\\\\", "\\");
        fullFilePath = fullFilePath.Replace(";bkslh", "\\");

        var directory = new System.IO.DirectoryInfo(fullFilePath);
        System.IO.FileInfo[] files;
        if (directory.Exists)
        {
            files = directory.GetFiles(recordId + "*");
            if (files != null && files.Length > 0)
            {
                var tempFileName = files[0].Name;
                var scriptsPath = HttpContext.Current.Application["ScriptsPath"].ToString();
                var scriptsDirectory = new System.IO.DirectoryInfo(scriptsPath);
                var tempSubDir = "Log/" + HttpContext.Current.Session.SessionID + "/" + Guid.NewGuid().ToString();
                scriptsDirectory.CreateSubdirectory(tempSubDir);
                System.IO.File.Copy(files[0].FullName, scriptsDirectory + "/" + tempSubDir + "/" + tempFileName);

                return System.Web.HttpContext.Current.Application["scriptsUrlPath"].ToString() + "/" + tempSubDir + "/" + tempFileName;
            }
        }
        else
            return "File path not exists";


        return "";
    }

    [WebMethod(EnableSession = true)]
    public static string GetSqlData(string sqlName, string sqlParams)
    {
        if (HttpContext.Current.Session["project"] == null)
        {
            return "Error: Session is expired. Please re-login.";
        }

        string proj = HttpContext.Current.Session["project"].ToString();
        string conStr = HttpContext.Current.Session["axconstr"].ToString();
        string strDB = HttpContext.Current.Session["axdb"].ToString();
        string conId = string.Empty;
        string dbSchemaName = "";
        string dbUserName = "";
        if (strDB.ToLower() == "postgresql" || strDB.ToLower() == "postgre")
        {
            for (int i = 0; i < conStr.Split(';').Length; i++)
            {
                string temp = conStr.Split(';')[i];
                if (temp.Contains("Server"))
                    conId = temp.Split('=')[1];
                var tempUserName = HttpContext.Current.Session["dbuser"].ToString();
                dbSchemaName = tempUserName.Split('~')[0];
                dbUserName = tempUserName.Split('~')[1];
            }
        }

        string getSqlDataStr = "";
        if (sqlParams.Trim() == "")
        {

            getSqlDataStr = @"{
            ""_parameters"": [
                {
                    ""getsqldata"": {
                        ""axpapp"": """ + proj + @""",
                        ""sqlname"": """ + sqlName + @""",
                        ""isdropdown"": ""F"",
                        ""trace"": ""false""
                    },
                    """ + proj + @""": {
                        ""type"": ""db"",
                        ""structurl"": """",
                        ""db"": """ + strDB + @""",
                        ""driver"": ""dbx"",
                        ""version"": """",
                        ""dbcon"": """ + conId + @""",
                        ""dbuser"": """ + dbSchemaName + "\\\\" + dbUserName + @""",
                        ""pwd"": """",
                        ""dataurl"": """"
                    }
                }
            ]
        }";

        }
        else
        {
            getSqlDataStr = @"{
            ""_parameters"": [
                {
                    ""getsqldata"": {
                        ""axpapp"": """ + proj + @""",
                        ""sqlname"": """ + sqlName + @""",
                        ""isdropdown"": ""F"",
                        ""trace"": ""false""
                    },
                    """ + proj + @""": {
                        ""type"": ""db"",
                        ""structurl"": """",
                        ""db"": """ + strDB + @""",
                        ""driver"": ""dbx"",
                        ""version"": """",
                        ""dbcon"": """ + conId + @""",
                        ""dbuser"": """ + dbSchemaName + "\\\\" + dbUserName + @""",
                        ""pwd"": """",
                        ""dataurl"": """"
                    },
                    ""sqlparams"":{""" + sqlParams.Replace("$", @"""") + @"""}
                }
            ]
        }";

        }

        API api = new API();
        var apiUrl = ConfigurationManager.AppSettings["GetSqlDataAPIUrl"].ToString();
        var apiResult = Task.Run(async () => api.POSTData(apiUrl, getSqlDataStr)).Result;
        return apiResult.Result.result.ToString();
    }

    [WebMethod(EnableSession = true)]
    public static string SetPersonalInfoGlobalVar(string val)
    {
        if (HttpContext.Current.Session["project"] == null)
        {
            return "Error: Session is expired. Please re-login.";
        }

        if (val == "a" || val == "p" || val == "o") {
            string currVal = HttpContext.Current.Session["m_infodc"].ToString();
            string currGlobalVars = HttpContext.Current.Session["axGlobalVars"].ToString();
            
            HttpContext.Current.Session["axGlobalVars"] = currGlobalVars.Replace("<m_infodc>"+ currVal + "</m_infodc>", "<m_infodc>"+ val + "</m_infodc>");
            HttpContext.Current.Session["m_infodc"] = val;

        }
        return val;
    }

    [WebMethod(EnableSession = true)]
    public static string ReSetPersonalInfoGlobalVar()
    {
        if (HttpContext.Current.Session["project"] == null)
        {
            return "Error: Session is expired. Please re-login.";
        }

        string currVal = HttpContext.Current.Session["m_infodc"].ToString();
        string currGlobalVars = HttpContext.Current.Session["axGlobalVars"].ToString();

        HttpContext.Current.Session["axGlobalVars"] = currGlobalVars.Replace("<m_infodc>" + currVal + "</m_infodc>", "<m_infodc>a</m_infodc>");
        HttpContext.Current.Session["m_infodc"] = "a";

        return "a";
    }

    [WebMethod(EnableSession = true)]
    public static string GetAxConfigJSONs()
    {
        string sql = "select rulename, rulejson from rulet1";

        return ExecuteSQL(sql);
    }

    [WebMethod(EnableSession = true)]
    public static string GetHolidays(String fromDate, String toDate, string employeeCodes)
    {
        string sql = string.Format("select emp_mst_basicinfoid, employee_code,group_code,to_char(holiday_date,'dd-MON-yy') holiday_date,holiday_type_desc,ismandatory,type from vw_employee_holiday_listing where holiday_date >= '{0}' and holiday_date <= '{1}' and employee_code in ({2})", fromDate, toDate,employeeCodes);

        return ExecuteSQL(sql);
    }
}
