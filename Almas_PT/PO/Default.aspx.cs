using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace RIMBA.PO
{
    public partial class Default : System.Web.UI.Page
    {
        protected const string PAGETOPIC = "Purchase Order";
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
                filter = Tools.append(filter, "(PurchaseOrder.[Approved] = 0 AND PurchaseOrder.[Rejected] = 0)", " AND ");
            filter = Tools.append(filter, string.Format("(PurchaseOrder.PurchaseOrderID LIKE '%{0}%')", txtNo.Text.Trim()), " AND ");
            if (!string.IsNullOrWhiteSpace(txtDate.Text))
                filter = Tools.append(filter, string.Format("(CONVERT(varchar, PurchaseOrder.Date, 1) = '{0:MM/dd/yy}')", DateTime.ParseExact(txtDate.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture)), " AND ");
            if (!string.IsNullOrWhiteSpace(txtApproveDate.Text))
                filter = Tools.append(filter, string.Format("(CONVERT(varchar, PurchaseOrder.ValidatedDate, 1) = '{0:MM/dd/yy}')", DateTime.ParseExact(txtApproveDate.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture)), " AND ");

            //populate repeater
            string sql = string.Format(@"
                SELECT 
                    PurchaseOrder.PurchaseOrderID, PurchaseOrder.Date, PurchaseOrder.SubTotal, PurchaseOrder.UserName, 
                    PurchaseOrder.Approved, PurchaseOrder.Rejected, PurchaseOrder.ValidatedDate,
                    Supplier.Name AS SupplierName,
                    Currency.CurrencySymbol
                FROM [DWSystem].PurchaseOrder
                    LEFT OUTER JOIN [DWSystem].Supplier ON Supplier.SupplierID = PurchaseOrder.SupplierID
                    LEFT OUTER JOIN [DWSystem].Currency ON Currency.CurrencyID = PurchaseOrder.CurrencyID
                    LEFT OUTER JOIN [DWSystem].POSlittingList ON POSlittingList.PurchaseOrderID = PurchaseOrder.PurchaseOrderID
                WHERE POSlittingList.[PurchaseOrderID] IS NULL AND {2}
                ORDER BY PurchaseOrder.[Date] ASC, PurchaseOrder.[PurchaseOrderID] ASC;
                
                SELECT 
                    PurchaseOrderLog.PurchaseOrderID, {0} AS FormattedQuantity, PurchaseOrderLog.Cost, PurchaseOrderLog.Discount, 
                    PurchaseOrderLog.Total, PurchaseOrderLog.No,
                    Inventory.InventoryID, Inventory.InventoryName,
                    {1} AS HighestUnitCode
                FROM [DWSystem].PurchaseOrderLog
                    LEFT OUTER JOIN [DWSystem].PurchaseOrder ON PurchaseOrder.PurchaseOrderID = PurchaseOrderLog.PurchaseOrderID
                    LEFT OUTER JOIN [DWSystem].Inventory ON Inventory.InventoryID = PurchaseOrderLog.InventoryID
                    LEFT OUTER JOIN [DWSystem].InventoryUnit U1 ON U1.UnitID = Inventory.UnitID
                    LEFT OUTER JOIN [DWSystem].InventoryUnit U2 ON U2.UnitID = Inventory.SecUnitID
                    LEFT OUTER JOIN [DWSystem].InventoryUnit U3 ON U3.UnitID = Inventory.ThirdUnitID
                    LEFT OUTER JOIN [DWSystem].POSlittingList ON POSlittingList.PurchaseOrderID = PurchaseOrder.PurchaseOrderID
                WHERE POSlittingList.[PurchaseOrderID] IS NULL AND {2}
                ORDER BY PurchaseOrderLog.[No] ASC
            ", DBUtil.getSQLToFormatQty("PurchaseOrderLog.Quantity"), DBUtil.getSQLForHighestUnitCode(), filter);
            rptParent.DataSource = DBUtil.getData(sql, RELATIONNAME, "PurchaseOrderID");
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
                        UPDATE [DWSystem].[PurchaseOrder] 
                        SET 
                            [Approved] = 1,
                            [Rejected] = 0,
                            [ValidatedDate] = CURRENT_TIMESTAMP,
                            [ValidatedBy] = @UserName,
                            [Web] = 1 
                        WHERE [PurchaseOrderID] = @PurchaseOrderID";

                using (SqlConnection sqlConnection = new SqlConnection(Settings.ConnectionString))
                using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@PurchaseOrderID", SqlDbType.VarChar).Value = e.CommandArgument.ToString();
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
                        UPDATE [DWSystem].[PurchaseOrder] 
                        SET 
                            [Approved] = 0,
                            [Rejected] = 1,
                            [ValidatedDate] = CURRENT_TIMESTAMP,
                            [ValidatedBy] = @UserName,
                            [Web] = 1 
                        WHERE [PurchaseOrderID] = @PurchaseOrderID";

                using (SqlConnection sqlConnection = new SqlConnection(Settings.ConnectionString))
                using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@PurchaseOrderID", SqlDbType.VarChar).Value = e.CommandArgument.ToString();
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
                        DECLARE @iCount as int

                        SELECT @iCount = COUNT(*) 
                        FROM 
                            [DWSystem].[PurchaseOrder] PO
                            ,[DWSystem].[PurchaseOrderLog] L 
                        WHERE 
                            PO.[PurchaseOrderID] = L.[PurchaseOrderID] 
                            AND PO.[PurchaseOrderID] = @PurchaseOrderID 
                            AND (
                                L.[StopOrdered] = 1 
                                OR L.[No] IN(
                                    SELECT [POLogNo] 
                                    FROM 
                                        [DWSystem].[PurchaseOrderLogReceived] D
                                        ,[DWSystem].[PurchaseOrderLog] L2
                                        ,[DWSystem].[BBMLog] BL,[DWSystem].[BBM] B 
                                    WHERE 
                                        D.[BBMLogNo] = BL.[No] 
                                        AND BL.[BBMID] = B.[BBMID] 
                                        AND B.[Canceled] = 0 
                                        AND D.[POLogNo] = L2.[No] 
                                        AND L2.[PurchaseOrderID] = @PurchaseOrderID
                                )
                            ) 

                        IF @iCount > 0 
                            SELECT 0
                        ELSE
                            SELECT 1
                    ";
                    using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@PurchaseOrderID", SqlDbType.VarChar).Value = e.CommandArgument.ToString();

                        if (sqlConnection.State != ConnectionState.Open) sqlConnection.Open();
                        isEligibleToCancel = Convert.ToBoolean(cmd.ExecuteScalar());
                    }

                    //cancel validation
                    if(!isEligibleToCancel)
                        message.error(PAGETOPIC + " tidak valid untuk di cancel.");
                    else
                    {
                        sql = @"
                            UPDATE [DWSystem].[PurchaseOrder] 
                            SET 
                                [Approved] = 0,
                                [Rejected] = 0,
                                [ValidatedDate] = NULL,
                                [ValidatedBy] = NULL,
                                [Web] = NULL
                            WHERE [PurchaseOrderID] = @PurchaseOrderID";
                        using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Add("@PurchaseOrderID", SqlDbType.VarChar).Value = e.CommandArgument.ToString();
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