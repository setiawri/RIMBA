using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RIMBA
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            setupControls();

            if (!Page.IsPostBack)
            {
                txtUsername.Focus();
            }
        }

        private void setupControls()
        {
            txtUsername.MaxLength = 30;
            txtPassword.MaxLength = 30;
        }

        protected void btnLogin_OnClick(object sender, EventArgs e)
        {
            DBUtil.sanitize(txtUsername, txtPassword);
            if(string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                Page.SetFocus(txtUsername);
                message.error("Silahkan lengkapi username");
                return;
            }
            else if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                Page.SetFocus(txtPassword);
                message.error("Silahkan lengkapi password");
                return;
            } 

            UserAccount userAccount = new UserAccount(txtUsername.Text);
            if(userAccount == null)
            {
                message.error("Invalid username atau password");
                Page.SetFocus(txtUsername);
            }
            else if (!userAccount.isCorrectPassword(txtPassword.Text))
            {
                message.error("Invalid username atau password");
                Page.SetFocus(txtPassword);
            }
            else
            {
                userAccount.redirectToOriginalPage();
                Response.Redirect(Request.QueryString["returnUrl"]);
            }
        }
    }
}