using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Data;
using System.Data.SqlClient;
using System.Web.Security;

namespace RIMBA
{
    public partial class Default : System.Web.UI.Page
    {
        private const string ERROR_NOPRIVILEGE = "Anda tidak mempunya akses. Harap hubungi administrator.";

        protected void Page_Init(object sender, EventArgs e)
        {
            if (!UserAccount.IsAuthenticated)
            {
                Response.Redirect("~/Login.aspx?returnUrl=" + Request.RawUrl);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
                populatePageData();
        }

        private void populatePageData()
        {
            string sql = @"
                DECLARE @ROCount int, @POCount int, @OPCount int, @SOCount int, @SICount int, @PICount int

                SELECT @ROCount = ISNULL(COUNT(RequestOrderLog.No),0)
                FROM [DWSystem].[RequestOrderLog] RequestOrderLog
                WHERE RequestOrderLog.[Approved] = 0 AND RequestOrderLog.[Rejected] = 0 AND (RequestOrderLog.ConfirmedWarehouse = 1 AND RequestOrderLog.ConfirmedPPIC = 1);

                SELECT @POCount = ISNULL(COUNT(PurchaseOrder.PurchaseOrderID),0)
                FROM [DWSystem].PurchaseOrder
                    LEFT OUTER JOIN [DWSystem].POSlittingList ON POSlittingList.PurchaseOrderID = PurchaseOrder.PurchaseOrderID
                WHERE POSlittingList.[PurchaseOrderID] IS NULL 
                    AND PurchaseOrder.[Approved] = 0 
                    AND PurchaseOrder.[Rejected] = 0;

                SELECT @PICount = ISNULL(COUNT(PurchaseInvoice.PurchaseInvoiceID),0)
                FROM [DWSystem].PurchaseInvoice
                WHERE PurchaseInvoice.[Approved] = 0 AND PurchaseInvoice.[Rejected] = 0

                SELECT @OPCount = ISNULL(COUNT(ProductionOrder.ProductionOrderID),0)
                FROM [DWSystem].ProductionOrder
                WHERE ProductionOrder.[Approved] = 0 AND ProductionOrder.[Rejected] = 0

                SELECT @SOCount = ISNULL(COUNT(SalesOrder.SalesOrderID),0)
                FROM [DWSystem].SalesOrder
                WHERE SalesOrder.[Approved] = 0 AND SalesOrder.[Rejected] = 0

                SELECT @SICount = ISNULL(COUNT(SalesInvoice.SalesInvoiceID),0)
                FROM [DWSystem].SalesInvoice
                WHERE SalesInvoice.[Approved] = 0 AND SalesInvoice.[Rejected] = 0

                SELECT @ROCount AS ROCount, @POCount AS POCount, @OPCount AS OPCount, @SOCount AS SOCount, @SICount AS SICount, @PICount AS PICount;
            ";
            DataTable datatable = DBUtil.getData(sql);

            lblROCount.Text = datatable.Rows[0]["ROCount"].ToString();
            //if (lblROCount.Text == "0") lbtnRO.Enabled = false;
            lblPOCount.Text = datatable.Rows[0]["POCount"].ToString();
            //if (lblPOCount.Text == "0") lbtnPO.Enabled = false;
            lblJOCount.Text = datatable.Rows[0]["OPCount"].ToString();
            //if (lblJOCount.Text == "0") lbtnJO.Enabled = false;
            lblSOCount.Text = datatable.Rows[0]["SOCount"].ToString();
            //if (lblSOCount.Text == "0") lbtnSO.Enabled = false;
            lblSICount.Text = datatable.Rows[0]["SICount"].ToString();
            lblPICount.Text = datatable.Rows[0]["PICount"].ToString();
        }

        protected void lbtnRO_Click(object sender, EventArgs e)
        {
            if (Tools.getCookieData<bool>(GlobalVariables.COOKIEDATA_CANAPPROVERO))
                Response.Redirect("~/RO");
            else
                message.error(ERROR_NOPRIVILEGE);
        }

        protected void lbtnPO_Click(object sender, EventArgs e)
        {
            if (Tools.getCookieData<bool>(GlobalVariables.COOKIEDATA_CANAPPROVEPO))
                Response.Redirect("~/PO");
            else
                message.error(ERROR_NOPRIVILEGE);
        }

        protected void lbtnPI_Click(object sender, EventArgs e)
        {
            if (Tools.getCookieData<bool>(GlobalVariables.COOKIEDATA_CANAPPROVEPI))
                Response.Redirect("~/PI");
            else
                message.error(ERROR_NOPRIVILEGE);
        }

        protected void lbtnJO_Click(object sender, EventArgs e)
        {
            if (Tools.getCookieData<bool>(GlobalVariables.COOKIEDATA_CANAPPROVEJO))
                Response.Redirect("~/JO");
            else
                message.error(ERROR_NOPRIVILEGE);
        }

        protected void lbtnSO_Click(object sender, EventArgs e)
        {
            if (Tools.getCookieData<bool>(GlobalVariables.COOKIEDATA_CANAPPROVESO))
                Response.Redirect("~/SO");
            else
                message.error(ERROR_NOPRIVILEGE);
        }

        protected void lbtnSI_Click(object sender, EventArgs e)
        {
            if (Tools.getCookieData<bool>(GlobalVariables.COOKIEDATA_CANAPPROVESI))
                Response.Redirect("~/SI");
            else
                message.error(ERROR_NOPRIVILEGE);
        }
    }
}