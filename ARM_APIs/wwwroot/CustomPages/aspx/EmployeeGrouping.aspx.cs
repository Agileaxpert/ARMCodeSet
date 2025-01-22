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

public partial class EmployeeGrouping : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string groupCode = Request.QueryString["gc"];
        string groupName = Request.QueryString["gn"];
        ScriptManager.RegisterStartupScript(this, this.GetType(), "jsparams", string.Format("var groupCode='{0}';var groupName='{1}';", groupCode, groupName), true);
    }
}
