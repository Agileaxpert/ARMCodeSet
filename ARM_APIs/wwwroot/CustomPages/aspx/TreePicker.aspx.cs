using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class CustomPages_aspx_TreePicker : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string companyCode = Request.QueryString["cc"];
        string hierarchyCode = Request.QueryString["hc"];

        ScriptManager.RegisterStartupScript(this, this.GetType(), "jsparams", string.Format("var companyCode='{0}';var hierarchyCode='{1}';", companyCode, hierarchyCode), true);
    }
}