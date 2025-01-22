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

public partial class TreeConfig : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string companyCode = Request.QueryString["cc"];
        string hierarchyCode = Request.QueryString["hc"];
        string treeCaption = Request.QueryString["caption"];
        string targetDimension = Request.QueryString["target"];
        if (string.IsNullOrEmpty(targetDimension)) {
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

    [WebMethod(EnableSession =true)]
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
			        ""axpapp"": """+ HttpContext.Current.Session["project"].ToString() + @""",
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

        string sql = string.Format("select * from vw_tree_definition_data  where company_code = '{0}' and hierarchy_code='{1}'", companyCode, hierarchyCode);

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
			        ""axpapp"": """+ HttpContext.Current.Session["project"].ToString() + @""",
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

        string sql = string.Format("select * from VW_Dimension_Data where form_name = '{0}' and company_code = '{1}' and hierarchy_code = '{2}'", formName, companyCode, hierarchyCode);

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
			        ""axpapp"": """+ HttpContext.Current.Session["project"].ToString() + @""",
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

        string sql = string.Format("select * from VW_Dimension_Properties where company_code = '{0}' and hierarchy_code = '{1}'", companyCode, hierarchyCode);

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
			        ""axpapp"": """+ HttpContext.Current.Session["project"].ToString() + @""",
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

        string sql = string.Format("call pr_publish_treecore('{0}','{1}')", companyCode, hierarchyCode);

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
			        ""axpapp"": """+ HttpContext.Current.Session["project"].ToString() + @""",
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

        string sql = string.Format("call pr_discard_treecore('{0}','{1}')", companyCode, hierarchyCode);

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
    public static string SaveTree(ArrayList treeData, ArrayList deletedNodes)
    {
        if (HttpContext.Current.Session["project"] == null)
        {
            return "Error: Session is expired. Please re-login.";
        }

        string password = MD5Hash(HttpContext.Current.Session["pwd"].ToString());

        string saveDataStr = @"{
            ""_parameters"": [
                {
                    ""savedata"": {
                         ""axpapp"": """ + HttpContext.Current.Session["project"].ToString() + @""",
			            ""username"":""" + HttpContext.Current.Session["username"].ToString() + @""",
			            ""password"":""" + password + @""",
			            ""s"": """",
                        ""transid"": ""trtmp"",
                        ""trace"": ""false"",
                        ""recordid"": ""$recordid$"",
                        ""changedrows"": {},
                        ""recdata"": [
                            {
                                ""axp_recid1"": [
                                    {
                                        ""rowno"": ""001"",
                                        ""text"": ""$recordid$"",
                                        ""columns"": $treeData$
                                    }
                                ]
                            }
                        ]
                    }
                }
            ]
        }";

        string result = "success";
        foreach (var rowObj in treeData)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(rowObj);
            var rowData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            if (deletedNodes.IndexOf(rowData["recordid"].ToString()) > -1)
            {
                continue;
            }

            string saveRowDataStr = saveDataStr.Replace("$treeData$", json).Replace("$recordid$", rowData["recordid"].ToString());

            API api = new API();
            var apiUrl = ConfigurationManager.AppSettings["SaveDataAPIUrl"].ToString();
            var apiBody = saveRowDataStr;
            var apiResult = Task.Run(async () => api.POSTData(apiUrl, apiBody)).Result;
            var tempResult = apiResult.Result.result.ToString().ToLower();
            if (tempResult.IndexOf("access violation") > -1 || tempResult.IndexOf("error") > -1)
            {
                result = tempResult;
                break;
            }
        }

        if (result != "success")
            return result;

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

        foreach (var node in deletedNodes)
        {            
            string sql = string.Format("delete from global_config_intermediate_treecore where global_config_intermediate_treecoreid = '{0}'", node.ToString());

            API api = new API();
            var apiUrl = ConfigurationManager.AppSettings["GetChoiceAPIUrl"].ToString();
            var apiBody = getChoiceStr.Replace("$SQL$", sql);
            var apiResult = Task.Run(async () => api.POSTData(apiUrl, apiBody)).Result;
            var tempResult = apiResult.Result.result.ToString().ToLower();
            if (tempResult.IndexOf("access violation") > -1 || tempResult.IndexOf("error") > -1)
            {
                result = apiResult.Result.result.ToString();
                break;
            }
        }
        return result;
    }    
}

public class API
{
    internal async Task<AsyncResult> GetData(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                var request = await client.GetAsync(url);

                return new AsyncResult { isSuccess = request.IsSuccessStatusCode, result = await request.Content.ReadAsStringAsync() };
            }
            catch (Exception ex)
            {
                return new AsyncResult { isSuccess = false, result = ex.Message };

            }
        }
    }

    internal async Task<AsyncResult> POSTData(string url, string body)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                var request = await client.PostAsync(new Uri(url), new StringContent(body, Encoding.UTF8, "application/json"));

                return new AsyncResult { isSuccess = request.IsSuccessStatusCode, result = await request.Content.ReadAsStringAsync() };
            }
            catch (Exception ex)
            {
                return new AsyncResult { isSuccess = false, result = ex.Message };

            }
        }
    }
}

public class AsyncResult
{
    public bool isSuccess { get; set; }
    public string result { get; set; }
}