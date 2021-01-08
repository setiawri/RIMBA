using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Web.Security;

namespace RIMBA
{
    public partial class Site : System.Web.UI.MasterPage
    {
        protected string username = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (UserAccount.IsAuthenticated)
            {
                username = Tools.getCookieData<string>(GlobalVariables.COOKIEDATA_USERNAME);
            }
        }
        
        protected void lbtnLogout_Click(object sender, EventArgs e) 
        {
            FormsAuthentication.SignOut();
            Response.Redirect("~/");
        }

        protected string getCompanyName()
        {
            return Settings.CompanyName;
        }
    }
}