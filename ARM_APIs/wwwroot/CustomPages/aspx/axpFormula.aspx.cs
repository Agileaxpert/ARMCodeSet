using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

public partial class CustomPages_aspx_axpFormula : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    [WebMethod]
    public static string axpFormulaKeywords(string transid)
    {
        try
        {
            if (HttpContext.Current.Session["project"] == null)
            {
                return "";
            }
            //select vtype, display_caption, display_value, field_name  from vw_axpformulawords where transid = 'tffre' order by 1 desc
            string result = string.Empty;
            string sqlInput = "select vtype, display_caption, display_value, field_name  from vw_axpformulawords where transid = '" + transid + "' order by 1 desc";
            ASBExt.WebServiceExt asbExt = new ASBExt.WebServiceExt();
            result = asbExt.ExecuteSQL("", sqlInput, "JSON");

            return result;
        }
        catch (Exception ex)
        {
            return "";
        }

    }

    [WebMethod]
    public static string axpFormulaValidate(string transid, string fieldName, string formula)
    {
        try
        {
            if (HttpContext.Current.Session["project"] == null)
            {
                return "";
            }
            //select fn_validate_axformula('transid', 'fieldname', 'formula')
            //select fn_validate_axformula('tffre','axpformula_accrual','dob-doc') -- success
            //select fn_validate_axformula('tffre','axpformula_accrual','dob-dod') -- error

            string result = string.Empty;
            string sqlInput = "select fn_validate_axformula('" + transid + "', '" + fieldName + "', '" + formula +"')";
            ASBExt.WebServiceExt asbExt = new ASBExt.WebServiceExt();
            result = asbExt.ExecuteSQL("", sqlInput, "JSON"); 
            return result;
        }
        catch (Exception ex)
        {
            return "";
        }
        
    }
}