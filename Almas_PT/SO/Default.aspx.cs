using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace RIMBA.SO
{
    public partial class Default : System.Web.UI.Page
    {
        protected const string PAGETOPIC = "Sales Order";
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
                filter = Tools.append(filter, "(SalesOrder.[Approved] = 0 AND SalesOrder.[Rejected] = 0)", " AND ");
            filter = Tools.append(filter, string.Format("(SalesOrder.SalesOrderID LIKE '%{0}%')", txtNo.Text.Trim()), " AND ");
            if (!string.IsNullOrWhiteSpace(txtDate.Text))
                filter = Tools.append(filter, string.Format("(CONVERT(varchar, SalesOrder.Date, 1) = '{0:MM/dd/yy}')", DateTime.ParseExact(txtDate.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture)), " AND ");
            if (!string.IsNullOrWhiteSpace(txtApproveDate.Text))
                filter = Tools.append(filter, string.Format("(CONVERT(varchar, SalesOrder.ValidatedDate, 1) = '{0:MM/dd/yy}')", DateTime.ParseExact(txtApproveDate.Text, "dd/MM/yyyy", CultureInfo.InvariantCulture)), " AND ");

            //populate repeater
            string sql = string.Format(@"
                SELECT 
                    SalesOrder.SalesOrderID, SalesOrder.Date, SalesOrder.SubTotal, SalesOrder.UserName, 
                    SalesOrder.Approved, SalesOrder.Rejected, SalesOrder.ValidatedDate,
                    Customer.Name AS CustomerName,
                    Currency.CurrencySymbol,
                    SalesPerson.Name AS SalesName
                FROM [DWSystem].SalesOrder
                    LEFT OUTER JOIN [DWSystem].Customer ON Customer.CustomerID = SalesOrder.CustomerID
                    LEFT OUTER JOIN [DWSystem].Currency ON Currency.CurrencyID = SalesOrder.CurrencyID
                    LEFT OUTER JOIN [DWSystem].SalesPerson ON SalesPerson.SalesID = SalesOrder.SalesID
                WHERE {2}
                ORDER BY SalesOrder.[Date] ASC, SalesOrder.[SalesOrderID] ASC;
                
                SELECT 
                    SalesOrderLog.SalesOrderID, {0} AS FormattedQuantity, SalesOrderLog.Price, SalesOrderLog.Discount, 
                    SalesOrderLog.Total, SalesOrderLog.No,
                    Inventory.InventoryID, Inventory.InventoryName,
                    {1} AS HighestUnitCode
                FROM [DWSystem].SalesOrderLog
                    LEFT OUTER JOIN [DWSystem].SalesOrder ON SalesOrder.SalesOrderID = SalesOrderLog.SalesOrderID
                    LEFT OUTER JOIN [DWSystem].Inventory ON Inventory.InventoryID = SalesOrderLog.InventoryID
                    LEFT OUTER JOIN [DWSystem].InventoryUnit U1 ON U1.UnitID = Inventory.UnitID
                    LEFT OUTER JOIN [DWSystem].InventoryUnit U2 ON U2.UnitID = Inventory.SecUnitID
                    LEFT OUTER JOIN [DWSystem].InventoryUnit U3 ON U3.UnitID = Inventory.ThirdUnitID
                WHERE {2}
                ORDER BY SalesOrderLog.[No] ASC
            ", DBUtil.getSQLToFormatQty("SalesOrderLog.Quantity"), DBUtil.getSQLForHighestUnitCode(), filter);
            rptParent.DataSource = DBUtil.getData(sql, RELATIONNAME, "SalesOrderID");
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
                        UPDATE [DWSystem].[SalesOrder] 
                        SET 
                            [Approved] = 1,
                            [Rejected] = 0,
                            [ValidatedDate] = CURRENT_TIMESTAMP,
                            [ValidatedBy] = @UserName,
                            [Web] = 1 
                        WHERE [SalesOrderID] = @SalesOrderID";

                using (SqlConnection sqlConnection = new SqlConnection(Settings.ConnectionString))
                using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@SalesOrderID", SqlDbType.VarChar).Value = e.CommandArgument.ToString();
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
                        UPDATE [DWSystem].[SalesOrder] 
                        SET 
                            [Approved] = 0,
                            [Rejected] = 1,
                            [ValidatedDate] = CURRENT_TIMESTAMP,
                            [ValidatedBy] = @UserName,
                            [Web] = 1 
                        WHERE [SalesOrderID] = @SalesOrderID";

                using (SqlConnection sqlConnection = new SqlConnection(Settings.ConnectionString))
                using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("@SalesOrderID", SqlDbType.VarChar).Value = e.CommandArgument.ToString();
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
                        FROM [DWSystem].[SalesOrder] 
                        WHERE [SalesOrderID] = @SalesOrderID 

                        DECLARE @iCount as int,@isApproved as bit  

                        IF @iApproved = 1 
                        BEGIN 
                            SELECT @iCount = COUNT(*) 
                            FROM 
                                [DWSystem].[SalesOrder] PO
                                ,[DWSystem].[SalesOrderLog] L 
                            WHERE 
                                PO.[SalesOrderID] = L.[SalesOrderID] 
                                AND PO.[SalesOrderID] = @SalesOrderID 
                                AND (
                                        PO.[Rejected] = 1 
                                        OR L.[StopOrdered] = 1 OR L.[No] IN(
                                                SELECT [SOLogNo] 
                                                FROM 
                                                    [DWSystem].[SalesOrderLogDelivered] LI
                                                    ,[DWSystem].[SalesOrderLog] L2
                                                    ,[DWSystem].[BBKLog] BL,[DWSystem].[BBK] B 
                                                WHERE 
                                                    LI.[BBKLogNo] = BL.[No] 
                                                    AND BL.[BBKID] = B.[BBKID] 
                                                    AND B.[Canceled] = 0 
                                                    AND LI.[SOLogNo] = L2.[No] 
                                                    AND L2.[SalesOrderID] = @SalesOrderID
                                            ) 
                                        OR PO.[SalesOrderID] IN(
                                                SELECT [SalesOrderID] 
                                                FROM [DWSystem].[ProductionOrder] 
                                                WHERE [Approved] = 1
                                            ) 
                                        OR L.[No] IN(
                                                SELECT [SOLogNo] 
                                                FROM [DWSystem].[SalesOrderLogStuffed] LS,[DWSystem].[SalesOrderLog] L3, 
                                                [DWSystem].[StuffingOrderLog] STL,[DWSystem].[StuffingOrder] ST 
                                                WHERE LS.[StuffingOrderLogNo] = STL.[No] AND STL.[StuffingOrderID] = ST.[StuffingOrderID] AND ST.[Rejected] = 0 
                                                AND LS.[SOLogNo] = L3.[No] AND L3.[SalesOrderID] = @SalesOrderID
                                            ) 
                                    ) 
                        END 
                        ELSE 
                        BEGIN 
                            SELECT @iCount = COUNT(*)
                            FROM [DWSystem].[SalesInvoice] PO,[DWSystem].[SalesInvoiceLog] L
                            WHERE 
                                PO.[SalesInvoiceID] = L.[SalesInvoiceID]
                                AND PO.[SalesInvoiceID] = @SalesInvoiceID  
                                AND ( 
                                    PO.[Approved] = 1
                                    OR PO.[SalesInvoiceID] IN ( 
                                            SELECT P.[SalesInvoiceID]
                                            FROM [DWSystem].[SellingInvoicePayment] P, [DWSystem].[ARPayment] A
                                            WHERE P.[PaymentID] = A.[PaymentID]  
                                                AND A.[Canceled] = 0
                                                AND P.[SalesInvoiceID] = @SalesInvoiceID 
                                        ) 
                                    )

                            SELECT @iCount = @iCount + COUNT(*)
                            FROM [DWSystem].[BBKLogSalesSettlement] P,
                                [DWSystem].[SalesInvoiceLog] L, 
                                [DWSystem].[SalesInvoice] I
                            WHERE P.[SILogNo] = L.[No]  
                                And L.[SalesInvoiceID] = I.[SalesInvoiceID]
                                AND I.[Rejected] = 0     
                                AND I.[SalesInvoiceID] <> @SalesInvoiceID
                                AND P.[BBKLogNo] IN (
                                        SELECT P2.[BBKLogNo]
                                        FROM [DWSystem].[BBKLogSalesSettlement] P2,
                                            [DWSystem].[SalesInvoiceLog] L2, [DWSystem].[SalesInvoice] I2
                                        WHERE P2.[SILogNo] = L2.[No]  
                                            And L2.[SalesInvoiceID] = I2.[SalesInvoiceID]
                                            AND I2.[SalesInvoiceID] = @SalesInvoiceID 
                                    )

                            SELECT @iCount = @iCount + COUNT(*)
                            FROM [DWSystem].[BBKLogSalesSettlement] P,
                                [DWSystem].[BBKLog] L,  
                                [DWSystem].[BBK] K
                            WHERE P.[BBKLogNo] = L.[No]  
                                And L.[BBKID] = K.[BBKID]  
                                And K.[Canceled] = 1
                                AND P.[SILogNo] IN(SELECT L2.[No]
                            FROM [DWSystem].[SalesInvoiceLog] L2,  
                                [DWSystem].[SalesInvoice] I2
                            WHERE L2.[SalesInvoiceID] = I2.[SalesInvoiceID]
                                AND I2.[SalesInvoiceID] = @SalesInvoiceID)

                        END 

                        IF @iCount > 0 
                            SELECT 0
                        ELSE
                            SELECT 1
                    ";
                    using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add("@SalesOrderID", SqlDbType.VarChar).Value = e.CommandArgument.ToString();

                        if (sqlConnection.State != ConnectionState.Open) sqlConnection.Open();
                        isEligibleToCancel = Convert.ToBoolean(cmd.ExecuteScalar());
                    }

                    //cancel validation
                    if(!isEligibleToCancel)
                        message.error(PAGETOPIC + " tidak valid untuk di cancel.");
                    else
                    {
                        sql = @"
                            UPDATE [DWSystem].[SalesOrder] 
                            SET 
                                [Approved] = 0,
                                [Rejected] = 0,
                                [ValidatedDate] = NULL,
                                [ValidatedBy] = NULL,
                                [Web] = NULL
                            WHERE [SalesOrderID] = @SalesOrderID";
                        using (SqlCommand cmd = new SqlCommand(sql, sqlConnection))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Add("@SalesOrderID", SqlDbType.VarChar).Value = e.CommandArgument.ToString();
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