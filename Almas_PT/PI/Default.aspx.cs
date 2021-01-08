using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace RIMBA.PI
{
    public partial class Default : System.Web.UI.Page
    {
        protected const string PAGETOPIC = "Purchase Invoice";
        protected const string RELATIONNAME = "relation";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack)
            {
                //textbox set to readonly causes value not retained during postback. This is the fix
                Tools.retainTextboxData(Request, txtDate, txtApproveDate);
            }
            else
            {
                populatePage();
            }
        }

        private void populatePage()
        {
            //generate filter
            string filter = "";
            if (chkNeedApprovalOnly.Checked)
                filter = Tools.append(filter, "(PurchaseInvoice.[Approved] = 0 AND PurchaseInvoice.[Rejected] = 0)", " AND ");
            filter = Tools.append(filter, string.Format("(PurchaseInvoice.PurchaseInvoiceID LIKE '%{0}%')", txtNo.Text.Trim()), " AND ");
            if (!string.IsNullOrWhiteSpace(txtDate.Text))
                filter = Tools.append(filter, string.Format("(CONVERT(varchar, PurchaseInvoice.Date, 1) = '{0:MM/dd/yy}')", DateTime.ParseExact(txtDate.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture)), " AND ");
            if (!string.IsNullOrWhiteSpace(txtApproveDate.Text))
                filter = Tools.append(filter, string.Format("(CONVERT(varchar, PurchaseInvoice.ValidatedDate, 1) = '{0:MM/dd/yy}')", DateTime.ParseExact(txtApproveDate.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture)), " AND ");

            //populate repeater
            string sql = string.Format(@"
                SELECT 
                    PurchaseInvoice.PurchaseInvoiceID, PurchaseInvoice.Date, PurchaseInvoice.SubTotal, PurchaseInvoice.UserName, 
                    PurchaseInvoice.Approved, PurchaseInvoice.Rejected, PurchaseInvoice.ValidatedDate,
                    Supplier.Name AS SupplierName,
                    Currency.CurrencySymbol
                FROM [DWSystem].PurchaseInvoice
                    LEFT OUTER JOIN [DWSystem].Supplier ON Supplier.SupplierID = PurchaseInvoice.SupplierID
                    LEFT OUTER JOIN [DWSystem].Currency ON Currency.CurrencyID = PurchaseInvoice.CurrencyID
                WHERE 1=1 AND {2}
                ORDER BY PurchaseInvoice.[Date] ASC, PurchaseInvoice.[PurchaseInvoiceID] ASC;
                
                SELECT 
                    PurchaseInvoiceLog.PurchaseInvoiceID, {0} AS FormattedQuantity, PurchaseInvoiceLog.Cost, PurchaseInvoiceLog.Discount, 
                    PurchaseInvoiceLog.Total, PurchaseInvoiceLog.No,
                    Inventory.InventoryID, Inventory.InventoryName,
                    {1} AS HighestUnitCode
                FROM [DWSystem].PurchaseInvoiceLog
                    LEFT OUTER JOIN [DWSystem].PurchaseInvoice ON PurchaseInvoice.PurchaseInvoiceID = PurchaseInvoiceLog.PurchaseInvoiceID
                    LEFT OUTER JOIN [DWSystem].Inventory ON Inventory.InventoryID = PurchaseInvoiceLog.InventoryID
                    LEFT OUTER JOIN [DWSystem].InventoryUnit U1 ON U1.UnitID = Inventory.UnitID
                    LEFT OUTER JOIN [DWSystem].InventoryUnit U2 ON U2.UnitID = Inventory.SecUnitID
                    LEFT OUTER JOIN [DWSystem].InventoryUnit U3 ON U3.UnitID = Inventory.ThirdUnitID
                WHERE 1=1 AND {2}
                ORDER BY PurchaseInvoiceLog.[No] ASC
            ", DBUtil.getSQLToFormatQty("PurchaseInvoiceLog.Quantity"), DBUtil.getSQLForHighestUnitCode(), filter);
            rptParent.DataSource = DBUtil.getData(sql, RELATIONNAME, "PurchaseInvoiceID");
            rptParent.DataBind();
            if (rptParent.Items.Count == 0)
                message.display("Search tidak menghasilkan data untuk ditampilkan. Silahkan rubah filter dan coba lagi.");
        }

        protected void lbtnClearNo_Click(object sender, EventArgs e)
        {
            txtNo.Text = "";
        }

        protected void lbtnClearApproveDate_Click(object sender, EventArgs e)
        {
            txtApproveDate.Text = "";
        }

        protected void lbtnClearDate_Click(object sender, EventArgs e)
        {
            txtDate.Text = "";
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            populatePage();
        }

        protected void btnApprove_Command(object sender, CommandEventArgs e)
        {
            try
            {
                string sql = @"
                        UPDATE [DWSystem].[PurchaseInvoice] 
                        SET 
                            [Approved] = 1,
                            [Rejected] = 0,
                            [ValidatedDate] = CURRENT_TIMESTAMP,
                            [ValidatedBy] = @UserName,
                            [Web] = 1 
                        WHERE [PurchaseInvoiceID] = @PurchaseInvoiceID";

                using (SqlConnection sqlConnection = new SqlConnection(Settings.ConnectionString))
                using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@PurchaseInvoiceID", SqlDbType.VarChar).Value = e.CommandArgument.ToString();
                    cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = Tools.getCookieData<string>(GlobalVariables.COOKIEDATA_USERNAME);

                    if (sqlConnection.State != ConnectionState.Open)
                        sqlConnection.Open();
                    cmd.ExecuteNonQuery();
                }
                populatePage();
                message.display(PAGETOPIC + " berhasil di approve");
            }
            catch (Exception ex) { message.error(PAGETOPIC + " gagal di approve. Hubungi administrator. Error: " + ex.Message); }
        }

        protected void btnReject_Command(object sender, CommandEventArgs e)
        {
            try
            {
                string sql = @"
                        UPDATE [DWSystem].[PurchaseInvoice] 
                        SET 
                            [Approved] = 0,
                            [Rejected] = 1,
                            [ValidatedDate] = CURRENT_TIMESTAMP,
                            [ValidatedBy] = @UserName,
                            [Web] = 1 
                        WHERE [PurchaseInvoiceID] = @PurchaseInvoiceID";

                using (SqlConnection sqlConnection = new SqlConnection(Settings.ConnectionString))
                using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@PurchaseInvoiceID", SqlDbType.VarChar).Value = e.CommandArgument.ToString();
                    cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = Tools.getCookieData<string>(GlobalVariables.COOKIEDATA_USERNAME);

                    if (sqlConnection.State != ConnectionState.Open)
                        sqlConnection.Open();
                    cmd.ExecuteNonQuery();
                }
                populatePage();
                message.display(PAGETOPIC + " berhasil di reject");
            }
            catch (Exception ex) { message.error(PAGETOPIC + " gagal di reject. Hubungi administrator. Error: " + ex.Message); }
        }

        protected void btnCancelValidation_Command(object sender, CommandEventArgs e)
        {
            try
            {
                string sql = "";
                bool isEligibleToCancel = false;

                using (SqlConnection sqlConnection = new SqlConnection(Settings.ConnectionString))
                {
                    //confirm eligible to cancel
                    sql = @"

                        DECLARE @iApproved as BIT,@iRejected as BIT  
                        SELECT @iApproved = [Approved],@iRejected = [Rejected] 
                        FROM [DWSystem].[PurchaseInvoice] 
                        WHERE [PurchaseInvoiceID] = @PurchaseInvoiceID 

                        DECLARE @iCount as int,@isApproved as bit  

                        IF @iApproved = 1 
                        BEGIN 

                            SELECT @iCount = COUNT(*) 
                            FROM [DWSystem].[PurchaseInvoice] PO,[DWSystem].[PurchaseInvoiceLog] L 
                            WHERE PO.[PurchaseInvoiceID] = L.[PurchaseInvoiceID] 
                                AND PO.[PurchaseInvoiceID] = @PurchaseInvoiceID 
                                AND (PO.[Rejected] = 1 
                                    OR PO.[PurchaseInvoiceID] IN (
                                            SELECT P.[PurchaseInvoiceID] 
                                            FROM [DWSystem].[PurchasingInvoicePayment] P, [DWSystem].[APPayment] A 
                                            WHERE P.[PaymentID] = A.[PaymentID] 
                                                AND A.[Canceled] = 0 
                                                AND P.[PurchaseInvoiceID] = @PurchaseInvoiceID
                                        )
                                ) 

                            SELECT @iCount = @iCount + COUNT(*) 
                            FROM [DWSystem].[BBMLogPurchaseSettlement] P, 
                                [DWSystem].[PurchaseInvoiceLog] L, [DWSystem].[PurchaseInvoice] I 
                            WHERE P.[PILogNo] = L.[No] And L.[PurchaseInvoiceID] = I.[PurchaseInvoiceID] 
                                AND I.[Rejected] = 0 AND I.[PurchaseInvoiceID] <> @PurchaseInvoiceID 
                                AND P.[BBMLogNo] IN (
                                        SELECT P2.[BBMLogNo] 
                                        FROM [DWSystem].[BBMLogPurchaseSettlement] P2, 
                                            [DWSystem].[PurchaseInvoiceLog] L2, [DWSystem].[PurchaseInvoice] I2 
                                        WHERE P2.[PILogNo] = L2.[No] 
                                            And L2.[PurchaseInvoiceID] = I2.[PurchaseInvoiceID] 
                                            AND I2.[PurchaseInvoiceID] = @PurchaseInvoiceID
                                    ) 

                        END 
                        ELSE 
                        BEGIN 

                            SELECT @iCount = COUNT(*) 
                            FROM [DWSystem].[PurchaseInvoice] PO,[DWSystem].[PurchaseInvoiceLog] L 
                            WHERE PO.[PurchaseInvoiceID] = L.[PurchaseInvoiceID] 
                                AND PO.[PurchaseInvoiceID] = @PurchaseInvoiceID 
                                AND (PO.[Approved] = 1 
                                    OR PO.[PurchaseInvoiceID] IN (
                                            SELECT P.[PurchaseInvoiceID] 
                                            FROM [DWSystem].[PurchasingInvoicePayment] P, [DWSystem].[APPayment] A 
                                            WHERE P.[PaymentID] = A.[PaymentID] AND A.[Canceled] = 0 
                                                AND P.[PurchaseInvoiceID] = @PurchaseInvoiceID
                                        )
                                ) 

                            SELECT @iCount = @iCount + COUNT(*) 
                            FROM [DWSystem].[BBMLogPurchaseSettlement] P, 
                                [DWSystem].[PurchaseInvoiceLog] L, [DWSystem].[PurchaseInvoice] I 
                            WHERE P.[PILogNo] = L.[No] And L.[PurchaseInvoiceID] = I.[PurchaseInvoiceID] 
                                AND I.[Rejected] = 0 AND I.[PurchaseInvoiceID] <> @PurchaseInvoiceID 
                                AND P.[BBMLogNo] IN (
                                        SELECT P2.[BBMLogNo] 
                                        FROM [DWSystem].[BBMLogPurchaseSettlement] P2, 
                                            [DWSystem].[PurchaseInvoiceLog] L2, [DWSystem].[PurchaseInvoice] I2 
                                        WHERE P2.[PILogNo] = L2.[No] And L2.[PurchaseInvoiceID] = I2.[PurchaseInvoiceID] 
                                            AND I2.[PurchaseInvoiceID] = @PurchaseInvoiceID
                                    ) 

                            SELECT @iCount = @iCount + COUNT(*) 
                            FROM [DWSystem].[BBMLogPurchaseSettlement] P, 
                                [DWSystem].[BBMLog] L, [DWSystem].[BBM] M 
                            WHERE P.[BBMLogNo] = L.[No] And L.[BBMID] = M.[BBMID] And M.[Canceled] = 1 
                                AND P.[PILogNo] IN (
                                        SELECT L2.[No] 
                                        FROM [DWSystem].[PurchaseInvoiceLog] L2, [DWSystem].[PurchaseInvoice] I2 
                                        WHERE L2.[PurchaseInvoiceID] = I2.[PurchaseInvoiceID] 
                                            AND I2.[PurchaseInvoiceID] = @PurchaseInvoiceID
                                    ) 

                        END

                        IF @iCount > 0 
                            SELECT 0
                        ELSE
                            SELECT 1
                    ";
                    using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@PurchaseInvoiceID", SqlDbType.VarChar).Value = e.CommandArgument.ToString();

                        if (sqlConnection.State != ConnectionState.Open) sqlConnection.Open();
                        isEligibleToCancel = Convert.ToBoolean(cmd.ExecuteScalar());
                    }

                    //cancel validation
                    if(!isEligibleToCancel)
                        message.error(PAGETOPIC + " tidak valid untuk di cancel.");
                    else
                    {
                        sql = @"
                            UPDATE [DWSystem].[PurchaseInvoice] 
                            SET 
                                [Approved] = 0,
                                [Rejected] = 0,
                                [ValidatedDate] = NULL,
                                [ValidatedBy] = NULL,
                                [Web] = NULL
                            WHERE [PurchaseInvoiceID] = @PurchaseInvoiceID

                            UPDATE[DWSystem].[BBMPurchasing] 
                            SET [PurchaseInvoiceID] = NULL 
                            WHERE [BBMID] IN (
                                    SELECT [BBMID] 
                                    FROM [DWSystem].[BBMLog] ML, [DWSystem].[BBMLogPurchaseSettlement] P, 
                                        [DWSystem].[PurchaseInvoiceLog] L 
                                    WHERE ML.[No] = P.[BBMLogNo] AND P.[PILogNo] = L.[No] 
                                        AND L.[PurchaseInvoiceID] = @PurchaseInvoiceID
                                ) 
                        ";
                        using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Add("@PurchaseInvoiceID", SqlDbType.VarChar).Value = e.CommandArgument.ToString();
                            cmd.Parameters.Add("@UserName", SqlDbType.VarChar).Value = Tools.getCookieData<string>(GlobalVariables.COOKIEDATA_USERNAME);

                            if (sqlConnection.State != ConnectionState.Open)
                                sqlConnection.Open();
                            cmd.ExecuteNonQuery();
                        }
                        message.display(PAGETOPIC + " berhasil di cancel");
                    }
                }
                populatePage();
            }
            catch (Exception ex) { message.error(PAGETOPIC + " gagal cancel approve/reject. Hubungi administrator. Error: " + ex.Message); }
        }
        
        protected void rptChild_ItemCommand(object sender, RepeaterCommandEventArgs e)
        {
            if (((Button)e.CommandSource).ID == "btnHideRO")
            {
                ((Panel)e.Item.FindControl("pnlRO")).Visible = false;
                ((Button)e.Item.FindControl("btnHideRO")).Visible = false;
                ((Button)e.Item.FindControl("btnShowRO")).Visible = true;
            }
            else if (((Button)e.CommandSource).ID == "btnShowRO")
            {
                ((Panel)e.Item.FindControl("pnlRO")).Visible = true;
                ((Button)e.Item.FindControl("btnHideRO")).Visible = true;
                ((Button)e.Item.FindControl("btnShowRO")).Visible = false;

                string sql = string.Format(@"
                    SELECT 
                        RequestOrderLog.RequestOrderID,
                        RequestOrderLog.Quantity,
                        RequestOrderLog.Detail
                    FROM [DWSystem].RequestOrderLogTaken
                        LEFT OUTER JOIN [DWSystem].RequestOrderLog ON RequestOrderLog.No = RequestOrderLogTaken.RoLogNo
                    WHERE POLogNo = '{0}'
                ", e.CommandArgument.ToString());
                DataTable datatable = DBUtil.getData(sql);

                if (datatable.Rows.Count == 0)
                    message.error("Item tidak mempunyai RO");
                else
                {
                    Repeater rptRO = ((Repeater)e.Item.FindControl("rptRO"));
                    rptRO.DataSource = datatable;
                    rptRO.DataBind();
                }

            }

            ClientScript.RegisterStartupScript(this.GetType(), "hash", "location.hash = '#" + ((Label)e.Item.FindControl("ID")).Text + "';", true);
        }
    }
}