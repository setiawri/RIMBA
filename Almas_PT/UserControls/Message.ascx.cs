using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RIMBA.UserControls
{
    public partial class Message : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            hide();
        }

        public void hide()
        {
            lblMessage.Text = "";
            pnlMessage.Visible = false;
            lblError.Text = "";
            pnlError.Visible = false;
        }

        public bool display(string msg)
        {
            lblMessage.Text = msg;
            pnlMessage.Visible = true;

            return true;
        }

        public bool error(string msg)
        {
            lblError.Text = msg;
            pnlError.Visible = true;

            return false;
        }
    }
}